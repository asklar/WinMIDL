using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.Syntax;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Handles textDocument/definition requests — navigates to where a symbol is defined.
/// </summary>
public static class GotoDefinitionHandler
{
    public static object? Handle(JsonElement? @params, DocumentManager docManager,
        IReadOnlyList<string>? includePaths = null, IReadOnlyList<string>? referencePaths = null,
        Compilation? cachedCompilation = null)
    {
        if (@params is null) return null;

        var textDocument = @params.Value.GetProperty("textDocument");
        var uri = textDocument.GetProperty("uri").GetString()!;
        var position = @params.Value.GetProperty("position");
        int line = position.GetProperty("line").GetInt32();
        int character = position.GetProperty("character").GetInt32();

        var text = docManager.GetDocumentText(uri);
        if (text is null) return null;

        // Extract file path from URI for import resolution
        string? filePath = null;
        if (uri.StartsWith("file:///"))
            filePath = Uri.UnescapeDataString(uri[8..]);

        return HandleCore(text, uri, line, character, filePath, includePaths, referencePaths, cachedCompilation);
    }

    /// <summary>
    /// Core logic, testable without LSP JSON plumbing.
    /// Returns an LSP Location object, a command response, or null.
    /// </summary>
    public static object? HandleCore(string sourceText, string uri, int line, int character,
        string? currentFilePath = null,
        IReadOnlyList<string>? includePaths = null,
        IReadOnlyList<string>? referencePaths = null,
        Compilation? cachedCompilation = null)
    {
        var lineStarts = PositionHelper.BuildLineStarts(sourceText);

        // For decompiled WinMD views: look up the type in the WinMD and navigate to its namespace
        if (uri.StartsWith("midl-winmd:", StringComparison.OrdinalIgnoreCase))
            return WinMdNavigator.HandleWinMdView(sourceText, uri, line, character, lineStarts);

        // Check if cursor is on a // @ref: line → navigate to the referenced file/folder
        var refTarget = RefDirectiveNavigator.TryGetRefDirectiveTarget(sourceText, lineStarts, line);
        if (refTarget != null)
            return RefDirectiveNavigator.CreateRefDirectiveResponse(refTarget, uri, lineStarts, line);

        int offset = PositionHelper.LineCharToOffset(lineStarts, line, character);

        var lexer = new Lexer.Lexer(sourceText);
        var tokens = lexer.Lex();
        var (token, tokenOffset) = FindTokenAtOffset(tokens, offset);
        if (token is null || token.Kind != SyntaxKind.IdentifierToken)
            return null;

        string name = token.Text;

        // Use cached compilation if available (shares type universe with diagnostics/autocomplete)
        Compilation compilation;
        if (cachedCompilation != null)
        {
            compilation = cachedCompilation;
        }
        else
        {
            try
            {
                compilation = CompilationHelper.Compile(sourceText, referencePaths,
                    CompilationMode.Strict, includePaths, currentFilePath);
            }
            catch
            {
                return null;
            }
        }

        // If the cursor is inside a fully qualified name, extract it and resolve directly
        var fullQualifiedName = SymbolResolver.ExtractFullQualifiedNameAtCursor(sourceText, tokenOffset, name);
        ISymbol? symbol = null;

        if (fullQualifiedName != null && fullQualifiedName != name)
        {
            // FQN found — resolve the type by qualified name
            symbol = compilation.SymbolTable?.LookupDefinedType(fullQualifiedName);
            symbol ??= compilation.GetTypeByName(fullQualifiedName);

            // Fallback: search lazy providers by the last segment (the actual type name)
            if (symbol is null)
            {
                foreach (var fqn in compilation.GetAllLazyTypeNames())
                {
                    if (fqn.Equals(fullQualifiedName, StringComparison.Ordinal))
                    {
                        symbol = compilation.GetTypeByName(fqn);
                        break;
                    }
                }
            }
        }

        // If no FQN or FQN didn't resolve, try simple name
        symbol ??= SymbolResolver.FindSymbol(compilation, name, sourceText, tokenOffset);

        if (symbol is null) return null;

        // Imported .idl symbol: navigate to definition in the source file
        if (symbol.SourceFile != null)
        {
            var resolvedSourceFile = symbol.SourceFile;
            if (File.Exists(resolvedSourceFile))
            {
                var otherText = File.ReadAllText(resolvedSourceFile);
                int defOffset = FindDefinitionOffset(otherText, symbol);
                if (defOffset >= 0)
                {
                    var otherLineStarts = PositionHelper.BuildLineStarts(otherText);
                    var (defLine, defChar) = PositionHelper.OffsetToLineChar(otherLineStarts, defOffset);
                    var (defEndLine, defEndChar) = PositionHelper.OffsetToLineChar(otherLineStarts, defOffset + symbol.Name.Length);
                    var otherUri = "file:///" + resolvedSourceFile.Replace('\\', '/');
                    return MakeLocation(otherUri, defLine, defChar, defEndLine, defEndChar);
                }
            }
        }

        // External WinMD symbol: navigate to decompiled view
        if (symbol.IsExternal && symbol.SourceWinMdPath != null)
        {
            if (File.Exists(symbol.SourceWinMdPath))
            {
                return WinMdNavigator.CreateWinMdDefinitionLocation(symbol);
            }

            // Built-in WinRT types have SourceWinMdPath = "<built-in>" — resolve via Windows.winmd
            if (symbol.SourceWinMdPath == "<built-in>")
            {
                var winmd = WinMdNavigator.FindWindowsWinMd();
                if (winmd != null)
                    return WinMdNavigator.CreateWinMdDefinitionLocation(new WinMdNavigator.BuiltInSymbolProxy(symbol, winmd));
            }

            // Fall back to @ref line navigation
            var refLine = RefDirectiveNavigator.FindRefLineForExternalSymbol(sourceText, symbol);
            if (refLine >= 0)
            {
                var lineText = GetLineText(sourceText, lineStarts, refLine);
                return MakeLocation(uri, refLine, 0, refLine, lineText.Length);
            }
            return null;
        }

        // Non-external built-in WinRT types (shouldn't happen, but handle gracefully)
        if (symbol.FullName.StartsWith("Windows.") && FindDefinitionOffset(sourceText, symbol) < 0)
        {
            var winmd = WinMdNavigator.FindWindowsWinMd();
            if (winmd != null)
                return WinMdNavigator.CreateWinMdDefinitionLocation(new WinMdNavigator.BuiltInSymbolProxy(symbol, winmd));
        }

        // Find the definition location in source text
        int localDefOffset = FindDefinitionOffset(sourceText, symbol);
        if (localDefOffset < 0) return null;

        var (localDefLine, localDefChar) = PositionHelper.OffsetToLineChar(lineStarts, localDefOffset);
        var (localDefEndLine, localDefEndChar) = PositionHelper.OffsetToLineChar(lineStarts, localDefOffset + symbol.Name.Length);

        return MakeLocation(uri, localDefLine, localDefChar, localDefEndLine, localDefEndChar);
    }

    internal static Dictionary<string, object?> MakeLocation(string uri, int startLine, int startChar, int endLine, int endChar)
    {
        return new Dictionary<string, object?>
        {
            ["uri"] = uri,
            ["range"] = new Dictionary<string, object?>
            {
                ["start"] = new Dictionary<string, object?> { ["line"] = startLine, ["character"] = startChar },
                ["end"] = new Dictionary<string, object?> { ["line"] = endLine, ["character"] = endChar }
            }
        };
    }

    /// <summary>
    /// Finds the offset of the symbol's defining identifier in source text.
    /// Uses keyword + name pattern matching (same approach as DocumentSymbolHandler).
    /// </summary>
    internal static int FindDefinitionOffset(string sourceText, ISymbol symbol)
    {
        string? keyword = symbol.Kind switch
        {
            SymbolKind.RuntimeClass => "runtimeclass",
            SymbolKind.Interface => "interface",
            SymbolKind.Struct => "struct",
            SymbolKind.Enum => "enum",
            SymbolKind.Delegate => null, // delegate has return type between keyword and name
            SymbolKind.ApiContract => "apicontract",
            SymbolKind.Namespace => "namespace",
            _ => null
        };

        if (keyword is not null)
        {
            string pattern = keyword + " " + symbol.Name;
            int searchFrom = 0;
            while (true)
            {
                int idx = sourceText.IndexOf(pattern, searchFrom, StringComparison.Ordinal);
                if (idx < 0) break;
                int afterName = idx + pattern.Length;
                // Verify word boundary: next char must not be alphanumeric/underscore
                if (afterName < sourceText.Length &&
                    (char.IsLetterOrDigit(sourceText[afterName]) || sourceText[afterName] == '_'))
                {
                    searchFrom = idx + 1;
                    continue;
                }
                return idx + keyword.Length + 1;
            }
        }

        // For type-level symbols, if the keyword pattern wasn't found, the type isn't defined here.
        // Don't fall back to usage-site matching (would find property types, method return types, etc.)
        if (keyword is not null)
            return -1;

        // For delegates: search for "delegate <returntype> <name>" pattern
        if (symbol.Kind == SymbolKind.Delegate)
        {
            int searchFrom = 0;
            while (true)
            {
                int delegateIdx = sourceText.IndexOf("delegate ", searchFrom, StringComparison.Ordinal);
                if (delegateIdx < 0) break;
                int afterDelegate = delegateIdx + 9;
                int nameIdx = sourceText.IndexOf(symbol.Name, afterDelegate, StringComparison.Ordinal);
                if (nameIdx >= 0 && nameIdx - afterDelegate < 40)
                    return nameIdx;
                searchFrom = afterDelegate;
            }
            return -1; // delegate not found either
        }

        // Fallback for members only (methods, properties, etc.): find with word boundary
        for (int i = sourceText.IndexOf(symbol.Name, StringComparison.Ordinal);
             i >= 0;
             i = sourceText.IndexOf(symbol.Name, i + 1, StringComparison.Ordinal))
        {
            int afterName = i + symbol.Name.Length;
            if (afterName < sourceText.Length && char.IsLetterOrDigit(sourceText[afterName]))
                continue;
            return i;
        }
        return -1;
    }

    internal static (SyntaxToken? Token, int TextStart) FindTokenAtOffset(IReadOnlyList<SyntaxToken> tokens, int offset)
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

    internal static string GetLineText(string sourceText, int[] lineStarts, int line)
    {
        if (line >= lineStarts.Length) return "";
        int start = lineStarts[line];
        int end = line + 1 < lineStarts.Length ? lineStarts[line + 1] : sourceText.Length;
        // Trim trailing newline
        while (end > start && (sourceText[end - 1] == '\n' || sourceText[end - 1] == '\r'))
            end--;
        return sourceText[start..end];
    }
}
