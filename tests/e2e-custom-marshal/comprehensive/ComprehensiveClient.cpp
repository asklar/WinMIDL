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

    CoUninitialize();
    printf("\n[MTA] Tests complete.\n");
    SetEvent(args->hDone);
    return 0;
}

int main() {
    printf("=== E2E Comprehensive Custom Marshal Test ===\n\n");

    HRESULT hr = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);
    if (FAILED(hr)) {
        printf("FAIL: CoInitializeEx(STA) failed: 0x%08lX\n", hr);
        return 1;
    }
    printf("[STA] COM initialized (apartment-threaded)\n");

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
