// PiidValidator.h — Validates computed parameterized IIDs against the Windows Runtime
// Uses RoGetParameterizedTypeInstanceIID with a minimal IRoMetaDataLocator implementation.

#pragma once
#include <windows.h>
#include <roparameterizediid.h>
#include <stdio.h>
#pragma comment(lib, "runtimeobject.lib")

// Well-known base PIIDs for WinRT parameterized interfaces
static const GUID PIID_IVector = { 0x913337e9, 0x11a1, 0x4345, {0xa3, 0xa2, 0x4e, 0x7f, 0x95, 0x6e, 0x22, 0x2d} };
static const GUID PIID_IVectorView = { 0xbbe1fa4c, 0xb0e3, 0x4583, {0xba, 0xef, 0x1f, 0x1b, 0x2e, 0x48, 0x3e, 0x56} };
static const GUID PIID_IMap = { 0x3c2925fe, 0x8519, 0x45c1, {0xaa, 0x79, 0x19, 0x7b, 0x67, 0x18, 0xc1, 0xc1} };
static const GUID PIID_IMapView = { 0xe480ce40, 0xa338, 0x4ada, {0xad, 0xcf, 0x27, 0x22, 0x72, 0xe4, 0x8c, 0xb9} };
static const GUID PIID_IIterable = { 0xfaa585ea, 0x6214, 0x4217, {0xaf, 0xda, 0x7f, 0x46, 0xde, 0x58, 0x69, 0xb3} };
static const GUID PIID_IReference = { 0x61c17706, 0x2d65, 0x11e0, {0x9a, 0xe8, 0xd4, 0x85, 0x64, 0x01, 0x52, 0x06} };
static const GUID PIID_IKeyValuePair = { 0x02b51929, 0xc1c4, 0x4a7e, {0x89, 0x40, 0x03, 0x12, 0xb5, 0xc1, 0x85, 0x00} };
static const GUID PIID_IIterator = { 0x6a79e863, 0x4300, 0x459a, {0x99, 0x66, 0xcb, 0xb6, 0x60, 0x96, 0x3e, 0xe1} };
static const GUID PIID_EventHandler = { 0x9de1c534, 0x6ae1, 0x11e0, {0x84, 0xe1, 0x18, 0xa9, 0x05, 0xbc, 0xc5, 0x3f} };
static const GUID PIID_TypedEventHandler = { 0x9de1c535, 0x6ae1, 0x11e0, {0x84, 0xe1, 0x18, 0xa9, 0x05, 0xbc, 0xc5, 0x3f} };

// Minimal IRoMetaDataLocator that knows about built-in WinRT parameterized types
struct BuiltinMetaDataLocator : IRoMetaDataLocator {
    HRESULT STDMETHODCALLTYPE Locate(PCWSTR nameElement, IRoSimpleMetaDataBuilder& builder) const override {
        // Primitive types
        if (wcscmp(nameElement, L"String") == 0 || wcscmp(nameElement, L"Int32") == 0 ||
            wcscmp(nameElement, L"UInt32") == 0 || wcscmp(nameElement, L"Int64") == 0 ||
            wcscmp(nameElement, L"Boolean") == 0 || wcscmp(nameElement, L"Double") == 0 ||
            wcscmp(nameElement, L"Single") == 0 || wcscmp(nameElement, L"UInt8") == 0 ||
            wcscmp(nameElement, L"Char16") == 0 || wcscmp(nameElement, L"Guid") == 0 ||
            wcscmp(nameElement, L"UInt64") == 0 || wcscmp(nameElement, L"Int16") == 0 ||
            wcscmp(nameElement, L"UInt16") == 0)
            return S_OK; // Primitives are recognized automatically

        // Parameterized interfaces
        if (wcscmp(nameElement, L"Windows.Foundation.Collections.IVector`1") == 0)
            return builder.SetParameterizedInterface(PIID_IVector, 1);
        if (wcscmp(nameElement, L"Windows.Foundation.Collections.IVectorView`1") == 0)
            return builder.SetParameterizedInterface(PIID_IVectorView, 1);
        if (wcscmp(nameElement, L"Windows.Foundation.Collections.IMap`2") == 0)
            return builder.SetParameterizedInterface(PIID_IMap, 2);
        if (wcscmp(nameElement, L"Windows.Foundation.Collections.IMapView`2") == 0)
            return builder.SetParameterizedInterface(PIID_IMapView, 2);
        if (wcscmp(nameElement, L"Windows.Foundation.Collections.IIterable`1") == 0)
            return builder.SetParameterizedInterface(PIID_IIterable, 1);
        if (wcscmp(nameElement, L"Windows.Foundation.Collections.IKeyValuePair`2") == 0)
            return builder.SetParameterizedInterface(PIID_IKeyValuePair, 2);
        if (wcscmp(nameElement, L"Windows.Foundation.Collections.IIterator`1") == 0)
            return builder.SetParameterizedInterface(PIID_IIterator, 1);
        if (wcscmp(nameElement, L"Windows.Foundation.IReference`1") == 0)
            return builder.SetParameterizedInterface(PIID_IReference, 1);
        if (wcscmp(nameElement, L"Windows.Foundation.EventHandler`1") == 0)
            return builder.SetParameterizedDelegate(PIID_EventHandler, 1);
        if (wcscmp(nameElement, L"Windows.Foundation.TypedEventHandler`2") == 0)
            return builder.SetParameterizedDelegate(PIID_TypedEventHandler, 2);

        printf("  WARNING: Unknown type in PIID resolution: %ls\n", nameElement);
        return E_NOTIMPL;
    }
};

// Compute a parameterized IID using the Windows Runtime API
// parts: e.g. {"Windows.Foundation.Collections.IVector`1", "String"}
static HRESULT ComputePiid(PCWSTR* parts, UINT32 count, GUID* outIid) {
    BuiltinMetaDataLocator locator;
    return RoGetParameterizedTypeInstanceIID(count, parts, locator, outIid, nullptr);
}

// Validate that a computed IID matches the Windows Runtime's computation
static bool ValidatePiid(const char* testName, const GUID& computed, PCWSTR* parts, UINT32 count) {
    GUID expected;
    HRESULT hr = ComputePiid(parts, count, &expected);
    if (FAILED(hr)) {
        printf("FAIL: %s — RoGetParameterizedTypeInstanceIID failed: 0x%08lx\n", testName, hr);
        return false;
    }
    if (IsEqualGUID(computed, expected)) {
        printf("  OK: %s\n", testName);
        return true;
    } else {
        printf("FAIL: %s\n", testName);
        printf("  Computed: {%08lx-%04x-%04x-%02x%02x-%02x%02x%02x%02x%02x%02x}\n",
            computed.Data1, computed.Data2, computed.Data3,
            computed.Data4[0], computed.Data4[1], computed.Data4[2], computed.Data4[3],
            computed.Data4[4], computed.Data4[5], computed.Data4[6], computed.Data4[7]);
        printf("  Expected: {%08lx-%04x-%04x-%02x%02x-%02x%02x%02x%02x%02x%02x}\n",
            expected.Data1, expected.Data2, expected.Data3,
            expected.Data4[0], expected.Data4[1], expected.Data4[2], expected.Data4[3],
            expected.Data4[4], expected.Data4[5], expected.Data4[6], expected.Data4[7]);
        return false;
    }
}
