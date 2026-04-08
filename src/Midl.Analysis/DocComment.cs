namespace Midl.Analysis;

/// <summary>
/// Parsed XML documentation comment from /// trivia.
/// </summary>
public class DocComment
{
    public string? Summary { get; set; }
    public Dictionary<string, string> Params { get; } = new();
    public string? Returns { get; set; }
    public string? Remarks { get; set; }
    public string RawXml { get; set; } = "";
    public List<string> ParseErrors { get; set; } = new();

    /// <summary>
    /// Parse concatenated doc comment XML text into a DocComment.
    /// Returns null if the text is empty or cannot be parsed.
    /// Supports both XDC (XML) and Doxygen (@/\ command) formats.
    /// </summary>
    public static DocComment? Parse(string xmlText)
    {
        if (string.IsNullOrWhiteSpace(xmlText))
            return null;

        // Detect Doxygen-style tags and translate to XDC XML
        if (IsDoxygenFormat(xmlText))
            xmlText = TranslateDoxygenToXdc(xmlText);

        var result = XmlTagParser.Parse(xmlText);
        var doc = new DocComment { RawXml = xmlText };

        foreach (var tag in result.Tags)
        {
            switch (tag.Name.ToLowerInvariant())
            {
                case "summary": doc.Summary = NormalizeWhitespace(tag.Content); break;
                case "returns": doc.Returns = NormalizeWhitespace(tag.Content); break;
                case "remarks": doc.Remarks = NormalizeWhitespace(tag.Content); break;
                case "param":
                    if (tag.Attributes.TryGetValue("name", out var paramName))
                        doc.Params[paramName] = NormalizeWhitespace(tag.Content);
                    break;
            }
        }

        doc.ParseErrors = result.Errors;

        if (doc.Summary == null && doc.Returns == null && doc.Params.Count == 0 && result.Errors.Count == 0)
            doc.Summary = NormalizeWhitespace(xmlText);

        return doc;
    }

    /// <summary>
    /// Detects whether the text uses Doxygen-style commands (@tag or \tag).
    /// </summary>
    private static bool IsDoxygenFormat(string text)
    {
        // If it starts with '<', it's likely XDC XML
        var trimmed = text.TrimStart();
        if (trimmed.Length > 0 && trimmed[0] == '<')
            return false;

        // Look for @tag or \tag patterns with known Doxygen commands
        foreach (var line in text.Split('\n'))
        {
            var t = line.TrimStart();
            if (t.Length > 1 && (t[0] == '@' || t[0] == '\\'))
            {
                var cmd = ExtractDoxygenCommand(t, 1);
                if (IsKnownDoxygenCommand(cmd))
                    return true;
            }
        }
        return false;
    }

    private static string ExtractDoxygenCommand(string text, int startIndex)
    {
        int end = startIndex;
        while (end < text.Length && char.IsLetterOrDigit(text[end]))
            end++;
        return text[startIndex..end];
    }

    private static bool IsKnownDoxygenCommand(string cmd) =>
        cmd is "brief" or "param" or "return" or "returns" or "see"
            or "note" or "deprecated" or "details" or "remark" or "remarks";

    /// <summary>
    /// Translates Doxygen-style commands to XDC XML format.
    /// </summary>
    private static string TranslateDoxygenToXdc(string text)
    {
        var sb = new System.Text.StringBuilder();
        string? currentTag = null;
        string? currentParamName = null;
        var contentBuilder = new System.Text.StringBuilder();

        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.TrimStart();
            if (line.Length > 1 && (line[0] == '@' || line[0] == '\\'))
            {
                var cmd = ExtractDoxygenCommand(line, 1);
                if (IsKnownDoxygenCommand(cmd))
                {
                    // Flush previous tag
                    FlushDoxygenTag(sb, currentTag, currentParamName, contentBuilder);
                    currentParamName = null;

                    var rest = line[(1 + cmd.Length)..].TrimStart();
                    switch (cmd)
                    {
                        case "brief" or "details":
                            currentTag = "summary";
                            contentBuilder.Append(rest);
                            break;
                        case "param":
                            currentTag = "param";
                            // Strip optional direction modifier: [in], [out], [in,out]
                            if (rest.StartsWith("["))
                            {
                                int bracketEnd = rest.IndexOf(']');
                                if (bracketEnd >= 0)
                                    rest = rest[(bracketEnd + 1)..].TrimStart();
                            }
                            var spaceIdx = rest.IndexOf(' ');
                            if (spaceIdx > 0)
                            {
                                currentParamName = rest[..spaceIdx];
                                contentBuilder.Append(rest[(spaceIdx + 1)..]);
                            }
                            else
                            {
                                currentParamName = rest;
                            }
                            break;
                        case "return" or "returns":
                            currentTag = "returns";
                            contentBuilder.Append(rest);
                            break;
                        case "see":
                            currentTag = null;
                            sb.Append($"<seealso cref=\"{EscapeXml(rest.Trim())}\"/>");
                            break;
                        case "note" or "remark" or "remarks":
                            currentTag = "remarks";
                            contentBuilder.Append(rest);
                            break;
                        case "deprecated":
                            currentTag = "remarks";
                            contentBuilder.Append("Deprecated: ");
                            contentBuilder.Append(rest);
                            break;
                    }
                    continue;
                }
            }
            // Continuation line — append to current tag content
            if (currentTag != null && line.Length > 0)
            {
                if (contentBuilder.Length > 0) contentBuilder.Append(' ');
                contentBuilder.Append(line);
            }
            else if (currentTag == null && line.Length > 0)
            {
                // Plain text before any tag — treat as summary
                currentTag = "summary";
                contentBuilder.Append(line);
            }
        }
        FlushDoxygenTag(sb, currentTag, currentParamName, contentBuilder);
        return sb.ToString();
    }

    private static void FlushDoxygenTag(System.Text.StringBuilder sb, string? tag, string? paramName,
        System.Text.StringBuilder content)
    {
        if (tag == null) { content.Clear(); return; }
        var text = content.ToString().Trim();
        if (tag == "param" && paramName != null)
            sb.Append($"<param name=\"{EscapeXml(paramName)}\">{EscapeXml(text)}</param>");
        else
            sb.Append($"<{tag}>{EscapeXml(text)}</{tag}>");
        content.Clear();
    }

    private static string EscapeXml(string text) =>
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    private static string NormalizeWhitespace(string text)
    {
        // Collapse runs of whitespace (including newlines) into single spaces and trim
        var span = text.AsSpan();
        var sb = new System.Text.StringBuilder(span.Length);
        bool prevWasSpace = true; // trim leading
        foreach (var ch in span)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!prevWasSpace) sb.Append(' ');
                prevWasSpace = true;
            }
            else
            {
                sb.Append(ch);
                prevWasSpace = false;
            }
        }
        // trim trailing space
        if (sb.Length > 0 && sb[^1] == ' ')
            sb.Length--;
        return sb.ToString();
    }
}
