namespace Midl.Analysis.Symbols;

public class DelegateSymbol : ISymbol
{
    public string Name { get; }
    public SymbolKind Kind => SymbolKind.Delegate;
    public ISymbol? ContainingSymbol { get; }

    public string FullName =>
        ContainingSymbol is { FullName.Length: > 0 }
            ? $"{ContainingSymbol.FullName}.{Name}"
            : Name;

    public TypeRef ReturnType { get; set; } = TypeRef.Void;
    public List<ParameterSymbol> Parameters { get; } = [];
    public bool IsExternal { get; set; }
    public string? SourceWinMdPath { get; set; }
    public string? SourceFile { get; set; }
    public int SourcePosition { get; set; } = -1;

    public Guid? Uuid { get; set; }
    public int? Version { get; set; }
    public string? DeprecatedMessage { get; set; }
    public bool IsExperimental { get; set; }
#if WINDOWS_INTERNAL
    public string? FeatureName { get; set; }
#endif

    // Extracted attribute data
    public List<AttributeData> Attributes { get; } = new();

    public DelegateSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }
}
