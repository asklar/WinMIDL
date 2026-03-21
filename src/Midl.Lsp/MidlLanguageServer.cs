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
                // Client acknowledgment — nothing to do
                break;

            case "shutdown":
                _shutdownRequested = true;
                await _writer.SendResponseAsync(id, new object());
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
                await _writer.SendResponseAsync(id, GotoDefinitionHandler.Handle(@params, _docManager));
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

            case "textDocument/codeAction":
                await _writer.SendResponseAsync(id, CodeActionHandler.Handle(@params, _docManager));
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
                    ["triggerCharacters"] = new[] { ".", " ", "[", "\"" },
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
                ["codeActionProvider"] = new Dictionary<string, object?>
                {
                    ["codeActionKinds"] = new[] { "quickfix" }
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
        if (_debounceTokens.TryRemove(uri, out var cts))
            cts.Cancel();
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
            var preprocessor = new Midl.Lexer.Preprocessor();
            var preprocessed = preprocessor.Process(text);

            // Resolve the file path for cross-file context
            string? filePath = null;
            if (uri.StartsWith("file:///"))
                filePath = Uri.UnescapeDataString(uri[8..]);

            var references = GetReferencePaths(uri);
            var includePaths = GetIncludePaths(filePath);
            var defines = _projectFile?.Defines;

            // Compile with cross-file context: sibling IDL files are compiled as imports
            var compilation = CompilationHelper.Compile(preprocessed, references, _compilationMode,
                includePaths, filePath,
                defines: defines,
                emitQualityHints: _enableQualityHints);

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

    private List<string>? GetReferencePaths(string uri)
    {
        var paths = new List<string>();

        // Explicit references from configuration
        if (_referencePaths is { Count: > 0 })
            paths.AddRange(_referencePaths);

        // References from winmidl.json project file
        if (_projectFile != null)
            paths.AddRange(_projectFile.GetAbsoluteReferencePaths().Where(File.Exists));

        if (paths.Count > 0)
            return paths;

        // Fallback: scan workspace directory for .winmd files
        try
        {
            string? dir = null;
            if (uri.StartsWith("file:///"))
            {
                var filePath = Uri.UnescapeDataString(uri[8..]);
                dir = Path.GetDirectoryName(filePath);
            }

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
