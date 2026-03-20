using Midl.Analysis;
using Midl.CodeGen.Decompiler;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

public class DecompilerTests
{
    private static byte[] CompileAndEmit(string source)
    {
        var compilation = CompilationHelper.Compile(source);
        Assert.Empty(compilation.Diagnostics);
        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    [Fact]
    public void DecompileEnum()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                enum Color { Red, Green, Blue }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("namespace Test", idl);
        Assert.Contains("enum Color", idl);
        Assert.Contains("Red", idl);
        Assert.Contains("Green", idl);
        Assert.Contains("Blue", idl);
    }

    [Fact]
    public void DecompileStruct()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                struct Point { Single X; Single Y; }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("namespace Test", idl);
        Assert.Contains("struct Point", idl);
        Assert.Contains("Single X;", idl);
        Assert.Contains("Single Y;", idl);
    }

    [Fact]
    public void DecompileInterface()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                interface IWidget
                {
                    void Activate();
                    String Name { get; };
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("namespace Test", idl);
        Assert.Contains("interface IWidget", idl);
        Assert.Contains("void Activate()", idl);
        Assert.Contains("String Name;", idl);
    }

    [Fact]
    public void DecompileRuntimeClass()
    {
        var bytes = CompileAndEmit("""
            namespace Test
            {
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """);

        var decompiler = new WinMdDecompiler();
        var idl = decompiler.Decompile(bytes);

        Assert.Contains("namespace Test", idl);
        Assert.Contains("runtimeclass Widget", idl);
        Assert.Contains("Widget()", idl);
        Assert.Contains("void DoWork()", idl);
    }

    [Fact]
    public void RoundTrip()
    {
        var originalIdl = """
            namespace Test
            {
                enum Color { Red, Green, Blue }
                struct Point { Single X; Single Y; }
                delegate void Callback();
                interface IWidget
                {
                    void Activate();
                }
                runtimeclass Widget
                {
                    Widget();
                    void DoWork();
                }
            }
            """;

        // Compile original → emit WinMD
        var bytes = CompileAndEmit(originalIdl);

        // Decompile WinMD → IDL
        var decompiler = new WinMdDecompiler();
        var decompiledIdl = decompiler.Decompile(bytes);

        // Recompile the decompiled IDL — should produce no errors
        var recompilation = CompilationHelper.Compile(decompiledIdl);
        var errors = recompilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();

        Assert.Empty(errors);
    }
}
