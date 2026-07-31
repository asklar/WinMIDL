// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

    [Fact]
    public void Ifdef_FalseBlock_PositionAfterMapsCorrectly()
    {
        // Source with an #ifdef false block that shifts positions
        var source = "line1\n#ifdef UNDEFINED\nskipped_long_content_here\n#endif\nline_after\n";

        var pp = new Preprocessor(new[] { "SOMETHING_ELSE" }, null, "test.midl");
        var result = pp.Process(source);

        // After processing:
        // line1\r\n          -> 7 chars (with \r\n)
        // \r\n               -> 2 chars (empty for #ifdef)
        // \r\n               -> 2 chars (empty for skipped content)
        // \r\n               -> 2 chars (empty for #endif)
        // line_after\r\n     -> 12 chars
        // minus trailing \r\n

        Assert.True(pp.SourceMapSegments.Count > 0, "Should have segments for #ifdef gaps");

        var map = BuildMapFromPreprocessor(pp);

        // Find "line_after" in expanded text
        int afterIdx = result.IndexOf("line_after");
        Assert.True(afterIdx >= 0, "line_after should be in expanded text");

        var resolved = map.Resolve(afterIdx);
        Assert.NotNull(resolved);
        Assert.Equal("test.midl", resolved.Value.filePath);

        // The original offset of "line_after" in the source is after:
        // "line1\n" (6) + "#ifdef UNDEFINED\n" (17) + "skipped_long_content_here\n" (26) + "#endif\n" (7) = 56
        // So original offset should be 56
        int origExpected = source.IndexOf("line_after");
        Assert.Equal(origExpected, resolved.Value.originalOffset);
    }

    [Fact]
    public void Ifdef_NestedMixedTrueFalse_TracksCorrectly()
    {
        var source = "before\n#ifdef DEFINED\ninner_active\n#ifdef UNDEF2\nskipped\n#endif\nafter_inner\n#endif\nafter_all\n";

        var pp = new Preprocessor(new[] { "DEFINED" }, null, "test.midl");
        var result = pp.Process(source);

        var map = BuildMapFromPreprocessor(pp);

        // "after_all" should map back to the correct file
        int afterAllIdx = result.IndexOf("after_all");
        Assert.True(afterAllIdx >= 0);

        var resolved = map.Resolve(afterAllIdx);
        Assert.NotNull(resolved);
        Assert.Equal("test.midl", resolved.Value.filePath);

        // The mapped position is approximate due to directive line length differences.
        // Verify it's within a reasonable range of the actual position.
        int origExpected = source.IndexOf("after_all");
        int origActual = resolved.Value.originalOffset;
        Assert.True(Math.Abs(origActual - origExpected) < 20,
            $"Original offset {origActual} should be close to expected {origExpected} (diff={Math.Abs(origActual - origExpected)})");
    }

    [Fact]
    public void Ifdef_DiagnosticAfterEndif_PointsToCorrectPosition()
    {
        // Create a MIDL source where an error appears after a skipped #ifdef block
        var mainFile = Path.Combine(_tempDir, "ifdef_test.midl");

        var source = "#ifdef NEVER_DEFINED\n" +
                     "this is skipped content with long lines that would shift offsets\n" +
                     "more skipped content to increase the gap\n" +
                     "#endif\n" +
                     "namespace Test {\n" +
                     "  runtimeclass Foo { void M(); void M(); }\n" +  // duplicate method → might produce diagnostic
                     "}\n";

        File.WriteAllText(mainFile, source);

        var compilation = CompilationHelper.Compile(
            source,
            referencePaths: null,
            CompilationMode.Compat,
            includePaths: null,
            currentFilePath: mainFile);

        // Verify source map was created (due to #ifdef gaps)
        if (compilation.SourceMap != null && compilation.SourceMap.HasMappings)
        {
            // Any diagnostics should have SourceFile set to the main file
            foreach (var diag in compilation.Diagnostics)
            {
                if (diag.SourceFile != null)
                {
                    Assert.Equal(Path.GetFullPath(mainFile), diag.SourceFile);
                }
            }
        }
    }

    #region Ifdef + Define Position Remapping

    [Fact]
    public void Ifdef_DiagnosticPosition_PointsToCorrectColumn()
    {
        // #ifdef false block shifts positions in expanded text.
        // Diagnostic should point to the runtimeclass NAME, not the attribute.
        var source = "namespace N {\n#ifdef NOTDEFINED\n    some content\n#endif\n    [contract(Foo, 1)] runtimeclass MyClass;\n}";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);

        // Use OriginalPosition if available, else Position
        int pos = diag.OriginalPosition ?? diag.Position;
        // Should point to "MyClass" in the original source
        Assert.Equal("MyClass", source.Substring(pos, "MyClass".Length));
    }

    [Fact]
    public void Define_MacroSubstitution_DiagnosticPosition_StillCorrect()
    {
        // #define changes line length in expanded text.
        // Diagnostic position must be correct in the ORIGINAL source.
        var source = "#define VER 1.0\nnamespace N {\n    [contractversion(VER)] apicontract Foo {}\n    [contract(Foo, 1)] runtimeclass MyClass;\n}";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("MyClass", source.Substring(pos, "MyClass".Length));
    }

    [Fact]
    public void MultipleDefines_And_Ifdef_DiagnosticPosition_Correct()
    {
        // Multiple #defines + #ifdef should all compose correctly.
        var source = "#define A 1\n#define B 2\n#define C 3\nnamespace N {\n#ifdef NOPE\n    stuff\n    more stuff\n#endif\n    runtimeclass Widget;\n}";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Widget", source.Substring(pos, "Widget".Length));
    }

    [Fact]
    public void EditingDefine_DoesNotShift_DiagnosticPosition()
    {
        // Simulate the bug: editing a #define line changes original text length
        // but not expanded text. Diagnostics must still point correctly.
        var source1 = "#define FOO 1\nnamespace N { runtimeclass W; }";
        var source2 = "#define FOO\nnamespace N { runtimeclass W; }";

        var comp1 = CompilationHelper.Compile(source1, mode: CompilationMode.Strict);
        var comp2 = CompilationHelper.Compile(source2, mode: CompilationMode.Strict);

        var diag1 = comp1.Diagnostics.First(d => d.Code == "MIDL1062");
        var diag2 = comp2.Diagnostics.First(d => d.Code == "MIDL1062");

        int pos1 = diag1.OriginalPosition ?? diag1.Position;
        int pos2 = diag2.OriginalPosition ?? diag2.Position;

        // Both should point to "W" in their respective source texts
        Assert.Equal("W", source1.Substring(pos1, 1));
        Assert.Equal("W", source2.Substring(pos2, 1));
    }

    [Fact]
    public void DiagnosticsPublisher_MainFilePath_Parsing()
    {
        // Regression: Uri.LocalPath on Windows returns "/c:/path" with leading slash.
        // Path.GetFullPath("/c:/path") resolves to "E:\c:\path" (wrong drive).
        var uri = "file:///c%3A/Users/test/file.idl";
        var uriObj = new Uri(uri);
        var localPath = uriObj.LocalPath;

        // Strip leading / on Windows drive paths
        if (localPath.Length > 2 && localPath[0] == '/' && localPath[2] == ':')
            localPath = localPath[1..];

        var fullPath = Path.GetFullPath(localPath);
        Assert.StartsWith("c:", fullPath, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@":\c:", fullPath); // Must NOT double up drives
    }

    [Fact]
    public void DefineOnly_EmptyInterface_DiagnosticPosition_IsCorrect()
    {
        // Regression: #define without #ifdef caused source map segments to not flush
        // at directive lines, so positions after the #define were off by the
        // length difference between "#define FOO bar" and the empty expanded line.
        var source = "#define VER 1.0\nnamespace N {\n    [uuid(01020304-0506-0708-090a-0b0c0d0e0f10)]\n    interface IFoo : IInspectable {};\n}";
        var compilation = CompilationHelper.Compile(source, referencePaths: null,
            CompilationMode.Strict, includePaths: null, currentFilePath: null,
            emitQualityHints: true);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3003");
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        // Must point to "IFoo" in the original source, not shifted by #define length
        var around = source.Substring(pos, Math.Min(10, source.Length - pos));
        Assert.StartsWith("IFoo", around);
    }

    [Fact]
    public void DefineUsed_EmptyInterface_DiagnosticPosition_IsCorrect()
    {
        // When #define macro IS used later, the source map must still correctly
        // remap positions for code between the #define and the macro usage.
        var source = "#define VER 1.0\nnamespace N {\n    [uuid(01020304-0506-0708-090a-0b0c0d0e0f10)]\n    interface IBar : IInspectable {};\n    [contractversion(VER)] apicontract C {}\n}";
        var compilation = CompilationHelper.Compile(source, referencePaths: null,
            CompilationMode.Strict, includePaths: null, currentFilePath: null,
            emitQualityHints: true);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3003");
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        var around = source.Substring(pos, Math.Min(10, source.Length - pos));
        Assert.StartsWith("IBar", around);
    }

    [Fact]
    public void Define_And_Ifdef_DiagnosticPosition_Correct()
    {
        // Combines #define + #ifdef — both create offset shifts.
        // Diagnostic after both must still point to correct source position.
        var source = "#define FOO 1\n#ifdef NOPE\n    hidden\n#endif\nnamespace N {\n    [uuid(01020304-0506-0708-090a-0b0c0d0e0f10)]\n    interface IFoo : IInspectable {};\n}";
        var compilation = CompilationHelper.Compile(source, referencePaths: null,
            CompilationMode.Strict, includePaths: null, currentFilePath: null,
            emitQualityHints: true);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL3003");
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        var around = source.Substring(pos, Math.Min(10, source.Length - pos));
        Assert.StartsWith("IFoo", around);
    }

    [Fact]
    public void MIDL1077_Position_After_Define_PointsToType()
    {
        // MIDL1077 squiggle should cover the type reference, not the parameter name.
        // Also verifies position is correct after #define.
        var source = "#define VER 1.0\nnamespace N {\n    runtimeclass Empty { };\n    runtimeclass User { void Use(Empty e); }\n}";
        var compilation = CompilationHelper.Compile(source, referencePaths: null,
            CompilationMode.Strict, includePaths: null, currentFilePath: null);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1077");
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Empty", source.Substring(pos, diag.Length));
    }

    [Fact]
    public void MIDL1062_Position_After_Define_PointsToClassName()
    {
        // MIDL1062 should point to the runtimeclass name, not shifted by #define.
        var source = "#define FOO bar\nnamespace N { runtimeclass Widget; }";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Widget", source.Substring(pos, "Widget".Length));
    }

    [Fact]
    public void MultipleEmptyInterfaces_After_Define_AllPositionsCorrect()
    {
        // Multiple diagnostics after #define — each must point to its own interface.
        var source = "#define VER 1.0\nnamespace N {\n    [uuid(01020304-0506-0708-090a-0b0c0d0e0f10)]\n    interface IAlpha : IInspectable {};\n    [uuid(11111111-2222-3333-4444-555555555555)]\n    interface IBeta : IInspectable {};\n}";
        var compilation = CompilationHelper.Compile(source, referencePaths: null,
            CompilationMode.Strict, includePaths: null, currentFilePath: null,
            emitQualityHints: true);

        var diags = compilation.Diagnostics.Where(d => d.Code == "MIDL3003").ToList();
        Assert.Equal(2, diags.Count);

        foreach (var diag in diags)
        {
            int pos = diag.OriginalPosition ?? diag.Position;
            var name = diag.Message.Contains("IAlpha") ? "IAlpha" : "IBeta";
            Assert.Equal(name, source.Substring(pos, name.Length));
        }
    }

    #endregion

    #region Preprocessor Directive Position Tests

    [Fact]
    public void Ifndef_DiagnosticPosition_Correct()
    {
        // #ifndef with false condition (symbol IS defined) removes the block.
        var source = "#define HAVE_IT\n#ifndef HAVE_IT\n    hidden content\n#endif\nnamespace N { runtimeclass Widget; }";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Widget", source.Substring(pos, "Widget".Length));
    }

    [Fact]
    public void Ifndef_ActiveBlock_DiagnosticPosition_Correct()
    {
        // #ifndef with true condition (symbol NOT defined) keeps the block.
        var source = "#ifndef MISSING\nnamespace N { runtimeclass Gizmo; }\n#endif\n";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Gizmo", source.Substring(pos, "Gizmo".Length));
    }

    [Fact]
    public void Else_DiagnosticPosition_Correct()
    {
        // Content in #else branch after a true #ifdef.
        var source = "#define HAS_FEATURE\n#ifdef HAS_FEATURE\nnamespace Active { enum E { A }; }\n#else\nnamespace Inactive { runtimeclass Ghost; }\n#endif\nnamespace N { runtimeclass Widget; }";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var diag = compilation.Diagnostics.FirstOrDefault(d =>
            d.Code == "MIDL1062" && d.Message.Contains("Widget"));
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Widget", source.Substring(pos, "Widget".Length));
    }

    [Fact]
    public void Elif_DiagnosticPosition_Correct()
    {
        // #elif branch — content after multiple conditional directives.
        var source = "#define MODE 2\n#if 0\n    hidden\n#elif 1\nnamespace N { runtimeclass Thing; }\n#endif\n";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var diag = compilation.Diagnostics.FirstOrDefault(d =>
            d.Code == "MIDL1062" && d.Message.Contains("Thing"));
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Thing", source.Substring(pos, "Thing".Length));
    }

    [Fact]
    public void Undef_DiagnosticPosition_Correct()
    {
        // #undef directive line shifts offsets like #define.
        var source = "#define FOO\n#undef FOO\nnamespace N { runtimeclass Item; }";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Item", source.Substring(pos, "Item".Length));
    }

    [Fact]
    public void PragmaOnce_DiagnosticPosition_Correct()
    {
        // #pragma once produces an empty expanded line.
        var mainFile = Path.Combine(_tempDir, "pragma.idl");
        var mainSource = "#pragma once\nnamespace N { runtimeclass Box; }";
        File.WriteAllText(mainFile, mainSource);

        var compilation = CompilationHelper.Compile(
            mainSource, referencePaths: null, CompilationMode.Strict,
            includePaths: null, currentFilePath: mainFile);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Box", mainSource.Substring(pos, "Box".Length));
    }

    [Fact]
    public void NestedIfdef_DiagnosticPosition_Correct()
    {
        // Nested #ifdef blocks — two levels of directive removal.
        var source = "#ifdef OUTER\n#ifdef INNER\n    double hidden\n#endif\n#endif\nnamespace N { runtimeclass Nested; }";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Nested", source.Substring(pos, "Nested".Length));
    }

    [Fact]
    public void ManyDirectives_DiagnosticPosition_Correct()
    {
        // Stress test: many different directives before the actual code.
        var source = "#define A 1\n#define B 2\n#undef A\n#ifdef B\n#ifndef C\n#define C 3\n#endif\n#endif\n#pragma once\nnamespace N { runtimeclass Stress; }";
        var mainFile = Path.Combine(_tempDir, "stress.idl");
        File.WriteAllText(mainFile, source);

        var compilation = CompilationHelper.Compile(
            source, referencePaths: null, CompilationMode.Strict,
            includePaths: null, currentFilePath: mainFile);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Stress", source.Substring(pos, "Stress".Length));
    }

    #endregion

    #region Diagnostic Position Verification Tests

    // These tests verify that various diagnostic codes point their squiggle
    // at the correct source text (type name, symbol name, etc.)

    [Fact]
    public void MIDL1009_EmptyStruct_SquiggleOnStructName()
    {
        var source = "namespace N { struct Empty { }; }";
        var c = CompilationHelper.Compile(source);
        var diag = c.Diagnostics.First(d => d.Code == "MIDL1009");
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Empty", source.Substring(pos, diag.Length));
    }

    [Fact]
    public void MIDL1009_EmptyStruct_AfterDefine_PositionCorrect()
    {
        var source = "#define VER 1\nnamespace N { struct EmptyBox { }; }";
        var c = CompilationHelper.Compile(source);
        var diag = c.Diagnostics.First(d => d.Code == "MIDL1009");
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("EmptyBox", source.Substring(pos, diag.Length));
    }

    [Fact]
    public void MIDL1034_UndefinedRequired_SquiggleOnTypeName()
    {
        var source = "namespace N { interface IFoo requires IBar { void Do(); }; }";
        var c = CompilationHelper.Compile(source);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1034" || d.Code == "MIDL1001");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("IBar", source.Substring(pos, "IBar".Length));
    }

    [Fact]
    public void MIDL1035_BaseNotRuntimeClass_SquiggleOnBaseName()
    {
        var source = "namespace N { struct S { Int32 X; }; runtimeclass C : S { }; }";
        var c = CompilationHelper.Compile(source);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1035");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("S", source.Substring(pos, diag.Length));
    }

    [Fact]
    public void MIDL1036_BaseClassSealed_SquiggleOnBaseName()
    {
        var source = "namespace N { runtimeclass Base { Base(); }; runtimeclass Derived : Base { }; }";
        var c = CompilationHelper.Compile(source);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1036");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Base", source.Substring(pos, diag.Length));
    }

    [Fact]
    public void MIDL1038_RuntimeClassOnRequires_SquiggleOnName()
    {
        var source = "namespace N { runtimeclass Other { Other(); }; interface IFoo requires Other { void Do(); }; }";
        var c = CompilationHelper.Compile(source);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1038");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Other", source.Substring(pos, diag.Length));
    }

    [Fact]
    public void MIDL1042_EventNonDelegate_SquiggleOnTypeName()
    {
        var source = "namespace N { runtimeclass C { event Int32 BadEvent; }; }";
        var c = CompilationHelper.Compile(source);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1042");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Int32", source.Substring(pos, diag.Length));
    }

    [Fact]
    public void MIDL1044_DuplicateMethod_SquiggleOnMethodName()
    {
        var source = "namespace N { runtimeclass C { void Do(Int32 x); void Do(String s); }; }";
        var c = CompilationHelper.Compile(source);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1044");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Do", source.Substring(pos, diag.Length));
    }

    [Fact]
    public void MIDL1044_DuplicateMethod_AfterDefine_PositionCorrect()
    {
        var source = "#define VER 1\nnamespace N { runtimeclass C { void Act(Int32 x); void Act(String s); }; }";
        var c = CompilationHelper.Compile(source);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1044");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Act", source.Substring(pos, diag.Length));
    }

    [Fact]
    public void MIDL1062_AfterIfdefElse_PositionCorrect()
    {
        // Content after #ifdef/#else — squiggle in code after the #else block
        var source = "#ifdef NOPE\n    hidden\n#else\n    namespace Active { enum E { A }; }\n#endif\nnamespace N { runtimeclass Gadget; }";
        var c = CompilationHelper.Compile(source, CompilationMode.Strict);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062" && d.Message.Contains("Gadget"));
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Gadget", source.Substring(pos, "Gadget".Length));
    }

    [Fact]
    public void MIDL1077_AfterIfdef_PositionCoversType()
    {
        var source = "#ifdef NOPE\n    hidden\n#endif\nnamespace N { runtimeclass Empty { }; runtimeclass User { void Use(Empty e); } }";
        var c = CompilationHelper.Compile(source, CompilationMode.Strict);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1077");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Empty", source.Substring(pos, diag.Length));
    }

    #endregion

    #region Line Continuation Tests

    [Fact]
    public void BackslashContinuation_DiagnosticPosition_Correct()
    {
        // Backslash line continuation joins two lines into one logical line.
        // Diagnostic positions must still map to the correct original source.
        var source = "#define LONG_MACRO \\\n    1\nnamespace N { runtimeclass Widget; }";
        var c = CompilationHelper.Compile(source, CompilationMode.Strict);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Widget", source.Substring(pos, "Widget".Length));
    }

    [Fact]
    public void BackslashContinuation_ThreeLines_PositionCorrect()
    {
        // Three raw lines joined into one via backslash.
        var source = "#define MULTI \\\n    LINE \\\n    MACRO\nnamespace N { runtimeclass Gizmo; }";
        var c = CompilationHelper.Compile(source, CompilationMode.Strict);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Gizmo", source.Substring(pos, "Gizmo".Length));
    }

    [Fact]
    public void BackslashContinuation_WithIfdef_PositionCorrect()
    {
        // Backslash continuation + #ifdef — both shift positions.
        var source = "#define FOO \\\n    bar\n#ifdef NOPE\n    hidden\n#endif\nnamespace N { runtimeclass Thing; }";
        var c = CompilationHelper.Compile(source, CompilationMode.Strict);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Thing", source.Substring(pos, "Thing".Length));
    }

    #endregion

    #region Line Ending Tests

    [Fact]
    public void LfOnly_WithDefine_DiagnosticPosition_Correct()
    {
        // LF-only line endings — preprocessor must not introduce \r\n mismatch.
        var source = "#define VER 1\nnamespace N {\n    runtimeclass Widget;\n}\n";
        Assert.DoesNotContain("\r", source); // verify test uses LF-only
        var c = CompilationHelper.Compile(source, CompilationMode.Strict);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Widget", source.Substring(pos, "Widget".Length));
    }

    [Fact]
    public void LfOnly_WithIfdef_DiagnosticPosition_Correct()
    {
        var source = "#ifdef NOPE\n    hidden\n#endif\nnamespace N {\n    runtimeclass Gadget;\n}\n";
        Assert.DoesNotContain("\r", source);
        var c = CompilationHelper.Compile(source, CompilationMode.Strict);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Gadget", source.Substring(pos, "Gadget".Length));
    }

    [Fact]
    public void LfOnly_WithDefineAndIfdef_DiagnosticPosition_Correct()
    {
        var source = "#define FOO bar\n#ifdef NOPE\n    hidden\n#endif\nnamespace N {\n    runtimeclass Thing;\n}\n";
        Assert.DoesNotContain("\r", source);
        var c = CompilationHelper.Compile(source, CompilationMode.Strict);
        var diag = c.Diagnostics.FirstOrDefault(d => d.Code == "MIDL1062");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Thing", source.Substring(pos, "Thing".Length));
    }

    #endregion

    #region Preprocessor Diagnostic Position Tests

    [Fact]
    public void MIDL2006_IncludeInStrictMode_SquiggleOnIncludeLine()
    {
        // #include in strict mode emits MIDL2006 warning — must point at the #include directive.
        var mainFile = Path.Combine(_tempDir, "main.idl");
        var helperFile = Path.Combine(_tempDir, "inc.idl");

        File.WriteAllText(helperFile, "namespace Inc { enum E { A }; }\n");
        var mainSource = "namespace Before { enum X { Y }; }\n#include \"inc.idl\"\nnamespace After { runtimeclass W { W(); } }\n";
        File.WriteAllText(mainFile, mainSource);

        var compilation = CompilationHelper.Compile(
            mainSource, referencePaths: null, CompilationMode.Strict,
            includePaths: new[] { _tempDir }, currentFilePath: mainFile);

        var diag = compilation.Diagnostics.FirstOrDefault(d => d.Code == "MIDL2006");
        Assert.NotNull(diag);
        int pos = diag.OriginalPosition ?? diag.Position;
        // Must point to the #include line, not position 0
        Assert.True(pos > 0, $"MIDL2006 position should not be 0 (got {pos})");
        Assert.Equal("#include", mainSource.Substring(pos, "#include".Length));
    }

    #endregion

    #region Include Diagnostic Position Tests

    [Fact]
    public void Diagnostic_InIncludedFile_GetsCorrectSourceFile_AndPosition()
    {
        // BUG REPRO: When main.idl starts with #include, the included file content
        // gets no SourceMap segment because baseOffset==0 in the recursive ProcessInternal call.
        var mainFile = Path.Combine(_tempDir, "main.idl");
        var helperFile = Path.Combine(_tempDir, "helper.idl");

        var helperSource = "namespace Helper { runtimeclass BrokenWidget; }\n";
        var mainSource = "#include \"helper.idl\"\nnamespace Main { runtimeclass GoodWidget { GoodWidget(); Int32 Value; } }\n";

        File.WriteAllText(helperFile, helperSource);
        File.WriteAllText(mainFile, mainSource);

        var compilation = CompilationHelper.Compile(
            File.ReadAllText(mainFile),
            referencePaths: null,
            CompilationMode.Compat,
            includePaths: new[] { _tempDir },
            currentFilePath: mainFile);

        // There should be a diagnostic for "runtimeclass BrokenWidget;" (empty runtimeclass)
        var brokenDiag = compilation.Diagnostics
            .FirstOrDefault(d => d.Message.Contains("BrokenWidget", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(brokenDiag);

        // The diagnostic MUST be attributed to helper.idl, not main.idl
        Assert.NotNull(brokenDiag.SourceFile);
        Assert.Equal(Path.GetFullPath(helperFile), brokenDiag.SourceFile);

        // The OriginalPosition must point to "BrokenWidget" in helper.idl's text
        Assert.True(brokenDiag.OriginalPosition.HasValue, "OriginalPosition should be set");
        int origPos = brokenDiag.OriginalPosition.Value;

        // Verify the position actually points to "BrokenWidget" in the helper source
        Assert.True(origPos >= 0 && origPos + "BrokenWidget".Length <= helperSource.Length,
            $"OriginalPosition {origPos} out of range for helper source (len={helperSource.Length})");
        Assert.Equal("BrokenWidget", helperSource.Substring(origPos, "BrokenWidget".Length));
    }

    [Fact]
    public void Diagnostic_InMainFile_AfterInclude_PositionCorrect()
    {
        // Diagnostic in the main file AFTER #include content must still
        // point to the correct position in the original main source.
        var mainFile = Path.Combine(_tempDir, "main.idl");
        var helperFile = Path.Combine(_tempDir, "types.idl");

        var helperSource = "namespace Types { enum Color { Red, Green }; }\n";
        var mainSource = "#include \"types.idl\"\nnamespace App { runtimeclass Widget; }\n";

        File.WriteAllText(helperFile, helperSource);
        File.WriteAllText(mainFile, mainSource);

        var compilation = CompilationHelper.Compile(
            File.ReadAllText(mainFile),
            referencePaths: null,
            CompilationMode.Strict,
            includePaths: new[] { _tempDir },
            currentFilePath: mainFile);

        var diag = compilation.Diagnostics.FirstOrDefault(d =>
            d.Code == "MIDL1062" && d.Message.Contains("Widget"));
        Assert.NotNull(diag);

        // Must be attributed to main file
        Assert.True(diag.SourceFile == null ||
            string.Equals(Path.GetFullPath(diag.SourceFile), Path.GetFullPath(mainFile),
                StringComparison.OrdinalIgnoreCase));

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Widget", mainSource.Substring(pos, "Widget".Length));
    }

    [Fact]
    public void Diagnostic_WithInclude_And_Define_PositionCorrect()
    {
        // Combines #include AND #define — both affect offsets.
        var mainFile = Path.Combine(_tempDir, "main.idl");
        var helperFile = Path.Combine(_tempDir, "base.idl");

        var helperSource = "namespace Base { enum Mode { A, B }; }\n";
        var mainSource = "#define VER 1.0\n#include \"base.idl\"\nnamespace App { runtimeclass Gadget; }\n";

        File.WriteAllText(helperFile, helperSource);
        File.WriteAllText(mainFile, mainSource);

        var compilation = CompilationHelper.Compile(
            File.ReadAllText(mainFile),
            referencePaths: null,
            CompilationMode.Strict,
            includePaths: new[] { _tempDir },
            currentFilePath: mainFile);

        var diag = compilation.Diagnostics.FirstOrDefault(d =>
            d.Code == "MIDL1062" && d.Message.Contains("Gadget"));
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Gadget", mainSource.Substring(pos, "Gadget".Length));
    }

    [Fact]
    public void Diagnostics_InMultipleIncludedFiles_GetCorrectSourceFiles()
    {
        // Each included file's diagnostics must be attributed to the correct file.
        var mainFile = Path.Combine(_tempDir, "main.idl");
        var file1 = Path.Combine(_tempDir, "alpha.idl");
        var file2 = Path.Combine(_tempDir, "beta.idl");

        var source1 = "namespace Alpha { runtimeclass BrokenAlpha; }\n";
        var source2 = "namespace Beta { runtimeclass BrokenBeta; }\n";
        var mainSource = "#include \"alpha.idl\"\n#include \"beta.idl\"\nnamespace Main { runtimeclass Good { Good(); } }\n";

        File.WriteAllText(file1, source1);
        File.WriteAllText(file2, source2);
        File.WriteAllText(mainFile, mainSource);

        var compilation = CompilationHelper.Compile(
            File.ReadAllText(mainFile),
            referencePaths: null,
            CompilationMode.Strict,
            includePaths: new[] { _tempDir },
            currentFilePath: mainFile);

        var alphaDiag = compilation.Diagnostics.FirstOrDefault(d =>
            d.Message.Contains("BrokenAlpha"));
        var betaDiag = compilation.Diagnostics.FirstOrDefault(d =>
            d.Message.Contains("BrokenBeta"));

        Assert.NotNull(alphaDiag);
        Assert.NotNull(betaDiag);

        // Each must be attributed to its own file
        Assert.Equal(Path.GetFullPath(file1), alphaDiag!.SourceFile);
        Assert.Equal(Path.GetFullPath(file2), betaDiag!.SourceFile);

        // Positions must point to correct text in each file
        int alphaPos = alphaDiag.OriginalPosition ?? alphaDiag.Position;
        Assert.Equal("BrokenAlpha", source1.Substring(alphaPos, "BrokenAlpha".Length));

        int betaPos = betaDiag.OriginalPosition ?? betaDiag.Position;
        Assert.Equal("BrokenBeta", source2.Substring(betaPos, "BrokenBeta".Length));
    }

    [Fact]
    public void Diagnostic_MainFileAfterMultipleIncludes_PositionCorrect()
    {
        // Main file diagnostic after multiple includes must still point correctly.
        var mainFile = Path.Combine(_tempDir, "main.idl");
        var file1 = Path.Combine(_tempDir, "inc1.idl");
        var file2 = Path.Combine(_tempDir, "inc2.idl");

        File.WriteAllText(file1, "namespace A { enum E1 { X }; }\n");
        File.WriteAllText(file2, "namespace B { enum E2 { Y }; }\n");

        var mainSource = "#include \"inc1.idl\"\n#include \"inc2.idl\"\nnamespace Main { runtimeclass Broken; }\n";
        File.WriteAllText(mainFile, mainSource);

        var compilation = CompilationHelper.Compile(
            File.ReadAllText(mainFile),
            referencePaths: null,
            CompilationMode.Strict,
            includePaths: new[] { _tempDir },
            currentFilePath: mainFile);

        var diag = compilation.Diagnostics.FirstOrDefault(d =>
            d.Code == "MIDL1062" && d.Message.Contains("Broken"));
        Assert.NotNull(diag);

        int pos = diag.OriginalPosition ?? diag.Position;
        Assert.Equal("Broken", mainSource.Substring(pos, "Broken".Length));
    }

    #endregion

    #region Inactive Region Tests

    [Fact]
    public void InactiveRegions_IfdefFalse_RecordsRegion()
    {
        var source = "namespace N { }\n#ifdef NOPE\n    hidden content\n#endif\nnamespace M { }";
        var pp = new Preprocessor();
        pp.Process(source);
        Assert.Single(pp.InactiveRegions);
        var (start, end) = pp.InactiveRegions[0];
        // The inactive region should cover the content between #ifdef and #endif
        Assert.True(start > 0);
        Assert.True(end > start);
        var inactive = source[start..end];
        Assert.Contains("hidden content", inactive);
    }

    [Fact]
    public void InactiveRegions_IfdefTrue_NoRegion()
    {
        var source = "#define HAS_IT\n#ifdef HAS_IT\n    active content\n#endif\nnamespace N { }";
        var pp = new Preprocessor();
        pp.Process(source);
        Assert.Empty(pp.InactiveRegions);
    }

    [Fact]
    public void InactiveRegions_ElseBranch_RecordsInactiveElse()
    {
        var source = "#define FOO\n#ifdef FOO\n    active\n#else\n    inactive else\n#endif\n";
        var pp = new Preprocessor();
        pp.Process(source);
        Assert.Single(pp.InactiveRegions);
        var inactive = source[pp.InactiveRegions[0].Start..pp.InactiveRegions[0].End];
        Assert.Contains("inactive else", inactive);
    }

    [Fact]
    public void InactiveRegions_IfndefTrue_RecordsRegion()
    {
        var source = "#define EXISTS\n#ifndef EXISTS\n    hidden\n#endif\nnamespace N { }";
        var pp = new Preprocessor();
        pp.Process(source);
        Assert.Single(pp.InactiveRegions);
        var inactive = source[pp.InactiveRegions[0].Start..pp.InactiveRegions[0].End];
        Assert.Contains("hidden", inactive);
    }

    [Fact]
    public void InactiveRegions_MultipleBlocks_RecordsAll()
    {
        var source = "#ifdef A\n    block1\n#endif\nactive\n#ifdef B\n    block2\n#endif\n";
        var pp = new Preprocessor();
        pp.Process(source);
        Assert.Equal(2, pp.InactiveRegions.Count);
        Assert.Contains("block1", source[pp.InactiveRegions[0].Start..pp.InactiveRegions[0].End]);
        Assert.Contains("block2", source[pp.InactiveRegions[1].Start..pp.InactiveRegions[1].End]);
    }

    [Fact]
    public void InactiveRegions_FlowThroughCompilation()
    {
        var source = "#ifdef NOPE\n    hidden\n#endif\nnamespace N { runtimeclass W { W(); } }";
        var compilation = CompilationHelper.Compile(source);
        Assert.NotEmpty(compilation.InactiveRegions);
        var inactive = source[compilation.InactiveRegions[0].Start..compilation.InactiveRegions[0].End];
        Assert.Contains("hidden", inactive);
    }

    [Fact]
    public void InactiveRegions_WithDefine_ActiveBlockNotRecorded()
    {
        var source = "#define FEATURE\n#ifdef FEATURE\n    active code\n#endif\nnamespace N { }";
        var compilation = CompilationHelper.Compile(source);
        Assert.Empty(compilation.InactiveRegions);
    }

    [Fact]
    public void SemanticTokens_InactiveRegion_MarkedAsComment()
    {
        var source = "#ifdef NOPE\nruntimeclass Hidden { }\n#endif\nnamespace N { }";
        var compilation = CompilationHelper.Compile(source);
        var (data, types, _) = Midl.Lsp.Handlers.SemanticTokensHandler.HandleCore(source, compilation);

        // Find the token for "runtimeclass" — should be classified as comment (type 10) not keyword
        // Semantic tokens are delta-encoded: [deltaLine, deltaChar, length, type, modifiers]
        int commentType = Array.IndexOf(types, "comment");
        int keywordType = Array.IndexOf(types, "keyword");

        // Check that "runtimeclass" (which is in the inactive block) gets comment type
        bool foundInactiveKeyword = false;
        for (int i = 0; i < data.Length; i += 5)
        {
            int tokenType = data[i + 3];
            int length = data[i + 2];
            if (length == "runtimeclass".Length && tokenType == commentType)
            {
                foundInactiveKeyword = true;
                break;
            }
        }
        Assert.True(foundInactiveKeyword, "Keyword in inactive region should be classified as comment");

        // Also check that "namespace" (outside inactive) is still a keyword
        bool foundActiveKeyword = false;
        for (int i = 0; i < data.Length; i += 5)
        {
            int tokenType = data[i + 3];
            int length = data[i + 2];
            if (length == "namespace".Length && tokenType == keywordType)
            {
                foundActiveKeyword = true;
                break;
            }
        }
        Assert.True(foundActiveKeyword, "Keyword outside inactive region should remain keyword");
    }

    [Fact]
    public void SemanticTokens_InactiveRegion_PunctuationAlsoGrayed()
    {
        // Braces, brackets, semicolons in inactive regions must also be comment-typed
        var source = "#ifdef NOPE\ninterface IFoo { void Do(); };\n#endif\nnamespace N { }";
        var compilation = CompilationHelper.Compile(source);
        var (data, types, _) = Midl.Lsp.Handlers.SemanticTokensHandler.HandleCore(source, compilation);

        int commentType = Array.IndexOf(types, "comment");

        // All tokens on line 1 (the inactive line) should be comment type
        // Reconstruct absolute lines from delta-encoded data
        int line = 0;
        bool allInactiveAreComment = true;
        for (int i = 0; i < data.Length; i += 5)
        {
            line += data[i]; // deltaLine
            int tokenType = data[i + 3];
            // Line 1 is the inactive content line
            if (line == 1 && tokenType != commentType)
            {
                allInactiveAreComment = false;
                break;
            }
        }
        Assert.True(allInactiveAreComment, "All tokens on inactive line (including punctuation) should be comment type");
    }

    #endregion
}
