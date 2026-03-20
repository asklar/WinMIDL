namespace Midl.Analysis.Symbols;

public class StructSymbol : ISymbol
{
    public string Name { get; }
    public SymbolKind Kind => SymbolKind.Struct;
    public ISymbol? ContainingSymbol { get; }

    public string FullName =>
        ContainingSymbol is { FullName.Length: > 0 }
            ? $"{ContainingSymbol.FullName}.{Name}"
            : Name;

    public bool IsExternal { get; set; }
    public string? SourceWinMdPath { get; set; }

    private readonly List<FieldSymbol> _fields = [];
    public IReadOnlyList<FieldSymbol> Fields => _fields;

    public StructSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }

    public void AddField(FieldSymbol field) => _fields.Add(field);
}
