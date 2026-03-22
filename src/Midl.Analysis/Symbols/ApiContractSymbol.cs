namespace Midl.Analysis.Symbols;

public class ApiContractSymbol : ISymbol
{
    public string Name { get; }
    public SymbolKind Kind => SymbolKind.ApiContract;
    public ISymbol? ContainingSymbol { get; }

    public string FullName =>
        ContainingSymbol is { FullName.Length: > 0 }
            ? $"{ContainingSymbol.FullName}.{Name}"
            : Name;

    public int Version { get; set; }
    public bool IsExternal { get; set; }
    public string? SourceWinMdPath { get; set; }
    public string? SourceFile { get; set; }
    public int SourcePosition { get; set; } = -1;

    // Extracted attribute data
    public List<AttributeData> Attributes { get; } = new();
    public int? ContractVersion { get; set; }

    public ApiContractSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }
}
