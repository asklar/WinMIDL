using System.Diagnostics;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Locates — or builds on demand via CMake — the C++ <c>winmd-dump</c> ground-truth tool
/// used by parity tests. The tool is intentionally not committed as a binary; it is built
/// from <c>tools/winmd-dump</c> the first time a test needs it.
///
/// Returns <c>null</c> when the tool cannot be produced (no CMake/MSVC toolchain or no
/// network to fetch the winmd dependency), so callers can skip gracefully.
/// </summary>
internal static class WinmdDumpTool
{
    private static readonly object Gate = new();
    private static string? _cached;
    private static bool _attempted;

    /// <summary>Returns the path to winmd-dump.exe, building it if necessary, or null if unavailable.</summary>
    public static string? EnsureBuilt()
    {
        lock (Gate)
        {
            if (_attempted)
                return _cached;
            _attempted = true;
            _cached = Locate() ?? Build();
            return _cached;
        }
    }

    private static string ToolDir() => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tools", "winmd-dump"));

    private static string? Locate()
    {
        var buildDir = Path.Combine(ToolDir(), "build");
        if (!Directory.Exists(buildDir))
            return null;
        return Directory.EnumerateFiles(buildDir, "winmd-dump.exe", SearchOption.AllDirectories)
            .FirstOrDefault();
    }

    private static string? Build()
    {
        var toolDir = ToolDir();
        if (!File.Exists(Path.Combine(toolDir, "CMakeLists.txt")))
            return null;
        var buildDir = Path.Combine(toolDir, "build");
        if (!RunCMake($"-S \"{toolDir}\" -B \"{buildDir}\""))
            return null;
        if (!RunCMake($"--build \"{buildDir}\" --config Release"))
            return null;
        return Locate();
    }

    private static bool RunCMake(string arguments)
    {
        try
        {
            var psi = new ProcessStartInfo("cmake", arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            using var proc = Process.Start(psi);
            if (proc is null)
                return false;
            proc.StandardOutput.ReadToEnd();
            proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            return proc.ExitCode == 0;
        }
        catch
        {
            return false; // cmake not on PATH, build failure, etc.
        }
    }
}
