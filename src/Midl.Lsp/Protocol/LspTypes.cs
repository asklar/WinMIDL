namespace Midl.Lsp.Protocol;

// Common position/range types
public record LspPosition(int Line, int Character);
public record LspRange(LspPosition Start, LspPosition End);
public record LspLocation(string Uri, LspRange Range);

// Completion
public record CompletionList(bool IsIncomplete, CompletionItem[] Items);
public record CompletionItem(
    string Label, int Kind, string? Detail = null,
    string? InsertText = null, int? InsertTextFormat = null,
    string? FilterText = null, string? SortText = null);

// Diagnostics
public record LspDiagnostic(LspRange Range, int Severity, string? Code, string Message, string Source = "midl");

// Hover
public record HoverResult(MarkupContent Contents, LspRange? Range = null);
public record MarkupContent(string Kind, string Value);

// DocumentSymbol
public record DocumentSymbolResult(
    string Name, int Kind, LspRange Range, LspRange SelectionRange,
    DocumentSymbolResult[]? Children = null);

// TextEdit (for code actions, rename)
public record TextEdit(LspRange Range, string NewText);
public record WorkspaceEdit(Dictionary<string, TextEdit[]> Changes);

// Code action
public record CodeAction(string Title, string Kind, WorkspaceEdit? Edit = null);
