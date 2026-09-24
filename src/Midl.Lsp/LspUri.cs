namespace Midl.Lsp;

/// <summary>
/// Produces canonical document URIs so equivalent file URI spellings share one server identity.
/// </summary>
internal static class LspUri
{
    internal static IEqualityComparer<string> UriComparer { get; } = new DocumentUriComparer();

    internal static StringComparer FilePathComparer { get; } = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    private sealed class DocumentUriComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y)
        {
            if (x is null || y is null)
                return x is null && y is null;

            return (x.StartsWith("file:///", StringComparison.OrdinalIgnoreCase)
                ? FilePathComparer
                : StringComparer.Ordinal).Equals(x, y);
        }

        public int GetHashCode(string uri) =>
            (uri.StartsWith("file:///", StringComparison.OrdinalIgnoreCase)
                ? FilePathComparer
                : StringComparer.Ordinal).GetHashCode(uri);
    }

    /// <summary>
    /// Normalizes a document URI to its canonical form.
    /// </summary>
    /// <param name="uri">The URI to normalize, as received from a client notification or built internally.</param>
    /// <returns>The canonical file URI, or <paramref name="uri"/> unchanged when it does not identify a local file.</returns>
    public static string Normalize(string uri)
    {
        string? filePath = GetFilePath(uri);

        return filePath is not null ? FromPath(filePath) : uri;
    }

    /// <summary>
    /// Builds the canonical <c>file://</c> URI for a local file-system path.
    /// </summary>
    /// <param name="filePath">A local file-system path, absolute or relative.</param>
    /// <returns>The canonical file URI with an upper-cased Windows drive letter.</returns>
    public static string FromPath(string filePath)
    {
        string fullPath = Path.GetFullPath(filePath);

        if (fullPath.Length >= 2 && fullPath[1] == ':')
            fullPath = char.ToUpperInvariant(fullPath[0]) + fullPath[1..];

        return new Uri(fullPath).AbsoluteUri;
    }

    /// <summary>
    /// Gets the local file-system path identified by a document URI.
    /// </summary>
    /// <param name="uri">Document URI to decode.</param>
    /// <returns>The local path, or <see langword="null"/> when <paramref name="uri"/> is not a file URI.</returns>
    public static string? GetFilePath(string uri)
    {
        const string prefix = "file:///";
        if (!uri.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return null;

        string filePath = Uri.UnescapeDataString(uri[(prefix.Length - 1)..]);
        if (filePath.Length >= 3 && filePath[0] == '/' && filePath[2] == ':')
            filePath = filePath[1..];

        return Path.GetFullPath(filePath);
    }
}
