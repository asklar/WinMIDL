using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests that HeaderEmitter.MapType correctly handles generic WinRT type references.
/// </summary>
public partial class HeaderEmitterGenericTests
{
    [Theory]
    [InlineData("Windows.Foundation.Collections.IVector", new[] { "String" },
        "__x_Windows_CFoundation_CCollections_CIVector*")]
    [InlineData("Windows.Foundation.Collections.IVectorView", new[] { "Int32" },
        "__x_Windows_CFoundation_CCollections_CIVectorView*")]
    [InlineData("Windows.Foundation.Collections.IMap", new[] { "String", "Object" },
        "__x_Windows_CFoundation_CCollections_CIMap*")]
    [InlineData("Windows.Foundation.IAsyncOperation", new[] { "Boolean" },
        "__x_Windows_CFoundation_CIAsyncOperation*")]
    [InlineData("Windows.Foundation.Collections.IIterable", new[] { "String" },
        "__x_Windows_CFoundation_CCollections_CIIterable*")]
    public void MapType_GenericTypeRef_NullResolvedSymbol_ManglesQualifiedName(
        string typeName, string[] typeArgNames, string expected)
    {
        var typeArgs = typeArgNames.Select(n => new TypeRef(n)).ToList();
        var typeRef = new TypeRef(typeName, typeArgs);
        // ResolvedSymbol is null — simulates unresolved generic instantiation

        var result = HeaderEmitter.MapType(typeRef);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void MapType_GenericTypeRef_ResolvedInterfaceSymbol_ManglesFullName()
    {
        // Build namespace hierarchy: Windows.Foundation.Collections
        var root = new NamespaceSymbol("Windows");
        var foundation = root.GetOrCreateChildNamespace("Foundation");
        var collections = foundation.GetOrCreateChildNamespace("Collections");

        var iface = new InterfaceSymbol("IVector", collections);
        var typeRef = new TypeRef("IVector", new[] { new TypeRef("String") });
        typeRef.ResolvedSymbol = iface;

        var result = HeaderEmitter.MapType(typeRef);

        Assert.Equal("__x_Windows_CFoundation_CCollections_CIVector*", result);
    }

    [Fact]
    public void MapType_NonGenericInterface_StillWorks()
    {
        var ns = new NamespaceSymbol("TestApp");
        var iface = new InterfaceSymbol("IGreeter", ns);
        var typeRef = new TypeRef("IGreeter");
        typeRef.ResolvedSymbol = iface;

        var result = HeaderEmitter.MapType(typeRef);

        Assert.Equal("__x_TestApp_CIGreeter*", result);
    }

    [Fact]
    public void MapType_QualifiedNameWithoutTypeArgs_ManglesAsPointer()
    {
        // A namespace-qualified unresolved type should still be mangled
        var typeRef = new TypeRef("SomeLib.Widgets.IFoo");

        var result = HeaderEmitter.MapType(typeRef);

        Assert.Equal("__x_SomeLib_CWidgets_CIFoo*", result);
    }

    [Fact]
    public void MapType_BuiltinType_NotAffected()
    {
        var typeRef = new TypeRef("String");
        var result = HeaderEmitter.MapType(typeRef);
        Assert.Equal("HSTRING", result);
    }

    [Fact]
    public void MapType_GenericWithSingleSegmentName_FallsBack()
    {
        // Unqualified generic name without resolved symbol — no namespace to mangle
        var typeRef = new TypeRef("IVector", new[] { new TypeRef("String") });
        // ResolvedSymbol is null, name has no dots
        var result = HeaderEmitter.MapType(typeRef);
        Assert.Equal("IVector", result);
    }
}
