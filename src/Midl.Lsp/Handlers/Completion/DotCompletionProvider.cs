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
    /// Also handles mid-typing after a dot: "Windows.Fo|" → returns "Windows".
    /// Returns null if cursor is not in a dotted context.
    /// </summary>
    internal static string? ExtractDotPrefix(string text, int offset)
    {
        // Walk backward from cursor to find the start of the current token
        int tokenEnd = offset;
        int pos = offset - 1;
        while (pos >= 0 && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_'))
            pos--;

        // Check if we hit a dot
        if (pos < 0 || text[pos] != '.') return null;

        // Walk backward from before the dot to collect the prefix (e.g. "Windows" or "Windows.Foundation")
        int dotPos = pos;
        int start = dotPos - 1;
        while (start >= 0 && (char.IsLetterOrDigit(text[start]) || text[start] == '_' || text[start] == '.'))
            start--;
        start++;

        if (start >= dotPos) return null;
        var prefix = text[start..dotPos];
        if (prefix.Length == 0 || prefix.EndsWith(".")) return null;
        return prefix;
    }

    /// <summary>
    /// Build completions for dot-triggered context: child namespaces, types, or enum members.
    /// </summary>
    internal static List<object> BuildDotCompletions(string prefix, string text,
        IReadOnlyList<string>? includePaths, string? fileDir,
        Compilation? cachedCompilation = null)
    {
        var items = new List<object>();

        try
        {
            var compilation = cachedCompilation;
            if (compilation == null)
            {
                try { compilation = CompilationHelper.Compile(text, referencePaths: null,
                    mode: CompilationMode.Strict, includePaths: includePaths, currentFilePath: fileDir); }
                catch { }
            }
            if (compilation == null) return items;

            // First try: resolve prefix as a namespace in the compilation tree
            var ns = ResolveNamespace(compilation.GlobalNamespace, prefix);
            if (ns != null)
            {
                // Add child namespaces
                foreach (var child in ns.ChildNamespaces.Values.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase))
                {
                    AddDotItem(items, child.Name, CompletionHandler.KindModule, child.FullName, prefix);
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
                    AddDotItem(items, member.Name, kind, member.FullName, prefix);
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
            var child = current.ChildNamespaces
                .FirstOrDefault(kvp => kvp.Key.Equals(part, StringComparison.OrdinalIgnoreCase)).Value;
            if (child == null) return null;
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
            if (t.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return t;
        }
        return null;
    }

    /// <summary>
    /// Add a completion item with filterText set to "prefix.name" so VS Code
    /// can match it against the full dotted text the user is typing.
    /// </summary>
    private static void AddDotItem(List<object> items, string name, int kind, string detail, string prefix)
    {
        var fullInsert = prefix + "." + name;
        items.Add(new Dictionary<string, object?>
        {
            ["label"] = name,
            ["kind"] = kind,
            ["detail"] = detail,
            ["insertText"] = fullInsert,
            ["filterText"] = fullInsert,
        });
    }

    private static void AddLazyProviderNamespaceMembers(List<object> items, Compilation compilation, string ns)
    {
        var existingLabels = new HashSet<string>(
            items.Select(i => ((Dictionary<string, object?>)i)["label"]?.ToString() ?? ""),
            StringComparer.Ordinal);

        var prefix = ns + ".";
        var childNamespaces = new HashSet<string>(StringComparer.Ordinal);

        foreach (var fullName in compilation.GetAllLazyTypeNames())
        {
            if (!fullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

            var remainder = fullName[prefix.Length..];
            var dotIdx = remainder.IndexOf('.');
            if (dotIdx >= 0)
            {
                // Child namespace
                var childNs = remainder[..dotIdx];
                if (childNamespaces.Add(childNs) && existingLabels.Add(childNs))
                {
                    AddDotItem(items, childNs, CompletionHandler.KindModule, prefix + childNs, ns);
                }
            }
            else
            {
                // Direct type in this namespace
                var typeName = remainder;
                var backtick = typeName.IndexOf('`');
                if (backtick >= 0) typeName = typeName[..backtick];
                if (existingLabels.Add(typeName))
                {
                    AddDotItem(items, typeName, CompletionHandler.KindClass, fullName, ns);
                }
            }
        }
    }
}
