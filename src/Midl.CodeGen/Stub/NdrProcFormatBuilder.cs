using System.Text;
using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.CodeGen.Stub;

/// <summary>
/// Builds the NDR procedure format string for proxy/stub generation.
/// Each method in an interface gets a procedure descriptor encoding its parameters,
/// stack layout, and type references into the companion type format string.
/// </summary>
public class NdrProcFormatBuilder
{
    private readonly List<byte> _formatBytes = new();

    // --- Oi flags (byte +1) ---
    private const byte OiObjectProc = 0x20;
    private const byte OiV2Interpreter = 0x40;
    private const byte OiRpcssAllocUsed = 0x04;
    // 0x6c = OiObjectProc | OiV2Interpreter | OiRpcssAllocUsed | extra (0x08 old Oi2)
    private const byte OiFlags = 0x6c;

    // --- Oi2 flags (byte +14) ---
    private const byte Oi2ServerMustSize = 0x01;
    private const byte Oi2ClientMustSize = 0x02;
    private const byte Oi2HasReturn = 0x04;
    private const byte Oi2HasExtensions = 0x40;

    // --- Extension flags (byte +17) ---
    private const byte ExtNewCorrDesc = 0x01;
    private const byte ExtClientCorrCheck = 0x02;
    private const byte ExtServerCorrCheck = 0x04;
    private const byte ExtHasBigByValueParam = 0x80;

    // --- PARAM_ATTRIBUTES bits ---
    private const ushort ParamMustSize = 0x0001;
    private const ushort ParamMustFree = 0x0002;
    private const ushort ParamIsIn = 0x0008;
    private const ushort ParamIsOut = 0x0010;
    private const ushort ParamIsReturn = 0x0020;
    private const ushort ParamIsBasetype = 0x0040;
    private const ushort ParamIsByValue = 0x0080;
    private const ushort ParamIsSimpleRef = 0x0100;
    private const ushort ParamServerAllocSize8 = 0x2000; // server alloc size / 8 = 1 → bits 12:15 = 0010

    private const int StackSlotSize = 8; // x64

    /// <summary>Current byte length of the format string.</summary>
    public int CurrentOffset => _formatBytes.Count;

    /// <summary>
    /// Adds a method descriptor and returns its offset in the proc format string.
    /// </summary>
    public int AddMethod(MethodSymbol method, int methodNumber, NdrTypeFormatBuilder typeBuilder)
    {
        var paramInfos = new List<ParamInfo>();
        int stackOffset = StackSlotSize; // after 'this'

        foreach (var param in method.Parameters)
        {
            paramInfos.Add(BuildParamInfo(param, stackOffset, typeBuilder));
            stackOffset += StackSlotSize;
        }

        // HRESULT return is always present in WinRT
        int returnStackOffset = stackOffset;
        stackOffset += StackSlotSize;

        int totalStackSize = stackOffset;
        int paramCount = paramInfos.Count + 1; // user params + HRESULT return

        // Compute Oi2 flags
        byte oi2Flags = (byte)(Oi2HasReturn | Oi2HasExtensions);
        byte extFlags = ExtNewCorrDesc;
        ushort clientCorrCount = 0;
        ushort serverCorrCount = 0;
        bool hasBigByVal = false;

        // Compute buffer size hints
        int clientBufferSize = 0;
        int serverBufferSize = 0;

        foreach (var pi in paramInfos)
        {
            if (pi.IsComplexIn)
            {
                oi2Flags |= Oi2ClientMustSize;
                if (pi.IsUserMarshalIn)
                    serverCorrCount++;
            }
            if (pi.IsComplexOut)
            {
                oi2Flags |= Oi2ServerMustSize;
                if (pi.IsUserMarshalOut)
                    clientCorrCount++;
            }
            if (pi.IsBigByVal)
                hasBigByVal = true;

            // Buffer size hints for base types
            if (pi.IsBasetype && pi.IsIn)
                clientBufferSize += GetNdrBufferContribution(pi.FcCode);
            if (pi.IsBasetype && pi.IsOut)
                serverBufferSize += GetNdrBufferContribution(pi.FcCode);
        }

        // HRESULT return contributes to server buffer
        serverBufferSize += GetNdrBufferContribution(FC.FC_LONG);

        if (clientCorrCount > 0)
            extFlags |= ExtClientCorrCheck;
        if (serverCorrCount > 0)
            extFlags |= ExtServerCorrCheck;
        if (hasBigByVal)
            extFlags |= ExtHasBigByValueParam;

        // --- Emit procedure header (26 bytes with extensions) ---
        int offset = _formatBytes.Count;

        // Byte 0: handle type
        _formatBytes.Add(0x33); // FC_AUTO_HANDLE
        // Byte 1: old Oi flags
        _formatBytes.Add(OiFlags);
        // Bytes 2-5: RPC flags (4 bytes LE)
        EmitLong(0x00000000);
        // Bytes 6-7: method number
        EmitShort((short)methodNumber);
        // Bytes 8-9: stack size
        EmitShort((short)totalStackSize);
        // Bytes 10-11: client buffer size hint
        EmitShort((short)clientBufferSize);
        // Bytes 12-13: server buffer size hint
        EmitShort((short)serverBufferSize);
        // Byte 14: Oi2 flags
        _formatBytes.Add(oi2Flags);
        // Byte 15: param count (including HRESULT return, excluding 'this')
        _formatBytes.Add((byte)paramCount);

        // Extension block (10 bytes)
        // Byte 16: extension size
        _formatBytes.Add(0x0a);
        // Byte 17: extension flags
        _formatBytes.Add(extFlags);
        // Bytes 18-19: client correlation count
        EmitShort((short)clientCorrCount);
        // Bytes 20-21: server correlation count
        EmitShort((short)serverCorrCount);
        // Bytes 22-23: notify table offset
        EmitShort(0);
        // Bytes 24-25: float/double mask (x64)
        EmitShort(ComputeFloatMask(method.Parameters));

        // --- Emit parameter descriptors ---
        foreach (var pi in paramInfos)
        {
            EmitShort((short)pi.Flags);
            EmitShort((short)pi.StackOffset);

            if (pi.IsBasetype)
            {
                _formatBytes.Add(pi.FcCode);
                _formatBytes.Add(0x00); // padding
            }
            else
            {
                EmitShort((short)pi.TypeOffset);
            }
        }

        // HRESULT return descriptor
        EmitShort((short)(ParamIsOut | ParamIsReturn | ParamIsBasetype)); // 0x0070
        EmitShort((short)returnStackOffset);
        _formatBytes.Add(FC.FC_LONG);
        _formatBytes.Add(0x00);

        return offset;
    }

    /// <summary>
    /// Adds property accessor method(s) and returns the offset of the first method.
    /// Increments methodNumber for each method added.
    /// </summary>
    public int AddProperty(PropertySymbol prop, ref int methodNumber, NdrTypeFormatBuilder typeBuilder)
    {
        int firstOffset = -1;

        if (prop.HasGetter)
        {
            // get_PropertyName: HRESULT get_Name([out, retval] Type* value)
            var getter = new MethodSymbol($"get_{prop.Name}", prop.ContainingSymbol)
            {
                ReturnType = new TypeRef("HRESULT"),
            };
            var outParam = new ParameterSymbol("value", getter)
            {
                Type = prop.Type,
                Direction = ParameterDirection.Out,
            };
            outParam.Attributes.Add(new AttributeData { Name = "retval" });
            getter.Parameters.Add(outParam);

            firstOffset = AddMethod(getter, methodNumber++, typeBuilder);
        }

        if (prop.HasSetter)
        {
            // put_PropertyName: HRESULT put_Name([in] Type value)
            var setter = new MethodSymbol($"put_{prop.Name}", prop.ContainingSymbol)
            {
                ReturnType = new TypeRef("HRESULT"),
            };
            setter.Parameters.Add(new ParameterSymbol("value", setter)
            {
                Type = prop.Type,
                Direction = ParameterDirection.In,
            });

            int setterOffset = AddMethod(setter, methodNumber++, typeBuilder);
            if (firstOffset < 0) firstOffset = setterOffset;
        }

        return firstOffset;
    }

    /// <summary>
    /// Adds event add/remove methods and returns the offset of the first method.
    /// Increments methodNumber for each method added.
    /// </summary>
    public int AddEvent(EventSymbol evt, ref int methodNumber, NdrTypeFormatBuilder typeBuilder)
    {
        // add_EventName: HRESULT add_Event([in] DelegateType* handler, [out, retval] EventRegistrationToken* token)
        var addMethod = new MethodSymbol($"add_{evt.Name}", evt.ContainingSymbol)
        {
            ReturnType = new TypeRef("HRESULT"),
        };
        addMethod.Parameters.Add(new ParameterSymbol("handler", addMethod)
        {
            Type = evt.DelegateType,
            Direction = ParameterDirection.In,
        });
        var tokenOutParam = new ParameterSymbol("token", addMethod)
        {
            Type = new TypeRef("Int64"), // EventRegistrationToken is 8 bytes
            Direction = ParameterDirection.Out,
        };
        tokenOutParam.Attributes.Add(new AttributeData { Name = "retval" });
        addMethod.Parameters.Add(tokenOutParam);

        int firstOffset = AddMethod(addMethod, methodNumber++, typeBuilder);

        // remove_EventName: HRESULT remove_Event([in] EventRegistrationToken token)
        var removeMethod = new MethodSymbol($"remove_{evt.Name}", evt.ContainingSymbol)
        {
            ReturnType = new TypeRef("HRESULT"),
        };
        removeMethod.Parameters.Add(new ParameterSymbol("token", removeMethod)
        {
            Type = new TypeRef("Int64"), // EventRegistrationToken
            Direction = ParameterDirection.In,
        });

        AddMethod(removeMethod, methodNumber++, typeBuilder);

        return firstOffset;
    }

    /// <summary>
    /// Returns the raw format bytes (without terminator).
    /// </summary>
    public byte[] Build()
    {
        return _formatBytes.ToArray();
    }

    /// <summary>
    /// Emits the C source code for the MIDL_PROC_FORMAT_STRING structure.
    /// </summary>
    public string EmitProcFormatString(string filePrefix)
    {
        var bytes = _formatBytes.ToList();
        bytes.Add(0x00); // terminator

        var sb = new StringBuilder();

        sb.AppendLine($"static const {filePrefix}_MIDL_PROC_FORMAT_STRING {filePrefix}__MIDL_ProcFormatString =");
        sb.AppendLine("    {");
        sb.AppendLine("        0,");
        sb.AppendLine("        {");

        for (int i = 0; i < bytes.Count; i++)
        {
            if (i == bytes.Count - 1 && bytes[i] == 0x00)
            {
                sb.AppendLine();
                sb.AppendLine("\t\t\t0x0");
                continue;
            }

            byte b = bytes[i];
            sb.AppendLine($"/* {i,3} */\t0x{b:x2},\t\t/* {GetByteComment(b, i)} */");
        }

        sb.AppendLine("        }");
        sb.AppendLine("    };");

        return sb.ToString();
    }

    #region Private helpers

    private struct ParamInfo
    {
        public ushort Flags;
        public int StackOffset;
        public byte FcCode;
        public int TypeOffset;
        public bool IsBasetype;
        public bool IsIn;
        public bool IsOut;
        public bool IsComplexIn;
        public bool IsComplexOut;
        public bool IsUserMarshalIn;
        public bool IsUserMarshalOut;
        public bool IsBigByVal;
    }

    private ParamInfo BuildParamInfo(ParameterSymbol param, int stackOffset, NdrTypeFormatBuilder typeBuilder)
    {
        bool isOut = param.Direction == ParameterDirection.Out
            || param.Attributes.Any(a => a.Name == "out");
        bool isIn = !isOut || param.Direction == ParameterDirection.In
            || param.Attributes.Any(a => a.Name == "in");
        if (!isOut) isIn = true; // default to [in]

        bool isRetval = param.Attributes.Any(a => a.Name == "retval");

        var info = new ParamInfo
        {
            StackOffset = stackOffset,
            IsIn = isIn && !isOut,
            IsOut = isOut,
        };

        var primFc = NdrTypeFormatBuilder.GetPrimitiveFormatChar(param.Type.Name);
        bool isEnum = param.Type.ResolvedSymbol is EnumSymbol;
        if (isEnum)
            primFc = FC.FC_ENUM32;

        if (primFc != null)
        {
            info.FcCode = primFc.Value;
            info.IsBasetype = true;

            if (isOut)
            {
                // [out] base type pointer: e.g., [out, retval] Int32* result
                info.Flags = ParamIsOut | ParamIsBasetype | ParamIsSimpleRef | ParamServerAllocSize8;
                // 0x2150
                // Also ensure the ref pointer is in the type format string
                typeBuilder.EmitRefPointerToBaseType(primFc.Value);
            }
            else
            {
                // [in] base type by value
                info.Flags = ParamIsIn | ParamIsBasetype;
                // 0x0048
            }
        }
        else
        {
            info.IsBasetype = false;
            string typeName = param.Type.Name;

            if (typeName == "String")
            {
                if (isOut)
                {
                    // [out] HSTRING*: MustSize | MustFree | IsOut | IsSimpleRef | ServerAllocSize8
                    info.Flags = ParamMustSize | ParamMustFree | ParamIsOut | ParamIsSimpleRef | ParamServerAllocSize8;
                    // 0x2113
                    info.TypeOffset = typeBuilder.EmitHStringOutType();
                    info.IsComplexOut = true;
                    info.IsUserMarshalOut = true;
                }
                else
                {
                    // [in] HSTRING by value: MustSize | MustFree | IsIn | IsByValue
                    info.Flags = ParamMustSize | ParamMustFree | ParamIsIn | ParamIsByValue;
                    // 0x008b
                    info.TypeOffset = typeBuilder.GetOrAddType(param.Type);
                    info.IsComplexIn = true;
                    info.IsUserMarshalIn = true;
                }
            }
            else if (param.Type.ResolvedSymbol is InterfaceSymbol or DelegateSymbol)
            {
                if (isOut)
                {
                    // [out] IFoo**: MustSize | MustFree | IsOut
                    info.Flags = ParamMustSize | ParamMustFree | ParamIsOut;
                    // 0x0013
                    info.TypeOffset = typeBuilder.EmitRefPointerDeref(param.Type);
                    info.IsComplexOut = true;
                }
                else
                {
                    // [in] IFoo*: MustSize | MustFree | IsIn | IsSimpleRef
                    info.Flags = ParamMustSize | ParamMustFree | ParamIsIn | ParamIsSimpleRef;
                    info.TypeOffset = typeBuilder.GetOrAddType(param.Type);
                    info.IsComplexIn = true;
                }
            }
            else if (typeName == "Guid" || param.Type.ResolvedSymbol is StructSymbol)
            {
                if (isOut)
                {
                    // [out] struct*
                    info.Flags = ParamMustFree | ParamIsOut | ParamIsSimpleRef | ParamServerAllocSize8;
                    info.TypeOffset = typeBuilder.EmitRefPointerToStruct(param.Type);
                    info.IsComplexOut = true;
                }
                else
                {
                    // [in] GUID/struct by ref: MustFree | IsIn | IsSimpleRef
                    info.Flags = ParamMustFree | ParamIsIn | ParamIsSimpleRef;
                    // 0x010a
                    info.TypeOffset = typeBuilder.GetOrAddType(param.Type);
                    info.IsComplexIn = true;
                    info.IsBigByVal = true;
                }
            }
            else
            {
                // Fallback: treat as complex [in] type
                info.Flags = isOut
                    ? (ushort)(ParamMustSize | ParamMustFree | ParamIsOut)
                    : (ushort)(ParamMustSize | ParamMustFree | ParamIsIn);
                info.TypeOffset = typeBuilder.GetOrAddType(param.Type);
                if (isOut) info.IsComplexOut = true;
                else info.IsComplexIn = true;
            }
        }

        return info;
    }

    /// <summary>
    /// Returns the NDR buffer contribution for a base type (size + padding to 4-byte alignment).
    /// </summary>
    private static int GetNdrBufferContribution(byte fc)
    {
        int size = fc switch
        {
            FC.FC_BYTE or FC.FC_CHAR or FC.FC_SMALL or FC.FC_USMALL => 1,
            FC.FC_WCHAR or FC.FC_SHORT or FC.FC_USHORT or FC.FC_ENUM16 => 2,
            FC.FC_LONG or FC.FC_ULONG or FC.FC_FLOAT or FC.FC_ENUM32 or FC.FC_ERROR_STATUS_T => 4,
            FC.FC_HYPER or FC.FC_DOUBLE => 8,
            _ => 4,
        };
        // Pad to 4-byte boundary + 4 for NDR marshaling overhead
        return ((size + 3) & ~3) + 4;
    }

    /// <summary>
    /// Computes the x64 float/double mask for the first 3 register params.
    /// Bit N is set if param N (0-indexed from first user param) is float/double.
    /// </summary>
    private static short ComputeFloatMask(IReadOnlyList<ParameterSymbol> parameters)
    {
        short mask = 0;
        for (int i = 0; i < parameters.Count && i < 3; i++)
        {
            var fc = NdrTypeFormatBuilder.GetPrimitiveFormatChar(parameters[i].Type.Name);
            if (fc == FC.FC_FLOAT || fc == FC.FC_DOUBLE)
                mask |= (short)(1 << i);
        }
        return mask;
    }

    private void EmitShort(short value)
    {
        _formatBytes.Add((byte)(value & 0xFF));
        _formatBytes.Add((byte)((value >> 8) & 0xFF));
    }

    private void EmitLong(int value)
    {
        _formatBytes.Add((byte)(value & 0xFF));
        _formatBytes.Add((byte)((value >> 8) & 0xFF));
        _formatBytes.Add((byte)((value >> 16) & 0xFF));
        _formatBytes.Add((byte)((value >> 24) & 0xFF));
    }

    private static string GetByteComment(byte b, int offset) => b switch
    {
        0x33 => "FC_AUTO_HANDLE",
        0x6c => "Old Flags: object, Oi2",
        FC.FC_LONG => "FC_LONG",
        FC.FC_SMALL => "FC_SMALL",
        FC.FC_BYTE => "FC_BYTE",
        FC.FC_WCHAR => "FC_WCHAR",
        FC.FC_SHORT => "FC_SHORT",
        FC.FC_USHORT => "FC_USHORT",
        FC.FC_ULONG => "FC_ULONG",
        FC.FC_FLOAT => "FC_FLOAT",
        FC.FC_HYPER => "FC_HYPER",
        FC.FC_DOUBLE => "FC_DOUBLE",
        _ => $"0x{b:x2}",
    };

    #endregion
}
