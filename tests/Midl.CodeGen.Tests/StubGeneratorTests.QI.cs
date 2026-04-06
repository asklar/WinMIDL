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

        // stub_base_t uses qi_check internally; generated code provides the kStubQIEntries table
        Assert.Contains("kStubQIEntries", output);
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

        Assert.Contains("static inline const winmidl::qi_entry_t kStubQIEntries[]", output);
    }

    [Fact]
    public void EmitStubClass_QueryInterface_PassesNullptrForOuter()
    {
        var output = GenerateSimpleWidget();

        // stub_base_t<Derived> calls qi_check(Derived::kStubQIEntries, this, nullptr, riid, ppv)
        // The generated class provides kStubQIEntries with non-delegating entries (offset=0, delegating=false)
        Assert.Contains("{ IID_IUnknown, 0, false }", output);
        Assert.Contains("{ IID_IRpcStubBuffer, 0, false }", output);
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

    // ─── stub_base_t CRTP inheritance ───

    [Fact]
    public void EmitStubClass_UsesStubBaseT_NotDirectIRpcStubBuffer()
    {
        var output = GenerateSimpleWidget();

        // Stubs now inherit from winmidl::stub_base_t<Derived> (mirrors winrt::implements pattern)
        Assert.Contains("winmidl::stub_base_t<CStub___x_TestApp_IWidget>", output);
        // Direct IRpcStubBuffer inheritance is replaced by the base template
        Assert.DoesNotContain(": public IRpcStubBuffer", output);
    }

    [Fact]
    public void EmitStubClass_StubBaseT_QIEntryTableStillEmitted()
    {
        var output = GenerateSimpleWidget();

        // kStubQIEntries must still be in generated code — stub_base_t accesses it via Derived::kStubQIEntries
        Assert.Contains("static inline const winmidl::qi_entry_t kStubQIEntries[]", output);
        Assert.Contains("{ IID_IUnknown, 0, false }", output);
        Assert.Contains("{ IID_IRpcStubBuffer, 0, false }", output);
    }
}
