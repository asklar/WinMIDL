using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Midl.Cli;

namespace Midl.E2E.Tests;

/// <summary>
/// Represents a discovered MSVC toolchain (compiler, linker, SDK paths).
/// </summary>
public record MsvcToolchain(
    string ClExe,
    string LinkExe,
    string VcInclude,
    string VcLib,
    string SdkIncludeUcrt,
    string SdkIncludeShared,
    string SdkIncludeUm,
    string SdkIncludeWinrt,
    string SdkLibUcrt,
    string SdkLibUm);

/// <summary>
/// Helper for E2E proxy/stub tests. Handles MSVC discovery, compilation,
/// COM registration, and process management — entirely in C#, no PowerShell.
/// </summary>
[SupportedOSPlatform("windows")]
public class E2ETestHelper : IDisposable
{
    public string TestDir { get; }
    public string GenDir { get; }
    public string BuildDir { get; }
    public string RepoRoot { get; }
    public MsvcToolchain Msvc { get; }

    private readonly List<string> _registeredClsids = new();
    private readonly List<string> _registeredIids = new();
    private Process? _serverProcess;

    public E2ETestHelper(string testDir, MsvcToolchain msvc)
    {
        TestDir = testDir;
        Msvc = msvc;
        RepoRoot = FindRepoRoot();
        GenDir = Path.Combine(testDir, "generated");
        BuildDir = Path.Combine(testDir, "build");

        // Clean and recreate build directories for a fresh run
        if (Directory.Exists(GenDir))
            Directory.Delete(GenDir, true);
        if (Directory.Exists(BuildDir))
            Directory.Delete(BuildDir, true);
        Directory.CreateDirectory(GenDir);
        Directory.CreateDirectory(BuildDir);
    }

    // ── Standard flag/lib sets ──────────────────────────────────────────

    public static string[] DefaultCppFlags =>
    [
        "/nologo", "/std:c++latest", "/EHsc", "/W3",
        "/DWIN32", "/D_WINDOWS", "/DUNICODE", "/D_UNICODE", "/Zi"
    ];

    public static string[] DefaultCFlags =>
    [
        "/nologo", "/W3",
        "/DWIN32", "/D_WINDOWS", "/DUNICODE", "/D_UNICODE"
    ];

    public static string[] DefaultLibs =>
    [
        "ole32.lib", "oleaut32.lib", "advapi32.lib",
        "user32.lib", "runtimeobject.lib"
    ];

    // ── MSVC Discovery ──────────────────────────────────────────────────

    /// <summary>
    /// Find the MSVC toolchain via vswhere. Returns null if not installed.
    /// </summary>
    public static MsvcToolchain? FindMsvc()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        // Find vswhere.exe
        var vswhere = Path.Combine(pf86, "Microsoft Visual Studio", "Installer", "vswhere.exe");
        if (!File.Exists(vswhere))
            vswhere = Path.Combine(pf, "Microsoft Visual Studio", "Installer", "vswhere.exe");
        if (!File.Exists(vswhere))
            return null;

        // Run vswhere to get VS install path
        var vsPath = RunProcessCapture(vswhere,
            "-latest -property installationPath -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64");
        if (string.IsNullOrWhiteSpace(vsPath))
            return null;
        vsPath = vsPath.Trim();

        // Get MSVC tools version
        var versionFile = Path.Combine(vsPath, "VC", "Auxiliary", "Build",
            "Microsoft.VCToolsVersion.default.txt");
        if (!File.Exists(versionFile))
            return null;
        var vcToolsVersion = File.ReadAllText(versionFile).Trim();

        var vcToolsDir = Path.Combine(vsPath, "VC", "Tools", "MSVC", vcToolsVersion);
        var cl = Path.Combine(vcToolsDir, "bin", "Hostx64", "x64", "cl.exe");
        var link = Path.Combine(vcToolsDir, "bin", "Hostx64", "x64", "link.exe");
        if (!File.Exists(cl))
            return null;

        // Find Windows SDK
        string? sdkRoot = null;
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows Kits\Installed Roots");
            sdkRoot = key?.GetValue("KitsRoot10") as string;
        }
        catch { /* registry not accessible */ }

        if (string.IsNullOrEmpty(sdkRoot))
            sdkRoot = Path.Combine(pf86, "Windows Kits", "10");
        if (!Directory.Exists(sdkRoot))
            return null;

        var includeDir = Path.Combine(sdkRoot, "Include");
        if (!Directory.Exists(includeDir))
            return null;

        var sdkVersion = Directory.GetDirectories(includeDir)
            .Select(Path.GetFileName)
            .Where(n => n != null && Regex.IsMatch(n, @"^\d+\.\d+"))
            .OrderByDescending(n => n)
            .FirstOrDefault();
        if (sdkVersion == null)
            return null;

        return new MsvcToolchain(
            cl, link,
            Path.Combine(vcToolsDir, "include"),
            Path.Combine(vcToolsDir, "lib", "x64"),
            Path.Combine(sdkRoot, "Include", sdkVersion, "ucrt"),
            Path.Combine(sdkRoot, "Include", sdkVersion, "shared"),
            Path.Combine(sdkRoot, "Include", sdkVersion, "um"),
            Path.Combine(sdkRoot, "Include", sdkVersion, "winrt"),
            Path.Combine(sdkRoot, "Lib", sdkVersion, "ucrt", "x64"),
            Path.Combine(sdkRoot, "Lib", sdkVersion, "um", "x64"));
    }

    // ── Code Generation ─────────────────────────────────────────────────

    /// <summary>
    /// Generate header, IID, and optional C++ proxy code from IDL via CompilationDriver.
    /// </summary>
    public void GenerateFromIdl(string idlPath, string headerName, string iidName,
        string? proxyName = null)
    {
        var options = new CommandLineOptions
        {
            InputFile = idlPath,
            OutputDirectory = GenDir,
            HeaderFile = headerName,
            IidFile = iidName,
            WinRtMode = true,
            NoLogo = true,
            NoProxy = true,
            NoDllData = true,
        };
        if (proxyName != null)
            options.CppProxyFile = proxyName;

        var driver = new CompilationDriver(TextWriter.Null, TextWriter.Null);
        var exitCode = driver.Run(options);

        // Exit code 1 = warnings only, acceptable
        if (exitCode > 1)
            throw new InvalidOperationException(
                $"CompilationDriver failed with exit code {exitCode} for {Path.GetFileName(idlPath)}");
    }

    // ── Compilation ─────────────────────────────────────────────────────

    /// <summary>
    /// Compile a C/C++ source file to an object file using cl.exe.
    /// </summary>
    public void CompileObj(string sourcePath, string objPath,
        string[] includes, string[]? flags = null)
    {
        flags ??= DefaultCppFlags;
        var args = new StringBuilder();
        foreach (var f in flags)
            args.Append(f).Append(' ');
        args.Append("/c ");
        args.Append($"/Fo\"{objPath}\" ");
        foreach (var inc in includes)
            args.Append($"/I\"{inc}\" ");
        args.Append($"\"{sourcePath}\"");

        var result = RunMsvcTool(Msvc.ClExe, args.ToString());
        if (result.ExitCode != 0)
            throw new InvalidOperationException(
                $"Compilation failed for {Path.GetFileName(sourcePath)}:\n" +
                $"{result.StdOut}\n{result.StdErr}");
    }

    /// <summary>
    /// Link object files into a DLL.
    /// </summary>
    public void LinkDll(string dllPath, string defFile,
        string[] objFiles, string[] libs)
    {
        var args = new StringBuilder();
        args.Append($"/nologo /DLL /DEBUG /OUT:\"{dllPath}\" ");
        args.Append($"/DEF:\"{defFile}\" ");
        foreach (var obj in objFiles)
            args.Append($"\"{obj}\" ");
        foreach (var lib in libs)
            args.Append(lib).Append(' ');

        var result = RunMsvcTool(Msvc.LinkExe, args.ToString());
        if (result.ExitCode != 0)
            throw new InvalidOperationException(
                $"DLL link failed for {Path.GetFileName(dllPath)}:\n" +
                $"{result.StdOut}\n{result.StdErr}");
    }

    /// <summary>
    /// Link object files into an EXE.
    /// </summary>
    public void LinkExe(string exePath, string[] objFiles, string[] libs)
    {
        var args = new StringBuilder();
        args.Append($"/nologo /DEBUG /SUBSYSTEM:CONSOLE /OUT:\"{exePath}\" ");
        foreach (var obj in objFiles)
            args.Append($"\"{obj}\" ");
        foreach (var lib in libs)
            args.Append(lib).Append(' ');

        var result = RunMsvcTool(Msvc.LinkExe, args.ToString());
        if (result.ExitCode != 0)
            throw new InvalidOperationException(
                $"EXE link failed for {Path.GetFileName(exePath)}:\n" +
                $"{result.StdOut}\n{result.StdErr}");
    }

    // ── COM Registration ────────────────────────────────────────────────

    /// <summary>
    /// Register a CLSID for InprocServer32 in HKCU (no admin required).
    /// </summary>
    public void RegisterInprocServer(string clsid, string dllPath,
        string? threadingModel = null)
    {
        var keyPath = $@"Software\Classes\CLSID\{clsid}\InprocServer32";
        using var key = Registry.CurrentUser.CreateSubKey(keyPath);
        key.SetValue(null, dllPath);
        if (threadingModel != null)
            key.SetValue("ThreadingModel", threadingModel);
        if (!_registeredClsids.Contains(clsid))
            _registeredClsids.Add(clsid);
    }

    /// <summary>
    /// Register a CLSID for LocalServer32 in HKCU (no admin required).
    /// </summary>
    public void RegisterLocalServer(string clsid, string exePath)
    {
        var keyPath = $@"Software\Classes\CLSID\{clsid}\LocalServer32";
        using var key = Registry.CurrentUser.CreateSubKey(keyPath);
        key.SetValue(null, exePath);
        if (!_registeredClsids.Contains(clsid))
            _registeredClsids.Add(clsid);
    }

    /// <summary>
    /// Register an IID → ProxyStubClsid32 mapping in HKCU.
    /// </summary>
    public void RegisterProxyStub(string iid, string psClsid)
    {
        var keyPath = $@"Software\Classes\Interface\{iid}\ProxyStubClsid32";
        using var key = Registry.CurrentUser.CreateSubKey(keyPath);
        key.SetValue(null, psClsid);
        if (!_registeredIids.Contains(iid))
            _registeredIids.Add(iid);
    }

    // ── Process Management ──────────────────────────────────────────────

    /// <summary>
    /// Start a server process and wait for it to initialize.
    /// </summary>
    public void StartServer(string exePath, int waitMs = 3000)
    {
        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        _serverProcess = Process.Start(psi)
            ?? throw new InvalidOperationException($"Failed to start server: {exePath}");

        // Give the server time to register its class factories
        Thread.Sleep(waitMs);

        if (_serverProcess.HasExited)
            throw new InvalidOperationException(
                $"Server exited prematurely with code {_serverProcess.ExitCode}");
    }

    /// <summary>
    /// Run a client process and capture its output.
    /// </summary>
    public (int ExitCode, string StdOut, string StdErr) RunClient(
        string exePath, int timeoutMs = 60_000)
    {
        using var proc = new Process();
        proc.StartInfo = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        proc.Start();

        // Read stdout/stderr asynchronously to avoid deadlocks
        var stdoutTask = proc.StandardOutput.ReadToEndAsync();
        var stderrTask = proc.StandardError.ReadToEndAsync();

        if (!proc.WaitForExit(timeoutMs))
        {
            proc.Kill(entireProcessTree: true);
            return (-1,
                stdoutTask.GetAwaiter().GetResult(),
                stderrTask.GetAwaiter().GetResult() + "\n[TIMEOUT]");
        }

        return (proc.ExitCode,
            stdoutTask.GetAwaiter().GetResult(),
            stderrTask.GetAwaiter().GetResult());
    }

    // ── GUID Extraction ─────────────────────────────────────────────────

    /// <summary>
    /// Extract all IID definitions from a generated _i.c file.
    /// Returns a dictionary mapping symbolic name → registry-format GUID string.
    /// </summary>
    public static Dictionary<string, string> ExtractIids(string iidFilePath)
    {
        var content = File.ReadAllText(iidFilePath);
        var iids = new Dictionary<string, string>();

        var regex = new Regex(
            @"MIDL_DEFINE_GUID\(\s*IID\s*,\s*(\w+)\s*," +
            @"\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*," +
            @"\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*," +
            @"\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*," +
            @"\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*," +
            @"\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*\)");

        foreach (Match match in regex.Matches(content))
        {
            var name = match.Groups[1].Value;
            var parts = Enumerable.Range(2, 11)
                .Select(i => match.Groups[i].Value)
                .ToArray();
            iids[name] = FormatGuid(parts);
        }

        return iids;
    }

    /// <summary>
    /// Extract CLSID_PSFactoryBuffer from a generated proxy source file.
    /// </summary>
    public static string? ExtractClsidPSFactory(string proxyFilePath)
    {
        var content = File.ReadAllText(proxyFilePath);
        var regex = new Regex(
            @"CLSID_PSFactoryBuffer\s*=\s*\{\s*" +
            @"0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*\{\s*" +
            @"0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*" +
            @"0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*" +
            @"0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*" +
            @"0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)");

        var match = regex.Match(content);
        if (!match.Success) return null;

        var parts = Enumerable.Range(1, 11)
            .Select(i => match.Groups[i].Value)
            .ToArray();
        return FormatGuid(parts);
    }

    /// <summary>
    /// Extract a named CLSID constant from a C++ source file.
    /// </summary>
    public static string? ExtractClsid(string sourceFilePath, string clsidName)
    {
        var content = File.ReadAllText(sourceFilePath);
        var regex = new Regex(
            Regex.Escape(clsidName) + @"\s*=\s*\{\s*" +
            @"0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*\{\s*" +
            @"0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*" +
            @"0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*" +
            @"0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*" +
            @"0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)");

        var match = regex.Match(content);
        if (!match.Success) return null;

        var parts = Enumerable.Range(1, 11)
            .Select(i => match.Groups[i].Value)
            .ToArray();
        return FormatGuid(parts);
    }

    // ── Repo Root ───────────────────────────────────────────────────────

    public static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "Midl.slnx")) ||
                File.Exists(Path.Combine(dir, "Directory.Build.props")))
                return dir;
            dir = Path.GetDirectoryName(dir);
        }
        dir = Directory.GetCurrentDirectory();
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "Midl.slnx")) ||
                File.Exists(Path.Combine(dir, "Directory.Build.props")))
                return dir;
            dir = Path.GetDirectoryName(dir);
        }
        return Directory.GetCurrentDirectory();
    }

    // ── Cleanup ─────────────────────────────────────────────────────────

    public void Dispose()
    {
        // Kill server process
        if (_serverProcess != null)
        {
            try
            {
                if (!_serverProcess.HasExited)
                    _serverProcess.Kill(entireProcessTree: true);
            }
            catch { }
            _serverProcess.Dispose();
            _serverProcess = null;
        }

        // Unregister CLSIDs
        foreach (var clsid in _registeredClsids)
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(
                    $@"Software\Classes\CLSID\{clsid}", throwOnMissingSubKey: false);
            }
            catch { }
        }

        // Unregister IIDs
        foreach (var iid in _registeredIids)
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(
                    $@"Software\Classes\Interface\{iid}", throwOnMissingSubKey: false);
            }
            catch { }
        }
    }

    // ── Private Helpers ─────────────────────────────────────────────────

    /// <summary>
    /// Convert 11 hex parts (d1, d2, d3, b1–b8) into a registry-format GUID string.
    /// </summary>
    private static string FormatGuid(string[] parts)
    {
        var d1 = parts[0].PadLeft(8, '0');
        var d2 = parts[1].PadLeft(4, '0');
        var d3 = parts[2].PadLeft(4, '0');
        var b = parts[3..11].Select(p => p.PadLeft(2, '0')).ToArray();
        return $"{{{d1}-{d2}-{d3}-{b[0]}{b[1]}-{b[2]}{b[3]}{b[4]}{b[5]}{b[6]}{b[7]}}}".ToUpper();
    }

    /// <summary>
    /// Run an MSVC tool (cl.exe or link.exe) with proper INCLUDE/LIB env vars.
    /// </summary>
    private (int ExitCode, string StdOut, string StdErr) RunMsvcTool(
        string fileName, string arguments)
    {
        using var proc = new Process();
        proc.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = BuildDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        // Set MSVC environment so cl/link find system headers and libs
        proc.StartInfo.Environment["INCLUDE"] = string.Join(";",
            Msvc.VcInclude, Msvc.SdkIncludeUcrt, Msvc.SdkIncludeShared,
            Msvc.SdkIncludeUm, Msvc.SdkIncludeWinrt);
        proc.StartInfo.Environment["LIB"] = string.Join(";",
            Msvc.VcLib, Msvc.SdkLibUcrt, Msvc.SdkLibUm);

        proc.Start();

        var stdoutTask = proc.StandardOutput.ReadToEndAsync();
        var stderrTask = proc.StandardError.ReadToEndAsync();

        if (!proc.WaitForExit(120_000))
        {
            proc.Kill(entireProcessTree: true);
            throw new TimeoutException(
                $"MSVC tool timed out: {Path.GetFileName(fileName)}");
        }

        return (proc.ExitCode,
            stdoutTask.GetAwaiter().GetResult(),
            stderrTask.GetAwaiter().GetResult());
    }

    /// <summary>
    /// Run a process and capture stdout (used for vswhere).
    /// </summary>
    private static string RunProcessCapture(string fileName, string arguments)
    {
        using var proc = new Process();
        proc.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        proc.Start();
        var output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit(30_000);
        return output;
    }

    // ── Test Result Parsing ─────────────────────────────────────────────

    /// <summary>
    /// Represents a single sub-test result parsed from C++ client output.
    /// </summary>
    public record TestCaseResult(string Name, string Status);

    /// <summary>
    /// Parses PASS:/FAIL:/SKIP: lines from C++ client stdout into individual results.
    /// </summary>
    public static List<TestCaseResult> ParseTestResults(string stdout)
    {
        var results = new List<TestCaseResult>();
        if (string.IsNullOrEmpty(stdout)) return results;

        foreach (var line in stdout.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("PASS:", StringComparison.Ordinal))
                results.Add(new TestCaseResult(trimmed["PASS:".Length..].Trim(), "PASS"));
            else if (trimmed.StartsWith("FAIL:", StringComparison.Ordinal))
                results.Add(new TestCaseResult(trimmed["FAIL:".Length..].Trim(), "FAIL"));
            else if (trimmed.StartsWith("SKIP:", StringComparison.Ordinal))
                results.Add(new TestCaseResult(trimmed["SKIP:".Length..].Trim(), "SKIP"));
        }
        return results;
    }

    /// <summary>
    /// Asserts all parsed test results, reporting each sub-test individually via ITestOutputHelper.
    /// Returns (passed, failed, skipped) counts.
    /// </summary>
    public static (int passed, int failed, int skipped) AssertTestResults(
        string stdout, Xunit.Abstractions.ITestOutputHelper output)
    {
        var results = ParseTestResults(stdout);
        int passed = 0, failed = 0, skipped = 0;
        var failures = new List<string>();

        foreach (var r in results)
        {
            switch (r.Status)
            {
                case "PASS":
                    passed++;
                    output.WriteLine($"  ✓ {r.Name}");
                    break;
                case "FAIL":
                    failed++;
                    output.WriteLine($"  ✗ {r.Name}");
                    failures.Add(r.Name);
                    break;
                case "SKIP":
                    skipped++;
                    output.WriteLine($"  ⊘ {r.Name} (skipped)");
                    break;
            }
        }

        output.WriteLine($"\n  {passed} passed, {failed} failed, {skipped} skipped ({results.Count} total)");

        if (failures.Count > 0)
            Assert.Fail($"{failures.Count} sub-test(s) failed:\n  " + string.Join("\n  ", failures));

        Assert.True(results.Count > 0, "No test results found in client output");
        return (passed, failed, skipped);
    }
}
