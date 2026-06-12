// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for WIL pattern integration in generated stub code.
/// Verifies RETURN_IF_FAILED macro usage and wil_compat.h include.
/// </summary>
public partial class StubGeneratorTests
{
    // ─── WIL compatibility ───

    [Fact]
    public void Stub_IncludesWilCompatHeader()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("#include \"winmidl/wil_compat.h\"", output);
    }

    [Fact]
    public void Stub_GetBuffer_UsesReturnIfFailed()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        var gbIdx = output.IndexOf("GetBuffer(pMsg");
        Assert.True(gbIdx >= 0, "GetBuffer should be present in stub");
        var nextChunk = output.Substring(gbIdx, Math.Min(200, output.Length - gbIdx));
        Assert.Contains("RETURN_IF_FAILED(hrBuf)", nextChunk);
        Assert.DoesNotContain("if (FAILED(hrBuf)) return hrBuf;", nextChunk);
    }

    [Fact]
    public void Stub_NoRawFailedPattern()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void DoWork();
                    String GetName();
                };
            }");
        var output = _generator.Generate(compilation);

        Assert.DoesNotContain("if (FAILED(hr)) return hr;", output);
        Assert.DoesNotContain("if (FAILED(hrBuf)) return hrBuf;", output);
    }

    [Fact]
    public void Stub_AsyncHString_UsesReturnIfFailed()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IAsyncAction {};
                interface IWidget {
                    IAsyncAction DoWorkAsync(String name);
                };
            }");
        var output = _generator.Generate(compilation);

        // The allocating unmarshal_hstring_alloc in async path should use RETURN_IF_FAILED
        var invokeIdx = output.IndexOf("HRESULT Invoke_DoWorkAsync(RPCOLEMESSAGE*");
        Assert.True(invokeIdx >= 0, "Invoke_DoWorkAsync handler should be present");
        var returnIdx = output.IndexOf("return S_OK;", invokeIdx);
        Assert.True(returnIdx >= 0, "return S_OK should be present");
        var handlerBody = output[invokeIdx..(returnIdx + 20)];
        Assert.Contains("winmidl::unmarshal_hstring_alloc(", handlerBody);
    }
}
