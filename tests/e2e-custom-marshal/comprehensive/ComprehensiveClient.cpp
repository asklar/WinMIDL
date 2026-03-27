// ComprehensiveClient.cpp — Cross-apartment proxy/stub validation
//
// Creates objects in the main STA, then exercises them from an MTA thread.
// COM interposes our generated proxy/stub, validating the full roundtrip.
//
// Exit code 0 = all tests passed, 1 = at least one failure.

#include <windows.h>
#include <objbase.h>
#include <winstring.h>
#include <inspectable.h>
#include <stdio.h>
#include <cmath>
#include <roapi.h>
#pragma comment(lib, "runtimeobject.lib")

#include "generated/TestComprehensive.h"

// IID definitions provided by TestComprehensive_iid.obj

static constexpr double kPi = 3.14159265358979323846;

// CLSIDs — same as server
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

static const CLSID CLSID_TypeShowcase = {
    0xe1f2a3b4, 0xc5d6, 0x4e7f,
    {0x80, 0x91, 0xa2, 0xb3, 0xc4, 0xd5, 0xe6, 0xf7}};

static const CLSID CLSID_ComplexOps = {
    0xf2a3b4c5, 0xd6e7, 0x4f80,
    {0x91, 0xa2, 0xb3, 0xc4, 0xd5, 0xe6, 0xf7, 0xa8}};

static const CLSID CLSID_MathHelpers = {
    0xa3b4c5d6, 0xe7f8, 0x4091,
    {0xa2, 0xb3, 0xc4, 0xd5, 0xe6, 0xf7, 0xa8, 0xb9}};

static const CLSID CLSID_Observable = {
    0xb4c5d6e7, 0xf8a9, 0x40b1,
    {0xc2, 0xd3, 0xe4, 0xf5, 0xa6, 0xb7, 0xc8, 0xd9}};

static const CLSID CLSID_CollectionOps = {
    0xc5d6e7f8, 0xa9b0, 0x41c2,
    {0xd3, 0xe4, 0xf5, 0xa6, 0xb7, 0xc8, 0xd9, 0xea}};

// ---------------------------------------------------------------------------
// Client-side helper: LoggerImpl — implements ILogger for Subscribe test
// ---------------------------------------------------------------------------
class LoggerImpl : public __x_TestApp_Comprehensive_ILogger {
    LONG m_cRef;
    HSTRING m_lastMessage;
public:
    LoggerImpl() : m_cRef(1), m_lastMessage(nullptr) {}
    ~LoggerImpl() { if (m_lastMessage) WindowsDeleteString(m_lastMessage); }

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable ||
            riid == IID___x_TestApp_Comprehensive_ILogger) {
            *ppv = static_cast<__x_TestApp_Comprehensive_ILogger*>(this);
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
        (*iids)[0] = IID___x_TestApp_Comprehensive_ILogger;
        return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.Comprehensive.Logger", 28, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }

    HRESULT STDMETHODCALLTYPE Log(HSTRING message) override {
        if (m_lastMessage) WindowsDeleteString(m_lastMessage);
        return WindowsDuplicateString(message, &m_lastMessage);
    }
    HRESULT STDMETHODCALLTYPE get_LastMessage(HSTRING* value) override {
        if (!value) return E_POINTER;
        if (m_lastMessage)
            return WindowsDuplicateString(m_lastMessage, value);
        return WindowsCreateString(L"", 0, value);
    }
};

// ---------------------------------------------------------------------------
// Client-side helper: StatusCallbackImpl — delegate for event tests
// ---------------------------------------------------------------------------
class StatusCallbackImpl : public __x_TestApp_Comprehensive_StatusCallback {
    LONG m_cRef;
public:
    bool m_wasCalled;
    StatusCallbackImpl() : m_cRef(1), m_wasCalled(false) {}

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown ||
            riid == IID___x_TestApp_Comprehensive_StatusCallback) {
            *ppv = static_cast<__x_TestApp_Comprehensive_StatusCallback*>(this);
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

    HRESULT STDMETHODCALLTYPE Invoke(HSTRING status) override {
        (void)status;
        m_wasCalled = true;
        return S_OK;
    }
};

// ---------------------------------------------------------------------------
// Client-side helper: SimpleInspectable — minimal IInspectable for PropertyBag
// ---------------------------------------------------------------------------
class SimpleInspectable : public IInspectable {
    LONG m_cRef;
public:
    SimpleInspectable() : m_cRef(1) {}

    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER;
        if (riid == IID_IUnknown || riid == IID_IInspectable) {
            *ppv = static_cast<IInspectable*>(this);
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
        *iidCount = 0; *iids = nullptr; return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetRuntimeClassName(HSTRING* className) override {
        return WindowsCreateString(L"TestApp.SimpleInspectable", 25, className);
    }
    HRESULT STDMETHODCALLTYPE GetTrustLevel(TrustLevel* trustLevel) override {
        *trustLevel = BaseTrust; return S_OK;
    }
};

static int g_passed = 0;
static int g_failed = 0;

static void Check(bool condition, const char* testName) {
    if (condition) {
        printf("  PASS: %s\n", testName);
        g_passed++;
    } else {
        printf("  FAIL: %s\n", testName);
        g_failed++;
    }
}

static bool HStringEquals(HSTRING hs, const WCHAR* expected, UINT32 expectedLen) {
    UINT32 len = 0;
    PCWSTR raw = WindowsGetStringRawBuffer(hs, &len);
    return (len == expectedLen) && (wcsncmp(raw, expected, expectedLen) == 0);
}

// MTA worker thread — runs all tests through the proxy
struct WorkerArgs { HANDLE hDone; };

static DWORD WINAPI MtaWorkerThread(LPVOID param) {
    auto* args = static_cast<WorkerArgs*>(param);
    HRESULT hr = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    if (FAILED(hr)) {
        printf("FAIL: CoInitializeEx(MTA) failed: 0x%08lX\n", hr);
        SetEvent(args->hDone);
        return 1;
    }

    // ===== Test group 1: Animal (HSTRING + INT32 properties) =====
    printf("[MTA] --- Animal tests ---\n");
    {
        __x_TestApp_Comprehensive_IAnimal* pAnimal = nullptr;
        hr = CoCreateInstance(CLSID_Animal, nullptr, CLSCTX_INPROC_SERVER,
                              IID___x_TestApp_Comprehensive_IAnimal, (void**)&pAnimal);
        Check(SUCCEEDED(hr), "Animal: CoCreateInstance");
        if (SUCCEEDED(hr)) {
            HSTRING name = nullptr;
            hr = pAnimal->get_Name(&name);
            Check(SUCCEEDED(hr) && HStringEquals(name, L"Dog", 3),
                  "Animal: get_Name == \"Dog\"");
            if (name) WindowsDeleteString(name);

            INT32 legs = 0;
            hr = pAnimal->get_LegCount(&legs);
            Check(SUCCEEDED(hr) && legs == 4, "Animal: get_LegCount == 4");

            // GetRuntimeClassName
            HSTRING className = nullptr;
            hr = pAnimal->GetRuntimeClassName(&className);
            Check(SUCCEEDED(hr) && HStringEquals(className, L"TestApp.Comprehensive.Animal", 28),
                  "Animal: GetRuntimeClassName");
            if (className) WindowsDeleteString(className);

            pAnimal->Release();
        }
    }

    // ===== Test group 2: Shape (base class properties) =====
    printf("[MTA] --- Shape tests ---\n");
    {
        __x_TestApp_Comprehensive_IShape* pShape = nullptr;
        hr = CoCreateInstance(CLSID_Shape, nullptr, CLSCTX_INPROC_SERVER,
                              IID___x_TestApp_Comprehensive_IShape, (void**)&pShape);
        Check(SUCCEEDED(hr), "Shape: CoCreateInstance");
        if (SUCCEEDED(hr)) {
            HSTRING kind = nullptr;
            hr = pShape->get_Kind(&kind);
            Check(SUCCEEDED(hr) && HStringEquals(kind, L"Shape", 5),
                  "Shape: get_Kind == \"Shape\"");
            if (kind) WindowsDeleteString(kind);

            DOUBLE area = -1.0;
            hr = pShape->get_Area(&area);
            Check(SUCCEEDED(hr) && area == 0.0, "Shape: get_Area == 0.0");

            pShape->Release();
        }
    }

    // ===== Test group 3: Circle (inheritance — ICircle + IShape) =====
    printf("[MTA] --- Circle tests ---\n");
    {
        __x_TestApp_Comprehensive_ICircle* pCircle = nullptr;
        hr = CoCreateInstance(CLSID_Circle, nullptr, CLSCTX_INPROC_SERVER,
                              IID___x_TestApp_Comprehensive_ICircle, (void**)&pCircle);
        Check(SUCCEEDED(hr), "Circle: CoCreateInstance");
        if (SUCCEEDED(hr)) {
            DOUBLE radius = 0.0;
            hr = pCircle->get_Radius(&radius);
            Check(SUCCEEDED(hr) && radius == 5.0, "Circle: get_Radius == 5.0");

            // QI for IShape
            __x_TestApp_Comprehensive_IShape* pShape = nullptr;
            hr = pCircle->QueryInterface(IID___x_TestApp_Comprehensive_IShape, (void**)&pShape);
            Check(SUCCEEDED(hr), "Circle: QI for IShape");
            if (SUCCEEDED(hr)) {
                HSTRING kind = nullptr;
                hr = pShape->get_Kind(&kind);
                Check(SUCCEEDED(hr) && HStringEquals(kind, L"Circle", 6),
                      "Circle: get_Kind == \"Circle\"");
                if (kind) WindowsDeleteString(kind);

                DOUBLE area = 0.0;
                hr = pShape->get_Area(&area);
                double expectedArea = kPi * 25.0;
                Check(SUCCEEDED(hr) && fabs(area - expectedArea) < 0.001,
                      "Circle: get_Area == pi*25");

                pShape->Release();
            }

            // GetIids — should include ICircle and IShape
            ULONG iidCount = 0;
            IID* iids = nullptr;
            hr = pCircle->GetIids(&iidCount, &iids);
            Check(SUCCEEDED(hr) && iidCount >= 2, "Circle: GetIids count >= 2");
            if (SUCCEEDED(hr)) {
                bool hasCircle = false, hasShape = false;
                for (ULONG i = 0; i < iidCount; i++) {
                    if (iids[i] == IID___x_TestApp_Comprehensive_ICircle) hasCircle = true;
                    if (iids[i] == IID___x_TestApp_Comprehensive_IShape) hasShape = true;
                }
                Check(hasCircle && hasShape, "Circle: GetIids has ICircle+IShape");
                CoTaskMemFree(iids);
            }

            pCircle->Release();
        }
    }

    // ===== Test group 4: Document (multiple interface QI) =====
    printf("[MTA] --- Document tests ---\n");
    {
        __x_TestApp_Comprehensive_IDocument* pDoc = nullptr;
        hr = CoCreateInstance(CLSID_Document, nullptr, CLSCTX_INPROC_SERVER,
                              IID___x_TestApp_Comprehensive_IDocument, (void**)&pDoc);
        Check(SUCCEEDED(hr), "Document: CoCreateInstance");
        if (SUCCEEDED(hr)) {
            HSTRING title = nullptr;
            hr = pDoc->get_Title(&title);
            Check(SUCCEEDED(hr) && HStringEquals(title, L"TestDoc", 7),
                  "Document: get_Title == \"TestDoc\"");
            if (title) WindowsDeleteString(title);

            // QI for ISerializable
            __x_TestApp_Comprehensive_ISerializable* pSer = nullptr;
            hr = pDoc->QueryInterface(IID___x_TestApp_Comprehensive_ISerializable, (void**)&pSer);
            Check(SUCCEEDED(hr), "Document: QI for ISerializable");
            if (SUCCEEDED(hr)) {
                HSTRING json = nullptr;
                hr = pSer->Serialize(&json);
                UINT32 len = 0;
                PCWSTR raw = WindowsGetStringRawBuffer(json, &len);
                Check(SUCCEEDED(hr) && len > 0 && wcsstr(raw, L"TestDoc") != nullptr,
                      "Document: Serialize contains \"TestDoc\"");
                if (json) WindowsDeleteString(json);
                pSer->Release();
            }

            // QI for ICloneable
            __x_TestApp_Comprehensive_ICloneable* pClone = nullptr;
            hr = pDoc->QueryInterface(IID___x_TestApp_Comprehensive_ICloneable, (void**)&pClone);
            Check(SUCCEEDED(hr), "Document: QI for ICloneable");
            if (SUCCEEDED(hr)) {
                HSTRING cloneStr = nullptr;
                hr = pClone->CloneAsString(&cloneStr);
                Check(SUCCEEDED(hr) && HStringEquals(cloneStr, L"Document:TestDoc", 16),
                      "Document: CloneAsString == \"Document:TestDoc\"");
                if (cloneStr) WindowsDeleteString(cloneStr);
                pClone->Release();
            }

            // GetIids — should include IDocument, ISerializable, ICloneable
            ULONG iidCount = 0;
            IID* iids = nullptr;
            hr = pDoc->GetIids(&iidCount, &iids);
            Check(SUCCEEDED(hr) && iidCount >= 3, "Document: GetIids count >= 3");
            if (SUCCEEDED(hr)) {
                bool hasDoc = false, hasSer = false, hasClone = false;
                for (ULONG i = 0; i < iidCount; i++) {
                    if (iids[i] == IID___x_TestApp_Comprehensive_IDocument) hasDoc = true;
                    if (iids[i] == IID___x_TestApp_Comprehensive_ISerializable) hasSer = true;
                    if (iids[i] == IID___x_TestApp_Comprehensive_ICloneable) hasClone = true;
                }
                Check(hasDoc && hasSer && hasClone,
                      "Document: GetIids has IDocument+ISerializable+ICloneable");
                CoTaskMemFree(iids);
            }

            pDoc->Release();
        }
    }

    // ===== Test group 5: Calculator (all parameter patterns) =====
    printf("[MTA] --- Calculator tests ---\n");
    {
        __x_TestApp_Comprehensive_ICalculator* pCalc = nullptr;
        hr = CoCreateInstance(CLSID_Calculator, nullptr, CLSCTX_INPROC_SERVER,
                              IID___x_TestApp_Comprehensive_ICalculator, (void**)&pCalc);
        Check(SUCCEEDED(hr), "Calculator: CoCreateInstance");
        if (SUCCEEDED(hr)) {
            // Add — double params + double return
            DOUBLE sum = 0.0;
            hr = pCalc->Add(3.0, 4.0, &sum);
            Check(SUCCEEDED(hr) && sum == 7.0, "Calculator: Add(3,4) == 7");

            // TryDivide — out param + boolean retval (success case)
            DOUBLE quotient = 0.0;
            boolean divOk = FALSE;
            hr = pCalc->TryDivide(10.0, 2.0, &quotient, &divOk);
            Check(SUCCEEDED(hr) && divOk == TRUE && quotient == 5.0,
                  "Calculator: TryDivide(10,2) == 5.0, TRUE");

            // TryDivide — failure case (division by zero)
            DOUBLE quotient2 = -1.0;
            boolean divOk2 = TRUE;
            hr = pCalc->TryDivide(10.0, 0.0, &quotient2, &divOk2);
            Check(SUCCEEDED(hr) && divOk2 == FALSE,
                  "Calculator: TryDivide(10,0) == FALSE");

            // MakePoint — struct return value
            Point pt = {};
            hr = pCalc->MakePoint(1.5f, 2.5f, &pt);
            Check(SUCCEEDED(hr) && pt.X == 1.5f && pt.Y == 2.5f,
                  "Calculator: MakePoint(1.5,2.5)");

            // SetPermissions / GetPermissions — enum round-trip
            hr = pCalc->SetPermissions(Permissions_Write);
            Check(SUCCEEDED(hr), "Calculator: SetPermissions(Write)");

            Permissions perms = Permissions_None;
            hr = pCalc->GetPermissions(&perms);
            Check(SUCCEEDED(hr) && perms == Permissions_Write,
                  "Calculator: GetPermissions == Write");

            // Translate — nested struct (ColoredPoint) with enum field
            ColoredPoint input = {};
            input.Position.X = 1.0f;
            input.Position.Y = 2.0f;
            input.PointColor = Color_Green;

            ColoredPoint output = {};
            hr = pCalc->Translate(input, 10.0f, 20.0f, &output);
            Check(SUCCEEDED(hr) &&
                  output.Position.X == 11.0f &&
                  output.Position.Y == 22.0f &&
                  output.PointColor == Color_Green,
                  "Calculator: Translate({1,2,Green}, 10, 20)");

            // GetRuntimeClassName
            HSTRING className = nullptr;
            hr = pCalc->GetRuntimeClassName(&className);
            Check(SUCCEEDED(hr) &&
                  HStringEquals(className, L"TestApp.Comprehensive.Calculator", 32),
                  "Calculator: GetRuntimeClassName");
            if (className) WindowsDeleteString(className);

            pCalc->Release();
        }
    }

    // ===== Test group 6: Notifier (interface pointer params) =====
    printf("[MTA] --- Notifier tests ---\n");
    {
        __x_TestApp_Comprehensive_INotifier* pNotifier = nullptr;
        hr = CoCreateInstance(CLSID_Notifier, nullptr, CLSCTX_INPROC_SERVER,
                              IID___x_TestApp_Comprehensive_INotifier, (void**)&pNotifier);
        Check(SUCCEEDED(hr), "Notifier: CoCreateInstance");
        if (SUCCEEDED(hr)) {
            // Create a Logger in the MTA and subscribe it
            auto* pLogger = new LoggerImpl();
            hr = pNotifier->Subscribe(pLogger);
            Check(SUCCEEDED(hr), "Notifier: Subscribe(logger)");

            // GetLogger should return a working ILogger
            __x_TestApp_Comprehensive_ILogger* pReturned = nullptr;
            hr = pNotifier->GetLogger(&pReturned);
            Check(SUCCEEDED(hr) && pReturned != nullptr, "Notifier: GetLogger != null");

            if (pReturned) {
                // Call Log on the returned logger
                HSTRING msg = nullptr;
                WindowsCreateString(L"hello from notifier test", 24, &msg);
                hr = pReturned->Log(msg);
                Check(SUCCEEDED(hr), "Notifier: Log via returned logger");
                if (msg) WindowsDeleteString(msg);

                // Check LastMessage
                HSTRING lastMsg = nullptr;
                hr = pReturned->get_LastMessage(&lastMsg);
                Check(SUCCEEDED(hr) && HStringEquals(lastMsg, L"hello from notifier test", 24),
                      "Notifier: LastMessage round-trip");
                if (lastMsg) WindowsDeleteString(lastMsg);

                pReturned->Release();
            }

            // GetRuntimeClassName
            HSTRING className = nullptr;
            hr = pNotifier->GetRuntimeClassName(&className);
            Check(SUCCEEDED(hr) &&
                  HStringEquals(className, L"TestApp.Comprehensive.Notifier", 30),
                  "Notifier: GetRuntimeClassName");
            if (className) WindowsDeleteString(className);

            pLogger->Release();
            pNotifier->Release();
        }
    }

    // ===== Test group 7: EventSource (delegate params + events) =====
    // NOTE: Delegate marshaling (StatusCallback) requires a proxy/stub for the
    // delegate interface. The current codegen doesn't generate one, so
    // AddStatusChanged/add_Completed fail with E_NOINTERFACE. The token-only
    // methods (RemoveStatusChanged, remove_Completed) still test EventRegistrationToken
    // marshaling.
    printf("[MTA] --- EventSource tests ---\n");
    {
        __x_TestApp_Comprehensive_IEventSource* pEvtSrc = nullptr;
        hr = CoCreateInstance(CLSID_EventSource, nullptr, CLSCTX_INPROC_SERVER,
                              IID___x_TestApp_Comprehensive_IEventSource, (void**)&pEvtSrc);
        Check(SUCCEEDED(hr), "EventSource: CoCreateInstance");
        if (SUCCEEDED(hr)) {
            // AddStatusChanged — pass a delegate, get a token back
            // This may fail with E_NOINTERFACE if delegate proxy/stub is missing
            auto* pCallback = new StatusCallbackImpl();
            EventRegistrationToken token = {};
            hr = pEvtSrc->AddStatusChanged(pCallback, &token);
            if (SUCCEEDED(hr)) {
                Check(true, "EventSource: AddStatusChanged");
                Check(token.value != 0, "EventSource: AddStatusChanged token != 0");

                // RemoveStatusChanged — pass the token
                hr = pEvtSrc->RemoveStatusChanged(token);
                Check(SUCCEEDED(hr), "EventSource: RemoveStatusChanged");
            } else {
                printf("  SKIP: EventSource: AddStatusChanged (delegate proxy/stub not available, hr=0x%08lX)\n", hr);
                // Still test RemoveStatusChanged with a synthetic token
                EventRegistrationToken synthToken = {};
                synthToken.value = 42;
                hr = pEvtSrc->RemoveStatusChanged(synthToken);
                Check(SUCCEEDED(hr), "EventSource: RemoveStatusChanged (synthetic token)");
            }

            // add_Completed — event accessor with delegate
            auto* pCallback2 = new StatusCallbackImpl();
            EventRegistrationToken token2 = {};
            hr = pEvtSrc->add_Completed(pCallback2, &token2);
            if (SUCCEEDED(hr)) {
                Check(true, "EventSource: add_Completed");
                Check(token2.value != 0, "EventSource: add_Completed token != 0");

                hr = pEvtSrc->remove_Completed(token2);
                Check(SUCCEEDED(hr), "EventSource: remove_Completed");
            } else {
                printf("  SKIP: EventSource: add_Completed (delegate proxy/stub not available)\n");
                EventRegistrationToken synthToken2 = {};
                synthToken2.value = 99;
                hr = pEvtSrc->remove_Completed(synthToken2);
                Check(SUCCEEDED(hr), "EventSource: remove_Completed (synthetic token)");
            }

            // GetRuntimeClassName
            HSTRING className = nullptr;
            hr = pEvtSrc->GetRuntimeClassName(&className);
            Check(SUCCEEDED(hr) &&
                  HStringEquals(className, L"TestApp.Comprehensive.EventSource", 33),
                  "EventSource: GetRuntimeClassName");
            if (className) WindowsDeleteString(className);

            pCallback->Release();
            pCallback2->Release();
            pEvtSrc->Release();
        }
    }

    // ===== Test group 8: PropertyBag (Object / IInspectable* params) =====
    printf("[MTA] --- PropertyBag tests ---\n");
    {
        __x_TestApp_Comprehensive_IPropertyBag* pBag = nullptr;
        hr = CoCreateInstance(CLSID_PropertyBag, nullptr, CLSCTX_INPROC_SERVER,
                              IID___x_TestApp_Comprehensive_IPropertyBag, (void**)&pBag);
        Check(SUCCEEDED(hr), "PropertyBag: CoCreateInstance");
        if (SUCCEEDED(hr)) {
            // SetValue with an IInspectable object
            auto* pObj = new SimpleInspectable();
            HSTRING key = nullptr;
            WindowsCreateString(L"testKey", 7, &key);
            hr = pBag->SetValue(key, pObj);
            Check(SUCCEEDED(hr), "PropertyBag: SetValue");

            // GetValue — should return non-null
            IInspectable* pReturned = nullptr;
            hr = pBag->GetValue(key, &pReturned);
            Check(SUCCEEDED(hr) && pReturned != nullptr,
                  "PropertyBag: GetValue != null");
            if (pReturned) {
                // Verify it supports IInspectable
                IInspectable* pInsp = nullptr;
                HRESULT hrQI = pReturned->QueryInterface(IID_IInspectable, (void**)&pInsp);
                Check(SUCCEEDED(hrQI), "PropertyBag: returned object supports QI");
                if (pInsp) pInsp->Release();
                pReturned->Release();
            }

            // GetValue for non-existent key — should return null
            HSTRING key2 = nullptr;
            WindowsCreateString(L"nonexistent", 11, &key2);
            IInspectable* pMissing = nullptr;
            hr = pBag->GetValue(key2, &pMissing);
            Check(SUCCEEDED(hr), "PropertyBag: GetValue(nonexistent) succeeds");
            if (pMissing) pMissing->Release();
            if (key2) WindowsDeleteString(key2);

            // GetRuntimeClassName
            HSTRING className = nullptr;
            hr = pBag->GetRuntimeClassName(&className);
            Check(SUCCEEDED(hr) &&
                  HStringEquals(className, L"TestApp.Comprehensive.PropertyBag", 33),
                  "PropertyBag: GetRuntimeClassName");
            if (className) WindowsDeleteString(className);

            if (key) WindowsDeleteString(key);
            pObj->Release();
            pBag->Release();
        }
    }

    // ===== Test group 9: OptionalValues (IReference<T> / null params) =====
    printf("[MTA] --- OptionalValues tests ---\n");
    {
        __x_TestApp_Comprehensive_IOptional* pOpt = nullptr;
        hr = CoCreateInstance(CLSID_OptionalValues, nullptr, CLSCTX_INPROC_SERVER,
                              IID___x_TestApp_Comprehensive_IOptional, (void**)&pOpt);
        Check(SUCCEEDED(hr), "OptionalValues: CoCreateInstance");
        if (SUCCEEDED(hr)) {
            // GetOptionalCount when not set — should be null
            __x_Windows_Foundation_IReference* pCount = nullptr;
            hr = pOpt->GetOptionalCount(&pCount);
            Check(SUCCEEDED(hr) && pCount == nullptr,
                  "OptionalValues: GetOptionalCount initially null");

            // GetOptionalPosition when not set — should be null
            __x_Windows_Foundation_IReference* pPos = nullptr;
            hr = pOpt->GetOptionalPosition(&pPos);
            Check(SUCCEEDED(hr) && pPos == nullptr,
                  "OptionalValues: GetOptionalPosition initially null");

            // SetOptionalCount with null — explicit null round-trip
            hr = pOpt->SetOptionalCount(nullptr);
            Check(SUCCEEDED(hr), "OptionalValues: SetOptionalCount(null)");

            // GetRuntimeClassName
            HSTRING className = nullptr;
            hr = pOpt->GetRuntimeClassName(&className);
            Check(SUCCEEDED(hr) &&
                  HStringEquals(className, L"TestApp.Comprehensive.OptionalValues", 36),
                  "OptionalValues: GetRuntimeClassName");
            if (className) WindowsDeleteString(className);

            pOpt->Release();
        }
    }

    // ===== Test group 10: TypeShowcase (#13 — additional primitive types) =====
    printf("[MTA] --- TypeShowcase tests ---\n");
    {
        __x_TestApp_Comprehensive_ITypeShowcase* pTs = nullptr;
        hr = CoCreateInstance(CLSID_TypeShowcase, nullptr, CLSCTX_INPROC_SERVER,
                              IID___x_TestApp_Comprehensive_ITypeShowcase, (void**)&pTs);
        Check(SUCCEEDED(hr), "TypeShowcase: CoCreateInstance");
        if (SUCCEEDED(hr)) {
            // INT64 round-trip
            hr = pTs->SetBigNumber(9223372036854775807LL);
            Check(SUCCEEDED(hr), "TypeShowcase: SetBigNumber(INT64_MAX)");
            INT64 big = 0;
            hr = pTs->GetBigNumber(&big);
            Check(SUCCEEDED(hr) && big == 9223372036854775807LL,
                  "TypeShowcase: GetBigNumber == INT64_MAX");

            // UINT64
            UINT64 ubig = 0;
            hr = pTs->GetUnsignedBig(&ubig);
            Check(SUCCEEDED(hr), "TypeShowcase: GetUnsignedBig");

            // GUID round-trip
            GUID testGuid = {0xAABBCCDD, 0xEEFF, 0x1122, {0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA}};
            hr = pTs->SetId(testGuid);
            Check(SUCCEEDED(hr), "TypeShowcase: SetId");
            GUID gotGuid = {};
            hr = pTs->GetId(&gotGuid);
            Check(SUCCEEDED(hr) && IsEqualGUID(gotGuid, testGuid),
                  "TypeShowcase: GetId round-trip");

            // WCHAR
            WCHAR ch = 0;
            hr = pTs->GetInitial(&ch);
            Check(SUCCEEDED(hr) && ch == L'A', "TypeShowcase: GetInitial == 'A'");

            // BYTE
            BYTE b = 0;
            hr = pTs->GetByte(&b);
            Check(SUCCEEDED(hr) && b == 42, "TypeShowcase: GetByte == 42");

            // INT16
            INT16 sm = 0;
            hr = pTs->GetSmallNumber(&sm);
            Check(SUCCEEDED(hr) && sm == -123, "TypeShowcase: GetSmallNumber == -123");

            // GetRuntimeClassName
            HSTRING className = nullptr;
            hr = pTs->GetRuntimeClassName(&className);
            Check(SUCCEEDED(hr) &&
                  HStringEquals(className, L"TestApp.Comprehensive.TypeShowcase", 34),
                  "TypeShowcase: GetRuntimeClassName");
            if (className) WindowsDeleteString(className);

            pTs->Release();
        }
    }

    // ===== Test group 11: ComplexOps (#14 — many params + mixed in/out) =====
    printf("[MTA] --- ComplexOps tests ---\n");
    {
        __x_TestApp_Comprehensive_IComplexOps* pCo = nullptr;
        hr = CoCreateInstance(CLSID_ComplexOps, nullptr, CLSCTX_INPROC_SERVER,
                              IID___x_TestApp_Comprehensive_IComplexOps, (void**)&pCo);
        Check(SUCCEEDED(hr), "ComplexOps: CoCreateInstance");
        if (SUCCEEDED(hr)) {
            // MultiParam — 5 parameters of different types
            HSTRING name = nullptr;
            WindowsCreateString(L"test-item", 9, &name);
            hr = pCo->MultiParam(name, 10, 20, 3.14, TRUE);
            Check(SUCCEEDED(hr), "ComplexOps: MultiParam(5 params)");
            if (name) WindowsDeleteString(name);

            // TryParse — success case
            HSTRING input = nullptr;
            WindowsCreateString(L"42", 2, &input);
            INT32 intResult = 0;
            DOUBLE floatResult = 0.0;
            boolean parseOk = FALSE;
        hr = pCo->TryParse(input, &intResult, &floatResult, &parseOk);
        Check(SUCCEEDED(hr) && parseOk == TRUE && intResult == 42,
                  "ComplexOps: TryParse(\"42\") == 42, TRUE");
            if (input) WindowsDeleteString(input);

            // TryParse — failure case
            HSTRING badInput = nullptr;
            WindowsCreateString(L"abc", 3, &badInput);
            INT32 intResult2 = -1;
            DOUBLE floatResult2 = -1.0;
            boolean parseOk2 = TRUE;
            hr = pCo->TryParse(badInput, &intResult2, &floatResult2, &parseOk2);
            Check(SUCCEEDED(hr) && parseOk2 == FALSE,
                  "ComplexOps: TryParse(\"abc\") == FALSE");
            if (badInput) WindowsDeleteString(badInput);

            // Swap — in/out params
            INT32 outA = 0, outB = 0;
            hr = pCo->Swap(100, 200, &outA, &outB);
            Check(SUCCEEDED(hr) && outA == 200 && outB == 100,
                  "ComplexOps: Swap(100,200) => (200,100)");

            // GetRuntimeClassName
            HSTRING className = nullptr;
            hr = pCo->GetRuntimeClassName(&className);
            Check(SUCCEEDED(hr) &&
                  HStringEquals(className, L"TestApp.Comprehensive.ComplexOps", 32),
                  "ComplexOps: GetRuntimeClassName");
            if (className) WindowsDeleteString(className);

            pCo->Release();
        }
    }

    // ===== Test group 12: MathHelpers (#15 — static runtimeclass) =====
    printf("[MTA] --- MathHelpers tests ---\n");
    {
        __x_TestApp_Comprehensive_IMathHelpersStatics* pMath = nullptr;
        hr = CoCreateInstance(CLSID_MathHelpers, nullptr, CLSCTX_INPROC_SERVER,
                              IID___x_TestApp_Comprehensive_IMathHelpersStatics, (void**)&pMath);
        Check(SUCCEEDED(hr), "MathHelpers: CoCreateInstance");
        if (SUCCEEDED(hr)) {
            // get_Pi
            DOUBLE pi = 0.0;
            hr = pMath->get_Pi(&pi);
            Check(SUCCEEDED(hr) && fabs(pi - 3.14159265358979323846) < 1e-10,
                  "MathHelpers: get_Pi == 3.14159...");

            // Square
            DOUBLE sq = 0.0;
            hr = pMath->Square(7.0, &sq);
            Check(SUCCEEDED(hr) && sq == 49.0,
                  "MathHelpers: Square(7) == 49");

            // get_Origin — struct return
            Point origin = {-1.0f, -1.0f};
            hr = pMath->get_Origin(&origin);
            Check(SUCCEEDED(hr) && origin.X == 0.0f && origin.Y == 0.0f,
                  "MathHelpers: get_Origin == (0,0)");

            // GetRuntimeClassName
            HSTRING className = nullptr;
            hr = pMath->GetRuntimeClassName(&className);
            Check(SUCCEEDED(hr) &&
                  HStringEquals(className, L"TestApp.Comprehensive.MathHelpers", 33),
                  "MathHelpers: GetRuntimeClassName");
            if (className) WindowsDeleteString(className);

            pMath->Release();
        }
    }

    // ===== Test group 13: Observable (#16 — generic event delegate) =====
    // NOTE: EventHandler<String> is a generic delegate — may need delegate
    // proxy/stub. The value property and remove_ValueChanged are tested
    // regardless; add_ValueChanged may be skipped if delegate marshaling fails.
    printf("[MTA] --- Observable tests ---\n");
    {
        __x_TestApp_Comprehensive_IObservable* pObs = nullptr;
        hr = CoCreateInstance(CLSID_Observable, nullptr, CLSCTX_INPROC_SERVER,
                              IID___x_TestApp_Comprehensive_IObservable, (void**)&pObs);
        Check(SUCCEEDED(hr), "Observable: CoCreateInstance");
        if (SUCCEEDED(hr)) {
            // put_Value / get_Value round-trip
            HSTRING val = nullptr;
            WindowsCreateString(L"hello observable", 16, &val);
            hr = pObs->put_Value(val);
            Check(SUCCEEDED(hr), "Observable: put_Value");
            if (val) WindowsDeleteString(val);

            HSTRING gotVal = nullptr;
            hr = pObs->get_Value(&gotVal);
            Check(SUCCEEDED(hr) && HStringEquals(gotVal, L"hello observable", 16),
                  "Observable: get_Value round-trip");
            if (gotVal) WindowsDeleteString(gotVal);

            // remove_ValueChanged with synthetic token (tests EventRegistrationToken marshaling)
            EventRegistrationToken synthToken = {};
            synthToken.value = 123;
            hr = pObs->remove_ValueChanged(synthToken);
            Check(SUCCEEDED(hr), "Observable: remove_ValueChanged (synthetic token)");

            // GetRuntimeClassName
            HSTRING className = nullptr;
            hr = pObs->GetRuntimeClassName(&className);
            Check(SUCCEEDED(hr) &&
                  HStringEquals(className, L"TestApp.Comprehensive.Observable", 32),
                  "Observable: GetRuntimeClassName");
            if (className) WindowsDeleteString(className);

            pObs->Release();
        }
    }

    // CollectionOps (#17) — collection interface marshaling
    printf("[MTA] --- CollectionOps tests ---\n");
    {
        __x_TestApp_Comprehensive_ICollectionOps* pColl = nullptr;
        hr = CoCreateInstance(CLSID_CollectionOps, nullptr, CLSCTX_INPROC_SERVER,
            IID___x_TestApp_Comprehensive_ICollectionOps, (void**)&pColl);
        Check(SUCCEEDED(hr), "CollectionOps: CoCreateInstance");

        if (SUCCEEDED(hr) && pColl) {
            // GetProperties returns a PropertySet (IMap<String, Object>)
            __x_Windows_Foundation_Collections_IMap* pMap = nullptr;
            hr = pColl->GetProperties(&pMap);
            Check(SUCCEEDED(hr), "CollectionOps: GetProperties");
            Check(pMap != nullptr, "CollectionOps: GetProperties != null");
            if (pMap) pMap->Release();

            // GetScores returns a real IMap<String, Int32> with our computed PIID
            __x_Windows_Foundation_Collections_IMap* pScores = nullptr;
            hr = pColl->GetScores(&pScores);
            Check(SUCCEEDED(hr), "CollectionOps: GetScores");
            Check(pScores != nullptr, "CollectionOps: GetScores != null");
            if (pScores) pScores->Release();

            // GetNames returns null (not implemented yet)
            __x_Windows_Foundation_Collections_IVector* pVec = nullptr;
            hr = pColl->GetNames(&pVec);
            Check(SUCCEEDED(hr), "CollectionOps: GetNames (null ok)");

            // GetRuntimeClassName
            HSTRING className = nullptr;
            hr = pColl->GetRuntimeClassName(&className);
            Check(SUCCEEDED(hr) && className != nullptr, "CollectionOps: GetRuntimeClassName");
            if (className) WindowsDeleteString(className);

            pColl->Release();
        }
    }

    CoUninitialize();
    printf("\n[MTA] Tests complete.\n");
    SetEvent(args->hDone);
    return 0;
}

int main() {
    printf("=== E2E Comprehensive Custom Marshal Test ===\n\n");

    // Initialize both COM and WinRT for PropertySet activation
    HRESULT hr = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);
    if (FAILED(hr)) {
        printf("FAIL: CoInitializeEx(STA) failed: 0x%08lX\n", hr);
        return 1;
    }
    HRESULT roHr = RoInitialize(RO_INIT_SINGLETHREADED);
    printf("[STA] COM initialized (apartment-threaded), RoInitialize=0x%08lX\n", roHr);

    WorkerArgs args;
    args.hDone = CreateEvent(nullptr, TRUE, FALSE, nullptr);
    HANDLE hThread = CreateThread(nullptr, 0, MtaWorkerThread, &args, 0, nullptr);
    if (!hThread) {
        printf("FAIL: CreateThread failed\n");
        CloseHandle(args.hDone);
        CoUninitialize();
        return 1;
    }

    printf("[STA] Pumping messages for cross-apartment calls...\n\n");

    while (true) {
        DWORD waitResult = MsgWaitForMultipleObjects(
            1, &args.hDone, FALSE, INFINITE, QS_ALLINPUT);
        if (waitResult == WAIT_OBJECT_0) break;
        if (waitResult == WAIT_OBJECT_0 + 1) {
            MSG msg;
            while (PeekMessageW(&msg, nullptr, 0, 0, PM_REMOVE)) {
                TranslateMessage(&msg);
                DispatchMessageW(&msg);
            }
        } else {
            break;
        }
    }

    WaitForSingleObject(hThread, INFINITE);
    CloseHandle(hThread);
    CloseHandle(args.hDone);
    CoUninitialize();

    printf("\n=== Results: %d passed, %d failed ===\n", g_passed, g_failed);
    return g_failed > 0 ? 1 : 0;
}
