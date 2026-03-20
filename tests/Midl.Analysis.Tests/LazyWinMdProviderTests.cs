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
    public void LazyProviderIndexIsFasterThanEagerLoad()
    {
        // Create a WinMD with many types to measure the difference
        var types = new System.Text.StringBuilder();
        types.AppendLine("namespace PerfTest {");
        for (int i = 0; i < 200; i++)
        {
            types.AppendLine($"    enum Type{i} {{ A, B, C }};");
        }
        types.AppendLine("}");

        var winmd = EmitWinMd(types.ToString());
        var tempPath = Path.Combine(Path.GetTempPath(), $"perf_test_{Guid.NewGuid()}.winmd");
        File.WriteAllBytes(tempPath, winmd);

        try
        {
            // Warm up
            using (var p = new LazyWinMdProvider(tempPath)) { }
            new WinMdReader().ReadTypes(tempPath);

            // Measure lazy index time (no type decoding)
            var sw = Stopwatch.StartNew();
            const int iterations = 50;
            for (int i = 0; i < iterations; i++)
            {
                using var provider = new LazyWinMdProvider(tempPath);
                Assert.True(provider.TypeCount >= 200);
            }
            var lazyTime = sw.Elapsed;

            // Measure eager load time (full type decoding)
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var reader = new WinMdReader();
                var all = reader.ReadTypes(tempPath);
                Assert.True(all.Count >= 200);
            }
            var eagerTime = sw.Elapsed;

            // Lazy should be faster (index-only vs full decoding)
            Assert.True(lazyTime < eagerTime,
                $"Lazy ({lazyTime.TotalMilliseconds:F1}ms) should be faster than eager ({eagerTime.TotalMilliseconds:F1}ms)");
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}
