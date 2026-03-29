using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.Syntax;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Handles textDocument/rename and textDocument/prepareRename requests.
/// </summary>
public static class RenameHandler
{
    public static object? Handle(JsonElement? @params, DocumentManager docManager, Compilation? cachedCompilation = null)
    {
        if (@params is null) return null;

        var textDocument = @params.Value.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;
        var position = @params.Value.GetProperty("position");
        int line = position.GetProperty("line").GetInt32();
        int character = position.GetProperty("character").GetInt32();
        var newName = @params.Value.GetProperty("newName").GetString()!;

        var text = docManager.GetDocumentText(uri);
        if (text is null) return null;

        return HandleCore(text, uri, line, character, newName, cachedCompilation);
    }

    public static object? HandlePrepare(JsonElement? @params, DocumentManager docManager, Compilation? cachedCompilation = null)
    {
        if (@params is null) return null;

        var textDocument = @params.Value.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;
        var position = @params.Value.GetProperty("position");
        int line = position.GetProperty("line").GetInt32();
        int character = position.GetProperty("character").GetInt32();

        var text = docManager.GetDocumentText(uri);
        if (text is null) return null;

        return HandlePrepareCore(text, line, character, cachedCompilation);
    }

    /// <summary>
    /// Core rename logic, testable without LSP JSON plumbing.
    /// Returns a WorkspaceEdit or null if rename is not possible.
    /// </summary>
    public static object? HandleCore(string sourceText, string uri, int line, int character, string newName, Compilation? cachedCompilation = null)
    {
        var lineStarts = PositionHelper.BuildLineStarts(sourceText);
        int offset = PositionHelper.LineCharToOffset(lineStarts, line, character);

        var lexer = new Lexer.Lexer(sourceText);
        var tokens = lexer.Lex();
        var (token, _) = FindTokenAtOffset(tokens, offset);
        if (token is null || token.Kind != SyntaxKind.IdentifierToken)
            return null;

        string name = token.Text;

        Compilation compilation;
        if (cachedCompilation != null)
        {
            compilation = cachedCompilation;
        }
        else
        {
            try
            {
                compilation = CompilationHelper.Compile(sourceText);
            }
            catch
            {
                return null;
            }
        }

        var symbol = FindSymbol(compilation, name);
        if (symbol is null || symbol.IsExternal)
            return null;

        // Build TextEdits for all identifier tokens matching this symbol name
        var edits = new List<object>();
        int pos = 0;
        foreach (var tok in tokens)
        {
            int leadingWidth = tok.LeadingTrivia.Sum(t => t.FullWidth);
            int textStart = pos + leadingWidth;
            int trailingWidth = tok.TrailingTrivia.Sum(t => t.FullWidth);

            if (tok.Kind == SyntaxKind.IdentifierToken && tok.Text == name)
            {
                var (startLine, startChar) = PositionHelper.OffsetToLineChar(lineStarts, textStart);
                var (endLine, endChar) = PositionHelper.OffsetToLineChar(lineStarts, textStart + tok.Text.Length);

                edits.Add(new Dictionary<string, object?>
                {
                    ["range"] = new Dictionary<string, object?>
                    {
                        ["start"] = new Dictionary<string, object?> { ["line"] = startLine, ["character"] = startChar },
                        ["end"] = new Dictionary<string, object?> { ["line"] = endLine, ["character"] = endChar }
                    },
                    ["newText"] = newName
                });
            }

            pos = textStart + tok.Text.Length + trailingWidth;
        }

        if (edits.Count == 0) return null;

        return new Dictionary<string, object?>
        {
            ["changes"] = new Dictionary<string, object?>
            {
                [uri] = edits
            }
        };
    }

    /// <summary>
    /// Validates that a rename is possible at the given position.
    /// Returns the range of the symbol token, or null if not renameable.
    /// </summary>
    public static object? HandlePrepareCore(string sourceText, int line, int character, Compilation? cachedCompilation = null)
    {
        var lineStarts = PositionHelper.BuildLineStarts(sourceText);
        int offset = PositionHelper.LineCharToOffset(lineStarts, line, character);

        var lexer = new Lexer.Lexer(sourceText);
        var tokens = lexer.Lex();
        var (token, textStart) = FindTokenAtOffset(tokens, offset);
        if (token is null || token.Kind != SyntaxKind.IdentifierToken)
            return null;

        string name = token.Text;

        Compilation compilation;
        if (cachedCompilation != null)
        {
            compilation = cachedCompilation;
        }
        else
        {
            try
            {
                compilation = CompilationHelper.Compile(sourceText);
            }
            catch
            {
                return null;
            }
        }

        var symbol = FindSymbol(compilation, name);
        if (symbol is null || symbol.IsExternal)
            return null;

        var (startLine, startChar) = PositionHelper.OffsetToLineChar(lineStarts, textStart);
        var (endLine, endChar) = PositionHelper.OffsetToLineChar(lineStarts, textStart + token.Text.Length);

        return new Dictionary<string, object?>
        {
            ["range"] = new Dictionary<string, object?>
            {
                ["start"] = new Dictionary<string, object?> { ["line"] = startLine, ["character"] = startChar },
                ["end"] = new Dictionary<string, object?> { ["line"] = endLine, ["character"] = endChar }
            },
            ["placeholder"] = name
        };
    }

    private static ISymbol? FindSymbol(Compilation compilation, string name)
    {
        var type = compilation.GetAllTypes().FirstOrDefault(s => s.Name == name);
        if (type is not null) return type;

        return FindMemberRecursive(compilation.GlobalNamespace, name);
    }

    private static ISymbol? FindMemberRecursive(NamespaceSymbol ns, string name)
    {
        foreach (var member in ns.Members)
        {
            if (member.Name == name) return member;

            var children = member switch
            {
                RuntimeClassSymbol rc => rc.Members,
                InterfaceSymbol iface => iface.Members,
                _ => null as IReadOnlyList<ISymbol>
            };

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
