using Midl.Lsp.Handlers;

namespace Midl.Integration.Tests;

public class HoverDecompiledTests
{
    #region InferSymbolKind

    [Theory]
    [InlineData("    runtimeclass Widget", 19, "runtimeclass")]
    [InlineData("    interface IFoo", 14, "interface")]
    [InlineData("    enum Color", 9, "enum")]
    [InlineData("    struct Point", 11, "struct")]
    [InlineData("    delegate void Handler", 19, "delegate")]
    [InlineData("    apicontract MyContract", 20, "apicontract")]
    [InlineData("    event EventHandler Clicked", 23, "event")]
    public void InferSymbolKind_TypeDeclarations(string lineText, int namePos, string expectedKind)
    {
        // Append enough after the name for detection
        var text = lineText + " { }\n";
        var kind = InvokeInferSymbolKind(text, namePos);
        Assert.Equal(expectedKind, kind);
    }

    [Fact]
    public void InferSymbolKind_Method_DetectedByParenthesis()
    {
        var text = "    void DoSomething(Int32 x);\n";
        var namePos = text.IndexOf("DoSomething");
        var kind = InvokeInferSymbolKind(text, namePos);
        Assert.Equal("method", kind);
    }

    [Fact]
    public void InferSymbolKind_StaticMethod()
    {
        var text = "    static void Create();\n";
        var namePos = text.IndexOf("Create");
        var kind = InvokeInferSymbolKind(text, namePos);
        Assert.Equal("method", kind);
    }

    [Fact]
    public void InferSymbolKind_Property_DetectedByBrace()
    {
        var text = "    String Name { get; set; };\n";
        var namePos = text.IndexOf("Name");
        var kind = InvokeInferSymbolKind(text, namePos);
        Assert.Equal("property", kind);
    }

    [Fact]
    public void InferSymbolKind_Property_Shorthand()
    {
        var text = "    String Name;\n";
        var namePos = text.IndexOf("Name");
        var kind = InvokeInferSymbolKind(text, namePos);
        Assert.Equal("property", kind);
    }

    #endregion

    #region FindEnclosingNamespace

    [Fact]
    public void FindEnclosingNamespace_BasicNamespace()
    {
        var text = "namespace Windows.UI.Xaml {\n    runtimeclass App { }\n}";
        var ns = InvokeFindEnclosingNamespace(text, text.IndexOf("App"));
        Assert.Equal("Windows.UI.Xaml", ns);
    }

    [Fact]
    public void FindEnclosingNamespace_NoNamespace()
    {
        var text = "runtimeclass Widget { }";
        var ns = InvokeFindEnclosingNamespace(text, text.IndexOf("Widget"));
        Assert.Null(ns);
    }

    [Fact]
    public void FindEnclosingNamespace_NestedPosition()
    {
        var text = "namespace Outer {\n    namespace Inner {\n        runtimeclass Deep { }\n    }\n}";
        var ns = InvokeFindEnclosingNamespace(text, text.IndexOf("Deep"));
        // Should find the nearest namespace (Inner)
        Assert.Equal("Inner", ns);
    }

    #endregion

    #region Full hover on decompiled docs

    [Fact]
    public void Hover_DecompiledType_ShowsRuntimeclassKind()
    {
        var text = "namespace Windows.UI.Xaml {\n    unsealed runtimeclass Application {\n    }\n}";
        var uri = "midl-winmd:///C:/sdk/Windows.winmd/Windows.UI.Xaml.Application";
        var dm = new Midl.Lsp.DocumentManager();
        dm.OpenDocument(uri, text);

        var namePos = text.IndexOf("Application");
        var (line, ch) = Midl.Lsp.PositionHelper.OffsetToLineChar(
            Midl.Lsp.PositionHelper.BuildLineStarts(text), namePos);

        var result = HoverHandler.Handle(MakePositionParams(uri, line, ch), dm);
        Assert.NotNull(result);
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        Assert.Contains("runtimeclass", json);
        Assert.Contains("Windows.UI.Xaml.Application", json);
    }

    [Fact]
    public void Hover_DecompiledMethod_ShowsMethodKind()
    {
        var text = "namespace Windows.UI.Xaml {\n    runtimeclass App {\n        void Initialize();\n    }\n}";
        var uri = "midl-winmd:///C:/sdk/Windows.winmd/Windows.UI.Xaml.App";
        var dm = new Midl.Lsp.DocumentManager();
        dm.OpenDocument(uri, text);

        var namePos = text.IndexOf("Initialize");
        var (line, ch) = Midl.Lsp.PositionHelper.OffsetToLineChar(
            Midl.Lsp.PositionHelper.BuildLineStarts(text), namePos);

        var result = HoverHandler.Handle(MakePositionParams(uri, line, ch), dm);
        Assert.NotNull(result);
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        Assert.Contains("method", json);
        Assert.Contains("Initialize", json);
        Assert.DoesNotContain("Windows.UI.Xaml.Initialize", json); // no namespace for methods
    }

    [Fact]
    public void Hover_DecompiledProperty_ShowsPropertyKind()
    {
        var text = "namespace Windows.UI.Xaml {\n    runtimeclass App {\n        String Name { get; };\n    }\n}";
        var uri = "midl-winmd:///C:/sdk/Windows.winmd/Windows.UI.Xaml.App";
        var dm = new Midl.Lsp.DocumentManager();
        dm.OpenDocument(uri, text);

        var namePos = text.IndexOf("Name");
        var (line, ch) = Midl.Lsp.PositionHelper.OffsetToLineChar(
            Midl.Lsp.PositionHelper.BuildLineStarts(text), namePos);

        var result = HoverHandler.Handle(MakePositionParams(uri, line, ch), dm);
        Assert.NotNull(result);
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        Assert.Contains("property", json);
    }

    [Fact]
    public void Hover_DecompiledSdkType_HasLearnLink()
    {
        var text = "namespace Windows.Foundation {\n    runtimeclass Uri {\n    }\n}";
        var uri = "midl-winmd:///C:/Program Files (x86)/Windows Kits/10/UnionMetadata/10.0.26100.0/Windows.winmd/Windows.Foundation.Uri";
        var dm = new Midl.Lsp.DocumentManager();
        dm.OpenDocument(uri, text);

        var namePos = text.IndexOf("Uri");
        var (line, ch) = Midl.Lsp.PositionHelper.OffsetToLineChar(
            Midl.Lsp.PositionHelper.BuildLineStarts(text), namePos);

        var result = HoverHandler.Handle(MakePositionParams(uri, line, ch), dm);
        Assert.NotNull(result);
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        Assert.Contains("learn.microsoft.com", json);
        Assert.Contains("Windows.Foundation.Uri", json);
    }

    [Fact]
    public void Hover_DecompiledLowercase_NoHover()
    {
        var text = "namespace Windows.UI.Xaml {\n    runtimeclass App {\n        void Do(String name);\n    }\n}";
        var uri = "midl-winmd:///C:/sdk/Windows.winmd/Windows.UI.Xaml.App";
        var dm = new Midl.Lsp.DocumentManager();
        dm.OpenDocument(uri, text);

        // "name" is lowercase — should get no decompiled hover (falls through)
        var namePos = text.IndexOf("name)");
        var (line, ch) = Midl.Lsp.PositionHelper.OffsetToLineChar(
            Midl.Lsp.PositionHelper.BuildLineStarts(text), namePos);

        var result = HoverHandler.Handle(MakePositionParams(uri, line, ch), dm);
        // Either null or generic identifier hover — not a type hover
        if (result != null)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(result);
            Assert.DoesNotContain("Windows.UI.Xaml.name", json);
        }
    }

    #endregion

    #region Helpers

    private static System.Text.Json.JsonElement? MakePositionParams(string uri, int line, int character)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            textDocument = new { uri },
            position = new { line, character }
        });
        return System.Text.Json.JsonDocument.Parse(json).RootElement;
    }

    // Use reflection to call private static methods for unit testing
    private static string InvokeInferSymbolKind(string text, int position)
    {
        var method = typeof(HoverHandler).GetMethod("InferSymbolKind",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (string)method!.Invoke(null, [text, position])!;
    }

    private static string? InvokeFindEnclosingNamespace(string text, int position)
    {
        var method = typeof(HoverHandler).GetMethod("FindEnclosingNamespace",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (string?)method!.Invoke(null, [text, position]);
    }

    #endregion
}
