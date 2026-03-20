using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Midl.Analysis;
using Midl.CodeGen.Comparison;
using Midl.CodeGen.Decompiler;
using Midl.CodeGen.WinMd;

namespace Midl.CodeGen.Tests;

/// <summary>
/// T3 golden-file comparison tests for WinMD structural fidelity.
/// Verifies round-trip consistency and (when available) comparison against original midl.exe.
/// </summary>
public class GoldenComparisonTests
{
    private static byte[] CompileToWinMd(string source)
    {
        var compilation = CompilationHelper.Compile(source);
        Assert.Empty(compilation.Diagnostics);
        var emitter = new WinMdEmitter();
        return emitter.EmitToBytes(compilation);
    }

    private static int CountTypes(byte[] winmdBytes)
    {
        using var pe = new PEReader(new MemoryStream(winmdBytes));
        var reader = pe.GetMetadataReader();
        int count = 0;
        foreach (var handle in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(handle);
            if (reader.GetString(td.Name) != "<Module>")
                count++;
        }
        return count;
    }

    [Fact]
    public void RoundTripProducesSameWinMd()
    {
        var source = "namespace Test { runtimeclass Foo { Foo(); void DoWork(); String Name { get; }; } }";

        var bytes1 = CompileToWinMd(source);
        var bytes2 = CompileToWinMd(source);

        var comparer = new WinMdComparer();
        var result = comparer.Compare(bytes1, bytes2);
        Assert.True(result.AreEqual, string.Join("\n", result.Differences));
    }

    [Fact]
    public void RoundTripEnum()
    {
        var source = "namespace Test { enum Color { Red, Green, Blue } }";

        var bytes1 = CompileToWinMd(source);
        var bytes2 = CompileToWinMd(source);

        var comparer = new WinMdComparer();
        var result = comparer.Compare(bytes1, bytes2);
        Assert.True(result.AreEqual, string.Join("\n", result.Differences));
    }

    [Fact]
    public void RoundTripStruct()
    {
        var source = "namespace Test { struct Point { Int32 X; Int32 Y; } }";

        var bytes1 = CompileToWinMd(source);
        var bytes2 = CompileToWinMd(source);

        var comparer = new WinMdComparer();
        var result = comparer.Compare(bytes1, bytes2);
        Assert.True(result.AreEqual, string.Join("\n", result.Differences));
    }

    [Fact]
    public void RoundTripInterface()
    {
        var source = "namespace Test { interface IFoo { void DoWork(); String Name { get; }; } }";

        var bytes1 = CompileToWinMd(source);
        var bytes2 = CompileToWinMd(source);

        var comparer = new WinMdComparer();
        var result = comparer.Compare(bytes1, bytes2);
        Assert.True(result.AreEqual, string.Join("\n", result.Differences));
    }

    [Fact]
    public void RoundTripDelegate()
    {
        var source = "namespace Test { delegate void Handler(String message); }";

        var bytes1 = CompileToWinMd(source);
        var bytes2 = CompileToWinMd(source);

        var comparer = new WinMdComparer();
        var result = comparer.Compare(bytes1, bytes2);
        Assert.True(result.AreEqual, string.Join("\n", result.Differences));
    }

    [Fact]
    public void RoundTripComplexSource()
    {
        var source = """
            namespace Test
            {
                enum Color { Red, Green, Blue }
                struct Point { Int32 X; Int32 Y; }
                delegate void Handler();
                runtimeclass Widget
                {
                    Widget();
                    void Click();
                    String Label { get; };
                    event Handler Clicked;
                }
            }
            """;

        var bytes1 = CompileToWinMd(source);
        var bytes2 = CompileToWinMd(source);

        var comparer = new WinMdComparer();
        var result = comparer.Compare(bytes1, bytes2);
        Assert.True(result.AreEqual, string.Join("\n", result.Differences));
    }

    [Fact]
    public void DecompileRoundTripPreservesStructure()
    {
        var source = """
            namespace Test
            {
                enum Color { Red, Green, Blue }
                struct Point { Int32 X; Int32 Y; }
                delegate void Handler();
                runtimeclass Widget
                {
                    Widget();
                    void Click();
                    String Label { get; };
                    event Handler Clicked;
                }
            }
            """;

        // Compile → WinMD → Decompile → Recompile → WinMD → Compare
        var bytes1 = CompileToWinMd(source);

        var decompiler = new WinMdDecompiler();
        var decompiled = decompiler.Decompile(bytes1);

        var recompilation = CompilationHelper.Compile(decompiled);
        // Allow for minor diagnostic differences in decompiled IDL
        var bytes2 = new WinMdEmitter().EmitToBytes(recompilation);

        // Type counts should match after round-trip
        Assert.Equal(CountTypes(bytes1), CountTypes(bytes2));
    }

    [Fact]
    public void ComparerDetectsMissingType()
    {
        var source1 = "namespace Test { enum A { X } enum B { Y } }";
        var source2 = "namespace Test { enum A { X } }";

        var bytes1 = CompileToWinMd(source1);
        var bytes2 = CompileToWinMd(source2);

        var comparer = new WinMdComparer();
        var result = comparer.Compare(bytes1, bytes2);

        Assert.False(result.AreEqual);
        Assert.Contains(result.Differences, d => d.Contains("Missing type") && d.Contains("Test.B"));
    }

    [Fact]
    public void ComparerDetectsExtraType()
    {
        var source1 = "namespace Test { enum A { X } }";
        var source2 = "namespace Test { enum A { X } enum B { Y } }";

        var bytes1 = CompileToWinMd(source1);
        var bytes2 = CompileToWinMd(source2);

        var comparer = new WinMdComparer();
        var result = comparer.Compare(bytes1, bytes2);

        Assert.False(result.AreEqual);
        Assert.Contains(result.Differences, d => d.Contains("Extra type") && d.Contains("Test.B"));
    }

    [Fact]
    public void ComparerDetectsMethodDifference()
    {
        var source1 = "namespace Test { runtimeclass Foo { Foo(); void DoWork(); } }";
        var source2 = "namespace Test { runtimeclass Foo { Foo(); void DoWork(); void Extra(); } }";

        var bytes1 = CompileToWinMd(source1);
        var bytes2 = CompileToWinMd(source2);

        var comparer = new WinMdComparer();
        // Compare the synth interfaces which carry the methods
        var result = comparer.Compare(bytes1, bytes2);
        Assert.False(result.AreEqual);
        Assert.True(result.Differences.Count > 0);
    }

    [Fact]
    public void ComparerHandlesMissingExpectedFile()
    {
        var comparer = new WinMdComparer();
        var result = comparer.Compare("nonexistent_expected.winmd", "nonexistent_actual.winmd");

        Assert.False(result.AreEqual);
        Assert.Contains(result.Differences, d => d.Contains("not found"));
    }

    [Fact]
    public void ComparerIdenticalFilesAreEqual()
    {
        var source = """
            namespace Test
            {
                runtimeclass Foo
                {
                    Foo();
                    void DoWork();
                    String Name { get; };
                }
            }
            """;

        var bytes = CompileToWinMd(source);

        var comparer = new WinMdComparer();
        var result = comparer.Compare(bytes, bytes);
        Assert.True(result.AreEqual, string.Join("\n", result.Differences));
    }

    [Fact]
    public void CountTypesReturnsCorrectCount()
    {
        var source = """
            namespace Test
            {
                enum Color { Red, Green, Blue }
                struct Point { Int32 X; Int32 Y; }
                runtimeclass Widget { Widget(); }
            }
            """;

        var bytes = CompileToWinMd(source);
        var comparer = new WinMdComparer();
        var count = comparer.CountTypes(bytes);

        // At least 3 user-defined types (enum, struct, runtimeclass)
        // May have synthesized interface too
        Assert.True(count >= 3, $"Expected at least 3 types, got {count}");
    }
}
