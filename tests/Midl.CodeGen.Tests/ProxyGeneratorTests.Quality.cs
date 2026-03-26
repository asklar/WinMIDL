using System.Text.RegularExpressions;
using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Quality sweep tests for ProxyGenerator:
/// Issue 1: sizeof() in constexpr sizes (no hardcoded numbers)
/// Issue 3: Consolidated includes
/// Issue 4: constexpr IID cache for QI
/// Issue 5: Error checking on COM calls
/// </summary>
public partial class ProxyGeneratorTests
{
    // ─── Issue 1: sizeof() everywhere ───

    [Fact]
    public void Proxy_ConstexprRequestSize_UsesSizeofNotBareNumbers()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetPosition(Single x, Single y);
                };
            }");
        var output = _generator.Generate(compilation);

        // constexpr should compose from sizeof, not be a bare number
        Assert.Matches(@"kSetPosition_RequestSize = sizeof\(", output);
        Assert.DoesNotMatch(@"kSetPosition_RequestSize = \d+;", output);
    }

    [Fact]
    public void Proxy_ConstexprReplySize_UsesSizeofNotBareNumbers()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    Int32 GetValue();
                };
            }");
        var output = _generator.Generate(compilation);

        // Reply size should use sizeof(INT32) + sizeof(HRESULT), not "8"
        Assert.Matches(@"kGetValue_ReplySize = sizeof\(", output);
        Assert.DoesNotMatch(@"kGetValue_ReplySize = \d+;", output);
    }

    [Fact]
    public void Proxy_ConstexprSize_ComposesFromSizeof()
    {
        var compilation = Compile(@"
            namespace TestApp {
                struct Point { Single X; Single Y; };
                interface IWidget {
                    void SetPosition(Point pos);
                };
            }");
        var output = _generator.Generate(compilation);

        // Should use sizeof(Point), not "8"
        Assert.Contains("sizeof(Point)", output);
    }

    // ─── Issue 3: Consolidated includes ───

    [Fact]
    public void Proxy_IncludesAppearOnlyOnce()
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

    [Fact]
    public void Proxy_IncludesAppearBeforeCode()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        var lines = output.Split('\n');
        int lastIncludeLine = -1;
        int firstClassLine = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].TrimStart().StartsWith("#include"))
                lastIncludeLine = i;
            if (lines[i].TrimStart().StartsWith("class ") && firstClassLine == -1)
                firstClassLine = i;
        }
        Assert.True(lastIncludeLine < firstClassLine, "All #includes should appear before any class definition");
    }

    [Fact]
    public void Proxy_UsesConsolidatedIncludes()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        // Should have the consolidated include set
        Assert.Contains("#include <windows.h>", output);
        Assert.Contains("#include <objidl.h>", output);
        Assert.Contains("#include <winstring.h>", output);
        Assert.Contains("#include <inspectable.h>", output);
    }

    // ─── Issue 4: constexpr IID cache for QI ───

    [Fact]
    public void Proxy_QI_UsesArrayLookup()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        // Should use qi_check-based lookup via shared helpers
        Assert.Contains("kQIEntries", output);
        Assert.Contains("winmidl::qi_check(", output);
    }

    [Fact]
    public void Proxy_QI_ContainsAllIIDs()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        // The array should contain IID_IUnknown, IID_IRpcProxyBuffer, and the interface IID
        Assert.Contains("IID_IUnknown", output);
        Assert.Contains("IID_IRpcProxyBuffer", output);
        Assert.Contains("IID___x_TestApp_IWidget", output);
    }

    // ─── Issue 5: Error checking on COM calls ───

    [Fact]
    public void Proxy_GetBuffer_IsChecked()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("GetBuffer(&msg", output);
        // GetBuffer result should be checked
        var getBufferIdx = output.IndexOf("GetBuffer(&msg");
        var nextLines = output.Substring(getBufferIdx, 200);
        Assert.Contains("FAILED(hr)", nextLines);
    }

    [Fact]
    public void Proxy_SendReceive_IsChecked()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("SendReceive(&msg", output);
        var srIdx = output.IndexOf("SendReceive(&msg");
        var nextLines = output.Substring(srIdx, 200);
        Assert.Contains("FAILED(hr)", nextLines);
    }

    [Fact]
    public void Proxy_FreeBuffer_IsChecked()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("FreeBuffer(&msg)", output);
        var fbIdx = output.IndexOf("FreeBuffer(&msg)");
        var nextLines = output.Substring(fbIdx, Math.Min(200, output.Length - fbIdx));
        Assert.Contains("FAILED(hr)", nextLines);
    }

    [Fact]
    public void Proxy_WindowsCreateString_IsChecked()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    String GetTitle();
                };
            }");
        var output = _generator.Generate(compilation);

        // unmarshal_hstring return must be checked
        var umIdx = output.IndexOf("winmidl::unmarshal_hstring(");
        Assert.True(umIdx >= 0, "unmarshal_hstring should be present");
        var nextLines = output.Substring(umIdx, Math.Min(200, output.Length - umIdx));
        Assert.Contains("RETURN_IF_FAILED(", nextLines);
    }
}
