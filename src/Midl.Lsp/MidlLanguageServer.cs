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
public sealed partial class MidlLanguageServer
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
    private bool _useWindowsSdk = true;
    private bool _flatSdkTypeCompletions;
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
            catch (Exception)
            {
                break;
            }


            using (msg)
            {
                if (msg is null) break; // EOF or stream closed
                var method = msg.RootElement.TryGetProperty("method", out var m) ? m.GetString() : "(response)";
                MidlTrace.Lsp($"Received: {method}");
                await HandleMessageAsync(msg.RootElement);
            }
        }
    }

    private async Task HandleMessageAsync(JsonElement msg)
    {
        var hasId = msg.TryGetProperty("id", out var idElement);
        object? id = hasId ? GetIdValue(idElement) : null;
        var method = msg.TryGetProperty("method", out var m) ? m.GetString() : null;
        var @params = msg.TryGetProperty("params", out var p) ? (JsonElement?)p : null;

        if (method is null)
            return; // Response messages are ignored (we don't make requests)


        // Skip all textDocument/* requests for virtual decompiled documents
        // (they would try to compile the decompiled IDL text and hang)
        if (method.StartsWith("textDocument/") && method != "textDocument/didOpen" && method != "textDocument/didChange" && method != "textDocument/didClose")
        {
            var docUri = @params?.TryGetProperty("textDocument", out var td) == true
                ? (td.TryGetProperty("uri", out var u) ? u.GetString() : null)
                : null;
            if (docUri != null && docUri.StartsWith("midl-winmd:", StringComparison.OrdinalIgnoreCase))
            {
                if (id != null)
                    await _writer.SendResponseAsync(id, (object?)null);
                return;
            }
        }

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
                {
                    var compUri = @params?.GetProperty("textDocument").GetProperty("uri").GetString();
                    Compilation? cachedComp = null;
                    if (compUri != null) _lastCompilation.TryGetValue(compUri, out cachedComp);
                    await _writer.SendResponseAsync(id, CompletionHandler.Handle(@params, _docManager, _sdkIncludePaths, _projectFile, cachedComp, _flatSdkTypeCompletions));
                }
                break;

            case "textDocument/hover":
                {
                    var hoverUri = @params?.GetProperty("textDocument").GetProperty("uri").GetString();
                    Compilation? hoverComp = null;
                    if (hoverUri != null) _lastCompilation.TryGetValue(hoverUri, out hoverComp);
                    await _writer.SendResponseAsync(id, HoverHandler.Handle(@params, _docManager, hoverComp));
                }
                break;

            case "textDocument/documentSymbol":
                await _writer.SendResponseAsync(id, DocumentSymbolHandler.Handle(@params, _docManager));
                break;

            case "textDocument/definition":
                {
                    var defUri = @params?.GetProperty("textDocument").GetProperty("uri").GetString();
                    Compilation? defCompilation = null;
                    if (defUri != null) _lastCompilation.TryGetValue(defUri, out defCompilation);
                    var defRefs = defUri != null ? GetReferencePaths(defUri) : _referencePaths;
                    string? defFilePath = defUri?.StartsWith("file:///") == true ? Uri.UnescapeDataString(defUri[8..]) : null;
                    var defIncludes = GetIncludePaths(defFilePath);
                    var defResult = GotoDefinitionHandler.Handle(@params, _docManager, defIncludes, defRefs, defCompilation);
                    await _writer.SendResponseAsync(id, defResult);
                    break;
                }

            case "midl/decompileDefinition":
                var decompResult = HandleDecompileDefinition(@params);
                await _writer.SendResponseAsync(id, decompResult);
                break;

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

                    if (initOpts.TryGetProperty("midl.useWindowsSdk", out var sdkProp) &&
                        sdkProp.ValueKind == JsonValueKind.False)
                    {
                        _useWindowsSdk = false;
                    }
                    else if (initOpts.TryGetProperty("useWindowsSdk", out var sdkProp2) &&
                             sdkProp2.ValueKind == JsonValueKind.False)
                    {
                        _useWindowsSdk = false;
                    }

                    if (initOpts.TryGetProperty("midl.flatSdkTypeCompletions", out var flatProp) &&
                        flatProp.ValueKind == JsonValueKind.True)
                    {
                        _flatSdkTypeCompletions = true;
                    }
                    else if (initOpts.TryGetProperty("flatSdkTypeCompletions", out var flatProp2) &&
                             flatProp2.ValueKind == JsonValueKind.True)
                    {
                        _flatSdkTypeCompletions = true;
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
                catch (Exception)
                {
                }
            }
        }

        // Auto-discover Windows.winmd from SDK if enabled
        if (_useWindowsSdk)
        {
            string? sdkVersion = null;
            if (@params is not null)
            {
                try
                {
                    if (@params.Value.TryGetProperty("initializationOptions", out var initOpts2))
                    {
                        if (initOpts2.TryGetProperty("midl.windowsSdkVersion", out var verProp))
                            sdkVersion = verProp.GetString();
                        else if (initOpts2.TryGetProperty("windowsSdkVersion", out var verProp2))
                            sdkVersion = verProp2.GetString();
                    }
                }
                catch { }
            }
            var winmd = Handlers.WinMdNavigator.FindWindowsWinMd(sdkVersion);
            if (winmd != null)
            {
                _referencePaths ??= new List<string>();
                if (!_referencePaths.Contains(winmd))
                {
                    _referencePaths.Add(winmd);
                    MidlTrace.Lsp($"Auto-discovered Windows SDK: {winmd}");
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
