// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Determinism contract for <c>/deterministic</c> WinMD emission.
///
/// Two properties must hold, and both matter for the feature's stated purpose
/// (build caching / content-addressable artifact stores):
///
///   1. Same input  ⇒ byte-identical output (reproducibility)
///   2. Different content ⇒ different MVID (no collisions)
///
/// (2) is what makes the MVID usable as a cache key. An MVID derived from a
/// partial view of the semantic model silently collides whenever two distinct
/// modules happen to share that view.
/// </summary>
public class WinMdDeterminismTests
{
    private static byte[] Emit(string source, bool deterministic = true, string? outputPath = null)
    {
        var compilation = CompilationHelper.Compile(source);
        Assert.All(compilation.Diagnostics,
            d => Assert.NotEqual(DiagnosticSeverity.Error, d.Severity));
        return new WinMdEmitter(deterministic).EmitToBytes(compilation, outputPath);
    }

    private static Guid Mvid(byte[] winmd)
    {
        using var pe = new PEReader(new MemoryStream(winmd));
        var reader = pe.GetMetadataReader();
        return reader.GetGuid(reader.GetModuleDefinition().Mvid);
    }

    private static uint TimeDateStamp(byte[] winmd)
    {
        using var pe = new PEReader(new MemoryStream(winmd));
        return (uint)pe.PEHeaders.CoffHeader.TimeDateStamp;
    }

    private const string Baseline = """
        namespace Test
        {
            struct Point { Int32 X; Int32 Y; };
            enum Color { Red = 1, Green = 2 };
            interface IThing { Int32 Bar(Int32 alpha); }
            [version(1)]
            runtimeclass C { C(); Int32 Go(); }
        }
        """;

    // ---------------------------------------------------------------
    // Property 1 — reproducibility
    // ---------------------------------------------------------------

    [Fact]
    public void SameSource_ProducesByteIdenticalOutput()
    {
        Assert.Equal(Emit(Baseline), Emit(Baseline));
    }

    [Fact]
    public void SameSource_ProducesStableMvidAndTimestamp()
    {
        var a = Emit(Baseline);
        var b = Emit(Baseline);

        Assert.Equal(Mvid(a), Mvid(b));
        Assert.Equal(TimeDateStamp(a), TimeDateStamp(b));
        Assert.NotEqual(Guid.Empty, Mvid(a));
    }

    [Fact]
    public void WithoutDeterministic_OutputStillVaries()
    {
        // Opt-in only: default behavior is unchanged.
        Assert.NotEqual(
            Mvid(Emit(Baseline, deterministic: false)),
            Mvid(Emit(Baseline, deterministic: false)));
    }

    // ---------------------------------------------------------------
    // Property 2 — no collisions
    // ---------------------------------------------------------------

    [Theory]
    // Each variant differs from Baseline only in the named dimension.
    [InlineData("struct field types", """
        namespace Test
        {
            struct Point { Double X; Double Y; Double Z; Single W; };
            enum Color { Red = 1, Green = 2 };
            interface IThing { Int32 Bar(Int32 alpha); }
            [version(1)]
            runtimeclass C { C(); Int32 Go(); }
        }
        """)]
    [InlineData("enum member names and values", """
        namespace Test
        {
            struct Point { Int32 X; Int32 Y; };
            enum Color { Crimson = 100, Emerald = 200, Cobalt = 300 };
            interface IThing { Int32 Bar(Int32 alpha); }
            [version(1)]
            runtimeclass C { C(); Int32 Go(); }
        }
        """)]
    [InlineData("parameter names", """
        namespace Test
        {
            struct Point { Int32 X; Int32 Y; };
            enum Color { Red = 1, Green = 2 };
            interface IThing { Int32 Bar(Int32 zulu); }
            [version(1)]
            runtimeclass C { C(); Int32 Go(); }
        }
        """)]
    [InlineData("version attribute", """
        namespace Test
        {
            struct Point { Int32 X; Int32 Y; };
            enum Color { Red = 1, Green = 2 };
            interface IThing { Int32 Bar(Int32 alpha); }
            [version(0x0A000010)]
            runtimeclass C { C(); Int32 Go(); }
        }
        """)]
    public void DifferentContent_ProducesDifferentMvid(string dimension, string variant)
    {
        var baseline = Emit(Baseline);
        var changed = Emit(variant);

        Assert.False(baseline.AsSpan().SequenceEqual(changed),
            $"test setup: '{dimension}' should change the emitted bytes");
        Assert.True(Mvid(baseline) != Mvid(changed),
            $"MVID collision: changing {dimension} produced different content but the same MVID");
    }

    [Fact]
    public void DifferentModuleName_ProducesDifferentMvid()
    {
        // The module and assembly names are derived from the output path and are
        // embedded in the metadata, so they are part of module identity.
        var alpha = Emit(Baseline, outputPath: "Alpha.winmd");
        var beta = Emit(Baseline, outputPath: "Beta.winmd");

        Assert.False(alpha.AsSpan().SequenceEqual(beta));
        Assert.True(Mvid(alpha) != Mvid(beta),
            "MVID collision: different module names produced the same MVID");
    }

    [Fact]
    public void PerNamespace_EachModuleGetsDistinctMvid()
    {
        var compilation = CompilationHelper.Compile("""
            namespace Alpha { runtimeclass A { A(); Int32 GoA(); } }
            namespace Beta  { runtimeclass B { B(); Int32 GoB(); } }
            namespace Gamma { runtimeclass G { G(); Int32 GoG(); } }
            """);
        Assert.All(compilation.Diagnostics,
            d => Assert.NotEqual(DiagnosticSeverity.Error, d.Severity));

        var modules = new WinMdEmitter(deterministic: true).EmitPerNamespace(compilation);
        Assert.Equal(3, modules.Count);

        var mvids = modules.ToDictionary(kv => kv.Key, kv => Mvid(kv.Value));
        Assert.Equal(3, mvids.Values.Distinct().Count());
    }

    [Fact]
    public void PerNamespace_IsReproducible()
    {
        const string source = """
            namespace Alpha { runtimeclass A { A(); Int32 GoA(); } }
            namespace Beta  { runtimeclass B { B(); Int32 GoB(); } }
            """;

        static Dictionary<string, byte[]> Run()
        {
            var c = CompilationHelper.Compile(source);
            return new WinMdEmitter(deterministic: true).EmitPerNamespace(c);
        }

        var first = Run();
        var second = Run();

        Assert.Equal(first.Keys.OrderBy(k => k), second.Keys.OrderBy(k => k));
        foreach (var key in first.Keys)
            Assert.Equal(first[key], second[key]);
    }

    // ---------------------------------------------------------------
    // Emitter reuse — state must not leak between emissions
    // ---------------------------------------------------------------

    [Fact]
    public void ReusedEmitter_ProducesIndependentMvids()
    {
        var emitter = new WinMdEmitter(deterministic: true);

        var a = emitter.EmitToBytes(CompilationHelper.Compile(Baseline));
        var b = emitter.EmitToBytes(CompilationHelper.Compile("""
            namespace Test
            {
                interface IOther { void Different(); }
            }
            """));

        Assert.True(Mvid(a) != Mvid(b));
        Assert.NotEqual(Guid.Empty, Mvid(a));
        Assert.NotEqual(Guid.Empty, Mvid(b));
    }
}
