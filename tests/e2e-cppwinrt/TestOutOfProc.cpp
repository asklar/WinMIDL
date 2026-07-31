// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// TestOutOfProc.cpp — Out-of-proc C++/WinRT E2E test client.
// Creates Widget via CoCreateInstance (CLSCTX_LOCAL_SERVER) and exercises
// properties/methods through the proxy/stub layer.
// Does NOT include Widget.h — uses only the consumer projection header.

#include "winrt/TestCppWinRT.h"
#include <windows.h>
#include <stdio.h>
#include <cmath>

// CLSID must match ServerMain.cpp
// {A7B8C9D0-E1F2-4A3B-8C5D-6E7F8091A2B3}
static constexpr CLSID CLSID_Widget = {
    0xa7b8c9d0, 0xe1f2, 0x4a3b,
    {0x8c, 0x5d, 0x6e, 0x7f, 0x80, 0x91, 0xa2, 0xb3}
};

// {B8C9D0E1-F2A3-5B4C-9D6E-7F8091A2B3C4} — activation factory CLSID
static constexpr CLSID CLSID_WidgetFactory = {
    0xb8c9d0e1, 0xf2a3, 0x5b4c,
    {0x9d, 0x6e, 0x7f, 0x80, 0x91, 0xa2, 0xb3, 0xc4}
};

static const wchar_t* g_shutdownEventName = L"TestCppWinRT_ServerShutdown";

// Activation handler: routes RoGetActivationFactory to the OOP server's
// activation factory via CoCreateInstance(CLSID_WidgetFactory).
static int32_t __stdcall oop_activation_handler(
    void* classId, winrt::guid const& iid, void** factory) noexcept
{
    *factory = nullptr;
    return CoCreateInstance(CLSID_WidgetFactory, nullptr, CLSCTX_LOCAL_SERVER,
                            reinterpret_cast<const IID&>(iid), factory);
}

static int g_pass = 0, g_fail = 0;
static void Check(bool cond, const char* name)
{
    if (cond) { printf("  PASS: %s\n", name); g_pass++; }
    else      { printf("  FAIL: %s\n", name); g_fail++; }
}

int main()
{
    winrt::init_apartment();

    // Route WinRT activation to the OOP server
    winrt_activation_handler = oop_activation_handler;

    printf("=== C++/WinRT Out-of-Proc E2E Tests ===\n\n");

    // Create Widget via CoCreateInstance — goes through proxy/stub
    winrt::TestCppWinRT::Widget widget{ nullptr };
    {
        // Request the default interface IID (IWidget) from the projection
        auto iid = winrt::guid_of<winrt::TestCppWinRT::Widget>();
        void* ptr = nullptr;
        HRESULT hr = CoCreateInstance(CLSID_Widget, nullptr, CLSCTX_LOCAL_SERVER,
                                      reinterpret_cast<const IID&>(iid), &ptr);
        if (FAILED(hr)) {
            printf("FAIL: CoCreateInstance failed: 0x%08X\n", hr);
            // Signal server to shut down
            HANDLE hEvent = OpenEventW(EVENT_MODIFY_STATE, FALSE, g_shutdownEventName);
            if (hEvent) { SetEvent(hEvent); CloseHandle(hEvent); }
            return 1;
        }
        winrt::attach_abi(widget, ptr);
    }

    Check(widget != nullptr, "Widget created via CoCreateInstance");

    // Test 1: Name property round-trip
    try {
        widget.Name(L"OutOfProc");
        Check(widget.Name() == L"OutOfProc", "Name property round-trip");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "Name property round-trip");
    }

    // Test 2: Level enum property
    try {
        widget.Level(winrt::TestCppWinRT::Priority::High);
        Check(widget.Level() == winrt::TestCppWinRT::Priority::High, "Level enum property");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "Level enum property");
    }

    // Test 3: Default level
    try {
        // Create a second widget to test default
        void* ptr2 = nullptr;
        auto iid = winrt::guid_of<winrt::TestCppWinRT::Widget>();
        HRESULT hr = CoCreateInstance(CLSID_Widget, nullptr, CLSCTX_LOCAL_SERVER,
                                      reinterpret_cast<const IID&>(iid), &ptr2);
        if (SUCCEEDED(hr)) {
            winrt::TestCppWinRT::Widget w2{ nullptr };
            winrt::attach_abi(w2, ptr2);
            Check(w2.Level() == winrt::TestCppWinRT::Priority::Medium, "Default level is Medium");
        } else {
            Check(false, "Default level is Medium (CoCreateInstance failed)");
        }
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "Default level is Medium");
    }

    // Test 4: MoveTo / GetPosition struct round-trip
    try {
        widget.MoveTo({ 10.0f, 20.0f });
        auto pos = widget.GetPosition();
        Check(pos.X == 10.0f && pos.Y == 20.0f, "MoveTo/GetPosition struct round-trip");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "MoveTo/GetPosition struct round-trip");
    }

    // Test 5: GetColor struct return
    try {
        auto c = widget.GetColor();
        Check(c.R == 255 && c.G == 128 && c.B == 64 && c.A == 255, "GetColor struct return");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "GetColor struct return");
    }

    // Test 6: ComputeDistance (double return, multi-param)
    try {
        double d = widget.ComputeDistance({ 0.0f, 0.0f }, { 3.0f, 4.0f });
        Check(std::abs(d - 5.0) < 0.001, "ComputeDistance == 5.0");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "ComputeDistance == 5.0");
    }

    // Test 7: ComputeDistance zero
    try {
        double d = widget.ComputeDistance({ 1.0f, 1.0f }, { 1.0f, 1.0f });
        Check(d == 0.0, "ComputeDistance zero");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "ComputeDistance zero");
    }

    // Test 8: Name overwrite
    try {
        widget.Name(L"First");
        widget.Name(L"Second");
        Check(widget.Name() == L"Second", "Name overwrite");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "Name overwrite");
    }

    // Test 9: Empty name
    try {
        widget.Name(L"");
        Check(widget.Name().empty(), "Empty name");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "Empty name");
    }

    // Test 10: Level round-trip for each enum value
    try {
        widget.Level(winrt::TestCppWinRT::Priority::Low);
        Check(widget.Level() == winrt::TestCppWinRT::Priority::Low, "Level Low round-trip");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "Level Low round-trip");
    }

    // Test 11: Multiple position updates
    try {
        widget.MoveTo({ 100.0f, 200.0f });
        widget.MoveTo({ -5.0f, -10.0f });
        auto pos = widget.GetPosition();
        Check(pos.X == -5.0f && pos.Y == -10.0f, "Multiple MoveTo - last wins");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "Multiple MoveTo - last wins");
    }

    // Test 12: Long string
    try {
        winrt::hstring longName(L"This is a longer string for testing cross-process HSTRING marshaling");
        widget.Name(longName);
        Check(widget.Name() == longName, "Long string round-trip");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "Long string round-trip");
    }

    // Test 13: ComputeDistance diagonal
    try {
        double d = widget.ComputeDistance({ 1.0f, 1.0f }, { 4.0f, 5.0f });
        Check(std::abs(d - 5.0) < 0.001, "ComputeDistance diagonal == 5.0");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "ComputeDistance diagonal == 5.0");
    }

    // Test 14: Position initially zero
    try {
        void* ptr3 = nullptr;
        auto iid = winrt::guid_of<winrt::TestCppWinRT::Widget>();
        HRESULT hr = CoCreateInstance(CLSID_Widget, nullptr, CLSCTX_LOCAL_SERVER,
                                      reinterpret_cast<const IID&>(iid), &ptr3);
        if (SUCCEEDED(hr)) {
            winrt::TestCppWinRT::Widget w3{ nullptr };
            winrt::attach_abi(w3, ptr3);
            auto pos = w3.GetPosition();
            Check(pos.X == 0.0f && pos.Y == 0.0f, "Initial position is zero (new object)");
        } else {
            Check(false, "Initial position is zero (CoCreateInstance failed)");
        }
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "Initial position is zero");
    }

    // Test 15: Negative coordinates
    try {
        widget.MoveTo({ -100.5f, -200.5f });
        auto pos = widget.GetPosition();
        Check(pos.X == -100.5f && pos.Y == -200.5f, "Negative coordinates round-trip");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "Negative coordinates round-trip");
    }

    // NOTE: Event/delegate tests are skipped for out-of-proc — callback
    // interfaces require additional proxy registration which is complex.

    // Test: Static methods via WinRT activation (RoGetActivationFactory)
    try {
        auto ver = winrt::TestCppWinRT::Widget::GetVersion();
        Check(ver == L"1.0.0", "Static GetVersion (cross-process)");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "Static GetVersion (cross-process)");
    }

    try {
        auto def = winrt::TestCppWinRT::Widget::CreateDefault();
        Check(def.Name() == L"Default", "Static CreateDefault (cross-process)");
        Check(def.IsActive(), "Static CreateDefault: IsActive is true");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "Static CreateDefault (cross-process)");
    }

    // Test: Boolean property
    try {
        widget.IsActive(true);
        Check(widget.IsActive(), "IsActive set to true (cross-process)");
        widget.IsActive(false);
        Check(!widget.IsActive(), "IsActive set to false (cross-process)");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "IsActive boolean property");
    }

    // Test: Count integer property
    try {
        widget.Count(99);
        Check(widget.Count() == 99, "Count set/get (cross-process)");
        widget.Count(-7);
        Check(widget.Count() == -7, "Count negative value (cross-process)");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "Count integer property");
    }

    // Test: TryParse with out parameter
    try {
        int32_t value = -1;
        bool ok = widget.TryParse(L"123", value);
        Check(ok && value == 123, "TryParse valid input (cross-process)");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "TryParse (cross-process)");
    }

    // Test: TryParse with invalid input
    try {
        int32_t value = -1;
        bool ok = widget.TryParse(L"abc", value);
        Check(!ok, "TryParse invalid input returns false (cross-process)");
    } catch (winrt::hresult_error const& e) {
        printf("    Exception: 0x%08X\n", (int)e.code());
        Check(false, "TryParse invalid (cross-process)");
    }

    // Signal server to shut down
    HANDLE hEvent = OpenEventW(EVENT_MODIFY_STATE, FALSE, g_shutdownEventName);
    if (hEvent) {
        SetEvent(hEvent);
        CloseHandle(hEvent);
    }

    printf("\nResults: %d passed, %d failed\n", g_pass, g_fail);
    return g_fail;
}
