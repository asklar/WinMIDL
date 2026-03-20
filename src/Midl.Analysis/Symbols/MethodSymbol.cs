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
    public bool IsProtected { get; set; }
    public bool IsOverridable { get; set; }
    public bool IsExternal { get; set; }
    public string? SourceWinMdPath { get; set; }

    // Extracted attribute data
    public List<AttributeData> Attributes { get; } = new();
    public string? MethodNameOverride { get; set; }
    public string? OverloadGroup { get; set; }
    public bool IsDefaultOverload { get; set; }
    public string? ReturnNameOverride { get; set; }
    public bool IsRemoteAsync { get; set; }

    public MethodSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }
}
