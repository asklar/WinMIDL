using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for /exceptions mode in generated stub code.
/// Verifies try/catch and throw_if_failed usage when exceptions are enabled.
/// </summary>
public partial class StubGeneratorTests
{
    // ─── Default (no exceptions) ───

    [Fact]
    public void Stub_Default_UsesReturnIfFailed()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("RETURN_IF_FAILED(hrBuf)", output);
        Assert.DoesNotContain("throw_if_failed", output);
        Assert.DoesNotContain("try {", output);
        Assert.DoesNotContain("catch", output);
    }

    // ─── With /exceptions ───

    [Fact]
    public void Stub_WithExceptions_EmitsTryCatch()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation, useExceptions: true);

        Assert.Contains("try {", output);
        Assert.Contains("catch (HRESULT hr) { return hr; }", output);
        Assert.Contains("catch (...) { return E_FAIL; }", output);
    }

    [Fact]
    public void Stub_WithExceptions_EmitsThrowIfFailed()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation, useExceptions: true);

        Assert.Contains("winmidl::throw_if_failed(", output);
        Assert.DoesNotContain("RETURN_IF_FAILED(", output);
    }

    [Fact]
    public void Stub_WithExceptions_GetBufferUsesThrowIfFailed()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation, useExceptions: true);

        var gbIdx = output.IndexOf("GetBuffer(pMsg");
        Assert.True(gbIdx >= 0, "GetBuffer should be present in stub");
        var chunkStart = Math.Max(0, gbIdx - 50);
        var chunk = output.Substring(chunkStart, Math.Min(250, output.Length - chunkStart));
        Assert.Contains("winmidl::throw_if_failed(", chunk);
    }
}
