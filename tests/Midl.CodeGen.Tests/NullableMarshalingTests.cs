using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for nullable / IReference&lt;T&gt; marshaling in proxy/stub codegen.
/// Method params/retvals: always use marshal_interface (preserves COM identity).
/// Struct fields: use inline encoding [uint8 hasValue][T value] for all value types
/// (primitives, enums, structs) via winrt::box_value/unbox_value.
/// </summary>
public class NullableMarshalingTests
{
    private readonly ProxyGenerator _proxyGen = new();
    private readonly StubGenerator _stubGen = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    // ─── Proxy: IReference<Int32> return uses marshal_interface (method param) ───

    [Fact]
    public void ProxyGenerator_NullableInt32_EmitsMarshalInterface()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.IReference<Int32> GetOptionalCount();
                }
            }");

        var output = _proxyGen.Generate(compilation);

        // IReference<T> as method return — uses interface marshaling
        Assert.Contains("marshal_interface", output);
        Assert.DoesNotContain("marshal_nullable", output);
    }

    // ─── Stub: IReference<Int32> param uses unmarshal_interface (method param) ───

    [Fact]
    public void StubGenerator_NullableParam_EmitsUnmarshalInterface()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetOptionalValue(Windows.Foundation.IReference<Int32> value);
                }
            }");

        var output = _stubGen.Generate(compilation);

        Assert.Contains("unmarshal_interface", output);
        Assert.DoesNotContain("unmarshal_nullable", output);
    }

    // ─── WireFormat: IReference<Int32> as method param is NOT fixed size ───

    [Fact]
    public void WireFormat_NullableMethodParam_IsNotFixedSize()
    {
        var ns = new NamespaceSymbol("Windows.Foundation", null);
        var ifaceSym = new InterfaceSymbol("IReference", ns);
        var typeArg = new TypeRef("Int32") { IsBuiltIn = true };
        var typeRef = new TypeRef("IReference", [typeArg]) { ResolvedSymbol = ifaceSym };

        // Method params use marshal_interface — not fixed size
        Assert.False(WireFormat.IsFixedSize(typeRef));
    }

    // ─── WireFormat: IReference<Int32> IS a nullable value type (for struct fields) ───

    [Fact]
    public void WireFormat_NullableInt32_IsNullableValueType()
    {
        var ns = new NamespaceSymbol("Windows.Foundation", null);
        var ifaceSym = new InterfaceSymbol("IReference", ns);
        var typeArg = new TypeRef("Int32") { IsBuiltIn = true };
        var typeRef = new TypeRef("IReference", [typeArg]) { ResolvedSymbol = ifaceSym };

        Assert.True(WireFormat.IsNullableValueType(typeRef));
        Assert.Equal(5, WireFormat.GetNullableWireSize(typeRef)); // 1 + sizeof(INT32)
    }

    // ─── WireFormat: IReference<Point> IS also a nullable value type ───

    [Fact]
    public void WireFormat_NullableStruct_IsNullableValueType()
    {
        var ns = new NamespaceSymbol("Windows.Foundation", null);
        var ifaceSym = new InterfaceSymbol("IReference", ns);
        var structSym = new StructSymbol("Point", null);
        structSym.AddField(new FieldSymbol("X", structSym) { Type = new TypeRef("Single") });
        structSym.AddField(new FieldSymbol("Y", structSym) { Type = new TypeRef("Single") });
        var typeArg = new TypeRef("Point") { ResolvedSymbol = structSym };
        var typeRef = new TypeRef("IReference", [typeArg]) { ResolvedSymbol = ifaceSym };

        Assert.True(WireFormat.IsNullableValueType(typeRef));
        Assert.Equal(9, WireFormat.GetNullableWireSize(typeRef)); // 1 + sizeof(float)*2
    }

    // ─── Proxy: IReference<Int32> retval still uses marshal_interface ───

    [Fact]
    public void ProxyGenerator_NullableRetval_UsesMarshalInterface()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.IReference<Int32> GetOptionalCount();
                }
            }");

        var output = _proxyGen.Generate(compilation);

        // Return type IReference<Int32> on method — uses interface marshaling
        Assert.Contains("unmarshal_interface", output);
        Assert.DoesNotContain("unmarshal_nullable", output);
    }

    // ─── WireFormat: IReference<T> still detected as interface type ───

    [Fact]
    public void WireFormat_NullableType_IsInterfaceType()
    {
        var ifaceSym = new InterfaceSymbol("IReference", null);
        var typeArg = new TypeRef("Int32") { IsBuiltIn = true };
        var typeRef = new TypeRef("IReference", [typeArg]) { ResolvedSymbol = ifaceSym };

        Assert.True(WireFormat.IsInterfaceType(typeRef));
    }

    // ─── IID names include type argument for parameterized interfaces ───

    [Fact]
    public void WireFormat_NullableInt32_IidIncludesTypeArgument()
    {
        var ns = new NamespaceSymbol("Windows.Foundation", null);
        var ifaceSym = new InterfaceSymbol("IReference", ns);
        var typeArg = new TypeRef("Int32") { IsBuiltIn = true };
        var typeRef = new TypeRef("IReference", [typeArg]) { ResolvedSymbol = ifaceSym };

        var iidName = WireFormat.GetInterfaceIidName(typeRef);

        // IID name must include type argument to disambiguate instantiations
        Assert.Contains("Int32", iidName);
        Assert.StartsWith("IID_", iidName);
    }

    [Fact]
    public void WireFormat_DifferentNullableTypes_ProduceDifferentIidNames()
    {
        var ns = new NamespaceSymbol("Windows.Foundation", null);
        var ifaceSym = new InterfaceSymbol("IReference", ns);

        var int32Ref = new TypeRef("IReference", [new TypeRef("Int32") { IsBuiltIn = true }])
            { ResolvedSymbol = ifaceSym };
        var doubleRef = new TypeRef("IReference", [new TypeRef("Double") { IsBuiltIn = true }])
            { ResolvedSymbol = ifaceSym };

        var iidNameInt32 = WireFormat.GetInterfaceIidName(int32Ref);
        var iidNameDouble = WireFormat.GetInterfaceIidName(doubleRef);

        Assert.NotEqual(iidNameInt32, iidNameDouble);
    }

    // ─── Non-parameterized interfaces are not affected ───

    [Fact]
    public void WireFormat_NonParameterizedInterface_IidUnchanged()
    {
        var ns = new NamespaceSymbol("TestApp", null);
        var ifaceSym = new InterfaceSymbol("IWidget", ns);
        var typeRef = new TypeRef("IWidget") { ResolvedSymbol = ifaceSym };

        var iidName = WireFormat.GetInterfaceIidName(typeRef);

        Assert.Equal("IID___x_TestApp_IWidget", iidName);
    }

    // ─── Proxy: IReference<Int32> param emits marshal_interface (method param) ───

    [Fact]
    public void ProxyGenerator_NullableParam_EmitsMarshalInterfaceWithIid()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetOptionalValue(Windows.Foundation.IReference<Int32> value);
                }
            }");

        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_interface", output);
        Assert.Contains("IReference", output);
        Assert.Contains("Int32", output);
    }

    // ─── Stub: IReference<Int32> retval emits marshal_interface ───

    [Fact]
    public void StubGenerator_NullableRetval_EmitsMarshalInterface()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.IReference<Int32> GetOptionalCount();
                }
            }");

        var output = _stubGen.Generate(compilation);

        // Stub serializes reply with marshal_interface (method retval, not struct field)
        Assert.Contains("winmidl::marshal_interface", output);
    }

    // ─── GetParamTypeEnum returns t_interface for IReference<T> ───

    [Fact]
    public void WireFormat_NullableType_ParamTypeIsInterface()
    {
        var ifaceSym = new InterfaceSymbol("IReference", null);
        var typeArg = new TypeRef("Int32") { IsBuiltIn = true };
        var typeRef = new TypeRef("IReference", [typeArg]) { ResolvedSymbol = ifaceSym };

        var paramType = WireFormat.GetParamTypeEnum(typeRef);

        Assert.Equal("winmidl::param_type::t_interface", paramType);
    }

    // ─── Both IReference params use marshal_interface as method params ───

    [Fact]
    public void ProxyGenerator_TwoIReferenceParams_BothUseMarshalInterface()
    {
        var idl = @"
            namespace TestApp {
                struct Point { Single X; Single Y; };
                interface IWidget {
                    void SetBoth(
                        Windows.Foundation.IReference<Int32> optCount,
                        Windows.Foundation.IReference<Point> optPos
                    );
                }
            }";
        var compilation = CompilationHelper.Compile(idl);
        var proxyGen = new ProxyGenerator();
        var stubGen = new StubGenerator();
        var proxyOutput = proxyGen.Generate(compilation);
        var stubOutput = stubGen.Generate(compilation);

        // Both are method params → marshal_interface
        Assert.Contains("marshal_interface(p, optCount, IID_IReference_1_Int32)", proxyOutput);
        Assert.Contains("marshal_interface(p, optPos, IID_IReference_1___x_TestApp_Point)", proxyOutput);
        Assert.Contains("unmarshal_interface(p, IID_IReference_1_Int32", stubOutput);
        Assert.Contains("unmarshal_interface(p, IID_IReference_1___x_TestApp_Point", stubOutput);
    }

    [Fact]
    public void ProxyGenerator_IReferenceSameBaseInterface_DifferentRetvals()
    {
        var idl = @"
            namespace TestApp {
                struct Point { Single X; Single Y; };
                interface IWidget {
                    Windows.Foundation.IReference<Int32> GetOptCount();
                    Windows.Foundation.IReference<Point> GetOptPos();
                }
            }";
        var compilation = CompilationHelper.Compile(idl);
        var proxyGen = new ProxyGenerator();
        var proxyOutput = proxyGen.Generate(compilation);

        // Both are method retvals → use unmarshal_interface
        Assert.Contains("unmarshal_interface(pReply, IID_IReference_1_Int32", proxyOutput);
        Assert.Contains("unmarshal_interface(pReply, IID_IReference_1___x_TestApp_Point", proxyOutput);
    }
}
