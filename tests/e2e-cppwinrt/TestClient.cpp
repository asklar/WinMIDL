// TestClient.cpp — Validates that cppwinrt-generated projection headers
// from our WinMIDL-emitted .winmd compile correctly.
// This is a compilation test, not a runtime test (no server implementation).

#pragma once

// Suppress coroutine deprecation warnings
#define _SILENCE_CLANG_COROUTINE_MESSAGE

#include "winrt/TestCppWinRT.h"
#include <stdio.h>

int main()
{
    // Verify enum type compiles and has correct values
    winrt::TestCppWinRT::Priority p = winrt::TestCppWinRT::Priority::High;
    (void)p;

    // Verify struct type compiles
    winrt::TestCppWinRT::Point pt{ 1.0f, 2.0f };
    (void)pt;

    // Verify class type is declared (can't instantiate without a server)
    using WidgetType = winrt::TestCppWinRT::Widget;
    (void)sizeof(WidgetType);

    // Verify interface types compile
    using IWidgetType = winrt::TestCppWinRT::IWidget;
    (void)sizeof(IWidgetType);

    printf("PASS: CppWinRT projection compiles OK\n");
    return 0;
}
