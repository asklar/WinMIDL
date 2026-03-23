using System.Reflection.Metadata;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Decompiler;
using Midl.Syntax;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Handles navigation into and within decompiled WinMD virtual documents.
/// </summary>
internal static class WinMdNavigator
{
    /// <summary>
    /// Creates a definition location for a WinMD external symbol by decompiling the WinMD
    /// and returning a location with the midl-winmd URI scheme.
    /// </summary>
    internal static object? CreateWinMdDefinitionLocation(ISymbol symbol)
    {
        try
        {
            // Extract the namespace to filter decompilation (avoids decompiling all of Windows.winmd)
            var ns = symbol.FullName;
            var lastDot = ns.LastIndexOf('.');
            var nsFilter = lastDot > 0 ? ns[..lastDot] : null;

            var decompiler = new WinMdDecompiler();
            var decompiled = decompiler.Decompile(symbol.SourceWinMdPath!, nsFilter);

            // Find the symbol in decompiled text using keyword + name pattern
            int defOffset = GotoDefinitionHandler.FindDefinitionOffset(decompiled, symbol);
            if (defOffset < 0)
            {
                // Symbol not found in decompiled text (e.g., CLR-projected type like TimeSpan).
                // Still open the namespace view but position at top.
                defOffset = 0;
            }

            var decompLineStarts = PositionHelper.BuildLineStarts(decompiled);
            var (dLine, dChar) = PositionHelper.OffsetToLineChar(decompLineStarts, defOffset);
            var (dEndLine, dEndChar) = PositionHelper.OffsetToLineChar(decompLineStarts, defOffset + symbol.Name.Length);

            // Encode namespace as part of the URI path so each namespace gets a distinct virtual document
            // Format: midl-winmd:///path/to/file.winmd/Namespace.Name
            var winmdUri = "midl-winmd:///" + symbol.SourceWinMdPath!.Replace('\\', '/');
            if (nsFilter != null)
                winmdUri += "/" + nsFilter;
            return GotoDefinitionHandler.MakeLocation(winmdUri, dLine, dChar, dEndLine, dEndChar);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Handles F12 within a decompiled WinMD virtual document.
    /// Finds the identifier under cursor and navigates to the type's namespace decompilation.
    /// </summary>
    internal static object? HandleWinMdView(string sourceText, string uri, int line, int character, int[] lineStarts)
    {
        int offset = PositionHelper.LineCharToOffset(lineStarts, line, character);
        var lexer = new Lexer.Lexer(sourceText);
        var tokens = lexer.Lex();
        var (token, tokenOffset) = GotoDefinitionHandler.FindTokenAtOffset(tokens, offset);
        if (token is null || token.Kind != SyntaxKind.IdentifierToken)
        {
            return null;
        }

        string name = token.Text;

        // Extract WinMD path and current namespace from URI
        // VS Code may normalize midl-winmd:///C:/... to midl-winmd:/C%3A/... 
        var uriPath = uri;
        var schemeEnd = uriPath.IndexOf("midl-winmd:", StringComparison.OrdinalIgnoreCase);
        if (schemeEnd < 0) return null;
        uriPath = uriPath[(schemeEnd + "midl-winmd:".Length)..].TrimStart('/');
        uriPath = Uri.UnescapeDataString(uriPath);
        var winmdIdx = uriPath.IndexOf(".winmd", StringComparison.OrdinalIgnoreCase);
        if (winmdIdx < 0) return null;
        var winmdPath = uriPath[..(winmdIdx + 6)];
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            winmdPath = winmdPath.Replace('/', '\\');
        if (!File.Exists(winmdPath)) return null;

        // Current namespace from URI path (after .winmd/)
        string? currentNs = null;
        if (uriPath.Length > winmdIdx + 7)
            currentNs = uriPath[(winmdIdx + 7)..].Replace('/', '.');

        // Build search candidates: FQN, currentNs.shortName, shortName
        var fqn = SymbolResolver.ExtractFullQualifiedNameAtCursor(sourceText, tokenOffset, name);
        var candidates = new List<string>();
        if (fqn != null && fqn != name) candidates.Add(fqn);
        if (currentNs != null) candidates.Add($"{currentNs}.{name}");
        candidates.Add(name);

        try
        {
            using var fs = File.OpenRead(winmdPath);
            using var peReader = new System.Reflection.PortableExecutable.PEReader(fs);
            var reader = peReader.GetMetadataReader();

            // First pass: prefer FQN or namespace-qualified match
            string? matchedName = null;
            string? matchedNs = null;
            foreach (var handle in reader.TypeDefinitions)
            {
                var td = reader.GetTypeDefinition(handle);
                var typeName = reader.GetString(td.Name);
                var typeNs = reader.GetString(td.Namespace);
                var backtick = typeName.IndexOf('`');
                var cleanName = backtick >= 0 ? typeName[..backtick] : typeName;
                var typeFullName = string.IsNullOrEmpty(typeNs) ? cleanName : $"{typeNs}.{cleanName}";

                // Check FQN and namespace-qualified candidates (not bare simple name)
                for (int ci = 0; ci < candidates.Count - 1; ci++)
                {
                    if (typeFullName.Equals(candidates[ci], StringComparison.Ordinal))
                    { matchedName = cleanName; matchedNs = typeNs; goto found; }
                }
            }

            // Second pass: bare simple name (last candidate)
            foreach (var handle in reader.TypeDefinitions)
            {
                var td = reader.GetTypeDefinition(handle);
                var typeName = reader.GetString(td.Name);
                var backtick = typeName.IndexOf('`');
                var cleanName = backtick >= 0 ? typeName[..backtick] : typeName;
                if (cleanName.Equals(name, StringComparison.Ordinal))
                { matchedName = cleanName; matchedNs = reader.GetString(td.Namespace); goto found; }
            }

            // Check CLR-projected types
            foreach (var c in candidates)
            {
                var projected = Analysis.ClrProjectionMap.ToWinRTName(c);
                if (projected != c)
                {
                    var lastDot = projected.LastIndexOf('.');
                    var projNs = lastDot >= 0 ? projected[..lastDot] : "";
                    return GotoDefinitionHandler.MakeLocation("midl-winmd:///" + winmdPath.Replace('\\', '/') + "/" + projNs, 0, 0, 0, 0);
                }
            }
            return null;

        found:
            // Same namespace as current view? Just search in the existing sourceText
            if (matchedNs == currentNs)
            {
                foreach (var kw in new[] { "runtimeclass", "interface", "struct", "enum", "delegate" })
                {
                    var pattern = kw + " " + matchedName;
                    int idx2 = FindWordBoundaryMatch(sourceText, pattern);
                    if (idx2 >= 0)
                    {
                        idx2 += kw.Length + 1;
                        var (dl, dc) = PositionHelper.OffsetToLineChar(lineStarts, idx2);
                        var (del, dec) = PositionHelper.OffsetToLineChar(lineStarts, idx2 + matchedName!.Length);
                        return GotoDefinitionHandler.MakeLocation(uri, dl, dc, del, dec);
                    }
                }
            }

            // Different namespace — decompile and navigate
            var decompiler = new WinMdDecompiler();
            var decompiled = decompiler.Decompile(winmdPath, matchedNs);
            int defOffset = -1;
            foreach (var kw in new[] { "runtimeclass", "interface", "struct", "enum", "delegate" })
            {
                var pattern = kw + " " + matchedName;
                defOffset = FindWordBoundaryMatch(decompiled, pattern);
                if (defOffset >= 0) { defOffset += kw.Length + 1; break; }
            }
            if (defOffset < 0) defOffset = 0;
            var decompLineStarts = PositionHelper.BuildLineStarts(decompiled);
            var (dLine2, dChar2) = PositionHelper.OffsetToLineChar(decompLineStarts, defOffset);
            var (dEndLine2, dEndChar2) = PositionHelper.OffsetToLineChar(decompLineStarts, defOffset + matchedName!.Length);
            return GotoDefinitionHandler.MakeLocation("midl-winmd:///" + winmdPath.Replace('\\', '/') + "/" + matchedNs, dLine2, dChar2, dEndLine2, dEndChar2);
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Find a pattern in text with word boundary enforcement.
    /// Returns the index of the match, or -1 if not found.
    /// </summary>
    private static int FindWordBoundaryMatch(string text, string pattern)
    {
        int searchFrom = 0;
        while (true)
        {
            int idx = text.IndexOf(pattern, searchFrom, StringComparison.Ordinal);
            if (idx < 0) return -1;
            int afterPattern = idx + pattern.Length;
            // Verify word boundary: next char must not be alphanumeric/underscore
            if (afterPattern < text.Length &&
                (char.IsLetterOrDigit(text[afterPattern]) || text[afterPattern] == '_'))
            {
                searchFrom = idx + 1;
                continue;
            }
            return idx;
        }
    }

    /// <summary>Find Windows.winmd from the SDK.</summary>
    internal static string? FindWindowsWinMd()
    {
        var sdkRoot = @"C:\Program Files (x86)\Windows Kits\10";
        var unionMeta = Path.Combine(sdkRoot, "UnionMetadata");
        if (!Directory.Exists(unionMeta)) return null;

        // Look for versioned directories (10.0.XXXXX.0), skip Facade (it's a type-forwarder stub)
        return Directory.GetDirectories(unionMeta)
            .Where(d => !Path.GetFileName(d).Equals("Facade", StringComparison.OrdinalIgnoreCase))
            .Select(d => Path.Combine(d, "Windows.winmd"))
            .Where(File.Exists)
            .OrderByDescending(f => Path.GetFileName(Path.GetDirectoryName(f)))
            .FirstOrDefault();
    }

    /// <summary>
    /// Lightweight proxy that wraps a built-in symbol with a SourceWinMdPath,
    /// allowing CreateWinMdDefinitionLocation to decompile it.
    /// </summary>
    internal sealed class BuiltInSymbolProxy : ISymbol
    {
        private readonly ISymbol _inner;
        public BuiltInSymbolProxy(ISymbol inner, string winmdPath)
        {
            _inner = inner;
            SourceWinMdPath = winmdPath;
        }
        public string Name => _inner.Name;
        public string FullName => _inner.FullName;
        public SymbolKind Kind => _inner.Kind;
        public ISymbol? ContainingSymbol => _inner.ContainingSymbol;
        public bool IsExternal => true;
        public string? SourceWinMdPath { get; }
        public string? SourceFile => null;
        public int SourcePosition { get => _inner.SourcePosition; set { } }
    }
}
