using Midl.Syntax;
using Midl.Lexer;

namespace Midl.Lexer.Tests;

public class LexerEdgeCaseTests
{
    private static IReadOnlyList<SyntaxToken> Lex(string source)
    {
        var lexer = new Lexer(source);
        return lexer.Lex();
    }

    // ─── 1: Hex literals ───

    [Fact]
    public void LexesHexLiterals()
    {
        var tokens = Lex("0xFF 0x0 0xDEADBEEF");
        Assert.Equal(4, tokens.Count); // 3 tokens + EOF
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[0].Kind);
        Assert.Equal("0xFF", tokens[0].Text);
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[1].Kind);
        Assert.Equal("0x0", tokens[1].Text);
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[2].Kind);
        Assert.Equal("0xDEADBEEF", tokens[2].Text);
    }

    // ─── 2: Octal literals ───

    [Fact]
    public void LexesOctalLiterals()
    {
        // The lexer scans leading-zero digit sequences as NumericLiteralToken
        var tokens = Lex("077 0123");
        Assert.Equal(3, tokens.Count); // 2 tokens + EOF
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[0].Kind);
        Assert.Equal("077", tokens[0].Text);
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[1].Kind);
        Assert.Equal("0123", tokens[1].Text);
    }

    // ─── 3: Numeric suffixes ───

    [Fact]
    public void LexesNumericSuffixes()
    {
        var tokens = Lex("42U 100L 0xFFUL");
        Assert.Equal(4, tokens.Count); // 3 tokens + EOF
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[0].Kind);
        Assert.Equal("42U", tokens[0].Text);
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[1].Kind);
        Assert.Equal("100L", tokens[1].Text);
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[2].Kind);
        Assert.Equal("0xFFUL", tokens[2].Text);
    }

    // ─── 4: Wide string literal ───

    [Fact]
    public void LexesWideStringLiteral()
    {
        var tokens = Lex("L\"hello\"");
        Assert.Equal(2, tokens.Count); // token + EOF
        Assert.Equal(SyntaxKind.StringLiteralToken, tokens[0].Kind);
        Assert.Equal("L\"hello\"", tokens[0].Text);
    }

    // ─── 5: Arrow token ───

    [Fact]
    public void LexesArrowToken()
    {
        var tokens = Lex("->");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.ArrowToken, tokens[0].Kind);
        Assert.Equal("->", tokens[0].Text);
    }

    // ─── 6: ColonColon token ───

    [Fact]
    public void LexesColonColonToken()
    {
        var tokens = Lex("::");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.ColonColonToken, tokens[0].Kind);
        Assert.Equal("::", tokens[0].Text);
    }

    // ─── 7: Double greater-than ───

    [Fact]
    public void LexesDoubleGreaterThan()
    {
        var tokens = Lex(">>");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.GreaterThanGreaterThanToken, tokens[0].Kind);
        Assert.Equal(">>", tokens[0].Text);
    }

    // ─── 8: Spaced greater-than ───

    [Fact]
    public void LexesSpacedGreaterThan()
    {
        var tokens = Lex("> >");
        Assert.Equal(3, tokens.Count); // 2 tokens + EOF
        Assert.Equal(SyntaxKind.GreaterThanToken, tokens[0].Kind);
        Assert.Equal(SyntaxKind.GreaterThanToken, tokens[1].Kind);
    }

    // ─── 9: Escape sequences in string ───

    [Fact]
    public void LexesEscapeSequences()
    {
        var tokens = Lex("\"hello\\nworld\\t\\\"end\\\\\"");
        Assert.Equal(2, tokens.Count); // string + EOF
        Assert.Equal(SyntaxKind.StringLiteralToken, tokens[0].Kind);
        Assert.Equal("\"hello\\nworld\\t\\\"end\\\\\"", tokens[0].Text);
    }

    // ─── 10: Pragma as trivia ───

    [Fact]
    public void LexesPragmaAsTrivia()
    {
        var tokens = Lex("#pragma once\nint x;");

        // The #pragma directive is consumed as leading trivia (SkippedTokensTrivia)
        // First real token should be `int` (IntKeyword)
        Assert.Equal(SyntaxKind.IntKeyword, tokens[0].Kind);
        Assert.Equal("int", tokens[0].Text);

        // Verify pragma is attached as leading trivia
        Assert.Contains(tokens[0].LeadingTrivia,
            t => t.Kind == SyntaxKind.SkippedTokensTrivia);
    }

    // ─── 11: Empty string literal ───

    [Fact]
    public void LexesEmptyStringLiteral()
    {
        var tokens = Lex("\"\"");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.StringLiteralToken, tokens[0].Kind);
        Assert.Equal("\"\"", tokens[0].Text);
    }

    // ─── 12: Multiple error tokens ───

    [Fact]
    public void LexesMultipleErrorTokens()
    {
        // Use characters that are individually unrecognized (# starts preprocessor trivia)
        var tokens = Lex("@$");
        var errorTokens = tokens.Where(t => t.Kind == SyntaxKind.ErrorToken).ToList();
        Assert.True(errorTokens.Count >= 2, $"Expected at least 2 error tokens, got {errorTokens.Count}");
        Assert.Equal("@", errorTokens[0].Text);
        Assert.Equal("$", errorTokens[1].Text);
    }

    // ─── 13: Unterminated comment ───

    [Fact]
    public void LexesUnterminatedComment()
    {
        // Unterminated multi-line comment is consumed as trivia; no crash
        var tokens = Lex("/* unterminated");

        // Should produce only EOF (the comment becomes leading trivia)
        Assert.Single(tokens);
        Assert.Equal(SyntaxKind.EndOfFileToken, tokens[0].Kind);

        // The unterminated comment is attached as leading trivia on EOF
        Assert.Contains(tokens[0].LeadingTrivia,
            t => t.Kind == SyntaxKind.MultiLineCommentTrivia);
    }

    // ─── 14: Adjacent tokens (no whitespace) ───

    [Fact]
    public void LexesAdjacentTokens()
    {
        var tokens = Lex("foo+bar");
        Assert.Equal(4, tokens.Count); // foo + bar + EOF
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[0].Kind);
        Assert.Equal("foo", tokens[0].Text);
        Assert.Equal(SyntaxKind.PlusToken, tokens[1].Kind);
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[2].Kind);
        Assert.Equal("bar", tokens[2].Text);
    }

    // ─── 15: All punctuation in sequence ───

    [Fact]
    public void LexesAllPunctuationInSequence()
    {
        // Each punctuation character separated by spaces to avoid multi-char operator merging
        var tokens = Lex("{ } ( ) [ ] : ; , . = + - * / % & | ^ ~ ! < >");
        var expected = new[]
        {
            SyntaxKind.OpenBraceToken,
            SyntaxKind.CloseBraceToken,
            SyntaxKind.OpenParenToken,
            SyntaxKind.CloseParenToken,
            SyntaxKind.OpenBracketToken,
            SyntaxKind.CloseBracketToken,
            SyntaxKind.ColonToken,
            SyntaxKind.SemicolonToken,
            SyntaxKind.CommaToken,
            SyntaxKind.DotToken,
            SyntaxKind.EqualsToken,
            SyntaxKind.PlusToken,
            SyntaxKind.MinusToken,
            SyntaxKind.AsteriskToken,
            SyntaxKind.SlashToken,
            SyntaxKind.PercentToken,
            SyntaxKind.AmpersandToken,
            SyntaxKind.PipeToken,
            SyntaxKind.CaretToken,
            SyntaxKind.TildeToken,
            SyntaxKind.ExclamationToken,
            SyntaxKind.LessThanToken,
            SyntaxKind.GreaterThanToken,
            SyntaxKind.EndOfFileToken,
        };
        Assert.Equal(expected.Length, tokens.Count);
        for (int i = 0; i < expected.Length; i++)
            Assert.Equal(expected[i], tokens[i].Kind);
    }
}
