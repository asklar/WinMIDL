// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Xunit.Abstractions;

namespace Midl.E2E.Tests;

/// <summary>
/// E2E test validating the comprehensive proxy/stub codegen pipeline:
///   IDL → header + proxy → compile server DLL + proxy DLL → cross-apartment calls.
///
/// Exercises all WinRT type patterns: enums, structs, properties, out params,
/// multiple interface QI, inheritance, GetIids, GetRuntimeClassName.
/// All logic runs in C# — no PowerShell scripts are spawned.
/// </summary>
[SupportedOSPlatform("windows")]
public class E2EComprehensiveMarshalTests
{
    private readonly ITestOutputHelper _output;

    public E2EComprehensiveMarshalTests(ITestOutputHelper output) => _output = output;
    // CLSIDs must match ComprehensiveServer.cpp
    private static readonly Dictionary<string, string> ClsidMap = new()
    {
        ["{6F33328A-B1CF-1A52-8B6C-008019838105}"] = "Animal",
        ["{FD4F3844-4389-D352-BC91-3D348670F83E}"] = "Shape",
        ["{8AA503FE-EB8A-655F-9195-12D9800B2077}"] = "Circle",
        ["{05B429F5-72C9-E559-9D1D-2C09B701DEB4}"] = "Document",
        ["{FE8DF0E1-4F9D-4851-B494-0B41AB812A5D}"] = "Calculator",
        ["{A1B2C3D4-E5F6-4A7B-8C9D-0E1F2A3B4C59}"] = "Notifier",
        ["{B2C3D4E5-F6A7-4B8C-9D0E-1F2A3B4C5D6E}"] = "EventSource",
        ["{C3D4E5F6-A7B8-4C9D-0E1F-2A3B4C5D6E7F}"] = "PropertyBag",
        ["{D4E5F6A7-B8C9-4D0E-1F2A-3B4C5D6E7F80}"] = "OptionalValues",
        ["{E1F2A3B4-C5D6-4E7F-8091-A2B3C4D5E6F7}"] = "TypeShowcase",
        ["{F2A3B4C5-D6E7-4F80-91A2-B3C4D5E6F7A8}"] = "ComplexOps",
        ["{A3B4C5D6-E7F8-4091-A2B3-C4D5E6F7A8B9}"] = "MathHelpers",
        ["{B4C5D6E7-F8A9-40B1-C2D3-E4F5A6B7C8D9}"] = "Observable",
        ["{C5D6E7F8-A9B0-41C2-D3E4-F5A6B7C8D9EA}"] = "CollectionOps",
        ["{7A8B9C0D-1E2F-4A3B-4C5D-6E7F8091A2B3}"] = "NullableOps",
    };

    [Fact]
    [Trait("Category", "E2E")]
    public void ComprehensiveProxyStub_AllTypePatterns_RoundTrips()
    {
        Assert.True(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
            "In-proc COM test requires Windows");

        var msvc = E2ETestHelper.FindMsvc();
        Assert.NotNull(msvc);

        var repoRoot = E2ETestHelper.FindRepoRoot();
        var testDir = Path.Combine(repoRoot, "tests", "e2e-custom-marshal", "comprehensive");
        using var helper = new E2ETestHelper(testDir, msvc!);

        // ── Step 1: Generate code from IDL ──────────────────────────────
        helper.GenerateFromIdl(
            Path.Combine(testDir, "TestComprehensive.idl"),
            "TestComprehensive.h",
            "TestComprehensive_i.c",
            "TestComprehensive_proxy.cpp",
            arraysEnabled: true);

        foreach (var f in new[] { "TestComprehensive.h", "TestComprehensive_i.c",
                                   "TestComprehensive_proxy.cpp" })
            Assert.True(File.Exists(Path.Combine(helper.GenDir, f)),
                $"Generated file not found: {f}");

        // Extract PSFactory CLSID from generated proxy
        var clsidPS = E2ETestHelper.ExtractClsidPSFactory(
            Path.Combine(helper.GenDir, "TestComprehensive_proxy.cpp"));
        Assert.NotNull(clsidPS);

        // Extract all IIDs from generated code
        var iids = E2ETestHelper.ExtractIids(
            Path.Combine(helper.GenDir, "TestComprehensive_i.c"));
        Assert.NotEmpty(iids);

        // ── Step 2: Compile ─────────────────────────────────────────────
        var cppFlags = E2ETestHelper.DefaultCppFlags.ToList();
        cppFlags.Add("/DWITH_ARRAYS");
        var cppFlagsArr = cppFlags.ToArray();
        var cFlags = E2ETestHelper.DefaultCFlags;

        // 2a: Compile IID definitions (as C — no /TP)
        var iidObj = Path.Combine(helper.BuildDir, "TestComprehensive_iid.obj");
        helper.CompileObj(
            Path.Combine(helper.GenDir, "TestComprehensive_i.c"),
            iidObj,
            [helper.GenDir],
            cFlags);

        // 2b: Compile & link server DLL
        var serverObj = Path.Combine(helper.BuildDir, "ComprehensiveServer.obj");
        helper.CompileObj(
            Path.Combine(testDir, "ComprehensiveServer.cpp"),
            serverObj,
            [testDir],
            cppFlagsArr);

        var serverDll = Path.Combine(helper.BuildDir, "ComprehensiveServer.dll");
        helper.LinkDll(serverDll,
            Path.Combine(testDir, "ComprehensiveServer.def"),
            [serverObj, iidObj],
            E2ETestHelper.DefaultLibs);

        // 2c: Compile & link proxy DLL
        var proxyObj = Path.Combine(helper.BuildDir, "TestComprehensive_proxy.obj");
        helper.CompileObj(
            Path.Combine(helper.GenDir, "TestComprehensive_proxy.cpp"),
            proxyObj,
            [helper.GenDir, Path.Combine(helper.RepoRoot, "include")],
            cppFlagsArr);

        var proxyDll = Path.Combine(helper.BuildDir, "TestComprehensiveProxy.dll");
        helper.LinkDll(proxyDll,
            Path.Combine(testDir, "TestComprehensiveProxy.def"),
            [proxyObj],
            E2ETestHelper.DefaultLibs);

        // 2d: Compile & link test client
        var clientObj = Path.Combine(helper.BuildDir, "ComprehensiveClient.obj");
        helper.CompileObj(
            Path.Combine(testDir, "ComprehensiveClient.cpp"),
            clientObj,
            [testDir],
            cppFlagsArr);

        var clientExe = Path.Combine(helper.BuildDir, "ComprehensiveClient.exe");
        helper.LinkExe(clientExe,
            [clientObj, iidObj],
            ["ole32.lib", "oleaut32.lib", "user32.lib", "runtimeobject.lib"]);

        // ── Step 3: Register COM classes (HKCU) ─────────────────────────
        var serverDllFull = Path.GetFullPath(serverDll);
        var proxyDllFull = Path.GetFullPath(proxyDll);

        foreach (var clsid in ClsidMap.Keys)
            helper.RegisterInprocServer(clsid, serverDllFull, "Apartment");

        helper.RegisterInprocServer("{D6E7F8A9-B0C1-42D3-E4F5-A6B7C8D9EAFB}", serverDllFull, "Apartment");

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
        E2ETestHelper.AssertTestResults(result.StdOut, _output);
    }
}
