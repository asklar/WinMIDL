// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Syntax;
using Midl.Lexer;

namespace Midl.Lexer.Tests;

/// <summary>
/// Tests verifying behavior is preserved after lexer and preprocessor optimizations.
/// </summary>
public class LexerOptimizationTests
{
    private static IReadOnlyList<SyntaxToken> Lex(string source) =>
        new Lexer(source).Lex();

    // ── Trivia allocation ────────────────────────────────────────────────

    [Fact]
    public void Token_WithNoTrivia_HasEmptyTriviaArrays()
    {
        // First token at start of input, no whitespace anywhere
        var tokens = Lex("x");
        Assert.Empty(tokens[0].LeadingTrivia);
        Assert.Empty(tokens[0].TrailingTrivia);
    }

    [Fact]
    public void Token_WithLeadingWhitespace_HasLeadingTrivia()
    {
        var tokens = Lex("  x");
        Assert.NotEmpty(tokens[0].LeadingTrivia);
        Assert.Equal(SyntaxKind.WhitespaceTrivia, tokens[0].LeadingTrivia[0].Kind);
    }

    [Fact]
    public void Token_WithTrailingWhitespace_HasTrailingTrivia()
    {
        var tokens = Lex("x  \ny");
        Assert.NotEmpty(tokens[0].TrailingTrivia);
    }

    [Fact]
    public void MultipleTokens_TriviaAttachedCorrectly()
    {
        // Ensure trivia buffer reuse doesn't corrupt across tokens
        var tokens = Lex("  a  b  c  ");
        Assert.Equal("a", tokens[0].Text);
        Assert.Equal("b", tokens[1].Text);
        Assert.Equal("c", tokens[2].Text);
        // First token has leading whitespace; subsequent tokens get
        // whitespace as trailing trivia of the previous token, not leading.
        Assert.NotEmpty(tokens[0].LeadingTrivia);
        // All tokens except EOF should have trailing whitespace
        Assert.NotEmpty(tokens[0].TrailingTrivia);
        Assert.NotEmpty(tokens[1].TrailingTrivia);
        Assert.NotEmpty(tokens[2].TrailingTrivia);
    }

    // ── Keyword text correctness ─────────────────────────────────────────

    [Fact]
    public void Keywords_HaveCorrectText()
    {
        var tokens = Lex("interface void struct enum namespace");
        Assert.Equal("interface", tokens[0].Text);
        Assert.Equal(SyntaxKind.InterfaceKeyword, tokens[0].Kind);
        Assert.Equal("void", tokens[1].Text);
        Assert.Equal(SyntaxKind.VoidKeyword, tokens[1].Kind);
        Assert.Equal("struct", tokens[2].Text);
        Assert.Equal(SyntaxKind.StructKeyword, tokens[2].Kind);
        Assert.Equal("enum", tokens[3].Text);
        Assert.Equal(SyntaxKind.EnumKeyword, tokens[3].Kind);
        Assert.Equal("namespace", tokens[4].Text);
        Assert.Equal(SyntaxKind.NamespaceKeyword, tokens[4].Kind);
    }

    [Fact]
    public void Keywords_SameKeywordLexedTwice_HasSameText()
    {
        var tokens = Lex("interface interface");
        Assert.Equal("interface", tokens[0].Text);
        Assert.Equal("interface", tokens[1].Text);
        // After optimization, should be the same string reference
        Assert.Same(tokens[0].Text, tokens[1].Text);
    }

    // ── Single-char token text ───────────────────────────────────────────

    [Fact]
    public void SingleCharTokens_HaveCorrectText()
    {
        var tokens = Lex("{ } ( ) [ ] ; : , . ~ ^ % * /");
        Assert.Equal("{", tokens[0].Text);
        Assert.Equal("}", tokens[1].Text);
        Assert.Equal("(", tokens[2].Text);
        Assert.Equal(")", tokens[3].Text);
        Assert.Equal("[", tokens[4].Text);
        Assert.Equal("]", tokens[5].Text);
        Assert.Equal(";", tokens[6].Text);
        Assert.Equal(":", tokens[7].Text);
        Assert.Equal(",", tokens[8].Text);
        Assert.Equal(".", tokens[9].Text);
        Assert.Equal("~", tokens[10].Text);
        Assert.Equal("^", tokens[11].Text);
        Assert.Equal("%", tokens[12].Text);
        Assert.Equal("*", tokens[13].Text);
        Assert.Equal("/", tokens[14].Text);
    }

    // ── ASCII identifier chars ───────────────────────────────────────────

    [Fact]
    public void Identifiers_WithUnderscoresAndDigits()
    {
        var tokens = Lex("_foo bar123 _baz_456_ __x");
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[0].Kind);
        Assert.Equal("_foo", tokens[0].Text);
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[1].Kind);
        Assert.Equal("bar123", tokens[1].Text);
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[2].Kind);
        Assert.Equal("_baz_456_", tokens[2].Text);
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[3].Kind);
        Assert.Equal("__x", tokens[3].Text);
    }

    [Fact]
    public void Identifiers_SingleUnderscore()
    {
        var tokens = Lex("_");
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[0].Kind);
        Assert.Equal("_", tokens[0].Text);
    }

    [Fact]
    public void Identifiers_MixedCaseKeywords()
    {
        // "Interface" (capital I) is not a keyword, should be identifier
        var tokens = Lex("Interface");
        Assert.Equal(SyntaxKind.IdentifierToken, tokens[0].Kind);
        Assert.Equal("Interface", tokens[0].Text);
    }

    // ── Multi-char operators ─────────────────────────────────────────────

    [Fact]
    public void MultiCharOperators_HaveCorrectText()
    {
        var tokens = Lex(":: == != <= >= << >> && || ++ -- ->");
        Assert.Equal("::", tokens[0].Text);
        Assert.Equal("==", tokens[1].Text);
        Assert.Equal("!=", tokens[2].Text);
        Assert.Equal("<=", tokens[3].Text);
        Assert.Equal(">=", tokens[4].Text);
        Assert.Equal("<<", tokens[5].Text);
        Assert.Equal(">>", tokens[6].Text);
        Assert.Equal("&&", tokens[7].Text);
        Assert.Equal("||", tokens[8].Text);
        Assert.Equal("++", tokens[9].Text);
        Assert.Equal("--", tokens[10].Text);
        Assert.Equal("->", tokens[11].Text);
    }

    // ── Numeric and string tokens ────────────────────────────────────────

    [Fact]
    public void NumericLiterals_StillWork()
    {
        var tokens = Lex("42 0xFF 3.14f 0x0A000000");
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[0].Kind);
        Assert.Equal("42", tokens[0].Text);
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[1].Kind);
        Assert.Equal("0xFF", tokens[1].Text);
        Assert.Equal(SyntaxKind.FloatLiteralToken, tokens[2].Kind);
        Assert.Equal("3.14f", tokens[2].Text);
        Assert.Equal(SyntaxKind.NumericLiteralToken, tokens[3].Kind);
        Assert.Equal("0x0A000000", tokens[3].Text);
    }

    [Fact]
    public void StringLiterals_StillWork()
    {
        var tokens = Lex("\"hello\" L\"wide\"");
        Assert.Equal(SyntaxKind.StringLiteralToken, tokens[0].Kind);
        Assert.Equal("\"hello\"", tokens[0].Text);
        Assert.Equal(SyntaxKind.StringLiteralToken, tokens[1].Kind);
        Assert.Equal("L\"wide\"", tokens[1].Text);
    }

    // ── Error tokens ─────────────────────────────────────────────────────

    [Fact]
    public void ErrorToken_UnrecognizedChar()
    {
        var tokens = Lex("@");
        Assert.Equal(SyntaxKind.ErrorToken, tokens[0].Kind);
        Assert.Equal("@", tokens[0].Text);
    }

    // ── Comprehensive end-to-end ─────────────────────────────────────────

    [Fact]
    public void FullIDL_LexesCorrectly()
    {
        const string source = """
            namespace Foo.Bar
            {
                [contract(FooContract, 1)]
                runtimeclass Widget
                {
                    Widget();
                    static Int32 Count { get; };
                    void DoSomething(String name, out Int32 result);
                }
            }
            """;
        var tokens = Lex(source);
        // Should have many tokens and end with EOF
        Assert.True(tokens.Count > 20);
        Assert.Equal(SyntaxKind.EndOfFileToken, tokens[^1].Kind);
        // Should contain expected keywords
        Assert.Contains(tokens, t => t.Kind == SyntaxKind.NamespaceKeyword);
        Assert.Contains(tokens, t => t.Kind == SyntaxKind.RuntimeclassKeyword);
        Assert.Contains(tokens, t => t.Kind == SyntaxKind.StaticKeyword);
        Assert.Contains(tokens, t => t.Kind == SyntaxKind.VoidKeyword);
    }
}

/// <summary>
/// Tests verifying preprocessor behavior is preserved after optimizations.
/// </summary>
public class PreprocessorOptimizationTests
{
    [Fact]
    public void SubstituteMacros_NoUserMacros_PreservesContent()
    {
        // Content with no macro names should pass through (normalize line endings)
        var pp = new Preprocessor();
        var result = pp.Process("hello world\nfoo bar").Replace("\r\n", "\n");
        Assert.Equal("hello world\nfoo bar", result);
    }

    [Fact]
    public void SubstituteMacros_MultipleMacros_AllSubstituted()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define A 1\n#define B 2\n#define C 3\nA B C");
        Assert.Contains("1 2 3", result);
    }

    [Fact]
    public void SubstituteMacros_RecursiveExpansion()
    {
        // X -> Y -> 42
        var pp = new Preprocessor();
        var result = pp.Process("#define X Y\n#define Y 42\nInt32 a = X;");
        Assert.Contains("Int32 a = 42;", result);
    }

    [Fact]
    public void SubstituteMacros_WholeWordOnly()
    {
        // FOOBAR should not be affected by a macro named FOO
        var pp = new Preprocessor();
        var result = pp.Process("#define FOO 1\nFOOBAR FOO");
        var lines = result.Split('\n');
        var lastLine = lines[^1];
        Assert.Contains("FOOBAR", lastLine);
        Assert.Contains("1", lastLine);
    }

    [Fact]
    public void FunctionMacro_StillWorks()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define MAX(a, b) ((a) > (b) ? (a) : (b))\nInt32 x = MAX(1, 2);");
        Assert.Contains("((1) > (2) ? (1) : (2))", result);
    }

    [Fact]
    public void ConditionalCompilation_ComplexNesting()
    {
        var pp = new Preprocessor();
        var result = pp.Process(
            "#define A\n" +
            "#ifdef A\n" +
            "  #define B\n" +
            "  #ifdef B\n" +
            "    inner\n" +
            "  #endif\n" +
            "#endif");
        Assert.Contains("inner", result);
    }

    [Fact]
    public void ExpressionEvaluation_HexComparison()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#if NTDDI_VERSION >= 0x0A000000\nincluded\n#endif");
        Assert.Contains("included", result);
    }

    [Fact]
    public void ExpressionEvaluation_LogicalOr()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#if defined(_WIN32) || defined(_WIN64)\nincluded\n#endif");
        Assert.Contains("included", result);
    }

    [Fact]
    public void DefineUndef_Cycle()
    {
        var pp = new Preprocessor();
        var result = pp.Process(
            "#define X 1\nX\n" +
            "#undef X\n" +
            "#define X 2\nX");
        var lines = result.Split('\n');
        // After first define, X=1; after undef+redefine, X=2
        Assert.Equal("1", lines[1].Trim());
        Assert.Equal("2", lines[4].Trim());
    }

    [Fact]
    public void LargeInput_HandledCorrectly()
    {
        var input = string.Join("\n", Enumerable.Range(0, 1000).Select(i => $"line_{i}"));
        var pp = new Preprocessor();
        var result = pp.Process(input);
        Assert.Contains("line_0", result);
        Assert.Contains("line_999", result);
    }

    [Fact]
    public void LineContinuation_PreservedBehavior()
    {
        var pp = new Preprocessor();
        var result = pp.Process("#define LONG_MACRO \\\n    42\nInt32 x = LONG_MACRO;");
        Assert.Contains("Int32 x = 42;", result);
    }

    [Fact]
    public void Elif_Chain()
    {
        var pp = new Preprocessor();
        var result = pp.Process(
            "#define X 2\n" +
            "#if X == 1\nfirst\n" +
            "#elif X == 2\nsecond\n" +
            "#elif X == 3\nthird\n" +
            "#else\nother\n" +
            "#endif");
        Assert.Contains("second", result);
        Assert.DoesNotContain("first", result);
        Assert.DoesNotContain("third", result);
        Assert.DoesNotContain("other", result);
    }
}
