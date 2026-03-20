using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;

namespace Midl.Analysis.Tests;

public class GenericResolutionTests
{
    private static byte[] EmitWinMd(string idlSource)
    {
        var compilation = CompilationHelper.Compile(idlSource);
        Assert.False(compilation.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            "IDL compilation should not have errors");
        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    [Fact]
    public void ResolveImportedGenericType()
    {
        var winmd = EmitWinMd(@"
            namespace TestLib
            {
                [uuid(11111111-2222-3333-4444-555555555555)]
                interface IMyList
                {
                    void Add();
                    String GetItem();
                };
            }");

        var reader = new WinMdReader();
        var types = reader.ReadTypes(winmd);

        var iface = types.OfType<InterfaceSymbol>().FirstOrDefault(i => i.Name == "IMyList");
        Assert.NotNull(iface);
        Assert.Equal("TestLib.IMyList", iface.FullName);

        var methods = iface.Members.OfType<MethodSymbol>().ToList();
        Assert.Contains(methods, m => m.Name == "GetItem");

        var getItem = methods.First(m => m.Name == "GetItem");
        Assert.Equal("String", getItem.ReturnType.Name);
        Assert.True(getItem.ReturnType.IsBuiltIn);

        // Test ParseTypeRef for generic type strings
        var genericRef = WinMdReader.ParseTypeRef("TestLib.IMyList<String>");
        Assert.Equal("TestLib.IMyList", genericRef.Name);
        Assert.Single(genericRef.TypeArguments);
        Assert.Equal("String", genericRef.TypeArguments[0].Name);
        Assert.True(genericRef.TypeArguments[0].IsBuiltIn);
    }

    [Fact]
    public void GenericTypeInMethodSignature()
    {
        var returnType = WinMdReader.ParseTypeRef("IVector<Int32>");
        Assert.Equal("IVector", returnType.Name);
        Assert.Single(returnType.TypeArguments);
        Assert.Equal("Int32", returnType.TypeArguments[0].Name);
        Assert.True(returnType.TypeArguments[0].IsBuiltIn);
        Assert.False(returnType.IsBuiltIn);

        var mapType = WinMdReader.ParseTypeRef("IMap<String, Int32>");
        Assert.Equal("IMap", mapType.Name);
        Assert.Equal(2, mapType.TypeArguments.Count);
        Assert.Equal("String", mapType.TypeArguments[0].Name);
        Assert.Equal("Int32", mapType.TypeArguments[1].Name);

        // Test the DisplayName reconstructs the generic syntax
        Assert.Equal("IVector<Int32>", returnType.DisplayName);
        Assert.Equal("IMap<String, Int32>", mapType.DisplayName);

        // Test array of generic types
        var arrayGeneric = WinMdReader.ParseTypeRef("IVector<String>[]");
        Assert.Equal("IVector", arrayGeneric.Name);
        Assert.True(arrayGeneric.IsArray);
        Assert.Single(arrayGeneric.TypeArguments);
        Assert.Equal("String", arrayGeneric.TypeArguments[0].Name);
    }
}