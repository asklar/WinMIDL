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
}
