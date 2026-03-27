using System.Text;
using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.CodeGen.Proxy;

/// <summary>
/// Generic collection interface support for WireFormat.
/// Handles IID computation and constant emission for parameterized interface
/// instantiations (IVector&lt;T&gt;, IMap&lt;K,V&gt;, IIterable&lt;T&gt;, etc.).
/// </summary>
public static partial class WireFormat
{
    /// <summary>
    /// Well-known base GUIDs for WinRT parameterized interfaces.
    /// These are the PIIDs used as input to the parameterized IID algorithm.
    /// </summary>
    private static readonly Dictionary<string, Guid> s_parameterizedInterfacePiids = new(StringComparer.Ordinal)
    {
        // Windows.Foundation.Collections
        ["Windows.Foundation.Collections.IIterable"] = new("faa585ea-6214-4217-afda-7f46de5869b3"),
        ["Windows.Foundation.Collections.IIterator"] = new("6a79e863-4300-459a-9966-cbb660963ee1"),
        ["Windows.Foundation.Collections.IVector"] = new("913337e9-11a1-4345-a3a2-4e7f956e222d"),
        ["Windows.Foundation.Collections.IVectorView"] = new("bbe1fa4c-b0e3-4583-baef-1f1b2e483e56"),
        ["Windows.Foundation.Collections.IMap"] = new("3c2925fe-8519-45c1-aa79-197b6718c1c1"),
        ["Windows.Foundation.Collections.IMapView"] = new("e480ce40-a338-4ada-adcf-272272e48cb9"),
        ["Windows.Foundation.Collections.IKeyValuePair"] = new("02b51929-c1c4-4a7e-8940-0312b5c18500"),
        ["Windows.Foundation.Collections.IObservableVector"] = new("5917eb53-50b4-4a0d-b309-65862b3f1dbc"),
        ["Windows.Foundation.Collections.IObservableMap"] = new("65df2bf5-bf39-41b5-aebc-5a9d865e472b"),
        // Windows.Foundation
        ["Windows.Foundation.IAsyncOperation"] = new("9fc2b0bb-e446-44e2-aa61-9a8696523175"),
        ["Windows.Foundation.IAsyncOperationWithProgress"] = new("b5d036d7-e297-498f-ba60-0289e76e23dd"),
        ["Windows.Foundation.IAsyncActionWithProgress"] = new("1f6db258-e803-48a1-9546-eb7353398884"),
        ["Windows.Foundation.IReference"] = new("61c17706-2d65-11e0-9ae8-d48564015472"),
        ["Windows.Foundation.EventHandler"] = new("9de1c534-6ae1-11e0-84e1-18a905bcc53f"),
        ["Windows.Foundation.TypedEventHandler"] = new("9de1c535-6ae1-11e0-84e1-18a905bcc53f"),
    };

    /// <summary>
    /// Returns a mangled IID constant name for a generic interface instantiation.
    /// E.g., IVector&lt;String&gt; → "IID_IVector_1_String",
    ///       IMap&lt;String, Int32&gt; → "IID_IMap_2_String_Int32".
    /// </summary>
    public static string GetGenericIidConstantName(TypeRef typeRef)
    {
        var sb = new StringBuilder("IID_");
        sb.Append(typeRef.ResolvedSymbol?.Name ?? typeRef.Name);
        sb.Append('_');
        sb.Append(typeRef.TypeArguments.Count);
        foreach (var arg in typeRef.TypeArguments)
        {
            sb.Append('_');
            sb.Append(MangleTypeArgName(arg));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Computes the IID for a specific generic interface instantiation using the PIID algorithm.
    /// Returns null if the base PIID is unknown.
    /// </summary>
    public static Guid? ComputeGenericIid(TypeRef typeRef)
    {
        if (typeRef.TypeArguments.Count == 0 || typeRef.ResolvedSymbol == null)
            return null;

        var fullName = typeRef.ResolvedSymbol.FullName;
        if (!s_parameterizedInterfacePiids.TryGetValue(fullName, out var basePiid))
            return null;

        var typeArgSigs = typeRef.TypeArguments.Select(GetTypeArgumentSignature).ToList();
        return PiidGenerator.GenerateIid(basePiid.ToString(), typeArgSigs);
    }

    /// <summary>
    /// Collects all unique generic interface types used as method parameters or return types
    /// across all non-external interfaces in a compilation.
    /// </summary>
    public static List<TypeRef> CollectGenericInterfaceTypes(Compilation compilation)
    {
        var seen = new HashSet<string>();
        var result = new List<TypeRef>();

        foreach (var sym in compilation.GetAllTypes().Where(t => !t.IsExternal))
        {
            if (sym is not InterfaceSymbol iface) continue;

            foreach (var member in iface.Members)
            {
                switch (member)
                {
                    case MethodSymbol method:
                        CollectFromTypeRef(method.ReturnType, seen, result);
                        foreach (var p in method.Parameters)
                            CollectFromTypeRef(p.Type, seen, result);
                        break;
                    case PropertySymbol prop:
                        CollectFromTypeRef(prop.Type, seen, result);
                        break;
                    case EventSymbol evt:
                        CollectFromTypeRef(evt.DelegateType, seen, result);
                        break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Emits C++ constexpr IID constant definitions for all generic interface types
    /// used in the compilation. Should be called after the prologue in proxy/stub files.
    /// </summary>
    public static void EmitGenericIidConstants(StringBuilder sb, Compilation compilation)
    {
        var genericTypes = CollectGenericInterfaceTypes(compilation);
        if (genericTypes.Count == 0) return;

        sb.AppendLine("// Parameterized interface IIDs (computed via WinRT PIID algorithm)");
        foreach (var typeRef in genericTypes)
        {
            var iid = ComputeGenericIid(typeRef);
            if (iid == null) continue;

            var constantName = GetGenericIidConstantName(typeRef);
            sb.AppendLine($"static constexpr IID {constantName} = {FormatGuidAsCpp(iid.Value)};");
        }
        sb.AppendLine();
    }

    private static void CollectFromTypeRef(TypeRef typeRef, HashSet<string> seen, List<TypeRef> result)
    {
        if (typeRef.TypeArguments.Count > 0 && typeRef.ResolvedSymbol is InterfaceSymbol or DelegateSymbol)
        {
            var key = GetGenericIidConstantName(typeRef);
            if (seen.Add(key))
                result.Add(typeRef);

            // Recurse into nested generic type arguments
            foreach (var arg in typeRef.TypeArguments)
                CollectFromTypeRef(arg, seen, result);
        }

        // Also check array element types
        if (typeRef.IsArray)
            CollectFromTypeRef(ElementType(typeRef), seen, result);
    }

    private static string MangleTypeArgName(TypeRef arg)
    {
        if (arg.TypeArguments.Count > 0)
        {
            var parts = new List<string> { arg.ResolvedSymbol?.Name ?? arg.Name };
            parts.Add(arg.TypeArguments.Count.ToString());
            foreach (var nested in arg.TypeArguments)
                parts.Add(MangleTypeArgName(nested));
            return string.Join("_", parts);
        }

        return arg.ResolvedSymbol?.Name ?? arg.Name;
    }

    /// <summary>
    /// Gets the WinRT type signature for a type argument, used in PIID computation.
    /// </summary>
    private static string GetTypeArgumentSignature(TypeRef arg)
    {
        if (arg.IsBuiltIn || arg.ResolvedSymbol == null)
            return PiidGenerator.GetPrimitiveTypeSignature(arg.Name);

        // Nested generic interface: compute pinterface signature recursively
        if (arg.TypeArguments.Count > 0 && arg.ResolvedSymbol is InterfaceSymbol or DelegateSymbol)
        {
            var fullName = arg.ResolvedSymbol.FullName;
            if (s_parameterizedInterfacePiids.TryGetValue(fullName, out var nestedPiid))
            {
                var nestedArgSigs = arg.TypeArguments.Select(GetTypeArgumentSignature);
                return $"pinterface({{{nestedPiid}}};{string.Join(";", nestedArgSigs)})";
            }
        }

        return PiidGenerator.GetTypeSignature(arg.ResolvedSymbol);
    }

    /// <summary>
    /// Formats a GUID as a C++ aggregate initializer, e.g.
    /// "{ 0x2f13c006, 0xa03a, 0x5f69, { 0xb0, 0x90, 0x75, 0xa4, 0x3e, 0x33, 0x42, 0x3e } }".
    /// </summary>
    internal static string FormatGuidAsCpp(Guid guid)
    {
        var bytes = guid.ToByteArray();
        var a = BitConverter.ToUInt32(bytes, 0);
        var b = BitConverter.ToUInt16(bytes, 4);
        var c = BitConverter.ToUInt16(bytes, 6);
        return $"{{ 0x{a:x8}, 0x{b:x4}, 0x{c:x4}, " +
               $"{{ 0x{bytes[8]:x2}, 0x{bytes[9]:x2}, 0x{bytes[10]:x2}, 0x{bytes[11]:x2}, " +
               $"0x{bytes[12]:x2}, 0x{bytes[13]:x2}, 0x{bytes[14]:x2}, 0x{bytes[15]:x2} }} }}";
    }
}
