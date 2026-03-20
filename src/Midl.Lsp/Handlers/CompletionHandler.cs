using System.Text.Json;
using Midl.Analysis;
using Midl.Analysis.Symbols;

namespace Midl.Lsp.Handlers;

/// <summary>
/// Provides keyword and type completions for MIDL3 documents.
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

    private static readonly object[] s_staticCompletionItems = BuildCompletionItems();

    public static object Handle(JsonElement? @params, DocumentManager docManager)
    {
        var items = new List<object>(s_staticCompletionItems);

        // Add external types from compilation if available
        if (@params != null)
        {
            try
            {
                var textDocument = @params.Value.GetProperty("textDocument");
                var uri = textDocument.GetProperty("uri").GetString()!;
                var text = docManager.GetDocumentText(uri);
                if (text != null)
                {
                    var compilation = CompilationHelper.Compile(text);
                    foreach (var type in compilation.GetAllTypes())
                    {
                        if (type.IsExternal)
                        {
                            var kind = type.Kind switch
                            {
                                SymbolKind.Interface => KindInterface,
                                SymbolKind.RuntimeClass => KindClass,
                                SymbolKind.Enum => KindEnum,
                                SymbolKind.Struct => KindStruct,
                                SymbolKind.Delegate => KindClass,
                                _ => KindClass,
                            };
                            items.Add(new
                            {
                                label = type.Name,
                                kind,
                                detail = $"(external) {type.FullName}",
                                insertText = type.FullName,
                            });
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors; return static items only
            }
        }

        return new { isIncomplete = false, items = items.ToArray() };
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
        AddItem(items, "Char", KindTypeParameter, "Unicode character");
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
        items.Add(new
        {
            label,
            kind,
            detail,
            insertText = label
        });
    }
}
