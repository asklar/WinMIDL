using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

#pragma warning disable CS0169 // Field is never used (scaffolded for future use)

namespace Midl.CodeGen.WinMd;

/// <summary>
/// Encodes and emits WinRT custom attributes to the metadata tables.
/// </summary>
internal class CustomAttributeEmitter
{
    private readonly WinMdEmitter _emitter;
    private MetadataBuilder Metadata => _emitter.Metadata;

    // Lazy-initialized attribute constructor MemberRefs
    private MemberReferenceHandle? _flagsCtorRef;
    private MemberReferenceHandle? _activatableCtorRef;
    private MemberReferenceHandle? _defaultCtorRef;
    private MemberReferenceHandle? _guidCtorRef;
    private MemberReferenceHandle? _versionCtorRef;
    private MemberReferenceHandle? _contractVersionCtorRef;
    private MemberReferenceHandle? _exclusiveToCtorRef;
    private MemberReferenceHandle? _activatableFactoryCtorRef;
    private MemberReferenceHandle? _staticCtorRef;
    private MemberReferenceHandle? _composableCtorRef;
    private MemberReferenceHandle? _threadingCtorRef;
    private MemberReferenceHandle? _marshalingCtorRef;
    private MemberReferenceHandle? _deprecatedCtorRef;
    private MemberReferenceHandle? _contractVersionNamedCtorRef;
    private MemberReferenceHandle? _overloadCtorRef;
    private MemberReferenceHandle? _defaultOverloadCtorRef;
    private MemberReferenceHandle? _overridableCtorRef;
    private MemberReferenceHandle? _protectedCtorRef;
    private MemberReferenceHandle? _experimentalCtorRef;
    private TypeReferenceHandle? _systemTypeRef;

    public CustomAttributeEmitter(WinMdEmitter emitter)
    {
        _emitter = emitter;
    }

    public void AddFlagsAttribute(EntityHandle parent)
    {
        _flagsCtorRef ??= CreateMscorlibCtorRef("System", "FlagsAttribute", Array.Empty<Action<SignatureTypeEncoder>>());
        var blob = EncodeParameterlessAttribute();
        Metadata.AddCustomAttribute(parent, _flagsCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddActivatableAttribute(EntityHandle parent, uint version = 0x00010000)
    {
        _activatableCtorRef ??= CreateWfmCtorRef("ActivatableAttribute",
            new Action<SignatureTypeEncoder>[] { e => e.UInt32(), e => e.String() });

        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteUInt32(version);
        blob.WriteSerializedString("");
        blob.WriteUInt16(0x0000); // named args
        Metadata.AddCustomAttribute(parent, _activatableCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddDefaultAttribute(EntityHandle parent)
    {
        _defaultCtorRef ??= CreateWfmCtorRef("DefaultAttribute", Array.Empty<Action<SignatureTypeEncoder>>());
        var blob = EncodeParameterlessAttribute();
        Metadata.AddCustomAttribute(parent, _defaultCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddGuidAttribute(EntityHandle parent, Guid guid)
    {
        _guidCtorRef ??= CreateWfmCtorRef("GuidAttribute", new Action<SignatureTypeEncoder>[]
        {
            e => e.UInt32(), e => e.UInt16(), e => e.UInt16(),
            e => e.Byte(), e => e.Byte(), e => e.Byte(), e => e.Byte(),
            e => e.Byte(), e => e.Byte(), e => e.Byte(), e => e.Byte(),
        });

        var bytes = guid.ToByteArray();
        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteUInt32(BitConverter.ToUInt32(bytes, 0));
        blob.WriteUInt16(BitConverter.ToUInt16(bytes, 4));
        blob.WriteUInt16(BitConverter.ToUInt16(bytes, 6));
        for (int i = 8; i < 16; i++)
            blob.WriteByte(bytes[i]);
        blob.WriteUInt16(0x0000); // named args
        Metadata.AddCustomAttribute(parent, _guidCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddVersionAttribute(EntityHandle parent, uint version = 0x00010000)
    {
        _versionCtorRef ??= CreateWfmCtorRef("VersionAttribute",
            new Action<SignatureTypeEncoder>[] { e => e.UInt32() });

        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteUInt32(version);
        blob.WriteUInt16(0x0000); // named args
        Metadata.AddCustomAttribute(parent, _versionCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddContractVersionAttribute(EntityHandle parent, uint version)
    {
        _contractVersionCtorRef ??= CreateWfmCtorRef("ContractVersionAttribute",
            new Action<SignatureTypeEncoder>[] { e => e.UInt32() });

        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteUInt32(version);
        blob.WriteUInt16(0x0000); // named args
        Metadata.AddCustomAttribute(parent, _contractVersionCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddContractVersionAttribute(EntityHandle parent, string contractName, uint version)
    {
        _contractVersionNamedCtorRef ??= CreateWfmCtorRef("ContractVersionAttribute",
            new Action<SignatureTypeEncoder>[] { e => e.String(), e => e.UInt32() });

        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteSerializedString(contractName);
        blob.WriteUInt32(version);
        blob.WriteUInt16(0x0000); // named args
        Metadata.AddCustomAttribute(parent, _contractVersionNamedCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddExclusiveToAttribute(EntityHandle parent, string runtimeClassName)
    {
        _exclusiveToCtorRef ??= CreateWfmCtorRef("ExclusiveToAttribute",
            new Action<SignatureTypeEncoder>[] { e => e.Type(GetSystemTypeRef(), isValueType: false) });

        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteSerializedString(runtimeClassName);
        blob.WriteUInt16(0x0000); // named args
        Metadata.AddCustomAttribute(parent, _exclusiveToCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddActivatableAttribute(EntityHandle parent, string factoryTypeName, uint version)
    {
        _activatableFactoryCtorRef ??= CreateWfmCtorRef("ActivatableAttribute",
            new Action<SignatureTypeEncoder>[]
            {
                e => e.Type(GetSystemTypeRef(), isValueType: false),
                e => e.UInt32(),
                e => e.String()
            });

        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteSerializedString(factoryTypeName);
        blob.WriteUInt32(version);
        blob.WriteSerializedString("");
        blob.WriteUInt16(0x0000); // named args
        Metadata.AddCustomAttribute(parent, _activatableFactoryCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddStaticAttribute(EntityHandle parent, string staticTypeName, uint version)
    {
        _staticCtorRef ??= CreateWfmCtorRef("StaticAttribute",
            new Action<SignatureTypeEncoder>[]
            {
                e => e.Type(GetSystemTypeRef(), isValueType: false),
                e => e.UInt32(),
                e => e.String()
            });

        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteSerializedString(staticTypeName);
        blob.WriteUInt32(version);
        blob.WriteSerializedString("");
        blob.WriteUInt16(0x0000); // named args
        Metadata.AddCustomAttribute(parent, _staticCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddComposableAttribute(EntityHandle parent, string factoryTypeName, int access, uint version)
    {
        _composableCtorRef ??= CreateWfmCtorRef("ComposableAttribute",
            new Action<SignatureTypeEncoder>[]
            {
                e => e.Type(GetSystemTypeRef(), isValueType: false),
                e => e.Int32(),
                e => e.UInt32(),
                e => e.String()
            });

        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteSerializedString(factoryTypeName);
        blob.WriteInt32(access);
        blob.WriteUInt32(version);
        blob.WriteSerializedString("");
        blob.WriteUInt16(0x0000); // named args
        Metadata.AddCustomAttribute(parent, _composableCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddThreadingAttribute(EntityHandle parent, int threadingModel)
    {
        _threadingCtorRef ??= CreateWfmCtorRef("ThreadingAttribute",
            new Action<SignatureTypeEncoder>[] { e => e.Int32() });

        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteInt32(threadingModel);
        blob.WriteUInt16(0x0000); // named args
        Metadata.AddCustomAttribute(parent, _threadingCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddMarshalingBehaviorAttribute(EntityHandle parent, int marshalingType)
    {
        _marshalingCtorRef ??= CreateWfmCtorRef("MarshalingBehaviorAttribute",
            new Action<SignatureTypeEncoder>[] { e => e.Int32() });

        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteInt32(marshalingType);
        blob.WriteUInt16(0x0000); // named args
        Metadata.AddCustomAttribute(parent, _marshalingCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddDeprecatedAttribute(EntityHandle parent, string message, int deprecationType, uint version)
    {
        _deprecatedCtorRef ??= CreateWfmCtorRef("DeprecatedAttribute",
            new Action<SignatureTypeEncoder>[] { e => e.String(), e => e.Int32(), e => e.UInt32() });

        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteSerializedString(message);
        blob.WriteInt32(deprecationType);
        blob.WriteUInt32(version);
        blob.WriteUInt16(0x0000); // named args
        Metadata.AddCustomAttribute(parent, _deprecatedCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddOverloadAttribute(EntityHandle parent, string methodName)
    {
        _overloadCtorRef ??= CreateWfmCtorRef("OverloadAttribute",
            new Action<SignatureTypeEncoder>[] { e => e.String() });

        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteSerializedString(methodName);
        blob.WriteUInt16(0x0000); // named args
        Metadata.AddCustomAttribute(parent, _overloadCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddDefaultOverloadAttribute(EntityHandle parent)
    {
        _defaultOverloadCtorRef ??= CreateWfmCtorRef("DefaultOverloadAttribute",
            Array.Empty<Action<SignatureTypeEncoder>>());
        var blob = EncodeParameterlessAttribute();
        Metadata.AddCustomAttribute(parent, _defaultOverloadCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddOverridableAttribute(EntityHandle parent)
    {
        _overridableCtorRef ??= CreateWfmCtorRef("OverridableAttribute",
            Array.Empty<Action<SignatureTypeEncoder>>());
        var blob = EncodeParameterlessAttribute();
        Metadata.AddCustomAttribute(parent, _overridableCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddProtectedAttribute(EntityHandle parent)
    {
        _protectedCtorRef ??= CreateWfmCtorRef("ProtectedAttribute",
            Array.Empty<Action<SignatureTypeEncoder>>());
        var blob = EncodeParameterlessAttribute();
        Metadata.AddCustomAttribute(parent, _protectedCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

    public void AddExperimentalAttribute(EntityHandle parent)
    {
        _experimentalCtorRef ??= CreateWfmCtorRef("ExperimentalAttribute",
            Array.Empty<Action<SignatureTypeEncoder>>());
        var blob = EncodeParameterlessAttribute();
        Metadata.AddCustomAttribute(parent, _experimentalCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }

#if WINDOWS_INTERNAL
    private MemberReferenceHandle? _featureCtorRef;

    public void AddFeatureAttribute(EntityHandle parent, int stage, bool validInAllBranches)
    {
        _featureCtorRef ??= CreateWfmCtorRef("FeatureAttribute",
            new Action<SignatureTypeEncoder>[] { e => e.Int32(), e => e.Boolean() });
        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteInt32(stage);
        blob.WriteBoolean(validInAllBranches);
        blob.WriteUInt16(0x0000); // named args
        Metadata.AddCustomAttribute(parent, _featureCtorRef.Value, Metadata.GetOrAddBlob(blob));
    }
#endif

    private TypeReferenceHandle GetSystemTypeRef()
    {
        _systemTypeRef ??= Metadata.AddTypeReference(
            _emitter.MscorlibRef,
            Metadata.GetOrAddString("System"),
            Metadata.GetOrAddString("Type"));
        return _systemTypeRef.Value;
    }

    private MemberReferenceHandle CreateMscorlibCtorRef(
        string ns, string typeName, Action<SignatureTypeEncoder>[] paramEncoders)
    {
        var typeRef = Metadata.AddTypeReference(
            _emitter.MscorlibRef,
            Metadata.GetOrAddString(ns),
            Metadata.GetOrAddString(typeName));
        return CreateCtorMemberRef(typeRef, paramEncoders);
    }

    private MemberReferenceHandle CreateWfmCtorRef(
        string typeName, Action<SignatureTypeEncoder>[] paramEncoders)
    {
        var typeRef = Metadata.AddTypeReference(
            _emitter.WfmRef,
            Metadata.GetOrAddString(MetadataConstants.WindowsFoundationMetadataNamespace),
            Metadata.GetOrAddString(typeName));
        return CreateCtorMemberRef(typeRef, paramEncoders);
    }

    private MemberReferenceHandle CreateCtorMemberRef(
        TypeReferenceHandle typeRef, Action<SignatureTypeEncoder>[] paramEncoders)
    {
        var sig = new BlobBuilder();
        new BlobEncoder(sig)
            .MethodSignature(isInstanceMethod: true)
            .Parameters(paramEncoders.Length,
                ret => ret.Void(),
                p =>
                {
                    foreach (var encode in paramEncoders)
                        encode(p.AddParameter().Type());
                });
        return Metadata.AddMemberReference(typeRef, Metadata.GetOrAddString(".ctor"), Metadata.GetOrAddBlob(sig));
    }

    private static BlobBuilder EncodeParameterlessAttribute()
    {
        var blob = new BlobBuilder();
        blob.WriteUInt16(0x0001); // prolog
        blob.WriteUInt16(0x0000); // named args
        return blob;
    }
}
