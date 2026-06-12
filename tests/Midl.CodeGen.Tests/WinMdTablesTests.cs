// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

public class WinMdTablesTests
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

    [Fact]
    public void PropertyHasMethodSemantics()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IWidget
                {
                    String Name { get; set; };
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IWidget");
            var td = reader.GetTypeDefinition(handle);

            // Verify PropertyMap: type has properties
            var properties = td.GetProperties().ToList();
            Assert.Single(properties);

            var prop = reader.GetPropertyDefinition(properties[0]);
            Assert.Equal("Name", reader.GetString(prop.Name));

            // Verify MethodSemantics: getter and setter linked
            var accessors = prop.GetAccessors();
            Assert.False(accessors.Getter.IsNil, "Property should have a getter");
            Assert.False(accessors.Setter.IsNil, "Property should have a setter");

            var getter = reader.GetMethodDefinition(accessors.Getter);
            Assert.Equal("get_Name", reader.GetString(getter.Name));

            var setter = reader.GetMethodDefinition(accessors.Setter);
            Assert.Equal("put_Name", reader.GetString(setter.Name));
        }
    }

    [Fact]
    public void EventHasMethodSemantics()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                delegate void Handler();
                interface IWidget
                {
                    event Handler Changed;
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IWidget");
            var td = reader.GetTypeDefinition(handle);

            // Verify EventMap: type has events
            var events = td.GetEvents().ToList();
            Assert.Single(events);

            var evt = reader.GetEventDefinition(events[0]);
            Assert.Equal("Changed", reader.GetString(evt.Name));

            // Verify MethodSemantics: adder and remover linked
            var accessors = evt.GetAccessors();
            Assert.False(accessors.Adder.IsNil, "Event should have an adder");
            Assert.False(accessors.Remover.IsNil, "Event should have a remover");

            var adder = reader.GetMethodDefinition(accessors.Adder);
            Assert.Equal("add_Changed", reader.GetString(adder.Name));

            var remover = reader.GetMethodDefinition(accessors.Remover);
            Assert.Equal("remove_Changed", reader.GetString(remover.Name));
        }
    }

    [Fact]
    public void MethodImplOnRuntimeClass()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                    String Name { get; };
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(reader, "Widget");

            // Collect all MethodImpl rows for the runtimeclass
            var methodImpls = new List<(string body, string decl)>();
            foreach (var implHandle in reader.GetTypeDefinition(classHandle).GetMethodImplementations())
            {
                var impl = reader.GetMethodImplementation(implHandle);

                string bodyName = GetMethodName(reader, impl.MethodBody);
                string declName = GetMethodName(reader, impl.MethodDeclaration);
                methodImpls.Add((bodyName, declName));
            }

            // Should have MethodImpl for DoWork, get_Name
            Assert.Contains(methodImpls, m => m.body == "DoWork" && m.decl == "DoWork");
            Assert.Contains(methodImpls, m => m.body == "get_Name" && m.decl == "get_Name");
        }
    }

    [Fact]
    public void InterfaceImplForRequires()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IBase
                {
                    void BaseMethod();
                }
                interface IDerived requires IBase
                {
                    void DerivedMethod();
                }
            }
            """);

        var reader = CreateReader(bytes, out var pe);
        using (pe)
        {
            var handle = FindTypeDef(reader, "IDerived");
            var td = reader.GetTypeDefinition(handle);

            // Verify InterfaceImpl exists for the required interface
            var ifaceImpls = td.GetInterfaceImplementations()
                .Select(h => reader.GetInterfaceImplementation(h))
                .ToList();

            Assert.Single(ifaceImpls);

            // The interface implementation should point to IBase
            var ifaceHandle = ifaceImpls[0].Interface;
            Assert.False(ifaceHandle.IsNil);

            string ifaceName;
            if (ifaceHandle.Kind == HandleKind.TypeDefinition)
            {
                var ifaceTd = reader.GetTypeDefinition((TypeDefinitionHandle)ifaceHandle);
                ifaceName = reader.GetString(ifaceTd.Name);
            }
            else
            {
                var ifaceRef = reader.GetTypeReference((TypeReferenceHandle)ifaceHandle);
                ifaceName = reader.GetString(ifaceRef.Name);
            }

            Assert.Equal("IBase", ifaceName);
        }
    }

    private static string GetMethodName(MetadataReader reader, EntityHandle handle)
    {
        if (handle.Kind == HandleKind.MethodDefinition)
        {
            var method = reader.GetMethodDefinition((MethodDefinitionHandle)handle);
            return reader.GetString(method.Name);
        }
        if (handle.Kind == HandleKind.MemberReference)
        {
            var memberRef = reader.GetMemberReference((MemberReferenceHandle)handle);
            return reader.GetString(memberRef.Name);
        }
        return "";
    }
}
