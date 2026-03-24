using Midl.Analysis;

namespace Midl.Lsp;

public sealed partial class MidlLanguageServer
{
    private List<LazyWinMdProvider>? GetCachedProviders(IReadOnlyList<string>? referencePaths)
    {
        if (referencePaths is null or { Count: 0 }) return null;
        var providers = new List<LazyWinMdProvider>();
        foreach (var path in referencePaths)
        {
            var provider = _winmdProviderCache.GetOrAdd(path, p => new LazyWinMdProvider(p));
            providers.Add(provider);
        }
        return providers;
    }

    private List<string>? GetReferencePaths(string uri)
    {
        var paths = new List<string>();

        // Explicit references from configuration
        if (_referencePaths is { Count: > 0 })
            paths.AddRange(_referencePaths);

        // References from winmidl.json project file
        if (_projectFile != null)
            paths.AddRange(_projectFile.GetAbsoluteReferencePaths().Where(File.Exists));

        // Parse // @ref: directives — .winmd files and directories
        string? filePath = null;
        if (uri.StartsWith("file:///"))
            filePath = Uri.UnescapeDataString(uri[8..]);

        List<string>? refDirs = null;
        {
            var text = _docManager.GetDocumentText(uri);
            if (text != null)
            {
                // For file:/// URIs, resolve relative paths against the file's directory.
                // For untitled/other URIs, dir is null — only absolute @ref paths will resolve.
                var dir = filePath != null ? Path.GetDirectoryName(filePath) : null;
                var refPaths = RefDirectiveParser.ParseRefs(text, dir);
                foreach (var refPath in refPaths)
                {
                    if (Directory.Exists(refPath))
                    {
                        // Directory ref — collect all .winmd files in it
                        refDirs ??= new List<string>();
                        refDirs.Add(refPath);
                        var winmds = RefDirectiveParser.ResolveWinMdsFromDirectory(refPath);
                        foreach (var winmd in winmds)
                        {
                            if (!paths.Contains(winmd))
                                paths.Add(winmd);
                        }
                    }
                    else if (refPath.EndsWith(".winmd", StringComparison.OrdinalIgnoreCase) && !paths.Contains(refPath))
                    {
                        paths.Add(refPath);
                    }
                }
            }
        }

        // Store directory refs for namespace-based lazy resolution
        _refDirectories = refDirs;

        if (paths.Count > 0)
            return paths;

        // Fallback: scan workspace directory for .winmd files
        try
        {
            string? dir = filePath != null ? Path.GetDirectoryName(filePath) : null;

            if (dir != null && Directory.Exists(dir))
            {
                var winmdFiles = Directory.GetFiles(dir, "*.winmd", SearchOption.TopDirectoryOnly);
                if (winmdFiles.Length > 0)
                    return new List<string>(winmdFiles);
            }
        }
        catch
        {
            // Ignore errors in workspace scanning
        }

        return null;
    }

    private List<string>? GetIncludePaths(string? filePath)
    {
        var paths = new List<string>();

        // SDK include paths
        if (_sdkIncludePaths is { Count: > 0 })
            paths.AddRange(_sdkIncludePaths);

        // Project file include paths
        if (_projectFile != null)
            paths.AddRange(_projectFile.GetAbsoluteIncludePaths());

        // File's own directory for relative imports
        if (filePath != null)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (dir != null && !paths.Contains(dir))
                paths.Add(dir);
        }

        // Parse // @ref: directives from sibling IDL files for cross-file context
        if (filePath != null)
        {
            var text = _docManager.GetDocumentText(
                "file:///" + Uri.EscapeDataString(filePath).Replace("%5C", "/").Replace("%2F", "/"));
            if (text != null)
            {
                var dir = Path.GetDirectoryName(filePath);
                var refPaths = RefDirectiveParser.ParseRefs(text, dir);
                foreach (var refPath in refPaths)
                {
                    var refDir = Path.GetDirectoryName(refPath);
                    if (refDir != null && !paths.Contains(refDir))
                        paths.Add(refDir);
                }
            }
        }

        return paths.Count > 0 ? paths : null;
    }
}
