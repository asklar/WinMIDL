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

    internal async Task ScanWorkspaceAsync()
    {
        if (_workspaceRoot is null || !Directory.Exists(_workspaceRoot))
            return;

        try
        {
            var idlFiles = Directory.EnumerateFiles(_workspaceRoot, "*.idl", SearchOption.AllDirectories)
                .Where(f => !IsExcludedPath(f))
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
    /// Exclude files in dependency/build directories from workspace scanning.
    /// These are typically MIDL2/COM files from NuGet packages, not user-authored MIDL3.
    /// </summary>
    private static bool IsExcludedPath(string filePath)
    {
        var normalized = filePath.Replace('\\', '/');
        // Exclude NuGet packages, build output, node_modules, .git, etc.
        return normalized.Contains("/packages/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/node_modules/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/.git/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/.vs/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/Generated Files/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/.nuget/", StringComparison.OrdinalIgnoreCase);
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
