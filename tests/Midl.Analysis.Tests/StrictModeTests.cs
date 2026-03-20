using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Analysis.Tests;

public class StrictModeTests
{
    [Fact]
    public void StrictMode_RejectCoclass()
    {
        var source = "coclass MyClass { }";
        var compilation = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Strict);

        var warning = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL2001");
        Assert.Equal(DiagnosticSeverity.Warning, warning.Severity);
        Assert.Contains("coclass", warning.Message);
    }

    [Fact]
    public void StrictMode_RejectLibrary()
    {
        var source = "library MyLib { }";
        var compilation = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Strict);

        var warning = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL2001");
        Assert.Equal(DiagnosticSeverity.Warning, warning.Severity);
        Assert.Contains("library", warning.Message);
    }

    [Fact]
    public void StrictMode_RejectTypedef()
    {
        var source = "typedef int MYINT;";
        var compilation = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Strict);

        // Should have MIDL2001 for typedef and MIDL2005 for lowercase 'int'
        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL2001" && d.Message.Contains("typedef"));
        Assert.All(compilation.Diagnostics.Where(d => d.Code == "MIDL2001"),
            d => Assert.Equal(DiagnosticSeverity.Warning, d.Severity));
    }

    [Fact]
    public void StrictMode_RejectCppQuote()
    {
        var source = """cpp_quote("#include <windows.h>")""";
        var compilation = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Strict);

        var warning = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL2001");
        Assert.Equal(DiagnosticSeverity.Warning, warning.Severity);
        Assert.Contains("cpp_quote", warning.Message);
    }

    [Fact]
    public void StrictMode_AcceptRuntimeclass()
    {
        var source = """
            namespace N
            {
                runtimeclass C { }
            }
            """;
        var compilation = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Strict);

        Assert.Empty(compilation.Diagnostics);
    }

    [Fact]
    public void StrictMode_AcceptFullMidl3()
    {
        var source = """
            namespace MyApp
            {
                interface IWidget
                {
                    void Activate();
                }

                runtimeclass Widget : IWidget
                {
                    Widget();
                    String Name { get; set; };
                }

                struct Point
                {
                    Single X;
                    Single Y;
                }

                enum Color
                {
                    Red,
                    Green,
                    Blue,
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Strict);

        Assert.Empty(compilation.Diagnostics);
    }

    [Fact]
    public void CompatMode_AcceptCoclass()
    {
        var source = "coclass MyClass { }";
        var compilation = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Compat);

        // No strict-mode warnings in compat mode
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code.StartsWith("MIDL2"));
    }

    [Fact]
    public void CompatMode_AcceptMixed()
    {
        var source = """
            coclass LegacyClass { }

            namespace Modern
            {
                runtimeclass NewClass { }
            }
            """;
        var compilation = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Compat);

        // No strict-mode warnings in compat mode
        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code.StartsWith("MIDL2"));
    }

    [Fact]
    public void StrictMode_WarnLowercaseBoolean()
    {
        // 'boolean' is lexed as an identifier in MIDL; use a keyword the lexer recognizes
        // The lexer maps lowercase type names like 'int', 'long', 'float' to specific SyntaxKind tokens
        var source = """
            namespace N
            {
                struct S
                {
                    int X;
                }
            }
            """;
        var compilation = CompilationHelper.Compile(source, referencePaths: null, mode: CompilationMode.Strict);

        var warning = Assert.Single(compilation.Diagnostics, d => d.Code == "MIDL2005");
        Assert.Equal(DiagnosticSeverity.Warning, warning.Severity);
        Assert.Contains("int", warning.Message);
        Assert.Contains("Int32", warning.Message);
    }

    [Fact]
    public void StrictMode_IsDefault()
    {
        // CompilationHelper.Compile(source) without mode parameter uses Strict
        var source = "coclass MyClass { }";
        var compilation = CompilationHelper.Compile(source);

        // Should produce strict-mode warning by default
        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL2001");
    }
}
