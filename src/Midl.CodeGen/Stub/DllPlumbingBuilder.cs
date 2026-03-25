using System.Text;

namespace Midl.CodeGen.Stub;

/// <summary>
/// Emits DLL registration plumbing for proxy/stub DLL files:
/// - ProxyVtblList / StubVtblList / InterfaceNamesList arrays
/// - CLSID_PSFactoryBuffer constant
/// - ProxyFileInfo struct
/// - DLL export functions (DllGetClassObject, DllCanUnloadNow, DllRegisterServer,
///   DllUnregisterServer, GetProxyDllInfo)
/// </summary>
public class DllPlumbingBuilder
{
    /// <summary>
    /// Emits all DLL plumbing in the correct order.
    /// </summary>
    public string EmitAll(IReadOnlyList<string> interfaceNames)
    {
        var sb = new StringBuilder();

        sb.AppendLine(EmitProxyVtblList(interfaceNames));
        sb.AppendLine(EmitStubVtblList(interfaceNames));
        sb.AppendLine(EmitInterfaceNamesList(interfaceNames));
        sb.AppendLine(EmitClsidPsFactoryBuffer());
        sb.AppendLine(EmitProxyFileInfo(interfaceNames));
        sb.AppendLine(EmitDllExports());

        return sb.ToString();
    }

    /// <summary>
    /// Emits the _ProxyVtblList array referencing each interface's proxy vtable.
    /// </summary>
    public string EmitProxyVtblList(IReadOnlyList<string> interfaceNames)
    {
        var sb = new StringBuilder();

        sb.AppendLine("static const CInterfaceProxyVtbl * const _ProxyVtblList[] =");
        sb.AppendLine("{");

        foreach (var name in interfaceNames)
        {
            sb.AppendLine($"    (CInterfaceProxyVtbl *) &_{name}ProxyVtbl,");
        }

        sb.AppendLine("    0");
        sb.AppendLine("};");

        return sb.ToString();
    }

    /// <summary>
    /// Emits the _StubVtblList array referencing each interface's stub vtable.
    /// </summary>
    public string EmitStubVtblList(IReadOnlyList<string> interfaceNames)
    {
        var sb = new StringBuilder();

        sb.AppendLine("static const CInterfaceStubVtbl * const _StubVtblList[] =");
        sb.AppendLine("{");

        foreach (var name in interfaceNames)
        {
            sb.AppendLine($"    (CInterfaceStubVtbl *) &_{name}StubVtbl,");
        }

        sb.AppendLine("    0");
        sb.AppendLine("};");

        return sb.ToString();
    }

    /// <summary>
    /// Emits the _InterfaceNamesList array with string names for each interface.
    /// </summary>
    public string EmitInterfaceNamesList(IReadOnlyList<string> interfaceNames)
    {
        var sb = new StringBuilder();

        sb.AppendLine("PCInterfaceName const _InterfaceNamesList[] =");
        sb.AppendLine("{");

        foreach (var name in interfaceNames)
        {
            sb.AppendLine($"    \"{name}\",");
        }

        sb.AppendLine("    0");
        sb.AppendLine("};");

        return sb.ToString();
    }

    /// <summary>
    /// Emits the CLSID_PSFactoryBuffer constant and _PROXY_FILE_LIST_DEFINED define.
    /// Uses a well-known CLSID for the proxy/stub factory.
    /// </summary>
    public string EmitClsidPsFactoryBuffer()
    {
        var sb = new StringBuilder();

        sb.AppendLine("#define _PROXY_FILE_LIST_DEFINED");
        sb.AppendLine();
        sb.AppendLine("const CLSID CLSID_PSFactoryBuffer = {0x00000000, 0x0000, 0x0000, {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}};");

        return sb.ToString();
    }

    /// <summary>
    /// Emits the __MIDL_ProxyFileInfo struct and g_ProxyFileList array.
    /// </summary>
    public string EmitProxyFileInfo(IReadOnlyList<string> interfaceNames)
    {
        int tableSize = interfaceNames.Count;
        var sb = new StringBuilder();

        sb.AppendLine("static const ProxyFileInfo __MIDL_ProxyFileInfo =");
        sb.AppendLine("{");
        sb.AppendLine("    (const PCInterfaceProxyVtblList *) & _ProxyVtblList,");
        sb.AppendLine("    (const PCInterfaceStubVtblList *) & _StubVtblList,");
        sb.AppendLine("    (const PCInterfaceName *) & _InterfaceNamesList,");
        sb.AppendLine("    0,      /* the IID lookup table */");
        sb.AppendLine("    & __MIDL_ProxyFileInfo.TableSize,");
        sb.AppendLine($"    {tableSize},      /* TableSize */");
        sb.AppendLine("    2,      /* version */");
        sb.AppendLine("    0,      /* length of async table */");
        sb.AppendLine("    0,      /* async table lookup */");
        sb.AppendLine("    0,      /* reserved */");
        sb.AppendLine("    0       /* reserved */");
        sb.AppendLine("};");
        sb.AppendLine();
        sb.AppendLine("const ProxyFileInfo * const g_ProxyFileList[] = {");
        sb.AppendLine("    &__MIDL_ProxyFileInfo,");
        sb.AppendLine("    NULL");
        sb.AppendLine("};");

        return sb.ToString();
    }

    /// <summary>
    /// Emits the DLL export functions: DllGetClassObject, DllCanUnloadNow,
    /// DllRegisterServer, DllUnregisterServer, GetProxyDllInfo.
    /// Guarded by REGISTER_PROXY_DLL to match standard MIDL output.
    /// </summary>
    public string EmitDllExports()
    {
        var sb = new StringBuilder();

        sb.AppendLine("#ifdef REGISTER_PROXY_DLL");
        sb.AppendLine();
        sb.AppendLine("CStdPSFactoryBuffer gPFactory = {0, 0, 0, 0};");
        sb.AppendLine();
        sb.AppendLine("HRESULT STDAPICALLTYPE DllGetClassObject(REFCLSID rclsid, REFIID riid, void** ppv)");
        sb.AppendLine("{");
        sb.AppendLine("    return NdrDllGetClassObject(rclsid, riid, ppv, g_ProxyFileList, &CLSID_PSFactoryBuffer, &gPFactory);");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("HRESULT STDAPICALLTYPE DllCanUnloadNow(void)");
        sb.AppendLine("{");
        sb.AppendLine("    return NdrDllCanUnloadNow(&gPFactory);");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("HRESULT STDAPICALLTYPE DllRegisterServer(void)");
        sb.AppendLine("{");
        sb.AppendLine("    return NdrDllRegisterProxy(NULL, g_ProxyFileList, &CLSID_PSFactoryBuffer);");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("HRESULT STDAPICALLTYPE DllUnregisterServer(void)");
        sb.AppendLine("{");
        sb.AppendLine("    return NdrDllUnregisterProxy(NULL, g_ProxyFileList, &CLSID_PSFactoryBuffer);");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("HRESULT STDAPICALLTYPE GetProxyDllInfo(const ProxyFileInfo*** pInfo, const CLSID** pClsid)");
        sb.AppendLine("{");
        sb.AppendLine("    *pInfo = g_ProxyFileList;");
        sb.AppendLine("    *pClsid = &CLSID_PSFactoryBuffer;");
        sb.AppendLine("    return S_OK;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("#endif /* REGISTER_PROXY_DLL */");

        return sb.ToString();
    }
}
