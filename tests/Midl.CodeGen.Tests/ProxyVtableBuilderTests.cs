using Midl.Analysis.Symbols;
using Midl.CodeGen.Stub;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for ProxyVtableBuilder — verifies proxy vtable, stub table,
/// and format string offset table emission for IUnknown and IInspectable-derived interfaces.
/// </summary>
public partial class ProxyVtableBuilderTests
{
    private static InterfaceSymbol MakeInterface(string name, bool isInspectable = true)
    {
        var iface = new InterfaceSymbol(name, null)
        {
            Uuid = new Guid("12345678-1234-1234-1234-123456789012")
        };
        if (isInspectable)
            iface.RequiredInterfaces.Add(new TypeRef("IInspectable"));
        return iface;
    }

    private static ProxyVtableBuilder.MethodEntry Entry(string name, int offset) =>
        new(name, offset);

    #region EmitFormatStringOffsetTable

    [Fact]
    public void FormatStringOffsetTable_IUnknownDerived_EmitsOnlyCustomOffsets()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[]
        {
            Entry("Add", 0),
            Entry("GetName", 50),
            Entry("SetValue", 88),
            Entry("Echo", 126),
        };

        var result = builder.EmitFormatStringOffsetTable("ISimpleCalc", methods, isInspectable: false);

        Assert.Contains("ISimpleCalc_FormatStringOffsetTable[]", result);
        // Should contain the 4 custom method offsets
        Assert.Contains("0", result);
        Assert.Contains("50", result);
        Assert.Contains("88", result);
        Assert.Contains("126", result);
        // Should NOT contain (unsigned short) -1 for IUnknown-derived
        Assert.DoesNotContain("(unsigned short) -1", result);
    }

    [Fact]
    public void FormatStringOffsetTable_IInspectableDerived_IncludesForwardedSlots()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[]
        {
            Entry("GetWidget", 0),
            Entry("SetName", 38),
        };

        var result = builder.EmitFormatStringOffsetTable("IWidgetProvider", methods, isInspectable: true);

        Assert.Contains("IWidgetProvider_FormatStringOffsetTable[]", result);
        // Should have 3 forwarded entries for IInspectable methods
        Assert.Contains("(unsigned short) -1", result);
        // Should contain custom method offsets
        Assert.Contains("0", result);
        Assert.Contains("38", result);
    }

    [Fact]
    public void FormatStringOffsetTable_IsStaticConst()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[] { Entry("Foo", 0) };

        var result = builder.EmitFormatStringOffsetTable("IFoo", methods, isInspectable: false);

        Assert.Contains("static const unsigned short", result);
    }

    #endregion

    #region EmitProxyVtable

    [Fact]
    public void ProxyVtable_IUnknownDerived_HasCorrectSlotCount()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[]
        {
            Entry("Add", 0),
            Entry("GetName", 50),
            Entry("SetValue", 88),
            Entry("Echo", 126),
        };

        // 3 IUnknown + 4 custom = 7
        var result = builder.EmitProxyVtable("ISimpleCalc", methods, isInspectable: false);

        Assert.Contains("CINTERFACE_PROXY_VTABLE(7)", result);
    }

    [Fact]
    public void ProxyVtable_IUnknownDerived_HasIUnknownMethods()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[] { Entry("Foo", 0) };

        var result = builder.EmitProxyVtable("IFoo", methods, isInspectable: false);

        Assert.Contains("IUnknown_QueryInterface_Proxy", result);
        Assert.Contains("IUnknown_AddRef_Proxy", result);
        Assert.Contains("IUnknown_Release_Proxy", result);
    }

    [Fact]
    public void ProxyVtable_IUnknownDerived_UsesStublessMarkerForUserMethods()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[]
        {
            Entry("Add", 0),
            Entry("GetName", 50),
        };

        var result = builder.EmitProxyVtable("ITest", methods, isInspectable: false);

        Assert.Contains("(void *) (INT_PTR) -1 /* ITest::Add */", result);
        Assert.Contains("(void *) (INT_PTR) -1 /* ITest::GetName */", result);
    }

    [Fact]
    public void ProxyVtable_IInspectableDerived_HasForwardedSlots()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[] { Entry("GetWidget", 0) };

        // 3 IUnknown + 3 IInspectable + 1 custom = 7
        var result = builder.EmitProxyVtable("IWidget", methods, isInspectable: true);

        Assert.Contains("CINTERFACE_PROXY_VTABLE(7)", result);
        // IInspectable forwarded slots use 0, not -1
        Assert.Contains("0 /* IInspectable::GetIids */", result);
        Assert.Contains("0 /* IInspectable::GetRuntimeClassName */", result);
        Assert.Contains("0 /* IInspectable::GetTrustLevel */", result);
        // Custom methods use stubless marker
        Assert.Contains("(void *) (INT_PTR) -1 /* IWidget::GetWidget */", result);
    }

    [Fact]
    public void ProxyVtable_IInspectableDerived_HasCorrectSlotCount()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[]
        {
            Entry("M1", 0),
            Entry("M2", 50),
            Entry("M3", 100),
        };

        // 3 IUnknown + 3 IInspectable + 3 custom = 9
        var result = builder.EmitProxyVtable("IMyIface", methods, isInspectable: true);

        Assert.Contains("CINTERFACE_PROXY_VTABLE(9)", result);
    }

    [Fact]
    public void ProxyVtable_ContainsProxyInfoAndIID()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[] { Entry("Foo", 0) };

        var result = builder.EmitProxyVtable("IFoo", methods, isInspectable: false);

        Assert.Contains("&IFoo_ProxyInfo", result);
        Assert.Contains("&IID_IFoo", result);
    }

    [Fact]
    public void ProxyVtable_NameUsesUnderscorePrefix()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[] { Entry("Foo", 0) };

        var result = builder.EmitProxyVtable("IFoo", methods, isInspectable: false);

        Assert.Contains("_IFooProxyVtbl", result);
    }

    #endregion

    #region EmitStubTable

    [Fact]
    public void StubTable_IInspectableDerived_HasForwardingAndNdrEntries()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[]
        {
            Entry("GetWidget", 0),
            Entry("SetName", 38),
        };

        var result = builder.EmitStubTable("IWidget", methods, isInspectable: true);

        Assert.NotNull(result);
        Assert.Contains("IWidget_table[]", result);
        // 3 forwarding entries for IInspectable methods
        var forwardingCount = CountOccurrences(result!, "STUB_FORWARDING_FUNCTION");
        Assert.Equal(3, forwardingCount);
        // 2 NdrStubCall2 entries for user methods
        var ndrCount = CountOccurrences(result!, "NdrStubCall2");
        Assert.Equal(2, ndrCount);
    }

    [Fact]
    public void StubTable_IUnknownDerived_ReturnsNull()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[] { Entry("Foo", 0) };

        var result = builder.EmitStubTable("IFoo", methods, isInspectable: false);

        // IUnknown-derived interfaces don't need a stub table
        Assert.Null(result);
    }

    [Fact]
    public void StubTable_IsPrpcStubFunction()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[] { Entry("M1", 0) };

        var result = builder.EmitStubTable("IFoo", methods, isInspectable: true);

        Assert.NotNull(result);
        Assert.Contains("PRPC_STUB_FUNCTION", result);
    }

    #endregion

    #region EmitProxyInfo

    [Fact]
    public void ProxyInfo_IUnknownDerived_UsesMinusThreeOffset()
    {
        var builder = new ProxyVtableBuilder();

        var result = builder.EmitProxyInfo("ISimpleCalc", isInspectable: false, "simple_test");

        Assert.Contains("ISimpleCalc_ProxyInfo", result);
        Assert.Contains("&Object_StubDesc", result);
        Assert.Contains("simple_test__MIDL_ProcFormatString.Format", result);
        Assert.Contains("&ISimpleCalc_FormatStringOffsetTable[-3]", result);
    }

    [Fact]
    public void ProxyInfo_IInspectableDerived_UsesMinusThreeOffset()
    {
        var builder = new ProxyVtableBuilder();

        var result = builder.EmitProxyInfo("IWidget", isInspectable: true, "my_file");

        // IInspectable also uses [-3] — the table already includes -1 entries for slots 3-5
        Assert.Contains("&IWidget_FormatStringOffsetTable[-3]", result);
    }

    [Fact]
    public void ProxyInfo_IsStublessProxyInfo()
    {
        var builder = new ProxyVtableBuilder();

        var result = builder.EmitProxyInfo("IFoo", isInspectable: false, "test");

        Assert.Contains("MIDL_STUBLESS_PROXY_INFO", result);
    }

    #endregion

    #region EmitServerInfo

    [Fact]
    public void ServerInfo_ContainsExpectedFields()
    {
        var builder = new ProxyVtableBuilder();

        var result = builder.EmitServerInfo("ISimpleCalc", isInspectable: false, "simple_test");

        Assert.Contains("ISimpleCalc_ServerInfo", result);
        Assert.Contains("MIDL_SERVER_INFO", result);
        Assert.Contains("&Object_StubDesc", result);
        Assert.Contains("simple_test__MIDL_ProcFormatString.Format", result);
        Assert.Contains("&ISimpleCalc_FormatStringOffsetTable[-3]", result);
    }

    #endregion

    #region EmitStubVtable

    [Fact]
    public void StubVtable_IUnknownDerived_UsesStdMethods()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[]
        {
            Entry("Add", 0),
            Entry("GetName", 50),
            Entry("SetValue", 88),
            Entry("Echo", 126),
        };

        var result = builder.EmitStubVtable("ISimpleCalc", methods, isInspectable: false);

        Assert.Contains("CInterfaceStubVtbl", result);
        Assert.Contains("_ISimpleCalcStubVtbl", result);
        Assert.Contains("&IID_ISimpleCalc", result);
        Assert.Contains("&ISimpleCalc_ServerInfo", result);
        Assert.Contains("7", result); // 3 + 4
        Assert.Contains("CStdStubBuffer_METHODS", result);
        // Non-delegating uses 0 for the table pointer
        Assert.DoesNotContain("CStdStubBuffer_DELEGATING_METHODS", result);
    }

    [Fact]
    public void StubVtable_IInspectableDerived_UsesDelegatingMethods()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[]
        {
            Entry("GetWidget", 0),
            Entry("SetName", 38),
        };

        var result = builder.EmitStubVtable("IWidget", methods, isInspectable: true);

        Assert.Contains("CInterfaceStubVtbl", result);
        Assert.Contains("_IWidgetStubVtbl", result);
        // 3 IUnknown + 3 IInspectable + 2 custom = 8
        Assert.Contains("8", result);
        Assert.Contains("CStdStubBuffer_DELEGATING_METHODS", result);
        Assert.Contains("&IWidget_table[-3]", result);
    }

    #endregion

    #region Edge cases

    [Fact]
    public void ProxyVtable_NoUserMethods_IUnknown_HasOnlyBaseSlots()
    {
        var builder = new ProxyVtableBuilder();
        var methods = Array.Empty<ProxyVtableBuilder.MethodEntry>();

        var result = builder.EmitProxyVtable("IEmpty", methods, isInspectable: false);

        Assert.Contains("CINTERFACE_PROXY_VTABLE(3)", result);
        Assert.DoesNotContain("(void *) (INT_PTR) -1", result);
    }

    [Fact]
    public void ProxyVtable_NoUserMethods_IInspectable_HasOnlyBaseSlots()
    {
        var builder = new ProxyVtableBuilder();
        var methods = Array.Empty<ProxyVtableBuilder.MethodEntry>();

        var result = builder.EmitProxyVtable("IEmpty", methods, isInspectable: true);

        Assert.Contains("CINTERFACE_PROXY_VTABLE(6)", result);
        Assert.Contains("0 /* IInspectable::GetIids */", result);
        Assert.DoesNotContain("(void *) (INT_PTR) -1", result);
    }

    [Fact]
    public void FormatStringOffsetTable_SingleMethod_IUnknown()
    {
        var builder = new ProxyVtableBuilder();
        var methods = new[] { Entry("DoWork", 42) };

        var result = builder.EmitFormatStringOffsetTable("IWorker", methods, isInspectable: false);

        Assert.Contains("42", result);
        Assert.DoesNotContain("(unsigned short) -1", result);
    }

    #endregion

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}
