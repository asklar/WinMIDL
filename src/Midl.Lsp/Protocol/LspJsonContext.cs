using System.Text.Json.Serialization;

namespace Midl.Lsp.Protocol;

[JsonSerializable(typeof(CompletionList))]
[JsonSerializable(typeof(CompletionItem[]))]
[JsonSerializable(typeof(LspDiagnostic[]))]
[JsonSerializable(typeof(HoverResult))]
[JsonSerializable(typeof(DocumentSymbolResult[]))]
[JsonSerializable(typeof(CodeAction[]))]
[JsonSerializable(typeof(WorkspaceEdit))]
[JsonSerializable(typeof(LspLocation))]
[JsonSerializable(typeof(LspLocation[]))]
[JsonSerializable(typeof(TextEdit))]
[JsonSerializable(typeof(TextEdit[]))]
[JsonSerializable(typeof(LspRange))]
[JsonSerializable(typeof(LspPosition))]
[JsonSerializable(typeof(MarkupContent))]
[JsonSerializable(typeof(CompletionItem))]
[JsonSerializable(typeof(LspDiagnostic))]
[JsonSerializable(typeof(DocumentSymbolResult))]
[JsonSerializable(typeof(CodeAction))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class LspJsonContext : JsonSerializerContext { }
