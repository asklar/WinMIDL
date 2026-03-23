using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides context-aware completions based on cursor position (body, base list, requires, etc.).
/// </summary>
internal static class ContextualCompletionProvider
{
    internal enum CompletionContext
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

    internal static CompletionContext DetectCompletionContext(string text, int offset, int line)
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
        var lineText = CompletionHandler.GetLineText(text, line).TrimStart();
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

                if (CompletionHandler.ContainsWordBefore(declText, "attribute")) return CompletionContext.AttributeBody;
                if (CompletionHandler.ContainsWordBefore(declText, "struct")) return CompletionContext.StructBody;
                if (CompletionHandler.ContainsWordBefore(declText, "enum")) return CompletionContext.EnumBody;
                if (CompletionHandler.ContainsWordBefore(declText, "interface"))
                    return DetectBodySubContext(text, offset, lineText, CompletionContext.InterfaceBody);
                if (CompletionHandler.ContainsWordBefore(declText, "runtimeclass"))
                    return DetectBodySubContext(text, offset, lineText, CompletionContext.RuntimeClassBody);
                if (CompletionHandler.ContainsWordBefore(declText, "namespace")) return CompletionContext.TopLevel;

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
            int lineStart = CompletionHandler.GetLineStartOffset(text, offset);
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
        return CompletionHandler.ContainsWordBefore(segment, "runtimeclass") ||
               CompletionHandler.ContainsWordBefore(segment, "interface");
    }

    internal static List<object> BuildContextualCompletions(CompletionContext context, string text, string uri,
        IReadOnlyList<string>? includePaths = null, string? currentFilePath = null,
        Compilation? cachedCompilation = null)
    {
        var items = new List<object>();

        // Fall back to ad-hoc compile if no cached compilation (e.g. in tests)
        var compilation = cachedCompilation;
        if (compilation == null)
        {
            try { compilation = CompilationHelper.Compile(text, referencePaths: null, mode: CompilationMode.Strict, includePaths: includePaths, currentFilePath: currentFilePath); }
            catch { /* partial file — skip user types */ }
        }

        switch (context)
        {
            case CompletionContext.StructBody:
                // Structs: only value types (primitives, enums, other structs, String, IReference<T>)
                AddValueTypes(items);
                CompletionHandler.AddItem(items, "}", CompletionHandler.KindKeyword, "Close struct");
                AddUserTypes(items, compilation, t => t is StructSymbol or EnumSymbol);
                break;

            case CompletionContext.AttributeBody:
                // Attribute fields: value types + String + enums (same as struct fields)
                AddValueTypes(items);
                CompletionHandler.AddItem(items, "}", CompletionHandler.KindKeyword, "Close attribute");
                AddUserTypes(items, compilation, t => t is EnumSymbol);
                break;

            case CompletionContext.EnumBody:
                // Enum body: no type completions needed (just member names)
                CompletionHandler.AddItem(items, "}", CompletionHandler.KindKeyword, "Close enum");
                break;

            case CompletionContext.InterfaceBody:
                AddAllTypes(items);
                CompletionHandler.AddItem(items, "void", CompletionHandler.KindKeyword, "No return value");
                CompletionHandler.AddItem(items, "event", CompletionHandler.KindEvent, "Declares an event");
                CompletionHandler.AddItem(items, "}", CompletionHandler.KindKeyword, "Close interface");
                CompletionHandler.AddItem(items, "requires", CompletionHandler.KindKeyword, "Requires clause");
                AddUserTypes(items, compilation);
                break;

            case CompletionContext.RuntimeClassBody:
                AddAllTypes(items);
                CompletionHandler.AddItem(items, "void", CompletionHandler.KindKeyword, "No return value");
                CompletionHandler.AddItem(items, "event", CompletionHandler.KindEvent, "Declares an event");
                CompletionHandler.AddItem(items, "static", CompletionHandler.KindKeyword, "Marks member as static");
                CompletionHandler.AddItem(items, "protected", CompletionHandler.KindKeyword, "Restricts to derived classes");
                CompletionHandler.AddItem(items, "overridable", CompletionHandler.KindKeyword, "Allows override in derived classes");
                CompletionHandler.AddItem(items, "}", CompletionHandler.KindKeyword, "Close runtimeclass");
                AddConstructorName(items, text);
                AddUserTypes(items, compilation);
                break;

            case CompletionContext.BaseList:
                // After : in runtimeclass/interface declaration → interfaces and classes
                AddUserTypes(items, compilation, t => t is InterfaceSymbol or RuntimeClassSymbol);
                CompletionHandler.AddItem(items, "IStringable", CompletionHandler.KindInterface, "Windows.Foundation.IStringable");
                CompletionHandler.AddItem(items, "IClosable", CompletionHandler.KindInterface, "Windows.Foundation.IClosable");
                break;

            case CompletionContext.RequiresList:
                // After requires → only interfaces
                AddUserTypes(items, compilation, t => t is InterfaceSymbol);
                CompletionHandler.AddItem(items, "IStringable", CompletionHandler.KindInterface, "Windows.Foundation.IStringable");
                CompletionHandler.AddItem(items, "IClosable", CompletionHandler.KindInterface, "Windows.Foundation.IClosable");
                break;

            case CompletionContext.ParameterType:
                // All types are valid as parameter/return types
                AddAllTypes(items);
                CompletionHandler.AddItem(items, "out", CompletionHandler.KindKeyword, "Output parameter");
                CompletionHandler.AddItem(items, "ref", CompletionHandler.KindKeyword, "Reference parameter");
                AddUserTypes(items, compilation);
                break;

            case CompletionContext.EventType:
                // Only delegates + EventHandler/TypedEventHandler
                CompletionHandler.AddItem(items, "EventHandler", CompletionHandler.KindClass, "Windows.Foundation.EventHandler<T>");
                CompletionHandler.AddItem(items, "TypedEventHandler", CompletionHandler.KindClass, "Windows.Foundation.TypedEventHandler<S,A>");
                AddUserTypes(items, compilation, t => t is DelegateSymbol);
                break;

            case CompletionContext.TopLevel:
            default:
                // Top-level: declaration keywords + import
                CompletionHandler.AddItem(items, "namespace", CompletionHandler.KindKeyword, "Declares a namespace scope");
                CompletionHandler.AddItem(items, "runtimeclass", CompletionHandler.KindClass, "Declares a Windows Runtime class");
                CompletionHandler.AddItem(items, "interface", CompletionHandler.KindInterface, "Declares a Windows Runtime interface");
                CompletionHandler.AddItem(items, "delegate", CompletionHandler.KindKeyword, "Declares a delegate type");
                CompletionHandler.AddItem(items, "struct", CompletionHandler.KindStruct, "Declares a value type struct");
                CompletionHandler.AddItem(items, "enum", CompletionHandler.KindEnum, "Declares an enumeration");
                CompletionHandler.AddItem(items, "apicontract", CompletionHandler.KindKeyword, "Declares an API contract");
                CompletionHandler.AddItem(items, "attribute", CompletionHandler.KindKeyword, "Declares a custom attribute type");
                CompletionHandler.AddItem(items, "import", CompletionHandler.KindKeyword, "Imports an IDL file");
                CompletionHandler.AddItem(items, "static", CompletionHandler.KindKeyword, "Static runtimeclass modifier");
                CompletionHandler.AddItem(items, "partial", CompletionHandler.KindKeyword, "Partial runtimeclass modifier");
                CompletionHandler.AddItem(items, "unsealed", CompletionHandler.KindKeyword, "Unsealed runtimeclass modifier");
#if WINDOWS_INTERNAL
                CompletionHandler.AddItem(items, "feature_name", CompletionHandler.KindKeyword, "Velocity feature staging declaration");
#endif
                CompletionHandler.AddItem(items, "}", CompletionHandler.KindKeyword, "Close scope");
                break;
        }

        return items;
    }

    /// <summary>Add WinRT value types (valid in struct fields).</summary>
    private static void AddValueTypes(List<object> items)
    {
        CompletionHandler.AddItem(items, "Boolean", CompletionHandler.KindTypeParameter, "8-bit Boolean");
        CompletionHandler.AddItem(items, "Char16", CompletionHandler.KindTypeParameter, "16-bit UTF-16 code unit");
        CompletionHandler.AddItem(items, "Int16", CompletionHandler.KindTypeParameter, "16-bit signed integer");
        CompletionHandler.AddItem(items, "Int32", CompletionHandler.KindTypeParameter, "32-bit signed integer");
        CompletionHandler.AddItem(items, "Int64", CompletionHandler.KindTypeParameter, "64-bit signed integer");
        CompletionHandler.AddItem(items, "UInt8", CompletionHandler.KindTypeParameter, "8-bit unsigned integer");
        CompletionHandler.AddItem(items, "UInt16", CompletionHandler.KindTypeParameter, "16-bit unsigned integer");
        CompletionHandler.AddItem(items, "UInt32", CompletionHandler.KindTypeParameter, "32-bit unsigned integer");
        CompletionHandler.AddItem(items, "UInt64", CompletionHandler.KindTypeParameter, "64-bit unsigned integer");
        CompletionHandler.AddItem(items, "Single", CompletionHandler.KindTypeParameter, "32-bit float");
        CompletionHandler.AddItem(items, "Double", CompletionHandler.KindTypeParameter, "64-bit float");
        CompletionHandler.AddItem(items, "String", CompletionHandler.KindTypeParameter, "UTF-16 string");
        CompletionHandler.AddItem(items, "Guid", CompletionHandler.KindTypeParameter, "128-bit GUID");
    }

    /// <summary>Add all WinRT types (primitives + reference types).</summary>
    private static void AddAllTypes(List<object> items)
    {
        AddValueTypes(items);
        CompletionHandler.AddItem(items, "Object", CompletionHandler.KindTypeParameter, "Base type for all WinRT types");
        // Common parameterized types
        CompletionHandler.AddItem(items, "IVector", CompletionHandler.KindInterface, "Windows.Foundation.Collections.IVector<T>");
        CompletionHandler.AddItem(items, "IVectorView", CompletionHandler.KindInterface, "Windows.Foundation.Collections.IVectorView<T>");
        CompletionHandler.AddItem(items, "IMap", CompletionHandler.KindInterface, "Windows.Foundation.Collections.IMap<K,V>");
        CompletionHandler.AddItem(items, "IMapView", CompletionHandler.KindInterface, "Windows.Foundation.Collections.IMapView<K,V>");
        CompletionHandler.AddItem(items, "IIterable", CompletionHandler.KindInterface, "Windows.Foundation.Collections.IIterable<T>");
        CompletionHandler.AddItem(items, "IAsyncOperation", CompletionHandler.KindInterface, "Windows.Foundation.IAsyncOperation<T>");
        CompletionHandler.AddItem(items, "IAsyncAction", CompletionHandler.KindInterface, "Windows.Foundation.IAsyncAction");
        CompletionHandler.AddItem(items, "IReference", CompletionHandler.KindInterface, "Windows.Foundation.IReference<T>");
        CompletionHandler.AddItem(items, "EventHandler", CompletionHandler.KindClass, "Windows.Foundation.EventHandler<T>");
        CompletionHandler.AddItem(items, "TypedEventHandler", CompletionHandler.KindClass, "Windows.Foundation.TypedEventHandler<S,A>");
    }

    /// <summary>Add user-defined and external types from the cached compilation (or fall back to ad-hoc compile).</summary>
    private static void AddUserTypes(List<object> items, Compilation? compilation,
        Func<ISymbol, bool>? filter = null)
    {
        if (compilation == null) return;
        try
        {
            foreach (var type in compilation.GetAllTypes())
            {
                if (string.IsNullOrEmpty(type.Name)) continue;
                if (filter != null && !filter(type)) continue;

                var kind = type.Kind switch
                {
                    Analysis.Symbols.SymbolKind.Interface => CompletionHandler.KindInterface,
                    Analysis.Symbols.SymbolKind.RuntimeClass => CompletionHandler.KindClass,
                    Analysis.Symbols.SymbolKind.Enum => CompletionHandler.KindEnum,
                    Analysis.Symbols.SymbolKind.Struct => CompletionHandler.KindStruct,
                    Analysis.Symbols.SymbolKind.Delegate => CompletionHandler.KindClass,
                    _ => CompletionHandler.KindClass,
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
                ["kind"] = CompletionHandler.KindConstructor,
                ["detail"] = "Constructor",
                ["insertText"] = name + "(${1});",
                ["insertTextFormat"] = 2,
            });
        }
    }
}
