using Midl.Analysis;
using Midl.Syntax;

namespace Midl.Integration.Tests;

/// <summary>
/// Tests for MidlTrace. Must run serially (not in parallel) because MidlTrace uses
/// static state — concurrent enable/disable corrupts the shared TextWriter.
/// </summary>
[Collection("MidlTrace")]
public class DebugTraceTests : IDisposable
{
    public DebugTraceTests()
    {
        // Ensure tracing is disabled before each test
        MidlTrace.Disable();
    }

    public void Dispose()
    {
        MidlTrace.Disable();
    }

    [Fact]
    public void TraceDisabledByDefault()
    {
        Assert.False(MidlTrace.IsEnabled);
    }

    [Fact]
    public void TraceCanBeEnabled()
    {
        MidlTrace.Enable();
        Assert.True(MidlTrace.IsEnabled);
        MidlTrace.Write("TEST", "Hello from test");
        MidlTrace.Disable();
        Assert.False(MidlTrace.IsEnabled);
    }

    [Fact]
    public void TraceWritesToFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            MidlTrace.Enable(tempFile);

            // Compile something to generate trace output
            _ = CompilationHelper.Compile("namespace Test { interface IFoo { void Bar(); } }");

            MidlTrace.Disable();

            var content = File.ReadAllText(tempFile);
            Assert.NotEmpty(content);
            Assert.Contains("[MIDL:", content);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void CompilationProducesTraceOutput()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            MidlTrace.Enable(tempFile);

            _ = CompilationHelper.Compile("namespace Test { interface IFoo { void Bar(); } }");

            MidlTrace.Disable();

            var content = File.ReadAllText(tempFile);
            Assert.Contains("[MIDL:LEX]", content);
            Assert.Contains("[MIDL:PARSE]", content);
            Assert.Contains("[MIDL:BIND]", content);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void TraceDoesNotAffectOutput()
    {
        // Compile without tracing
        var compilationWithout = CompilationHelper.Compile(
            "namespace Test { interface IFoo { void Bar(); } }");
        var diagsWithout = compilationWithout.Diagnostics.Select(d => d.Code).ToList();

        // Compile with tracing
        var tempFile = Path.GetTempFileName();
        try
        {
            MidlTrace.Enable(tempFile);
            var compilationWith = CompilationHelper.Compile(
                "namespace Test { interface IFoo { void Bar(); } }");
            MidlTrace.Disable();

            var diagsWith = compilationWith.Diagnostics.Select(d => d.Code).ToList();
            Assert.Equal(diagsWithout, diagsWith);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
