using System.Collections.Concurrent;
using System.Text.Json;
using Midl.Analysis;
using Midl.Lsp.Handlers;
using Midl.Syntax;

namespace Midl.Lsp;

/// <summary>
/// MIDL Language Server Protocol implementation.
/// Communicates over stdin/stdout using JSON-RPC per the LSP specification.
/// </summary>
public sealed class MidlLanguageServer
{
    private readonly JsonRpcReader _reader;
    private readonly JsonRpcWriter _writer;
    private readonly DocumentManager _docManager = new();
    private readonly DiagnosticsPublisher _diagnostics;
    private bool _shutdownRequested;
    private List<string>? _referencePaths = null;
    private List<string>? _refDirectories = null;
    private List<string>? _sdkIncludePaths = null;
    private CompilationMode _compilationMode = CompilationMode.Strict;
    private bool _enableQualityHints = true;
    private ProjectFile? _projectFile;
    private string? _workspaceRoot;

    // Debounce: delay analysis after last edit to avoid recompiling on every keystroke
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _debounceTokens = new();
    private const int DebounceDelayMs = 300;

    // Content cache: skip recompilation if text hasn't changed
    private readonly ConcurrentDictionary<string, int> _lastAnalyzedHash = new();

    // WinMD provider cache: reuse providers across compilations to avoid re-reading .winmd files
    private readonly ConcurrentDictionary<string, LazyWinMdProvider> _winmdProviderCache = new();

    // Compilation cache: reuse last compilation result for code actions / completions
    private readonly ConcurrentDictionary<string, Compilation> _lastCompilation = new();

    // Suppress CS0649 — _referencePaths can be set via SetReferencePaths for testing/config
    public void SetReferencePaths(List<string> paths) => _referencePaths = paths;
    public void SetSdkIncludePaths(List<string> paths) => _sdkIncludePaths = paths;

    public MidlLanguageServer(Stream input, Stream output)
    {
        _reader = new JsonRpcReader(input);
        _writer = new JsonRpcWriter(output);
        _diagnostics = new DiagnosticsPublisher(_writer);
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        Console.Error.WriteLine("[MIDL LSP] Server starting, waiting for messages...");
        while (!ct.IsCancellationRequested)
        {
            JsonDocument? msg;
            try
            {
                msg = await _reader.ReadMessageAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[MIDL LSP] Read error: {ex.Message}");
                break;
            }

            if (msg is null) { Console.Error.WriteLine("[MIDL LSP] Stream closed"); break; }

            using (msg)
            {
                var method = msg.RootElement.TryGetProperty("method", out var m) ? m.GetString() : "(response)";
                MidlTrace.Lsp($"Received: {method}");
                await HandleMessageAsync(msg.RootElement);
            }
        }
        Console.Error.WriteLine("[MIDL LSP] Server exiting");
    }

    private async Task HandleMessageAsync(JsonElement msg)
    {
        var hasId = msg.TryGetProperty("id", out var idElement);
        object? id = hasId ? GetIdValue(idElement) : null;
        var method = msg.TryGetProperty("method", out var m) ? m.GetString() : null;
        var @params = msg.TryGetProperty("params", out var p) ? (JsonElement?)p : null;

        if (method is null)
            return; // Response messages are ignored (we don't make requests)

        switch (method)
        {
            case "initialize":
                await HandleInitializeAsync(id, @params);
                break;

            case "initialized":
                await HandleInitializedAsync();
                break;

            case "workspace/didChangeWatchedFiles":
                await HandleWatchedFileChangesAsync(@params);
                break;

            case "shutdown":
                _shutdownRequested = true;
                await _writer.SendResponseAsync(id, null);
                break;

            case "exit":
                if (!_shutdownRequested)
                {
                    // exit without prior shutdown is a protocol violation — exit with code 1
                    Environment.ExitCode = 1;
                }
                return; // The RunAsync loop will exit on stream close

            case "textDocument/didOpen":
                await HandleDidOpenAsync(@params);
                break;

            case "textDocument/didChange":
                await HandleDidChangeAsync(@params);
                break;

            case "textDocument/didClose":
                HandleDidClose(@params);
                break;

            case "textDocument/completion":
                await _writer.SendResponseAsync(id, CompletionHandler.Handle(@params, _docManager, _sdkIncludePaths, _projectFile));
                break;

            case "textDocument/hover":
                await _writer.SendResponseAsync(id, HoverHandler.Handle(@params, _docManager));
                break;

            case "textDocument/documentSymbol":
                await _writer.SendResponseAsync(id, DocumentSymbolHandler.Handle(@params, _docManager));
                break;

            case "textDocument/definition":
                {
                    var defUri = @params?.GetProperty("textDocument").GetProperty("uri").GetString();
                    Console.Error.WriteLine($"[MIDL:LSP] definition request for: {defUri?.Substring(0, Math.Min(60, defUri?.Length ?? 0))}");
                    Compilation? defCompilation = null;
                    if (defUri != null) _lastCompilation.TryGetValue(defUri, out defCompilation);
                    var defRefs = defUri != null ? GetReferencePaths(defUri) : _referencePaths;
                    string? defFilePath = defUri?.StartsWith("file:///") == true ? Uri.UnescapeDataString(defUri[8..]) : null;
                    var defIncludes = GetIncludePaths(defFilePath);
                    var defResult = GotoDefinitionHandler.Handle(@params, _docManager, defIncludes, defRefs, defCompilation);
                    Console.Error.WriteLine($"[MIDL:LSP] definition result: {(defResult == null ? "null" : "found")}");
                    await _writer.SendResponseAsync(id, defResult);
                    break;
                }

            case "textDocument/references":
                await _writer.SendResponseAsync(id, FindReferencesHandler.Handle(@params, _docManager));
                break;

            case "textDocument/signatureHelp":
                await _writer.SendResponseAsync(id, SignatureHelpHandler.Handle(@params, _docManager));
                break;

            case "textDocument/semanticTokens/full":
                await _writer.SendResponseAsync(id, SemanticTokensHandler.Handle(@params, _docManager));
                break;

            case "textDocument/formatting":
                await _writer.SendResponseAsync(id, FormattingHandler.Handle(@params, _docManager));
                break;

            case "textDocument/rename":
                await _writer.SendResponseAsync(id, RenameHandler.Handle(@params, _docManager));
                break;

            case "textDocument/prepareRename":
                await _writer.SendResponseAsync(id, RenameHandler.HandlePrepare(@params, _docManager));
                break;

            case "textDocument/codeAction":
            {
                string? caFilePath = null;
                if (@params is not null)
                {
                    var caTd = @params.Value.GetProperty("textDocument");
                    var caUri = caTd.GetProperty("uri").GetString()!;
                    if (caUri.StartsWith("file:///"))
                        caFilePath = Uri.UnescapeDataString(caUri[8..]);
                }
                var caRefs = caFilePath != null ? GetReferencePaths(@params!.Value.GetProperty("textDocument").GetProperty("uri").GetString()!) : _referencePaths;
                var caIncludes = GetIncludePaths(caFilePath);
                await _writer.SendResponseAsync(id, CodeActionHandler.Handle(@params, _docManager, caRefs, caIncludes, caFilePath));
                break;
            }

            default:
                if (hasId)
                {
                    // Unknown request — respond with MethodNotFound
                    await _writer.SendErrorAsync(id, -32601, $"Method not found: {method}");
                }
                // Unknown notifications are silently ignored per LSP spec
                break;
        }
    }

    private async Task HandleInitializeAsync(object? id, JsonElement? @params)
    {
        // Read midl.strictMode from initialization options
        if (@params is not null)
        {
            try
            {
                if (@params.Value.TryGetProperty("initializationOptions", out var initOpts))
                {
                    if (initOpts.TryGetProperty("midl.strictMode", out var strictProp) &&
                        strictProp.ValueKind == JsonValueKind.False)
                    {
#if WITH_COMPAT
                        _compilationMode = CompilationMode.Compat;
#endif
                    }
                    else if (initOpts.TryGetProperty("strictMode", out var strictProp2) &&
                             strictProp2.ValueKind == JsonValueKind.False)
                    {
#if WITH_COMPAT
                        _compilationMode = CompilationMode.Compat;
#endif
                    }

                    if (initOpts.TryGetProperty("midl.codeQualityHints", out var qualityProp) &&
                        qualityProp.ValueKind == JsonValueKind.False)
                    {
                        _enableQualityHints = false;
                    }
                    else if (initOpts.TryGetProperty("codeQualityHints", out var qualityProp2) &&
                             qualityProp2.ValueKind == JsonValueKind.False)
                    {
                        _enableQualityHints = false;
                    }
                }

                // Discover workspace root
                if (@params.Value.TryGetProperty("rootUri", out var rootUri) &&
                    rootUri.GetString() is { } rootUriStr && rootUriStr.StartsWith("file:///"))
                {
                    _workspaceRoot = Uri.UnescapeDataString(rootUriStr[8..]);
                }
                else if (@params.Value.TryGetProperty("rootPath", out var rootPath) &&
                         rootPath.GetString() is { } rootPathStr)
                {
                    _workspaceRoot = rootPathStr;
                }
            }
            catch
            {
                // Ignore errors reading initialization options
            }
        }

        // Discover winmidl.json project file in workspace
        if (_workspaceRoot != null)
        {
            var projectPath = Path.Combine(_workspaceRoot, "winmidl.json");
            if (File.Exists(projectPath))
            {
                try
                {
                    _projectFile = ProjectFile.Load(projectPath);
                    MidlTrace.Lsp($"Loaded winmidl.json: {_projectFile.Inputs.Count} inputs, {_projectFile.References.Count} references");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[MIDL LSP] Failed to load winmidl.json: {ex.Message}");
                }
            }
        }

        var capabilities = new Dictionary<string, object?>
        {
            ["capabilities"] = new Dictionary<string, object?>
            {
                ["textDocumentSync"] = new Dictionary<string, object?>
                {
                    ["openClose"] = true,
                    ["change"] = 1, // Full document sync
                    ["save"] = new Dictionary<string, object?> { ["includeText"] = true }
                },
                ["completionProvider"] = new Dictionary<string, object?>
                {
                    ["triggerCharacters"] = new[] { ".", " ", "[", "\"", "/", "\\", ":" },
                    ["resolveProvider"] = false
                },
                ["hoverProvider"] = true,
                ["documentSymbolProvider"] = true,
                ["definitionProvider"] = true,
                ["referencesProvider"] = true,
                ["signatureHelpProvider"] = new Dictionary<string, object?>
                {
                    ["triggerCharacters"] = new[] { "(", "," }
                },
                ["semanticTokensProvider"] = new Dictionary<string, object?>
                {
                    ["full"] = true,
                    ["legend"] = new Dictionary<string, object?>
                    {
                        ["tokenTypes"] = Handlers.SemanticTokensHandler.TokenTypes,
                        ["tokenModifiers"] = Handlers.SemanticTokensHandler.TokenModifiers
                    }
                },
                ["documentFormattingProvider"] = true,
                ["renameProvider"] = new Dictionary<string, object?>
                {
                    ["prepareProvider"] = true
                },
                ["codeActionProvider"] = new Dictionary<string, object?>
                {
                    ["codeActionKinds"] = new[] { "quickfix" }
                },
                ["workspace"] = new Dictionary<string, object?>
                {
                    ["workspaceFolders"] = new Dictionary<string, object?>
                    {
                        ["supported"] = true,
                    },
                }
            },
            ["serverInfo"] = new Dictionary<string, object?>
            {
                ["name"] = "midl-language-server",
                ["version"] = "0.1.0"
            }
        };
        await _writer.SendResponseAsync(id, capabilities);
    }

    private async Task HandleDidOpenAsync(JsonElement? @params)
    {
        if (@params is null) return;

        var td = @params.Value.GetProperty("textDocument");
        var uri = td.GetProperty("uri").GetString()!;
        var text = td.GetProperty("text").GetString()!;

        _docManager.OpenDocument(uri, text);

        // Skip analysis for virtual documents (decompiled WinMD views)
        if (uri.StartsWith("midl-winmd://")) return;

        await AnalyzeAndPublishAsync(uri, text);
    }

    private async Task HandleDidChangeAsync(JsonElement? @params)
    {
        if (@params is null) return;

        var td = @params.Value.GetProperty("textDocument");
        var uri = td.GetProperty("uri").GetString()!;

        var changes = @params.Value.GetProperty("contentChanges");
        if (changes.GetArrayLength() > 0)
        {
            var text = changes[0].GetProperty("text").GetString()!;
            _docManager.UpdateDocument(uri, text);

            // Debounce: cancel any pending analysis for this URI, schedule a new one
            if (_debounceTokens.TryRemove(uri, out var oldCts))
                oldCts.Cancel();

            var cts = new CancellationTokenSource();
            _debounceTokens[uri] = cts;

            try
            {
                await Task.Delay(DebounceDelayMs, cts.Token);
                _debounceTokens.TryRemove(uri, out _);
                await AnalyzeAndPublishAsync(uri, text);
            }
            catch (TaskCanceledException)
            {
                // Superseded by a newer edit — expected
            }
        }
    }

    private void HandleDidClose(JsonElement? @params)
    {
        if (@params is null) return;

        var td = @params.Value.GetProperty("textDocument");
        var uri = td.GetProperty("uri").GetString()!;
        _docManager.CloseDocument(uri);
        _lastAnalyzedHash.TryRemove(uri, out _);
        _lastCompilation.TryRemove(uri, out _);
        if (_debounceTokens.TryRemove(uri, out var cts))
            cts.Cancel();
    }

    private async Task HandleInitializedAsync()
    {
        // Register for file watching so we get workspace/didChangeWatchedFiles notifications
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
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[MIDL LSP] Error reading {filePath}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[MIDL LSP] Workspace scan error: {ex.Message}");
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
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[MIDL LSP] Error reading changed file {filePath}: {ex.Message}");
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

    private async Task AnalyzeAndPublishAsync(string uri, string text)
    {
        // Skip recompilation if the text hasn't changed since last analysis
        var hash = text.GetHashCode();
        if (_lastAnalyzedHash.TryGetValue(uri, out var lastHash) && lastHash == hash)
        {
            MidlTrace.Lsp($"Skipping analysis for {uri} (unchanged)");
            return;
        }

        try
        {
            MidlTrace.Lsp($"Analyzing {uri} ({text.Length} chars)");
            _lastAnalyzedHash[uri] = hash;

            // Resolve the file path for cross-file context
            string? filePath = null;
            if (uri.StartsWith("file:///"))
                filePath = Uri.UnescapeDataString(uri[8..]);

            var references = GetReferencePaths(uri);
            var includePaths = GetIncludePaths(filePath);
            var defines = _projectFile?.Defines;
            var cachedProviders = GetCachedProviders(references);

            // Use cached providers when available to skip disk I/O
            Compilation compilation;
            if (cachedProviders != null)
            {
                compilation = CompilationHelper.Compile(text, cachedProviders, _compilationMode,
                    includePaths, filePath,
                    defines: defines,
                    emitQualityHints: _enableQualityHints,
                    refDirectories: _refDirectories);
            }
            else
            {
                compilation = CompilationHelper.Compile(text, references, _compilationMode,
                    includePaths, filePath,
                    defines: defines,
                    emitQualityHints: _enableQualityHints,
                    refDirectories: _refDirectories);
            }

            // Cache compilation for reuse by code action / completion handlers
            _lastCompilation[uri] = compilation;

            MidlTrace.Lsp($"Analysis done: {compilation.Diagnostics.Count} diagnostics");
            await _diagnostics.PublishDiagnosticsAsync(uri, text, compilation.Diagnostics);
            MidlTrace.Lsp($"Diagnostics published");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[MIDL LSP] Analysis error: {ex}");
            var errorDiag = new Analysis.Diagnostics.Diagnostic(
                Analysis.Diagnostics.DiagnosticSeverity.Error,
                "MIDL0000",
                $"Internal compiler error: {ex.Message}",
                0, 0);
            await _diagnostics.PublishDiagnosticsAsync(uri, text, [errorDiag]);
        }
    }

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
        if (filePath != null)
        {
            var text = _docManager.GetDocumentText(uri);
            if (text != null)
            {
                var dir = Path.GetDirectoryName(filePath);
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

    private static object? GetIdValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.GetInt64(),
            JsonValueKind.String => element.GetString(),
            _ => null
        };
    }
}
