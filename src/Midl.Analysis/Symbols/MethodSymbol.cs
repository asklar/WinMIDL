namespace Midl.Analysis.Symbols;

public class MethodSymbol : ISymbol
{
    public string Name { get; }
    public SymbolKind Kind => SymbolKind.Method;
    public ISymbol? ContainingSymbol { get; }

    public string FullName =>
        ContainingSymbol is { FullName.Length: > 0 }
            ? $"{ContainingSymbol.FullName}.{Name}"
            : Name;

    public TypeRef ReturnType { get; set; } = TypeRef.Void;
    public List<ParameterSymbol> Parameters { get; } = [];
    public bool IsStatic { get; set; }
    public bool IsExternal { get; set; }
    public string? SourceWinMdPath { get; set; }

    public MethodSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }
}
