// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Syntax;

namespace Midl.Lsp;

/// <summary>
/// Converts analysis diagnostics to LSP diagnostic notifications and writes them to the output.
/// </summary>
public sealed class DiagnosticsPublisher
{
    private readonly JsonRpcWriter _writer;
    private readonly DocumentManager? _documentManager;
    private readonly SemaphoreSlim _publishGate = new(1, 1);
    private readonly Dictionary<string, Dictionary<string, IReadOnlyList<PublishedDiagnostic>>> _snapshotsByOwner =
        new(LspUri.UriComparer);
    private readonly Dictionary<string, string> _preferredUris = new(LspUri.UriComparer);

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsPublisher"/> class.
    /// </summary>
    /// <param name="writer">Writer used to send LSP notifications.</param>
    public DiagnosticsPublisher(JsonRpcWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsPublisher"/> class with access to synchronized documents.
    /// </summary>
    /// <param name="writer">Writer used to send LSP notifications.</param>
    /// <param name="documentManager">Manager used to resolve current open-document text.</param>
    internal DiagnosticsPublisher(JsonRpcWriter writer, DocumentManager documentManager)
    {
        _writer = writer;
        _documentManager = documentManager;
    }

    /// <summary>
    /// Replaces one analysis owner's diagnostic snapshot and publishes complete diagnostic sets for every affected file.
    /// </summary>
    /// <param name="uri">URI of the document that owns this analysis.</param>
    /// <param name="sourceText">Current source text of the owning document.</param>
    /// <param name="diagnostics">Diagnostics produced by the analysis.</param>
    /// <param name="inactiveRegions">Inactive preprocessor regions, or <see langword="null"/> when none were computed.</param>
    /// <param name="preferInputUri">Whether future publications for the owner should use the exact URI spelling supplied in <paramref name="uri"/>.</param>
    public async Task PublishDiagnosticsAsync(
        string uri,
        string sourceText,
        IReadOnlyList<Analysis.Diagnostics.Diagnostic> diagnostics,
        IReadOnlyList<(int Start, int End)>? inactiveRegions = null,
        bool preferInputUri = false)
    {
        string ownerUri = LspUri.Normalize(uri);
        string? mainFilePath = LspUri.GetFilePath(ownerUri);
        List<Analysis.Diagnostics.Diagnostic> mainFileDiagnostics = [];
        Dictionary<string, List<Analysis.Diagnostics.Diagnostic>> diagnosticsByFile =
            new(LspUri.FilePathComparer);

        foreach (Analysis.Diagnostics.Diagnostic diagnostic in diagnostics)
        {
            Analysis.Diagnostics.Diagnostic adjustedDiagnostic = diagnostic.OriginalPosition.HasValue
                ? diagnostic with { Position = diagnostic.OriginalPosition.Value }
                : diagnostic;

            bool isMainFile = diagnostic.SourceFile is null ||
                (mainFilePath is not null && PathsEqual(diagnostic.SourceFile, mainFilePath));

            if (!isMainFile)
            {
                if (!diagnosticsByFile.TryGetValue(
                    diagnostic.SourceFile!,
                    out List<Analysis.Diagnostics.Diagnostic>? fileDiagnostics))
                {
                    fileDiagnostics = [];
                    diagnosticsByFile[diagnostic.SourceFile!] = fileDiagnostics;
                }

                fileDiagnostics.Add(adjustedDiagnostic);
            }
            else if (adjustedDiagnostic.Position < sourceText.Length)
            {
                mainFileDiagnostics.Add(adjustedDiagnostic);
            }
        }

        Dictionary<string, IReadOnlyList<PublishedDiagnostic>> snapshot =
            new(LspUri.UriComparer);

        foreach ((string filePath, List<Analysis.Diagnostics.Diagnostic> fileDiagnostics) in diagnosticsByFile)
        {
            string targetUri = LspUri.FromPath(filePath);
            if (LspUri.UriComparer.Equals(targetUri, ownerUri))
            {
                mainFileDiagnostics.AddRange(fileDiagnostics);
                continue;
            }

            string? fileText = _documentManager?.GetDocumentText(targetUri);
            if (fileText is null)
            {
                try
                {
                    fileText = File.ReadAllText(filePath);
                }
                catch (IOException)
                {
                    mainFileDiagnostics.AddRange(fileDiagnostics);
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    mainFileDiagnostics.AddRange(fileDiagnostics);
                    continue;
                }
            }

            IReadOnlyList<PublishedDiagnostic> publishedDiagnostics =
                BuildPublishedDiagnostics(fileText, fileDiagnostics);

            if (snapshot.TryGetValue(targetUri, out IReadOnlyList<PublishedDiagnostic>? existingDiagnostics))
                snapshot[targetUri] = [.. existingDiagnostics, .. publishedDiagnostics];
            else
                snapshot[targetUri] = publishedDiagnostics;
        }

        snapshot[ownerUri] = BuildPublishedDiagnostics(sourceText, mainFileDiagnostics, inactiveRegions);

        await ReplaceSnapshotAndPublishAsync(ownerUri, uri, preferInputUri, snapshot);
    }

    private async Task ReplaceSnapshotAndPublishAsync(
        string ownerUri,
        string preferredUri,
        bool preferInputUri,
        Dictionary<string, IReadOnlyList<PublishedDiagnostic>> snapshot)
    {
        await _publishGate.WaitAsync();
        try
        {
            HashSet<string> affectedUris = new(LspUri.UriComparer);
            if (_snapshotsByOwner.TryGetValue(
                ownerUri,
                out Dictionary<string, IReadOnlyList<PublishedDiagnostic>>? previousSnapshot))
            {
                affectedUris.UnionWith(previousSnapshot.Keys);
            }

            affectedUris.UnionWith(snapshot.Keys);
            if (preferInputUri || !_preferredUris.ContainsKey(ownerUri))
                _preferredUris[ownerUri] = preferredUri;

            _snapshotsByOwner[ownerUri] = snapshot;

            foreach (string affectedUri in affectedUris)
                await PublishMergedDiagnosticsAsync(affectedUri);
        }
        finally
        {
            _publishGate.Release();
        }
    }

    private async Task PublishMergedDiagnosticsAsync(string targetUri)
    {
        List<PublishedDiagnostic> mergedDiagnostics = [];
        HashSet<PublishedDiagnostic> uniqueDiagnostics = [];

        foreach (Dictionary<string, IReadOnlyList<PublishedDiagnostic>> snapshot in _snapshotsByOwner.Values)
        {
            if (!snapshot.TryGetValue(targetUri, out IReadOnlyList<PublishedDiagnostic>? ownerDiagnostics))
                continue;

            foreach (PublishedDiagnostic diagnostic in ownerDiagnostics)
            {
                if (uniqueDiagnostics.Add(diagnostic))
                    mergedDiagnostics.Add(diagnostic);
            }
        }

        string publishUri = _preferredUris.TryGetValue(targetUri, out string? preferredUri)
            ? preferredUri
            : targetUri;

        List<object> protocolDiagnostics =
            mergedDiagnostics.Select(diagnostic => (object)ToProtocolDiagnostic(diagnostic)).ToList();

        await _writer.SendNotificationAsync("textDocument/publishDiagnostics", new Dictionary<string, object?>
        {
            ["uri"] = publishUri,
            ["diagnostics"] = protocolDiagnostics,
        });
    }

    private static IReadOnlyList<PublishedDiagnostic> BuildPublishedDiagnostics(
        string sourceText,
        IReadOnlyList<Analysis.Diagnostics.Diagnostic> diagnostics,
        IReadOnlyList<(int Start, int End)>? inactiveRegions = null)
    {
        int[] lineStarts = PositionHelper.BuildLineStarts(sourceText);
        List<PublishedDiagnostic> publishedDiagnostics = [];

        foreach (Analysis.Diagnostics.Diagnostic diagnostic in diagnostics)
        {
            (int startLine, int startCharacter) =
                PositionHelper.OffsetToLineChar(lineStarts, diagnostic.Position);
            (int endLine, int endCharacter) =
                PositionHelper.OffsetToLineChar(lineStarts, diagnostic.Position + diagnostic.Length);

            if (endLine != startLine)
            {
                endLine = startLine;
                endCharacter = Math.Max(
                    startCharacter,
                    EndOfLineChar(sourceText, lineStarts, startLine));
            }

            if (diagnostic.Code == "MIDL3003")
                MidlTrace.Write("PUB", $"MIDL3003 '{diagnostic.Message.Substring(0, Math.Min(30, diagnostic.Message.Length))}': pos={diagnostic.Position} origPos={diagnostic.OriginalPosition} → {startLine}:{startCharacter} (srcLen={sourceText.Length})");

            PublishedDiagnostic publishedDiagnostic = new(
                startLine,
                startCharacter,
                endLine,
                endCharacter,
                MapSeverity(diagnostic.Severity),
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.HelpUri,
                IsUnnecessary: false);
            publishedDiagnostics.Add(publishedDiagnostic);
        }

        if (inactiveRegions is not null)
        {
            foreach ((int start, int end) in inactiveRegions)
            {
                if (start >= sourceText.Length)
                    continue;

                int clampedEnd = Math.Min(end, sourceText.Length);
                (int startLine, int startCharacter) =
                    PositionHelper.OffsetToLineChar(lineStarts, start);
                (int endLine, int endCharacter) =
                    PositionHelper.OffsetToLineChar(lineStarts, clampedEnd);

                PublishedDiagnostic publishedDiagnostic = new(
                    startLine,
                    startCharacter,
                    endLine,
                    endCharacter,
                    Severity: 4,
                    Code: null,
                    Message: "Inactive preprocessor block",
                    HelpUri: null,
                    IsUnnecessary: true);
                publishedDiagnostics.Add(publishedDiagnostic);
            }
        }

        return publishedDiagnostics;
    }

    private static int EndOfLineChar(string sourceText, int[] lineStarts, int line)
    {
        if (line < 0 || line >= lineStarts.Length) return 0;
        int start = lineStarts[line];
        int end = line + 1 < lineStarts.Length ? lineStarts[line + 1] : sourceText.Length;
        while (end > start && (sourceText[end - 1] == '\n' || sourceText[end - 1] == '\r'))
            end--;
        return end - start;
    }

    private static Dictionary<string, object?> ToProtocolDiagnostic(PublishedDiagnostic diagnostic)
    {
        Dictionary<string, object?> start = new()
        {
            ["line"] = diagnostic.StartLine,
            ["character"] = diagnostic.StartCharacter,
        };
        Dictionary<string, object?> end = new()
        {
            ["line"] = diagnostic.EndLine,
            ["character"] = diagnostic.EndCharacter,
        };
        Dictionary<string, object?> range = new()
        {
            ["start"] = start,
            ["end"] = end,
        };
        Dictionary<string, object?> protocolDiagnostic = new()
        {
            ["range"] = range,
            ["severity"] = diagnostic.Severity,
            ["source"] = "midl",
            ["message"] = diagnostic.Message,
        };

        if (diagnostic.Code is not null)
            protocolDiagnostic["code"] = diagnostic.Code;

        if (diagnostic.HelpUri is not null)
        {
            Dictionary<string, object?> codeDescription = new()
            {
                ["href"] = diagnostic.HelpUri,
            };
            protocolDiagnostic["codeDescription"] = codeDescription;
        }

        if (diagnostic.IsUnnecessary)
            protocolDiagnostic["tags"] = new[] { 1 };

        return protocolDiagnostic;
    }

    private static bool PathsEqual(string firstPath, string secondPath)
    {
        string firstFullPath = Path.GetFullPath(firstPath);
        string secondFullPath = Path.GetFullPath(secondPath);

        return LspUri.FilePathComparer.Equals(firstFullPath, secondFullPath);
    }

    private static int MapSeverity(Analysis.Diagnostics.DiagnosticSeverity severity) => severity switch
    {
        Analysis.Diagnostics.DiagnosticSeverity.Error => 1,
        Analysis.Diagnostics.DiagnosticSeverity.Warning => 2,
        Analysis.Diagnostics.DiagnosticSeverity.Info => 3,
        _ => 4,
    };

    private sealed record PublishedDiagnostic(
        int StartLine,
        int StartCharacter,
        int EndLine,
        int EndCharacter,
        int Severity,
        string? Code,
        string Message,
        string? HelpUri,
        bool IsUnnecessary);
}
