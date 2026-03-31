using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Midl.Analysis;
using Midl.Lsp.Handlers;
using Midl.Lsp.Protocol;
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
    private bool _arraysEnabled;
    private List<string> _defines = new();
    private bool _useWindowsSdk = true;
    private bool _flatSdkTypeCompletions;
    private bool _clientSupportsMarkdown = true; // VS Code sends markdown; VS sends plaintext only
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

        // Enable trace logging to file for diagnostics (no stderr tee — avoids console noise in tests)
        if (!MidlTrace.IsEnabled)
        {
            var logPath = Environment.GetEnvironmentVariable("MIDL_DEBUG_LOG")
                ?? Path.Combine(Path.GetTempPath(), "midl-lsp.log");
            MidlTrace.Enable(logPath, alsoWriteToStdErr: false);
            MidlTrace.Lsp($"Server started, logging to {logPath}");
        }
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
                var memMB = GC.GetTotalMemory(false) / (1024 * 1024);
                MidlTrace.Lsp($"Received: {method} (mem={memMB}MB, compilations={_lastCompilation.Count})");
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
        // Block most textDocument requests for decompiled WinMD documents
        // (they would try to compile the decompiled IDL text and hang)
        // Allow semantic tokens through — they work from lexer-only classification
        if (method.StartsWith("textDocument/") && method != "textDocument/didOpen" && method != "textDocument/didChange" && method != "textDocument/didClose" && method != "textDocument/semanticTokens/full")
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

        try
        {
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

            case "workspace/didChangeConfiguration":
                HandleDidChangeConfiguration(@params);
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
                    var hoverResult = HoverHandler.Handle(@params, _docManager, hoverComp);
                    // Strip markdown for clients that only support plaintext (e.g. VS)
                    if (!_clientSupportsMarkdown && hoverResult is HoverResult hr)
                    {
                        var plain = StripMarkdown(hr.Contents.Value);
                        hoverResult = new HoverResult(new MarkupContent("plaintext", plain), hr.Range);
                    }
                    await _writer.SendResponseAsync(id, hoverResult);
                }
                break;

            case "textDocument/documentSymbol":
                {
                    var dsUri = @params?.GetProperty("textDocument").GetProperty("uri").GetString();
                    Compilation? dsComp = null;
                    if (dsUri != null) _lastCompilation.TryGetValue(dsUri, out dsComp);
                    await _writer.SendResponseAsync(id, DocumentSymbolHandler.Handle(@params, _docManager, dsComp));
                }
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
                {
                    var refUri = @params?.GetProperty("textDocument").GetProperty("uri").GetString();
                    Compilation? refComp = null;
                    if (refUri != null) _lastCompilation.TryGetValue(refUri, out refComp);
                    await _writer.SendResponseAsync(id, FindReferencesHandler.Handle(@params, _docManager, refComp));
                }
                break;

            case "textDocument/signatureHelp":
                {
                    var shUri = @params?.GetProperty("textDocument").GetProperty("uri").GetString();
                    Compilation? shComp = null;
                    if (shUri != null) _lastCompilation.TryGetValue(shUri, out shComp);
                    await _writer.SendResponseAsync(id, SignatureHelpHandler.Handle(@params, _docManager, shComp));
                    break;
                }

            case "textDocument/semanticTokens/full":
                {
                    var stUri = @params?.GetProperty("textDocument").GetProperty("uri").GetString();
                    Compilation? stComp = null;
                    if (stUri != null) _lastCompilation.TryGetValue(stUri, out stComp);
                    await _writer.SendResponseAsync(id, SemanticTokensHandler.Handle(@params, _docManager, stComp));
                }
                break;

            case "textDocument/formatting":
                await _writer.SendResponseAsync(id, FormattingHandler.Handle(@params, _docManager));
                break;

            case "textDocument/onTypeFormatting":
                await _writer.SendResponseAsync(id, OnTypeFormattingHandler.Handle(@params, _docManager));
                break;

            case "textDocument/rename":
                {
                    var renUri = @params?.GetProperty("textDocument").GetProperty("uri").GetString();
                    Compilation? renComp = null;
                    if (renUri != null) _lastCompilation.TryGetValue(renUri, out renComp);
                    await _writer.SendResponseAsync(id, RenameHandler.Handle(@params, _docManager, renComp));
                }
                break;

            case "textDocument/prepareRename":
                {
                    var prUri = @params?.GetProperty("textDocument").GetProperty("uri").GetString();
                    Compilation? prComp = null;
                    if (prUri != null) _lastCompilation.TryGetValue(prUri, out prComp);
                    await _writer.SendResponseAsync(id, RenameHandler.HandlePrepare(@params, _docManager, prComp));
                }
                break;

            case "textDocument/codeAction":
            {
                string? caFilePath = null;
                string? caUri = null;
                if (@params is not null)
                {
                    var caTd = @params.Value.GetProperty("textDocument");
                    caUri = caTd.GetProperty("uri").GetString()!;
                    if (caUri.StartsWith("file:///"))
                        caFilePath = Uri.UnescapeDataString(caUri[8..]);
                }
                var caRefs = caFilePath != null ? GetReferencePaths(@params!.Value.GetProperty("textDocument").GetProperty("uri").GetString()!) : _referencePaths;
                var caIncludes = GetIncludePaths(caFilePath);
                Compilation? caComp = null;
                if (caUri != null) _lastCompilation.TryGetValue(caUri, out caComp);
                await _writer.SendResponseAsync(id, CodeActionHandler.Handle(@params, _docManager, caRefs, caIncludes, caFilePath, caComp));
                break;
            }

            case "textDocument/codeLens":
                {
                    var clUri = @params?.GetProperty("textDocument").GetProperty("uri").GetString();
                    Compilation? clComp = null;
                    if (clUri != null) _lastCompilation.TryGetValue(clUri, out clComp);
                    await _writer.SendResponseAsync(id, CodeLensHandler.Handle(@params, _docManager, clComp));
                }
                break;

            case "textDocument/foldingRange":
                await _writer.SendResponseAsync(id, FoldingRangeHandler.Handle(@params, _docManager));
                break;

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
        catch (Exception ex)
        {
            MidlTrace.Lsp($"Handler error for {method}: {ex.Message}");
            if (hasId)
            {
                try { await _writer.SendErrorAsync(id, -32603, $"Internal error: {ex.Message}"); }
                catch { /* stream may be closed */ }
            }
        }
    }

    /// <summary>
    /// Strips markdown formatting for clients that only support plaintext hover.
    /// </summary>
    private static string StripMarkdown(string md)
    {
        var sb = new StringBuilder(md.Length);
        var lines = md.Split('\n');
        for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
        {
            var line = lines[lineIdx];
            if (lineIdx > 0) sb.Append('\n');

            // Check for horizontal rule: line is all dashes, 3+
            var trimmed = line.TrimEnd('\r');
            if (trimmed.Length >= 3 && IsAllDashes(trimmed))
            {
                sb.Append("────────────────");
                continue;
            }

            int start = 0;
            // Strip heading markers at start of line: #{1,6} followed by whitespace
            if (trimmed.Length > 0 && trimmed[0] == '#')
            {
                int h = 0;
                while (h < trimmed.Length && h < 6 && trimmed[h] == '#') h++;
                if (h < trimmed.Length && char.IsWhiteSpace(trimmed[h]))
                {
                    start = h;
                    while (start < trimmed.Length && char.IsWhiteSpace(trimmed[start])) start++;
                }
            }

            // Process inline formatting
            int i = start;
            while (i < trimmed.Length)
            {
                // Bold: **text**
                if (i + 1 < trimmed.Length && trimmed[i] == '*' && trimmed[i + 1] == '*')
                {
                    int close = trimmed.IndexOf("**", i + 2);
                    if (close > i + 2)
                    {
                        sb.Append(trimmed, i + 2, close - i - 2);
                        i = close + 2;
                        continue;
                    }
                }
                // Underline/italic: __text__
                if (i + 1 < trimmed.Length && trimmed[i] == '_' && trimmed[i + 1] == '_')
                {
                    int close = trimmed.IndexOf("__", i + 2);
                    if (close > i + 2)
                    {
                        sb.Append(trimmed, i + 2, close - i - 2);
                        i = close + 2;
                        continue;
                    }
                }
                // Inline code: `text`
                if (trimmed[i] == '`')
                {
                    int close = trimmed.IndexOf('`', i + 1);
                    if (close > i + 1)
                    {
                        sb.Append(trimmed, i + 1, close - i - 1);
                        i = close + 1;
                        continue;
                    }
                }
                // Links: [text](url)
                if (trimmed[i] == '[')
                {
                    int closeBracket = trimmed.IndexOf(']', i + 1);
                    if (closeBracket > i + 1 && closeBracket + 1 < trimmed.Length && trimmed[closeBracket + 1] == '(')
                    {
                        int closeParen = trimmed.IndexOf(')', closeBracket + 2);
                        if (closeParen > closeBracket + 1)
                        {
                            sb.Append(trimmed, i + 1, closeBracket - i - 1);
                            i = closeParen + 1;
                            continue;
                        }
                    }
                }
                sb.Append(trimmed[i]);
                i++;
            }
        }
        return sb.ToString();

        static bool IsAllDashes(string s)
        {
            foreach (var c in s)
                if (c != '-') return false;
            return true;
        }
    }

    private void HandleDidChangeConfiguration(JsonElement? @params)
    {
        if (@params is null) return;

        // Settings come in params.settings (flat keys or nested)
        if (!@params.Value.TryGetProperty("settings", out var settings)) return;

        MidlTrace.Lsp("Configuration changed, applying new settings");

        if (TryGetBool(settings, "midl.strictMode", out var strict))
        {
#if WITH_COMPAT
            _compilationMode = strict ? CompilationMode.Strict : CompilationMode.Compat;
#endif
            MidlTrace.Lsp($"  strictMode = {strict}");
        }

        if (TryGetBool(settings, "midl.codeQualityHints", out var quality))
        {
            _enableQualityHints = quality;
            MidlTrace.Lsp($"  codeQualityHints = {quality}");
        }

        if (TryGetBool(settings, "midl.useWindowsSdk", out var useSdk))
        {
            _useWindowsSdk = useSdk;
            MidlTrace.Lsp($"  useWindowsSdk = {useSdk}");
        }

        if (TryGetBool(settings, "midl.flatSdkTypeCompletions", out var flat))
        {
            _flatSdkTypeCompletions = flat;
            MidlTrace.Lsp($"  flatSdkTypeCompletions = {flat}");
        }

        if (TryGetBool(settings, "midl.arrays", out var arrays))
        {
            _arraysEnabled = arrays;
            MidlTrace.Lsp($"  arrays = {arrays}");
        }

        if (settings.TryGetProperty("midl.defines", out var definesProp) && definesProp.ValueKind == JsonValueKind.Array)
        {
            _defines.Clear();
            foreach (var d in definesProp.EnumerateArray())
            {
                if (d.ValueKind == JsonValueKind.String && d.GetString() is { } val)
                    _defines.Add(val);
            }
            MidlTrace.Lsp($"  defines = [{string.Join(", ", _defines)}]");
        }

        // Invalidate all compilation caches so next analysis uses new settings
        _lastAnalyzedHash.Clear();
        _lastCompilation.Clear();
        Handlers.SemanticTokensHandler.ClearCache();
        MidlTrace.Lsp("Settings changed: cleared all caches including semantic tokens");

        // Re-analyze all open documents with new settings, then refresh semantic tokens
        _ = Task.Run(async () =>
        {
            foreach (var uri in _docManager.GetOpenDocumentUris())
            {
                var text = _docManager.GetDocumentText(uri);
                if (text != null)
                {
                    try { await AnalyzeAndPublishAsync(uri, text); }
                    catch { }
                }
            }

            // Now that all compilations are updated, ask client to re-fetch semantic tokens
            // workspace/semanticTokens/refresh is a server→client REQUEST (needs an id)
            MidlTrace.Lsp("Sending workspace/semanticTokens/refresh request");
            await _writer.SendRequestAsync("semtok-refresh-" + DateTime.UtcNow.Ticks, "workspace/semanticTokens/refresh", null);
        });
    }

    private static bool TryGetBool(JsonElement settings, string key, out bool value)
    {
        value = false;
        // Try flat key: "midl.strictMode"
        if (settings.TryGetProperty(key, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.True) { value = true; return true; }
            if (prop.ValueKind == JsonValueKind.False) { value = false; return true; }
        }
        // Try short key: "strictMode"
        var shortKey = key.Contains(".") ? key[(key.LastIndexOf('.') + 1)..] : key;
        if (settings.TryGetProperty(shortKey, out var prop2))
        {
            if (prop2.ValueKind == JsonValueKind.True) { value = true; return true; }
            if (prop2.ValueKind == JsonValueKind.False) { value = false; return true; }
        }
        return false;
    }

    private async Task HandleInitializeAsync(object? id, JsonElement? @params)
    {
        // Check client capabilities for markdown support in hover
        if (@params is not null)
        {
            try
            {
                if (@params.Value.TryGetProperty("capabilities", out var caps) &&
                    caps.TryGetProperty("textDocument", out var textDoc) &&
                    textDoc.TryGetProperty("hover", out var hoverCaps) &&
                    hoverCaps.TryGetProperty("contentFormat", out var formats))
                {
                    _clientSupportsMarkdown = false;
                    foreach (var fmt in formats.EnumerateArray())
                    {
                        if (fmt.GetString() == "markdown")
                        {
                            _clientSupportsMarkdown = true;
                            break;
                        }
                    }
                    MidlTrace.Lsp($"Client hover markdown support: {_clientSupportsMarkdown}");
                }
            }
            catch { }
        }

        // Read midl.strictMode from initialization options
        if (@params is not null)
        {
            try
            {
                if (@params.Value.TryGetProperty("initializationOptions", out var initOpts))
                {
                    MidlTrace.Lsp($"InitializationOptions received: {initOpts}");
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

                    if (initOpts.TryGetProperty("midl.arrays", out var arraysProp) &&
                        arraysProp.ValueKind == JsonValueKind.True)
                    {
                        _arraysEnabled = true;
                    }
                    else if (initOpts.TryGetProperty("arrays", out var arraysProp2) &&
                             arraysProp2.ValueKind == JsonValueKind.True)
                    {
                        _arraysEnabled = true;
                    }

                    // Read defines from initialization options
                    if (initOpts.TryGetProperty("midl.defines", out var defProp) && defProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var d in defProp.EnumerateArray())
                            if (d.GetString() is { } val) _defines.Add(val);
                    }
                    else if (initOpts.TryGetProperty("defines", out var defProp2) && defProp2.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var d in defProp2.EnumerateArray())
                            if (d.GetString() is { } val) _defines.Add(val);
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
                    if (_projectFile.Arrays)
                        _arraysEnabled = true;
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
                ["documentOnTypeFormattingProvider"] = new Dictionary<string, object?>
                {
                    ["firstTriggerCharacter"] = "\n",
                    ["moreTriggerCharacter"] = new[] { "}", "{", ";" }
                },
                ["renameProvider"] = new Dictionary<string, object?>
                {
                    ["prepareProvider"] = true
                },
                ["codeActionProvider"] = new Dictionary<string, object?>
                {
                    ["codeActionKinds"] = new[] { "quickfix" }
                },
                ["codeLensProvider"] = new Dictionary<string, object?>
                {
                    ["resolveProvider"] = false
                },
                ["foldingRangeProvider"] = true,
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
