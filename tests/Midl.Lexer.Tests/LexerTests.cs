using Midl.Syntax;
using Midl.Lexer;

namespace Midl.Lexer.Tests;

public class LexerTests
{
    private static IReadOnlyList<SyntaxToken> Lex(string source)
    {
        var lexer = new Lexer(source);
        return lexer.Lex();
    }

    [Fact]
    public void LexesEmptyString()
    {
        var tokens = Lex("");
        Assert.Single(tokens);
        Assert.Equal(SyntaxKind.EndOfFileToken, tokens[0].Kind);
    }

    [Fact]
    public void LexesPunctuation()
    {
        var tokens = Lex("{ } ( ) [ ] ; : , .");
        var expected = new[]
        {
            SyntaxKind.OpenBraceToken,
            SyntaxKind.CloseBraceToken,
            SyntaxKind.OpenParenToken,
            SyntaxKind.CloseParenToken,
            SyntaxKind.OpenBracketToken,
            SyntaxKind.CloseBracketToken,
            SyntaxKind.SemicolonToken,
            SyntaxKind.ColonToken,
            SyntaxKind.CommaToken,
            SyntaxKind.DotToken,
            SyntaxKind.EndOfFileToken,
        };
        Assert.Equal(expected.Length, tokens.Count);
        for (int i = 0; i < expected.Length; i++)
            Assert.Equal(expected[i], tokens[i].Kind);
    }

    [Fact]
    public void LexesOperators()
    {
        var tokens = Lex("+ - * / = == != < > <= >= << >> && || | & ^ ~ !");
        var expected = new[]
        {
            SyntaxKind.PlusToken,
            SyntaxKind.MinusToken,
            SyntaxKind.AsteriskToken,
            SyntaxKind.SlashToken,
            SyntaxKind.EqualsToken,
            SyntaxKind.EqualsEqualsToken,
            SyntaxKind.ExclamationEqualsToken,
            SyntaxKind.LessThanToken,
            SyntaxKind.GreaterThanToken,
            SyntaxKind.LessThanEqualsToken,
            SyntaxKind.GreaterThanEqualsToken,
            SyntaxKind.LessThanLessThanToken,
            SyntaxKind.GreaterThanGreaterThanToken,
            SyntaxKind.AmpersandAmpersandToken,
            SyntaxKind.PipePipeToken,
            SyntaxKind.PipeToken,
            SyntaxKind.AmpersandToken,
            SyntaxKind.CaretToken,
            SyntaxKind.TildeToken,
            SyntaxKind.ExclamationToken,
            SyntaxKind.EndOfFileToken,
        };
        Assert.Equal(expected.Length, tokens.Count);
        for (int i = 0; i < expected.Length; i++)
            Assert.Equal(expected[i], tokens[i].Kind);
    }

    [Fact]
    public void LexesNumericLiterals()
    {
        var tokens = Lex("42 0xFF 0x1234ABCD");
        Assert.Equal(4, tokens.Count);
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[0].Kind);
        Assert.Equal("42", tokens[0].Text);
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[1].Kind);
        Assert.Equal("0xFF", tokens[1].Text);
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[2].Kind);
        Assert.Equal("0x1234ABCD", tokens[2].Text);
    }

    [Fact]
    public void LexesFloatLiterals()
    {
        var tokens = Lex("1.1 3.14 0.5 100.0");
        Assert.Equal(5, tokens.Count); // 4 floats + EOF
        Assert.Equal(SyntaxKind.FloatLiteralToken, tokens[0].Kind);
        Assert.Equal("1.1", tokens[0].Text);
        Assert.Equal(SyntaxKind.FloatLiteralToken, tokens[1].Kind);
        Assert.Equal("3.14", tokens[1].Text);
        Assert.Equal(SyntaxKind.FloatLiteralToken, tokens[2].Kind);
        Assert.Equal("0.5", tokens[2].Text);
        Assert.Equal(SyntaxKind.FloatLiteralToken, tokens[3].Kind);
        Assert.Equal("100.0", tokens[3].Text);
    }

    [Fact]
    public void FloatWithSuffix()
    {
        var tokens = Lex("1.5f");
        Assert.Equal(2, tokens.Count); // float + EOF
        Assert.Equal(SyntaxKind.FloatLiteralToken, tokens[0].Kind);
        Assert.Equal("1.5f", tokens[0].Text);
    }

    [Fact]
    public void IntegerDotIdentifier_NotFloat()
    {
        // "1.foo" should be 1 (integer), . (dot), foo (identifier) — not a float
        var tokens = Lex("1.foo");
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[0].Kind);
        Assert.Equal("1", tokens[0].Text);
        Assert.Equal(SyntaxKind.DotToken, tokens[1].Kind);
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[2].Kind);
        Assert.Equal("foo", tokens[2].Text);
    }

    [Fact]
    public void NegativeInteger()
    {
        var tokens = Lex("-42");
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[0].Kind);
        Assert.Equal("-42", tokens[0].Text);
    }

    [Fact]
    public void NegativeFloat()
    {
        var tokens = Lex("-3.14");
        Assert.Equal(SyntaxKind.FloatLiteralToken, tokens[0].Kind);
        Assert.Equal("-3.14", tokens[0].Text);
    }

    [Fact]
    public void NegativeHex()
    {
        var tokens = Lex("-0xFF");
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[0].Kind);
        Assert.Equal("-0xFF", tokens[0].Text);
    }

    [Fact]
    public void MinusWithSpace_StaysSeparate()
    {
        // "- 42" has whitespace between minus and digit — stays as separate tokens
        var tokens = Lex("- 42");
        Assert.Equal(SyntaxKind.MinusToken, tokens[0].Kind);
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[1].Kind);
        Assert.Equal("42", tokens[1].Text);
    }

    [Fact]
    public void LexesStringLiteral()
    {
        var tokens = Lex("\"hello world\"");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.StringLiteralToken, tokens[0].Kind);
        Assert.Equal("\"hello world\"", tokens[0].Text);

        tokens = Lex("\"with \\\"escapes\\\"\"");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.StringLiteralToken, tokens[0].Kind);
        Assert.Equal("\"with \\\"escapes\\\"\"", tokens[0].Text);
    }

    [Fact]
    public void LexesIdentifier()
    {
        var tokens = Lex("myVariable _private Windows");
        Assert.Equal(4, tokens.Count);
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[0].Kind);
        Assert.Equal("myVariable", tokens[0].Text);
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[1].Kind);
        Assert.Equal("_private", tokens[1].Text);
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[2].Kind);
        Assert.Equal("Windows", tokens[2].Text);
    }

    [Fact]
    public void LexesKeywords()
    {
        var tokens = Lex("namespace runtimeclass interface Int32 String Boolean");
        Assert.Equal(7, tokens.Count);
        Assert.Equal(SyntaxKind.NamespaceKeyword, tokens[0].Kind);
        Assert.Equal(SyntaxKind.RuntimeclassKeyword, tokens[1].Kind);
        Assert.Equal(SyntaxKind.InterfaceKeyword, tokens[2].Kind);
        Assert.Equal(SyntaxKind.Int32Keyword, tokens[3].Kind);
        Assert.Equal(SyntaxKind.StringKeyword, tokens[4].Kind);
        Assert.Equal(SyntaxKind.BooleanKeyword, tokens[5].Kind);
    }

    [Fact]
    public void LexesSingleLineComment()
    {
        var tokens = Lex("x // this is a comment\ny");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[0].Kind);
        Assert.Equal("x", tokens[0].Text);
        Assert.Contains(tokens[0].TrailingTrivia,
            t => t.Kind == SyntaxKind.SingleLineCommentTrivia);
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[1].Kind);
        Assert.Equal("y", tokens[1].Text);
    }

    [Fact]
    public void LexesMultiLineComment()
    {
        var tokens = Lex("/* comment */ x");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[0].Kind);
        Assert.Equal("x", tokens[0].Text);
        Assert.Contains(tokens[0].LeadingTrivia,
            t => t.Kind == SyntaxKind.MultiLineCommentTrivia);
    }

    [Fact]
    public void LexesWhitespaceTrivia()
    {
        var tokens = Lex("  \t x");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[0].Kind);
        Assert.Contains(tokens[0].LeadingTrivia,
            t => t.Kind == SyntaxKind.WhitespaceTrivia);
    }

    [Fact]
    public void LexesRealisticMidl3()
    {
        var source = """
            namespace Bookstore
            {
                runtimeclass BookSku
                {
                    BookSku();
                    String Title { get; };
                    Single Price;
                }
            }
            """;

        var tokens = Lex(source);

        // Verify keywords are correctly identified
        Assert.Equal(SyntaxKind.NamespaceKeyword, tokens[0].Kind);
        Assert.Equal("Bookstore", tokens[1].Text);
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[1].Kind);
        Assert.Equal(SyntaxKind.OpenBraceToken, tokens[2].Kind);
        Assert.Equal(SyntaxKind.RuntimeclassKeyword, tokens[3].Kind);
        Assert.Equal(SyntaxKind.StringKeyword, tokens[10].Kind);
        Assert.Equal(SyntaxKind.GetKeyword, tokens[13].Kind);
        Assert.Equal(SyntaxKind.SingleKeyword, tokens[17].Kind);

        // Last token is EOF
        Assert.Equal(SyntaxKind.EndOfFileToken, tokens[^1].Kind);

        // Verify total token count: namespace Bookstore { runtimeclass BookSku {
        //   BookSku ( ) ; String Title { get ; } ; Single Price ; } } EOF = 22 + EOF = 23
        Assert.Equal(23, tokens.Count);
    }

    [Fact]
    public void ErrorTokenForInvalidCharacter()
    {
        var tokens = Lex("@");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.ErrorToken, tokens[0].Kind);
        Assert.Equal("@", tokens[0].Text);

        tokens = Lex("$");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.ErrorToken, tokens[0].Kind);
        Assert.Equal("$", tokens[0].Text);
    }

    [Fact]
    public void UnterminatedStringProducesErrorToken()
    {
        var tokens = Lex("\"unterminated");
        Assert.Equal(2, tokens.Count);
        Assert.Equal(SyntaxKind.ErrorToken, tokens[0].Kind);
        Assert.Equal("\"unterminated", tokens[0].Text);
    }
}
