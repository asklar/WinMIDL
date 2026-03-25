using System.Text;
using Midl.Analysis.Symbols;

namespace Midl.CodeGen.Header;

/// <summary>
/// Partial class for emitting the C compatibility block:
/// C forward declarations, vtable structs, and COBJMACROS.
/// </summary>
public partial class HeaderEmitter
{
    /// <summary>
    /// Emits the full C compatibility block (#else branch) for all interfaces and delegates.
    /// </summary>
    private static void EmitCBlock(StringBuilder sb, List<ISymbol> cppTypes,
        List<TypeRef>? parameterizedInstantiations = null,
        List<RuntimeClassSymbol>? runtimeClasses = null)
    {
        sb.AppendLine("#else // !defined(__cplusplus)");
        sb.AppendLine();

        EmitCForwardDeclarations(sb, cppTypes);

        // Section comment headers (matching real midl.exe)
        sb.AppendLine("// Parameterized interface forward declarations (C)");
        sb.AppendLine();
        sb.AppendLine("// Collection interface definitions");
        sb.AppendLine();

        // Parameterized interface C vtable structs
        if (parameterizedInstantiations != null)
            EmitParameterizedInterfacesCBlock(sb, parameterizedInstantiations);

        foreach (var symbol in cppTypes)
        {
            switch (symbol)
            {
                case InterfaceSymbol iface:
                    EmitCInterface(sb, iface);
                    break;
                case DelegateSymbol d:
                    EmitCDelegate(sb, d);
                    break;
            }
        }

        // RuntimeClass definitions (inside C block)
        if (runtimeClasses != null)
        {
            foreach (var rc in runtimeClasses)
                EmitRuntimeClassWithComment(sb, rc);
        }
    }

    /// <summary>
    /// Emits C-style forward declarations (typedef interface) with FWD_DEFINED guards.
    /// </summary>
    private static void EmitCForwardDeclarations(StringBuilder sb, List<ISymbol> cppTypes)
    {
        sb.AppendLine("/* Forward Declarations */");

        foreach (var symbol in cppTypes)
        {
            var mangledName = MangleName(symbol.FullName);
            var fwdGuard = $"__{mangledName}_FWD_DEFINED__";

            sb.AppendLine($"#ifndef {fwdGuard}");
            sb.AppendLine($"#define {fwdGuard}");
            sb.AppendLine($"typedef interface {mangledName} {mangledName};");
            sb.AppendLine($"#endif // {fwdGuard}");
            sb.AppendLine();
        }
    }

    /// <summary>
    /// Emits a full C interface definition: vtable struct, interface struct, and COBJMACROS.
    /// </summary>
    private static void EmitCInterface(StringBuilder sb, InterfaceSymbol iface)
    {
        var mangledName = MangleName(iface.FullName);
        var guard = $"____{mangledName.TrimStart('_')}_INTERFACE_DEFINED__";
        var underscoreName = iface.FullName.Replace(".", "_");

        sb.AppendLine($"#if !defined({guard})");
        sb.AppendLine($"#define {guard}");
        sb.AppendLine();

        sb.AppendLine($"extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_{underscoreName}[] = L\"{iface.FullName}\";");
        sb.AppendLine();

        EmitCVtable(sb, mangledName, iface.Members, isInspectable: true);

        // Interface struct
        sb.AppendLine($"interface {mangledName}");
        sb.AppendLine("{");
        sb.AppendLine($"    CONST_VTBL struct {mangledName}Vtbl *lpVtbl;");
        sb.AppendLine("};");
        sb.AppendLine();

        EmitCObjMacros(sb, mangledName, iface.Members, isInspectable: true);

        sb.AppendLine($"#endif // {guard}");
        sb.AppendLine();
    }

    /// <summary>
    /// Emits a full C delegate definition: vtable struct, interface struct, and COBJMACROS.
    /// Delegates derive from IUnknown (not IInspectable).
    /// </summary>
    private static void EmitCDelegate(StringBuilder sb, DelegateSymbol d)
    {
        var mangledName = MangleName(d.FullName);
        var guard = $"____{mangledName.TrimStart('_')}_INTERFACE_DEFINED__";

        sb.AppendLine($"#if !defined({guard})");
        sb.AppendLine($"#define {guard}");
        sb.AppendLine();

        // Build a synthetic Invoke member list for the vtable
        var invokeMethod = new MethodSymbol("Invoke", null) { ReturnType = d.ReturnType };
        invokeMethod.Parameters.AddRange(d.Parameters);
        var members = new List<ISymbol> { invokeMethod };

        EmitCVtable(sb, mangledName, members, isInspectable: false);

        // Interface struct
        sb.AppendLine($"interface {mangledName}");
        sb.AppendLine("{");
        sb.AppendLine($"    CONST_VTBL struct {mangledName}Vtbl *lpVtbl;");
        sb.AppendLine("};");
        sb.AppendLine();

        EmitCObjMacros(sb, mangledName, members, isInspectable: false);

        sb.AppendLine($"#endif // {guard}");
        sb.AppendLine();
    }

    /// <summary>
    /// Emits the vtable struct with base methods and user methods as function pointers.
    /// </summary>
    private static void EmitCVtable(StringBuilder sb, string mangledName, IReadOnlyList<ISymbol> members, bool isInspectable)
    {
        sb.AppendLine($"typedef struct {mangledName}Vtbl");
        sb.AppendLine("{");
        sb.AppendLine("    BEGIN_INTERFACE");

        // IUnknown base methods
        sb.AppendLine($"    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in {mangledName} * This, __RPC__in REFIID riid, _COM_Outptr_ void **ppvObject);");
        sb.AppendLine($"    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in {mangledName} * This);");
        sb.AppendLine($"    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in {mangledName} * This);");

        // IInspectable base methods (only for interfaces, not delegates)
        if (isInspectable)
        {
            sb.AppendLine($"    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in {mangledName} * This, __RPC__out ULONG *iidCount, __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);");
            sb.AppendLine($"    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in {mangledName} * This, __RPC__deref_out_opt HSTRING *className);");
            sb.AppendLine($"    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in {mangledName} * This, __RPC__out TrustLevel *trustLevel);");
        }

        // User methods
        foreach (var member in members)
        {
            switch (member)
            {
                case PropertySymbol prop:
                    EmitCVtableProperty(sb, mangledName, prop);
                    break;
                case MethodSymbol method:
                    EmitCVtableMethod(sb, mangledName, method);
                    break;
                case EventSymbol evt:
                    EmitCVtableEvent(sb, mangledName, evt);
                    break;
            }
        }

        sb.AppendLine("    END_INTERFACE");
        sb.AppendLine($"}} {mangledName}Vtbl;");
        sb.AppendLine();
    }

    private static void EmitCVtableMethod(StringBuilder sb, string mangledName, MethodSymbol method)
    {
        var paramParts = new List<string> { $"{mangledName} * This" };

        foreach (var p in method.Parameters)
        {
            var cType = MapTypeForC(p.Type);
            if (p.Direction == ParameterDirection.Out)
                paramParts.Add($"/* [out] */{cType} * {p.Name}");
            else if (p.Direction == ParameterDirection.RefConst)
                paramParts.Add($"/* [in] */{cType} * {p.Name}");
            else
                paramParts.Add($"/* [in] */{cType} {p.Name}");
        }

        if (method.ReturnType.Name != "void")
        {
            var retType = MapTypeForC(method.ReturnType);
            paramParts.Add($"/* [out, retval] */{retType} * result");
        }

        var paramStr = string.Join(", ", paramParts);
        sb.AppendLine($"    HRESULT ( STDMETHODCALLTYPE *{method.Name} )({paramStr});");
    }

    private static void EmitCVtableProperty(StringBuilder sb, string mangledName, PropertySymbol prop)
    {
        var cType = MapTypeForC(prop.Type);
        if (prop.HasGetter)
        {
            sb.AppendLine($"    /* [propget] */HRESULT ( STDMETHODCALLTYPE *get_{prop.Name} )({mangledName} * This, /* [out, retval] */{cType} * value);");
        }
        if (prop.HasSetter)
        {
            sb.AppendLine($"    /* [propput] */HRESULT ( STDMETHODCALLTYPE *put_{prop.Name} )({mangledName} * This, /* [in] */{cType} value);");
        }
    }

    private static void EmitCVtableEvent(StringBuilder sb, string mangledName, EventSymbol evt)
    {
        var delegateType = MapTypeForC(evt.DelegateType);
        sb.AppendLine($"    /* [eventadd] */HRESULT ( STDMETHODCALLTYPE *add_{evt.Name} )({mangledName} * This, /* [in] */{delegateType} handler, /* [out, retval] */EventRegistrationToken * token);");
        sb.AppendLine($"    /* [eventremove] */HRESULT ( STDMETHODCALLTYPE *remove_{evt.Name} )({mangledName} * This, /* [in] */EventRegistrationToken token);");
    }

    /// <summary>
    /// Emits COBJMACROS (#define wrappers) for all vtable methods.
    /// </summary>
    private static void EmitCObjMacros(StringBuilder sb, string mangledName, IReadOnlyList<ISymbol> members, bool isInspectable)
    {
        sb.AppendLine("#ifdef COBJMACROS");

        // IUnknown macros
        sb.AppendLine($"#define {mangledName}_QueryInterface(This,riid,ppvObject) \\");
        sb.AppendLine($"    ( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )");
        sb.AppendLine();
        sb.AppendLine($"#define {mangledName}_AddRef(This) \\");
        sb.AppendLine($"    ( (This)->lpVtbl->AddRef(This) )");
        sb.AppendLine();
        sb.AppendLine($"#define {mangledName}_Release(This) \\");
        sb.AppendLine($"    ( (This)->lpVtbl->Release(This) )");
        sb.AppendLine();

        // IInspectable macros
        if (isInspectable)
        {
            sb.AppendLine($"#define {mangledName}_GetIids(This,iidCount,iids) \\");
            sb.AppendLine($"    ( (This)->lpVtbl->GetIids(This,iidCount,iids) )");
            sb.AppendLine();
            sb.AppendLine($"#define {mangledName}_GetRuntimeClassName(This,className) \\");
            sb.AppendLine($"    ( (This)->lpVtbl->GetRuntimeClassName(This,className) )");
            sb.AppendLine();
            sb.AppendLine($"#define {mangledName}_GetTrustLevel(This,trustLevel) \\");
            sb.AppendLine($"    ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )");
            sb.AppendLine();
        }

        // User method macros
        foreach (var member in members)
        {
            switch (member)
            {
                case PropertySymbol prop:
                    EmitCObjMacroProperty(sb, mangledName, prop);
                    break;
                case MethodSymbol method:
                    EmitCObjMacroMethod(sb, mangledName, method);
                    break;
                case EventSymbol evt:
                    EmitCObjMacroEvent(sb, mangledName, evt);
                    break;
            }
        }

        sb.AppendLine("#endif /* COBJMACROS */");
        sb.AppendLine();
    }

    private static void EmitCObjMacroMethod(StringBuilder sb, string mangledName, MethodSymbol method)
    {
        var paramNames = new List<string> { "This" };
        foreach (var p in method.Parameters)
            paramNames.Add(p.Name);
        if (method.ReturnType.Name != "void")
            paramNames.Add("result");

        var macroParams = string.Join(",", paramNames);
        var callParams = string.Join(",", paramNames);

        sb.AppendLine($"#define {mangledName}_{method.Name}({macroParams}) \\");
        sb.AppendLine($"    ( (This)->lpVtbl->{method.Name}({callParams}) )");
        sb.AppendLine();
    }

    private static void EmitCObjMacroProperty(StringBuilder sb, string mangledName, PropertySymbol prop)
    {
        if (prop.HasGetter)
        {
            sb.AppendLine($"#define {mangledName}_get_{prop.Name}(This,value) \\");
            sb.AppendLine($"    ( (This)->lpVtbl->get_{prop.Name}(This,value) )");
            sb.AppendLine();
        }
        if (prop.HasSetter)
        {
            sb.AppendLine($"#define {mangledName}_put_{prop.Name}(This,value) \\");
            sb.AppendLine($"    ( (This)->lpVtbl->put_{prop.Name}(This,value) )");
            sb.AppendLine();
        }
    }

    private static void EmitCObjMacroEvent(StringBuilder sb, string mangledName, EventSymbol evt)
    {
        sb.AppendLine($"#define {mangledName}_add_{evt.Name}(This,handler,token) \\");
        sb.AppendLine($"    ( (This)->lpVtbl->add_{evt.Name}(This,handler,token) )");
        sb.AppendLine();
        sb.AppendLine($"#define {mangledName}_remove_{evt.Name}(This,token) \\");
        sb.AppendLine($"    ( (This)->lpVtbl->remove_{evt.Name}(This,token) )");
        sb.AppendLine();
    }
}
