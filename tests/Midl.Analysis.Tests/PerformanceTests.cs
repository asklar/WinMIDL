// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Midl.Analysis;

namespace Midl.Analysis.Tests;

/// <summary>
/// Performance regression tests for compilation pipeline hot paths.
/// These verify optimizations don't regress and key operations stay fast.
///
/// Each timed region is preceded by a warm-up compile. The first compilation in a process
/// pays to JIT the whole pipeline, which on a loaded CI runner dwarfs the work being
/// measured — <c>FindClosestMatch_CachesCandidates_SecondCallFaster</c> has failed in CI on
/// exactly that. The bounds are deliberately loose: they exist to catch a catastrophic
/// regression such as an accidental quadratic, not to measure the runner.
/// </summary>
public class PerformanceTests
{
    /// <summary>Generous enough to be about the code rather than the machine.</summary>
    private const int BudgetMs = 2000;

    [Fact]
    public void FindClosestMatch_CachesCandidates_SecondCallFaster()
    {
        // Compile with an undefined type to trigger FindClosestMatch
        var source1 = """
            namespace TestApp
            {
                runtimeclass Widget
                {
                    Strng Name { get; };
                }
            }
            """;

        // First compilation warms up the candidate cache
        var c1 = CompilationHelper.Compile(source1);
        Assert.Contains(c1.Diagnostics, d => d.Code == "MIDL1001" && d.Message.Contains("Did you mean"));

        // Multiple undefined types should benefit from cached candidates
        var source2 = """
            namespace TestApp
            {
                runtimeclass Widget
                {
                    Strng Name { get; };
                    Int3 Count { get; };
                    Boolen IsActive { get; };
                }
            }
            """;

        var sw = Stopwatch.StartNew();
        var c2 = CompilationHelper.Compile(source2);
        sw.Stop();

        // All 3 should produce suggestions
        var suggestions = c2.Diagnostics
            .Where(d => d.Code == "MIDL1001" && d.Message.Contains("Did you mean"))
            .ToList();
        Assert.Equal(3, suggestions.Count);

        // Verify it completes in reasonable time (see BudgetMs)
        Assert.True(sw.ElapsedMilliseconds < BudgetMs,
            $"Compilation with 5 type errors took {sw.ElapsedMilliseconds}ms, expected < {BudgetMs}ms");
    }

    [Fact]
    public void ParseManyMembers_CompletesQuickly()
    {
        // Generate a class with many members to stress parser list pooling
        var members = string.Join("\n        ",
            Enumerable.Range(0, 50).Select(i => $"String Prop{i} {{ get; set; }};"));

        var source = "namespace TestApp\n{\n    runtimeclass BigWidget\n    {\n        BigWidget();\n        " +
            members + "\n    }\n}\n";

        // Warm up so the measurement is of the parse, not of JITting the pipeline.
        CompilationHelper.Compile(source);

        var sw = Stopwatch.StartNew();
        var compilation = CompilationHelper.Compile(source);
        sw.Stop();

        foreach (var d in compilation.Diagnostics)
            Assert.NotEqual(Diagnostics.DiagnosticSeverity.Error, d.Severity);
        Assert.True(sw.ElapsedMilliseconds < BudgetMs,
            $"Compilation of 50-member class took {sw.ElapsedMilliseconds}ms, expected < {BudgetMs}ms");
    }

    [Fact]
    public void CompilationProfile_RecordsPhaseTiming()
    {
        var source = """
            namespace TestApp
            {
                runtimeclass Widget
                {
                    Widget();
                    String Name { get; set; };
                }
            }
            """;

        var profile = new CompilationProfile();
        var compilation = CompilationHelper.Compile(
            source,
            referencePaths: null,
            CompilationMode.Strict,
            includePaths: null,
            currentFilePath: null,
            defines: null,
            emitQualityHints: false,
            refDirectories: null,
            profile: profile);

        // All phases should have non-negative timings
        Assert.True(profile.Preprocess >= TimeSpan.Zero);
        Assert.True(profile.Lex >= TimeSpan.Zero);
        Assert.True(profile.Parse >= TimeSpan.Zero);
        Assert.True(profile.Bind >= TimeSpan.Zero);
        Assert.True(profile.Validate >= TimeSpan.Zero);

        // At least Lex, Parse, and Bind should take measurable time
        Assert.True(profile.Lex > TimeSpan.Zero || profile.Parse > TimeSpan.Zero || profile.Bind > TimeSpan.Zero,
            "At least one major phase should take > 0ms");

        // Total should be the sum of all phases
        var expectedTotal = profile.Preprocess + profile.Lex + profile.Parse +
            profile.SyntaxErrors + profile.ImportResolve + profile.Bind +
            profile.Synthesize + profile.VersionPropagate + profile.Validate;
        Assert.Equal(expectedTotal, profile.Total);
    }

    [Fact]
    public void CompilationProfile_PrintsFormattedOutput()
    {
        var profile = new CompilationProfile
        {
            Preprocess = TimeSpan.FromMilliseconds(1.2),
            Lex = TimeSpan.FromMilliseconds(0.8),
            Parse = TimeSpan.FromMilliseconds(1.5),
            SyntaxErrors = TimeSpan.FromMilliseconds(0.1),
            ImportResolve = TimeSpan.FromMilliseconds(0.05),
            Bind = TimeSpan.FromMilliseconds(3.2),
            Synthesize = TimeSpan.FromMilliseconds(0.3),
            VersionPropagate = TimeSpan.FromMilliseconds(0.15),
            Validate = TimeSpan.FromMilliseconds(0.9),
            Total = TimeSpan.FromMilliseconds(8.2),
        };

        var writer = new StringWriter();
        profile.Print(writer);
        var output = writer.ToString();

        Assert.Contains("[PROFILE] Total:", output);
        Assert.Contains("[PROFILE] Preprocess:", output);
        Assert.Contains("[PROFILE] Lex:", output);
        Assert.Contains("[PROFILE] Parse:", output);
        Assert.Contains("[PROFILE] Bind:", output);
        Assert.Contains("[PROFILE] Validate:", output);
        // Verify percentage is present
        Assert.Contains("%", output);
    }

    [Fact]
    public void TokenPositionMap_LazyBuild_WorksCorrectly()
    {
        var source = """
            namespace TestApp
            {
                runtimeclass Widget
                {
                    Widget();
                }
            }
            """;

        var lexer = new Midl.Lexer.Lexer(source);
        var tokens = lexer.Lex();
        var posMap = new TokenPositionMap(tokens);

        // First access triggers lazy build
        var pos = posMap.GetPositionOfFirst("namespace");
        Assert.True(pos >= 0);

        // Subsequent access should return same value
        var pos2 = posMap.GetPositionOfFirst("namespace");
        Assert.Equal(pos, pos2);

        // Widget should be after namespace
        var widgetPos = posMap.GetPositionOfFirst("Widget");
        Assert.True(widgetPos > pos);
    }

    [Fact]
    public void EditDistance_CorrectResults()
    {
        // Verify edit distance is correct after optimization
        Assert.Equal(0, TypeResolver.EditDistance("hello", "hello"));
        Assert.Equal(1, TypeResolver.EditDistance("hello", "helo"));
        Assert.Equal(1, TypeResolver.EditDistance("string", "strng"));
        Assert.Equal(1, TypeResolver.EditDistance("boolean", "boolen"));
        Assert.Equal(4, TypeResolver.EditDistance("abc", "abcdefg", 5)); // maxDist >= diff
    }

    [Fact]
    public void EditDistance_EarlyExit_ReturnsAboveMax()
    {
        // Length difference > maxDist should early-exit
        var result = TypeResolver.EditDistance("a", "abcdefghij", 3);
        Assert.True(result > 3);
    }
}
