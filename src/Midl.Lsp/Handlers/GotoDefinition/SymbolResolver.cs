using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Resolves symbols by name from a compilation, including qualified name extraction.
/// </summary>
internal static class SymbolResolver
{
    internal static ISymbol? FindSymbol(Compilation compilation, string name, string sourceText, int tokenOffset)
    {
        // First try top-level types (includes resolved imports)
        var type = compilation.GetAllTypes().FirstOrDefault(s => s.Name == name);
        if (type is not null) return type;

        // Search members recursively (methods, properties, etc.)
        var member = FindMemberRecursive(compilation.GlobalNamespace, name);
        if (member is not null) return member;

        // Try to reconstruct fully qualified name from source context
        // e.g., cursor on "TypedEventHandler" in "Windows.Foundation.TypedEventHandler<...>"
        var qualifiedName = ExtractQualifiedName(sourceText, tokenOffset, name);
        if (qualifiedName != null && qualifiedName != name)
        {
            // Try symbol table first (includes built-in WinRT types)
            var defined = compilation.SymbolTable?.LookupDefinedType(qualifiedName);
            if (defined is not null) return defined;

            var resolved = compilation.GetTypeByName(qualifiedName);
            if (resolved is not null) return resolved;
        }

        // Try lazy WinMD providers — search by simple name across all known types
        var lazyNames = compilation.GetAllLazyTypeNames();
        foreach (var fullName in lazyNames)
        {
            var simpleName = fullName.Contains('.') ? fullName[(fullName.LastIndexOf('.') + 1)..] : fullName;
            if (simpleName.Equals(name, StringComparison.Ordinal))
            {
                var resolved = compilation.GetTypeByName(fullName);
                if (resolved is not null) return resolved;
            }
        }

        return null;
    }

    /// <summary>
    /// Given a token at offset, walk backward through dots and identifiers to reconstruct
    /// the fully qualified name (e.g., "Windows.Foundation.TypedEventHandler").
    /// </summary>
    internal static string? ExtractQualifiedName(string text, int tokenOffset, string name)
    {
        // Walk backward from the token to collect dot-separated identifiers
        var parts = new List<string> { name };
        int pos = tokenOffset - 1;

        while (pos >= 0)
        {
            // Skip whitespace
            while (pos >= 0 && char.IsWhiteSpace(text[pos])) pos--;
            if (pos < 0 || text[pos] != '.') break;
            pos--; // skip the dot

            // Collect the identifier before the dot
            int end = pos + 1;
            while (pos >= 0 && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_')) pos--;
            pos++;
            if (pos < end)
                parts.Insert(0, text[pos..end]);
            else
                break;
            pos--;
        }

        return parts.Count > 1 ? string.Join('.', parts) : null;
    }

    /// <summary>
    /// Extracts the full qualified name the cursor is within, walking both backward AND forward.
    /// e.g., cursor on "UI" in "Microsoft.UI.Xaml.Controls.Control" returns the full string.
    /// </summary>
    internal static string? ExtractFullQualifiedNameAtCursor(string text, int tokenOffset, string name)
    {
        var parts = new List<string> { name };

        // Walk backward
        int pos = tokenOffset - 1;
        while (pos >= 0)
        {
            while (pos >= 0 && char.IsWhiteSpace(text[pos])) pos--;
            if (pos < 0 || text[pos] != '.') break;
            pos--;
            int end = pos + 1;
            while (pos >= 0 && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_')) pos--;
            pos++;
            if (pos < end) parts.Insert(0, text[pos..end]); else break;
            pos--;
        }

        // Walk forward
        pos = tokenOffset + name.Length;
        while (pos < text.Length)
        {
            if (text[pos] != '.') break;
            pos++; // skip dot
            int start = pos;
            while (pos < text.Length && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_')) pos++;
            if (pos > start) parts.Add(text[start..pos]); else break;
        }

        return parts.Count > 1 ? string.Join('.', parts) : null;
    }

    internal static ISymbol? FindMemberRecursive(NamespaceSymbol ns, string name)
    {
        foreach (var member in ns.Members)
        {
            if (member.Name == name) return member;

            var children = GetMembers(member);
            if (children is not null)
            {
                foreach (var child in children)
                {
                    if (child.Name == name) return child;
                }
            }

            if (member is NamespaceSymbol childNs)
            {
                var found = FindMemberRecursive(childNs, name);
                if (found is not null) return found;
            }
        }
        return null;
    }

    internal static IReadOnlyList<ISymbol>? GetMembers(ISymbol symbol) => symbol switch
    {
        RuntimeClassSymbol rc => rc.Members,
        InterfaceSymbol iface => iface.Members,
        _ => null
    };
}
