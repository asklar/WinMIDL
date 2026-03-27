using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for nullable / IReference&lt;T&gt; marshaling in proxy/stub codegen.
/// IReference&lt;T&gt; is a WinRT interface — marshaled via CoMarshalInterface
/// like other interface pointers.
/// </summary>
public class NullableMarshalingTests
{
    private readonly ProxyGenerator _proxyGen = new();
    private readonly StubGenerator _stubGen = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    // ─── Proxy: IReference<T> return emits marshal_interface ───

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

        // IReference<T> is an interface — should use interface marshaling
        Assert.Contains("marshal_interface", output);
        Assert.DoesNotContain("// TODO", output);
    }

    // ─── Stub: IReference<T> param emits unmarshal_interface ───

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
    }

    // ─── WireFormat: IReference<T> is not fixed size ───

    [Fact]
    public void WireFormat_NullableParam_IsNotFixedSize()
    {
        var ifaceSym = new InterfaceSymbol("IReference", null);
        var typeArg = new TypeRef("Int32") { IsBuiltIn = true };
        var typeRef = new TypeRef("IReference", [typeArg]) { ResolvedSymbol = ifaceSym };

        Assert.False(WireFormat.IsFixedSize(typeRef));
    }

    // ─── Proxy: IReference<T> retval unmarshals as interface ───

    [Fact]
    public void ProxyGenerator_NullableRetval_HandledCorrectly()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    Windows.Foundation.IReference<Int32> GetOptionalCount();
                }
            }");

        var output = _proxyGen.Generate(compilation);

        // Return type IReference<T> should unmarshal as interface on the proxy side
        Assert.Contains("unmarshal_interface", output);
    }

    // ─── WireFormat: IReference<T> detected as interface type ───

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

    // ─── Proxy: IReference<T> param emits marshal_interface with IID ───

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
        // The IID should reference the parameterized instantiation, not just the base
        Assert.Contains("IReference", output);
        Assert.Contains("Int32", output);
    }

    // ─── Stub: IReference<T> retval emits marshal_interface ───

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

        // Stub serializes reply with marshal_interface
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

    [Fact]
    public void ProxyGenerator_TwoIReferenceParams_UseDifferentIids()
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

        // Both IIDs should be present and different
        Assert.Contains("IID_IReference_1_Int32", proxyOutput);
        Assert.Contains("IID_IReference_1___x_TestApp_Point", proxyOutput);

        // Proxy marshals each param with its own IID
        Assert.Contains("marshal_interface(p, optCount, IID_IReference_1_Int32)", proxyOutput);
        Assert.Contains("marshal_interface(p, optPos, IID_IReference_1___x_TestApp_Point)", proxyOutput);

        // Stub unmarshals each param with its own IID
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

        // Return values should unmarshal with different IIDs
        Assert.Contains("unmarshal_interface(pReply, IID_IReference_1_Int32", proxyOutput);
        Assert.Contains("unmarshal_interface(pReply, IID_IReference_1___x_TestApp_Point", proxyOutput);
    }
}
