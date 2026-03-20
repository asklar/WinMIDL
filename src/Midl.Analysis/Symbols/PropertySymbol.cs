namespace Midl.Analysis.Symbols;

public class PropertySymbol : ISymbol
{
    public string Name { get; }
    public SymbolKind Kind => SymbolKind.Property;
    public ISymbol? ContainingSymbol { get; }

    public string FullName =>
        ContainingSymbol is { FullName.Length: > 0 }
            ? $"{ContainingSymbol.FullName}.{Name}"
            : Name;

    public TypeRef Type { get; set; } = TypeRef.Unknown;
    public bool HasGetter { get; set; }
    public bool HasSetter { get; set; }
    public bool IsStatic { get; set; }
    public bool IsProtected { get; set; }
    public bool IsOverridable { get; set; }
    public bool IsExternal { get; set; }
    public string? SourceWinMdPath { get; set; }

    // Extracted attribute data
    public List<AttributeData> Attributes { get; } = new();

    public PropertySymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }
}
