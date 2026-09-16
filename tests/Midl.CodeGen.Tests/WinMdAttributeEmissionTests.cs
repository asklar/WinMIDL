// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
                else if (memberRef.Parent.Kind == HandleKind.TypeDefinition)
                {
                    var typeDef = reader.GetTypeDefinition((TypeDefinitionHandle)memberRef.Parent);
                    var fullName = $"{reader.GetString(typeDef.Namespace)}.{reader.GetString(typeDef.Name)}";
                    if (fullName == attributeTypeName)
                        return true;
                }
            }
        }
        return false;
    }

    private static string? ReadNamedContractVersionName(MetadataReader reader, EntityHandle parent)
    {
        foreach (var attrHandle in reader.GetCustomAttributes(parent))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind != HandleKind.MemberReference)
                continue;

            var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
            if (memberRef.Parent.Kind != HandleKind.TypeReference)
                continue;

            var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
            if (reader.GetString(typeRef.Name) != "ContractVersionAttribute")
                continue;

            var blob = reader.GetBlobReader(attr.Value);
            blob.ReadUInt16();
            return blob.ReadSerializedString();
        }

        return null;
    }

    private static uint? ReadNamedContractVersionValue(MetadataReader reader, EntityHandle parent)
    {
        foreach (var attrHandle in reader.GetCustomAttributes(parent))
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            if (attr.Constructor.Kind != HandleKind.MemberReference)
                continue;

            var memberRef = reader.GetMemberReference((MemberReferenceHandle)attr.Constructor);
            if (memberRef.Parent.Kind != HandleKind.TypeReference)
                continue;

            var typeRef = reader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
            if (reader.GetString(typeRef.Name) != "ContractVersionAttribute")
                continue;

            var blob = reader.GetBlobReader(attr.Value);
            blob.ReadUInt16();
            blob.ReadSerializedString();
            return blob.ReadUInt32();
        }

        return null;
    }

    private static int CountCustomAttributes(MetadataReader reader, EntityHandle parent, string attributeTypeName)
    {
        int count = 0;
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
                        count++;
                }
            }
        }
        return count;
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
                [default_interface]
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
                [threading(both), default_interface]
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
                [marshaling_behavior(agile), default_interface]
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

                [contract(TestContract, 2), default_interface]
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
            var methodHandle = FindMethod(reader, ifaceHandle, "DoWorkFast");

            Assert.True(HasCustomAttribute(reader, methodHandle,
                "Windows.Foundation.Metadata.OverloadAttribute"));

            var overloadAttribute = reader.GetMethodDefinition(methodHandle).GetCustomAttributes()
                .Select(reader.GetCustomAttribute)
                .Single(attribute =>
                {
                    var memberRef = reader.GetMemberReference(
                        (MemberReferenceHandle)attribute.Constructor);
                    var typeRef = reader.GetTypeReference(
                        (TypeReferenceHandle)memberRef.Parent);
                    return reader.GetString(typeRef.Name) == "OverloadAttribute";
                });
            var blob = reader.GetBlobReader(overloadAttribute.Value);
            blob.ReadUInt16();
            Assert.Equal("DoWork", blob.ReadSerializedString());
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

    [Fact]
    public void MultipleStaticAttributesEmitted()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [exclusiveto(Calculator)]
                interface ICalcStatics
                {
                    void Add();
                }

                [exclusiveto(Calculator)]
                interface ICalcStatics2
                {
                    void Subtract();
                }

                [static(ICalcStatics, 1)]
                [static(ICalcStatics2, 2)]
                [default_interface]
                runtimeclass Calculator
                {
                    Calculator();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Calculator");
            Assert.Equal(2, CountCustomAttributes(reader, classHandle,
                "Windows.Foundation.Metadata.StaticAttribute"));
        }
    }

    [Fact]
    public void ContractVersionAttributeOnStruct()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(1)]
                apicontract TestContract {}

                [contract(TestContract, 1)]
                struct Payload
                {
                    Int32 Value;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var structHandle = FindTypeDef(reader, "Payload");
            Assert.True(HasCustomAttribute(reader, structHandle,
                "Windows.Foundation.Metadata.ContractVersionAttribute"));
            Assert.False(HasCustomAttribute(reader, structHandle,
                "Windows.Foundation.Metadata.VersionAttribute"));
        }
    }

    [Fact]
    public void ContractVersionAttributeOnEnumMember()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(2)]
                apicontract TestContract {}

                [contract(TestContract, 1)]
                enum Choice
                {
                    First,
                    [contract(TestContract, 2)]
                    Second,
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var enumHandle = FindTypeDef(reader, "Choice");
            var second = reader.GetTypeDefinition(enumHandle).GetFields()
                .Select(handle => (Handle: handle, Definition: reader.GetFieldDefinition(handle)))
                .Single(field => reader.GetString(field.Definition.Name) == "Second");
            Assert.Equal("Test.TestContract",
                ReadNamedContractVersionName(reader, second.Handle));

            var attribute = second.Definition.GetCustomAttributes()
                .Select(reader.GetCustomAttribute)
                .Single();
            var constructor = reader.GetMemberReference(
                (MemberReferenceHandle)attribute.Constructor);
            Assert.Equal("2002010E09",
                Convert.ToHexString(reader.GetBlobBytes(constructor.Signature)));
        }
    }

    [Fact]
    public void ContractVersionAttributeOnSynthesizedInterfaces()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(1)]
                apicontract TestContract {}

                [contract(TestContract, 1)]
                unsealed runtimeclass Widget
                {
                    Widget(Int32 initialValue);
                    void InstanceMethod();
                    static void StaticMethod();
                    protected void ProtectedMethod();
                    overridable void OverridableMethod();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            foreach (var interfaceName in new[]
            {
                "IWidget",
                "IWidgetFactory",
                "IWidgetStatics",
                "IWidgetProtected",
                "IWidgetOverrides",
            })
            {
                var interfaceHandle = FindTypeDef(reader, interfaceName);
                Assert.True(HasCustomAttribute(reader, interfaceHandle,
                    "Windows.Foundation.Metadata.ContractVersionAttribute"),
                    $"{interfaceName} must inherit the runtimeclass contract.");
                Assert.False(HasCustomAttribute(reader, interfaceHandle,
                    "Windows.Foundation.Metadata.VersionAttribute"),
                    $"{interfaceName} must not use VersionAttribute when it has a contract.");
            }
        }
    }

    [Fact]
    public void InternalContractMetadataMatchesMidl()
    {
        var bytes = CompileAndEmit("""
            namespace Test.Internal
            {
                [internal, contractversion(1)]
                apicontract TestContract {}

                [contract(TestContract, 1)]
                enum Choice
                {
                    First,
                }

                [contract(TestContract, 1)]
                struct Payload
                {
                    Int32 Value;
                }

                [contract(TestContract, 1)]
                delegate void Callback(String value);

                [contract(TestContract, 1)]
                runtimeclass Widget
                {
                    void InstanceMethod();
                    static void StaticMethod();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            foreach (var typeName in new[]
            {
                "TestContract",
                "Choice",
                "Payload",
                "Callback",
                "Widget",
                "IWidget",
                "IWidgetStatics",
            })
            {
                var typeHandle = FindTypeDef(reader, typeName);
                Assert.True(HasCustomAttribute(reader, typeHandle,
                    "Windows.Foundation.Metadata.InternalAttribute"),
                    $"{typeName} must inherit the internal contract visibility.");
            }

            foreach (var typeName in new[]
            {
                "Choice",
                "Payload",
                "Callback",
                "Widget",
                "IWidget",
                "IWidgetStatics",
            })
            {
                var typeHandle = FindTypeDef(reader, typeName);
                Assert.Equal("Test.Internal.TestContract",
                    ReadNamedContractVersionName(reader, typeHandle));
            }

            Assert.Single(reader.TypeReferences.Select(reader.GetTypeReference),
                typeRef => reader.GetString(typeRef.Name) == "ContractVersionAttribute");
        }
    }

    [Fact]
    public void ContractVersionAttributeOnRuntimeClassMembers()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(1)]
                apicontract TestContract {}

                [contract(TestContract, 1)]
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                    Int32 Value { get; };
                    static void StaticMethod();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Widget");
            var classDefinition = reader.GetTypeDefinition(classHandle);
            foreach (var methodHandle in classDefinition.GetMethods())
            {
                Assert.True(HasCustomAttribute(reader, methodHandle,
                    "Windows.Foundation.Metadata.ContractVersionAttribute"));
                Assert.False(HasCustomAttribute(reader, methodHandle,
                    "Windows.Foundation.Metadata.VersionAttribute"));
            }

            var propertyHandle = Assert.Single(classDefinition.GetProperties());
            Assert.True(HasCustomAttribute(reader, propertyHandle,
                "Windows.Foundation.Metadata.ContractVersionAttribute"));
        }
    }

    [Fact]
    public void VersionedRuntimeClassInterfacesMatchMidl()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(3)]
                apicontract TestContract {}

                [contract(TestContract, 1)]
                runtimeclass Service
                {
                    void V1();

                    [contract(TestContract, 2)]
                    {
                        void V2();
                    }

                    [contract(TestContract, 3)]
                    {
                        void V3();
                        static void S3();
                    }
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            Assert.Equal(1u << 16,
                ReadNamedContractVersionValue(reader, FindTypeDef(reader, "IService")));
            Assert.Equal(2u << 16,
                ReadNamedContractVersionValue(reader, FindTypeDef(reader, "IService2")));
            Assert.Equal(3u << 16,
                ReadNamedContractVersionValue(reader, FindTypeDef(reader, "IService3")));
            Assert.Equal(3u << 16,
                ReadNamedContractVersionValue(reader, FindTypeDef(reader, "IServiceStatics")));

            var classDefinition = reader.GetTypeDefinition(FindTypeDef(reader, "Service"));
            var implementationHandles = classDefinition.GetInterfaceImplementations().ToList();
            Assert.Equal(3, implementationHandles.Count);
            Assert.Null(ReadNamedContractVersionValue(reader, implementationHandles[0]));
            Assert.Equal(2u << 16,
                ReadNamedContractVersionValue(reader, implementationHandles[1]));
            Assert.Equal(3u << 16,
                ReadNamedContractVersionValue(reader, implementationHandles[2]));
        }
    }

    [Fact]
    public void VersionedModifierInterfaceImplsMatchMidl()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [contractversion(2)]
                apicontract TestContract {}

                [contract(TestContract, 1)]
                unsealed runtimeclass Widget
                {
                    void InstanceV1();
                    overridable void OverrideV1();

                    [contract(TestContract, 2)]
                    {
                        protected void ProtectedV2();
                        overridable void OverrideV2();
                    }
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            Assert.Equal(2u << 16,
                ReadNamedContractVersionValue(reader, FindTypeDef(reader, "IWidgetProtected")));
            Assert.Equal(1u << 16,
                ReadNamedContractVersionValue(reader, FindTypeDef(reader, "IWidgetOverrides")));
            Assert.Equal(2u << 16,
                ReadNamedContractVersionValue(reader, FindTypeDef(reader, "IWidgetOverrides2")));

            var classDefinition = reader.GetTypeDefinition(FindTypeDef(reader, "Widget"));
            var modifierImplementations = classDefinition.GetInterfaceImplementations()
                .Select(handle => (Handle: handle, Definition: reader.GetInterfaceImplementation(handle)))
                .Where(implementation =>
                    HasCustomAttribute(reader, implementation.Handle,
                        "Windows.Foundation.Metadata.ProtectedAttribute")
                    || HasCustomAttribute(reader, implementation.Handle,
                        "Windows.Foundation.Metadata.OverridableAttribute"))
                .ToList();

            Assert.Equal(3, modifierImplementations.Count);
            Assert.Equal(2, modifierImplementations.Count(implementation =>
                ReadNamedContractVersionValue(reader, implementation.Handle) == 2u << 16));
        }
    }

    [Fact]
    public void UserDefinedCustomAttributeEmitted()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                [attributeusage(target_runtimeclass)]
                [attributename("myattr")]
                attribute MyAttrAttribute
                {
                    String Value;
                }

                [myattr("hello")]
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Widget");
            Assert.True(HasCustomAttribute(reader, classHandle,
                "Test.MyAttrAttribute"),
                "User-defined custom attribute should be present on class");
        }
    }
}
