using System.Text;
using System.Text.Json;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Handles textDocument/formatting requests — basic document formatting for MIDL files.
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

        var formatted = Format(text);
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

    /// <summary>
    /// Formats MIDL source text with consistent indentation and spacing.
    /// </summary>
    public static string Format(string sourceText)
    {
        var lines = SplitLines(sourceText);

        // Pre-process: split "declaration {" into two lines
        // (but keep inline property accessors like "{ get; set; }" on one line)
        lines = SplitBraceLines(lines);

        var result = new List<string>();
        int indentLevel = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            string trimmed = lines[i].Trim();

            // Collapse multiple consecutive blank lines to one
            if (trimmed.Length == 0)
            {
                if (result.Count > 0 && result[^1].Trim().Length == 0) continue; // skip duplicate blank
                result.Add("");
                continue;
            }

            // Decrease indent before lines that start with '}' or '};'
            if (trimmed.StartsWith('}'))
            {
                indentLevel = Math.Max(0, indentLevel - 1);
            }

            // Remove blank line right after opening '{'
            if (result.Count > 0 && result[^1].Trim().Length == 0 &&
                result.Count >= 2 && result[^2].TrimEnd().EndsWith('{'))
            {
                result.RemoveAt(result.Count - 1);
            }

            // Remove blank line right before closing '}'
            if (trimmed.StartsWith('}') && result.Count > 0 && result[^1].Trim().Length == 0)
            {
                result.RemoveAt(result.Count - 1);
            }

            // Format the line content
            string formatted = FormatLineContent(trimmed);

            // Apply indentation
            string indented = new string(' ', indentLevel * 4) + formatted;

            result.Add(indented.TrimEnd());

            // Increase indent after lines that end with '{'
            if (trimmed.EndsWith('{'))
            {
                indentLevel++;
            }
        }

        // Ensure blank line between top-level declarations
        result = EnsureBlankLineBetweenDeclarations(result);

        // Ensure trailing newline
        var output = string.Join("\n", result);
        if (output.Length > 0 && !output.EndsWith('\n'))
            output += "\n";

        return output;
    }

    private static string FormatLineContent(string line)
    {
        var sb = new StringBuilder(line.Length + 8);
        bool inString = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"' && (i == 0 || line[i - 1] != '\\'))
            {
                inString = !inString;
                sb.Append(c);
                continue;
            }

            if (inString)
            {
                sb.Append(c);
                continue;
            }

            // No space before semicolons
            if (c == ';')
            {
                while (sb.Length > 0 && sb[^1] == ' ')
                    sb.Remove(sb.Length - 1, 1);
                sb.Append(';');
                continue;
            }

            // Ensure space after commas (but not before)
            if (c == ',')
            {
                while (sb.Length > 0 && sb[^1] == ' ')
                    sb.Remove(sb.Length - 1, 1);
                sb.Append(',');
                if (i + 1 < line.Length && line[i + 1] != ' ')
                    sb.Append(' ');
                continue;
            }

            // Tight spacing around generics: no space before < or after <, no space before >
            if (c == '<')
            {
                while (sb.Length > 0 && sb[^1] == ' ')
                    sb.Remove(sb.Length - 1, 1);
                sb.Append('<');
                // Skip spaces after <
                while (i + 1 < line.Length && line[i + 1] == ' ')
                    i++;
                continue;
            }

            if (c == '>')
            {
                while (sb.Length > 0 && sb[^1] == ' ')
                    sb.Remove(sb.Length - 1, 1);
                sb.Append('>');
                continue;
            }

            // Ensure space around ':' (but not '::')
            if (c == ':' && i + 1 < line.Length && line[i + 1] == ':')
            {
                sb.Append("::");
                i++;
                continue;
            }

            if (c == ':')
            {
                if (sb.Length > 0 && sb[^1] != ' ')
                    sb.Append(' ');
                sb.Append(':');
                if (i + 1 < line.Length && line[i + 1] != ' ')
                    sb.Append(' ');
                continue;
            }

            // Ensure space before '{' (property accessors)
            if (c == '{')
            {
                if (sb.Length > 0 && sb[^1] != ' ')
                    sb.Append(' ');
                sb.Append('{');
                if (i + 1 < line.Length && line[i + 1] != ' ')
                    sb.Append(' ');
                continue;
            }

            // Handle '}'
            if (c == '}')
            {
                sb.Append('}');
                continue;
            }

            // No space after '(' or before ')'
            if (c == '(')
            {
                sb.Append('(');
                while (i + 1 < line.Length && line[i + 1] == ' ')
                    i++;
                continue;
            }

            if (c == ')')
            {
                while (sb.Length > 0 && sb[^1] == ' ')
                    sb.Remove(sb.Length - 1, 1);
                sb.Append(')');
                continue;
            }

            // Collapse multiple spaces into one
            if (c == ' ')
            {
                if (sb.Length == 0 || sb[^1] != ' ')
                    sb.Append(' ');
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    private static List<string> EnsureBlankLineBetweenDeclarations(List<string> lines)
    {
        var result = new List<string>(lines.Count);
        string[] declarationKeywords = ["namespace", "runtimeclass", "interface", "struct", "enum", "delegate", "apicontract"];

        for (int i = 0; i < lines.Count; i++)
        {
            string trimmed = lines[i].TrimStart();

            // Check if this line starts a top-level declaration
            bool isDeclaration = false;
            foreach (var kw in declarationKeywords)
            {
                if (trimmed.StartsWith(kw) && (trimmed.Length == kw.Length || !char.IsLetterOrDigit(trimmed[kw.Length])))
                {
                    isDeclaration = true;
                    break;
                }
            }

            // If this is a declaration and the previous non-empty line is not a blank line or the first line, insert blank line
            if (isDeclaration && result.Count > 0)
            {
                // Find last non-empty line
                bool hasPrecedingBlank = result.Count > 0 && result[^1].Trim().Length == 0;
                bool isPreviousOpenBrace = result.Count > 0 && result[^1].TrimEnd().EndsWith('{');

                if (!hasPrecedingBlank && !isPreviousOpenBrace)
                {
                    result.Add("");
                }
            }

            result.Add(lines[i]);
        }

        return result;
    }

    /// <summary>
    /// Split lines where '{' appears at the end after a declaration, putting '{' on its own line.
    /// Keeps inline property accessors like "String Name { get; set; };" on one line.
    /// </summary>
    private static List<string> SplitBraceLines(List<string> lines)
    {
        var result = new List<string>(lines.Count);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Skip empty lines
            if (trimmed.Length == 0)
            {
                result.Add(line);
                continue;
            }

            // Check if line ends with '{' and has content before it
            // e.g. "namespace Foo{" or "runtimeclass Widget {"
            if (trimmed.EndsWith('{') && trimmed.Length > 1)
            {
                var beforeBrace = trimmed[..^1].TrimEnd();
                if (beforeBrace.Length > 0)
                {
                    // Don't split inline property accessors: "Type Name { get; set; };"
                    // Those have '}' on the same line — but we're checking lines ending with '{'
                    // which means this is the opening brace of a block, not inline
                    result.Add(beforeBrace);
                    result.Add("{");
                    continue;
                }
            }

            // Check for "} else {" or similar patterns — not common in MIDL but handle
            // Also handle "};" which should stay together
            result.Add(line);
        }

        return result;
    }

    private static List<string> SplitLines(string text)
    {
        var lines = new List<string>();
        int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                lines.Add(text[start..i]);
                start = i + 1;
            }
            else if (text[i] == '\r')
            {
                lines.Add(text[start..i]);
                if (i + 1 < text.Length && text[i + 1] == '\n')
                    i++;
                start = i + 1;
            }
        }
        if (start <= text.Length)
        {
            lines.Add(text[start..]);
        }
        return lines;
    }
}
