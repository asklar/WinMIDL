namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides completions for // @ref: directive lines.
/// </summary>
internal static class RefDirectiveCompletionProvider
{
    /// <summary>
    /// Detects if the cursor is on a @ref directive line: // @ref: path
    /// Returns the partial path typed so far, or "" if just after "// @ref: ".
    /// </summary>
    internal static string? DetectRefDirectiveContext(string text, int line, int character)
    {
        var lines = text.Split('\n');
        if (line < 0 || line >= lines.Length) return null;

        var currentLine = lines[line].TrimEnd('\r');
        // Match: // @ref: partial_path (with or without quotes)
        var trimmed = currentLine.TrimStart();
        if (!trimmed.StartsWith("// @ref:", StringComparison.Ordinal) &&
            !trimmed.StartsWith("//@ref:", StringComparison.Ordinal))
            return null;

        int colonIdx = currentLine.IndexOf(':', currentLine.IndexOf("@ref", StringComparison.Ordinal));
        if (colonIdx < 0) return null;

        var afterColon = currentLine[(colonIdx + 1)..];
        // Strip leading whitespace after colon
        var pathPart = afterColon.TrimStart();
        int trimmedChars = afterColon.Length - pathPart.Length;

        // Handle quoted path
        if (pathPart.StartsWith('"'))
        {
            pathPart = pathPart[1..];
            int endQuote = pathPart.IndexOf('"');
            if (endQuote >= 0)
                pathPart = pathPart[..endQuote];
        }

        // Only return if cursor is positioned within the path area
        int pathStartInLine = colonIdx + 1 + trimmedChars;
        if (pathPart.StartsWith('"')) pathStartInLine++;
        if (character < pathStartInLine) return null;

        // Trim to what the user has typed up to cursor
        int charsTyped = character - pathStartInLine;
        if (charsTyped < 0) charsTyped = 0;
        if (charsTyped < pathPart.Length)
            pathPart = pathPart[..charsTyped];

        return pathPart;
    }

    /// <summary>Build completions for // @ref: — lists .idl and .winmd files + directories.</summary>
    internal static List<object> BuildRefDirectiveCompletions(string partialPath, string? fileDir,
        IReadOnlyList<string>? includePaths = null)
    {
        if (fileDir == null && (includePaths == null || includePaths.Count == 0))
            return new List<object>();

        // Detect which separator the user prefers based on what they've already typed
        var dirSep = partialPath.Contains('/') ? "/" : "\\";

        var seenFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var items = new List<object>();

        // Determine the filename filter from the partial path
        string filter;
        string? subPath = null;
        if (partialPath.Contains('/') || partialPath.Contains('\\'))
        {
            var lastSep = Math.Max(partialPath.LastIndexOf('/'), partialPath.LastIndexOf('\\'));
            subPath = partialPath[..(lastSep + 1)];
            filter = partialPath[(lastSep + 1)..];
        }
        else
        {
            filter = partialPath;
        }

        // Build search list: local dir first, then include paths
        var searchDirs = new List<(string dir, bool isLocal)>();
        if (fileDir != null)
            searchDirs.Add((fileDir, true));
        if (includePaths != null)
        {
            foreach (var ip in includePaths)
                searchDirs.Add((ip, false));
        }

        foreach (var (baseDir, isLocal) in searchDirs)
        {
            var searchDir = subPath != null
                ? Path.Combine(baseDir, subPath.Replace('/', Path.DirectorySeparatorChar))
                : baseDir;

            if (!Directory.Exists(searchDir)) continue;

            try
            {
                // Enumerate .idl and .winmd files
                foreach (var file in Directory.EnumerateFiles(searchDir))
                {
                    var name = Path.GetFileName(file);
                    var ext = Path.GetExtension(name);
                    if (!ext.Equals(".idl", StringComparison.OrdinalIgnoreCase) &&
                        !ext.Equals(".winmd", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (filter.Length > 0 &&
                        !name.StartsWith(filter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!seenFiles.Add(name)) continue;

                    var kind = ext.Equals(".winmd", StringComparison.OrdinalIgnoreCase) ? CompletionHandler.KindModule : CompletionHandler.KindFile;
                    var sortPrefix = isLocal ? "0_" : "1_";
                    var detail = isLocal
                        ? (ext.Equals(".winmd", StringComparison.OrdinalIgnoreCase) ? "WinMD reference" : "IDL file")
                        : baseDir;
                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = name,
                        ["kind"] = kind,
                        ["detail"] = detail,
                        ["insertText"] = name,
                        ["filterText"] = name,
                        ["sortText"] = sortPrefix + name,
                    });
                }

                // Enumerate subdirectories
                foreach (var dir in Directory.EnumerateDirectories(searchDir))
                {
                    var name = Path.GetFileName(dir);
                    if (name.StartsWith('.')) continue;

                    if (filter.Length > 0 &&
                        !name.StartsWith(filter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!seenFolders.Add(name)) continue;

                    var sortPrefix = isLocal ? "2_" : "3_";
                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = name,
                        ["kind"] = CompletionHandler.KindFolder,
                        ["detail"] = isLocal ? "Directory" : baseDir,
                        ["insertText"] = name + dirSep,
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

        // Sort by sortText to enforce: local files, include files, local folders, include folders
        items.Sort((a, b) =>
        {
            var as_ = ((Dictionary<string, object?>)a)["sortText"]?.ToString() ?? "";
            var bs_ = ((Dictionary<string, object?>)b)["sortText"]?.ToString() ?? "";
            return string.Compare(as_, bs_, StringComparison.OrdinalIgnoreCase);
        });

        return items;
    }
}
