// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;
using Midl.CodeGen.WinMd;

namespace Midl.Analysis.Tests;

public class LazyWinMdProviderTests
{
    /// <summary>Helper: compile IDL source to WinMD bytes.</summary>
    private static byte[] EmitWinMd(string idlSource)
    {
        var compilation = CompilationHelper.Compile(idlSource);
        Assert.False(compilation.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            "IDL compilation should not have errors");
        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    [Fact]
    public void LazyProviderResolvesType()
    {
        var winmd = EmitWinMd(@"
            namespace TestNs
            {
                enum StatusCode
                {
                    Ok,
                    Error
                };

                struct MyPoint
                {
                    Single X;
                    Single Y;
                };

                [uuid(12345678-1234-1234-1234-123456789abc)]
                interface IWidget
                {
                    void DoWork();
                };

                [default_interface]
                runtimeclass Widget
                {
                    Widget();
                };
            }");

        using var provider = new LazyWinMdProvider(winmd);
        Assert.True(provider.TypeCount >= 4);

        // Resolve enum
        var enumSym = provider.TryResolve("TestNs.StatusCode");
        Assert.NotNull(enumSym);
        Assert.IsType<EnumSymbol>(enumSym);
        Assert.Equal("TestNs.StatusCode", enumSym!.FullName);
        Assert.True(enumSym.IsExternal);

        // Resolve struct
        var structSym = provider.TryResolve("TestNs.MyPoint");
        Assert.NotNull(structSym);
        Assert.IsType<StructSymbol>(structSym);

        // Resolve interface
        var ifaceSym = provider.TryResolve("TestNs.IWidget");
        Assert.NotNull(ifaceSym);
        Assert.IsType<InterfaceSymbol>(ifaceSym);

        // Resolve runtime class
        var rcSym = provider.TryResolve("TestNs.Widget");
        Assert.NotNull(rcSym);
        Assert.IsType<RuntimeClassSymbol>(rcSym);
    }

    [Fact]
    public void LazyProviderCachesResult()
    {
        var winmd = EmitWinMd(@"
            namespace TestNs
            {
                enum Color { Red, Green, Blue };
            }");

        using var provider = new LazyWinMdProvider(winmd);

        var first = provider.TryResolve("TestNs.Color");
        var second = provider.TryResolve("TestNs.Color");

        Assert.NotNull(first);
        Assert.Same(first, second); // Same cached instance
    }

    [Fact]
    public void LazyProviderReturnsNullForUnknown()
    {
        var winmd = EmitWinMd(@"
            namespace TestNs
            {
                enum Color { Red };
            }");

        using var provider = new LazyWinMdProvider(winmd);

        Assert.Null(provider.TryResolve("NonExistent.Type"));
        Assert.Null(provider.TryResolve("TestNs.NonExistent"));
        Assert.False(provider.HasType("Bogus"));
    }

    [Fact]
    public void LazyProviderHasType()
    {
        var winmd = EmitWinMd(@"
            namespace TestNs
            {
                enum Color { Red };
            }");

        using var provider = new LazyWinMdProvider(winmd);

        Assert.True(provider.HasType("TestNs.Color"));
        Assert.False(provider.HasType("TestNs.Bogus"));
    }

    [Fact]
    public void LazyProviderGetTypesInNamespace()
    {
        var winmd = EmitWinMd(@"
            namespace TestNs
            {
                enum Color { Red };
                struct Point { Int32 X; Int32 Y; };
            }");

        using var provider = new LazyWinMdProvider(winmd);

        var types = provider.GetTypesInNamespace("TestNs").ToList();
        Assert.Contains("TestNs.Color", types);
        Assert.Contains("TestNs.Point", types);
    }

    [Fact]
    public void CompilationWithLazyProvider()
    {
        // Create a WinMD with types we'll reference
        var winmd = EmitWinMd(@"
            namespace External
            {
                enum StatusCode
                {
                    Ok,
                    Error
                };
            }");

        var tempPath = Path.Combine(Path.GetTempPath(), $"lazy_test_{Guid.NewGuid()}.winmd");
        try
        {
            File.WriteAllBytes(tempPath, winmd);

            // Compile IDL that references the external type
            var source = @"
                namespace MyApp
                {
                    runtimeclass Widget
                    {
                        External.StatusCode GetStatus();
                    };
                }";

            var compilation = CompilationHelper.Compile(source, new[] { tempPath });

            // Should resolve without errors
            Assert.False(compilation.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
                "Compilation with lazy provider should succeed: " +
                string.Join("; ", compilation.Diagnostics.Select(d => d.ToString())));

            // The resolved type should be accessible
            var externalType = compilation.GetTypeByName("External.StatusCode");
            Assert.NotNull(externalType);
            Assert.True(externalType!.IsExternal);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void LazyProviderReusableAcrossCompilations()
    {
        var winmd = EmitWinMd(@"
            namespace External
            {
                enum StatusCode { Ok, Error };
            }");

        using var provider = new LazyWinMdProvider(winmd);

        var source = @"
            namespace MyApp
            {
                runtimeclass Widget
                {
                    External.StatusCode GetStatus();
                };
            }";

        // Compile twice with the same provider instance
        var comp1 = CompilationHelper.Compile(source, new LazyWinMdProvider[] { provider });
        var comp2 = CompilationHelper.Compile(source, new LazyWinMdProvider[] { provider });

        Assert.False(comp1.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            "First compilation should succeed: " +
            string.Join("; ", comp1.Diagnostics.Select(d => d.ToString())));
        Assert.False(comp2.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            "Second compilation should succeed: " +
            string.Join("; ", comp2.Diagnostics.Select(d => d.ToString())));

        // Both should resolve the external type
        Assert.NotNull(comp1.GetTypeByName("External.StatusCode"));
        Assert.NotNull(comp2.GetTypeByName("External.StatusCode"));
    }

    [Fact]
    public void CachedCompilationHasSameDiagnosticsAsFresh()
    {
        var winmd = EmitWinMd(@"
            namespace External
            {
                enum StatusCode { Ok, Error };
            }");

        using var provider = new LazyWinMdProvider(winmd);

        // Source that produces both successful resolution and an error
        var source = @"
            namespace MyApp
            {
                runtimeclass Widget
                {
                    External.StatusCode GetStatus();
                    External.Bogus GetBogus();
                };
            }";

        var comp1 = CompilationHelper.Compile(source, new LazyWinMdProvider[] { provider });
        var comp2 = CompilationHelper.Compile(source, new LazyWinMdProvider[] { provider });

        Assert.Equal(comp1.Diagnostics.Count, comp2.Diagnostics.Count);
        for (int i = 0; i < comp1.Diagnostics.Count; i++)
        {
            Assert.Equal(comp1.Diagnostics[i].Code, comp2.Diagnostics[i].Code);
            Assert.Equal(comp1.Diagnostics[i].Message, comp2.Diagnostics[i].Message);
        }
    }

    /// <summary>
    /// The lazy provider indexes names without decoding types, so it should be materially
    /// faster than a full read.
    ///
    /// This used to compare a single wall-clock sample of each with no margin
    /// (<c>lazyTime &lt; eagerTime</c>), which fails whenever the machine is busy — it has
    /// been observed failing at 222.0ms vs 217.4ms, a 2% difference that says nothing.
    /// Take the best of several samples, which removes most scheduling noise, and require a
    /// margin large enough that only a real regression can breach it.
    /// </summary>
    [Fact]
    public void LazyProviderIndexIsFasterThanEagerLoad()
    {
        // Enough types that index-only versus full decode is a structural difference rather
        // than a handful of microseconds.
        var types = new System.Text.StringBuilder();
        types.AppendLine("namespace PerfTest {");
        for (int i = 0; i < 1000; i++)
        {
            types.AppendLine($"    enum Type{i} {{ A, B, C }};");
        }
        types.AppendLine("}");

        var winmd = EmitWinMd(types.ToString());
        var tempPath = Path.Combine(Path.GetTempPath(), $"perf_test_{Guid.NewGuid()}.winmd");
        File.WriteAllBytes(tempPath, winmd);

        try
        {
            // Warm up: JIT, file cache, and any one-time metadata setup.
            for (int i = 0; i < 3; i++)
            {
                using (var p = new LazyWinMdProvider(tempPath)) { }
                new WinMdReader().ReadTypes(tempPath);
            }

            const int samples = 5;
            const int iterations = 20;

            var lazyBest = TimeSpan.MaxValue;
            var eagerBest = TimeSpan.MaxValue;

            // Interleave the samples so a slow patch of machine time cannot land entirely on
            // one of the two measurements.
            for (int s = 0; s < samples; s++)
            {
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < iterations; i++)
                {
                    using var provider = new LazyWinMdProvider(tempPath);
                    Assert.True(provider.TypeCount >= 1000);
                }
                var lazy = sw.Elapsed;
                if (lazy < lazyBest) lazyBest = lazy;

                sw.Restart();
                for (int i = 0; i < iterations; i++)
                {
                    var reader = new WinMdReader();
                    var all = reader.ReadTypes(tempPath);
                    Assert.True(all.Count >= 1000);
                }
                var eager = sw.Elapsed;
                if (eager < eagerBest) eagerBest = eager;
            }

            // A clear margin: indexing should not be within a whisker of full decoding.
            Assert.True(lazyBest * 2 < eagerBest,
                $"Lazy indexing ({lazyBest.TotalMilliseconds:F1}ms best of {samples}) should be " +
                $"comfortably faster than eager loading ({eagerBest.TotalMilliseconds:F1}ms best of {samples})");
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}
