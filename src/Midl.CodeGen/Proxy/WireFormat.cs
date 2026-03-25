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
            return "sizeof(UINT32) + len * sizeof(WCHAR)";
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
        if (type.Name == "String")
            return "sizeof(UINT32) + WindowsGetStringLen({0}) * sizeof(WCHAR)";
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
            var expr = GetTypeSizeExpression(p.Type);
            if (expr.Contains("{0}"))
                expr = string.Format(expr, p.Name);
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
            sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}, sizeof(GUID)); {bufferPtr} += sizeof(GUID);");
            return;
        }

        if (typeRef.ResolvedSymbol is EnumSymbol)
        {
            sb.AppendLine($"{indent}INT32 {SanitizeVarName(varName)}_val = static_cast<INT32>({varName});");
            sb.AppendLine($"{indent}memcpy({bufferPtr}, &{SanitizeVarName(varName)}_val, sizeof(INT32)); {bufferPtr} += sizeof(INT32);");
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
            sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}, sizeof(EventRegistrationToken)); {bufferPtr} += sizeof(EventRegistrationToken);");
            return;
        }

        var sizeExpr = GetCppSizeExpression(typeRef);
        if (sizeExpr != null)
        {
            sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}, {sizeExpr}); {bufferPtr} += {sizeExpr};");
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
            sb.AppendLine($"{indent}memcpy(&{varName}, {bufferPtr}, sizeof(GUID)); {bufferPtr} += sizeof(GUID);");
            return;
        }

        if (typeRef.ResolvedSymbol is EnumSymbol)
        {
            sb.AppendLine($"{indent}memcpy(&{varName}, {bufferPtr}, sizeof(INT32)); {bufferPtr} += sizeof(INT32);");
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
                    var fieldSizeExpr = GetCppSizeExpression(field.Type);
                    sb.AppendLine($"{indent}memcpy(&{varName}.{field.Name}, {bufferPtr}, {fieldSizeExpr}); {bufferPtr} += {fieldSizeExpr};");
                }
            }
            return;
        }

        if (typeRef.Name == "EventRegistrationToken")
        {
            sb.AppendLine($"{indent}memcpy(&{varName}, {bufferPtr}, sizeof(EventRegistrationToken)); {bufferPtr} += sizeof(EventRegistrationToken);");
            return;
        }

        var sizeExpr = GetCppSizeExpression(typeRef);
        sb.AppendLine($"{indent}memcpy(&{varName}, {bufferPtr}, {sizeExpr}); {bufferPtr} += {sizeExpr};");
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
            sb.AppendLine($"{indent}UINT32 {targetName}_len; memcpy(&{targetName}_len, {bufferPtr}, sizeof(UINT32)); {bufferPtr} += sizeof(UINT32);");
            sb.AppendLine($"{indent}hr = WindowsCreateString((PCWSTR){bufferPtr}, {targetName}_len, {(isPointer ? targetName : $"&{targetName}")}); RETURN_IF_FAILED(hr);");
            sb.AppendLine($"{indent}{bufferPtr} += {targetName}_len * sizeof(WCHAR);");
            return;
        }

        if (typeRef.Name == "Guid")
        {
            sb.AppendLine($"{indent}memcpy({(isPointer ? targetName : $"&{targetName}")}, {bufferPtr}, sizeof(GUID)); {bufferPtr} += sizeof(GUID);");
            return;
        }

        if (typeRef.ResolvedSymbol is EnumSymbol enumSym)
        {
            var enumName = enumSym.Name;
            var safeName = SanitizeVarName(targetName);
            sb.AppendLine($"{indent}INT32 {safeName}_val; memcpy(&{safeName}_val, {bufferPtr}, sizeof(INT32)); {bufferPtr} += sizeof(INT32);");
            sb.AppendLine($"{indent}{deref} = static_cast<{enumName}>({safeName}_val);");
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

        var sizeExpr2 = GetCppSizeExpression(typeRef);
        sb.AppendLine($"{indent}memcpy({(isPointer ? targetName : $"&{targetName}")}, {bufferPtr}, {sizeExpr2}); {bufferPtr} += {sizeExpr2};");
    }

    /// <summary>
    /// Emits C++ code to serialize an HSTRING value (length-prefixed UTF-16).
    /// Wire format: [uint32_t char_count][wchar_t chars * char_count]
    /// </summary>
    public static void EmitSerializeHString(StringBuilder sb, string varName, string bufferPtr, string indent = "        ")
    {
        var safeName = SanitizeVarName(varName);
        sb.AppendLine($"{indent}UINT32 {safeName}_len = WindowsGetStringLen({varName});");
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
                    var fieldSizeExpr = GetCppSizeExpression(field.Type);
                    sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}.{field.Name}, {fieldSizeExpr}); {bufferPtr} += {fieldSizeExpr};");
                }
            }
        }
        else if (typeRef.Name == "String")
        {
            EmitSerializeHStringReply(sb, varName, bufferPtr, indent);
        }
        else
        {
            var sizeExpr = GetCppSizeExpression(typeRef);
            sb.AppendLine($"{indent}memcpy({bufferPtr}, &{varName}, {sizeExpr}); {bufferPtr} += {sizeExpr};");
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

        parts.Add("sizeof(HRESULT)");

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
}
