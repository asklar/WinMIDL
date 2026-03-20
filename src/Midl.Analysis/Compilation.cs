using Midl.Analysis.Diagnostics;
using Midl.Analysis.Symbols;

namespace Midl.Analysis;

/// <summary>
/// Compilation pipeline result — semantic analysis, symbol resolution, type checking.
/// </summary>
public class Compilation
{
    public NamespaceSymbol GlobalNamespace { get; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; }

    public Compilation(NamespaceSymbol globalNamespace, IReadOnlyList<Diagnostic> diagnostics)
    {
        GlobalNamespace = globalNamespace;
        Diagnostics = diagnostics;
    }

    /// <summary>Returns a new Compilation with additional diagnostics merged in.</summary>
    public Compilation WithAdditionalDiagnostics(IReadOnlyList<Diagnostic> additional)
    {
        var merged = new List<Diagnostic>(additional.Count + Diagnostics.Count);
        merged.AddRange(additional);
        merged.AddRange(Diagnostics);
        return new Compilation(GlobalNamespace, merged);
    }

    /// <summary>Lookup any type by fully qualified name.</summary>
    public ISymbol? GetTypeByName(string fullyQualifiedName)
    {
        var parts = fullyQualifiedName.Split('.');
        var typeName = parts[^1];
        var nsParts = parts[..^1];

        var current = GlobalNamespace;
        foreach (var part in nsParts)
        {
            if (!current.ChildNamespaces.TryGetValue(part, out var child))
                return null;
            current = child;
        }

        return current.Members.FirstOrDefault(m => m.Name == typeName && m.Kind != SymbolKind.Namespace);
    }

    /// <summary>Get all types (non-namespace symbols) in the compilation.</summary>
    public IEnumerable<ISymbol> GetAllTypes()
    {
        return GetAllTypesRecursive(GlobalNamespace);
    }

    private static IEnumerable<ISymbol> GetAllTypesRecursive(NamespaceSymbol ns)
    {
        foreach (var m in ns.Members)
        {
            if (m.Kind != SymbolKind.Namespace)
                yield return m;
        }
        foreach (var child in ns.ChildNamespaces.Values)
        {
            foreach (var t in GetAllTypesRecursive(child))
                yield return t;
        }
    }
}
