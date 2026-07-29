// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Syntax;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides doc comment completions: skeleton generation after /// and XML tag completions inside ///.
/// </summary>
public static class DocCommentCompletionProvider
{
    private static readonly string[] XmlTags =
        ["summary", "param", "returns", "remarks", "example", "see", "seealso", "value", "exception"];

    /// <summary>
    /// Detect if the cursor is on a doc comment line (///) and build appropriate completions.
    /// Returns null if not in a doc comment context.
    /// </summary>
    public static List<Dictionary<string, object?>>? BuildDocCommentCompletions(
        string text, int line, int character)
    {
        // Get the current line text
        var lines = text.Split('\n');
        if (line < 0 || line >= lines.Length) return null;

        var currentLine = lines[line].TrimEnd('\r');
        var trimmed = currentLine.TrimStart();

        // Check if we're on a /// line
        if (!trimmed.StartsWith("///")) return null;

        // Check if we're inside a tag angle bracket: "/// <" or "/// <s"
        var afterSlashes = trimmed[3..];
        int posInAfter = character - (currentLine.Length - currentLine.TrimStart().Length) - 3;

        if (posInAfter >= 0 && afterSlashes.TrimStart().StartsWith("<"))
        {
            // Offer XML tag completions
            return BuildXmlTagCompletions();
        }

        // Check if the line is just "///" or "/// " (skeleton trigger)
        if (string.IsNullOrWhiteSpace(afterSlashes))
        {
            return BuildSkeletonCompletion(text, line);
        }

        return null;
    }

    private static List<Dictionary<string, object?>> BuildXmlTagCompletions()
    {
        var items = new List<Dictionary<string, object?>>();
        foreach (var tag in XmlTags)
        {
            var item = new Dictionary<string, object?>
            {
                ["label"] = tag,
                ["kind"] = CompletionHandler.KindKeyword,
                ["detail"] = $"XML doc tag <{tag}>",
                ["insertText"] = tag == "param"
                    ? $"param name=\"${{1:name}}\">${{2}}</param>"
                    : tag == "see" || tag == "seealso"
                        ? $"{tag} cref=\"${{1}}\"/>"
                        : tag == "exception"
                            ? $"exception cref=\"${{1}}\">${{2}}</exception>"
                            : $"{tag}>${{1}}</{tag}>",
                ["insertTextFormat"] = 2 // Snippet
            };
            items.Add(item);
        }
        return items;
    }

    private static List<Dictionary<string, object?>>? BuildSkeletonCompletion(string text, int docLine)
    {
        // Look at the next non-empty, non-comment line after the doc comment block
        var lines = text.Split('\n');

        int nextLine = docLine + 1;
        while (nextLine < lines.Length)
        {
            var trimmed = lines[nextLine].TrimStart().TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("///"))
            {
                nextLine++;
                continue;
            }
            break;
        }

        if (nextLine >= lines.Length) return null;

        var declarationLine = lines[nextLine].TrimStart().TrimEnd('\r');
        var skeleton = GenerateSkeleton(declarationLine);
        if (skeleton == null) return null;

        var items = new List<Dictionary<string, object?>>
        {
            new()
            {
                ["label"] = "/// (doc comment)",
                ["kind"] = CompletionHandler.KindKeyword,
                ["detail"] = "Generate XML doc comment skeleton",
                ["insertText"] = skeleton,
                ["insertTextFormat"] = 2 // Snippet
            }
        };
        return items;
    }

    private static string? GenerateSkeleton(string declarationLine)
    {
        // Check if it looks like a method/constructor (has parentheses)
        bool isMethod = declarationLine.Contains('(');

        if (isMethod)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(" <summary>${1}</summary>");

            // Extract parameter names from the declaration
            var openParen = declarationLine.IndexOf('(');
            var closeParen = declarationLine.LastIndexOf(')');
            if (openParen >= 0 && closeParen > openParen)
            {
                var paramText = declarationLine[(openParen + 1)..closeParen].Trim();
                if (!string.IsNullOrEmpty(paramText))
                {
                    var paramPairs = SplitParams(paramText);
                    int tabStop = 2;
                    foreach (var pair in paramPairs)
                    {
                        var paramName = ExtractParamName(pair.Trim());
                        if (paramName != null)
                        {
                            sb.Append($"\n/// <param name=\"{paramName}\">${{{tabStop}}}</param>");
                            tabStop++;
                        }
                    }
                }
            }

            // Check for non-void return type
            var beforeParen = declarationLine[..openParen].Trim();
            if (!beforeParen.EndsWith("void", StringComparison.OrdinalIgnoreCase) &&
                !IsConstructor(beforeParen, declarationLine))
            {
                sb.Append($"\n/// <returns>${{{(sb.ToString().Count(c => c == '$') + 1)}}}</returns>");
            }

            return sb.ToString();
        }

        // Default: just summary for types, properties, events, etc.
        return " <summary>${1}</summary>";
    }

    private static bool IsConstructor(string beforeParen, string fullLine)
    {
        // A constructor has the form: ClassName(...) with no return type
        var parts = beforeParen.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 1 && !parts[0].Contains('.');
    }

    private static List<string> SplitParams(string paramText)
    {
        var result = new List<string>();
        int depth = 0;
        int start = 0;
        for (int i = 0; i < paramText.Length; i++)
        {
            char c = paramText[i];
            if (c == '<') depth++;
            else if (c == '>') depth--;
            else if (c == ',' && depth == 0)
            {
                result.Add(paramText[start..i]);
                start = i + 1;
            }
        }
        if (start < paramText.Length)
            result.Add(paramText[start..]);
        return result;
    }

    private static string? ExtractParamName(string paramDecl)
    {
        // Pattern: [out] TypeName paramName
        var parts = paramDecl.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;
        return parts[^1]; // last word is the parameter name
    }
}
