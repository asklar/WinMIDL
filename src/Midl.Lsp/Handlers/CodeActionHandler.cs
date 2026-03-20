using System.Text.Json;
using Midl.Analysis;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides quick-fix code actions for MIDL diagnostics (e.g., replacing lowercase types).
/// </summary>
public static class CodeActionHandler
{
    public static object? Handle(JsonElement? @params, DocumentManager docManager)
    {
        if (@params is null) return null;

        var textDocument = @params.Value.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;
        var context = @params.Value.GetProperty("context");
        var diagnostics = context.GetProperty("diagnostics");

        var text = docManager.GetDocumentText(uri);
        if (text is null) return null;

        var lineStarts = PositionHelper.BuildLineStarts(text);
        var actions = new List<object>();

        // Recompile to get our diagnostics with Data (replacement info)
        var compilation = CompilationHelper.Compile(text);

        foreach (var lspDiag in diagnostics.EnumerateArray())
        {
            var code = lspDiag.TryGetProperty("code", out var c) ? c.GetString() : null;
            if (code is null) continue;

            // Resolve the LSP range start to an offset
            var range = lspDiag.GetProperty("range");
            var startLine = range.GetProperty("start").GetProperty("line").GetInt32();
            var startChar = range.GetProperty("start").GetProperty("character").GetInt32();
            int offset = PositionHelper.LineCharToOffset(lineStarts, startLine, startChar);

            // Find matching diagnostic from our compilation
            var ourDiag = compilation.Diagnostics.FirstOrDefault(d =>
                d.Code == code && Math.Abs(d.Position - offset) <= 2);

            if (ourDiag?.Data is not null && ourDiag.Data.TryGetValue("replacement", out var replacement))
            {
                var (sl, sc) = PositionHelper.OffsetToLineChar(lineStarts, ourDiag.Position);
                var (el, ec) = PositionHelper.OffsetToLineChar(lineStarts, ourDiag.Position + ourDiag.Length);

                actions.Add(new
                {
                    title = $"Replace with '{replacement}'",
                    kind = "quickfix",
                    diagnostics = new[] { lspDiag },
                    isPreferred = true,
                    edit = new
                    {
                        changes = new Dictionary<string, object[]>
                        {
                            [uri] = new object[]
                            {
                                new
                                {
                                    range = new
                                    {
                                        start = new { line = sl, character = sc },
                                        end = new { line = el, character = ec }
                                    },
                                    newText = replacement
                                }
                            }
                        }
                    }
                });
            }
        }

        return actions.Count > 0 ? actions : null;
    }
}
