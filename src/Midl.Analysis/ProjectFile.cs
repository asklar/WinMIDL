using System.Text.Json;

namespace Midl.Analysis;

/// <summary>
/// Represents a winmidl.json project file that configures multi-file compilation.
/// The LSP discovers this to provide cross-file symbol resolution.
/// </summary>
public class ProjectFile
{
    /// <summary>IDL input files (relative to the project file directory).</summary>
    public List<string> Inputs { get; set; } = [];

    /// <summary>WinMD reference paths.</summary>
    public List<string> References { get; set; } = [];

    /// <summary>Preprocessor defines.</summary>
    public List<string> Defines { get; set; } = [];

    /// <summary>Include search paths for import/include directives.</summary>
    public List<string> IncludePaths { get; set; } = [];

    /// <summary>If true, do not reference the Windows SDK WinMD automatically.</summary>
    public bool NoSdk { get; set; }

    /// <summary>Directory containing the winmidl.json file.</summary>
    public string BaseDirectory { get; set; } = "";

    /// <summary>
    /// Discover and load a winmidl.json file by searching upward from the given file path.
    /// Returns null if no project file is found.
    /// </summary>
    public static ProjectFile? Discover(string filePath)
    {
        try
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(filePath));
            while (dir != null)
            {
                var projectPath = Path.Combine(dir, "winmidl.json");
                if (File.Exists(projectPath))
                    return Load(projectPath);
                dir = Path.GetDirectoryName(dir);
            }
        }
        catch
        {
            // Ignore filesystem errors during discovery
        }
        return null;
    }

    /// <summary>Load a winmidl.json file.</summary>
    public static ProjectFile Load(string path)
    {
        var json = File.ReadAllText(path);
        var baseDir = Path.GetDirectoryName(Path.GetFullPath(path)) ?? "";
        var project = new ProjectFile { BaseDirectory = baseDir };

        using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        var root = doc.RootElement;

        if (root.TryGetProperty("inputs", out var inputs))
            foreach (var item in inputs.EnumerateArray())
                if (item.GetString() is { } s)
                    project.Inputs.Add(s);

        if (root.TryGetProperty("references", out var refs))
            foreach (var item in refs.EnumerateArray())
                if (item.GetString() is { } s)
                    project.References.Add(s);

        if (root.TryGetProperty("defines", out var defs))
            foreach (var item in defs.EnumerateArray())
                if (item.GetString() is { } s)
                    project.Defines.Add(s);

        if (root.TryGetProperty("includePaths", out var incl))
            foreach (var item in incl.EnumerateArray())
                if (item.GetString() is { } s)
                    project.IncludePaths.Add(s);

        if (root.TryGetProperty("noSdk", out var noSdk))
            project.NoSdk = noSdk.GetBoolean();

        return project;
    }

    /// <summary>Resolve all relative paths to absolute paths based on the project directory.</summary>
    public List<string> GetAbsoluteInputPaths()
    {
        return Inputs.Select(p => Path.GetFullPath(Path.Combine(BaseDirectory, p))).ToList();
    }

    public List<string> GetAbsoluteReferencePaths()
    {
        return References.Select(p => Path.GetFullPath(Path.Combine(BaseDirectory, p))).ToList();
    }

    public List<string> GetAbsoluteIncludePaths()
    {
        var paths = IncludePaths.Select(p => Path.GetFullPath(Path.Combine(BaseDirectory, p))).ToList();
        if (!paths.Contains(BaseDirectory))
            paths.Insert(0, BaseDirectory);
        return paths;
    }

    /// <summary>
    /// Get all sibling IDL files from the project that should be compiled together
    /// with the given file (excludes the file itself).
    /// </summary>
    public List<string> GetSiblingInputs(string currentFilePath)
    {
        var current = Path.GetFullPath(currentFilePath);
        return GetAbsoluteInputPaths()
            .Where(p => !string.Equals(p, current, StringComparison.OrdinalIgnoreCase))
            .Where(File.Exists)
            .ToList();
    }
}

/// <summary>
/// Extracts // @ref: directives from IDL source text.
/// These allow lightweight cross-file hints without a project file.
/// Supports: file paths, directory paths, and quoted paths.
/// </summary>
public static class RefDirectiveParser
{
    /// <summary>
    /// Parse // @ref: directives from source text.
    /// Returns resolved absolute paths (files or directories) relative to the given base directory.
    /// Paths may be quoted with double quotes.
    /// </summary>
    public static List<string> ParseRefs(string source, string? baseDir)
    {
        var refs = new List<string>();
        using var reader = new StringReader(source);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var trimmed = line.TrimStart();
            if (!trimmed.StartsWith("//")) continue;
            var comment = trimmed[2..].TrimStart();

            if (comment.StartsWith("@ref:", StringComparison.OrdinalIgnoreCase))
            {
                var path = comment[5..].Trim();

                // Strip surrounding quotes
                if (path.Length >= 2 && path[0] == '"' && path[^1] == '"')
                    path = path[1..^1].Trim();

                if (!string.IsNullOrEmpty(path))
                {
                    if (baseDir != null && !Path.IsPathRooted(path))
                        path = Path.GetFullPath(Path.Combine(baseDir, path));
                    if (File.Exists(path) || Directory.Exists(path))
                        refs.Add(path);
                }
            }

            // Stop scanning after the first non-comment, non-blank line
            if (!trimmed.StartsWith("//") && trimmed.Length > 0)
                break;
        }
        return refs;
    }

    /// <summary>
    /// Validate // @ref: directives in source text.
    /// Returns diagnostics for paths that don't resolve to existing files or directories.
    /// Positions cover the path portion only (not the // @ref: prefix).
    /// </summary>
    public static List<Diagnostics.Diagnostic> ValidateRefDirectives(string source, string? baseDir)
    {
        var diagnostics = new List<Diagnostics.Diagnostic>();
        int lineStart = 0;
        using var reader = new StringReader(source);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var trimmed = line.TrimStart();
            var leadingSpaces = line.Length - trimmed.Length;

            if (!trimmed.StartsWith("//"))
            {
                // Stop scanning after the first non-comment, non-blank line
                if (trimmed.Length > 0)
                    break;
                lineStart += line.Length + 1; // +1 for newline
                continue;
            }

            var comment = trimmed[2..].TrimStart();
            var spacesAfterSlashes = trimmed.Length - 2 - comment.Length;

            if (comment.StartsWith("@ref:", StringComparison.OrdinalIgnoreCase))
            {
                var afterRef = comment[5..];
                var spacesAfterColon = afterRef.Length - afterRef.TrimStart().Length;
                var rawPath = afterRef.TrimStart();

                // Strip surrounding quotes for resolution, but track original text for position
                string path;
                int pathOffset;
                int pathDisplayLength;

                if (rawPath.Length >= 2 && rawPath[0] == '"' && rawPath[^1] == '"')
                {
                    path = rawPath[1..^1].Trim();
                    // Position covers the quoted content (without quotes)
                    pathOffset = lineStart + leadingSpaces + 2 + spacesAfterSlashes + 5 + spacesAfterColon + 1; // +1 for opening quote
                    pathDisplayLength = rawPath.Length - 2; // content without quotes
                }
                else
                {
                    path = rawPath.TrimEnd();
                    pathOffset = lineStart + leadingSpaces + 2 + spacesAfterSlashes + 5 + spacesAfterColon;
                    pathDisplayLength = path.Length;
                }

                if (!string.IsNullOrEmpty(path))
                {
                    var resolvedPath = path;
                    if (baseDir != null && !Path.IsPathRooted(path))
                        resolvedPath = Path.GetFullPath(Path.Combine(baseDir, path));

                    if (!File.Exists(resolvedPath) && !Directory.Exists(resolvedPath))
                    {
                        diagnostics.Add(Diagnostics.DiagnosticDescriptors.RefTargetNotFound(
                            path, pathOffset, pathDisplayLength));
                    }
                }
            }

            lineStart += line.Length + 1; // +1 for newline
        }
        return diagnostics;
    }

    /// <summary>
    /// Given a directory path, find .winmd files that could contain a given namespace.
    /// Tries: "Namespace.winmd", "Namespace.SubNs.winmd", walking up the namespace chain.
    /// Also returns all .winmd files in the directory as fallback.
    /// </summary>
    public static List<string> ResolveWinMdsFromDirectory(string dirPath, string? targetNamespace = null)
    {
        var results = new List<string>();
        if (!Directory.Exists(dirPath)) return results;

        try
        {
            var allWinmds = Directory.GetFiles(dirPath, "*.winmd", SearchOption.TopDirectoryOnly);

            // If we have a target namespace, try namespace-based matching first
            if (!string.IsNullOrEmpty(targetNamespace))
            {
                var ns = targetNamespace;
                while (!string.IsNullOrEmpty(ns))
                {
                    var candidate = Path.Combine(dirPath, ns + ".winmd");
                    if (File.Exists(candidate) && !results.Contains(candidate))
                        results.Add(candidate);
                    var dot = ns.LastIndexOf('.');
                    ns = dot >= 0 ? ns[..dot] : null;
                }
            }

            // Fallback: add all .winmd files from the directory
            foreach (var winmd in allWinmds)
            {
                if (!results.Contains(winmd))
                    results.Add(winmd);
            }
        }
        catch
        {
            // Ignore directory access errors
        }

        return results;
    }
}
