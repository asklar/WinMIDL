using Midl.Syntax;

namespace Midl.Parser;

/// <summary>
/// Error-recovering recursive-descent parser for MIDL3.
/// Accepts a token stream and produces a lossless green syntax tree.
/// </summary>
public sealed partial class Parser
{
    private readonly IReadOnlyList<SyntaxToken> _tokens;
    private int _position;

    private static readonly SyntaxToken s_eof = new(SyntaxKind.EndOfFileToken, "");

    public Parser(IReadOnlyList<SyntaxToken> tokens)
    {
        _tokens = tokens;
        _position = 0;
    }

    /// <summary>Returns the current token without consuming it.</summary>
    private SyntaxToken Current => _position < _tokens.Count ? _tokens[_position] : s_eof;

    /// <summary>Look ahead by offset tokens from the current position.</summary>
    private SyntaxToken Peek(int offset)
    {
        int index = _position + offset;
        return index < _tokens.Count ? _tokens[index] : s_eof;
    }

    /// <summary>Check if the current token has the given kind.</summary>
    private bool At(SyntaxKind kind) => Current.Kind == kind;

    /// <summary>Check if the current token matches any of the given kinds.</summary>
    private bool AtAny(params SyntaxKind[] kinds)
    {
        var current = Current.Kind;
        foreach (var k in kinds)
        {
            if (current == k) return true;
        }
        return false;
    }

    /// <summary>Check if current token is an identifier with the given text (for contextual keywords).</summary>
    private bool AtContextual(string text)
        => Current.Kind == SyntaxKind.IdentifierToken &&
           string.Equals(Current.Text, text, StringComparison.Ordinal);

    /// <summary>Consume and return the current token if it matches the expected kind, else return a missing token.</summary>
    private SyntaxToken Eat(SyntaxKind expected)
    {
        if (Current.Kind == expected)
        {
            return EatToken();
        }
        return CreateMissingToken(expected);
    }

    /// <summary>Consume and return the current token unconditionally.</summary>
    private SyntaxToken EatToken()
    {
        var token = Current;
        _position++;
        return token;
    }

    /// <summary>Create a synthetic missing token for error recovery.</summary>
    private SyntaxToken CreateMissingToken(SyntaxKind expected)
    {
        string expectedName = expected switch {
            SyntaxKind.SemicolonToken => ";",
            SyntaxKind.OpenBraceToken => "{",
            SyntaxKind.CloseBraceToken => "}",
            SyntaxKind.OpenParenToken => "(",
            SyntaxKind.CloseParenToken => ")",
            SyntaxKind.OpenBracketToken => "[",
            SyntaxKind.CloseBracketToken => "]",
            SyntaxKind.GreaterThanToken => ">",
            SyntaxKind.IdentifierToken => "identifier",
            _ => expected.ToString()
        };
        return new SyntaxToken(SyntaxKind.MissingToken, "", value: expectedName);
    }

    /// <summary>Build a green node list from a kind and variable children.</summary>
    private static GreenNodeList MakeNode(SyntaxKind kind, params GreenNode?[] children)
    {
        return new GreenNodeList(kind, children);
    }

    /// <summary>Build a green node list from a kind and a list of children.</summary>
    private static GreenNodeList MakeNode(SyntaxKind kind, List<GreenNode?> children)
    {
        return new GreenNodeList(kind, [.. children]);
    }

    /// <summary>Skip tokens until we find one of the stop kinds. Returns collected tokens as a SkippedTokens node, or null.</summary>
    private GreenNode? SkipTokensUntil(params SyntaxKind[] stopKinds)
    {
        if (MidlTrace.IsEnabled)
            MidlTrace.Parser($"Error recovery: skipping to {stopKinds[0]} at token {_position}");
        var skipped = new List<GreenNode>();
        while (!At(SyntaxKind.EndOfFileToken) && !AtAny(stopKinds))
        {
            skipped.Add(EatToken());
        }
        if (skipped.Count == 0) return null;
        return new GreenNodeList(SyntaxKind.SkippedTokens, [.. skipped]);
    }

    /// <summary>Skip to the start of the next declaration-level construct.</summary>
    private GreenNode? SkipToDeclarationStart()
    {
        return SkipTokensUntil(
            SyntaxKind.NamespaceKeyword,
            SyntaxKind.RuntimeclassKeyword,
            SyntaxKind.InterfaceKeyword,
            SyntaxKind.StructKeyword,
            SyntaxKind.EnumKeyword,
            SyntaxKind.DelegateKeyword,
            SyntaxKind.ApicontractKeyword,
            SyntaxKind.AttributeKeyword,
            SyntaxKind.OpenBracketToken,
            SyntaxKind.CloseBraceToken,
            SyntaxKind.ImportKeyword,
            SyntaxKind.DeclareKeyword,
            SyntaxKind.CoclassKeyword,
            SyntaxKind.LibraryKeyword,
            SyntaxKind.TypedefKeyword,
            SyntaxKind.CppQuoteKeyword,
            SyntaxKind.UnionKeyword,
            SyntaxKind.DispinterfaceKeyword,
            SyntaxKind.ImportlibKeyword,
            SyntaxKind.EndOfFileToken);
    }

    /// <summary>Check if the current token can start a declaration.</summary>
    private bool IsDeclarationStart()
    {
        return AtAny(
            SyntaxKind.NamespaceKeyword,
            SyntaxKind.RuntimeclassKeyword,
            SyntaxKind.InterfaceKeyword,
            SyntaxKind.StructKeyword,
            SyntaxKind.EnumKeyword,
            SyntaxKind.DelegateKeyword,
            SyntaxKind.ApicontractKeyword,
            SyntaxKind.AttributeKeyword,
            SyntaxKind.OpenBracketToken,
            SyntaxKind.ImportKeyword,
            SyntaxKind.DeclareKeyword,
            SyntaxKind.StaticKeyword,
            SyntaxKind.PartialKeyword,
            SyntaxKind.UnsealedKeyword,
            SyntaxKind.CoclassKeyword,
            SyntaxKind.LibraryKeyword,
            SyntaxKind.TypedefKeyword,
            SyntaxKind.CppQuoteKeyword,
            SyntaxKind.UnionKeyword,
            SyntaxKind.DispinterfaceKeyword);
    }

    /// <summary>Check if a kind is a modifier keyword for class declarations.</summary>
    private static bool IsModifier(SyntaxKind kind)
    {
        return kind is SyntaxKind.StaticKeyword
            or SyntaxKind.PartialKeyword
            or SyntaxKind.UnsealedKeyword
            or SyntaxKind.ProtectedKeyword
            or SyntaxKind.OverridableKeyword;
    }

    /// <summary>Skip tokens until we reach a member-level boundary.</summary>
    private GreenNode? SkipToMemberStart()
    {
        return SkipTokensUntil(
            SyntaxKind.SemicolonToken,
            SyntaxKind.CloseBraceToken,
            SyntaxKind.OpenBracketToken,
            SyntaxKind.EventKeyword,
            SyntaxKind.StaticKeyword,
            SyntaxKind.ProtectedKeyword,
            SyntaxKind.OverridableKeyword,
            SyntaxKind.EndOfFileToken);
    }

    /// <summary>Entry point: parse a full compilation unit.</summary>
    public GreenNode Parse()
    {
        MidlTrace.Parser($"Parsing {_tokens.Count} tokens");
        var tree = ParseCompilationUnit();
        MidlTrace.Parser($"Parse complete: {tree.Kind}, {tree.SlotCount} children");
        return tree;
    }
}
