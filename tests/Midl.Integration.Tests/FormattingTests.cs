// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

        var formatted = DocumentFormatter.Format(input);

        Assert.Contains("namespace TestApp", formatted);
        Assert.Contains("    runtimeclass Widget", formatted);
        Assert.Contains("        void DoWork", formatted);
    }

    [Fact]
    public void PreserveCorrectFormatting()
    {
        var input = "namespace TestApp\n{\n    runtimeclass Widget\n    {\n        void DoWork();\n    }\n}\n";

        var formatted = DocumentFormatter.Format(input);

        Assert.Contains("namespace TestApp", formatted);
        Assert.Contains("    runtimeclass Widget", formatted);
        Assert.Contains("        void DoWork();", formatted);
    }

    [Fact]
    public void SpaceAfterComma()
    {
        var input = "namespace TestApp\n{\n    runtimeclass Widget\n    {\n        void DoWork(Int32 a,String b);\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        Assert.Contains("Int32 a, String b", formatted);
    }

    [Fact]
    public void SpaceAroundColon()
    {
        var input = "namespace TestApp\n{\n    runtimeclass Widget:Base\n    {\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        Assert.Contains("Widget : Base", formatted);
    }

    [Fact]
    public void NoSpaceBeforeSemicolon()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        void Foo() ;\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        Assert.Contains("void Foo();", formatted);
        Assert.DoesNotContain("() ;", formatted);
    }

    [Fact]
    public void CollapseMultipleBlankLines()
    {
        var input = "namespace N\n{\n\n\n\n    runtimeclass C\n    {\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        // Should have at most one blank line between namespace { and runtimeclass
        Assert.DoesNotContain("\n\n\n", formatted);
    }

    [Fact]
    public void NoBlankLineAfterOpenBrace()
    {
        var input = "namespace N\n{\n\n    runtimeclass C\n    {\n\n        void Foo();\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        // No blank line immediately after {
        Assert.DoesNotContain("{\n\n", formatted);
    }

    [Fact]
    public void NoBlankLineBeforeCloseBrace()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        void Foo();\n\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        Assert.DoesNotContain("\n\n    }", formatted);
    }

    [Fact]
    public void TightGenericSpacing()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        IVector < String > Foo();\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        Assert.Contains("IVector<String>", formatted);
        Assert.DoesNotContain("< String >", formatted);
    }

    [Fact]
    public void NoSpaceInsideParentheses()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        void Foo( Int32 x , String y );\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        Assert.Contains("Foo(Int32 x, String y)", formatted);
    }

    [Fact]
    public void PropertyAccessorSpacing()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        String Name{get; set;};\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        Assert.Contains("Name { get; set; };", formatted);
    }

    [Fact]
    public void CommentPreserved()
    {
        var input = "// header comment\nnamespace N\n{\n    // type comment\n    runtimeclass C\n    {\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        Assert.Contains("// header comment", formatted);
        Assert.Contains("// type comment", formatted);
    }

    [Fact]
    public void TrailingNewline()
    {
        var input = "namespace N\n{\n}";
        var formatted = DocumentFormatter.Format(input);
        Assert.EndsWith("\n", formatted);
    }

    [Fact]
    public void MultipleSpacesCollapsed()
    {
        var input = "namespace N\n{\n    runtimeclass   C\n    {\n        void    Foo();\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        Assert.Contains("runtimeclass C", formatted);
        Assert.Contains("void Foo()", formatted);
        Assert.DoesNotContain("  C", formatted);
    }

    [Fact]
    public void BraceOnSameLineAsDeclration_MovesToOwnLine()
    {
        var input = "namespace Foo{\n    runtimeclass Widget{\n        void DoWork();\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        // { should be on its own line, not same line as namespace/runtimeclass
        Assert.Contains("namespace Foo\n{", formatted);
        Assert.Contains("runtimeclass Widget\n    {", formatted);
    }

    [Fact]
    public void ClosingBraceOnSameLineAsContent_MovesToOwnLine()
    {
        var input = "namespace hello{\n\n    interface Ibar{\n        Single x { set; get;};     }\n\n}\n";
        var formatted = DocumentFormatter.Format(input);
        // The closing } of the interface should be on its own line
        Assert.DoesNotContain("};     }", formatted);
        Assert.DoesNotContain(";}", formatted.Replace("};", "##").Replace("; }", "##"));
        // Should have } on its own line
        var lines = formatted.Split('\n');
        var closingBraceLines = lines.Where(l => l.Trim() == "}" || l.Trim() == "};").ToList();
        Assert.True(closingBraceLines.Count >= 2, "Should have at least 2 closing brace lines (interface + namespace)");
    }

    [Fact]
    public void PropertyAccessorInline_GetOnly()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        Int32 Value{get;};\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        Assert.Contains("Int32 Value { get; };", formatted);
    }

    [Fact]
    public void PropertyAccessorInline_GetSet()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        String Name{get;set;};\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        Assert.Contains("String Name { get; set; };", formatted);
    }

    [Fact]
    public void MultiLinePropertyAccessor_JoinedInline()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        String Name\n        {\n            get;\n            set;\n        };\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        Assert.Contains("String Name { get; set; };", formatted);
        // Should NOT have get; or set; on their own indented lines
        var lines = formatted.Split('\n');
        Assert.DoesNotContain(lines, l => l.Trim() == "get;");
        Assert.DoesNotContain(lines, l => l.Trim() == "set;");
    }

    [Fact]
    public void AttributeGroupBlankLine()
    {
        var input = "namespace N\n{\n    runtimeclass A\n    {\n    }\n    [contract(C, 1)]\n    [activatable(1)]\n    runtimeclass B\n    {\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        // Blank line before first attribute of a group
        Assert.Contains("}\n\n    [contract(C, 1)]", formatted);
        // No blank line between consecutive attributes
        Assert.Contains("[contract(C, 1)]\n    [activatable(1)]", formatted);
    }

    [Fact]
    public void CommentReindented()
    {
        var input = "namespace N\n{\n// misindented comment\n    runtimeclass C\n    {\n// another comment\n        void Foo();\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        // Comments should be indented to match their context
        Assert.Contains("    // misindented comment", formatted);
        Assert.Contains("        // another comment", formatted);
    }

    [Fact]
    public void GenericCommaSpacing()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        event TypedEventHandler<Widget,Object> Changed;\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        Assert.Contains("TypedEventHandler<Widget, Object>", formatted);
    }

    [Fact]
    public void SemicolonSpacingInsideAccessors()
    {
        var input = "namespace N\n{\n    runtimeclass C\n    {\n        String X{get;set;};\n    }\n}\n";
        var formatted = DocumentFormatter.Format(input);
        // Space after ; between accessors
        Assert.Contains("{ get; set; }", formatted);
        // No space before ;
        Assert.DoesNotContain("get ;", formatted);
        Assert.DoesNotContain("set ;", formatted);
    }

    [Fact]
    public void FullDocumentFormatting()
    {
        var input = """
            // My widget
            namespace MyApp{
            [contract(MyContract, 1)]
                 [activatable(1)]
            runtimeclass Widget : SomeBase
            {
            Widget();
                 Widget(String name);
                     String Name{get;set;};
                Int32 Value{get;};
                  void DoWork(  Int32 x,String y  );
            event TypedEventHandler<Widget,Object> Changed;
            }
            }
            """;

        var formatted = DocumentFormatter.Format(input);

        Assert.Contains("// My widget", formatted);
        Assert.Contains("namespace MyApp", formatted);
        Assert.Contains("    [contract(MyContract, 1)]", formatted);
        Assert.Contains("    [activatable(1)]", formatted);
        Assert.Contains("    runtimeclass Widget : SomeBase", formatted);
        Assert.Contains("        Widget();", formatted);
        Assert.Contains("        Widget(String name);", formatted);
        Assert.Contains("        String Name { get; set; };", formatted);
        Assert.Contains("        Int32 Value { get; };", formatted);
        Assert.Contains("        void DoWork(Int32 x, String y);", formatted);
        Assert.Contains("        event TypedEventHandler<Widget, Object> Changed;", formatted);
    }
}
