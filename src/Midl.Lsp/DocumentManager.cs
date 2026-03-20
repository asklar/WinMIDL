using System.Collections.Concurrent;

namespace Midl.Lsp;

/// <summary>
/// Tracks open documents by URI, storing their latest text content.
/// </summary>
public sealed class DocumentManager
{
    private readonly ConcurrentDictionary<string, string> _documents = new(StringComparer.Ordinal);

    public void OpenDocument(string uri, string text) => _documents[uri] = text;

    public void UpdateDocument(string uri, string text) => _documents[uri] = text;

    public void CloseDocument(string uri) => _documents.TryRemove(uri, out _);

    public string? GetDocumentText(string uri) =>
        _documents.TryGetValue(uri, out var text) ? text : null;

    public IEnumerable<string> GetOpenDocumentUris() => _documents.Keys;
}
