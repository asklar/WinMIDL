using Midl.Analysis;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides completions for import "..." statements and feature_name declarations.
/// </summary>
internal static class ImportCompletionProvider
{
#if WINDOWS_INTERNAL
    /// <summary>
    /// Detects if the cursor is inside a feature_name body: feature_name Name = {Stage, Bool}
    /// Returns completion items for stage (first position) or bool (after comma), or null if not in context.
    /// </summary>
    internal static List<object>? DetectFeatureNameContext(string text, int line, int character)
    {
        var lines = text.Split('\n');
        if (line < 0 || line >= lines.Length) return null;

        var currentLine = lines[line];
        // Find "feature_name" keyword on this line
        int fnIdx = currentLine.IndexOf("feature_name", StringComparison.Ordinal);
        if (fnIdx < 0) return null;

        // Find the opening brace on this line after feature_name
        int braceIdx = currentLine.IndexOf('{', fnIdx);
        if (braceIdx < 0 || character <= braceIdx) return null;

        // Cursor is after '{'. Check if there's a comma between '{' and cursor.
        var insideBrace = currentLine.Substring(braceIdx + 1, character - braceIdx - 1);
        var items = new List<object>();

        if (insideBrace.Contains(','))
        {
            // After comma → bool completions
            CompletionHandler.AddItem(items, "TRUE", CompletionHandler.KindKeyword, "Valid in all branches");
            CompletionHandler.AddItem(items, "FALSE", CompletionHandler.KindKeyword, "Not valid in all branches");
        }
        else
        {
            // Before comma → stage completions
            CompletionHandler.AddItem(items, "AlwaysDisabled", CompletionHandler.KindEnumMember, "Feature is always disabled");
            CompletionHandler.AddItem(items, "DisabledByDefault", CompletionHandler.KindEnumMember, "Feature is disabled by default");
            CompletionHandler.AddItem(items, "EnabledByDefault", CompletionHandler.KindEnumMember, "Feature is enabled by default");
            CompletionHandler.AddItem(items, "AlwaysEnabled", CompletionHandler.KindEnumMember, "Feature is always enabled");
        }

        return items;
    }

    /// <summary>
    /// Detects if the cursor is inside [feature(|)] and returns completion items
    /// for all declared feature_name identifiers in the source.
    /// </summary>
    internal static List<object>? DetectFeatureAttributeArgContext(string text, int offset)
    {
        // Walk backward from cursor to find an unmatched '('
        int parenDepth = 0;
        int openParen = -1;
        for (int i = offset - 1; i >= 0; i--)
        {
            char c = text[i];
            if (c == ')') parenDepth++;
            else if (c == '(')
            {
                if (parenDepth > 0) parenDepth--;
                else { openParen = i; break; }
            }
            else if (c == '[' || c == ']') break; // left the attribute bracket
        }
        if (openParen < 0) return null;

        // Extract the attribute name before '('
        int nameEnd = openParen;
        int nameStart = nameEnd - 1;
        while (nameStart >= 0 && (char.IsLetterOrDigit(text[nameStart]) || text[nameStart] == '_'))
            nameStart--;
        nameStart++;

        var attrName = text[nameStart..nameEnd];
        if (!attrName.Equals("feature", StringComparison.OrdinalIgnoreCase))
            return null;

        // Compile the source and collect all feature_name declarations
        var items = new List<object>();
        try
        {
            var compilation = CompilationHelper.Compile(text);
            foreach (var name in compilation.FeatureNames.Keys)
            {
                CompletionHandler.AddItem(items, name, CompletionHandler.KindEnumMember, "feature_name declaration");
            }
        }
        catch
        {
            // Partial file — return empty
        }
        return items;
    }
#endif

    /// <summary>
    /// Detects if the cursor is on an import line: import "..."
    /// Returns the partial text typed so far inside the quotes, or "" if just opened.
    /// </summary>
    internal static string? DetectImportContext(string text, int line)
    {
        var lines = text.Split('\n');
        if (line < 0 || line >= lines.Length) return null;

        var currentLine = lines[line].TrimStart();
        // Match: import "partial_text  (with or without closing quote)
        if (!currentLine.StartsWith("import ", StringComparison.Ordinal) &&
            !currentLine.StartsWith("import\t", StringComparison.Ordinal))
            return null;

        int quoteStart = currentLine.IndexOf('"');
        if (quoteStart < 0) return null;

        // Extract text after the opening quote
        var afterQuote = currentLine[(quoteStart + 1)..];
        int quoteEnd = afterQuote.IndexOf('"');
        return quoteEnd >= 0 ? afterQuote[..quoteEnd] : afterQuote.TrimEnd('\r');
    }

    /// <summary>Build completions for import "..." — lists .idl files from include paths and project.</summary>
    internal static List<object> BuildImportCompletions(string partialPath,
        string? fileDir, IReadOnlyList<string>? includePaths, ProjectFile? projectFile,
        string? currentFileName = null)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var items = new List<object>();

        // Exclude the current file
        if (currentFileName != null)
            seen.Add(currentFileName);

        // Build search list with local/non-local tracking
        var searchDirs = new List<(string dir, bool isLocal)>();
        if (fileDir != null) searchDirs.Add((fileDir, true));
        if (includePaths != null)
        {
            foreach (var ip in includePaths)
                searchDirs.Add((ip, false));
        }
        if (projectFile != null)
        {
            foreach (var ip in projectFile.GetAbsoluteIncludePaths())
                searchDirs.Add((ip, false));
        }

        // Also add project input file directories
        if (projectFile != null)
        {
            var existingDirs = new HashSet<string>(searchDirs.Select(sd => sd.dir), StringComparer.OrdinalIgnoreCase);
            foreach (var input in projectFile.GetAbsoluteInputPaths())
            {
                var dir = Path.GetDirectoryName(input);
                if (dir != null && existingDirs.Add(dir))
                    searchDirs.Add((dir, false));
            }
        }

        foreach (var (dir, isLocal) in searchDirs)
        {
            if (!Directory.Exists(dir)) continue;

            try
            {
                foreach (var file in Directory.EnumerateFiles(dir, "*.idl", SearchOption.TopDirectoryOnly))
                {
                    var name = Path.GetFileName(file);
                    if (!seen.Add(name)) continue;

                    // Filter by partial text typed so far
                    if (partialPath.Length > 0 &&
                        !name.StartsWith(partialPath, StringComparison.OrdinalIgnoreCase) &&
                        !name.Contains(partialPath, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var isInProject = projectFile?.Inputs.Any(i =>
                        Path.GetFileName(i).Equals(name, StringComparison.OrdinalIgnoreCase)) ?? false;

                    var sortPrefix = isLocal ? "0_" : "1_";
                    var detail = isLocal
                        ? (isInProject ? "(project)" : "(local)")
                        : (isInProject ? "(project)" : dir);

                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = name,
                        ["kind"] = CompletionHandler.KindFile,
                        ["detail"] = detail,
                        ["insertText"] = name,
                        ["filterText"] = name,
                        ["sortText"] = sortPrefix + name,
                    });
                }
            }
            catch
            {
                // Ignore directory access errors
            }
        }

        // Sort by sortText: local files first, then include-path files
        items.Sort((a, b) =>
        {
            var as_ = ((Dictionary<string, object?>)a)["sortText"]?.ToString() ?? "";
            var bs_ = ((Dictionary<string, object?>)b)["sortText"]?.ToString() ?? "";
            return string.Compare(as_, bs_, StringComparison.OrdinalIgnoreCase);
        });

        return items;
    }
}
