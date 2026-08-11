// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection.Metadata;
using Midl.Analysis;
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

            // Parse the decompiled text to get accurate symbol positions
            int defOffset = FindSymbolByParsing(decompiled, symbol);
            if (defOffset < 0)
            {
                // Fallback: text search for non-type symbols or parse failures
                defOffset = GotoDefinitionHandler.FindDefinitionOffset(decompiled, symbol);
            }
            if (defOffset < 0)
            {
                // Still not found (e.g., CLR-projected type like TimeSpan).
                // Open the namespace view positioned at top.
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
    /// Parses decompiled IDL text and looks up the symbol by name to get its exact source position.
    /// Returns the offset of the symbol's name token, or -1 if not found.
    /// </summary>
    internal static int FindSymbolByParsing(string decompiledText, ISymbol symbol)
    {
        try
        {
            var compilation = CompilationHelper.Compile(decompiledText);
            foreach (var type in compilation.GetAllTypes())
            {
                if (type.Name == symbol.Name && type.SourcePosition >= 0)
                {
                    // SourcePosition may be off by 1 due to trivia computation.
                    // Verify and adjust to land exactly on the name.
                    int pos = type.SourcePosition;
                    if (pos < decompiledText.Length && pos + symbol.Name.Length <= decompiledText.Length
                        && decompiledText.Substring(pos, symbol.Name.Length) == symbol.Name)
                        return pos;
                    // Try pos-1 (common off-by-one from leading trivia)
                    if (pos > 0 && pos - 1 + symbol.Name.Length <= decompiledText.Length
                        && decompiledText.Substring(pos - 1, symbol.Name.Length) == symbol.Name)
                        return pos - 1;
                    // Search nearby (+/- 3 chars) for the name
                    for (int delta = -3; delta <= 3; delta++)
                    {
                        int candidate = pos + delta;
                        if (candidate >= 0 && candidate + symbol.Name.Length <= decompiledText.Length
                            && decompiledText.Substring(candidate, symbol.Name.Length) == symbol.Name)
                            return candidate;
                    }
                    return pos; // best guess
                }
            }
        }
        catch { }
        return -1;
    }

    /// <summary>
    /// Handles F12 within a decompiled WinMD virtual document.
    /// Finds the identifier under cursor and navigates to the type's namespace decompilation.
    /// </summary>
    internal static object? HandleWinMdView(string sourceText, string uri, int line, int character, int[] lineStarts,
        IReadOnlyList<string>? referencePaths = null)
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
            // The type may not live in the WinMD being viewed. Nearly every real WinMD
            // refers to types from others — SDK types above all — so when the current file
            // does not define it, look through the WinMDs this compilation references.
            var searchOrder = new List<string> { winmdPath };
            if (referencePaths != null)
            {
                foreach (var reference in referencePaths)
                {
                    if (string.IsNullOrEmpty(reference)) continue;
                    if (!reference.EndsWith(".winmd", StringComparison.OrdinalIgnoreCase)) continue;
                    if (searchOrder.Any(p => string.Equals(p, reference, StringComparison.OrdinalIgnoreCase))) continue;
                    if (File.Exists(reference)) searchOrder.Add(reference);
                }
            }

            string? matchedName = null;
            string? matchedNs = null;
            string? matchedWinMd = null;

            foreach (var candidateWinMd in searchOrder)
            {
                if (TryFindType(candidateWinMd, candidates, name, out var foundName, out var foundNs))
                {
                    matchedName = foundName;
                    matchedNs = foundNs;
                    matchedWinMd = candidateWinMd;
                    break;
                }
            }

            if (matchedWinMd == null)
            {
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
            }

            // Same file and namespace as the current view? Navigate within this document.
            if (matchedNs == currentNs &&
                string.Equals(matchedWinMd, winmdPath, StringComparison.OrdinalIgnoreCase))
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

            // Different namespace or a different WinMD — decompile that view and navigate.
            var decompiler = new WinMdDecompiler();
            var decompiled = decompiler.Decompile(matchedWinMd, matchedNs);
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
            return GotoDefinitionHandler.MakeLocation("midl-winmd:///" + matchedWinMd.Replace('\\', '/') + "/" + matchedNs, dLine2, dChar2, dEndLine2, dEndChar2);
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Looks for a type in one WinMD, preferring a fully-qualified or namespace-qualified
    /// match over a bare simple name.
    /// </summary>
    private static bool TryFindType(string winmdPath, List<string> candidates, string simpleName,
        out string? matchedName, out string? matchedNs)
    {
        matchedName = null;
        matchedNs = null;

        try
        {
            using var fs = File.OpenRead(winmdPath);
            using var peReader = new System.Reflection.PortableExecutable.PEReader(fs);
            var reader = peReader.GetMetadataReader();

            // First pass: prefer FQN or namespace-qualified match
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
                    {
                        matchedName = cleanName;
                        matchedNs = typeNs;
                        return true;
                    }
                }
            }

            // Second pass: bare simple name (last candidate)
            foreach (var handle in reader.TypeDefinitions)
            {
                var td = reader.GetTypeDefinition(handle);
                var typeName = reader.GetString(td.Name);
                var backtick = typeName.IndexOf('`');
                var cleanName = backtick >= 0 ? typeName[..backtick] : typeName;
                if (cleanName.Equals(simpleName, StringComparison.Ordinal))
                {
                    matchedName = cleanName;
                    matchedNs = reader.GetString(td.Namespace);
                    return true;
                }
            }
        }
        catch { }

        return false;
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
    /// <param name="preferredVersion">Specific SDK version like "10.0.26100.0", or null/empty/"latest" for newest.</param>
    /// <param name="sdkRoot">
    /// SDK root to search. Callers that already know where the SDK is — the host discovers it
    /// at startup and configures the server — should pass it rather than letting this guess.
    /// </param>
    internal static string? FindWindowsWinMd(string? preferredVersion = null, string? sdkRoot = null)
    {
        sdkRoot ??= DefaultSdkRoot();
        if (sdkRoot == null) return null;
        var unionMeta = Path.Combine(sdkRoot, "UnionMetadata");
        if (!Directory.Exists(unionMeta)) return null;

        // If a specific version is requested, try it directly
        if (!string.IsNullOrEmpty(preferredVersion) &&
            !preferredVersion.Equals("latest", StringComparison.OrdinalIgnoreCase))
        {
            var specific = Path.Combine(unionMeta, preferredVersion, "Windows.winmd");
            if (File.Exists(specific)) return specific;
        }

        // Look for versioned directories (10.0.XXXXX.0), skip Facade (it's a type-forwarder stub)
        return Directory.GetDirectories(unionMeta)
            .Select(d => (Dir: d, Version: Version.TryParse(Path.GetFileName(d), out var v) ? v : null))
            .Where(x => x.Version != null)
            .OrderByDescending(x => x.Version)
            .Select(x => Path.Combine(x.Dir, "Windows.winmd"))
            .FirstOrDefault(File.Exists);
    }

    /// <summary>Get all available SDK versions that have Windows.winmd.</summary>
    internal static List<string> GetAvailableSdkVersions(string? sdkRoot = null)
    {
        sdkRoot ??= DefaultSdkRoot();
        if (sdkRoot == null) return new List<string>();
        var unionMeta = Path.Combine(sdkRoot, "UnionMetadata");
        if (!Directory.Exists(unionMeta)) return new List<string>();

        return Directory.GetDirectories(unionMeta)
            .Select(d => (Dir: d, Version: Version.TryParse(Path.GetFileName(d), out var v) ? v : null))
            .Where(x => x.Version != null)
            .Where(x => File.Exists(Path.Combine(x.Dir, "Windows.winmd")))
            .OrderByDescending(x => x.Version)
            .Select(x => Path.GetFileName(x.Dir)!)
            .ToList();
    }

    /// <summary>
    /// Last resort when no SDK root was supplied: the standard install location, then the
    /// registry. Prefer passing the root the host already discovered.
    /// </summary>
    private static string? DefaultSdkRoot()
    {
        const string standardRoot = @"C:\Program Files (x86)\Windows Kits\10";
        if (Directory.Exists(standardRoot)) return standardRoot;

        if (!OperatingSystem.IsWindows()) return null;
        try
        {
            using var hklm = Microsoft.Win32.RegistryKey.OpenBaseKey(
                Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry32);
            using var key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows Kits\Installed Roots");
            return key?.GetValue("KitsRoot10") as string;
        }
        catch { return null; }
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
        public DocComment? DocComment { get => _inner.DocComment; set { } }
    }
}
