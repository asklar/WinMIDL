// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Analysis.Symbols;

public class NamespaceSymbol : ISymbol
{
    public string Name { get; }
    public SymbolKind Kind => SymbolKind.Namespace;
    public ISymbol? ContainingSymbol { get; }

    public bool IsExternal { get; set; }
    public string? SourceWinMdPath { get; set; }
    public string? SourceFile { get; set; }
    public int SourcePosition { get; set; } = -1;
    public DocComment? DocComment { get; set; }

    private readonly List<ISymbol> _members = [];
    public IReadOnlyList<ISymbol> Members => _members;

    private readonly Dictionary<string, NamespaceSymbol> _childNamespaces = new(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, NamespaceSymbol> ChildNamespaces => _childNamespaces;

    public string FullName =>
        ContainingSymbol is NamespaceSymbol { Name.Length: > 0 } parent
            ? $"{parent.FullName}.{Name}"
            : Name;

    public NamespaceSymbol(string name, ISymbol? containingSymbol = null)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }

    public void AddMember(ISymbol member)
    {
        _members.Add(member);
    }

    public NamespaceSymbol GetOrCreateChildNamespace(string name)
    {
        if (!_childNamespaces.TryGetValue(name, out var child))
        {
            child = new NamespaceSymbol(name, this);
            _childNamespaces[name] = child;
            _members.Add(child);
        }
        return child;
    }
}
