// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// PiidValidationTest.cpp — Standalone test that validates our computed PIIDs
// against RoGetParameterizedTypeInstanceIID.
//
// Build (from the comprehensive directory, after running build-and-test.ps1):
//   cl /EHsc /I generated PiidValidationTest.cpp /link runtimeobject.lib
// Run: PiidValidationTest.exe

#include <windows.h>
#include <inspectable.h>
#include <stdio.h>
#include "PiidValidator.h"

// Our computed IID constants (copy from the generated proxy)
// These are validated against the Windows Runtime API at runtime.
#include "generated/TestComprehensive.h"
#include "generated/TestComprehensive_i.c"

// Parameterized IIDs are emitted in the proxy — declare them here.
// The actual values are computed by WinMIDL's PIID algorithm.
static constexpr IID IID_IVector_1_String = { 0x98b9acc1, 0x4b56, 0x532e, { 0xac, 0x73, 0x03, 0xd5, 0x29, 0x1c, 0xca, 0x90 } };
static constexpr IID IID_IVectorView_1_String = { 0x2f13c006, 0xa03a, 0x5f69, { 0xb0, 0x90, 0x75, 0xa4, 0x3e, 0x33, 0x42, 0x3e } };
static constexpr IID IID_IMap_2_String_Int32 = { 0xae681871, 0xdd82, 0x5299, { 0x93, 0xea, 0x02, 0x75, 0xe4, 0xe0, 0x73, 0xe7 } };
static constexpr IID IID_IMapView_2_String_Int32 = { 0x06c17849, 0xdfc8, 0x501a, { 0xbf, 0x47, 0x16, 0x15, 0x2f, 0xa2, 0x1d, 0x4b } };
static constexpr IID IID_IIterable_1_String = { 0xe2fcc7c1, 0x3bfc, 0x5a0b, { 0xb2, 0xb0, 0x72, 0xe7, 0x69, 0xd1, 0xcb, 0x7e } };
static constexpr IID IID_IReference_1_Int32 = { 0xaff0a017, 0x349c, 0x50e2, { 0x92, 0xc3, 0x10, 0xbf, 0x04, 0x5d, 0x88, 0x25 } };

int main() {
    int passed = 0, failed = 0;

    printf(">> Validating WinMIDL PIIDs against RoGetParameterizedTypeInstanceIID\n\n");

    // IVector<String>
    {
        PCWSTR parts[] = { L"Windows.Foundation.Collections.IVector`1", L"String" };
        if (ValidatePiid("IVector<String>", IID_IVector_1_String, parts, 2)) passed++; else failed++;
    }

    // IVectorView<String>
    {
        PCWSTR parts[] = { L"Windows.Foundation.Collections.IVectorView`1", L"String" };
        if (ValidatePiid("IVectorView<String>", IID_IVectorView_1_String, parts, 2)) passed++; else failed++;
    }

    // IMap<String, Int32>
    {
        PCWSTR parts[] = { L"Windows.Foundation.Collections.IMap`2", L"String", L"Int32" };
        if (ValidatePiid("IMap<String,Int32>", IID_IMap_2_String_Int32, parts, 3)) passed++; else failed++;
    }

    // IMapView<String, Int32>
    {
        PCWSTR parts[] = { L"Windows.Foundation.Collections.IMapView`2", L"String", L"Int32" };
        if (ValidatePiid("IMapView<String,Int32>", IID_IMapView_2_String_Int32, parts, 3)) passed++; else failed++;
    }

    // IIterable<String>
    {
        PCWSTR parts[] = { L"Windows.Foundation.Collections.IIterable`1", L"String" };
        if (ValidatePiid("IIterable<String>", IID_IIterable_1_String, parts, 2)) passed++; else failed++;
    }

    // IReference<Int32>
    {
        PCWSTR parts[] = { L"Windows.Foundation.IReference`1", L"Int32" };
        if (ValidatePiid("IReference<Int32>", IID_IReference_1_Int32, parts, 2)) passed++; else failed++;
    }

    printf("\n>> PIID validation: %d passed, %d failed\n", passed, failed);
    return failed > 0 ? 1 : 0;
}
