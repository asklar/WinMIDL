// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Midl.Syntax;

namespace Midl.Lsp;

/// <summary>
/// Converts Midl.Analysis diagnostics to LSP diagnostic notifications and writes them to the output.
/// </summary>
public sealed class DiagnosticsPublisher
{
    private readonly JsonRpcWriter _writer;

    /// <summary>
    /// Tracks URIs that had diagnostics published in the previous cycle,
    /// so we can clear them if they no longer have errors.
    /// </summary>
    private readonly HashSet<string> _previousDiagnosticUris = new(StringComparer.OrdinalIgnoreCase);

    public DiagnosticsPublisher(JsonRpcWriter writer)
    {
        _writer = writer;
    }

    public async Task PublishDiagnosticsAsync(string uri, string sourceText,
        IReadOnlyList<Analysis.Diagnostics.Diagnostic> diagnostics,
        IReadOnlyList<(int Start, int End)>? inactiveRegions = null)
    {
        // Group diagnostics by source file
        var mainFileDiags = new List<Analysis.Diagnostics.Diagnostic>();
        var fileGroups = new Dictionary<string, List<Analysis.Diagnostics.Diagnostic>>(StringComparer.OrdinalIgnoreCase);

        // Determine the main file path from the URI for matching SourceFile values
        string? mainFilePath = null;
        try
        {
            var uriObj = new Uri(uri);
            if (uriObj.IsFile)
            {
                mainFilePath = uriObj.LocalPath;
                // On Windows, Uri.LocalPath for file:///c%3A/... gives "/c:/..." — strip leading /
                if (mainFilePath.Length > 2 && mainFilePath[0] == '/' && mainFilePath[2] == ':')
                    mainFilePath = mainFilePath[1..];
            }
        }
        catch { }

        foreach (var d in diagnostics)
        {
            // If the diagnostic has OriginalPosition, always use it (regardless of SourceFile)
            var adjusted = d;
            if (d.OriginalPosition.HasValue)
                adjusted = d with { Position = d.OriginalPosition.Value };

            bool isMainFile = d.SourceFile == null ||
                (mainFilePath != null && string.Equals(
                    Path.GetFullPath(d.SourceFile), Path.GetFullPath(mainFilePath),
                    StringComparison.OrdinalIgnoreCase));

            if (!isMainFile)
            {
                // Diagnostic from a different file (e.g., #include)
                if (!fileGroups.TryGetValue(d.SourceFile!, out var list))
                {
                    list = new List<Analysis.Diagnostics.Diagnostic>();
                    fileGroups[d.SourceFile!] = list;
                }
                list.Add(adjusted);
            }
            else if (adjusted.Position >= sourceText.Length)
            {
                // Position is past the user's source — this diagnostic leaked from
                // included content without SourceFile metadata. Don't show it in
                // the user's file.
            }
            else
            {
                // Diagnostic from the main file (or same file after SourceMap resolution)
                mainFileDiags.Add(adjusted);
            }
        }

        var currentUris = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { uri };

        // Publish main file diagnostics using original source text positions
        await PublishForFileAsync(uri, sourceText, mainFileDiags, inactiveRegions);

        // Publish diagnostics for included files using their original text and positions
        foreach (var (filePath, fileDiags) in fileGroups)
        {
            var fileUri = PathToUri(filePath);
            currentUris.Add(fileUri);

            string fileText;
            try
            {
                fileText = File.ReadAllText(filePath);
            }
            catch
            {
                // Can't read file — fall back to publishing to main URI
                mainFileDiags.AddRange(fileDiags);
                await PublishForFileAsync(uri, sourceText, mainFileDiags);
                continue;
            }

            await PublishForFileAsync(fileUri, fileText, fileDiags);
        }

        // Clear diagnostics from files that previously had errors but no longer do
        foreach (var prevUri in _previousDiagnosticUris)
        {
            if (!currentUris.Contains(prevUri))
            {
                await PublishForFileAsync(prevUri, "", []);
            }
        }

        _previousDiagnosticUris.Clear();
        foreach (var u in currentUris)
            _previousDiagnosticUris.Add(u);
    }

    private async Task PublishForFileAsync(string uri, string sourceText,
        IReadOnlyList<Analysis.Diagnostics.Diagnostic> diagnostics,
        IReadOnlyList<(int Start, int End)>? inactiveRegions = null)
    {
        var lineStarts = PositionHelper.BuildLineStarts(sourceText);
        var lspDiags = new List<object>();

        foreach (var d in diagnostics)
        {
            var (startLine, startChar) = PositionHelper.OffsetToLineChar(lineStarts, d.Position);
            var (endLine, endChar) = PositionHelper.OffsetToLineChar(lineStarts, d.Position + d.Length);

            if (d.Code == "MIDL3003")
                MidlTrace.Write("PUB", $"MIDL3003 '{d.Message.Substring(0, Math.Min(30, d.Message.Length))}': pos={d.Position} origPos={d.OriginalPosition} → {startLine}:{startChar} (srcLen={sourceText.Length})");

            var diag = new Dictionary<string, object?>
            {
                ["range"] = new Dictionary<string, object?>
                {
                    ["start"] = new Dictionary<string, object?> { ["line"] = startLine, ["character"] = startChar },
                    ["end"] = new Dictionary<string, object?> { ["line"] = endLine, ["character"] = endChar }
                },
                ["severity"] = MapSeverity(d.Severity),
                ["code"] = d.Code,
                ["source"] = "midl",
                ["message"] = d.Message
            };
            if (d.HelpUri != null)
            {
                diag["codeDescription"] = new Dictionary<string, object?> { ["href"] = d.HelpUri };
            }
            lspDiags.Add(diag);
        }

        // Emit "unnecessary" hint diagnostics for inactive preprocessor regions
        // VS Code renders these with reduced opacity (grays out all content including brackets)
        if (inactiveRegions != null)
        {
            foreach (var (start, end) in inactiveRegions)
            {
                // Skip regions from included content (positions past the original source)
                if (start >= sourceText.Length) continue;

                var clampedEnd = Math.Min(end, sourceText.Length);
                var (sLine, sChar) = PositionHelper.OffsetToLineChar(lineStarts, start);
                var (eLine, eChar) = PositionHelper.OffsetToLineChar(lineStarts, clampedEnd);
                lspDiags.Add(new Dictionary<string, object?>
                {
                    ["range"] = new Dictionary<string, object?>
                    {
                        ["start"] = new Dictionary<string, object?> { ["line"] = sLine, ["character"] = sChar },
                        ["end"] = new Dictionary<string, object?> { ["line"] = eLine, ["character"] = eChar }
                    },
                    ["severity"] = 4, // Hint
                    ["tags"] = new[] { 1 }, // 1 = Unnecessary
                    ["source"] = "midl",
                    ["message"] = "Inactive preprocessor block"
                });
            }
        }

        await _writer.SendNotificationAsync("textDocument/publishDiagnostics", new Dictionary<string, object?>
        {
            ["uri"] = uri,
            ["diagnostics"] = lspDiags
        });
    }

    private static string PathToUri(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        // Convert to file:/// URI format
        return "file:///" + fullPath.Replace('\\', '/');
    }

    private static int MapSeverity(Analysis.Diagnostics.DiagnosticSeverity severity) => severity switch
    {
        Analysis.Diagnostics.DiagnosticSeverity.Error => 1,
        Analysis.Diagnostics.DiagnosticSeverity.Warning => 2,
        Analysis.Diagnostics.DiagnosticSeverity.Info => 3,
        _ => 4 // Hint
    };
}
