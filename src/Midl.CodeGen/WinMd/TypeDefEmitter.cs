using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
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

    /// <summary>Emit the synthesized default interface for a runtime class.</summary>
    public TypeDefinitionHandle EmitSynthesizedInterface(RuntimeClassSymbol rc)
    {
        var ns = GetNamespace(rc);
        var ifaceName = $"I{rc.Name}";
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

        // Add a deterministic GUID based on the interface name
        var guid = GenerateDeterministicGuid($"{ns}.{ifaceName}");
        _attrs.AddGuidAttribute(typeDef, guid);
        _attrs.AddExclusiveToAttribute(typeDef, rc.FullName);

        // Track properties and events for map tables
        var properties = new List<(PropertySymbol prop, MethodDefinitionHandle getter, MethodDefinitionHandle? setter)>();
        var events = new List<(EventSymbol evt, MethodDefinitionHandle adder, MethodDefinitionHandle remover)>();

        // Track interface method handles for MethodImpl table
        var ifaceMethodHandles = new List<(string memberName, MethodDefinitionHandle handle)>();

        // Emit members from the runtime class (excluding constructors)
        foreach (var member in rc.Members)
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

        _synthMethodHandles[rc.FullName] = ifaceMethodHandles;

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

        // value__ field
        var valueSig = EncodeFieldSig(underlyingTypeName);
        M.AddFieldDefinition(
            FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName,
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
        }

        if (isFlags)
            _attrs.AddFlagsAttribute(typeDef);

        _attrs.AddVersionAttribute(typeDef);
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
            M.AddFieldDefinition(
                FieldAttributes.Public,
                M.GetOrAddString(field.Name),
                M.GetOrAddBlob(sig));
            _emitter.IncrementFieldRow();
        }

        _attrs.AddVersionAttribute(typeDef);
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

        // Emit [Guid] if the interface has a UUID
        if (symbol.Uuid.HasValue)
            _attrs.AddGuidAttribute(typeDef, symbol.Uuid.Value);
        else
            _attrs.AddGuidAttribute(typeDef, GenerateDeterministicGuid(symbol.FullName));

        // Emit version attribute with the symbol's version if available
        if (symbol.Version.HasValue)
            _attrs.AddVersionAttribute(typeDef, (uint)symbol.Version.Value);
        else
            _attrs.AddVersionAttribute(typeDef);

        // Emit [deprecated] if present
        if (symbol.DeprecatedMessage != null)
            _attrs.AddDeprecatedAttribute(typeDef, symbol.DeprecatedMessage, 0, (uint)(symbol.Version ?? 0x00010000));

        if (symbol.IsExclusive && symbol.ExclusiveToClass != null)
            _attrs.AddExclusiveToAttribute(typeDef, symbol.ExclusiveToClass);
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

        var guid = GenerateDeterministicGuid(symbol.FullName);
        _attrs.AddGuidAttribute(typeDef, guid);
        _attrs.AddVersionAttribute(typeDef);
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
            MethodAttributes.Public | MethodAttributes.HideBySig
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
                        p.AddParameter().Type().FromTypeRef(param.Type, _emitter);
                });

        var paramList = _emitter.NextParamHandle;
        M.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
            MethodImplAttributes.Runtime,
            M.GetOrAddString("Invoke"),
            M.GetOrAddBlob(sig),
            -1,
            paramList);
        _emitter.IncrementMethodRow();

        for (int i = 0; i < symbol.Parameters.Count; i++)
        {
            M.AddParameter(ParameterAttributes.None,
                M.GetOrAddString(symbol.Parameters[i].Name),
                i + 1);
            _emitter.IncrementParamRow();
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

        // Emit runtime method stubs for direct members, tracking handles for MethodImpl
        var classMethodStubs = new List<(string memberName, MethodDefinitionHandle handle)>();
        if (hasDirectMembers)
        {
            foreach (var member in symbol.Members)
            {
                switch (member)
                {
                    case MethodSymbol method:
                    {
                        var stubHandle = EmitRuntimeMethodStub(method);
                        classMethodStubs.Add((method.Name, stubHandle));
                        break;
                    }
                    case PropertySymbol prop:
                    {
                        var (getterStub, setterStub) = EmitRuntimePropertyStubs(prop);
                        if (!getterStub.IsNil) classMethodStubs.Add(($"get_{prop.Name}", getterStub));
                        if (setterStub.HasValue) classMethodStubs.Add(($"put_{prop.Name}", setterStub.Value));
                        break;
                    }
                    case EventSymbol evt:
                    {
                        var (adderStub, removerStub) = EmitRuntimeEventStubs(evt);
                        classMethodStubs.Add(($"add_{evt.Name}", adderStub));
                        classMethodStubs.Add(($"remove_{evt.Name}", removerStub));
                        break;
                    }
                }
            }
        }

        // InterfaceImpl for synthesized default interface
        if (hasDirectMembers)
        {
            var synthRef = _emitter.ResolveSynthInterfaceHandle(symbol.FullName);
            if (!synthRef.IsNil)
            {
                var ifaceImpl = M.AddInterfaceImplementation(typeDef, synthRef);
                _attrs.AddDefaultAttribute(ifaceImpl);
            }
        }

        // InterfaceImpl for explicit interfaces
        foreach (var iface in symbol.Interfaces)
        {
            var h = _emitter.ResolveTypeHandle(iface);
            if (!h.IsNil)
                M.AddInterfaceImplementation(typeDef, h);
        }

        // InterfaceImpl for protected interface
        if (symbol.ProtectedInterface != null)
        {
            var h = _emitter.ResolveSymbolHandle(symbol.ProtectedInterface);
            if (!h.IsNil)
            {
                var ifaceImpl = M.AddInterfaceImplementation(typeDef, h);
                _attrs.AddProtectedAttribute(ifaceImpl);
            }
        }

        // InterfaceImpl for overridable interface
        if (symbol.OverridesInterface != null)
        {
            var h = _emitter.ResolveSymbolHandle(symbol.OverridesInterface);
            if (!h.IsNil)
            {
                var ifaceImpl = M.AddInterfaceImplementation(typeDef, h);
                _attrs.AddOverridableAttribute(ifaceImpl);
            }
        }

        // MethodImpl: link class method stubs to synthesized interface method declarations
        if (hasDirectMembers && _synthMethodHandles.TryGetValue(symbol.FullName, out var ifaceMethods))
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

        // ActivatableAttribute
        uint attrVersion = symbol.Version != null ? (uint)(symbol.Version.Value << 16) : 0x00010000;
        if (symbol.Members.Any(m => m is ConstructorSymbol) || symbol.HasDefaultActivation)
            _attrs.AddActivatableAttribute(typeDef, attrVersion);

        if (symbol.FactoryInterface != null)
            _attrs.AddActivatableAttribute(typeDef, symbol.FactoryInterface.FullName, attrVersion);

        // StaticAttribute
        if (symbol.StaticsInterface != null)
            _attrs.AddStaticAttribute(typeDef, symbol.StaticsInterface.FullName, attrVersion);

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

        // DeprecatedAttribute
        if (symbol.DeprecatedMessage != null)
            _attrs.AddDeprecatedAttribute(typeDef, symbol.DeprecatedMessage, 0, attrVersion);

        // ContractVersionAttribute
        if (symbol.ContractName != null && symbol.ContractVersion != null)
            _attrs.AddContractVersionAttribute(typeDef, symbol.ContractName, (uint)symbol.ContractVersion.Value);

        _attrs.AddVersionAttribute(typeDef, attrVersion);
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
            M.AddParameter(ParameterAttributes.None,
                M.GetOrAddString(ctorParams[i].Name),
                i + 1);
            _emitter.IncrementParamRow();
        }
    }

    private MethodDefinitionHandle EmitRuntimeMethodStub(MethodSymbol method)
    {
        var sig = EncodeMethodSig(method.ReturnType, method.Parameters, hasThis: false);
        var paramList = _emitter.NextParamHandle;
        var handle = M.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.HideBySig,
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
            M.AddParameter(ParameterAttributes.None,
                M.GetOrAddString(method.Parameters[i].Name),
                i + 1);
            _emitter.IncrementParamRow();
        }

        return handle;
    }

    private (MethodDefinitionHandle getter, MethodDefinitionHandle? setter) EmitRuntimePropertyStubs(PropertySymbol prop)
    {
        MethodDefinitionHandle getter = default;
        MethodDefinitionHandle? setter = null;

        if (prop.HasGetter)
        {
            var sig = EncodeMethodSig(prop.Type, Array.Empty<ParameterSymbol>(), hasThis: false);
            getter = M.AddMethodDefinition(
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                MethodImplAttributes.Runtime,
                M.GetOrAddString($"get_{prop.Name}"),
                M.GetOrAddBlob(sig),
                -1,
                _emitter.NextParamHandle);
            _emitter.IncrementMethodRow();
        }
        if (prop.HasSetter)
        {
            var sig = EncodeSetterSig(prop.Type);
            setter = M.AddMethodDefinition(
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                MethodImplAttributes.Runtime,
                M.GetOrAddString($"put_{prop.Name}"),
                M.GetOrAddBlob(sig),
                -1,
                _emitter.NextParamHandle);
            _emitter.IncrementMethodRow();
        }

        return (getter, setter);
    }

    private (MethodDefinitionHandle adder, MethodDefinitionHandle remover) EmitRuntimeEventStubs(EventSymbol evt)
    {
        // add_ method: takes EventRegistrationToken handler, returns void (simplified)
        var addSig = EncodeEventAccessorSig(evt, isAdd: true);
        var adder = M.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
            MethodImplAttributes.Runtime,
            M.GetOrAddString($"add_{evt.Name}"),
            M.GetOrAddBlob(addSig),
            -1,
            _emitter.NextParamHandle);
        _emitter.IncrementMethodRow();

        // remove_ method
        var removeSig = EncodeEventAccessorSig(evt, isAdd: false);
        var remover = M.AddMethodDefinition(
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
            MethodImplAttributes.Runtime,
            M.GetOrAddString($"remove_{evt.Name}"),
            M.GetOrAddBlob(removeSig),
            -1,
            _emitter.NextParamHandle);
        _emitter.IncrementMethodRow();

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
            _emitter.SystemObjectRef,
            fieldList,
            methodList);

        _attrs.AddContractVersionAttribute(typeDef, (uint)symbol.Version);
        _attrs.AddVersionAttribute(typeDef);
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

        _attrs.AddVersionAttribute(typeDef);
    }

    #endregion

    #region Method Emission Helpers

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
            M.AddParameter(ParameterAttributes.None,
                M.GetOrAddString(method.Parameters[i].Name),
                i + 1);
            _emitter.IncrementParamRow();
        }

        if (method.OverloadGroup != null)
            _attrs.AddOverloadAttribute(methodDef, method.OverloadGroup);
        if (method.IsDefaultOverload)
            _attrs.AddDefaultOverloadAttribute(methodDef);

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

        M.AddParameter(ParameterAttributes.None, M.GetOrAddString("value"), 1);
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

        M.AddParameter(ParameterAttributes.None, M.GetOrAddString("handler"), 1);
        _emitter.IncrementParamRow();

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

        M.AddParameter(ParameterAttributes.None, M.GetOrAddString("token"), 1);
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

        var genTypeHandle = _emitter.ResolveSymbolHandle(typeRef.ResolvedSymbol);
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
                        p.AddParameter().Type().FromTypeRef(param.Type, _emitter);
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
        return rc.Members.Any(m => m is MethodSymbol or PropertySymbol or EventSymbol);
    }

    private static Guid GenerateDeterministicGuid(string name)
    {
        // WinRT UUID v5 algorithm with base UUID {11f47ad5-7b73-42c0-abae-878b1e16adee}
        byte[] baseUuid = {
            0x11, 0xf4, 0x7a, 0xd5,
            0x7b, 0x73,
            0x42, 0xc0,
            0xab, 0xae,
            0x87, 0x8b, 0x1e, 0x16, 0xad, 0xee
        };

        var nameBytes = System.Text.Encoding.UTF8.GetBytes(name);
        var input = new byte[baseUuid.Length + nameBytes.Length];
        Array.Copy(baseUuid, input, baseUuid.Length);
        Array.Copy(nameBytes, 0, input, baseUuid.Length, nameBytes.Length);

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
            genTypeHandle = emitter.ResolveSymbolHandle(typeRef.ResolvedSymbol);
            isValueType = typeRef.ResolvedSymbol is EnumSymbol or StructSymbol;
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
