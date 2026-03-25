using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for /exceptions mode in generated proxy code.
/// Verifies try/catch and throw_if_failed usage when exceptions are enabled.
/// </summary>
public partial class ProxyGeneratorTests
{
    // ─── Default (no exceptions) ───

    [Fact]
    public void Proxy_Default_UsesReturnIfFailed()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("RETURN_IF_FAILED(hr)", output);
        Assert.DoesNotContain("throw_if_failed", output);
        Assert.DoesNotContain("try {", output);
        Assert.DoesNotContain("catch", output);
    }

    // ─── With /exceptions ───

    [Fact]
    public void Proxy_WithExceptions_EmitsTryCatch()
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
    public void Proxy_WithExceptions_EmitsThrowIfFailed()
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
    public void Proxy_WithExceptions_GetBufferUsesThrowIfFailed()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation, useExceptions: true);

        var getBufferIdx = output.IndexOf("GetBuffer(&msg");
        Assert.True(getBufferIdx >= 0, "GetBuffer should be present");
        var chunkStart = Math.Max(0, getBufferIdx - 50);
        var chunk = output.Substring(chunkStart, Math.Min(250, output.Length - chunkStart));
        Assert.Contains("winmidl::throw_if_failed(", chunk);
    }

    [Fact]
    public void Proxy_WithExceptions_SendReceiveUsesThrowIfFailed()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation, useExceptions: true);

        var srIdx = output.IndexOf("SendReceive(&msg");
        Assert.True(srIdx >= 0, "SendReceive should be present");
        var chunkStart = Math.Max(0, srIdx - 50);
        var chunk = output.Substring(chunkStart, Math.Min(250, output.Length - chunkStart));
        Assert.Contains("winmidl::throw_if_failed(", chunk);
    }

    [Fact]
    public void Proxy_WithExceptions_FreeBufferUsesThrowIfFailed()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation, useExceptions: true);

        var fbIdx = output.IndexOf("FreeBuffer(&msg)");
        Assert.True(fbIdx >= 0, "FreeBuffer should be present");
        var chunkStart = Math.Max(0, fbIdx - 50);
        var chunk = output.Substring(chunkStart, Math.Min(250, output.Length - chunkStart));
        Assert.Contains("winmidl::throw_if_failed(", chunk);
    }
}
