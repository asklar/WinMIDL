using System.Text;
using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Lsp;

namespace Midl.Integration.Tests;

/// <summary>
/// Tests that DiagnosticsPublisher sends correct line/col positions to the LSP client,
/// especially after preprocessing (#ifdef, #define) changes source positions.
/// </summary>
public class DiagnosticPublisherPositionTests
{
    /// <summary>
    /// Captures the JSON-RPC messages written by DiagnosticsPublisher.
    /// </summary>
    private static async Task<List<JsonDocument>> CapturePublishedDiagnostics(
        string uri, string editorText, IReadOnlyList<Diagnostic> diagnostics)
    {
        var output = new MemoryStream();
        var writer = new JsonRpcWriter(output);
        var publisher = new DiagnosticsPublisher(writer);

        await publisher.PublishDiagnosticsAsync(uri, editorText, diagnostics);

        output.Position = 0;
        var reader = new StreamReader(output, Encoding.UTF8);
        var content = await reader.ReadToEndAsync();

        // Parse JSON-RPC messages (skip Content-Length headers)
        var docs = new List<JsonDocument>();
        foreach (var line in content.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith('{'))
            {
                try { docs.Add(JsonDocument.Parse(trimmed)); }
                catch { }
            }
        }
        return docs;
    }

    private static (int line, int character) ExtractStartPosition(JsonDocument doc, string diagCode)
    {
        var diags = doc.RootElement.GetProperty("params").GetProperty("diagnostics");
        foreach (var d in diags.EnumerateArray())
        {
            if (d.GetProperty("code").GetString() == diagCode)
            {
                var start = d.GetProperty("range").GetProperty("start");
                return (start.GetProperty("line").GetInt32(), start.GetProperty("character").GetInt32());
            }
        }
        throw new Exception($"Diagnostic {diagCode} not found");
    }

    [Fact]
    public async Task Squiggle_WithIfdef_PointsToCorrectColumn()
    {
        var source = "namespace N {\n#ifdef NOPE\n    stuff\n#endif\n    runtimeclass Widget;\n}";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var uri = "file:///c%3A/test.idl";
        var docs = await CapturePublishedDiagnostics(uri, source, compilation.Diagnostics);

        Assert.NotEmpty(docs);
        var (line, col) = ExtractStartPosition(docs[0], "MIDL1062");

        // "Widget" is on line 4 (0-indexed) in the source
        var lines = source.Split('\n');
        var widgetCol = lines[4].IndexOf("Widget", StringComparison.Ordinal);
        Assert.Equal(4, line);
        Assert.Equal(widgetCol, col);
    }

    [Fact]
    public async Task Squiggle_WithDefineAndIfdef_PointsToCorrectColumn()
    {
        var source = "#define VER 1.0\nnamespace N {\n#ifdef NOPE\n    stuff\n#endif\n    [contractversion(VER)] apicontract Foo {}\n    [contract(Foo, 1)] runtimeclass MyClass;\n}";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var uri = "file:///c%3A/test.idl";
        var docs = await CapturePublishedDiagnostics(uri, source, compilation.Diagnostics);

        Assert.NotEmpty(docs);
        var (line, col) = ExtractStartPosition(docs[0], "MIDL1062");

        // "MyClass" is on line 6 (0-indexed)
        var lines = source.Split('\n');
        var myClassCol = lines[6].IndexOf("MyClass", StringComparison.Ordinal);
        Assert.Equal(6, line);
        Assert.Equal(myClassCol, col);
    }

    [Fact]
    public async Task Squiggle_AfterEditingDefine_UpdatesPosition()
    {
        // Simulate: user edits #define line, changing source length.
        // Squiggles must recalculate against the new source.
        var source1 = "#define FOO 1\nnamespace N { runtimeclass W; }";
        var source2 = "#define FOO\nnamespace N { runtimeclass W; }"; // 2 chars shorter

        var comp1 = CompilationHelper.Compile(source1, mode: CompilationMode.Strict);
        var comp2 = CompilationHelper.Compile(source2, mode: CompilationMode.Strict);

        var uri = "file:///c%3A/test.idl";

        var docs1 = await CapturePublishedDiagnostics(uri, source1, comp1.Diagnostics);
        var docs2 = await CapturePublishedDiagnostics(uri, source2, comp2.Diagnostics);

        var (line1, col1) = ExtractStartPosition(docs1[0], "MIDL1062");
        var (line2, col2) = ExtractStartPosition(docs2[0], "MIDL1062");

        // Both should point to "W" at the same column on line 1
        var wCol1 = source1.Split('\n')[1].IndexOf("W", StringComparison.Ordinal);
        var wCol2 = source2.Split('\n')[1].IndexOf("W", StringComparison.Ordinal);

        Assert.Equal(1, line1);
        Assert.Equal(1, line2);
        Assert.Equal(wCol1, col1);
        Assert.Equal(wCol2, col2);
        // Column should be the same since W is at the same position within its line
        Assert.Equal(col1, col2);
    }

    [Fact]
    public async Task Squiggle_InIncludedFile_PointsToCorrectFileAndPosition()
    {
        // Create temp files for #include scenario
        var tempDir = Path.Combine(Path.GetTempPath(), "midl_diag_pub_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);
        try
        {
            var mainFile = Path.Combine(tempDir, "main.idl");
            var helperFile = Path.Combine(tempDir, "helper.idl");

            var helperSource = "namespace Helper { runtimeclass BrokenWidget; }\n";
            var mainSource = "#include \"helper.idl\"\nnamespace Main { runtimeclass GoodWidget { GoodWidget(); Int32 Value; } }\n";

            File.WriteAllText(helperFile, helperSource);
            File.WriteAllText(mainFile, mainSource);

            var compilation = CompilationHelper.Compile(
                File.ReadAllText(mainFile),
                referencePaths: null,
                CompilationMode.Compat,
                includePaths: new[] { tempDir },
                currentFilePath: mainFile);

            var mainUri = "file:///" + mainFile.Replace('\\', '/');
            var docs = await CapturePublishedDiagnostics(mainUri, mainSource, compilation.Diagnostics);

            Assert.NotEmpty(docs);

            // Find the notification that targets helper.idl
            JsonDocument? helperDoc = null;
            foreach (var doc in docs)
            {
                var docUri = doc.RootElement.GetProperty("params").GetProperty("uri").GetString()!;
                if (docUri.Contains("helper.idl", StringComparison.OrdinalIgnoreCase))
                {
                    helperDoc = doc;
                    break;
                }
            }

            Assert.NotNull(helperDoc);

            // The diagnostic should point to "BrokenWidget" in helper.idl
            var diags = helperDoc.RootElement.GetProperty("params").GetProperty("diagnostics");
            Assert.True(diags.GetArrayLength() > 0, "helper.idl should have diagnostics");

            // Verify position points to "BrokenWidget" in the helper source
            var firstDiag = diags[0];
            var startLine = firstDiag.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32();
            var startChar = firstDiag.GetProperty("range").GetProperty("start").GetProperty("character").GetInt32();

            var helperLines = helperSource.Split('\n');
            int expectedCol = helperLines[startLine].IndexOf("BrokenWidget", StringComparison.Ordinal);

            // The diagnostic should be on line 0 of helper.idl
            Assert.Equal(0, startLine);
            Assert.True(expectedCol >= 0, "BrokenWidget should be on the reported line");
            // Allow pointing to runtimeclass keyword or the class name itself
            Assert.True(startChar >= helperLines[0].IndexOf("runtimeclass") && startChar <= expectedCol + "BrokenWidget".Length,
                $"Expected diagnostic position near 'BrokenWidget' (col {expectedCol}), got col {startChar}");
        }
        finally
        {
            try { Directory.Delete(tempDir, recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task Squiggle_InMainFile_AfterInclude_StillCorrect()
    {
        // Diagnostic in main file after #include should still point to main.idl with correct position
        var tempDir = Path.Combine(Path.GetTempPath(), "midl_diag_pub2_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);
        try
        {
            var mainFile = Path.Combine(tempDir, "main.idl");
            var helperFile = Path.Combine(tempDir, "helper.idl");

            var helperSource = "namespace Helper { }\n";
            // Main file has a broken runtimeclass AFTER the include
            var mainSource = "#include \"helper.idl\"\nnamespace Main { runtimeclass BadWidget; }\n";

            File.WriteAllText(helperFile, helperSource);
            File.WriteAllText(mainFile, mainSource);

            var compilation = CompilationHelper.Compile(
                File.ReadAllText(mainFile),
                referencePaths: null,
                CompilationMode.Compat,
                includePaths: new[] { tempDir },
                currentFilePath: mainFile);

            var mainUri = "file:///" + mainFile.Replace('\\', '/');
            var docs = await CapturePublishedDiagnostics(mainUri, mainSource, compilation.Diagnostics);

            Assert.NotEmpty(docs);

            // Find the notification for the main file
            JsonDocument? mainDoc = null;
            foreach (var doc in docs)
            {
                var docUri = doc.RootElement.GetProperty("params").GetProperty("uri").GetString()!;
                if (docUri.Equals(mainUri, StringComparison.OrdinalIgnoreCase))
                {
                    mainDoc = doc;
                    break;
                }
            }

            Assert.NotNull(mainDoc);

            var diags = mainDoc.RootElement.GetProperty("params").GetProperty("diagnostics");
            Assert.True(diags.GetArrayLength() > 0, "main.idl should have diagnostics for BadWidget");

            // The diagnostic should be on line 1 (0-indexed) of the main file
            var firstDiag = diags[0];
            var startLine = firstDiag.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32();
            Assert.Equal(1, startLine);
        }
        finally
        {
            try { Directory.Delete(tempDir, recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task Squiggle_MIDL1077_TypeRef_PublishesCorrectPosition()
    {
        // Verify the publisher sends correct line/col for MIDL1077 (type reference position).
        var source = "namespace N {\n    runtimeclass Empty { };\n    runtimeclass User {\n        void Use(Empty e);\n    }\n}";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var uri = "file:///c%3A/test.idl";
        var docs = await CapturePublishedDiagnostics(uri, source, compilation.Diagnostics);
        Assert.NotEmpty(docs);

        var (line, col) = ExtractStartPosition(docs[0], "MIDL1077");
        var lines = source.Split('\n');
        var emptyCol = lines[line].IndexOf("Empty", StringComparison.Ordinal);
        Assert.Equal(3, line); // "void Use(Empty e);" is line 3
        Assert.Equal(emptyCol, col);
    }

    [Fact]
    public async Task Squiggle_MIDL3003_EmptyInterface_PublishesCorrectPosition()
    {
        // Verify MIDL3003 info diagnostic publishes correct position.
        var source = "namespace N {\n    [uuid(01020304-0506-0708-090a-0b0c0d0e0f10)]\n    interface IFoo : IInspectable {};\n}";
        var compilation = CompilationHelper.Compile(source, referencePaths: null,
            CompilationMode.Strict, includePaths: null, currentFilePath: null,
            emitQualityHints: true);

        var uri = "file:///c%3A/test.idl";
        var docs = await CapturePublishedDiagnostics(uri, source, compilation.Diagnostics);
        Assert.NotEmpty(docs);

        var (line, col) = ExtractStartPosition(docs[0], "MIDL3003");
        var lines = source.Split('\n');
        var ifooCol = lines[line].IndexOf("IFoo", StringComparison.Ordinal);
        Assert.Equal(2, line); // "interface IFoo : IInspectable {};" is line 2
        Assert.Equal(ifooCol, col);
    }

    [Fact]
    public async Task Squiggle_MIDL1009_EmptyStruct_PublishesCorrectPosition()
    {
        var source = "namespace N {\n    struct Empty { };\n}";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var uri = "file:///c%3A/test.idl";
        var docs = await CapturePublishedDiagnostics(uri, source, compilation.Diagnostics);
        Assert.NotEmpty(docs);

        var (line, col) = ExtractStartPosition(docs[0], "MIDL1009");
        var lines = source.Split('\n');
        var emptyCol = lines[line].IndexOf("Empty", StringComparison.Ordinal);
        Assert.Equal(1, line);
        Assert.Equal(emptyCol, col);
    }

    [Fact]
    public async Task Squiggle_MIDL1077_AfterDefine_PublishesCorrectPosition()
    {
        // Verify MIDL1077 position is correct after #define in the publisher pipeline.
        var source = "#define VER 1.0\nnamespace N {\n    runtimeclass Empty { };\n    runtimeclass User {\n        void Use(Empty e);\n    }\n}";
        var compilation = CompilationHelper.Compile(source, mode: CompilationMode.Strict);

        var uri = "file:///c%3A/test.idl";
        var docs = await CapturePublishedDiagnostics(uri, source, compilation.Diagnostics);
        Assert.NotEmpty(docs);

        var (line, col) = ExtractStartPosition(docs[0], "MIDL1077");
        var lines = source.Split('\n');
        var emptyCol = lines[line].IndexOf("Empty", StringComparison.Ordinal);
        Assert.Equal(4, line); // "void Use(Empty e);" is line 4 (after #define on line 0)
        Assert.Equal(emptyCol, col);
    }

    [Fact]
    public async Task InactiveRegions_FromIncludes_NotPublished()
    {
        // Inactive regions with positions past the source text (from included SDK headers)
        // must not be published as "inactive preprocessor block" hints.
        var editorText = "#include \"sdk.h\"\nnamespace N { }";
        var inactiveRegions = new List<(int Start, int End)>
        {
            (5, 15),     // within the source — should be published
            (100, 200),  // past source length — from included content, skip
            (500, 600),  // past source length — from included content, skip
        };

        var output = new MemoryStream();
        var writer = new JsonRpcWriter(output);
        var publisher = new DiagnosticsPublisher(writer);

        await publisher.PublishDiagnosticsAsync(
            "file:///test.idl", editorText,
            Array.Empty<Diagnostic>(), inactiveRegions);

        output.Position = 0;
        var content = await new StreamReader(output, Encoding.UTF8).ReadToEndAsync();
        var doc = JsonDocument.Parse(
            content.Split('\n').First(l => l.TrimStart().StartsWith('{')));

        var diags = doc.RootElement.GetProperty("params").GetProperty("diagnostics");
        var inactiveHints = diags.EnumerateArray()
            .Where(d => d.GetProperty("message").GetString() == "Inactive preprocessor block")
            .ToList();

        // Only the region within source bounds should be published
        Assert.Single(inactiveHints);
    }
}
