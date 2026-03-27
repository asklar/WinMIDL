#pragma once
#include <windows.h>
#include <objidl.h>
#include <objbase.h>
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
// Null-safe: nullptr HSTRING writes count=0 (empty string).
inline void marshal_hstring(BYTE*& buf, HSTRING val) {
    UINT32 len = val ? WindowsGetStringLen(val) : 0;
    memcpy(buf, &len, sizeof(UINT32)); buf += sizeof(UINT32);
    if (len > 0) {
        PCWSTR raw = WindowsGetStringRawBuffer(val, nullptr);
        memcpy(buf, raw, len * sizeof(WCHAR)); buf += len * sizeof(WCHAR);
    }
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
// HSTRING deserialization (stub-side) — allocating, since RPC buffer data isn't null-terminated
// and WindowsCreateStringReference requires null termination.
inline HRESULT unmarshal_hstring_ref(BYTE*& buf, HSTRING_HEADER& /*hdr*/, HSTRING* out) {
    UINT32 len; memcpy(&len, buf, sizeof(UINT32)); buf += sizeof(UINT32);
    if (len == 0) {
        *out = nullptr;
        return S_OK;
    }
    HRESULT hr = WindowsCreateString(reinterpret_cast<PCWSTR>(buf), len, out);
    buf += len * sizeof(WCHAR);
    return hr;
}
// Legacy overload for existing generated code
inline HSTRING unmarshal_hstring_ref(BYTE*& buf, HSTRING_HEADER& hdr) {
    HSTRING result = nullptr;
    unmarshal_hstring_ref(buf, hdr, &result);
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

// Get the marshaled size of an interface pointer for buffer allocation.
inline HRESULT get_marshal_size(IUnknown* pInterface, REFIID riid, UINT32& size) {
    if (!pInterface) {
        size = sizeof(UINT32);
        return S_OK;
    }
    DWORD mshlSize = 0;
    HRESULT hr = CoGetMarshalSizeMax(&mshlSize, riid, pInterface, MSHCTX_INPROC, nullptr, MSHLFLAGS_NORMAL);
    if (FAILED(hr)) return hr;
    size = sizeof(UINT32) + static_cast<UINT32>(mshlSize);
    return S_OK;
}

// ════════════════════════════════════════════════════════════════
// Interface pointer marshaling via CoMarshalInterface/CoUnmarshalInterface
// Wire format: [uint32 streamSize][streamBytes...]
// ════════════════════════════════════════════════════════════════

// Marshal an interface pointer into the RPC buffer.
// Null-safe: nullptr writes size=0 without calling CoMarshalInterface.
// Returns S_OK on success. Advances buf past the written data.
inline HRESULT marshal_interface(BYTE*& buf, IUnknown* pInterface, REFIID riid) {
    if (!pInterface) {
        UINT32 zero = 0;
        memcpy(buf, &zero, sizeof(UINT32));
        buf += sizeof(UINT32);
        return S_OK;
    }
    IStream* pStream = nullptr;
    HRESULT hr = CreateStreamOnHGlobal(nullptr, TRUE, &pStream);
    if (FAILED(hr)) return hr;
    hr = CoMarshalInterface(pStream, riid, pInterface, MSHCTX_INPROC, nullptr, MSHLFLAGS_NORMAL);
    if (FAILED(hr)) { pStream->Release(); return hr; }
    STATSTG stat;
    pStream->Stat(&stat, STATFLAG_NONAME);
    UINT32 streamSize = static_cast<UINT32>(stat.cbSize.QuadPart);
    memcpy(buf, &streamSize, sizeof(UINT32));
    buf += sizeof(UINT32);
    LARGE_INTEGER zero_li = {};
    pStream->Seek(zero_li, STREAM_SEEK_SET, nullptr);
    ULONG bytesRead;
    pStream->Read(buf, streamSize, &bytesRead);
    buf += streamSize;
    pStream->Release();
    return S_OK;
}

// Unmarshal an interface pointer from the RPC buffer.
// Null-safe: size=0 yields *ppInterface=nullptr without calling CoUnmarshalInterface.
// Returns S_OK on success. Advances buf past the read data.
inline HRESULT unmarshal_interface(const BYTE*& buf, REFIID riid, void** ppInterface) {
    UINT32 streamSize;
    memcpy(&streamSize, buf, sizeof(UINT32));
    buf += sizeof(UINT32);
    if (streamSize == 0) {
        *ppInterface = nullptr;
        return S_OK;
    }
    HGLOBAL hGlobal = GlobalAlloc(GMEM_MOVEABLE, streamSize);
    if (!hGlobal) return E_OUTOFMEMORY;
    void* pMem = GlobalLock(hGlobal);
    memcpy(pMem, buf, streamSize);
    GlobalUnlock(hGlobal);
    buf += streamSize;
    IStream* pStream = nullptr;
    HRESULT hr = CreateStreamOnHGlobal(hGlobal, TRUE, &pStream);
    if (FAILED(hr)) { GlobalFree(hGlobal); return hr; }
    hr = CoUnmarshalInterface(pStream, riid, ppInterface);
    pStream->Release();
    return hr;
}

// Overload for mutable buf pointer (stub-side code that uses BYTE* not const BYTE*)
inline HRESULT unmarshal_interface(BYTE*& buf, REFIID riid, void** ppInterface) {
    const BYTE* cbuf = buf;
    HRESULT hr = unmarshal_interface(cbuf, riid, ppInterface);
    buf = const_cast<BYTE*>(cbuf);
    return hr;
}

// ════════════════════════════════════════════════════════════════
// Array serialization/deserialization helpers
// ════════════════════════════════════════════════════════════════

// Marshal a fixed-size array: [uint32 count][element × count]
// Null-safe: count=0 writes only the count prefix; items is not dereferenced.
template<typename T>
inline void marshal_array(BYTE*& buf, const T* items, UINT32 count) {
    memcpy(buf, &count, sizeof(UINT32));
    buf += sizeof(UINT32);
    if (count > 0) {
        memcpy(buf, items, count * sizeof(T));
        buf += count * sizeof(T);
    }
}

// Unmarshal a fixed-size array (allocating with CoTaskMemAlloc)
template<typename T>
inline HRESULT unmarshal_array(const BYTE*& buf, T** items, UINT32* count) {
    memcpy(count, buf, sizeof(UINT32));
    buf += sizeof(UINT32);
    if (*count == 0) { *items = nullptr; return S_OK; }
    *items = static_cast<T*>(CoTaskMemAlloc(*count * sizeof(T)));
    if (!*items) return E_OUTOFMEMORY;
    memcpy(*items, buf, *count * sizeof(T));
    buf += *count * sizeof(T);
    return S_OK;
}

// Overload with mutable buf pointer (for stub-side code)
template<typename T>
inline HRESULT unmarshal_array(BYTE*& buf, T** items, UINT32* count) {
    memcpy(count, buf, sizeof(UINT32));
    buf += sizeof(UINT32);
    if (*count == 0) { *items = nullptr; return S_OK; }
    *items = static_cast<T*>(CoTaskMemAlloc(*count * sizeof(T)));
    if (!*items) return E_OUTOFMEMORY;
    memcpy(*items, buf, *count * sizeof(T));
    buf += *count * sizeof(T);
    return S_OK;
}

// Marshal an HSTRING array: [uint32 count][hstring × count]
inline void marshal_hstring_array(BYTE*& buf, HSTRING* items, UINT32 count) {
    memcpy(buf, &count, sizeof(UINT32));
    buf += sizeof(UINT32);
    for (UINT32 i = 0; i < count; i++) {
        marshal_hstring(buf, items[i]);
    }
}

// Unmarshal an HSTRING array (allocating with CoTaskMemAlloc)
inline HRESULT unmarshal_hstring_array(BYTE*& buf, HSTRING** items, UINT32* count) {
    memcpy(count, buf, sizeof(UINT32));
    buf += sizeof(UINT32);
    if (*count == 0) { *items = nullptr; return S_OK; }
    *items = static_cast<HSTRING*>(CoTaskMemAlloc(*count * sizeof(HSTRING)));
    if (!*items) return E_OUTOFMEMORY;
    for (UINT32 i = 0; i < *count; i++) {
        HRESULT hr = unmarshal_hstring(buf, &(*items)[i]);
        if (FAILED(hr)) {
            for (UINT32 j = 0; j < i; j++) WindowsDeleteString((*items)[j]);
            CoTaskMemFree(*items);
            *items = nullptr;
            *count = 0;
            return hr;
        }
    }
    return S_OK;
}

// Compute buffer size for an HSTRING array
inline UINT32 hstring_array_size(HSTRING* items, UINT32 count) {
    UINT32 size = sizeof(UINT32); // count prefix
    for (UINT32 i = 0; i < count; i++) {
        size += sizeof(UINT32) + WindowsGetStringLen(items[i]) * sizeof(WCHAR);
    }
    return size;
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
