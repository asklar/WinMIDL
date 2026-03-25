# Wire Format Specification

This document describes the binary wire format used by the MIDL-generated proxy/stub code for WinRT COM marshaling. This format replaces NDR (Network Data Representation) format strings with direct compiled serialization.

## Overview

Each RPC call is transmitted as two buffers:
- **Request buffer**: Contains serialized input parameters (sent by the proxy)
- **Reply buffer**: Contains serialized output parameters, return value, and HRESULT (sent by the stub)

There is **no header** in either buffer — the method index is carried in `RPCOLEMESSAGE.iMethod`.

## Vtable Slot Assignment

| Slots | Owner |
|-------|-------|
| 0–2 | IUnknown (QueryInterface, AddRef, Release) |
| 3–5 | IInspectable (GetIids, GetRuntimeClassName, GetTrustLevel) |
| 6+ | User-defined members in declaration order |

Members expand to ABI slots as follows:
- **Property (get/set)**: getter slot, then setter slot
- **Method**: one slot
- **Event**: `add_` slot, then `remove_` slot

## Request Buffer Layout

Parameters are serialized in declaration order with no padding or alignment gaps.

```
[param_0][param_1]...[param_N]
```

For fixed-size signatures (no strings or variable-length types), the buffer size is a compile-time constant:

```cpp
static constexpr UINT32 kSetPosition_RequestSize = sizeof(Point); // blittable struct
msg.cbBuffer = kSetPosition_RequestSize;
```

For variable-size signatures:

```cpp
msg.cbBuffer = sizeof(uint32_t) + WindowsGetStringLen(name) * sizeof(wchar_t);
```

## Reply Buffer Layout

Reply layout order is always:

```
[out_param_0]...[out_param_N][return_value][HRESULT]
```

The trailing HRESULT (4 bytes) is always present, even for `void` methods.

For fixed-size replies, the size is also a compile-time constant:

```cpp
static constexpr UINT32 kReplySize = sizeof(int32_t) + sizeof(int32_t) + sizeof(HRESULT);
pMsg->cbBuffer = kReplySize;
```

## Type Encoding

### Primitive Types

All primitives are encoded at their natural size with no alignment padding:

| Type | Wire Size (bytes) |
|------|------------------|
| Boolean | 1 |
| UInt8 | 1 |
| Char (WCHAR) | 2 |
| Int16 | 2 |
| UInt16 | 2 |
| Int32 | 4 |
| UInt32 | 4 |
| Single (FLOAT) | 4 |
| HRESULT | 4 |
| Int64 | 8 |
| UInt64 | 8 |
| Double | 8 |

### GUID

GUIDs are encoded as 16 raw bytes (`sizeof(GUID)`).

### EventRegistrationToken

Encoded as 8 raw bytes (`sizeof(EventRegistrationToken)`).

### Enum Encoding

Enums are serialized as `INT32` (4 bytes) with an explicit `static_cast`:

```cpp
// Serialize
INT32 val = static_cast<INT32>(level);
memcpy(p, &val, 4); p += 4;

// Deserialize
memcpy(&level, p, 4); p += 4;
```

### HSTRING Encoding

Strings use a length-prefixed UTF-16 encoding:

```
[UINT32 char_count][WCHAR × char_count]
```

- `char_count` is the number of UTF-16 code units (not bytes)
- The payload is `char_count × 2` bytes
- A zero-length string is encoded as just `char_count = 0` with no payload

#### Proxy side (serialize outgoing, deserialize reply)

```cpp
// Serialize
UINT32 len = WindowsGetStringLen(name);
memcpy(p, &len, sizeof(len)); p += sizeof(len);
PCWSTR raw = WindowsGetStringRawBuffer(name, nullptr);
memcpy(p, raw, len * sizeof(wchar_t)); p += len * sizeof(wchar_t);

// Deserialize reply (allocating — buffer is freed after)
UINT32 len; memcpy(&len, pReply, sizeof(len)); pReply += sizeof(len);
WindowsCreateString((PCWSTR)pReply, len, &str); pReply += len * sizeof(wchar_t);
```

#### Stub side (zero-copy deserialize)

On the stub side, the RPC buffer is alive for the duration of the `Invoke` call. We use `WindowsCreateStringReference` to create a **non-owning view** into the buffer, avoiding allocation and copy:

```cpp
UINT32 len; memcpy(&len, p, sizeof(len)); p += sizeof(len);
HSTRING_HEADER hdr;
HSTRING str;
WindowsCreateStringReference(reinterpret_cast<PCWSTR>(p), len, &hdr, &str);
p += len * sizeof(wchar_t);
// str is valid until Invoke returns — no WindowsDeleteString needed
```

### Struct Encoding

#### Blittable Structs

A struct is **blittable** if all its fields are primitive types (no enums, no strings, no pointers). For blittable structs, the codegen emits a single `memcpy` with compile-time layout verification:

```cpp
static_assert(offsetof(Point, X) == 0);
static_assert(offsetof(Point, Y) == 4);
static_assert(sizeof(Point) == 8);
static_assert(std::is_trivially_copyable_v<Point>);
memcpy(p, &pos, sizeof(Point)); p += sizeof(Point);
```

This is safe because:
1. The `static_assert` on `offsetof` verifies the C++ compiler's layout matches the wire format
2. `is_trivially_copyable_v` ensures `memcpy` is a valid copy mechanism
3. No padding gaps exist (verified by `sizeof` == sum of field sizes)

#### Non-blittable Structs

Structs containing enums, strings, or other non-trivial types are serialized field-by-field:

```cpp
memcpy(p, &item.Id, sizeof(item.Id)); p += sizeof(item.Id);
int32_t State_val = static_cast<int32_t>(item.State);
memcpy(p, &State_val, sizeof(State_val)); p += sizeof(State_val);
```

## Alignment Rules

**There are no alignment gaps.** All types are packed sequentially in the buffer with no padding between them. This differs from NDR which requires alignment to natural boundaries.

This works because:
- All reads/writes use `memcpy`, which handles unaligned access correctly
- The `static_assert` checks verify that struct layouts match expectations
- Modern CPUs handle unaligned access efficiently

## Why This Is Faster Than NDR

| Aspect | NDR | WinMIDL Compiled |
|--------|-----|------------------|
| Format strings | Interpreted at runtime | Compiled to direct memcpy |
| Buffer sizes | Computed by interpreter | `constexpr` at compile time (fixed-size) |
| String handling | Always allocates + copies | Zero-copy on stub side (`WindowsCreateStringReference`) |
| Struct handling | Field-by-field with alignment | Single `memcpy` for blittable structs |
| Code generation | Shared interpreter + per-interface format strings | Shared helpers + per-interface data tables |
| Identical signatures | Each method gets format string entries | Folded: one function per unique signature shape |
| Compile-time safety | None | `static_assert` on struct layouts |

### Key performance wins:

1. **constexpr buffer sizes**: For fixed-size signatures, `GetBuffer` is called with a compile-time constant — no runtime computation.
2. **Zero-copy strings**: `WindowsCreateStringReference` avoids heap allocation and `memcpy` for every string parameter on the stub side.
3. **Blittable struct memcpy**: A struct with N fields is copied in one `memcpy` call instead of N separate calls.
4. **No interpreter overhead**: NDR walks format strings at runtime; compiled proxies are straight-line code.
5. **Signature folding**: Methods with identical parameter/return shapes share one implementation. 50 HSTRING property getters → 1 function.
6. **Data-driven IInspectable**: `GetIids`, `GetRuntimeClassName`, `QueryInterface` are shared helpers parameterized by constexpr data tables — zero per-interface code.

## Async Method Safety

For methods returning `IAsyncOperation<T>`, `IAsyncAction`, or related async types, the stub MUST NOT use zero-copy `WindowsCreateStringReference` for input HSTRING parameters. The server may capture params beyond the call lifetime. These methods use allocating `WindowsCreateString` instead.
