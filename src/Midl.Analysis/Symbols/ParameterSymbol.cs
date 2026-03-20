namespace Midl.Analysis.Symbols;

public enum ParameterDirection { In, Out, Ref }

public class ParameterSymbol : ISymbol
{
    public string Name { get; }
    public SymbolKind Kind => SymbolKind.Parameter;
    public ISymbol? ContainingSymbol { get; }

    public string FullName =>
        ContainingSymbol is { FullName.Length: > 0 }
            ? $"{ContainingSymbol.FullName}.{Name}"
            : Name;

    public TypeRef Type { get; set; } = TypeRef.Unknown;
    public ParameterDirection Direction { get; set; } = ParameterDirection.In;
    public bool IsArray { get; set; }
    public bool IsExternal { get; set; }
    public string? SourceWinMdPath { get; set; }

    public ParameterSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }
}
