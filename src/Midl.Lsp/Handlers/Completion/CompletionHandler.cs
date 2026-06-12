// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Midl.Analysis;
using Midl.Lsp.Protocol;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides keyword and type completions for MIDL3 documents.
/// Context-aware: when inside [...], shows only applicable attributes.
/// Routes to specialized providers for each completion context.
/// </summary>
public static class CompletionHandler
{
    // CompletionItemKind values per LSP spec
    internal const int KindKeyword = 14;
    internal const int KindClass = 7;
    internal const int KindInterface = 8;
    internal const int KindEnum = 13;
    internal const int KindStruct = 22;
    internal const int KindTypeParameter = 25;
    internal const int KindModule = 9;  // for namespace
    internal const int KindEvent = 23;
    internal const int KindProperty = 10; // for attributes
    internal const int KindFile = 17; // for import file completions
    internal const int KindConstructor = 4; // for constructor suggestions
    internal const int KindEnumMember = 20; // for enum member values
    internal const int KindFolder = 19; // for directory completions

    private static readonly object[] s_staticCompletionItems = BuildCompletionItems();

    public static object Handle(JsonElement? @params, DocumentManager docManager,
        IReadOnlyList<string>? includePaths = null, ProjectFile? projectFile = null,
        Compilation? cachedCompilation = null, bool flatSdkTypeCompletions = false)
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

#if FEATURE_STAGING
            var featureItems = ImportCompletionProvider.DetectFeatureNameContext(text, line, character);
            if (featureItems != null)
                return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = featureItems.ToArray() };
#endif

            var importContext = ImportCompletionProvider.DetectImportContext(text, line);
            if (importContext != null)
            {
                string? currentFileName = null;
                if (uri.StartsWith("file:///"))
                    currentFileName = Path.GetFileName(Uri.UnescapeDataString(uri[8..]));
                var items = ImportCompletionProvider.BuildImportCompletions(importContext, fileDir, includePaths, projectFile, currentFileName);
                return new Dictionary<string, object?> { ["isIncomplete"] = true, ["items"] = items.ToArray() };
            }

            // Check if cursor is on a @ref directive line: // @ref: partial_path
            var refContext = RefDirectiveCompletionProvider.DetectRefDirectiveContext(text, line, character);
            if (refContext != null)
            {
                var items = RefDirectiveCompletionProvider.BuildRefDirectiveCompletions(refContext, fileDir, includePaths);
                return new Dictionary<string, object?> { ["isIncomplete"] = true, ["items"] = items.ToArray() };
            }

            // Check if cursor follows a dot (e.g. "Windows.Foundation." or "MyEnum.")
            var dotPrefix = DotCompletionProvider.ExtractDotPrefix(text, offset, out int exprStart);
            if (dotPrefix != null)
            {
                // Compute textEdit range: from expression start to cursor position
                var lineStarts = PositionHelper.BuildLineStarts(text);
                var (startLine, startChar) = PositionHelper.OffsetToLineChar(lineStarts, exprStart);
                var editRange = new Dictionary<string, object?>
                {
                    ["start"] = new Dictionary<string, object?> { ["line"] = startLine, ["character"] = startChar },
                    ["end"] = new Dictionary<string, object?> { ["line"] = line, ["character"] = character }
                };

                var items = DotCompletionProvider.BuildDotCompletions(dotPrefix, text, includePaths, fileDir, cachedCompilation, editRange);
                // Always return from dot path — empty means we're after a leaf type
                return new Dictionary<string, object?> { ["isIncomplete"] = items.Count > 0, ["items"] = items.ToArray() };
            }

            // Check if cursor is inside an attribute argument list [attrName(args|)]
            var attrArgContext = AttributeArgCompletionProvider.DetectAttributeArgContext(text, offset);
            if (attrArgContext != null)
            {
                // Ensure we have a compilation for type lookups
                Compilation? argCompilation = cachedCompilation;
                if (argCompilation == null)
                {
                    try
                    {
                        argCompilation = CompilationHelper.Compile(text, referencePaths: null,
                            mode: CompilationMode.Strict, includePaths: includePaths, currentFilePath: fileDir);
                    }
                    catch { }
                }

                var argItems = AttributeArgCompletionProvider.BuildAttributeArgCompletions(
                    attrArgContext.Value.attrName, attrArgContext.Value.argPosition, argCompilation);
                if (argItems != null)
                    return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = argItems.ToArray() };
            }

            // Check if cursor is inside an attribute context [...]
            var attrContext = AttributeCompletionProvider.DetectAttributeContext(text, offset);

            if (attrContext != null)
            {
#if FEATURE_STAGING
                var featureAttrItems = ImportCompletionProvider.DetectFeatureAttributeArgContext(text, offset, cachedCompilation);
                if (featureAttrItems != null)
                    return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = featureAttrItems.ToArray() };
#endif
                var items = AttributeCompletionProvider.BuildAttributeCompletions(text, attrContext, includePaths, fileDir, cachedCompilation);
                return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = items.ToArray() };
            }

            // Check if cursor is on a /// doc comment line
            var docItems = DocCommentCompletionProvider.BuildDocCommentCompletions(text, line, character);
            if (docItems != null)
                return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = docItems.ToArray() };

            // Context-aware completion based on cursor position
            var context = ContextualCompletionProvider.DetectCompletionContext(text, offset, line);
            var contextItems = ContextualCompletionProvider.BuildContextualCompletions(context, text, uri, includePaths, fileDir, cachedCompilation, flatSdkTypeCompletions);
            // Use isIncomplete=true for contexts with dynamic user types so VS Code re-queries
            bool isIncomplete = context != ContextualCompletionProvider.CompletionContext.TopLevel &&
                                context != ContextualCompletionProvider.CompletionContext.EnumBody;
            return new Dictionary<string, object?> { ["isIncomplete"] = isIncomplete, ["items"] = contextItems.ToArray() };
        }
        catch
        {
            return new Dictionary<string, object?> { ["isIncomplete"] = false, ["items"] = s_staticCompletionItems };
        }
    }

    internal static int GetOffset(string text, int line, int character)
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

    internal static bool StartsWithWord(ReadOnlySpan<char> text, string word)
    {
        if (text.Length < word.Length) return false;
        if (!text.StartsWith(word.AsSpan(), StringComparison.Ordinal)) return false;
        return text.Length == word.Length || !char.IsLetterOrDigit(text[word.Length]);
    }

    internal static bool ContainsWordBefore(ReadOnlySpan<char> text, string word)
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

    internal static string GetLineText(string text, int line)
    {
        var lines = text.Split('\n');
        return line >= 0 && line < lines.Length ? lines[line] : "";
    }

    internal static int GetLineStartOffset(string text, int offset)
    {
        for (int i = offset - 1; i >= 0; i--)
        {
            if (text[i] == '\n') return i + 1;
        }
        return 0;
    }

    internal static void AddItem(List<object> items, string label, int kind, string detail)
    {
        items.Add(new Dictionary<string, object?>
        {
            ["label"] = label,
            ["kind"] = kind,
            ["detail"] = detail,
            ["insertText"] = label
        });
    }

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
}
