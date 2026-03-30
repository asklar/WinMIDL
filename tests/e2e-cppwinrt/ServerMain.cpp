// ServerMain.cpp — Out-of-proc C++/WinRT server.
// Registers two CLSIDs:
//   CLSID_Widget        → creates Widget instances (for CoCreateInstance)
//   CLSID_WidgetFactory → returns the activation factory (for statics cross-process)

#include "winrt/Windows.Foundation.h"
#include "Widget.h"
#include <windows.h>
#include <stdio.h>

// Forward-declare from module.g.cpp
extern void* __stdcall winrt_get_activation_factory(std::wstring_view const& name);

// {A7B8C9D0-E1F2-4A3B-8C5D-6E7F8091A2B3}
static constexpr CLSID CLSID_Widget = {
    0xa7b8c9d0, 0xe1f2, 0x4a3b,
    {0x8c, 0x5d, 0x6e, 0x7f, 0x80, 0x91, 0xa2, 0xb3}
};

// {B8C9D0E1-F2A3-5B4C-9D6E-7F8091A2B3C4}
static constexpr CLSID CLSID_WidgetFactory = {
    0xb8c9d0e1, 0xf2a3, 0x5b4c,
    {0x9d, 0x6e, 0x7f, 0x80, 0x91, 0xa2, 0xb3, 0xc4}
};

static const wchar_t* g_shutdownEventName = L"TestCppWinRT_ServerShutdown";

// Creates Widget instances
class WidgetClassFactory : public IClassFactory
{
    LONG m_cRef = 1;
public:
    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override
    {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IClassFactory) {
            *ppv = static_cast<IClassFactory*>(this);
            AddRef();
            return S_OK;
        }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override { auto c = InterlockedDecrement(&m_cRef); if (c == 0) delete this; return c; }

    HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown* pOuter, REFIID riid, void** ppv) override
    {
        if (pOuter) return CLASS_E_NOAGGREGATION;
        if (!ppv) return E_POINTER;
        *ppv = nullptr;
        try {
            auto widget = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
            return winrt::get_unknown(widget)->QueryInterface(riid, ppv);
        } catch (winrt::hresult_error const& e) { return e.code(); }
    }
    HRESULT STDMETHODCALLTYPE LockServer(BOOL) override { return S_OK; }
};

// Returns the activation factory (implements IWidgetStatics, IWidgetFactory, IActivationFactory)
class ActivationFactoryClassFactory : public IClassFactory
{
    LONG m_cRef = 1;
public:
    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override
    {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IClassFactory) {
            *ppv = static_cast<IClassFactory*>(this);
            AddRef();
            return S_OK;
        }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override { auto c = InterlockedDecrement(&m_cRef); if (c == 0) delete this; return c; }

    HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown* pOuter, REFIID riid, void** ppv) override
    {
        if (pOuter) return CLASS_E_NOAGGREGATION;
        if (!ppv) return E_POINTER;
        *ppv = nullptr;
        void* raw = winrt_get_activation_factory(L"TestCppWinRT.Widget");
        if (!raw) return CLASS_E_CLASSNOTAVAILABLE;
        auto unk = static_cast<IUnknown*>(raw);
        HRESULT hr = unk->QueryInterface(riid, ppv);
        unk->Release();
        return hr;
    }
    HRESULT STDMETHODCALLTYPE LockServer(BOOL) override { return S_OK; }
};

int main()
{
    winrt::init_apartment();

    printf("CppWinRTServer: Starting...\n");
    fflush(stdout);

    HANDLE hShutdown = CreateEventW(nullptr, TRUE, FALSE, g_shutdownEventName);
    if (!hShutdown) {
        printf("CppWinRTServer: Failed to create shutdown event: %lu\n", GetLastError());
        return 1;
    }

    auto widgetFactory = new WidgetClassFactory();
    auto activationFactory = new ActivationFactoryClassFactory();
    DWORD cookie1 = 0, cookie2 = 0;

    HRESULT hr = CoRegisterClassObject(CLSID_Widget, widgetFactory,
                                       CLSCTX_LOCAL_SERVER, REGCLS_MULTIPLEUSE, &cookie1);
    if (FAILED(hr)) {
        printf("CppWinRTServer: CoRegisterClassObject(Widget) failed: 0x%08X\n", hr);
        return 1;
    }

    hr = CoRegisterClassObject(CLSID_WidgetFactory, activationFactory,
                               CLSCTX_LOCAL_SERVER, REGCLS_MULTIPLEUSE, &cookie2);
    if (FAILED(hr)) {
        printf("CppWinRTServer: CoRegisterClassObject(Factory) failed: 0x%08X\n", hr);
        return 1;
    }

    printf("CppWinRTServer: Registered, waiting for clients...\n");
    fflush(stdout);

    WaitForSingleObject(hShutdown, 60000);

    printf("CppWinRTServer: Shutting down...\n");
    fflush(stdout);

    CoRevokeClassObject(cookie1);
    CoRevokeClassObject(cookie2);
    widgetFactory->Release();
    activationFactory->Release();
    CloseHandle(hShutdown);

    return 0;
}
