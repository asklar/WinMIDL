namespace Midl.Analysis.Symbols;

public class FieldSymbol : ISymbol
{
    public string Name { get; }
    public SymbolKind Kind => SymbolKind.Field;
    public ISymbol? ContainingSymbol { get; }

    public string FullName =>
        ContainingSymbol is { FullName.Length: > 0 }
            ? $"{ContainingSymbol.FullName}.{Name}"
            : Name;

    public TypeRef Type { get; set; } = TypeRef.Unknown;
    public bool IsExternal { get; set; }
    public string? SourceWinMdPath { get; set; }

    public FieldSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }
}
