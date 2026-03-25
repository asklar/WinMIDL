using System.Text;
using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.CodeGen.Header;

/// <summary>
/// Partial class for emitting parameterized interface instantiations (__FI blocks).
/// Handles C++ template specializations and C vtable structs for generic WinRT types
/// like TypedEventHandler&lt;T1,T2&gt;, IVector&lt;T&gt;, IMap&lt;K,V&gt;, etc.
/// </summary>
public partial class HeaderEmitter
{
    /// <summary>
    /// Maps a WinRT primitive type name to its __FI mangled form.
    /// Different from MapType — this is used inside __FI identifier construction.
    /// </summary>
    private static readonly Dictionary<string, string> FIMangledPrimitives = new(StringComparer.Ordinal)
    {
        ["String"] = "HSTRING",
        ["Int16"] = "short",
        ["Int32"] = "int",
        ["Int64"] = "__int64",
        ["UInt8"] = "BYTE",
        ["UInt16"] = "UINT16",
        ["UInt32"] = "UINT32",
        ["UInt64"] = "unsigned__int64",
        ["Single"] = "float",
        ["Double"] = "double",
        ["Boolean"] = "boolean",
        ["Char"] = "wchar_t",
        ["Guid"] = "GUID",
        ["Object"] = "IInspectable",
    };

    /// <summary>
    /// Known WinRT parameterized types: whether they derive from IInspectable (true)
    /// or IUnknown (false = delegates).
    /// </summary>
    private static readonly Dictionary<string, bool> KnownParameterizedIsInspectable = new(StringComparer.Ordinal)
    {
        ["Windows.Foundation.TypedEventHandler"] = false,
        ["Windows.Foundation.EventHandler"] = false,
        ["Windows.Foundation.Collections.IVector"] = true,
        ["Windows.Foundation.Collections.IVectorView"] = true,
        ["Windows.Foundation.Collections.IIterable"] = true,
        ["Windows.Foundation.Collections.IIterator"] = true,
        ["Windows.Foundation.Collections.IMap"] = true,
        ["Windows.Foundation.Collections.IMapView"] = true,
        ["Windows.Foundation.Collections.IKeyValuePair"] = true,
        ["Windows.Foundation.Collections.IObservableVector"] = true,
        ["Windows.Foundation.Collections.IObservableMap"] = true,
        ["Windows.Foundation.IAsyncOperation"] = true,
        ["Windows.Foundation.IAsyncOperationWithProgress"] = true,
        ["Windows.Foundation.IAsyncActionWithProgress"] = true,
        ["Windows.Foundation.IReference"] = true,
    };

    /// <summary>
    /// Maps a single type argument to its __FI identifier component.
    /// </summary>
    internal static string MapTypeArgForFI(TypeRef typeRef)
    {
        // Nested generic: recurse into ComputeFIMangledName
        if (typeRef.TypeArguments.Count > 0)
            return ComputeFIMangledName(typeRef);

        // Primitive types
        if (FIMangledPrimitives.TryGetValue(typeRef.Name, out var mapped))
            return mapped;

        // Namespaced types: replace '.' with '__C'
        if (typeRef.Name.Contains('.'))
            return typeRef.Name.Replace(".", "__C");

        // Check resolved symbol for namespace
        if (typeRef.ResolvedSymbol != null)
        {
            var fullName = typeRef.ResolvedSymbol.FullName;
            if (fullName.Contains('.'))
                return fullName.Replace(".", "__C");
        }

        // Simple unqualified name
        return typeRef.Name;
    }

    /// <summary>
    /// Computes the __FI mangled name for a parameterized interface instantiation.
    /// Pattern: __FI + BaseName (with leading I stripped for interfaces) + _arity_ + encoded type args joined by _
    /// </summary>
    internal static string ComputeFIMangledName(TypeRef typeRef)
    {
        // Get the short name (last segment after '.')
        var fullBaseName = typeRef.Name;
        var lastDot = fullBaseName.LastIndexOf('.');
        var shortName = lastDot >= 0 ? fullBaseName[(lastDot + 1)..] : fullBaseName;

        // Strip leading 'I' if followed by an uppercase letter (WinRT interface convention)
        var fiBaseName = shortName;
        if (shortName.Length > 1 && shortName[0] == 'I' && char.IsUpper(shortName[1]))
            fiBaseName = shortName[1..];

        var arity = typeRef.TypeArguments.Count;
        var encodedArgs = typeRef.TypeArguments.Select(MapTypeArgForFI);

        return $"__FI{fiBaseName}_{arity}_{string.Join("_", encodedArgs)}";
    }

    /// <summary>
    /// Collects all unique parameterized interface instantiations from a compilation.
    /// Scans all TypeRefs across all types looking for those with TypeArguments.
    /// </summary>
    internal static List<TypeRef> CollectParameterizedInstantiations(Compilation compilation)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<TypeRef>();

        var allTypes = compilation.GetAllTypes().Where(t => !t.IsExternal).ToList();

        foreach (var symbol in allTypes)
        {
            CollectFromSymbol(symbol, seen, result);
        }

        return result;
    }

    private static void CollectFromSymbol(ISymbol symbol, HashSet<string> seen, List<TypeRef> result)
    {
        switch (symbol)
        {
            case InterfaceSymbol iface:
                foreach (var member in iface.Members)
                    CollectFromMember(member, seen, result);
                break;
            case DelegateSymbol del:
                CollectFromTypeRef(del.ReturnType, seen, result);
                foreach (var p in del.Parameters)
                    CollectFromTypeRef(p.Type, seen, result);
                break;
            case RuntimeClassSymbol rc:
                foreach (var iface in rc.ImplementedInterfaces)
                {
                    // Check the interface's members for parameterized types
                    foreach (var member in iface.Members)
                        CollectFromMember(member, seen, result);
                }
                break;
        }
    }

    private static void CollectFromMember(ISymbol member, HashSet<string> seen, List<TypeRef> result)
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

    private static void CollectFromTypeRef(TypeRef typeRef, HashSet<string> seen, List<TypeRef> result)
    {
        if (typeRef.TypeArguments.Count > 0)
        {
            // Recurse into type arguments first (inner generics before outer)
            foreach (var arg in typeRef.TypeArguments)
                CollectFromTypeRef(arg, seen, result);

            var fiName = ComputeFIMangledName(typeRef);
            if (seen.Add(fiName))
                result.Add(typeRef);
        }
    }

    /// <summary>
    /// Emits all parameterized interface instantiation blocks into the header.
    /// Called once from Emit() — C++ blocks go before user interfaces,
    /// C blocks go in the C section.
    /// </summary>
    private static void EmitParameterizedInterfacesCpp(StringBuilder sb, List<TypeRef> instantiations)
    {
        if (instantiations.Count == 0) return;

        foreach (var typeRef in instantiations)
        {
            EmitParameterizedInterfaceCpp(sb, typeRef);
        }
    }

    private static void EmitParameterizedInterfacesCBlock(StringBuilder sb, List<TypeRef> instantiations)
    {
        if (instantiations.Count == 0) return;

        foreach (var typeRef in instantiations)
        {
            EmitParameterizedInterfaceCBlock(sb, typeRef);
        }
    }

    /// <summary>
    /// Emits the C++ template specialization block for one parameterized interface instantiation.
    /// </summary>
    internal static void EmitParameterizedInterfaceCpp(StringBuilder sb, TypeRef typeRef)
    {
        var fiName = ComputeFIMangledName(typeRef);
        var defGuard = $"DEF_{fiName}_USE";

        // Get base interface info
        var fullBaseName = typeRef.Name;
        var lastDot = fullBaseName.LastIndexOf('.');
        var shortName = lastDot >= 0 ? fullBaseName[(lastDot + 1)..] : fullBaseName;
        var namespacePart = lastDot >= 0 ? fullBaseName[..lastDot] : "";
        var nsParts = namespacePart.Length > 0 ? namespacePart.Split('.') : Array.Empty<string>();

        // Build C++ template args: Type* for reference types, value types as-is
        var cppTypeArgs = typeRef.TypeArguments.Select(a => MapTypeToCppTemplateArg(a)).ToList();
        var cppTypeArgStr = string.Join(",", cppTypeArgs);

        // Build WinRT rc_name string: GenericBase`arity<Arg1, Arg2>
        var winrtArgStr = string.Join(", ", typeRef.TypeArguments.Select(a => GetWinRTTypeName(a)));
        var rcName = $"{fullBaseName}`{typeRef.TypeArguments.Count}<{winrtArgStr}>";

        sb.AppendLine($"#ifndef {defGuard}");
        sb.AppendLine($"#define {defGuard}");
        sb.AppendLine("#if !defined(RO_NO_TEMPLATE_NAME)");

        // Open namespace blocks
        for (int i = 0; i < nsParts.Length; i++)
        {
            var indent = new string(' ', i * 4);
            sb.Append($"{indent}namespace {nsParts[i]} {{ ");
        }
        sb.AppendLine();

        var nsIndent = new string(' ', nsParts.Length * 4);

        // Compute PIID for the uuid
        var piid = ComputeParameterizedIid(typeRef);

        sb.AppendLine($"{nsIndent}template <>");
        sb.AppendLine($"{nsIndent}struct __declspec(uuid(\"{piid:D}\"))");
        sb.AppendLine($"{nsIndent}{shortName}<{cppTypeArgStr}> : {shortName}_impl<{cppTypeArgStr}>");
        sb.AppendLine($"{nsIndent}{{");
        sb.AppendLine($"{nsIndent}    static const wchar_t* z_get_rc_name_impl()");
        sb.AppendLine($"{nsIndent}    {{");
        sb.AppendLine($"{nsIndent}        return L\"{rcName}\";");
        sb.AppendLine($"{nsIndent}    }}");
        sb.AppendLine($"{nsIndent}}};");

        // typedef
        sb.AppendLine($"{nsIndent}typedef {shortName}<{cppTypeArgStr}> {fiName}_t;");

        // #define alias
        var nsPrefix = string.Join("::", nsParts);
        if (nsPrefix.Length > 0) nsPrefix += "::";
        sb.AppendLine($"#define {fiName} {nsPrefix}{fiName}_t");

        // Close namespace blocks
        for (int i = nsParts.Length - 1; i >= 0; i--)
        {
            sb.Append($"/* {nsParts[i]} */ }} ");
        }
        sb.AppendLine();

        sb.AppendLine("#endif // !defined(RO_NO_TEMPLATE_NAME)");
        sb.AppendLine($"#endif /* {defGuard} */");
        sb.AppendLine();
    }

    /// <summary>
    /// Emits the C vtable struct block for one parameterized interface instantiation.
    /// </summary>
    internal static void EmitParameterizedInterfaceCBlock(StringBuilder sb, TypeRef typeRef)
    {
        var fiName = ComputeFIMangledName(typeRef);
        var guard = $"____{fiName}_INTERFACE_DEFINED__";
        var isInspectable = IsParameterizedTypeInspectable(typeRef.Name);

        sb.AppendLine($"#if !defined({guard})");
        sb.AppendLine($"#define {guard}");
        sb.AppendLine();

        // typedef interface forward declaration
        sb.AppendLine($"typedef interface {fiName} {fiName};");
        sb.AppendLine();

        // EXTERN_C IID
        sb.AppendLine($"EXTERN_C const IID IID_{fiName};");
        sb.AppendLine();

        // Get the vtable methods for this parameterized type
        var vtableMethods = GetParameterizedVtableMethods(typeRef);

        // Emit vtable struct
        sb.AppendLine($"typedef struct {fiName}Vtbl");
        sb.AppendLine("{");
        sb.AppendLine("    BEGIN_INTERFACE");

        // IUnknown base methods
        sb.AppendLine($"    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in {fiName} * This, __RPC__in REFIID riid, _COM_Outptr_ void **ppvObject);");
        sb.AppendLine($"    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in {fiName} * This);");
        sb.AppendLine($"    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in {fiName} * This);");

        // IInspectable base methods (for interfaces, not delegates)
        if (isInspectable)
        {
            sb.AppendLine($"    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in {fiName} * This, __RPC__out ULONG *iidCount, __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);");
            sb.AppendLine($"    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in {fiName} * This, __RPC__deref_out_opt HSTRING *className);");
            sb.AppendLine($"    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in {fiName} * This, __RPC__out TrustLevel *trustLevel);");
        }

        // Type-specific methods
        foreach (var (methodName, methodParams) in vtableMethods)
        {
            sb.AppendLine($"    HRESULT ( STDMETHODCALLTYPE *{methodName} )({fiName} * This{methodParams});");
        }

        sb.AppendLine("    END_INTERFACE");
        sb.AppendLine($"}} {fiName}Vtbl;");
        sb.AppendLine();

        // Interface struct
        sb.AppendLine($"interface {fiName}");
        sb.AppendLine("{");
        sb.AppendLine($"    CONST_VTBL struct {fiName}Vtbl *lpVtbl;");
        sb.AppendLine("};");
        sb.AppendLine();

        // COBJMACROS
        sb.AppendLine("#ifdef COBJMACROS");
        EmitFICObjMacros(sb, fiName, vtableMethods, isInspectable);
        sb.AppendLine("#endif /* COBJMACROS */");
        sb.AppendLine();

        sb.AppendLine($"#endif // {guard}");
        sb.AppendLine();
    }

    private static void EmitFICObjMacros(StringBuilder sb, string fiName,
        List<(string Name, string Params)> methods, bool isInspectable)
    {
        // IUnknown macros
        sb.AppendLine($"#define {fiName}_QueryInterface(This,riid,ppvObject) \\");
        sb.AppendLine($"    ( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )");
        sb.AppendLine();
        sb.AppendLine($"#define {fiName}_AddRef(This) \\");
        sb.AppendLine($"    ( (This)->lpVtbl->AddRef(This) )");
        sb.AppendLine();
        sb.AppendLine($"#define {fiName}_Release(This) \\");
        sb.AppendLine($"    ( (This)->lpVtbl->Release(This) )");
        sb.AppendLine();

        if (isInspectable)
        {
            sb.AppendLine($"#define {fiName}_GetIids(This,iidCount,iids) \\");
            sb.AppendLine($"    ( (This)->lpVtbl->GetIids(This,iidCount,iids) )");
            sb.AppendLine();
            sb.AppendLine($"#define {fiName}_GetRuntimeClassName(This,className) \\");
            sb.AppendLine($"    ( (This)->lpVtbl->GetRuntimeClassName(This,className) )");
            sb.AppendLine();
            sb.AppendLine($"#define {fiName}_GetTrustLevel(This,trustLevel) \\");
            sb.AppendLine($"    ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )");
            sb.AppendLine();
        }

        foreach (var (methodName, _) in methods)
        {
            var macroParams = GetMacroParamsForMethod(methodName);
            sb.AppendLine($"#define {fiName}_{methodName}({macroParams}) \\");
            sb.AppendLine($"    ( (This)->lpVtbl->{methodName}({macroParams}) )");
            sb.AppendLine();
        }
    }

    private static string GetMacroParamsForMethod(string methodName)
    {
        // Known method signatures for macro generation
        return methodName switch
        {
            "Invoke" => "This,sender,args",
            "First" => "This,result",
            "GetAt" => "This,index,result",
            "get_Size" => "This,result",
            "GetView" => "This,result",
            "IndexOf" => "This,value,index,result",
            "SetAt" => "This,index,value",
            "InsertAt" => "This,index,value",
            "RemoveAt" => "This,index",
            "Append" => "This,value",
            "RemoveAtEnd" => "This",
            "Clear" => "This",
            "GetMany" => "This,startIndex,itemsLength,items,result",
            "ReplaceAll" => "This,countItems,items",
            "Lookup" => "This,key,result",
            "HasKey" => "This,key,result",
            "Insert" => "This,key,value,result",
            "Remove" => "This,key",
            "get_Current" => "This,result",
            "get_HasCurrent" => "This,result",
            "MoveNext" => "This,result",
            "GetResults" => "This,result",
            "get_Value" => "This,result",
            _ => "This",
        };
    }

    /// <summary>
    /// Returns the list of vtable methods for a known parameterized WinRT type,
    /// with type arguments substituted.
    /// </summary>
    private static List<(string Name, string Params)> GetParameterizedVtableMethods(TypeRef typeRef)
    {
        var baseName = typeRef.Name;

        // Map type arguments to C types
        var cTypeArgs = typeRef.TypeArguments.Select(a => MapTypeForC(a)).ToList();

        return baseName switch
        {
            // Delegates (derive from IUnknown)
            "Windows.Foundation.TypedEventHandler" =>
            [
                ("Invoke", $", {cTypeArgs[0]} sender, {cTypeArgs[1]} args"),
            ],
            "Windows.Foundation.EventHandler" =>
            [
                ("Invoke", $", IInspectable * sender, {cTypeArgs[0]} args"),
            ],

            // IVector<T>
            "Windows.Foundation.Collections.IVector" =>
            [
                ("GetAt", $", unsigned int index, {AddOutPtr(cTypeArgs[0])} result"),
                ("get_Size", ", unsigned int * result"),
                ("GetView", ", __FIVectorView_1_* * result"),
                ("IndexOf", $", {cTypeArgs[0]} value, unsigned int * index, boolean * result"),
                ("SetAt", $", unsigned int index, {cTypeArgs[0]} value"),
                ("InsertAt", $", unsigned int index, {cTypeArgs[0]} value"),
                ("RemoveAt", ", unsigned int index"),
                ("Append", $", {cTypeArgs[0]} value"),
                ("RemoveAtEnd", ""),
                ("Clear", ""),
                ("GetMany", $", unsigned int startIndex, unsigned int itemsLength, {AddOutPtr(cTypeArgs[0])} items, unsigned int * result"),
                ("ReplaceAll", $", unsigned int countItems, {cTypeArgs[0]} * items"),
            ],

            // IVectorView<T>
            "Windows.Foundation.Collections.IVectorView" =>
            [
                ("GetAt", $", unsigned int index, {AddOutPtr(cTypeArgs[0])} result"),
                ("get_Size", ", unsigned int * result"),
                ("IndexOf", $", {cTypeArgs[0]} value, unsigned int * index, boolean * result"),
                ("GetMany", $", unsigned int startIndex, unsigned int itemsLength, {AddOutPtr(cTypeArgs[0])} items, unsigned int * result"),
            ],

            // IIterable<T>
            "Windows.Foundation.Collections.IIterable" =>
            [
                ("First", ", __FIIterator_1_* * result"),
            ],

            // IIterator<T>
            "Windows.Foundation.Collections.IIterator" =>
            [
                ("get_Current", $", {AddOutPtr(cTypeArgs[0])} result"),
                ("get_HasCurrent", ", boolean * result"),
                ("MoveNext", ", boolean * result"),
                ("GetMany", $", unsigned int itemsLength, {AddOutPtr(cTypeArgs[0])} items, unsigned int * result"),
            ],

            // IMap<K,V>
            "Windows.Foundation.Collections.IMap" =>
            [
                ("Lookup", $", {cTypeArgs[0]} key, {AddOutPtr(cTypeArgs[1])} result"),
                ("get_Size", ", unsigned int * result"),
                ("HasKey", $", {cTypeArgs[0]} key, boolean * result"),
                ("GetView", ", __FIMapView_2_* * result"),
                ("Insert", $", {cTypeArgs[0]} key, {cTypeArgs[1]} value, boolean * result"),
                ("Remove", $", {cTypeArgs[0]} key"),
                ("Clear", ""),
            ],

            // IMapView<K,V>
            "Windows.Foundation.Collections.IMapView" =>
            [
                ("Lookup", $", {cTypeArgs[0]} key, {AddOutPtr(cTypeArgs[1])} result"),
                ("get_Size", ", unsigned int * result"),
                ("HasKey", $", {cTypeArgs[0]} key, boolean * result"),
            ],

            // IKeyValuePair<K,V>
            "Windows.Foundation.Collections.IKeyValuePair" =>
            [
                ("get_Key", $", {AddOutPtr(cTypeArgs[0])} result"),
                ("get_Value", $", {AddOutPtr(cTypeArgs[1])} result"),
            ],

            // IAsyncOperation<T>
            "Windows.Foundation.IAsyncOperation" =>
            [
                ("GetResults", $", {AddOutPtr(cTypeArgs[0])} result"),
            ],

            // IObservableVector<T>
            "Windows.Foundation.Collections.IObservableVector" =>
            [
                ("add_VectorChanged", ", __FIVectorChangedEventHandler_1_* handler, EventRegistrationToken * token"),
                ("remove_VectorChanged", ", EventRegistrationToken token"),
            ],

            // IObservableMap<K,V>
            "Windows.Foundation.Collections.IObservableMap" =>
            [
                ("add_MapChanged", ", __FIMapChangedEventHandler_2_* handler, EventRegistrationToken * token"),
                ("remove_MapChanged", ", EventRegistrationToken token"),
            ],

            // IReference<T>
            "Windows.Foundation.IReference" =>
            [
                ("get_Value", $", {AddOutPtr(cTypeArgs[0])} result"),
            ],

            // IAsyncOperationWithProgress<T,P>
            "Windows.Foundation.IAsyncOperationWithProgress" =>
            [
                ("GetResults", $", {AddOutPtr(cTypeArgs[0])} result"),
            ],

            // IAsyncActionWithProgress<P>
            "Windows.Foundation.IAsyncActionWithProgress" =>
            [],

            // Fallback: no methods known
            _ => [],
        };
    }

    /// <summary>
    /// Maps a TypeRef to its C ABI type for use in parameterized vtable parameters.
    /// Pointer types (interfaces, delegates, Object, String) include the pointer star.
    /// </summary>
    // MapTypeForC is defined in HeaderEmitter.cs

    /// <summary>
    /// If the C type already ends with *, return it plus another * for out param.
    /// Otherwise add " *".
    /// </summary>
    private static string AddOutPtr(string cType)
    {
        if (cType.EndsWith("*"))
            return cType + " *";
        return cType + " *";
    }

    /// <summary>
    /// Maps a TypeRef to its C++ template argument form (with pointer for ref types).
    /// </summary>
    private static string MapTypeToCppTemplateArg(TypeRef typeRef)
    {
        if (typeRef.TypeArguments.Count > 0)
        {
            // Nested generic: use the C++ form
            var baseName = typeRef.Name;
            var lastDot = baseName.LastIndexOf('.');
            var shortName = lastDot >= 0 ? baseName[(lastDot + 1)..] : baseName;
            var nsPart = lastDot >= 0 ? baseName[..lastDot].Replace(".", "::") : "";
            var innerArgs = string.Join(",", typeRef.TypeArguments.Select(MapTypeToCppTemplateArg));
            var prefix = nsPart.Length > 0 ? $"{nsPart}::" : "";
            return $"{prefix}{shortName}<{innerArgs}>";
        }

        return typeRef.Name switch
        {
            "Object" => "IInspectable*",
            "String" => "HSTRING",
            "Int16" => "short",
            "Int32" => "int",
            "Int64" => "__int64",
            "UInt8" => "BYTE",
            "UInt16" => "unsigned short",
            "UInt32" => "unsigned int",
            "UInt64" => "unsigned __int64",
            "Single" => "float",
            "Double" => "double",
            "Boolean" => "boolean",
            "Char" => "wchar_t",
            "Guid" => "GUID",
            _ when typeRef.Name.Contains('.') =>
                $"{typeRef.Name.Replace(".", "::")}*",
            _ when typeRef.ResolvedSymbol is InterfaceSymbol or DelegateSymbol =>
                $"{typeRef.ResolvedSymbol.FullName.Replace(".", "::")}*",
            _ when typeRef.ResolvedSymbol is RuntimeClassSymbol =>
                $"{typeRef.ResolvedSymbol.FullName.Replace(".", "::")}*",
            _ => typeRef.Name,
        };
    }

    /// <summary>
    /// Gets the WinRT type name for a type argument, used in z_get_rc_name_impl.
    /// </summary>
    private static string GetWinRTTypeName(TypeRef typeRef)
    {
        if (typeRef.TypeArguments.Count > 0)
        {
            var innerArgs = string.Join(", ", typeRef.TypeArguments.Select(GetWinRTTypeName));
            return $"{typeRef.Name}`{typeRef.TypeArguments.Count}<{innerArgs}>";
        }

        // Primitives use simple names in WinRT
        if (typeRef.ResolvedSymbol != null)
            return typeRef.ResolvedSymbol.FullName;

        return typeRef.Name;
    }

    /// <summary>
    /// Returns whether a parameterized type derives from IInspectable (true) or IUnknown (false).
    /// </summary>
    private static bool IsParameterizedTypeInspectable(string fullName)
    {
        return KnownParameterizedIsInspectable.TryGetValue(fullName, out var isInsp) && isInsp;
    }

    /// <summary>
    /// Computes the parameterized interface IID using PiidGenerator.
    /// Falls back to a deterministic UUID v5 from the __FI name if the pinterface GUID is unknown.
    /// </summary>
    private static Guid ComputeParameterizedIid(TypeRef typeRef)
    {
        // Try to get the pinterface GUID from the resolved symbol
        Guid? pinterfaceGuid = null;
        if (typeRef.ResolvedSymbol is InterfaceSymbol iface && iface.Uuid.HasValue)
            pinterfaceGuid = iface.Uuid.Value;
        else if (typeRef.ResolvedSymbol is DelegateSymbol del && del.Uuid.HasValue)
            pinterfaceGuid = del.Uuid.Value;

        if (pinterfaceGuid.HasValue)
        {
            var typeArgSigs = typeRef.TypeArguments.Select(a => GetTypeArgSignature(a)).ToList();
            return PiidGenerator.GenerateIid(pinterfaceGuid.Value.ToString("D"), typeArgSigs);
        }

        // Fallback: use the FI name as a seed for deterministic GUID
        var fiName = ComputeFIMangledName(typeRef);
        return new Guid(HeaderEmitter.GenerateUuid(fiName));
    }

    /// <summary>
    /// Gets the WinRT type signature for a type argument, used in PIID computation.
    /// </summary>
    private static string GetTypeArgSignature(TypeRef typeRef)
    {
        if (typeRef.ResolvedSymbol != null)
            return PiidGenerator.GetTypeSignature(typeRef.ResolvedSymbol);

        // For parameterized type args, compute recursively
        if (typeRef.TypeArguments.Count > 0)
        {
            var fiName = ComputeFIMangledName(typeRef);
            // Use pinterface signature
            return fiName;
        }

        // For unresolved primitives
        return PiidGenerator.GetPrimitiveTypeSignature(typeRef.Name);
    }
}
