// CalcServer.cpp — Out-of-proc COM/WinRT server hosting ICalculator
//
// This EXE registers a class factory via CoRegisterClassObject and serves
// Calculator objects over LRPC. Our custom proxy/stub DLL handles marshaling.
//
// Exit: waits for shutdown event or 60-second timeout.

#include <windows.h>
#include <winstring.h>
#include <inspectable.h>
#include <new>
#include <stdio.h>
#include <math.h>
#include "generated/TestCalculator.h"

// Same IID definitions as the proxy — needed so QI works on the server side
EXTERN_C const IID IID___x_TestApp_OutOfProc_ICalculator =
    {0x5dda72af, 0xdacc, 0xad55, {0xa8, 0xf3, 0xf0, 0x08, 0x6a, 0x27, 0xd8, 0x58}};

// CLSID for Calculator — deterministic UUID v5 of "TestApp.OutOfProc.Calculator"
// {B340F2FC-5C14-FD5E-A7F2-BE8DC2D67C04}
static const CLSID CLSID_Calculator = {
    0xb340f2fc, 0x5c14, 0xfd5e,
    {0xa7, 0xf2, 0xbe, 0x8d, 0xc2, 0xd6, 0x7c, 0x04}};

static HANDLE g_hShutdown = nullptr;
static LONG g_cServerObjects = 0;

// ---------------------------------------------------------------------------
// Calculator — implements ICalculator + IInspectable
// ---------------------------------------------------------------------------
class Calculator : public __x_TestApp_OutOfProc_ICalculator {
    LONG m_cRef;

public:
    Calculator() : m_cRef(1) {
        InterlockedIncrement(&g_cServerObjects);
    }

    ~Calculator() {
        if (InterlockedDecrement(&g_cServerObjects) == 0) {
            // Last object released — signal shutdown
            if (g_hShutdown) SetEvent(g_hShutdown);
        }
    }

    // IUnknown
    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_OutOfProc_ICalculator) {
            *ppv = static_cast<__x_TestApp_OutOfProc_ICalculator*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IMarshal) {
            // Reject free-threaded marshaler — force standard marshaling through our proxy
            *ppv = nullptr;
            return E_NOINTERFACE;
        }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }

    ULONG STDMETHODCALLTYPE AddRef() override {
        return InterlockedIncrement(&m_cRef);
    }

    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    // IInspectable
    HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {
        *iidCount = 1;
        *iids = (IID*)CoTaskMemAlloc(sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID___x_TestApp_OutOfProc_ICalculator;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.OutOfProc.Calculator", 28, className);
    }

    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust;
        return S_OK;
    }

    // ICalculator
    HRESULT STDMETHODCALLTYPE get_Version(HSTRING* value) override {
        if (!value) return E_POINTER;
        return WindowsCreateString(L"1.0.0", 5, value);
    }

    HRESULT STDMETHODCALLTYPE Add(DOUBLE a, DOUBLE b, DOUBLE* returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = a + b;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE Divide(DOUBLE numerator, DOUBLE denominator, MathResult* returnValue) override {
        if (!returnValue) return E_POINTER;
        if (denominator == 0.0) return E_INVALIDARG;
        returnValue->Value = numerator / denominator;
        // Check if result is exact (no fractional remainder)
        returnValue->IsExact = (fmod(numerator, denominator) == 0.0) ? TRUE : FALSE;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE TryParseNumber(HSTRING input, DOUBLE* result, boolean* returnValue) override {
        if (!result || !returnValue) return E_POINTER;
        UINT32 len = 0;
        PCWSTR raw = WindowsGetStringRawBuffer(input, &len);
        if (!raw || len == 0) {
            *result = 0.0;
            *returnValue = FALSE;
            return S_OK;
        }
        // Convert PCWSTR to double
        WCHAR* end = nullptr;
        double val = wcstod(raw, &end);
        if (end == raw || (end - raw) != (ptrdiff_t)len) {
            *result = 0.0;
            *returnValue = FALSE;
        } else {
            *result = val;
            *returnValue = TRUE;
        }
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// CalculatorFactory — IClassFactory for Calculator
// ---------------------------------------------------------------------------
class CalculatorFactory : public IClassFactory {
    LONG m_cRef;

public:
    CalculatorFactory() : m_cRef(1) {}

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (riid == IID_IUnknown || riid == IID_IClassFactory) {
            *ppv = static_cast<IClassFactory*>(this);
            AddRef();
            return S_OK;
        }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }

    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown* pOuter, REFIID riid, void** ppv) override {
        if (pOuter) return CLASS_E_NOAGGREGATION;
        auto* calc = new (std::nothrow) Calculator();
        if (!calc) return E_OUTOFMEMORY;
        HRESULT hr = calc->QueryInterface(riid, ppv);
        calc->Release();
        return hr;
    }

    HRESULT STDMETHODCALLTYPE LockServer(BOOL fLock) override {
        if (fLock) InterlockedIncrement(&g_cServerObjects);
        else {
            if (InterlockedDecrement(&g_cServerObjects) == 0)
                if (g_hShutdown) SetEvent(g_hShutdown);
        }
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// Main — register class factory and wait
// ---------------------------------------------------------------------------
int main() {
    printf("[CalcServer] Starting out-of-proc Calculator server...\n");

    HRESULT hr = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    if (FAILED(hr)) {
        printf("[CalcServer] CoInitializeEx failed: 0x%08lX\n", hr);
        return 1;
    }

    g_hShutdown = CreateEventW(nullptr, TRUE, FALSE, nullptr);

    CalculatorFactory factory;
    DWORD dwRegister = 0;
    hr = CoRegisterClassObject(CLSID_Calculator, &factory,
                               CLSCTX_LOCAL_SERVER,
                               REGCLS_MULTIPLEUSE | REGCLS_SUSPENDED,
                               &dwRegister);
    if (FAILED(hr)) {
        printf("[CalcServer] CoRegisterClassObject failed: 0x%08lX\n", hr);
        CloseHandle(g_hShutdown);
        CoUninitialize();
        return 1;
    }

    hr = CoResumeClassObjects();
    if (FAILED(hr)) {
        printf("[CalcServer] CoResumeClassObjects failed: 0x%08lX\n", hr);
        CoRevokeClassObject(dwRegister);
        CloseHandle(g_hShutdown);
        CoUninitialize();
        return 1;
    }

    printf("[CalcServer] Class factory registered. Waiting for clients...\n");
    fflush(stdout);

    // Wait for shutdown signal or 60-second timeout
    WaitForSingleObject(g_hShutdown, 60000);

    printf("[CalcServer] Shutting down...\n");
    CoRevokeClassObject(dwRegister);
    CloseHandle(g_hShutdown);
    CoUninitialize();
    printf("[CalcServer] Server exited cleanly.\n");
    return 0;
}
