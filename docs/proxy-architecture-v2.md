# Proxy/Stub Architecture: Data-Driven Refactor Plan

## Goal

Refactor the generated proxy/stub C++ code from inline-everything to a **data-driven** architecture where:
- Per-interface generated code is minimal (constexpr data tables + thin trampolines)
- All logic lives in shared runtime helpers (one implementation, reused by every interface)
- Methods with identical signature shapes are folded into a single function
- The compiler reduces vtable entries to `lea + jmp` (load data pointer, tail call helper)

## Current Architecture (v1)

Each interface generates ~200 lines of inline C++:
```
Per interface:
  - Full QI implementation (if/else chain)
  - Full GetIids (CoTaskMemAlloc + memcpy inline)
  - Full GetRuntimeClassName (WindowsCreateStringReference inline)
  - Per-method proxy body (serialize + GetBuffer + SendReceive + deserialize)
  - Per-method stub body (deserialize + call server + serialize reply)
```

Problem: 50 interfaces × 200 lines = 10,000 lines of largely identical code.

## Target Architecture (v2)

```
Shared runtime header (~300 lines, one copy):
  proxy_helpers.h
    - type_info_t struct definition
    - qi_entry_t struct definition
    - qi_check()
    - get_iids()
    - get_runtime_class_name()
    - get_trust_level()
    - Per-signature-shape proxy/stub functions

Per interface (constexpr data + thin trampolines):
  - constexpr IID kIIDs[] = { ... };
  - constexpr qi_entry_t kQIEntries[] = { ... };
  - constexpr type_info_t kTypeInfo = { kIIDs, L"ClassName", kQIEntries };
  - QI/GetIids/GetRTCN overrides: one-liner forwards to helpers
  - Per-method vtable entries: thin trampolines to signature-shaped helpers
```

## Key Data Structures

### `type_info_t`
```cpp
struct type_info_t {
    std::span<const IID>       iids;         // interfaces this object implements
    std::wstring_view          className;    // runtime class name
    std::span<const qi_entry_t> qiEntries;   // QI dispatch table
};
```

### `qi_entry_t`
```cpp
struct qi_entry_t {
    IID      iid;
    uint32_t offset;     // byte offset from proxy base to interface vtable ptr
    bool     delegating; // true = AddRef outer, false = AddRef inner
};
```

## Shared Helpers

### `qi_check`
```cpp
HRESULT qi_check(type_info_t const& info, void* pThis, REFIID riid, void** ppv) {
    for (auto& e : info.qiEntries) {
        if (e.iid == riid) {
            *ppv = reinterpret_cast<std::byte*>(pThis) + e.offset;
            // AddRef logic based on e.delegating
            return S_OK;
        }
    }
    *ppv = nullptr;
    return E_NOINTERFACE;
}
```

### `get_iids`
```cpp
HRESULT get_iids(type_info_t const& info, ULONG* count, IID** iids) {
    *count = static_cast<ULONG>(info.iids.size());
    *iids = static_cast<IID*>(CoTaskMemAlloc(info.iids.size_bytes()));
    if (!*iids) return E_OUTOFMEMORY;
    memcpy(*iids, info.iids.data(), info.iids.size_bytes());
    return S_OK;
}
```

### `get_runtime_class_name`
```cpp
HRESULT get_runtime_class_name(type_info_t const& info, HSTRING* name) {
    HSTRING_HEADER hdr;
    return WindowsCreateStringReference(
        info.className.data(), 
        static_cast<UINT32>(info.className.size()), 
        &hdr, name);
}
```

## Signature Folding

Methods with identical parameter/return type shapes share ONE implementation.

### Example: all `() → HSTRING` getters share one proxy function
```cpp
// Shared: ONE function for all void→HSTRING proxy calls
HRESULT proxy_call_void_to_hstring(
    IRpcChannelBuffer* ch, REFIID iid, uint32_t iMethod, HSTRING* result);

// Generated vtable entries — compiler emits: mov r8d, N; jmp proxy_call_void_to_hstring
HRESULT get_Name(HSTRING* r)  override { return proxy_call_void_to_hstring(m_ch, kIID, 6, r); }
HRESULT get_Title(HSTRING* r) override { return proxy_call_void_to_hstring(m_ch, kIID, 7, r); }
HRESULT get_Desc(HSTRING* r)  override { return proxy_call_void_to_hstring(m_ch, kIID, 8, r); }
```

### Signature shape catalog
At codegen time, collect all unique (param_types, return_type) tuples across all interfaces.
For each unique shape, emit ONE proxy function and ONE stub function.
Wire each vtable slot to the appropriate shape function with the method index.

### Common WinRT shapes (most APIs use a small set):
| Shape | Example | Frequency |
|-------|---------|-----------|
| `() → HSTRING` | Property getters returning String | Very high |
| `(HSTRING) → void` | Property setters taking String | High |
| `() → int32_t` | Property getters returning Int32 | High |
| `(int32_t) → void` | Property setters taking Int32 | High |
| `() → bool` | Property getters returning Boolean | High |
| `() → void` | No-param void methods | Medium |
| `(HSTRING, int32_t) → bool` | Search/filter methods | Medium |

## Future Optimization: Single Vtable (`/Os` flag)

NDR's ultimate trick: ONE vtable for ALL IInspectable proxies. Every slot calls
`generic_dispatch(this, N)` which looks up the method descriptor from data tables.

Trade-off: smaller binary (one vtable instead of N), but slower per-call (runtime lookup).

Not implemented in v2. Can be added as `/Os` (optimize-for-size) flag when binary size
is the primary concern (e.g., IoT devices, DLL size-constrained scenarios).

## Implementation Phases

### Phase 1: Shared Runtime Header
Create `include/winmidl/proxy_helpers.h`:
- `type_info_t`, `qi_entry_t` structs
- `qi_check()`, `get_iids()`, `get_runtime_class_name()`, `get_trust_level()`
- Include in generated proxy .cpp files

### Phase 2: Refactor IInspectable + QI
- `ProxyGenerator.cs`: emit constexpr `type_info_t` + one-liner overrides
- `StubGenerator.cs`: same for stub QI
- Remove all inline QI/GetIids/GetRuntimeClassName code

### Phase 3: Signature Folding
- Catalog unique signature shapes across all interfaces in a compilation
- Emit one `proxy_call_<shape>` and one `stub_invoke_<shape>` per unique shape
- Wire vtable entries as `{ return shape_fn(ch, iid, METHOD_INDEX, args...); }`

### Phase 4: WIL Integration
- Use `wil::com_ptr_nothrow` for channel/server pointers
- Use try/catch with `wil::check_hresult` instead of `if (FAILED(hr))` chains
- More space-efficient on amd64

### Phase 5: E2E Validation
- Rebuild all e2e tests with new architecture
- Verify binary size reduction
- Verify no performance regression
- Compare generated code size: v1 vs v2

## Dependencies
- Phase 1 is independent
- Phase 2 depends on Phase 1
- Phase 3 depends on Phase 2
- Phase 4 is independent (can happen in parallel with Phase 3)
- Phase 5 depends on all
