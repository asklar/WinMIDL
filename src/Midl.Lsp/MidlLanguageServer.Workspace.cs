using System.Text.Json;
using Midl.Syntax;

namespace Midl.Lsp;

public sealed partial class MidlLanguageServer
{
    private async Task HandleInitializedAsync()
    {
        // Register for file watching. SendRequestAsync only writes the message
        // (doesn't await a response), so this won't block even without a real client.
        try
        {
            await _writer.SendRequestAsync("midl-register-1", "client/registerCapability", new Dictionary<string, object?>
            {
                ["registrations"] = new[]
                {
                    new Dictionary<string, object?>
                    {
                        ["id"] = "midl-file-watcher",
                        ["method"] = "workspace/didChangeWatchedFiles",
                        ["registerOptions"] = new Dictionary<string, object?>
                        {
                            ["watchers"] = new[]
                            {
                                new Dictionary<string, object?>
                                {
                                    ["globPattern"] = "**/*.idl"
                                }
                            }
                        }
                    }
                }
            });
        }
        catch { /* stream may be closed in test environments */ }

        // Scan workspace for .idl files and publish initial diagnostics
        await ScanWorkspaceAsync();
    }

    private const int MaxWorkspaceFiles = 100;
    private const long MaxFileSizeBytes = 100 * 1024; // 100 KB — skip huge generated files

    private static readonly HashSet<string> DefaultSkipDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "node_modules", ".git", "bin", "obj", ".vs", "packages", "AppData",
    };

    internal async Task ScanWorkspaceAsync()
    {
        if (_workspaceRoot is null || !Directory.Exists(_workspaceRoot))
            return;

        try
        {
            // Merge default skip list with project-level excludes
            var skipDirs = new HashSet<string>(DefaultSkipDirectories, StringComparer.OrdinalIgnoreCase);
            if (_projectFile?.ExcludeDirectories is { Count: > 0 } projExcludes)
                foreach (var d in projExcludes) skipDirs.Add(d);

            bool recurse = _projectFile?.ScanSubfolders ?? true;

            var idlFiles = EnumerateIdlFiles(_workspaceRoot, skipDirs, recurse)
                .Take(MaxWorkspaceFiles)
                .ToList();

            MidlTrace.Lsp($"Workspace scan: found {idlFiles.Count} .idl files in {_workspaceRoot}");

            foreach (var filePath in idlFiles)
            {
                var uri = "file:///" + Uri.EscapeDataString(filePath.Replace('\\', '/')).Replace("%2F", "/");
                // Skip files already open in the editor
                if (_docManager.GetDocumentText(uri) != null)
                    continue;

                try
                {
                    var text = await File.ReadAllTextAsync(filePath);
                    await AnalyzeAndPublishAsync(uri, text);
                }
                catch (Exception)
                {
                }
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// Enumerate .idl files, skipping known non-source directories and files over the size limit.
    /// </summary>
    private static IEnumerable<string> EnumerateIdlFiles(string root, HashSet<string> skipDirs, bool recurse)
    {
        Queue<string> dirs = new();
        dirs.Enqueue(root);

        while (dirs.Count > 0)
        {
            var dir = dirs.Dequeue();

            // Enqueue subdirectories, skipping excluded ones
            if (recurse || dir == root) // always process root, recurse into children only if enabled
            {
                try
                {
                    if (recurse)
                    {
                        foreach (var subDir in Directory.EnumerateDirectories(dir))
                        {
                            var dirName = Path.GetFileName(subDir);
                            if (!skipDirs.Contains(dirName))
                                dirs.Enqueue(subDir);
                        }
                    }
                }
                catch { /* access denied, etc. */ }
            }

            // Yield .idl files under size limit
            IEnumerable<string> files;
            try { files = Directory.EnumerateFiles(dir, "*.idl"); }
            catch { continue; }

            foreach (var file in files)
            {
                long length = 0;
                try { length = new FileInfo(file).Length; }
                catch { continue; }

                if (length > MaxFileSizeBytes)
                {
                    MidlTrace.Lsp($"  Skipping large file ({length / 1024} KB): {file}");
                    continue;
                }
                yield return file;
            }
        }
    }

    private async Task HandleWatchedFileChangesAsync(JsonElement? @params)
    {
        if (@params is null) return;

        if (!@params.Value.TryGetProperty("changes", out var changes))
            return;

        foreach (var change in changes.EnumerateArray())
        {
            var uri = change.GetProperty("uri").GetString();
            var type = change.GetProperty("type").GetInt32();
            if (uri is null) continue;

            // Only handle .idl files
            if (!uri.EndsWith(".idl", StringComparison.OrdinalIgnoreCase))
                continue;

            MidlTrace.Lsp($"File watcher: {uri} type={type}");

            if (type == 3) // Deleted
            {
                // Clear diagnostics for deleted file
                await _diagnostics.PublishDiagnosticsAsync(uri, "", []);
                _lastAnalyzedHash.TryRemove(uri, out _);
                _lastCompilation.TryRemove(uri, out _);
            }
            else // Created (1) or Changed (2)
            {
                // Read file from disk and analyze
                string? filePath = null;
                if (uri.StartsWith("file:///"))
                    filePath = Uri.UnescapeDataString(uri[8..]);

                if (filePath != null && File.Exists(filePath))
                {
                    try
                    {
                        // Force re-analysis by clearing cached hash
                        _lastAnalyzedHash.TryRemove(uri, out _);
                        var text = await File.ReadAllTextAsync(filePath);
                        await AnalyzeAndPublishAsync(uri, text);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        // Re-analyze any open documents that might be affected by the changed files
        foreach (var openUri in _docManager.GetOpenDocumentUris())
        {
            var openText = _docManager.GetDocumentText(openUri);
            if (openText != null)
            {
                _lastAnalyzedHash.TryRemove(openUri, out _);
                await AnalyzeAndPublishAsync(openUri, openText);
            }
        }
    }
}
