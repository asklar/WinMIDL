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
    private CompilationMode _compilationMode = CompilationMode.Strict;

    // Suppress CS0649 — _referencePaths can be set via SetReferencePaths for testing/config
    public void SetReferencePaths(List<string> paths) => _referencePaths = paths;

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
                await _writer.SendResponseAsync(id, CompletionHandler.Handle(@params, _docManager));
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
                        _compilationMode = CompilationMode.Compat;
                    }
                    else if (initOpts.TryGetProperty("strictMode", out var strictProp2) &&
                             strictProp2.ValueKind == JsonValueKind.False)
                    {
                        _compilationMode = CompilationMode.Compat;
                    }
                }
            }
            catch
            {
                // Ignore errors reading initialization options
            }
        }

        var capabilities = new
        {
            capabilities = new
            {
                textDocumentSync = new
                {
                    openClose = true,
                    change = 1, // Full document sync
                    save = new { includeText = true }
                },
                completionProvider = new
                {
                    triggerCharacters = new[] { ".", " " },
                    resolveProvider = false
                },
                hoverProvider = true,
                documentSymbolProvider = true,
                definitionProvider = true,
                referencesProvider = true,
                signatureHelpProvider = new
                {
                    triggerCharacters = new[] { "(", "," }
                },
                semanticTokensProvider = new
                {
                    full = true,
                    legend = new
                    {
                        tokenTypes = Handlers.SemanticTokensHandler.TokenTypes,
                        tokenModifiers = Handlers.SemanticTokensHandler.TokenModifiers
                    }
                },
                documentFormattingProvider = true,
                codeActionProvider = new
                {
                    codeActionKinds = new[] { "quickfix" }
                }
            },
            serverInfo = new
            {
                name = "midl-language-server",
                version = "0.1.0"
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

        // With full document sync (change=1), the first content change has the full text
        var changes = @params.Value.GetProperty("contentChanges");
        if (changes.GetArrayLength() > 0)
        {
            var text = changes[0].GetProperty("text").GetString()!;
            _docManager.UpdateDocument(uri, text);
            await AnalyzeAndPublishAsync(uri, text);
        }
    }

    private void HandleDidClose(JsonElement? @params)
    {
        if (@params is null) return;

        var td = @params.Value.GetProperty("textDocument");
        var uri = td.GetProperty("uri").GetString()!;
        _docManager.CloseDocument(uri);
    }

    private async Task AnalyzeAndPublishAsync(string uri, string text)
    {
        try
        {
            MidlTrace.Lsp($"Analyzing {uri} ({text.Length} chars)");
            var references = GetReferencePaths(uri);
            var compilation = CompilationHelper.Compile(text, references, _compilationMode);
            MidlTrace.Lsp($"Analysis done: {compilation.Diagnostics.Count} diagnostics");
            await _diagnostics.PublishDiagnosticsAsync(uri, text, compilation.Diagnostics);
            MidlTrace.Lsp($"Diagnostics published");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[MIDL LSP] Analysis error: {ex}");
            // If compilation crashes, report a single error diagnostic
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
        if (_referencePaths is { Count: > 0 })
            return _referencePaths;

        // Try to find .winmd files in the workspace directory
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
