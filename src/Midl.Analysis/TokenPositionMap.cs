using Midl.Syntax;

namespace Midl.Analysis;

/// <summary>
/// Builds a mapping from token index to absolute source position.
/// Used by the Binder to emit diagnostics with accurate positions.
/// </summary>
public sealed class TokenPositionMap
{
    private int[]? _tokenTextStarts;
    private readonly IReadOnlyList<SyntaxToken> _tokens;

    public IReadOnlyList<SyntaxToken> Tokens => _tokens;

    public TokenPositionMap(IReadOnlyList<SyntaxToken> tokens)
    {
        _tokens = tokens;
    }

    private int[] EnsureBuilt()
    {
        if (_tokenTextStarts != null) return _tokenTextStarts;
        var starts = new int[_tokens.Count];
        int pos = 0;
        for (int i = 0; i < _tokens.Count; i++)
        {
            var token = _tokens[i];
            int leadingWidth = 0;
            foreach (var t in token.LeadingTrivia) leadingWidth += t.FullWidth;
            starts[i] = pos + leadingWidth;
            pos += leadingWidth + token.Text.Length;
            int trailingWidth = 0;
            foreach (var t in token.TrailingTrivia) trailingWidth += t.FullWidth;
            pos += trailingWidth;
        }
        _tokenTextStarts = starts;
        return starts;
    }

    /// <summary>Get the absolute source position where the token text starts.</summary>
    public int GetPosition(int tokenIndex)
    {
        var starts = EnsureBuilt();
        return tokenIndex >= 0 && tokenIndex < starts.Length ? starts[tokenIndex] : 0;
    }

    /// <summary>Find the token index whose text contains or matches the given text, starting from startIndex.</summary>
    public int FindToken(string text, int startIndex = 0)
    {
        for (int i = startIndex; i < _tokens.Count; i++)
        {
            if (_tokens[i].Text == text)
                return i;
        }
        return -1;
    }

    /// <summary>Find the token index by SyntaxKind, starting from startIndex.</summary>
    public int FindTokenByKind(SyntaxKind kind, int startIndex = 0)
    {
        for (int i = startIndex; i < _tokens.Count; i++)
        {
            if (_tokens[i].Kind == kind)
                return i;
        }
        return -1;
    }

    /// <summary>Get absolute position of first token matching text.</summary>
    public int GetPositionOf(string text, int startFromIndex = 0)
    {
        int idx = FindToken(text, startFromIndex);
        return idx >= 0 ? EnsureBuilt()[idx] : 0;
    }

    /// <summary>Get absolute position of first token of given kind.</summary>
    public int GetPositionOfKind(SyntaxKind kind, int startFromIndex = 0)
    {
        int idx = FindTokenByKind(kind, startFromIndex);
        return idx >= 0 ? EnsureBuilt()[idx] : 0;
    }
}
