// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Midl.Analysis;
using Midl.Syntax;

namespace Midl.Lsp;

public sealed partial class MidlLanguageServer
{
    private async Task HandleDidOpenAsync(JsonElement? @params)
    {
        if (@params is null) return;

        var td = @params.Value.GetProperty("textDocument");
        var uri = td.GetProperty("uri").GetString()!;
        var text = td.GetProperty("text").GetString()!;

        _docManager.OpenDocument(uri, text);

        // Skip analysis for virtual documents (decompiled WinMD views)
        if (uri.StartsWith("midl-winmd:", StringComparison.OrdinalIgnoreCase)) return;

        await AnalyzeAndPublishAsync(uri, text);
    }

    private async Task HandleDidChangeAsync(JsonElement? @params)
    {
        if (@params is null) return;

        var td = @params.Value.GetProperty("textDocument");
        var uri = td.GetProperty("uri").GetString()!;

        // Skip analysis for virtual documents
        if (uri.StartsWith("midl-winmd:", StringComparison.OrdinalIgnoreCase))
        {
            var changes2 = @params.Value.GetProperty("contentChanges");
            if (changes2.GetArrayLength() > 0)
                _docManager.UpdateDocument(uri, changes2[0].GetProperty("text").GetString()!);
            return;
        }

        var changes = @params.Value.GetProperty("contentChanges");
        if (changes.GetArrayLength() > 0)
        {
            var text = changes[0].GetProperty("text").GetString()!;
            _docManager.UpdateDocument(uri, text);
            MidlTrace.Lsp($"didChange: {uri} ({text.Length} chars)");

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

    private async Task AnalyzeAndPublishAsync(string uri, string text)
    {
        // Never analyze virtual documents (decompiled WinMD views)
        if (uri.StartsWith("midl-winmd:", StringComparison.OrdinalIgnoreCase)) return;

        // Skip recompilation if the text hasn't changed since last analysis
        var hash = text.GetHashCode();
        if (_lastAnalyzedHash.TryGetValue(uri, out var lastHash) && lastHash == hash)
        {
            MidlTrace.Lsp($"Skipping analysis for {uri} (unchanged, hash={hash})");
            return;
        }

        MidlTrace.Lsp($"Analysis starting for {uri} ({text.Length} chars, hash={hash}, prevHash={lastHash})");

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
            // Merge in defines from settings (midl.defines)
            if (_defines.Count > 0)
            {
                var merged = new List<string>(defines ?? []);
                merged.AddRange(_defines);
                defines = merged;
            }
            var cachedProviders = GetCachedProviders(references);

            // Use cached providers when available to skip disk I/O
            Compilation compilation;
            if (cachedProviders != null)
            {
                compilation = CompilationHelper.Compile(text, cachedProviders, _compilationMode,
                    includePaths, filePath,
                    defines: defines,
                    emitQualityHints: _enableQualityHints,
                    refDirectories: _refDirectories,
                    arraysEnabled: _arraysEnabled);
            }
            else
            {
                compilation = CompilationHelper.Compile(text, references, _compilationMode,
                    includePaths, filePath,
                    defines: defines,
                    emitQualityHints: _enableQualityHints,
                    refDirectories: _refDirectories,
                    arraysEnabled: _arraysEnabled);
            }

            // Cache compilation for reuse by code action / completion handlers
            _lastCompilation[uri] = compilation;

            var memAfter = GC.GetTotalMemory(false) / (1024 * 1024);
            MidlTrace.Lsp($"Analysis done: {compilation.Diagnostics.Count} diagnostics, mem={memAfter}MB, providers={_winmdProviderCache.Count}");
            await _diagnostics.PublishDiagnosticsAsync(uri, text, compilation.Diagnostics, compilation.InactiveRegions);
            MidlTrace.Lsp($"Diagnostics published");
        }
        catch (Exception ex)
        {
            var errorDiag = new Analysis.Diagnostics.Diagnostic(
                Analysis.Diagnostics.DiagnosticSeverity.Error,
                "MIDL0000",
                $"Internal compiler error: {ex.Message}",
                0, 0);
            await _diagnostics.PublishDiagnosticsAsync(uri, text, [errorDiag]);
        }
    }
}
