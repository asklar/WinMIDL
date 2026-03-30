// TestInProc.cpp — In-proc C++/WinRT E2E test
// Creates Widget objects directly via winrt::make (no COM registration needed).

#include "winrt/Windows.Foundation.h"
#include "Widget.h"
#include <stdio.h>
#include <cmath>

// Forward-declare the factory function from module.g.cpp
extern void* __stdcall winrt_get_activation_factory(std::wstring_view const& name);

// Adapter to wire up in-proc activation (so projected constructors & statics work)
static int32_t __stdcall local_activation_handler(
    void* classId, winrt::guid const& iid, void** factory) noexcept
{
    *factory = nullptr;
    std::wstring_view name{ *reinterpret_cast<winrt::hstring*>(&classId) };
    void* raw = winrt_get_activation_factory(name);
    if (!raw) return static_cast<int32_t>(0x80040154); // REGDB_E_CLASSNOTREG
    auto unk = static_cast<winrt::impl::unknown_abi*>(raw);
    auto hr = unk->QueryInterface(iid, factory);
    unk->Release();
    return hr;
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

    // Wire up in-proc activation so static methods and projected constructors work
    winrt_activation_handler = local_activation_handler;

    printf("=== C++/WinRT In-Proc E2E Tests ===\n\n");

    // Test 1: Default constructor
    {
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
        Check(w.Name().empty(), "Default constructor - empty name");
    }

    // Test 2: Parameterized constructor
    {
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>(L"TestWidget");
        auto w2 = winrt::TestCppWinRT::Widget(L"TestWidget");
        Check(w.Name() == L"TestWidget", "Parameterized constructor");
    }

    // Test 3: Name get/set
    {
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
        w.Name(L"Hello");
        Check(w.Name() == L"Hello", "Name get/set");
    }

    // Test 4: Level default is Medium
    {
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
        Check(w.Level() == winrt::TestCppWinRT::Priority::Medium, "Level default is Medium");
    }

    // Test 5: Level set to High
    {
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
        w.Level(winrt::TestCppWinRT::Priority::High);
        Check(w.Level() == winrt::TestCppWinRT::Priority::High, "Level set to High");
    }

    // Test 6: MoveTo / GetPosition struct round-trip
    {
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
        w.MoveTo({ 3.0f, 4.0f });
        auto pos = w.GetPosition();
        Check(pos.X == 3.0f && pos.Y == 4.0f, "MoveTo/GetPosition struct round-trip");
    }

    // Test 7: GetColor struct return
    {
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
        auto c = w.GetColor();
        Check(c.R == 255 && c.G == 128 && c.B == 64 && c.A == 255, "GetColor struct return");
    }

    // Test 8: ComputeDistance
    {
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
        double d = w.ComputeDistance({ 0.0f, 0.0f }, { 3.0f, 4.0f });
        Check(std::abs(d - 5.0) < 0.001, "ComputeDistance(0,0 -> 3,4) == 5.0");
    }

    // Test 9: Static CreateDefault
    {
        auto w = winrt::TestCppWinRT::Widget::CreateDefault();
        Check(w.Name() == L"Default", "Static CreateDefault");
    }

    // Test 10: Static GetVersion
    {
        auto ver = winrt::TestCppWinRT::Widget::GetVersion();
        Check(ver == L"1.0.0", "Static GetVersion");
    }

    // Test 11: StatusHandler delegate invocation
    {
        bool called = false;
        winrt::hstring received;
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
        w.Subscribe([&](winrt::hstring const& msg) {
            called = true;
            received = msg;
        });
        Check(called && received == L"subscribed", "StatusHandler delegate invocation");
    }

    // Test 12: NameChanged event subscribe and fire
    {
        bool fired = false;
        winrt::hstring eventArg;
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
        auto token = w.NameChanged([&](auto const&, winrt::hstring const& name) {
            fired = true;
            eventArg = name;
        });
        w.Name(L"NewName");
        Check(fired, "NameChanged event fired");
        Check(eventArg == L"NewName", "NameChanged event arg correct");
    }

    // Test 13: NameChanged event unsubscribe
    {
        int fireCount = 0;
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
        auto token = w.NameChanged([&](auto const&, winrt::hstring const&) {
            fireCount++;
        });
        w.Name(L"First");
        w.NameChanged(token);
        w.Name(L"Second");
        Check(fireCount == 1, "NameChanged unsubscribe prevents further events");
    }

    // Test 14: Flags enum bitwise operations
    {
        using P = winrt::TestCppWinRT::Permissions;
        auto rw = P::Read | P::Write;
        Check((rw & P::Read) == P::Read, "Flags enum: Read bit set");
        Check((rw & P::Write) == P::Write, "Flags enum: Write bit set");
        Check((rw & P::Execute) != P::Execute, "Flags enum: Execute bit not set");
    }

    // Test 15: Point struct field access
    {
        winrt::TestCppWinRT::Point pt{ 1.5f, 2.5f };
        Check(pt.X == 1.5f && pt.Y == 2.5f, "Point struct field access");
    }

    // Test 16: Color struct field access
    {
        winrt::TestCppWinRT::Color c{ 10, 20, 30, 40 };
        Check(c.R == 10 && c.G == 20 && c.B == 30 && c.A == 40, "Color struct field access");
    }

    // Test 17: ComputeDistance zero distance
    {
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
        double d = w.ComputeDistance({ 5.0f, 5.0f }, { 5.0f, 5.0f });
        Check(d == 0.0, "ComputeDistance zero distance");
    }

    // Test 18: Multiple Name sets
    {
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
        w.Name(L"First");
        w.Name(L"Second");
        w.Name(L"Third");
        Check(w.Name() == L"Third", "Multiple Name sets - last wins");
    }

    // Test 19: Enum values
    {
        Check(static_cast<int>(winrt::TestCppWinRT::Priority::Low) == 0, "Priority::Low == 0");
        Check(static_cast<int>(winrt::TestCppWinRT::Priority::Medium) == 1, "Priority::Medium == 1");
        Check(static_cast<int>(winrt::TestCppWinRT::Priority::High) == 2, "Priority::High == 2");
    }

    // Test 20: Initial position is zero
    {
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
        auto pos = w.GetPosition();
        Check(pos.X == 0.0f && pos.Y == 0.0f, "Initial position is zero");
    }

    // Test 21: Multiple MoveTo calls
    {
        auto w = winrt::make<winrt::TestCppWinRT::implementation::Widget>();
        w.MoveTo({ 1.0f, 1.0f });
        w.MoveTo({ 2.0f, 2.0f });
        auto pos = w.GetPosition();
        Check(pos.X == 2.0f && pos.Y == 2.0f, "Multiple MoveTo - last position wins");
    }

    // Test 22: Permissions flags enum values
    {
        using P = winrt::TestCppWinRT::Permissions;
        Check(static_cast<uint32_t>(P::None) == 0, "Permissions::None == 0");
        Check(static_cast<uint32_t>(P::Read) == 1, "Permissions::Read == 1");
        Check(static_cast<uint32_t>(P::Write) == 2, "Permissions::Write == 2");
        Check(static_cast<uint32_t>(P::Execute) == 4, "Permissions::Execute == 4");
    }

    printf("\nResults: %d passed, %d failed\n", g_pass, g_fail);
    return g_fail;
}
