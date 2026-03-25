using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for ProxyGenerator — verifies generated C++ proxy class content.
/// Uses partial class to reduce merge conflicts.
/// </summary>
public partial class ProxyGeneratorTests
{
    private readonly ProxyGenerator _generator = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    [Fact]
    public void GeneratesProxyClassForSimpleInterface()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("class CIWidgetProxy : public __x_TestApp_IWidget, public IRpcProxyBuffer {", output);
        Assert.Contains("IUnknown* m_pUnkOuter;", output);
        Assert.Contains("IRpcChannelBuffer* m_pChannel;", output);
        Assert.Contains("LONG m_cRef;", output);
    }

    [Fact]
    public void GeneratesConstructor()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("CIWidgetProxy(IUnknown* pUnkOuter) : m_pUnkOuter(pUnkOuter), m_pChannel(nullptr), m_cRef(1) {}", output);
    }

    [Fact]
    public void GeneratesConnectMethod()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("HRESULT STDMETHODCALLTYPE Connect(IRpcChannelBuffer* pChannel) override {", output);
        Assert.Contains("m_pChannel = pChannel;", output);
        Assert.Contains("if (pChannel) pChannel->AddRef();", output);
    }

    [Fact]
    public void GeneratesDisconnectMethod()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("void STDMETHODCALLTYPE Disconnect() override {", output);
        Assert.Contains("if (m_pChannel) { m_pChannel->Release(); m_pChannel = nullptr; }", output);
    }

    [Fact]
    public void GeneratesQueryInterfaceWithBothInterfaces()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {", output);
        Assert.Contains("riid == IID_IUnknown || riid == IID_IRpcProxyBuffer", output);
        Assert.Contains("static_cast<IRpcProxyBuffer*>(this)", output);
        Assert.Contains("riid == IID___x_TestApp_IWidget", output);
        Assert.Contains("static_cast<__x_TestApp_IWidget*>(this)", output);
    }

    [Fact]
    public void GeneratesAddRefAndRelease()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }", output);
        Assert.Contains("ULONG STDMETHODCALLTYPE Release() override {", output);
        Assert.Contains("InterlockedDecrement(&m_cRef)", output);
        Assert.Contains("if (c == 0) delete this;", output);
    }

    [Fact]
    public void GeneratesIInspectableForwarding()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("GetIids(ULONG* iidCount, IID** iids) override", output);
        Assert.Contains("GetRuntimeClassName(HSTRING* className) override", output);
        Assert.Contains("GetTrustLevel(TrustLevel* trustLevel) override", output);
        Assert.Contains("m_pUnkOuter->QueryInterface(IID_IInspectable", output);
    }

    [Fact]
    public void GeneratesVoidMethodNoParams()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("HRESULT STDMETHODCALLTYPE DoWork() override {", output);
        Assert.Contains("if (!m_pChannel) return RPC_E_DISCONNECTED;", output);
        Assert.Contains("msg.iMethod = 6;", output);
        Assert.Contains("msg.cbBuffer = kDoWork_RequestSize;", output);
    }

    [Fact]
    public void GeneratesMethodWithReturnValue()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    String GetName();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("HRESULT STDMETHODCALLTYPE GetName(HSTRING* result) override {", output);
        Assert.Contains("msg.cbBuffer = kGetName_RequestSize;", output);
        Assert.Contains("WindowsCreateString", output);
    }

    [Fact]
    public void GeneratesMethodWithPrimitiveInParam()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetCount(Int32 count);
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("HRESULT STDMETHODCALLTYPE SetCount(INT32 count) override {", output);
        Assert.Contains("msg.cbBuffer = kSetCount_RequestSize;", output);
    }

    [Fact]
    public void GeneratesMethodWithMultipleParams()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetPosition(Single x, Single y);
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("HRESULT STDMETHODCALLTYPE SetPosition(FLOAT x, FLOAT y) override {", output);
        Assert.Contains("msg.cbBuffer = kSetPosition_RequestSize;", output);
        Assert.Contains("memcpy(p, &x, 4); p += 4;", output);
        Assert.Contains("memcpy(p, &y, 4); p += 4;", output);
    }

    [Fact]
    public void GeneratesMethodWithOutParam()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void GetSize(out Int32 width, out Int32 height);
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("HRESULT STDMETHODCALLTYPE GetSize(INT32* width, INT32* height) override {", output);
        Assert.Contains("msg.cbBuffer = kGetSize_RequestSize;", output);
        Assert.Contains("memcpy(width, pReply, 4); pReply += 4;", output);
        Assert.Contains("memcpy(height, pReply, 4); pReply += 4;", output);
    }

    [Fact]
    public void GeneratesPropertyGetter()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    String Name { get; };
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("HRESULT STDMETHODCALLTYPE get_Name(HSTRING* result) override {", output);
        Assert.Contains("msg.iMethod = 6;", output);
        Assert.Contains("msg.cbBuffer = kget_Name_RequestSize;", output);
    }

    [Fact]
    public void GeneratesPropertySetter()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    String Name { get; set; };
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("HRESULT STDMETHODCALLTYPE get_Name(HSTRING* result) override {", output);
        Assert.Contains("msg.iMethod = 6;", output);
        Assert.Contains("HRESULT STDMETHODCALLTYPE put_Name(HSTRING name) override {", output);
        Assert.Contains("msg.iMethod = 7;", output);
    }

    [Fact]
    public void GeneratesPropertySetterSerializesHstring()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    String Name { get; set; };
                };
            }");

        var output = _generator.Generate(compilation);

        // put_Name should serialize the HSTRING
        Assert.Contains("WindowsGetStringLen(name)", output);
        Assert.Contains("WindowsGetStringRawBuffer(name", output);
    }

    [Fact]
    public void GeneratesEventAddAndRemove()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                delegate void ClickHandler(Object sender);
                interface IWidget
                {
                    event ClickHandler Click;
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("add_Click", output);
        Assert.Contains("remove_Click", output);
    }

    [Fact]
    public void VtableSlotNumbersAreCorrect()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    String Name { get; set; };
                    void DoWork(Int32 count);
                    void Reset();
                };
            }");

        var output = _generator.Generate(compilation);

        // get_Name = slot 6, put_Name = slot 7, DoWork = slot 8, Reset = slot 9
        Assert.Contains("get_Name(HSTRING* result) override {", output);
        Assert.Contains("msg.iMethod = 6;", output);
        Assert.Contains("put_Name(HSTRING name) override {", output);
        Assert.Contains("msg.iMethod = 7;", output);
        Assert.Contains("DoWork(INT32 count) override {", output);
        Assert.Contains("msg.iMethod = 8;", output);
        Assert.Contains("Reset() override {", output);
        Assert.Contains("msg.iMethod = 9;", output);
    }

    [Fact]
    public void GeneratesGuidSerialization()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetId(Guid id);
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("msg.cbBuffer = kSetId_RequestSize;", output);
        Assert.Contains("memcpy(p, &id, 16); p += 16;", output);
    }

    [Fact]
    public void GeneratesBooleanSerialization()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetEnabled(Boolean value);
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("msg.cbBuffer = kSetEnabled_RequestSize;", output);
        Assert.Contains("memcpy(p, &value, 1); p += 1;", output);
    }

    [Fact]
    public void GeneratesDoubleSerialization()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetValue(Double value);
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("msg.cbBuffer = kSetValue_RequestSize;", output);
        Assert.Contains("memcpy(p, &value, 8); p += 8;", output);
    }

    [Fact]
    public void GeneratesInt64Serialization()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void SetTimestamp(Int64 ticks);
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("msg.cbBuffer = kSetTimestamp_RequestSize;", output);
        Assert.Contains("memcpy(p, &ticks, 8); p += 8;", output);
    }

    [Fact]
    public void GeneratesMultipleInterfaceProxies()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IFirst
                {
                    void Alpha();
                };
                interface ISecond
                {
                    void Beta();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("class CIFirstProxy", output);
        Assert.Contains("class CISecondProxy", output);
    }

    [Fact]
    public void GeneratesPrologueAndEpilogue()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("#include <windows.h>", output);
        Assert.Contains("#include <objidl.h>", output);
        Assert.Contains("#include <rpcproxy.h>", output);
        Assert.Contains("#include <winstring.h>", output);
        Assert.Contains("#include <inspectable.h>", output);
        Assert.Contains("// End of generated proxy file", output);
    }

    [Fact]
    public void RpcDisconnectedCheckPresent()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("if (!m_pChannel) return RPC_E_DISCONNECTED;", output);
    }

    [Fact]
    public void ReplyDeserialization_IncludesTrailingHresult()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("HRESULT retHr; memcpy(&retHr, pReply, 4);", output);
        Assert.Contains("return retHr;", output);
    }

    [Fact]
    public void GeneratesMethodWithHstringReturn()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    String GetTitle();
                };
            }");

        var output = _generator.Generate(compilation);

        // Deserialize HSTRING: read length, then create string
        Assert.Contains("result_len", output);
        Assert.Contains("WindowsCreateString((PCWSTR)pReply, result_len, result)", output);
    }

    [Fact]
    public void GeneratesSendReceivePattern()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("RPCOLEMESSAGE msg = {};", output);
        Assert.Contains("m_pChannel->GetBuffer(&msg, IID___x_TestApp_IWidget);", output);
        Assert.Contains("m_pChannel->SendReceive(&msg, &status);", output);
    }
}
