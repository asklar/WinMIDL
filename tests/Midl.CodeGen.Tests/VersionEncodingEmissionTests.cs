using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

public class VersionEncodingEmissionTests
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

    /// <summary>
    /// Read the uint32 value from the first matching custom attribute on an entity.
    /// For VersionAttribute: prolog(2) + uint32(4).
    /// For ContractVersionAttribute (1-param): prolog(2) + uint32(4).
    /// </summary>
    private static uint? ReadAttributeUInt32(MetadataReader reader, EntityHandle parent, string attributeTypeName)
    {
        foreach (var attrHandle in reader.GetCustomAttributes(parent))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind == HandleKind.MemberReference)
            {
                var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
                if (memberRef.Parent.Kind == HandleKind.TypeReference)
                {
                    var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                    var fullName = $"{reader.GetString(typeRef.Namespace)}.{reader.GetString(typeRef.Name)}";
                    if (fullName == attributeTypeName)
                    {
                        var blob = reader.GetBlobReader(attr.Value);
                        if (blob.Length >= 6)
                        {
                            blob.ReadUInt16(); // prolog
                            return blob.ReadUInt32();
                        }
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Read the version uint32 from a ContractVersionAttribute with 2 params (string + uint32).
    /// Skips the string and reads the uint32.
    /// </summary>
    private static uint? ReadContractVersionFromNamedAttr(MetadataReader reader, EntityHandle parent)
    {
        foreach (var attrHandle in reader.GetCustomAttributes(parent))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind == HandleKind.MemberReference)
            {
                var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
                if (memberRef.Parent.Kind == HandleKind.TypeReference)
                {
                    var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                    var fullName = $"{reader.GetString(typeRef.Namespace)}.{reader.GetString(typeRef.Name)}";
                    if (fullName == "Windows.Foundation.Metadata.ContractVersionAttribute")
                    {
                        // Check ctor param count
                        var sigBlob = reader.GetBlobReader(memberRef.Signature);
                        sigBlob.ReadSignatureHeader();
                        int paramCount = sigBlob.ReadCompressedInteger();
                        if (paramCount >= 2)
                        {
                            var blob = reader.GetBlobReader(attr.Value);
                            blob.ReadUInt16(); // prolog
                            // Read past the serialized string
                            int strLen = blob.ReadCompressedInteger();
                            blob.Offset += strLen;
                            if (blob.RemainingBytes >= 4)
                                return blob.ReadUInt32();
                        }
                    }
                }
            }
        }
        return null;
    }

    [Fact]
    public void ContractVersion_ApiContract_EmitsEncodedValue()
    {
        // [contractversion(3)] should emit ContractVersionAttribute with value (3 << 16) = 196608
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(3)]
                apicontract TestContract { }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "TestContract");
            var version = ReadAttributeUInt32(reader, handle,
                "Windows.Foundation.Metadata.ContractVersionAttribute");
            Assert.NotNull(version);
            Assert.Equal(196608u, version.Value); // (3 << 16)
        }
    }

    [Fact]
    public void ContractVersion_One_EmitsEncodedValue()
    {
        // [contractversion(1)] → 65536
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(1)]
                apicontract TestContract { }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "TestContract");
            var version = ReadAttributeUInt32(reader, handle,
                "Windows.Foundation.Metadata.ContractVersionAttribute");
            Assert.NotNull(version);
            Assert.Equal(65536u, version.Value); // (1 << 16)
        }
    }

    [Fact]
    public void Version_RuntimeClass_EmitsEncodedValue()
    {
        // [version(1)] → VersionAttribute value = 65536
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [version(1), default_interface]
                runtimeclass Widget { Widget(); }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Widget");
            var version = ReadAttributeUInt32(reader, handle,
                "Windows.Foundation.Metadata.VersionAttribute");
            Assert.NotNull(version);
            Assert.Equal(65536u, version.Value); // (1 << 16)
        }
    }

    [Fact]
    public void Contract_RuntimeClass_EmitsEncodedVersion()
    {
        // [contract(C, 2)] → ContractVersionAttribute version = (2 << 16) = 131072
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(5)]
                apicontract TestContract { }

                [contract(TestContract, 2), default_interface]
                runtimeclass Widget { Widget(); }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Widget");
            var version = ReadContractVersionFromNamedAttr(reader, handle);
            Assert.NotNull(version);
            Assert.Equal(131072u, version.Value); // (2 << 16)
        }
    }

    [Fact]
    public void DefaultVersion_EmitsCorrectDefault()
    {
        // No explicit [version], default should be 0x00010000 = 65536
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [default_interface]
                runtimeclass Widget { Widget(); }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Widget");
            var version = ReadAttributeUInt32(reader, handle,
                "Windows.Foundation.Metadata.VersionAttribute");
            Assert.NotNull(version);
            Assert.Equal(0x00010000u, version.Value);
        }
    }

    [Fact]
    public void ContractVersion_FloatOneDotZero_EmitsCorrectValue()
    {
        // [contractversion(1.0)] should emit ContractVersionAttribute with value 65536
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(1.0)]
                apicontract TestContract { }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "TestContract");
            var version = ReadAttributeUInt32(reader, handle,
                "Windows.Foundation.Metadata.ContractVersionAttribute");
            Assert.NotNull(version);
            Assert.Equal(65536u, version.Value); // (1 << 16) | 0
        }
    }
}
