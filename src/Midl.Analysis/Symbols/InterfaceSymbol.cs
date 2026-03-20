namespace Midl.Analysis.Symbols;

public class InterfaceSymbol : ISymbol
{
    public string Name { get; }
    public SymbolKind Kind => SymbolKind.Interface;
    public ISymbol? ContainingSymbol { get; }

    public string FullName =>
        ContainingSymbol is { FullName.Length: > 0 }
            ? $"{ContainingSymbol.FullName}.{Name}"
            : Name;

    public Guid? Uuid { get; set; }
    public bool IsExternal { get; set; }
    public string? SourceWinMdPath { get; set; }

    public bool IsExclusive { get; set; }
    public string? ExclusiveToClass { get; set; }
    public bool IsSynthesized { get; set; }

    public List<TypeRef> RequiredInterfaces { get; } = [];

    private readonly List<ISymbol> _members = [];
    public IReadOnlyList<ISymbol> Members => _members;

    public InterfaceSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }

    public void AddMember(ISymbol member) => _members.Add(member);
}
