// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

    /// <summary>Reason the tool could not be produced (set when <see cref="EnsureBuilt"/> returns null).</summary>
    public static string? LastError { get; private set; }

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
        {
            LastError = $"CMakeLists.txt not found under {toolDir}.";
            return null;
        }
        var buildDir = Path.Combine(toolDir, "build");
        if (!RunCMake($"-S \"{toolDir}\" -B \"{buildDir}\"", out var configError))
        {
            LastError = $"CMake configure failed (CMake, MSVC, or network may be unavailable): {configError}";
            return null;
        }
        if (!RunCMake($"--build \"{buildDir}\" --config Release", out var buildError))
        {
            LastError = $"CMake build failed: {buildError}";
            return null;
        }
        var path = Locate();
        if (path is null)
            LastError = "winmd-dump.exe was not found after a successful CMake build.";
        return path;
    }

    private static bool RunCMake(string arguments, out string error)
    {
        error = "";
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
            {
                error = "could not start cmake.";
                return false;
            }
            var stdout = proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            if (proc.ExitCode == 0)
                return true;
            error = Summarize(string.IsNullOrWhiteSpace(stderr) ? stdout : stderr);
            return false;
        }
        catch (Exception ex)
        {
            error = ex.Message; // e.g. cmake not found on PATH
            return false;
        }
    }

    private static string Summarize(string text)
    {
        text = text.Trim();
        const int max = 300;
        return text.Length <= max ? text : text[^max..];
    }
}
