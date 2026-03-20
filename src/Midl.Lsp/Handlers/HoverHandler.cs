using System.Text.Json;
using Midl.Analysis;
using Midl.Syntax;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides hover information for tokens in MIDL documents.
/// </summary>
public static class HoverHandler
{
    private static readonly Dictionary<string, string> s_keywordDocs = new(StringComparer.Ordinal)
    {
        ["namespace"] = "**namespace** — Declares a namespace scope that groups related types.",
        ["runtimeclass"] = "**runtimeclass** — Declares a Windows Runtime class that can be activated and consumed across language boundaries.",
        ["interface"] = "**interface** — Declares a Windows Runtime interface that defines a contract of methods, properties, and events.",
        ["delegate"] = "**delegate** — Declares a delegate type for callbacks and event handlers.",
        ["struct"] = "**struct** — Declares a value type with fields. Structs in WinRT can only contain value types.",
        ["enum"] = "**enum** — Declares an enumeration type with named integer constants.",
        ["event"] = "**event** — Declares an event that can be subscribed to by consumers.",
        ["static"] = "**static** — Indicates that a class contains only static members (factory pattern).",
        ["partial"] = "**partial** — Allows a runtime class declaration to be split across multiple locations.",
        ["unsealed"] = "**unsealed** — Allows a runtime class to be used as a base class.",
        ["protected"] = "**protected** — Restricts member access to the declaring class and its derived classes.",
        ["overridable"] = "**overridable** — Allows a virtual method to be overridden in a derived class.",
        ["apicontract"] = "**apicontract** — Declares an API contract used for version checking.",
        ["import"] = "**import** — Imports type definitions from another IDL file.",
        ["void"] = "**void** — Indicates no return value.",
        ["Boolean"] = "**Boolean** — WinRT boolean type (true/false).",
        ["Int16"] = "**Int16** — 16-bit signed integer.",
        ["Int32"] = "**Int32** — 32-bit signed integer.",
        ["Int64"] = "**Int64** — 64-bit signed integer.",
        ["UInt8"] = "**UInt8** — 8-bit unsigned integer.",
        ["UInt16"] = "**UInt16** — 16-bit unsigned integer.",
        ["UInt32"] = "**UInt32** — 32-bit unsigned integer.",
        ["UInt64"] = "**UInt64** — 64-bit unsigned integer.",
        ["Single"] = "**Single** — 32-bit IEEE floating point.",
        ["Double"] = "**Double** — 64-bit IEEE floating point.",
        ["String"] = "**String** — UTF-16 Unicode string (HSTRING).",
        ["Char"] = "**Char** — UTF-16 Unicode character.",
        ["Guid"] = "**Guid** — 128-bit globally unique identifier.",
        ["Object"] = "**Object** — The base type for all Windows Runtime reference types.",
        ["get"] = "**get** — Property getter accessor.",
        ["set"] = "**set** — Property setter accessor.",
        ["out"] = "**out** — Output parameter direction.",
        ["ref"] = "**ref** — Reference parameter direction.",
        ["requires"] = "**requires** — Specifies interfaces that a runtime class must implement.",
    };

    public static object? Handle(JsonElement? @params, DocumentManager docManager)
    {
        if (@params is null) return null;

        var textDocument = @params.Value.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;
        var position = @params.Value.GetProperty("position");
        int line = position.GetProperty("line").GetInt32();
        int character = position.GetProperty("character").GetInt32();

        var text = docManager.GetDocumentText(uri);
        if (text is null) return null;

        var lineStarts = PositionHelper.BuildLineStarts(text);
        int offset = PositionHelper.LineCharToOffset(lineStarts, line, character);

        // Lex and find token at offset
        var lexer = new Lexer.Lexer(text);
        var tokens = lexer.Lex();
        var (token, tokenStart) = FindTokenAtOffset(tokens, offset);
        if (token is null) return null;

        string tokenText = token.Text;

        // Check keyword docs first
        if (s_keywordDocs.TryGetValue(tokenText, out var doc))
        {
            return MakeHover(doc, tokenStart, token.Text.Length, lineStarts);
        }

        // Try to find in compilation symbols
        if (token.Kind == SyntaxKind.IdentifierToken)
        {
            try
            {
                var compilation = CompilationHelper.Compile(text);
                var symbol = FindSymbolByName(compilation, tokenText);
                if (symbol is not null)
                {
                    string info;
                    if (symbol.IsExternal)
                    {
                        var source = symbol.SourceWinMdPath != null
                            ? $"\nDefined in: {Path.GetFileName(symbol.SourceWinMdPath)}"
                            : "";
                        info = $"(external) **{symbol.Kind}** `{symbol.FullName}`{source}";
                    }
                    else
                    {
                        info = $"**{symbol.Kind}** `{symbol.FullName}`";
                    }
                    return MakeHover(info, tokenStart, token.Text.Length, lineStarts);
                }
            }
            catch
            {
                // Compilation may fail on partial code — ignore
            }

            return MakeHover($"*identifier* `{tokenText}`", tokenStart, token.Text.Length, lineStarts);
        }

        return null;
    }

    private static object MakeHover(string markdown, int tokenStart, int tokenLength, int[] lineStarts)
    {
        var (startLine, startChar) = PositionHelper.OffsetToLineChar(lineStarts, tokenStart);
        var (endLine, endChar) = PositionHelper.OffsetToLineChar(lineStarts, tokenStart + tokenLength);

        return new Dictionary<string, object?>
        {
            ["contents"] = new Dictionary<string, object?> { ["kind"] = "markdown", ["value"] = markdown },
            ["range"] = new Dictionary<string, object?>
            {
                ["start"] = new Dictionary<string, object?> { ["line"] = startLine, ["character"] = startChar },
                ["end"] = new Dictionary<string, object?> { ["line"] = endLine, ["character"] = endChar }
            }
        };
    }

    /// <summary>
    /// Finds the token whose text span contains the given offset, returning the token and its absolute start position.
    /// </summary>
    private static (SyntaxToken? Token, int TextStart) FindTokenAtOffset(IReadOnlyList<SyntaxToken> tokens, int offset)
    {
        int pos = 0;
        foreach (var token in tokens)
        {
            int leadingWidth = token.LeadingTrivia.Sum(t => t.FullWidth);
            int textStart = pos + leadingWidth;
            int textEnd = textStart + token.Text.Length;
            int trailingWidth = token.TrailingTrivia.Sum(t => t.FullWidth);

            if (offset >= textStart && offset < textEnd)
                return (token, textStart);

            pos = textEnd + trailingWidth;
        }
        return (null, 0);
    }

    private static Analysis.Symbols.ISymbol? FindSymbolByName(Compilation compilation, string name)
    {
        return compilation.GetAllTypes().FirstOrDefault(s => s.Name == name);
    }
}
