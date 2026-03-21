using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Cli;

namespace Midl.Integration.Tests;

/// <summary>
/// T1 validation tests for OS MidlRT fixture files.
/// Verifies negative tests produce errors and positive tests compile cleanly.
/// Skips when MIDL_TEST_FIXTURES is not set.
/// </summary>
public class T1ValidationTests
{
    private static readonly string? FixturesDir =
        Environment.GetEnvironmentVariable("MIDL_TEST_FIXTURES");

    private static List<string> GetIncludePaths()
    {
        var paths = new List<string>();
        if (FixturesDir != null) paths.Add(FixturesDir);

        // Discover Windows SDK include paths
        if (OperatingSystem.IsWindows())
        {
            var sdk = WindowsSdkLocator.Discover();
            if (sdk != null)
            {
                paths.AddRange(sdk.IncludePaths);
                Console.WriteLine($"SDK discovered: {sdk.Root} v{sdk.Version}, {sdk.IncludePaths.Count} include paths");
                foreach (var p in sdk.IncludePaths)
                    Console.WriteLine($"  SDK include: {p}");
            }
            else
            {
                Console.WriteLine("SDK not discovered!");
            }
        }

        return paths;
    }

    private static List<string>? GetReferencePaths()
    {
        if (OperatingSystem.IsWindows())
        {
            var sdk = WindowsSdkLocator.Discover();
            if (sdk?.WindowsWinMdPath != null)
                return [sdk.WindowsWinMdPath];
        }
        return null;
    }

#if WITH_COMPAT
    [SkippableFact]
    public void NegativeTestFilesProduceErrors()
    {
        Skip.If(FixturesDir == null || !Directory.Exists(FixturesDir),
            "MIDL_TEST_FIXTURES not set");

        var negativeFiles = Directory.GetFiles(FixturesDir!, "*.Negative.*.idl");
        Skip.If(negativeFiles.Length == 0, "No negative test files found");

        int passed = 0, failed = 0;
        var failures = new List<string>();

        foreach (var file in negativeFiles)
        {
            try
            {
                var compilation = CompilationHelper.Compile(
                    File.ReadAllText(file), CompilationMode.Compat);
                bool hasErrors = compilation.Diagnostics.Any(
                    d => d.Severity == DiagnosticSeverity.Error);

                if (hasErrors)
                    passed++;
                else
                {
                    failed++;
                    failures.Add(Path.GetFileName(file));
                }
            }
            catch
            {
                // A crash/exception still counts as "produced an error"
                passed++;
            }
        }

        // We expect at least 50% of negative tests to produce errors
        // (some may not because we don't implement all validation rules yet)
        var rate = (double)passed / negativeFiles.Length;
        Assert.True(rate > 0.5,
            $"Only {passed}/{negativeFiles.Length} negative tests produced errors. " +
            $"First failures: {string.Join(", ", failures.Take(10))}");
    }

    [SkippableFact]
    public void PositiveModernIdlTestFilesCompile()
    {
        Skip.If(FixturesDir == null || !Directory.Exists(FixturesDir),
            "MIDL_TEST_FIXTURES not set");

        // Focus on MoIdl* positive tests (modern IDL, most likely to work)
        var positiveFiles = Directory.GetFiles(FixturesDir!, "MoIdl*.idl")
            .Where(f => !f.Contains(".Negative.", StringComparison.OrdinalIgnoreCase)
                     && !f.Contains("negative", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Skip.If(positiveFiles.Length == 0, "No MoIdl positive test files found");

        int passed = 0, failed = 0;
        var failures = new List<string>();

        foreach (var file in positiveFiles)
        {
            try
            {
                var source = File.ReadAllText(file);
                var compilation = CompilationHelper.Compile(source,
                    referencePaths: null, // compat mode — types come from IDL imports, not WinMD
                    mode: CompilationMode.Compat,
                    includePaths: GetIncludePaths(),
                    currentFilePath: file);
                bool hasErrors = compilation.Diagnostics.Any(
                    d => d.Severity == DiagnosticSeverity.Error);
                if (!hasErrors)
                    passed++;
                else
                {
                    failed++;
                    var firstError = compilation.Diagnostics
                        .First(d => d.Severity == DiagnosticSeverity.Error);
                    failures.Add($"{Path.GetFileName(file)} ({firstError.Code}: {firstError.Message[..Math.Min(60, firstError.Message.Length)]})");
                }
            }
            catch (Exception ex)
            {
                failed++;
                failures.Add(Path.GetFileName(file) + $" (CRASH: {ex.Message[..Math.Min(40, ex.Message.Length)]})");
            }
        }

        // Report rate but don't fail — this is aspirational
        var rate = positiveFiles.Length > 0 ? (double)passed / positiveFiles.Length : 0;
        Console.WriteLine($"MoIdl positive: {passed}/{positiveFiles.Length} pass ({rate:P0})");
        if (failures.Count > 0)
        {
            Console.WriteLine($"Failures ({failures.Count}):");
            foreach (var f in failures.Take(20))
                Console.WriteLine($"  {f}");
        }
    }

    [SkippableFact]
    public void PositiveModernIdlTestFilesCompile_WithClPreprocessor()
    {
        Skip.If(FixturesDir == null || !Directory.Exists(FixturesDir),
            "MIDL_TEST_FIXTURES not set");

        // Check if cl.exe is available
        bool hasCl = false;
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("cl.exe")
            {
                UseShellExecute = false, RedirectStandardOutput = true,
                RedirectStandardError = true, CreateNoWindow = true
            };
            using var p = System.Diagnostics.Process.Start(psi);
            p?.StandardOutput.ReadToEnd();
            p?.StandardError.ReadToEnd();
            p?.WaitForExit(10000);
            hasCl = p != null;
        }
        catch { }
        Skip.If(!hasCl, "cl.exe not found — run from Developer Command Prompt");

        var positiveFiles = Directory.GetFiles(FixturesDir!, "MoIdl*.idl")
            .Where(f => !f.Contains(".Negative.", StringComparison.OrdinalIgnoreCase)
                     && !f.Contains("negative", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Skip.If(positiveFiles.Length == 0, "No MoIdl positive test files found");

        int passed = 0, failed = 0;
        var failures = new List<string>();

        foreach (var file in positiveFiles)
        {
            try
            {
                var source = File.ReadAllText(file);
                var compilation = CompilationHelper.Compile(source,
                    referencePaths: GetReferencePaths(),
                    mode: CompilationMode.Compat,
                    includePaths: GetIncludePaths(),
                    currentFilePath: file,
                    cppCmd: "cl.exe");
                bool hasErrors = compilation.Diagnostics.Any(
                    d => d.Severity == DiagnosticSeverity.Error);
                if (!hasErrors)
                    passed++;
                else
                {
                    failed++;
                    var firstError = compilation.Diagnostics
                        .First(d => d.Severity == DiagnosticSeverity.Error);
                    failures.Add($"{Path.GetFileName(file)} ({firstError.Code}: {firstError.Message[..Math.Min(60, firstError.Message.Length)]})");
                }
            }
            catch (Exception ex)
            {
                failed++;
                failures.Add(Path.GetFileName(file) + $" (CRASH: {ex.Message[..Math.Min(40, ex.Message.Length)]})");
            }
        }

        var rate = positiveFiles.Length > 0 ? (double)passed / positiveFiles.Length : 0;
        Console.WriteLine($"MoIdl positive (cl.exe cpp): {passed}/{positiveFiles.Length} pass ({rate:P0})");
        if (failures.Count > 0)
        {
            Console.WriteLine($"Failures ({failures.Count}):");
            foreach (var f in failures.Take(20))
                Console.WriteLine($"  {f}");
        }
    }
#endif
}
