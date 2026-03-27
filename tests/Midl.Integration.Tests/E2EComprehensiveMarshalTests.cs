using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Midl.Integration.Tests;

/// <summary>
/// End-to-end test validating the comprehensive proxy/stub codegen pipeline:
///   IDL → header + proxy code → compile server DLL + proxy DLL → cross-apartment calls.
///
/// Exercises all WinRT type patterns: enums, structs (including nested),
/// properties, out params, multiple interface QI, inheritance, GetIids,
/// and GetRuntimeClassName.
/// </summary>
public partial class E2EComprehensiveMarshalTests
{
    private static readonly string ScriptPath = Path.Combine(
        FindRepoRoot(), "tests", "e2e-custom-marshal", "comprehensive", "build-and-test.ps1");

    [SkippableFact]
    public void ComprehensiveProxyStub_AllTypePatterns_RoundTrips()
    {
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
            "In-proc COM test requires Windows");
        Skip.IfNot(File.Exists(ScriptPath),
            $"Build script not found: {ScriptPath}");
        Skip.IfNot(HasMsvcToolchain(),
            "MSVC toolchain not found (vswhere/cl.exe)");

        var psi = new ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{ScriptPath}\"",
            WorkingDirectory = Path.GetDirectoryName(ScriptPath)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        // Fall back to Windows PowerShell if pwsh isn't available
        if (!IsOnPath("pwsh"))
        {
            psi.FileName = "powershell";
        }

        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit(TimeSpan.FromMinutes(5));

        // Output for diagnostics
        if (stdout.Length > 0)
            Console.Write(stdout);
        if (stderr.Length > 0)
            Console.Error.Write(stderr);

        Assert.True(process.ExitCode == 0,
            $"build-and-test.ps1 failed with exit code {process.ExitCode}.\n" +
            $"stdout:\n{stdout}\nstderr:\n{stderr}");
        Assert.Contains("PASS:", stdout);
        Assert.DoesNotContain("FAIL:", stdout);
    }

    private static bool HasMsvcToolchain()
    {
        var vswhere = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Microsoft Visual Studio", "Installer", "vswhere.exe");
        if (!File.Exists(vswhere))
        {
            vswhere = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Microsoft Visual Studio", "Installer", "vswhere.exe");
        }
        return File.Exists(vswhere);
    }

    private static bool IsOnPath(string exe)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = Process.Start(psi);
            p?.WaitForExit(5000);
            return p?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "Midl.slnx")))
                return dir;
            dir = Path.GetDirectoryName(dir);
        }
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }
}
