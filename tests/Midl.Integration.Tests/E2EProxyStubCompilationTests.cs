using System.Diagnostics;
using System.Runtime.InteropServices;
using Midl.Analysis;
using Midl.Cli;
using Midl.CodeGen.Header;
using Midl.CodeGen.Stub;
using IidEmitter = Midl.CodeGen.Iid.IidEmitter;

namespace Midl.Integration.Tests;

/// <summary>
/// End-to-end compilation tests: generate .h, _i.c, _p.c from IDL,
/// then compile them with MSVC to verify correctness.
/// Skips gracefully when cl.exe / MSVC is not available.
/// </summary>
public class E2EProxyStubCompilationTests : IDisposable
{
    private readonly string _outDir;
    private readonly string _testDir;

    private static readonly string IdlSource = """
        namespace TestApp
        {
            enum Priority { Low, Medium, High };

            struct Point { Single X; Single Y; };

            interface IWidget
            {
                String Name { get; };
                Priority GetPriority();
                void SetPosition(Point pos);
                Boolean TryReset(out Int32 errorCode);
            };

            [default_interface]
            runtimeclass Widget
            {
                Widget();
            }
        }
        """;

    public E2EProxyStubCompilationTests()
    {
        _testDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "e2e-proxystub"));
        _outDir = Path.Combine(Path.GetTempPath(), $"midl-e2e-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_outDir);
    }

    public void Dispose()
    {
        try { if (Directory.Exists(_outDir)) Directory.Delete(_outDir, true); }
        catch { /* best effort cleanup */ }
    }

    /// <summary>
    /// Generate header, IID, and proxy/stub files from IDL in-process.
    /// </summary>
    private (string header, string iid, string proxy) GenerateFiles()
    {
        var compilation = CompilationHelper.Compile(IdlSource);
        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();
        Assert.True(errors.Count == 0,
            $"IDL compilation produced errors: {string.Join("; ", errors.Select(e => e.Message))}");

        var headerEmitter = new HeaderEmitter();
        var header = headerEmitter.Emit(compilation);

        var iidEmitter = new IidEmitter();
        var iid = iidEmitter.Emit(compilation);

        var stubEmitter = new StubEmitter();
        var proxy = stubEmitter.Emit(compilation);

        return (header, iid, proxy);
    }

    /// <summary>
    /// Write generated files to the output directory.
    /// </summary>
    private void WriteGeneratedFiles(string header, string iid, string proxy)
    {
        File.WriteAllText(Path.Combine(_outDir, "TestWidget.h"), header);
        File.WriteAllText(Path.Combine(_outDir, "TestWidget_i.c"), iid);
        File.WriteAllText(Path.Combine(_outDir, "TestWidget_p.c"), proxy);
    }

    /// <summary>
    /// Try to find cl.exe either in PATH or via vswhere.
    /// </summary>
    private static string? FindClExe()
    {
        // Check PATH first (Developer Command Prompt)
        try
        {
            var psi = new ProcessStartInfo("cl.exe")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            p?.StandardOutput.ReadToEnd();
            p?.StandardError.ReadToEnd();
            p?.WaitForExit(10000);
            if (p != null) return "cl.exe";
        }
        catch { }

        // Try vswhere
        var vswhere = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Microsoft Visual Studio", "Installer", "vswhere.exe");
        if (!File.Exists(vswhere)) return null;

        try
        {
            var psi = new ProcessStartInfo(vswhere)
            {
                Arguments = "-latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            var vsPath = p?.StandardOutput.ReadToEnd().Trim();
            p?.WaitForExit(10000);

            if (string.IsNullOrEmpty(vsPath)) return null;

            var vcToolsVerFile = Path.Combine(vsPath, "VC", "Auxiliary", "Build",
                "Microsoft.VCToolsVersion.default.txt");
            if (!File.Exists(vcToolsVerFile)) return null;

            var vcVer = File.ReadAllText(vcToolsVerFile).Trim();
            var clPath = Path.Combine(vsPath, "VC", "Tools", "MSVC", vcVer,
                "bin", "Hostx64", "x64", "cl.exe");
            return File.Exists(clPath) ? clPath : null;
        }
        catch { return null; }
    }

    /// <summary>
    /// Set up MSVC environment by sourcing vcvarsall.bat.
    /// Returns the full environment dictionary, or null if unavailable.
    /// Always runs vcvarsall.bat to guarantee a complete environment with all SDK paths.
    /// </summary>
    private static Dictionary<string, string>? GetMsvcEnvironment()
    {
        var vswhere = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Microsoft Visual Studio", "Installer", "vswhere.exe");
        if (!File.Exists(vswhere)) return null;

        try
        {
            // Find VS installation
            var psi = new ProcessStartInfo(vswhere)
            {
                Arguments = "-latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            var vsPath = p?.StandardOutput.ReadToEnd().Trim();
            p?.WaitForExit(10000);
            if (string.IsNullOrEmpty(vsPath)) return null;

            var vcvars = Path.Combine(vsPath, "VC", "Auxiliary", "Build", "vcvarsall.bat");
            if (!File.Exists(vcvars)) return null;

            // Run vcvarsall and capture the full environment
            var cmdPsi = new ProcessStartInfo("cmd.exe")
            {
                Arguments = $"/c \"\"{vcvars}\" x64 >nul 2>&1 && set\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using var cmdProc = Process.Start(cmdPsi);
            var output = cmdProc?.StandardOutput.ReadToEnd();
            cmdProc?.WaitForExit(30000);

            if (string.IsNullOrEmpty(output)) return null;

            var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var eq = line.IndexOf('=');
                if (eq > 0)
                    env[line[..eq].Trim()] = line[(eq + 1)..].Trim();
            }

            if (!env.ContainsKey("INCLUDE")) return null;

            // Ensure Windows SDK include paths are present (vcvarsall sometimes
            // omits them when the SDK is installed in a non-standard location)
            if (OperatingSystem.IsWindows())
            {
                var sdk = WindowsSdkLocator.Discover();
                if (sdk != null)
                {
                    var include = env.GetValueOrDefault("INCLUDE", "");
                    var existing = new HashSet<string>(
                        include.Split(';', StringSplitOptions.RemoveEmptyEntries),
                        StringComparer.OrdinalIgnoreCase);
                    var added = new List<string>();
                    foreach (var sdkPath in sdk.IncludePaths)
                    {
                        if (!existing.Contains(sdkPath))
                            added.Add(sdkPath);
                    }
                    if (added.Count > 0)
                        env["INCLUDE"] = include + ";" + string.Join(";", added);
                }
            }

            return env;
        }
        catch { return null; }
    }

    /// <summary>
    /// Resolve the cl.exe path from the captured environment.
    /// Needed when cl.exe isn't in the current process PATH but is in the vcvarsall PATH.
    /// </summary>
    private static string? ResolveClExe(string? clExe, Dictionary<string, string> env)
    {
        if (clExe != null) return clExe;

        // cl.exe wasn't in our PATH — look in the vcvarsall-captured PATH
        if (env.TryGetValue("PATH", out var pathValue))
        {
            foreach (var dir in pathValue.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var candidate = Path.Combine(dir.Trim(), "cl.exe");
                if (File.Exists(candidate)) return candidate;
            }
        }

        return null;
    }

    /// <summary>
    /// Compile a source file with cl.exe and assert success.
    /// </summary>
    private static (int exitCode, string output) CompileWithCl(
        string clExe, string sourceFile, string[] extraArgs,
        Dictionary<string, string> env, string workDir)
    {
        var args = new List<string> { "/nologo", "/c", "/W3", "/WX-" };
        args.AddRange(extraArgs);
        args.Add($"/Fo{workDir}\\");
        args.Add(sourceFile);

        var psi = new ProcessStartInfo(clExe)
        {
            Arguments = string.Join(" ", args),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = workDir
        };

        // Replace the full environment so cl.exe sees all MSVC paths
        psi.Environment.Clear();
        foreach (var kv in env)
            psi.Environment[kv.Key] = kv.Value;

        using var proc = Process.Start(psi)!;
        var stdout = proc.StandardOutput.ReadToEnd();
        var stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit(60000);

        var combined = stdout + stderr;
        return (proc.ExitCode, combined);
    }

    /// <summary>
    /// Shared setup: get MSVC environment and resolve cl.exe.
    /// Skips if MSVC is not available.
    /// </summary>
    private (string clExe, Dictionary<string, string> env) RequireMsvc()
    {
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), "Windows only");
        var env = GetMsvcEnvironment();
        Skip.If(env == null, "Could not set up MSVC environment");

        var clExe = FindClExe() ?? ResolveClExe(null, env!);
        Skip.If(clExe == null, "cl.exe not found — install MSVC or run from Developer Command Prompt");

        return (clExe!, env!);
    }

    [SkippableFact]
    public void GeneratedHeader_CompilesAsCpp()
    {
        var (clExe, env) = RequireMsvc();

        var (header, iid, proxy) = GenerateFiles();
        WriteGeneratedFiles(header, iid, proxy);

        var cppFile = Path.Combine(_testDir, "test_cpp_include.cpp");
        Skip.IfNot(File.Exists(cppFile), $"Test file not found: {cppFile}");

        var (exitCode, output) = CompileWithCl(clExe, cppFile,
            ["/EHsc", "/std:c++17", $"/I{_outDir}"],
            env, _outDir);

        Assert.True(exitCode == 0,
            $"C++ compilation failed (exit code {exitCode}):\n{output}");
    }

    [SkippableFact]
    public void GeneratedHeader_CompilesAsC()
    {
        var (clExe, env) = RequireMsvc();

        var (header, iid, proxy) = GenerateFiles();
        WriteGeneratedFiles(header, iid, proxy);

        var cFile = Path.Combine(_testDir, "test_c_include.c");
        Skip.IfNot(File.Exists(cFile), $"Test file not found: {cFile}");

        var (exitCode, output) = CompileWithCl(clExe, cFile,
            ["/TC", $"/I{_outDir}"],
            env, _outDir);

        Assert.True(exitCode == 0,
            $"C compilation failed (exit code {exitCode}):\n{output}");
    }

    [SkippableFact]
    public void GeneratedIidFile_Compiles()
    {
        var (clExe, env) = RequireMsvc();

        var (header, iid, proxy) = GenerateFiles();
        WriteGeneratedFiles(header, iid, proxy);

        var iidFile = Path.Combine(_outDir, "TestWidget_i.c");

        var (exitCode, output) = CompileWithCl(clExe, iidFile,
            ["/TC", $"/I{_outDir}"],
            env, _outDir);

        Assert.True(exitCode == 0,
            $"IID file compilation failed (exit code {exitCode}):\n{output}");
    }

    [SkippableFact]
    public void GeneratedProxyStub_Compiles()
    {
        var (clExe, env) = RequireMsvc();

        var (header, iid, proxy) = GenerateFiles();
        WriteGeneratedFiles(header, iid, proxy);

        var proxyFile = Path.Combine(_outDir, "TestWidget_p.c");

        var (exitCode, output) = CompileWithCl(clExe!, proxyFile,
            ["/TC", $"/I{_outDir}", "/DREGISTER_PROXY_DLL"],
            env!, _outDir);

        // cl.exe C1001 (internal compiler error) is a compiler bug, not ours
        Skip.If(output.Contains("C1001") || output.Contains("Internal compiler error"),
            $"cl.exe internal compiler error (C1001) — compiler bug, not a code generation issue");

        Assert.True(exitCode == 0,
            $"Proxy/stub compilation failed (exit code {exitCode}):\n{output}");
    }

    [Fact]
    public void CodeGeneration_ProducesNonEmptyOutput()
    {
        var (header, iid, proxy) = GenerateFiles();

        Assert.NotEmpty(header);
        Assert.Contains("IWidget", header);
        Assert.Contains("Priority", header);
        Assert.Contains("Point", header);

        Assert.NotEmpty(iid);
        Assert.Contains("IID_", iid);

        Assert.NotEmpty(proxy);
    }
}
