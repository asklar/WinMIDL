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

        return new[]
        {
            new
            {
                range = new
                {
                    start = new { line = 0, character = 0 },
                    end = new { line = endLine, character = endChar }
                },
                newText = formatted
            }
        };
    }

    /// <summary>
    /// Formats MIDL source text with consistent indentation and spacing.
    /// </summary>
    public static string Format(string sourceText)
    {
        var lines = SplitLines(sourceText);
        var result = new List<string>();
        int indentLevel = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            string trimmed = lines[i].Trim();

            if (trimmed.Length == 0)
            {
                result.Add("");
                continue;
            }

            // Decrease indent before lines that start with '}'
            if (trimmed.StartsWith('}'))
            {
                indentLevel = Math.Max(0, indentLevel - 1);
            }

            // Apply formatting rules to the line content
            string formatted = FormatLineContent(trimmed);

            // Apply indentation
            string indented = new string(' ', indentLevel * 4) + formatted;

            result.Add(indented);

            // Increase indent after lines that end with '{'
            if (trimmed.EndsWith('{'))
            {
                indentLevel++;
            }
        }

        // Trim trailing whitespace from each line
        for (int i = 0; i < result.Count; i++)
        {
            result[i] = result[i].TrimEnd();
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

            // Ensure space after commas
            if (c == ',')
            {
                sb.Append(',');
                if (i + 1 < line.Length && line[i + 1] != ' ')
                    sb.Append(' ');
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
                // Ensure space before ':'
                if (sb.Length > 0 && sb[^1] != ' ')
                    sb.Append(' ');
                sb.Append(':');
                // Ensure space after ':'
                if (i + 1 < line.Length && line[i + 1] != ' ')
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
