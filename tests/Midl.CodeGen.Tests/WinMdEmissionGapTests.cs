// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.CodeGen.WinMd;
using Midl.CodeGen.Decompiler;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for WinMD emission gaps discovered by comparison with MIDL.exe:
/// - AttributeUsageAttribute on custom attribute types
/// - ApiContract emitted as struct with [ApiContract] attribute
/// - Contract version encoding for Activatable/Version/ContractVersion
/// - Version attribute suppression when ContractVersion is present
/// - CLR AttributeTargets flag encoding
/// </summary>
public class WinMdEmissionGapTests
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
        return peReader.GetMetadataReader(MetadataReaderOptions.None);
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

    private static bool HasCustomAttribute(MetadataReader reader, EntityHandle parent, string attrTypeName)
    {
        foreach (var attrHandle in reader.GetCustomAttributes(parent))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind != HandleKind.MemberReference) continue;
            var mr = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
            if (mr.Parent.Kind != HandleKind.TypeReference) continue;
            var tr = reader.GetTypeReference((TypeReferenceHandle)mr.Parent);
            if (reader.GetString(tr.Name) == attrTypeName) return true;
        }
        return false;
    }

    private static uint? ReadAttributeUInt32(MetadataReader reader, EntityHandle parent, string attrTypeName)
    {
        foreach (var attrHandle in reader.GetCustomAttributes(parent))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind != HandleKind.MemberReference) continue;
            var mr = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
            if (mr.Parent.Kind != HandleKind.TypeReference) continue;
            var tr = reader.GetTypeReference((TypeReferenceHandle)mr.Parent);
            if (reader.GetString(tr.Name) != attrTypeName) continue;

            var blob = reader.GetBlobReader(attr.Value);
            if (blob.Length >= 6)
            {
                blob.ReadUInt16(); // prolog
                return blob.ReadUInt32();
            }
        }
        return null;
    }

    private static int? ReadAttributeInt32(MetadataReader reader, EntityHandle parent, string attrTypeName)
    {
        foreach (var attrHandle in reader.GetCustomAttributes(parent))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind != HandleKind.MemberReference) continue;
            var mr = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
            if (mr.Parent.Kind != HandleKind.TypeReference) continue;
            var tr = reader.GetTypeReference((TypeReferenceHandle)mr.Parent);
            if (reader.GetString(tr.Name) != attrTypeName) continue;

            var blob = reader.GetBlobReader(attr.Value);
            if (blob.Length >= 6)
            {
                blob.ReadUInt16(); // prolog
                return blob.ReadInt32();
            }
        }
        return null;
    }

    #region AttributeUsageAttribute Emission

    [Fact]
    public void AttributeType_EmitsAttributeUsageAttribute()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [attributeusage(target_runtimeclass, target_interface)]
                attribute MyMarkerAttribute {};
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "MyMarkerAttribute");
            Assert.True(HasCustomAttribute(reader, handle, "AttributeUsageAttribute"));
        }
    }

    [Fact]
    public void AttributeType_AttributeUsage_UsesClrFlags()
    {
        // target_runtimeclass → CLR Class=0x04, target_interface → CLR Interface=0x400
        // Expected: 0x04 | 0x400 = 0x404 = 1028
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [attributeusage(target_runtimeclass, target_interface)]
                attribute MyMarkerAttribute {};
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "MyMarkerAttribute");
            var flags = ReadAttributeInt32(reader, handle, "AttributeUsageAttribute");
            Assert.NotNull(flags);
            Assert.Equal(0x0404, flags.Value); // CLR Class | Interface
        }
    }

    [Fact]
    public void AttributeType_MethodOnly_UsesClrMethodFlag()
    {
        // target_method → CLR Method=0x40
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [attributeusage(target_method)]
                attribute MethodOnlyAttribute {};
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "MethodOnlyAttribute");
            var flags = ReadAttributeInt32(reader, handle, "AttributeUsageAttribute");
            Assert.NotNull(flags);
            Assert.Equal(0x0040, flags.Value); // CLR Method
        }
    }

    [Fact]
    public void AttributeType_AllTargets_UsesClrFlags()
    {
        // target_delegate=0x1000, target_enum=0x10, target_event=0x800,
        // target_field=0x200, target_interface=0x400, target_method=0x40,
        // target_parameter=0x80 (CLR 0x4000), target_property=0x100,
        // target_runtimeclass=0x04, target_struct=0x08
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [attributeusage(target_runtimeclass, target_interface, target_delegate,
                    target_enum, target_struct, target_method, target_property,
                    target_event, target_field)]
                attribute AllTargetsAttribute {};
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "AllTargetsAttribute");
            var flags = ReadAttributeInt32(reader, handle, "AttributeUsageAttribute");
            Assert.NotNull(flags);
            // CLR: Class(4) | Struct(8) | Enum(16) | Method(64) | Property(256)
            //    | Field(512) | Interface(1024) | Event(2048) | Delegate(4096)
            int expected = 0x04 | 0x08 | 0x10 | 0x40 | 0x100 | 0x200 | 0x400 | 0x800 | 0x1000;
            Assert.Equal(expected, flags.Value);
        }
    }

    #endregion

    #region ApiContract Emission

    [Fact]
    public void ApiContract_EmittedAsStruct()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(1)]
                apicontract TestContract {};
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "TestContract");
            var td = reader.GetTypeDefinition(handle);
            // Base type should be System.ValueType (struct), not System.Object
            var baseRef = reader.GetTypeReference((TypeReferenceHandle)td.BaseType);
            Assert.Equal("ValueType", reader.GetString(baseRef.Name));
            Assert.Equal("System", reader.GetString(baseRef.Namespace));
        }
    }

    [Fact]
    public void ApiContract_HasApiContractAttribute()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(1)]
                apicontract TestContract {};
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "TestContract");
            Assert.True(HasCustomAttribute(reader, handle, "ApiContractAttribute"));
        }
    }

    [Fact]
    public void ApiContract_NoVersionAttribute()
    {
        // MIDL.exe does not emit [Version] on apicontract — only [ContractVersion]
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(1)]
                apicontract TestContract {};
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "TestContract");
            Assert.False(HasCustomAttribute(reader, handle, "VersionAttribute"));
            Assert.True(HasCustomAttribute(reader, handle, "ContractVersionAttribute"));
        }
    }

    #endregion

    #region Contract Version Encoding

    [Fact]
    public void RuntimeClass_WithContract_ActivatableUsesContractVersion()
    {
        // [contract(Foo, 1)] → ContractVersion=65536 (1<<16). Activatable should also use 65536.
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(1)]
                apicontract TestContract {};

                [contract(Test.TestContract, 1)]
                runtimeclass Widget
                {
                    Widget();
                    Int32 Value;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Widget");
            var activatableVersion = ReadAttributeUInt32(reader, handle, "ActivatableAttribute");
            Assert.NotNull(activatableVersion);
            Assert.Equal(65536u, activatableVersion.Value); // 1 << 16
        }
    }

    [Fact]
    public void RuntimeClass_WithContractOnly_NoVersionAttribute()
    {
        // When [contract] is present but NOT [version], MIDL does NOT emit [Version] — only [ContractVersion]
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(1)]
                apicontract TestContract {};

                [contract(Test.TestContract, 1)]
                runtimeclass Widget
                {
                    Widget();
                    Int32 Value;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Widget");
            Assert.False(HasCustomAttribute(reader, handle, "VersionAttribute"),
                "RuntimeClass with [contract] only (no [version]) should NOT have [Version]");
            Assert.True(HasCustomAttribute(reader, handle, "ContractVersionAttribute"));
        }
    }

    [Fact]
    public void RuntimeClass_WithVersionAndContract_EmitsBoth()
    {
        // When BOTH [version] and [contract] are specified, MIDL emits both [Version] and [ContractVersion]
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(1)]
                apicontract TestContract {};

                [version(2)]
                [contract(Test.TestContract, 1)]
                runtimeclass Widget
                {
                    Widget();
                    Int32 Value;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Widget");
            Assert.True(HasCustomAttribute(reader, handle, "VersionAttribute"),
                "RuntimeClass with both [version] and [contract] should have [Version]");
            Assert.True(HasCustomAttribute(reader, handle, "ContractVersionAttribute"));

            var versionVal = ReadAttributeUInt32(reader, handle, "VersionAttribute");
            Assert.Equal(131072u, versionVal!.Value); // 2 << 16

            var activatableVal = ReadAttributeUInt32(reader, handle, "ActivatableAttribute");
            Assert.Equal(65536u, activatableVal!.Value); // contract version 1 << 16
        }
    }

    [Fact]
    public void RuntimeClass_WithoutContract_HasVersionAttribute()
    {
        // Without [contract], runtime class should still have [Version(1)]
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    Int32 Value;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Widget");
            Assert.True(HasCustomAttribute(reader, handle, "VersionAttribute"));
            var version = ReadAttributeUInt32(reader, handle, "VersionAttribute");
            Assert.Equal(1u, version!.Value);
        }
    }

    #endregion

    #region Attribute Type with Contract

    [Fact]
    public void AttributeType_WithContract_HasContractVersionAttribute()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(1)]
                apicontract TestContract {};

                [attributeusage(target_runtimeclass)]
                [contract(Test.TestContract, 1)]
                attribute MyAttribute {};
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "MyAttribute");
            Assert.True(HasCustomAttribute(reader, handle, "ContractVersionAttribute"),
                "Attribute with [contract] should have [ContractVersion]");
            Assert.False(HasCustomAttribute(reader, handle, "VersionAttribute"),
                "Attribute with [contract] should NOT have [Version]");
        }
    }

    #endregion

    #region Decompiler Round-Trip

    [Fact]
    public void Decompile_ApiContract_ShowsApicontractKeyword()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(3)]
                apicontract MyContract {}
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("apicontract MyContract", idl);
        Assert.Contains("[contractversion(", idl);
        Assert.DoesNotContain("struct MyContract", idl);
    }

    [Fact]
    public void Decompile_AttributeType_ShowsAttributeUsage()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [attributeusage(target_runtimeclass, target_interface)]
                attribute MarkerAttribute {};
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("[attributeusage(", idl);
        Assert.Contains("target_runtimeclass", idl);
        Assert.Contains("target_interface", idl);
        Assert.Contains("attribute MarkerAttribute", idl);
    }

    [Fact]
    public void Decompile_MidlWinMd_CorrectlyDecodesClrFlags()
    {
        // Simulate what MIDL.exe would produce: CLR flags in the blob
        // CLR Class=0x04 + Interface=0x400 = 0x0404
        // Should decompile to target_runtimeclass, target_interface
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [attributeusage(target_runtimeclass, target_interface)]
                attribute TestAttribute {};
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("target_runtimeclass", idl);
        Assert.Contains("target_interface", idl);
        // Should NOT contain wrongly-decoded WinRT targets
        Assert.DoesNotContain("target_event", idl);
        Assert.DoesNotContain("target_struct", idl);
    }

    #endregion

    #region CLR AttributeTargets Encoding

    [Fact]
    public void ClrAttributeTargets_FromTargetStrings_EncodesCorrectly()
    {
        var targets = new[] { "target_runtimeclass", "target_interface" };
        int flags = MetadataConstants.ClrAttributeTargets.FromTargetStrings(targets);
        Assert.Equal(0x0404, flags); // CLR Class(4) | Interface(1024)
    }

    [Fact]
    public void ClrAttributeTargets_ToTargetStrings_DecodesCorrectly()
    {
        int flags = 0x0404; // CLR Class | Interface
        var targets = MetadataConstants.ClrAttributeTargets.ToTargetStrings(flags);
        Assert.Contains("target_runtimeclass", targets);
        Assert.Contains("target_interface", targets);
        Assert.Equal(2, targets.Count);
    }

    [Fact]
    public void ClrAttributeTargets_RoundTrip()
    {
        var original = new[] { "target_delegate", "target_enum", "target_event",
            "target_field", "target_interface", "target_method",
            "target_property", "target_runtimeclass", "target_struct" };
        int flags = MetadataConstants.ClrAttributeTargets.FromTargetStrings(original);
        var decoded = MetadataConstants.ClrAttributeTargets.ToTargetStrings(flags);
        Assert.Equal(original.OrderBy(x => x), decoded.OrderBy(x => x));
    }

    #endregion
}
