using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for unified type_info_t: kQIEntries is promoted to static constexpr
/// alongside kIIDs, and kTypeInfo includes kQIEntries. qi_check takes kTypeInfo.
/// </summary>
public partial class ProxyGeneratorTests
{
    // ─── kQIEntries is static constexpr (not local to QI method) ───

    [Fact]
    public void Proxy_KQIEntries_IsStaticConstexpr()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("static const winmidl::qi_entry_t kQIEntries[]", output);
    }

    // ─── kTypeInfo includes kQIEntries ───

    [Fact]
    public void Proxy_TypeInfo_IncludesQIEntries()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        // kTypeInfo should reference kQIEntries as the third member
        Assert.Matches(@"kTypeInfo\s*=\s*\{[^}]*kQIEntries", output);
    }

    // ─── qi_check takes kTypeInfo instead of kQIEntries ───

    [Fact]
    public void Proxy_QICheck_TakesTypeInfo()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        Assert.Contains("winmidl::qi_check(kTypeInfo,", output);
    }

    [Fact]
    public void Proxy_QICheck_DoesNotTakeKQIEntriesDirectly()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget { void DoWork(); };
            }");
        var output = _generator.Generate(compilation);

        // qi_check should NOT be called with kQIEntries directly
        Assert.DoesNotContain("qi_check(kQIEntries", output);
    }
}
