using System.Text.Json;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Handles textDocument/formatting requests — delegates to <see cref="DocumentFormatter"/>.
/// </summary>
public static class FormattingHandler
{
    public static object? Handle(JsonElement? @params, DocumentManager docManager)
    {
        if (@params is null) return null;

        var textDocument = @params.Value.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;

        var text = docManager.GetDocumentText(uri);
        if (text is null) return null;

        var formatted = DocumentFormatter.Format(text);
        if (formatted == text) return Array.Empty<object>();

        // Return a single full-document text edit
        var lineStarts = PositionHelper.BuildLineStarts(text);
        var (endLine, endChar) = PositionHelper.OffsetToLineChar(lineStarts, text.Length);

        return new object[]
        {
            new Dictionary<string, object?>
            {
                ["range"] = new Dictionary<string, object?>
                {
                    ["start"] = new Dictionary<string, object?> { ["line"] = 0, ["character"] = 0 },
                    ["end"] = new Dictionary<string, object?> { ["line"] = endLine, ["character"] = endChar }
                },
                ["newText"] = formatted
            }
        };
    }
}
