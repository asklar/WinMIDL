#pragma once
#include <windows.h>
#include <objidl.h>
#include <winstring.h>
#include <inspectable.h>
#include <span>
#include <string_view>
#include <cstdint>

namespace winmidl {

struct qi_entry_t {
    IID iid;
    uint32_t offset;
    bool delegating;
};

struct type_info_t {
    std::span<const IID> iids;
    std::wstring_view className;
    std::span<const qi_entry_t> qiEntries;
};

inline HRESULT get_iids(type_info_t const& info, ULONG* count, IID** iids) {
    *count = static_cast<ULONG>(info.iids.size());
    auto cb = info.iids.size() * sizeof(IID);
    *iids = static_cast<IID*>(CoTaskMemAlloc(cb));
    if (!*iids) return E_OUTOFMEMORY;
    memcpy(*iids, info.iids.data(), cb);
    return S_OK;
}

inline HRESULT get_runtime_class_name(type_info_t const& info, HSTRING* name) {
    HSTRING_HEADER hdr;
    return WindowsCreateStringReference(
        info.className.data(),
        static_cast<UINT32>(info.className.size()),
        &hdr, name);
}

inline HRESULT get_trust_level(TrustLevel* level) {
    *level = BaseTrust;
    return S_OK;
}

inline HRESULT qi_check(type_info_t const& info,
                         void* pThis, IUnknown* pOuter,
                         REFIID riid, void** ppv) {
    for (auto& e : info.qiEntries) {
        if (e.iid == riid) {
            *ppv = reinterpret_cast<std::byte*>(pThis) + e.offset;
            if (e.delegating) pOuter->AddRef();
            else static_cast<IUnknown*>(*ppv)->AddRef();
            return S_OK;
        }
    }
    *ppv = nullptr;
    return E_NOINTERFACE;
}

// Overload for stub-side QI (no type_info, just QI entries)
inline HRESULT qi_check(std::span<const qi_entry_t> entries,
                         void* pThis, IUnknown* pOuter,
                         REFIID riid, void** ppv) {
    for (auto& e : entries) {
        if (e.iid == riid) {
            *ppv = reinterpret_cast<std::byte*>(pThis) + e.offset;
            if (pOuter) pOuter->AddRef();
            else static_cast<IUnknown*>(*ppv)->AddRef();
            return S_OK;
        }
    }
    *ppv = nullptr;
    return E_NOINTERFACE;
}

} // namespace winmidl
