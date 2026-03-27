using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for v2 shared proxy_helpers.h integration:
/// - winmidl/proxy_helpers.h include
/// - kIIDs[] / kTypeInfo static data
/// - One-liner forwarding to winmidl:: helpers for IInspectable
/// - qi_check-based QueryInterface
/// </summary>
public partial class ProxyGeneratorTests
{
    // ─── Include ───

    [Fact]
    public void Proxy_IncludesSharedHelperHeader()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("#include \"winmidl/proxy_helpers.h\"", output);
    }

    // ─── Static data ───

    [Fact]
    public void Proxy_EmitsKIIDsArray()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("kIIDs[]", output);
        Assert.Contains("IID___x_TestApp_IWidget", output);
    }

    [Fact]
    public void Proxy_EmitsTypeInfo()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("winmidl::type_info_t kTypeInfo", output);
        Assert.Contains("kIIDs", output);
    }

    // ─── IInspectable one-liner forwards ───

    [Fact]
    public void Proxy_GetIids_ForwardsToHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("winmidl::get_iids(kTypeInfo", output);
    }

    [Fact]
    public void Proxy_GetRuntimeClassName_UsesStaticReference()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("constexpr WCHAR s_name[]", output);
        Assert.Contains("ARRAYSIZE(s_name) - 1", output);
    }

    [Fact]
    public void Proxy_GetTrustLevel_ForwardsToHelper()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("winmidl::get_trust_level(", output);
    }

    // ─── No more inline IInspectable implementation ───

    [Fact]
    public void Proxy_NoInlineGetIidsBody()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        // Old inline body should be gone
        Assert.DoesNotContain("_countof(kImplementedIIDs)", output);
        Assert.DoesNotContain("CoTaskMemAlloc(sizeof(kImplementedIIDs))", output);
    }

    [Fact]
    public void Proxy_NoInlineGetRuntimeClassNameBody()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.DoesNotContain("_countof(kRuntimeClassName)", output);
        Assert.DoesNotContain("m_hdrClassName", output);
    }

    // ─── qi_check-based QI ───

    [Fact]
    public void Proxy_QI_UsesQiCheck()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("winmidl::qi_check(", output);
        Assert.Contains("kQIEntries", output);
    }

    [Fact]
    public void Proxy_QI_EntriesContainAllIIDs()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        // kQIEntries should contain IID_IUnknown, IID_IRpcProxyBuffer, and the interface IID
        Assert.Contains("IID_IUnknown", output);
        Assert.Contains("IID_IRpcProxyBuffer", output);
        Assert.Contains("IID___x_TestApp_IWidget", output);
    }

    [Fact]
    public void Proxy_QI_NoInlineForLoop()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        // Old for-loop QI pattern should be replaced by qi_check
        Assert.DoesNotContain("kNonDelegatingIIDs", output);
    }
}
