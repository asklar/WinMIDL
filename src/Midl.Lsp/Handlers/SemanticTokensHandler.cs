using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.Syntax;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Handles textDocument/semanticTokens/full requests — provides rich semantic
/// token classification for syntax highlighting beyond TextMate grammars.
/// </summary>
public static class SemanticTokensHandler
{
    // Standard LSP semantic token type names in legend order
    public static readonly string[] TokenTypes =
    [
        "namespace",    // 0
        "type",         // 1
        "interface",    // 2
        "enum",         // 3
        "enumMember",   // 4
        "method",       // 5
        "property",     // 6
        "event",        // 7
        "parameter",    // 8
        "keyword",      // 9
        "comment",      // 10
        "string",       // 11
        "number",       // 12
        "struct",       // 13
    ];

    public static readonly string[] TokenModifiers = ["declaration", "static"];

    private const int TypeNamespace = 0;
    private const int TypeType = 1;
    private const int TypeInterface = 2;
    private const int TypeEnum = 3;
    private const int TypeEnumMember = 4;
    private const int TypeMethod = 5;
    private const int TypeProperty = 6;
    private const int TypeEvent = 7;
    private const int TypeParameter = 8;
    private const int TypeKeyword = 9;
    private const int TypeComment = 10;
    private const int TypeString = 11;
    private const int TypeNumber = 12;
    private const int TypeStruct = 13;

    public static object? Handle(JsonElement? @params, DocumentManager docManager)
    {
        if (@params is null) return null;

        var textDocument = @params.Value.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;

        var text = docManager.GetDocumentText(uri);
        if (text is null) return new Dictionary<string, object?> { ["data"] = Array.Empty<int>() };

        var (data, _, _) = HandleCore(text);
        return new Dictionary<string, object?> { ["data"] = data };
    }

    /// <summary>
    /// Core logic, testable without LSP JSON plumbing.
    /// Returns delta-encoded semantic tokens data, plus the legend arrays.
    /// </summary>
    public static (int[] Data, string[] TokenTypeNames, string[] TokenModifierNames) HandleCore(string sourceText)
    {
        var lexer = new Lexer.Lexer(sourceText);
        var tokens = lexer.Lex();
        var lineStarts = PositionHelper.BuildLineStarts(sourceText);

        // Try to compile for symbol resolution; if it fails, classify without symbols
        Compilation? compilation = null;
        try
        {
            compilation = CompilationHelper.Compile(sourceText);
        }
        catch
        {
            // Continue without symbol info
        }

        var allTypes = compilation?.GetAllTypes().ToList();

        var encodedTokens = new List<int>();
        int prevLine = 0;
        int prevChar = 0;

        int pos = 0;
        foreach (var token in tokens)
        {
            // Classify trivia (comments)
            foreach (var trivia in token.LeadingTrivia)
            {
                if (trivia.Kind is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia
                    && trivia is SyntaxTrivia st)
                {
                    var (tLine, tChar) = PositionHelper.OffsetToLineChar(lineStarts, pos);
                    int deltaLine = tLine - prevLine;
                    int deltaChar = deltaLine == 0 ? tChar - prevChar : tChar;
                    encodedTokens.AddRange([deltaLine, deltaChar, st.Text.Length, TypeComment, 0]);
                    prevLine = tLine;
                    prevChar = tChar;
                }
                pos += trivia.FullWidth;
            }

            int textStart = pos;

            // Classify the token itself
            int tokenType = ClassifyToken(token, allTypes);
            if (tokenType >= 0 && token.Text.Length > 0)
            {
                var (tLine, tChar) = PositionHelper.OffsetToLineChar(lineStarts, textStart);
                int deltaLine = tLine - prevLine;
                int deltaChar = deltaLine == 0 ? tChar - prevChar : tChar;
                encodedTokens.AddRange([deltaLine, deltaChar, token.Text.Length, tokenType, 0]);
                prevLine = tLine;
                prevChar = tChar;
            }

            pos = textStart + token.Text.Length;

            // Skip trailing trivia (don't classify)
            foreach (var trivia in token.TrailingTrivia)
            {
                pos += trivia.FullWidth;
            }
        }

        return (encodedTokens.ToArray(), TokenTypes, TokenModifiers);
    }

    private static int ClassifyToken(SyntaxToken token, List<ISymbol>? allTypes)
    {
        // Keywords
        if (IsKeyword(token.Kind))
            return TypeKeyword;

        // String literals
        if (token.Kind == SyntaxKind.StringLiteralToken)
            return TypeString;

        // Numeric and float literals
        if (token.Kind is SyntaxKind.NumericLiteralToken or SyntaxKind.FloatLiteralToken)
            return TypeNumber;

        // Identifiers — resolve against symbols if available
        if (token.Kind == SyntaxKind.IdentifierToken && allTypes is not null)
        {
            return ClassifyIdentifier(token.Text, allTypes);
        }

        return -1; // Don't emit a token for punctuation, etc.
    }

    private static int ClassifyIdentifier(string name, List<ISymbol> allTypes)
    {
        // Check top-level types first
        var symbol = allTypes.FirstOrDefault(s => s.Name == name);
        if (symbol is not null)
        {
            return symbol.Kind switch
            {
                Analysis.Symbols.SymbolKind.Interface => TypeInterface,
                Analysis.Symbols.SymbolKind.Enum => TypeEnum,
                Analysis.Symbols.SymbolKind.Struct => TypeStruct,
                Analysis.Symbols.SymbolKind.Namespace => TypeNamespace,
                _ => TypeType // RuntimeClass, Delegate, etc.
            };
        }

        // Check members of all types
        foreach (var type in allTypes)
        {
            var members = type switch
            {
                RuntimeClassSymbol rc => rc.Members,
                InterfaceSymbol iface => iface.Members,
                _ => null as IReadOnlyList<ISymbol>
            };

            if (members is null) continue;

            var member = members.FirstOrDefault(m => m.Name == name);
            if (member is not null)
            {
                return member.Kind switch
                {
                    Analysis.Symbols.SymbolKind.Method or Analysis.Symbols.SymbolKind.Constructor => TypeMethod,
                    Analysis.Symbols.SymbolKind.Property => TypeProperty,
                    Analysis.Symbols.SymbolKind.Event => TypeEvent,
                    Analysis.Symbols.SymbolKind.Field => TypeProperty,
                    _ => TypeType
                };
            }

            if (type is EnumSymbol enumSym)
            {
                if (enumSym.EnumMembers.Any(em => em.Name == name))
                    return TypeEnumMember;
            }
        }

        return TypeType; // Default unresolved identifiers to type
    }

    private static bool IsKeyword(SyntaxKind kind) => kind is
        SyntaxKind.NamespaceKeyword or SyntaxKind.RuntimeclassKeyword or
        SyntaxKind.InterfaceKeyword or SyntaxKind.DelegateKeyword or
        SyntaxKind.StructKeyword or SyntaxKind.EnumKeyword or
        SyntaxKind.AttributeKeyword or SyntaxKind.ApicontractKeyword or
        SyntaxKind.EventKeyword or SyntaxKind.ImportKeyword or
        SyntaxKind.DeclareKeyword or SyntaxKind.StaticKeyword or
        SyntaxKind.PartialKeyword or SyntaxKind.UnsealedKeyword or
        SyntaxKind.ProtectedKeyword or SyntaxKind.OverridableKeyword or
        SyntaxKind.OutKeyword or SyntaxKind.RefKeyword or
        SyntaxKind.ConstKeyword or SyntaxKind.GetKeyword or
        SyntaxKind.SetKeyword or SyntaxKind.RequiresKeyword or
        SyntaxKind.ContractKeyword or SyntaxKind.FlagsKeyword or
        SyntaxKind.VoidKeyword or SyntaxKind.BooleanKeyword or
        SyntaxKind.CharKeyword or SyntaxKind.Int16Keyword or
        SyntaxKind.Int32Keyword or SyntaxKind.Int64Keyword or
        SyntaxKind.UInt8Keyword or SyntaxKind.UInt16Keyword or
        SyntaxKind.UInt32Keyword or SyntaxKind.UInt64Keyword or
        SyntaxKind.SingleKeyword or SyntaxKind.DoubleKeyword or
        SyntaxKind.StringKeyword or SyntaxKind.GuidKeyword or
        SyntaxKind.ObjectKeyword or SyntaxKind.TrueKeyword or
        SyntaxKind.FalseKeyword or SyntaxKind.NullKeyword;
}
