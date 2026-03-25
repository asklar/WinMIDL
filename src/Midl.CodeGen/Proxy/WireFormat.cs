using System.Text;
using Midl.Analysis.Symbols;

namespace Midl.CodeGen.Proxy;

/// <summary>
/// Shared wire-format helpers for proxy and stub code generation.
/// Both ProxyGenerator and StubGenerator MUST use these methods to ensure
/// they agree on serialization layout, type sizes, and reply ordering.
/// </summary>
public static class WireFormat
{
    /// <summary>Size of an HRESULT on the wire (always 4 bytes).</summary>
    public const int HResultSize = 4;

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
    /// Set of primitive type names that are blittable (directly memcpy-able without casting).
    /// Enums require a cast so are not blittable. Strings are variable-length.
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
            return "4 + len * 2";
        if (typeRef.ResolvedSymbol is StructSymbol structSym)
            return structSym.Fields.Sum(f => GetWireSize(f.Type)).ToString();
        return GetWireSize(typeRef).ToString();
    }

    /// <summary>
    /// Returns a C++ size expression for a type, suitable for use in buffer size computation.
    /// For HSTRING, includes a placeholder {0} for the variable name.
    /// </summary>
    public static string GetTypeSizeExpression(TypeRef type)
    {
        var name = type.Name;

        if (name is "Boolean" or "UInt8") return "1";
        if (name is "Int16" or "UInt16" or "Char") return "2";
        if (name is "Int32" or "UInt32" or "Single" or "HRESULT") return "4";
        if (name is "Int64" or "UInt64" or "Double") return "8";
        if (name is "Guid") return "16";
        if (name is "EventRegistrationToken") return "8";
        if (type.ResolvedSymbol is EnumSymbol) return "4";
        if (name is "String") return "4 + WindowsGetStringLen({0}) * 2";
        if (type.ResolvedSymbol is StructSymbol structSym)
            return structSym.Fields.Sum(f => GetWireSize(f.Type)).ToString();
        return "sizeof(void*)";
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
            var expr = GetTypeSizeExpression(p.Type);
            // Replace placeholder {0} with actual variable name (for HSTRING)
            expr = expr.Replace("{0}", p.Name);
            parts.Add(expr);
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
    /// Emits C++ code to serialize a value into a buffer.
    /// Assumes a BYTE* variable named <paramref name="bufferPtr"/> is in scope and will be advanced.
    /// </summary>
    public static void EmitSerialize(StringBuilder sb, TypeRef typeRef, string varName, string bufferPtr, string indent = "        ")
    {
        if (typeRef.Name == "String")
        {
            EmitSerializeHString(sb, varName, bufferPtr, indent);
            return;
        }

        if (typeRef.Name == "Guid")
        {
            sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}, 16); {bufferPtr} += 16;");
            return;
        }

        if (typeRef.ResolvedSymbol is EnumSymbol)
        {
            sb.AppendLine($"{indent}INT32 {SanitizeVarName(varName)}_val = static_cast<INT32>({varName});");
            sb.AppendLine($"{indent}memcpy({bufferPtr}, &{SanitizeVarName(varName)}_val, 4); {bufferPtr} += 4;");
            return;
        }

        if (typeRef.ResolvedSymbol is StructSymbol structSym)
        {
            if (IsBlittable(structSym))
            {
                sb.AppendLine($"{indent}static_assert(std::is_trivially_copyable_v<{structSym.Name}>);");
                sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}, sizeof({structSym.Name})); {bufferPtr} += sizeof({structSym.Name});");
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

        if (typeRef.Name == "EventRegistrationToken")
        {
            sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}, 8); {bufferPtr} += 8;");
            return;
        }

        var size = GetWireSize(typeRef);
        if (size > 0)
        {
            sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}, {size}); {bufferPtr} += {size};");
            return;
        }

        sb.AppendLine($"{indent}// TODO: marshal {varName} of type {typeRef.Name}");
    }

    /// <summary>
    /// Emits C++ code to deserialize a value from a buffer into a local variable.
    /// Used by StubGenerator for deserializing input parameters (declares the variable).
    /// </summary>
    public static void EmitDeserialize(StringBuilder sb, TypeRef typeRef, string varName, string bufferPtr, string indent = "        ")
    {
        if (typeRef.Name == "String")
        {
            EmitDeserializeHString(sb, varName, bufferPtr, indent);
            return;
        }

        if (typeRef.Name == "Guid")
        {
            sb.AppendLine($"{indent}memcpy(&{varName}, {bufferPtr}, 16); {bufferPtr} += 16;");
            return;
        }

        if (typeRef.ResolvedSymbol is EnumSymbol)
        {
            sb.AppendLine($"{indent}memcpy(&{varName}, {bufferPtr}, 4); {bufferPtr} += 4;");
            return;
        }

        if (typeRef.ResolvedSymbol is StructSymbol structSym)
        {
            if (IsBlittable(structSym))
            {
                sb.AppendLine($"{indent}static_assert(std::is_trivially_copyable_v<{structSym.Name}>);");
                sb.AppendLine($"{indent}memcpy(&{varName}, {bufferPtr}, sizeof({structSym.Name})); {bufferPtr} += sizeof({structSym.Name});");
            }
            else
            {
                foreach (var field in structSym.Fields)
                {
                    var fieldSize = GetWireSize(field.Type);
                    sb.AppendLine($"{indent}memcpy(&{varName}.{field.Name}, {bufferPtr}, {fieldSize}); {bufferPtr} += {fieldSize};");
                }
            }
            return;
        }

        if (typeRef.Name == "EventRegistrationToken")
        {
            sb.AppendLine($"{indent}memcpy(&{varName}, {bufferPtr}, 8); {bufferPtr} += 8;");
            return;
        }

        var size = GetWireSize(typeRef);
        sb.AppendLine($"{indent}memcpy(&{varName}, {bufferPtr}, {size}); {bufferPtr} += {size};");
    }

    /// <summary>
    /// Emits C++ code to deserialize a value from the reply buffer into a pointer target.
    /// Used by ProxyGenerator for deserializing reply out-params and retvals.
    /// </summary>
    public static void EmitDeserializeReply(StringBuilder sb, TypeRef typeRef, string targetName, string bufferPtr, bool isPointer, string indent = "        ")
    {
        var deref = isPointer ? $"*{targetName}" : targetName;

        if (typeRef.Name == "String")
        {
            sb.AppendLine($"{indent}UINT32 {targetName}_len; memcpy(&{targetName}_len, {bufferPtr}, 4); {bufferPtr} += 4;");
            sb.AppendLine($"{indent}WindowsCreateString((PCWSTR){bufferPtr}, {targetName}_len, {(isPointer ? targetName : $"&{targetName}")}); {bufferPtr} += {targetName}_len * 2;");
            return;
        }

        if (typeRef.Name == "Guid")
        {
            sb.AppendLine($"{indent}memcpy({(isPointer ? targetName : $"&{targetName}")}, {bufferPtr}, 16); {bufferPtr} += 16;");
            return;
        }

        if (typeRef.ResolvedSymbol is EnumSymbol)
        {
            sb.AppendLine($"{indent}INT32 {targetName}_val; memcpy(&{targetName}_val, {bufferPtr}, 4); {bufferPtr} += 4;");
            sb.AppendLine($"{indent}{deref} = static_cast<decltype({deref})>({targetName}_val);");
            return;
        }

        if (typeRef.ResolvedSymbol is StructSymbol structSym)
        {
            if (IsBlittable(structSym))
            {
                sb.AppendLine($"{indent}static_assert(std::is_trivially_copyable_v<{structSym.Name}>);");
                sb.AppendLine($"{indent}memcpy({(isPointer ? targetName : $"&{targetName}")}, {bufferPtr}, sizeof({structSym.Name})); {bufferPtr} += sizeof({structSym.Name});");
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

        var size = GetWireSize(typeRef);
        if (typeRef.Name == "EventRegistrationToken") size = 8;
        sb.AppendLine($"{indent}memcpy({(isPointer ? targetName : $"&{targetName}")}, {bufferPtr}, {size}); {bufferPtr} += {size};");
    }

    /// <summary>
    /// Emits C++ code to serialize an HSTRING value (length-prefixed UTF-16).
    /// </summary>
    public static void EmitSerializeHString(StringBuilder sb, string varName, string bufferPtr, string indent = "        ")
    {
        var safeName = SanitizeVarName(varName);
        sb.AppendLine($"{indent}UINT32 {safeName}_len = WindowsGetStringLen({varName});");
        sb.AppendLine($"{indent}memcpy({bufferPtr}, &{safeName}_len, 4); {bufferPtr} += 4;");
        sb.AppendLine($"{indent}PCWSTR {safeName}_raw = WindowsGetStringRawBuffer({varName}, nullptr);");
        sb.AppendLine($"{indent}memcpy({bufferPtr}, {safeName}_raw, {safeName}_len * 2); {bufferPtr} += {safeName}_len * 2;");
    }

    /// <summary>
    /// Emits C++ code to deserialize an HSTRING value (length-prefixed UTF-16).
    /// Declares the HSTRING variable.
    /// </summary>
    public static void EmitDeserializeHString(StringBuilder sb, string varName, string bufferPtr, string indent = "        ")
    {
        sb.AppendLine($"{indent}UINT32 {varName}_len;");
        sb.AppendLine($"{indent}memcpy(&{varName}_len, {bufferPtr}, 4); {bufferPtr} += 4;");
        sb.AppendLine($"{indent}HSTRING {varName};");
        sb.AppendLine($"{indent}if ({varName}_len > 0) {{");
        sb.AppendLine($"{indent}    WindowsCreateString((PCNZWCH){bufferPtr}, {varName}_len, &{varName});");
        sb.AppendLine($"{indent}    {bufferPtr} += {varName}_len * 2;");
        sb.AppendLine($"{indent}}} else {{");
        sb.AppendLine($"{indent}    {varName} = nullptr;");
        sb.AppendLine($"{indent}}}");
    }

    /// <summary>
    /// Emits C++ code to serialize an HSTRING for reply (using WindowsGetStringRawBuffer).
    /// Used by StubGenerator for serializing HSTRING out params or return values.
    /// </summary>
    public static void EmitSerializeHStringReply(StringBuilder sb, string varName, string bufferPtr, string indent = "        ")
    {
        sb.AppendLine($"{indent}UINT32 {varName}_len = 0;");
        sb.AppendLine($"{indent}PCWSTR {varName}_raw = nullptr;");
        sb.AppendLine($"{indent}if ({varName}) {{ {varName}_raw = WindowsGetStringRawBuffer({varName}, &{varName}_len); }}");
        sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}_len, 4); {bufferPtr} += 4;");
        sb.AppendLine($"{indent}if ({varName}_len > 0) {{ memcpy({bufferPtr}, {varName}_raw, {varName}_len * 2); {bufferPtr} += {varName}_len * 2; }}");
    }

    /// <summary>
    /// Emits C++ serialization for a value in the reply buffer (used by StubGenerator).
    /// </summary>
    public static void EmitSerializeReplyValue(StringBuilder sb, TypeRef typeRef, string varName, string bufferPtr, string indent = "        ")
    {
        if (typeRef.ResolvedSymbol is StructSymbol structSym)
        {
            if (IsBlittable(structSym))
            {
                sb.AppendLine($"{indent}static_assert(std::is_trivially_copyable_v<{structSym.Name}>);");
                sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}, sizeof({structSym.Name})); {bufferPtr} += sizeof({structSym.Name});");
            }
            else
            {
                foreach (var field in structSym.Fields)
                {
                    var fieldSize = GetWireSize(field.Type);
                    sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}.{field.Name}, {fieldSize}); {bufferPtr} += {fieldSize};");
                }
            }
        }
        else if (typeRef.Name == "String")
        {
            EmitSerializeHStringReply(sb, varName, bufferPtr, indent);
        }
        else
        {
            var size = GetWireSize(typeRef);
            sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}, {size}); {bufferPtr} += {size};");
        }
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

        parts.Add(HResultSize.ToString());

        return string.Join(" + ", parts);
    }

    /// <summary>
    /// Returns true if the type has a fixed wire size (no strings or variable-length types).
    /// </summary>
    public static bool IsFixedSize(TypeRef typeRef)
    {
        if (typeRef.Name == "String")
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
    /// Returns true if a struct is blittable: all fields are primitive types that can be
    /// memcpy'd without casting. Enums are NOT blittable (require INT32 cast).
    /// Strings are NOT blittable (variable length). Empty structs are NOT blittable.
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
    /// </summary>
    public static void EmitStructLayoutAsserts(StringBuilder sb, StructSymbol structSym, string cTypeName, string indent = "")
    {
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
    /// </summary>
    public static void EmitDeserializeHStringZeroCopy(StringBuilder sb, string varName, string bufferPtr, string indent = "        ")
    {
        sb.AppendLine($"{indent}UINT32 {varName}_len;");
        sb.AppendLine($"{indent}memcpy(&{varName}_len, {bufferPtr}, 4); {bufferPtr} += 4;");
        sb.AppendLine($"{indent}HSTRING_HEADER {varName}_hdr;");
        sb.AppendLine($"{indent}HSTRING {varName};");
        sb.AppendLine($"{indent}if ({varName}_len > 0) {{");
        sb.AppendLine($"{indent}    WindowsCreateStringReference(reinterpret_cast<PCWSTR>({bufferPtr}), {varName}_len, &{varName}_hdr, &{varName});");
        sb.AppendLine($"{indent}    {bufferPtr} += {varName}_len * 2;");
        sb.AppendLine($"{indent}}} else {{");
        sb.AppendLine($"{indent}    {varName} = nullptr;");
        sb.AppendLine($"{indent}}}");
    }


    /// <summary>
    /// Sanitizes a variable name for use as a prefix in generated locals
    /// (replaces '.' and '->' with '_').
    /// </summary>
    private static string SanitizeVarName(string name)
    {
        return name.Replace(".", "_").Replace("->", "_");
    }
}
