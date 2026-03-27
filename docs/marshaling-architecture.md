# Marshaling Architecture

WinMIDL generates direct C++ proxy/stub code for WinRT COM interface marshaling. Instead of NDR format strings (interpreted at runtime), all serialization is compiled C++ that plugs into COM's standard `IRpcProxyBuffer` / `IRpcStubBuffer` / `IPSFactoryBuffer` infrastructure.

## Overview

```
IDL source
  → WinMIDL /cppproxy foo_proxy.cpp
    → foo_proxy.cpp   (proxy classes + stub classes + factory + DLL plumbing)
    → foo_proxy.h     (internal declarations)
    → foo_i.c         (IID constants, constexpr inline)
  → cl.exe
    → fooproxy.dll    (registers via DllRegisterServer)
  → COM runtime loads fooproxy.dll for cross-apartment/cross-process calls
```

## Wire Format

Simple packed binary — no NDR, no alignment padding, no type tags:

| WinRT Type | Wire Encoding |
|------------|--------------|
| Int32/UInt32 | 4 bytes raw (`memcpy`) |
| Int64/UInt64 | 8 bytes raw |
| Single/Double | 4/8 bytes raw |
| Boolean | 4 bytes (BOOL) |
| HSTRING | `[uint32_t charCount][wchar_t × charCount]` |
| GUID | 16 bytes raw |
| Enum | 4 bytes (Int32) |
| Struct | Fields packed sequentially (validated via `static_assert` on layout) |
| EventRegistrationToken | 8 bytes raw |
| Interface* | `CoMarshalInterface` to nested IStream |
| out params | In reply buffer |
| HRESULT return | Last 4 bytes of reply |

### Zero-copy HSTRING on stub side

Stub-side HSTRING deserialization uses `WindowsCreateStringReference` (zero-copy, no allocation) when the string lifetime is bounded by the `Invoke` call. Proxy-side uses `WindowsCreateString` (allocating) since the caller owns the returned HSTRING.

## Generated Code Structure

### Per interface: proxy class

```cpp
class CIWidgetProxy : public IRpcProxyBuffer {
    IUnknown* m_pUnkOuter;      // aggregation outer
    IRpcChannelBuffer* m_pChannel;
    LONG m_cRef;

    // Nested Face class for COM aggregation
    struct Face : public IWidget {
        CIWidgetProxy* m_owner;

        // Delegating IUnknown — forwards to outer
        HRESULT QueryInterface(REFIID riid, void** ppv) override {
            return m_owner->m_pUnkOuter->QueryInterface(riid, ppv);
        }
        ULONG AddRef() override { return m_owner->m_pUnkOuter->AddRef(); }
        ULONG Release() override { return m_owner->m_pUnkOuter->Release(); }

        // IInspectable via shared helpers + static constexpr data
        HRESULT GetIids(ULONG* count, IID** iids) override {
            return winmidl::get_iids(kTypeInfo, count, iids);
        }
        HRESULT GetRuntimeClassName(HSTRING* name) override {
            return winmidl::get_runtime_class_name(kTypeInfo, name);
        }

        // Methods delegate to shared signature-shape functions
        HRESULT GetName(HSTRING* retval) override {
            return proxy_shape_void_to_hstring(m_owner->m_pChannel, IID_IWidget, 6, retval);
        }
        HRESULT SetName(HSTRING value) override {
            return proxy_shape_hstring_to_void(m_owner->m_pChannel, IID_IWidget, 7, value);
        }
    };

    // Non-delegating IUnknown + IRpcProxyBuffer
    static constexpr winmidl::qi_entry_t kQIEntries[] = { ... };
    static constexpr winmidl::type_info_t kTypeInfo = { kIIDs, L"Widget", kQIEntries };

    HRESULT QueryInterface(REFIID riid, void** ppv) override {
        return winmidl::qi_check(kTypeInfo, this, m_pUnkOuter, riid, ppv);
    }
};
```

### Per interface: stub class

```cpp
class CStub_IWidget : public IRpcStubBuffer {
    IWidget* m_pServer;
    LONG m_cRef;

    HRESULT Invoke(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel) override {
        switch (pMsg->iMethod) {
            case 6: return Invoke_GetName(pMsg, pChannel);
            case 7: return Invoke_SetName(pMsg, pChannel);
            // ...
        }
    }

    HRESULT Invoke_GetName(RPCOLEMESSAGE* pMsg, IRpcChannelBuffer* pChannel) {
        HSTRING returnValue;
        HRESULT hr = m_pServer->GetName(&returnValue);
        // Serialize reply using shared helpers
        winmidl::marshal_hstring(pOut, returnValue);
        memcpy(pOut, &hr, sizeof(HRESULT));
        // ...
    }
};
```

### Factory + DLL plumbing

```cpp
class CPSFactory : public IPSFactoryBuffer {
    HRESULT CreateProxy(IUnknown* pOuter, REFIID riid, IRpcProxyBuffer** ppProxy, void** ppv) {
        if (riid == IID_IWidget) { /* new CIWidgetProxy(pOuter) */ }
    }
    HRESULT CreateStub(REFIID riid, IUnknown* pServer, IRpcStubBuffer** ppStub) {
        if (riid == IID_IWidget) { /* new CStub_IWidget() */ }
    }
};

// DLL exports
HRESULT DllGetClassObject(REFCLSID, REFIID, void**);
HRESULT DllCanUnloadNow();
HRESULT DllRegisterServer();    // registers Interface\{IID}\ProxyStubClsid32
HRESULT DllUnregisterServer();
```

CLSID is deterministic (UUID v5 derived from the set of interface IIDs).

## Shared Runtime: `proxy_helpers.h`

### Data structures

```cpp
namespace winmidl {
    struct qi_entry_t {
        IID      iid;
        uint32_t offset;     // byte offset to interface vtable ptr
        bool     delegating; // true = AddRef outer, false = AddRef inner
    };

    struct type_info_t {
        std::span<const IID>         iids;      // interfaces for GetIids
        std::wstring_view            className; // for GetRuntimeClassName
        std::span<const qi_entry_t>  qiEntries; // for QueryInterface
    };

    // Method descriptor types (for data tables)
    enum class param_type { t_bool, t_int16, t_int32, t_int64, t_uint8, t_uint16,
                            t_uint32, t_uint64, t_float, t_double, t_hstring,
                            t_guid, t_enum, t_struct, t_interface, t_event_token };
    enum class param_dir { dir_in, dir_out, dir_retval };

    struct param_descriptor { param_type type; param_dir dir; };
    struct method_descriptor { uint32_t slot; const char* name;
                               const param_descriptor* params; uint32_t paramCount; };
    struct interface_descriptor { const IID* iid;
                                  const method_descriptor* methods; uint32_t methodCount; };
}
```

### IInspectable helpers

```cpp
HRESULT qi_check(type_info_t const& info, void* pThis, IUnknown* pOuter, REFIID riid, void** ppv);
HRESULT get_iids(type_info_t const& info, ULONG* count, IID** iids);
HRESULT get_runtime_class_name(type_info_t const& info, HSTRING* name);
HRESULT get_trust_level(TrustLevel* level);  // always returns BaseTrust
```

### Marshal/unmarshal helpers

Inline functions for all WinRT primitive types:

```cpp
void marshal_int32(BYTE*& buf, INT32 val);
void marshal_hstring(BYTE*& buf, HSTRING val);
void marshal_guid(BYTE*& buf, const GUID& val);
void marshal_event_token(BYTE*& buf, EventRegistrationToken val);
// ... all WinRT types

HRESULT unmarshal_hstring(BYTE*& buf, HSTRING* val);      // allocating (proxy side)
HRESULT unmarshal_hstring_ref(BYTE*& buf, HSTRING* val);   // zero-copy (stub side)
void unmarshal_int32(BYTE*& buf, INT32* val);
// ... all WinRT types
```

## Optimization: Method Descriptor Tables

Each interface generates `static constexpr` descriptor tables:

```cpp
static constexpr winmidl::param_descriptor IWidget_GetName_params[] = {
    { winmidl::param_type::t_hstring, winmidl::param_dir::dir_retval },
};
static constexpr winmidl::method_descriptor IWidget_methods[] = {
    { 6, "GetName",  IWidget_GetName_params, 1 },
    { 7, "SetName",  IWidget_SetName_params, 1 },
    { 8, "get_Count", IWidget_get_Count_params, 1 },
};
```

## Optimization: Signature Folding

Methods with identical parameter/return shapes share ONE proxy and stub implementation:

```cpp
// Shared by ALL void→HSTRING methods (GetName, get_Title, get_Label, ...)
static HRESULT proxy_shape_void_to_hstring(
    IRpcChannelBuffer* channel, const GUID& iid, UINT32 slot, HSTRING* retval)
{
    RPCOLEMESSAGE msg = {};
    msg.iMethod = slot;
    msg.cbBuffer = 0;
    HRESULT hr = channel->GetBuffer(&msg, iid);
    RETURN_IF_FAILED(hr);
    ULONG status;
    hr = channel->SendReceive(&msg, &status);
    RETURN_IF_FAILED(hr);
    BYTE* pReply = (BYTE*)msg.Buffer;
    hr = winmidl::unmarshal_hstring(pReply, retval); RETURN_IF_FAILED(hr);
    HRESULT retHr; memcpy(&retHr, pReply, sizeof(HRESULT));
    hr = channel->FreeBuffer(&msg);
    RETURN_IF_FAILED(hr);
    return retHr;
}
```

Common WinRT shapes (most APIs use a small set):

| Shape | Example | Frequency |
|-------|---------|-----------|
| `() → HSTRING` | Property getters returning String | Very high |
| `(HSTRING) → void` | Property setters taking String | High |
| `() → Int32` | Property getters returning Int32 | High |
| `() → Boolean` | Property getters returning Boolean | High |
| `(EventHandler) → EventRegistrationToken` | Event subscription | High |
| `(EventRegistrationToken) → void` | Event unsubscription | High |

## Codegen Implementation

| File | Responsibility |
|------|---------------|
| `ProxyGenerator.cs` | C++ proxy class per interface (Face pattern, IUnknown, IRpcProxyBuffer) |
| `StubGenerator.cs` | C++ stub class per interface (IRpcStubBuffer, Invoke dispatch switch) |
| `FactoryGenerator.cs` | IPSFactoryBuffer, DLL exports, registry plumbing, UUID v5 CLSID |
| `WireFormat.cs` | Shared serialization logic: buffer sizing, marshal/unmarshal emit, struct layout |

## E2E Tests

| Test Suite | Description | Status |
|-----------|-------------|--------|
| `tests/e2e-custom-marshal/inproc/` | 18 test cases, same-process proxy/stub round-trip | ✅ |
| `tests/e2e-custom-marshal/comprehensive/` | 31 test cases, all WinRT type patterns | ✅ |
| `tests/e2e-custom-marshal/outofproc/` | Cross-process marshaling | ✅ |
| `tests/Midl.CodeGen.Tests/` | 540 codegen unit tests asserting on generated C++ | ✅ |

## Future Improvements

### Single generic vtable (`/Os` flag)
One vtable for ALL IInspectable proxies — every slot calls `generic_dispatch(this, N)` which does a runtime lookup in the method descriptor table. Trades per-call speed for smaller binary size. Useful for IoT/size-constrained scenarios.

### Interface pointer marshaling
`CoMarshalInterface` / `CoUnmarshalInterface` for interface-typed parameters. Currently stubs are generated but the marshal stream handling needs completion.

### Async method support
Methods returning `IAsyncOperation<T>` / `IAsyncAction` may need special handling for marshaling the async completion across apartments.

### Array parameter support
`[in] UINT32 count, [in, size_is(count)] T* items` pattern for passing arrays. Requires length-prefixed wire encoding.

### Generic interface marshaling
Parameterized interfaces like `IVector<T>`, `IMap<K,V>` need template-based or type-erased proxy/stub generation.

### Binary size benchmarking
Compare generated DLL sizes before and after signature folding to quantify the optimization impact.
