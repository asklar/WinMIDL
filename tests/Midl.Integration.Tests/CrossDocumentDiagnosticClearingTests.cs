// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Midl.Analysis.Diagnostics;
using Midl.Lsp;

namespace Midl.Integration.Tests;

/// <summary>
/// Analysing one document must not clear another document's diagnostics.
///
/// The publisher tracked the files it had reported on in a single server-wide set, so every
/// publish cleared any URI that was not part of *that* document's result. With several .idl
/// files open, whichever was analysed last kept its squiggles and the rest were wiped —
/// which looked like "no errors on open, errors after I type", because typing re-analysed
/// the file the user was looking at.
///
/// The set is meant to clear stale diagnostics for #included files that no longer have
/// errors, so it has to be scoped to the document whose analysis produced them.
/// </summary>
public class CrossDocumentDiagnosticClearingTests
{
    private sealed record Published(string Uri, int Count);

    private static List<Published> Drain(MemoryStream stream)
    {
        // Parse Content-Length framing rather than splitting on newlines: consecutive
        // notifications run together, so a line-based reader silently drops all but the
        // first and the test then reports a missing publish that did happen.
        var content = Encoding.UTF8.GetString(stream.ToArray());
        var result = new List<Published>();
        int i = 0;
        while (true)
        {
            int header = content.IndexOf("Content-Length:", i, StringComparison.Ordinal);
            if (header < 0) break;
            int eol = content.IndexOf("\r\n\r\n", header, StringComparison.Ordinal);
            if (eol < 0) break;
            int len = int.Parse(content[(header + "Content-Length:".Length)..eol].Trim());
            int bodyStart = eol + 4;
            if (bodyStart + len > content.Length) break;
            var body = content.Substring(bodyStart, len);
            i = bodyStart + len;

            JsonDocument doc;
            try { doc = JsonDocument.Parse(body); } catch { continue; }
            using (doc)
            {
                if (!doc.RootElement.TryGetProperty("method", out var m) ||
                    m.GetString() != "textDocument/publishDiagnostics") continue;
                var p = doc.RootElement.GetProperty("params");
                result.Add(new Published(p.GetProperty("uri").GetString()!,
                    p.GetProperty("diagnostics").GetArrayLength()));
            }
        }
        return result;
    }

    private static Diagnostic Error(string message, int position = 0) =>
        new(DiagnosticSeverity.Error, "MIDL9999", message, position, 1);

    [Fact]
    public async Task PublishingForOneDocumentDoesNotClearAnother()
    {
        var stream = new MemoryStream();
        var publisher = new DiagnosticsPublisher(new JsonRpcWriter(stream));

        var uriA = "file:///c:/work/a.idl";
        var uriB = "file:///c:/work/b.idl";
        const string textA = "runtimeclass A { }";
        const string textB = "runtimeclass B { }";

        await publisher.PublishDiagnosticsAsync(uriA, textA, [Error("problem in A")]);
        await publisher.PublishDiagnosticsAsync(uriB, textB, [Error("problem in B")]);

        var published = Drain(stream);

        // b.idl's analysis must not have emitted an empty result for a.idl.
        var clearedA = published.Any(p =>
            string.Equals(p.Uri, uriA, StringComparison.OrdinalIgnoreCase) && p.Count == 0);

        Assert.False(clearedA,
            "analysing b.idl cleared a.idl's diagnostics; publishes were: " +
            string.Join(", ", published.Select(p => $"{p.Uri.Split('/')[^1]}={p.Count}")));
    }

    [Fact]
    public async Task ADocumentStillClearsItsOwnDiagnosticsWhenTheyAreFixed()
    {
        var stream = new MemoryStream();
        var publisher = new DiagnosticsPublisher(new JsonRpcWriter(stream));

        var uri = "file:///c:/work/a.idl";
        const string text = "runtimeclass A { }";

        await publisher.PublishDiagnosticsAsync(uri, text, [Error("problem in A")]);
        await publisher.PublishDiagnosticsAsync(uri, text, []);

        var published = Drain(stream);
        var last = published.Last(p => string.Equals(p.Uri, uri, StringComparison.OrdinalIgnoreCase));

        Assert.Equal(0, last.Count);
    }

    /// <summary>
    /// The behaviour the shared set existed for: a diagnostic attributed to an #included
    /// file must be cleared when the including document no longer produces it.
    /// </summary>
    [Fact]
    public async Task StaleIncludedFileDiagnosticsAreStillCleared()
    {
        var dir = Path.Combine(Path.GetTempPath(), "midl-crossdoc-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var included = Path.Combine(dir, "helper.h");
            File.WriteAllText(included, "// helper\n");

            var stream = new MemoryStream();
            var publisher = new DiagnosticsPublisher(new JsonRpcWriter(stream));

            var mainPath = Path.Combine(dir, "main.idl");
            var mainUri = new Uri(mainPath).AbsoluteUri;
            const string text = "runtimeclass A { }";

            var fromInclude = Error("problem in the header") with { SourceFile = included, OriginalPosition = 0 };

            await publisher.PublishDiagnosticsAsync(mainUri, text, [fromInclude]);
            await publisher.PublishDiagnosticsAsync(mainUri, text, []);

            var published = Drain(stream);
            var includedUri = new Uri(included).AbsoluteUri;

            var lastForInclude = published
                .LastOrDefault(p => string.Equals(p.Uri, includedUri, StringComparison.OrdinalIgnoreCase));

            Assert.True(lastForInclude is not null, "the included file should have been reported on");
            Assert.Equal(0, lastForInclude!.Count);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    [Fact]
    public async Task ThreeOpenDocumentsAllKeepTheirDiagnostics()
    {
        var stream = new MemoryStream();
        var publisher = new DiagnosticsPublisher(new JsonRpcWriter(stream));

        var uris = new[] { "file:///c:/work/a.idl", "file:///c:/work/b.idl", "file:///c:/work/c.idl" };
        foreach (var u in uris)
            await publisher.PublishDiagnosticsAsync(u, "runtimeclass X { }", [Error("problem")]);

        var published = Drain(stream);

        foreach (var u in uris)
        {
            var last = published.Last(p => string.Equals(p.Uri, u, StringComparison.OrdinalIgnoreCase));
            Assert.True(last.Count > 0, $"{u} ended up with its diagnostics cleared");
        }
    }
}
