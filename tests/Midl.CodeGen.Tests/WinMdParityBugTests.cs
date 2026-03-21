using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for 5 critical WinMD emitter bugs found by parity comparison.
/// Each test is written to fail before the fix is applied.
/// </summary>
public class WinMdParityBugTests
{
    private static byte[] CompileAndEmit(string source)
    {
        var compilation = CompilationHelper.Compile(source);
        Assert.Empty(compilation.Diagnostics);
        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    private static MetadataReader CreateReader(byte[] bytes, out PEReader peReader)
    {
        peReader = new PEReader(new MemoryStream(bytes));
        return peReader.GetMetadataReader();
    }

    private static TypeDefinitionHandle FindTypeDef(MetadataReader reader, string name)
    {
        foreach (var handle in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(handle);
            if (reader.GetString(td.Name) == name)
                return handle;
        }
        throw new Exception($"TypeDef '{name}' not found");
    }

    private static uint? GetVersionAttributeValue(MetadataReader reader, EntityHandle parent)
    {
        foreach (var attrHandle in reader.GetCustomAttributes(parent))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind != HandleKind.MemberReference) continue;
            var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
            if (memberRef.Parent.Kind != HandleKind.TypeReference) continue;
            var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
            if (reader.GetString(typeRef.Name) != "VersionAttribute") continue;

            var blob = reader.GetBlobReader(attr.Value);
            blob.ReadUInt16(); // prolog
            return blob.ReadUInt32();
        }
        return null;
    }

    #region Bug 1: Enum values ignored

    [Fact]
    public void FlagsEnumExplicitValues_ArePreserved()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [flags]
                enum Perms
                {
                    Read = 0x1,
                    Write = 0x2,
                    Execute = 0x4,
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Perms");
            var td = reader.GetTypeDefinition(handle);
            var fields = td.GetFields().Select(f => reader.GetFieldDefinition(f)).ToList();

            // Skip value__ field, get member fields
            var memberFields = fields.Skip(1).ToList();
            Assert.Equal(3, memberFields.Count);
            Assert.Equal("Read", reader.GetString(memberFields[0].Name));
            Assert.Equal("Write", reader.GetString(memberFields[1].Name));
            Assert.Equal("Execute", reader.GetString(memberFields[2].Name));

            // Verify explicit values 1, 2, 4 (not sequential 0, 1, 2)
            var values = memberFields.Select(f =>
            {
                var constant = reader.GetConstant(f.GetDefaultValue());
                var blobReader = reader.GetBlobReader(constant.Value);
                return (int)blobReader.ReadUInt32();
            }).ToList();

            Assert.Equal(new[] { 1, 2, 4 }, values);
        }
    }

    [Fact]
    public void NonFlagsEnumExplicitValues_ArePreserved()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                enum Priority
                {
                    Low = 10,
                    Medium = 20,
                    High = 30,
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Priority");
            var td = reader.GetTypeDefinition(handle);
            var fields = td.GetFields().Select(f => reader.GetFieldDefinition(f)).ToList();

            var memberFields = fields.Skip(1).ToList();
            var values = memberFields.Select(f =>
            {
                var constant = reader.GetConstant(f.GetDefaultValue());
                var blobReader = reader.GetBlobReader(constant.Value);
                return blobReader.ReadInt32();
            }).ToList();

            Assert.Equal(new[] { 10, 20, 30 }, values);
        }
    }

    [Fact]
    public void SequentialEnumValues_StillWorkCorrectly()
    {
        // Enum with no explicit values — should still be 0, 1, 2
        var bytes = CompileAndEmit("""
            namespace Test
            {
                enum Color { Red, Green, Blue }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Color");
            var td = reader.GetTypeDefinition(handle);
            var fields = td.GetFields().Select(f => reader.GetFieldDefinition(f)).ToList();
            var memberFields = fields.Skip(1).ToList();

            var values = memberFields.Select(f =>
            {
                var constant = reader.GetConstant(f.GetDefaultValue());
                var blobReader = reader.GetBlobReader(constant.Value);
                return blobReader.ReadInt32();
            }).ToList();

            Assert.Equal(new[] { 0, 1, 2 }, values);
        }
    }

    #endregion

    #region Bug 2: Version attribute uses wrong value

    [Fact]
    public void RuntimeClassVersionAttribute_UsesExplicitVersion()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [version(3)]
                [default_interface]
                runtimeclass Widget
                {
                    Widget();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Widget");
            var versionValue = GetVersionAttributeValue(reader, handle);

            // [version(3)] should emit 3 * 0x10000 = 196608 in NTDDI format
            Assert.NotNull(versionValue);
            Assert.Equal(196608u, versionValue.Value); // 3 << 16
        }
    }

    [Fact]
    public void RuntimeClassDefaultVersion_Is0x10000()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [default_interface]
                runtimeclass Widget
                {
                    Widget();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Widget");
            var versionValue = GetVersionAttributeValue(reader, handle);

            // Default version (no explicit [version]) → 1 * 0x10000 = 65536
            Assert.NotNull(versionValue);
            Assert.Equal(0x00010000u, versionValue.Value);
        }
    }

    #endregion

    #region Bug 3: Event add/remove signatures wrong

    [Fact]
    public void EventAddMethod_ReturnsEventRegistrationToken()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                delegate void MyHandler();
                runtimeclass Widget
                {
                    Widget();
                    event MyHandler Changed;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            // Check the synthesized interface's add_ method
            var ifaceHandle = FindTypeDef(reader, "IWidget");
            var ifaceTd = reader.GetTypeDefinition(ifaceHandle);

            var addMethod = ifaceTd.GetMethods()
                .Select(m => reader.GetMethodDefinition(m))
                .First(m => reader.GetString(m.Name) == "add_Changed");

            var sig = reader.GetBlobReader(addMethod.Signature);
            var decoder = new SignatureDecoder<string, object>(new TypeNameProvider(reader), reader, null!);
            var methodSig = decoder.DecodeMethodSignature(ref sig);

            // add_ should return EventRegistrationToken (a value type), not void
            Assert.NotEqual("void", methodSig.ReturnType);
            Assert.Contains("EventRegistrationToken", methodSig.ReturnType);
        }
    }

    [Fact]
    public void EventRemoveMethod_TakesEventRegistrationToken()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                delegate void MyHandler();
                runtimeclass Widget
                {
                    Widget();
                    event MyHandler Changed;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var ifaceHandle = FindTypeDef(reader, "IWidget");
            var ifaceTd = reader.GetTypeDefinition(ifaceHandle);

            var removeMethod = ifaceTd.GetMethods()
                .Select(m => reader.GetMethodDefinition(m))
                .First(m => reader.GetString(m.Name) == "remove_Changed");

            var sig = reader.GetBlobReader(removeMethod.Signature);
            var decoder = new SignatureDecoder<string, object>(new TypeNameProvider(reader), reader, null!);
            var methodSig = decoder.DecodeMethodSignature(ref sig);

            // remove_ should take EventRegistrationToken, not Int64
            Assert.Single(methodSig.ParameterTypes);
            Assert.Contains("EventRegistrationToken", methodSig.ParameterTypes[0]);
        }
    }

    #endregion

    #region Bug 4: Generic type arguments not emitted in event table

    [Fact]
    public void GenericEventType_PreservesTypeArguments()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    event Windows.Foundation.TypedEventHandler<Widget, String> Changed;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var ifaceHandle = FindTypeDef(reader, "IWidget");
            var ifaceTd = reader.GetTypeDefinition(ifaceHandle);

            // Find the event in the event table
            var events = ifaceTd.GetEvents();
            Assert.NotEmpty(events);
            var evt = reader.GetEventDefinition(events.First());
            Assert.Equal("Changed", reader.GetString(evt.Name));

            // The event type should be a TypeSpec (generic instantiation), not a plain TypeRef
            var eventTypeHandle = evt.Type;
            Assert.Equal(HandleKind.TypeSpecification, eventTypeHandle.Kind);

            // Decode the TypeSpec to verify generic args are present
            var typeSpec = reader.GetTypeSpecification((TypeSpecificationHandle)eventTypeHandle);
            var sigBlob = reader.GetBlobReader(typeSpec.Signature);

            // The signature should start with GENERICINST (0x15)
            var callingConvention = sigBlob.ReadByte();
            Assert.Equal(0x15, callingConvention); // ELEMENT_TYPE_GENERICINST
        }
    }

    #endregion

    #region Bug 5: Interface GUIDs wrong

    [Fact]
    public void SynthesizedInterfaceGuid_UsesWinRTUuidV5Algorithm()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    void DoSomething();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var ifaceHandle = FindTypeDef(reader, "IWidget");

            // Get the GUID from the GuidAttribute
            var guid = GetGuidAttributeValue(reader, ifaceHandle);
            Assert.NotNull(guid);

            // Compute expected GUID using the correct WinRT UUID v5 algorithm
            var expectedGuid = ComputeWinRTUuidV5("Test.IWidget");
            Assert.Equal(expectedGuid, guid.Value);
        }
    }

    [Fact]
    public void InterfaceGuid_WithoutExplicitUuid_UsesWinRTUuidV5()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IFoo { void Bar(); }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IFoo");
            var guid = GetGuidAttributeValue(reader, handle);
            Assert.NotNull(guid);

            var expectedGuid = ComputeWinRTUuidV5("Test.IFoo");
            Assert.Equal(expectedGuid, guid.Value);
        }
    }

    private static Guid? GetGuidAttributeValue(MetadataReader reader, TypeDefinitionHandle typeDef)
    {
        foreach (var attrHandle in reader.GetCustomAttributes(typeDef))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind != HandleKind.MemberReference) continue;
            var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
            if (memberRef.Parent.Kind != HandleKind.TypeReference) continue;
            var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
            if (reader.GetString(typeRef.Name) != "GuidAttribute") continue;

            var blob = reader.GetBlobReader(attr.Value);
            blob.ReadUInt16(); // prolog
            uint a = blob.ReadUInt32();
            ushort b = blob.ReadUInt16();
            ushort c = blob.ReadUInt16();
            byte[] d = new byte[8];
            for (int i = 0; i < 8; i++)
                d[i] = blob.ReadByte();
            return new Guid((int)a, (short)b, (short)c, d);
        }
        return null;
    }

    /// <summary>
    /// Reference implementation of WinRT UUID v5 algorithm.
    /// Uses base UUID {11f47ad5-7b73-42c0-abae-878b1e16adee} and SHA-1.
    /// </summary>
    private static Guid ComputeWinRTUuidV5(string name)
    {
        // WinRT base UUID in big-endian (network byte order)
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

    /// <summary>
    /// Simple type name provider for decoding method signatures in tests.
    /// </summary>
    private class TypeNameProvider : ISignatureTypeProvider<string, object>
    {
        private readonly MetadataReader _reader;
        public TypeNameProvider(MetadataReader reader) => _reader = reader;

        public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode.ToString();
        public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            var td = reader.GetTypeDefinition(handle);
            return reader.GetString(td.Name);
        }
        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var tr = reader.GetTypeReference(handle);
            return reader.GetString(tr.Name);
        }
        public string GetTypeFromSpecification(MetadataReader reader, object genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        {
            return "TypeSpec";
        }
        public string GetSZArrayType(string elementType) => $"{elementType}[]";
        public string GetGenericInstantiation(string genericType, System.Collections.Immutable.ImmutableArray<string> typeArguments)
            => $"{genericType}<{string.Join(", ", typeArguments)}>";
        public string GetArrayType(string elementType, ArrayShape shape) => $"{elementType}[{shape.Rank}]";
        public string GetByReferenceType(string elementType) => $"{elementType}&";
        public string GetPointerType(string elementType) => $"{elementType}*";
        public string GetFunctionPointerType(MethodSignature<string> signature) => "fnptr";
        public string GetGenericMethodParameter(object genericContext, int index) => $"!!{index}";
        public string GetGenericTypeParameter(object genericContext, int index) => $"!{index}";
        public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;
        public string GetPinnedType(string elementType) => elementType;
    }
}
