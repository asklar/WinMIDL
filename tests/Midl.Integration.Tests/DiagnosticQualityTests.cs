using Midl.Analysis;
using Midl.Analysis.Diagnostics;

namespace Midl.Integration.Tests;

public class DiagnosticQualityTests
{
    [Fact]
    public void LowercaseBooleanSuggestsBoolean()
    {
        var compilation = CompilationHelper.Compile(@"
namespace TestNs
{
    runtimeclass Foo
    {
        void DoWork(boolean x);
    }
}");
        var warnings = compilation.Diagnostics
            .Where(d => d.Code == "MIDL2005" && d.Message.Contains("Boolean"))
            .ToList();
        Assert.NotEmpty(warnings);
        Assert.Contains(warnings, w => w.Message.Contains("boolean"));
    }

    [Fact]
    public void LowercaseDoubleSuggestsDouble()
    {
        var compilation = CompilationHelper.Compile(@"
namespace TestNs
{
    runtimeclass Foo
    {
        void DoWork(double x);
    }
}");
        var warnings = compilation.Diagnostics
            .Where(d => d.Code == "MIDL2005" && d.Message.Contains("Double"))
            .ToList();
        Assert.NotEmpty(warnings);
    }

    [Fact]
    public void LowercaseIntSuggestsInt32()
    {
        var compilation = CompilationHelper.Compile(@"
namespace TestNs
{
    runtimeclass Foo
    {
        void DoWork(int x);
    }
}");
        var warnings = compilation.Diagnostics
            .Where(d => d.Code == "MIDL2005" && d.Message.Contains("Int32"))
            .ToList();
        Assert.NotEmpty(warnings);
    }

    [Fact]
    public void HResultWarnsInStrictMode()
    {
        var compilation = CompilationHelper.Compile(@"
namespace TestNs
{
    runtimeclass Foo
    {
        HRESULT DoWork();
    }
}");
        var warnings = compilation.Diagnostics
            .Where(d => d.Code == "MIDL2002" && d.Message.Contains("HRESULT"))
            .ToList();
        Assert.NotEmpty(warnings);
    }

    [Fact]
    public void BSTRSuggestsString()
    {
        var compilation = CompilationHelper.Compile(@"
namespace TestNs
{
    runtimeclass Foo
    {
        void DoWork(BSTR x);
    }
}");
        var warnings = compilation.Diagnostics
            .Where(d => d.Code == "MIDL2005" && d.Message.Contains("String"))
            .ToList();
        Assert.NotEmpty(warnings);
    }

#if WITH_COMPAT
    [Fact]
    public void MissingTokenHasExpectedInfo()
    {
        // Missing closing brace
        var compilation = CompilationHelper.Compile("namespace Foo {", CompilationMode.Compat);
        var errors = compilation.Diagnostics
            .Where(d => d.Code == "MIDL1102" && d.Message.Contains("Expected") && d.Message.Contains("}"))
            .ToList();
        Assert.NotEmpty(errors);
    }
#endif

    [Fact]
    public void EmptyStructDiagnostic()
    {
        var compilation = CompilationHelper.Compile(@"
namespace TestNs
{
    struct Empty {};
}");
        var errors = compilation.Diagnostics
            .Where(d => d.Code == "MIDL1009" && d.Message.Contains("Empty"))
            .ToList();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ReservedParamNameWarning()
    {
        var compilation = CompilationHelper.Compile(@"
namespace TestNs
{
    runtimeclass Foo
    {
        void DoWork(Int32 result);
    }
}");
        var warnings = compilation.Diagnostics
            .Where(d => d.Code == "MIDL1013" && d.Message.Contains("result"))
            .ToList();
        Assert.NotEmpty(warnings);
    }

    [Fact]
    public void CaseInsensitiveCollision()
    {
        var compilation = CompilationHelper.Compile(@"
namespace TestNs
{
    runtimeclass Foo {}
    runtimeclass foo {}
}");
        var errors = compilation.Diagnostics
            .Where(d => d.Code == "MIDL1010" && d.Message.Contains("foo") && d.Message.Contains("Foo"))
            .ToList();
        Assert.NotEmpty(errors);
    }

#if WITH_COMPAT
    [Fact]
    public void AllDiagnosticsHaveNonEmptyMessages()
    {
        var inputs = new[]
        {
            "@@@ garbage",
            "namespace { }",
            "runtimeclass Broken",
            "namespace Ns { struct Empty {}; }",
            "namespace Ns { runtimeclass X { void Foo(Int32 result); }; }",
        };

        foreach (var input in inputs)
        {
            var compilation = CompilationHelper.Compile(input, CompilationMode.Compat);
            foreach (var d in compilation.Diagnostics)
            {
                Assert.False(string.IsNullOrEmpty(d.Message),
                    $"Diagnostic {d.Code} at position {d.Position} has empty message for input: {input}");
            }
        }
    }
#endif
}
