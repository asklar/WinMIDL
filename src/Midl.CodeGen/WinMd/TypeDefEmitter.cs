using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.CodeGen.WinMd;

/// <summary>
/// Emits individual WinRT type definitions to the metadata tables.
/// </summary>
internal class TypeDefEmitter
{
    private readonly WinMdEmitter _emitter;
    private readonly CustomAttributeEmitter _attrs;
    private MetadataBuilder M => _emitter.Metadata;

    // Tracks synthesized interface method handles keyed by runtimeclass FullName → member name → MethodDef handle
    private readonly Dictionary<string, List<(string memberName, MethodDefinitionHandle handle)>> _synthMethodHandles = new();

    public TypeDefEmitter(WinMdEmitter emitter, CustomAttributeEmitter attrs)
    {
        _emitter = emitter;
        _attrs = attrs;
    }

    public void Emit(ISymbol type)
    {
        switch (type)
        {
            case EnumSymbol e: EmitEnum(e); break;
            case StructSymbol s: EmitStruct(s); break;
            case InterfaceSymbol i: EmitInterface(i); break;
            case DelegateSymbol d: EmitDelegate(d); break;
            case RuntimeClassSymbol rc: EmitRuntimeClass(rc); break;
            case ApiContractSymbol ac: EmitApiContract(ac); break;
            case AttributeTypeSymbol at: EmitAttribute(at); break;
        }
    }

    /// <summary>
    /// Well-known attribute names already handled by explicit Add*Attribute calls.
    /// These must NOT be re-emitted by the generic path.
    /// </summary>
    private static readonly HashSet<string> HandledAttributeNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "uuid", "version", "contract", "contractversion",
            "deprecated", "activatable", "composable", "static",
            "threading", "marshaling_behavior", "exclusiveto",
            "default_interface", "default_overload", "overload",
            "method_name", "return_name", "flags", "noexcept",
            "attributeusage", "attributename", "allowmultiple",
            "experimental", "default", "overridable", "protected",
            "remote_async",
#if WINDOWS_INTERNAL
            "feature", "feature_name",
#endif
        };

    /// <summary>
    /// Emit any custom attributes on the symbol that are NOT already handled
    /// by the explicit well-known attribute calls.
    /// </summary>
    private void EmitRemainingAttributes(EntityHandle handle, List<AttributeData>? attrs)
    {
        if (attrs == null || attrs.Count == 0) return;

        foreach (var attr in attrs)
        {
            if (HandledAttributeNames.Contains(attr.Name)) continue;
            _attrs.AddGenericCustomAttribute(handle, attr);
        }
    }

    private void EmitArrayParamAttribute(EntityHandle paramHandle, ParameterSymbol param)
    {
        if (!param.IsArray) return;

        if (param.Direction == ParameterDirection.In)
            _attrs.AddReadOnlyArrayAttribute(paramHandle);
        else
            _attrs.AddWriteOnlyArrayAttribute(paramHandle);
    }

    /// <summary>Emit a synthesized exclusive interface for a runtime class.</summary>
    /// <param name="rc">The owning runtime class.</param>
    /// <param name="iface">The synthesized interface symbol (default, protected, or overridable).</param>
    /// <param name="trackMethodHandles">Key to store method handles under for MethodImpl linking.</param>
    public TypeDefinitionHandle EmitSynthesizedInterface(RuntimeClassSymbol rc, InterfaceSymbol iface, string trackMethodHandles)
    {
        var ns = GetNamespace(rc);
        var ifaceName = iface.Name;
        var fieldList = _emitter.NextFieldHandle;
        var methodList = _emitter.NextMethodHandle;

        var typeDef = M.AddTypeDefinition(
            (TypeAttributes)(TypeAttributes.Interface | TypeAttributes.Abstract
                | (TypeAttributes)MetadataConstants.TdWindowsRuntime),
            M.GetOrAddString(ns),
            M.GetOrAddString(ifaceName),
            default,
            fieldList,
            methodList);

        // Add a deterministic GUID based on the interface ABI signature
        var members = iface.Members
            .Where(m => m is MethodSymbol or PropertySymbol or EventSymbol)
            .ToList();
        var guid = GenerateDeterministicGuid($"{ns}.{ifaceName}", members);
        _attrs.AddGuidAttribute(typeDef, guid);
        _attrs.AddExclusiveToAttribute(typeDef, rc.FullName);

        // VersionAttribute — match original midl which adds [Version(N)] to synthesized interfaces
        // Use contract version when available
        bool rcHasContract = rc.ContractName != null && rc.ContractVersion != null;
        uint attrVersion = rcHasContract
            ? (uint)rc.ContractVersion!.Value
            : (rc.Version != null ? (uint)rc.Version.Value : 1u);
        _attrs.AddVersionAttribute(typeDef, attrVersion);

        // Track properties and events for map tables
        var properties = new List<(PropertySymbol prop, MethodDefinitionHandle getter, MethodDefinitionHandle? setter)>();
        var events = new List<(EventSymbol evt, MethodDefinitionHandle adder, MethodDefinitionHandle remover)>();

        // Track interface method handles for MethodImpl table
        var ifaceMethodHandles = new List<(string memberName, MethodDefinitionHandle handle)>();

        foreach (var member in iface.Members)
        {
            switch (member)
            {
                case MethodSymbol method:
                {
                    var methodHandle = EmitInterfaceMethod(method);
                    ifaceMethodHandles.Add((method.Name, methodHandle));
                    break;
                }
                case PropertySymbol prop:
                {
                    var getter = EmitPropertyGetter(prop);
                    ifaceMethodHandles.Add(($"get_{prop.Name}", getter));
                    MethodDefinitionHandle? setter = prop.HasSetter ? EmitPropertySetter(prop) : null;
                    if (setter.HasValue)
                        ifaceMethodHandles.Add(($"put_{prop.Name}", setter.Value));
                    properties.Add((prop, getter, setter));
                    break;
                }
                case EventSymbol evt:
                {
                    var (adder, remover) = EmitEventAccessors(evt);
                    ifaceMethodHandles.Add(($"add_{evt.Name}", adder));
                    ifaceMethodHandles.Add(($"remove_{evt.Name}", remover));
                    events.Add((evt, adder, remover));
                    break;
                }
            }
        }

        if (!_synthMethodHandles.ContainsKey(trackMethodHandles))
            _synthMethodHandles[trackMethodHandles] = ifaceMethodHandles;
        else
            _synthMethodHandles[trackMethodHandles].AddRange(ifaceMethodHandles);

        EmitPropertyTable(typeDef, properties);
        EmitEventTable(typeDef, events);

        return typeDef;
    }

    #region Enum

    private void EmitEnum(EnumSymbol symbol)
    {
        var ns = GetNamespace(symbol);
        var fieldList = _emitter.NextFieldHandle;
        var methodList = _emitter.NextMethodHandle;

        // Flags enums use UInt32 underlying type per WinRT convention
        var isFlags = symbol.IsFlags;
        var underlyingTypeName = isFlags ? "UInt32" : symbol.UnderlyingType;

        var typeDef = M.AddTypeDefinition(
            (TypeAttributes)(TypeAttributes.Public | TypeAttributes.Sealed
                | (TypeAttributes)MetadataConstants.TdWindowsRuntime),
            M.GetOrAddString(ns),
            M.GetOrAddString(symbol.Name),
            _emitter.SystemEnumRef,
            fieldList,
            methodList);

        // value__ field (Private per original midl convention)
        var valueSig = EncodeFieldSig(underlyingTypeName);
        M.AddFieldDefinition(
            FieldAttributes.Private | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName,
            M.GetOrAddString("value__"),
            M.GetOrAddBlob(valueSig));
        _emitter.IncrementFieldRow();

        // Enum member literal fields
        var enumFieldSig = EncodeTypedFieldSig(typeDef);
        for (int i = 0; i < symbol.EnumMembers.Count; i++)
        {
            var member = symbol.EnumMembers[i];
            var fieldHandle = M.AddFieldDefinition(
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault,
                M.GetOrAddString(member.Name),
                M.GetOrAddBlob(enumFieldSig));

            if (isFlags)
                M.AddConstant(fieldHandle, member.ExplicitValue.HasValue ? (uint)member.ExplicitValue.Value : (uint)i);
            else
                M.AddConstant(fieldHandle, member.ExplicitValue.HasValue ? (int)member.ExplicitValue.Value : i);

            _emitter.IncrementFieldRow();

            // DeprecatedAttribute on enum member
            var memberDeprecatedAttr = member.Attributes.FirstOrDefault(a => a.Name.Equals("deprecated", StringComparison.OrdinalIgnoreCase));
            if (memberDeprecatedAttr != null)
                _attrs.AddDeprecatedAttribute(fieldHandle, memberDeprecatedAttr.GetString(0) ?? "", 0, (uint)(member.ContractVersion ?? 0x00010000));

            EmitRemainingAttributes(fieldHandle, member.Attributes);
        }

        // VersionAttribute before FlagsAttribute (matches original midl ordering)
        _attrs.AddVersionAttribute(typeDef);

        if (isFlags)
            _attrs.AddFlagsAttribute(typeDef);

        // ExperimentalAttribute
        if (symbol.IsExperimental)
            _attrs.AddExperimentalAttribute(typeDef);

#if WINDOWS_INTERNAL
        // FeatureAttribute
        if (symbol.FeatureName != null && _emitter.FeatureNames?.TryGetValue(symbol.FeatureName, out var enumFInfo) == true)
            _attrs.AddFeatureAttribute(typeDef, (int)enumFInfo.Stage, enumFInfo.ValidInAllBranches);
#endif

        EmitRemainingAttributes(typeDef, symbol.Attributes);
    }

    #endregion

    #region Struct

    private void EmitStruct(StructSymbol symbol)
    {
        var ns = GetNamespace(symbol);
        var fieldList = _emitter.NextFieldHandle;
        var methodList = _emitter.NextMethodHandle;

        var typeDef = M.AddTypeDefinition(
            (TypeAttributes)(TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout
                | (TypeAttributes)MetadataConstants.TdWindowsRuntime),
            M.GetOrAddString(ns),
            M.GetOrAddString(symbol.Name),
            _emitter.SystemValueTypeRef,
            fieldList,
            methodList);

        foreach (var field in symbol.Fields)
        {
            var sig = new BlobBuilder();
            new BlobEncoder(sig).FieldSignature().FromTypeRef(field.Type, _emitter);
            var fieldHandle = M.AddFieldDefinition(
                FieldAttributes.Public,
                M.GetOrAddString(field.Name),
                M.GetOrAddBlob(sig));
            _emitter.IncrementFieldRow();

            // DeprecatedAttribute on struct field
            var fieldDeprecatedAttr = field.Attributes.FirstOrDefault(a => a.Name.Equals("deprecated", StringComparison.OrdinalIgnoreCase));
            if (fieldDeprecatedAttr != null)
                _attrs.AddDeprecatedAttribute(fieldHandle, fieldDeprecatedAttr.GetString(0) ?? "", 0, fieldDeprecatedAttr.GetVersion(3) ?? 0x00010000);

            EmitRemainingAttributes(fieldHandle, field.Attributes);
        }

        // ExperimentalAttribute
        if (symbol.IsExperimental)
            _attrs.AddExperimentalAttribute(typeDef);

#if WINDOWS_INTERNAL
        // FeatureAttribute
        if (symbol.FeatureName != null && _emitter.FeatureNames?.TryGetValue(symbol.FeatureName, out var structFInfo) == true)
            _attrs.AddFeatureAttribute(typeDef, (int)structFInfo.Stage, structFInfo.ValidInAllBranches);
#endif

        _attrs.AddVersionAttribute(typeDef);

        EmitRemainingAttributes(typeDef, symbol.Attributes);
    }

    #endregion

    #region Interface

    private void EmitInterface(InterfaceSymbol symbol)
    {
        var ns = GetNamespace(symbol);
        var fieldList = _emitter.NextFieldHandle;
        var methodList = _emitter.NextMethodHandle;

        var typeAttrs = TypeAttributes.Interface | TypeAttributes.Abstract
            | (TypeAttributes)MetadataConstants.TdWindowsRuntime;
        if (!symbol.IsExclusive)
            typeAttrs |= TypeAttributes.Public;

        var typeDef = M.AddTypeDefinition(
            typeAttrs,
            M.GetOrAddString(ns),
            M.GetOrAddString(symbol.Name),
            default,
            fieldList,
            methodList);

        var properties = new List<(PropertySymbol prop, MethodDefinitionHandle getter, MethodDefinitionHandle? setter)>();
        var events = new List<(EventSymbol evt, MethodDefinitionHandle adder, MethodDefinitionHandle remover)>();

        foreach (var member in symbol.Members)
        {
            switch (member)
            {
                case MethodSymbol method:
                    EmitInterfaceMethod(method);
                    break;
                case PropertySymbol prop:
                {
                    var getter = EmitPropertyGetter(prop);
                    MethodDefinitionHandle? setter = prop.HasSetter ? EmitPropertySetter(prop) : null;
                    properties.Add((prop, getter, setter));
                    break;
                }
                case EventSymbol evt:
                {
                    var (adder, remover) = EmitEventAccessors(evt);
                    events.Add((evt, adder, remover));
                    break;
                }
            }
        }

        EmitPropertyTable(typeDef, properties);
        EmitEventTable(typeDef, events);

        // InterfaceImpl for required interfaces
        foreach (var req in symbol.RequiredInterfaces)
        {
            var handle = _emitter.ResolveTypeHandle(req);
            if (!handle.IsNil)
                M.AddInterfaceImplementation(typeDef, handle);
        }

        // Emit version attribute before GUID (matches original midl ordering)
        if (symbol.Version.HasValue)
            _attrs.AddVersionAttribute(typeDef, (uint)symbol.Version.Value);
        else
            _attrs.AddVersionAttribute(typeDef);

        // Emit [Guid] if the interface has a UUID
        if (symbol.Uuid.HasValue)
            _attrs.AddGuidAttribute(typeDef, symbol.Uuid.Value);
        else
            _attrs.AddGuidAttribute(typeDef, GenerateDeterministicGuid(symbol.FullName, symbol.Members.ToList()));

        // Emit [deprecated] if present
        if (symbol.DeprecatedMessage != null)
            _attrs.AddDeprecatedAttribute(typeDef, symbol.DeprecatedMessage, 0, (uint)(symbol.Version ?? 0x00010000));

        // Emit [experimental] if present
        if (symbol.IsExperimental)
            _attrs.AddExperimentalAttribute(typeDef);

#if WINDOWS_INTERNAL
        // FeatureAttribute
        if (symbol.FeatureName != null && _emitter.FeatureNames?.TryGetValue(symbol.FeatureName, out var ifaceFInfo) == true)
            _attrs.AddFeatureAttribute(typeDef, (int)ifaceFInfo.Stage, ifaceFInfo.ValidInAllBranches);
#endif

        if (symbol.IsExclusive && symbol.ExclusiveToClass != null)
            _attrs.AddExclusiveToAttribute(typeDef, symbol.ExclusiveToClass);

        EmitRemainingAttributes(typeDef, symbol.Attributes);
    }

    #endregion

    #region Delegate

    private void EmitDelegate(DelegateSymbol symbol)
    {
        var ns = GetNamespace(symbol);
        var fieldList = _emitter.NextFieldHandle;
        var methodList = _emitter.NextMethodHandle;

        var typeDef = M.AddTypeDefinition(
            (TypeAttributes)(TypeAttributes.Public | TypeAttributes.Sealed
                | (TypeAttributes)MetadataConstants.TdWindowsRuntime),
            M.GetOrAddString(ns),
            M.GetOrAddString(symbol.Name),
            _emitter.SystemMulticastDelegateRef,
            fieldList,
            methodList);

        // .ctor(object, native int)
        EmitDelegateConstructor();

        // Invoke method with delegate's signature
        EmitDelegateInvoke(symbol);

        // GUID for delegates uses the Invoke method signature
        var invokeMethod = new MethodSymbol("Invoke", null) { ReturnType = symbol.ReturnType };
        invokeMethod.Parameters.AddRange(symbol.Parameters);
        var guid = GenerateDeterministicGuid(symbol.FullName, [invokeMethod]);
        _attrs.AddGuidAttribute(typeDef, guid);

        // ExperimentalAttribute
        if (symbol.IsExperimental)
            _attrs.AddExperimentalAttribute(typeDef);

#if WINDOWS_INTERNAL
        // FeatureAttribute
        if (symbol.FeatureName != null && _emitter.FeatureNames?.TryGetValue(symbol.FeatureName, out var delFInfo) == true)
            _attrs.AddFeatureAttribute(typeDef, (int)delFInfo.Stage, delFInfo.ValidInAllBranches);
#endif

        _attrs.AddVersionAttribute(typeDef);

        EmitRemainingAttributes(typeDef, symbol.Attributes);
    }

    private void EmitDelegateConstructor()
    {
        var sig = new BlobBuilder();
        new BlobEncoder(sig)
            .MethodSignature(isInstanceMethod: true)
            .Parameters(2,
                ret => ret.Void(),
                p =>
                {
                    p.AddParameter().Type().Object();
                    p.AddParameter().Type().IntPtr();
                });

        M.AddMethodDefinition(
            MethodAttributes.Private | MethodAttributes.HideBySig
                | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            MethodImplAttributes.Runtime,
            M.GetOrAddString(".ctor"),
            M.GetOrAddBlob(sig),
            -1,
            _emitter.NextParamHandle);
        _emitter.IncrementMethodRow();

        // Parameters
        M.AddParameter(ParameterAttributes.None, M.GetOrAddString("object"), 1);
        _emitter.IncrementParamRow();
        M.AddParameter(ParameterAttributes.None, M.GetOrAddString("method"), 2);
        _emitter.IncrementParamRow();
    }

    private void EmitDelegateInvoke(DelegateSymbol symbol)
    {
        var sig = new BlobBuilder();
        new BlobEncoder(sig)
            .MethodSignature(isInstanceMethod: true)
            .Parameters(symbol.Parameters.Count,
                ret =>
                {
                    if (symbol.ReturnType.Name == "void")
                        ret.Void();
                    else
                        ret.Type().FromTypeRef(symbol.ReturnType, _emitter);
                },
                p =>
                {
                    foreach (var param in symbol.Parameters)
                    {
                        var paramEnc = p.AddParameter();
                        if (param.Direction is ParameterDirection.Out or ParameterDirection.RefConst)
                            paramEnc.Type(isByRef: true).FromTypeRef(param.Type, _emitter);
                        else
                            paramEnc.Type().FromTypeRef(param.Type, _emitter);
                    }
                });

        var paramList = _emitter.NextParamHandle;
        M.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig
                | MethodAttributes.NewSlot | MethodAttributes.SpecialName,
            MethodImplAttributes.Runtime,
            M.GetOrAddString("Invoke"),
            M.GetOrAddBlob(sig),
            -1,
            paramList);
        _emitter.IncrementMethodRow();

        for (int i = 0; i < symbol.Parameters.Count; i++)
        {
            var paramHandle = M.AddParameter(GetParamAttributes(symbol.Parameters[i]),
                M.GetOrAddString(symbol.Parameters[i].Name),
                i + 1);
            _emitter.IncrementParamRow();
            EmitRemainingAttributes(paramHandle, symbol.Parameters[i].Attributes);
            EmitArrayParamAttribute(paramHandle, symbol.Parameters[i]);
        }
    }

    #endregion

    #region RuntimeClass

    private void EmitRuntimeClass(RuntimeClassSymbol symbol)
    {
        var ns = GetNamespace(symbol);
        var fieldList = _emitter.NextFieldHandle;
        var methodList = _emitter.NextMethodHandle;

        var flags = TypeAttributes.Public | TypeAttributes.Class
            | (TypeAttributes)MetadataConstants.TdWindowsRuntime;
        if (!symbol.IsUnsealed) flags |= TypeAttributes.Sealed;
        if (symbol.IsStatic) flags |= TypeAttributes.Abstract;

        EntityHandle baseType = _emitter.SystemObjectRef;
        if (symbol.BaseClass?.ResolvedSymbol != null)
        {
            var h = _emitter.ResolveTypeHandle(symbol.BaseClass);
            if (!h.IsNil) baseType = h;
        }

        var typeDef = M.AddTypeDefinition(
            flags,
            M.GetOrAddString(ns),
            M.GetOrAddString(symbol.Name),
            baseType,
            fieldList,
            methodList);

        // Emit constructors
        foreach (var ctor in symbol.Members.OfType<ConstructorSymbol>())
        {
            EmitConstructor(ctor, symbol.IsUnsealed);
        }

        bool hasDirectMembers = symbol.Members.Any(m => m is MethodSymbol or PropertySymbol or EventSymbol);

        // Build phased member lists to match midl.exe MethodList ordering:
        // (1) ctors (already emitted above), (2) direct instance, (3) flattened external
        // instance members, (4) direct static.
        var classMethodStubs = new List<(string memberName, MethodDefinitionHandle handle)>();
        var classProperties = new List<(PropertySymbol prop, MethodDefinitionHandle getter, MethodDefinitionHandle? setter)>();
        var classEvents = new List<(EventSymbol evt, MethodDefinitionHandle adder, MethodDefinitionHandle remover)>();

        // Tracks method-level names already emitted on the class (used to dedupe inherited
        // interface stubs against direct members). Keys are the underlying method names
        // (e.g. "Foo", "get_Bar", "add_Baz"). We also track property/event names separately
        // so the PropertyMap/EventMap stays de-duplicated.
        var emittedMethodNames = new HashSet<string>(StringComparer.Ordinal);
        var emittedPropertyNames = new HashSet<string>(StringComparer.Ordinal);
        var emittedEventNames = new HashSet<string>(StringComparer.Ordinal);

        void EmitMemberAsStub(ISymbol member)
        {
            switch (member)
            {
                case MethodSymbol method:
                {
                    // Methods are keyed by full signature (name + param types + return type)
                    // to allow overloads but drop exact duplicates (e.g., when the same
                    // interface appears transitively through multiple required-interface chains).
                    var sigKey = MethodSignatureKey(method);
                    if (!emittedMethodNames.Add(sigKey)) return;
                    var stubHandle = EmitRuntimeMethodStub(method);
                    classMethodStubs.Add((method.Name, stubHandle));
                    if (method.ContractVersion is int cv)
                        _attrs.AddVersionAttribute(stubHandle, (uint)cv);
                    break;
                }
                case PropertySymbol prop:
                {
                    if (!emittedPropertyNames.Add(prop.Name)) return;
                    var (getterStub, setterStub) = EmitRuntimePropertyStubs(prop);
                    classProperties.Add((prop, getterStub, setterStub));
                    if (prop.ContractVersion is int pcv)
                    {
                        if (!getterStub.IsNil) _attrs.AddVersionAttribute(getterStub, (uint)pcv);
                        if (setterStub.HasValue) _attrs.AddVersionAttribute(setterStub.Value, (uint)pcv);
                    }
                    if (!getterStub.IsNil) classMethodStubs.Add(($"get_{prop.Name}", getterStub));
                    if (setterStub.HasValue) classMethodStubs.Add(($"put_{prop.Name}", setterStub.Value));
                    break;
                }
                case EventSymbol evt:
                {
                    if (!emittedEventNames.Add(evt.Name)) return;
                    var (adderStub, removerStub) = EmitRuntimeEventStubs(evt);
                    classMethodStubs.Add(($"add_{evt.Name}", adderStub));
                    classMethodStubs.Add(($"remove_{evt.Name}", removerStub));
                    classEvents.Add((evt, adderStub, removerStub));
                    if (evt.ContractVersion is int ecv)
                    {
                        _attrs.AddVersionAttribute(adderStub, (uint)ecv);
                        _attrs.AddVersionAttribute(removerStub, (uint)ecv);
                    }
                    break;
                }
            }
        }

        // Phase 2: direct instance members
        foreach (var member in symbol.Members)
        {
            if (member is MethodSymbol { IsStatic: false } or PropertySymbol { IsStatic: false } or EventSymbol { IsStatic: false })
                EmitMemberAsStub(member);
        }

        // Phase 3: flattened members of external interfaces declared on this class
        // (plus their transitively required interfaces). midl.exe flattens these onto the
        // runtimeclass after the direct-instance surface but before statics.
        foreach (var iface in EnumerateFlattenInterfaceClosure(symbol))
        {
            foreach (var member in iface.Members)
            {
                if (member is MethodSymbol { IsStatic: false } or PropertySymbol { IsStatic: false } or EventSymbol { IsStatic: false })
                    EmitMemberAsStub(member);
            }
        }

        // Phase 4: direct static members
        foreach (var member in symbol.Members)
        {
            if (member is MethodSymbol { IsStatic: true } or PropertySymbol { IsStatic: true } or EventSymbol { IsStatic: true })
                EmitMemberAsStub(member);
        }

        // Emit Property/Event map tables on the runtimeclass TypeDef (single batch).
        EmitPropertyTable(typeDef, classProperties);
        EmitEventTable(typeDef, classEvents);

        // InterfaceImpl for synthesized default interface
        if (symbol.DefaultInterface is { IsSynthesized: true })
        {
            var synthRef = _emitter.ResolveSynthInterfaceHandle(symbol.DefaultInterface.FullName);
            if (!synthRef.IsNil)
            {
                var ifaceImpl = M.AddInterfaceImplementation(typeDef, synthRef);
                _attrs.AddDefaultAttribute(ifaceImpl);
                // Also add Protected if the default IS the protected interface
                if (symbol.DefaultInterface == symbol.ProtectedInterface)
                    _attrs.AddProtectedAttribute(ifaceImpl);
            }
        }

        // InterfaceImpl for explicit interfaces (from base list).
        // If the class has no synthesized default interface, mark one of the explicit
        // interfaces as [Default] (either the one tagged with [default], or the first).
        bool needsDefaultFromExplicit = symbol.DefaultInterface is null && symbol.Interfaces.Count > 0;
        TypeRef? explicitDefault = null;
        if (needsDefaultFromExplicit)
        {
            explicitDefault = symbol.Interfaces.FirstOrDefault(i => i.IsDefaultInterface)
                ?? symbol.Interfaces[0];
        }
        foreach (var iface in symbol.Interfaces)
        {
            var h = _emitter.ResolveTypeHandle(iface);
            if (!h.IsNil)
            {
                var ifaceImpl = M.AddInterfaceImplementation(typeDef, h);
                if (iface.IsDefaultInterface || ReferenceEquals(iface, explicitDefault))
                    _attrs.AddDefaultAttribute(ifaceImpl);
            }
        }

        // InterfaceImpl for protected interface (if separate from default)
        if (symbol.ProtectedInterface != null && symbol.ProtectedInterface != symbol.DefaultInterface)
        {
            var h = _emitter.ResolveSynthInterfaceHandle(symbol.ProtectedInterface.FullName);
            if (!h.IsNil)
            {
                var ifaceImpl = M.AddInterfaceImplementation(typeDef, h);
                _attrs.AddProtectedAttribute(ifaceImpl);
            }
        }

        // InterfaceImpl for overridable interface
        if (symbol.OverridesInterface != null && symbol.OverridesInterface != symbol.DefaultInterface)
        {
            var h = _emitter.ResolveSynthInterfaceHandle(symbol.OverridesInterface.FullName);
            if (!h.IsNil)
            {
                var ifaceImpl = M.AddInterfaceImplementation(typeDef, h);
                _attrs.AddOverridableAttribute(ifaceImpl);
            }
        }

        // MethodImpl: link class method stubs to synthesized interface method declarations
        if (_synthMethodHandles.TryGetValue(symbol.FullName, out var ifaceMethods))
        {
            foreach (var (stubName, stubHandle) in classMethodStubs)
            {
                var ifaceMethod = ifaceMethods.FirstOrDefault(m => m.memberName == stubName);
                if (!ifaceMethod.handle.IsNil)
                {
                    M.AddMethodImplementation(typeDef, stubHandle, ifaceMethod.handle);
                }
            }
        }

        // Reorder attributes to match original midl: Threading → Marshaling → Activatable → ContractVersion → Static
        // Use contract version for Activatable/Static/etc. when [contract] is present
        bool hasContract = symbol.ContractName != null && symbol.ContractVersion != null;
        bool hasExplicitVersion = symbol.Version != null;
        uint activatableVersion = hasContract
            ? (uint)symbol.ContractVersion!.Value
            : (hasExplicitVersion ? (uint)symbol.Version!.Value : 1u);

        // ThreadingAttribute
        if (symbol.ThreadingModel != null)
        {
            int threadingValue = symbol.ThreadingModel.ToLowerInvariant() switch
            {
                "sta" => 1,
                "mta" => 2,
                "both" => 3,
                _ => 3
            };
            _attrs.AddThreadingAttribute(typeDef, threadingValue);
        }
        else
        {
            _attrs.AddThreadingAttribute(typeDef, 3); // Default: Both
        }

        // MarshalingBehaviorAttribute
        if (symbol.MarshalingBehavior != null)
        {
            int marshalingValue = symbol.MarshalingBehavior.ToLowerInvariant() switch
            {
                "none" => 0,
                "standard" => 1,
                "agile" => 2,
                _ => 2
            };
            _attrs.AddMarshalingBehaviorAttribute(typeDef, marshalingValue);
        }
        else
        {
            _attrs.AddMarshalingBehaviorAttribute(typeDef, 2); // Default: Agile
        }

        // VersionAttribute — emit when explicit [version] is present, OR when no [contract] (default version)
        // MIDL emits [Version] alongside [ContractVersion] when both are specified in IDL
        if (hasExplicitVersion)
            _attrs.AddVersionAttribute(typeDef, (uint)symbol.Version!.Value);
        else if (!hasContract)
            _attrs.AddVersionAttribute(typeDef, 1u);

        // ActivatableAttribute — uses contract version when available
        if (symbol.FactoryInterface != null)
            _attrs.AddActivatableAttribute(typeDef, symbol.FactoryInterface.FullName, activatableVersion);

        // Default activation ([Activatable(version)] short form) only when the class has a
        // zero-arg constructor. Parameterized constructors are covered by the factory-interface
        // long form above; emitting both would produce duplicate [Activatable] attributes.
        if (symbol.HasDefaultActivation)
            _attrs.AddActivatableAttribute(typeDef, activatableVersion);

        // ComposableAttribute
        foreach (var attr in symbol.Attributes)
        {
            if (!attr.Name.Equals("composable", StringComparison.OrdinalIgnoreCase))
                continue;
            var factoryName = attr.GetTypeName(0) ?? attr.GetIdentifier(0);
            if (factoryName != null)
            {
                var classNs = GetNamespace(symbol);
                var fullFactoryName = string.IsNullOrEmpty(classNs) ? factoryName : $"{classNs}.{factoryName}";
                var accessStr = attr.GetIdentifier(1)?.ToLowerInvariant();
                int access = accessStr == "protected" ? 1 : 2;
                var version = attr.GetVersion(2) ?? activatableVersion;
                _attrs.AddComposableAttribute(typeDef, fullFactoryName, access, version);
            }
        }

        // StaticAttribute — from explicit [static] attributes if present, else from synthesized
        var hasExplicitStatic = false;
        foreach (var attr in symbol.Attributes)
        {
            if (!attr.Name.Equals("static", StringComparison.OrdinalIgnoreCase))
                continue;
            hasExplicitStatic = true;
            var ifaceName = attr.GetTypeName(0) ?? attr.GetIdentifier(0);
            if (ifaceName != null)
            {
                var classNs = GetNamespace(symbol);
                var fullIfaceName = string.IsNullOrEmpty(classNs) ? ifaceName : $"{classNs}.{ifaceName}";
                var version = attr.GetVersion(2) ?? attr.GetVersion(1) ?? activatableVersion;
                _attrs.AddStaticAttribute(typeDef, fullIfaceName, version);
            }
        }

        if (!hasExplicitStatic)
        {
            foreach (var iface in symbol.StaticsInterfaces)
            {
                _attrs.AddStaticAttribute(typeDef, iface.FullName, activatableVersion);
            }
        }

        // DeprecatedAttribute
        if (symbol.DeprecatedMessage != null)
            _attrs.AddDeprecatedAttribute(typeDef, symbol.DeprecatedMessage, 0, activatableVersion);

        // ExperimentalAttribute
        if (symbol.IsExperimental)
            _attrs.AddExperimentalAttribute(typeDef);

#if WINDOWS_INTERNAL
        // FeatureAttribute
        if (symbol.FeatureName != null && _emitter.FeatureNames?.TryGetValue(symbol.FeatureName, out var rcFInfo) == true)
            _attrs.AddFeatureAttribute(typeDef, (int)rcFInfo.Stage, rcFInfo.ValidInAllBranches);
#endif

        // ContractVersionAttribute
        if (symbol.ContractName != null && symbol.ContractVersion != null)
            _attrs.AddContractVersionAttribute(typeDef, symbol.ContractName, (uint)symbol.ContractVersion.Value);

        EmitRemainingAttributes(typeDef, symbol.Attributes);
    }

    /// <summary>
    /// Enumerates the external interfaces whose members should be flattened onto
    /// <paramref name="symbol"/>'s class-level MethodList. Walks the class's explicit
    /// interfaces and their transitive <c>RequiredInterfaces</c> closure, yielding each
    /// resolved <see cref="InterfaceSymbol"/> at most once in a stable source order.
    /// Parameterized (generic) interfaces are skipped: their members reference type
    /// parameters rather than concrete types, so flattening them cannot produce a
    /// well-formed class MethodDefSig.
    /// </summary>
    private static IEnumerable<InterfaceSymbol> EnumerateFlattenInterfaceClosure(RuntimeClassSymbol symbol)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<InterfaceSymbol>();

        void Visit(TypeRef? tr)
        {
            if (tr is null) return;
            // Skip generic instantiations — flattening those requires substituting type
            // arguments into member signatures, which we don't yet do.
            if (tr.TypeArguments.Count > 0) return;
            if (tr.ResolvedSymbol is not InterfaceSymbol iface) return;
            if (!visited.Add(iface.FullName)) return;
            result.Add(iface);
            foreach (var req in iface.RequiredInterfaces)
                Visit(req);
        }

        foreach (var iface in symbol.Interfaces)
            Visit(iface);

        return result;
    }

    /// <summary>Builds a dedup key that preserves overloads but collapses exact-duplicate signatures.</summary>
    private static string MethodSignatureKey(MethodSymbol m)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(m.Name).Append('(');
        for (int i = 0; i < m.Parameters.Count; i++)
        {
            if (i > 0) sb.Append(',');
            AppendTypeRefKey(sb, m.Parameters[i].Type);
        }
        sb.Append(")->");
        AppendTypeRefKey(sb, m.ReturnType);
        return sb.ToString();
    }

    private static void AppendTypeRefKey(System.Text.StringBuilder sb, TypeRef? t)
    {
        if (t is null) { sb.Append('?'); return; }
        sb.Append(t.Name);
        if (t.TypeArguments.Count > 0)
        {
            sb.Append('<');
            for (int i = 0; i < t.TypeArguments.Count; i++)
            {
                if (i > 0) sb.Append(',');
                AppendTypeRefKey(sb, t.TypeArguments[i]);
            }
            sb.Append('>');
        }
        if (t.IsArray) sb.Append("[]");
    }

    private void EmitConstructor(ConstructorSymbol ctor, bool isUnsealed)
    {
        // For composable (unsealed) runtimeclasses, the factory method has extra
        // IInspectable* outer (in) and IInspectable** inner (out) params.
        // These must be omitted from the .ctor MethodDefSig in WinMD metadata.
        var ctorParams = ctor.Parameters;
        int winmdParamCount = ctorParams.Count;
        if (isUnsealed && winmdParamCount >= 2)
            winmdParamCount -= 2;

        var sig = new BlobBuilder();
        new BlobEncoder(sig)
            .MethodSignature(isInstanceMethod: true)
            .Parameters(winmdParamCount,
                ret => ret.Void(),
                p =>
                {
                    for (int i = 0; i < winmdParamCount; i++)
                        p.AddParameter().Type().FromTypeRef(ctorParams[i].Type, _emitter);
                });

        var paramList = _emitter.NextParamHandle;
        M.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.HideBySig
                | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            MethodImplAttributes.Runtime,
            M.GetOrAddString(".ctor"),
            M.GetOrAddBlob(sig),
            -1,
            paramList);
        _emitter.IncrementMethodRow();

        for (int i = 0; i < winmdParamCount; i++)
        {
            var paramHandle = M.AddParameter(GetParamAttributes(ctorParams[i]),
                M.GetOrAddString(ctorParams[i].Name),
                i + 1);
            _emitter.IncrementParamRow();
            EmitRemainingAttributes(paramHandle, ctorParams[i].Attributes);
        }
    }

    private MethodDefinitionHandle EmitRuntimeMethodStub(MethodSymbol method)
    {
        var sig = EncodeMethodSig(method.ReturnType, method.Parameters, hasThis: false);
        var paramList = _emitter.NextParamHandle;

        // Static methods: Public | Static | HideBySig
        // Protected methods: Family | Final | Virtual | HideBySig | NewSlot
        // Non-static methods: Public | Final | Virtual | HideBySig | NewSlot
        MethodAttributes methodFlags;
        if (method.IsStatic)
            methodFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;
        else if (method.IsProtected)
            methodFlags = MethodAttributes.Family | MethodAttributes.Final | MethodAttributes.Virtual
                | MethodAttributes.HideBySig | MethodAttributes.NewSlot;
        else
            methodFlags = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual
                | MethodAttributes.HideBySig | MethodAttributes.NewSlot;

        var handle = M.AddMethodDefinition(
            methodFlags,
            MethodImplAttributes.Runtime,
            M.GetOrAddString(method.Name),
            M.GetOrAddBlob(sig),
            -1,
            paramList);
        _emitter.IncrementMethodRow();

        // Emit retval param at sequence 0 for non-void return types
        var retvalName = GetRetvalParamName(method);
        if (retvalName != null)
        {
            M.AddParameter(ParameterAttributes.Out, M.GetOrAddString(retvalName), 0);
            _emitter.IncrementParamRow();
        }

        for (int i = 0; i < method.Parameters.Count; i++)
        {
            var paramHandle = M.AddParameter(GetParamAttributes(method.Parameters[i]),
                M.GetOrAddString(method.Parameters[i].Name),
                i + 1);
            _emitter.IncrementParamRow();
            EmitRemainingAttributes(paramHandle, method.Parameters[i].Attributes);
            EmitArrayParamAttribute(paramHandle, method.Parameters[i]);
        }

        return handle;
    }

    private (MethodDefinitionHandle getter, MethodDefinitionHandle? setter) EmitRuntimePropertyStubs(PropertySymbol prop)
    {
        MethodDefinitionHandle getter = default;
        MethodDefinitionHandle? setter = null;

        // Static properties: Public | Static | HideBySig | SpecialName
        // Protected properties: Family | Final | Virtual | HideBySig | NewSlot | SpecialName
        // Non-static properties: Public | Final | Virtual | HideBySig | NewSlot | SpecialName
        MethodAttributes baseFlags;
        if (prop.IsStatic)
            baseFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
        else if (prop.IsProtected)
            baseFlags = MethodAttributes.Family | MethodAttributes.Final | MethodAttributes.Virtual
                | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName;
        else
            baseFlags = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual
                | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName;

        if (prop.HasGetter)
        {
            var sig = EncodeMethodSig(prop.Type, Array.Empty<ParameterSymbol>(), hasThis: false);
            getter = M.AddMethodDefinition(
                baseFlags,
                MethodImplAttributes.Runtime,
                M.GetOrAddString($"get_{prop.Name}"),
                M.GetOrAddBlob(sig),
                -1,
                _emitter.NextParamHandle);
            _emitter.IncrementMethodRow();

            // NoExceptionAttribute on property getter
            if (prop.Attributes.Any(a => a.Name.Equals("noexcept", StringComparison.OrdinalIgnoreCase)))
                _attrs.AddNoExceptionAttribute(getter);

            // DeprecatedAttribute on property getter
            var deprecatedPropAttr = prop.Attributes.FirstOrDefault(a => a.Name.Equals("deprecated", StringComparison.OrdinalIgnoreCase));
            if (deprecatedPropAttr != null)
                _attrs.AddDeprecatedAttribute(getter, deprecatedPropAttr.GetString(0) ?? "", 0, (uint)(prop.ContractVersion ?? 0x00010000));

            EmitRemainingAttributes(getter, prop.Attributes);
        }
        if (prop.HasSetter)
        {
            var sig = EncodeSetterSig(prop.Type);
            setter = M.AddMethodDefinition(
                baseFlags,
                MethodImplAttributes.Runtime,
                M.GetOrAddString($"put_{prop.Name}"),
                M.GetOrAddBlob(sig),
                -1,
                _emitter.NextParamHandle);
            _emitter.IncrementMethodRow();

            M.AddParameter(ParameterAttributes.In, M.GetOrAddString("value"), 1);
            _emitter.IncrementParamRow();
        }

        return (getter, setter);
    }

    private (MethodDefinitionHandle adder, MethodDefinitionHandle remover) EmitRuntimeEventStubs(EventSymbol evt)
    {
        // Static events: Public | Static | HideBySig | SpecialName
        // Protected events: Family | Final | Virtual | HideBySig | NewSlot | SpecialName
        // Non-static events: Public | Final | Virtual | HideBySig | NewSlot | SpecialName
        MethodAttributes baseFlags;
        if (evt.IsStatic)
            baseFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
        else if (evt.IsProtected)
            baseFlags = MethodAttributes.Family | MethodAttributes.Final | MethodAttributes.Virtual
                | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName;
        else
            baseFlags = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual
                | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName;

        // add_ method: takes delegate handler, returns EventRegistrationToken
        var addSig = EncodeEventAccessorSig(evt, isAdd: true);
        var adder = M.AddMethodDefinition(
            baseFlags,
            MethodImplAttributes.Runtime,
            M.GetOrAddString($"add_{evt.Name}"),
            M.GetOrAddBlob(addSig),
            -1,
            _emitter.NextParamHandle);
        _emitter.IncrementMethodRow();

        M.AddParameter(ParameterAttributes.In, M.GetOrAddString("handler"), 1);
        _emitter.IncrementParamRow();
        EmitRemainingAttributes(adder, evt.Attributes);

        // remove_ method
        var removeSig = EncodeEventAccessorSig(evt, isAdd: false);
        var remover = M.AddMethodDefinition(
            baseFlags,
            MethodImplAttributes.Runtime,
            M.GetOrAddString($"remove_{evt.Name}"),
            M.GetOrAddBlob(removeSig),
            -1,
            _emitter.NextParamHandle);
        _emitter.IncrementMethodRow();

        M.AddParameter(ParameterAttributes.In, M.GetOrAddString("token"), 1);
        _emitter.IncrementParamRow();

        return (adder, remover);
    }

    #endregion

    #region ApiContract

    private void EmitApiContract(ApiContractSymbol symbol)
    {
        var ns = GetNamespace(symbol);
        var fieldList = _emitter.NextFieldHandle;
        var methodList = _emitter.NextMethodHandle;

        var typeDef = M.AddTypeDefinition(
            (TypeAttributes)(TypeAttributes.Public | TypeAttributes.Sealed
                | (TypeAttributes)MetadataConstants.TdWindowsRuntime),
            M.GetOrAddString(ns),
            M.GetOrAddString(symbol.Name),
            _emitter.SystemValueTypeRef,
            fieldList,
            methodList);

        _attrs.AddApiContractAttribute(typeDef);
        _attrs.AddContractVersionAttribute(typeDef, (uint)(symbol.ContractVersion ?? 0x00010000));
    }

    #endregion

    #region Attribute

    private void EmitAttribute(AttributeTypeSymbol symbol)
    {
        var ns = GetNamespace(symbol);
        var fieldList = _emitter.NextFieldHandle;
        var methodList = _emitter.NextMethodHandle;

        var typeDef = M.AddTypeDefinition(
            (TypeAttributes)(TypeAttributes.Public | TypeAttributes.Sealed
                | (TypeAttributes)MetadataConstants.TdWindowsRuntime),
            M.GetOrAddString(ns),
            M.GetOrAddString(symbol.Name),
            _emitter.SystemAttributeRef,
            fieldList,
            methodList);

        // .ctor with parameters matching fields
        var ctorSig = new BlobBuilder();
        new BlobEncoder(ctorSig)
            .MethodSignature(isInstanceMethod: true)
            .Parameters(symbol.Fields.Count,
                ret => ret.Void(),
                p =>
                {
                    foreach (var field in symbol.Fields)
                        p.AddParameter().Type().FromTypeRef(field.Type, _emitter);
                });

        var ctorParamList = _emitter.NextParamHandle;
        M.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.HideBySig
                | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            MethodImplAttributes.Runtime,
            M.GetOrAddString(".ctor"),
            M.GetOrAddBlob(ctorSig),
            -1,
            ctorParamList);
        _emitter.IncrementMethodRow();

        for (int i = 0; i < symbol.Fields.Count; i++)
        {
            M.AddParameter(ParameterAttributes.None,
                M.GetOrAddString(symbol.Fields[i].Name),
                i + 1);
            _emitter.IncrementParamRow();
        }

        // [AttributeUsage] — emit CLR System.AttributeTargets flags (matching MIDL.exe)
        if (symbol.ValidTargets.Count > 0)
        {
            var clrFlags = MetadataConstants.ClrAttributeTargets.FromTargetStrings(symbol.ValidTargets);
            _attrs.AddAttributeUsageAttribute(typeDef, clrFlags);
        }

        // [ContractVersion] from [contract] attribute, or [Version] as fallback
        var contractAttr = symbol.Attributes
            .FirstOrDefault(a => a.Name.Equals("contract", StringComparison.OrdinalIgnoreCase));
        if (contractAttr != null && contractAttr.Arguments.Count >= 2)
        {
            var contractName = contractAttr.GetTypeName(0) ?? contractAttr.GetIdentifier(0);
            var version = contractAttr.GetVersion(1) ?? 0x00010000u;
            if (contractName != null)
                _attrs.AddContractVersionAttribute(typeDef, contractName, version);
            else
                _attrs.AddContractVersionAttribute(typeDef, version);
        }
        else
        {
            _attrs.AddVersionAttribute(typeDef);
        }
    }

    #endregion

    #region Method Emission Helpers

    private static ParameterAttributes GetParamAttributes(ParameterSymbol param) =>
        param.Direction == ParameterDirection.Out ? ParameterAttributes.Out : ParameterAttributes.In;

    private MethodDefinitionHandle EmitInterfaceMethod(MethodSymbol method)
    {
        var sig = EncodeMethodSig(method.ReturnType, method.Parameters, hasThis: false);
        var paramList = _emitter.NextParamHandle;

        var methodDef = M.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Abstract
                | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
            MethodImplAttributes.IL,
            M.GetOrAddString(method.Name),
            M.GetOrAddBlob(sig),
            -1,
            paramList);
        _emitter.IncrementMethodRow();

        // Emit retval param at sequence 0 for non-void return types
        var retvalName = GetRetvalParamName(method);
        if (retvalName != null)
        {
            M.AddParameter(ParameterAttributes.Out, M.GetOrAddString(retvalName), 0);
            _emitter.IncrementParamRow();
        }

        for (int i = 0; i < method.Parameters.Count; i++)
        {
            var paramHandle = M.AddParameter(GetParamAttributes(method.Parameters[i]),
                M.GetOrAddString(method.Parameters[i].Name),
                i + 1);
            _emitter.IncrementParamRow();
            EmitRemainingAttributes(paramHandle, method.Parameters[i].Attributes);
            EmitArrayParamAttribute(paramHandle, method.Parameters[i]);
        }

        if (method.OverloadGroup != null)
            _attrs.AddOverloadAttribute(methodDef, method.OverloadGroup);
        if (method.IsDefaultOverload)
            _attrs.AddDefaultOverloadAttribute(methodDef);

        // MethodNameAttribute
        if (method.MethodNameOverride != null)
            _attrs.AddMethodNameAttribute(methodDef, method.MethodNameOverride);

        // ReturnNameAttribute
        if (method.ReturnNameOverride != null)
            _attrs.AddReturnNameAttribute(methodDef, method.ReturnNameOverride);

        // NoExceptionAttribute
        if (method.Attributes.Any(a => a.Name.Equals("noexcept", StringComparison.OrdinalIgnoreCase)))
            _attrs.AddNoExceptionAttribute(methodDef);

        // RemoteAsyncAttribute
        if (method.IsRemoteAsync)
            _attrs.AddRemoteAsyncAttribute(methodDef);

        // DeprecatedAttribute on method
        var deprecatedAttr = method.Attributes.FirstOrDefault(a => a.Name.Equals("deprecated", StringComparison.OrdinalIgnoreCase));
        if (deprecatedAttr != null)
            _attrs.AddDeprecatedAttribute(methodDef, deprecatedAttr.GetString(0) ?? "", 0, (uint)(method.ContractVersion ?? 0x00010000));

        // Emit remaining non-handled method-level attributes via the generic path
        EmitRemainingAttributes(methodDef, method.Attributes);

        return methodDef;
    }

    private MethodDefinitionHandle EmitPropertyGetter(PropertySymbol prop)
    {
        var sig = EncodeMethodSig(prop.Type, Array.Empty<ParameterSymbol>(), hasThis: false);
        var paramList = _emitter.NextParamHandle;

        var handle = M.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Abstract
                | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName,
            MethodImplAttributes.IL,
            M.GetOrAddString($"get_{prop.Name}"),
            M.GetOrAddBlob(sig),
            -1,
            paramList);
        _emitter.IncrementMethodRow();

        // NoExceptionAttribute on property getter
        if (prop.Attributes.Any(a => a.Name.Equals("noexcept", StringComparison.OrdinalIgnoreCase)))
            _attrs.AddNoExceptionAttribute(handle);

        // DeprecatedAttribute on property getter
        var deprecatedAttr = prop.Attributes.FirstOrDefault(a => a.Name.Equals("deprecated", StringComparison.OrdinalIgnoreCase));
        if (deprecatedAttr != null)
            _attrs.AddDeprecatedAttribute(handle, deprecatedAttr.GetString(0) ?? "", 0, (uint)(prop.ContractVersion ?? 0x00010000));

        EmitRemainingAttributes(handle, prop.Attributes);
        return handle;
    }

    private MethodDefinitionHandle EmitPropertySetter(PropertySymbol prop)
    {
        var sig = EncodeSetterSig(prop.Type);
        var paramList = _emitter.NextParamHandle;

        var handle = M.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Abstract
                | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName,
            MethodImplAttributes.IL,
            M.GetOrAddString($"put_{prop.Name}"),
            M.GetOrAddBlob(sig),
            -1,
            paramList);
        _emitter.IncrementMethodRow();

        M.AddParameter(ParameterAttributes.In, M.GetOrAddString("value"), 1);
        _emitter.IncrementParamRow();

        return handle;
    }

    private (MethodDefinitionHandle adder, MethodDefinitionHandle remover) EmitEventAccessors(EventSymbol evt)
    {
        // add_ method
        var addSig = EncodeEventAccessorSig(evt, isAdd: true);
        var addHandle = M.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Abstract
                | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName,
            MethodImplAttributes.IL,
            M.GetOrAddString($"add_{evt.Name}"),
            M.GetOrAddBlob(addSig),
            -1,
            _emitter.NextParamHandle);
        _emitter.IncrementMethodRow();

        M.AddParameter(ParameterAttributes.In, M.GetOrAddString("handler"), 1);
        _emitter.IncrementParamRow();
        EmitRemainingAttributes(addHandle, evt.Attributes);

        // remove_ method
        var removeSig = EncodeEventAccessorSig(evt, isAdd: false);
        var removeHandle = M.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Abstract
                | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName,
            MethodImplAttributes.IL,
            M.GetOrAddString($"remove_{evt.Name}"),
            M.GetOrAddBlob(removeSig),
            -1,
            _emitter.NextParamHandle);
        _emitter.IncrementMethodRow();

        M.AddParameter(ParameterAttributes.In, M.GetOrAddString("token"), 1);
        _emitter.IncrementParamRow();

        return (addHandle, removeHandle);
    }

    #endregion

    #region Property/Event Table Helpers

    private void EmitPropertyTable(
        TypeDefinitionHandle typeDef,
        List<(PropertySymbol prop, MethodDefinitionHandle getter, MethodDefinitionHandle? setter)> properties)
    {
        if (properties.Count == 0) return;

        bool first = true;
        foreach (var (prop, getter, setter) in properties)
        {
            var propSig = EncodePropertySig(prop.Type);
            var propHandle = M.AddProperty(
                PropertyAttributes.None,
                M.GetOrAddString(prop.Name),
                M.GetOrAddBlob(propSig));

            if (first)
            {
                M.AddPropertyMap(typeDef, propHandle);
                first = false;
            }

            M.AddMethodSemantics(propHandle, MethodSemanticsAttributes.Getter, getter);
            if (setter.HasValue)
                M.AddMethodSemantics(propHandle, MethodSemanticsAttributes.Setter, setter.Value);
        }
    }

    private void EmitEventTable(
        TypeDefinitionHandle typeDef,
        List<(EventSymbol evt, MethodDefinitionHandle adder, MethodDefinitionHandle remover)> events)
    {
        if (events.Count == 0) return;

        bool first = true;
        foreach (var (evt, adder, remover) in events)
        {
            EntityHandle eventType;
            if (evt.DelegateType.TypeArguments.Count > 0)
                eventType = ResolveGenericTypeHandle(evt.DelegateType);
            else
                eventType = _emitter.ResolveTypeHandle(evt.DelegateType);

            var eventHandle = M.AddEvent(
                EventAttributes.None,
                M.GetOrAddString(evt.Name),
                eventType.IsNil ? _emitter.SystemObjectRef : eventType);

            if (first)
            {
                M.AddEventMap(typeDef, eventHandle);
                first = false;
            }

            M.AddMethodSemantics(eventHandle, MethodSemanticsAttributes.Adder, adder);
            M.AddMethodSemantics(eventHandle, MethodSemanticsAttributes.Remover, remover);
        }
    }

    /// <summary>
    /// Resolves a generic type reference to a TypeSpec handle encoding the generic instantiation.
    /// </summary>
    private EntityHandle ResolveGenericTypeHandle(TypeRef typeRef)
    {
        if (typeRef.ResolvedSymbol == null)
            return default;

        var genTypeHandle = _emitter.ResolveSymbolHandle(typeRef.ResolvedSymbol, typeRef.TypeArguments.Count);
        if (genTypeHandle.IsNil)
            return default;

        bool isValueType = typeRef.ResolvedSymbol is EnumSymbol or StructSymbol;
        var typeSpecSig = new BlobBuilder();
        var typeSpecEncoder = new BlobEncoder(typeSpecSig).TypeSpecificationSignature();
        var genInstEncoder = typeSpecEncoder.GenericInstantiation(genTypeHandle, typeRef.TypeArguments.Count, isValueType);
        foreach (var arg in typeRef.TypeArguments)
        {
            genInstEncoder.AddArgument().FromTypeRef(arg, _emitter);
        }

        return M.AddTypeSpecification(M.GetOrAddBlob(typeSpecSig));
    }

    /// <summary>
    /// Encodes a generic type reference into a SignatureTypeEncoder (for method signatures).
    /// </summary>
    private void EncodeGenericTypeRef(SignatureTypeEncoder encoder, TypeRef typeRef)
    {
        encoder.FromTypeRef(typeRef, _emitter);
    }

    #endregion

    #region Signature Encoding Helpers

    private BlobBuilder EncodeFieldSig(string typeName)
    {
        var blob = new BlobBuilder();
        var enc = new BlobEncoder(blob).FieldSignature();
        EncodePrimitiveType(enc, typeName);
        return blob;
    }

    private BlobBuilder EncodeTypedFieldSig(TypeDefinitionHandle typeDef)
    {
        var blob = new BlobBuilder();
        new BlobEncoder(blob).FieldSignature().Type(typeDef, isValueType: true);
        return blob;
    }

    private BlobBuilder EncodeMethodSig(TypeRef returnType, IReadOnlyList<ParameterSymbol> parameters, bool hasThis)
    {
        // In WinRT WinMD, array params have an implicit preceding UInt32 length param
        // that must be omitted from the MethodDefSig. Filter those out.
        var winmdParams = new List<ParameterSymbol>();
        for (int i = 0; i < parameters.Count; i++)
        {
            // Skip UInt32 length params that precede array params
            if (i + 1 < parameters.Count && parameters[i + 1].IsArray
                && parameters[i].Type.Name is "UInt32")
            {
                continue;
            }
            winmdParams.Add(parameters[i]);
        }

        var blob = new BlobBuilder();
        new BlobEncoder(blob)
            .MethodSignature(isInstanceMethod: hasThis)
            .Parameters(winmdParams.Count,
                ret =>
                {
                    if (returnType.Name == "void")
                        ret.Void();
                    else
                        ret.Type().FromTypeRef(returnType, _emitter);
                },
                p =>
                {
                    foreach (var param in winmdParams)
                    {
                        var paramEnc = p.AddParameter();
                        if (param.Direction is ParameterDirection.Out or ParameterDirection.RefConst)
                            paramEnc.Type(isByRef: true).FromTypeRef(param.Type, _emitter);
                        else
                            paramEnc.Type().FromTypeRef(param.Type, _emitter);
                    }
                });
        return blob;
    }

    private BlobBuilder EncodeSetterSig(TypeRef propertyType)
    {
        var blob = new BlobBuilder();
        new BlobEncoder(blob)
            .MethodSignature()
            .Parameters(1,
                ret => ret.Void(),
                p => p.AddParameter().Type().FromTypeRef(propertyType, _emitter));
        return blob;
    }

    private BlobBuilder EncodePropertySig(TypeRef propertyType)
    {
        var blob = new BlobBuilder();
        new BlobEncoder(blob)
            .PropertySignature(isInstanceProperty: true)
            .Parameters(0,
                ret => ret.Type().FromTypeRef(propertyType, _emitter),
                _ => { });
        return blob;
    }

    private BlobBuilder EncodeEventAccessorSig(EventSymbol evt, bool isAdd)
    {
        var blob = new BlobBuilder();
        if (isAdd)
        {
            // add: takes delegate handler, returns EventRegistrationToken
            new BlobEncoder(blob)
                .MethodSignature()
                .Parameters(1,
                    ret => ret.Type().Type(_emitter.EventRegistrationTokenRef, isValueType: true),
                    p =>
                    {
                        var typeEnc = p.AddParameter().Type();
                        var handle = _emitter.ResolveTypeHandle(evt.DelegateType);
                        if (!handle.IsNil)
                        {
                            if (evt.DelegateType.TypeArguments.Count > 0)
                                EncodeGenericTypeRef(typeEnc, evt.DelegateType);
                            else
                                typeEnc.Type(handle, isValueType: false);
                        }
                        else
                            typeEnc.Object();
                    });
        }
        else
        {
            // remove: takes EventRegistrationToken, returns void
            new BlobEncoder(blob)
                .MethodSignature()
                .Parameters(1,
                    ret => ret.Void(),
                    p => p.AddParameter().Type().Type(_emitter.EventRegistrationTokenRef, isValueType: true));
        }
        return blob;
    }

    private static void EncodePrimitiveType(SignatureTypeEncoder enc, string typeName)
    {
        switch (typeName)
        {
            case "Boolean": enc.Boolean(); break;
            case "Char": enc.Char(); break;
            case "Int16": enc.Int16(); break;
            case "Int32": enc.Int32(); break;
            case "Int64": enc.Int64(); break;
            case "UInt8" or "byte" or "Byte": enc.Byte(); break;
            case "UInt16": enc.UInt16(); break;
            case "UInt32": enc.UInt32(); break;
            case "UInt64": enc.UInt64(); break;
            case "Single": enc.Single(); break;
            case "Double": enc.Double(); break;
            case "String": enc.String(); break;
            case "Object": enc.Object(); break;
            default: enc.Object(); break;
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Auto-generates the retval parameter name per WinRT convention.
    /// Returns null for void-returning methods.
    /// </summary>
    private static string? GetRetvalParamName(MethodSymbol method)
    {
        var returnTypeName = method.ReturnType.Name;
        if (returnTypeName == "void")
            return null;

        if (returnTypeName is "IAsyncAction" or "IAsyncOperation"
            || returnTypeName.StartsWith("IAsyncOperation<")
            || returnTypeName.StartsWith("IAsyncAction<"))
            return "operation";

        return "result";
    }

    private static string GetNamespace(ISymbol symbol)
    {
        if (symbol.ContainingSymbol is NamespaceSymbol ns && ns.FullName.Length > 0)
            return ns.FullName;
        return "";
    }

    internal static bool HasDirectMembers(RuntimeClassSymbol rc)
    {
        return rc.Members.Any(m => m is MethodSymbol { IsStatic: false }
            or PropertySymbol { IsStatic: false }
            or EventSymbol { IsStatic: false });
    }

    /// <summary>
    /// Well-known namespace UUID for non-parameterized interface GUID generation.
    /// Different from the pinterface namespace UUID (11f47ad5-...).
    /// </summary>
    private static readonly Guid InterfaceNamespaceGuid = new("e72a134c-baf7-4dd3-b542-77848e87b138");

    /// <summary>
    /// Generates a deterministic GUID for an interface using the ABI signature seed.
    /// Matches original midl.exe algorithm: UUID v5 with base {e72a134c-...} and
    /// seed = "{fullName}:{method1Sig};{method2Sig};..."
    /// </summary>
    private static Guid GenerateDeterministicGuid(string fullName, IReadOnlyList<ISymbol> members)
    {
        var seed = BuildAbiSignatureSeed(fullName, members);
        return GenerateUuidV5(InterfaceNamespaceGuid, seed);
    }

    /// <summary>
    /// Builds the ABI signature seed string for GUID generation.
    /// Format: "{fullName}:HRESULT method1(params);HRESULT method2(params);..."
    /// </summary>
    internal static string BuildAbiSignatureSeed(string fullName, IReadOnlyList<ISymbol> members)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(fullName);
        sb.Append(':');

        foreach (var member in members)
        {
            switch (member)
            {
                case MethodSymbol method:
                    AppendMethodSeed(sb, method.Name, method.Parameters, method.ReturnType);
                    break;
                case PropertySymbol prop:
                    // getter: HRESULT get_Name(Type*);
                    AppendMethodSeed(sb, $"get_{prop.Name}", [], prop.Type);
                    if (prop.HasSetter)
                    {
                        // setter: HRESULT put_Name(Type);
                        var setterParam = new ParameterSymbol("value", null) { Type = prop.Type };
                        AppendMethodSeed(sb, $"put_{prop.Name}", [setterParam], TypeRef.Void);
                    }
                    break;
                case EventSymbol evt:
                    // add_: HRESULT add_Name(DelegateType*,Windows.Foundation.EventRegistrationToken*);
                    AppendEventAddSeed(sb, evt);
                    // remove_: HRESULT remove_Name(EventRegistrationToken);
                    AppendEventRemoveSeed(sb, evt);
                    break;
            }
        }

        return sb.ToString();
    }

    private static void AppendMethodSeed(System.Text.StringBuilder sb, string name,
        IReadOnlyList<ParameterSymbol> parameters, TypeRef returnType)
    {
        sb.Append("HRESULT ");
        sb.Append(name);
        sb.Append('(');

        bool first = true;
        foreach (var p in parameters)
        {
            if (!first) sb.Append(',');
            first = false;
            AppendTypeSeed(sb, p.Type);
            if (IsReferenceType(p.Type))
                sb.Append('*');
        }

        if (returnType.Name != "void")
        {
            if (!first) sb.Append(',');
            AppendTypeSeed(sb, returnType);
            // Reference types get * for pointer, then * for out = **
            // Value types get * for out
            sb.Append(IsReferenceType(returnType) ? "**" : "*");
        }

        sb.Append(");");
    }

    /// <summary>
    /// Determines if a TypeRef is a reference type in ABI (passed as pointer).
    /// Interfaces, delegates, runtime classes, and Object (IInspectable) are reference types.
    /// </summary>
    private static bool IsReferenceType(TypeRef typeRef)
    {
        if (typeRef.Name == "Object") return true;
        return typeRef.ResolvedSymbol is InterfaceSymbol or DelegateSymbol or RuntimeClassSymbol;
    }

    private static void AppendEventAddSeed(System.Text.StringBuilder sb, EventSymbol evt)
    {
        sb.Append("HRESULT add_");
        sb.Append(evt.Name);
        sb.Append('(');
        AppendTypeSeed(sb, evt.DelegateType);
        sb.Append("*,Windows.Foundation.EventRegistrationToken*);");
    }

    private static void AppendEventRemoveSeed(System.Text.StringBuilder sb, EventSymbol evt)
    {
        sb.Append("HRESULT remove_");
        sb.Append(evt.Name);
        sb.Append("(EventRegistrationToken);");
    }

    private static void AppendTypeSeed(System.Text.StringBuilder sb, TypeRef typeRef)
    {
        if (typeRef.TypeArguments.Count > 0)
        {
            // Generic type: Windows.Foundation.EventHandler`1<Object>
            var resolvedName = typeRef.ResolvedSymbol?.FullName ?? typeRef.Name;
            sb.Append(resolvedName);
            sb.Append('`');
            sb.Append(typeRef.TypeArguments.Count);
            sb.Append('<');
            for (int i = 0; i < typeRef.TypeArguments.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                AppendTypeSeed(sb, typeRef.TypeArguments[i]);
            }
            sb.Append('>');
        }
        else
        {
            sb.Append(typeRef.ResolvedSymbol?.FullName ?? typeRef.Name);
        }
    }

    /// <summary>
    /// Computes a UUID v5 (SHA-1 based) from a namespace UUID and a name string.
    /// </summary>
    private static Guid GenerateUuidV5(Guid namespaceId, string name)
    {
        var nsBytes = namespaceId.ToByteArray();
        // Convert to big-endian (network byte order)
        (nsBytes[0], nsBytes[3]) = (nsBytes[3], nsBytes[0]);
        (nsBytes[1], nsBytes[2]) = (nsBytes[2], nsBytes[1]);
        (nsBytes[4], nsBytes[5]) = (nsBytes[5], nsBytes[4]);
        (nsBytes[6], nsBytes[7]) = (nsBytes[7], nsBytes[6]);

        var nameBytes = System.Text.Encoding.UTF8.GetBytes(name);
        var input = new byte[nsBytes.Length + nameBytes.Length];
        Array.Copy(nsBytes, input, nsBytes.Length);
        Array.Copy(nameBytes, 0, input, nsBytes.Length, nameBytes.Length);

        var hash = System.Security.Cryptography.SHA1.HashData(input);

        // Set version 5 (byte 6, high nibble)
        hash[6] = (byte)((hash[6] & 0x0F) | 0x50);
        // Set variant RFC 4122 (byte 8, high 2 bits)
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        // Convert from big-endian UUID to .NET Guid
        int a = (hash[0] << 24) | (hash[1] << 16) | (hash[2] << 8) | hash[3];
        short b = (short)((hash[4] << 8) | hash[5]);
        short c = (short)((hash[6] << 8) | hash[7]);
        byte[] d = { hash[8], hash[9], hash[10], hash[11], hash[12], hash[13], hash[14], hash[15] };

        return new Guid(a, b, c, d);
    }

    #endregion
}

/// <summary>
/// Extension methods for encoding type references in metadata signatures.
/// </summary>
internal static class SignatureTypeEncoderExtensions
{
    public static void FromTypeRef(this SignatureTypeEncoder encoder, TypeRef typeRef, WinMdEmitter emitter)
    {
        if (typeRef.IsArray)
        {
            // Encode as SZArray (single-dimensional zero-based array)
            var elemType = new TypeRef(typeRef.Name, typeRef.TypeArguments)
            {
                IsBuiltIn = typeRef.IsBuiltIn,
                ResolvedSymbol = typeRef.ResolvedSymbol,
            };
            encoder.SZArray().FromTypeRef(elemType, emitter);
            return;
        }

        switch (typeRef.Name)
        {
            case "Boolean": encoder.Boolean(); break;
            case "Char" or "wchar_t": encoder.Char(); break;
            case "Int16" or "short": encoder.Int16(); break;
            case "Int32" or "int" or "long": encoder.Int32(); break;
            case "Int64" or "hyper": encoder.Int64(); break;
            case "UInt8" or "byte": encoder.Byte(); break;
            case "UInt16": encoder.UInt16(); break;
            case "UInt32": encoder.UInt32(); break;
            case "UInt64": encoder.UInt64(); break;
            case "Single" or "float": encoder.Single(); break;
            case "Double": encoder.Double(); break;
            case "String": encoder.String(); break;
            case "Object": encoder.Object(); break;
            case "Guid":
                encoder.Type(emitter.SystemGuidRef, isValueType: true);
                break;
            case "HRESULT":
                // /winrt compat mode: HRESULT return/param types are emitted as the
                // Windows.Foundation.HResult value struct. CLR projection converts this
                // to System.Exception when the WinMD is read by .NET tools (matching midlrt).
                encoder.Type(emitter.ResolveExternalByName("Windows.Foundation.HResult"), isValueType: true);
                break;
            default:
                if (typeRef.TypeArguments.Count > 0)
                {
                    EncodeGenericInstantiation(encoder, typeRef, emitter);
                }
                else if (typeRef.ResolvedSymbol != null)
                {
                    var handle = emitter.ResolveSymbolHandle(typeRef.ResolvedSymbol);
                    bool isValueType = typeRef.ResolvedSymbol is EnumSymbol or StructSymbol;
                    encoder.Type(handle, isValueType);
                }
                else if (!string.IsNullOrEmpty(typeRef.Name))
                {
                    // Unresolved external type (e.g., flattened interface member signatures
                    // carry raw name strings). Emit a TypeRef by name.
                    var handle = emitter.ResolveExternalByName(typeRef.Name);
                    if (!handle.IsNil)
                        encoder.Type(handle, isValueType: false);
                    else
                        encoder.Object();
                }
                else
                {
                    encoder.Object();
                }
                break;
        }
    }

    private static void EncodeGenericInstantiation(SignatureTypeEncoder encoder, TypeRef typeRef, WinMdEmitter emitter)
    {
        EntityHandle genTypeHandle = default;
        bool isValueType = false;

        if (typeRef.ResolvedSymbol != null)
        {
            genTypeHandle = emitter.ResolveSymbolHandle(typeRef.ResolvedSymbol, typeRef.TypeArguments.Count);
            isValueType = typeRef.ResolvedSymbol is EnumSymbol or StructSymbol;
        }
        else if (!string.IsNullOrEmpty(typeRef.Name))
        {
            // Unresolved external generic instantiation — look up by full name + arity.
            genTypeHandle = emitter.ResolveExternalByName(typeRef.Name, typeRef.TypeArguments.Count);
        }

        if (genTypeHandle.IsNil)
        {
            encoder.Object();
            return;
        }

        // Write generic instantiation inline in the signature using GenericInstantiation encoder
        var genInstEncoder = encoder.GenericInstantiation(genTypeHandle, typeRef.TypeArguments.Count, isValueType);
        foreach (var arg in typeRef.TypeArguments)
        {
            genInstEncoder.AddArgument().FromTypeRef(arg, emitter);
        }
    }
}
