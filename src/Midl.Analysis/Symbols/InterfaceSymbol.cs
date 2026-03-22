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
    public int SourcePosition { get; set; } = -1;

    public bool IsExclusive { get; set; }
    public string? ExclusiveToClass { get; set; }
    public bool IsSynthesized { get; set; }

    public List<TypeRef> RequiredInterfaces { get; } = [];

    private readonly List<ISymbol> _members = [];
    public IReadOnlyList<ISymbol> Members => _members;

    // Extracted attribute data
    public List<AttributeData> Attributes { get; } = new();
    public string? ContractName { get; set; }
    public int? ContractVersion { get; set; }
    public int? Version { get; set; }
    public string? DeprecatedMessage { get; set; }
    public bool IsExperimental { get; set; }
#if WINDOWS_INTERNAL
    public string? FeatureName { get; set; }
#endif

    public InterfaceSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }

    public void AddMember(ISymbol member) => _members.Add(member);
}
