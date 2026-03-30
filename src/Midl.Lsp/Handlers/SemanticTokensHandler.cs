using System.Collections.Concurrent;
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
        "decorator",    // 14
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
    private const int TypeDecorator = 14;

    // Cache: avoid re-lexing/re-classifying when the document hasn't changed
    private static readonly ConcurrentDictionary<string, (int hash, int[] data)> _cache = new();

    /// <summary>Clear the semantic token cache (e.g., when preprocessor defines change).</summary>
    public static void ClearCache() => _cache.Clear();

    public static object? Handle(JsonElement? @params, DocumentManager docManager, Compilation? cachedCompilation = null)
    {
        if (@params is null) return null;

        var textDocument = @params.Value.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;

        var text = docManager.GetDocumentText(uri);
        if (text is null) return new Dictionary<string, object?> { ["data"] = Array.Empty<int>() };

        // Include inactive regions and defines in hash so settings changes invalidate cache
        int inactiveCount = cachedCompilation?.InactiveRegions?.Count ?? -1;
        int definesHash = 0;
        if (cachedCompilation?.InactiveRegions != null)
            foreach (var (s, e) in cachedCompilation.InactiveRegions)
                definesHash = HashCode.Combine(definesHash, s, e);
        var hash = HashCode.Combine(text.GetHashCode(), inactiveCount, definesHash);
        if (_cache.TryGetValue(uri, out var cached) && cached.hash == hash)
        {
            return new Dictionary<string, object?> { ["data"] = cached.data };
        }

        var (data, _, _) = HandleCore(text, cachedCompilation);
        _cache[uri] = (hash, data);
        return new Dictionary<string, object?> { ["data"] = data };
    }

    /// <summary>
    /// Core logic, testable without LSP JSON plumbing.
    /// Returns delta-encoded semantic tokens data, plus the legend arrays.
    /// </summary>
    public static (int[] Data, string[] TokenTypeNames, string[] TokenModifierNames) HandleCore(string sourceText, Compilation? cachedCompilation = null)
    {
        var lexer = new Lexer.Lexer(sourceText);
        var tokens = lexer.Lex();
        var lineStarts = PositionHelper.BuildLineStarts(sourceText);

        // Use cached compilation if available (normal path — analysis runs on didOpen/didChange).
        // Fall back to a lightweight compile (no SDK references) if cache is cold.
        Compilation? compilation = cachedCompilation;
        if (compilation == null)
        {
            try
            {
                // Compile with no references to avoid loading SDK — just local types
                compilation = CompilationHelper.Compile(sourceText);
            }
            catch
            {
                // Continue without symbol info
            }
        }

        var allTypes = compilation?.GetAllTypes().ToList();
        var inactiveRegions = compilation?.InactiveRegions;

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

                    // Check for // @ref: directive — split into decorator + string tokens
                    if (trivia.Kind == SyntaxKind.SingleLineCommentTrivia && TryParseRefDirective(st.Text, out var slashLen, out var refLen, out var pathStart, out var pathLen))
                    {
                        // "// " part as comment
                        int deltaLine = tLine - prevLine;
                        int deltaChar = deltaLine == 0 ? tChar - prevChar : tChar;
                        encodedTokens.AddRange([deltaLine, deltaChar, slashLen, TypeComment, 0]);
                        prevLine = tLine;
                        prevChar = tChar;

                        // "@ref:" part as decorator
                        int refChar = tChar + slashLen;
                        deltaLine = 0;
                        deltaChar = refChar - prevChar;
                        encodedTokens.AddRange([deltaLine, deltaChar, refLen, TypeDecorator, 0]);
                        prevChar = refChar;

                        // path part as string
                        if (pathLen > 0)
                        {
                            int pathChar = tChar + pathStart;
                            deltaChar = pathChar - prevChar;
                            encodedTokens.AddRange([0, deltaChar, pathLen, TypeString, 0]);
                            prevChar = pathChar;
                        }
                    }
                    else
                    {
                        int deltaLine = tLine - prevLine;
                        int deltaChar = deltaLine == 0 ? tChar - prevChar : tChar;
                        encodedTokens.AddRange([deltaLine, deltaChar, st.Text.Length, TypeComment, 0]);
                        prevLine = tLine;
                        prevChar = tChar;
                    }
                }
                pos += trivia.FullWidth;
            }

            int textStart = pos;

            // Classify the token itself
            int tokenType = ClassifyToken(token, allTypes);
            bool isInactive = inactiveRegions != null && token.Text.Length > 0 &&
                              IsInInactiveRegion(textStart, inactiveRegions);

            if (isInactive && token.Text.Length > 0)
            {
                // Force all tokens in inactive regions to comment (including punctuation)
                var (tLine, tChar) = PositionHelper.OffsetToLineChar(lineStarts, textStart);
                int deltaLine = tLine - prevLine;
                int deltaChar = deltaLine == 0 ? tChar - prevChar : tChar;
                encodedTokens.AddRange([deltaLine, deltaChar, token.Text.Length, TypeComment, 0]);
                prevLine = tLine;
                prevChar = tChar;
            }
            else if (tokenType >= 0 && token.Text.Length > 0)
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

    private static bool IsInInactiveRegion(int position, IReadOnlyList<(int Start, int End)> regions)
    {
        foreach (var (start, end) in regions)
        {
            if (position >= start && position < end) return true;
            if (start > position) break; // regions are sorted by start
        }
        return false;
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

    /// <summary>
    /// Checks if a comment is a // @ref: directive and returns the spans of each part.
    /// </summary>
    private static bool TryParseRefDirective(string text, out int slashLen, out int refLen, out int pathStart, out int pathLen)
    {
        slashLen = refLen = pathStart = pathLen = 0;
        if (!text.StartsWith("//")) return false;

        int i = 2;
        while (i < text.Length && text[i] == ' ') i++;
        if (i + 5 > text.Length) return false;
        if (!text.AsSpan(i).StartsWith("@ref:", StringComparison.OrdinalIgnoreCase)) return false;

        slashLen = i;       // "// " (including spaces)
        refLen = 5;         // "@ref:"
        int afterRef = i + 5;
        while (afterRef < text.Length && text[afterRef] == ' ') afterRef++;
        pathStart = afterRef;
        pathLen = text.Length - afterRef;
        return true;
    }
}
