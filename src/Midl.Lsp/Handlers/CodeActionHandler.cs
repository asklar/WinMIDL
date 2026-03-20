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

        // Recompile to get our diagnostics with Data (replacement info).
        // Use quality hints so MIDL3xxx diagnostics are available for code actions.
        var compilation = CompilationHelper.Compile(text, referencePaths: null,
            mode: CompilationMode.Strict, includePaths: null, currentFilePath: null,
            defines: null, emitQualityHints: true);

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

                actions.Add(MakeReplaceAction($"Replace with '{replacement}'", uri, lspDiag,
                    sl, sc, el, ec, replacement));
            }

            // MIDL3005: Insert default constructor
            if (code == "MIDL3005" && ourDiag is not null)
            {
                var className = text.Substring(ourDiag.Position, ourDiag.Length);
                var braceIdx = text.IndexOf('{', ourDiag.Position);
                if (braceIdx >= 0)
                {
                    var (il, ic) = PositionHelper.OffsetToLineChar(lineStarts, braceIdx + 1);
                    actions.Add(MakeReplaceAction($"Add default constructor '{className}()'", uri, lspDiag,
                        il, ic, il, ic, $" {className}();"));
                }
            }

            // MIDL3007: Add getter before setter
            if (code == "MIDL3007" && ourDiag is not null)
            {
                var searchStart = ourDiag.Position;
                var setIdx = text.IndexOf("set", searchStart);
                if (setIdx >= 0)
                {
                    var (sl2, sc2) = PositionHelper.OffsetToLineChar(lineStarts, setIdx);
                    actions.Add(MakeReplaceAction("Add getter", uri, lspDiag,
                        sl2, sc2, sl2, sc2, "get; "));
                }
            }
        }

        return actions.Count > 0 ? actions : null;
    }

    private static Dictionary<string, object?> MakeReplaceAction(
        string title, string uri, JsonElement lspDiag,
        int startLine, int startChar, int endLine, int endChar, string newText)
    {
        return new Dictionary<string, object?>
        {
            ["title"] = title,
            ["kind"] = "quickfix",
            ["diagnostics"] = new object[] { lspDiag },
            ["isPreferred"] = true,
            ["edit"] = new Dictionary<string, object?>
            {
                ["changes"] = new Dictionary<string, object?>
                {
                    [uri] = new object[]
                    {
                        new Dictionary<string, object?>
                        {
                            ["range"] = new Dictionary<string, object?>
                            {
                                ["start"] = new Dictionary<string, object?> { ["line"] = startLine, ["character"] = startChar },
                                ["end"] = new Dictionary<string, object?> { ["line"] = endLine, ["character"] = endChar }
                            },
                            ["newText"] = newText
                        }
                    }
                }
            }
        };
    }
}
