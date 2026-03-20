namespace Midl.Analysis.Symbols;

public class RuntimeClassSymbol : ISymbol
{
    public string Name { get; }
    public SymbolKind Kind => SymbolKind.RuntimeClass;
    public ISymbol? ContainingSymbol { get; }

    public string FullName =>
        ContainingSymbol is { FullName.Length: > 0 }
            ? $"{ContainingSymbol.FullName}.{Name}"
            : Name;

    public Guid? Uuid { get; set; }
    public bool IsExternal { get; set; }
    public string? SourceWinMdPath { get; set; }

    public TypeRef? BaseClass { get; set; }
    public List<TypeRef> Interfaces { get; } = [];

    private readonly List<ISymbol> _members = [];
    public IReadOnlyList<ISymbol> Members => _members;

    public bool IsStatic { get; set; }
    public bool IsPartial { get; set; }
    public bool IsUnsealed { get; set; }

    public InterfaceSymbol? DefaultInterface { get; set; }
    public InterfaceSymbol? FactoryInterface { get; set; }
    public InterfaceSymbol? StaticsInterface { get; set; }
    public InterfaceSymbol? ProtectedInterface { get; set; }
    public InterfaceSymbol? OverridesInterface { get; set; }
    public bool HasDefaultActivation { get; set; }
    public List<InterfaceSymbol> ImplementedInterfaces { get; } = new();

    // Extracted attribute data
    public List<AttributeData> Attributes { get; } = new();
    public string? ContractName { get; set; }
    public int? ContractVersion { get; set; }
    public int? Version { get; set; }
    public string? ThreadingModel { get; set; }
    public string? MarshalingBehavior { get; set; }
    public string? DeprecatedMessage { get; set; }
    public bool IsDefaultInterface { get; set; }

    public RuntimeClassSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }

    public void AddMember(ISymbol member) => _members.Add(member);
}
