using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests for interface pointer marshaling in proxy/stub codegen.
/// Verifies that interface, delegate, runtime class, and Object parameters
/// use CoMarshalInterface/CoUnmarshalInterface via winmidl helpers.
/// </summary>
public class InterfaceMarshalingTests
{
    private readonly ProxyGenerator _proxyGen = new();
    private readonly StubGenerator _stubGen = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    // ─── Proxy: interface param emits marshal_interface ───

    [Fact]
    public void ProxyGenerator_InterfaceParam_EmitsMarshalInterface()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface ICallback { void OnComplete(); }
                interface IWidget {
                    void RegisterCallback(ICallback callback);
                }
            }");

        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_interface", output);
        Assert.Contains("IID___x_TestApp_ICallback", output);
        Assert.DoesNotContain("// TODO: marshal", output);
    }

    // ─── Stub: interface param emits unmarshal_interface ───

    [Fact]
    public void StubGenerator_InterfaceParam_EmitsUnmarshalInterface()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface ICallback { void OnComplete(); }
                interface IWidget {
                    void RegisterCallback(ICallback callback);
                }
            }");

        var output = _stubGen.Generate(compilation);

        Assert.Contains("winmidl::unmarshal_interface", output);
        Assert.Contains("IID___x_TestApp_ICallback", output);
    }

    // ─── Proxy: interface retval emits unmarshal_interface ───

    [Fact]
    public void ProxyGenerator_InterfaceRetval_EmitsUnmarshalInterface()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface ICallback { void OnComplete(); }
                interface IWidget {
                    ICallback GetCallback();
                }
            }");

        var output = _proxyGen.Generate(compilation);

        // Proxy reads reply with unmarshal_interface
        Assert.Contains("winmidl::unmarshal_interface", output);
        Assert.Contains("IID___x_TestApp_ICallback", output);
    }

    // ─── Stub: interface retval emits marshal_interface ───

    [Fact]
    public void StubGenerator_InterfaceRetval_EmitsMarshalInterface()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface ICallback { void OnComplete(); }
                interface IWidget {
                    ICallback GetCallback();
                }
            }");

        var output = _stubGen.Generate(compilation);

        // Stub serializes reply with marshal_interface
        Assert.Contains("winmidl::marshal_interface", output);
        Assert.Contains("IID___x_TestApp_ICallback", output);
    }

    // ─── Delegate param ───

    [Fact]
    public void ProxyGenerator_DelegateParam_EmitsMarshalInterface()
    {
        var compilation = Compile(@"
            namespace TestApp {
                delegate void MyHandler(Int32 value);
                interface IWidget {
                    void Subscribe(MyHandler handler);
                }
            }");

        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_interface", output);
        Assert.Contains("IID___x_TestApp_MyHandler", output);
        Assert.DoesNotContain("// TODO: marshal", output);
    }

    [Fact]
    public void StubGenerator_DelegateParam_EmitsUnmarshalInterface()
    {
        var compilation = Compile(@"
            namespace TestApp {
                delegate void MyHandler(Int32 value);
                interface IWidget {
                    void Subscribe(MyHandler handler);
                }
            }");

        var output = _stubGen.Generate(compilation);

        Assert.Contains("winmidl::unmarshal_interface", output);
        Assert.Contains("IID___x_TestApp_MyHandler", output);
    }

    // ─── Object param (IInspectable) ───

    [Fact]
    public void ProxyGenerator_ObjectParam_EmitsMarshalInterface()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetValue(Object val);
                }
            }");

        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::marshal_interface", output);
        Assert.Contains("IID_IInspectable", output);
        Assert.DoesNotContain("// TODO: marshal", output);
    }

    [Fact]
    public void StubGenerator_ObjectParam_EmitsUnmarshalInterface()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface IWidget {
                    void SetValue(Object val);
                }
            }");

        var output = _stubGen.Generate(compilation);

        Assert.Contains("winmidl::unmarshal_interface", output);
        Assert.Contains("IID_IInspectable", output);
    }

    // ─── WireFormat.IsFixedSize ───

    [Fact]
    public void WireFormat_InterfaceParam_IsNotFixedSize()
    {
        var ifaceSym = new InterfaceSymbol("IFoo", null);
        var typeRef = new TypeRef("IFoo") { ResolvedSymbol = ifaceSym };

        Assert.False(WireFormat.IsFixedSize(typeRef));
    }

    [Fact]
    public void WireFormat_DelegateParam_IsNotFixedSize()
    {
        var delegateSym = new DelegateSymbol("MyHandler", null);
        var typeRef = new TypeRef("MyHandler") { ResolvedSymbol = delegateSym };

        Assert.False(WireFormat.IsFixedSize(typeRef));
    }

    [Fact]
    public void WireFormat_ObjectParam_IsNotFixedSize()
    {
        var typeRef = new TypeRef("Object");
        Assert.False(WireFormat.IsFixedSize(typeRef));
    }

    // ─── WireFormat.IsInterfaceType ───

    [Fact]
    public void WireFormat_IsInterfaceType_TrueForInterface()
    {
        var ifaceSym = new InterfaceSymbol("IFoo", null);
        var typeRef = new TypeRef("IFoo") { ResolvedSymbol = ifaceSym };

        Assert.True(WireFormat.IsInterfaceType(typeRef));
    }

    [Fact]
    public void WireFormat_IsInterfaceType_TrueForDelegate()
    {
        var delegateSym = new DelegateSymbol("MyHandler", null);
        var typeRef = new TypeRef("MyHandler") { ResolvedSymbol = delegateSym };

        Assert.True(WireFormat.IsInterfaceType(typeRef));
    }

    [Fact]
    public void WireFormat_IsInterfaceType_TrueForObject()
    {
        var typeRef = new TypeRef("Object");
        Assert.True(WireFormat.IsInterfaceType(typeRef));
    }

    [Fact]
    public void WireFormat_IsInterfaceType_FalseForPrimitive()
    {
        var typeRef = new TypeRef("Int32");
        Assert.False(WireFormat.IsInterfaceType(typeRef));
    }

    [Fact]
    public void WireFormat_IsInterfaceType_FalseForString()
    {
        var typeRef = new TypeRef("String");
        Assert.False(WireFormat.IsInterfaceType(typeRef));
    }

    // ─── WireFormat.GetInterfaceIidName ───

    [Fact]
    public void WireFormat_GetInterfaceIidName_ReturnsCorrectName()
    {
        var ns = new NamespaceSymbol("TestApp", null);
        var ifaceSym = new InterfaceSymbol("IWidget", ns);
        var typeRef = new TypeRef("IWidget") { ResolvedSymbol = ifaceSym };

        Assert.Equal("IID___x_TestApp_IWidget", WireFormat.GetInterfaceIidName(typeRef));
    }

    [Fact]
    public void WireFormat_GetInterfaceIidName_ObjectReturnsIInspectable()
    {
        var typeRef = new TypeRef("Object");
        Assert.Equal("IID_IInspectable", WireFormat.GetInterfaceIidName(typeRef));
    }

    // ─── Buffer size computation ───

    [Fact]
    public void ProxyGenerator_InterfaceParam_EmitsGetMarshalSize()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface ICallback { void OnComplete(); }
                interface IWidget {
                    void RegisterCallback(ICallback callback);
                }
            }");

        var output = _proxyGen.Generate(compilation);

        Assert.Contains("winmidl::get_marshal_size", output);
        Assert.Contains("_marshalSize", output);
    }

    // ─── Stub cleanup: interface params get Released ───

    [Fact]
    public void StubGenerator_InterfaceParam_EmitsRelease()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface ICallback { void OnComplete(); }
                interface IWidget {
                    void RegisterCallback(ICallback callback);
                }
            }");

        var output = _stubGen.Generate(compilation);

        Assert.Contains("callback->Release()", output);
    }

    // ─── No TODO comments for interface types ───

    [Fact]
    public void ProxyGenerator_NoTodoForInterfaceOrDelegate()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface ICallback { void OnComplete(); }
                delegate void MyHandler(Int32 value);
                interface IWidget {
                    void Register(ICallback callback);
                    void Subscribe(MyHandler handler);
                    void SetValue(Object val);
                }
            }");

        var output = _proxyGen.Generate(compilation);

        Assert.DoesNotContain("// TODO: marshal", output);
    }

    // ─── Stub: interface retval gets Released ───

    [Fact]
    public void StubGenerator_InterfaceRetval_EmitsRelease()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface ICallback { void OnComplete(); }
                interface IWidget {
                    ICallback GetCallback();
                }
            }");

        var output = _stubGen.Generate(compilation);

        Assert.Contains("returnValue->Release()", output);
    }

    // ─── Stub: interface retval get_marshal_size for buffer ───

    [Fact]
    public void StubGenerator_InterfaceRetval_EmitsGetMarshalSize()
    {
        var compilation = Compile(@"
            namespace TestApp {
                interface ICallback { void OnComplete(); }
                interface IWidget {
                    ICallback GetCallback();
                }
            }");

        var output = _stubGen.Generate(compilation);

        Assert.Contains("winmidl::get_marshal_size", output);
        Assert.Contains("returnValue_marshalSize", output);
    }
}
