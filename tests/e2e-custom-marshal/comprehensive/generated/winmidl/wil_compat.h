#pragma once

#if __has_include(<wil/com.h>)
#include <wil/com.h>
#include <wil/result.h>
#define WINMIDL_HAS_WIL 1
#else
#define WINMIDL_HAS_WIL 0

#define RETURN_IF_FAILED(hr) do { HRESULT __hr = (hr); if (FAILED(__hr)) return __hr; } while(0)

namespace winmidl {
    inline void throw_if_failed(HRESULT hr) { if (FAILED(hr)) throw hr; }
}

namespace wil {
    template<typename T>
    class com_ptr_nothrow {
        T* m_ptr = nullptr;
    public:
        com_ptr_nothrow() = default;
        ~com_ptr_nothrow() { if (m_ptr) m_ptr->Release(); }
        com_ptr_nothrow(com_ptr_nothrow&& o) noexcept : m_ptr(o.m_ptr) { o.m_ptr = nullptr; }
        com_ptr_nothrow& operator=(com_ptr_nothrow&& o) noexcept {
            if (m_ptr) m_ptr->Release(); m_ptr = o.m_ptr; o.m_ptr = nullptr; return *this;
        }
        com_ptr_nothrow(const com_ptr_nothrow&) = delete;
        com_ptr_nothrow& operator=(const com_ptr_nothrow&) = delete;
        T* get() const { return m_ptr; }
        T** put() { if (m_ptr) { m_ptr->Release(); m_ptr = nullptr; } return &m_ptr; }
        T* operator->() const { return m_ptr; }
        explicit operator bool() const { return m_ptr != nullptr; }
        void reset() { if (m_ptr) { m_ptr->Release(); m_ptr = nullptr; } }
    };
}
#endif
