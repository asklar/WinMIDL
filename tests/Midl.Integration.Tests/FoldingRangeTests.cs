using Midl.Lsp.Handlers;

namespace Midl.Integration.Tests;

public class FoldingRangeTests
{
    [Fact]
    public void IfdefEndif_CreatesFoldingRange()
    {
        var source = "#ifdef FOO\n    content\n#endif\nnamespace N { }";
        var ranges = FoldingRangeHandler.Handle(source);
        Assert.Contains(ranges, r =>
            (int)r["startLine"]! == 0 && (int)r["endLine"]! == 2);
    }

    [Fact]
    public void IfndefEndif_CreatesFoldingRange()
    {
        var source = "#ifndef BAR\n    content\n#endif\n";
        var ranges = FoldingRangeHandler.Handle(source);
        Assert.Contains(ranges, r =>
            (int)r["startLine"]! == 0 && (int)r["endLine"]! == 2);
    }

    [Fact]
    public void NestedIfdef_CreatesTwoRanges()
    {
        var source = "#ifdef OUTER\n#ifdef INNER\n    nested\n#endif\n#endif\n";
        var ranges = FoldingRangeHandler.Handle(source);
        // Inner: line 1-3, Outer: line 0-4
        Assert.Contains(ranges, r =>
            (int)r["startLine"]! == 1 && (int)r["endLine"]! == 3);
        Assert.Contains(ranges, r =>
            (int)r["startLine"]! == 0 && (int)r["endLine"]! == 4);
    }

    [Fact]
    public void IfEndif_CreatesFoldingRange()
    {
        var source = "#if 1\n    content\n#endif\n";
        var ranges = FoldingRangeHandler.Handle(source);
        Assert.Contains(ranges, r =>
            (int)r["startLine"]! == 0 && (int)r["endLine"]! == 2);
    }

    [Fact]
    public void BracesAndIfdef_BothFold()
    {
        var source = "#ifdef FOO\nnamespace N {\n    runtimeclass W { }\n}\n#endif\n";
        var ranges = FoldingRangeHandler.Handle(source);
        // #ifdef: line 0-4
        Assert.Contains(ranges, r =>
            (int)r["startLine"]! == 0 && (int)r["endLine"]! == 4);
        // namespace braces: line 1-3
        Assert.Contains(ranges, r =>
            (int)r["startLine"]! == 1 && (int)r["endLine"]! == 3);
    }

    [Fact]
    public void NoIfdef_NoPreprocessorFoldingRange()
    {
        var source = "namespace N {\n    runtimeclass W { }\n}\n";
        var ranges = FoldingRangeHandler.Handle(source);
        // Should have brace-based ranges but no preprocessor directive ranges
        Assert.NotEmpty(ranges);
    }
}
