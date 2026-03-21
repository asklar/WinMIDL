using Midl.Analysis;

namespace Midl.Integration.Tests;

public class SyntaxErrorDiagnosticTests
{
#if WITH_COMPAT
    [Fact]
    public void InvalidCharactersProduceDiagnostics()
    {
        var compilation = CompilationHelper.Compile("@@@ namespace Foo { }", CompilationMode.Compat);
        var errors = compilation.Diagnostics.Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error).ToList();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Code == "MIDL1101"); // Unexpected character
    }
#endif

    [Fact]
    public void ValidMidl3ProducesNoDiagnostics()
    {
        var source = @"
namespace TestApp
{
    runtimeclass MyClass
    {
        MyClass();
        String Name { get; };
        void DoWork(Int32 x);
    }
}";
        var compilation = CompilationHelper.Compile(source);
        var errors = compilation.Diagnostics.Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
    }

#if WITH_COMPAT
    [Fact]
    public void ErrorTokensHaveCorrectPositions()
    {
        // The @ is at position 0
        var compilation = CompilationHelper.Compile("@ namespace Foo { }", CompilationMode.Compat);
        var errors = compilation.Diagnostics.Where(d => d.Code == "MIDL1101").ToList();
        Assert.Single(errors);
        Assert.Equal(0, errors[0].Position);
    }

    [Fact]
    public void SkippedTokensProduceDiagnostics()
    {
        // "$$$" produces ErrorTokens that should become diagnostics
        var compilation = CompilationHelper.Compile("namespace Foo { } $$$", CompilationMode.Compat);
        var hasErrors = compilation.Diagnostics.Any(d =>
            d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);
        Assert.True(hasErrors, "Invalid characters after valid IDL should produce diagnostics");
    }

    [Fact]
    public void LspReceivesDiagnosticsFromSyntaxErrors()
    {
        // Simulate what the LSP does: compile and check diagnostics are non-empty for bad input
        var compilation = CompilationHelper.Compile("$$$", CompilationMode.Compat);
        Assert.NotEmpty(compilation.Diagnostics);
    }
#endif

    [Fact]
    public void TypeWithoutNamespaceProducesError()
    {
        // MIDL3 requires all types to be in a namespace
        var compilation = CompilationHelper.Compile("runtimeclass Foo { }");
        var errors = compilation.Diagnostics.Where(d => d.Code == "MIDL1003").ToList();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Message.Contains("Foo"));
    }

    [Fact]
    public void ForwardDeclaration_AttributeNoCascadingErrors()
    {
        // "attribute foo;" without a body should parse cleanly without cascading errors
        var compilation = CompilationHelper.Compile("""
            namespace hello
            {
                attribute foo;
                interface bar
                {
                    Single x { set; get; };
                }
            }
            """);

        // No MIDL1102 (syntax errors) — forward decl should be handled gracefully
        var syntaxErrors = compilation.Diagnostics.Where(d => d.Code == "MIDL1102").ToList();
        Assert.Empty(syntaxErrors);
    }

    [Fact]
    public void ForwardDeclaration_InterfaceNoCascadingErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                interface IFoo;
                runtimeclass MyClass
                {
                    MyClass();
                }
            }
            """);

        var syntaxErrors = compilation.Diagnostics.Where(d => d.Code == "MIDL1102").ToList();
        Assert.Empty(syntaxErrors);
    }

    [Fact]
    public void ForwardDeclaration_RuntimeclassNoCascadingErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                runtimeclass Foo;
                runtimeclass Bar
                {
                    Bar();
                }
            }
            """);

        var syntaxErrors = compilation.Diagnostics.Where(d => d.Code == "MIDL1102").ToList();
        Assert.Empty(syntaxErrors);
    }

    [Fact]
    public void ForwardDeclaration_StructNoCascadingErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                struct MyPoint;
                struct RealPoint
                {
                    Single X;
                    Single Y;
                };
            }
            """);

        var syntaxErrors = compilation.Diagnostics.Where(d => d.Code == "MIDL1102").ToList();
        Assert.Empty(syntaxErrors);
    }

    [Fact]
    public void ForwardDeclaration_EnumNoCascadingErrors()
    {
        var compilation = CompilationHelper.Compile("""
            namespace TestApp
            {
                enum Color;
                enum Shape { Circle, Square };
            }
            """);

        var syntaxErrors = compilation.Diagnostics.Where(d => d.Code == "MIDL1102").ToList();
        Assert.Empty(syntaxErrors);
    }

    [Fact]
    public void UnrecognizedKeyword_InNamespace_ProducesError()
    {
        var compilation = CompilationHelper.Compile("namespace foo { class x { } }");
        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL1100");
    }

    [Fact]
    public void BareIdentifier_InNamespace_ProducesError()
    {
        var compilation = CompilationHelper.Compile("namespace foo { bar }");
        Assert.Contains(compilation.Diagnostics, d => d.Code == "MIDL1100");
    }

    [Fact]
    public void UnquotedGuid_InAttribute_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N {
                [uuid(12345678-1234-1234-1234-123456789abc)]
                interface IFoo { void Bar(); };
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1100");
    }

    [Fact]
    public void UnquotedHexGuid_InAttribute_NoError()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N {
                [uuid(AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE)]
                interface IFoo { void Bar(); };
            }
            """);

        Assert.DoesNotContain(compilation.Diagnostics, d => d.Code == "MIDL1100");
    }

#if WITH_COMPAT
    [Fact]
    public void MultipleAttributesInOneBracket_ParsesCleanly()
    {
        var compilation = CompilationHelper.Compile("""
            namespace N
            {
                [uuid(12345678-1234-1234-1234-123456789abc), webhosthidden]
                interface IFoo { void Bar(); };

                [activatable(1), threading(both), marshaling_behavior(agile), default_interface]
                runtimeclass Widget { Widget(); }
            }
            """, CompilationMode.Compat);

        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
    }
#endif
}
