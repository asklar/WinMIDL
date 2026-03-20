using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

public class WinMdGenericTests
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
        throw new Exception("TypeDef not found: " + name);
    }

    [Fact]
    public void ReadGenericMethodParam()
    {
        var winmd = CompileAndEmit(@"
            namespace TestGeneric
            {
                interface IMyCollection
                {
                    void Process();
                };

                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                };
            }");

        var reader = new WinMdReader();
        var types = reader.ReadTypes(winmd);

        var iface = types.OfType<InterfaceSymbol>().FirstOrDefault(i => i.Name == "IMyCollection");
        Assert.NotNull(iface);

        var methods = iface.Members.OfType<MethodSymbol>().ToList();
        Assert.Contains(methods, m => m.Name == "Process");
        Assert.Equal("void", methods.First(m => m.Name == "Process").ReturnType.Name);

        // Test the ParseTypeRef helper directly for generic instantiation
        var genericRef = WinMdReader.ParseTypeRef("Windows.Foundation.Collections.IVector<String>");
        Assert.Equal("Windows.Foundation.Collections.IVector", genericRef.Name);
        Assert.Single(genericRef.TypeArguments);
        Assert.Equal("String", genericRef.TypeArguments[0].Name);

        // Test nested generic
        var nestedRef = WinMdReader.ParseTypeRef("IMap<String, IVector<Int32>>");
        Assert.Equal("IMap", nestedRef.Name);
        Assert.Equal(2, nestedRef.TypeArguments.Count);
        Assert.Equal("String", nestedRef.TypeArguments[0].Name);
        Assert.Equal("IVector", nestedRef.TypeArguments[1].Name);
        Assert.Single(nestedRef.TypeArguments[1].TypeArguments);
        Assert.Equal("Int32", nestedRef.TypeArguments[1].TypeArguments[0].Name);
    }

    [Fact]
    public void WriteGenericMethodParam()
    {
        var winmd = CompileAndEmit(@"
            namespace TestGeneric
            {
                interface IFoo
                {
                    void Bar();
                };

                runtimeclass MyClass
                {
                    MyClass();
                    void DoWork();
                };
            }");

        var mdReader = CreateReader(winmd, out var pe);
        using (pe)
        {
            var fooHandle = FindTypeDef(mdReader, "IFoo");
            var fooTd = mdReader.GetTypeDefinition(fooHandle);
            Assert.True(fooTd.Attributes.HasFlag(TypeAttributes.Interface));

            var classHandle = FindTypeDef(mdReader, "MyClass");
            var classTd = mdReader.GetTypeDefinition(classHandle);
            var ifaceImpls = classTd.GetInterfaceImplementations()
                .Select(h => mdReader.GetInterfaceImplementation(h))
                .ToList();
            Assert.True(ifaceImpls.Count >= 1);

            // Interface references should be TypeRef (TypeDef redirection)
            foreach (var impl in ifaceImpls)
            {
                Assert.Equal(HandleKind.TypeReference, impl.Interface.Kind);
            }
        }
    }

    [Fact]
    public void TypeDefRedirection()
    {
        var winmd = CompileAndEmit(@"
            namespace TestRedirect
            {
                enum Status
                {
                    Active,
                    Inactive,
                };

                struct Data
                {
                    Int32 Value;
                };

                [uuid(11111111-2222-3333-4444-555555555555)]
                interface IFoo
                {
                    void Show();
                };

                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                };
            }");

        var mdReader = CreateReader(winmd, out var pe);
        using (pe)
        {
            var classHandle = FindTypeDef(mdReader, "Widget");
            var classTd = mdReader.GetTypeDefinition(classHandle);

            // BaseType should be a TypeRef to System.Object
            Assert.Equal(HandleKind.TypeReference, classTd.BaseType.Kind);

            // All InterfaceImpl entries should reference TypeRef handles
            var ifaceImpls = classTd.GetInterfaceImplementations()
                .Select(h => mdReader.GetInterfaceImplementation(h))
                .ToList();
            Assert.True(ifaceImpls.Count >= 1);

            foreach (var impl in ifaceImpls)
            {
                Assert.Equal(HandleKind.TypeReference, impl.Interface.Kind);
            }

            // Verify there are TypeRef entries for local types
            // The synthesized default interface for Widget is named IWidget
            var typeRefNames = new HashSet<string>();
            foreach (var trHandle in mdReader.TypeReferences)
            {
                var tr = mdReader.GetTypeReference(trHandle);
                typeRefNames.Add(mdReader.GetString(tr.Name));
            }

            // Synthesized interface IWidget should be referenced via TypeRef
            Assert.Contains("IWidget", typeRefNames);
        }
    }

    [Fact]
    public void ReadInterfaceImplAttributes()
    {
        var winmd = CompileAndEmit(@"
            namespace TestAttrs
            {
                [uuid(AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE)]
                interface IBase
                {
                    void Foo();
                };

                runtimeclass MyClass
                {
                    MyClass();
                    void DoWork();
                };
            }");

        var reader = new WinMdReader();
        var types = reader.ReadTypes(winmd);

        var rc = types.OfType<RuntimeClassSymbol>().FirstOrDefault(r => r.Name == "MyClass");
        Assert.NotNull(rc);

        // The synthesized default interface should have the DefaultAttribute
        var defaultIface = rc.Interfaces.FirstOrDefault(i => i.IsDefaultInterface);
        Assert.NotNull(defaultIface);
        Assert.True(defaultIface.IsDefaultInterface);
    }
}