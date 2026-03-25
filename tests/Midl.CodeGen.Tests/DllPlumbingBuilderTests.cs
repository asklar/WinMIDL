using Midl.Analysis.Symbols;
using Midl.CodeGen.Stub;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for DllPlumbingBuilder — verifies DLL registration plumbing emission
/// including proxy/stub vtable lists, interface names, CLSID, ProxyFileInfo, and DLL exports.
/// Uses partial class to allow splitting tests across files.
/// </summary>
public partial class DllPlumbingBuilderTests
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

    private static MethodSymbol MakeMethod(string name, InterfaceSymbol iface)
    {
        return new MethodSymbol(name, iface)
        {
            ReturnType = new TypeRef("HRESULT"),
        };
    }

    #region EmitProxyVtblList

    [Fact]
    public void EmitProxyVtblList_SingleInterface_ContainsProxyVtblReference()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IWidget" };

        var result = builder.EmitProxyVtblList(interfaces);

        Assert.Contains("_IWidgetProxyVtbl", result);
    }

    [Fact]
    public void EmitProxyVtblList_SingleInterface_HasNullTerminator()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitProxyVtblList(interfaces);

        Assert.Contains("0", result);
    }

    [Fact]
    public void EmitProxyVtblList_MultipleInterfaces_ContainsAllReferences()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFirst", "ISecond", "IThird" };

        var result = builder.EmitProxyVtblList(interfaces);

        Assert.Contains("_IFirstProxyVtbl", result);
        Assert.Contains("_ISecondProxyVtbl", result);
        Assert.Contains("_IThirdProxyVtbl", result);
    }

    [Fact]
    public void EmitProxyVtblList_IsStaticConst()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitProxyVtblList(interfaces);

        Assert.Contains("static const CInterfaceProxyVtbl * const _ProxyVtblList[]", result);
    }

    [Fact]
    public void EmitProxyVtblList_CastsToCorrectType()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitProxyVtblList(interfaces);

        Assert.Contains("(CInterfaceProxyVtbl *)", result);
    }

    #endregion

    #region EmitStubVtblList

    [Fact]
    public void EmitStubVtblList_SingleInterface_ContainsStubVtblReference()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IWidget" };

        var result = builder.EmitStubVtblList(interfaces);

        Assert.Contains("_IWidgetStubVtbl", result);
    }

    [Fact]
    public void EmitStubVtblList_SingleInterface_HasNullTerminator()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitStubVtblList(interfaces);

        Assert.Contains("0", result);
    }

    [Fact]
    public void EmitStubVtblList_MultipleInterfaces_ContainsAllReferences()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IAlpha", "IBeta" };

        var result = builder.EmitStubVtblList(interfaces);

        Assert.Contains("_IAlphaStubVtbl", result);
        Assert.Contains("_IBetaStubVtbl", result);
    }

    [Fact]
    public void EmitStubVtblList_IsStaticConst()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitStubVtblList(interfaces);

        Assert.Contains("static const CInterfaceStubVtbl * const _StubVtblList[]", result);
    }

    [Fact]
    public void EmitStubVtblList_CastsToCorrectType()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitStubVtblList(interfaces);

        Assert.Contains("(CInterfaceStubVtbl *)", result);
    }

    #endregion

    #region EmitInterfaceNamesList

    [Fact]
    public void EmitInterfaceNamesList_SingleInterface_ContainsInterfaceName()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "ISimpleInterface" };

        var result = builder.EmitInterfaceNamesList(interfaces);

        Assert.Contains("\"ISimpleInterface\"", result);
    }

    [Fact]
    public void EmitInterfaceNamesList_SingleInterface_HasNullTerminator()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitInterfaceNamesList(interfaces);

        Assert.Contains("0", result);
    }

    [Fact]
    public void EmitInterfaceNamesList_MultipleInterfaces_ContainsAllNames()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFirst", "ISecond" };

        var result = builder.EmitInterfaceNamesList(interfaces);

        Assert.Contains("\"IFirst\"", result);
        Assert.Contains("\"ISecond\"", result);
    }

    [Fact]
    public void EmitInterfaceNamesList_IsStaticConst()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitInterfaceNamesList(interfaces);

        Assert.Contains("static const PCInterfaceName const _InterfaceNamesList[]", result);
    }

    #endregion

    #region EmitClsidPsFactoryBuffer

    [Fact]
    public void EmitClsidPsFactoryBuffer_ContainsClsidDeclaration()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitClsidPsFactoryBuffer();

        Assert.Contains("CLSID_PSFactoryBuffer", result);
    }

    [Fact]
    public void EmitClsidPsFactoryBuffer_DefinesProxyFileListDefined()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitClsidPsFactoryBuffer();

        Assert.Contains("#define _PROXY_FILE_LIST_DEFINED", result);
    }

    [Fact]
    public void EmitClsidPsFactoryBuffer_HasGuidFormat()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitClsidPsFactoryBuffer();

        // Should contain a GUID in the { 0x..., 0x..., 0x..., { ... } } format
        Assert.Contains("const CLSID CLSID_PSFactoryBuffer", result);
        Assert.Contains("0x", result);
    }

    #endregion

    #region EmitProxyFileInfo

    [Fact]
    public void EmitProxyFileInfo_ContainsProxyVtblListReference()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitProxyFileInfo(interfaces);

        Assert.Contains("_ProxyVtblList", result);
    }

    [Fact]
    public void EmitProxyFileInfo_ContainsStubVtblListReference()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitProxyFileInfo(interfaces);

        Assert.Contains("_StubVtblList", result);
    }

    [Fact]
    public void EmitProxyFileInfo_ContainsInterfaceNamesListReference()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitProxyFileInfo(interfaces);

        Assert.Contains("_InterfaceNamesList", result);
    }

    [Fact]
    public void EmitProxyFileInfo_ContainsTableSizeForSingleInterface()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IWidget" };

        var result = builder.EmitProxyFileInfo(interfaces);

        // TableSize should be 1 for a single interface
        Assert.Contains("1,", result);
    }

    [Fact]
    public void EmitProxyFileInfo_ContainsTableSizeForMultipleInterfaces()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFirst", "ISecond", "IThird" };

        var result = builder.EmitProxyFileInfo(interfaces);

        // TableSize should be 3
        Assert.Contains("3,", result);
    }

    [Fact]
    public void EmitProxyFileInfo_ContainsVersionTwo()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitProxyFileInfo(interfaces);

        // version field should be 2
        Assert.Contains("2,", result);
    }

    [Fact]
    public void EmitProxyFileInfo_IsStaticConst()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitProxyFileInfo(interfaces);

        Assert.Contains("static const ProxyFileInfo", result);
    }

    [Fact]
    public void EmitProxyFileInfo_ContainsGProxyFileList()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitProxyFileInfo(interfaces);

        Assert.Contains("g_ProxyFileList", result);
    }

    #endregion

    #region EmitDllExports

    [Fact]
    public void EmitDllExports_ContainsDllGetClassObject()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitDllExports();

        Assert.Contains("DllGetClassObject", result);
    }

    [Fact]
    public void EmitDllExports_ContainsDllCanUnloadNow()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitDllExports();

        Assert.Contains("DllCanUnloadNow", result);
    }

    [Fact]
    public void EmitDllExports_ContainsDllRegisterServer()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitDllExports();

        Assert.Contains("DllRegisterServer", result);
    }

    [Fact]
    public void EmitDllExports_ContainsDllUnregisterServer()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitDllExports();

        Assert.Contains("DllUnregisterServer", result);
    }

    [Fact]
    public void EmitDllExports_ContainsGetProxyDllInfo()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitDllExports();

        Assert.Contains("GetProxyDllInfo", result);
    }

    [Fact]
    public void EmitDllExports_DllGetClassObject_CallsNdrDllGetClassObject()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitDllExports();

        Assert.Contains("NdrDllGetClassObject", result);
    }

    [Fact]
    public void EmitDllExports_DllCanUnloadNow_CallsNdrDllCanUnloadNow()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitDllExports();

        Assert.Contains("NdrDllCanUnloadNow", result);
    }

    [Fact]
    public void EmitDllExports_DllRegisterServer_CallsNdrDllRegisterProxy()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitDllExports();

        Assert.Contains("NdrDllRegisterProxy", result);
    }

    [Fact]
    public void EmitDllExports_DllUnregisterServer_CallsNdrDllUnregisterProxy()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitDllExports();

        Assert.Contains("NdrDllUnregisterProxy", result);
    }

    [Fact]
    public void EmitDllExports_ReferencesGProxyFileList()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitDllExports();

        Assert.Contains("g_ProxyFileList", result);
    }

    [Fact]
    public void EmitDllExports_ReferencesClsidPsFactoryBuffer()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitDllExports();

        Assert.Contains("CLSID_PSFactoryBuffer", result);
    }

    [Fact]
    public void EmitDllExports_GetProxyDllInfo_SetsOutputParameters()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitDllExports();

        Assert.Contains("*pInfo = g_ProxyFileList", result);
        Assert.Contains("*pClsid = &CLSID_PSFactoryBuffer", result);
    }

    [Fact]
    public void EmitDllExports_UsesStdapicalltype()
    {
        var builder = new DllPlumbingBuilder();

        var result = builder.EmitDllExports();

        Assert.Contains("STDAPICALLTYPE", result);
    }

    #endregion

    #region EmitAll

    [Fact]
    public void EmitAll_ContainsAllSections()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IWidget" };

        var result = builder.EmitAll(interfaces);

        Assert.Contains("_ProxyVtblList", result);
        Assert.Contains("_StubVtblList", result);
        Assert.Contains("_InterfaceNamesList", result);
        Assert.Contains("CLSID_PSFactoryBuffer", result);
        Assert.Contains("__MIDL_ProxyFileInfo", result);
        Assert.Contains("g_ProxyFileList", result);
        Assert.Contains("DllGetClassObject", result);
        Assert.Contains("DllCanUnloadNow", result);
        Assert.Contains("DllRegisterServer", result);
        Assert.Contains("DllUnregisterServer", result);
        Assert.Contains("GetProxyDllInfo", result);
    }

    [Fact]
    public void EmitAll_CorrectOrder_ProxyVtblBeforeStubVtbl()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = new[] { "IFoo" };

        var result = builder.EmitAll(interfaces);

        int proxyIdx = result.IndexOf("_ProxyVtblList");
        int stubIdx = result.IndexOf("_StubVtblList");
        int namesIdx = result.IndexOf("_InterfaceNamesList");
        int clsidIdx = result.IndexOf("CLSID_PSFactoryBuffer");
        int fileInfoIdx = result.IndexOf("__MIDL_ProxyFileInfo");
        int exportsIdx = result.IndexOf("DllGetClassObject");

        Assert.True(proxyIdx < stubIdx, "ProxyVtblList should come before StubVtblList");
        Assert.True(stubIdx < namesIdx, "StubVtblList should come before InterfaceNamesList");
        Assert.True(namesIdx < clsidIdx, "InterfaceNamesList should come before CLSID");
        Assert.True(clsidIdx < fileInfoIdx, "CLSID should come before ProxyFileInfo");
        Assert.True(fileInfoIdx < exportsIdx, "ProxyFileInfo should come before DLL exports");
    }

    [Fact]
    public void EmitAll_EmptyInterfaces_ProducesMinimalOutput()
    {
        var builder = new DllPlumbingBuilder();
        var interfaces = Array.Empty<string>();

        var result = builder.EmitAll(interfaces);

        // Should still have the structure, just with no interface entries
        Assert.Contains("_ProxyVtblList", result);
        Assert.Contains("_StubVtblList", result);
        Assert.Contains("0", result); // null terminators
    }

    #endregion
}
