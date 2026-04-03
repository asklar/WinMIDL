using System.Diagnostics;
using System.Runtime.InteropServices;
using Midl.Lsp.Protocol;

namespace Midl.Lsp;

public sealed partial class MidlLanguageServer
{
    /// <summary>
    /// Handle the midl/diagnosticReport custom request.
    /// Returns a snapshot of server health: event log, per-file stats, memory, version.
    /// </summary>
    private DiagnosticReportResult HandleDiagnosticReport()
    {
        var process = Process.GetCurrentProcess();
        var privateBytesMB = process.PrivateMemorySize64 / (1024 * 1024);
        var workingSetMB = process.WorkingSet64 / (1024 * 1024);
        var gcMemoryMB = GC.GetTotalMemory(false) / (1024 * 1024);

        // Build per-file stats from cached compilations
        var fileStats = new List<FileStatEntry>();
        foreach (var (uri, compilation) in _lastCompilation)
        {
            var relativePath = RedactUri(uri);
            var text = _docManager.GetDocumentText(uri);
            var lines = text?.Split('\n').Length ?? 0;
            var errors = compilation.Diagnostics.Count(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Error);
            var warnings = compilation.Diagnostics.Count(d => d.Severity == Analysis.Diagnostics.DiagnosticSeverity.Warning);

            // Separate user-defined types from lazily-resolved SDK/WinMD types
            var allTypes = compilation.GetAllTypes().ToList();
            var userTypes = allTypes.Where(t => t.SourceWinMdPath is null).ToList();
            var winmdTypes = allTypes.Where(t => t.SourceWinMdPath is not null).ToList();

            // Group WinMD types by source to show where they came from
            var winmdSources = new Dictionary<string, int>();
            foreach (var g in winmdTypes.GroupBy(t => Path.GetFileName(t.SourceWinMdPath!)))
                winmdSources[g.Key] = g.Count();

            var isMidl3 = text != null && LooksLikeMidl3(text);

            fileStats.Add(new FileStatEntry(
                RelativePath: relativePath,
                Lines: lines,
                SizeBytes: text?.Length ?? 0,
                Errors: errors,
                Warnings: warnings,
                UserTypes: userTypes.Count,
                ResolvedTypes: winmdTypes.Count,
                ResolvedFrom: winmdSources,
                IsMidl3: isMidl3,
                Content: text));
        }

        return new DiagnosticReportResult(
            ServerVersion: "0.1.0",
            Platform: RuntimeInformation.OSDescription,
            Architecture: RuntimeInformation.OSArchitecture.ToString(),
            Runtime: RuntimeInformation.FrameworkDescription,
            PrivateBytesMB: privateBytesMB,
            WorkingSetMB: workingSetMB,
            GcHeapMB: gcMemoryMB,
            WorkspaceRoot: _workspaceRoot != null ? RedactPath(_workspaceRoot) : null,
            FilesScanned: _lastCompilation.Count,
            FilesOpen: _docManager.GetOpenDocumentUris().Count(),
            CompilationsCached: _lastCompilation.Count,
            WinmdProvidersCached: _winmdProviderCache.Count,
            HasProjectFile: _projectFile != null,
            CompilationMode: _compilationMode.ToString(),
            Events: _eventLog.ToArray(),
            FileStats: fileStats.ToArray());
    }

    /// <summary>
    /// Simple heuristic: does the IDL source look like MIDL3 (WinRT)?
    /// Checks for runtimeclass, namespace, or Windows.Foundation imports.
    /// </summary>
    internal static bool LooksLikeMidl3(string source)
    {
        // Quick keyword scan — not a full parse, just a heuristic
        return source.Contains("runtimeclass", StringComparison.OrdinalIgnoreCase)
            || source.Contains("namespace ", StringComparison.OrdinalIgnoreCase)
            || source.Contains("Windows.Foundation", StringComparison.Ordinal)
            || source.Contains("import \"", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Redact a file URI to a relative workspace path for privacy.</summary>
    private string RedactUri(string uri)
    {
        if (!uri.StartsWith("file:///")) return uri;
        var path = Uri.UnescapeDataString(uri[8..]);
        return RedactPath(path);
    }

    /// <summary>Redact an absolute path to a workspace-relative path for privacy.</summary>
    private string RedactPath(string path)
    {
        if (_workspaceRoot != null && path.StartsWith(_workspaceRoot, StringComparison.OrdinalIgnoreCase))
        {
            var relative = path[_workspaceRoot.Length..].TrimStart('/', '\\');
            return relative.Length > 0 ? relative : ".";
        }
        // Outside workspace — just show the filename
        return Path.GetFileName(path);
    }
}
