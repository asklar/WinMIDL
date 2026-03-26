#pragma once
#include <windows.h>
#include <objidl.h>
#include <winstring.h>
#include <inspectable.h>
#include <eventtoken.h>
#include <span>
#include <string_view>
#include <cstdint>
#include <type_traits>

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
template<size_t N>
inline HRESULT qi_check(const qi_entry_t (&entries)[N],
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

// ════════════════════════════════════════════════════════════════
// Serialization helpers — advance buffer pointer after writing
// ════════════════════════════════════════════════════════════════

inline void marshal_bool(BYTE*& buf, boolean val) {
    memcpy(buf, &val, sizeof(boolean)); buf += sizeof(boolean);
}
inline void marshal_uint8(BYTE*& buf, BYTE val) {
    memcpy(buf, &val, sizeof(BYTE)); buf += sizeof(BYTE);
}
inline void marshal_char16(BYTE*& buf, WCHAR val) {
    memcpy(buf, &val, sizeof(WCHAR)); buf += sizeof(WCHAR);
}
inline void marshal_int16(BYTE*& buf, INT16 val) {
    memcpy(buf, &val, sizeof(INT16)); buf += sizeof(INT16);
}
inline void marshal_uint16(BYTE*& buf, UINT16 val) {
    memcpy(buf, &val, sizeof(UINT16)); buf += sizeof(UINT16);
}
inline void marshal_int32(BYTE*& buf, INT32 val) {
    memcpy(buf, &val, sizeof(INT32)); buf += sizeof(INT32);
}
inline void marshal_uint32(BYTE*& buf, UINT32 val) {
    memcpy(buf, &val, sizeof(UINT32)); buf += sizeof(UINT32);
}
inline void marshal_float(BYTE*& buf, FLOAT val) {
    memcpy(buf, &val, sizeof(FLOAT)); buf += sizeof(FLOAT);
}
inline void marshal_int64(BYTE*& buf, INT64 val) {
    memcpy(buf, &val, sizeof(INT64)); buf += sizeof(INT64);
}
inline void marshal_uint64(BYTE*& buf, UINT64 val) {
    memcpy(buf, &val, sizeof(UINT64)); buf += sizeof(UINT64);
}
inline void marshal_double(BYTE*& buf, DOUBLE val) {
    memcpy(buf, &val, sizeof(DOUBLE)); buf += sizeof(DOUBLE);
}
inline void marshal_guid(BYTE*& buf, const GUID& val) {
    memcpy(buf, &val, sizeof(GUID)); buf += sizeof(GUID);
}
inline void marshal_event_token(BYTE*& buf, EventRegistrationToken val) {
    memcpy(buf, &val, sizeof(EventRegistrationToken)); buf += sizeof(EventRegistrationToken);
}
template<typename E>
inline void marshal_enum(BYTE*& buf, E val) {
    INT32 v = static_cast<INT32>(val);
    memcpy(buf, &v, sizeof(INT32)); buf += sizeof(INT32);
}
inline void marshal_hstring(BYTE*& buf, HSTRING val) {
    UINT32 len = WindowsGetStringLen(val);
    memcpy(buf, &len, sizeof(UINT32)); buf += sizeof(UINT32);
    PCWSTR raw = WindowsGetStringRawBuffer(val, nullptr);
    memcpy(buf, raw, len * sizeof(WCHAR)); buf += len * sizeof(WCHAR);
}
template<typename T>
inline void marshal_struct(BYTE*& buf, const T& val) {
    static_assert(std::is_trivially_copyable_v<T>);
    memcpy(buf, &val, sizeof(T)); buf += sizeof(T);
}

// ════════════════════════════════════════════════════════════════
// Deserialization helpers — advance buffer pointer after reading
// ════════════════════════════════════════════════════════════════

inline void unmarshal_bool(const BYTE*& buf, boolean& out) {
    memcpy(&out, buf, sizeof(boolean)); buf += sizeof(boolean);
}
inline void unmarshal_uint8(const BYTE*& buf, BYTE& out) {
    memcpy(&out, buf, sizeof(BYTE)); buf += sizeof(BYTE);
}
inline void unmarshal_char16(const BYTE*& buf, WCHAR& out) {
    memcpy(&out, buf, sizeof(WCHAR)); buf += sizeof(WCHAR);
}
inline void unmarshal_int16(const BYTE*& buf, INT16& out) {
    memcpy(&out, buf, sizeof(INT16)); buf += sizeof(INT16);
}
inline void unmarshal_uint16(const BYTE*& buf, UINT16& out) {
    memcpy(&out, buf, sizeof(UINT16)); buf += sizeof(UINT16);
}
inline void unmarshal_int32(const BYTE*& buf, INT32& out) {
    memcpy(&out, buf, sizeof(INT32)); buf += sizeof(INT32);
}
inline void unmarshal_uint32(const BYTE*& buf, UINT32& out) {
    memcpy(&out, buf, sizeof(UINT32)); buf += sizeof(UINT32);
}
inline void unmarshal_float(const BYTE*& buf, FLOAT& out) {
    memcpy(&out, buf, sizeof(FLOAT)); buf += sizeof(FLOAT);
}
inline void unmarshal_int64(const BYTE*& buf, INT64& out) {
    memcpy(&out, buf, sizeof(INT64)); buf += sizeof(INT64);
}
inline void unmarshal_uint64(const BYTE*& buf, UINT64& out) {
    memcpy(&out, buf, sizeof(UINT64)); buf += sizeof(UINT64);
}
inline void unmarshal_double(const BYTE*& buf, DOUBLE& out) {
    memcpy(&out, buf, sizeof(DOUBLE)); buf += sizeof(DOUBLE);
}
inline void unmarshal_guid(const BYTE*& buf, GUID& out) {
    memcpy(&out, buf, sizeof(GUID)); buf += sizeof(GUID);
}
inline void unmarshal_event_token(const BYTE*& buf, EventRegistrationToken& out) {
    memcpy(&out, buf, sizeof(EventRegistrationToken)); buf += sizeof(EventRegistrationToken);
}
template<typename E>
inline void unmarshal_enum(const BYTE*& buf, E& out) {
    INT32 v; memcpy(&v, buf, sizeof(INT32)); buf += sizeof(INT32);
    out = static_cast<E>(v);
}
// Zero-copy HSTRING deserialization (stub-side, sync methods)
inline HSTRING unmarshal_hstring_ref(BYTE*& buf, HSTRING_HEADER& hdr) {
    UINT32 len; memcpy(&len, buf, sizeof(UINT32)); buf += sizeof(UINT32);
    HSTRING result = nullptr;
    if (len > 0) {
        WindowsCreateStringReference(reinterpret_cast<PCWSTR>(buf), len, &hdr, &result);
    }
    buf += len * sizeof(WCHAR);
    return result;
}
// Allocating HSTRING deserialization (proxy reply side)
inline HRESULT unmarshal_hstring(BYTE*& buf, HSTRING* out) {
    UINT32 len; memcpy(&len, buf, sizeof(UINT32)); buf += sizeof(UINT32);
    HRESULT hr = WindowsCreateString(reinterpret_cast<PCWSTR>(buf), len, out);
    buf += len * sizeof(WCHAR);
    return hr;
}
// Allocating HSTRING deserialization (stub-side, async methods)
inline HRESULT unmarshal_hstring_alloc(BYTE*& buf, HSTRING* out) {
    return unmarshal_hstring(buf, out);
}
template<typename T>
inline void unmarshal_struct(const BYTE*& buf, T& out) {
    static_assert(std::is_trivially_copyable_v<T>);
    memcpy(&out, buf, sizeof(T)); buf += sizeof(T);
}

// Overloads with mutable buf pointer (for stub-side code that uses BYTE* not const BYTE*)
inline void unmarshal_int32(BYTE*& buf, INT32& out) {
    memcpy(&out, buf, sizeof(INT32)); buf += sizeof(INT32);
}
inline void unmarshal_uint32(BYTE*& buf, UINT32& out) {
    memcpy(&out, buf, sizeof(UINT32)); buf += sizeof(UINT32);
}
inline void unmarshal_int64(BYTE*& buf, INT64& out) {
    memcpy(&out, buf, sizeof(INT64)); buf += sizeof(INT64);
}
inline void unmarshal_uint64(BYTE*& buf, UINT64& out) {
    memcpy(&out, buf, sizeof(UINT64)); buf += sizeof(UINT64);
}
inline void unmarshal_int16(BYTE*& buf, INT16& out) {
    memcpy(&out, buf, sizeof(INT16)); buf += sizeof(INT16);
}
inline void unmarshal_uint16(BYTE*& buf, UINT16& out) {
    memcpy(&out, buf, sizeof(UINT16)); buf += sizeof(UINT16);
}
inline void unmarshal_float(BYTE*& buf, FLOAT& out) {
    memcpy(&out, buf, sizeof(FLOAT)); buf += sizeof(FLOAT);
}
inline void unmarshal_double(BYTE*& buf, DOUBLE& out) {
    memcpy(&out, buf, sizeof(DOUBLE)); buf += sizeof(DOUBLE);
}
inline void unmarshal_bool(BYTE*& buf, boolean& out) {
    memcpy(&out, buf, sizeof(boolean)); buf += sizeof(boolean);
}
inline void unmarshal_uint8(BYTE*& buf, BYTE& out) {
    memcpy(&out, buf, sizeof(BYTE)); buf += sizeof(BYTE);
}
inline void unmarshal_char16(BYTE*& buf, WCHAR& out) {
    memcpy(&out, buf, sizeof(WCHAR)); buf += sizeof(WCHAR);
}
inline void unmarshal_guid(BYTE*& buf, GUID& out) {
    memcpy(&out, buf, sizeof(GUID)); buf += sizeof(GUID);
}
inline void unmarshal_event_token(BYTE*& buf, EventRegistrationToken& out) {
    memcpy(&out, buf, sizeof(EventRegistrationToken)); buf += sizeof(EventRegistrationToken);
}
template<typename E>
inline void unmarshal_enum(BYTE*& buf, E& out) {
    INT32 v; memcpy(&v, buf, sizeof(INT32)); buf += sizeof(INT32);
    out = static_cast<E>(v);
}
template<typename T>
inline void unmarshal_struct(BYTE*& buf, T& out) {
    static_assert(std::is_trivially_copyable_v<T>);
    memcpy(&out, buf, sizeof(T)); buf += sizeof(T);
}

// ════════════════════════════════════════════════════════════════
// Method/interface descriptor types for data-driven dispatch
// ════════════════════════════════════════════════════════════════

enum class param_type : uint8_t {
    t_bool, t_uint8, t_char16,
    t_int16, t_uint16,
    t_int32, t_uint32, t_float,
    t_int64, t_uint64, t_double,
    t_guid, t_hstring, t_event_token,
    t_enum, t_struct, t_interface,
};

enum class param_dir : uint8_t {
    dir_in, dir_out, dir_retval,
};

struct param_descriptor {
    param_type type;
    param_dir dir;
};

struct method_descriptor {
    uint32_t vtable_slot;
    const char* name;
    const param_descriptor* params;
    uint32_t param_count;
};

struct interface_descriptor {
    const IID* iid;
    const method_descriptor* methods;
    uint32_t method_count;
};

} // namespace winmidl
