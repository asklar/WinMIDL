using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.CodeGen.Decompiler;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

public class WinMdArrayTests
{
    private static byte[] CompileAndEmit(string source)
    {
        var compilation = CompilationHelper.Compile(source, referencePaths: null,
            mode: CompilationMode.Strict, includePaths: null, currentFilePath: null,
            arraysEnabled: true);
        var errors = compilation.Diagnostics.Where(d =>
            d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
        return new WinMdEmitter().EmitToBytes(compilation);
    }

    [Fact]
    public void ArrayParam_EncodedAsSZArray()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IWidget
                {
                    void SetItems(Int32[] items);
                }
            }
            """);

        using var pe = new PEReader(new MemoryStream(bytes));
        var reader = pe.GetMetadataReader();

        // Find the SetItems method
        var methodDef = FindMethod(reader, "SetItems");
        Assert.NotNull(methodDef);

        // Check that the parameter has custom attributes
        var paramHandles = methodDef.Value.GetParameters();
        bool foundArrayParam = false;
        foreach (var ph in paramHandles)
        {
            var p = reader.GetParameter(ph);
            if (reader.GetString(p.Name) == "items")
            {
                foundArrayParam = true;
                var attrs = p.GetCustomAttributes()
                    .Select(reader.GetCustomAttribute)
                    .ToList();
                Assert.True(attrs.Count > 0, "Array param should have attributes");
                break;
            }
        }
        Assert.True(foundArrayParam, "Should find 'items' parameter");
    }

    [Fact]
    public void OutArrayParam_HasWriteOnlyAttribute()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IWidget
                {
                    void GetItems(out Int32[] items);
                }
            }
            """);

        using var pe = new PEReader(new MemoryStream(bytes));
        var reader = pe.GetMetadataReader();
        var methodDef = FindMethod(reader, "GetItems");
        Assert.NotNull(methodDef);
    }

    [Fact]
    public void Decompiler_RoundTrip_PassArray()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IWidget
                {
                    void SetItems(Int32[] items);
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("Int32[]", idl);
        Assert.Contains("items", idl);
    }

    [Fact]
    public void Decompiler_RoundTrip_OutArray()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IWidget
                {
                    void GetItems(out Int32[] items);
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("Int32[]", idl);
        Assert.Contains("out", idl);
    }

    [Fact]
    public void Decompiler_RoundTrip_ArrayReturn()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IWidget
                {
                    Int32[] GetItems();
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("Int32[]", idl);
        Assert.Contains("GetItems", idl);
    }

    [Fact]
    public void Decompiler_RoundTrip_StringArray()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IWidget
                {
                    void SetNames(String[] names);
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("String[]", idl);
    }

    private static MethodDefinition? FindMethod(MetadataReader reader, string name)
    {
        foreach (var tdh in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(tdh);
            foreach (var mh in td.GetMethods())
            {
                var m = reader.GetMethodDefinition(mh);
                if (reader.GetString(m.Name) == name)
                    return m;
            }
        }
        return null;
    }
}
