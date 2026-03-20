using Midl.Syntax;

namespace Midl.Analysis;

/// <summary>
/// Builds a mapping from token index to absolute source position.
/// Used by the Binder to emit diagnostics with accurate positions.
/// </summary>
public sealed class TokenPositionMap
{
    private readonly int[] _tokenTextStarts;
    private readonly IReadOnlyList<SyntaxToken> _tokens;

    public TokenPositionMap(IReadOnlyList<SyntaxToken> tokens)
    {
        _tokens = tokens;
        _tokenTextStarts = new int[tokens.Count];
        int pos = 0;
        for (int i = 0; i < tokens.Count; i++)
        {
            int leadingWidth = tokens[i].LeadingTrivia.Sum(t => t.FullWidth);
            _tokenTextStarts[i] = pos + leadingWidth;
            pos += leadingWidth + tokens[i].Text.Length;
            int trailingWidth = tokens[i].TrailingTrivia.Sum(t => t.FullWidth);
            pos += trailingWidth;
        }
    }

    /// <summary>Get the absolute source position where the token text starts.</summary>
    public int GetPosition(int tokenIndex) =>
        tokenIndex >= 0 && tokenIndex < _tokenTextStarts.Length ? _tokenTextStarts[tokenIndex] : 0;

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
        return idx >= 0 ? _tokenTextStarts[idx] : 0;
    }

    /// <summary>Get absolute position of first token of given kind.</summary>
    public int GetPositionOfKind(SyntaxKind kind, int startFromIndex = 0)
    {
        int idx = FindTokenByKind(kind, startFromIndex);
        return idx >= 0 ? _tokenTextStarts[idx] : 0;
    }
}
