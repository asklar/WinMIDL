using Midl.Analysis.Symbols;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Handles F12 navigation for // @ref: directives.
/// </summary>
internal static class RefDirectiveNavigator
{
    /// <summary>
    /// Checks if the given line is a // @ref: directive and returns the resolved path.
    /// </summary>
    internal static string? TryGetRefDirectiveTarget(string sourceText, int[] lineStarts, int line)
    {
        var lineText = GotoDefinitionHandler.GetLineText(sourceText, lineStarts, line).TrimStart();
        if (!lineText.StartsWith("//")) return null;
        var comment = lineText[2..].TrimStart();
        if (!comment.StartsWith("@ref:", StringComparison.OrdinalIgnoreCase)) return null;
        var path = comment[5..].Trim();
        if (path.Length >= 2 && path[0] == '"' && path[^1] == '"')
            path = path[1..^1].Trim();
        return string.IsNullOrEmpty(path) ? null : path;
    }

    /// <summary>
    /// Creates a response for F12 on a @ref line.
    /// For files: returns an LSP Location pointing to the file.
    /// For directories: returns a location with a special URI scheme that the extension handles.
    /// </summary>
    internal static object? CreateRefDirectiveResponse(string path, string currentUri, int[] lineStarts, int line)
    {
        // Try to resolve relative to current file
        string? resolvedPath = null;
        if (currentUri.StartsWith("file:///"))
        {
            var filePath = Uri.UnescapeDataString(currentUri[8..]);
            var dir = Path.GetDirectoryName(filePath);
            if (dir != null && !Path.IsPathRooted(path))
                resolvedPath = Path.GetFullPath(Path.Combine(dir, path));
            else
                resolvedPath = path;
        }
        else
        {
            resolvedPath = path;
        }

        if (resolvedPath == null) return null;

        if (Directory.Exists(resolvedPath))
        {
            var dirUri = "file:///" + resolvedPath.Replace('\\', '/');
            return GotoDefinitionHandler.MakeLocation(dirUri, 0, 0, 0, 0);
        }

        if (File.Exists(resolvedPath))
        {
            // For .winmd files, use the decompiled view URI scheme
            if (resolvedPath.EndsWith(".winmd", StringComparison.OrdinalIgnoreCase))
            {
                var winmdUri = "midl-winmd:///" + resolvedPath.Replace('\\', '/');
                return GotoDefinitionHandler.MakeLocation(winmdUri, 0, 0, 0, 0);
            }
            var fileUri = "file:///" + resolvedPath.Replace('\\', '/');
            return GotoDefinitionHandler.MakeLocation(fileUri, 0, 0, 0, 0);
        }

        return null;
    }

    /// <summary>
    /// For an external symbol (from WinMD), find which // @ref: line in the source
    /// could have provided it — matches by WinMD filename or namespace prefix.
    /// </summary>
    internal static int FindRefLineForExternalSymbol(string sourceText, ISymbol symbol)
    {
        var winmdPath = symbol.SourceWinMdPath;
        var winmdFileName = winmdPath != null ? Path.GetFileName(winmdPath) : null;
        var ns = symbol.FullName;

        using var reader = new StringReader(sourceText);
        string? line;
        int lineNum = 0;
        while ((line = reader.ReadLine()) != null)
        {
            var trimmed = line.TrimStart();
            if (!trimmed.StartsWith("//"))
            {
                if (trimmed.Length > 0) break; // past comment header
                lineNum++;
                continue;
            }

            var comment = trimmed[2..].TrimStart();
            if (comment.StartsWith("@ref:", StringComparison.OrdinalIgnoreCase))
            {
                var path = comment[5..].Trim();
                if (path.Length >= 2 && path[0] == '"' && path[^1] == '"')
                    path = path[1..^1].Trim();

                // Match by WinMD filename
                if (winmdFileName != null &&
                    Path.GetFileName(path).Equals(winmdFileName, StringComparison.OrdinalIgnoreCase))
                    return lineNum;

                // Match by namespace prefix in directory name or filename
                if (ns != null)
                {
                    var fileNameNoExt = Path.GetFileNameWithoutExtension(path);
                    if (ns.StartsWith(fileNameNoExt, StringComparison.OrdinalIgnoreCase))
                        return lineNum;
                }
            }

            lineNum++;
        }
        return -1;
    }
}
