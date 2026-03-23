using System.Text.Json;
using Midl.Analysis;
using Midl.Lsp.Protocol;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides Code Lens annotations for @ref directive lines,
/// showing the number of types loaded from each reference.
/// </summary>
public static class CodeLensHandler
{
    public static object? Handle(JsonElement? @params, DocumentManager docManager, Compilation? compilation)
    {
        if (@params is null) return null;

        var uri = @params.Value.GetProperty("textDocument").GetProperty("uri").GetString()!;
        var text = docManager.GetDocumentText(uri);
        if (text is null) return Array.Empty<object>();

        string? filePath = null;
        if (uri.StartsWith("file:///"))
            filePath = Uri.UnescapeDataString(uri[8..]);
        var dir = filePath != null ? Path.GetDirectoryName(filePath) : null;

        var lenses = new List<object>();
        var lines = text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimStart();
            if (!trimmed.StartsWith("// @ref:", StringComparison.Ordinal)) continue;

            var refValue = trimmed["// @ref:".Length..].Trim();
            string title;

            if (compilation?.LazyProviders != null)
            {
                int typeCount = CountTypesForRef(compilation, refValue, dir);
                title = typeCount > 0
                    ? $"✓ {typeCount:N0} types"
                    : "⚠ no types found";
            }
            else
            {
                title = "⏳ Loading...";
            }

            lenses.Add(new Dictionary<string, object?>
            {
                ["range"] = new Dictionary<string, object?>
                {
                    ["start"] = new Dictionary<string, object?> { ["line"] = i, ["character"] = 0 },
                    ["end"] = new Dictionary<string, object?> { ["line"] = i, ["character"] = lines[i].Length }
                },
                ["command"] = new Dictionary<string, object?>
                {
                    ["title"] = title,
                    ["command"] = ""
                }
            });
        }

        return lenses;
    }

    private static int CountTypesForRef(Compilation compilation, string refValue, string? dir)
    {
        // Normalize forward slashes to backslashes (Windows)
        var normalizedRefValue = refValue.Replace('/', '\\');

        // Resolve the ref path
        string resolvedPath = normalizedRefValue;
        if (dir != null && !Path.IsPathRooted(normalizedRefValue))
            resolvedPath = Path.GetFullPath(Path.Combine(dir, normalizedRefValue));
        else if (Path.IsPathRooted(normalizedRefValue))
            resolvedPath = Path.GetFullPath(normalizedRefValue);

        int total = 0;
        var normalizedRef = resolvedPath.TrimEnd('\\') + '\\';

        foreach (var provider in compilation.LazyProviders!)
        {
            if (provider.SourcePath == null) continue;

            var normalizedSource = Path.GetFullPath(provider.SourcePath);

            if (Directory.Exists(resolvedPath))
            {
                if (normalizedSource.StartsWith(normalizedRef, StringComparison.OrdinalIgnoreCase))
                {
                    total += provider.TypeCount;
                }
            }
            else if (normalizedSource.Equals(Path.GetFullPath(resolvedPath), StringComparison.OrdinalIgnoreCase))
            {
                total += provider.TypeCount;
            }
        }

        return total;
    }
}
