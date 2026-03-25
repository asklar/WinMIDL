using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Stub;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests for DllPlumbingBuilder — compiles IDL source end-to-end
/// and verifies DLL registration plumbing is emitted by StubEmitter.
/// Uses partial class to allow splitting tests across files.
/// </summary>
public partial class DllPlumbingIntegrationTests
{
    private static Compilation Compile(string source) =>
        CompilationHelper.Compile(source, CompilationMode.Compat);

    private static void AssertNoErrors(Compilation compilation)
    {
        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Midl.Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void StubEmitter_SingleInterface_EmitsDllPlumbing()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789012)]
                interface IWidget : IInspectable
                {
                    HRESULT GetValue([out, retval] Int32* result);
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        var output = emitter.Emit(compilation, "test_file");

        // Should contain DLL plumbing sections
        Assert.Contains("_ProxyVtblList", output);
        Assert.Contains("_StubVtblList", output);
        Assert.Contains("_InterfaceNamesList", output);
        Assert.Contains("CLSID_PSFactoryBuffer", output);
        Assert.Contains("__MIDL_ProxyFileInfo", output);
        Assert.Contains("g_ProxyFileList", output);
    }

    [Fact]
    public void StubEmitter_SingleInterface_EmitsDllExports()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789012)]
                interface ISimple : IInspectable
                {
                    HRESULT DoWork();
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        var output = emitter.Emit(compilation, "test_file");

        Assert.Contains("DllGetClassObject", output);
        Assert.Contains("DllCanUnloadNow", output);
        Assert.Contains("DllRegisterServer", output);
        Assert.Contains("DllUnregisterServer", output);
        Assert.Contains("GetProxyDllInfo", output);
    }

    [Fact]
    public void StubEmitter_MultipleInterfaces_EmitsAllInVtblLists()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(11111111-1111-1111-1111-111111111111)]
                interface IFirst : IInspectable
                {
                    HRESULT Alpha();
                }

                [uuid(22222222-2222-2222-2222-222222222222)]
                interface ISecond : IInspectable
                {
                    HRESULT Beta();
                    HRESULT Gamma();
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        var output = emitter.Emit(compilation, "multi_test");

        // ProxyVtblList should reference both interfaces
        Assert.Contains("_IFirstProxyVtbl", output);
        Assert.Contains("_ISecondProxyVtbl", output);

        // StubVtblList should reference both interfaces
        Assert.Contains("_IFirstStubVtbl", output);
        Assert.Contains("_ISecondStubVtbl", output);

        // InterfaceNamesList should contain both names
        Assert.Contains("\"IFirst\"", output);
        Assert.Contains("\"ISecond\"", output);
    }

    [Fact]
    public void StubEmitter_DllPlumbing_ComesAfterPerInterfaceStructures()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789012)]
                interface IWidget : IInspectable
                {
                    HRESULT DoWork();
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        var output = emitter.Emit(compilation, "test_file");

        // Per-interface structures should come before DLL plumbing
        int stubVtblIdx = output.IndexOf("_IWidgetStubVtbl");
        int proxyListIdx = output.IndexOf("_ProxyVtblList");
        int dllExportIdx = output.IndexOf("DllGetClassObject");

        Assert.True(stubVtblIdx >= 0, "Should contain per-interface stub vtable");
        Assert.True(proxyListIdx >= 0, "Should contain ProxyVtblList");
        Assert.True(dllExportIdx >= 0, "Should contain DLL exports");
        Assert.True(stubVtblIdx < proxyListIdx, "Per-interface structures should come before DLL plumbing");
        Assert.True(proxyListIdx < dllExportIdx, "Proxy file info should come before DLL exports");
    }

    [Fact]
    public void StubEmitter_ProxyFileInfo_HasCorrectTableSize()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(11111111-1111-1111-1111-111111111111)]
                interface IAlpha : IInspectable
                {
                    HRESULT Foo();
                }

                [uuid(22222222-2222-2222-2222-222222222222)]
                interface IBeta : IInspectable
                {
                    HRESULT Bar();
                }

                [uuid(33333333-3333-3333-3333-333333333333)]
                interface IGamma : IInspectable
                {
                    HRESULT Baz();
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        var output = emitter.Emit(compilation, "test_file");

        // ProxyFileInfo should have TableSize = 3
        Assert.Contains("__MIDL_ProxyFileInfo", output);
    }

    [Fact]
    public void StubEmitter_DllExports_CallNdrFunctions()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789012)]
                interface IFoo : IInspectable
                {
                    HRESULT DoWork();
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        var output = emitter.Emit(compilation, "test_file");

        Assert.Contains("NdrDllGetClassObject", output);
        Assert.Contains("NdrDllCanUnloadNow", output);
        Assert.Contains("NdrDllRegisterProxy", output);
        Assert.Contains("NdrDllUnregisterProxy", output);
    }

    [Fact]
    public void StubEmitter_IUnknownDerived_EmitsInPlumbing()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789012)]
                interface ISimpleCalc requires IUnknown
                {
                    HRESULT Add([in] Int32 a, [in] Int32 b, [out, retval] Int32* result);
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        var output = emitter.Emit(compilation, "test_file");

        // Should include IUnknown-derived interface in plumbing
        Assert.Contains("_ISimpleCalcProxyVtbl", output);
        Assert.Contains("_ISimpleCalcStubVtbl", output);
        Assert.Contains("\"ISimpleCalc\"", output);
    }
}
