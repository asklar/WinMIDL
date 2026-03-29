using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Xunit.Abstractions;

namespace Midl.E2E.Tests;

/// <summary>
/// E2E test: builds an in-proc WinRT server DLL, proxy DLL, and test client,
/// then validates cross-apartment marshaling through our generated proxy/stub.
/// All logic runs in C# — no PowerShell scripts are spawned.
/// </summary>
[SupportedOSPlatform("windows")]
public class E2EInProcMarshalTests
{
    private readonly ITestOutputHelper _output;

    public E2EInProcMarshalTests(ITestOutputHelper output) => _output = output;
    [SkippableFact]
    [Trait("Category", "E2E")]
    public void InProcProxyStub_CrossApartment_RoundTrips()
    {
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
            "In-proc COM test requires Windows");

        var msvc = E2ETestHelper.FindMsvc();
        Skip.If(msvc == null, "MSVC toolchain not found");

        var repoRoot = E2ETestHelper.FindRepoRoot();
        var testDir = Path.Combine(repoRoot, "tests", "e2e-custom-marshal", "inproc");
        using var helper = new E2ETestHelper(testDir, msvc!);

        // ── Step 1: Generate code from IDL ──────────────────────────────
        helper.GenerateFromIdl(
            Path.Combine(testDir, "TestWidget.idl"),
            "TestWidget.h",
            "TestWidget_i.c",
            "TestWidget_proxy.cpp");

        Assert.True(File.Exists(Path.Combine(helper.GenDir, "TestWidget.h")),
            "Generated header not found");
        Assert.True(File.Exists(Path.Combine(helper.GenDir, "TestWidget_i.c")),
            "Generated IID file not found");
        Assert.True(File.Exists(Path.Combine(helper.GenDir, "TestWidget_proxy.cpp")),
            "Generated proxy not found");

        // ── Step 2: Compile ─────────────────────────────────────────────
        var cppFlags = E2ETestHelper.DefaultCppFlags;
        var genIncludes = new[] { helper.GenDir };
        var proxyIncludes = new[] { helper.GenDir, Path.Combine(helper.RepoRoot, "include") };

        // 2a: Compile IID definitions (as C++ via /TP)
        var iidObj = Path.Combine(helper.BuildDir, "TestWidget_i.obj");
        helper.CompileObj(
            Path.Combine(helper.GenDir, "TestWidget_i.c"),
            iidObj, genIncludes,
            [.. cppFlags, "/TP"]);

        // 2b: Compile & link server DLL
        var serverObj = Path.Combine(helper.BuildDir, "WidgetServer.obj");
        helper.CompileObj(
            Path.Combine(testDir, "WidgetServer.cpp"),
            serverObj, genIncludes, cppFlags);

        var serverDll = Path.Combine(helper.BuildDir, "WidgetServer.dll");
        helper.LinkDll(serverDll,
            Path.Combine(testDir, "WidgetServer.def"),
            [serverObj, iidObj],
            E2ETestHelper.DefaultLibs);

        // 2c: Compile & link proxy DLL
        var proxyObj = Path.Combine(helper.BuildDir, "TestWidget_proxy.obj");
        helper.CompileObj(
            Path.Combine(helper.GenDir, "TestWidget_proxy.cpp"),
            proxyObj, proxyIncludes, cppFlags);

        var proxyDll = Path.Combine(helper.BuildDir, "TestWidgetProxy.dll");
        helper.LinkDll(proxyDll,
            Path.Combine(testDir, "TestWidgetProxy.def"),
            [proxyObj],
            E2ETestHelper.DefaultLibs);

        // 2d: Compile & link test client
        var clientObj = Path.Combine(helper.BuildDir, "TestClient.obj");
        helper.CompileObj(
            Path.Combine(testDir, "TestClient.cpp"),
            clientObj, genIncludes, cppFlags);

        var clientExe = Path.Combine(helper.BuildDir, "TestClient.exe");
        helper.LinkExe(clientExe,
            [clientObj, iidObj],
            ["ole32.lib", "oleaut32.lib", "user32.lib", "runtimeobject.lib"]);

        // ── Step 3: Register COM classes (HKCU) ─────────────────────────
        var clsidWidget = E2ETestHelper.ExtractClsid(
            Path.Combine(testDir, "WidgetServer.cpp"), "CLSID_Widget");
        Assert.NotNull(clsidWidget);

        var iids = E2ETestHelper.ExtractIids(
            Path.Combine(helper.GenDir, "TestWidget_i.c"));
        Assert.NotEmpty(iids);

        var clsidPS = E2ETestHelper.ExtractClsidPSFactory(
            Path.Combine(helper.GenDir, "TestWidget_proxy.cpp"));
        Assert.NotNull(clsidPS);

        var serverDllFull = Path.GetFullPath(serverDll);
        var proxyDllFull = Path.GetFullPath(proxyDll);

        helper.RegisterInprocServer(clsidWidget!, serverDllFull, "Apartment");
        helper.RegisterInprocServer(clsidPS!, proxyDllFull);
        foreach (var iid in iids.Values)
            helper.RegisterProxyStub(iid, clsidPS!);

        // ── Step 4: Run test client ─────────────────────────────────────
        var result = helper.RunClient(clientExe);

        if (!string.IsNullOrWhiteSpace(result.StdOut))
            _output.WriteLine(result.StdOut);
        if (!string.IsNullOrWhiteSpace(result.StdErr))
            _output.WriteLine($"[stderr] {result.StdErr}");

        // ── Step 5: Assert ──────────────────────────────────────────────
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("PASS:", result.StdOut);
        Assert.DoesNotContain("FAIL:", result.StdOut);
    }
}
