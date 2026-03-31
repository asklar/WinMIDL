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
    /// </summary>
    public static DocComment? Parse(string xmlText)
    {
        if (string.IsNullOrWhiteSpace(xmlText))
            return null;

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
