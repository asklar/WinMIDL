using System.Text.Json;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Handles textDocument/onTypeFormatting — provides auto-indentation
/// when typing {, }, ;, or pressing Enter.
/// </summary>
public static class OnTypeFormattingHandler
{
    public static object? Handle(JsonElement? @params, DocumentManager docManager)
    {
        if (@params is null) return null;

        var textDocument = @params.Value.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;
        var position = @params.Value.GetProperty("position");
        int line = position.GetProperty("line").GetInt32();
        int character = position.GetProperty("character").GetInt32();
        var ch = @params.Value.GetProperty("ch").GetString()!;

        var text = docManager.GetDocumentText(uri);
        if (text is null) return Array.Empty<object>();

        var lines = text.Split('\n');
        if (line < 0 || line >= lines.Length) return Array.Empty<object>();

        var edits = new List<object>();

        if (ch == "\n" && line > 0)
        {
            // After pressing Enter, match the indentation of the previous line
            // and increase it if the previous line ends with {
            var prevLineText = lines[line - 1].TrimEnd('\r');
            int prevIndent = GetIndentLevel(prevLineText);

            // If previous line ends with {, indent one more level
            if (prevLineText.TrimEnd().EndsWith("{"))
                prevIndent++;

            var currentLineText = line < lines.Length ? lines[line].TrimEnd('\r') : "";
            int currentIndent = GetIndentLevel(currentLineText);

            if (currentIndent != prevIndent)
            {
                string newIndent = new string(' ', prevIndent * 4);
                string trimmedCurrent = currentLineText.TrimStart();

                // If current line starts with }, outdent by one
                if (trimmedCurrent.StartsWith("}"))
                {
                    prevIndent = Math.Max(0, prevIndent - 1);
                    newIndent = new string(' ', prevIndent * 4);
                }

                edits.Add(new Dictionary<string, object?>
                {
                    ["range"] = MakeRange(line, 0, line, currentIndent * 4 + (currentLineText.Length - currentLineText.TrimStart().Length)),
                    ["newText"] = newIndent
                });
            }
        }
        else if (ch == "}")
        {
            // When typing }, outdent the current line
            var currentLineText = lines[line].TrimEnd('\r');
            int currentIndent = GetIndentLevel(currentLineText);

            if (currentIndent > 0)
            {
                int newIndent = currentIndent - 1;
                int oldSpaces = currentLineText.Length - currentLineText.TrimStart().Length;
                edits.Add(new Dictionary<string, object?>
                {
                    ["range"] = MakeRange(line, 0, line, oldSpaces),
                    ["newText"] = new string(' ', newIndent * 4)
                });
            }
        }

        return edits;
    }

    private static int GetIndentLevel(string line)
    {
        int spaces = 0;
        foreach (char c in line)
        {
            if (c == ' ') spaces++;
            else if (c == '\t') spaces += 4;
            else break;
        }
        return spaces / 4;
    }

    private static object MakeRange(int startLine, int startChar, int endLine, int endChar)
    {
        return new Dictionary<string, object?>
        {
            ["start"] = new Dictionary<string, object?> { ["line"] = startLine, ["character"] = startChar },
            ["end"] = new Dictionary<string, object?> { ["line"] = endLine, ["character"] = endChar }
        };
    }
}
