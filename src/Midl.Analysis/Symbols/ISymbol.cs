namespace Midl.Analysis.Symbols;

public interface ISymbol
{
    string Name { get; }
    string FullName { get; }
    SymbolKind Kind { get; }
    ISymbol? ContainingSymbol { get; }
    bool IsExternal { get; }
    string? SourceWinMdPath { get; }
}
