// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Analysis.Diagnostics;

public enum DiagnosticSeverity { Error, Warning, Info }

public record Diagnostic(
    DiagnosticSeverity Severity,
    string Code,
    string Message,
    int Position,
    int Length,
    string? HelpUri = null,
    Dictionary<string, string>? Data = null
)
{
    /// <summary>
    /// The source file this diagnostic originates from (set via SourceMap after compilation).
    /// Null when no #include expansion occurred or when the diagnostic is in the main file.
    /// </summary>
    public string? SourceFile { get; init; }

    /// <summary>
    /// The position in the original source file (before #include expansion).
    /// Only set when SourceFile is set; otherwise use Position.
    /// </summary>
    public int? OriginalPosition { get; init; }
}
