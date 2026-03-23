using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides attribute completions when the cursor is inside [...].
/// </summary>
internal static class AttributeCompletionProvider
{
    /// <summary>
    /// Detects if the cursor is inside an attribute bracket [...].
    /// Returns the target kind string (e.g. "target_runtimeclass") or null if not in attribute context.
    /// </summary>
    internal static string? DetectAttributeContext(string text, int offset)
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

        if (CompletionHandler.StartsWithWord(remaining, "runtimeclass")) return "target_runtimeclass";
        if (CompletionHandler.StartsWithWord(remaining, "interface")) return "target_interface";
        if (CompletionHandler.StartsWithWord(remaining, "enum")) return "target_enum";
        if (CompletionHandler.StartsWithWord(remaining, "struct")) return "target_struct";
        if (CompletionHandler.StartsWithWord(remaining, "delegate")) return "target_delegate";
        if (CompletionHandler.StartsWithWord(remaining, "attribute")) return "target_attribute";
        if (CompletionHandler.StartsWithWord(remaining, "apicontract")) return "target_apicontract";

        // Inside a type body: check if we're inside a class/interface body for member-level attrs
        // Also handle modifiers before the keyword
        if (CompletionHandler.StartsWithWord(remaining, "static") || CompletionHandler.StartsWithWord(remaining, "partial") ||
            CompletionHandler.StartsWithWord(remaining, "unsealed"))
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
                if (CompletionHandler.ContainsWordBefore(before, "runtimeclass")) return "target_method"; // members
                if (CompletionHandler.ContainsWordBefore(before, "interface")) return "target_method";
                if (CompletionHandler.ContainsWordBefore(before, "namespace")) return "target_all"; // any type attribute
                break;
            }
        }
        return "target_all"; // default: show everything
    }

    /// <summary>Build context-appropriate attribute completions.</summary>
    internal static List<object> BuildAttributeCompletions(string text, string targetKind,
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
                    ["kind"] = CompletionHandler.KindProperty,
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
                            ["kind"] = CompletionHandler.KindProperty,
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
    internal static readonly (string Name, string Detail, string? Snippet)[] WellKnownAttributeCompletions =
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
}
