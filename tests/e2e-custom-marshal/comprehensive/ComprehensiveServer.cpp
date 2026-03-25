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
        &CLSID_Animal, &CLSID_Shape, &CLSID_Circle, &CLSID_Document, &CLSID_Calculator
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
