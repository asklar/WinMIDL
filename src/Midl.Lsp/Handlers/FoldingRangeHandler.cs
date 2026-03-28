namespace Midl.Lsp.Handlers;

/// <summary>
/// Returns folding ranges for MIDL documents by matching brace pairs.
/// </summary>
public static class FoldingRangeHandler
{
    public static List<Dictionary<string, object?>> Handle(string text)
    {
        var ranges = new List<Dictionary<string, object?>>();
        var lineStarts = PositionHelper.BuildLineStarts(text);
        var braceStack = new Stack<int>(); // stack of '{' offsets

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            // Skip string literals
            if (c == '"')
            {
                i++;
                while (i < text.Length && text[i] != '"')
                {
                    if (text[i] == '\\') i++; // skip escaped char
                    i++;
                }
                continue;
            }

            // Skip single-line comments
            if (c == '/' && i + 1 < text.Length && text[i + 1] == '/')
            {
                while (i < text.Length && text[i] != '\n') i++;
                continue;
            }

            // Skip block comments
            if (c == '/' && i + 1 < text.Length && text[i + 1] == '*')
            {
                i += 2;
                while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/')) i++;
                i++; // skip closing '/'
                continue;
            }

            // Skip attribute brackets [...] — they use [] not {}
            if (c == '[')
            {
                int depth = 1;
                int attrStart = i;
                i++;
                while (i < text.Length && depth > 0)
                {
                    if (text[i] == '[') depth++;
                    else if (text[i] == ']') depth--;
                    else if (text[i] == '"')
                    {
                        i++;
                        while (i < text.Length && text[i] != '"')
                        {
                            if (text[i] == '\\') i++;
                            i++;
                        }
                    }
                    i++;
                }
                i--; // back up so the outer loop's i++ lands on next char

                // Create a folding range for multi-line attributes
                var (attrStartLine, _) = PositionHelper.OffsetToLineChar(lineStarts, attrStart);
                var (attrEndLine, _) = PositionHelper.OffsetToLineChar(lineStarts, i);
                if (attrEndLine > attrStartLine)
                {
                    ranges.Add(new Dictionary<string, object?>
                    {
                        ["startLine"] = attrStartLine,
                        ["endLine"] = attrEndLine,
                        ["kind"] = "region"
                    });
                }
                continue;
            }

            if (c == '{')
            {
                braceStack.Push(i);
            }
            else if (c == '}' && braceStack.Count > 0)
            {
                int openOffset = braceStack.Pop();
                var (startLine, _) = PositionHelper.OffsetToLineChar(lineStarts, openOffset);
                var (endLine, _) = PositionHelper.OffsetToLineChar(lineStarts, i);

                if (endLine > startLine)
                {
                    ranges.Add(new Dictionary<string, object?>
                    {
                        ["startLine"] = startLine,
                        ["endLine"] = endLine,
                        ["kind"] = "region"
                    });
                }
            }
        }

        return ranges;
    }

    public static object? Handle(System.Text.Json.JsonElement? @params, DocumentManager docManager)
    {
        if (@params is null) return Array.Empty<object>();

        var textDocument = @params.Value.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;

        var text = docManager.GetDocumentText(uri);
        if (text is null) return Array.Empty<object>();

        return Handle(text);
    }
}
