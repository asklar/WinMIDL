// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        Assert.Contains("winmidl::stub_base_t<CStub___x_TestApp_IWidget>", output);
    }

    [Fact]
    public void EmitStubClass_HasServerPointer_NoCRef()
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

        // Server pointer still present
        Assert.Contains("__x_TestApp_IWidget* m_pServer;", output);
        Assert.Contains("m_pServer(nullptr)", output);
        // Ref count is now managed by stub_base_t — not emitted in generated class
        Assert.DoesNotContain("LONG m_cRef;", output);
        Assert.DoesNotContain("m_cRef(1)", output);
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
        Assert.Contains("kTypeInfo.iids", output);
        Assert.Contains("IID___x_TestApp_IWidget", output);
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
    public void EmitStubClass_QueryInterface_DelegatedToStubBase()
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

        // QI is now provided by stub_base_t — generated class only supplies the entry table
        Assert.Contains("kStubQIEntries", output);
        Assert.Contains("IID_IRpcStubBuffer", output);
        Assert.Contains("IID_IUnknown", output);
        // No inline QI implementation in the generated class body
        Assert.DoesNotContain("HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override", output);
    }

    [Fact]
    public void EmitStubClass_AddRefRelease_DelegatedToStubBase()
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

        // AddRef/Release are now provided by stub_base_t — not emitted in generated class
        Assert.DoesNotContain("ULONG STDMETHODCALLTYPE AddRef()", output);
        Assert.DoesNotContain("InterlockedIncrement(&m_cRef)", output);
        Assert.DoesNotContain("ULONG STDMETHODCALLTYPE Release()", output);
        Assert.DoesNotContain("InterlockedDecrement(&m_cRef)", output);
    }

    [Fact]
    public void EmitStubClass_StringReturn_UsesHstringLen()
    {
        var source = """
            namespace TestApp
            {
                interface IWidget
                {
                    String GetName();
                }
            }
            """;

        var compilation = Compile(source);
        var output = _generator.Generate(compilation);

        // Stub reply buffer sizing must use winmidl::hstring_len, not WindowsGetStringLen
        Assert.Contains("winmidl::hstring_len", output);
        Assert.DoesNotContain("WindowsGetStringLen", output);
    }
}
