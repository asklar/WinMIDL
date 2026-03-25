using System.Text;

namespace Midl.CodeGen.Stub;

/// <summary>
/// Emits proxy vtable, stub table, format string offset table, proxy info,
/// server info, and stub vtable structures for COM/WinRT proxy/stub generation.
/// </summary>
public class ProxyVtableBuilder
{
    /// <summary>
    /// Represents a user method with its name and byte offset in the proc format string.
    /// </summary>
    public readonly record struct MethodEntry(string Name, int ProcFormatOffset);

    private const int IUnknownSlotCount = 3;
    private const int IInspectableSlotCount = 3; // GetIids, GetRuntimeClassName, GetTrustLevel

    /// <summary>
    /// Emits the FormatStringOffsetTable for an interface.
    /// For IUnknown-derived: contains only custom method offsets.
    /// For IInspectable-derived: contains (unsigned short)-1 for 3 forwarded IInspectable slots,
    /// then custom method offsets.
    /// </summary>
    public string EmitFormatStringOffsetTable(
        string interfaceName,
        IReadOnlyList<MethodEntry> methods,
        bool isInspectable)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"static const unsigned short {interfaceName}_FormatStringOffsetTable[] =");
        sb.AppendLine("    {");

        if (isInspectable)
        {
            sb.AppendLine("    (unsigned short) -1,    /* GetIids */");
            sb.AppendLine("    (unsigned short) -1,    /* GetRuntimeClassName */");
            sb.AppendLine("    (unsigned short) -1,    /* GetTrustLevel */");
        }

        for (int i = 0; i < methods.Count; i++)
        {
            var comma = i < methods.Count - 1 ? "," : "";
            sb.AppendLine($"    {methods[i].ProcFormatOffset}{comma}");
        }

        sb.AppendLine("    };");
        return sb.ToString();
    }

    /// <summary>
    /// Emits the CINTERFACE_PROXY_VTABLE for an interface.
    /// Includes IUnknown proxy methods, optional IInspectable forwarded slots (0),
    /// and stubless client markers (-1) for each user method.
    /// </summary>
    public string EmitProxyVtable(
        string interfaceName,
        IReadOnlyList<MethodEntry> methods,
        bool isInspectable)
    {
        int totalSlots = IUnknownSlotCount
            + (isInspectable ? IInspectableSlotCount : 0)
            + methods.Count;

        var sb = new StringBuilder();
        sb.AppendLine($"CINTERFACE_PROXY_VTABLE({totalSlots}) _{interfaceName}ProxyVtbl =");
        sb.AppendLine("{");
        sb.AppendLine($"    &{interfaceName}_ProxyInfo,");
        sb.AppendLine($"    &IID_{interfaceName},");
        sb.AppendLine("    IUnknown_QueryInterface_Proxy,");
        sb.AppendLine("    IUnknown_AddRef_Proxy,");

        if (!isInspectable && methods.Count == 0)
        {
            sb.AppendLine("    IUnknown_Release_Proxy");
        }
        else
        {
            sb.AppendLine("    IUnknown_Release_Proxy ,");
        }

        if (isInspectable)
        {
            sb.AppendLine("    0 /* IInspectable::GetIids */ ,");
            sb.AppendLine("    0 /* IInspectable::GetRuntimeClassName */ ,");

            if (methods.Count == 0)
            {
                sb.AppendLine("    0 /* IInspectable::GetTrustLevel */");
            }
            else
            {
                sb.AppendLine("    0 /* IInspectable::GetTrustLevel */ ,");
            }
        }

        for (int i = 0; i < methods.Count; i++)
        {
            var trailing = i < methods.Count - 1 ? " ," : "";
            sb.AppendLine($"    (void *) (INT_PTR) -1 /* {interfaceName}::{methods[i].Name} */{trailing}");
        }

        sb.AppendLine("};");
        return sb.ToString();
    }

    /// <summary>
    /// Emits the PRPC_STUB_FUNCTION table for an IInspectable-derived interface.
    /// Contains STUB_FORWARDING_FUNCTION for the 3 IInspectable methods
    /// and NdrStubCall2 for each user method.
    /// Returns null for IUnknown-derived interfaces (no table needed).
    /// </summary>
    public string? EmitStubTable(
        string interfaceName,
        IReadOnlyList<MethodEntry> methods,
        bool isInspectable)
    {
        if (!isInspectable)
            return null;

        var sb = new StringBuilder();
        sb.AppendLine($"static const PRPC_STUB_FUNCTION {interfaceName}_table[] =");
        sb.AppendLine("{");

        int totalEntries = IInspectableSlotCount + methods.Count;
        int entryIndex = 0;

        // 3 forwarding entries for IInspectable methods
        for (int i = 0; i < IInspectableSlotCount; i++)
        {
            var comma = entryIndex < totalEntries - 1 ? "," : "";
            sb.AppendLine($"    STUB_FORWARDING_FUNCTION{comma}");
            entryIndex++;
        }

        // NdrStubCall2 for each user method
        for (int i = 0; i < methods.Count; i++)
        {
            var comma = entryIndex < totalEntries - 1 ? "," : "";
            sb.AppendLine($"    NdrStubCall2{comma}");
            entryIndex++;
        }

        sb.AppendLine("};");
        return sb.ToString();
    }

    /// <summary>
    /// Emits the MIDL_STUBLESS_PROXY_INFO structure for an interface.
    /// Uses the [-3] offset trick for both IUnknown and IInspectable-derived interfaces.
    /// </summary>
    public string EmitProxyInfo(
        string interfaceName,
        bool isInspectable,
        string filePrefix)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"static const MIDL_STUBLESS_PROXY_INFO {interfaceName}_ProxyInfo =");
        sb.AppendLine("    {");
        sb.AppendLine("    &Object_StubDesc,");
        sb.AppendLine($"    {filePrefix}__MIDL_ProcFormatString.Format,");
        sb.AppendLine($"    &{interfaceName}_FormatStringOffsetTable[-3],");
        sb.AppendLine("    0,");
        sb.AppendLine("    0,");
        sb.AppendLine("    0");
        sb.AppendLine("    };");
        return sb.ToString();
    }

    /// <summary>
    /// Emits the MIDL_SERVER_INFO structure for an interface.
    /// </summary>
    public string EmitServerInfo(
        string interfaceName,
        bool isInspectable,
        string filePrefix)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"static const MIDL_SERVER_INFO {interfaceName}_ServerInfo =");
        sb.AppendLine("    {");
        sb.AppendLine("    &Object_StubDesc,");
        sb.AppendLine("    0,");
        sb.AppendLine($"    {filePrefix}__MIDL_ProcFormatString.Format,");
        sb.AppendLine($"    &{interfaceName}_FormatStringOffsetTable[-3],");
        sb.AppendLine("    0,");
        sb.AppendLine("    0,");
        sb.AppendLine("    0,");
        sb.AppendLine("    0};");
        return sb.ToString();
    }

    /// <summary>
    /// Emits the CInterfaceStubVtbl structure for an interface.
    /// IUnknown-derived uses CStdStubBuffer_METHODS.
    /// IInspectable-derived uses CStdStubBuffer_DELEGATING_METHODS with a stub function table.
    /// </summary>
    public string EmitStubVtable(
        string interfaceName,
        IReadOnlyList<MethodEntry> methods,
        bool isInspectable)
    {
        int totalSlots = IUnknownSlotCount
            + (isInspectable ? IInspectableSlotCount : 0)
            + methods.Count;

        var sb = new StringBuilder();
        sb.AppendLine($"const CInterfaceStubVtbl _{interfaceName}StubVtbl =");
        sb.AppendLine("{");
        sb.AppendLine($"    &IID_{interfaceName},");
        sb.AppendLine($"    &{interfaceName}_ServerInfo,");
        sb.AppendLine($"    {totalSlots},");

        if (isInspectable)
        {
            sb.AppendLine($"    &{interfaceName}_table[-3],");
            sb.AppendLine($"    CStdStubBuffer_DELEGATING_METHODS");
        }
        else
        {
            sb.AppendLine("    0, /* pure interpreted */");
            sb.AppendLine("    CStdStubBuffer_METHODS");
        }

        sb.AppendLine("};");
        return sb.ToString();
    }
}
