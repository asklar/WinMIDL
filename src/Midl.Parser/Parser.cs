using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Midl.Syntax;

namespace Midl.Parser;

/// <summary>
/// Error-recovering recursive-descent parser for MIDL3.
/// Accepts a token stream and produces a lossless green syntax tree.
/// </summary>
public sealed partial class Parser
{
    private readonly SyntaxToken[] _tokens;
    private readonly int _tokenCount;
    private int _position;
    private SyntaxToken? _pendingToken;
    private readonly Stack<List<GreenNode?>> _childListPool = new();

    private static readonly SyntaxToken s_eof = new(SyntaxKind.EndOfFileToken, "");

    public Parser(IReadOnlyList<SyntaxToken> tokens)
    {
        // Store as array for fast bounds-checked indexing (avoids interface dispatch)
        _tokens = tokens switch
        {
            SyntaxToken[] arr => arr,
            List<SyntaxToken> list => CollectionsMarshal.AsSpan(list).ToArray(),
            _ => [.. tokens]
        };
        _tokenCount = _tokens.Length;
        _position = 0;
    }

    /// <summary>Returns the current token without consuming it.</summary>
    private SyntaxToken Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _pendingToken ?? (_position < _tokenCount ? _tokens[_position] : s_eof);
    }

    /// <summary>Look ahead by offset tokens from the current position.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxToken Peek(int offset)
    {
        if (offset == 0) return Current;
        int actualOffset = _pendingToken != null ? offset - 1 : offset;
        int index = _position + actualOffset;
        return (uint)index < (uint)_tokenCount ? _tokens[index] : s_eof;
    }

    /// <summary>Check if the current token has the given kind.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool At(SyntaxKind kind) => Current.Kind == kind;

    /// <summary>Check if the current token matches any of the given kinds.</summary>
    private bool AtAny(params ReadOnlySpan<SyntaxKind> kinds)
    {
        var current = Current.Kind;
        for (int i = 0; i < kinds.Length; i++)
        {
            if (current == kinds[i]) return true;
        }
        return false;
    }

    /// <summary>Check if current token is an identifier with the given text (for contextual keywords).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AtContextual(string text)
        => Current.Kind == SyntaxKind.IdentifierToken &&
           string.Equals(Current.Text, text, StringComparison.Ordinal);

    /// <summary>Consume and return the current token if it matches the expected kind, else return a missing token.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxToken Eat(SyntaxKind expected)
    {
        if (Current.Kind == expected)
        {
            return EatToken();
        }
        return CreateMissingToken(expected);
    }

    /// <summary>Consume and return the current token unconditionally.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SyntaxToken EatToken()
    {
        if (_pendingToken != null)
        {
            var token = _pendingToken;
            _pendingToken = null;
            return token;
        }
        var t = Current;
        _position++;
        return t;
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

    /// <summary>Rent a pooled child list to avoid repeated allocations.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private List<GreenNode?> RentChildren()
    {
        if (_childListPool.Count > 0)
        {
            var list = _childListPool.Pop();
            list.Clear();
            return list;
        }
        return new List<GreenNode?>(8);
    }

    /// <summary>Build a single-child green node without array allocation.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static GreenNode MakeNode(SyntaxKind kind, GreenNode? child)
    {
        return new SingleChildGreenNode(kind, child);
    }

    /// <summary>Build a green node list from a kind and variable children.</summary>
    private static GreenNodeList MakeNode(SyntaxKind kind, params GreenNode?[] children)
    {
        return new GreenNodeList(kind, children);
    }

    /// <summary>Build a green node list from a kind and a list of children, returning the list to pool.</summary>
    private GreenNodeList MakeNode(SyntaxKind kind, List<GreenNode?> children)
    {
        var node = new GreenNodeList(kind, children.ToArray());
        _childListPool.Push(children);
        return node;
    }

    /// <summary>Skip tokens until we find one of the stop kinds. Returns collected tokens as a SkippedTokens node, or null.</summary>
    private GreenNode? SkipTokensUntil(params ReadOnlySpan<SyntaxKind> stopKinds)
    {
        if (MidlTrace.IsEnabled)
            MidlTrace.Parser($"Error recovery: skipping to {stopKinds[0]} at token {_position}");
        var skipped = RentChildren();
        while (!At(SyntaxKind.EndOfFileToken) && !AtAny(stopKinds))
        {
            skipped.Add(EatToken());
        }
        if (skipped.Count == 0)
        {
            _childListPool.Push(skipped);
            return null;
        }
        return MakeNode(SyntaxKind.SkippedTokens, skipped);
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
#if WITH_COMPAT
            SyntaxKind.CoclassKeyword,
            SyntaxKind.LibraryKeyword,
            SyntaxKind.TypedefKeyword,
            SyntaxKind.CppQuoteKeyword,
            SyntaxKind.UnionKeyword,
            SyntaxKind.DispinterfaceKeyword,
            SyntaxKind.ImportlibKeyword,
#endif
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
#if WITH_COMPAT
            SyntaxKind.CoclassKeyword,
            SyntaxKind.LibraryKeyword,
            SyntaxKind.TypedefKeyword,
            SyntaxKind.CppQuoteKeyword,
            SyntaxKind.UnionKeyword,
            SyntaxKind.DispinterfaceKeyword);
#else
            SyntaxKind.EndOfFileToken); // dummy terminator — never matches
#endif
    }

    /// <summary>Check if a kind is a modifier keyword for class declarations.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        MidlTrace.Parser($"Parsing {_tokenCount} tokens");
        var tree = ParseCompilationUnit();
        MidlTrace.Parser($"Parse complete: {tree.Kind}, {tree.SlotCount} children");
        return tree;
    }
}
