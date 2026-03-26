using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for WIL pattern integration in generated proxy code.
/// Verifies RETURN_IF_FAILED macro usage and wil_compat.h include.
/// </summary>
public partial class ProxyGeneratorTests
{
    // ─── WIL compatibility ───

    [Fact]
    public void Proxy_IncludesWilCompatHeader()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("#include \"winmidl/wil_compat.h\"", output);
    }

    [Fact]
    public void Proxy_GetBuffer_UsesReturnIfFailed()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        // Should use RETURN_IF_FAILED(hr) instead of if (FAILED(hr)) return hr
        Assert.Contains("GetBuffer(&msg", output);
        var getBufferIdx = output.IndexOf("GetBuffer(&msg");
        var nextChunk = output.Substring(getBufferIdx, 200);
        Assert.Contains("RETURN_IF_FAILED(hr)", nextChunk);
        Assert.DoesNotContain("if (FAILED(hr)) return hr", nextChunk);
    }

    [Fact]
    public void Proxy_SendReceive_UsesReturnIfFailed()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("SendReceive(&msg", output);
        var srIdx = output.IndexOf("SendReceive(&msg");
        var nextChunk = output.Substring(srIdx, 200);
        Assert.Contains("RETURN_IF_FAILED(hr)", nextChunk);
        Assert.DoesNotContain("if (FAILED(hr)) return hr", nextChunk);
    }

    [Fact]
    public void Proxy_FreeBuffer_UsesReturnIfFailed()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("FreeBuffer(&msg)", output);
        var fbIdx = output.IndexOf("FreeBuffer(&msg)");
        var nextChunk = output.Substring(fbIdx, Math.Min(200, output.Length - fbIdx));
        Assert.Contains("RETURN_IF_FAILED(hr)", nextChunk);
        Assert.DoesNotContain("if (FAILED(hr)) return hr", nextChunk);
    }

    [Fact]
    public void Proxy_WindowsCreateString_UsesReturnIfFailed()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    String GetTitle();
                };
            }");
        var output = _generator.Generate(compilation);

        // unmarshal_hstring in reply deserialization should use RETURN_IF_FAILED
        var umIdx = output.IndexOf("winmidl::unmarshal_hstring(");
        Assert.True(umIdx >= 0, "unmarshal_hstring should be present");
        var nextChunk = output.Substring(umIdx, Math.Min(200, output.Length - umIdx));
        Assert.Contains("RETURN_IF_FAILED(hr)", nextChunk);
    }

    [Fact]
    public void Proxy_NoRawFailedPattern()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void DoWork();
                    String GetName();
                };
            }");
        var output = _generator.Generate(compilation);

        // No occurrences of the old pattern should remain
        Assert.DoesNotContain("if (FAILED(hr)) return hr;", output);
        Assert.DoesNotContain("if (FAILED(hrBuf)) return hrBuf;", output);
    }
}
