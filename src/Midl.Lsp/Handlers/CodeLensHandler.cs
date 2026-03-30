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

        return HandleCore(text, compilation, dir);
    }

    /// <summary>Testable entry point without LSP JSON plumbing.</summary>
    public static List<Dictionary<string, object?>> HandleCore(string text, Compilation? compilation, string? dir = null)
    {
        var lenses = new List<Dictionary<string, object?>>();
        var lines = text.Split('\n');

        // @ref lenses
        for (int i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimStart();
            if (!trimmed.StartsWith("// @ref:", StringComparison.Ordinal)) continue;
            var refValue = trimmed["// @ref:".Length..].Trim();
            if (string.IsNullOrEmpty(refValue)) continue;
            string title = compilation?.LazyProviders != null
                ? (CountTypesForRef(compilation, refValue, dir) is int c and > 0 ? $"✓ {c:N0} types" : "⚠ no types found")
                : "⏳ Loading...";
            lenses.Add(MakeLens(i, lines[i].Length, title));
        }

        if (compilation != null) AddNamespaceLenses(text, lines, compilation, lenses);
        return lenses;
    }

    private static Dictionary<string, object?> MakeLens(int line, int lineLen, string title) => new()
    {
        ["range"] = new Dictionary<string, object?>
        {
            ["start"] = new Dictionary<string, object?> { ["line"] = line, ["character"] = 0 },
            ["end"] = new Dictionary<string, object?> { ["line"] = line, ["character"] = lineLen }
        },
        ["command"] = new Dictionary<string, object?> { ["title"] = title, ["command"] = "" }
    };

    private static void AddNamespaceLenses(string text, string[] lines, Compilation compilation, List<Dictionary<string, object?>> lenses)
    {
        // Find namespace blocks and count their types
        for (int i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimStart();
            if (!trimmed.StartsWith("namespace ", StringComparison.Ordinal) &&
                !trimmed.StartsWith("namespace\t", StringComparison.Ordinal))
                continue;

            // Extract namespace name: "namespace Foo.Bar {" or "namespace Foo.Bar"
            var rest = trimmed["namespace".Length..].TrimStart();
            var nameEnd = rest.IndexOfAny([' ', '\t', '{', '\r', '\n']);
            var nsName = nameEnd >= 0 ? rest[..nameEnd].Trim() : rest.Trim();
            if (string.IsNullOrEmpty(nsName)) continue;

            // Count types in this namespace from the compilation
            var ns = FindNamespace(compilation.GlobalNamespace, nsName);
            if (ns == null) continue;

            int typeCount = ns.Members.Count;
            if (typeCount == 0) continue;

            var kindBreakdown = new Dictionary<string, int>();
            foreach (var member in ns.Members)
            {
                var kind = member switch
                {
                    Analysis.Symbols.RuntimeClassSymbol => "class",
                    Analysis.Symbols.InterfaceSymbol iface when !iface.IsSynthesized => "interface",
                    Analysis.Symbols.EnumSymbol => "enum",
                    Analysis.Symbols.StructSymbol => "struct",
                    Analysis.Symbols.DelegateSymbol => "delegate",
                    Analysis.Symbols.ApiContractSymbol => "contract",
                    Analysis.Symbols.AttributeTypeSymbol => "attribute",
                    _ => null
                };
                if (kind != null)
                {
                    kindBreakdown.TryGetValue(kind, out var count);
                    kindBreakdown[kind] = count + 1;
                }
            }

            if (kindBreakdown.Count == 0) continue;

            var parts = kindBreakdown.Select(kv => $"{kv.Value} {kv.Key}{(kv.Value > 1 ? (kv.Key.EndsWith("s") ? "es" : "s") : "")}");
            var title = string.Join(", ", parts);

            lenses.Add(MakeLens(i, lines[i].Length, title));
        }
    }

    private static Analysis.Symbols.NamespaceSymbol? FindNamespace(Analysis.Symbols.NamespaceSymbol root, string fullName)
    {
        var parts = fullName.Split('.');
        var current = root;
        foreach (var part in parts)
        {
            if (!current.ChildNamespaces.TryGetValue(part, out var child))
                return null;
            current = child;
        }
        return current;
    }

    private static int CountTypesForRef(Compilation compilation, string refValue, string? dir)
    {
        if (string.IsNullOrEmpty(refValue)) return 0;

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
