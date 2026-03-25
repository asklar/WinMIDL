using Midl.Analysis;
using Midl.Analysis.Symbols;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Unit tests for FactoryGenerator — verifies generated CPSFactory class,
/// DLL exports, .def file, and CLSID computation.
/// Uses partial class to reduce merge conflicts.
/// </summary>
public partial class FactoryGeneratorTests
{
    private readonly FactoryGenerator _generator = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    [Fact]
    public void GeneratesCPSFactoryClass()
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

        Assert.Contains("class CPSFactory : public IPSFactoryBuffer {", output);
        Assert.Contains("LONG m_cRef;", output);
        Assert.Contains("CPSFactory() : m_cRef(1) {}", output);
    }

    [Fact]
    public void GeneratesIPSFactoryBufferQueryInterface()
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
        Assert.Contains("IID_IUnknown", output);
        Assert.Contains("IID_IPSFactoryBuffer", output);
        Assert.Contains("*ppv = nullptr; return E_NOINTERFACE;", output);
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
    public void GeneratesCreateProxyForInterface()
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

        Assert.Contains("HRESULT STDMETHODCALLTYPE CreateProxy(", output);
        Assert.Contains("IUnknown* pUnkOuter, REFIID riid,", output);
        Assert.Contains("IRpcProxyBuffer** ppProxy, void** ppv) override", output);
        // Should reference the proxy class name from ProxyGenerator
        Assert.Contains("riid == IID___x_TestApp_IWidget", output);
        Assert.Contains("new CIWidgetProxy(pUnkOuter)", output);
        Assert.Contains("*ppProxy = proxy;", output);
        Assert.Contains("proxy->QueryInterface(riid, ppv)", output);
    }

    [Fact]
    public void GeneratesCreateStubForInterface()
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

        Assert.Contains("HRESULT STDMETHODCALLTYPE CreateStub(", output);
        Assert.Contains("REFIID riid, IUnknown* pServer,", output);
        Assert.Contains("IRpcStubBuffer** ppStub) override", output);
        // Should reference the stub class name from StubGenerator
        Assert.Contains("riid == IID___x_TestApp_IWidget", output);
        Assert.Contains("new CStub___x_TestApp_IWidget()", output);
        Assert.Contains("stub->Connect(pServer)", output);
        Assert.Contains("*ppStub = stub;", output);
    }

    [Fact]
    public void GeneratesCreateProxyAndStubForMultipleInterfaces()
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

        // CreateProxy dispatches for both
        Assert.Contains("new CIFirstProxy(pUnkOuter)", output);
        Assert.Contains("new CISecondProxy(pUnkOuter)", output);

        // CreateStub dispatches for both
        Assert.Contains("new CStub___x_TestApp_IFirst()", output);
        Assert.Contains("new CStub___x_TestApp_ISecond()", output);
    }

    [Fact]
    public void GeneratesGlobalFactoryInstance()
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

        Assert.Contains("static CPSFactory g_PSFactory;", output);
    }

    [Fact]
    public void GeneratesDllGetClassObject()
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

        Assert.Contains("HRESULT STDAPICALLTYPE DllGetClassObject(REFCLSID rclsid, REFIID riid, void** ppv)", output);
        Assert.Contains("CLSID_PSFactoryBuffer", output);
        Assert.Contains("g_PSFactory.QueryInterface(riid, ppv)", output);
        Assert.Contains("CLASS_E_CLASSNOTAVAILABLE", output);
    }

    [Fact]
    public void GeneratesDllCanUnloadNow()
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

        Assert.Contains("HRESULT STDAPICALLTYPE DllCanUnloadNow()", output);
        Assert.Contains("S_FALSE", output);
    }

    [Fact]
    public void GeneratesDllRegisterServer()
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

        Assert.Contains("HRESULT STDAPICALLTYPE DllRegisterServer()", output);
        // Should register each IID
        Assert.Contains("IID___x_TestApp_IWidget", output);
        Assert.Contains("ProxyStubClsid32", output);
        Assert.Contains("CLSID_PSFactoryBuffer", output);
        Assert.Contains("InprocServer32", output);
    }

    [Fact]
    public void GeneratesDllUnregisterServer()
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

        Assert.Contains("HRESULT STDAPICALLTYPE DllUnregisterServer()", output);
        Assert.Contains("RegDeleteKey", output);
    }

    [Fact]
    public void GeneratesCLSID_PSFactoryBuffer()
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

        // Should have a CLSID definition
        Assert.Contains("CLSID_PSFactoryBuffer", output);
        // CLSID should be a valid GUID constant
        Assert.Matches(@"static const CLSID CLSID_PSFactoryBuffer = \{", output);
    }

    [Fact]
    public void CLSID_IsDeterministic()
    {
        var source = @"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }";

        var compilation1 = Compile(source);
        var compilation2 = Compile(source);

        var output1 = _generator.Generate(compilation1);
        var output2 = _generator.Generate(compilation2);

        // The CLSID should be the same for the same input
        Assert.Equal(output1, output2);
    }

    [Fact]
    public void GeneratesCreateProxyFallbackToNoInterface()
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

        // CreateProxy should return E_NOINTERFACE for unknown IIDs
        var createProxyIdx = output.IndexOf("CreateProxy(");
        var createProxyEnd = output.IndexOf("CreateStub(", createProxyIdx);
        var createProxyBody = output[createProxyIdx..createProxyEnd];
        Assert.Contains("E_NOINTERFACE", createProxyBody);
    }

    [Fact]
    public void GeneratesCreateStubFallbackToNoInterface()
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

        // CreateStub should return E_NOINTERFACE for unknown IIDs
        var createStubIdx = output.IndexOf("CreateStub(");
        var remainder = output[createStubIdx..];
        Assert.Contains("E_NOINTERFACE", remainder);
    }

    [Fact]
    public void GeneratesFileHeader()
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

        Assert.Contains("auto-generated", output);
        Assert.Contains("#include", output);
    }

    [Fact]
    public void NoInterfaces_GeneratesMinimalOutput()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                enum Color { Red, Green, Blue };
            }");

        var output = _generator.Generate(compilation);

        // Should still produce valid file but no factory class
        Assert.Contains("auto-generated", output);
        Assert.DoesNotContain("class CPSFactory", output);
    }

    [Fact]
    public void GeneratesDefFile()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var defContent = _generator.GenerateDefFile(compilation, "fooproxy");

        Assert.Contains("LIBRARY fooproxy", defContent);
        Assert.Contains("EXPORTS", defContent);
        Assert.Contains("DllGetClassObject   PRIVATE", defContent);
        Assert.Contains("DllCanUnloadNow     PRIVATE", defContent);
        Assert.Contains("DllRegisterServer   PRIVATE", defContent);
        Assert.Contains("DllUnregisterServer PRIVATE", defContent);
    }

    [Fact]
    public void GeneratesDefFile_UsesProvidedLibraryName()
    {
        var compilation = Compile(@"
            namespace TestApp
            {
                interface IWidget
                {
                    void DoWork();
                };
            }");

        var defContent = _generator.GenerateDefFile(compilation, "myproxydll");

        Assert.Contains("LIBRARY myproxydll", defContent);
    }

    [Fact]
    public void DeepNamespace_UsesCorrectMangledNames()
    {
        var compilation = Compile(@"
            namespace Company.Product.Feature
            {
                interface IService
                {
                    void Execute();
                };
            }");

        var output = _generator.Generate(compilation);

        Assert.Contains("IID___x_Company_Product_Feature_IService", output);
        Assert.Contains("CIServiceProxy", output);
        Assert.Contains("CStub___x_Company_Product_Feature_IService", output);
    }
}
