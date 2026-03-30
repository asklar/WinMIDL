using Midl.Analysis;
using Midl.Lsp.Handlers;

namespace Midl.Integration.Tests;

public class CodeLensNamespaceTests
{
    [Fact]
    public void NamespaceBlock_ShowsTypeCount()
    {
        var source = "namespace Animals {\n    enum Species { Cat, Dog };\n    struct Point { Int32 X; Int32 Y; };\n    runtimeclass Pet { Pet(); };\n}";
        var compilation = CompilationHelper.Compile(source);

        var lenses = CodeLensHandler.HandleCore(source, compilation);

        Assert.NotEmpty(lenses);
        var cmd = (Dictionary<string, object?>)lenses[0]["command"]!;
        var title = (string)cmd["title"]!;
        Assert.Contains("enum", title);
        Assert.Contains("struct", title);
        Assert.Contains("class", title);
    }

    [Fact]
    public void EmptyNamespace_NoLens()
    {
        var source = "namespace Empty { }";
        var compilation = CompilationHelper.Compile(source);
        var lenses = CodeLensHandler.HandleCore(source, compilation);
        Assert.Empty(lenses);
    }

    [Fact]
    public void MultipleNamespaces_EachGetsLens()
    {
        var source = "namespace A {\n    enum E1 { X };\n}\nnamespace B {\n    struct S { Int32 V; };\n    runtimeclass C { C(); };\n}";
        var compilation = CompilationHelper.Compile(source);
        var lenses = CodeLensHandler.HandleCore(source, compilation);
        Assert.Equal(2, lenses.Count);
    }
}
