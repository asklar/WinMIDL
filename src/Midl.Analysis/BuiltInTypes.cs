using Midl.Analysis.Symbols;

namespace Midl.Analysis;

/// <summary>
/// Registers built-in WinRT types that MIDL knows about intrinsically.
/// These are available without importing any .winmd files.
/// Built-in types use detached namespace hierarchies so they don't
/// appear in the compilation's output namespace tree.
/// </summary>
public static class BuiltInTypes
{
    private const string BuiltInSource = "<built-in>";

    // Detached namespace cache so multiple types in the same namespace share one instance
    private static readonly Dictionary<string, NamespaceSymbol> _detachedNamespaces = new(StringComparer.Ordinal);

    /// <summary>
    /// Register all built-in WinRT types into the symbol table.
    /// Called once at compilation start, before user code binding.
    /// </summary>
    public static void Register(SymbolTable symbolTable, CompilationMode mode)
    {
        _detachedNamespaces.Clear();

        // 1. Core WinRT interfaces (global namespace)
        RegisterInterface(symbolTable, "", "IInspectable");
        RegisterInterface(symbolTable, "", "IUnknown");

        // 2. Windows.Foundation types
        RegisterInterface(symbolTable, "Windows.Foundation", "IStringable");
        RegisterInterface(symbolTable, "Windows.Foundation", "IClosable");
        RegisterInterface(symbolTable, "Windows.Foundation", "IAsyncAction");
        RegisterInterface(symbolTable, "Windows.Foundation", "IAsyncInfo");
        RegisterInterface(symbolTable, "Windows.Foundation", "IMemoryBuffer");
        RegisterInterface(symbolTable, "Windows.Foundation", "IPropertyValue");
        RegisterStruct(symbolTable, "Windows.Foundation", "DateTime");
        RegisterStruct(symbolTable, "Windows.Foundation", "TimeSpan");
        RegisterStruct(symbolTable, "Windows.Foundation", "Point");
        RegisterStruct(symbolTable, "Windows.Foundation", "Size");
        RegisterStruct(symbolTable, "Windows.Foundation", "Rect");
        RegisterStruct(symbolTable, "Windows.Foundation", "EventRegistrationToken");
        RegisterStruct(symbolTable, "Windows.Foundation", "HResult");
        RegisterRuntimeClass(symbolTable, "Windows.Foundation", "Uri");
        RegisterDelegate(symbolTable, "Windows.Foundation", "EventHandler");
        RegisterDelegate(symbolTable, "Windows.Foundation", "TypedEventHandler");
        RegisterDelegate(symbolTable, "Windows.Foundation", "AsyncActionCompletedHandler");

        // Parameterized types
        RegisterInterface(symbolTable, "Windows.Foundation", "IAsyncOperation");
        RegisterInterface(symbolTable, "Windows.Foundation", "IAsyncActionWithProgress");
        RegisterInterface(symbolTable, "Windows.Foundation", "IAsyncOperationWithProgress");
        RegisterInterface(symbolTable, "Windows.Foundation", "IReference");

        // 3. Windows.Foundation.Collections types
        RegisterInterface(symbolTable, "Windows.Foundation.Collections", "IIterable");
        RegisterInterface(symbolTable, "Windows.Foundation.Collections", "IIterator");
        RegisterInterface(symbolTable, "Windows.Foundation.Collections", "IVector");
        RegisterInterface(symbolTable, "Windows.Foundation.Collections", "IVectorView");
        RegisterInterface(symbolTable, "Windows.Foundation.Collections", "IMap");
        RegisterInterface(symbolTable, "Windows.Foundation.Collections", "IMapView");
        RegisterInterface(symbolTable, "Windows.Foundation.Collections", "IKeyValuePair");
        RegisterInterface(symbolTable, "Windows.Foundation.Collections", "IObservableVector");
        RegisterInterface(symbolTable, "Windows.Foundation.Collections", "IObservableMap");
        RegisterDelegate(symbolTable, "Windows.Foundation.Collections", "VectorChangedEventHandler");
        RegisterDelegate(symbolTable, "Windows.Foundation.Collections", "MapChangedEventHandler");
        RegisterInterface(symbolTable, "Windows.Foundation.Collections", "IMapChangedEventArgs");
        RegisterInterface(symbolTable, "Windows.Foundation.Collections", "IPropertySet");
        RegisterInterface(symbolTable, "Windows.Foundation.Collections", "IVectorChangedEventArgs");
        RegisterEnum(symbolTable, "Windows.Foundation.Collections", "CollectionChange");

        // 4. Global type aliases (classic C / Win32 types → WinRT equivalents)
        symbolTable.RegisterAlias("HSTRING", "String");
        symbolTable.RegisterAlias("HRESULT", "Int32");
        symbolTable.RegisterAlias("GUID", "Guid");
        symbolTable.RegisterAlias("EventRegistrationToken", "Windows.Foundation.EventRegistrationToken");
        symbolTable.RegisterAlias("BYTE", "UInt8");
        symbolTable.RegisterAlias("UINT8", "UInt8");
        symbolTable.RegisterAlias("INT16", "Int16");
        symbolTable.RegisterAlias("UINT16", "UInt16");
        symbolTable.RegisterAlias("INT32", "Int32");
        symbolTable.RegisterAlias("UINT32", "UInt32");
        symbolTable.RegisterAlias("INT64", "Int64");
        symbolTable.RegisterAlias("UINT64", "UInt64");
        symbolTable.RegisterAlias("DWORD", "UInt32");
        symbolTable.RegisterAlias("WORD", "UInt16");
        symbolTable.RegisterAlias("FLOAT", "Single");
        symbolTable.RegisterAlias("DOUBLE", "Double");
        symbolTable.RegisterAlias("BOOL", "Boolean");
        symbolTable.RegisterAlias("WCHAR", "Char");
        symbolTable.RegisterAlias("ULONGLONG", "UInt64");
        symbolTable.RegisterAlias("LONGLONG", "Int64");
        symbolTable.RegisterAlias("__int64", "Int64");
        symbolTable.RegisterAlias("AsyncStatus", "Int32");

        // 5. Compat-mode classic C lowercase types
        if (mode == CompilationMode.Compat)
        {
            symbolTable.RegisterAlias("double", "Double");
            symbolTable.RegisterAlias("float", "Single");
            symbolTable.RegisterAlias("boolean", "Boolean");
            symbolTable.RegisterAlias("int", "Int32");
            symbolTable.RegisterAlias("long", "Int32");
            symbolTable.RegisterAlias("short", "Int16");
            symbolTable.RegisterAlias("byte", "UInt8");
            symbolTable.RegisterAlias("char", "Char");
            symbolTable.RegisterAlias("wchar_t", "Char");
            symbolTable.RegisterAlias("hyper", "Int64");
            symbolTable.RegisterAlias("small", "UInt8");
            symbolTable.RegisterAlias("unsigned", "UInt32");
        }

        _detachedNamespaces.Clear();
    }

    /// <summary>
    /// Creates a detached namespace hierarchy not connected to the real global namespace.
    /// This ensures built-in types don't appear in compilation output.
    /// </summary>
    private static NamespaceSymbol GetDetachedNamespace(string dottedName)
    {
        if (string.IsNullOrEmpty(dottedName))
        {
            if (!_detachedNamespaces.TryGetValue("", out var globalNs))
            {
                globalNs = new NamespaceSymbol("");
                _detachedNamespaces[""] = globalNs;
            }
            return globalNs;
        }

        if (_detachedNamespaces.TryGetValue(dottedName, out var cached))
            return cached;

        var parts = dottedName.Split('.');
        var current = GetDetachedNamespace("");
        var pathSoFar = "";
        foreach (var part in parts)
        {
            pathSoFar = pathSoFar.Length > 0 ? $"{pathSoFar}.{part}" : part;
            if (!_detachedNamespaces.TryGetValue(pathSoFar, out var child))
            {
                child = new NamespaceSymbol(part, current);
                _detachedNamespaces[pathSoFar] = child;
            }
            current = child;
        }
        return current;
    }

    private static void RegisterInterface(SymbolTable symbolTable, string namespaceName, string name)
    {
        var ns = GetDetachedNamespace(namespaceName);
        var symbol = new InterfaceSymbol(name, ns)
        {
            IsExternal = true,
            SourceWinMdPath = BuiltInSource,
        };
        symbolTable.Define(symbol);
    }

    private static void RegisterStruct(SymbolTable symbolTable, string namespaceName, string name)
    {
        var ns = GetDetachedNamespace(namespaceName);
        var symbol = new StructSymbol(name, ns)
        {
            IsExternal = true,
            SourceWinMdPath = BuiltInSource,
        };
        symbolTable.Define(symbol);
    }

    private static void RegisterEnum(SymbolTable symbolTable, string namespaceName, string name)
    {
        var ns = GetDetachedNamespace(namespaceName);
        var symbol = new EnumSymbol(name, ns)
        {
            IsExternal = true,
            SourceWinMdPath = BuiltInSource,
        };
        symbolTable.Define(symbol);
    }

    private static void RegisterDelegate(SymbolTable symbolTable, string namespaceName, string name)
    {
        var ns = GetDetachedNamespace(namespaceName);
        var symbol = new DelegateSymbol(name, ns)
        {
            IsExternal = true,
            SourceWinMdPath = BuiltInSource,
        };
        symbolTable.Define(symbol);
    }

    private static void RegisterRuntimeClass(SymbolTable symbolTable, string namespaceName, string name)
    {
        var ns = GetDetachedNamespace(namespaceName);
        var symbol = new RuntimeClassSymbol(name, ns)
        {
            IsExternal = true,
            SourceWinMdPath = BuiltInSource,
        };
        symbolTable.Define(symbol);
    }
}
