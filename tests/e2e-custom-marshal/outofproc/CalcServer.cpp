// CalcServer.cpp — Out-of-proc COM/WinRT server hosting multiple runtimeclasses
//
// This EXE registers class factories via CoRegisterClassObject and serves
// objects over LRPC. Our custom proxy/stub DLL handles marshaling.
//
// Exit: waits for shutdown event or 60-second timeout.

#include <windows.h>
#include <winstring.h>
#include <inspectable.h>
#include <new>
#include <stdio.h>
#include <math.h>
#include <vector>
#include <string>
#include "generated/TestCalculator.h"

// IID definitions from generated code
#include "generated/TestCalculator_i.c"

// CLSIDs — deterministic UUID v5 of each class name
static const CLSID CLSID_Calculator = {
    0xb340f2fc, 0x5c14, 0xfd5e,
    {0xa7, 0xf2, 0xbe, 0x8d, 0xc2, 0xd6, 0x7c, 0x04}};

static const CLSID CLSID_TypeShowcase = {
    0xe1f2a3b4, 0xc5d6, 0x4e7f,
    {0x80, 0x91, 0xa2, 0xb3, 0xc4, 0xd5, 0xe6, 0xf7}};

static const CLSID CLSID_Notifier = {
    0xa1b2c3d4, 0xe5f6, 0x4a7b,
    {0x8c, 0x9d, 0x0e, 0x1f, 0x2a, 0x3b, 0x4c, 0x59}};

static const CLSID CLSID_PropertyBag = {
    0xc3d4e5f6, 0xa7b8, 0x4c9d,
    {0x0e, 0x1f, 0x2a, 0x3b, 0x4c, 0x5d, 0x6e, 0x7f}};

static const CLSID CLSID_ComplexOps = {
    0xf2a3b4c5, 0xd6e7, 0x4f80,
    {0x91, 0xa2, 0xb3, 0xc4, 0xd5, 0xe6, 0xf7, 0xa8}};

static const CLSID CLSID_OptionalValues = {
    0xd4e5f6a7, 0xb8c9, 0x4d0e,
    {0x1f, 0x2a, 0x3b, 0x4c, 0x5d, 0x6e, 0x7f, 0x80}};

static const CLSID CLSID_Circle = {
    0x1a2b3c4d, 0x5e6f, 0x4a7b,
    {0x8c, 0x9d, 0xae, 0xbf, 0xc0, 0xd1, 0xe2, 0xf3}};

static const CLSID CLSID_EventSource = {
    0x2b3c4d5e, 0x6f7a, 0x4b8c,
    {0x9d, 0xae, 0xbf, 0xc0, 0xd1, 0xe2, 0xf3, 0xa4}};

static const CLSID CLSID_DelegateInvoker = {
    0x3c4d5e6f, 0x7a8b, 0x4c9d,
    {0xae, 0xbf, 0xc0, 0xd1, 0xe2, 0xf3, 0xa4, 0xb5}};

static const CLSID CLSID_StringListProvider = {
    0x4d5e6f7a, 0x8b9c, 0x4d0e,
    {0x1f, 0x2a, 0x3b, 0x4c, 0x5d, 0x6e, 0x7f, 0x81}};

static const CLSID CLSID_AsyncCalculator = {
    0x5e6f7a8b, 0x9c0d, 0x4e1f,
    {0x2a, 0x3b, 0x4c, 0x5d, 0x6e, 0x7f, 0x80, 0x91}};

static HANDLE g_hShutdown = nullptr;
static LONG g_cServerObjects = 0;

static void ServerObjIncRef() { InterlockedIncrement(&g_cServerObjects); }
static void ServerObjDecRef() {
    // Don't auto-shutdown on last object release — let the timeout handle it.
    // This prevents premature exit between test groups when objects from one
    // group are released before the next group's CoCreateInstance arrives.
    InterlockedDecrement(&g_cServerObjects);
}

// ---------------------------------------------------------------------------
// Calculator — implements ICalculator + IInspectable
// ---------------------------------------------------------------------------
class Calculator : public __x_TestApp_OutOfProc_ICalculator,
                   public __x_TestApp_OutOfProc_ICalculatorStatics {
    LONG m_cRef;
    static constexpr DOUBLE kPi = 3.14159265358979323846;

public:
    Calculator() : m_cRef(1) { ServerObjIncRef(); }
    ~Calculator() { ServerObjDecRef(); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_OutOfProc_ICalculator) {
            *ppv = static_cast<__x_TestApp_OutOfProc_ICalculator*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID___x_TestApp_OutOfProc_ICalculatorStatics) {
            *ppv = static_cast<__x_TestApp_OutOfProc_ICalculatorStatics*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IMarshal) { *ppv = nullptr; return E_NOINTERFACE; }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }

    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {
        *iidCount = 2;
        *iids = (IID*)CoTaskMemAlloc(2 * sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID___x_TestApp_OutOfProc_ICalculator;
        (*iids)[1] = IID___x_TestApp_OutOfProc_ICalculatorStatics;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.OutOfProc.Calculator", 28, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

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

    // ICalculatorStatics
    HRESULT STDMETHODCALLTYPE get_Pi(DOUBLE* value) override {
        if (!value) return E_POINTER;
        *value = kPi;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE IsEven(INT32 value, boolean* returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = (value % 2 == 0) ? TRUE : FALSE;
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// TypeShowcaseImpl — implements ITypeShowcase (additional primitive types)
// ---------------------------------------------------------------------------
class TypeShowcaseImpl : public __x_TestApp_OutOfProc_ITypeShowcase {
    LONG m_cRef;
    INT64 m_bigNumber;
    GUID m_id;
    WCHAR m_initial;
    BYTE m_byte;
    INT16 m_smallNumber;
public:
    TypeShowcaseImpl()
        : m_cRef(1), m_bigNumber(0),
          m_initial(L'A'), m_byte(42), m_smallNumber(-123) {
        m_id = {0x12345678, 0x9abc, 0xdef0, {0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde, 0xf0}};
        ServerObjIncRef();
    }
    ~TypeShowcaseImpl() { ServerObjDecRef(); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_OutOfProc_ITypeShowcase) {
            *ppv = static_cast<__x_TestApp_OutOfProc_ITypeShowcase*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IMarshal) { *ppv = nullptr; return E_NOINTERFACE; }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {
        *iidCount = 1;
        *iids = (IID*)CoTaskMemAlloc(sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID___x_TestApp_OutOfProc_ITypeShowcase;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.OutOfProc.TypeShowcase", 30, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE GetBigNumber(INT64* returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = m_bigNumber;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE SetBigNumber(INT64 value) override {
        m_bigNumber = value;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetId(GUID* returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = m_id;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE SetId(GUID id) override {
        m_id = id;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetInitial(WCHAR* returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = m_initial;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetByte(BYTE* returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = m_byte;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetSmallNumber(INT16* returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = m_smallNumber;
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// NotifierImpl — implements INotifier (interface pointer params)
// ---------------------------------------------------------------------------
class NotifierImpl : public __x_TestApp_OutOfProc_INotifier {
    LONG m_cRef;
    __x_TestApp_OutOfProc_ICallback* m_callback;
public:
    NotifierImpl() : m_cRef(1), m_callback(nullptr) { ServerObjIncRef(); }
    ~NotifierImpl() {
        if (m_callback) m_callback->Release();
        ServerObjDecRef();
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_OutOfProc_INotifier) {
            *ppv = static_cast<__x_TestApp_OutOfProc_INotifier*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IMarshal) { *ppv = nullptr; return E_NOINTERFACE; }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {
        *iidCount = 1;
        *iids = (IID*)CoTaskMemAlloc(sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID___x_TestApp_OutOfProc_INotifier;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.OutOfProc.Notifier", 26, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE Subscribe(__x_TestApp_OutOfProc_ICallback* callback) override {
        if (m_callback) m_callback->Release();
        m_callback = callback;
        if (m_callback) m_callback->AddRef();
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetCallback(__x_TestApp_OutOfProc_ICallback** returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = m_callback;
        if (m_callback) m_callback->AddRef();
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// PropertyBagImpl — implements IPropertyBag (Object / IInspectable* params)
// ---------------------------------------------------------------------------
class PropertyBagImpl : public __x_TestApp_OutOfProc_IPropertyBag {
    LONG m_cRef;
    static constexpr int kMaxEntries = 16;
    struct Entry {
        HSTRING key;
        IInspectable* value;
    };
    Entry m_entries[kMaxEntries];
    int m_count;

    int FindKey(HSTRING key) {
        UINT32 keyLen = 0;
        PCWSTR keyRaw = WindowsGetStringRawBuffer(key, &keyLen);
        for (int i = 0; i < m_count; i++) {
            UINT32 eLen = 0;
            PCWSTR eRaw = WindowsGetStringRawBuffer(m_entries[i].key, &eLen);
            if (eLen == keyLen && wcsncmp(eRaw, keyRaw, keyLen) == 0) return i;
        }
        return -1;
    }
public:
    PropertyBagImpl() : m_cRef(1), m_count(0) {
        memset(m_entries, 0, sizeof(m_entries));
        ServerObjIncRef();
    }
    ~PropertyBagImpl() {
        for (int i = 0; i < m_count; i++) {
            if (m_entries[i].key) WindowsDeleteString(m_entries[i].key);
            if (m_entries[i].value) m_entries[i].value->Release();
        }
        ServerObjDecRef();
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_OutOfProc_IPropertyBag) {
            *ppv = static_cast<__x_TestApp_OutOfProc_IPropertyBag*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IMarshal) { *ppv = nullptr; return E_NOINTERFACE; }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {
        *iidCount = 1;
        *iids = (IID*)CoTaskMemAlloc(sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID___x_TestApp_OutOfProc_IPropertyBag;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.OutOfProc.PropertyBag", 29, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE SetValue(HSTRING key, IInspectable* value) override {
        int idx = FindKey(key);
        if (idx >= 0) {
            if (m_entries[idx].value) m_entries[idx].value->Release();
            m_entries[idx].value = value;
            if (value) value->AddRef();
            return S_OK;
        }
        if (m_count >= kMaxEntries) return E_OUTOFMEMORY;
        HSTRING keyCopy = nullptr;
        HRESULT hr = WindowsDuplicateString(key, &keyCopy);
        if (FAILED(hr)) return hr;
        m_entries[m_count].key = keyCopy;
        m_entries[m_count].value = value;
        if (value) value->AddRef();
        m_count++;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetValue(HSTRING key, IInspectable** returnValue) override {
        if (!returnValue) return E_POINTER;
        int idx = FindKey(key);
        if (idx < 0) { *returnValue = nullptr; return S_OK; }
        *returnValue = m_entries[idx].value;
        if (*returnValue) (*returnValue)->AddRef();
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// ComplexOpsImpl — implements IComplexOps (many params + mixed in/out)
// ---------------------------------------------------------------------------
class ComplexOpsImpl : public __x_TestApp_OutOfProc_IComplexOps {
    LONG m_cRef;
    Permissions m_perms;
public:
    ComplexOpsImpl() : m_cRef(1), m_perms(Permissions_None) { ServerObjIncRef(); }
    ~ComplexOpsImpl() { ServerObjDecRef(); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_OutOfProc_IComplexOps) {
            *ppv = static_cast<__x_TestApp_OutOfProc_IComplexOps*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IMarshal) { *ppv = nullptr; return E_NOINTERFACE; }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {
        *iidCount = 1;
        *iids = (IID*)CoTaskMemAlloc(sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID___x_TestApp_OutOfProc_IComplexOps;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.OutOfProc.ComplexOps", 28, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE MultiParam(HSTRING name, INT32 x, INT32 y,
                                          DOUBLE scale, boolean visible) override {
        (void)name; (void)x; (void)y; (void)scale; (void)visible;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE TryParse(HSTRING input, INT32* intResult,
                                        DOUBLE* floatResult, boolean* returnValue) override {
        if (!intResult || !floatResult || !returnValue) return E_POINTER;
        UINT32 len = 0;
        PCWSTR raw = WindowsGetStringRawBuffer(input, &len);
        if (len > 0 && raw) {
            WCHAR* end = nullptr;
            long val = wcstol(raw, &end, 10);
            if (end != raw) {
                *intResult = (INT32)val;
                *floatResult = (DOUBLE)val;
                *returnValue = TRUE;
                return S_OK;
            }
        }
        *intResult = 0;
        *floatResult = 0.0;
        *returnValue = FALSE;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE Swap(INT32 a, INT32 b, INT32* outA, INT32* outB) override {
        if (!outA || !outB) return E_POINTER;
        *outA = b;
        *outB = a;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE MakePoint(FLOAT x, FLOAT y, Point* returnValue) override {
        if (!returnValue) return E_POINTER;
        returnValue->X = x;
        returnValue->Y = y;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE SetPermissions(Permissions perms) override {
        m_perms = perms;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE GetPermissions(Permissions* returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = m_perms;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE Translate(ColoredPoint pt, FLOAT dx, FLOAT dy,
                                         ColoredPoint* returnValue) override {
        if (!returnValue) return E_POINTER;
        returnValue->Position.X = pt.Position.X + dx;
        returnValue->Position.Y = pt.Position.Y + dy;
        returnValue->PointColor = pt.PointColor;
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// OptionalValues — implements IOptionalValues (IReference<T> round-trip)
// ---------------------------------------------------------------------------
class OptionalValuesImpl : public __x_TestApp_OutOfProc_IOptionalValues {
    LONG m_cRef;
    IInspectable* m_optionalCount;  // IReference<Int32>
    IInspectable* m_optionalScore;  // IReference<Double>

public:
    OptionalValuesImpl() : m_cRef(1), m_optionalCount(nullptr), m_optionalScore(nullptr) {
        ServerObjIncRef();
    }
    ~OptionalValuesImpl() {
        if (m_optionalCount) m_optionalCount->Release();
        if (m_optionalScore) m_optionalScore->Release();
        ServerObjDecRef();
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_OutOfProc_IOptionalValues) {
            *ppv = static_cast<__x_TestApp_OutOfProc_IOptionalValues*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IMarshal) { *ppv = nullptr; return E_NOINTERFACE; }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }

    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {
        *iidCount = 1;
        *iids = (IID*)CoTaskMemAlloc(sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID___x_TestApp_OutOfProc_IOptionalValues;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.OutOfProc.OptionalValues", 31, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE GetOptionalCount(IInspectable** returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = m_optionalCount;
        if (m_optionalCount) m_optionalCount->AddRef();
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE SetOptionalCount(IInspectable* count) override {
        if (m_optionalCount) m_optionalCount->Release();
        m_optionalCount = count;
        if (m_optionalCount) m_optionalCount->AddRef();
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE GetOptionalScore(IInspectable** returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = m_optionalScore;
        if (m_optionalScore) m_optionalScore->AddRef();
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE SetOptionalScore(IInspectable* score) override {
        if (m_optionalScore) m_optionalScore->Release();
        m_optionalScore = score;
        if (m_optionalScore) m_optionalScore->AddRef();
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// CircleImpl — implements ICircle + IShape (inheritance cross-process)
// ---------------------------------------------------------------------------
class CircleImpl : public __x_TestApp_OutOfProc_ICircle,
                   public __x_TestApp_OutOfProc_IShape {
    LONG m_cRef;
    static constexpr DOUBLE kRadius = 5.0;
    static constexpr DOUBLE kPi = 3.14159265358979323846;

public:
    CircleImpl() : m_cRef(1) { ServerObjIncRef(); }
    ~CircleImpl() { ServerObjDecRef(); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_OutOfProc_ICircle) {
            *ppv = static_cast<__x_TestApp_OutOfProc_ICircle*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID___x_TestApp_OutOfProc_IShape) {
            *ppv = static_cast<__x_TestApp_OutOfProc_IShape*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IMarshal) { *ppv = nullptr; return E_NOINTERFACE; }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {
        *iidCount = 2;
        *iids = (IID*)CoTaskMemAlloc(2 * sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID___x_TestApp_OutOfProc_ICircle;
        (*iids)[1] = IID___x_TestApp_OutOfProc_IShape;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.OutOfProc.Circle", 24, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    // ICircle
    HRESULT STDMETHODCALLTYPE get_Radius(DOUBLE* value) override {
        if (!value) return E_POINTER;
        *value = kRadius;
        return S_OK;
    }

    // IShape
    HRESULT STDMETHODCALLTYPE get_Kind(HSTRING* value) override {
        if (!value) return E_POINTER;
        return WindowsCreateString(L"Circle", 6, value);
    }
    HRESULT STDMETHODCALLTYPE get_Area(DOUBLE* value) override {
        if (!value) return E_POINTER;
        *value = kPi * kRadius * kRadius;
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// EventSourceImpl — implements IEventSource (cross-process events)
// ---------------------------------------------------------------------------
class EventSourceImpl : public __x_TestApp_OutOfProc_IEventSource {
    LONG m_cRef;
    __x_TestApp_OutOfProc_StatusNotifyHandler* m_handler;
    INT64 m_nextToken;

public:
    EventSourceImpl() : m_cRef(1), m_handler(nullptr), m_nextToken(1) {
        ServerObjIncRef();
    }
    ~EventSourceImpl() {
        if (m_handler) m_handler->Release();
        ServerObjDecRef();
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_OutOfProc_IEventSource) {
            *ppv = static_cast<__x_TestApp_OutOfProc_IEventSource*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IMarshal) { *ppv = nullptr; return E_NOINTERFACE; }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {
        *iidCount = 1;
        *iids = (IID*)CoTaskMemAlloc(sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID___x_TestApp_OutOfProc_IEventSource;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.OutOfProc.EventSource", 29, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE add_StatusChanged(
        __x_TestApp_OutOfProc_StatusNotifyHandler* handler,
        EventRegistrationToken* token) override {
        if (!token) return E_POINTER;
        if (m_handler) m_handler->Release();
        m_handler = handler;
        if (m_handler) m_handler->AddRef();
        token->value = m_nextToken++;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE remove_StatusChanged(
        EventRegistrationToken token) override {
        (void)token;
        if (m_handler) { m_handler->Release(); m_handler = nullptr; }
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE RaiseStatus(HSTRING message) override {
        if (m_handler) {
            printf("[CalcServer] RaiseStatus: calling Invoke on handler %p\n", (void*)m_handler);
            fflush(stdout);
            HRESULT hr = m_handler->Invoke(
                static_cast<IInspectable*>(
                    static_cast<__x_TestApp_OutOfProc_IEventSource*>(this)),
                message);
            printf("[CalcServer] RaiseStatus: Invoke returned hr=0x%08lX\n", hr);
            fflush(stdout);
            return hr;
        }
        printf("[CalcServer] RaiseStatus: no handler\n");
        fflush(stdout);
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// DelegateInvokerImpl — implements IDelegateInvoker (cross-process delegate)
// ---------------------------------------------------------------------------
class DelegateInvokerImpl : public __x_TestApp_OutOfProc_IDelegateInvoker {
    LONG m_cRef;

public:
    DelegateInvokerImpl() : m_cRef(1) { ServerObjIncRef(); }
    ~DelegateInvokerImpl() { ServerObjDecRef(); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_OutOfProc_IDelegateInvoker) {
            *ppv = static_cast<__x_TestApp_OutOfProc_IDelegateInvoker*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IMarshal) { *ppv = nullptr; return E_NOINTERFACE; }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {
        *iidCount = 1;
        *iids = (IID*)CoTaskMemAlloc(sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID___x_TestApp_OutOfProc_IDelegateInvoker;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.OutOfProc.DelegateInvoker", 33, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE ApplyTransform(HSTRING input,
        __x_TestApp_OutOfProc_StringTransform* transform,
        HSTRING* returnValue) override {
        if (!returnValue) return E_POINTER;
        if (!transform) return E_INVALIDARG;
        return transform->Invoke(input, returnValue);
    }
};

// ---------------------------------------------------------------------------
// StringListImpl — implements IStringList (custom collection with full vtable)
// ---------------------------------------------------------------------------
class StringListImpl : public __x_TestApp_OutOfProc_IStringList {
    LONG m_cRef;
    std::vector<std::wstring> m_items;
public:
    StringListImpl() : m_cRef(1) { ServerObjIncRef(); }
    ~StringListImpl() { ServerObjDecRef(); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_OutOfProc_IStringList) {
            *ppv = static_cast<__x_TestApp_OutOfProc_IStringList*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IMarshal) { *ppv = nullptr; return E_NOINTERFACE; }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {
        *iidCount = 1;
        *iids = (IID*)CoTaskMemAlloc(sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID___x_TestApp_OutOfProc_IStringList;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.OutOfProc.StringList", 28, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE get_Count(UINT32* value) override {
        if (!value) return E_POINTER;
        *value = (UINT32)m_items.size();
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetItem(UINT32 index, HSTRING* returnValue) override {
        if (!returnValue) return E_POINTER;
        if (index >= (UINT32)m_items.size()) return E_BOUNDS;
        return WindowsCreateString(m_items[index].c_str(),
                                    (UINT32)m_items[index].size(), returnValue);
    }
    HRESULT STDMETHODCALLTYPE Append(HSTRING value) override {
        UINT32 len = 0;
        PCWSTR raw = WindowsGetStringRawBuffer(value, &len);
        m_items.push_back(std::wstring(raw ? raw : L"", len));
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE Clear() override {
        m_items.clear();
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// StringListProviderImpl — implements IStringListProvider
// ---------------------------------------------------------------------------
class StringListProviderImpl : public __x_TestApp_OutOfProc_IStringListProvider {
    LONG m_cRef;
public:
    StringListProviderImpl() : m_cRef(1) { ServerObjIncRef(); }
    ~StringListProviderImpl() { ServerObjDecRef(); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_OutOfProc_IStringListProvider) {
            *ppv = static_cast<__x_TestApp_OutOfProc_IStringListProvider*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IMarshal) { *ppv = nullptr; return E_NOINTERFACE; }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {
        *iidCount = 1;
        *iids = (IID*)CoTaskMemAlloc(sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID___x_TestApp_OutOfProc_IStringListProvider;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.OutOfProc.StringListProvider", 36, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE CreateList(
        __x_TestApp_OutOfProc_IStringList** returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = new (std::nothrow) StringListImpl();
        return *returnValue ? S_OK : E_OUTOFMEMORY;
    }
};

// ---------------------------------------------------------------------------
// CompletedAsyncOp — minimal IAsyncOperation<Int32> that is always completed
// ---------------------------------------------------------------------------

// IAsyncInfo: {00000036-0000-0000-C000-000000000046}
static const IID IID_IAsyncInfo_Local =
    {0x00000036, 0x0000, 0x0000, {0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46}};

// IAsyncOperation<Int32> PIID (from generated proxy code)
static const IID IID_IAsyncOperation_1_Int32_Local =
    {0xc8a72ed6, 0xd07c, 0x5aa7, {0x83, 0xb2, 0x20, 0x20, 0xa5, 0x22, 0x45, 0xaf}};

struct IAsyncInfoVtbl : public IInspectable {
    virtual HRESULT STDMETHODCALLTYPE get_Id(UINT32* id) = 0;
    virtual HRESULT STDMETHODCALLTYPE get_Status(INT32* status) = 0;
    virtual HRESULT STDMETHODCALLTYPE get_ErrorCode(HRESULT* errorCode) = 0;
    virtual HRESULT STDMETHODCALLTYPE Cancel() = 0;
    virtual HRESULT STDMETHODCALLTYPE Close() = 0;
};

struct IAsyncOperation_Int32_Vtbl : public IInspectable {
    virtual HRESULT STDMETHODCALLTYPE put_Completed(IInspectable* handler) = 0;
    virtual HRESULT STDMETHODCALLTYPE get_Completed(IInspectable** handler) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetResults(INT32* results) = 0;
};

class CompletedAsyncOp : public IAsyncInfoVtbl,
                         public IAsyncOperation_Int32_Vtbl {
    LONG m_cRef;
    INT32 m_result;

public:
    CompletedAsyncOp(INT32 result) : m_cRef(1), m_result(result) { ServerObjIncRef(); }
    ~CompletedAsyncOp() { ServerObjDecRef(); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID_IAsyncInfo_Local) {
            *ppv = static_cast<IAsyncInfoVtbl*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IAsyncOperation_1_Int32_Local) {
            *ppv = static_cast<IAsyncOperation_Int32_Vtbl*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IMarshal) { *ppv = nullptr; return E_NOINTERFACE; }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    // IInspectable (routed via IAsyncInfoVtbl)
    HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {
        *iidCount = 2;
        *iids = (IID*)CoTaskMemAlloc(2 * sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID_IAsyncInfo_Local;
        (*iids)[1] = IID_IAsyncOperation_1_Int32_Local;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.OutOfProc.CompletedAsyncOp", 34, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    // IAsyncInfo
    HRESULT STDMETHODCALLTYPE get_Id(UINT32* id) override {
        if (!id) return E_POINTER;
        *id = 1;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE get_Status(INT32* status) override {
        if (!status) return E_POINTER;
        *status = 1; // AsyncStatus::Completed
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE get_ErrorCode(HRESULT* errorCode) override {
        if (!errorCode) return E_POINTER;
        *errorCode = S_OK;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE Cancel() override { return S_OK; }
    HRESULT STDMETHODCALLTYPE Close() override { return S_OK; }

    // IAsyncOperation<Int32>
    HRESULT STDMETHODCALLTYPE put_Completed(IInspectable* handler) override {
        (void)handler;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE get_Completed(IInspectable** handler) override {
        if (!handler) return E_POINTER;
        *handler = nullptr;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetResults(INT32* results) override {
        if (!results) return E_POINTER;
        *results = m_result;
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// AsyncCalculatorImpl — implements IAsyncCalculator
// ---------------------------------------------------------------------------
class AsyncCalculatorImpl : public __x_TestApp_OutOfProc_IAsyncCalculator {
    LONG m_cRef;

public:
    AsyncCalculatorImpl() : m_cRef(1) { ServerObjIncRef(); }
    ~AsyncCalculatorImpl() { ServerObjDecRef(); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_OutOfProc_IAsyncCalculator) {
            *ppv = static_cast<__x_TestApp_OutOfProc_IAsyncCalculator*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID_IMarshal) { *ppv = nullptr; return E_NOINTERFACE; }
        *ppv = nullptr;
        return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return InterlockedIncrement(&m_cRef); }
    ULONG STDMETHODCALLTYPE Release() override {
        ULONG c = InterlockedDecrement(&m_cRef);
        if (c == 0) delete this;
        return c;
    }

    HRESULT STDMETHODCALLTYPE GetIids(ULONG* iidCount, IID** iids) override {
        *iidCount = 1;
        *iids = (IID*)CoTaskMemAlloc(sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID___x_TestApp_OutOfProc_IAsyncCalculator;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.OutOfProc.AsyncCalculator", 33, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE ComputeAsync(INT32 a, INT32 b,
                                            IInspectable** returnValue) override {
        if (!returnValue) return E_POINTER;
        auto* op = new (std::nothrow) CompletedAsyncOp(a + b);
        if (!op) { *returnValue = nullptr; return E_OUTOFMEMORY; }
        // Return the IAsyncOperation<Int32> face so marshal_interface can
        // CoMarshalInterface with IID_IAsyncOperation_1_Int32 successfully
        *returnValue = static_cast<IAsyncOperation_Int32_Vtbl*>(op);
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// MultiClassFactory — switches on CLSID to create the right object
// ---------------------------------------------------------------------------
class MultiClassFactory : public IClassFactory {
    LONG m_cRef;
    const CLSID& m_clsid;

public:
    MultiClassFactory(const CLSID& clsid) : m_cRef(1), m_clsid(clsid) {}

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
        IInspectable* obj = nullptr;
        if (m_clsid == CLSID_Calculator)
            obj = static_cast<__x_TestApp_OutOfProc_ICalculator*>(new (std::nothrow) Calculator());
        else if (m_clsid == CLSID_TypeShowcase)
            obj = static_cast<__x_TestApp_OutOfProc_ITypeShowcase*>(new (std::nothrow) TypeShowcaseImpl());
        else if (m_clsid == CLSID_Notifier)
            obj = static_cast<__x_TestApp_OutOfProc_INotifier*>(new (std::nothrow) NotifierImpl());
        else if (m_clsid == CLSID_PropertyBag)
            obj = static_cast<__x_TestApp_OutOfProc_IPropertyBag*>(new (std::nothrow) PropertyBagImpl());
        else if (m_clsid == CLSID_ComplexOps)
            obj = static_cast<__x_TestApp_OutOfProc_IComplexOps*>(new (std::nothrow) ComplexOpsImpl());
        else if (m_clsid == CLSID_OptionalValues)
            obj = static_cast<__x_TestApp_OutOfProc_IOptionalValues*>(new (std::nothrow) OptionalValuesImpl());
        else if (m_clsid == CLSID_Circle)
            obj = static_cast<__x_TestApp_OutOfProc_ICircle*>(new (std::nothrow) CircleImpl());
        else if (m_clsid == CLSID_EventSource)
            obj = static_cast<__x_TestApp_OutOfProc_IEventSource*>(new (std::nothrow) EventSourceImpl());
        else if (m_clsid == CLSID_DelegateInvoker)
            obj = static_cast<__x_TestApp_OutOfProc_IDelegateInvoker*>(new (std::nothrow) DelegateInvokerImpl());
        else if (m_clsid == CLSID_StringListProvider)
            obj = static_cast<__x_TestApp_OutOfProc_IStringListProvider*>(new (std::nothrow) StringListProviderImpl());
        else if (m_clsid == CLSID_AsyncCalculator)
            obj = static_cast<__x_TestApp_OutOfProc_IAsyncCalculator*>(new (std::nothrow) AsyncCalculatorImpl());
        else return CLASS_E_CLASSNOTAVAILABLE;
        if (!obj) return E_OUTOFMEMORY;
        HRESULT hr = obj->QueryInterface(riid, ppv);
        obj->Release();
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
// Main — register all class factories and wait
// ---------------------------------------------------------------------------
int main() {
    printf("[CalcServer] Starting out-of-proc server...\n");

    HRESULT hr = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    if (FAILED(hr)) {
        printf("[CalcServer] CoInitializeEx failed: 0x%08lX\n", hr);
        return 1;
    }

    g_hShutdown = CreateEventW(nullptr, TRUE, FALSE, nullptr);

    struct FactoryReg {
        const CLSID& clsid;
        DWORD cookie;
    };

    FactoryReg regs[] = {
        { CLSID_Calculator, 0 },
        { CLSID_TypeShowcase, 0 },
        { CLSID_Notifier, 0 },
        { CLSID_PropertyBag, 0 },
        { CLSID_ComplexOps, 0 },
        { CLSID_OptionalValues, 0 },
        { CLSID_Circle, 0 },
        { CLSID_EventSource, 0 },
        { CLSID_DelegateInvoker, 0 },
        { CLSID_StringListProvider, 0 },
        { CLSID_AsyncCalculator, 0 },
    };

    for (auto& reg : regs) {
        auto* factory = new MultiClassFactory(reg.clsid);
        hr = CoRegisterClassObject(reg.clsid, factory,
                                   CLSCTX_LOCAL_SERVER,
                                   REGCLS_MULTIPLEUSE | REGCLS_SUSPENDED,
                                   &reg.cookie);
        if (FAILED(hr)) {
            printf("[CalcServer] CoRegisterClassObject failed: 0x%08lX\n", hr);
            CloseHandle(g_hShutdown);
            CoUninitialize();
            return 1;
        }
    }

    hr = CoResumeClassObjects();
    if (FAILED(hr)) {
        printf("[CalcServer] CoResumeClassObjects failed: 0x%08lX\n", hr);
        for (auto& reg : regs) CoRevokeClassObject(reg.cookie);
        CloseHandle(g_hShutdown);
        CoUninitialize();
        return 1;
    }

    printf("[CalcServer] %d class factories registered. Waiting for clients...\n",
           (int)(sizeof(regs)/sizeof(regs[0])));
    fflush(stdout);

    WaitForSingleObject(g_hShutdown, 60000);

    printf("[CalcServer] Shutting down...\n");
    for (auto& reg : regs) CoRevokeClassObject(reg.cookie);
    CloseHandle(g_hShutdown);
    CoUninitialize();
    printf("[CalcServer] Server exited cleanly.\n");
    return 0;
}
