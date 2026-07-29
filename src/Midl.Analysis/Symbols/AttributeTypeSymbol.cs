// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Midl.Analysis.Symbols;

/// <summary>
/// Represents a user-defined custom attribute type declared with the 'attribute' keyword.
/// </summary>
public class AttributeTypeSymbol : ISymbol
{
    public string Name { get; }
    public SymbolKind Kind => SymbolKind.Attribute;
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

    // Extracted attribute data (attributes on the attribute declaration itself)
    public List<AttributeData> Attributes { get; } = new();

    // Fields of the attribute (constructor parameters in WinMD)
    private readonly List<FieldSymbol> _fields = [];
    public IReadOnlyList<FieldSymbol> Fields => _fields;

    // From [attributeusage(target_runtimeclass, target_method, ...)]
    public List<string> ValidTargets { get; } = new();

    // From [attributename("ShortName")]
    public string? AttributeNameOverride { get; set; }

    // From [allowmultiple]
    public bool AllowMultiple { get; set; }

    public AttributeTypeSymbol(string name, ISymbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }

    public void AddField(FieldSymbol field) => _fields.Add(field);
}
