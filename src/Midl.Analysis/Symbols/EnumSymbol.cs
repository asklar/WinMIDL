namespace Midl.Analysis.Symbols;

public class EnumSymbol : ISymbol
{
    public string Name { get; }
    public SymbolKind Kind => SymbolKind.Enum;
    public ISymbol? ContainingSymbol { get; }

    public string FullName =>
        ContainingSymbol is { FullName.Length: > 0 }
            ? $"{ContainingSymbol.FullName}.{Name}"
            : Name;

    public bool IsFlags { get; set; }
    public string UnderlyingType { get; set; } = "Int32";
    public bool IsExternal { get; set; }
    public string? SourceWinMdPath { get; set; }

    private readonly List<EnumMemberSymbol> _members = [];
    public IReadOnlyList<EnumMemberSymbol> EnumMembers => _members;

    public EnumSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }

    public void AddMember(EnumMemberSymbol member) => _members.Add(member);
}
