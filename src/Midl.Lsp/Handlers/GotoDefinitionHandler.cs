using System.Reflection.Metadata;
using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Decompiler;
using Midl.Syntax;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Handles textDocument/definition requests — navigates to where a symbol is defined.
/// </summary>
public static class GotoDefinitionHandler
{
    public static object? Handle(JsonElement? @params, DocumentManager docManager,
        IReadOnlyList<string>? includePaths = null, IReadOnlyList<string>? referencePaths = null,
        Compilation? cachedCompilation = null)
    {
        if (@params is null) return null;

        var textDocument = @params.Value.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;
        var position = @params.Value.GetProperty("position");
        int line = position.GetProperty("line").GetInt32();
        int character = position.GetProperty("character").GetInt32();

        var text = docManager.GetDocumentText(uri);
        if (text is null) return null;

        // Extract file path from URI for import resolution
        string? filePath = null;
        if (uri.StartsWith("file:///"))
            filePath = Uri.UnescapeDataString(uri[8..]);

        return HandleCore(text, uri, line, character, filePath, includePaths, referencePaths, cachedCompilation);
    }

    /// <summary>
    /// Core logic, testable without LSP JSON plumbing.
    /// Returns an LSP Location object, a command response, or null.
    /// </summary>
    public static object? HandleCore(string sourceText, string uri, int line, int character,
        string? currentFilePath = null,
        IReadOnlyList<string>? includePaths = null,
        IReadOnlyList<string>? referencePaths = null,
        Compilation? cachedCompilation = null)
    {
        var lineStarts = PositionHelper.BuildLineStarts(sourceText);

        // For decompiled WinMD views: look up the type in the WinMD and navigate to its namespace
        if (uri.StartsWith("midl-winmd://"))
            return HandleWinMdView(sourceText, uri, line, character, lineStarts);

        // Check if cursor is on a // @ref: line → navigate to the referenced file/folder
        var refTarget = TryGetRefDirectiveTarget(sourceText, lineStarts, line);
        if (refTarget != null)
            return CreateRefDirectiveResponse(refTarget, uri, lineStarts, line);

        int offset = PositionHelper.LineCharToOffset(lineStarts, line, character);

        var lexer = new Lexer.Lexer(sourceText);
        var tokens = lexer.Lex();
        var (token, tokenOffset) = FindTokenAtOffset(tokens, offset);
        if (token is null || token.Kind != SyntaxKind.IdentifierToken)
            return null;

        string name = token.Text;

        // Use cached compilation if available (shares type universe with diagnostics/autocomplete)
        Compilation compilation;
        if (cachedCompilation != null)
        {
            compilation = cachedCompilation;
        }
        else
        {
            try
            {
                if (currentFilePath != null)
                {
                    compilation = CompilationHelper.Compile(sourceText, referencePaths,
                        CompilationMode.Strict, includePaths, currentFilePath);
                }
                else
                {
                    compilation = CompilationHelper.Compile(sourceText);
                }
            }
            catch
            {
                return null;
            }
        }

        // Search for the symbol in compilation
        var symbol = FindSymbol(compilation, name, sourceText, tokenOffset);

        // If not found by simple name, try the full qualified name the cursor is within
        // e.g., cursor on "Xaml" in "Microsoft.UI.Xaml.Controls.Control" → look up the whole thing
        if (symbol is null)
        {
            var fullQualifiedName = ExtractFullQualifiedNameAtCursor(sourceText, tokenOffset, name);
            if (fullQualifiedName != null && fullQualifiedName != name)
            {
                // Try the full qualified name directly via SymbolTable
                symbol = compilation.SymbolTable?.LookupDefinedType(fullQualifiedName);

                // Also try FindSymbol with the last segment + correct offset
                if (symbol is null)
                {
                    var lastDot = fullQualifiedName.LastIndexOf('.');
                    var typeName = lastDot >= 0 ? fullQualifiedName[(lastDot + 1)..] : fullQualifiedName;
                    // Find the actual offset of typeName in source by locating the full QN
                    var qnIdx = sourceText.IndexOf(fullQualifiedName, StringComparison.Ordinal);
                    if (qnIdx >= 0)
                        symbol = FindSymbol(compilation, typeName, sourceText, qnIdx + lastDot + 1);
                }
            }
        }

        if (symbol is null) return null;

        // Imported .idl symbol: navigate to definition in the source file
        if (symbol.SourceFile != null)
        {
            var resolvedSourceFile = symbol.SourceFile;
            if (File.Exists(resolvedSourceFile))
            {
                var otherText = File.ReadAllText(resolvedSourceFile);
                int defOffset = FindDefinitionOffset(otherText, symbol);
                if (defOffset >= 0)
                {
                    var otherLineStarts = PositionHelper.BuildLineStarts(otherText);
                    var (defLine, defChar) = PositionHelper.OffsetToLineChar(otherLineStarts, defOffset);
                    var (defEndLine, defEndChar) = PositionHelper.OffsetToLineChar(otherLineStarts, defOffset + symbol.Name.Length);
                    var otherUri = "file:///" + resolvedSourceFile.Replace('\\', '/');
                    return MakeLocation(otherUri, defLine, defChar, defEndLine, defEndChar);
                }
            }
        }

        // External WinMD symbol: navigate to decompiled view
        if (symbol.IsExternal && symbol.SourceWinMdPath != null)
        {
            if (File.Exists(symbol.SourceWinMdPath))
            {
                return CreateWinMdDefinitionLocation(symbol);
            }

            // Built-in WinRT types have SourceWinMdPath = "<built-in>" — resolve via Windows.winmd
            if (symbol.SourceWinMdPath == "<built-in>")
            {
                var winmd = FindWindowsWinMd();
                if (winmd != null)
                    return CreateWinMdDefinitionLocation(new BuiltInSymbolProxy(symbol, winmd));
            }

            // Fall back to @ref line navigation
            var refLine = FindRefLineForExternalSymbol(sourceText, symbol);
            if (refLine >= 0)
            {
                var lineText = GetLineText(sourceText, lineStarts, refLine);
                return MakeLocation(uri, refLine, 0, refLine, lineText.Length);
            }
            return null;
        }

        // Non-external built-in WinRT types (shouldn't happen, but handle gracefully)
        if (symbol.FullName.StartsWith("Windows.") && FindDefinitionOffset(sourceText, symbol) < 0)
        {
            var winmd = FindWindowsWinMd();
            if (winmd != null)
                return CreateWinMdDefinitionLocation(new BuiltInSymbolProxy(symbol, winmd));
        }

        // Find the definition location in source text
        int localDefOffset = FindDefinitionOffset(sourceText, symbol);
        if (localDefOffset < 0) return null;

        var (localDefLine, localDefChar) = PositionHelper.OffsetToLineChar(lineStarts, localDefOffset);
        var (localDefEndLine, localDefEndChar) = PositionHelper.OffsetToLineChar(lineStarts, localDefOffset + symbol.Name.Length);

        return MakeLocation(uri, localDefLine, localDefChar, localDefEndLine, localDefEndChar);
    }

    /// <summary>
    /// Creates a definition location for a WinMD external symbol by decompiling the WinMD
    /// and returning a location with the midl-winmd URI scheme.
    /// </summary>
    public static object? CreateWinMdDefinitionLocation(ISymbol symbol)
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
            int defOffset = FindDefinitionOffset(decompiled, symbol);
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
            return MakeLocation(winmdUri, dLine, dChar, dEndLine, dEndChar);
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
    private static object? HandleWinMdView(string sourceText, string uri, int line, int character, int[] lineStarts)
    {
        int offset = PositionHelper.LineCharToOffset(lineStarts, line, character);
        var lexer = new Lexer.Lexer(sourceText);
        var tokens = lexer.Lex();
        var (token, tokenOffset) = FindTokenAtOffset(tokens, offset);
        if (token is null || token.Kind != SyntaxKind.IdentifierToken)
            return null;

        string name = token.Text;

        // Extract the WinMD path from the URI: midl-winmd:///path/to/file.winmd/Namespace
        var uriPath = uri["midl-winmd:///".Length..];
        // On Windows, path is like C:/Program Files/.../file.winmd/Namespace
        var winmdIdx = uriPath.IndexOf(".winmd", StringComparison.OrdinalIgnoreCase);
        if (winmdIdx < 0) return null;
        var winmdPath = uriPath[..(winmdIdx + 6)];
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            winmdPath = winmdPath.Replace('/', '\\');

        if (!File.Exists(winmdPath)) return null;

        // Try to find the type in the WinMD by looking up all type names
        var qualifiedName = ExtractQualifiedName(sourceText, tokenOffset, name) ?? name;

        // Search the WinMD for a type matching this name
        try
        {
            using var fs = File.OpenRead(winmdPath);
            using var peReader = new System.Reflection.PortableExecutable.PEReader(fs);
            var reader = peReader.GetMetadataReader();

            foreach (var handle in reader.TypeDefinitions)
            {
                var td = reader.GetTypeDefinition(handle);
                var typeName = reader.GetString(td.Name);
                var typeNs = reader.GetString(td.Namespace);

                // Strip generic arity
                var backtick = typeName.IndexOf('`');
                var cleanName = backtick >= 0 ? typeName[..backtick] : typeName;

                if (cleanName.Equals(name, StringComparison.Ordinal) ||
                    $"{typeNs}.{cleanName}".Equals(qualifiedName, StringComparison.Ordinal))
                {
                    // Found — navigate to this type's namespace decompilation
                    var targetNs = typeNs;
                    var decompiler = new WinMdDecompiler();
                    var decompiled = decompiler.Decompile(winmdPath, targetNs);
                    int defOffset = FindDefinitionOffset(decompiled,
                        new BuiltInSymbolProxy(
                            new Analysis.Symbols.InterfaceSymbol(cleanName, null!) { IsExternal = true },
                            winmdPath)
                        { });

                    // Try struct/enum/delegate/runtimeclass keywords too
                    if (defOffset < 0)
                    {
                        foreach (var kw in new[] { "struct", "enum", "interface", "runtimeclass", "delegate" })
                        {
                            var pattern = kw + " " + cleanName;
                            defOffset = decompiled.IndexOf(pattern, StringComparison.Ordinal);
                            if (defOffset >= 0) { defOffset += kw.Length + 1; break; }
                        }
                    }
                    if (defOffset < 0) defOffset = 0;

                    var decompLineStarts = PositionHelper.BuildLineStarts(decompiled);
                    var (dLine, dChar) = PositionHelper.OffsetToLineChar(decompLineStarts, defOffset);
                    var (dEndLine, dEndChar) = PositionHelper.OffsetToLineChar(decompLineStarts, defOffset + cleanName.Length);

                    var targetUri = "midl-winmd:///" + winmdPath.Replace('\\', '/') + "/" + targetNs;
                    return MakeLocation(targetUri, dLine, dChar, dEndLine, dEndChar);
                }
            }

            // Also check CLR-projected types
            var projected = Analysis.ClrProjectionMap.ToWinRTName(qualifiedName);
            if (projected != qualifiedName)
            {
                var lastDot = projected.LastIndexOf('.');
                var projNs = lastDot >= 0 ? projected[..lastDot] : "";
                var projName = lastDot >= 0 ? projected[(lastDot + 1)..] : projected;
                var targetUri = "midl-winmd:///" + winmdPath.Replace('\\', '/') + "/" + projNs;
                return MakeLocation(targetUri, 0, 0, 0, 0);
            }
        }
        catch { }

        return null;
    }

    private static Dictionary<string, object?> MakeLocation(string uri, int startLine, int startChar, int endLine, int endChar)
    {
        return new Dictionary<string, object?>
        {
            ["uri"] = uri,
            ["range"] = new Dictionary<string, object?>
            {
                ["start"] = new Dictionary<string, object?> { ["line"] = startLine, ["character"] = startChar },
                ["end"] = new Dictionary<string, object?> { ["line"] = endLine, ["character"] = endChar }
            }
        };
    }

    /// <summary>
    /// Checks if the given line is a // @ref: directive and returns the resolved path.
    /// </summary>
    public static string? TryGetRefDirectiveTarget(string sourceText, int[] lineStarts, int line)
    {
        var lineText = GetLineText(sourceText, lineStarts, line).TrimStart();
        if (!lineText.StartsWith("//")) return null;
        var comment = lineText[2..].TrimStart();
        if (!comment.StartsWith("@ref:", StringComparison.OrdinalIgnoreCase)) return null;
        var path = comment[5..].Trim();
        if (path.Length >= 2 && path[0] == '"' && path[^1] == '"')
            path = path[1..^1].Trim();
        return string.IsNullOrEmpty(path) ? null : path;
    }

    /// <summary>
    /// Creates a response for F12 on a @ref line.
    /// For files: returns an LSP Location pointing to the file.
    /// For directories: returns a location with a special URI scheme that the extension handles.
    /// </summary>
    private static object? CreateRefDirectiveResponse(string path, string currentUri, int[] lineStarts, int line)
    {
        // Try to resolve relative to current file
        string? resolvedPath = null;
        if (currentUri.StartsWith("file:///"))
        {
            var filePath = Uri.UnescapeDataString(currentUri[8..]);
            var dir = Path.GetDirectoryName(filePath);
            if (dir != null && !Path.IsPathRooted(path))
                resolvedPath = Path.GetFullPath(Path.Combine(dir, path));
            else
                resolvedPath = path;
        }
        else
        {
            resolvedPath = path;
        }

        if (resolvedPath == null) return null;

        if (Directory.Exists(resolvedPath))
        {
            var dirUri = "file:///" + resolvedPath.Replace('\\', '/');
            return MakeLocation(dirUri, 0, 0, 0, 0);
        }

        if (File.Exists(resolvedPath))
        {
            // For .winmd files, use the decompiled view URI scheme
            if (resolvedPath.EndsWith(".winmd", StringComparison.OrdinalIgnoreCase))
            {
                var winmdUri = "midl-winmd:///" + resolvedPath.Replace('\\', '/');
                return MakeLocation(winmdUri, 0, 0, 0, 0);
            }
            var fileUri = "file:///" + resolvedPath.Replace('\\', '/');
            return MakeLocation(fileUri, 0, 0, 0, 0);
        }

        return null;
    }

    /// <summary>
    /// For an external symbol (from WinMD), find which // @ref: line in the source
    /// could have provided it — matches by WinMD filename or namespace prefix.
    /// </summary>
    public static int FindRefLineForExternalSymbol(string sourceText, ISymbol symbol)
    {
        var winmdPath = symbol.SourceWinMdPath;
        var winmdFileName = winmdPath != null ? Path.GetFileName(winmdPath) : null;
        var ns = symbol.FullName;

        using var reader = new StringReader(sourceText);
        string? line;
        int lineNum = 0;
        while ((line = reader.ReadLine()) != null)
        {
            var trimmed = line.TrimStart();
            if (!trimmed.StartsWith("//"))
            {
                if (trimmed.Length > 0) break; // past comment header
                lineNum++;
                continue;
            }

            var comment = trimmed[2..].TrimStart();
            if (comment.StartsWith("@ref:", StringComparison.OrdinalIgnoreCase))
            {
                var path = comment[5..].Trim();
                if (path.Length >= 2 && path[0] == '"' && path[^1] == '"')
                    path = path[1..^1].Trim();

                // Match by WinMD filename
                if (winmdFileName != null &&
                    Path.GetFileName(path).Equals(winmdFileName, StringComparison.OrdinalIgnoreCase))
                    return lineNum;

                // Match by namespace prefix in directory name or filename
                if (ns != null)
                {
                    var fileNameNoExt = Path.GetFileNameWithoutExtension(path);
                    if (ns.StartsWith(fileNameNoExt, StringComparison.OrdinalIgnoreCase))
                        return lineNum;
                }
            }

            lineNum++;
        }
        return -1;
    }

    private static string GetLineText(string sourceText, int[] lineStarts, int line)
    {
        if (line >= lineStarts.Length) return "";
        int start = lineStarts[line];
        int end = line + 1 < lineStarts.Length ? lineStarts[line + 1] : sourceText.Length;
        // Trim trailing newline
        while (end > start && (sourceText[end - 1] == '\n' || sourceText[end - 1] == '\r'))
            end--;
        return sourceText[start..end];
    }

    private static ISymbol? FindSymbol(Compilation compilation, string name, string sourceText, int tokenOffset)
    {
        // First try top-level types (includes resolved imports)
        var type = compilation.GetAllTypes().FirstOrDefault(s => s.Name == name);
        if (type is not null) return type;

        // Search members recursively (methods, properties, etc.)
        var member = FindMemberRecursive(compilation.GlobalNamespace, name);
        if (member is not null) return member;

        // Try to reconstruct fully qualified name from source context
        // e.g., cursor on "TypedEventHandler" in "Windows.Foundation.TypedEventHandler<...>"
        var qualifiedName = ExtractQualifiedName(sourceText, tokenOffset, name);
        if (qualifiedName != null && qualifiedName != name)
        {
            // Try symbol table first (includes built-in WinRT types)
            var defined = compilation.SymbolTable?.LookupDefinedType(qualifiedName);
            if (defined is not null) return defined;

            var resolved = compilation.GetTypeByName(qualifiedName);
            if (resolved is not null) return resolved;
        }

        // Try lazy WinMD providers — search by simple name across all known types
        var lazyNames = compilation.GetAllLazyTypeNames();
        foreach (var fullName in lazyNames)
        {
            var simpleName = fullName.Contains('.') ? fullName[(fullName.LastIndexOf('.') + 1)..] : fullName;
            if (simpleName.Equals(name, StringComparison.Ordinal))
            {
                var resolved = compilation.GetTypeByName(fullName);
                if (resolved is not null) return resolved;
            }
        }

        return null;
    }

    /// <summary>
    /// Given a token at offset, walk backward through dots and identifiers to reconstruct
    /// the fully qualified name (e.g., "Windows.Foundation.TypedEventHandler").
    /// </summary>
    private static string? ExtractQualifiedName(string text, int tokenOffset, string name)
    {
        // Walk backward from the token to collect dot-separated identifiers
        var parts = new List<string> { name };
        int pos = tokenOffset - 1;

        while (pos >= 0)
        {
            // Skip whitespace
            while (pos >= 0 && char.IsWhiteSpace(text[pos])) pos--;
            if (pos < 0 || text[pos] != '.') break;
            pos--; // skip the dot

            // Collect the identifier before the dot
            int end = pos + 1;
            while (pos >= 0 && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_')) pos--;
            pos++;
            if (pos < end)
                parts.Insert(0, text[pos..end]);
            else
                break;
            pos--;
        }

        return parts.Count > 1 ? string.Join('.', parts) : null;
    }

    /// <summary>
    /// Extracts the full qualified name the cursor is within, walking both backward AND forward.
    /// e.g., cursor on "UI" in "Microsoft.UI.Xaml.Controls.Control" returns the full string.
    /// </summary>
    private static string? ExtractFullQualifiedNameAtCursor(string text, int tokenOffset, string name)
    {
        var parts = new List<string> { name };

        // Walk backward
        int pos = tokenOffset - 1;
        while (pos >= 0)
        {
            while (pos >= 0 && char.IsWhiteSpace(text[pos])) pos--;
            if (pos < 0 || text[pos] != '.') break;
            pos--;
            int end = pos + 1;
            while (pos >= 0 && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_')) pos--;
            pos++;
            if (pos < end) parts.Insert(0, text[pos..end]); else break;
            pos--;
        }

        // Walk forward
        pos = tokenOffset + name.Length;
        while (pos < text.Length)
        {
            if (text[pos] != '.') break;
            pos++; // skip dot
            int start = pos;
            while (pos < text.Length && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_')) pos++;
            if (pos > start) parts.Add(text[start..pos]); else break;
        }

        return parts.Count > 1 ? string.Join('.', parts) : null;
    }

    private static ISymbol? FindMemberRecursive(NamespaceSymbol ns, string name)
    {
        foreach (var member in ns.Members)
        {
            if (member.Name == name) return member;

            var children = GetMembers(member);
            if (children is not null)
            {
                foreach (var child in children)
                {
                    if (child.Name == name) return child;
                }
            }

            if (member is NamespaceSymbol childNs)
            {
                var found = FindMemberRecursive(childNs, name);
                if (found is not null) return found;
            }
        }
        return null;
    }

    private static IReadOnlyList<ISymbol>? GetMembers(ISymbol symbol) => symbol switch
    {
        RuntimeClassSymbol rc => rc.Members,
        InterfaceSymbol iface => iface.Members,
        _ => null
    };

    /// <summary>
    /// Finds the offset of the symbol's defining identifier in source text.
    /// Uses keyword + name pattern matching (same approach as DocumentSymbolHandler).
    /// </summary>
    internal static int FindDefinitionOffset(string sourceText, ISymbol symbol)
    {
        string? keyword = symbol.Kind switch
        {
            SymbolKind.RuntimeClass => "runtimeclass",
            SymbolKind.Interface => "interface",
            SymbolKind.Struct => "struct",
            SymbolKind.Enum => "enum",
            SymbolKind.Delegate => null, // delegate has return type between keyword and name
            SymbolKind.ApiContract => "apicontract",
            SymbolKind.Namespace => "namespace",
            _ => null
        };

        if (keyword is not null)
        {
            string pattern = keyword + " " + symbol.Name;
            int idx = sourceText.IndexOf(pattern, StringComparison.Ordinal);
            if (idx >= 0)
                return idx + keyword.Length + 1;
        }

        // For type-level symbols, if the keyword pattern wasn't found, the type isn't defined here.
        // Don't fall back to usage-site matching (would find property types, method return types, etc.)
        if (keyword is not null)
            return -1;

        // For delegates: search for "delegate <returntype> <name>" pattern
        if (symbol.Kind == SymbolKind.Delegate)
        {
            int searchFrom = 0;
            while (true)
            {
                int delegateIdx = sourceText.IndexOf("delegate ", searchFrom, StringComparison.Ordinal);
                if (delegateIdx < 0) break;
                int afterDelegate = delegateIdx + 9;
                int nameIdx = sourceText.IndexOf(symbol.Name, afterDelegate, StringComparison.Ordinal);
                if (nameIdx >= 0 && nameIdx - afterDelegate < 40)
                    return nameIdx;
                searchFrom = afterDelegate;
            }
            return -1; // delegate not found either
        }

        // Fallback for members only (methods, properties, etc.): find with word boundary
        for (int i = sourceText.IndexOf(symbol.Name, StringComparison.Ordinal);
             i >= 0;
             i = sourceText.IndexOf(symbol.Name, i + 1, StringComparison.Ordinal))
        {
            int afterName = i + symbol.Name.Length;
            if (afterName < sourceText.Length && char.IsLetterOrDigit(sourceText[afterName]))
                continue;
            return i;
        }
        return -1;
    }

    private static (SyntaxToken? Token, int TextStart) FindTokenAtOffset(IReadOnlyList<SyntaxToken> tokens, int offset)
    {
        int pos = 0;
        foreach (var token in tokens)
        {
            int leadingWidth = token.LeadingTrivia.Sum(t => t.FullWidth);
            int textStart = pos + leadingWidth;
            int textEnd = textStart + token.Text.Length;
            int trailingWidth = token.TrailingTrivia.Sum(t => t.FullWidth);

            if (offset >= textStart && offset < textEnd)
                return (token, textStart);

            pos = textEnd + trailingWidth;
        }
        return (null, 0);
    }

    /// <summary>Find Windows.winmd from the SDK.</summary>
    private static string? FindWindowsWinMd()
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
    private sealed class BuiltInSymbolProxy : ISymbol
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
