using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Xunit.Abstractions;

namespace Midl.E2E.Tests;

/// <summary>
/// E2E test: builds an out-of-proc WinRT EXE server, proxy DLL, and client EXE,
/// then validates cross-process marshaling through our generated proxy/stub.
/// All logic runs in C# — no PowerShell scripts are spawned.
/// </summary>
[SupportedOSPlatform("windows")]
public class E2EOutOfProcMarshalTests
{
    private readonly ITestOutputHelper _output;

    public E2EOutOfProcMarshalTests(ITestOutputHelper output) => _output = output;
    // CLSIDs must match the server source (CalcServer.cpp)
    private static readonly string[] AllClsids =
    [
        "{B340F2FC-5C14-FD5E-A7F2-BE8DC2D67C04}", // Calculator
        "{E1F2A3B4-C5D6-4E7F-8091-A2B3C4D5E6F7}", // TypeShowcase
        "{A1B2C3D4-E5F6-4A7B-8C9D-0E1F2A3B4C59}", // Notifier
        "{C3D4E5F6-A7B8-4C9D-0E1F-2A3B4C5D6E7F}", // PropertyBag
        "{F2A3B4C5-D6E7-4F80-91A2-B3C4D5E6F7A8}", // ComplexOps
        "{D4E5F6A7-B8C9-4D0E-1F2A-3B4C5D6E7F80}", // OptionalValues
        "{1A2B3C4D-5E6F-4A7B-8C9D-AEBFC0D1E2F3}", // Circle
        "{2B3C4D5E-6F7A-4B8C-9DAE-BFC0D1E2F3A4}", // EventSource
        "{3C4D5E6F-7A8B-4C9D-AEBF-C0D1E2F3A4B5}", // DelegateInvoker
        "{4D5E6F7A-8B9C-4D0E-1F2A-3B4C5D6E7F81}", // StringListProvider
        "{5E6F7A8B-9C0D-4E1F-2A3B-4C5D6E7F8091}", // AsyncCalculator
    ];

    [SkippableFact]
    [Trait("Category", "E2E")]
    public void OutOfProc_CustomMarshal_CrossProcess_RoundTrip()
    {
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
            "Out-of-proc COM test requires Windows");

        var msvc = E2ETestHelper.FindMsvc();
        Skip.If(msvc == null, "MSVC toolchain not found");

        var repoRoot = E2ETestHelper.FindRepoRoot();
        var testDir = Path.Combine(repoRoot, "tests", "e2e-custom-marshal", "outofproc");
        using var helper = new E2ETestHelper(testDir, msvc!);

        // ── Step 1: Generate code from IDL ──────────────────────────────
        helper.GenerateFromIdl(
            Path.Combine(testDir, "TestCalculator.idl"),
            "TestCalculator.h",
            "TestCalculator_i.c",
            "TestCalculator_proxy.cpp");

        Assert.True(File.Exists(Path.Combine(helper.GenDir, "TestCalculator_proxy.cpp")),
            "Generated proxy not found");

        // ── Step 2: Compile ─────────────────────────────────────────────
        var oopFlags = new[]
        {
            "/nologo", "/EHsc", "/std:c++latest", "/W3",
            "/DWIN32_LEAN_AND_MEAN", "/D_WIN32_WINNT=0x0A00"
        };
        var includes = new[] { helper.GenDir, Path.Combine(helper.RepoRoot, "include") };

        // 2a: Compile & link proxy DLL
        var proxyObj = Path.Combine(helper.BuildDir, "TestCalculator_proxy.obj");
        helper.CompileObj(
            Path.Combine(helper.GenDir, "TestCalculator_proxy.cpp"),
            proxyObj, includes, oopFlags);

        var proxyDll = Path.Combine(helper.BuildDir, "TestCalculatorProxy.dll");
        helper.LinkDll(proxyDll,
            Path.Combine(testDir, "TestCalculatorProxy.def"),
            [proxyObj],
            ["ole32.lib", "oleaut32.lib", "rpcrt4.lib", "advapi32.lib",
             "user32.lib", "windowsapp.lib"]);

        // 2b: Compile & link server EXE
        var serverObj = Path.Combine(helper.BuildDir, "CalcServer.obj");
        helper.CompileObj(
            Path.Combine(testDir, "CalcServer.cpp"),
            serverObj, includes, oopFlags);

        var serverExe = Path.Combine(helper.BuildDir, "CalcServer.exe");
        helper.LinkExe(serverExe,
            [serverObj],
            ["ole32.lib", "oleaut32.lib", "windowsapp.lib"]);

        // 2c: Compile & link client EXE
        var clientObj = Path.Combine(helper.BuildDir, "CalcClient.obj");
        helper.CompileObj(
            Path.Combine(testDir, "CalcClient.cpp"),
            clientObj, includes, oopFlags);

        var clientExe = Path.Combine(helper.BuildDir, "CalcClient.exe");
        helper.LinkExe(clientExe,
            [clientObj],
            ["ole32.lib", "oleaut32.lib", "windowsapp.lib"]);

        // ── Step 3: Register ────────────────────────────────────────────
        var serverExeFull = Path.GetFullPath(serverExe);
        var proxyDllFull = Path.GetFullPath(proxyDll);

        // Extract PSFactory CLSID from generated proxy
        var clsidPS = E2ETestHelper.ExtractClsidPSFactory(
            Path.Combine(helper.GenDir, "TestCalculator_proxy.cpp"));
        Assert.NotNull(clsidPS);

        // Extract all IIDs from generated code
        var iids = E2ETestHelper.ExtractIids(
            Path.Combine(helper.GenDir, "TestCalculator_i.c"));
        Assert.NotEmpty(iids);

        // Register all CLSIDs as LocalServer32
        foreach (var clsid in AllClsids)
            helper.RegisterLocalServer(clsid, serverExeFull);

        // Register PSFactory as InprocServer32 (Both)
        helper.RegisterInprocServer(clsidPS!, proxyDllFull, "Both");

        // Register all IIDs → ProxyStubClsid32
        foreach (var iid in iids.Values)
            helper.RegisterProxyStub(iid, clsidPS!);

        // ── Step 4: Start server ────────────────────────────────────────
        helper.StartServer(serverExeFull);

        // ── Step 5: Run client ──────────────────────────────────────────
        var result = helper.RunClient(clientExe);

        if (!string.IsNullOrWhiteSpace(result.StdOut))
            _output.WriteLine(result.StdOut);
        if (!string.IsNullOrWhiteSpace(result.StdErr))
            _output.WriteLine($"[stderr] {result.StdErr}");

        // ── Step 6: Assert ──────────────────────────────────────────────
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("PASS:", result.StdOut);
        Assert.DoesNotContain("FAIL:", result.StdOut);
    }
}
