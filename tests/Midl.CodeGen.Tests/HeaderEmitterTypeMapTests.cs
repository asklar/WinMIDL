using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Header;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests that HeaderEmitter.MapType produces C type names matching real midl.exe output.
/// </summary>
public partial class HeaderEmitterTypeMapTests
{
    private static string Map(string winrtTypeName)
    {
        var typeRef = new TypeRef(winrtTypeName);
        return HeaderEmitter.MapType(typeRef);
    }

    // ── Primitive type mappings (must match real midl.exe) ────────────

    [Theory]
    [InlineData("Int16", "short")]
    [InlineData("Int32", "int")]
    [InlineData("Int64", "__int64")]
    [InlineData("UInt8", "::byte")]
    [InlineData("UInt16", "unsigned short")]
    [InlineData("UInt32", "unsigned int")]
    [InlineData("UInt64", "unsigned __int64")]
    [InlineData("Single", "float")]
    [InlineData("Double", "double")]
    [InlineData("Boolean", "::boolean")]
    [InlineData("Char", "wchar_t")]
    public void MapType_PrimitiveMatchesMidlExe(string winrtType, string expectedCType)
    {
        Assert.Equal(expectedCType, Map(winrtType));
    }

    // ── Types that should stay the same ──────────────────────────────

    [Theory]
    [InlineData("String", "HSTRING")]
    [InlineData("Guid", "GUID")]
    [InlineData("Object", "IInspectable*")]
    [InlineData("void", "void")]
    [InlineData("HRESULT", "HRESULT")]
    public void MapType_UnchangedTypes(string winrtType, string expectedCType)
    {
        Assert.Equal(expectedCType, Map(winrtType));
    }

    // ── Struct field emission uses correct types ─────────────────────

    [Fact]
    public void EmitStruct_UsesCorrectPrimitiveTypes()
    {
        var emitter = new HeaderEmitter();
        var compilation = CompilationHelper.Compile(@"
            namespace TestApp
            {
                struct AllTypes
                {
                    Int16 a;
                    Int32 b;
                    Int64 c;
                    UInt8 d;
                    UInt16 e;
                    UInt32 f;
                    UInt64 g;
                    Single h;
                    Double i;
                    Boolean j;
                    Char k;
                    String l;
                    Guid m;
                };
            }");

        var header = emitter.Emit(compilation);

        Assert.Contains("short a;", header);
        Assert.Contains("int b;", header);
        Assert.Contains("__int64 c;", header);
        Assert.Contains("::byte d;", header);
        Assert.Contains("unsigned short e;", header);
        Assert.Contains("unsigned int f;", header);
        Assert.Contains("unsigned __int64 g;", header);
        Assert.Contains("float h;", header);
        Assert.Contains("double i;", header);
        Assert.Contains("::boolean j;", header);
        Assert.Contains("wchar_t k;", header);
        Assert.Contains("HSTRING l;", header);
        Assert.Contains("GUID m;", header);
    }

    // ── Interface method parameters use correct types ────────────────

    [Fact]
    public void EmitInterface_MethodParamsUseCorrectTypes()
    {
        var emitter = new HeaderEmitter();
        var compilation = CompilationHelper.Compile(@"
            namespace TestApp
            {
                interface ITypeTester
                {
                    void SetInt(Int32 v);
                    void SetFloat(Single v);
                    void SetBool(Boolean v);
                    Int32 GetInt();
                };
            }");

        var header = emitter.Emit(compilation);

        Assert.Contains("/* [in] */ int v", header);
        Assert.Contains("/* [in] */ float v", header);
        Assert.Contains("/* [in] */ ::boolean v", header);
        Assert.Contains("/* [out, retval] */ int* returnValue", header);
    }
}
