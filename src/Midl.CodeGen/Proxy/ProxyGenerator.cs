using System.Text;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Proxy;

/// <summary>
/// Generates C++ proxy classes that implement IRpcProxyBuffer for WinRT interface marshaling.
/// Each proxy class implements both the target interface and IRpcProxyBuffer,
/// replacing NDR with direct compiled serialization.
/// </summary>
public class ProxyGenerator
{
    // IUnknown occupies vtable slots 0-2, IInspectable 3-5, user methods start at 6
    private static readonly int UserMethodStartSlot = WireFormat.GetFirstUserSlot(isInspectable: true);

    public string Generate(Compilation compilation, bool useExceptions = false)
    {
        var sb = new StringBuilder();
        EmitPrologue(sb);

        var allTypes = compilation.GetAllTypes().Where(t => !t.IsExternal).ToList();

        // Build interface → runtimeclass reverse map, and runtimeclass → all IIDs map
        var ifaceToRcName = new Dictionary<string, string>();
        var rcToAllIids = new Dictionary<string, List<string>>();

        // First pass: collect direct IIDs for each runtimeclass
        foreach (var sym in allTypes)
        {
            if (sym is RuntimeClassSymbol rc)
            {
                var iids = new List<string>();

                // Synthesized exclusive interfaces (from ImplementedInterfaces)
                foreach (var impl in rc.ImplementedInterfaces)
                {
                    var iid = $"IID_{HeaderEmitter.MangleName(impl.FullName)}";
                    if (!iids.Contains(iid)) iids.Add(iid);
                    ifaceToRcName[impl.FullName] = rc.FullName;
                }

                // Declared base class — may be an interface (`: IPuppy`)
                if (rc.BaseClass?.ResolvedSymbol is InterfaceSymbol baseIface)
                {
                    var iid = $"IID_{HeaderEmitter.MangleName(baseIface.FullName)}";
                    if (!iids.Contains(iid)) iids.Add(iid);
                    ifaceToRcName[baseIface.FullName] = rc.FullName;
                }

                // Declared interfaces (from Interfaces TypeRef list)
                foreach (var ifaceRef in rc.Interfaces)
                {
                    if (ifaceRef.ResolvedSymbol is InterfaceSymbol resolved)
                    {
                        var iid = $"IID_{HeaderEmitter.MangleName(resolved.FullName)}";
                        if (!iids.Contains(iid)) iids.Add(iid);
                        ifaceToRcName[resolved.FullName] = rc.FullName;
                    }
                }

                // Default/factory/statics/protected/overrides interfaces
                foreach (var special in new[] { rc.DefaultInterface, rc.FactoryInterface, rc.StaticsInterface, rc.ProtectedInterface, rc.OverridesInterface })
                {
                    if (special != null)
                    {
                        var iid = $"IID_{HeaderEmitter.MangleName(special.FullName)}";
                        if (!iids.Contains(iid)) iids.Add(iid);
                        ifaceToRcName[special.FullName] = rc.FullName;
                    }
                }

                rcToAllIids[rc.FullName] = iids;
            }
        }

        // Second pass: inherit IIDs from base runtimeclasses (composable/unsealed)
        // Walk up the full inheritance chain for each runtimeclass
        foreach (var sym in allTypes)
        {
            if (sym is RuntimeClassSymbol rc && rcToAllIids.ContainsKey(rc.FullName))
            {
                var derivedIids = rcToAllIids[rc.FullName];
                var current = rc;
                while (current.BaseClass?.ResolvedSymbol is RuntimeClassSymbol baseRc)
                {
                    if (rcToAllIids.TryGetValue(baseRc.FullName, out var baseIids))
                    {
                        foreach (var iid in baseIids)
                        {
                            if (!derivedIids.Contains(iid)) derivedIids.Add(iid);
                        }
                    }
                    current = baseRc;
                }
            }
        }

        // Track emitted shape functions and descriptor tables at file level to avoid duplicates
        var emittedShapesGlobal = new HashSet<string>();
        var emittedDescriptorsGlobal = new HashSet<string>();

        foreach (var symbol in allTypes)
        {
            if (symbol is InterfaceSymbol iface)
            {
                // Find all sibling IIDs if this interface belongs to a runtimeclass
                List<string>? allIids = null;
                string? rcName = null;
                if (ifaceToRcName.TryGetValue(iface.FullName, out rcName))
                    rcToAllIids.TryGetValue(rcName, out allIids);
                EmitProxyClass(sb, iface, allIids, rcName, useExceptions, emittedShapesGlobal, emittedDescriptorsGlobal);
            }
        }

        EmitEpilogue(sb);
        return sb.ToString();
    }

    public void GenerateToFile(Compilation compilation, string outputPath)
    {
        var content = Generate(compilation, useExceptions: false);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
        File.WriteAllText(outputPath, content);
    }

    private static void EmitPrologue(StringBuilder sb)
    {
        sb.AppendLine("// Proxy classes auto-generated by MIDL compiler");
        sb.AppendLine("// Direct compiled serialization — replaces NDR format strings");
        sb.AppendLine();
        WireFormat.EmitConsolidatedIncludes(sb);
        sb.AppendLine("#include \"winmidl/proxy_helpers.h\"");
        sb.AppendLine();
    }

    private static void EmitEpilogue(StringBuilder sb)
    {
        sb.AppendLine("// End of generated proxy file");
    }

    private static void EmitMethodDescriptorTables(StringBuilder sb, List<(string Name, List<ProxyParam> Params, TypeRef ReturnType, MemberKind Kind)> methods, string mangledName, string iidName)
    {
        if (methods.Count == 0)
            return;

        int slot = UserMethodStartSlot;
        foreach (var (methodName, parameters, returnType, _) in methods)
        {
            var allParams = new List<(TypeRef Type, string Dir)>();
            foreach (var p in parameters)
            {
                var dir = p.Direction == ParamDirection.Out ? "winmidl::param_dir::dir_out" : "winmidl::param_dir::dir_in";
                allParams.Add((p.Type, dir));
            }
            if (returnType.Name != "void")
            {
                allParams.Add((returnType, "winmidl::param_dir::dir_retval"));
            }

            if (allParams.Count > 0)
            {
                sb.AppendLine($"static constexpr winmidl::param_descriptor {mangledName}_{methodName}_params[] = {{");
                foreach (var (type, dir) in allParams)
                {
                    var typeEnum = WireFormat.GetParamTypeEnum(type) ?? "winmidl::param_type::t_int32";
                    sb.AppendLine($"    {{ {typeEnum}, {dir} }},");
                }
                sb.AppendLine("};");
            }
            slot++;
        }

        // Method descriptor table
        slot = UserMethodStartSlot;
        sb.AppendLine($"static constexpr winmidl::method_descriptor {mangledName}_methods[] = {{");
        foreach (var (methodName, parameters, returnType, _) in methods)
        {
            var paramCount = parameters.Count + (returnType.Name != "void" ? 1 : 0);
            var paramsRef = paramCount > 0 ? $"{mangledName}_{methodName}_params" : "nullptr";
            sb.AppendLine($"    {{ {slot}, \"{methodName}\", {paramsRef}, {paramCount} }},");
            slot++;
        }
        sb.AppendLine("};");

        // Interface descriptor
        sb.AppendLine($"static constexpr winmidl::interface_descriptor {mangledName}_desc = {{");
        sb.AppendLine($"    &{iidName}, {mangledName}_methods, {methods.Count}");
        sb.AppendLine("};");
        sb.AppendLine();
    }

    /// <summary>
    /// Computes a shape key for a method signature for signature folding.
    /// Methods with identical shapes share proxy/stub implementations.
    /// </summary>
    internal static string ComputeShapeKey(List<ProxyParam> parameters, TypeRef returnType)
    {
        var parts = new List<string>();
        foreach (var p in parameters.Where(p => p.Direction == ParamDirection.In))
        {
            parts.Add(GetShapeTypeName(p.Type));
        }
        var inSig = parts.Count > 0 ? string.Join("_", parts) : "void";
        var outSig = returnType.Name != "void" ? GetShapeTypeName(returnType) : "void";
        return $"{inSig}_to_{outSig}";
    }

    private static string GetShapeTypeName(TypeRef typeRef)
    {
        if (typeRef.ResolvedSymbol is EnumSymbol) return "enum";
        if (typeRef.ResolvedSymbol is StructSymbol s) return $"struct_{s.Name}";
        return typeRef.Name switch
        {
            "String" => "hstring",
            "Int32" => "int32",
            "UInt32" => "uint32",
            "Int64" => "int64",
            "UInt64" => "uint64",
            "Boolean" => "bool",
            "Single" => "float",
            "Double" => "double",
            "Guid" => "guid",
            "Int16" => "int16",
            "UInt16" => "uint16",
            "UInt8" => "uint8",
            "Char" => "char16",
            "EventRegistrationToken" => "event_token",
            _ => typeRef.Name.ToLowerInvariant(),
        };
    }

    /// <summary>
    /// Emits a shared static proxy function for a given method shape (signature).
    /// Multiple methods with the same in/out type pattern share this single implementation.
    /// </summary>
    private static void EmitShapeFunction(StringBuilder sb, string shapeKey, List<ProxyParam> templateParams, TypeRef templateReturn, bool useExceptions, bool isProxy)
    {
        var shapeName = $"proxy_shape_{shapeKey}";
        var inParams = templateParams.Where(p => p.Direction == ParamDirection.In).ToList();
        var outParams = templateParams.Where(p => p.Direction == ParamDirection.Out).ToList();
        bool hasRetval = templateReturn.Name != "void";

        // Build parameter list for the shape function
        var sigParts = new List<string> { "IRpcChannelBuffer* channel", "const GUID& iid", "UINT32 slot" };
        int argIdx = 0;
        foreach (var p in inParams)
        {
            var cType = HeaderEmitter.MapType(p.Type);
            sigParts.Add($"{cType} arg{argIdx}");
            argIdx++;
        }
        foreach (var p in outParams)
        {
            var cType = HeaderEmitter.MapType(p.Type);
            sigParts.Add($"{cType}* out{argIdx}");
            argIdx++;
        }
        if (hasRetval)
        {
            var retCType = HeaderEmitter.MapType(templateReturn);
            sigParts.Add($"{retCType}* retval");
        }

        sb.AppendLine($"// Shared proxy shape: {shapeKey}");
        sb.AppendLine($"static HRESULT {shapeName}({string.Join(", ", sigParts)}) {{");
        sb.AppendLine("    RPCOLEMESSAGE msg = {};");
        sb.AppendLine($"    msg.iMethod = slot;");

        // Buffer size
        bool requestFixed = WireFormat.AllParamsFixedSize(inParams);
        if (requestFixed && inParams.Count > 0)
        {
            var fixedExpr = WireFormat.ComputeFixedBufferSizeExpression(inParams);
            sb.AppendLine($"    msg.cbBuffer = {fixedExpr};");
        }
        else if (inParams.Count > 0)
        {
            var bufExpr = WireFormat.ComputeBufferSize(inParams.Select((p, i) => new ProxyParam($"arg{i}", p.Type, p.Direction)).ToList());
            sb.AppendLine($"    msg.cbBuffer = {bufExpr};");
        }
        else
        {
            sb.AppendLine("    msg.cbBuffer = 0;");
        }

        sb.AppendLine("    HRESULT hr = channel->GetBuffer(&msg, iid);");
        sb.AppendLine("    RETURN_IF_FAILED(hr);");

        // Serialize input params
        if (inParams.Count > 0)
        {
            sb.AppendLine("    BYTE* p = (BYTE*)msg.Buffer;");
            argIdx = 0;
            foreach (var p in inParams)
            {
                WireFormat.EmitSerialize(sb, p.Type, $"arg{argIdx}", "p", "    ");
                argIdx++;
            }
        }

        sb.AppendLine("    ULONG status;");
        sb.AppendLine("    hr = channel->SendReceive(&msg, &status);");
        sb.AppendLine("    RETURN_IF_FAILED(hr);");

        sb.AppendLine("    BYTE* pReply = (BYTE*)msg.Buffer;");

        // Deserialize out params
        argIdx = inParams.Count;
        foreach (var p in outParams)
        {
            WireFormat.EmitDeserializeReply(sb, p.Type, $"out{argIdx}", "pReply", isPointer: true, "    ");
            argIdx++;
        }

        // Deserialize retval
        if (hasRetval)
        {
            WireFormat.EmitDeserializeReply(sb, templateReturn, "retval", "pReply", isPointer: true, "    ");
        }

        sb.AppendLine("    HRESULT retHr; memcpy(&retHr, pReply, sizeof(HRESULT));");
        sb.AppendLine("    hr = channel->FreeBuffer(&msg);");
        sb.AppendLine("    RETURN_IF_FAILED(hr);");
        sb.AppendLine("    return retHr;");
        sb.AppendLine("}");
        sb.AppendLine();
    }

    internal static void EmitProxyClass(StringBuilder sb, InterfaceSymbol iface, List<string>? allRuntimeClassIids = null, string? runtimeClassName = null, bool useExceptions = false, HashSet<string>? emittedShapesGlobal = null, HashSet<string>? emittedDescriptorsGlobal = null)
    {
        var mangledName = HeaderEmitter.MangleName(iface.FullName);
        var className = $"C{iface.Name}Proxy";
        var iidName = $"IID_{mangledName}";

        // Phase 6: Signature folding — emit shared shape functions before the class
        var methods = CollectInterfaceMethods(iface);
        var shapeSet = emittedShapesGlobal ?? new HashSet<string>();
        foreach (var (_, parameters, returnType, _) in methods)
        {
            var shapeKey = ComputeShapeKey(parameters, returnType);
            if (shapeSet.Add(shapeKey))
            {
                EmitShapeFunction(sb, shapeKey, parameters, returnType, useExceptions, isProxy: true);
            }
        }

        sb.AppendLine($"// Proxy for {iface.FullName}");
        sb.AppendLine($"class {className} : public IRpcProxyBuffer {{");
        sb.AppendLine("    IUnknown* m_pUnkOuter;");
        sb.AppendLine("    IRpcChannelBuffer* m_pChannel;");
        sb.AppendLine("    LONG m_cRef;");
        sb.AppendLine();

        // Emit the nested Face class that implements the interface with delegating IUnknown
        EmitFaceClass(sb, iface, mangledName, iidName, className, allRuntimeClassIids, runtimeClassName, useExceptions);

        sb.AppendLine("    Face m_face;");
        sb.AppendLine("    friend struct Face;");
        sb.AppendLine();

        // Declare kQIEntries and kTypeInfo (defined out-of-class after offsetof is valid)
        sb.AppendLine($"    static const winmidl::qi_entry_t kQIEntries[];");
        sb.AppendLine($"    static const winmidl::type_info_t kTypeInfo;");
        sb.AppendLine();
        sb.AppendLine("public:");

        // Constructor
        sb.AppendLine($"    {className}(IUnknown* pUnkOuter) : m_pUnkOuter(pUnkOuter), m_pChannel(nullptr), m_cRef(1), m_face(this) {{}}");
        sb.AppendLine();
        sb.AppendLine($"    {mangledName}* GetFace() {{ return &m_face; }}");
        sb.AppendLine();

        // IRpcProxyBuffer methods
        EmitConnect(sb);
        EmitDisconnect(sb);
        EmitNonDelegatingQI(sb, mangledName, iidName);
        EmitAddRef(sb);
        EmitRelease(sb, className);

        sb.AppendLine("};");
        sb.AppendLine();

        // Out-of-class definitions — offsetof requires a complete type
        var rcName = runtimeClassName ?? iface.ExclusiveToClass ?? iface.FullName;
        sb.AppendLine($"const winmidl::qi_entry_t {className}::kQIEntries[] = {{");
        sb.AppendLine($"    {{ IID_IUnknown, 0, false }},");
        sb.AppendLine($"    {{ IID_IRpcProxyBuffer, 0, false }},");
        sb.AppendLine($"    {{ {iidName}, static_cast<uint32_t>(offsetof({className}, m_face)), true }},");
        sb.AppendLine($"}};");
        sb.AppendLine($"const winmidl::type_info_t {className}::kTypeInfo = {{ {className}::Face::kIIDs, L\"{rcName}\", {className}::kQIEntries }};");
        sb.AppendLine();

        // Emit method descriptor tables (Phase 5) — reuse methods from signature folding
        var descSet = emittedDescriptorsGlobal ?? new HashSet<string>();
        if (descSet.Add(mangledName))
        {
            EmitMethodDescriptorTables(sb, methods, mangledName, iidName);
        }
    }

    private static void EmitFaceClass(StringBuilder sb, InterfaceSymbol iface, string mangledName, string iidName, string outerClassName, List<string>? allRuntimeClassIids, string? runtimeClassName, bool useExceptions)
    {
        sb.AppendLine($"    struct Face : public {mangledName} {{");
        sb.AppendLine($"        {outerClassName}* m_owner;");
        sb.AppendLine($"        Face({outerClassName}* owner) : m_owner(owner) {{}}");
        sb.AppendLine();

        // Delegating IUnknown — forwards to outer unknown
        sb.AppendLine("        // Delegating IUnknown — forwards to outer unknown");
        sb.AppendLine("        HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {");
        sb.AppendLine("            return m_owner->m_pUnkOuter->QueryInterface(riid, ppv);");
        sb.AppendLine("        }");
        sb.AppendLine("        ULONG STDMETHODCALLTYPE AddRef() override { return m_owner->m_pUnkOuter->AddRef(); }");
        sb.AppendLine("        ULONG STDMETHODCALLTYPE Release() override { return m_owner->m_pUnkOuter->Release(); }");
        sb.AppendLine();

        // IInspectable — static constexpr data, no forwarding
        var iidList = allRuntimeClassIids ?? new List<string> { iidName };
        EmitFaceIInspectable(sb, iidList);

        // Interface methods
        var methods = CollectInterfaceMethods(iface);
        int slot = UserMethodStartSlot;
        foreach (var (methodName, parameters, returnType, memberKind) in methods)
        {
            EmitFaceProxyMethod(sb, iface, methodName, parameters, returnType, memberKind, mangledName, slot, useExceptions);
            slot++;
        }

        sb.AppendLine("    };");
        sb.AppendLine();
    }

    private static void EmitFaceIInspectable(StringBuilder sb, List<string> iidNames)
    {
        var iidList = string.Join(", ", iidNames);
        sb.AppendLine("        // IInspectable — static data via shared helpers");
        sb.AppendLine($"        static constexpr IID kIIDs[] = {{ {iidList} }};");
        sb.AppendLine();
        sb.AppendLine("        HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {");
        sb.AppendLine("            return winmidl::get_iids(kTypeInfo, iidCount, iids);");
        sb.AppendLine("        }");
        sb.AppendLine("        HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {");
        sb.AppendLine("            return winmidl::get_runtime_class_name(kTypeInfo, className);");
        sb.AppendLine("        }");
        sb.AppendLine("        HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {");
        sb.AppendLine("            return winmidl::get_trust_level(trustLevel);");
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static void EmitConnect(StringBuilder sb)
    {
        sb.AppendLine("    // IRpcProxyBuffer::Connect");
        sb.AppendLine("    HRESULT STDMETHODCALLTYPE Connect(IRpcChannelBuffer* pChannel) override {");
        sb.AppendLine("        m_pChannel = pChannel;");
        sb.AppendLine("        if (pChannel) pChannel->AddRef();");
        sb.AppendLine("        return S_OK;");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void EmitDisconnect(StringBuilder sb)
    {
        sb.AppendLine("    // IRpcProxyBuffer::Disconnect");
        sb.AppendLine("    void STDMETHODCALLTYPE Disconnect() override {");
        sb.AppendLine("        if (m_pChannel) { m_pChannel->Release(); m_pChannel = nullptr; }");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void EmitNonDelegatingQI(StringBuilder sb, string mangledName, string iidName)
    {
        sb.AppendLine("    // Non-delegating IUnknown::QueryInterface — qi_check lookup");
        sb.AppendLine("    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {");
        sb.AppendLine("        return winmidl::qi_check(kTypeInfo, this, m_pUnkOuter, riid, ppv);");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void EmitAddRef(StringBuilder sb)
    {
        sb.AppendLine("    // IUnknown::AddRef");
        sb.AppendLine("    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }");
        sb.AppendLine();
    }

    private static void EmitRelease(StringBuilder sb, string className)
    {
        sb.AppendLine("    // IUnknown::Release");
        sb.AppendLine("    ULONG STDMETHODCALLTYPE Release() override {");
        sb.AppendLine("        ULONG c = InterlockedDecrement(&m_cRef);");
        sb.AppendLine("        if (c == 0) delete this;");
        sb.AppendLine("        return c;");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    /// <summary>
    /// Collects all ABI methods from an interface in vtable order:
    /// properties → getter then setter, events → add_ then remove_, methods in declaration order.
    /// </summary>
    internal static List<(string Name, List<ProxyParam> Params, TypeRef ReturnType, MemberKind Kind)> CollectInterfaceMethods(InterfaceSymbol iface)
    {
        var methods = new List<(string Name, List<ProxyParam> Params, TypeRef ReturnType, MemberKind Kind)>();

        foreach (var member in iface.Members)
        {
            switch (member)
            {
                case PropertySymbol prop:
                    if (prop.HasGetter)
                    {
                        methods.Add(($"get_{prop.Name}",
                            [],
                            prop.Type,
                            MemberKind.PropertyGetter));
                    }
                    if (prop.HasSetter)
                    {
                        methods.Add(($"put_{prop.Name}",
                            [new ProxyParam(prop.Name.ToLowerInvariant(), prop.Type, ParamDirection.In)],
                            TypeRef.Void,
                            MemberKind.PropertySetter));
                    }
                    break;

                case MethodSymbol method:
                    var methodParams = method.Parameters.Select(p =>
                        new ProxyParam(p.Name, p.Type,
                            p.Direction == ParameterDirection.Out ? ParamDirection.Out : ParamDirection.In))
                        .ToList();
                    methods.Add((method.Name, methodParams, method.ReturnType, MemberKind.Method));
                    break;

                case EventSymbol evt:
                    methods.Add(($"add_{evt.Name}",
                        [new ProxyParam("handler", evt.DelegateType, ParamDirection.In)],
                        new TypeRef("EventRegistrationToken"),
                        MemberKind.EventAdd));
                    methods.Add(($"remove_{evt.Name}",
                        [new ProxyParam("token", new TypeRef("EventRegistrationToken"), ParamDirection.In)],
                        TypeRef.Void,
                        MemberKind.EventRemove));
                    break;
            }
        }

        return methods;
    }

    private static void EmitFaceProxyMethod(
        StringBuilder sb,
        InterfaceSymbol iface,
        string methodName,
        List<ProxyParam> parameters,
        TypeRef returnType,
        MemberKind memberKind,
        string mangledName,
        int vtableSlot,
        bool useExceptions = false)
    {
        // Build C++ signature
        var sigParts = new List<string>();
        foreach (var p in parameters)
        {
            var cType = HeaderEmitter.MapType(p.Type);
            if (p.Direction == ParamDirection.Out)
                sigParts.Add($"{cType}* {p.Name}");
            else
                sigParts.Add($"{cType} {p.Name}");
        }

        bool hasRetval = returnType.Name != "void";
        string retvalName = "returnValue";
        if (hasRetval)
        {
            var retCType = HeaderEmitter.MapType(returnType);
            // Avoid collision with out params
            var paramNames = new HashSet<string>(parameters.Select(p => p.Name));
            while (paramNames.Contains(retvalName))
                retvalName = "_" + retvalName;
            sigParts.Add($"{retCType}* {retvalName}");
        }

        var signature = string.Join(", ", sigParts);
        var iidName = $"IID_{mangledName}";

        sb.AppendLine($"        // {methodName} (vtable slot {vtableSlot})");

        // Compute buffer size for input params
        var inParams = parameters.Where(p => p.Direction == ParamDirection.In).ToList();
        bool requestFixed = WireFormat.AllParamsFixedSize(inParams);

        // Emit constexpr buffer size constants for fixed-size signatures
        if (requestFixed)
        {
            var fixedSizeExpr = WireFormat.ComputeFixedBufferSizeExpression(inParams);
            sb.AppendLine($"        static constexpr UINT32 k{methodName}_RequestSize = {fixedSizeExpr};");
        }

        // Check if reply is also fixed-size
        var outParams = parameters.Where(p => p.Direction == ParamDirection.Out).ToList();
        var replyParts = outParams.Select(p => p.Type).ToList();
        bool replyFixed = replyParts.All(t => WireFormat.IsFixedSize(t))
            && (!hasRetval || WireFormat.IsFixedSize(returnType));
        if (replyFixed)
        {
            var replySizeParts = new List<string>();
            foreach (var p in outParams)
                replySizeParts.Add(WireFormat.GetCppSizeExpression(p.Type));
            if (hasRetval)
                replySizeParts.Add(WireFormat.GetCppSizeExpression(returnType));
            replySizeParts.Add("sizeof(HRESULT)");
            var replySizeExpr = string.Join(" + ", replySizeParts);
            sb.AppendLine($"        static constexpr UINT32 k{methodName}_ReplySize = {replySizeExpr};");
        }

        sb.AppendLine($"        HRESULT STDMETHODCALLTYPE {methodName}({signature}) override {{");
        sb.AppendLine("            if (!m_owner->m_pChannel) return RPC_E_DISCONNECTED;");
        sb.AppendLine("            RPCOLEMESSAGE msg = {};");
        sb.AppendLine($"            msg.iMethod = {vtableSlot};");

        if (requestFixed)
        {
            sb.AppendLine($"            msg.cbBuffer = k{methodName}_RequestSize;");
        }
        else
        {
            var bufferSizeExpr = ComputeBufferSize(inParams);
            sb.AppendLine($"            msg.cbBuffer = {bufferSizeExpr};");
        }

        if (useExceptions)
        {
            sb.AppendLine("            try {");
            sb.AppendLine($"            winmidl::throw_if_failed(m_owner->m_pChannel->GetBuffer(&msg, {iidName}));");
        }
        else
        {
            sb.AppendLine($"            HRESULT hr = m_owner->m_pChannel->GetBuffer(&msg, {iidName});");
            sb.AppendLine("            RETURN_IF_FAILED(hr);");
        }

        // Serialize input params
        if (inParams.Count > 0)
        {
            sb.AppendLine("            BYTE* p = (BYTE*)msg.Buffer;");
            foreach (var p in inParams)
            {
                EmitSerialize(sb, p.Name, p.Type, "            ");
            }
        }

        if (useExceptions)
        {
            sb.AppendLine("            ULONG status;");
            sb.AppendLine("            winmidl::throw_if_failed(m_owner->m_pChannel->SendReceive(&msg, &status));");
        }
        else
        {
            sb.AppendLine("            ULONG status;");
            sb.AppendLine("            hr = m_owner->m_pChannel->SendReceive(&msg, &status);");
            sb.AppendLine("            RETURN_IF_FAILED(hr);");
        }

        // Deserialize reply
        bool needsReadPointer = hasRetval || outParams.Count > 0;

        // Always declare pReply — needed for trailing HRESULT even when no out/retval params
        sb.AppendLine("            BYTE* pReply = (BYTE*)msg.Buffer;");

        // Deserialize out params
        foreach (var p in outParams)
        {
            EmitDeserialize(sb, p.Name, p.Type, isPointer: true, indent: "            ");
        }

        // Deserialize retval
        if (hasRetval)
        {
            EmitDeserialize(sb, retvalName, returnType, isPointer: true, indent: "            ");
        }

        // Deserialize trailing HRESULT
        sb.AppendLine("            HRESULT retHr; memcpy(&retHr, pReply, sizeof(HRESULT));");

        if (useExceptions)
        {
            sb.AppendLine("            winmidl::throw_if_failed(m_owner->m_pChannel->FreeBuffer(&msg));");
            sb.AppendLine("            return retHr;");
            sb.AppendLine("            } catch (HRESULT hr) { return hr; }");
            sb.AppendLine("              catch (...) { return E_FAIL; }");
        }
        else
        {
            sb.AppendLine("            hr = m_owner->m_pChannel->FreeBuffer(&msg);");
            sb.AppendLine("            RETURN_IF_FAILED(hr);");
            sb.AppendLine("            return retHr;");
        }
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static void EmitProxyMethod(
        StringBuilder sb,
        InterfaceSymbol iface,
        string methodName,
        List<ProxyParam> parameters,
        TypeRef returnType,
        MemberKind memberKind,
        string mangledName,
        int vtableSlot)
    {
        // Build C++ signature
        var sigParts = new List<string>();
        foreach (var p in parameters)
        {
            var cType = HeaderEmitter.MapType(p.Type);
            if (p.Direction == ParamDirection.Out)
                sigParts.Add($"{cType}* {p.Name}");
            else
                sigParts.Add($"{cType} {p.Name}");
        }

        bool hasRetval = returnType.Name != "void";
        string retvalName = "returnValue";
        if (hasRetval)
        {
            var retCType = HeaderEmitter.MapType(returnType);
            // Avoid collision with out params
            var paramNames = new HashSet<string>(parameters.Select(p => p.Name));
            while (paramNames.Contains(retvalName))
                retvalName = "_" + retvalName;
            sigParts.Add($"{retCType}* {retvalName}");
        }

        var signature = string.Join(", ", sigParts);
        var iidName = $"IID_{mangledName}";

        sb.AppendLine($"    // {methodName} (vtable slot {vtableSlot})");

        // Compute buffer size for input params
        var inParams = parameters.Where(p => p.Direction == ParamDirection.In).ToList();
        bool requestFixed = WireFormat.AllParamsFixedSize(inParams);

        // Emit constexpr buffer size constants for fixed-size signatures
        if (requestFixed)
        {
            var fixedSizeExpr = WireFormat.ComputeFixedBufferSizeExpression(inParams);
            sb.AppendLine($"    static constexpr UINT32 k{methodName}_RequestSize = {fixedSizeExpr};");
        }

        // Check if reply is also fixed-size
        var outParams = parameters.Where(p => p.Direction == ParamDirection.Out).ToList();
        var replyParts = outParams.Select(p => p.Type).ToList();
        bool replyFixed = replyParts.All(t => WireFormat.IsFixedSize(t))
            && (!hasRetval || WireFormat.IsFixedSize(returnType));
        if (replyFixed)
        {
            var replySizeParts = new List<string>();
            foreach (var p in outParams)
                replySizeParts.Add(WireFormat.GetCppSizeExpression(p.Type));
            if (hasRetval)
                replySizeParts.Add(WireFormat.GetCppSizeExpression(returnType));
            replySizeParts.Add("sizeof(HRESULT)");
            var replySizeExpr = string.Join(" + ", replySizeParts);
            sb.AppendLine($"    static constexpr UINT32 k{methodName}_ReplySize = {replySizeExpr};");
        }

        sb.AppendLine($"    HRESULT STDMETHODCALLTYPE {methodName}({signature}) override {{");
        sb.AppendLine("        if (!m_pChannel) return RPC_E_DISCONNECTED;");
        sb.AppendLine("        RPCOLEMESSAGE msg = {};");
        sb.AppendLine($"        msg.iMethod = {vtableSlot};");

        if (requestFixed)
        {
            sb.AppendLine($"        msg.cbBuffer = k{methodName}_RequestSize;");
        }
        else
        {
            var bufferSizeExpr = ComputeBufferSize(inParams);
            sb.AppendLine($"        msg.cbBuffer = {bufferSizeExpr};");
        }

        sb.AppendLine($"        HRESULT hr = m_pChannel->GetBuffer(&msg, {iidName});");
        sb.AppendLine("        RETURN_IF_FAILED(hr);");

        // Serialize input params
        if (inParams.Count > 0)
        {
            sb.AppendLine("        BYTE* p = (BYTE*)msg.Buffer;");
            foreach (var p in inParams)
            {
                EmitSerialize(sb, p.Name, p.Type);
            }
        }

        sb.AppendLine("        ULONG status;");
        sb.AppendLine("        hr = m_pChannel->SendReceive(&msg, &status);");
        sb.AppendLine("        RETURN_IF_FAILED(hr);");

        // Deserialize reply
        bool needsReadPointer = hasRetval || outParams.Count > 0;

        // Always declare pReply — needed for trailing HRESULT even when no out/retval params
        sb.AppendLine("        BYTE* pReply = (BYTE*)msg.Buffer;");

        // Deserialize out params
        foreach (var p in outParams)
        {
            EmitDeserialize(sb, p.Name, p.Type, isPointer: true);
        }

        // Deserialize retval
        if (hasRetval)
        {
            EmitDeserialize(sb, retvalName, returnType, isPointer: true);
        }

        // Deserialize trailing HRESULT
        sb.AppendLine("        HRESULT retHr; memcpy(&retHr, pReply, sizeof(HRESULT));");
        sb.AppendLine("        hr = m_pChannel->FreeBuffer(&msg);");
        sb.AppendLine("        RETURN_IF_FAILED(hr);");
        sb.AppendLine("        return retHr;");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    /// <summary>
    /// Computes the C++ expression for the buffer size needed to serialize the given parameters.
    /// Delegates to <see cref="WireFormat.ComputeBufferSize"/>.
    /// </summary>
    internal static string ComputeBufferSize(List<ProxyParam> inParams)
    {
        return WireFormat.ComputeBufferSize(inParams);
    }

    /// <summary>
    /// Returns a C++ expression for the serialized size of a type.
    /// Delegates to <see cref="WireFormat.GetTypeSizeExpression"/>.
    /// </summary>
    internal static string GetTypeSize(TypeRef type)
    {
        return WireFormat.GetTypeSizeExpression(type);
    }

    /// <summary>
    /// Computes the size expression for a struct by summing its field sizes.
    /// Delegates to <see cref="WireFormat.GetWireSize"/>.
    /// </summary>
    internal static string ComputeStructSize(StructSymbol structSym)
    {
        if (structSym.Fields.Count == 0)
            return "0";
        var typeRef = new TypeRef(structSym.Name) { ResolvedSymbol = structSym };
        return WireFormat.GetWireSize(typeRef).ToString();
    }

    /// <summary>
    /// Emits C++ code to serialize a parameter into the RPC buffer.
    /// Delegates to <see cref="WireFormat.EmitSerialize"/>.
    /// </summary>
    internal static void EmitSerialize(StringBuilder sb, string paramName, TypeRef type, string indent = "        ")
    {
        WireFormat.EmitSerialize(sb, type, paramName, "p", indent);
    }

    /// <summary>
    /// Emits C++ code to deserialize a value from the RPC reply buffer.
    /// Delegates to <see cref="WireFormat.EmitDeserializeReply"/>.
    /// </summary>
    internal static void EmitDeserialize(StringBuilder sb, string targetName, TypeRef type, bool isPointer, string indent = "        ")
    {
        WireFormat.EmitDeserializeReply(sb, type, targetName, "pReply", isPointer, indent);
    }

    /// <summary>
    /// Returns the byte size for a primitive type name, or 0 if not a recognized primitive.
    /// Delegates to <see cref="WireFormat.GetWireSize"/>.
    /// </summary>
    internal static int GetPrimitiveSize(string typeName)
    {
        return typeName switch
        {
            "Boolean" or "UInt8" => 1,
            "Int16" or "UInt16" or "Char" => 2,
            "Int32" or "UInt32" or "Single" or "HRESULT" => 4,
            "Int64" or "UInt64" or "Double" => 8,
            _ => 0,
        };
    }

    /// <summary>
    /// Computes the vtable slot for a specific method within an interface.
    /// IUnknown: 0-2, IInspectable: 3-5, user methods: 6+
    /// </summary>
    internal static int ComputeVtableSlot(InterfaceSymbol iface, string methodName)
    {
        var methods = CollectInterfaceMethods(iface);
        int slot = UserMethodStartSlot;
        foreach (var m in methods)
        {
            if (m.Name == methodName)
                return slot;
            slot++;
        }
        return -1; // not found
    }
}

/// <summary>
/// Represents a parameter in the proxy method, with direction information.
/// </summary>
public record ProxyParam(string Name, TypeRef Type, ParamDirection Direction);

/// <summary>
/// Parameter direction for proxy serialization.
/// </summary>
public enum ParamDirection
{
    In,
    Out
}

/// <summary>
/// Categorizes what kind of interface member generated this method.
/// </summary>
public enum MemberKind
{
    Method,
    PropertyGetter,
    PropertySetter,
    EventAdd,
    EventRemove
}
