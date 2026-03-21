using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.Syntax;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Handles textDocument/definition requests — navigates to where a symbol is defined.
/// </summary>
public static class GotoDefinitionHandler
{
    public static object? Handle(JsonElement? @params, DocumentManager docManager)
    {
        if (@params is null) return null;

        var textDocument = @params.Value.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;
        var position = @params.Value.GetProperty("position");
        int line = position.GetProperty("line").GetInt32();
        int character = position.GetProperty("character").GetInt32();

        var text = docManager.GetDocumentText(uri);
        if (text is null) return null;

        return HandleCore(text, uri, line, character);
    }

    /// <summary>
    /// Core logic, testable without LSP JSON plumbing.
    /// Returns an LSP Location object, a command response, or null.
    /// </summary>
    public static object? HandleCore(string sourceText, string uri, int line, int character)
    {
        var lineStarts = PositionHelper.BuildLineStarts(sourceText);

        // Check if cursor is on a // @ref: line → navigate to the referenced file/folder
        var refTarget = TryGetRefDirectiveTarget(sourceText, lineStarts, line);
        if (refTarget != null)
            return CreateRefDirectiveResponse(refTarget, uri, lineStarts, line);

        int offset = PositionHelper.LineCharToOffset(lineStarts, line, character);

        var lexer = new Lexer.Lexer(sourceText);
        var tokens = lexer.Lex();
        var (token, _) = FindTokenAtOffset(tokens, offset);
        if (token is null || token.Kind != SyntaxKind.IdentifierToken)
            return null;

        string name = token.Text;

        Compilation compilation;
        try
        {
            compilation = CompilationHelper.Compile(sourceText);
        }
        catch
        {
            return null;
        }

        // Search for the symbol in compilation
        var symbol = FindSymbol(compilation, name);
        if (symbol is null) return null;

        // External symbols: navigate to the @ref line that provides them
        if (symbol.IsExternal)
        {
            var refLine = FindRefLineForExternalSymbol(sourceText, symbol);
            if (refLine >= 0)
            {
                var (rl, rc) = PositionHelper.OffsetToLineChar(lineStarts,
                    PositionHelper.LineCharToOffset(lineStarts, refLine, 0));
                var lineText = GetLineText(sourceText, lineStarts, refLine);
                return new Dictionary<string, object?>
                {
                    ["uri"] = uri,
                    ["range"] = new Dictionary<string, object?>
                    {
                        ["start"] = new Dictionary<string, object?> { ["line"] = refLine, ["character"] = 0 },
                        ["end"] = new Dictionary<string, object?> { ["line"] = refLine, ["character"] = lineText.Length }
                    }
                };
            }
            return null;
        }

        // Find the definition location in source text
        int defOffset = FindDefinitionOffset(sourceText, symbol);
        if (defOffset < 0) return null;

        var (defLine, defChar) = PositionHelper.OffsetToLineChar(lineStarts, defOffset);
        var (defEndLine, defEndChar) = PositionHelper.OffsetToLineChar(lineStarts, defOffset + symbol.Name.Length);

        return new Dictionary<string, object?>
        {
            ["uri"] = uri,
            ["range"] = new Dictionary<string, object?>
            {
                ["start"] = new Dictionary<string, object?> { ["line"] = defLine, ["character"] = defChar },
                ["end"] = new Dictionary<string, object?> { ["line"] = defEndLine, ["character"] = defEndChar }
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
            // For directories, return a file URI to the directory (VS Code will open it in explorer)
            var dirUri = "file:///" + resolvedPath.Replace('\\', '/');
            return new Dictionary<string, object?>
            {
                ["uri"] = dirUri,
                ["range"] = new Dictionary<string, object?>
                {
                    ["start"] = new Dictionary<string, object?> { ["line"] = 0, ["character"] = 0 },
                    ["end"] = new Dictionary<string, object?> { ["line"] = 0, ["character"] = 0 }
                }
            };
        }

        if (File.Exists(resolvedPath))
        {
            var fileUri = "file:///" + resolvedPath.Replace('\\', '/');
            return new Dictionary<string, object?>
            {
                ["uri"] = fileUri,
                ["range"] = new Dictionary<string, object?>
                {
                    ["start"] = new Dictionary<string, object?> { ["line"] = 0, ["character"] = 0 },
                    ["end"] = new Dictionary<string, object?> { ["line"] = 0, ["character"] = 0 }
                }
            };
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

    private static ISymbol? FindSymbol(Compilation compilation, string name)
    {
        // First try top-level types
        var type = compilation.GetAllTypes().FirstOrDefault(s => s.Name == name);
        if (type is not null) return type;

        // Then search members recursively
        return FindMemberRecursive(compilation.GlobalNamespace, name);
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
    private static int FindDefinitionOffset(string sourceText, ISymbol symbol)
    {
        string? keyword = symbol.Kind switch
        {
            SymbolKind.RuntimeClass => "runtimeclass",
            SymbolKind.Interface => "interface",
            SymbolKind.Struct => "struct",
            SymbolKind.Enum => "enum",
            SymbolKind.Delegate => "delegate",
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

        // For members (methods, properties, etc.), find the name in context
        int nameIdx = sourceText.IndexOf(symbol.Name, StringComparison.Ordinal);
        return nameIdx;
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
}
