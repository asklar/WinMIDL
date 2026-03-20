using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

public class WinMdAttributeEmissionTests
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

    private static MethodDefinitionHandle FindMethod(MetadataReader reader, TypeDefinitionHandle typeDef, string methodName)
    {
        var td = reader.GetTypeDefinition(typeDef);
        foreach (var mh in td.GetMethods())
        {
            var md = reader.GetMethodDefinition(mh);
            if (reader.GetString(md.Name) == methodName)
                return mh;
        }
        throw new Exception($"Method '{methodName}' not found");
    }

    [Fact]
    public void ExclusiveToOnSynthesizedInterface()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass MyClass
                {
                    MyClass();
                    void DoWork();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var ifaceHandle = FindTypeDef(reader, "IMyClass");
            Assert.True(HasCustomAttribute(reader, ifaceHandle,
                "Windows.Foundation.Metadata.ExclusiveToAttribute"));
        }
    }

    [Fact]
    public void DefaultAttributeOnInterfaceImpl()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    void Activate();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Widget");
            var classTd = reader.GetTypeDefinition(classHandle);

            var ifaceImpls = classTd.GetInterfaceImplementations()
                .Select(h => reader.GetInterfaceImplementation(h))
                .ToList();
            Assert.True(ifaceImpls.Count >= 1);

            // At least one InterfaceImpl should have DefaultAttribute
            bool foundDefault = false;
            foreach (var ifImpl in classTd.GetInterfaceImplementations())
            {
                if (HasCustomAttribute(reader, ifImpl, "Windows.Foundation.Metadata.DefaultAttribute"))
                {
                    foundDefault = true;
                    break;
                }
            }
            Assert.True(foundDefault, "DefaultAttribute should be on an InterfaceImpl");
        }
    }

    [Fact]
    public void ActivatableAttributeOnClass()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Foo
                {
                    Foo();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Foo");
            Assert.True(HasCustomAttribute(reader, classHandle,
                "Windows.Foundation.Metadata.ActivatableAttribute"));
        }
    }

    [Fact]
    public void StaticAttributeOnClass()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Foo
                {
                    Foo();
                    static void Helper();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Foo");
            Assert.True(HasCustomAttribute(reader, classHandle,
                "Windows.Foundation.Metadata.StaticAttribute"),
                "StaticAttribute should be present on class with static members");
        }
    }

    [Fact]
    public void ThreadingAttributeEmitted()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [threading(both)]
                runtimeclass Widget
                {
                    Widget();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Widget");
            Assert.True(HasCustomAttribute(reader, classHandle,
                "Windows.Foundation.Metadata.ThreadingAttribute"));
        }
    }

    [Fact]
    public void MarshalingBehaviorEmitted()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [marshaling_behavior(agile)]
                runtimeclass Widget
                {
                    Widget();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Widget");
            Assert.True(HasCustomAttribute(reader, classHandle,
                "Windows.Foundation.Metadata.MarshalingBehaviorAttribute"));
        }
    }

    [Fact]
    public void ContractVersionEmitted()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(3)]
                apicontract TestContract { }

                [contract(TestContract, 2)]
                runtimeclass Widget
                {
                    Widget();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Widget");
            Assert.True(HasCustomAttribute(reader, classHandle,
                "Windows.Foundation.Metadata.ContractVersionAttribute"));
        }
    }

    [Fact]
    public void ExclusiveInterfaceIsNotPublic()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass MyClass
                {
                    MyClass();
                    void DoWork();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var ifaceHandle = FindTypeDef(reader, "IMyClass");
            var ifaceTd = reader.GetTypeDefinition(ifaceHandle);

            // Synthesized exclusive interface should NOT be public
            Assert.False(ifaceTd.Attributes.HasFlag(TypeAttributes.Public),
                "Synthesized exclusive interface should not be public");

            // Should still be an interface
            Assert.True(ifaceTd.Attributes.HasFlag(TypeAttributes.Interface));
            Assert.True(((int)ifaceTd.Attributes & MetadataConstants.TdWindowsRuntime) != 0);
        }
    }

    [Fact]
    public void OverloadAttributeOnMethod()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    [overload("DoWorkFast")]
                    void DoWork(Int32 x);
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            // The overload attribute should be on the synthesized interface method
            var ifaceHandle = FindTypeDef(reader, "IWidget");
            var methodHandle = FindMethod(reader, ifaceHandle, "DoWork");

            Assert.True(HasCustomAttribute(reader, methodHandle,
                "Windows.Foundation.Metadata.OverloadAttribute"));
        }
    }

    [Fact]
    public void DefaultOverloadAttributeOnMethod()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    [default_overload]
                    void DoWork(String s);
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var ifaceHandle = FindTypeDef(reader, "IWidget");
            var methodHandle = FindMethod(reader, ifaceHandle, "DoWork");

            Assert.True(HasCustomAttribute(reader, methodHandle,
                "Windows.Foundation.Metadata.DefaultOverloadAttribute"));
        }
    }
}
