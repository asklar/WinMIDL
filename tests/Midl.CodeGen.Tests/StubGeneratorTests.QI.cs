using Midl.Analysis;
using Midl.CodeGen.Proxy;
using Xunit;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for table-driven QueryInterface and IsIIDSupported in generated stubs.
/// </summary>
public partial class StubGeneratorTests
{
    private const string SimpleWidgetSource = """
        namespace TestApp
        {
            interface IWidget
            {
                void DoWork();
            }
        }
        """;

    private string GenerateSimpleWidget()
    {
        var compilation = Compile(SimpleWidgetSource);
        return _generator.Generate(compilation);
    }

    // ─── Table-driven QueryInterface ───

    [Fact]
    public void EmitStubClass_QueryInterface_EmitsQIEntryTable()
    {
        var output = GenerateSimpleWidget();

        Assert.Contains("winmidl::qi_entry_t", output);
        Assert.Contains("kStubQIEntries", output);
    }

    [Fact]
    public void EmitStubClass_QueryInterface_TableContainsIUnknownAndStubBuffer()
    {
        var output = GenerateSimpleWidget();

        Assert.Contains("IID_IUnknown", output);
        Assert.Contains("IID_IRpcStubBuffer", output);
    }

    [Fact]
    public void EmitStubClass_QueryInterface_UsesQiCheck()
    {
        var output = GenerateSimpleWidget();

        Assert.Contains("winmidl::qi_check(kStubQIEntries", output);
    }

    [Fact]
    public void EmitStubClass_QueryInterface_NoOldForLoop()
    {
        var output = GenerateSimpleWidget();

        Assert.DoesNotContain("for (const auto& iid : kSupportedIIDs)", output);
        Assert.DoesNotContain("static const IID kSupportedIIDs[]", output);
    }

    [Fact]
    public void EmitStubClass_QueryInterface_EntriesAreNonDelegating()
    {
        var output = GenerateSimpleWidget();

        // Stubs are simpler — all entries offset=0, delegating=false
        Assert.Contains("{ IID_IUnknown, 0, false }", output);
        Assert.Contains("{ IID_IRpcStubBuffer, 0, false }", output);
    }

    [Fact]
    public void EmitStubClass_QueryInterface_TableIsStaticConstexpr()
    {
        var output = GenerateSimpleWidget();

        Assert.Contains("static constexpr winmidl::qi_entry_t kStubQIEntries[]", output);
    }

    [Fact]
    public void EmitStubClass_QueryInterface_PassesNullptrForOuter()
    {
        var output = GenerateSimpleWidget();

        // Stubs have no outer unknown — pass nullptr
        Assert.Contains("winmidl::qi_check(kStubQIEntries, this, nullptr, riid, ppv)", output);
    }

    // ─── IsIIDSupported with type_info ───

    [Fact]
    public void EmitStubClass_HasTypeInfo()
    {
        var output = GenerateSimpleWidget();

        Assert.Contains("winmidl::type_info_t", output);
        Assert.Contains("kTypeInfo", output);
    }

    [Fact]
    public void EmitStubClass_TypeInfo_ContainsInterfaceIID()
    {
        var output = GenerateSimpleWidget();

        Assert.Contains("IID___x_TestApp_IWidget", output);
    }

    [Fact]
    public void EmitStubClass_IsIIDSupported_UsesTypeInfo()
    {
        var output = GenerateSimpleWidget();

        Assert.Contains("kTypeInfo.iids", output);
    }

    [Fact]
    public void EmitStubClass_IsIIDSupported_NoHardcodedIIDComparison()
    {
        var output = GenerateSimpleWidget();

        // Old pattern was: if (riid == IID___x_TestApp_IWidget) { ... }
        Assert.DoesNotContain("riid == IID___x_TestApp_IWidget", output);
    }
}
