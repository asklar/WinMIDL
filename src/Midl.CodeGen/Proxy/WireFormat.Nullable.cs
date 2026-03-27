using Midl.Analysis.Symbols;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Proxy;

/// <summary>
/// Partial class extension for parameterized interface (IReference&lt;T&gt;, etc.) support.
/// Handles type-argument-aware IID name generation for generic WinRT interfaces.
/// </summary>
public static partial class WireFormat
{
    /// <summary>
    /// Appends a mangled type argument suffix to an IID constant name when the
    /// TypeRef has type arguments (e.g. IReference&lt;Int32&gt;).
    /// This ensures each parameterized instantiation gets a unique IID name.
    /// </summary>
    internal static string AppendTypeArgumentSuffix(string baseName, TypeRef typeRef)
    {
        if (typeRef.TypeArguments.Count == 0)
            return baseName;

        var suffix = string.Join("_", typeRef.TypeArguments.Select(MangleTypeArgForIid));
        return $"{baseName}__{suffix}";
    }

    /// <summary>
    /// Returns a mangled name for a type argument suitable for use in IID constant names.
    /// For types with a resolved symbol (user-defined), uses the full mangled name.
    /// For built-in types, uses the type name directly.
    /// </summary>
    private static string MangleTypeArgForIid(TypeRef typeArg)
    {
        if (typeArg.ResolvedSymbol is not null)
        {
            var mangled = HeaderEmitter.MangleName(typeArg.ResolvedSymbol.FullName);
            return AppendTypeArgumentSuffix(mangled, typeArg);
        }

        // Built-in type (Int32, String, etc.) — use name directly
        return typeArg.Name;
    }
}
