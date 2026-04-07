using System.Text;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Proxy;

/// <summary>
/// Shared wire-format helpers for proxy and stub code generation.
/// Both ProxyGenerator and StubGenerator MUST use these methods to ensure
/// they agree on serialization layout, type sizes, and reply ordering.
/// </summary>
public static partial class WireFormat
{
    /// <summary>Size of an HRESULT on the wire (always 4 bytes).</summary>
    public const int HResultSize = 4;

    /// <summary>
    /// Returns a TypeRef representing the element type of an array TypeRef.
    /// The element type has the same name and resolved symbol but IsArray = false.
    /// </summary>
    public static TypeRef ElementType(TypeRef arrayType)
    {
        return new TypeRef(arrayType.Name, arrayType.TypeArguments)
        {
            ResolvedSymbol = arrayType.ResolvedSymbol,
            IsBuiltIn = arrayType.IsBuiltIn,
        };
    }

    private static readonly Dictionary<string, int> WireSizeMap = new(StringComparer.Ordinal)
    {
        ["Boolean"] = 1,
        ["UInt8"] = 1,
        ["Char"] = 2,
        ["Int16"] = 2,
        ["UInt16"] = 2,
        ["Int32"] = 4,
        ["UInt32"] = 4,
        ["Single"] = 4,
        ["HRESULT"] = 4,
        ["Int64"] = 8,
        ["UInt64"] = 8,
        ["Double"] = 8,
        ["Guid"] = 16,
        ["EventRegistrationToken"] = 8,
    };

    /// <summary>
    /// Set of primitive type names that are blittable (directly memcpy-able).
    /// Strings are variable-length. Enums are handled separately in IsFieldBlittable.
    /// </summary>
    private static readonly HashSet<string> BlittablePrimitives = new(StringComparer.Ordinal)
    {
        "Boolean", "UInt8", "Char", "Int16", "UInt16",
        "Int32", "UInt32", "Single", "Int64", "UInt64",
        "Double", "Guid", "EventRegistrationToken",
    };

    /// <summary>
    /// Returns the fixed wire size in bytes for a type.
    /// For structs, recursively sums field sizes.
    /// </summary>
    public static int GetWireSize(TypeRef typeRef)
    {
        if (WireSizeMap.TryGetValue(typeRef.Name, out var size))
            return size;
        if (typeRef.ResolvedSymbol is EnumSymbol)
            return 4;
        if (typeRef.ResolvedSymbol is StructSymbol structSym)
            return structSym.Fields.Sum(f => GetWireSize(f.Type));
        return 4; // default fallback
    }

    /// <summary>
    /// Returns a C++ expression string for the wire size of a type.
    /// For HSTRING, returns a dynamic expression involving the string length.
    /// </summary>
    public static string GetWireSizeExpression(TypeRef typeRef)
    {
        if (typeRef.Name == "String")
            return "sizeof(UINT32) + len * sizeof(WCHAR) + sizeof(WCHAR)";
        if (IsInterfaceType(typeRef))
            return "marshalSize";
        if (typeRef.ResolvedSymbol is StructSymbol structSym)
            return string.Join(" + ", structSym.Fields.Select(f => GetCppSizeExpression(f.Type)));
        return GetCppSizeExpression(typeRef);
    }

    /// <summary>
    /// Returns a C++ size expression for a type, suitable for use in buffer size computation.
    /// For HSTRING, includes a placeholder {0} for the variable name.
    /// </summary>
    public static string GetTypeSizeExpression(TypeRef type)
    {
        if (type.IsArray)
        {
            var elemType = ElementType(type);
            if (elemType.Name == "String")
                return "winmidl::hstring_array_size({0}, __{0}Size)";
            return "sizeof(UINT32) + __{0}Size * " + GetCppSizeExpression(elemType);
        }
        if (type.Name == "String")
            return "sizeof(UINT32) + winmidl::hstring_len({0}) * sizeof(WCHAR) + sizeof(WCHAR)";
        if (IsInterfaceType(type))
            return "{0}_marshalSize";
        if (type.ResolvedSymbol is StructSymbol structSym)
            return string.Join(" + ", structSym.Fields.Select(f => GetCppSizeExpression(f.Type)));
        return GetCppSizeExpression(type);
    }

    /// <summary>
    /// Returns the C++ sizeof(...) expression for a type, suitable for use in memcpy calls
    /// and constexpr buffer size computations.
    /// </summary>
    public static string GetCppSizeExpression(TypeRef typeRef)
    {
        return typeRef.Name switch
        {
            "Boolean" => "sizeof(boolean)",
            "UInt8" => "sizeof(BYTE)",
            "Char" => "sizeof(WCHAR)",
            "Int16" => "sizeof(INT16)",
            "UInt16" => "sizeof(UINT16)",
            "Int32" => "sizeof(INT32)",
            "UInt32" => "sizeof(UINT32)",
            "Single" => "sizeof(FLOAT)",
            "HRESULT" => "sizeof(HRESULT)",
            "Int64" => "sizeof(INT64)",
            "UInt64" => "sizeof(UINT64)",
            "Double" => "sizeof(DOUBLE)",
            "Guid" => "sizeof(GUID)",
            "EventRegistrationToken" => "sizeof(EventRegistrationToken)",
            _ when typeRef.ResolvedSymbol is EnumSymbol => "sizeof(INT32)",
            _ when typeRef.ResolvedSymbol is StructSymbol s && !IsBlittable(s) =>
                string.Join(" + ", s.Fields.Select(f => GetCppSizeExpression(f.Type))),
            _ => $"sizeof({typeRef.Name})",
        };
    }

    /// <summary>
    /// Computes the C++ expression for the total buffer size needed for a list of parameters.
    /// </summary>
    public static string ComputeBufferSize(List<ProxyParam> inParams)
    {
        if (inParams.Count == 0)
            return "0";

        var parts = new List<string>();
        foreach (var p in inParams)
        {
            if (p.Type.IsArray)
            {
                var elemType = ElementType(p.Type);
                if (elemType.Name == "String")
                    parts.Add($"winmidl::hstring_array_size({p.Name}, __{p.Name}Size)");
                else
                    parts.Add($"sizeof(UINT32) + __{p.Name}Size * {GetCppSizeExpression(elemType)}");
            }
            else
            {
                var expr = GetTypeSizeExpression(p.Type);
                if (expr.Contains("{0}"))
                    expr = string.Format(expr, p.Name);
                parts.Add(expr);
            }
        }

        return string.Join(" + ", parts);
    }

    /// <summary>
    /// Returns the first user vtable slot index.
    /// IInspectable-derived: 6 (IUnknown 0-2, IInspectable 3-5).
    /// IUnknown-only: 3 (IUnknown 0-2).
    /// </summary>
    public static int GetFirstUserSlot(bool isInspectable)
    {
        return isInspectable ? 6 : 3;
    }

    /// <summary>
    /// Emits C++ code to serialize a value into a buffer using winmidl:: helper functions.
    /// Assumes a BYTE* variable named <paramref name="bufferPtr"/> is in scope and will be advanced.
    /// </summary>
    public static void EmitSerialize(StringBuilder sb, TypeRef typeRef, string varName, string bufferPtr, string indent = "        ")
    {
        if (typeRef.IsArray)
        {
            var elemType = ElementType(typeRef);
            var sizeVar = $"__{varName}Size";
            if (elemType.Name == "String")
                sb.AppendLine($"{indent}winmidl::marshal_hstring_array({bufferPtr}, {varName}, {sizeVar});");
            else
                sb.AppendLine($"{indent}winmidl::marshal_array({bufferPtr}, {varName}, {sizeVar});");
            return;
        }

        var helper = GetMarshalHelper(typeRef);
        if (helper != null)
        {
            sb.AppendLine($"{indent}winmidl::{helper}({bufferPtr}, {varName});");
            return;
        }

        if (typeRef.ResolvedSymbol is StructSymbol structSym)
        {
            if (IsBlittable(structSym))
            {
                sb.AppendLine($"{indent}winmidl::marshal_struct({bufferPtr}, {varName});");
            }
            else
            {
                foreach (var field in structSym.Fields)
                {
                    EmitSerialize(sb, field.Type, $"{varName}.{field.Name}", bufferPtr, indent);
                }
            }
            return;
        }

        if (IsInterfaceType(typeRef))
        {
            var iidName = GetInterfaceIidName(typeRef);
            sb.AppendLine($"{indent}hr = winmidl::marshal_interface({bufferPtr}, {varName}, {iidName}); RETURN_IF_FAILED(hr);");
            return;
        }

        // Fallback for unknown fixed-size types
        var sizeExpr = GetCppSizeExpression(typeRef);
        sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}, {sizeExpr}); {bufferPtr} += {sizeExpr};");
    }

    /// <summary>
    /// Returns the winmidl:: marshal helper function name for a type, or null if none.
    /// </summary>
    public static string? GetMarshalHelper(TypeRef typeRef)
    {
        return typeRef.Name switch
        {
            "Boolean" => "marshal_bool",
            "UInt8" => "marshal_uint8",
            "Char" => "marshal_char16",
            "Int16" => "marshal_int16",
            "UInt16" => "marshal_uint16",
            "Int32" => "marshal_int32",
            "UInt32" => "marshal_uint32",
            "Single" => "marshal_float",
            "Int64" => "marshal_int64",
            "UInt64" => "marshal_uint64",
            "Double" => "marshal_double",
            "Guid" => "marshal_guid",
            "String" => "marshal_hstring",
            "EventRegistrationToken" => "marshal_event_token",
            _ when typeRef.ResolvedSymbol is EnumSymbol => "marshal_enum",
            _ => null,
        };
    }

    /// <summary>
    /// Returns the winmidl:: unmarshal helper function name for a type, or null if none.
    /// </summary>
    public static string? GetUnmarshalHelper(TypeRef typeRef)
    {
        return typeRef.Name switch
        {
            "Boolean" => "unmarshal_bool",
            "UInt8" => "unmarshal_uint8",
            "Char" => "unmarshal_char16",
            "Int16" => "unmarshal_int16",
            "UInt16" => "unmarshal_uint16",
            "Int32" => "unmarshal_int32",
            "UInt32" => "unmarshal_uint32",
            "Single" => "unmarshal_float",
            "Int64" => "unmarshal_int64",
            "UInt64" => "unmarshal_uint64",
            "Double" => "unmarshal_double",
            "Guid" => "unmarshal_guid",
            "EventRegistrationToken" => "unmarshal_event_token",
            _ => null,
        };
    }

    /// <summary>
    /// Returns the winmidl::param_type enum value for a TypeRef, for method descriptor tables.
    /// </summary>
    public static string? GetParamTypeEnum(TypeRef typeRef)
    {
        return typeRef.Name switch
        {
            "Boolean" => "winmidl::param_type::t_bool",
            "UInt8" => "winmidl::param_type::t_uint8",
            "Char" => "winmidl::param_type::t_char16",
            "Int16" => "winmidl::param_type::t_int16",
            "UInt16" => "winmidl::param_type::t_uint16",
            "Int32" => "winmidl::param_type::t_int32",
            "UInt32" => "winmidl::param_type::t_uint32",
            "Single" => "winmidl::param_type::t_float",
            "Int64" => "winmidl::param_type::t_int64",
            "UInt64" => "winmidl::param_type::t_uint64",
            "Double" => "winmidl::param_type::t_double",
            "Guid" => "winmidl::param_type::t_guid",
            "String" => "winmidl::param_type::t_hstring",
            "EventRegistrationToken" => "winmidl::param_type::t_event_token",
            _ when typeRef.ResolvedSymbol is EnumSymbol => "winmidl::param_type::t_enum",
            _ when typeRef.ResolvedSymbol is StructSymbol => "winmidl::param_type::t_struct",
            _ when typeRef.ResolvedSymbol is InterfaceSymbol or DelegateSymbol => "winmidl::param_type::t_interface",
            _ => null,
        };
    }

    /// <summary>
    /// Emits C++ code to deserialize a value from a buffer into an existing variable.
    /// Uses winmidl:: helper functions. Used by StubGenerator for deserializing input parameters.
    /// </summary>
    public static void EmitDeserialize(StringBuilder sb, TypeRef typeRef, string varName, string bufferPtr, string indent = "        ")
    {
        if (typeRef.IsArray)
        {
            var elemType = ElementType(typeRef);
            var elemCType = GetCppTypeName(elemType);
            var sizeVar = $"__{varName}Size";
            sb.AppendLine($"{indent}UINT32 {sizeVar};");
            sb.AppendLine($"{indent}{elemCType}* {varName};");
            if (elemType.Name == "String")
                sb.AppendLine($"{indent}winmidl::unmarshal_hstring_array({bufferPtr}, &{varName}, &{sizeVar});");
            else
                sb.AppendLine($"{indent}winmidl::unmarshal_array({bufferPtr}, &{varName}, &{sizeVar});");
            return;
        }

        if (typeRef.Name == "String")
        {
            // HSTRING deserialization is handled specially in StubGenerator
            // (zero-copy ref vs allocating depending on async context)
            EmitDeserializeHString(sb, varName, bufferPtr, indent);
            return;
        }

        var helper = GetUnmarshalHelper(typeRef);
        if (helper != null)
        {
            sb.AppendLine($"{indent}winmidl::{helper}({bufferPtr}, {varName});");
            return;
        }

        if (typeRef.ResolvedSymbol is EnumSymbol)
        {
            sb.AppendLine($"{indent}winmidl::unmarshal_enum({bufferPtr}, {varName});");
            return;
        }

        if (typeRef.ResolvedSymbol is StructSymbol structSym)
        {
            if (IsBlittable(structSym))
            {
                sb.AppendLine($"{indent}winmidl::unmarshal_struct({bufferPtr}, {varName});");
            }
            else
            {
                foreach (var field in structSym.Fields)
                {
                    EmitDeserialize(sb, field.Type, $"{varName}.{field.Name}", bufferPtr, indent);
                }
            }
            return;
        }

        if (IsInterfaceType(typeRef))
        {
            var iidName = GetInterfaceIidName(typeRef);
            sb.AppendLine($"{indent}hr = winmidl::unmarshal_interface({bufferPtr}, {iidName}, (void**)&{varName}); RETURN_IF_FAILED(hr);");
            return;
        }

        // Fallback for unknown types
        var sizeExpr = GetCppSizeExpression(typeRef);
        sb.AppendLine($"{indent}memcpy(&{varName}, {bufferPtr}, {sizeExpr}); {bufferPtr} += {sizeExpr};");
    }

    /// <summary>
    /// Emits C++ code to deserialize a value from the reply buffer into a pointer target.
    /// Used by ProxyGenerator for deserializing reply out-params and retvals.
    /// </summary>
    public static void EmitDeserializeReply(StringBuilder sb, TypeRef typeRef, string targetName, string bufferPtr, bool isPointer, string indent = "        ")
    {
        if (typeRef.IsArray)
        {
            var elemType = ElementType(typeRef);
            var sizeTarget = $"__{targetName}Size";
            if (elemType.Name == "String")
                sb.AppendLine($"{indent}hr = winmidl::unmarshal_hstring_array({bufferPtr}, {targetName}, {sizeTarget}); RETURN_IF_FAILED(hr);");
            else
                sb.AppendLine($"{indent}hr = winmidl::unmarshal_array({bufferPtr}, {targetName}, {sizeTarget}); RETURN_IF_FAILED(hr);");
            return;
        }

        if (typeRef.Name == "String")
        {
            sb.AppendLine($"{indent}hr = winmidl::unmarshal_hstring({bufferPtr}, {(isPointer ? targetName : $"&{targetName}")}); RETURN_IF_FAILED(hr);");
            return;
        }

        var helper = GetUnmarshalHelper(typeRef);
        if (helper != null)
        {
            sb.AppendLine($"{indent}winmidl::{helper}({bufferPtr}, {(isPointer ? $"*{targetName}" : targetName)});");
            return;
        }

        if (typeRef.ResolvedSymbol is EnumSymbol enumSym)
        {
            var enumName = enumSym.Name;
            sb.AppendLine($"{indent}winmidl::unmarshal_enum({bufferPtr}, {(isPointer ? $"*{targetName}" : targetName)});");
            return;
        }

        if (typeRef.ResolvedSymbol is StructSymbol structSym)
        {
            if (IsBlittable(structSym))
            {
                sb.AppendLine($"{indent}winmidl::unmarshal_struct({bufferPtr}, {(isPointer ? $"*{targetName}" : targetName)});");
            }
            else
            {
                var prefix = isPointer ? $"{targetName}->" : $"{targetName}.";
                foreach (var field in structSym.Fields)
                {
                    var fieldTarget = $"{prefix}{field.Name}";
                    EmitDeserializeReply(sb, field.Type, fieldTarget, bufferPtr, isPointer: false, indent);
                }
            }
            return;
        }

        if (IsInterfaceType(typeRef))
        {
            var iidName = GetInterfaceIidName(typeRef);
            sb.AppendLine($"{indent}hr = winmidl::unmarshal_interface({bufferPtr}, {iidName}, (void**){(isPointer ? targetName : $"&{targetName}")}); RETURN_IF_FAILED(hr);");
            return;
        }

        var sizeExpr2 = GetCppSizeExpression(typeRef);
        sb.AppendLine($"{indent}memcpy({(isPointer ? targetName : $"&{targetName}")}, {bufferPtr}, {sizeExpr2}); {bufferPtr} += {sizeExpr2};");
    }

    /// <summary>
    /// Emits C++ code to deserialize a FillArray (ref T[]) reply on the proxy side.
    /// Reads actual count + elements from the reply buffer into the caller's pre-allocated buffer.
    /// Wire format: [UINT32 actual][T × actual]
    /// </summary>
    public static void EmitDeserializeFillArrayReply(StringBuilder sb, TypeRef typeRef, string bufferName, string bufferPtr, string indent = "        ", string? actualPtrOverride = null)
    {
        var elemType = ElementType(typeRef);
        var actualPtr = actualPtrOverride ?? $"__{bufferName}Actual";
        var elemCType = GetCppTypeName(elemType);

        // Read actual count
        sb.AppendLine($"{indent}UINT32 __{bufferName}_actual_temp; memcpy(&__{bufferName}_actual_temp, {bufferPtr}, sizeof(UINT32)); {bufferPtr} += sizeof(UINT32);");
        sb.AppendLine($"{indent}*{actualPtr} = __{bufferName}_actual_temp;");

        if (elemType.Name == "String")
        {
            // Unmarshal each HSTRING into the caller's buffer
            sb.AppendLine($"{indent}for (UINT32 i = 0; i < __{bufferName}_actual_temp; i++) {{");
            sb.AppendLine($"{indent}    winmidl::unmarshal_hstring_alloc({bufferPtr}, &{bufferName}[i]);");
            sb.AppendLine($"{indent}}}");
        }
        else
        {
            // memcpy blittable elements into caller's buffer
            var sizeExpr = GetCppSizeExpression(elemType);
            sb.AppendLine($"{indent}memcpy({bufferName}, {bufferPtr}, __{bufferName}_actual_temp * {sizeExpr}); {bufferPtr} += __{bufferName}_actual_temp * {sizeExpr};");
        }
    }

    /// <summary>
    /// Emits C++ code to serialize an HSTRING value (length-prefixed UTF-16).
    /// Wire format: [uint32_t char_count][wchar_t chars * char_count]
    /// </summary>
    public static void EmitSerializeHString(StringBuilder sb, string varName, string bufferPtr, string indent = "        ")
    {
        var safeName = SanitizeVarName(varName);
        sb.AppendLine($"{indent}UINT32 {safeName}_len = winmidl::hstring_len({varName});");
        sb.AppendLine($"{indent}memcpy({bufferPtr}, &{safeName}_len, sizeof(UINT32)); {bufferPtr} += sizeof(UINT32);");
        sb.AppendLine($"{indent}PCWSTR {safeName}_raw = WindowsGetStringRawBuffer({varName}, nullptr);");
        sb.AppendLine($"{indent}memcpy({bufferPtr}, {safeName}_raw, {safeName}_len * sizeof(WCHAR)); {bufferPtr} += {safeName}_len * sizeof(WCHAR);");
    }

    /// <summary>
    /// Emits C++ code to deserialize an HSTRING value (length-prefixed UTF-16).
    /// Uses zero-copy WindowsCreateStringReference. Declares the HSTRING variable.
    /// </summary>
    public static void EmitDeserializeHString(StringBuilder sb, string varName, string bufferPtr, string indent = "        ")
    {
        sb.AppendLine($"{indent}UINT32 {varName}_len;");
        sb.AppendLine($"{indent}memcpy(&{varName}_len, {bufferPtr}, sizeof(UINT32)); {bufferPtr} += sizeof(UINT32);");
        sb.AppendLine($"{indent}HSTRING {varName} = nullptr;");
        sb.AppendLine($"{indent}HSTRING_HEADER {varName}_header;");
        sb.AppendLine($"{indent}if ({varName}_len > 0) {{");
        sb.AppendLine($"{indent}    WindowsCreateStringReference((PCWSTR){bufferPtr}, {varName}_len, &{varName}_header, &{varName});");
        sb.AppendLine($"{indent}}}");
        sb.AppendLine($"{indent}{bufferPtr} += {varName}_len * sizeof(WCHAR);");
    }

    /// <summary>
    /// Emits C++ code to serialize an HSTRING for reply (using WindowsGetStringRawBuffer).
    /// Used by StubGenerator for serializing HSTRING out params or return values.
    /// Wire format: [uint32_t char_count][wchar_t chars * char_count]
    /// </summary>
    public static void EmitSerializeHStringReply(StringBuilder sb, string varName, string bufferPtr, string indent = "        ")
    {
        sb.AppendLine($"{indent}UINT32 {varName}_len = 0;");
        sb.AppendLine($"{indent}PCWSTR {varName}_raw = WindowsGetStringRawBuffer({varName}, &{varName}_len);");
        sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}_len, sizeof(UINT32)); {bufferPtr} += sizeof(UINT32);");
        sb.AppendLine($"{indent}memcpy({bufferPtr}, {varName}_raw, {varName}_len * sizeof(WCHAR)); {bufferPtr} += {varName}_len * sizeof(WCHAR);");
    }

    /// <summary>
    /// Emits C++ serialization for a value in the reply buffer using winmidl:: helpers.
    /// </summary>
    public static void EmitSerializeReplyValue(StringBuilder sb, TypeRef typeRef, string varName, string bufferPtr, string indent = "        ")
    {
        if (typeRef.IsArray)
        {
            var elemType = ElementType(typeRef);
            var sizeVar = $"__{varName}Size";
            if (elemType.Name == "String")
                sb.AppendLine($"{indent}winmidl::marshal_hstring_array({bufferPtr}, {varName}, {sizeVar});");
            else
                sb.AppendLine($"{indent}winmidl::marshal_array({bufferPtr}, {varName}, {sizeVar});");
            return;
        }

        var helper = GetMarshalHelper(typeRef);
        if (helper != null)
        {
            sb.AppendLine($"{indent}winmidl::{helper}({bufferPtr}, {varName});");
            return;
        }

        if (typeRef.ResolvedSymbol is StructSymbol structSym)
        {
            if (IsBlittable(structSym))
            {
                sb.AppendLine($"{indent}winmidl::marshal_struct({bufferPtr}, {varName});");
            }
            else
            {
                foreach (var field in structSym.Fields)
                {
                    EmitSerializeReplyValue(sb, field.Type, $"{varName}.{field.Name}", bufferPtr, indent);
                }
            }
            return;
        }

        if (typeRef.ResolvedSymbol is EnumSymbol)
        {
            sb.AppendLine($"{indent}winmidl::marshal_enum({bufferPtr}, {varName});");
            return;
        }

        if (IsInterfaceType(typeRef))
        {
            var iidName = GetInterfaceIidName(typeRef);
            sb.AppendLine($"{indent}hr = winmidl::marshal_interface({bufferPtr}, {varName}, {iidName}); RETURN_IF_FAILED(hr);");
            return;
        }

        // Fallback for unknown types
        var sizeExpr = GetCppSizeExpression(typeRef);
        sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}, {sizeExpr}); {bufferPtr} += {sizeExpr};");
    }

    /// <summary>
    /// Builds the C++ expression for the reply buffer size.
    /// Reply layout: [out params in declaration order][retval][HRESULT]
    /// </summary>
    public static string BuildReplySizeExpression(List<(TypeRef Type, string Name)> outParams, bool hasRetval, TypeRef returnType)
    {
        var parts = new List<string>();

        foreach (var (type, _) in outParams)
        {
            parts.Add(GetWireSizeExpression(type));
        }

        if (hasRetval)
        {
            parts.Add(GetWireSizeExpression(returnType));
        }

        parts.Add("sizeof(HRESULT)");

        return string.Join(" + ", parts);
    }

    /// <summary>
    /// Returns true if the type has a fixed wire size (no strings or variable-length types).
    /// </summary>
    public static bool IsFixedSize(TypeRef typeRef)
    {
        if (typeRef.IsArray)
            return false;
        if (typeRef.Name == "String")
            return false;
        if (IsInterfaceType(typeRef))
            return false;
        if (WireSizeMap.ContainsKey(typeRef.Name))
            return true;
        if (typeRef.ResolvedSymbol is EnumSymbol)
            return true;
        if (typeRef.ResolvedSymbol is StructSymbol structSym)
            return structSym.Fields.All(f => IsFixedSize(f.Type));
        return true; // unknown types treated as fixed-size (pointer-sized fallback)
    }

    /// <summary>
    /// Returns true if a struct is blittable: all fields are types that can be
    /// memcpy'd directly. Strings and interface pointers are NOT blittable.
    /// Empty structs are NOT blittable.
    /// Nested structs are blittable only if all their fields are also blittable.
    /// </summary>
    public static bool IsBlittable(StructSymbol structSym)
    {
        if (structSym.Fields.Count == 0)
            return false;
        return structSym.Fields.All(f => IsFieldBlittable(f.Type));
    }

    private static bool IsFieldBlittable(TypeRef typeRef)
    {
        if (BlittablePrimitives.Contains(typeRef.Name))
            return true;
        // WinRT enums are always 4 bytes (Int32/UInt32) — same as a primitive
        if (typeRef.ResolvedSymbol is EnumSymbol)
            return true;
        if (typeRef.ResolvedSymbol is StructSymbol nested)
            return IsBlittable(nested);
        return false;
    }

    /// <summary>
    /// Returns true if all parameters have fixed wire sizes.
    /// </summary>
    public static bool AllParamsFixedSize(List<ProxyParam> parms)
    {
        return parms.All(p => IsFixedSize(p.Type));
    }

    /// <summary>
    /// Computes the total fixed buffer size in bytes for a list of fixed-size parameters.
    /// </summary>
    public static int ComputeFixedBufferSize(List<ProxyParam> parms)
    {
        return parms.Sum(p => GetWireSize(p.Type));
    }

    /// <summary>
    /// Emits C++ static_assert statements verifying struct layout matches expected offsets and size.
    /// Also emits a static_assert for std::is_standard_layout to ensure the struct is C-compatible
    /// and safe for wire-format memcpy operations.
    /// </summary>
    public static void EmitStructLayoutAsserts(StringBuilder sb, StructSymbol structSym, string cTypeName, string indent = "")
    {
        sb.AppendLine($"{indent}static_assert(std::is_standard_layout_v<{cTypeName}>);");
        int offset = 0;
        foreach (var field in structSym.Fields)
        {
            sb.AppendLine($"{indent}static_assert(offsetof({cTypeName}, {field.Name}) == {offset});");
            offset += GetWireSize(field.Type);
        }
        sb.AppendLine($"{indent}static_assert(sizeof({cTypeName}) == {offset});");
    }

    /// <summary>
    /// Emits C++ code to deserialize an HSTRING using zero-copy WindowsCreateStringReference.
    /// Creates a non-owning string reference into the existing buffer.
    /// The string is valid as long as the buffer is alive (during Invoke).
    /// Wire format: [uint32_t char_count][wchar_t chars * char_count]
    /// </summary>
    public static void EmitDeserializeHStringZeroCopy(StringBuilder sb, string varName, string bufferPtr, string indent = "        ")
    {
        sb.AppendLine($"{indent}UINT32 {varName}_len;");
        sb.AppendLine($"{indent}memcpy(&{varName}_len, {bufferPtr}, sizeof(UINT32)); {bufferPtr} += sizeof(UINT32);");
        sb.AppendLine($"{indent}HSTRING_HEADER {varName}_hdr;");
        sb.AppendLine($"{indent}HSTRING {varName};");
        sb.AppendLine($"{indent}if ({varName}_len > 0) {{");
        sb.AppendLine($"{indent}    WindowsCreateStringReference(reinterpret_cast<PCWSTR>({bufferPtr}), {varName}_len, &{varName}_hdr, &{varName});");
        sb.AppendLine($"{indent}    {bufferPtr} += {varName}_len * sizeof(WCHAR);");
        sb.AppendLine($"{indent}}} else {{");
        sb.AppendLine($"{indent}    {varName} = nullptr;");
        sb.AppendLine($"{indent}}}");
    }


    /// <summary>
    /// Returns the C++ type name for a TypeRef, suitable for variable declarations.
    /// </summary>
    public static string GetCppTypeName(TypeRef typeRef)
    {
        return typeRef.Name switch
        {
            "Boolean" => "boolean",
            "UInt8" => "BYTE",
            "Char" => "WCHAR",
            "Int16" => "INT16",
            "UInt16" => "UINT16",
            "Int32" => "INT32",
            "UInt32" => "UINT32",
            "Single" => "FLOAT",
            "Int64" => "INT64",
            "UInt64" => "UINT64",
            "Double" => "DOUBLE",
            "Guid" => "GUID",
            "String" => "HSTRING",
            "EventRegistrationToken" => "EventRegistrationToken",
            _ when typeRef.ResolvedSymbol is EnumSymbol => typeRef.ResolvedSymbol.Name,
            _ when typeRef.ResolvedSymbol is StructSymbol => typeRef.ResolvedSymbol.Name,
            _ => typeRef.Name,
        };
    }

    /// <summary>
    /// Sanitizes a variable name for use as a prefix in generated locals
    /// (replaces '.' and '->' with '_').
    /// </summary>
    private static string SanitizeVarName(string name)
    {
        return name.Replace(".", "_").Replace("->", "_");
    }

    /// <summary>
    /// Returns a C++ sizeof-composed expression for the total buffer size of fixed-size params.
    /// Unlike <see cref="ComputeFixedBufferSize"/> which returns an int, this returns
    /// an expression like "sizeof(FLOAT) + sizeof(FLOAT)" for use in constexpr definitions.
    /// </summary>
    public static string ComputeFixedBufferSizeExpression(List<ProxyParam> parms)
    {
        if (parms.Count == 0)
            return "0";
        return string.Join(" + ", parms.Select(p => GetCppSizeExpression(p.Type)));
    }

    /// <summary>
    /// Returns true if the return type indicates an async method (IAsyncOperation, IAsyncAction, etc.).
    /// Async methods must not use zero-copy HSTRING deserialization because the server
    /// may capture parameters beyond the call lifetime.
    /// </summary>
    public static bool IsAsyncReturnType(TypeRef returnType)
    {
        return returnType.Name is "IAsyncOperation" or "IAsyncAction"
            or "IAsyncOperationWithProgress" or "IAsyncActionWithProgress";
    }

    /// <summary>
    /// Emits C++ code to deserialize an HSTRING using allocating WindowsCreateString.
    /// Unlike <see cref="EmitDeserializeHString"/> which uses zero-copy WindowsCreateStringReference,
    /// this creates an owning copy that is safe to capture beyond the RPC buffer lifetime.
    /// Used for async method parameters in stubs.
    /// </summary>
    public static void EmitDeserializeHStringAllocating(StringBuilder sb, string varName, string bufferPtr, string indent = "        ")
    {
        sb.AppendLine($"{indent}UINT32 {varName}_len;");
        sb.AppendLine($"{indent}memcpy(&{varName}_len, {bufferPtr}, sizeof(UINT32)); {bufferPtr} += sizeof(UINT32);");
        sb.AppendLine($"{indent}HSTRING {varName} = nullptr;");
        sb.AppendLine($"{indent}if ({varName}_len > 0) {{");
        sb.AppendLine($"{indent}    HRESULT hr = WindowsCreateString((PCWSTR){bufferPtr}, {varName}_len, &{varName});");
        sb.AppendLine($"{indent}    RETURN_IF_FAILED(hr);");
        sb.AppendLine($"{indent}}}");
        sb.AppendLine($"{indent}{bufferPtr} += {varName}_len * sizeof(WCHAR);");
    }

    /// <summary>
    /// Emits a consolidated set of #include directives for proxy/stub/factory files.
    /// Groups headers by category: system, WinRT, and optionally a generated header.
    /// </summary>
    public static void EmitConsolidatedIncludes(StringBuilder sb, string? headerFile = null)
    {
        sb.AppendLine("// System headers");
        sb.AppendLine("#include <windows.h>");
        sb.AppendLine("#include <objidl.h>");
        sb.AppendLine("// WinRT headers");
        sb.AppendLine("#include <hstring.h>");
        sb.AppendLine("#include <winstring.h>");
        sb.AppendLine("#include <inspectable.h>");
        sb.AppendLine("// C++ standard headers");
        sb.AppendLine("#include <type_traits>");
        sb.AppendLine("#include <array>");
        sb.AppendLine("// WIL compatibility");
        sb.AppendLine("#include \"winmidl/wil_compat.h\"");
        if (headerFile != null)
        {
            sb.AppendLine("// Generated header");
            sb.AppendLine($"#include \"{headerFile}\"");
        }
        sb.AppendLine();
    }

    /// <summary>
    /// Returns true if the type represents an interface pointer (interface, delegate,
    /// runtime class, or the special "Object" type / IInspectable).
    /// </summary>
    public static bool IsInterfaceType(TypeRef typeRef)
    {
        return typeRef.ResolvedSymbol is InterfaceSymbol or DelegateSymbol or RuntimeClassSymbol
            || typeRef.Name == "Object";
    }

    /// <summary>
    /// Returns the C++ IID constant name for an interface type (e.g. "IID___x_TestApp_IWidget").
    /// For the special "Object" type, returns "IID_IUnknown" — IInspectable has no
    /// registered proxy/stub, but IUnknown is universally supported.
    /// </summary>
    public static string GetInterfaceIidName(TypeRef typeRef)
    {
        if (typeRef.Name == "Object")
            return "IID_IUnknown";

        // Generic interface instantiation — unique IID per type arguments
        if (typeRef.TypeArguments.Count > 0 && typeRef.ResolvedSymbol is InterfaceSymbol or DelegateSymbol)
            return GetGenericIidConstantName(typeRef);

        // RuntimeClass: marshal via its default interface's IID (runtimeclasses don't have ABI-level IIDs)
        if (typeRef.ResolvedSymbol is RuntimeClassSymbol rc && rc.DefaultInterface is { } defaultIface)
        {
            var mangledName = HeaderEmitter.MangleName(defaultIface.FullName);
            return $"IID_{mangledName}";
        }

        if (typeRef.ResolvedSymbol is InterfaceSymbol or DelegateSymbol)
        {
            var mangledName = HeaderEmitter.MangleName(typeRef.ResolvedSymbol.FullName);
            var baseName = $"IID_{mangledName}";
            return AppendTypeArgumentSuffix(baseName, typeRef);
        }

        return $"IID_{typeRef.Name}";
    }

    /// <summary>
    /// Emits C++ code to pre-compute the marshaled size of interface-typed parameters.
    /// Must be called before buffer size computation when interface params are present.
    /// </summary>
    public static void EmitInterfaceMarshalSizeComputation(StringBuilder sb, IEnumerable<(string Name, TypeRef Type)> interfaceParams, string indent = "        ")
    {
        foreach (var (name, type) in interfaceParams)
        {
            var iidName = GetInterfaceIidName(type);
            sb.AppendLine($"{indent}UINT32 {name}_marshalSize = 0;");
            sb.AppendLine($"{indent}hr = winmidl::get_marshal_size({name}, {iidName}, {name}_marshalSize); RETURN_IF_FAILED(hr);");
        }
    }
}
