using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Quality sweep tests for StubGenerator:
/// Issue 1: sizeof() in constexpr sizes (no hardcoded numbers)
/// Issue 3: Consolidated includes
/// Issue 5: Error checking on COM calls
/// Issue 6: Async HSTRING safety
/// </summary>
public partial class StubGeneratorTests
{
    // ─── Issue 1: sizeof() everywhere ───

    [Fact]
    public void Stub_ConstexprReplySize_UsesSizeofNotBareNumbers()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    Int32 GetValue();
                };
            }");
        var output = _generator.Generate(compilation);

        // Should use sizeof(INT32) + sizeof(HRESULT), not a bare number
        Assert.Contains("constexpr", output);
        Assert.DoesNotMatch(@"constexpr UINT32 kReplySize = [1-9]\d*;", output);
    }

    [Fact]
    public void Stub_ConstexprReplySize_ContainsSizeof()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    Boolean TryReset(out Int32 errorCode);
                };
            }");
        var output = _generator.Generate(compilation);

        // Reply size for out Int32 + retval Boolean + HRESULT should be sizeof-composed
        Assert.Contains("sizeof(HRESULT)", output);
    }

    // ─── Issue 3: Consolidated includes ───

    [Fact]
    public void Stub_IncludesAppearOnlyOnce()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        var lines = output.Split('\n');
        var includeLines = lines.Where(l => l.TrimStart().StartsWith("#include")).ToList();
        var uniqueIncludes = includeLines.Select(l => l.Trim()).Distinct().ToList();
        Assert.Equal(uniqueIncludes.Count, includeLines.Count);
    }

    // ─── Issue 5: Error checking on COM calls ───

    [Fact]
    public void Stub_GetBuffer_IsChecked()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        // GetBuffer in stub must have error checking
        var gbIdx = output.IndexOf("GetBuffer(pMsg");
        Assert.True(gbIdx >= 0, "GetBuffer should be present in stub");
        var nextLines = output.Substring(gbIdx, Math.Min(200, output.Length - gbIdx));
        Assert.Contains("FAILED(", nextLines);
    }

    // ─── Issue 6: Async HSTRING safety ───

    [Fact]
    public void Stub_AsyncMethod_HStringInput_UsesWindowsCreateString()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IAsyncAction {};
                interface IWidget {
                    IAsyncAction DoWorkAsync(String name);
                };
            }");
        var output = _generator.Generate(compilation);

        // Find the handler definition (not the switch dispatch call)
        var invokeIdx = output.IndexOf("HRESULT Invoke_DoWorkAsync(RPCOLEMESSAGE*");
        Assert.True(invokeIdx >= 0, "Invoke_DoWorkAsync handler should be present");
        var returnIdx = output.IndexOf("return S_OK;", invokeIdx);
        Assert.True(returnIdx >= 0, "return S_OK should be present");
        var handlerBody = output[invokeIdx..(returnIdx + 20)];
        Assert.Contains("winmidl::unmarshal_hstring_alloc(", handlerBody);
        Assert.DoesNotContain("WindowsCreateStringReference", handlerBody);
    }

    [Fact]
    public void Stub_NonAsyncMethod_HStringInput_UsesWindowsCreateStringReference()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetName(String name);
                };
            }");
        var output = _generator.Generate(compilation);

        // Non-async methods should use unmarshal_hstring_ref for zero-copy
        Assert.Contains("winmidl::unmarshal_hstring_ref(", output);
    }

    [Fact]
    public void Stub_AsyncOperationReturn_HStringInput_UsesWindowsCreateString()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IAsyncOperation {};
                interface IWidget {
                    IAsyncOperation ProcessAsync(String data);
                };
            }");
        var output = _generator.Generate(compilation);

        var invokeIdx = output.IndexOf("HRESULT Invoke_ProcessAsync(RPCOLEMESSAGE*");
        Assert.True(invokeIdx >= 0, "Invoke_ProcessAsync handler should be present");
        var returnIdx = output.IndexOf("return S_OK;", invokeIdx);
        Assert.True(returnIdx >= 0, "return S_OK should be present");
        var handlerBody = output[invokeIdx..(returnIdx + 20)];
        Assert.Contains("winmidl::unmarshal_hstring_alloc(", handlerBody);
    }

    [Fact]
    public void Stub_HStringReturn_NoNullTerminator()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    String GetName();
                };
            }");
        var output = _generator.Generate(compilation);

        // Wire format should not include null terminator
        Assert.Contains("len * sizeof(WCHAR)", output);
        Assert.DoesNotContain("+ 1) * sizeof(WCHAR)", output);
    }
}
