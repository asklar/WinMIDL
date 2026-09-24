// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;

namespace Midl.Lsp;

/// <summary>
/// Tracks open documents by URI, storing their latest text content.
/// </summary>
public sealed class DocumentManager
{
    private readonly ConcurrentDictionary<string, string> _documents = new(LspUri.UriComparer);

    public void OpenDocument(string uri, string text) => _documents[LspUri.Normalize(uri)] = text;

    public void UpdateDocument(string uri, string text) => _documents[LspUri.Normalize(uri)] = text;

    public void CloseDocument(string uri) => _documents.TryRemove(LspUri.Normalize(uri), out _);

    public string? GetDocumentText(string uri) =>
        _documents.TryGetValue(LspUri.Normalize(uri), out string? text) ? text : null;

    public IEnumerable<string> GetOpenDocumentUris() => _documents.Keys;
}
