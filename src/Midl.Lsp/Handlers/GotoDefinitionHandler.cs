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
    /// Returns an LSP Location object or null.
    /// </summary>
    public static object? HandleCore(string sourceText, string uri, int line, int character)
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

        // External symbols can't be navigated to
        if (symbol.IsExternal) return null;

        // Find the definition location in source text
        int defOffset = FindDefinitionOffset(sourceText, symbol);
        if (defOffset < 0) return null;

        var (defLine, defChar) = PositionHelper.OffsetToLineChar(lineStarts, defOffset);
        var (defEndLine, defEndChar) = PositionHelper.OffsetToLineChar(lineStarts, defOffset + symbol.Name.Length);

        return new
        {
            uri,
            range = new
            {
                start = new { line = defLine, character = defChar },
                end = new { line = defEndLine, character = defEndChar }
            }
        };
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
