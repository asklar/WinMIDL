// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Analysis.Symbols;

public class PropertySymbol : ISymbol
{
    public string Name { get; }
    public SymbolKind Kind => SymbolKind.Property;
    public ISymbol? ContainingSymbol { get; }

    public string FullName =>
        ContainingSymbol is { FullName.Length: > 0 }
            ? $"{ContainingSymbol.FullName}.{Name}"
            : Name;

    public TypeRef Type { get; set; } = TypeRef.Unknown;
    public bool HasGetter { get; set; }
    public bool HasSetter { get; set; }
    public bool IsShorthand { get; set; }
    public bool IsGetterProtected { get; set; }
    public bool IsSetterProtected { get; set; }
    public bool IsStatic { get; set; }
    public bool IsProtected { get; set; }
    public bool IsOverridable { get; set; }
    public bool IsExternal { get; set; }
    public string? SourceWinMdPath { get; set; }
    public string? SourceFile { get; set; }
    public int SourcePosition { get; set; } = -1;
    public DocComment? DocComment { get; set; }

    // Extracted attribute data
    public List<AttributeData> Attributes { get; } = new();
    public string? ContractName { get; set; }
    public int? ContractVersion { get; set; }

    public PropertySymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }
}
