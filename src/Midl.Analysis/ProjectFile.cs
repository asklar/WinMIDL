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
/// </summary>
public static class RefDirectiveParser
{
    /// <summary>
    /// Parse // @ref: directives from source text.
    /// Returns resolved absolute paths relative to the given base directory.
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
                if (!string.IsNullOrEmpty(path))
                {
                    if (baseDir != null && !Path.IsPathRooted(path))
                        path = Path.GetFullPath(Path.Combine(baseDir, path));
                    if (File.Exists(path))
                        refs.Add(path);
                }
            }

            // Stop scanning after the first non-comment, non-blank line
            if (!trimmed.StartsWith("//") && trimmed.Length > 0)
                break;
        }
        return refs;
    }
}
