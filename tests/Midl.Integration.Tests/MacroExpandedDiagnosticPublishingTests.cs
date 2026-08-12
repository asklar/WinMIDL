// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Diagnostics;
using Midl.Lsp;

namespace Midl.Integration.Tests;

/// <summary>
/// The symptom that started this: a line with a genuine error and no squiggle.
///
/// <c>#define FOO</c> and <c>#define BAR x</c> turn <c>Int32 FOO BAR x;</c> into
/// <c>Int32  x x;</c>, which is not valid. The parser did report it, but against the
/// expanded text, and the macro-expanded line had no source-map coverage — so the position
/// stayed in expanded coordinates. DiagnosticsPublisher drops anything past the end of the
/// document rather than paint a squiggle in the wrong place, so the error vanished.
///
/// This exercises the whole path an editor takes, because every layer of it was involved.
/// </summary>
public class MacroExpandedDiagnosticPublishingTests
{
    private static async Task<List<JsonDocument>> Publish(string uri, string editorText,
        IReadOnlyList<Diagnostic> diagnostics)
    {
        var output = new MemoryStream();
        var publisher = new DiagnosticsPublisher(new JsonRpcWriter(output));
        await publisher.PublishDiagnosticsAsync(uri, editorText, diagnostics);

        output.Position = 0;
        var content = await new StreamReader(output, Encoding.UTF8).ReadToEndAsync();

        var docs = new List<JsonDocument>();
        foreach (var line in content.Split('\n'))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith('{')) continue;
            try { docs.Add(JsonDocument.Parse(trimmed)); } catch { }
        }
        return docs;
    }

    [Fact]
    public async Task AnErrorOnAMacroExpandedLineReachesTheEditor()
    {
        var dir = Path.Combine(Path.GetTempPath(), "midl-macropub-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            // An #include is what forces the source-map path; without one the compiler
            // remaps positions by line instead and the bug does not appear.
            File.WriteAllText(Path.Combine(dir, "helper.h"), "#define UNUSED_THING 1\n");

            var mainPath = Path.Combine(dir, "main.idl");
            var source = """
                #define FOO
                #define BAR  x
                #include "helper.h"

                namespace N{

                    runtimeclass C {
                        Int32 Y;
                        Int32 FOO BAR x;
                        Int32 X;
                        C();
                    }
                }
                """;
            File.WriteAllText(mainPath, source);

            var compilation = CompilationHelper.Compile(source, referencePaths: null,
                mode: CompilationMode.Strict, includePaths: new[] { dir }, currentFilePath: mainPath);

            var uri = new Uri(mainPath).AbsoluteUri;
            var docs = await Publish(uri, source, compilation.Diagnostics);

            // The notification for the edited document itself.
            var mine = docs.FirstOrDefault(d =>
                d.RootElement.TryGetProperty("params", out var p) &&
                p.TryGetProperty("uri", out var u) &&
                string.Equals(u.GetString(), uri, StringComparison.OrdinalIgnoreCase));

            Assert.True(mine is not null, "the edited document should get a publishDiagnostics notification");

            var published = mine!.RootElement.GetProperty("params").GetProperty("diagnostics")
                .EnumerateArray().ToList();

            var onMacroLine = published
                .Where(d => d.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32() == 8)
                .ToList();

            Assert.True(onMacroLine.Count > 0,
                "expected a squiggle on line 8 ('Int32 FOO BAR x;'), got lines: " +
                string.Join(", ", published.Select(d =>
                    d.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32())));

            // It should point at the duplicated trailing identifier, not the start of the line.
            int character = onMacroLine[0].GetProperty("range").GetProperty("start").GetProperty("character").GetInt32();
            Assert.True(character > 10,
                $"expected the squiggle near the trailing 'x', got character {character}");
        }
        finally { Directory.Delete(dir, recursive: true); }
    }
}
