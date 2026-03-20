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
);
