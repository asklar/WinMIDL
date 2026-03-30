namespace Midl.Analysis.Symbols;

public class EnumMemberSymbol : ISymbol
{
    public string Name { get; }
    public SymbolKind Kind => SymbolKind.EnumMember;
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

    // Extracted attribute data
    public List<AttributeData> Attributes { get; } = new();
    public string? ContractName { get; set; }
    public int? ContractVersion { get; set; }

    /// <summary>Explicit value if assigned (e.g. A = 42). Null if auto-assigned.</summary>
    public long? ExplicitValue { get; set; }
    /// <summary>Raw text of the explicit value (for diagnostics).</summary>
    public string? ExplicitValueText { get; set; }

    public EnumMemberSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }
}
