namespace Midl.Lexer;

using Midl.Syntax;

/// <summary>
/// MIDL lexer — converts source text into a sequence of tokens with trivia.
/// </summary>
public sealed class Lexer
{
    private readonly SlidingTextWindow _window;
    private readonly int _sourceLength;
    private SyntaxKind _lastTokenKind = SyntaxKind.None;
    private readonly List<GreenNode> _triviaBuffer = new();

    public Lexer(string sourceText)
    {
        _window = new SlidingTextWindow(sourceText);
        _sourceLength = sourceText.Length;
    }

    public IReadOnlyList<SyntaxToken> Lex()
    {
        MidlTrace.Lexer($"Lexing {_sourceLength} chars");
        var tokens = new List<SyntaxToken>();

        while (true)
        {
            var leading = ScanLeadingTrivia();
            var (kind, text, value) = ScanToken();
            var trailing = ScanTrailingTrivia();

            var token = new SyntaxToken(
                kind, text, value,
                leading.Length > 0 ? leading : null,
                trailing.Length > 0 ? trailing : null);

            tokens.Add(token);
            _lastTokenKind = kind;

            if (kind == SyntaxKind.EndOfFileToken)
                break;
        }

        if (MidlTrace.IsEnabled)
        {
            int keywords = tokens.Count(t => t.Kind >= SyntaxKind.BooleanKeyword && t.Kind <= SyntaxKind.DefaultKeyword);
            MidlTrace.Lexer($"Produced {tokens.Count} tokens ({keywords} keywords, {tokens.Count(t => t.Kind == SyntaxKind.IdentifierToken)} identifiers)");
        }

        return tokens;
    }

    // -- Trivia ---------------------------------------------------------------

    private static readonly GreenNode[] s_emptyTrivia = [];

    private GreenNode[] ScanLeadingTrivia()
    {
        _triviaBuffer.Clear();

        while (!_window.IsAtEnd)
        {
            char ch = _window.PeekChar();

            if (ch == ' ' || ch == '\t')
                _triviaBuffer.Add(ScanWhitespace());
            else if (ch == '\r' || ch == '\n')
                _triviaBuffer.Add(ScanEndOfLine());
            else if (ch == '/' && _window.PeekChar(1) == '/')
                _triviaBuffer.Add(ScanSingleLineComment());
            else if (ch == '/' && _window.PeekChar(1) == '*')
                _triviaBuffer.Add(ScanMultiLineComment());
            else if (ch == '#')
                _triviaBuffer.Add(ScanPreprocessorDirective());
            else
                break;
        }

        return _triviaBuffer.Count > 0 ? _triviaBuffer.ToArray() : s_emptyTrivia;
    }

    private GreenNode[] ScanTrailingTrivia()
    {
        _triviaBuffer.Clear();

        while (!_window.IsAtEnd)
        {
            char ch = _window.PeekChar();

            if (ch == ' ' || ch == '\t')
            {
                _triviaBuffer.Add(ScanWhitespace());
            }
            else if (ch == '/' && _window.PeekChar(1) == '/')
            {
                _triviaBuffer.Add(ScanSingleLineComment());
                // Consume the end-of-line after a single-line comment
                if (!_window.IsAtEnd && (_window.PeekChar() == '\r' || _window.PeekChar() == '\n'))
                    _triviaBuffer.Add(ScanEndOfLine());
                break;
            }
            else if (ch == '/' && _window.PeekChar(1) == '*')
            {
                _triviaBuffer.Add(ScanMultiLineComment());
            }
            else if (ch == '\r' || ch == '\n')
            {
                _triviaBuffer.Add(ScanEndOfLine());
                break;
            }
            else
            {
                break;
            }
        }

        return _triviaBuffer.Count > 0 ? _triviaBuffer.ToArray() : s_emptyTrivia;
    }

    private SyntaxTrivia ScanWhitespace()
    {
        int start = _window.Position;
        while (!_window.IsAtEnd)
        {
            char ch = _window.PeekChar();
            if (ch == ' ' || ch == '\t')
                _window.AdvanceChar();
            else
                break;
        }
        return new SyntaxTrivia(SyntaxKind.WhitespaceTrivia,
            _window.GetText(start, _window.Position - start));
    }

    private SyntaxTrivia ScanEndOfLine()
    {
        int start = _window.Position;
        if (_window.PeekChar() == '\r')
        {
            _window.AdvanceChar();
            if (_window.PeekChar() == '\n')
                _window.AdvanceChar();
        }
        else
        {
            _window.AdvanceChar(); // '\n'
        }
        return new SyntaxTrivia(SyntaxKind.EndOfLineTrivia,
            _window.GetText(start, _window.Position - start));
    }

    private SyntaxTrivia ScanSingleLineComment()
    {
        int start = _window.Position;
        _window.AdvanceChar(2); // skip //
        // Distinguish doc comments from regular // comments:
        //   /// (but not ////) and //! are doc comments
        bool isDocComment = !_window.IsAtEnd &&
            ((_window.PeekChar() == '/' && (_window.IsAtEnd || _window.PeekChar(1) != '/')) ||
             _window.PeekChar() == '!');
        if (isDocComment)
            _window.AdvanceChar(); // skip third char (/ or !)
        while (!_window.IsAtEnd)
        {
            char ch = _window.PeekChar();
            if (ch == '\r' || ch == '\n')
                break;
            _window.AdvanceChar();
        }
        var kind = isDocComment ? SyntaxKind.DocCommentTrivia : SyntaxKind.SingleLineCommentTrivia;
        return new SyntaxTrivia(kind,
            _window.GetText(start, _window.Position - start));
    }

    private SyntaxTrivia ScanMultiLineComment()
    {
        int start = _window.Position;
        _window.AdvanceChar(2); // skip /*
        // Detect /** or /*! (but not /***) as doc comments
        bool isDocComment = !_window.IsAtEnd &&
            ((_window.PeekChar() == '*' && _window.PeekChar(1) != '*' && _window.PeekChar(1) != '/') ||
             _window.PeekChar() == '!');
        while (!_window.IsAtEnd)
        {
            if (_window.PeekChar() == '*' && _window.PeekChar(1) == '/')
            {
                _window.AdvanceChar(2);
                break;
            }
            _window.AdvanceChar();
        }
        var kind = isDocComment ? SyntaxKind.DocCommentTrivia : SyntaxKind.MultiLineCommentTrivia;
        return new SyntaxTrivia(kind,
            _window.GetText(start, _window.Position - start));
    }

    // -- Token scanning -------------------------------------------------------

    private (SyntaxKind Kind, string Text, object? Value) ScanToken()
    {
        if (_window.IsAtEnd)
            return (SyntaxKind.EndOfFileToken, "", null);

        char ch = _window.PeekChar();

        switch (ch)
        {
            // Single-character punctuation
            case '{': _window.AdvanceChar(); return (SyntaxKind.OpenBraceToken, "{", null);
            case '}': _window.AdvanceChar(); return (SyntaxKind.CloseBraceToken, "}", null);
            case '(': _window.AdvanceChar(); return (SyntaxKind.OpenParenToken, "(", null);
            case ')': _window.AdvanceChar(); return (SyntaxKind.CloseParenToken, ")", null);
            case '[': _window.AdvanceChar(); return (SyntaxKind.OpenBracketToken, "[", null);
            case ']': _window.AdvanceChar(); return (SyntaxKind.CloseBracketToken, "]", null);
            case ';': _window.AdvanceChar(); return (SyntaxKind.SemicolonToken, ";", null);
            case ':':
                _window.AdvanceChar();
                if (_window.PeekChar() == ':') { _window.AdvanceChar(); return (SyntaxKind.ColonColonToken, "::", null); }
                return (SyntaxKind.ColonToken, ":", null);
            case ',': _window.AdvanceChar(); return (SyntaxKind.CommaToken, ",", null);
            case '.': _window.AdvanceChar(); return (SyntaxKind.DotToken, ".", null);
            case '~': _window.AdvanceChar(); return (SyntaxKind.TildeToken, "~", null);
            case '^': _window.AdvanceChar(); return (SyntaxKind.CaretToken, "^", null);
            case '%': _window.AdvanceChar(); return (SyntaxKind.PercentToken, "%", null);

            // Multi-character operators
            case '+':
                _window.AdvanceChar();
                if (_window.PeekChar() == '+') { _window.AdvanceChar(); return (SyntaxKind.PlusPlusToken, "++", null); }
                return (SyntaxKind.PlusToken, "+", null);

            case '-':
                _window.AdvanceChar();
                if (_window.PeekChar() == '-') { _window.AdvanceChar(); return (SyntaxKind.MinusMinusToken, "--", null); }
                if (_window.PeekChar() == '>') { _window.AdvanceChar(); return (SyntaxKind.ArrowToken, "->", null); }
                // Negative numeric literal only when previous token is a value-preceding operator
                if (IsAsciiDigit(_window.PeekChar()) && IsValuePrecedingToken(_lastTokenKind))
                {
                    var (numKind, numText, numVal) = ScanNumericLiteral();
                    return (numKind, "-" + numText, "-" + numText);
                }
                return (SyntaxKind.MinusToken, "-", null);

            case '*': _window.AdvanceChar(); return (SyntaxKind.AsteriskToken, "*", null);
            case '/': _window.AdvanceChar(); return (SyntaxKind.SlashToken, "/", null);

            case '=':
                _window.AdvanceChar();
                if (_window.PeekChar() == '=') { _window.AdvanceChar(); return (SyntaxKind.EqualsEqualsToken, "==", null); }
                return (SyntaxKind.EqualsToken, "=", null);

            case '!':
                _window.AdvanceChar();
                if (_window.PeekChar() == '=') { _window.AdvanceChar(); return (SyntaxKind.ExclamationEqualsToken, "!=", null); }
                return (SyntaxKind.ExclamationToken, "!", null);

            case '<':
                _window.AdvanceChar();
                if (_window.PeekChar() == '<') { _window.AdvanceChar(); return (SyntaxKind.LessThanLessThanToken, "<<", null); }
                if (_window.PeekChar() == '=') { _window.AdvanceChar(); return (SyntaxKind.LessThanEqualsToken, "<=", null); }
                return (SyntaxKind.LessThanToken, "<", null);

            case '>':
                _window.AdvanceChar();
                if (_window.PeekChar() == '>') { _window.AdvanceChar(); return (SyntaxKind.GreaterThanGreaterThanToken, ">>", null); }
                if (_window.PeekChar() == '=') { _window.AdvanceChar(); return (SyntaxKind.GreaterThanEqualsToken, ">=", null); }
                return (SyntaxKind.GreaterThanToken, ">", null);

            case '&':
                _window.AdvanceChar();
                if (_window.PeekChar() == '&') { _window.AdvanceChar(); return (SyntaxKind.AmpersandAmpersandToken, "&&", null); }
                return (SyntaxKind.AmpersandToken, "&", null);

            case '|':
                _window.AdvanceChar();
                if (_window.PeekChar() == '|') { _window.AdvanceChar(); return (SyntaxKind.PipePipeToken, "||", null); }
                return (SyntaxKind.PipeToken, "|", null);

            case '"':
                return ScanStringLiteral();
        }

        if (IsAsciiDigit(ch))
            return ScanNumericLiteral();

        if (IsAsciiIdentStart(ch))
            return ScanIdentifierOrKeyword();

        // Unrecognized character -> error token
        _window.AdvanceChar();
        return (SyntaxKind.ErrorToken, ch.ToString(), null);
    }

    private (SyntaxKind, string, object?) ScanStringLiteral()
    {
        int start = _window.Position;
        _window.AdvanceChar(); // opening "

        while (!_window.IsAtEnd)
        {
            char ch = _window.PeekChar();
            if (ch == '\\')
            {
                _window.AdvanceChar();
                if (!_window.IsAtEnd) _window.AdvanceChar();
            }
            else if (ch == '"')
            {
                _window.AdvanceChar(); // closing "
                string text = _window.GetText(start, _window.Position - start);
                return (SyntaxKind.StringLiteralToken, text, text[1..^1]);
            }
            else if (ch == '\r' || ch == '\n')
            {
                break; // unterminated at end-of-line
            }
            else
            {
                _window.AdvanceChar();
            }
        }

        // Unterminated string -> error token
        string errorText = _window.GetText(start, _window.Position - start);
        return (SyntaxKind.ErrorToken, errorText, null);
    }

    private (SyntaxKind, string, object?) ScanNumericLiteral()
    {
        int start = _window.Position;

        if (_window.PeekChar() == '0' && (_window.PeekChar(1) == 'x' || _window.PeekChar(1) == 'X'))
        {
            _window.AdvanceChar(2); // 0x
            while (!_window.IsAtEnd && IsHexDigit(_window.PeekChar()))
                _window.AdvanceChar();
        }
        else
        {
            while (!_window.IsAtEnd && IsAsciiDigit(_window.PeekChar()))
                _window.AdvanceChar();

            // Float literal: digits followed by '.' and more digits (e.g. 1.1, 3.14)
            if (!_window.IsAtEnd && _window.PeekChar() == '.' && IsAsciiDigit(_window.PeekChar(1)))
            {
                _window.AdvanceChar(); // '.'
                while (!_window.IsAtEnd && IsAsciiDigit(_window.PeekChar()))
                    _window.AdvanceChar();

                // Optional f/F suffix
                if (!_window.IsAtEnd && _window.PeekChar() is 'f' or 'F')
                    _window.AdvanceChar();

                string floatText = _window.GetText(start, _window.Position - start);
                return (SyntaxKind.FloatLiteralToken, floatText, floatText);
            }
        }

        // Optional U/L suffixes
        while (!_window.IsAtEnd)
        {
            char c = _window.PeekChar();
            if (c is 'U' or 'u' or 'L' or 'l')
                _window.AdvanceChar();
            else
                break;
        }

        string text = _window.GetText(start, _window.Position - start);
        return (SyntaxKind.NumericLiteralToken, text, text);
    }

    private (SyntaxKind, string, object?) ScanIdentifierOrKeyword()
    {
        int start = _window.Position;
        _window.AdvanceChar(); // first char (letter or _)

        while (!_window.IsAtEnd)
        {
            char ch = _window.PeekChar();
            if (IsAsciiIdentPart(ch))
                _window.AdvanceChar();
            else
                break;
        }

        string text = _window.GetText(start, _window.Position - start);

        // Wide string literal: L"..."
        if (text == "L" && !_window.IsAtEnd && _window.PeekChar() == '"')
        {
            var (_, strText, strValue) = ScanStringLiteral();
            string fullText = text + strText;
            return (SyntaxKind.StringLiteralToken, fullText, strValue);
        }

        if (KeywordTable.TryGetKeyword(text, out var kind, out var canonicalText))
            return (kind, canonicalText, canonicalText);

        return (SyntaxKind.IdentifierToken, text, text);
    }

    private static bool IsHexDigit(char ch) =>
        (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F');

    private static bool IsAsciiDigit(char ch) =>
        (uint)(ch - '0') <= 9;

    private static bool IsAsciiIdentStart(char ch) =>
        (uint)(ch - 'a') <= 25 || (uint)(ch - 'A') <= 25 || ch == '_';

    private static bool IsAsciiIdentPart(char ch) =>
        (uint)(ch - 'a') <= 25 || (uint)(ch - 'A') <= 25 || (uint)(ch - '0') <= 9 || ch == '_';

    /// <summary>
    /// Returns true if the previous token kind can precede a negative numeric literal.
    /// This prevents "-1234" from being a single token in contexts like UUID dashes.
    /// </summary>
    private static bool IsValuePrecedingToken(SyntaxKind kind) => kind is
        SyntaxKind.None              // start of input
        or SyntaxKind.OpenParenToken // (-1)
        or SyntaxKind.CommaToken     // foo(-1, -2)
        or SyntaxKind.EqualsToken    // A = -1
        or SyntaxKind.OpenBracketToken // [-1]
        or SyntaxKind.ColonToken     // case: -1
        or SyntaxKind.PlusToken      // 1 + -2
        or SyntaxKind.MinusToken     // 1 - -2
        or SyntaxKind.AsteriskToken  // * -1
        or SyntaxKind.SlashToken;    // / -1

    private SyntaxTrivia ScanPreprocessorDirective()
    {
        int start = _window.Position;
        while (!_window.IsAtEnd)
        {
            char ch = _window.PeekChar();
            if (ch == '\r' || ch == '\n')
                break;
            _window.AdvanceChar();
        }
        return new SyntaxTrivia(SyntaxKind.SkippedTokensTrivia,
            _window.GetText(start, _window.Position - start));
    }
}
