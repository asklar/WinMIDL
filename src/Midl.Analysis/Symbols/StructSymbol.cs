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
    public string? SourceFile { get; set; }
    public int SourcePosition { get; set; } = -1;
    public DocComment? DocComment { get; set; }

    private readonly List<FieldSymbol> _fields = [];
    public IReadOnlyList<FieldSymbol> Fields => _fields;

    // Extracted attribute data
    public List<AttributeData> Attributes { get; } = new();
    public bool IsExperimental { get; set; }
#if FEATURE_STAGING
    public string? FeatureName { get; set; }
#endif

    public StructSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }

    public void AddField(FieldSymbol field) => _fields.Add(field);
}
