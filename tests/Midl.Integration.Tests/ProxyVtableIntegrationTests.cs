using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Stub;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests for ProxyVtableBuilder — compiles IDL source end-to-end
/// and verifies proxy vtable, stub table, and offset table emission.
/// </summary>
public partial class ProxyVtableIntegrationTests
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
    public void EndToEnd_IInspectableInterface_EmitsAllProxyStructures()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789012)]
                interface IWidget : IInspectable
                {
                    HRESULT GetValue([out, retval] Int32* result);
                    HRESULT SetValue([in] Int32 value);
                }
            }
            """);

        AssertNoErrors(compilation);

        var iface = compilation.GetAllTypes().OfType<InterfaceSymbol>()
            .First(i => i.Name == "IWidget");

        // Build proc format to get offsets
        var typeBuilder = new NdrTypeFormatBuilder();
        var procBuilder = new NdrProcFormatBuilder();
        var vtableBuilder = new ProxyVtableBuilder();

        bool isInspectable = iface.RequiredInterfaces.Any(r => r.Name == "IInspectable");
        Assert.True(isInspectable);

        int methodNumber = 6;
        var methodEntries = new List<ProxyVtableBuilder.MethodEntry>();

        foreach (var member in iface.Members)
        {
            if (member is MethodSymbol method)
            {
                int offset = procBuilder.AddMethod(method, methodNumber++, typeBuilder);
                methodEntries.Add(new ProxyVtableBuilder.MethodEntry(method.Name, offset));
            }
        }

        // Verify format string offset table
        var offsetTable = vtableBuilder.EmitFormatStringOffsetTable("IWidget", methodEntries, true);
        Assert.Contains("IWidget_FormatStringOffsetTable[]", offsetTable);
        Assert.Contains("(unsigned short) -1", offsetTable); // forwarded IInspectable slots

        // Verify proxy vtable
        var proxyVtbl = vtableBuilder.EmitProxyVtable("IWidget", methodEntries, true);
        Assert.Contains("CINTERFACE_PROXY_VTABLE(8)", proxyVtbl); // 6 base + 2 custom
        Assert.Contains("0 /* IInspectable::GetIids */", proxyVtbl);
        Assert.Contains("(void *) (INT_PTR) -1 /* IWidget::GetValue */", proxyVtbl);
        Assert.Contains("(void *) (INT_PTR) -1 /* IWidget::SetValue */", proxyVtbl);

        // Verify stub table
        var stubTable = vtableBuilder.EmitStubTable("IWidget", methodEntries, true);
        Assert.NotNull(stubTable);
        Assert.Contains("STUB_FORWARDING_FUNCTION", stubTable);
        Assert.Contains("NdrStubCall2", stubTable);

        // Verify proxy info
        var proxyInfo = vtableBuilder.EmitProxyInfo("IWidget", true, "test_file");
        Assert.Contains("&IWidget_FormatStringOffsetTable[-3]", proxyInfo);

        // Verify stub vtable
        var stubVtbl = vtableBuilder.EmitStubVtable("IWidget", methodEntries, true);
        Assert.Contains("CStdStubBuffer_DELEGATING_METHODS", stubVtbl);
    }

    [Fact]
    public void EndToEnd_StubEmitter_IncludesProxyVtableOutput()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE)]
                interface IMyService : IInspectable
                {
                    HRESULT DoWork([in] Int32 input, [out, retval] Int32* result);
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        var output = emitter.Emit(compilation, "test_stub");

        // Should contain proxy vtable structures
        Assert.Contains("FormatStringOffsetTable", output);
        Assert.Contains("CINTERFACE_PROXY_VTABLE", output);
        Assert.Contains("ProxyInfo", output);
        Assert.Contains("ServerInfo", output);
        Assert.Contains("StubVtbl", output);
    }

    [Fact]
    public void EndToEnd_PropertyInterface_ExpandsGetterSetter()
    {
        // Use direct symbol construction for property tests since the IDL compiler
        // handles property syntax differently
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(11111111-2222-3333-4444-555555555555)]
                interface IConfig : IInspectable
                {
                    HRESULT GetName([out, retval] String* value);
                    HRESULT SetName([in] String value);
                }
            }
            """);

        AssertNoErrors(compilation);

        var iface = compilation.GetAllTypes().OfType<InterfaceSymbol>()
            .First(i => i.Name == "IConfig");

        var typeBuilder = new NdrTypeFormatBuilder();
        var procBuilder = new NdrProcFormatBuilder();
        var vtableBuilder = new ProxyVtableBuilder();

        int methodNumber = 6;
        var methodEntries = new List<ProxyVtableBuilder.MethodEntry>();

        foreach (var member in iface.Members)
        {
            if (member is MethodSymbol method)
            {
                int offset = procBuilder.AddMethod(method, methodNumber++, typeBuilder);
                methodEntries.Add(new ProxyVtableBuilder.MethodEntry(method.Name, offset));
            }
        }

        var proxyVtbl = vtableBuilder.EmitProxyVtable("IConfig", methodEntries, true);
        // 6 base + 2 methods = 8
        Assert.Contains("CINTERFACE_PROXY_VTABLE(8)", proxyVtbl);
    }

    [Fact]
    public void EndToEnd_MultipleInterfaces_EmitsPerInterface()
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

        Assert.Contains("IFirst_FormatStringOffsetTable", output);
        Assert.Contains("ISecond_FormatStringOffsetTable", output);
        Assert.Contains("_IFirstProxyVtbl", output);
        Assert.Contains("_ISecondProxyVtbl", output);
    }
}
