namespace Midl.Analysis.Symbols;

public interface ISymbol
{
    string Name { get; }
    string FullName { get; }
    SymbolKind Kind { get; }
    ISymbol? ContainingSymbol { get; }
    bool IsExternal { get; }
    string? SourceWinMdPath { get; }
    /// <summary>Absolute source position of this symbol's name token. -1 if unknown.</summary>
    int SourcePosition { get; set; }
}
