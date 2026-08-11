// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Midl.Analysis;
using Midl.Lsp.Protocol;
using Midl.Syntax;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides hover information for tokens in MIDL documents.
/// </summary>
public static class HoverHandler
{
    private static readonly Dictionary<string, string> s_keywordDocs= new(StringComparer.Ordinal)
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

    public static object? Handle(JsonElement? @params, DocumentManager docManager,
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

        // For decompiled WinMD views, extract the WinMD path from the URI for Learn links
        string? decompileWinMdPath = null;
        if (uri.StartsWith("midl-winmd:", StringComparison.OrdinalIgnoreCase))
        {
            // URI format: midl-winmd:///path/to/file.winmd/Namespace.Name
            var pathPart = uri.IndexOf("midl-winmd:", StringComparison.OrdinalIgnoreCase);
            var rawPath = uri[(pathPart + "midl-winmd:".Length)..].TrimStart('/');
            rawPath = Uri.UnescapeDataString(rawPath);
            var winmdIdx = rawPath.IndexOf(".winmd", StringComparison.OrdinalIgnoreCase);
            if (winmdIdx >= 0)
                decompileWinMdPath = rawPath[..(winmdIdx + 6)];
        }

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
            // For decompiled WinMD views, build Learn links from the identifier directly
            if (decompileWinMdPath != null)
            {
                var fqn = SymbolResolver.ExtractFullQualifiedNameAtCursor(text, tokenStart, tokenText);
                var lookupName = fqn ?? tokenText;

                // Only show hover for type-like identifiers (skip lowercase params etc.)
                if (lookupName.Length == 0 || char.IsLower(lookupName[0])) goto skipDecompileHover;

                var kind = InferSymbolKind(text, tokenStart);

                // Only prepend namespace for type declarations, not methods/properties/events
                bool isTypeDecl = kind is "runtimeclass" or "interface" or "enum" or "struct" or "delegate" or "apicontract";
                if (!lookupName.Contains('.') && isTypeDecl)
                {
                    var ns = FindEnclosingNamespace(text, tokenStart);
                    if (ns != null) lookupName = ns + "." + lookupName;
                }

                // For methods/properties, show just the short name
                if (!isTypeDecl)
                {
                    lookupName = tokenText;
                }

                var learnUrl = isTypeDecl ? GetMsLearnUrl(lookupName, decompileWinMdPath) : null;
                var info = learnUrl != null
                    ? $"(SDK) **{kind}** `{lookupName}`"
                    : $"**{kind}** `{lookupName}`";

                if (learnUrl != null)
                {
                    var cmdArgs = Uri.EscapeDataString("[\"" + learnUrl + "\"]");
                    info += $"\n\n[📖 View on Microsoft Learn](command:midl.openDocs?{cmdArgs})";
                }
                return MakeHover(info, tokenStart, token.Text.Length, lineStarts);
            }
            skipDecompileHover:

            try
            {
                var compilation = cachedCompilation ?? CompilationHelper.Compile(text);

                // A macro is substituted out before binding, so it will never resolve to a
                // symbol. Check it before the symbol lookup: if the name is a macro, that is
                // what the compiler actually did with it here.
                if (compilation.Macros.TryGetValue(tokenText, out var macro))
                {
                    return MakeHover(DescribeMacro(macro, text, lineStarts),
                        tokenStart, token.Text.Length, lineStarts);
                }

                // Try extracting a full qualified name (e.g. hovering "Windows" in "Windows.UI.Xaml.Application")
                var fqn = SymbolResolver.ExtractFullQualifiedNameAtCursor(text, tokenStart, tokenText);
                var lookupName = fqn ?? tokenText;

                var symbol = FindSymbolByName(compilation, lookupName);
                // If FQN didn't match, fall back to short name
                if (symbol is null && fqn != null)
                    symbol = FindSymbolByName(compilation, tokenText);
                if (symbol is not null)
                {
                    string info;
                    if (symbol.IsExternal)
                    {
                        var source = symbol.SourceWinMdPath != null
                            ? $"\nDefined in: {Path.GetFileName(symbol.SourceWinMdPath)}"
                            : "";
                        info = $"(external) **{symbol.Kind}** `{symbol.FullName}`{source}";

                        // Add Microsoft Learn link for Windows SDK types
                        var learnUrl = GetMsLearnUrl(symbol.FullName, symbol.SourceWinMdPath);
                        if (learnUrl != null)
                        {
                            var cmdArgs = Uri.EscapeDataString("[\"" + learnUrl + "\"]");
                            info += $"\n\n[📖 View on Microsoft Learn](command:midl.openDocs?{cmdArgs})";
                        }
                    }
                    else
                    {
                        info = $"**{symbol.Kind}** `{symbol.FullName}`";

                        // Show doc comment summary for local symbols
                        if (symbol.DocComment?.Summary != null)
                            info += $"\n\n{symbol.DocComment.Summary}";

                        // Show parameter docs for methods
                        if (symbol is Analysis.Symbols.MethodSymbol ms && ms.DocComment?.Params.Count > 0)
                        {
                            foreach (var kvp in ms.DocComment.Params)
                                info += $"\n\n**param** {kvp.Key}: {kvp.Value}";
                        }
                    }
                    return MakeHover(info, tokenStart, token.Text.Length, lineStarts);
                }

                // Also check lazy providers for SDK types not yet resolved
                // Try FQN match first, then short name
                foreach (var fullName in compilation.GetAllLazyTypeNames())
                {
                    var lastDot = fullName.LastIndexOf('.');
                    var shortName = lastDot >= 0 ? fullName[(lastDot + 1)..] : fullName;
                    var backtick = shortName.IndexOf('`');
                    if (backtick >= 0) shortName = shortName[..backtick];
                    var cleanName = backtick >= 0 ? fullName[..fullName.IndexOf('`')] : fullName;

                    bool matched = false;
                    if (fqn != null && cleanName.Equals(fqn, StringComparison.OrdinalIgnoreCase))
                        matched = true;
                    else if (shortName.Equals(tokenText, StringComparison.OrdinalIgnoreCase))
                        matched = true;

                    if (matched)
                    {
                        var info = $"(SDK) **type** `{cleanName}`";

                        var learnUrl = GetMsLearnUrl(cleanName, null);
                        if (learnUrl != null)
                        {
                            var cmdArgs = Uri.EscapeDataString("[\"" + learnUrl + "\"]");
                            info += $"\n\n[📖 View on Microsoft Learn](command:midl.openDocs?{cmdArgs})";
                        }
                        return MakeHover(info, tokenStart, token.Text.Length, lineStarts);
                    }
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

    /// <summary>
    /// Infer the symbol kind by scanning the current line for a declaring keyword.
    /// </summary>
    private static string InferSymbolKind(string text, int position)
    {
        int lineStart = text.LastIndexOf('\n', Math.Max(0, position - 1)) + 1;
        var lineBefore = text[lineStart..position].TrimStart();

        // Type declarations
        if (lineBefore.Contains("runtimeclass ") || lineBefore.Contains("runtimeclass\t"))
            return "runtimeclass";
        if (lineBefore.Contains("interface ") || lineBefore.Contains("interface\t"))
            return "interface";
        if (lineBefore.Contains("enum "))
            return "enum";
        if (lineBefore.Contains("struct "))
            return "struct";
        if (lineBefore.Contains("delegate "))
            return "delegate";
        if (lineBefore.Contains("apicontract "))
            return "apicontract";

        // Events
        if (lineBefore.Contains("event "))
            return "event";

        // Methods: check if there's a '(' after the identifier name
        int afterIdent = position;
        while (afterIdent < text.Length && (char.IsLetterOrDigit(text[afterIdent]) || text[afterIdent] == '_')) afterIdent++;
        while (afterIdent < text.Length && (text[afterIdent] == ' ' || text[afterIdent] == '\t')) afterIdent++;
        if (afterIdent < text.Length && text[afterIdent] == '(')
            return "method";

        // Properties: followed by '{' or ';' (shorthand)
        if (afterIdent < text.Length && (text[afterIdent] == '{' || text[afterIdent] == ';'))
            return "property";

        return "type";
    }

    /// <summary>
    /// Find the enclosing namespace declaration by scanning backwards from the cursor position.
    /// </summary>
    private static string? FindEnclosingNamespace(string text, int position)
    {
        // Scan backwards for "namespace X.Y.Z"
        var searchRegion = text[..Math.Min(position, text.Length)];
        int nsIdx = searchRegion.LastIndexOf("namespace ", StringComparison.Ordinal);
        if (nsIdx < 0) return null;

        int nameStart = nsIdx + "namespace ".Length;
        int nameEnd = nameStart;
        while (nameEnd < text.Length && text[nameEnd] != '{' && text[nameEnd] != '\r' && text[nameEnd] != '\n')
            nameEnd++;
        var ns = text[nameStart..nameEnd].Trim();
        return ns.Length > 0 ? ns : null;
    }

    /// <summary>
    /// Generate a Microsoft Learn URL for a Windows SDK type.
    /// </summary>
    private static string? GetMsLearnUrl(string fullName, string? winmdPath)
    {
        if (!fullName.StartsWith("Windows.", StringComparison.OrdinalIgnoreCase) &&
            !fullName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase))
            return null;

        // Extract SDK version from WinMD path (e.g. ...UnionMetadata\10.0.26100.0\Windows.winmd)
        string? sdkVersion = null;
        if (winmdPath != null)
        {
            var dir = Path.GetDirectoryName(winmdPath);
            if (dir != null)
            {
                var dirName = Path.GetFileName(dir);
                if (dirName != null && dirName.StartsWith("10.0."))
                    sdkVersion = dirName;
            }
        }
        // Fallback: find SDK version from installed SDKs
        sdkVersion ??= FindInstalledSdkVersion();

        var baseUrl = fullName.StartsWith("Windows.", StringComparison.OrdinalIgnoreCase)
            ? "https://learn.microsoft.com/en-us/uwp/api/"
            : "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/";

        var typePath = fullName.ToLowerInvariant();
        var url = baseUrl + typePath;

        // Append SDK version as winrt-NNNNN view parameter
        if (sdkVersion != null)
        {
            // 10.0.26100.0 → winrt-26100
            var parts = sdkVersion.Split('.');
            if (parts.Length >= 3)
                url += $"?view=winrt-{parts[2]}";
        }

        return url;
    }

    private static string? FindInstalledSdkVersion()
    {
        try
        {
            var unionMeta = @"C:\Program Files (x86)\Windows Kits\10\UnionMetadata";
            if (!Directory.Exists(unionMeta)) return null;
            return Directory.GetDirectories(unionMeta)
                .Select(Path.GetFileName)
                .Where(d => d != null && d.StartsWith("10.0.") && !d.Equals("Facade", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(d => d)
                .FirstOrDefault();
        }
        catch { return null; }
    }

    /// <summary>
    /// Describe a preprocessor macro for hover.
    ///
    /// The expansion is the point: a macro that expands to nothing is why a line such as
    /// <c>Int32 FOO x;</c> compiles as <c>Int32 x;</c>, and nothing else in the editor says
    /// so. An empty code block would read as a rendering glitch, so that case is spelled out
    /// in words instead.
    /// </summary>
    private static string DescribeMacro(Midl.Lexer.MacroInfo macro, string text, int[] lineStarts)
    {
        var signature = macro.IsFunctionLike
            ? $"{macro.Name}({string.Join(", ", macro.Parameters!)}{(macro.IsVariadic ? ", ..." : "")})"
            : macro.Name;

        var sb = new System.Text.StringBuilder();
        sb.Append("**macro** `").Append(signature).Append("`\n\n");

        if (string.IsNullOrEmpty(macro.Body))
            sb.Append("Expands to nothing — the name is removed from the code.\n");
        else
            sb.Append("Expands to:\n```\n").Append(macro.Body).Append("\n```\n");

        if (macro.IsBuiltIn)
        {
            sb.Append("\nPredefined by the compiler.");
        }
        else if (TryFindDefineLine(text, lineStarts, macro, out int line))
        {
            sb.Append("\nDefined on line ").Append(line + 1).Append('.');
        }
        else if (macro.FilePath != null)
        {
            sb.Append("\nDefined in ").Append(Path.GetFileName(macro.FilePath)).Append('.');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Confirm that the macro's recorded offset really is its <c>#define</c> in <em>this</em>
    /// document before reporting a line number.
    ///
    /// Offsets are recorded per file, so a macro from an #include carries an offset into that
    /// file — meaningless against this document's line starts, and confidently wrong if
    /// printed. Checking the line says what it claims to say costs one string comparison.
    /// </summary>
    private static bool TryFindDefineLine(string text, int[] lineStarts, Midl.Lexer.MacroInfo macro, out int line)
    {
        line = 0;
        if (macro.Position < 0 || macro.Position >= text.Length) return false;

        (line, _) = PositionHelper.OffsetToLineChar(lineStarts, macro.Position);
        int start = lineStarts[line];
        int end = line + 1 < lineStarts.Length ? lineStarts[line + 1] : text.Length;

        var lineText = text[start..end];
        int hash = lineText.IndexOf('#');
        if (hash < 0) return false;

        var afterHash = lineText[(hash + 1)..].TrimStart();
        if (!afterHash.StartsWith("define", StringComparison.Ordinal)) return false;

        var afterDefine = afterHash["define".Length..].TrimStart();
        return afterDefine.StartsWith(macro.Name, StringComparison.Ordinal);
    }

    private static HoverResult MakeHover(string markdown, int tokenStart, int tokenLength, int[] lineStarts)
    {
        var (startLine, startChar) = PositionHelper.OffsetToLineChar(lineStarts, tokenStart);
        var (endLine, endChar) = PositionHelper.OffsetToLineChar(lineStarts, tokenStart + tokenLength);

        return new HoverResult(
            new MarkupContent("markdown", markdown),
            new LspRange(
                new LspPosition(startLine, startChar),
                new LspPosition(endLine, endChar)));
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
        // Try FQN match first
        if (name.Contains('.'))
        {
            var byFullName = compilation.GetAllTypes().FirstOrDefault(s =>
                s.FullName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (byFullName != null) return byFullName;

            // Try resolving via GetTypeByName (triggers lazy providers)
            var resolved = compilation.GetTypeByName(name);
            if (resolved != null) return resolved;
        }
        return compilation.GetAllTypes().FirstOrDefault(s => s.Name == name);
    }

}
