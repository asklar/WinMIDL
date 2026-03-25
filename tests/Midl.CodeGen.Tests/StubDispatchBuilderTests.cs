using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Stub;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for StubDispatchBuilder — MIDL_STUB_DESC, MIDL_SERVER_INFO,
/// FormatStringOffsetTable, and CInterfaceStubVtbl emission.
/// Uses partial class to allow splitting tests across files.
/// </summary>
public partial class StubDispatchBuilderTests
{
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

    private static ParameterSymbol InParam(string name, string typeName, MethodSymbol? parent = null)
    {
        return new ParameterSymbol(name, parent)
        {
            Type = new TypeRef(typeName),
            Direction = ParameterDirection.In,
        };
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

    #region EmitStubDesc

    [Fact]
    public void EmitStubDesc_ContainsNdrOleAllocate()
    {
        var builder = new StubDispatchBuilder("test");
        string output = builder.EmitStubDesc();

        Assert.Contains("NdrOleAllocate", output);
    }

    [Fact]
    public void EmitStubDesc_ContainsNdrOleFree()
    {
        var builder = new StubDispatchBuilder("test");
        string output = builder.EmitStubDesc();

        Assert.Contains("NdrOleFree", output);
    }

    [Fact]
    public void EmitStubDesc_ContainsTypeFormatStringReference()
    {
        var builder = new StubDispatchBuilder("test");
        string output = builder.EmitStubDesc();

        Assert.Contains("test__MIDL_TypeFormatString.Format", output);
    }

    [Fact]
    public void EmitStubDesc_ContainsBoundsCheckFlag()
    {
        var builder = new StubDispatchBuilder("test");
        string output = builder.EmitStubDesc();

        Assert.Contains("1,", output); // -error bounds_check flag
    }

    [Fact]
    public void EmitStubDesc_ContainsNdrLibraryVersion()
    {
        var builder = new StubDispatchBuilder("test");
        string output = builder.EmitStubDesc();

        Assert.Contains("0x50002", output);
    }

    [Fact]
    public void EmitStubDesc_ContainsMidlVersion()
    {
        var builder = new StubDispatchBuilder("test");
        string output = builder.EmitStubDesc();

        Assert.Contains("0x8010274", output);
    }

    [Fact]
    public void EmitStubDesc_WrappedInCppNamespace()
    {
        var builder = new StubDispatchBuilder("test");
        string output = builder.EmitStubDesc();

        Assert.Contains("#ifdef __cplusplus", output);
        Assert.Contains("namespace {", output);
    }

    [Fact]
    public void EmitStubDesc_IsStaticConst()
    {
        var builder = new StubDispatchBuilder("test");
        string output = builder.EmitStubDesc();

        Assert.Contains("static const MIDL_STUB_DESC Object_StubDesc", output);
    }

    [Fact]
    public void EmitStubDesc_DifferentFilePrefix_UsesCorrectPrefix()
    {
        var builder = new StubDispatchBuilder("my_proxy");
        string output = builder.EmitStubDesc();

        Assert.Contains("my_proxy__MIDL_TypeFormatString.Format", output);
    }

    #endregion

    #region EmitFormatStringOffsetTable

    [Fact]
    public void EmitFormatStringOffsetTable_SingleMethod_ContainsOffset()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");
        var method = MakeMethod("DoWork", iface);

        var typeBuilder = new NdrTypeFormatBuilder();
        var procBuilder = new NdrProcFormatBuilder();
        int offset = procBuilder.AddMethod(method, 6, typeBuilder);

        string output = builder.EmitFormatStringOffsetTable(iface, new[] { offset });

        Assert.Contains("IFoo_FormatStringOffsetTable", output);
        Assert.Contains("unsigned short", output);
        Assert.Contains($"{offset}", output);
    }

    [Fact]
    public void EmitFormatStringOffsetTable_MultipleMethods_ContainsAllOffsets()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("ICalc");
        var m1 = MakeMethod("Add", iface, InParam("a", "Int32"), InParam("b", "Int32"), OutParam("result", "Int32"));
        var m2 = MakeMethod("Sub", iface, InParam("a", "Int32"), InParam("b", "Int32"), OutParam("result", "Int32"));

        var typeBuilder = new NdrTypeFormatBuilder();
        var procBuilder = new NdrProcFormatBuilder();
        int o1 = procBuilder.AddMethod(m1, 6, typeBuilder);
        int o2 = procBuilder.AddMethod(m2, 7, typeBuilder);

        string output = builder.EmitFormatStringOffsetTable(iface, new[] { o1, o2 });

        Assert.Contains($"{o1}", output);
        Assert.Contains($"{o2}", output);
    }

    [Fact]
    public void EmitFormatStringOffsetTable_IsStaticConstArray()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");

        string output = builder.EmitFormatStringOffsetTable(iface, new[] { 0 });

        Assert.Contains("static const unsigned short", output);
    }

    #endregion

    #region EmitServerInfo

    [Fact]
    public void EmitServerInfo_ContainsStubDescReference()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");

        string output = builder.EmitServerInfo(iface);

        Assert.Contains("&Object_StubDesc", output);
    }

    [Fact]
    public void EmitServerInfo_ContainsProcFormatStringReference()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");

        string output = builder.EmitServerInfo(iface);

        Assert.Contains("test__MIDL_ProcFormatString.Format", output);
    }

    [Fact]
    public void EmitServerInfo_ContainsFormatStringOffsetTableWithOffset()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");

        string output = builder.EmitServerInfo(iface);

        // IInspectable-derived: offset by [-3]
        Assert.Contains("&IFoo_FormatStringOffsetTable[-3]", output);
    }

    [Fact]
    public void EmitServerInfo_IsStaticConst()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");

        string output = builder.EmitServerInfo(iface);

        Assert.Contains("static const MIDL_SERVER_INFO IFoo_ServerInfo", output);
    }

    [Fact]
    public void EmitServerInfo_IUnknownDerived_OffsetIsMinusThree()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo", isInspectable: false);

        string output = builder.EmitServerInfo(iface);

        // IUnknown-derived also uses [-3]
        Assert.Contains("&IFoo_FormatStringOffsetTable[-3]", output);
    }

    #endregion

    #region EmitStubVtbl

    [Fact]
    public void EmitStubVtbl_IUnknownDerived_UsesCStdStubBufferMethods()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo", isInspectable: false);
        iface.AddMember(MakeMethod("DoWork", iface));

        string output = builder.EmitStubVtbl(iface);

        Assert.Contains("CStdStubBuffer_METHODS", output);
        Assert.DoesNotContain("CStdStubBuffer_DELEGATING_METHODS", output);
    }

    [Fact]
    public void EmitStubVtbl_IInspectableDerived_UsesDelegatingMethods()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");
        iface.AddMember(MakeMethod("DoWork", iface));

        string output = builder.EmitStubVtbl(iface);

        Assert.Contains("CStdStubBuffer_DELEGATING_METHODS", output);
    }

    [Fact]
    public void EmitStubVtbl_ContainsIIDReference()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");
        iface.AddMember(MakeMethod("DoWork", iface));

        string output = builder.EmitStubVtbl(iface);

        Assert.Contains("&IID_IFoo", output);
    }

    [Fact]
    public void EmitStubVtbl_ContainsServerInfoReference()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");
        iface.AddMember(MakeMethod("DoWork", iface));

        string output = builder.EmitStubVtbl(iface);

        Assert.Contains("&IFoo_ServerInfo", output);
    }

    [Fact]
    public void EmitStubVtbl_IUnknownDerived_OneMethod_VtableSlotCountIs4()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo", isInspectable: false);
        iface.AddMember(MakeMethod("DoWork", iface));

        string output = builder.EmitStubVtbl(iface);

        // IUnknown(3) + 1 method = 4
        Assert.Contains("4,", output);
    }

    [Fact]
    public void EmitStubVtbl_IInspectableDerived_OneMethod_VtableSlotCountIs7()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");
        iface.AddMember(MakeMethod("DoWork", iface));

        string output = builder.EmitStubVtbl(iface);

        // IInspectable(6) + 1 method = 7
        Assert.Contains("7,", output);
    }

    [Fact]
    public void EmitStubVtbl_IUnknownDerived_PureInterpreted_ZeroTablePointer()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo", isInspectable: false);
        iface.AddMember(MakeMethod("DoWork", iface));

        string output = builder.EmitStubVtbl(iface);

        // Non-delegating: 0 for table pointer (pure interpreted)
        Assert.Contains("0,", output);
    }

    [Fact]
    public void EmitStubVtbl_IInspectableDerived_HasTableReference()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");
        iface.AddMember(MakeMethod("DoWork", iface));

        string output = builder.EmitStubVtbl(iface);

        // Delegating: &IFoo_table[-3]
        Assert.Contains("&IFoo_table[-3]", output);
    }

    [Fact]
    public void EmitStubVtbl_StructName_HasUnderscoredPrefix()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IWidget");
        iface.AddMember(MakeMethod("DoWork", iface));

        string output = builder.EmitStubVtbl(iface);

        Assert.Contains("_IWidgetStubVtbl", output);
    }

    #endregion

    #region EmitStubFunctionTable

    [Fact]
    public void EmitStubFunctionTable_IInspectableDerived_HasForwardingFunctions()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");
        iface.AddMember(MakeMethod("DoWork", iface));

        string? output = builder.EmitStubFunctionTable(iface);
        Assert.NotNull(output);

        // 3 forwarding functions for IInspectable methods (GetIids, GetRuntimeClassName, GetTrustLevel)
        var lines = output!.Split('\n');
        int forwardCount = lines.Count(l => l.Contains("STUB_FORWARDING_FUNCTION"));
        Assert.Equal(3, forwardCount);
    }

    [Fact]
    public void EmitStubFunctionTable_IInspectableDerived_HasNdrStubCall2()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");
        iface.AddMember(MakeMethod("DoWork", iface));

        string? output = builder.EmitStubFunctionTable(iface);
        Assert.NotNull(output);

        Assert.Contains("NdrStubCall2", output!);
    }

    [Fact]
    public void EmitStubFunctionTable_IInspectableDerived_TwoMethods_HasTwoNdrStubCalls()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");
        iface.AddMember(MakeMethod("Method1", iface));
        iface.AddMember(MakeMethod("Method2", iface));

        string? output = builder.EmitStubFunctionTable(iface);
        Assert.NotNull(output);

        var lines = output!.Split('\n');
        int ndrCount = lines.Count(l => l.Contains("NdrStubCall2"));
        Assert.Equal(2, ndrCount);
    }

    [Fact]
    public void EmitStubFunctionTable_IUnknownDerived_ReturnsNull()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo", isInspectable: false);
        iface.AddMember(MakeMethod("DoWork", iface));

        // IUnknown-derived interfaces don't need a stub function table
        string? output = builder.EmitStubFunctionTable(iface);

        Assert.Null(output);
    }

    [Fact]
    public void EmitStubFunctionTable_ContainsTableName()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IWidget");
        iface.AddMember(MakeMethod("DoWork", iface));

        string? output = builder.EmitStubFunctionTable(iface);

        Assert.Contains("IWidget_table", output);
    }

    [Fact]
    public void EmitStubFunctionTable_IsExternCSelectany()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");
        iface.AddMember(MakeMethod("DoWork", iface));

        string? output = builder.EmitStubFunctionTable(iface);

        Assert.Contains("EXTERN_C", output);
        Assert.Contains("DECLSPEC_SELECTANY", output);
    }

    [Fact]
    public void EmitStubFunctionTable_PropertyMethods_CountsCorrectly()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");
        var prop = new PropertySymbol("Name", iface)
        {
            Type = new TypeRef("String"),
            HasGetter = true,
            HasSetter = true,
        };
        iface.AddMember(prop);

        string? output = builder.EmitStubFunctionTable(iface);

        // Property with getter+setter = 2 NdrStubCall2 entries
        var lines = output!.Split('\n');
        int ndrCount = lines.Count(l => l.Contains("NdrStubCall2"));
        Assert.Equal(2, ndrCount);
    }

    [Fact]
    public void EmitStubFunctionTable_EventMembers_CountsCorrectly()
    {
        var builder = new StubDispatchBuilder("test");
        var iface = MakeInterface("IFoo");
        var evt = new EventSymbol("Changed", iface)
        {
            DelegateType = new TypeRef("EventHandler"),
        };
        iface.AddMember(evt);

        string? output = builder.EmitStubFunctionTable(iface);

        // Event = add_ + remove_ = 2 NdrStubCall2 entries
        var lines = output!.Split('\n');
        int ndrCount = lines.Count(l => l.Contains("NdrStubCall2"));
        Assert.Equal(2, ndrCount);
    }

    #endregion

    #region VtableSlotCount

    [Fact]
    public void GetVtableSlotCount_IInspectableWithThreeMethods_Returns9()
    {
        var iface = MakeInterface("IFoo");
        iface.AddMember(MakeMethod("A", iface));
        iface.AddMember(MakeMethod("B", iface));
        iface.AddMember(MakeMethod("C", iface));

        int count = StubDispatchBuilder.GetVtableSlotCount(iface);

        // 6 (IInspectable) + 3 = 9
        Assert.Equal(9, count);
    }

    [Fact]
    public void GetVtableSlotCount_IUnknownWithTwoMethods_Returns5()
    {
        var iface = MakeInterface("IFoo", isInspectable: false);
        iface.AddMember(MakeMethod("A", iface));
        iface.AddMember(MakeMethod("B", iface));

        int count = StubDispatchBuilder.GetVtableSlotCount(iface);

        // 3 (IUnknown) + 2 = 5
        Assert.Equal(5, count);
    }

    [Fact]
    public void GetVtableSlotCount_PropertyCountsGetterAndSetter()
    {
        var iface = MakeInterface("IFoo");
        var prop = new PropertySymbol("Name", iface)
        {
            Type = new TypeRef("String"),
            HasGetter = true,
            HasSetter = true,
        };
        iface.AddMember(prop);

        int count = StubDispatchBuilder.GetVtableSlotCount(iface);

        // 6 (IInspectable) + 2 (get+put) = 8
        Assert.Equal(8, count);
    }

    [Fact]
    public void GetVtableSlotCount_ReadOnlyProperty_CountsOnlyGetter()
    {
        var iface = MakeInterface("IFoo");
        var prop = new PropertySymbol("Value", iface)
        {
            Type = new TypeRef("Int32"),
            HasGetter = true,
            HasSetter = false,
        };
        iface.AddMember(prop);

        int count = StubDispatchBuilder.GetVtableSlotCount(iface);

        // 6 (IInspectable) + 1 (get only) = 7
        Assert.Equal(7, count);
    }

    [Fact]
    public void GetVtableSlotCount_EventCountsAddAndRemove()
    {
        var iface = MakeInterface("IFoo");
        var evt = new EventSymbol("Changed", iface)
        {
            DelegateType = new TypeRef("EventHandler"),
        };
        iface.AddMember(evt);

        int count = StubDispatchBuilder.GetVtableSlotCount(iface);

        // 6 (IInspectable) + 2 (add+remove) = 8
        Assert.Equal(8, count);
    }

    #endregion
}
