// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Midl.Analysis;
using Midl.CodeGen.Proxy;

namespace Midl.CodeGen.Tests;

/// <summary>
/// Tests that delegate types get proper proxy/stub registration and codegen,
/// so they can be marshaled across COM apartments.
/// </summary>
public class DelegateMarshalingTests
{
    private readonly FactoryGenerator _factory = new();
    private readonly ProxyGenerator _proxyGen = new();
    private readonly StubGenerator _stubGen = new();

    private static Compilation Compile(string source) => CompilationHelper.Compile(source);

    // ──────────────────────────────────────────────────────────────────
    // FactoryGenerator tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public void FactoryGenerator_RegistersDelegateIIDs()
    {
        var idl = @"
            namespace TestApp {
                delegate void MyCallback(String message);
                interface IWidget {
                    void DoWork();
                }
            }";
        var compilation = Compile(idl);
        var output = _factory.Generate(compilation);

        // Factory should reference the delegate's IID in CreateProxy/CreateStub
        Assert.Contains("IID___x_TestApp_MyCallback", output);
        Assert.Contains("MyCallback", output);
    }

    [Fact]
    public void FactoryGenerator_CreateProxyHandlesDelegateIID()
    {
        var idl = @"
            namespace TestApp {
                delegate void StatusHandler(Int32 code);
                interface IService {
                    void Start();
                }
            }";
        var compilation = Compile(idl);
        var output = _factory.Generate(compilation);

        // CreateProxy should have an entry for the delegate
        Assert.Contains("IID___x_TestApp_StatusHandler", output);
        Assert.Contains("CStatusHandlerProxy", output);
    }

    [Fact]
    public void FactoryGenerator_CreateStubHandlesDelegateIID()
    {
        var idl = @"
            namespace TestApp {
                delegate void StatusHandler(Int32 code);
                interface IService {
                    void Start();
                }
            }";
        var compilation = Compile(idl);
        var output = _factory.Generate(compilation);

        // CreateStub should have an entry for the delegate
        Assert.Contains("CStub___x_TestApp_StatusHandler", output);
    }

    [Fact]
    public void FactoryGenerator_DllRegisterServerRegistersDelegateIID()
    {
        var idl = @"
            namespace TestApp {
                delegate void ProgressHandler(Double percent);
                interface IDownloader {
                    void Download(String url);
                }
            }";
        var compilation = Compile(idl);
        var output = _factory.Generate(compilation);

        // DllRegisterServer should register the delegate IID
        Assert.Contains("Register IID___x_TestApp_ProgressHandler", output);
        Assert.Contains("StringFromGUID2(IID___x_TestApp_ProgressHandler", output);
    }

    [Fact]
    public void FactoryGenerator_DllUnregisterServerRemovesDelegateIID()
    {
        var idl = @"
            namespace TestApp {
                delegate void ProgressHandler(Double percent);
                interface IDownloader {
                    void Download(String url);
                }
            }";
        var compilation = Compile(idl);
        var output = _factory.Generate(compilation);

        // DllUnregisterServer should remove the delegate IID
        Assert.Contains("Remove IID___x_TestApp_ProgressHandler", output);
    }

    [Fact]
    public void FactoryGenerator_DelegateOnlyIDL_GeneratesFactory()
    {
        var idl = @"
            namespace TestApp {
                delegate void SimpleCallback();
            }";
        var compilation = Compile(idl);
        var output = _factory.Generate(compilation);

        // Even with only delegates (no interfaces), factory should be generated
        Assert.Contains("class CPSFactory", output);
        Assert.Contains("IID___x_TestApp_SimpleCallback", output);
    }

    // ──────────────────────────────────────────────────────────────────
    // ProxyGenerator tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public void ProxyGenerator_EmitsProxyClassForDelegate()
    {
        var idl = @"
            namespace TestApp {
                delegate void MyCallback(String message);
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        // Proxy class should be emitted for the delegate
        Assert.Contains("class CMyCallbackProxy : public IRpcProxyBuffer {", output);
        Assert.Contains("IUnknown* m_pUnkOuter;", output);
        Assert.Contains("IRpcChannelBuffer* m_pChannel;", output);
    }

    [Fact]
    public void ProxyGenerator_DelegateProxyHasInvokeMethod()
    {
        var idl = @"
            namespace TestApp {
                delegate void MyCallback(String message);
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        // Face class should have Invoke method
        Assert.Contains("Invoke(", output);
    }

    [Fact]
    public void ProxyGenerator_DelegateProxyHasGetFace()
    {
        var idl = @"
            namespace TestApp {
                delegate void OnComplete(Boolean success);
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        Assert.Contains("GetFace()", output);
        Assert.Contains("__x_TestApp_OnComplete* GetFace()", output);
    }

    [Fact]
    public void ProxyGenerator_DelegateProxyNoIInspectable()
    {
        var idl = @"
            namespace TestApp {
                delegate void OnComplete(Boolean success);
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        // Delegate proxy Face should NOT have IInspectable methods (GetIids, GetRuntimeClassName, GetTrustLevel)
        // because delegates extend IUnknown, not IInspectable
        Assert.DoesNotContain("GetIids", output);
        Assert.DoesNotContain("GetRuntimeClassName", output);
        Assert.DoesNotContain("GetTrustLevel", output);
    }

    [Fact]
    public void ProxyGenerator_DelegateAndInterfaceBothGetProxies()
    {
        var idl = @"
            namespace TestApp {
                delegate void MyCallback(String message);
                interface IWidget {
                    void DoWork();
                }
            }";
        var compilation = Compile(idl);
        var output = _proxyGen.Generate(compilation);

        // Both should have proxy classes
        Assert.Contains("CMyCallbackProxy", output);
        Assert.Contains("CIWidgetProxy", output);
    }

    // ──────────────────────────────────────────────────────────────────
    // StubGenerator tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public void StubGenerator_EmitsStubClassForDelegate()
    {
        var idl = @"
            namespace TestApp {
                delegate void MyCallback(String message);
            }";
        var compilation = Compile(idl);
        var output = _stubGen.Generate(compilation);

        // Stub class should be emitted for the delegate
        Assert.Contains("class CStub___x_TestApp_MyCallback", output);
        Assert.Contains("winmidl::stub_base_t<CStub___x_TestApp_MyCallback>", output);
    }

    [Fact]
    public void StubGenerator_DelegateStubHasServerPointer()
    {
        var idl = @"
            namespace TestApp {
                delegate void MyCallback(String message);
            }";
        var compilation = Compile(idl);
        var output = _stubGen.Generate(compilation);

        Assert.Contains("__x_TestApp_MyCallback* m_pServer;", output);
        Assert.Contains("m_pServer(nullptr)", output);
    }

    [Fact]
    public void StubGenerator_DelegateStubHasConnectDisconnect()
    {
        var idl = @"
            namespace TestApp {
                delegate void MyCallback(String message);
            }";
        var compilation = Compile(idl);
        var output = _stubGen.Generate(compilation);

        Assert.Contains("Connect(IUnknown* pUnkServer)", output);
        Assert.Contains("QueryInterface(IID___x_TestApp_MyCallback", output);
        Assert.Contains("Disconnect()", output);
    }

    [Fact]
    public void StubGenerator_DelegateStubHasInvokeDispatch()
    {
        var idl = @"
            namespace TestApp {
                delegate void MyCallback(String message);
            }";
        var compilation = Compile(idl);
        var output = _stubGen.Generate(compilation);

        // Invoke dispatch for the single delegate method at slot 3
        Assert.Contains("pMsg->iMethod == 3", output);
        Assert.Contains("Invoke_Invoke", output);
    }

    [Fact]
    public void StubGenerator_DelegateStubUsesSlot3()
    {
        // Delegates inherit from IUnknown (3 methods), so Invoke is at slot 3
        var idl = @"
            namespace TestApp {
                delegate void SimpleCallback();
            }";
        var compilation = Compile(idl);
        var output = _stubGen.Generate(compilation);

        Assert.Contains("iMethod == 3", output);
    }

    [Fact]
    public void StubGenerator_DelegateAndInterfaceBothGetStubs()
    {
        var idl = @"
            namespace TestApp {
                delegate void MyCallback(String message);
                interface IWidget {
                    void DoWork();
                }
            }";
        var compilation = Compile(idl);
        var output = _stubGen.Generate(compilation);

        Assert.Contains("CStub___x_TestApp_MyCallback", output);
        Assert.Contains("CStub___x_TestApp_IWidget", output);
    }

    [Fact]
    public void StubGenerator_DelegateStubCallsServerInvoke()
    {
        var idl = @"
            namespace TestApp {
                delegate void OnProgress(Double percent);
            }";
        var compilation = Compile(idl);
        var output = _stubGen.Generate(compilation);

        // The stub should call m_pServer->Invoke(...)
        Assert.Contains("m_pServer->Invoke(", output);
    }

    // ──────────────────────────────────────────────────────────────────
    // Integration: delegate used in event
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public void AllGenerators_DelegateUsedInEvent_FullCoverage()
    {
        var idl = @"
            namespace TestApp {
                delegate void StatusCallback(String status);

                interface IWidget {
                    event StatusCallback StatusChanged;
                }

                runtimeclass Widget {
                    Widget();
                    event StatusCallback StatusChanged;
                }
            }";
        var compilation = Compile(idl);

        var factoryOut = _factory.Generate(compilation);
        var proxyOut = _proxyGen.Generate(compilation);
        var stubOut = _stubGen.Generate(compilation);

        // Factory registers both the interface and delegate
        Assert.Contains("IID___x_TestApp_StatusCallback", factoryOut);
        Assert.Contains("IID___x_TestApp_IWidget", factoryOut);

        // Proxy classes exist for both
        Assert.Contains("CStatusCallbackProxy", proxyOut);
        Assert.Contains("CIWidgetProxy", proxyOut);

        // Stub classes exist for both
        Assert.Contains("CStub___x_TestApp_StatusCallback", stubOut);
        Assert.Contains("CStub___x_TestApp_IWidget", stubOut);
    }
}
