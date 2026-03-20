using Midl.Analysis;

namespace Midl.Integration.Tests;

public class SyntaxErrorDiagnosticTests
{
    [Fact]
    public void InvalidCharactersProduceDiagnostics()
    {
        var compilation = CompilationHelper.Compile("@@@ namespace Foo { }", CompilationMode.Compat);
        var errors = compilation.Diagnostics.Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error).ToList();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Code == "MIDL1101"); // Unexpected character
    }

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

    [Fact]
    public void TypeWithoutNamespaceProducesError()
    {
        // MIDL3 requires all types to be in a namespace
        var compilation = CompilationHelper.Compile("runtimeclass Foo { }");
        var errors = compilation.Diagnostics.Where(d => d.Code == "MIDL1003").ToList();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Message.Contains("Foo"));
    }
}
