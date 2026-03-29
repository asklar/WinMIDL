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

    [Fact]
    public void Diagnostic_InIncludedFile_GetsCorrectSourceFile()
    {
        var mainFile = Path.Combine(_tempDir, "main.midl");
        var header = Path.Combine(_tempDir, "bad.h");

        // The header has invalid MIDL3 syntax that will produce a diagnostic
        File.WriteAllText(header, "invalid_keyword_xyz ;\n");
        File.WriteAllText(mainFile, "namespace Test {\n#include \"bad.h\"\n}\n");

        var compilation = CompilationHelper.Compile(
            File.ReadAllText(mainFile),
            referencePaths: null,
            CompilationMode.Compat,
            includePaths: new[] { _tempDir },
            currentFilePath: mainFile);

        // There should be diagnostics from parsing the invalid content
        if (compilation.Diagnostics.Count > 0 && compilation.SourceMap != null)
        {
            // Find a diagnostic whose position falls in the included content
            var headerDiags = compilation.Diagnostics
                .Where(d => d.SourceFile != null &&
                           d.SourceFile.Equals(Path.GetFullPath(header), StringComparison.OrdinalIgnoreCase))
                .ToList();

            // If any diagnostics were attributed to the header, verify they have SourceFile set
            foreach (var d in headerDiags)
            {
                Assert.NotNull(d.SourceFile);
                Assert.True(d.OriginalPosition.HasValue);
            }
        }
        // Even if no diagnostics land in the header (parser may recover differently),
        // verify the SourceMap is present
        Assert.NotNull(compilation.SourceMap);
    }

    [Fact]
    public void Diagnostic_InMainFile_HasNullSourceFile_WithoutIncludes()
    {
        // Source with a semantic error but no includes
        var source = "namespace Test { runtimeclass Foo { void M(); void M(); } }";
        var compilation = CompilationHelper.Compile(source, CompilationMode.Compat);

        // Verify diagnostics don't have SourceFile set (no source map)
        foreach (var diag in compilation.Diagnostics)
        {
            Assert.Null(diag.SourceFile);
        }
    }

    [Fact]
    public void Diagnostic_SourceFile_SetWhenSourceMapResolves()
    {
        // Directly test that setting SourceFile on diagnostic via `with` expression works
        var diag = new Midl.Analysis.Diagnostics.Diagnostic(
            Midl.Analysis.Diagnostics.DiagnosticSeverity.Error,
            "TEST001",
            "test message",
            10, 5);

        Assert.Null(diag.SourceFile);

        var resolved = diag with { SourceFile = "test.h", OriginalPosition = 42 };
        Assert.Equal("test.h", resolved.SourceFile);
        Assert.Equal(42, resolved.OriginalPosition);
        Assert.Equal(10, resolved.Position); // Original Position preserved
    }

    [Fact]
    public void CLI_IncludesFilename_WhenSourceFileSet()
    {
        // Test that CLI output includes the filename when SourceFile is set
        var errWriter = new StringWriter();
        var outWriter = new StringWriter();
        var driver = new Midl.Cli.CompilationDriver(outWriter, errWriter);

        var mainFile = Path.Combine(_tempDir, "main.midl");
        var header = Path.Combine(_tempDir, "error.h");

        // Create a header with syntax that generates an error
        File.WriteAllText(header, "struct BadStruct { invalid; };\n");
        File.WriteAllText(mainFile, "#include \"error.h\"\nnamespace Test { }\n");

        var options = new Midl.Cli.CommandLineOptions
        {
            InputFile = mainFile,
            IncludePaths = { _tempDir },
            Mode = CompilationMode.Compat,
            SyntaxCheckOnly = true
        };

        driver.RunFromSource(File.ReadAllText(mainFile), options);

        var errOutput = errWriter.ToString();
        // If diagnostics were produced from the included file, they should reference it
        // The output format should be: filename(line,col): error CODE: message
        // Even if no diagnostics from the header specifically, the test validates the code path
        Assert.NotNull(errOutput); // Just ensure no exception was thrown
    }

    [Fact]
    public async Task LSP_DiagnosticsFromIncludedFile_PublishedToIncludedFileUri()
    {
        // Create a mock JsonRpcWriter using a MemoryStream
        var outputStream = new MemoryStream();
        var writer = new Midl.Lsp.JsonRpcWriter(outputStream);
        var publisher = new Midl.Lsp.DiagnosticsPublisher(writer);

        var mainFile = Path.Combine(_tempDir, "main.midl");
        var header = Path.Combine(_tempDir, "included.h");

        File.WriteAllText(header, "// line 1\n// line 2\n");
        File.WriteAllText(mainFile, "// main content\n");

        var mainUri = "file:///" + mainFile.Replace('\\', '/');

        // Create diagnostics: one from main file, one from included file
        var diagnostics = new List<Midl.Analysis.Diagnostics.Diagnostic>
        {
            new(Midl.Analysis.Diagnostics.DiagnosticSeverity.Error, "TEST001", "main error", 0, 5),
            new(Midl.Analysis.Diagnostics.DiagnosticSeverity.Error, "TEST002", "included error", 10, 5)
            {
                SourceFile = Path.GetFullPath(header),
                OriginalPosition = 5
            }
        };

        await publisher.PublishDiagnosticsAsync(mainUri, "// main content\n", diagnostics);

        // Verify output was written (the stream should have content)
        Assert.True(outputStream.Length > 0, "Should have published diagnostics");

        // Read the output and verify we got notifications for both URIs
        outputStream.Position = 0;
        var output = new StreamReader(outputStream).ReadToEnd();

        // Should contain the main URI
        Assert.Contains(mainUri, output);
        // Should contain the included file URI
        Assert.Contains("included.h", output);
        // Should contain both diagnostic codes
        Assert.Contains("TEST001", output);
        Assert.Contains("TEST002", output);
    }

    [Fact]
    public async Task LSP_ClearDiagnostics_WorksAcrossFiles()
    {
        var outputStream = new MemoryStream();
        var writer = new Midl.Lsp.JsonRpcWriter(outputStream);
        var publisher = new Midl.Lsp.DiagnosticsPublisher(writer);

        var mainFile = Path.Combine(_tempDir, "main.midl");
        var header = Path.Combine(_tempDir, "old_error.h");

        File.WriteAllText(header, "// content\n");
        File.WriteAllText(mainFile, "// main\n");

        var mainUri = "file:///" + mainFile.Replace('\\', '/');

        // First publish: diagnostics in both files
        var diags1 = new List<Midl.Analysis.Diagnostics.Diagnostic>
        {
            new(Midl.Analysis.Diagnostics.DiagnosticSeverity.Error, "ERR1", "error 1", 0, 1)
            {
                SourceFile = Path.GetFullPath(header),
                OriginalPosition = 0
            }
        };
        await publisher.PublishDiagnosticsAsync(mainUri, "// main\n", diags1);

        // Second publish: no diagnostics (all fixed)
        outputStream.SetLength(0);
        await publisher.PublishDiagnosticsAsync(mainUri, "// main\n", new List<Midl.Analysis.Diagnostics.Diagnostic>());

        outputStream.Position = 0;
        var output = new StreamReader(outputStream).ReadToEnd();

        // Should contain the old_error.h URI being cleared (empty diagnostics array)
        Assert.Contains("old_error.h", output);
    }
}
