using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.Syntax;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Handles textDocument/references requests — finds all references to a symbol.
/// </summary>
public static class FindReferencesHandler
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
    /// Returns a list of LSP Location objects.
    /// </summary>
    public static List<object> HandleCore(string sourceText, string uri, int line, int character)
    {
        var lineStarts = PositionHelper.BuildLineStarts(sourceText);
        int offset = PositionHelper.LineCharToOffset(lineStarts, line, character);

        var lexer = new Lexer.Lexer(sourceText);
        var tokens = lexer.Lex();
        var (token, _) = FindTokenAtOffset(tokens, offset);
        if (token is null || token.Kind != SyntaxKind.IdentifierToken)
            return [];

        string name = token.Text;

        Compilation compilation;
        try
        {
            compilation = CompilationHelper.Compile(sourceText);
        }
        catch
        {
            return [];
        }

        // Verify the symbol exists
        var symbol = FindSymbol(compilation, name);
        if (symbol is null) return [];

        // Walk all tokens to find matching identifiers
        var results = new List<object>();
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

                results.Add(new
                {
                    uri,
                    range = new
                    {
                        start = new { line = startLine, character = startChar },
                        end = new { line = endLine, character = endChar }
                    }
                });
            }

            pos = textStart + tok.Text.Length + trailingWidth;
        }

        return results;
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
