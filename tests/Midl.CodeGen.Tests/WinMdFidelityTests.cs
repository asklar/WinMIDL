using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

public class WinMdFidelityTests
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

    private static bool HasCustomAttribute(MetadataReader reader, EntityHandle parent, string attributeTypeName)
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
                        return true;
                }
            }
        }
        return false;
    }

    [Fact]
    public void WinMdVersionString()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                struct S { Int32 X; }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            Assert.Equal(MetadataConstants.VersionString, reader.MetadataVersion);
        }
    }

    [Fact]
    public void EnumExtendsSystemEnum()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                enum Status { Active, Inactive }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Status");
            var td = reader.GetTypeDefinition(handle);

            Assert.Equal(HandleKind.TypeReference, td.BaseType.Kind);
            var baseRef = reader.GetTypeReference((TypeReferenceHandle)td.BaseType);
            Assert.Equal("Enum", reader.GetString(baseRef.Name));
            Assert.Equal("System", reader.GetString(baseRef.Namespace));
        }
    }

    [Fact]
    public void StructExtendsSystemValueType()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                struct Point { Single X; Single Y; }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Point");
            var td = reader.GetTypeDefinition(handle);

            Assert.Equal(HandleKind.TypeReference, td.BaseType.Kind);
            var baseRef = reader.GetTypeReference((TypeReferenceHandle)td.BaseType);
            Assert.Equal("ValueType", reader.GetString(baseRef.Name));
            Assert.Equal("System", reader.GetString(baseRef.Namespace));
        }
    }

    [Fact]
    public void DelegateExtendsMulticastDelegate()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                delegate void Callback();
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Callback");
            var td = reader.GetTypeDefinition(handle);

            Assert.Equal(HandleKind.TypeReference, td.BaseType.Kind);
            var baseRef = reader.GetTypeReference((TypeReferenceHandle)td.BaseType);
            Assert.Equal("MulticastDelegate", reader.GetString(baseRef.Name));
            Assert.Equal("System", reader.GetString(baseRef.Namespace));
        }
    }

    [Fact]
    public void RuntimeClassExtendsSystemObject()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [default_interface] runtimeclass Widget { Widget(); }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Widget");
            var td = reader.GetTypeDefinition(handle);

            Assert.Equal(HandleKind.TypeReference, td.BaseType.Kind);
            var baseRef = reader.GetTypeReference((TypeReferenceHandle)td.BaseType);
            Assert.Equal("Object", reader.GetString(baseRef.Name));
            Assert.Equal("System", reader.GetString(baseRef.Namespace));
        }
    }

    [Fact]
    public void InterfaceHasAbstractFlag()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IWidget { void Activate(); }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IWidget");
            var td = reader.GetTypeDefinition(handle);

            Assert.True(td.Attributes.HasFlag(TypeAttributes.Abstract));
            Assert.True(td.Attributes.HasFlag(TypeAttributes.Interface));
            Assert.True(((int)td.Attributes & MetadataConstants.TdWindowsRuntime) != 0);
        }
    }

    [Fact]
    public void EnumFieldsHaveCorrectValues()
    {
        // The emitter assigns sequential zero-based values to enum members.
        var bytes = CompileAndEmit("""
            namespace Test
            {
                enum E { A, B, C }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "E");
            var td = reader.GetTypeDefinition(handle);
            var fields = td.GetFields().Select(f => reader.GetFieldDefinition(f)).ToList();

            // First field is value__, then A, B, C
            Assert.True(fields.Count >= 4);
            Assert.Equal("value__", reader.GetString(fields[0].Name));

            var memberFields = fields.Skip(1).ToList();
            Assert.Equal("A", reader.GetString(memberFields[0].Name));
            Assert.Equal("B", reader.GetString(memberFields[1].Name));
            Assert.Equal("C", reader.GetString(memberFields[2].Name));

            // Verify sequential constant values: 0, 1, 2
            var values = memberFields.Select(f =>
            {
                var constant = reader.GetConstant(f.GetDefaultValue());
                var blobReader = reader.GetBlobReader(constant.Value);
                return blobReader.ReadInt32();
            }).ToList();

            Assert.Equal(new[] { 0, 1, 2 }, values);
        }
    }

    [Fact]
    public void FlagsEnumHasFlagsAttribute()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [flags]
                enum Perms { None, Read, Write }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "Perms");
            Assert.True(HasCustomAttribute(reader, handle, "System.FlagsAttribute"));
        }
    }

    [Fact]
    public void NonFlagsEnumNoFlagsAttribute()
    {
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
            Assert.False(HasCustomAttribute(reader, handle, "System.FlagsAttribute"));
        }
    }

    [Fact]
    public void InterfaceMethodIsVirtual()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IFoo { void DoWork(); }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IFoo");
            var td = reader.GetTypeDefinition(handle);

            var methods = td.GetMethods().Select(m => reader.GetMethodDefinition(m)).ToList();
            var doWork = methods.First(m => reader.GetString(m.Name) == "DoWork");

            Assert.True(doWork.Attributes.HasFlag(MethodAttributes.Virtual));
            Assert.True(doWork.Attributes.HasFlag(MethodAttributes.Abstract));
            Assert.True(doWork.Attributes.HasFlag(MethodAttributes.HideBySig));
            Assert.True(doWork.Attributes.HasFlag(MethodAttributes.NewSlot));
            Assert.True(doWork.Attributes.HasFlag(MethodAttributes.Public));
        }
    }
}
