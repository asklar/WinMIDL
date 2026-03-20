using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

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
