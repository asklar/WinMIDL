using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.CodeGen.Stub;

/// <summary>
/// NDR format character constants from rpcndr.h.
/// </summary>
public static class FC
{
    public const byte FC_BYTE = 0x01;
    public const byte FC_CHAR = 0x02;
    public const byte FC_SMALL = 0x03;
    public const byte FC_USMALL = 0x04;
    public const byte FC_WCHAR = 0x05;
    public const byte FC_SHORT = 0x06;
    public const byte FC_USHORT = 0x07;
    public const byte FC_LONG = 0x08;
    public const byte FC_ULONG = 0x09;
    public const byte FC_FLOAT = 0x0A;
    public const byte FC_HYPER = 0x0B;
    public const byte FC_DOUBLE = 0x0C;
    public const byte FC_ENUM16 = 0x0D;
    public const byte FC_ENUM32 = 0x0E;
    public const byte FC_ERROR_STATUS_T = 0x10;

    public const byte FC_RP = 0x11;
    public const byte FC_UP = 0x12;
    public const byte FC_OP = 0x13;
    public const byte FC_FP = 0x14;

    public const byte FC_STRUCT = 0x15;
    public const byte FC_CSTRUCT = 0x17;
    public const byte FC_CARRAY = 0x1B;
    public const byte FC_SMFARRAY = 0x1D;

    public const byte FC_IP = 0x2F;
    public const byte FC_EMBEDDED_COMPLEX = 0x4C;
    public const byte FC_CONSTANT_IID = 0x5A;
    public const byte FC_END = 0x5B;
    public const byte FC_PAD = 0x5C;

    public const byte FC_USER_MARSHAL = 0xB4;

    // Pointer flags (second byte after FC_RP/FC_UP/FC_OP)
    public const byte ALLOCED_ON_STACK = 0x04;
    public const byte SIMPLE_POINTER = 0x08;
    public const byte POINTER_DEREF = 0x10;
}

/// <summary>
/// Builds the NDR type format string for proxy/stub generation.
/// The type format string encodes the wire representation of all non-trivial types
/// used in interface methods.
/// </summary>
public class NdrTypeFormatBuilder
{
    private readonly List<byte> _formatBytes = new();
    private readonly Dictionary<string, int> _typeOffsets = new();
    private bool _hstringWireEmitted;
    private int _hstringWireOffset;
    private bool _guidStructEmitted;
    private int _guidStructOffset;

    public NdrTypeFormatBuilder()
    {
        // Start with NdrFcShort(0x0) padding — 2 zero bytes at offset 0
        _formatBytes.Add(0x00);
        _formatBytes.Add(0x00);
    }

    /// <summary>
    /// Returns the FC_* format character for a primitive/base type, or null if the type is not a base type.
    /// Base types are encoded inline in the procedure format string and don't need type format entries.
    /// </summary>
    public static byte? GetPrimitiveFormatChar(string typeName) => typeName switch
    {
        "UInt8" or "byte" => FC.FC_BYTE,
        "Boolean" or "boolean" => FC.FC_SMALL,
        "Char16" or "wchar_t" or "char" => FC.FC_WCHAR,
        "Int16" or "short" => FC.FC_SHORT,
        "UInt16" => FC.FC_USHORT,
        "Int32" or "int" or "long" or "HRESULT" => FC.FC_LONG,
        "UInt32" => FC.FC_ULONG,
        "Single" or "float" => FC.FC_FLOAT,
        "Int64" or "UInt64" or "hyper" => FC.FC_HYPER,
        "Double" or "double" => FC.FC_DOUBLE,
        "error_status_t" => FC.FC_ERROR_STATUS_T,
        _ => null,
    };

    /// <summary>
    /// Returns the offset in the type format string for the given type.
    /// If the type hasn't been added yet, it is encoded and appended.
    /// Returns -1 for base types that are encoded inline (no type format entry needed).
    /// </summary>
    public int GetOrAddType(TypeRef typeRef)
    {
        // Check for base types first
        if (GetPrimitiveFormatChar(typeRef.Name) != null)
            return -1;

        string key = GetTypeKey(typeRef);
        if (_typeOffsets.TryGetValue(key, out int offset))
            return offset;

        offset = typeRef.Name switch
        {
            "String" => EmitHStringType(),
            "Guid" => EmitGuidType(),
            "Object" => EmitInterfacePointer(typeRef),
            _ => EmitResolvedType(typeRef),
        };

        _typeOffsets[key] = offset;
        return offset;
    }

    /// <summary>
    /// Returns the complete format string bytes including the initial padding and terminator.
    /// </summary>
    public byte[] Build()
    {
        var result = new List<byte>(_formatBytes);
        result.Add(0x00); // terminator
        return result.ToArray();
    }

    /// <summary>
    /// Emits the C source code for the MIDL_TYPE_FORMAT_STRING structure.
    /// </summary>
    public string EmitTypeFormatString(string filePrefix)
    {
        var bytes = Build();
        var sb = new System.Text.StringBuilder();

        // Emit the typedef for the format string struct
        sb.AppendLine($"typedef struct _{filePrefix}_MIDL_TYPE_FORMAT_STRING");
        sb.AppendLine("    {");
        sb.AppendLine("        short Pad;");
        sb.AppendLine($"        unsigned char Format[{bytes.Length}];");
        sb.AppendLine($"    }} {filePrefix}_MIDL_TYPE_FORMAT_STRING;");
        sb.AppendLine();

        sb.AppendLine($"static const {filePrefix}_MIDL_TYPE_FORMAT_STRING {filePrefix}__MIDL_TypeFormatString =");
        sb.AppendLine("    {");
        sb.AppendLine("        0,");
        sb.AppendLine("        {");

        for (int i = 0; i < bytes.Length; i++)
        {
            if (i == 0)
            {
                // NdrFcShort(0x0) — the first two bytes
                sb.AppendLine($"\t\t\tNdrFcShort( 0x0 ),\t/* 0 */");
                i++; // skip second byte of the short
                continue;
            }

            byte b = bytes[i];

            if (i == bytes.Length - 1 && b == 0x00)
            {
                // Terminator
                sb.AppendLine();
                sb.AppendLine("\t\t\t0x0");
                continue;
            }

            sb.AppendLine($"/* {i,2} */\t0x{b:x2},\t\t/* {GetFormatCharName(b)} */");
        }

        sb.AppendLine("        }");
        sb.AppendLine("    };");

        return sb.ToString();
    }

    /// <summary>Current byte length of the format string (before terminator).</summary>
    public int CurrentOffset => _formatBytes.Count;

    private string GetTypeKey(TypeRef typeRef)
    {
        if (typeRef.ResolvedSymbol != null)
            return typeRef.ResolvedSymbol.FullName;
        return typeRef.Name;
    }

    private int EmitHStringType()
    {
        EnsureHStringWireRepresentation();

        int offset = _formatBytes.Count;

        // FC_USER_MARSHAL for [in] HSTRING
        _formatBytes.Add(FC.FC_USER_MARSHAL);  // 0xB4
        _formatBytes.Add(0x83);                 // flags: unique (0x80) | alignment 3
        EmitShort(0x0000);                      // user marshal table index = 0
        EmitShort(0x0008);                      // user type memory size = 8 (pointer on x64)
        EmitShort(0x0000);                      // user type buffer size = 0 (variable)
        EmitShort((short)(_hstringWireOffset - _formatBytes.Count)); // relative offset to wire repr

        return offset;
    }

    private void EnsureHStringWireRepresentation()
    {
        if (_hstringWireEmitted)
            return;

        _hstringWireEmitted = true;

        // FC_UP → FC_CSTRUCT wire representation for HSTRING/BSTR
        _hstringWireOffset = _formatBytes.Count;

        // FC_OP (object pointer to CSTRUCT)
        _formatBytes.Add(FC.FC_OP);
        _formatBytes.Add(0x00);
        int opOffsetPos = _formatBytes.Count;
        EmitShort(0); // placeholder for offset to FC_CSTRUCT

        // FC_CARRAY - conformant array of wchar_t
        int carrayOffset = _formatBytes.Count;
        _formatBytes.Add(FC.FC_CARRAY);
        _formatBytes.Add(0x01);                 // alignment = 1
        EmitShort(0x0002);                      // element size = 2 (wchar_t)
        _formatBytes.Add(0x09);                 // corr desc: FC_ULONG
        _formatBytes.Add(0x00);
        EmitShort(unchecked((short)0xFFFC));     // offset to length field = -4
        EmitShort(0x0001);                      // corr flags: early
        _formatBytes.Add(FC.FC_SHORT);          // element type
        _formatBytes.Add(FC.FC_END);

        // FC_CSTRUCT - conformant structure {length, alloc, data[]}
        int cstructOffset = _formatBytes.Count;
        _formatBytes.Add(FC.FC_CSTRUCT);
        _formatBytes.Add(0x03);                 // alignment = 3 (4-byte aligned)
        EmitShort(0x0008);                      // struct size = 8
        EmitShort((short)(carrayOffset - (_formatBytes.Count))); // offset back to FC_CARRAY
        _formatBytes.Add(FC.FC_LONG);           // length field
        _formatBytes.Add(FC.FC_LONG);           // alloc field
        _formatBytes.Add(FC.FC_PAD);
        _formatBytes.Add(FC.FC_END);

        // Patch the FC_OP offset to point to FC_CSTRUCT
        PatchShort(opOffsetPos, (short)(cstructOffset - opOffsetPos));
    }

    private int EmitGuidType()
    {
        if (_guidStructEmitted)
            return _guidStructOffset;

        _guidStructEmitted = true;

        // FC_SMFARRAY for Data4[8]
        int smfarrayOffset = _formatBytes.Count;
        _formatBytes.Add(FC.FC_SMFARRAY);
        _formatBytes.Add(0x00);                 // alignment = 0
        EmitShort(0x0008);                      // total size = 8
        _formatBytes.Add(FC.FC_BYTE);           // element type
        _formatBytes.Add(FC.FC_END);

        // FC_STRUCT for GUID: { DWORD Data1, WORD Data2, WORD Data3, BYTE Data4[8] }
        _guidStructOffset = _formatBytes.Count;
        _formatBytes.Add(FC.FC_STRUCT);
        _formatBytes.Add(0x03);                 // alignment = 3 (4-byte aligned)
        EmitShort(0x0010);                      // struct size = 16
        _formatBytes.Add(FC.FC_LONG);           // Data1
        _formatBytes.Add(FC.FC_SHORT);          // Data2
        _formatBytes.Add(FC.FC_SHORT);          // Data3
        _formatBytes.Add(FC.FC_EMBEDDED_COMPLEX);
        _formatBytes.Add(0x00);                 // padding
        EmitShort((short)(smfarrayOffset - _formatBytes.Count)); // offset back to FC_SMFARRAY
        _formatBytes.Add(FC.FC_END);

        return _guidStructOffset;
    }

    private int EmitInterfacePointer(TypeRef typeRef)
    {
        int offset = _formatBytes.Count;

        Guid iid;
        if (typeRef.Name == "Object")
        {
            // IInspectable IID: {AF86E2E0-B12D-4C6A-9C5A-D7AA65101E90}
            iid = new Guid("AF86E2E0-B12D-4C6A-9C5A-D7AA65101E90");
        }
        else if (typeRef.ResolvedSymbol is InterfaceSymbol iface && iface.Uuid.HasValue)
        {
            iid = iface.Uuid.Value;
        }
        else if (typeRef.ResolvedSymbol is DelegateSymbol del && del.Uuid.HasValue)
        {
            iid = del.Uuid.Value;
        }
        else if (TryGetUuidFromAttributes(typeRef.ResolvedSymbol, out var attrGuid))
        {
            iid = attrGuid;
        }
        else
        {
            // Unknown interface — use zero GUID as placeholder
            iid = Guid.Empty;
        }

        // FC_IP + FC_CONSTANT_IID + 16-byte IID
        _formatBytes.Add(FC.FC_IP);
        _formatBytes.Add(FC.FC_CONSTANT_IID);
        EmitGuid(iid);

        return offset;
    }

    private int EmitResolvedType(TypeRef typeRef)
    {
        if (typeRef.ResolvedSymbol is InterfaceSymbol or DelegateSymbol)
            return EmitInterfacePointer(typeRef);

        if (typeRef.ResolvedSymbol is EnumSymbol)
        {
            // Enums are base types (FC_ENUM32), encoded inline
            return -1;
        }

        if (typeRef.ResolvedSymbol is StructSymbol structSym)
            return EmitSimpleStruct(structSym);

        // Fallback: treat as unknown complex type
        return -1;
    }

    private int EmitSimpleStruct(StructSymbol structSym)
    {
        string key = structSym.FullName;
        if (_typeOffsets.TryGetValue(key, out int existing))
            return existing;

        int offset = _formatBytes.Count;
        _typeOffsets[key] = offset;

        // Calculate struct size and max alignment
        int structSize = 0;
        byte maxAlignment = 0;

        foreach (var field in structSym.Fields)
        {
            var fc = GetPrimitiveFormatChar(field.Type.Name);
            byte fieldAlignment = fc != null ? GetAlignment(fc.Value) : (byte)0x03;
            int fieldSize = fc != null ? GetSize(fc.Value) : 8; // default to pointer size

            if (fieldAlignment > maxAlignment)
                maxAlignment = fieldAlignment;

            // Align (alignment value IS the mask: 0=1B, 1=2B, 3=4B, 7=8B)
            int alignMask = fieldAlignment;
            structSize = (structSize + alignMask) & ~alignMask;
            structSize += fieldSize;
        }

        // Pad struct size to alignment
        if (maxAlignment > 0)
        {
            int alignMask = maxAlignment;
            structSize = (structSize + alignMask) & ~alignMask;
        }

        // FC_STRUCT
        _formatBytes.Add(FC.FC_STRUCT);
        _formatBytes.Add(maxAlignment);
        EmitShort((short)structSize);

        foreach (var field in structSym.Fields)
        {
            var fc = GetPrimitiveFormatChar(field.Type.Name);
            if (fc != null)
            {
                _formatBytes.Add(fc.Value);
            }
            else
            {
                // Embedded complex type
                _formatBytes.Add(FC.FC_EMBEDDED_COMPLEX);
                _formatBytes.Add(0x00);
                int innerOffset = GetOrAddType(field.Type);
                EmitShort((short)(innerOffset - _formatBytes.Count));
            }
        }

        // Ensure even alignment for FC_END
        if (_formatBytes.Count % 2 != 0)
            _formatBytes.Add(FC.FC_PAD);
        _formatBytes.Add(FC.FC_END);

        return offset;
    }

    /// <summary>
    /// Emits an FC_RP (reference pointer) entry for an [out] parameter pointing to the given base type.
    /// Returns the offset of the FC_RP entry.
    /// </summary>
    public int EmitRefPointerToBaseType(byte baseTypeFc)
    {
        string key = $"__rp_simple_{baseTypeFc:x2}";
        if (_typeOffsets.TryGetValue(key, out int existing))
            return existing;

        int offset = _formatBytes.Count;
        _typeOffsets[key] = offset;

        _formatBytes.Add(FC.FC_RP);
        _formatBytes.Add(FC.ALLOCED_ON_STACK | FC.SIMPLE_POINTER); // 0x0C
        _formatBytes.Add(baseTypeFc);
        _formatBytes.Add(FC.FC_PAD);

        return offset;
    }

    /// <summary>
    /// Emits an FC_RP [pointer_deref] entry for an [out] interface pointer parameter (e.g., IFoo**).
    /// Returns the offset of the FC_RP entry.
    /// </summary>
    public int EmitRefPointerDeref(TypeRef interfaceTypeRef)
    {
        string key = $"__rp_deref_{GetTypeKey(interfaceTypeRef)}";
        if (_typeOffsets.TryGetValue(key, out int existing))
            return existing;

        int offset = _formatBytes.Count;
        _typeOffsets[key] = offset;

        _formatBytes.Add(FC.FC_RP);
        _formatBytes.Add(FC.POINTER_DEREF); // 0x10
        // Offset to the FC_IP entry — we may need to emit it
        int ipOffset = GetOrAddType(interfaceTypeRef);
        EmitShort((short)(ipOffset - _formatBytes.Count));

        return offset;
    }

    /// <summary>
    /// Emits an FC_RP [alloced_on_stack] entry for an [out] HSTRING* parameter.
    /// This uses FC_OP → wire struct → FC_USER_MARSHAL pattern.
    /// </summary>
    public int EmitHStringOutType()
    {
        const string key = "__hstring_out";
        if (_typeOffsets.TryGetValue(key, out int existing))
            return existing;

        EnsureHStringWireRepresentation();

        // FC_RP [alloced_on_stack] pointing to FC_USER_MARSHAL
        int rpOffset = _formatBytes.Count;

        _formatBytes.Add(FC.FC_RP);
        _formatBytes.Add(FC.ALLOCED_ON_STACK); // 0x04
        int rpOffsetPos = _formatBytes.Count;
        EmitShort(0); // placeholder

        // FC_OP pointing to the CSTRUCT wire repr
        int opOffset = _formatBytes.Count;
        _formatBytes.Add(FC.FC_OP);
        _formatBytes.Add(0x00);
        // Find the CSTRUCT offset within the wire representation
        // The wire repr starts at _hstringWireOffset, FC_OP(2) + offset(2) + CARRAY(12) = CSTRUCT at +14
        int cstructOffset = _hstringWireOffset + 2 + 2 + 12; // approximate
        EmitShort((short)(cstructOffset - _formatBytes.Count));

        // FC_USER_MARSHAL for [out] HSTRING
        int umOffset = _formatBytes.Count;
        _formatBytes.Add(FC.FC_USER_MARSHAL);
        _formatBytes.Add(0x83);
        EmitShort(0x0000);                      // table index
        EmitShort(0x0008);                      // memory size
        EmitShort(0x0000);                      // buffer size
        EmitShort((short)(opOffset - _formatBytes.Count)); // offset to FC_OP

        // Patch FC_RP offset to point to FC_USER_MARSHAL
        PatchShort(rpOffsetPos, (short)(umOffset - rpOffsetPos));

        _typeOffsets[key] = rpOffset;
        return rpOffset;
    }

    /// <summary>
    /// Emits an FC_RP entry for an [out] struct parameter (e.g., [out] GUID*).
    /// </summary>
    public int EmitRefPointerToStruct(TypeRef structTypeRef)
    {
        string key = $"__rp_struct_{GetTypeKey(structTypeRef)}";
        if (_typeOffsets.TryGetValue(key, out int existing))
            return existing;

        int offset = _formatBytes.Count;
        _typeOffsets[key] = offset;

        _formatBytes.Add(FC.FC_RP);
        _formatBytes.Add(0x00); // no special flags
        int structOffset = GetOrAddType(structTypeRef);
        EmitShort((short)(structOffset - _formatBytes.Count));

        return offset;
    }

    private void EmitShort(short value)
    {
        _formatBytes.Add((byte)(value & 0xFF));
        _formatBytes.Add((byte)((value >> 8) & 0xFF));
    }

    private void EmitShort(int value) => EmitShort((short)value);

    private void EmitGuid(Guid iid)
    {
        var bytes = iid.ToByteArray();
        // Guid.ToByteArray() returns mixed-endian: Data1 as LE, Data2 as LE, Data3 as LE, Data4 as-is
        // NDR uses NdrFcLong (LE int32) for Data1, NdrFcShort (LE int16) for Data2/Data3, then raw bytes
        _formatBytes.AddRange(bytes);
    }

    private void PatchShort(int position, short value)
    {
        _formatBytes[position] = (byte)(value & 0xFF);
        _formatBytes[position + 1] = (byte)((value >> 8) & 0xFF);
    }

    /// <summary>
    /// Attempts to find a UUID from the [uuid(...)] attribute on a symbol.
    /// In compat mode, UUIDs may be stored as attributes rather than the Uuid property.
    /// </summary>
    private static bool TryGetUuidFromAttributes(ISymbol? symbol, out Guid guid)
    {
        guid = Guid.Empty;
        if (symbol == null) return false;

        var attrs = symbol switch
        {
            InterfaceSymbol i => i.Attributes,
            DelegateSymbol d => d.Attributes,
            _ => null,
        };

        if (attrs == null) return false;

        var uuidAttr = attrs.FirstOrDefault(a =>
            string.Equals(a.Name, "uuid", StringComparison.OrdinalIgnoreCase));

        if (uuidAttr != null)
        {
            var g = uuidAttr.GetGuid(0);
            if (g.HasValue)
            {
                guid = g.Value;
                return true;
            }
        }

        return false;
    }

    private static byte GetAlignment(byte fc) => fc switch
    {
        FC.FC_BYTE or FC.FC_CHAR or FC.FC_SMALL or FC.FC_USMALL => 0,
        FC.FC_WCHAR or FC.FC_SHORT or FC.FC_USHORT or FC.FC_ENUM16 => 1,
        FC.FC_LONG or FC.FC_ULONG or FC.FC_FLOAT or FC.FC_ENUM32 or FC.FC_ERROR_STATUS_T => 3,
        FC.FC_HYPER or FC.FC_DOUBLE => 7,
        _ => 0,
    };

    private static int GetSize(byte fc) => fc switch
    {
        FC.FC_BYTE or FC.FC_CHAR or FC.FC_SMALL or FC.FC_USMALL => 1,
        FC.FC_WCHAR or FC.FC_SHORT or FC.FC_USHORT or FC.FC_ENUM16 => 2,
        FC.FC_LONG or FC.FC_ULONG or FC.FC_FLOAT or FC.FC_ENUM32 or FC.FC_ERROR_STATUS_T => 4,
        FC.FC_HYPER or FC.FC_DOUBLE => 8,
        _ => 0,
    };

    private static string GetFormatCharName(byte fc) => fc switch
    {
        FC.FC_BYTE => "FC_BYTE",
        FC.FC_CHAR => "FC_CHAR",
        FC.FC_SMALL => "FC_SMALL",
        FC.FC_USMALL => "FC_USMALL",
        FC.FC_WCHAR => "FC_WCHAR",
        FC.FC_SHORT => "FC_SHORT",
        FC.FC_USHORT => "FC_USHORT",
        FC.FC_LONG => "FC_LONG",
        FC.FC_ULONG => "FC_ULONG",
        FC.FC_FLOAT => "FC_FLOAT",
        FC.FC_HYPER => "FC_HYPER",
        FC.FC_DOUBLE => "FC_DOUBLE",
        FC.FC_ENUM16 => "FC_ENUM16",
        FC.FC_ENUM32 => "FC_ENUM32",
        FC.FC_ERROR_STATUS_T => "FC_ERROR_STATUS_T",
        FC.FC_RP => "FC_RP",
        FC.FC_UP => "FC_UP",
        FC.FC_OP => "FC_OP",
        FC.FC_FP => "FC_FP",
        FC.FC_STRUCT => "FC_STRUCT",
        FC.FC_CSTRUCT => "FC_CSTRUCT",
        FC.FC_CARRAY => "FC_CARRAY",
        FC.FC_SMFARRAY => "FC_SMFARRAY",
        FC.FC_IP => "FC_IP",
        FC.FC_EMBEDDED_COMPLEX => "FC_EMBEDDED_COMPLEX",
        FC.FC_CONSTANT_IID => "FC_CONSTANT_IID",
        FC.FC_END => "FC_END",
        FC.FC_PAD => "FC_PAD",
        FC.FC_USER_MARSHAL => "FC_USER_MARSHAL",
        _ => $"0x{fc:x2}",
    };
}
