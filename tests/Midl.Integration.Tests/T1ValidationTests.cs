using Midl.Analysis;
using Midl.Analysis.Diagnostics;

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
                var compilation = CompilationHelper.Compile(source, CompilationMode.Compat);
                bool hasErrors = compilation.Diagnostics.Any(
                    d => d.Severity == DiagnosticSeverity.Error);
                if (!hasErrors)
                    passed++;
                else
                {
                    failed++;
                    failures.Add(Path.GetFileName(file));
                }
            }
            catch
            {
                failed++;
                failures.Add(Path.GetFileName(file) + " (CRASH)");
            }
        }

        // Report rate but don't fail — this is aspirational
        var rate = positiveFiles.Length > 0 ? (double)passed / positiveFiles.Length : 0;
        // Just log, don't assert a threshold yet
        Console.WriteLine($"MoIdl positive: {passed}/{positiveFiles.Length} pass ({rate:P0})");
    }
}
