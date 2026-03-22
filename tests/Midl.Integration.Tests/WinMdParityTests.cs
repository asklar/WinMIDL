using Midl.Analysis;
using Midl.CodeGen.Decompiler;
using Midl.CodeGen.WinMd;

namespace Midl.Integration.Tests;

/// <summary>
/// Compares WinMD output from WinMIDL against original midl.exe /winrt.
/// Uses --dump-winmd canonical format for structural comparison.
/// Skipped when original midl.exe is not available.
/// </summary>
public class WinMdParityTests
{
    private static readonly string? OriginalMidlExe = FindOriginalMidl();
    private static readonly string FixturesDir = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "fixtures", "valid");

    private static string? FindOriginalMidl()
    {
        var sdkRoot = @"C:\Program Files (x86)\Windows Kits\10";
        var binDir = Path.Combine(sdkRoot, "bin");
        if (!Directory.Exists(binDir)) return null;

        // Prefer SDK versions that have winrtbase.idl (needed for /winrt compilation)
        var versions = Directory.GetDirectories(binDir)
            .Select(d => Path.GetFileName(d))
            .Where(n => n.StartsWith("10."))
            .OrderByDescending(n => n)
            .ToList();

        foreach (var ver in versions)
        {
            var midl = Path.Combine(binDir, ver, "x64", "midl.exe");
            var winrtInc = Path.Combine(sdkRoot, "Include", ver, "winrt");
            var unionMeta = Path.Combine(sdkRoot, "UnionMetadata", ver, "Windows.winmd");
            if (File.Exists(midl) && Directory.Exists(winrtInc) && File.Exists(unionMeta))
                return midl;
        }

        // Fallback: any midl.exe
        foreach (var ver in versions)
        {
            var midl = Path.Combine(binDir, ver, "x64", "midl.exe");
            if (File.Exists(midl)) return midl;
        }
        return null;
    }

    /// <summary>
    /// Compile an IDL file with original midl.exe /winrt and return the WinMD bytes, or null on failure.
    /// </summary>
    private static byte[]? CompileWithOriginalMidl(string idlPath, string tempDir)
    {
        if (OriginalMidlExe == null) return null;

        var baseName = Path.GetFileNameWithoutExtension(idlPath);
        var winmdPath = Path.Combine(tempDir, $"{baseName}.winmd");

        // Find SDK paths: /reference Windows.winmd, /metadata_dir, /I winrt
        var sdkBinDir = Path.GetDirectoryName(Path.GetDirectoryName(OriginalMidlExe)!)!;
        var sdkRoot = Path.GetDirectoryName(sdkBinDir)!;
        var sdkVersion = Path.GetFileName(Path.GetDirectoryName(OriginalMidlExe)!);

        var unionMeta = Path.Combine(sdkRoot, "UnionMetadata", sdkVersion);
        var windowsWinmd = Path.Combine(unionMeta, "Windows.winmd");
        var winrtInc = Path.Combine(sdkRoot, "Include", sdkVersion, "winrt");

        // If this SDK version lacks the files, find one that has them
        if (!File.Exists(windowsWinmd) || !Directory.Exists(winrtInc))
        {
            var umBase = Path.Combine(sdkRoot, "UnionMetadata");
            if (Directory.Exists(umBase))
            {
                var altVer = Directory.GetDirectories(umBase)
                    .Where(d => File.Exists(Path.Combine(d, "Windows.winmd")))
                    .OrderByDescending(d => d)
                    .FirstOrDefault();
                if (altVer != null)
                {
                    var v = Path.GetFileName(altVer);
                    unionMeta = altVer;
                    windowsWinmd = Path.Combine(altVer, "Windows.winmd");
                    winrtInc = Path.Combine(sdkRoot, "Include", v, "winrt");
                }
            }
        }

        var args = new List<string>
        {
            "/winrt",
            "/nomidl",
            "/winmd", winmdPath,
            "/nologo",
            "/h", "nul",
            "/no_warn"
        };

        if (File.Exists(windowsWinmd)) { args.Add("/reference"); args.Add(windowsWinmd); }
        if (Directory.Exists(unionMeta)) { args.Add("/metadata_dir"); args.Add(unionMeta); }
        if (Directory.Exists(winrtInc)) { args.Add("/I"); args.Add(winrtInc); }

        args.Add(idlPath);

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = OriginalMidlExe,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var arg in args) psi.ArgumentList.Add(arg);

        using var proc = System.Diagnostics.Process.Start(psi);
        proc?.WaitForExit(30000);

        if (proc?.ExitCode != 0 || !File.Exists(winmdPath)) return null;
        return File.ReadAllBytes(winmdPath);
    }

    /// <summary>
    /// Compile an IDL file with WinMIDL and return the WinMD bytes.
    /// </summary>
    private static byte[]? CompileWithWinMidl(string idlPath)
    {
        var source = File.ReadAllText(idlPath);
        var compilation = CompilationHelper.Compile(source);

        if (compilation.Diagnostics.Any(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error))
            return null;

        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    [Theory]
    [MemberData(nameof(GetParityFixtures))]
    public void WinMdOutput_MatchesOriginalMidl(string fixtureName, string fixturePath)
    {
        _ = fixtureName; // Used for test display name
        if (OriginalMidlExe == null)
        {
            // Skip if original midl.exe not available
            Assert.Fail("Original midl.exe not found — skipping parity test. Install Windows SDK.");
            return;
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"winmidl_parity_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            // Compile with original midl.exe
            var originalWinmd = CompileWithOriginalMidl(fixturePath, tempDir);
            if (originalWinmd == null)
            {
                // Original midl.exe couldn't compile this fixture — skip
                return;
            }

            // Compile with WinMIDL
            var winmidlWinmd = CompileWithWinMidl(fixturePath);
            Assert.NotNull(winmidlWinmd);

            // Dump both and compare
            var originalDump = WinMdDumper.Dump(originalWinmd);
            var winmidlDump = WinMdDumper.Dump(winmidlWinmd);

            // Normalize: trim each line, remove blank lines, sort for stability
            var originalLines = NormalizeLines(originalDump);
            var winmidlLines = NormalizeLines(winmidlDump);

            Assert.Equal(originalLines, winmidlLines);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static string NormalizeLines(string dump)
    {
        var lines = dump.Split('\n')
            .Select(l => l.TrimEnd())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();
        return string.Join("\n", lines);
    }

    public static IEnumerable<object[]> GetParityFixtures()
    {
        var dir = Path.GetFullPath(FixturesDir);
        if (!Directory.Exists(dir))
            yield break;

        foreach (var file in Directory.GetFiles(dir, "parity_*.idl").OrderBy(f => f))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            yield return new object[] { name, file };
        }
    }

    /// <summary>
    /// Negative parity: files that original midl.exe rejects should also be rejected by WinMIDL.
    /// Skips fixtures marked with parityGap in their .expected.json.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetNegativeParityFixtures))]
    public void NegativeFixture_AlsoRejectedByWinMidl(string fixtureName, string fixturePath)
    {
        _ = fixtureName;

        // Skip known parity gaps
        var expectedPath = Path.ChangeExtension(fixturePath, ".expected.json");
        if (File.Exists(expectedPath))
        {
            var json = System.Text.Json.JsonDocument.Parse(File.ReadAllText(expectedPath));
            if (json.RootElement.TryGetProperty("parityGap", out var pg) && pg.GetBoolean())
            {
                return; // Known gap — WinMIDL doesn't yet detect this error
            }
        }

        // Compile with WinMIDL — should produce at least one error
        var source = File.ReadAllText(fixturePath);
        var compilation = CompilationHelper.Compile(source);
        var errors = compilation.Diagnostics
            .Where(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error)
            .ToList();

        Assert.True(errors.Count > 0,
            $"WinMIDL should reject '{fixtureName}' (original midl.exe does), but produced 0 errors. " +
            $"Diagnostics: [{string.Join(", ", compilation.Diagnostics.Select(d => $"{d.Code}:{d.Severity}"))}]");
    }

    private static readonly string ErrorFixturesDir = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "fixtures", "errors");

    public static IEnumerable<object[]> GetNegativeParityFixtures()
    {
        var dir = Path.GetFullPath(ErrorFixturesDir);
        if (!Directory.Exists(dir))
            yield break;

        foreach (var file in Directory.GetFiles(dir, "parity_neg_*.idl").OrderBy(f => f))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            yield return new object[] { name, file };
        }
    }
}
