using Midl.Analysis;
using Midl.Lexer;

namespace Midl.Analysis.Tests;

public class SourceMapTests : IDisposable
{
    private readonly string _tempDir;

    public SourceMapTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "midl_sourcemap_tests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    [Fact]
    public void Resolve_MainFilePosition_ReturnsCorrectMapping()
    {
        var map = new SourceMap();
        // Segment: expanded 0..100 maps to "main.midl" original offset 0
        map.AddSegment(0, 100, "main.midl", 0);

        var result = map.Resolve(50);
        Assert.NotNull(result);
        Assert.Equal("main.midl", result.Value.filePath);
        Assert.Equal(50, result.Value.originalOffset);
    }

    [Fact]
    public void Resolve_IncludedFilePosition_ReturnsCorrectMapping()
    {
        var map = new SourceMap();
        map.AddSegment(0, 20, "main.midl", 0);
        map.AddSegment(20, 50, "included.h", 0);
        map.AddSegment(50, 80, "main.midl", 30);

        // Position 30 falls in the included file segment
        var result = map.Resolve(30);
        Assert.NotNull(result);
        Assert.Equal("included.h", result.Value.filePath);
        Assert.Equal(10, result.Value.originalOffset); // 30 - 20 = 10
    }

    [Fact]
    public void Resolve_MultipleIncludes_ResolvesEachCorrectly()
    {
        var map = new SourceMap();
        map.AddSegment(0, 10, "main.midl", 0);
        map.AddSegment(10, 30, "a.h", 0);
        map.AddSegment(30, 40, "main.midl", 20);
        map.AddSegment(40, 60, "b.h", 0);
        map.AddSegment(60, 80, "main.midl", 30);

        Assert.Equal("main.midl", map.Resolve(5)!.Value.filePath);
        Assert.Equal("a.h", map.Resolve(15)!.Value.filePath);
        Assert.Equal("main.midl", map.Resolve(35)!.Value.filePath);
        Assert.Equal("b.h", map.Resolve(50)!.Value.filePath);
        Assert.Equal("main.midl", map.Resolve(70)!.Value.filePath);
    }

    [Fact]
    public void Resolve_NestedIncludes_ResolvesCorrectly()
    {
        var map = new SourceMap();
        // main includes outer.h, which includes inner.h
        map.AddSegment(0, 10, "main.midl", 0);
        map.AddSegment(10, 20, "outer.h", 0);  // outer content before inner include
        map.AddSegment(20, 30, "inner.h", 0);  // inner.h content
        map.AddSegment(30, 40, "outer.h", 15); // outer content after inner include
        map.AddSegment(40, 50, "main.midl", 20);

        Assert.Equal("inner.h", map.Resolve(25)!.Value.filePath);
        Assert.Equal(5, map.Resolve(25)!.Value.originalOffset);
        Assert.Equal("outer.h", map.Resolve(35)!.Value.filePath);
    }

    [Fact]
    public void Resolve_OutOfRange_ReturnsNull()
    {
        var map = new SourceMap();
        map.AddSegment(10, 20, "main.midl", 0);

        Assert.Null(map.Resolve(5));   // before any segment
        Assert.Null(map.Resolve(20));  // at end (exclusive)
        Assert.Null(map.Resolve(100)); // way beyond
    }

    [Fact]
    public void HasMappings_EmptyMap_ReturnsFalse()
    {
        var map = new SourceMap();
        Assert.False(map.HasMappings);
    }

    [Fact]
    public void HasMappings_WithSegments_ReturnsTrue()
    {
        var map = new SourceMap();
        map.AddSegment(0, 10, "test.midl", 0);
        Assert.True(map.HasMappings);
    }

    private static SourceMap BuildMapFromPreprocessor(Preprocessor pp)
    {
        var map = new SourceMap();
        foreach (var seg in pp.SourceMapSegments)
            map.AddSegment(seg.ExpandedStart, seg.ExpandedEnd, seg.FilePath, seg.OriginalStart);
        map.SortSegments();
        return map;
    }

    [Fact]
    public void Preprocessor_BuildsSourceMap_DuringIncludeExpansion()
    {
        // Create test files
        var mainFile = Path.Combine(_tempDir, "main.midl");
        var headerFile = Path.Combine(_tempDir, "header.h");

        File.WriteAllText(headerFile, "int x;\nint y;\n");
        File.WriteAllText(mainFile, "// preamble\n#include \"header.h\"\n// postamble\n");

        var pp = new Preprocessor(null, new[] { _tempDir }, mainFile);
        var result = pp.Process(File.ReadAllText(mainFile));

        Assert.True(pp.SourceMapSegments.Count > 0, "Preprocessor should produce source map segments");

        var map = BuildMapFromPreprocessor(pp);

        Assert.True(map.HasMappings);

        // The preamble is in the expanded output at offset 0
        var preambleResult = map.Resolve(0);
        Assert.NotNull(preambleResult);
        Assert.Equal(Path.GetFullPath(mainFile), preambleResult.Value.filePath);

        // Find where the included content is
        int includedStart = result.IndexOf("int x");
        Assert.True(includedStart >= 0, "Included content should appear in expanded text");

        var includedResult = map.Resolve(includedStart);
        Assert.NotNull(includedResult);
        Assert.Equal(Path.GetFullPath(headerFile), includedResult.Value.filePath);
    }

    [Fact]
    public void Preprocessor_NoIncludes_NoSegments()
    {
        var pp = new Preprocessor(null, null, null);
        pp.Process("namespace Foo { }");

        Assert.Empty(pp.SourceMapSegments);
    }

    [Fact]
    public void Preprocessor_MultipleIncludes_ProducesCorrectSegments()
    {
        var mainFile = Path.Combine(_tempDir, "main.midl");
        var header1 = Path.Combine(_tempDir, "a.h");
        var header2 = Path.Combine(_tempDir, "b.h");

        File.WriteAllText(header1, "// from a\n");
        File.WriteAllText(header2, "// from b\n");
        File.WriteAllText(mainFile, "// start\n#include \"a.h\"\n// middle\n#include \"b.h\"\n// end\n");

        var pp = new Preprocessor(null, new[] { _tempDir }, mainFile);
        var result = pp.Process(File.ReadAllText(mainFile));

        var map = BuildMapFromPreprocessor(pp);
        int aStart = result.IndexOf("// from a");
        int bStart = result.IndexOf("// from b");
        int middleStart = result.IndexOf("// middle");

        Assert.True(aStart >= 0);
        Assert.True(bStart >= 0);
        Assert.True(middleStart >= 0);

        Assert.Equal(Path.GetFullPath(header1), map.Resolve(aStart)!.Value.filePath);
        Assert.Equal(Path.GetFullPath(header2), map.Resolve(bStart)!.Value.filePath);
        Assert.Equal(Path.GetFullPath(mainFile), map.Resolve(middleStart)!.Value.filePath);
    }

    [Fact]
    public void Preprocessor_NestedIncludes_ProducesCorrectSegments()
    {
        var mainFile = Path.Combine(_tempDir, "main.midl");
        var outer = Path.Combine(_tempDir, "outer.h");
        var inner = Path.Combine(_tempDir, "inner.h");

        File.WriteAllText(inner, "// inner content\n");
        File.WriteAllText(outer, "// outer before\n#include \"inner.h\"\n// outer after\n");
        File.WriteAllText(mainFile, "// main before\n#include \"outer.h\"\n// main after\n");

        var pp = new Preprocessor(null, new[] { _tempDir }, mainFile);
        var result = pp.Process(File.ReadAllText(mainFile));

        var map = BuildMapFromPreprocessor(pp);

        int innerStart = result.IndexOf("// inner content");
        int outerBeforeStart = result.IndexOf("// outer before");
        int mainAfterStart = result.IndexOf("// main after");

        Assert.True(innerStart >= 0);
        Assert.True(outerBeforeStart >= 0);
        Assert.True(mainAfterStart >= 0);

        Assert.Equal(Path.GetFullPath(inner), map.Resolve(innerStart)!.Value.filePath);
        Assert.Equal(Path.GetFullPath(outer), map.Resolve(outerBeforeStart)!.Value.filePath);
        Assert.Equal(Path.GetFullPath(mainFile), map.Resolve(mainAfterStart)!.Value.filePath);
    }

    [Fact]
    public void Compilation_StoresSourceMap_WhenIncludesPresent()
    {
        var mainFile = Path.Combine(_tempDir, "main.midl");
        var header = Path.Combine(_tempDir, "types.h");

        File.WriteAllText(header, "enum MyEnum { Value1, Value2 };\n");
        File.WriteAllText(mainFile, "#include \"types.h\"\nnamespace Test { }\n");

        var compilation = CompilationHelper.Compile(
            File.ReadAllText(mainFile),
            referencePaths: null,
            CompilationMode.Compat,
            includePaths: new[] { _tempDir },
            currentFilePath: mainFile);

        Assert.NotNull(compilation.SourceMap);
        Assert.True(compilation.SourceMap.HasMappings);
    }

    [Fact]
    public void Compilation_NoSourceMap_WithoutIncludes()
    {
        var compilation = CompilationHelper.Compile("namespace Test { }");
        Assert.Null(compilation.SourceMap);
    }
}
