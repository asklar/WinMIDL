// ServerMain.cpp — Out-of-proc C++/WinRT server with COM class factory
// and WinRT activation factory (for static methods).

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

static const wchar_t* g_shutdownEventName = L"TestCppWinRT_ServerShutdown";

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
    ULONG STDMETHODCALLTYPE Release() override
    {
        auto c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown* pOuter, REFIID riid, void** ppv) override
    {
        if (pOuter) return CLASS_E_NOAGGREGATION;
        if (!ppv) return E_POINTER;
        *ppv = nullptr;
        try {
            auto widget = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
            return winrt::get_unknown(widget)->QueryInterface(riid, ppv);
        } catch (winrt::hresult_error const& e) {
            return e.code();
        }
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

    // Register COM class factory (for CoCreateInstance)
    auto factory = new WidgetClassFactory();
    DWORD cookie = 0;
    HRESULT hr = CoRegisterClassObject(CLSID_Widget, factory,
                                       CLSCTX_LOCAL_SERVER, REGCLS_MULTIPLEUSE, &cookie);
    if (FAILED(hr)) {
        printf("CppWinRTServer: CoRegisterClassObject failed: 0x%08X\n", hr);
        factory->Release();
        CloseHandle(hShutdown);
        return 1;
    }

    printf("CppWinRTServer: Registered, waiting for clients...\n");
    fflush(stdout);

    WaitForSingleObject(hShutdown, 60000);

    printf("CppWinRTServer: Shutting down...\n");
    fflush(stdout);

    CoRevokeClassObject(cookie);
    factory->Release();
    CloseHandle(hShutdown);

    return 0;
}
