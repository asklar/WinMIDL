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

        // Should have proper 4-space indentation
        Assert.Contains("namespace TestApp", formatted);
        Assert.Contains("    runtimeclass Widget", formatted);
        Assert.Contains("        void DoWork", formatted);
    }

    [Fact]
    public void PreserveCorrectFormatting()
    {
        var input = "namespace TestApp\n{\n    runtimeclass Widget\n    {\n        void DoWork();\n    }\n}\n";

        var formatted = FormattingHandler.Format(input);

        // Already well-formatted content should be mostly preserved
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
}
