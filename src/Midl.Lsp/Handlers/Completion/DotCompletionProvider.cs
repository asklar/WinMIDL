using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides completions for dot-triggered contexts: child namespaces, types, or enum members.
/// </summary>
internal static class DotCompletionProvider
{
    /// <summary>
    /// Extracts the dotted prefix before the cursor, e.g. "Windows.Foundation" from "Windows.Foundation.|".
    /// Returns null if cursor is not immediately after a dot.
    /// </summary>
    internal static string? ExtractDotPrefix(string text, int offset)
    {
        if (offset <= 0 || text[offset - 1] != '.') return null;

        // Walk backward from the character before the dot to collect the prefix
        int end = offset - 1; // position of the dot
        int start = end - 1;
        while (start >= 0 && (char.IsLetterOrDigit(text[start]) || text[start] == '_' || text[start] == '.'))
            start--;
        start++;

        if (start >= end) return null;
        var prefix = text[start..end];
        // Must contain at least one valid identifier character
        if (prefix.Length == 0 || prefix.EndsWith(".")) return null;
        return prefix;
    }

    /// <summary>
    /// Build completions for dot-triggered context: child namespaces, types, or enum members.
    /// </summary>
    internal static List<object> BuildDotCompletions(string prefix, string text,
        IReadOnlyList<string>? includePaths, string? fileDir)
    {
        var items = new List<object>();

        try
        {
            var compilation = CompilationHelper.Compile(text, referencePaths: null,
                mode: CompilationMode.Strict, includePaths: includePaths, currentFilePath: fileDir);

            // First try: resolve prefix as a namespace in the compilation tree
            var ns = ResolveNamespace(compilation.GlobalNamespace, prefix);
            if (ns != null)
            {
                // Add child namespaces
                foreach (var child in ns.ChildNamespaces.Values.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase))
                {
                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = child.Name,
                        ["kind"] = CompletionHandler.KindModule,
                        ["detail"] = child.FullName,
                        ["insertText"] = child.Name,
                    });
                }

                // Add types in this namespace
                foreach (var member in ns.Members.Where(m => m.Kind != SymbolKind.Namespace)
                    .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase))
                {
                    var kind = member.Kind switch
                    {
                        SymbolKind.Interface => CompletionHandler.KindInterface,
                        SymbolKind.RuntimeClass => CompletionHandler.KindClass,
                        SymbolKind.Enum => CompletionHandler.KindEnum,
                        SymbolKind.Struct => CompletionHandler.KindStruct,
                        SymbolKind.Delegate => CompletionHandler.KindClass,
                        _ => CompletionHandler.KindClass,
                    };
                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = member.Name,
                        ["kind"] = kind,
                        ["detail"] = member.FullName,
                        ["insertText"] = member.Name,
                    });
                }

                // Also check lazy providers for types in this namespace
                AddLazyProviderNamespaceMembers(items, compilation, prefix);

                return items;
            }

            // Second try: resolve as a type name (e.g. "MyEnum" → show enum members)
            var typeSymbol = ResolveTypeByShortOrFullName(compilation, prefix);
            if (typeSymbol is EnumSymbol enumSym)
            {
                foreach (var member in enumSym.EnumMembers)
                {
                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = member.Name,
                        ["kind"] = CompletionHandler.KindEnumMember,
                        ["detail"] = $"{enumSym.FullName}.{member.Name}",
                        ["insertText"] = member.Name,
                    });
                }
                return items;
            }

            // Third try: prefix might be a namespace only known to lazy providers
            AddLazyProviderNamespaceMembers(items, compilation, prefix);
        }
        catch
        {
            // Partial file — return empty
        }

        return items;
    }

    private static NamespaceSymbol? ResolveNamespace(NamespaceSymbol root, string dottedName)
    {
        var parts = dottedName.Split('.');
        var current = root;
        foreach (var part in parts)
        {
            if (!current.ChildNamespaces.TryGetValue(part, out var child))
                return null;
            current = child;
        }
        return current;
    }

    private static ISymbol? ResolveTypeByShortOrFullName(Compilation compilation, string name)
    {
        // Try fully qualified first
        var symbol = compilation.GetTypeByName(name);
        if (symbol != null) return symbol;

        // Try short name match in all types
        foreach (var t in compilation.GetAllTypes())
        {
            if (t.Name.Equals(name, StringComparison.Ordinal))
                return t;
        }
        return null;
    }

    private static void AddLazyProviderNamespaceMembers(List<object> items, Compilation compilation, string ns)
    {
        // Use reflection-free approach: the compilation exposes lazy providers indirectly
        // through GetTypesInNamespace-like queries by checking all type names
        var existingLabels = new HashSet<string>(
            items.Select(i => ((Dictionary<string, object?>)i)["label"]?.ToString() ?? ""),
            StringComparer.Ordinal);

        // Walk all types looking for ones in this namespace (lazy providers are queried through compilation)
        // We trigger lazy resolution by looking at the namespace prefix
        try
        {
            // Try resolving a dummy type to trigger namespace population from providers
            compilation.GetTypeByName(ns + ".__probe__");
        }
        catch { }

        // Re-check the namespace tree after probing
        var resolvedNs = ResolveNamespace(compilation.GlobalNamespace, ns);
        if (resolvedNs != null)
        {
            foreach (var child in resolvedNs.ChildNamespaces.Values)
            {
                if (existingLabels.Add(child.Name))
                {
                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = child.Name,
                        ["kind"] = CompletionHandler.KindModule,
                        ["detail"] = child.FullName,
                        ["insertText"] = child.Name,
                    });
                }
            }
            foreach (var member in resolvedNs.Members.Where(m => m.Kind != SymbolKind.Namespace))
            {
                if (existingLabels.Add(member.Name))
                {
                    var kind = member.Kind switch
                    {
                        SymbolKind.Interface => CompletionHandler.KindInterface,
                        SymbolKind.RuntimeClass => CompletionHandler.KindClass,
                        SymbolKind.Enum => CompletionHandler.KindEnum,
                        SymbolKind.Struct => CompletionHandler.KindStruct,
                        SymbolKind.Delegate => CompletionHandler.KindClass,
                        _ => CompletionHandler.KindClass,
                    };
                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = member.Name,
                        ["kind"] = kind,
                        ["detail"] = member.FullName,
                        ["insertText"] = member.Name,
                    });
                }
            }
        }
    }
}
