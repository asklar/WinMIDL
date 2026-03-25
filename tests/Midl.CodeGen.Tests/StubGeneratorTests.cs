using Midl.Analysis;
using Midl.CodeGen.Proxy;
using Xunit;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for StubGenerator — verifies generated C++ stub class structure.
/// Uses partial class to reduce merge conflicts.
/// </summary>
public partial class StubGeneratorTests
{
    private readonly StubGenerator _generator = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    [Fact]
    public void EmitStubClass_BasicInterface_ContainsClassName()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("class CStub___x_TestApp_IWidget", output);
        Assert.Contains("public IRpcStubBuffer", output);
    }

    [Fact]
    public void EmitStubClass_HasRefCountAndServerPointer()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("__x_TestApp_IWidget* m_pServer;", output);
        Assert.Contains("LONG m_cRef;", output);
        Assert.Contains("m_pServer(nullptr)", output);
        Assert.Contains("m_cRef(1)", output);
    }

    [Fact]
    public void EmitStubClass_ConnectMethod_QueriesServer()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("HRESULT STDMETHODCALLTYPE Connect(IUnknown* pUnkServer)", output);
        Assert.Contains("QueryInterface(IID___x_TestApp_IWidget", output);
        Assert.Contains("(void**)&m_pServer", output);
    }

    [Fact]
    public void EmitStubClass_DisconnectMethod_ReleasesServer()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("void STDMETHODCALLTYPE Disconnect()", output);
        Assert.Contains("m_pServer->Release()", output);
        Assert.Contains("m_pServer = nullptr", output);
    }

    [Fact]
    public void EmitStubClass_InvokeMethod_DispatchesByMethodIndex()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                    String GetName();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("HRESULT STDMETHODCALLTYPE Invoke(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel)", output);
        Assert.Contains("switch (pMsg->iMethod)", output);
        Assert.Contains("case 6:", output); // first user method after IUnknown(0-2)+IInspectable(3-5)
        Assert.Contains("Invoke_DoWork(pMsg, pChannel)", output);
        Assert.Contains("case 7:", output);
        Assert.Contains("Invoke_GetName(pMsg, pChannel)", output);
        Assert.Contains("default: return E_UNEXPECTED;", output);
    }

    [Fact]
    public void EmitStubClass_IsIIDSupported_ChecksCorrectIID()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("IRpcStubBuffer* STDMETHODCALLTYPE IsIIDSupported(REFIID riid)", output);
        Assert.Contains("riid == IID___x_TestApp_IWidget", output);
    }

    [Fact]
    public void EmitStubClass_CountRefs_ReturnsServerRefCount()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("ULONG STDMETHODCALLTYPE CountRefs()", output);
        Assert.Contains("m_pServer ? 1 : 0", output);
    }

    [Fact]
    public void EmitStubClass_DebugServerQueryInterface()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("DebugServerQueryInterface(void** ppv)", output);
        Assert.Contains("QueryInterface(IID___x_TestApp_IWidget, ppv)", output);
    }

    [Fact]
    public void EmitStubClass_DebugServerRelease()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("DebugServerRelease(void* pv)", output);
        Assert.Contains("((IUnknown*)pv)->Release()", output);
    }

    [Fact]
    public void EmitStubClass_QueryInterface()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv)", output);
        Assert.Contains("IID_IRpcStubBuffer", output);
        Assert.Contains("IID_IUnknown", output);
    }

    [Fact]
    public void EmitStubClass_AddRefRelease()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        Assert.Contains("ULONG STDMETHODCALLTYPE AddRef()", output);
        Assert.Contains("InterlockedIncrement(&m_cRef)", output);
        Assert.Contains("ULONG STDMETHODCALLTYPE Release()", output);
        Assert.Contains("InterlockedDecrement(&m_cRef)", output);
    }
}
