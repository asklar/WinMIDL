using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides keyword and type completions for MIDL3 documents.
/// Context-aware: when inside [...], shows only applicable attributes.
/// </summary>
public static class CompletionHandler
{
    // CompletionItemKind values per LSP spec
    private const int KindKeyword = 14;
    private const int KindClass = 7;
    private const int KindInterface = 8;
    private const int KindEnum = 13;
    private const int KindStruct = 22;
    private const int KindTypeParameter = 25;
    private const int KindModule = 9;  // for namespace
    private const int KindEvent = 23;
    private const int KindProperty = 10; // for attributes
    private const int KindFile = 17; // for import file completions
    private const int KindConstructor = 4; // for constructor suggestions
    private const int KindEnumMember = 20; // for enum member values
    private const int KindFolder = 19; // for directory completions

    private static readonly object[] s_staticCompletionItems = BuildCompletionItems();

    public static object Handle(JsonElement? @params, DocumentManager docManager,
        IReadOnlyList<string>? includePaths = null, ProjectFile? projectFile = null)
    {
        if (@params == null)
            return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = s_staticCompletionItems };

        try
        {
            var textDocument = @params.Value.GetProperty("textDocument");
            var uri = textDocument.GetProperty("uri").GetString()!;
            var position = @params.Value.GetProperty("position");
            int line = position.GetProperty("line").GetInt32();
            int character = position.GetProperty("character").GetInt32();

            var text = docManager.GetDocumentText(uri);
            if (text == null)
                return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = s_staticCompletionItems };

            int offset = GetOffset(text, line, character);

            // Check if cursor is inside an import "..." string
            string? fileDir = null;
            if (uri.StartsWith("file:///"))
            {
                var dir = Path.GetDirectoryName(Uri.UnescapeDataString(uri[8..]));
                fileDir = string.IsNullOrEmpty(dir) ? null : dir;
            }

#if WINDOWS_INTERNAL
            var featureItems = DetectFeatureNameContext(text, line, character);
            if (featureItems != null)
                return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = featureItems.ToArray() };
#endif

            var importContext = DetectImportContext(text, line);
            if (importContext != null)
            {
                string? currentFileName = null;
                if (uri.StartsWith("file:///"))
                    currentFileName = Path.GetFileName(Uri.UnescapeDataString(uri[8..]));
                var items = BuildImportCompletions(importContext, fileDir, includePaths, projectFile, currentFileName);
                return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = items.ToArray() };
            }

            // Check if cursor is on a @ref directive line: // @ref: partial_path
            var refContext = DetectRefDirectiveContext(text, line, character);
            if (refContext != null)
            {
                var items = BuildRefDirectiveCompletions(refContext, fileDir);
                return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = items.ToArray() };
            }

            // Check if cursor follows a dot (e.g. "Windows.Foundation." or "MyEnum.")
            var dotPrefix = ExtractDotPrefix(text, offset);
            if (dotPrefix != null)
            {
                var items = BuildDotCompletions(dotPrefix, text, includePaths, fileDir);
                if (items.Count > 0)
                    return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = items.ToArray() };
            }

            // Check if cursor is inside an attribute context [...]
            var attrContext = DetectAttributeContext(text, offset);

            if (attrContext != null)
            {
#if WINDOWS_INTERNAL
                var featureAttrItems = DetectFeatureAttributeArgContext(text, offset);
                if (featureAttrItems != null)
                    return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = featureAttrItems.ToArray() };
#endif
                var items = BuildAttributeCompletions(text, attrContext, includePaths, fileDir);
                return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = items.ToArray() };
            }

            // Context-aware completion based on cursor position
            var context = DetectCompletionContext(text, offset, line);
            var contextItems = BuildContextualCompletions(context, text, uri, includePaths, fileDir);
            // Use isIncomplete=true for contexts with dynamic user types so VS Code re-queries
            bool isIncomplete = context != CompletionContext.TopLevel && context != CompletionContext.EnumBody;
            return new Dictionary<string, object?> { ["isIncomplete"] = isIncomplete, ["items"] = contextItems.ToArray() };
        }
        catch
        {
            return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = s_staticCompletionItems };
        }
    }

    /// <summary>
    /// Detects if the cursor is inside an attribute bracket [...].
    /// Returns the target kind string (e.g. "target_runtimeclass") or null if not in attribute context.
    /// </summary>
    private static string? DetectAttributeContext(string text, int offset)
    {
        // Walk backward from cursor to find an unmatched '[' (not inside strings/comments)
        int depth = 0;
        for (int i = offset - 1; i >= 0; i--)
        {
            char c = text[i];
            if (c == ']') depth++;
            else if (c == '[')
            {
                if (depth > 0) depth--;
                else
                {
                    // Found unmatched '[' — we're inside an attribute list.
                    // Now determine the target by scanning forward from ']' to find what follows.
                    return DetermineAttributeTarget(text, offset);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// From the cursor position (inside [...]), look forward past the ']' to find
    /// what declaration keyword follows, determining the attribute target.
    /// </summary>
    private static string DetermineAttributeTarget(string text, int cursorOffset)
    {
        // Find the closing ']' after the cursor (or use end of text)
        int closeBracket = text.IndexOf(']', cursorOffset);
        int searchFrom = closeBracket >= 0 ? closeBracket + 1 : cursorOffset;

        // Skip whitespace and find the next keyword
        var remaining = text.AsSpan(searchFrom).TrimStart();
        // Also try scanning forward through possible nested attribute lists [attr][attr2]
        while (remaining.Length > 0 && remaining[0] == '[')
        {
            int nextClose = remaining.IndexOf(']');
            if (nextClose < 0) break;
            remaining = remaining[(nextClose + 1)..].TrimStart();
        }

        if (StartsWithWord(remaining, "runtimeclass")) return "target_runtimeclass";
        if (StartsWithWord(remaining, "interface")) return "target_interface";
        if (StartsWithWord(remaining, "enum")) return "target_enum";
        if (StartsWithWord(remaining, "struct")) return "target_struct";
        if (StartsWithWord(remaining, "delegate")) return "target_delegate";
        if (StartsWithWord(remaining, "attribute")) return "target_attribute";
        if (StartsWithWord(remaining, "apicontract")) return "target_apicontract";

        // Inside a type body: check if we're inside a class/interface body for member-level attrs
        // Also handle modifiers before the keyword
        if (StartsWithWord(remaining, "static") || StartsWithWord(remaining, "partial") ||
            StartsWithWord(remaining, "unsealed"))
            return "target_runtimeclass";

        // Check if cursor is inside a type body (between { and })
        // by looking backward for the enclosing declaration keyword
        return DetermineEnclosingTarget(text, cursorOffset);
    }

    /// <summary>
    /// Look backward from cursor to find the enclosing declaration (runtimeclass, interface, etc.)
    /// to determine what member-level attributes would be valid.
    /// </summary>
    private static string DetermineEnclosingTarget(string text, int offset)
    {
        int braceDepth = 0;
        for (int i = offset - 1; i >= 0; i--)
        {
            char c = text[i];
            if (c == '}') braceDepth++;
            else if (c == '{')
            {
                if (braceDepth > 0) { braceDepth--; continue; }
                // Found the opening brace of our enclosing scope — scan backward for keyword
                var before = text.AsSpan(0, i).TrimEnd();
                // Skip past the type name/base list to find the keyword
                if (ContainsWordBefore(before, "runtimeclass")) return "target_method"; // members
                if (ContainsWordBefore(before, "interface")) return "target_method";
                if (ContainsWordBefore(before, "namespace")) return "target_all"; // any type attribute
                break;
            }
        }
        return "target_all"; // default: show everything
    }

    private static bool StartsWithWord(ReadOnlySpan<char> text, string word)
    {
        if (text.Length < word.Length) return false;
        if (!text.StartsWith(word.AsSpan(), StringComparison.Ordinal)) return false;
        return text.Length == word.Length || !char.IsLetterOrDigit(text[word.Length]);
    }

    private static bool ContainsWordBefore(ReadOnlySpan<char> text, string word)
    {
        // Scan for keyword as a whole word in the last ~200 chars
        int searchLen = Math.Min(text.Length, 200);
        var search = text.Slice(text.Length - searchLen, searchLen);
        var wordSpan = word.AsSpan();
        int startIdx = 0;
        while (startIdx <= search.Length - word.Length)
        {
            int idx = search.Slice(startIdx).IndexOf(wordSpan);
            if (idx < 0) return false;
            idx += startIdx;

            bool leftOk = idx == 0 || !char.IsLetterOrDigit(search[idx - 1]) && search[idx - 1] != '_';
            bool rightOk = idx + word.Length >= search.Length ||
                           !char.IsLetterOrDigit(search[idx + word.Length]) && search[idx + word.Length] != '_';
            if (leftOk && rightOk) return true;

            startIdx = idx + 1;
        }
        return false;
    }

    /// <summary>Build context-appropriate attribute completions.</summary>
    private static List<object> BuildAttributeCompletions(string text, string targetKind,
        IReadOnlyList<string>? includePaths = null, string? currentFilePath = null)
    {
        var items = new List<object>();

        // Add well-known built-in attributes with their descriptions and target applicability
        foreach (var (name, detail, snippet) in WellKnownAttributeCompletions)
        {
            // Look up targets from the centralized map; no entry = valid everywhere
            var hasTargets = AttributeValidator.BuiltInAttributeTargets.TryGetValue(name, out var validTargets);
            if (targetKind == "target_all" || !hasTargets ||
                validTargets!.Contains(targetKind) ||
                (targetKind == "target_method" && validTargets!.Any(t => t.StartsWith("target_method"))))
            {
                var item = new Dictionary<string, object?>
                {
                    ["label"] = name,
                    ["kind"] = KindProperty,
                    ["detail"] = detail,
                };
                if (snippet != null)
                {
                    item["insertText"] = snippet;
                    item["insertTextFormat"] = 2; // Snippet format
                }
                else
                {
                    item["insertText"] = name;
                }
                items.Add(item);
            }
        }

        // Add user-defined custom attributes from the compilation
        try
        {
            var compilation = CompilationHelper.Compile(text, referencePaths: null,
                mode: CompilationMode.Strict, includePaths: includePaths, currentFilePath: currentFilePath);
            foreach (var type in compilation.GetAllTypes())
            {
                if (type is AttributeTypeSymbol attr)
                {
                    // Check attributeusage targets
                    bool applicable = attr.ValidTargets.Count == 0 || // no restriction = all targets
                        targetKind == "target_all" ||
                        attr.ValidTargets.Any(t => t.Equals(targetKind, StringComparison.OrdinalIgnoreCase)) ||
                        // Inside a type body, show attrs for any member kind
                        (targetKind == "target_method" && attr.ValidTargets.Any(t =>
                            t.StartsWith("target_method", StringComparison.OrdinalIgnoreCase) ||
                            t.StartsWith("target_property", StringComparison.OrdinalIgnoreCase) ||
                            t.StartsWith("target_event", StringComparison.OrdinalIgnoreCase)));

                    if (applicable)
                    {
                        var argSnippet = BuildAttributeArgSnippet(attr);
                        var displayName = attr.AttributeNameOverride ?? attr.Name;
                        items.Add(new Dictionary<string, object?>
                        {
                            ["label"] = displayName,
                            ["kind"] = KindProperty,
                            ["detail"] = $"(custom) {attr.FullName}",
                            ["insertText"] = argSnippet,
                            ["insertTextFormat"] = 2, // Snippet
                        });
                    }
                }
            }
        }
        catch
        {
            // Partial file — just return well-known attributes
        }

        return items;
    }

    /// <summary>Build a snippet like "MyAttr(${1:Level}, ${2:Description})" for a custom attribute.</summary>
    private static string BuildAttributeArgSnippet(AttributeTypeSymbol attr)
    {
        var displayName = attr.AttributeNameOverride ?? attr.Name;
        if (attr.Fields.Count == 0)
            return displayName;

        var args = new List<string>();
        for (int i = 0; i < attr.Fields.Count; i++)
        {
            var field = attr.Fields[i];
            args.Add($"${{{i + 1}:{field.Name}}}");
        }
        return $"{displayName}({string.Join(", ", args)})";
    }

    /// <summary>
    /// Well-known attributes with their descriptions and optional snippets.
    /// Target filtering is derived from <see cref="AttributeValidator.BuiltInAttributeTargets"/>.
    /// </summary>
    private static readonly (string Name, string Detail, string? Snippet)[] WellKnownAttributeCompletions =
    [
        // Type-level attributes
        ("uuid", "Assigns a GUID to an interface",
            "uuid(\"${1:00000000-0000-0000-0000-000000000000}\")"),
        ("contract", "Associates type with an API contract version",
            "contract(${1:ContractName}, ${2:version})"),
        ("version", "Specifies the version number",
            "version(${1:1})"),
        ("contractversion", "Sets the version of an API contract",
            "contractversion(${1:1})"),
        ("deprecated", "Marks as deprecated",
            "deprecated(\"${1:Use alternative}\", deprecate, ${2:ContractName}, ${3:version})"),
        ("experimental", "Marks as experimental", null),

        // Runtimeclass activation/composition
        ("activatable", "Makes class default-activatable or with factory",
            "activatable(${1:ContractName}, ${2:version})"),
        ("composable", "Makes class composable (base for derivation)",
            "composable(${1:IFactory}, ${2:public}, ${3:ContractName}, ${4:version})"),
        ("static", "Declares a static interface on the class",
            "static(${1:IStatics}, ${2:ContractName}, ${3:version})"),
        ("threading", "Threading model: both, sta, mta",
            "threading(${1|both,sta,mta|})"),
        ("marshaling_behavior", "Marshaling: agile, standard, none",
            "marshaling_behavior(${1|agile,standard,none|})"),
        ("webhosthidden", "Hides type from JavaScript projection", null),
        ("default_interface", "Marks interface as the default", null),
        ("contentproperty", "Specifies the content property for XAML", "contentproperty(\"${1:PropertyName}\")"),

        // Interface attributes
        ("exclusiveto", "Restricts interface to a specific class",
            "exclusiveto(${1:ClassName})"),
        ("default", "Marks as default interface implementation", null),
        ("overridable", "Interface can be overridden in derived classes", null),
        ("protected", "Interface is accessible only from derived classes", null),

        // Enum attributes
        ("flags", "Marks enum as a flags/bitmask enum", null),

        // Attribute meta-attributes
        ("attributeusage", "Specifies where a custom attribute can be applied",
            "attributeusage(${1|target_runtimeclass,target_interface,target_enum,target_struct,target_delegate,target_method,target_property,target_event,target_parameter|})"),
        ("attributename", "Overrides the short name of a custom attribute",
            "attributename(\"${1:ShortName}\")"),
        ("allowmultiple", "Allows attribute to be applied multiple times", null),

        // Member-level attributes
        ("method_name", "Overrides the projected method name",
            "method_name(\"${1:Name}\")"),
        ("overload", "Groups overloaded methods",
            "overload(\"${1:GroupName}\")"),
        ("default_overload", "Marks as the default overload in a group", null),
        ("return_name", "Overrides the return value parameter name",
            "return_name(\"${1:name}\")"),
        ("remote_async", "Marks method for remote async projection", null),
    ];

    #region Import Completions

#if WINDOWS_INTERNAL
    /// <summary>
    /// Detects if the cursor is inside a feature_name body: feature_name Name = {Stage, Bool}
    /// Returns completion items for stage (first position) or bool (after comma), or null if not in context.
    /// </summary>
    private static List<object>? DetectFeatureNameContext(string text, int line, int character)
    {
        var lines = text.Split('\n');
        if (line < 0 || line >= lines.Length) return null;

        var currentLine = lines[line];
        // Find "feature_name" keyword on this line
        int fnIdx = currentLine.IndexOf("feature_name", StringComparison.Ordinal);
        if (fnIdx < 0) return null;

        // Find the opening brace on this line after feature_name
        int braceIdx = currentLine.IndexOf('{', fnIdx);
        if (braceIdx < 0 || character <= braceIdx) return null;

        // Cursor is after '{'. Check if there's a comma between '{' and cursor.
        var insideBrace = currentLine.Substring(braceIdx + 1, character - braceIdx - 1);
        var items = new List<object>();

        if (insideBrace.Contains(','))
        {
            // After comma → bool completions
            AddItem(items, "TRUE", KindKeyword, "Valid in all branches");
            AddItem(items, "FALSE", KindKeyword, "Not valid in all branches");
        }
        else
        {
            // Before comma → stage completions
            AddItem(items, "AlwaysDisabled", KindEnumMember, "Feature is always disabled");
            AddItem(items, "DisabledByDefault", KindEnumMember, "Feature is disabled by default");
            AddItem(items, "EnabledByDefault", KindEnumMember, "Feature is enabled by default");
            AddItem(items, "AlwaysEnabled", KindEnumMember, "Feature is always enabled");
        }

        return items;
    }

    /// <summary>
    /// Detects if the cursor is inside [feature(|)] and returns completion items
    /// for all declared feature_name identifiers in the source.
    /// </summary>
    private static List<object>? DetectFeatureAttributeArgContext(string text, int offset)
    {
        // Walk backward from cursor to find an unmatched '('
        int parenDepth = 0;
        int openParen = -1;
        for (int i = offset - 1; i >= 0; i--)
        {
            char c = text[i];
            if (c == ')') parenDepth++;
            else if (c == '(')
            {
                if (parenDepth > 0) parenDepth--;
                else { openParen = i; break; }
            }
            else if (c == '[' || c == ']') break; // left the attribute bracket
        }
        if (openParen < 0) return null;

        // Extract the attribute name before '('
        int nameEnd = openParen;
        int nameStart = nameEnd - 1;
        while (nameStart >= 0 && (char.IsLetterOrDigit(text[nameStart]) || text[nameStart] == '_'))
            nameStart--;
        nameStart++;

        var attrName = text[nameStart..nameEnd];
        if (!attrName.Equals("feature", StringComparison.OrdinalIgnoreCase))
            return null;

        // Compile the source and collect all feature_name declarations
        var items = new List<object>();
        try
        {
            var compilation = CompilationHelper.Compile(text);
            foreach (var name in compilation.FeatureNames.Keys)
            {
                AddItem(items, name, KindEnumMember, "feature_name declaration");
            }
        }
        catch
        {
            // Partial file — return empty
        }
        return items;
    }
#endif

    /// <summary>
    /// Detects if the cursor is on an import line: import "..."
    /// Returns the partial text typed so far inside the quotes, or "" if just opened.
    /// </summary>
    private static string? DetectImportContext(string text, int line)
    {
        var lines = text.Split('\n');
        if (line < 0 || line >= lines.Length) return null;

        var currentLine = lines[line].TrimStart();
        // Match: import "partial_text  (with or without closing quote)
        if (!currentLine.StartsWith("import ", StringComparison.Ordinal) &&
            !currentLine.StartsWith("import\t", StringComparison.Ordinal))
            return null;

        int quoteStart = currentLine.IndexOf('"');
        if (quoteStart < 0) return null;

        // Extract text after the opening quote
        var afterQuote = currentLine[(quoteStart + 1)..];
        int quoteEnd = afterQuote.IndexOf('"');
        return quoteEnd >= 0 ? afterQuote[..quoteEnd] : afterQuote.TrimEnd('\r');
    }

    /// <summary>Build completions for import "..." — lists .idl files from include paths and project.</summary>
    private static List<object> BuildImportCompletions(string partialPath,
        string? fileDir, IReadOnlyList<string>? includePaths, ProjectFile? projectFile,
        string? currentFileName = null)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var items = new List<object>();

        // Exclude the current file
        if (currentFileName != null)
            seen.Add(currentFileName);

        // Collect all search directories
        var searchDirs = new List<string>();
        if (fileDir != null) searchDirs.Add(fileDir);
        if (includePaths != null) searchDirs.AddRange(includePaths);
        if (projectFile != null) searchDirs.AddRange(projectFile.GetAbsoluteIncludePaths());

        // Also add project input file directories
        if (projectFile != null)
        {
            foreach (var input in projectFile.GetAbsoluteInputPaths())
            {
                var dir = Path.GetDirectoryName(input);
                if (dir != null && !searchDirs.Contains(dir))
                    searchDirs.Add(dir);
            }
        }

        foreach (var dir in searchDirs)
        {
            if (!Directory.Exists(dir)) continue;

            try
            {
                foreach (var file in Directory.EnumerateFiles(dir, "*.idl", SearchOption.TopDirectoryOnly))
                {
                    var name = Path.GetFileName(file);
                    if (!seen.Add(name)) continue;

                    // Filter by partial text typed so far
                    if (partialPath.Length > 0 &&
                        !name.StartsWith(partialPath, StringComparison.OrdinalIgnoreCase) &&
                        !name.Contains(partialPath, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var isInProject = projectFile?.Inputs.Any(i =>
                        Path.GetFileName(i).Equals(name, StringComparison.OrdinalIgnoreCase)) ?? false;

                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = name,
                        ["kind"] = KindFile,
                        ["detail"] = isInProject ? "(project)" : Path.GetDirectoryName(file),
                        ["insertText"] = name,
                        ["filterText"] = name,
                    });
                }
            }
            catch
            {
                // Ignore directory access errors
            }
        }

        // Sort: project files first, then alphabetical
        items.Sort((a, b) =>
        {
            var ad = ((Dictionary<string, object?>)a)["detail"]?.ToString() ?? "";
            var bd = ((Dictionary<string, object?>)b)["detail"]?.ToString() ?? "";
            bool aProj = ad == "(project)";
            bool bProj = bd == "(project)";
            if (aProj != bProj) return aProj ? -1 : 1;
            var al = ((Dictionary<string, object?>)a)["label"]?.ToString() ?? "";
            var bl = ((Dictionary<string, object?>)b)["label"]?.ToString() ?? "";
            return string.Compare(al, bl, StringComparison.OrdinalIgnoreCase);
        });

        return items;
    }

    #endregion

    #region Dot-triggered Completion

    /// <summary>
    /// Extracts the dotted prefix before the cursor, e.g. "Windows.Foundation" from "Windows.Foundation.|".
    /// Returns null if cursor is not immediately after a dot.
    /// </summary>
    private static string? ExtractDotPrefix(string text, int offset)
    {
        if (offset <= 0 || text[offset - 1] != '.') return null;

        // Walk backward from the character before the dot to collect the prefix
        int end = offset - 1; // position of the dot
        int start = end - 1;
        while (start >= 0 && (char.IsLetterOrDigit(text[start]) || text[start] == '_' || text[start] == '.'))
            start--;
        start++;

        if (start >= end) return null;
        var prefix = text[start..end];
        // Must contain at least one valid identifier character
        if (prefix.Length == 0 || prefix.EndsWith(".")) return null;
        return prefix;
    }

    /// <summary>
    /// Build completions for dot-triggered context: child namespaces, types, or enum members.
    /// </summary>
    private static List<object> BuildDotCompletions(string prefix, string text,
        IReadOnlyList<string>? includePaths, string? fileDir)
    {
        var items = new List<object>();

        try
        {
            var compilation = CompilationHelper.Compile(text, referencePaths: null,
                mode: CompilationMode.Strict, includePaths: includePaths, currentFilePath: fileDir);

            // First try: resolve prefix as a namespace in the compilation tree
            var ns = ResolveNamespace(compilation.GlobalNamespace, prefix);
            if (ns != null)
            {
                // Add child namespaces
                foreach (var child in ns.ChildNamespaces.Values.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase))
                {
                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = child.Name,
                        ["kind"] = KindModule,
                        ["detail"] = child.FullName,
                        ["insertText"] = child.Name,
                    });
                }

                // Add types in this namespace
                foreach (var member in ns.Members.Where(m => m.Kind != SymbolKind.Namespace)
                    .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase))
                {
                    var kind = member.Kind switch
                    {
                        SymbolKind.Interface => KindInterface,
                        SymbolKind.RuntimeClass => KindClass,
                        SymbolKind.Enum => KindEnum,
                        SymbolKind.Struct => KindStruct,
                        SymbolKind.Delegate => KindClass,
                        _ => KindClass,
                    };
                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = member.Name,
                        ["kind"] = kind,
                        ["detail"] = member.FullName,
                        ["insertText"] = member.Name,
                    });
                }

                // Also check lazy providers for types in this namespace
                AddLazyProviderNamespaceMembers(items, compilation, prefix);

                return items;
            }

            // Second try: resolve as a type name (e.g. "MyEnum" → show enum members)
            var typeSymbol = ResolveTypeByShortOrFullName(compilation, prefix);
            if (typeSymbol is EnumSymbol enumSym)
            {
                foreach (var member in enumSym.EnumMembers)
                {
                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = member.Name,
                        ["kind"] = KindEnumMember,
                        ["detail"] = $"{enumSym.FullName}.{member.Name}",
                        ["insertText"] = member.Name,
                    });
                }
                return items;
            }

            // Third try: prefix might be a namespace only known to lazy providers
            AddLazyProviderNamespaceMembers(items, compilation, prefix);
        }
        catch
        {
            // Partial file — return empty
        }

        return items;
    }

    private static NamespaceSymbol? ResolveNamespace(NamespaceSymbol root, string dottedName)
    {
        var parts = dottedName.Split('.');
        var current = root;
        foreach (var part in parts)
        {
            if (!current.ChildNamespaces.TryGetValue(part, out var child))
                return null;
            current = child;
        }
        return current;
    }

    private static ISymbol? ResolveTypeByShortOrFullName(Compilation compilation, string name)
    {
        // Try fully qualified first
        var symbol = compilation.GetTypeByName(name);
        if (symbol != null) return symbol;

        // Try short name match in all types
        foreach (var t in compilation.GetAllTypes())
        {
            if (t.Name.Equals(name, StringComparison.Ordinal))
                return t;
        }
        return null;
    }

    private static void AddLazyProviderNamespaceMembers(List<object> items, Compilation compilation, string ns)
    {
        // Use reflection-free approach: the compilation exposes lazy providers indirectly
        // through GetTypesInNamespace-like queries by checking all type names
        var existingLabels = new HashSet<string>(
            items.Select(i => ((Dictionary<string, object?>)i)["label"]?.ToString() ?? ""),
            StringComparer.Ordinal);

        // Walk all types looking for ones in this namespace (lazy providers are queried through compilation)
        // We trigger lazy resolution by looking at the namespace prefix
        try
        {
            // Try resolving a dummy type to trigger namespace population from providers
            compilation.GetTypeByName(ns + ".__probe__");
        }
        catch { }

        // Re-check the namespace tree after probing
        var resolvedNs = ResolveNamespace(compilation.GlobalNamespace, ns);
        if (resolvedNs != null)
        {
            foreach (var child in resolvedNs.ChildNamespaces.Values)
            {
                if (existingLabels.Add(child.Name))
                {
                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = child.Name,
                        ["kind"] = KindModule,
                        ["detail"] = child.FullName,
                        ["insertText"] = child.Name,
                    });
                }
            }
            foreach (var member in resolvedNs.Members.Where(m => m.Kind != SymbolKind.Namespace))
            {
                if (existingLabels.Add(member.Name))
                {
                    var kind = member.Kind switch
                    {
                        SymbolKind.Interface => KindInterface,
                        SymbolKind.RuntimeClass => KindClass,
                        SymbolKind.Enum => KindEnum,
                        SymbolKind.Struct => KindStruct,
                        SymbolKind.Delegate => KindClass,
                        _ => KindClass,
                    };
                    items.Add(new Dictionary<string, object?>
                    {
                        ["label"] = member.Name,
                        ["kind"] = kind,
                        ["detail"] = member.FullName,
                        ["insertText"] = member.Name,
                    });
                }
            }
        }
    }

    #endregion

    #region @ref Directive Completion

    /// <summary>
    /// Detects if the cursor is on a @ref directive line: // @ref: path
    /// Returns the partial path typed so far, or "" if just after "// @ref: ".
    /// </summary>
    private static string? DetectRefDirectiveContext(string text, int line, int character)
    {
        var lines = text.Split('\n');
        if (line < 0 || line >= lines.Length) return null;

        var currentLine = lines[line].TrimEnd('\r');
        // Match: // @ref: partial_path (with or without quotes)
        var trimmed = currentLine.TrimStart();
        if (!trimmed.StartsWith("// @ref:", StringComparison.Ordinal) &&
            !trimmed.StartsWith("//@ref:", StringComparison.Ordinal))
            return null;

        int colonIdx = currentLine.IndexOf(':', currentLine.IndexOf("@ref", StringComparison.Ordinal));
        if (colonIdx < 0) return null;

        var afterColon = currentLine[(colonIdx + 1)..];
        // Strip leading whitespace after colon
        var pathPart = afterColon.TrimStart();
        int trimmedChars = afterColon.Length - pathPart.Length;

        // Handle quoted path
        if (pathPart.StartsWith('"'))
        {
            pathPart = pathPart[1..];
            int endQuote = pathPart.IndexOf('"');
            if (endQuote >= 0)
                pathPart = pathPart[..endQuote];
        }

        // Only return if cursor is positioned within the path area
        int pathStartInLine = colonIdx + 1 + trimmedChars;
        if (pathPart.StartsWith('"')) pathStartInLine++;
        if (character < pathStartInLine) return null;

        // Trim to what the user has typed up to cursor
        int charsTyped = character - pathStartInLine;
        if (charsTyped < 0) charsTyped = 0;
        if (charsTyped < pathPart.Length)
            pathPart = pathPart[..charsTyped];

        return pathPart;
    }

    /// <summary>Build completions for // @ref: — lists .idl and .winmd files + directories.</summary>
    private static List<object> BuildRefDirectiveCompletions(string partialPath, string? fileDir)
    {
        if (fileDir == null) return new List<object>();

        var files = new List<object>();
        var folders = new List<object>();

        // Determine the base directory and filename filter
        string searchDir;
        string filter;
        if (partialPath.Contains('/') || partialPath.Contains('\\'))
        {
            var lastSep = Math.Max(partialPath.LastIndexOf('/'), partialPath.LastIndexOf('\\'));
            var subPath = partialPath[..(lastSep + 1)];
            filter = partialPath[(lastSep + 1)..];
            searchDir = Path.Combine(fileDir, subPath.Replace('/', Path.DirectorySeparatorChar));
        }
        else
        {
            searchDir = fileDir;
            filter = partialPath;
        }

        if (!Directory.Exists(searchDir)) return new List<object>();

        try
        {
            // Enumerate .idl and .winmd files
            foreach (var file in Directory.EnumerateFiles(searchDir))
            {
                var name = Path.GetFileName(file);
                var ext = Path.GetExtension(name);
                if (!ext.Equals(".idl", StringComparison.OrdinalIgnoreCase) &&
                    !ext.Equals(".winmd", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (filter.Length > 0 &&
                    !name.StartsWith(filter, StringComparison.OrdinalIgnoreCase))
                    continue;

                var kind = ext.Equals(".winmd", StringComparison.OrdinalIgnoreCase) ? KindModule : KindFile;
                files.Add(new Dictionary<string, object?>
                {
                    ["label"] = name,
                    ["kind"] = kind,
                    ["detail"] = ext.Equals(".winmd", StringComparison.OrdinalIgnoreCase) ? "WinMD reference" : "IDL file",
                    ["insertText"] = name,
                    ["filterText"] = name,
                });
            }

            // Enumerate subdirectories
            foreach (var dir in Directory.EnumerateDirectories(searchDir))
            {
                var name = Path.GetFileName(dir);
                if (name.StartsWith('.')) continue;

                if (filter.Length > 0 &&
                    !name.StartsWith(filter, StringComparison.OrdinalIgnoreCase))
                    continue;

                folders.Add(new Dictionary<string, object?>
                {
                    ["label"] = name,
                    ["kind"] = KindFolder,
                    ["detail"] = "Directory",
                    ["insertText"] = name + "/",
                    ["filterText"] = name,
                });
            }
        }
        catch
        {
            // Ignore directory access errors
        }

        // Files first (alphabetical), then folders (alphabetical)
        files.Sort((a, b) =>
        {
            var al = ((Dictionary<string, object?>)a)["label"]?.ToString() ?? "";
            var bl = ((Dictionary<string, object?>)b)["label"]?.ToString() ?? "";
            return string.Compare(al, bl, StringComparison.OrdinalIgnoreCase);
        });
        folders.Sort((a, b) =>
        {
            var al = ((Dictionary<string, object?>)a)["label"]?.ToString() ?? "";
            var bl = ((Dictionary<string, object?>)b)["label"]?.ToString() ?? "";
            return string.Compare(al, bl, StringComparison.OrdinalIgnoreCase);
        });

        var result = new List<object>(files.Count + folders.Count);
        result.AddRange(files);
        result.AddRange(folders);
        return result;
    }

    #endregion

    private static int GetOffset(string text, int line, int character)
    {
        int offset = 0;
        int currentLine = 0;
        while (currentLine < line && offset < text.Length)
        {
            if (text[offset] == '\n') currentLine++;
            offset++;
        }
        return Math.Min(offset + character, text.Length);
    }

    #region Contextual Completion

    private enum CompletionContext
    {
        TopLevel,           // Namespace body or file top-level → declaration keywords
        StructBody,         // Inside struct { } → value types only (primitives, enums, structs)
        EnumBody,           // Inside enum { } → no type completions, just identifiers
        InterfaceBody,      // Inside interface { } → types + void + event
        RuntimeClassBody,   // Inside runtimeclass { } → types + void + event + modifiers + constructor
        AttributeBody,      // Inside attribute { } → value types for fields
        TypePosition,       // Expecting a type (param, return, field) → all types
        BaseList,           // After : → interfaces, classes
        RequiresList,       // After requires → interfaces only
        ParameterType,      // Cursor inside ( ) for a parameter type
        EventType,          // After "event" keyword, expecting delegate type
    }

    private static CompletionContext DetectCompletionContext(string text, int offset, int line)
    {
        // First check if we're on a requires clause or base list (before the opening brace)
        // Scan backward from cursor for requires keyword or : token on the same "declaration line"
        for (int i = offset - 1; i >= 0; i--)
        {
            char c = text[i];
            if (c == '\n') break; // Only check current line
            if (c == '{' || c == '}') break; // Hit a brace boundary
        }

        // Check the text between the last declaration keyword and cursor
        // for requires or : that would indicate a base/requires list context
        var lineText = GetLineText(text, line).TrimStart();
        if (lineText.StartsWith("requires ", StringComparison.Ordinal) ||
            lineText.StartsWith("requires\t", StringComparison.Ordinal))
            return CompletionContext.RequiresList;

        // Check if cursor follows a : in a base list context (e.g. "runtimeclass Foo : ")
        // or is after a comma in a base list
        var beforeCursor = text.AsSpan(0, offset);
        int lastBrace = beforeCursor.LastIndexOf('{');
        int lastCloseBrace = beforeCursor.LastIndexOf('}');
        int effectiveStart = Math.Max(lastBrace, lastCloseBrace) + 1;
        var segment = text.AsSpan(effectiveStart, offset - effectiveStart);
        if (segment.Contains("requires", StringComparison.Ordinal))
        {
            // We're after a requires keyword, before the opening brace
            int reqIdx = segment.LastIndexOf("requires".AsSpan(), StringComparison.Ordinal);
            var afterReq = segment[(reqIdx + 8)..].TrimStart();
            // If there's no '{' after requires, we're in the requires list
            if (!afterReq.Contains('{'))
                return CompletionContext.RequiresList;
        }
        // Check for base list: "runtimeclass Foo :" or "interface IFoo :"  
        if (IsInBaseList(segment))
            return CompletionContext.BaseList;

        // Walk backward to find enclosing scope
        int braceDepth = 0;
        for (int i = offset - 1; i >= 0; i--)
        {
            char c = text[i];
            if (c == '}') { braceDepth++; continue; }
            if (c == '{')
            {
                if (braceDepth > 0) { braceDepth--; continue; }

                // Narrow search to the declaration text between the previous boundary and this brace
                int scopeStart = 0;
                for (int j = i - 1; j >= 0; j--)
                {
                    if (text[j] == '{' || text[j] == '}' || text[j] == ';')
                    {
                        scopeStart = j + 1;
                        break;
                    }
                }
                var declText = text.AsSpan(scopeStart, i - scopeStart);

                if (ContainsWordBefore(declText, "attribute")) return CompletionContext.AttributeBody;
                if (ContainsWordBefore(declText, "struct")) return CompletionContext.StructBody;
                if (ContainsWordBefore(declText, "enum")) return CompletionContext.EnumBody;
                if (ContainsWordBefore(declText, "interface"))
                    return DetectBodySubContext(text, offset, lineText, CompletionContext.InterfaceBody);
                if (ContainsWordBefore(declText, "runtimeclass"))
                    return DetectBodySubContext(text, offset, lineText, CompletionContext.RuntimeClassBody);
                if (ContainsWordBefore(declText, "namespace")) return CompletionContext.TopLevel;

                // Could be inside a property accessor { get; set; } — keep scanning up
                continue;
            }
        }

        return CompletionContext.TopLevel;
    }

    /// <summary>
    /// Refines an interface/runtimeclass body context into a more specific context
    /// (ParameterType, EventType) based on the cursor's position within the body.
    /// </summary>
    private static CompletionContext DetectBodySubContext(string text, int offset, string lineText, CompletionContext bodyContext)
    {
        var trimmed = lineText.TrimStart();

        // After "event " keyword → expecting delegate type
        if (trimmed.StartsWith("event ", StringComparison.Ordinal) ||
            trimmed.StartsWith("event\t", StringComparison.Ordinal))
        {
            // Only if cursor is after the "event " keyword
            int eventPos = lineText.IndexOf("event", StringComparison.Ordinal);
            int lineStart = GetLineStartOffset(text, offset);
            int cursorInLine = offset - lineStart;
            if (eventPos >= 0 && cursorInLine > eventPos + 5)
                return CompletionContext.EventType;
        }

        // Check if cursor is inside parentheses (parameter position)
        // Walk backward from cursor to find unmatched '('
        int parenDepth = 0;
        for (int i = offset - 1; i >= 0; i--)
        {
            char c = text[i];
            if (c == ')') { parenDepth++; continue; }
            if (c == '(')
            {
                if (parenDepth > 0) { parenDepth--; continue; }
                // Found unmatched '(' — we're inside a parameter list
                return CompletionContext.ParameterType;
            }
            // Stop at statement/block boundaries
            if (c == '{' || c == '}' || c == ';') break;
        }

        return bodyContext;
    }

    private static int GetLineStartOffset(string text, int offset)
    {
        for (int i = offset - 1; i >= 0; i--)
        {
            if (text[i] == '\n') return i + 1;
        }
        return 0;
    }

    private static bool IsInBaseList(ReadOnlySpan<char> segment)
    {
        // Look for pattern: keyword Name : ... (with no { yet)
        // The colon must come after a type declaration keyword + name
        // Also handles comma-separated base lists (e.g. "runtimeclass Foo : IBar, ")
        bool foundColon = false;
        for (int i = segment.Length - 1; i >= 0; i--)
        {
            if (segment[i] == ':') { foundColon = true; break; }
            if (segment[i] == '{' || segment[i] == '}' || segment[i] == ';') break;
        }
        if (!foundColon) return false;

        // Verify a declaration keyword precedes the colon
        return ContainsWordBefore(segment, "runtimeclass") ||
               ContainsWordBefore(segment, "interface");
    }

    private static string GetLineText(string text, int line)
    {
        var lines = text.Split('\n');
        return line >= 0 && line < lines.Length ? lines[line] : "";
    }

    private static List<object> BuildContextualCompletions(CompletionContext context, string text, string uri,
        IReadOnlyList<string>? includePaths = null, string? currentFilePath = null)
    {
        var items = new List<object>();

        switch (context)
        {
            case CompletionContext.StructBody:
                // Structs: only value types (primitives, enums, other structs, String, IReference<T>)
                AddValueTypes(items);
                AddItem(items, "}", KindKeyword, "Close struct");
                AddUserTypes(items, text, t => t is StructSymbol or EnumSymbol,
                    includePaths, currentFilePath);
                break;

            case CompletionContext.AttributeBody:
                // Attribute fields: value types + String + enums (same as struct fields)
                AddValueTypes(items);
                AddItem(items, "}", KindKeyword, "Close attribute");
                AddUserTypes(items, text, t => t is EnumSymbol,
                    includePaths, currentFilePath);
                break;

            case CompletionContext.EnumBody:
                // Enum body: no type completions needed (just member names)
                AddItem(items, "}", KindKeyword, "Close enum");
                break;

            case CompletionContext.InterfaceBody:
                AddAllTypes(items);
                AddItem(items, "void", KindKeyword, "No return value");
                AddItem(items, "event", KindEvent, "Declares an event");
                AddItem(items, "}", KindKeyword, "Close interface");
                AddItem(items, "requires", KindKeyword, "Requires clause");
                AddUserTypes(items, text, includePaths: includePaths, currentFilePath: currentFilePath);
                break;

            case CompletionContext.RuntimeClassBody:
                AddAllTypes(items);
                AddItem(items, "void", KindKeyword, "No return value");
                AddItem(items, "event", KindEvent, "Declares an event");
                AddItem(items, "static", KindKeyword, "Marks member as static");
                AddItem(items, "protected", KindKeyword, "Restricts to derived classes");
                AddItem(items, "overridable", KindKeyword, "Allows override in derived classes");
                AddItem(items, "}", KindKeyword, "Close runtimeclass");
                AddConstructorName(items, text);
                AddUserTypes(items, text, includePaths: includePaths, currentFilePath: currentFilePath);
                break;

            case CompletionContext.BaseList:
                // After : in runtimeclass/interface declaration → interfaces and classes
                AddUserTypes(items, text, t => t is InterfaceSymbol or RuntimeClassSymbol,
                    includePaths, currentFilePath);
                AddItem(items, "IStringable", KindInterface, "Windows.Foundation.IStringable");
                AddItem(items, "IClosable", KindInterface, "Windows.Foundation.IClosable");
                break;

            case CompletionContext.RequiresList:
                // After requires → only interfaces
                AddUserTypes(items, text, t => t is InterfaceSymbol,
                    includePaths, currentFilePath);
                AddItem(items, "IStringable", KindInterface, "Windows.Foundation.IStringable");
                AddItem(items, "IClosable", KindInterface, "Windows.Foundation.IClosable");
                break;

            case CompletionContext.ParameterType:
                // All types are valid as parameter types
                AddAllTypes(items);
                AddItem(items, "out", KindKeyword, "Output parameter");
                AddItem(items, "ref", KindKeyword, "Reference parameter");
                AddUserTypes(items, text, includePaths: includePaths, currentFilePath: currentFilePath);
                break;

            case CompletionContext.EventType:
                // Only delegates + EventHandler/TypedEventHandler
                AddItem(items, "EventHandler", KindClass, "Windows.Foundation.EventHandler<T>");
                AddItem(items, "TypedEventHandler", KindClass, "Windows.Foundation.TypedEventHandler<S,A>");
                AddUserTypes(items, text, t => t is DelegateSymbol,
                    includePaths, currentFilePath);
                break;

            case CompletionContext.TopLevel:
            default:
                // Top-level: declaration keywords + import
                AddItem(items, "namespace", KindKeyword, "Declares a namespace scope");
                AddItem(items, "runtimeclass", KindClass, "Declares a Windows Runtime class");
                AddItem(items, "interface", KindInterface, "Declares a Windows Runtime interface");
                AddItem(items, "delegate", KindKeyword, "Declares a delegate type");
                AddItem(items, "struct", KindStruct, "Declares a value type struct");
                AddItem(items, "enum", KindEnum, "Declares an enumeration");
                AddItem(items, "apicontract", KindKeyword, "Declares an API contract");
                AddItem(items, "attribute", KindKeyword, "Declares a custom attribute type");
                AddItem(items, "import", KindKeyword, "Imports an IDL file");
                AddItem(items, "static", KindKeyword, "Static runtimeclass modifier");
                AddItem(items, "partial", KindKeyword, "Partial runtimeclass modifier");
                AddItem(items, "unsealed", KindKeyword, "Unsealed runtimeclass modifier");
#if WINDOWS_INTERNAL
                AddItem(items, "feature_name", KindKeyword, "Velocity feature staging declaration");
#endif
                AddItem(items, "}", KindKeyword, "Close scope");
                break;
        }

        return items;
    }

    /// <summary>Add WinRT value types (valid in struct fields).</summary>
    private static void AddValueTypes(List<object> items)
    {
        AddItem(items, "Boolean", KindTypeParameter, "8-bit Boolean");
        AddItem(items, "Char16", KindTypeParameter, "16-bit UTF-16 code unit");
        AddItem(items, "Int16", KindTypeParameter, "16-bit signed integer");
        AddItem(items, "Int32", KindTypeParameter, "32-bit signed integer");
        AddItem(items, "Int64", KindTypeParameter, "64-bit signed integer");
        AddItem(items, "UInt8", KindTypeParameter, "8-bit unsigned integer");
        AddItem(items, "UInt16", KindTypeParameter, "16-bit unsigned integer");
        AddItem(items, "UInt32", KindTypeParameter, "32-bit unsigned integer");
        AddItem(items, "UInt64", KindTypeParameter, "64-bit unsigned integer");
        AddItem(items, "Single", KindTypeParameter, "32-bit float");
        AddItem(items, "Double", KindTypeParameter, "64-bit float");
        AddItem(items, "String", KindTypeParameter, "UTF-16 string");
        AddItem(items, "Guid", KindTypeParameter, "128-bit GUID");
    }

    /// <summary>Add all WinRT types (primitives + reference types).</summary>
    private static void AddAllTypes(List<object> items)
    {
        AddValueTypes(items);
        AddItem(items, "Object", KindTypeParameter, "Base type for all WinRT types");
        // Common parameterized types
        AddItem(items, "IVector", KindInterface, "Windows.Foundation.Collections.IVector<T>");
        AddItem(items, "IVectorView", KindInterface, "Windows.Foundation.Collections.IVectorView<T>");
        AddItem(items, "IMap", KindInterface, "Windows.Foundation.Collections.IMap<K,V>");
        AddItem(items, "IMapView", KindInterface, "Windows.Foundation.Collections.IMapView<K,V>");
        AddItem(items, "IIterable", KindInterface, "Windows.Foundation.Collections.IIterable<T>");
        AddItem(items, "IAsyncOperation", KindInterface, "Windows.Foundation.IAsyncOperation<T>");
        AddItem(items, "IAsyncAction", KindInterface, "Windows.Foundation.IAsyncAction");
        AddItem(items, "IReference", KindInterface, "Windows.Foundation.IReference<T>");
        AddItem(items, "EventHandler", KindClass, "Windows.Foundation.EventHandler<T>");
        AddItem(items, "TypedEventHandler", KindClass, "Windows.Foundation.TypedEventHandler<S,A>");
    }

    /// <summary>Add user-defined types from the compilation.</summary>
    private static void AddUserTypes(List<object> items, string text,
        Func<ISymbol, bool>? filter = null,
        IReadOnlyList<string>? includePaths = null, string? currentFilePath = null)
    {
        try
        {
            var compilation = CompilationHelper.Compile(text, referencePaths: null,
                mode: CompilationMode.Strict, includePaths: includePaths, currentFilePath: currentFilePath);
            foreach (var type in compilation.GetAllTypes())
            {
                if (string.IsNullOrEmpty(type.Name)) continue;
                if (filter != null && !filter(type)) continue;

                var kind = type.Kind switch
                {
                    Analysis.Symbols.SymbolKind.Interface => KindInterface,
                    Analysis.Symbols.SymbolKind.RuntimeClass => KindClass,
                    Analysis.Symbols.SymbolKind.Enum => KindEnum,
                    Analysis.Symbols.SymbolKind.Struct => KindStruct,
                    Analysis.Symbols.SymbolKind.Delegate => KindClass,
                    _ => KindClass,
                };
                var detail = type.IsExternal ? $"(external) {type.FullName}" : type.FullName;
                items.Add(new Dictionary<string, object?>
                {
                    ["label"] = type.Name,
                    ["kind"] = kind,
                    ["detail"] = detail,
                    ["insertText"] = type.IsExternal ? type.FullName : type.Name,
                });
            }
        }
        catch { /* partial file — skip user types */ }
    }

    /// <summary>Suggest the enclosing runtimeclass name as a constructor.</summary>
    private static void AddConstructorName(List<object> items, string text)
    {
        // Scan for the runtimeclass name
        int idx = text.LastIndexOf("runtimeclass ", StringComparison.Ordinal);
        if (idx < 0) return;

        int nameStart = idx + "runtimeclass ".Length;
        int nameEnd = nameStart;
        while (nameEnd < text.Length && (char.IsLetterOrDigit(text[nameEnd]) || text[nameEnd] == '_'))
            nameEnd++;

        if (nameEnd > nameStart)
        {
            var name = text[nameStart..nameEnd];
            items.Insert(0, new Dictionary<string, object?>
            {
                ["label"] = name,
                ["kind"] = KindConstructor,
                ["detail"] = "Constructor",
                ["insertText"] = name + "(${1});",
                ["insertTextFormat"] = 2,
            });
        }
    }

    #endregion

    private static object[] BuildCompletionItems()
    {
        var items = new List<object>();

        // Declaration keywords
        AddItem(items, "namespace", KindKeyword, "Declares a namespace scope");
        AddItem(items, "runtimeclass", KindClass, "Declares a Windows Runtime class");
        AddItem(items, "interface", KindInterface, "Declares a Windows Runtime interface");
        AddItem(items, "delegate", KindKeyword, "Declares a delegate type");
        AddItem(items, "struct", KindStruct, "Declares a value type struct");
        AddItem(items, "enum", KindEnum, "Declares an enumeration");
        AddItem(items, "event", KindEvent, "Declares an event");
        AddItem(items, "apicontract", KindKeyword, "Declares an API contract for versioning");
        AddItem(items, "import", KindKeyword, "Imports an IDL file");

        // Modifier keywords
        AddItem(items, "static", KindKeyword, "Marks a class or member as static");
        AddItem(items, "partial", KindKeyword, "Allows a class to be split across declarations");
        AddItem(items, "unsealed", KindKeyword, "Allows a runtime class to be subclassed");
        AddItem(items, "protected", KindKeyword, "Restricts access to derived classes");
        AddItem(items, "overridable", KindKeyword, "Allows a method to be overridden in derived classes");

        // Built-in type keywords
        AddItem(items, "void", KindKeyword, "No return value");
        AddItem(items, "Boolean", KindTypeParameter, "Boolean value (true/false)");
        AddItem(items, "Int16", KindTypeParameter, "16-bit signed integer");
        AddItem(items, "Int32", KindTypeParameter, "32-bit signed integer");
        AddItem(items, "Int64", KindTypeParameter, "64-bit signed integer");
        AddItem(items, "UInt8", KindTypeParameter, "8-bit unsigned integer (Byte)");
        AddItem(items, "UInt16", KindTypeParameter, "16-bit unsigned integer");
        AddItem(items, "UInt32", KindTypeParameter, "32-bit unsigned integer");
        AddItem(items, "UInt64", KindTypeParameter, "64-bit unsigned integer");
        AddItem(items, "Single", KindTypeParameter, "32-bit floating point");
        AddItem(items, "Double", KindTypeParameter, "64-bit floating point");
        AddItem(items, "String", KindTypeParameter, "UTF-16 string");
        AddItem(items, "Char16", KindTypeParameter, "16-bit UTF-16 code unit");
        AddItem(items, "Char", KindTypeParameter, "Alias for Char16");
        AddItem(items, "Guid", KindTypeParameter, "128-bit globally unique identifier");
        AddItem(items, "Object", KindTypeParameter, "Base type for all WinRT types");

        // Property accessors
        AddItem(items, "get", KindKeyword, "Property getter accessor");
        AddItem(items, "set", KindKeyword, "Property setter accessor");

        // Parameter modifiers
        AddItem(items, "out", KindKeyword, "Output parameter");
        AddItem(items, "ref", KindKeyword, "Reference parameter");

        // Common WinRT collection types
        AddItem(items, "IVector", KindInterface, "Windows.Foundation.Collections.IVector<T>");
        AddItem(items, "IVectorView", KindInterface, "Windows.Foundation.Collections.IVectorView<T>");
        AddItem(items, "IMap", KindInterface, "Windows.Foundation.Collections.IMap<K,V>");
        AddItem(items, "IMapView", KindInterface, "Windows.Foundation.Collections.IMapView<K,V>");
        AddItem(items, "IIterable", KindInterface, "Windows.Foundation.Collections.IIterable<T>");
        AddItem(items, "IAsyncOperation", KindInterface, "Windows.Foundation.IAsyncOperation<T>");
        AddItem(items, "IAsyncAction", KindInterface, "Windows.Foundation.IAsyncAction");
        AddItem(items, "EventHandler", KindClass, "Windows.Foundation.EventHandler<T>");
        AddItem(items, "TypedEventHandler", KindClass, "Windows.Foundation.TypedEventHandler<S,A>");

        return items.ToArray();
    }

    private static void AddItem(List<object> items, string label, int kind, string detail)
    {
        items.Add(new Dictionary<string, object?>
        {
            ["label"] = label,
            ["kind"] = kind,
            ["detail"] = detail,
            ["insertText"] = label
        });
    }
}
