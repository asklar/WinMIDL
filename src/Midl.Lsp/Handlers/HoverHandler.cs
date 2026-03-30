using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using Midl.Analysis;
using Midl.Lsp.Protocol;
using Midl.Syntax;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides hover information for tokens in MIDL documents.
/// </summary>
public static class HoverHandler
{
    private static readonly HttpClient s_httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };
    private static readonly ConcurrentDictionary<string, string?> s_descriptionCache = new();
    internal static bool FetchSdkDocs { get; set; } = true;
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
                var compilation = cachedCompilation ?? CompilationHelper.Compile(text);

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

                        // Add cached description if available
                        var desc = GetCachedDescription(symbol.FullName);
                        if (desc != null)
                            info += $"\n\n{desc}";

                        // Add Microsoft Learn link for Windows SDK types
                        var learnUrl = GetMsLearnUrl(symbol.FullName, symbol.SourceWinMdPath);
                        if (learnUrl != null)
                        {
                            var cmdArgs = Uri.EscapeDataString("[\"" + learnUrl + "\"]");
                            info += $"\n\n[📖 View on Microsoft Learn](command:midl.openDocs?{cmdArgs})";
                            if (desc == null && FetchSdkDocs)
                                _ = FetchDescriptionAsync(symbol.FullName, learnUrl);
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

                        var desc = GetCachedDescription(cleanName);
                        if (desc != null)
                            info += $"\n\n{desc}";

                        var learnUrl = GetMsLearnUrl(cleanName, null);
                        if (learnUrl != null)
                        {
                            var cmdArgs = Uri.EscapeDataString("[\"" + learnUrl + "\"]");
                            info += $"\n\n[📖 View on Microsoft Learn](command:midl.openDocs?{cmdArgs})";
                            if (desc == null && FetchSdkDocs)
                                _ = FetchDescriptionAsync(cleanName, learnUrl);
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

    /// <summary>Get a cached description for a type, or null if not yet fetched.</summary>
    private static string? GetCachedDescription(string fullName)
    {
        return s_descriptionCache.TryGetValue(fullName, out var desc) ? desc : null;
    }

    /// <summary>Fetch a type description from Microsoft Learn in the background and cache it.</summary>
    private static async Task FetchDescriptionAsync(string fullName, string learnUrl)
    {
        if (s_descriptionCache.ContainsKey(fullName)) return;

        try
        {
            var html = await s_httpClient.GetStringAsync(learnUrl);

            // Extract the first paragraph from the page content
            // The MS Learn markdown/HTML has the description as the first meaningful paragraph
            var desc = ExtractDescription(html);
            s_descriptionCache[fullName] = desc;
        }
        catch
        {
            // Network error — cache null to avoid retrying
            s_descriptionCache.TryAdd(fullName, null);
        }
    }

    /// <summary>Extract the type description from a MS Learn HTML page.</summary>
    private static string? ExtractDescription(string html)
    {
        // Look for the first <p> tag content after the main heading area
        // MS Learn pages have the description as early paragraph text
        var match = Regex.Match(html, @"<meta\s+name=""description""\s+content=""([^""]+)""", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var desc = match.Groups[1].Value.Trim();
            // Clean up HTML entities
            desc = desc.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&#39;", "'").Replace("&quot;", "\"");
            // Truncate to first ~200 chars for hover readability
            if (desc.Length > 200)
                desc = desc[..197] + "...";
            return desc;
        }
        return null;
    }
}
