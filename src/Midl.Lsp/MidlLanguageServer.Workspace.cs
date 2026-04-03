using System.Diagnostics;
using System.Text.Json;
using Midl.Syntax;

namespace Midl.Lsp;

public sealed partial class MidlLanguageServer
{
    private async Task HandleInitializedAsync()
    {
        _eventLog.Log(DiagnosticEventLog.Categories.Init, "Server initialized",
            new() { ["workspaceRoot"] = _workspaceRoot ?? "(none)", ["compilationMode"] = _compilationMode.ToString() });

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
    private const long MemoryBudgetMB = 512; // abort scan if GC memory exceeds this
    private const int MaxErrorsBeforeSkip = 100; // skip files producing too many errors (likely not MIDL3)

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
            _eventLog.Log(DiagnosticEventLog.Categories.Scan, "Workspace scan started",
                new() { ["filesFound"] = idlFiles.Count.ToString(), ["recurse"] = recurse.ToString() });

            int compiled = 0;
            int skippedErrors = 0;
            int skippedMemory = 0;

            foreach (var filePath in idlFiles)
            {
                // Memory budget check (private bytes — matches Task Manager)
                var memMB = Process.GetCurrentProcess().PrivateMemorySize64 / (1024 * 1024);
                if (memMB > MemoryBudgetMB)
                {
                    _eventLog.Log(DiagnosticEventLog.Categories.Memory,
                        $"Memory budget exceeded ({memMB}MB > {MemoryBudgetMB}MB), aborting workspace scan",
                        new() { ["memoryMB"] = memMB.ToString(), ["filesCompiled"] = compiled.ToString(), ["filesRemaining"] = (idlFiles.Count - compiled).ToString() });
                    MidlTrace.Lsp($"Memory budget exceeded ({memMB}MB), aborting scan after {compiled} files");

                    try
                    {
                        await _writer.SendNotificationAsync("window/showMessage", new Dictionary<string, object?>
                        {
                            ["type"] = 2, // Warning
                            ["message"] = $"MIDL: Workspace scan stopped — memory usage is {memMB}MB. Consider adding a winmidl.json with \"scanSubfolders\": false."
                        });
                    }
                    catch { /* stream may be closed */ }

                    skippedMemory = idlFiles.Count - compiled;
                    break;
                }

                var uri = "file:///" + Uri.EscapeDataString(filePath.Replace('\\', '/')).Replace("%2F", "/");
                // Skip files already open in the editor
                if (_docManager.GetDocumentText(uri) != null)
                    continue;

                try
                {
                    var text = await File.ReadAllTextAsync(filePath);
                    var lineCount = text.Split('\n').Length;
                    var isMidl3 = LooksLikeMidl3(text);
                    var memBefore = Process.GetCurrentProcess().PrivateMemorySize64 / (1024 * 1024);
                    var sw = System.Diagnostics.Stopwatch.StartNew();

                    await AnalyzeAndPublishAsync(uri, text);
                    sw.Stop();

                    var memAfter = Process.GetCurrentProcess().PrivateMemorySize64 / (1024 * 1024);
                    compiled++;

                    // Collect stats from the compilation
                    int errorCount = 0;
                    int typeCount = 0;
                    if (_lastCompilation.TryGetValue(uri, out var comp))
                    {
                        errorCount = comp.Diagnostics.Count(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);
                        typeCount = comp.GetAllTypes().Count();
                    }

                    _eventLog.Log(DiagnosticEventLog.Categories.Compile, $"Compiled {RedactPath(filePath)}",
                        new()
                        {
                            ["file"] = RedactPath(filePath),
                            ["sizeBytes"] = text.Length.ToString(),
                            ["lines"] = lineCount.ToString(),
                            ["errors"] = errorCount.ToString(),
                            ["types"] = typeCount.ToString(),
                            ["durationMs"] = sw.ElapsedMilliseconds.ToString(),
                            ["memoryMB"] = memAfter.ToString(),
                            ["memoryDeltaMB"] = (memAfter - memBefore).ToString(),
                            ["isMidl3"] = isMidl3.ToString(),
                        });

                    // If this file produced too many errors, it's probably not MIDL3 — evict its compilation
                    if (errorCount > MaxErrorsBeforeSkip)
                    {
                        _lastCompilation.TryRemove(uri, out _);
                        _lastAnalyzedHash.TryRemove(uri, out _);
                        skippedErrors++;
                        _eventLog.Log(DiagnosticEventLog.Categories.Scan,
                            $"Evicted {RedactPath(filePath)} — too many errors ({errorCount}), likely not MIDL3");
                    }
                }
                catch (Exception)
                {
                }
            }

            _eventLog.Log(DiagnosticEventLog.Categories.Scan, "Workspace scan completed",
                new()
                {
                    ["filesCompiled"] = compiled.ToString(),
                    ["skippedHighErrors"] = skippedErrors.ToString(),
                    ["skippedMemoryBudget"] = skippedMemory.ToString(),
                    ["memoryMB"] = (Process.GetCurrentProcess().PrivateMemorySize64 / (1024 * 1024)).ToString(),
                });
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
            _eventLog.Log(DiagnosticEventLog.Categories.Watcher, $"File changed: {RedactUri(uri)}",
                new() { ["type"] = type == 1 ? "created" : type == 2 ? "changed" : "deleted" });

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
