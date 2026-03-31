namespace Midl.Analysis;

/// <summary>
/// Lightweight XML tag parser for doc comments. No System.Xml dependency.
/// Extracts tag name, attributes, and inner text content.
/// </summary>
public static class XmlTagParser
{
    public record XmlTag(string Name, Dictionary<string, string> Attributes, string Content, bool IsSelfClosing);

    public record ParseResult(List<XmlTag> Tags, List<string> Errors);

    /// <summary>
    /// Parse simple XML tags from doc comment text.
    /// Handles: &lt;summary&gt;text&lt;/summary&gt;, &lt;param name="x"&gt;text&lt;/param&gt;,
    /// &lt;see cref="Type"/&gt;, and other self-closing tags.
    /// Reports errors for unclosed/mismatched tags.
    /// </summary>
    public static ParseResult Parse(string xml)
    {
        var tags = new List<XmlTag>();
        var errors = new List<string>();
        int i = 0;

        while (i < xml.Length)
        {
            int openBracket = xml.IndexOf('<', i);
            if (openBracket < 0) break;

            // Skip closing tags at top level (they are consumed by open-tag handling)
            if (openBracket + 1 < xml.Length && xml[openBracket + 1] == '/')
            {
                // Stray close tag — skip past it
                int strayEnd = xml.IndexOf('>', openBracket);
                if (strayEnd < 0)
                {
                    errors.Add($"Unterminated close tag at position {openBracket}");
                    break;
                }
                i = strayEnd + 1;
                continue;
            }

            // Parse open tag: <tagname attr1="val1" ...> or <tagname ... />
            int closeBracket = xml.IndexOf('>', openBracket);
            if (closeBracket < 0)
            {
                errors.Add($"Unterminated tag at position {openBracket}");
                break;
            }

            var tagContent = xml.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim();
            bool selfClosing = tagContent.EndsWith("/");
            if (selfClosing)
                tagContent = tagContent[..^1].Trim();

            // Extract tag name
            int nameEnd = 0;
            while (nameEnd < tagContent.Length && !char.IsWhiteSpace(tagContent[nameEnd]))
                nameEnd++;
            var tagName = tagContent[..nameEnd];

            if (tagName.Length == 0)
            {
                i = closeBracket + 1;
                continue;
            }

            // Parse attributes
            var attributes = ParseAttributes(tagContent[nameEnd..]);

            if (selfClosing)
            {
                tags.Add(new XmlTag(tagName, attributes, "", true));
                i = closeBracket + 1;
                continue;
            }

            // Find matching close tag </tagname>
            var closeTag = $"</{tagName}>";
            int closeIndex = FindCloseTag(xml, closeBracket + 1, tagName);
            if (closeIndex < 0)
            {
                errors.Add($"Unclosed tag '<{tagName}>'");
                i = closeBracket + 1;
                continue;
            }

            var content = xml.Substring(closeBracket + 1, closeIndex - closeBracket - 1);
            tags.Add(new XmlTag(tagName, attributes, content, false));
            i = closeIndex + tagName.Length + 3; // skip past </tagname>
        }

        return new ParseResult(tags, errors);
    }

    private static int FindCloseTag(string xml, int start, string tagName)
    {
        // Search for </tagName> case-sensitive
        var target = $"</{tagName}>";
        int idx = xml.IndexOf(target, start, StringComparison.Ordinal);
        if (idx >= 0) return idx;

        // Check for mismatched close tags — find any close tag
        int pos = start;
        while (pos < xml.Length)
        {
            int bracket = xml.IndexOf("</", pos, StringComparison.Ordinal);
            if (bracket < 0) break;

            int end = xml.IndexOf('>', bracket);
            if (end < 0) break;

            // There's a close tag but for a different element — mismatch
            return -1;
        }

        return -1;
    }

    private static Dictionary<string, string> ParseAttributes(string attrText)
    {
        var attrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var text = attrText.Trim();
        int i = 0;

        while (i < text.Length)
        {
            // Skip whitespace
            while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
            if (i >= text.Length) break;

            // Read attribute name
            int nameStart = i;
            while (i < text.Length && text[i] != '=' && !char.IsWhiteSpace(text[i]))
                i++;
            var name = text[nameStart..i];
            if (name.Length == 0) break;

            // Skip whitespace and '='
            while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
            if (i >= text.Length || text[i] != '=')
            {
                attrs[name] = "";
                continue;
            }
            i++; // skip '='

            // Skip whitespace
            while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
            if (i >= text.Length) { attrs[name] = ""; break; }

            // Read quoted value
            if (text[i] == '"' || text[i] == '\'')
            {
                char quote = text[i];
                i++;
                int valStart = i;
                while (i < text.Length && text[i] != quote) i++;
                attrs[name] = DecodeEntities(text[valStart..i]);
                if (i < text.Length) i++; // skip closing quote
            }
            else
            {
                // Unquoted value (read until whitespace)
                int valStart = i;
                while (i < text.Length && !char.IsWhiteSpace(text[i])) i++;
                attrs[name] = DecodeEntities(text[valStart..i]);
            }
        }

        return attrs;
    }

    private static string DecodeEntities(string text)
    {
        if (!text.Contains('&')) return text;
        return text
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&apos;", "'");
    }
}
