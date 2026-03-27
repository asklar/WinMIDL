using System.Text.Json;
using Midl.Syntax;

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

        MidlTrace.Lsp($"onTypeFormatting: ch='{(ch == "\n" ? "\\n" : ch)}' line={line} char={character} totalLines={lines.Length}");

        if (line < 0 || line >= lines.Length) return Array.Empty<object>();

        var edits = new List<object>();

        if (ch == "\n" && line > 0)
        {
            var prevLineText = lines[line - 1].TrimEnd('\r');
            var currentLineText = lines[line].TrimEnd('\r');
            int prevSpaces = CountLeadingSpaces(prevLineText);
            var prevTrimmed = prevLineText.TrimEnd();

            // Desired indent = previous line's indent + 4 if prev ends with {
            int desiredSpaces = prevSpaces;
            if (prevTrimmed.EndsWith("{"))
                desiredSpaces += 4;

            // If current line starts with }, reduce by 4
            if (currentLineText.TrimStart().StartsWith("}"))
                desiredSpaces = Math.Max(0, desiredSpaces - 4);

            int currentSpaces = CountLeadingSpaces(currentLineText);
            MidlTrace.Lsp($"  prevLine='{prevTrimmed}' prevSpaces={prevSpaces} desired={desiredSpaces} currentSpaces={currentSpaces}");

            // Always emit an edit to set the indentation on the current line
            edits.Add(new Dictionary<string, object?>
            {
                ["range"] = MakeRange(line, 0, line, currentSpaces),
                ["newText"] = new string(' ', desiredSpaces)
            });
        }
        else if (ch == "}")
        {
            var currentLineText = lines[line].TrimEnd('\r');
            int currentSpaces = CountLeadingSpaces(currentLineText);
            int desiredSpaces = Math.Max(0, currentSpaces - 4);

            MidlTrace.Lsp($"  '}}' on line {line}: currentSpaces={currentSpaces} desired={desiredSpaces}");

            edits.Add(new Dictionary<string, object?>
            {
                ["range"] = MakeRange(line, 0, line, currentSpaces),
                ["newText"] = new string(' ', desiredSpaces)
            });
        }

        MidlTrace.Lsp($"  returning {edits.Count} edits");
        return edits;
    }

    private static int CountLeadingSpaces(string line)
    {
        int count = 0;
        foreach (char c in line)
        {
            if (c == ' ') count++;
            else if (c == '\t') count += 4;
            else break;
        }
        return count;
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
