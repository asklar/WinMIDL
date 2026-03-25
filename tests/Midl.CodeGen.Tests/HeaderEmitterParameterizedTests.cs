using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for parameterized interface instantiation emission (__FI blocks).
/// Partial class to reduce merge conflicts with existing HeaderEmitterTests.
/// </summary>
public partial class HeaderEmitterParameterizedTests
{
    private readonly HeaderEmitter _emitter = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    #region __FI Name Mangling

    [Theory]
    [InlineData("IVector", "String", "__FIVector_1_HSTRING")]
    [InlineData("IMap", "String,Int32", "__FIMap_2_HSTRING_int")]
    [InlineData("IVectorView", "Int32", "__FIVectorView_1_int")]
    [InlineData("IIterable", "String", "__FIIterable_1_HSTRING")]
    [InlineData("IAsyncOperation", "Boolean", "__FIAsyncOperation_1_boolean")]
    public void ComputeFIMangledName_KnownTypes_ProducesExpectedName(
        string baseName, string typeArgsCsv, string expected)
    {
        var typeArgs = typeArgsCsv.Split(',').Select(n => new TypeRef(n.Trim())).ToList();
        var typeRef = new TypeRef($"Windows.Foundation.Collections.{baseName}", typeArgs);

        var result = HeaderEmitter.ComputeFIMangledName(typeRef);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeFIMangledName_TypedEventHandler_WithNamespacedArg()
    {
        // TypedEventHandler<TestApp.Parity.BasicWidget, Object>
        // → __FITypedEventHandler_2_TestApp__CParity__CBasicWidget_IInspectable
        var typeArgs = new List<TypeRef>
        {
            new TypeRef("TestApp.Parity.BasicWidget"),
            new TypeRef("Object"),
        };
        var typeRef = new TypeRef("Windows.Foundation.TypedEventHandler", typeArgs);

        var result = HeaderEmitter.ComputeFIMangledName(typeRef);

        Assert.Equal("__FITypedEventHandler_2_TestApp__CParity__CBasicWidget_IInspectable", result);
    }

    [Fact]
    public void ComputeFIMangledName_EventHandler_Int32()
    {
        // EventHandler<Int32> → __FIEventHandler_1_int
        var typeArgs = new List<TypeRef> { new TypeRef("Int32") };
        var typeRef = new TypeRef("Windows.Foundation.EventHandler", typeArgs);

        var result = HeaderEmitter.ComputeFIMangledName(typeRef);

        Assert.Equal("__FIEventHandler_1_int", result);
    }

    [Fact]
    public void ComputeFIMangledName_IReference_Guid()
    {
        var typeArgs = new List<TypeRef> { new TypeRef("Guid") };
        var typeRef = new TypeRef("Windows.Foundation.IReference", typeArgs);

        var result = HeaderEmitter.ComputeFIMangledName(typeRef);

        Assert.Equal("__FIReference_1_GUID", result);
    }

    [Theory]
    [InlineData("String", "HSTRING")]
    [InlineData("Int32", "int")]
    [InlineData("UInt32", "UINT32")]
    [InlineData("Int64", "__int64")]
    [InlineData("UInt64", "unsigned__int64")]
    [InlineData("Boolean", "boolean")]
    [InlineData("Single", "float")]
    [InlineData("Double", "double")]
    [InlineData("Char", "wchar_t")]
    [InlineData("Object", "IInspectable")]
    [InlineData("Guid", "GUID")]
    [InlineData("UInt8", "BYTE")]
    [InlineData("Int16", "short")]
    [InlineData("UInt16", "UINT16")]
    public void MapTypeArgForFI_Primitives(string input, string expected)
    {
        var typeRef = new TypeRef(input);
        var result = HeaderEmitter.MapTypeArgForFI(typeRef);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("TestApp.Parity.BasicWidget", "TestApp__CParity__CBasicWidget")]
    [InlineData("TestApp.Parity.IBasicWidget", "TestApp__CParity__CIBasicWidget")]
    [InlineData("Foo.Bar.Baz", "Foo__CBar__CBaz")]
    public void MapTypeArgForFI_NamespacedTypes(string input, string expected)
    {
        var typeRef = new TypeRef(input);
        var result = HeaderEmitter.MapTypeArgForFI(typeRef);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ComputeFIMangledName_NestedGeneric_IVector_IMap()
    {
        // IVector<IMap<String, Int32>>
        // → __FIVector_1___FIMap_2_HSTRING_int
        var innerArgs = new List<TypeRef>
        {
            new TypeRef("String"),
            new TypeRef("Int32"),
        };
        var inner = new TypeRef("Windows.Foundation.Collections.IMap", innerArgs);
        var typeRef = new TypeRef("Windows.Foundation.Collections.IVector",
            new List<TypeRef> { inner });

        var result = HeaderEmitter.ComputeFIMangledName(typeRef);

        Assert.Equal("__FIVector_1___FIMap_2_HSTRING_int", result);
    }

    #endregion

    #region Collecting Instantiations

    [Fact]
    public void CollectParameterizedInstantiations_EmptyCompilation_ReturnsEmpty()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                enum Color { Red };
            }");

        var instantiations = HeaderEmitter.CollectParameterizedInstantiations(compilation);

        Assert.Empty(instantiations);
    }

    #endregion

    #region C++ Emission

    [Fact]
    public void EmitParameterizedCpp_TypedEventHandler_EmitsDefGuardAndTypedef()
    {
        var typeArgs = new List<TypeRef>
        {
            new TypeRef("TestApp.Parity.BasicWidget"),
            new TypeRef("Object"),
        };
        var typeRef = new TypeRef("Windows.Foundation.TypedEventHandler", typeArgs);

        var sb = new System.Text.StringBuilder();
        HeaderEmitter.EmitParameterizedInterfaceCpp(sb, typeRef);
        var output = sb.ToString();

        // Should have DEF guard
        Assert.Contains("DEF___FITypedEventHandler_2_TestApp__CParity__CBasicWidget_IInspectable_USE", output);
        // Should have #ifndef / #define
        Assert.Contains("#ifndef DEF___FITypedEventHandler_2_TestApp__CParity__CBasicWidget_IInspectable_USE", output);
        // Should have RO_NO_TEMPLATE_NAME guard
        Assert.Contains("RO_NO_TEMPLATE_NAME", output);
        // Should have typedef
        Assert.Contains("__FITypedEventHandler_2_TestApp__CParity__CBasicWidget_IInspectable_t", output);
    }

    #endregion

    #region C Block Emission

    [Fact]
    public void EmitParameterizedCBlock_TypedEventHandler_EmitsVtableWithInvoke()
    {
        var typeArgs = new List<TypeRef>
        {
            new TypeRef("TestApp.Parity.BasicWidget"),
            new TypeRef("Object"),
        };
        var typeRef = new TypeRef("Windows.Foundation.TypedEventHandler", typeArgs);

        var sb = new System.Text.StringBuilder();
        HeaderEmitter.EmitParameterizedInterfaceCBlock(sb, typeRef);
        var output = sb.ToString();

        var fiName = "__FITypedEventHandler_2_TestApp__CParity__CBasicWidget_IInspectable";

        // Should have INTERFACE_DEFINED guard
        Assert.Contains($"____{fiName}_INTERFACE_DEFINED__", output);
        // Should have EXTERN_C const IID
        Assert.Contains($"EXTERN_C const IID IID_{fiName}", output);
        // Should have vtable struct
        Assert.Contains($"{fiName}Vtbl", output);
        // Should have QueryInterface, AddRef, Release
        Assert.Contains("QueryInterface", output);
        Assert.Contains("AddRef", output);
        Assert.Contains("Release", output);
        // Should have Invoke method
        Assert.Contains("Invoke", output);
        // Should have COBJMACROS
        Assert.Contains("COBJMACROS", output);
    }

    [Fact]
    public void EmitParameterizedCBlock_IVector_EmitsInspectableBaseMethods()
    {
        var typeArgs = new List<TypeRef> { new TypeRef("String") };
        var typeRef = new TypeRef("Windows.Foundation.Collections.IVector", typeArgs);

        var sb = new System.Text.StringBuilder();
        HeaderEmitter.EmitParameterizedInterfaceCBlock(sb, typeRef);
        var output = sb.ToString();

        // IVector derives from IInspectable, should have GetIids etc.
        Assert.Contains("GetIids", output);
        Assert.Contains("GetRuntimeClassName", output);
        Assert.Contains("GetTrustLevel", output);
        // Should have interface-specific method GetAt
        Assert.Contains("GetAt", output);
    }

    #endregion
}
