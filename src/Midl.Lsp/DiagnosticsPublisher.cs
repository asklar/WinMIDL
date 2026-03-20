using System.Text.Json;

namespace Midl.Lsp;

/// <summary>
/// Converts Midl.Analysis diagnostics to LSP diagnostic notifications and writes them to the output.
/// </summary>
public sealed class DiagnosticsPublisher
{
    private readonly JsonRpcWriter _writer;

    public DiagnosticsPublisher(JsonRpcWriter writer)
    {
        _writer = writer;
    }

    public async Task PublishDiagnosticsAsync(string uri, string sourceText, IReadOnlyList<Analysis.Diagnostics.Diagnostic> diagnostics)
    {
        var lineStarts = PositionHelper.BuildLineStarts(sourceText);
        var lspDiags = new List<object>();

        foreach (var d in diagnostics)
        {
            var (startLine, startChar) = PositionHelper.OffsetToLineChar(lineStarts, d.Position);
            var (endLine, endChar) = PositionHelper.OffsetToLineChar(lineStarts, d.Position + d.Length);

            var diag = new Dictionary<string, object?>
            {
                ["range"] = new Dictionary<string, object?>
                {
                    ["start"] = new Dictionary<string, object?> { ["line"] = startLine, ["character"] = startChar },
                    ["end"] = new Dictionary<string, object?> { ["line"] = endLine, ["character"] = endChar }
                },
                ["severity"] = MapSeverity(d.Severity),
                ["code"] = d.Code,
                ["source"] = "midl",
                ["message"] = d.Message
            };
            if (d.HelpUri != null)
            {
                diag["codeDescription"] = new Dictionary<string, object?> { ["href"] = d.HelpUri };
            }
            lspDiags.Add(diag);
        }

        await _writer.SendNotificationAsync("textDocument/publishDiagnostics", new Dictionary<string, object?>
        {
            ["uri"] = uri,
            ["diagnostics"] = lspDiags
        });
    }

    private static int MapSeverity(Analysis.Diagnostics.DiagnosticSeverity severity) => severity switch
    {
        Analysis.Diagnostics.DiagnosticSeverity.Error => 1,
        Analysis.Diagnostics.DiagnosticSeverity.Warning => 2,
        Analysis.Diagnostics.DiagnosticSeverity.Info => 3,
        _ => 4 // Hint
    };
}
