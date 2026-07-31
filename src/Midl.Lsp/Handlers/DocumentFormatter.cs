// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Pure formatting logic for MIDL source text — indentation, spacing, brace placement.
/// </summary>
internal static class DocumentFormatter
{
    /// <summary>
    /// Formats MIDL source text with consistent indentation and spacing.
    /// </summary>
    public static string Format(string sourceText)
    {
        var lines = SplitLines(sourceText);

        // Pre-process: split "declaration {" into two lines
        // (but keep inline property accessors like "{ get; set; }" on one line)
        lines = SplitBraceLines(lines);

        // Join multi-line property accessors onto the preceding declaration line
        lines = JoinPropertyAccessors(lines);

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
        // Comment-only lines: preserve content as-is
        if (line.StartsWith("//"))
            return line;

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

            // Inline comment: preserve rest of line as-is
            if (c == '/' && i + 1 < line.Length && line[i + 1] == '/')
            {
                if (sb.Length > 0 && sb[^1] != ' ')
                    sb.Append(' ');
                sb.Append(line[i..]);
                break;
            }

            // No space before semicolons; space after when followed by content
            if (c == ';')
            {
                while (sb.Length > 0 && sb[^1] == ' ')
                    sb.Remove(sb.Length - 1, 1);
                sb.Append(';');
                if (i + 1 < line.Length && line[i + 1] != ' ' && line[i + 1] != '}' && line[i + 1] != ')')
                    sb.Append(' ');
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

            // Ensure space before '{' (property accessors and inline blocks)
            if (c == '{')
            {
                if (sb.Length > 0 && sb[^1] != ' ')
                    sb.Append(' ');
                sb.Append('{');
                if (i + 1 < line.Length && line[i + 1] != ' ')
                    sb.Append(' ');
                continue;
            }

            // Ensure space before '}' when inline (property accessors)
            if (c == '}')
            {
                if (sb.Length > 0 && sb[^1] != ' ')
                    sb.Append(' ');
                sb.Append('}');
                continue;
            }

            // No space before '(' (except after control-flow keywords), no space after '('
            if (c == '(')
            {
                while (sb.Length > 0 && sb[^1] == ' ')
                    sb.Remove(sb.Length - 1, 1);
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

            bool isDeclaration = false;
            foreach (var kw in declarationKeywords)
            {
                if (trimmed.StartsWith(kw) && (trimmed.Length == kw.Length || !char.IsLetterOrDigit(trimmed[kw.Length])))
                {
                    isDeclaration = true;
                    break;
                }
            }

            bool isAttribute = trimmed.StartsWith('[') && trimmed.TrimEnd().EndsWith(']');

            if ((isDeclaration || isAttribute) && result.Count > 0)
            {
                string prev = result[^1].Trim();
                bool hasPrecedingBlank = prev.Length == 0;
                bool isPreviousOpenBrace = result[^1].TrimEnd().EndsWith('{');
                bool isPreviousAttribute = prev.StartsWith('[') && prev.TrimEnd().EndsWith(']');
                bool isPreviousComment = prev.StartsWith("//");

                if (!hasPrecedingBlank && !isPreviousOpenBrace && !isPreviousAttribute && !isPreviousComment)
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

            if (trimmed.Length == 0)
            {
                result.Add(line);
                continue;
            }

            // Split opening brace: "namespace Foo{" → "namespace Foo" + "{"
            if (trimmed.EndsWith('{') && trimmed.Length > 1)
            {
                var beforeBrace = trimmed[..^1].TrimEnd();
                if (beforeBrace.Length > 0)
                {
                    result.Add(beforeBrace);
                    result.Add("{");
                    continue;
                }
            }

            // Split closing brace that follows content on the same line:
            // "Single x { set; get;};    }" → "Single x { set; get;};" + "}"
            // But DON'T split inline accessors: "{ get; set; }" that are part of a property
            if (trimmed.EndsWith('}') || trimmed.EndsWith("};"))
            {
                var suffix = trimmed.EndsWith("};") ? "};" : "}";
                var core = trimmed[..^suffix.Length].TrimEnd();

                // If there's content before the closing brace AND it's not just "{"
                // then check if this is a standalone closing brace on a line with other stuff
                if (core.Length > 0 && core != "{")
                {
                    // Count braces to see if } is closing a block (not an inline accessor)
                    int depth = 0;
                    bool inStr = false;
                    for (int j = 0; j < core.Length; j++)
                    {
                        if (core[j] == '"') inStr = !inStr;
                        if (inStr) continue;
                        if (core[j] == '{') depth++;
                        if (core[j] == '}') depth--;
                    }

                    // If depth is 0, the closing } is a block-level brace (not part of an inline accessor)
                    if (depth == 0)
                    {
                        result.Add(core);
                        result.Add(suffix);
                        continue;
                    }
                }
            }

            result.Add(line);
        }

        return result;
    }

    /// <summary>
    /// Joins multi-line property accessor blocks (get;/set; on separate lines) onto the
    /// preceding property declaration line, producing e.g. "String Name { get; set; };".
    /// </summary>
    private static List<string> JoinPropertyAccessors(List<string> lines)
    {
        var result = new List<string>(lines.Count);
        string[] typeKeywords = ["namespace", "runtimeclass", "interface", "struct", "enum", "delegate", "apicontract"];
        int i = 0;

        while (i < lines.Count)
        {
            string trimmed = lines[i].Trim();

            if (trimmed == "{" && result.Count > 0)
            {
                // Look ahead for get;/set; accessor lines
                int j = i + 1;
                var accessors = new List<string>();
                while (j < lines.Count)
                {
                    var t = lines[j].Trim();
                    if (t == "get;" || t == "set;")
                    {
                        accessors.Add(t);
                        j++;
                    }
                    else break;
                }

                if (accessors.Count > 0 && j < lines.Count)
                {
                    var closing = lines[j].Trim();
                    if (closing == "}" || closing == "};")
                    {
                        // Don't join if preceding line is a type/namespace declaration
                        string prev = result[^1].Trim();
                        bool isTypeDecl = false;
                        foreach (var kw in typeKeywords)
                        {
                            if (prev.StartsWith(kw) && (prev.Length == kw.Length || !char.IsLetterOrDigit(prev[kw.Length])))
                            {
                                isTypeDecl = true;
                                break;
                            }
                        }

                        if (!isTypeDecl)
                        {
                            string accessorStr = string.Join(" ", accessors);
                            string suffix = closing == "};" ? ";" : "";

                            // Handle stray semicolon on next line
                            if (suffix == "" && j + 1 < lines.Count && lines[j + 1].Trim() == ";")
                            {
                                suffix = ";";
                                j++;
                            }

                            result[^1] = result[^1].TrimEnd() + " { " + accessorStr + " }" + suffix;
                            i = j + 1;
                            continue;
                        }
                    }
                }
            }

            result.Add(lines[i]);
            i++;
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
