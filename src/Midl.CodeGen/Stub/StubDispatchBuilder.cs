using System.Text;
using Midl.Analysis.Symbols;

namespace Midl.CodeGen.Stub;

/// <summary>
/// Emits stub dispatch structures for proxy/stub files:
/// - MIDL_STUB_DESC (one per file)
/// - FormatStringOffsetTable (one per interface)
/// - MIDL_SERVER_INFO (one per interface)
/// - Stub function table (for IInspectable-derived / delegating interfaces)
/// - CInterfaceStubVtbl (one per interface)
/// </summary>
public class StubDispatchBuilder
{
    private readonly string _filePrefix;

    // NDR library version: 5.2
    private const int NdrLibraryVersion = 0x50002;
    // MIDL compiler version: 8.1.628
    private const int MidlVersion = 0x8010274;
    // MIDL flags
    private const int MidlFlags = 0x1;

    public StubDispatchBuilder(string filePrefix)
    {
        _filePrefix = filePrefix;
    }

    /// <summary>
    /// Emits the shared MIDL_STUB_DESC (Object_StubDesc) for the proxy/stub file.
    /// </summary>
    public string EmitStubDesc()
    {
        var sb = new StringBuilder();

        sb.AppendLine("#ifdef __cplusplus");
        sb.AppendLine("namespace {");
        sb.AppendLine("#endif");

        sb.AppendLine("static const MIDL_STUB_DESC Object_StubDesc =");
        sb.AppendLine("    {");
        sb.AppendLine("    0,");
        sb.AppendLine("    NdrOleAllocate,");
        sb.AppendLine("    NdrOleFree,");
        sb.AppendLine("    0,");
        sb.AppendLine("    0,");
        sb.AppendLine("    0,");
        sb.AppendLine("    0,");
        sb.AppendLine("    0,");
        sb.AppendLine($"    {_filePrefix}__MIDL_TypeFormatString.Format,");
        sb.AppendLine("    1, /* -error bounds_check flag */");
        sb.AppendLine($"    0x{NdrLibraryVersion:x}, /* Ndr library version */");
        sb.AppendLine("    0,");
        sb.AppendLine($"    0x{MidlVersion:x}, /* MIDL Version 8.1.628 */");
        sb.AppendLine("    0,");
        sb.AppendLine("    0,");
        sb.AppendLine("    0,  /* notify & notify_flag routine table */");
        sb.AppendLine($"    0x{MidlFlags:x}, /* MIDL flag */");
        sb.AppendLine("    0, /* cs routines */");
        sb.AppendLine("    0,   /* proxy/server info */");
        sb.AppendLine("    0");
        sb.AppendLine("    };");

        sb.AppendLine("#ifdef __cplusplus");
        sb.AppendLine("}");
        sb.AppendLine("#endif");

        return sb.ToString();
    }

    /// <summary>
    /// Emits the per-interface FormatStringOffsetTable.
    /// </summary>
    public string EmitFormatStringOffsetTable(InterfaceSymbol iface, IReadOnlyList<int> methodOffsets)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"static const unsigned short {iface.Name}_FormatStringOffsetTable[] =");
        sb.AppendLine("    {");

        for (int i = 0; i < methodOffsets.Count; i++)
        {
            string comma = i < methodOffsets.Count - 1 ? "," : "";
            sb.AppendLine($"    {methodOffsets[i]}{comma}");
        }

        sb.AppendLine("    };");

        return sb.ToString();
    }

    /// <summary>
    /// Emits the per-interface MIDL_SERVER_INFO.
    /// </summary>
    public string EmitServerInfo(InterfaceSymbol iface)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"static const MIDL_SERVER_INFO {iface.Name}_ServerInfo =");
        sb.AppendLine("    {");
        sb.AppendLine("    &Object_StubDesc,");
        sb.AppendLine("    0,");
        sb.AppendLine($"    {_filePrefix}__MIDL_ProcFormatString.Format,");
        sb.AppendLine($"    &{iface.Name}_FormatStringOffsetTable[-3],");
        sb.AppendLine("    0,");
        sb.AppendLine("    0,");
        sb.AppendLine("    0,");
        sb.AppendLine("    0};");

        return sb.ToString();
    }

    /// <summary>
    /// Emits the stub function table for IInspectable-derived (delegating) interfaces.
    /// Returns null for IUnknown-derived interfaces (no table needed).
    /// </summary>
    public string? EmitStubFunctionTable(InterfaceSymbol iface)
    {
        if (!IsInspectableDerived(iface))
            return null;

        int customMethodCount = CountCustomMethods(iface);

        var sb = new StringBuilder();

        sb.AppendLine($"EXTERN_C DECLSPEC_SELECTANY const PRPC_STUB_FUNCTION {iface.Name}_table[] =");
        sb.AppendLine("{");

        // 3 forwarding functions for IInspectable methods (GetIids, GetRuntimeClassName, GetTrustLevel)
        sb.AppendLine("    STUB_FORWARDING_FUNCTION,");
        sb.AppendLine("    STUB_FORWARDING_FUNCTION,");
        sb.AppendLine("    STUB_FORWARDING_FUNCTION,");

        // NdrStubCall2 for each custom method
        for (int i = 0; i < customMethodCount; i++)
        {
            string comma = i < customMethodCount - 1 ? "," : "";
            sb.AppendLine($"    NdrStubCall2{comma}");
        }

        sb.AppendLine("};");

        return sb.ToString();
    }

    /// <summary>
    /// Emits the CInterfaceStubVtbl for an interface.
    /// </summary>
    public string EmitStubVtbl(InterfaceSymbol iface)
    {
        bool isDelegating = IsInspectableDerived(iface);
        int vtableSlots = GetVtableSlotCount(iface);

        var sb = new StringBuilder();

        if (isDelegating)
        {
            sb.AppendLine($"CInterfaceStubVtbl _{iface.Name}StubVtbl =");
        }
        else
        {
            sb.AppendLine($"const CInterfaceStubVtbl _{iface.Name}StubVtbl =");
        }

        sb.AppendLine("{");
        sb.AppendLine($"    &IID_{iface.Name},");
        sb.AppendLine($"    &{iface.Name}_ServerInfo,");
        sb.AppendLine($"    {vtableSlots},");

        if (isDelegating)
        {
            sb.AppendLine($"    &{iface.Name}_table[-3],");
            sb.AppendLine("    CStdStubBuffer_DELEGATING_METHODS");
        }
        else
        {
            sb.AppendLine("    0, /* pure interpreted */");
            sb.AppendLine("    CStdStubBuffer_METHODS");
        }

        sb.AppendLine("};");

        return sb.ToString();
    }

    /// <summary>
    /// Returns the total vtable slot count for an interface
    /// (base interface slots + custom methods/properties/events).
    /// </summary>
    public static int GetVtableSlotCount(InterfaceSymbol iface)
    {
        int baseSlots = IsInspectableDerived(iface) ? 6 : 3;
        return baseSlots + CountCustomMethods(iface);
    }

    /// <summary>
    /// Counts the number of custom vtable methods (expanding properties and events).
    /// </summary>
    private static int CountCustomMethods(InterfaceSymbol iface)
    {
        int count = 0;
        foreach (var member in iface.Members)
        {
            switch (member)
            {
                case MethodSymbol:
                    count++;
                    break;
                case PropertySymbol prop:
                    if (prop.HasGetter) count++;
                    if (prop.HasSetter) count++;
                    break;
                case EventSymbol:
                    count += 2; // add_ and remove_
                    break;
            }
        }
        return count;
    }

    /// <summary>
    /// Determines if the interface is IInspectable-derived (delegating).
    /// </summary>
    private static bool IsInspectableDerived(InterfaceSymbol iface)
    {
        return iface.RequiredInterfaces.Any(r => r.Name == "IInspectable");
    }
}
