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

    private readonly IReadOnlyList<LazyWinMdProvider>? _lazyProviders;

#if WINDOWS_INTERNAL
    /// <summary>Feature_name declarations encountered during binding, keyed by name.</summary>
    public Dictionary<string, FeatureNameInfo> FeatureNames { get; } = new(StringComparer.OrdinalIgnoreCase);
#endif

    public Compilation(NamespaceSymbol globalNamespace, IReadOnlyList<Diagnostic> diagnostics,
        IReadOnlyList<LazyWinMdProvider>? lazyProviders = null)
    {
        GlobalNamespace = globalNamespace;
        Diagnostics = diagnostics;
        _lazyProviders = lazyProviders;
    }

    /// <summary>Returns a new Compilation with additional diagnostics merged in.</summary>
    public Compilation WithAdditionalDiagnostics(IReadOnlyList<Diagnostic> additional)
    {
        var merged = new List<Diagnostic>(additional.Count + Diagnostics.Count);
        merged.AddRange(additional);
        merged.AddRange(Diagnostics);
        var result = new Compilation(GlobalNamespace, merged, _lazyProviders);
#if WINDOWS_INTERNAL
        foreach (var kvp in FeatureNames) result.FeatureNames[kvp.Key] = kvp.Value;
#endif
        return result;
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
            {
                // Not in namespace tree — try lazy providers
                return TryResolveLazyType(fullyQualifiedName);
            }
            current = child;
        }

        return current.Members.FirstOrDefault(m => m.Name == typeName && m.Kind != SymbolKind.Namespace)
            ?? TryResolveLazyType(fullyQualifiedName);
    }

    private ISymbol? TryResolveLazyType(string fullyQualifiedName)
    {
        if (_lazyProviders == null) return null;

        foreach (var provider in _lazyProviders)
        {
            var symbol = provider.TryResolve(fullyQualifiedName);
            if (symbol != null)
            {
                // Import into namespace tree for future lookups
                ImportLazyTypeIntoTree(symbol);
                // Return from tree
                return GetTypeFromTree(fullyQualifiedName);
            }
        }
        return null;
    }

    private void ImportLazyTypeIntoTree(ISymbol symbol)
    {
        var fullName = symbol.FullName;
        var lastDot = fullName.LastIndexOf('.');
        var nsName = lastDot >= 0 ? fullName[..lastDot] : "";
        var ns = GetOrCreateNamespaceHierarchy(nsName);
        ns.AddMember(CreateImportedSymbol(symbol, ns));
    }

    private NamespaceSymbol GetOrCreateNamespaceHierarchy(string dottedName)
    {
        if (string.IsNullOrEmpty(dottedName)) return GlobalNamespace;
        var parts = dottedName.Split('.');
        var current = GlobalNamespace;
        foreach (var part in parts)
            current = current.GetOrCreateChildNamespace(part);
        return current;
    }

    private ISymbol? GetTypeFromTree(string fullyQualifiedName)
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

    private static ISymbol CreateImportedSymbol(ISymbol source, NamespaceSymbol ns)
    {
        switch (source)
        {
            case InterfaceSymbol iface:
                var newIface = new InterfaceSymbol(iface.Name, ns) { Uuid = iface.Uuid, IsExternal = true, SourceWinMdPath = iface.SourceWinMdPath, SourceFile = iface.SourceFile };
                foreach (var m in iface.Members) newIface.AddMember(m);
                foreach (var r in iface.RequiredInterfaces) newIface.RequiredInterfaces.Add(r);
                return newIface;
            case EnumSymbol e:
                var newEnum = new EnumSymbol(e.Name, ns) { IsFlags = e.IsFlags, UnderlyingType = e.UnderlyingType, IsExternal = true, SourceWinMdPath = e.SourceWinMdPath, SourceFile = e.SourceFile };
                foreach (var m in e.EnumMembers) newEnum.AddMember(m);
                return newEnum;
            case StructSymbol s:
                var newStruct = new StructSymbol(s.Name, ns) { IsExternal = true, SourceWinMdPath = s.SourceWinMdPath, SourceFile = s.SourceFile };
                foreach (var f in s.Fields) newStruct.AddField(f);
                return newStruct;
            case DelegateSymbol d:
                var newDel = new DelegateSymbol(d.Name, ns) { ReturnType = d.ReturnType, IsExternal = true, SourceWinMdPath = d.SourceWinMdPath, SourceFile = d.SourceFile };
                newDel.Parameters.AddRange(d.Parameters);
                return newDel;
            case RuntimeClassSymbol rc:
                var newRc = new RuntimeClassSymbol(rc.Name, ns) { Uuid = rc.Uuid, BaseClass = rc.BaseClass, IsStatic = rc.IsStatic, IsExternal = true, SourceWinMdPath = rc.SourceWinMdPath, SourceFile = rc.SourceFile };
                foreach (var i in rc.Interfaces) newRc.Interfaces.Add(i);
                foreach (var m in rc.Members) newRc.AddMember(m);
                return newRc;
            case ApiContractSymbol ac:
                return new ApiContractSymbol(ac.Name, ns) { Version = ac.Version, IsExternal = true, SourceWinMdPath = ac.SourceWinMdPath, SourceFile = ac.SourceFile };
            default:
                return source;
        }
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
