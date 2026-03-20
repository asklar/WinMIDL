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

    public DelegateSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }
}
