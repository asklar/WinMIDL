// ComprehensiveServer.cpp — In-proc COM server implementing all runtimeclasses
// for the comprehensive proxy/stub e2e test.

#include <windows.h>
#include <winstring.h>
#include <inspectable.h>
#include <new>
#include <cmath>

#include "generated/TestComprehensive.h"

// IID definitions provided by TestComprehensive_iid.obj

// CLSIDs — deterministic UUID v5 of each class name
static const CLSID CLSID_Animal = {
    0x6f33328a, 0xb1cf, 0x1a52,
    {0x8b, 0x6c, 0x00, 0x80, 0x19, 0x83, 0x81, 0x05}};

static const CLSID CLSID_Shape = {
    0xfd4f3844, 0x4389, 0xd352,
    {0xbc, 0x91, 0x3d, 0x34, 0x86, 0x70, 0xf8, 0x3e}};

static const CLSID CLSID_Circle = {
    0x8aa503fe, 0xeb8a, 0x655f,
    {0x91, 0x95, 0x12, 0xd9, 0x80, 0x0b, 0x20, 0x77}};

static const CLSID CLSID_Document = {
    0x05b429f5, 0x72c9, 0xe559,
    {0x9d, 0x1d, 0x2c, 0x09, 0xb7, 0x01, 0xde, 0xb4}};

static const CLSID CLSID_Calculator = {
    0xfe8df0e1, 0x4f9d, 0x4851,
    {0xb4, 0x94, 0x0b, 0x41, 0xab, 0x81, 0x2a, 0x5d}};

static const CLSID CLSID_Notifier = {
    0xa1b2c3d4, 0xe5f6, 0x4a7b,
    {0x8c, 0x9d, 0x0e, 0x1f, 0x2a, 0x3b, 0x4c, 0x59}};

static const CLSID CLSID_EventSource = {
    0xb2c3d4e5, 0xf6a7, 0x4b8c,
    {0x9d, 0x0e, 0x1f, 0x2a, 0x3b, 0x4c, 0x5d, 0x6e}};

static const CLSID CLSID_PropertyBag = {
    0xc3d4e5f6, 0xa7b8, 0x4c9d,
    {0x0e, 0x1f, 0x2a, 0x3b, 0x4c, 0x5d, 0x6e, 0x7f}};

static const CLSID CLSID_OptionalValues = {
    0xd4e5f6a7, 0xb8c9, 0x4d0e,
    {0x1f, 0x2a, 0x3b, 0x4c, 0x5d, 0x6e, 0x7f, 0x80}};

static constexpr double kPi = 3.14159265358979323846;
static LONG g_cDllRef = 0;

// ---------------------------------------------------------------------------
// AnimalImpl — implements IAnimal
// ---------------------------------------------------------------------------
class AnimalImpl : public __x_TestApp_Comprehensive_IAnimal {
    LONG m_cRef;
public:
    AnimalImpl() : m_cRef(1) { InterlockedIncrement(&g_cDllRef); }
    ~AnimalImpl() { InterlockedDecrement(&g_cDllRef); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_Comprehensive_IAnimal) {
            *ppv = static_cast<__x_TestApp_Comprehensive_IAnimal*>(this);
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
        (*iids)[0] = IID___x_TestApp_Comprehensive_IAnimal;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.Comprehensive.Animal", 28, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE get_Name(HSTRING* value) override {
        if (!value) return E_POINTER;
        return WindowsCreateString(L"Dog", 3, value);
    }
    HRESULT STDMETHODCALLTYPE get_LegCount(INT32* value) override {
        if (!value) return E_POINTER;
        *value = 4;
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// ShapeImpl — implements IShape (synthesized default interface for Shape)
// ---------------------------------------------------------------------------
class ShapeImpl : public __x_TestApp_Comprehensive_IShape {
    LONG m_cRef;
public:
    ShapeImpl() : m_cRef(1) { InterlockedIncrement(&g_cDllRef); }
    ~ShapeImpl() { InterlockedDecrement(&g_cDllRef); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_Comprehensive_IShape) {
            *ppv = static_cast<__x_TestApp_Comprehensive_IShape*>(this);
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
        (*iids)[0] = IID___x_TestApp_Comprehensive_IShape;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.Comprehensive.Shape", 27, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE get_Kind(HSTRING* value) override {
        if (!value) return E_POINTER;
        return WindowsCreateString(L"Shape", 5, value);
    }
    HRESULT STDMETHODCALLTYPE get_Area(DOUBLE* value) override {
        if (!value) return E_POINTER;
        *value = 0.0;
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// CircleImpl — implements ICircle + IShape (inheritance)
// ---------------------------------------------------------------------------
class CircleImpl : public __x_TestApp_Comprehensive_ICircle,
                   public __x_TestApp_Comprehensive_IShape {
    LONG m_cRef;
    double m_radius;
public:
    CircleImpl() : m_cRef(1), m_radius(5.0) { InterlockedIncrement(&g_cDllRef); }
    ~CircleImpl() { InterlockedDecrement(&g_cDllRef); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_Comprehensive_ICircle) {
            *ppv = static_cast<__x_TestApp_Comprehensive_ICircle*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID___x_TestApp_Comprehensive_IShape) {
            *ppv = static_cast<__x_TestApp_Comprehensive_IShape*>(this);
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
        (*iids)[0] = IID___x_TestApp_Comprehensive_ICircle;
        (*iids)[1] = IID___x_TestApp_Comprehensive_IShape;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.Comprehensive.Circle", 28, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    // ICircle
    HRESULT STDMETHODCALLTYPE get_Radius(DOUBLE* value) override {
        if (!value) return E_POINTER;
        *value = m_radius;
        return S_OK;
    }

    // IShape
    HRESULT STDMETHODCALLTYPE get_Kind(HSTRING* value) override {
        if (!value) return E_POINTER;
        return WindowsCreateString(L"Circle", 6, value);
    }
    HRESULT STDMETHODCALLTYPE get_Area(DOUBLE* value) override {
        if (!value) return E_POINTER;
        *value = kPi * m_radius * m_radius;
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// DocumentImpl — implements IDocument + ISerializable + ICloneable
// ---------------------------------------------------------------------------
class DocumentImpl : public __x_TestApp_Comprehensive_IDocument,
                     public __x_TestApp_Comprehensive_ISerializable,
                     public __x_TestApp_Comprehensive_ICloneable {
    LONG m_cRef;
public:
    DocumentImpl() : m_cRef(1) { InterlockedIncrement(&g_cDllRef); }
    ~DocumentImpl() { InterlockedDecrement(&g_cDllRef); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_Comprehensive_IDocument) {
            *ppv = static_cast<__x_TestApp_Comprehensive_IDocument*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID___x_TestApp_Comprehensive_ISerializable) {
            *ppv = static_cast<__x_TestApp_Comprehensive_ISerializable*>(this);
            AddRef();
            return S_OK;
        }
        if (riid == IID___x_TestApp_Comprehensive_ICloneable) {
            *ppv = static_cast<__x_TestApp_Comprehensive_ICloneable*>(this);
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
        *iidCount = 3;
        *iids = (IID*)CoTaskMemAlloc(3 * sizeof(IID));
        if (!*iids) return E_OUTOFMEMORY;
        (*iids)[0] = IID___x_TestApp_Comprehensive_IDocument;
        (*iids)[1] = IID___x_TestApp_Comprehensive_ISerializable;
        (*iids)[2] = IID___x_TestApp_Comprehensive_ICloneable;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.Comprehensive.Document", 30, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    // IDocument
    HRESULT STDMETHODCALLTYPE get_Title(HSTRING* value) override {
        if (!value) return E_POINTER;
        return WindowsCreateString(L"TestDoc", 7, value);
    }

    // ISerializable
    HRESULT STDMETHODCALLTYPE Serialize(HSTRING* returnValue) override {
        if (!returnValue) return E_POINTER;
        const WCHAR json[] = L"{\"type\":\"Document\",\"title\":\"TestDoc\"}";
        return WindowsCreateString(json, (UINT32)wcslen(json), returnValue);
    }

    // ICloneable
    HRESULT STDMETHODCALLTYPE CloneAsString(HSTRING* returnValue) override {
        if (!returnValue) return E_POINTER;
        return WindowsCreateString(L"Document:TestDoc", 16, returnValue);
    }
};

// ---------------------------------------------------------------------------
// CalculatorImpl — implements ICalculator
// ---------------------------------------------------------------------------
class CalculatorImpl : public __x_TestApp_Comprehensive_ICalculator {
    LONG m_cRef;
    Permissions m_perms;
public:
    CalculatorImpl() : m_cRef(1), m_perms(Permissions_None) {
        InterlockedIncrement(&g_cDllRef);
    }
    ~CalculatorImpl() { InterlockedDecrement(&g_cDllRef); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_Comprehensive_ICalculator) {
            *ppv = static_cast<__x_TestApp_Comprehensive_ICalculator*>(this);
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
        (*iids)[0] = IID___x_TestApp_Comprehensive_ICalculator;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.Comprehensive.Calculator", 32, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE Add(DOUBLE a, DOUBLE b, DOUBLE* returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = a + b;
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE TryDivide(DOUBLE num, DOUBLE denom,
                                         DOUBLE* result, boolean* returnValue) override {
        if (!result || !returnValue) return E_POINTER;
        if (denom == 0.0) {
            *result = 0.0;
            *returnValue = FALSE;
        } else {
            *result = num / denom;
            *returnValue = TRUE;
        }
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
// NotifierImpl — implements INotifier (interface pointer params)
// ---------------------------------------------------------------------------
class NotifierImpl : public __x_TestApp_Comprehensive_INotifier {
    LONG m_cRef;
    __x_TestApp_Comprehensive_ILogger* m_logger;
public:
    NotifierImpl() : m_cRef(1), m_logger(nullptr) { InterlockedIncrement(&g_cDllRef); }
    ~NotifierImpl() {
        if (m_logger) m_logger->Release();
        InterlockedDecrement(&g_cDllRef);
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_Comprehensive_INotifier) {
            *ppv = static_cast<__x_TestApp_Comprehensive_INotifier*>(this);
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
        (*iids)[0] = IID___x_TestApp_Comprehensive_INotifier;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.Comprehensive.Notifier", 30, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE Subscribe(__x_TestApp_Comprehensive_ILogger* logger) override {
        if (m_logger) m_logger->Release();
        m_logger = logger;
        if (m_logger) m_logger->AddRef();
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetLogger(__x_TestApp_Comprehensive_ILogger** returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = m_logger;
        if (m_logger) m_logger->AddRef();
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// EventSourceImpl — implements IEventSource (delegate params + events)
// ---------------------------------------------------------------------------
class EventSourceImpl : public __x_TestApp_Comprehensive_IEventSource {
    LONG m_cRef;
    __x_TestApp_Comprehensive_StatusCallback* m_statusHandler;
    __x_TestApp_Comprehensive_StatusCallback* m_completedHandler;
    LONG m_nextToken;
public:
    EventSourceImpl()
        : m_cRef(1), m_statusHandler(nullptr), m_completedHandler(nullptr), m_nextToken(0)
    { InterlockedIncrement(&g_cDllRef); }
    ~EventSourceImpl() {
        if (m_statusHandler) m_statusHandler->Release();
        if (m_completedHandler) m_completedHandler->Release();
        InterlockedDecrement(&g_cDllRef);
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_Comprehensive_IEventSource) {
            *ppv = static_cast<__x_TestApp_Comprehensive_IEventSource*>(this);
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
        (*iids)[0] = IID___x_TestApp_Comprehensive_IEventSource;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.Comprehensive.EventSource", 33, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE AddStatusChanged(
            __x_TestApp_Comprehensive_StatusCallback* handler,
            EventRegistrationToken* returnValue) override {
        if (!returnValue) return E_POINTER;
        if (m_statusHandler) m_statusHandler->Release();
        m_statusHandler = handler;
        if (m_statusHandler) m_statusHandler->AddRef();
        returnValue->value = InterlockedIncrement(&m_nextToken);
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE RemoveStatusChanged(EventRegistrationToken token) override {
        (void)token;
        if (m_statusHandler) { m_statusHandler->Release(); m_statusHandler = nullptr; }
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE add_Completed(
            __x_TestApp_Comprehensive_StatusCallback* handler,
            EventRegistrationToken* token) override {
        if (!token) return E_POINTER;
        if (m_completedHandler) m_completedHandler->Release();
        m_completedHandler = handler;
        if (m_completedHandler) m_completedHandler->AddRef();
        token->value = InterlockedIncrement(&m_nextToken);
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE remove_Completed(EventRegistrationToken token) override {
        (void)token;
        if (m_completedHandler) { m_completedHandler->Release(); m_completedHandler = nullptr; }
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// PropertyBagImpl — implements IPropertyBag (Object / IInspectable* params)
// ---------------------------------------------------------------------------
class PropertyBagImpl : public __x_TestApp_Comprehensive_IPropertyBag {
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
        InterlockedIncrement(&g_cDllRef);
    }
    ~PropertyBagImpl() {
        for (int i = 0; i < m_count; i++) {
            if (m_entries[i].key) WindowsDeleteString(m_entries[i].key);
            if (m_entries[i].value) m_entries[i].value->Release();
        }
        InterlockedDecrement(&g_cDllRef);
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_Comprehensive_IPropertyBag) {
            *ppv = static_cast<__x_TestApp_Comprehensive_IPropertyBag*>(this);
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
        (*iids)[0] = IID___x_TestApp_Comprehensive_IPropertyBag;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.Comprehensive.PropertyBag", 33, className);
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
// OptionalValuesImpl — implements IOptional (IReference<T> params)
// ---------------------------------------------------------------------------
class OptionalValuesImpl : public __x_TestApp_Comprehensive_IOptional {
    LONG m_cRef;
    __x_Windows_Foundation_IReference* m_count;
    __x_Windows_Foundation_IReference* m_position;
public:
    OptionalValuesImpl() : m_cRef(1), m_count(nullptr), m_position(nullptr) {
        InterlockedIncrement(&g_cDllRef);
    }
    ~OptionalValuesImpl() {
        if (m_count) m_count->Release();
        if (m_position) m_position->Release();
        InterlockedDecrement(&g_cDllRef);
    }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_Comprehensive_IOptional) {
            *ppv = static_cast<__x_TestApp_Comprehensive_IOptional*>(this);
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
        (*iids)[0] = IID___x_TestApp_Comprehensive_IOptional;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.Comprehensive.OptionalValues", 36, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE GetOptionalCount(__x_Windows_Foundation_IReference** returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = m_count;
        if (m_count) m_count->AddRef();
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE SetOptionalCount(__x_Windows_Foundation_IReference* count) override {
        if (m_count) m_count->Release();
        m_count = count;
        if (m_count) m_count->AddRef();
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetOptionalPosition(__x_Windows_Foundation_IReference** returnValue) override {
        if (!returnValue) return E_POINTER;
        *returnValue = m_position;
        if (m_position) m_position->AddRef();
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// ClassFactory — switches on CLSID to create the right object
// ---------------------------------------------------------------------------
class ClassFactory : public IClassFactory {
    LONG m_cRef;
    const CLSID& m_clsid;
public:
    ClassFactory(const CLSID& clsid) : m_cRef(1), m_clsid(clsid) {}

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
        // Use explicit casts to resolve ambiguity for multiply-inheriting classes
        IInspectable* obj = nullptr;
        if (m_clsid == CLSID_Animal)
            obj = static_cast<__x_TestApp_Comprehensive_IAnimal*>(new (std::nothrow) AnimalImpl());
        else if (m_clsid == CLSID_Shape)
            obj = static_cast<__x_TestApp_Comprehensive_IShape*>(new (std::nothrow) ShapeImpl());
        else if (m_clsid == CLSID_Circle)
            obj = static_cast<__x_TestApp_Comprehensive_ICircle*>(new (std::nothrow) CircleImpl());
        else if (m_clsid == CLSID_Document)
            obj = static_cast<__x_TestApp_Comprehensive_IDocument*>(new (std::nothrow) DocumentImpl());
        else if (m_clsid == CLSID_Calculator)
            obj = static_cast<__x_TestApp_Comprehensive_ICalculator*>(new (std::nothrow) CalculatorImpl());
        else if (m_clsid == CLSID_Notifier)
            obj = static_cast<__x_TestApp_Comprehensive_INotifier*>(new (std::nothrow) NotifierImpl());
        else if (m_clsid == CLSID_EventSource)
            obj = static_cast<__x_TestApp_Comprehensive_IEventSource*>(new (std::nothrow) EventSourceImpl());
        else if (m_clsid == CLSID_PropertyBag)
            obj = static_cast<__x_TestApp_Comprehensive_IPropertyBag*>(new (std::nothrow) PropertyBagImpl());
        else if (m_clsid == CLSID_OptionalValues)
            obj = static_cast<__x_TestApp_Comprehensive_IOptional*>(new (std::nothrow) OptionalValuesImpl());
        else return CLASS_E_CLASSNOTAVAILABLE;
        if (!obj) return E_OUTOFMEMORY;
        HRESULT hr = obj->QueryInterface(riid, ppv);
        obj->Release();
        return hr;
    }

    HRESULT STDMETHODCALLTYPE LockServer(BOOL fLock) override {
        if (fLock) InterlockedIncrement(&g_cDllRef);
        else InterlockedDecrement(&g_cDllRef);
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// DLL exports
// ---------------------------------------------------------------------------
extern "C" {

HRESULT STDAPICALLTYPE DllGetClassObject(REFCLSID rclsid, REFIID riid, void** ppv) {
    static const CLSID* knownClsids[] = {
        &CLSID_Animal, &CLSID_Shape, &CLSID_Circle, &CLSID_Document, &CLSID_Calculator,
        &CLSID_Notifier, &CLSID_EventSource, &CLSID_PropertyBag, &CLSID_OptionalValues
    };
    for (auto* c : knownClsids) {
        if (rclsid == *c) {
            auto* factory = new (std::nothrow) ClassFactory(*c);
            if (!factory) return E_OUTOFMEMORY;
            HRESULT hr = factory->QueryInterface(riid, ppv);
            factory->Release();
            return hr;
        }
    }
    return CLASS_E_CLASSNOTAVAILABLE;
}

HRESULT STDAPICALLTYPE DllCanUnloadNow() {
    return g_cDllRef == 0 ? S_OK : S_FALSE;
}

} // extern "C"
