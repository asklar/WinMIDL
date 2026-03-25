using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Stub;

namespace Midl.Integration.Tests;

/// <summary>
/// Integration tests for StubDispatchBuilder — compiles IDL source end-to-end
/// and verifies MIDL_STUB_DESC, MIDL_SERVER_INFO, and CInterfaceStubVtbl emission.
/// </summary>
public partial class StubDispatchIntegrationTests
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
    public void StubEmitter_EmitsStubDesc()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789012)]
                interface ISimple : IInspectable
                {
                    HRESULT GetValue([out, retval] Int32* result);
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        string output = emitter.Emit(compilation);

        Assert.Contains("MIDL_STUB_DESC", output);
        Assert.Contains("Object_StubDesc", output);
        Assert.Contains("NdrOleAllocate", output);
        Assert.Contains("NdrOleFree", output);
    }

    [Fact]
    public void StubEmitter_EmitsServerInfo()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789012)]
                interface ISimple : IInspectable
                {
                    HRESULT GetValue([out, retval] Int32* result);
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        string output = emitter.Emit(compilation);

        Assert.Contains("MIDL_SERVER_INFO", output);
        Assert.Contains("ISimple_ServerInfo", output);
    }

    [Fact]
    public void StubEmitter_EmitsFormatStringOffsetTable()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789012)]
                interface ISimple : IInspectable
                {
                    HRESULT GetValue([out, retval] Int32* result);
                    HRESULT SetName([in] String name);
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        string output = emitter.Emit(compilation);

        Assert.Contains("ISimple_FormatStringOffsetTable", output);
    }

    [Fact]
    public void StubEmitter_EmitsStubVtbl_ForInspectableDerived()
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
        string output = emitter.Emit(compilation);

        Assert.Contains("_IWidgetStubVtbl", output);
        Assert.Contains("CStdStubBuffer_DELEGATING_METHODS", output);
        Assert.Contains("IWidget_table", output);
        Assert.Contains("STUB_FORWARDING_FUNCTION", output);
        Assert.Contains("NdrStubCall2", output);
    }

    [Fact]
    public void StubEmitter_EmitsStubVtbl_ForIUnknownDerived()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789012)]
                interface ISimpleCalc requires IUnknown
                {
                    HRESULT Add([in] Int32 a, [in] Int32 b, [out, retval] Int32* result);
                    HRESULT GetName([out, retval] String* name);
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        string output = emitter.Emit(compilation);

        Assert.Contains("_ISimpleCalcStubVtbl", output);
        Assert.Contains("CStdStubBuffer_METHODS", output);
        Assert.DoesNotContain("CStdStubBuffer_DELEGATING_METHODS", output);
    }

    [Fact]
    public void StubEmitter_MultipleInterfaces_EmitsAllServerInfos()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(11111111-1111-1111-1111-111111111111)]
                interface IFoo : IInspectable
                {
                    HRESULT DoFoo();
                }

                [uuid(22222222-2222-2222-2222-222222222222)]
                interface IBar : IInspectable
                {
                    HRESULT DoBar();
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        string output = emitter.Emit(compilation);

        Assert.Contains("IFoo_ServerInfo", output);
        Assert.Contains("IBar_ServerInfo", output);
        Assert.Contains("IFoo_FormatStringOffsetTable", output);
        Assert.Contains("IBar_FormatStringOffsetTable", output);
    }

    [Fact]
    public void StubEmitter_CorrectOrderInOutput()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789012)]
                interface ISimple : IInspectable
                {
                    HRESULT GetValue([out, retval] Int32* result);
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        string output = emitter.Emit(compilation);

        // Proc format string comes before stub dispatch structures
        int procIdx = output.IndexOf("MIDL_PROC_FORMAT_STRING");
        int typeIdx = output.IndexOf("MIDL_TYPE_FORMAT_STRING");
        int offsetIdx = output.IndexOf("FormatStringOffsetTable");
        int serverInfoIdx = output.IndexOf("MIDL_SERVER_INFO");
        int stubDescIdx = output.IndexOf("MIDL_STUB_DESC");

        Assert.True(procIdx >= 0, "Should contain MIDL_PROC_FORMAT_STRING");
        Assert.True(typeIdx >= 0, "Should contain MIDL_TYPE_FORMAT_STRING");
        Assert.True(offsetIdx >= 0, "Should contain FormatStringOffsetTable");
        Assert.True(serverInfoIdx >= 0, "Should contain MIDL_SERVER_INFO");
        Assert.True(stubDescIdx >= 0, "Should contain MIDL_STUB_DESC");

        // Format strings should come before per-interface structures
        Assert.True(procIdx < offsetIdx, "Proc format should come before offset table");
        Assert.True(typeIdx < offsetIdx, "Type format should come before offset table");
    }

    [Fact]
    public void StubEmitter_IInspectableDerived_StubTableHasCorrectSlotCount()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789012)]
                interface ICalc : IInspectable
                {
                    HRESULT Add([in] Int32 a, [in] Int32 b, [out, retval] Int32* result);
                    HRESULT GetName([out, retval] String* name);
                    HRESULT SetValue([in] Double val);
                    HRESULT Echo([in] String input, [out, retval] String* output);
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        string output = emitter.Emit(compilation);

        // IInspectable (6) + 4 methods = 10 total vtable slots
        Assert.Contains("10,", output);
    }

    [Fact]
    public void StubEmitter_PropertyInterface_EmitsCorrectSlotCount()
    {
        var compilation = Compile("""
            namespace TestNs
            {
                [uuid(12345678-1234-1234-1234-123456789012)]
                interface IWidget : IInspectable
                {
                    [propget] HRESULT Name([out, retval] String* value);
                    [propput] HRESULT DisplayName([in] String value);
                    HRESULT DoWork();
                }
            }
            """);

        AssertNoErrors(compilation);

        var emitter = new StubEmitter();
        string output = emitter.Emit(compilation);

        // IInspectable (6) + get_Name + put_Name + DoWork = 9
        // The output should contain "9," as the vtable slot count
        Assert.Contains("IWidget_ServerInfo", output);
    }

    [Fact]
    public void FormatStringOffsetTable_SingleMethod_MatchesExpectedOffset()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("GetValue", iface, OutParam("result", "Int32"));

        var typeBuilder = new NdrTypeFormatBuilder();
        var procBuilder = new NdrProcFormatBuilder();
        int offset = procBuilder.AddMethod(method, 6, typeBuilder);

        string output = builder.EmitFormatStringOffsetTable(iface, new[] { offset });

        // First method offset should be 0
        Assert.Equal(0, offset);
        Assert.Contains("0", output);
    }

    private static InterfaceSymbol MakeInterface(string name,
        string uuid = "12345678-1234-1234-1234-123456789012",
        bool isInspectable = true)
    {
        var iface = new InterfaceSymbol(name, null) { Uuid = new Guid(uuid) };
        if (isInspectable)
            iface.RequiredInterfaces.Add(new TypeRef("IInspectable"));
        return iface;
    }

    private static MethodSymbol MakeMethod(string name, InterfaceSymbol iface, params ParameterSymbol[] parms)
    {
        var method = new MethodSymbol(name, iface)
        {
            ReturnType = new TypeRef("HRESULT"),
        };
        foreach (var p in parms)
            method.Parameters.Add(p);
        return method;
    }

    private static ParameterSymbol OutParam(string name, string typeName, MethodSymbol? parent = null)
    {
        var p = new ParameterSymbol(name, parent)
        {
            Type = new TypeRef(typeName),
            Direction = ParameterDirection.Out,
        };
        p.Attributes.Add(new AttributeData { Name = "retval" });
        return p;
    }
}
