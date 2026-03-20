using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.Syntax;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Returns document symbols (outline) for MIDL documents by walking the parsed green tree.
/// </summary>
public static class DocumentSymbolHandler
{
    // LSP SymbolKind values
    private const int SymbolKindNamespace = 3;
    private const int SymbolKindClass = 5;
    private const int SymbolKindMethod = 6;
    private const int SymbolKindProperty = 7;
    private const int SymbolKindInterface = 11;
    private const int SymbolKindEnum = 10;
    private const int SymbolKindStruct = 23;
    private const int SymbolKindEvent = 24;
    private const int SymbolKindConstructor = 9;
    private const int SymbolKindField = 8;
    private const int SymbolKindEnumMember = 22;

    public static object? Handle(JsonElement? @params, DocumentManager docManager)
    {
        if (@params is null) return null;

        var textDocument = @params.Value.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;

        var text = docManager.GetDocumentText(uri);
        if (text is null) return Array.Empty<object>();

        var lineStarts = PositionHelper.BuildLineStarts(text);

        // Use the compilation to get symbols, which is more reliable than walking the green tree
        try
        {
            var compilation = CompilationHelper.Compile(text);
            var symbols = new List<object>();
            CollectSymbols(compilation.GlobalNamespace, lineStarts, text, symbols);
            return symbols;
        }
        catch
        {
            return Array.Empty<object>();
        }
    }

    private static void CollectSymbols(NamespaceSymbol ns, int[] lineStarts, string text, List<object> symbols)
    {
        foreach (var member in ns.Members)
        {
            if (member.Kind == Analysis.Symbols.SymbolKind.Namespace && member is NamespaceSymbol childNs)
            {
                // Find namespace in text for range
                var range = FindSymbolRange(text, lineStarts, "namespace", member.Name);
                var children = new List<object>();
                CollectSymbols(childNs, lineStarts, text, children);

                symbols.Add(MakeDocumentSymbol(member.Name, SymbolKindNamespace, range, range, children));
                continue;
            }

            var symbolRange = FindSymbolRange(text, lineStarts, KindToKeyword(member.Kind), member.Name);
            int lspKind = MapSymbolKind(member.Kind);

            var memberChildren = new List<object>();
            CollectMembers(member, lineStarts, text, memberChildren);

            symbols.Add(MakeDocumentSymbol(member.Name, lspKind, symbolRange, symbolRange, memberChildren));
        }
    }

    private static void CollectMembers(ISymbol symbol, int[] lineStarts, string text, List<object> children)
    {
        IReadOnlyList<ISymbol>? members = symbol switch
        {
            RuntimeClassSymbol rc => rc.Members,
            InterfaceSymbol iface => iface.Members,
            StructSymbol s => s.Fields.Cast<ISymbol>().ToList(),
            EnumSymbol e => e.EnumMembers.Cast<ISymbol>().ToList(),
            _ => null
        };

        if (members is null) return;

        foreach (var m in members)
        {
            var range = FindSymbolRange(text, lineStarts, null, m.Name);
            int kind = MapSymbolKind(m.Kind);
            children.Add(MakeDocumentSymbol(m.Name, kind, range, range, null));
        }
    }

    private static object MakeDocumentSymbol(string name, int kind, object range, object selectionRange, List<object>? children)
    {
        if (children is { Count: > 0 })
        {
            return new
            {
                name,
                kind,
                range,
                selectionRange,
                children
            };
        }

        return new
        {
            name,
            kind,
            range,
            selectionRange
        };
    }

    private static object FindSymbolRange(string text, int[] lineStarts, string? keyword, string name)
    {
        // Try to find the name in the source text, optionally preceded by keyword
        int nameIdx = -1;
        if (keyword is not null)
        {
            string pattern = keyword + " " + name;
            nameIdx = text.IndexOf(pattern, StringComparison.Ordinal);
            if (nameIdx >= 0)
                nameIdx += keyword.Length + 1; // point to the name part
        }

        if (nameIdx < 0)
            nameIdx = text.IndexOf(name, StringComparison.Ordinal);

        if (nameIdx < 0)
        {
            // Fallback: beginning of document
            return new
            {
                start = new { line = 0, character = 0 },
                end = new { line = 0, character = 0 }
            };
        }

        var (startLine, startChar) = PositionHelper.OffsetToLineChar(lineStarts, nameIdx);
        var (endLine, endChar) = PositionHelper.OffsetToLineChar(lineStarts, nameIdx + name.Length);

        return new
        {
            start = new { line = startLine, character = startChar },
            end = new { line = endLine, character = endChar }
        };
    }

    private static int MapSymbolKind(Analysis.Symbols.SymbolKind kind) => kind switch
    {
        Analysis.Symbols.SymbolKind.Namespace => SymbolKindNamespace,
        Analysis.Symbols.SymbolKind.RuntimeClass => SymbolKindClass,
        Analysis.Symbols.SymbolKind.Interface => SymbolKindInterface,
        Analysis.Symbols.SymbolKind.Struct => SymbolKindStruct,
        Analysis.Symbols.SymbolKind.Enum => SymbolKindEnum,
        Analysis.Symbols.SymbolKind.Delegate => SymbolKindClass,
        Analysis.Symbols.SymbolKind.Method => SymbolKindMethod,
        Analysis.Symbols.SymbolKind.Property => SymbolKindProperty,
        Analysis.Symbols.SymbolKind.Event => SymbolKindEvent,
        Analysis.Symbols.SymbolKind.Constructor => SymbolKindConstructor,
        Analysis.Symbols.SymbolKind.Field => SymbolKindField,
        Analysis.Symbols.SymbolKind.EnumMember => SymbolKindEnumMember,
        _ => SymbolKindField
    };

    private static string? KindToKeyword(Analysis.Symbols.SymbolKind kind) => kind switch
    {
        Analysis.Symbols.SymbolKind.RuntimeClass => "runtimeclass",
        Analysis.Symbols.SymbolKind.Interface => "interface",
        Analysis.Symbols.SymbolKind.Struct => "struct",
        Analysis.Symbols.SymbolKind.Enum => "enum",
        Analysis.Symbols.SymbolKind.Delegate => "delegate",
        Analysis.Symbols.SymbolKind.ApiContract => "apicontract",
        _ => null
    };
}
