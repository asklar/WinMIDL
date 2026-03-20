using Midl.Lsp.Handlers;

namespace Midl.Integration.Tests;

public class FormattingTests
{
    [Fact]
    public void NormalizeIndentation()
    {
        var input = """
            namespace TestApp{
            runtimeclass Widget{
            void DoWork(Int32 count);
            String Title{get; set;};
            }
            }
            """;

        var formatted = FormattingHandler.Format(input);

        Assert.Contains("namespace TestApp", formatted);
        Assert.Contains("    runtimeclass Widget", formatted);
        Assert.Contains("        void DoWork", formatted);
    }

    [Fact]
    public void PreserveCorrectFormatting()
    {
        var input = "namespace TestApp\n{\n    runtimeclass Widget\n    {\n        void DoWork();\n    }\n}\n";

        var formatted = FormattingHandler.Format(input);

        Assert.Contains("namespace TestApp", formatted);
        Assert.Contains("    runtimeclass Widget", formatted);
        Assert.Contains("        void DoWork();", formatted);
    }

    [Fact]
    public void SpaceAfterComma()
    {
        var input = "namespace TestApp\n{\n    runtimeclass Widget\n    {\n        void DoWork(Int32 a,String b);\n    }\n}\n";
        var formatted = FormattingHandler.Format(input);
        Assert.Contains("Int32 a, String b", formatted);
    }

    [Fact]
    public void SpaceAroundColon()
    {
        var input = "namespace TestApp\n{\n    runtimeclass Widget:Base\n    {\n    }\n}\n";
        var formatted = FormattingHandler.Format(input);
        Assert.Contains("Widget : Base", formatted);
    }

    [Fact]
    public void NoSpaceBeforeSemicolon()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        void Foo() ;\n    }\n}\n";
        var formatted = FormattingHandler.Format(input);
        Assert.Contains("void Foo();", formatted);
        Assert.DoesNotContain("() ;", formatted);
    }

    [Fact]
    public void CollapseMultipleBlankLines()
    {
        var input = "namespace N\n{\n\n\n\n    runtimeclass C\n    {\n    }\n}\n";
        var formatted = FormattingHandler.Format(input);
        // Should have at most one blank line between namespace { and runtimeclass
        Assert.DoesNotContain("\n\n\n", formatted);
    }

    [Fact]
    public void NoBlankLineAfterOpenBrace()
    {
        var input = "namespace N\n{\n\n    runtimeclass C\n    {\n\n        void Foo();\n    }\n}\n";
        var formatted = FormattingHandler.Format(input);
        // No blank line immediately after {
        Assert.DoesNotContain("{\n\n", formatted);
    }

    [Fact]
    public void NoBlankLineBeforeCloseBrace()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        void Foo();\n\n    }\n}\n";
        var formatted = FormattingHandler.Format(input);
        Assert.DoesNotContain("\n\n    }", formatted);
    }

    [Fact]
    public void TightGenericSpacing()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        IVector < String > Foo();\n    }\n}\n";
        var formatted = FormattingHandler.Format(input);
        Assert.Contains("IVector<String>", formatted);
        Assert.DoesNotContain("< String >", formatted);
    }

    [Fact]
    public void NoSpaceInsideParentheses()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        void Foo( Int32 x , String y );\n    }\n}\n";
        var formatted = FormattingHandler.Format(input);
        Assert.Contains("Foo(Int32 x, String y)", formatted);
    }

    [Fact]
    public void PropertyAccessorSpacing()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        String Name{get; set;};\n    }\n}\n";
        var formatted = FormattingHandler.Format(input);
        Assert.Contains("Name { get; set;}", formatted);
    }

    [Fact]
    public void CommentPreserved()
    {
        var input = "// header comment\nnamespace N\n{\n    // type comment\n    runtimeclass C\n    {\n    }\n}\n";
        var formatted = FormattingHandler.Format(input);
        Assert.Contains("// header comment", formatted);
        Assert.Contains("// type comment", formatted);
    }

    [Fact]
    public void TrailingNewline()
    {
        var input = "namespace N\n{\n}";
        var formatted = FormattingHandler.Format(input);
        Assert.EndsWith("\n", formatted);
    }

    [Fact]
    public void MultipleSpacesCollapsed()
    {
        var input = "namespace N\n{\n    runtimeclass   C\n    {\n        void    Foo();\n    }\n}\n";
        var formatted = FormattingHandler.Format(input);
        Assert.Contains("runtimeclass C", formatted);
        Assert.Contains("void Foo()", formatted);
        Assert.DoesNotContain("  C", formatted);
    }
}
