using System.Xml;

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

    /// <summary>
    /// Parse concatenated doc comment XML text into a DocComment.
    /// Returns null if the text is empty or cannot be parsed.
    /// </summary>
    public static DocComment? Parse(string xmlText)
    {
        if (string.IsNullOrWhiteSpace(xmlText))
            return null;

        var doc = new DocComment { RawXml = xmlText };

        try
        {
            // Wrap in a root element to make it valid XML
            var xml = new XmlDocument();
            xml.LoadXml($"<root>{xmlText}</root>");
            var root = xml.DocumentElement!;

            var summaryNode = root.SelectSingleNode("summary");
            if (summaryNode != null)
                doc.Summary = NormalizeWhitespace(summaryNode.InnerText);

            foreach (XmlNode paramNode in root.SelectNodes("param")!)
            {
                var name = paramNode.Attributes?["name"]?.Value;
                if (name != null)
                    doc.Params[name] = NormalizeWhitespace(paramNode.InnerText);
            }

            var returnsNode = root.SelectSingleNode("returns");
            if (returnsNode != null)
                doc.Returns = NormalizeWhitespace(returnsNode.InnerText);

            var remarksNode = root.SelectSingleNode("remarks");
            if (remarksNode != null)
                doc.Remarks = NormalizeWhitespace(remarksNode.InnerText);
        }
        catch (XmlException)
        {
            // If XML is malformed, store raw text as summary
            doc.Summary = NormalizeWhitespace(xmlText);
        }

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
