// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Analysis.Symbols;

public interface ISymbol
{
    string Name { get; }
    string FullName { get; }
    SymbolKind Kind { get; }
    ISymbol? ContainingSymbol { get; }
    bool IsExternal { get; }
    string? SourceWinMdPath { get; }
    /// <summary>Absolute path of the .idl source file that defined this symbol, or null if defined in the current file.</summary>
    string? SourceFile { get; }
    /// <summary>Absolute source position of this symbol's name token. -1 if unknown.</summary>
    int SourcePosition { get; set; }
    /// <summary>Parsed XML documentation comment (/// trivia), or null if none.</summary>
    DocComment? DocComment { get; set; }
}
