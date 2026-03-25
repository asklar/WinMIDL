# NDR Proxy/Stub Generation ΓÇö TODO

This documents the remaining work needed for NDR-based proxy/stub generation.
The implementation lives on branch `feature/abi-headers-ndr-proxystub`.

## What's Done (on the feature branch)

### ABI Header Generation
- Full MIDL-style boilerplate (rpc.h, version checks, MIDL_CONST_ID)
- Namespace-scoped C++ declarations with `#define` aliases
- Forward declarations with `FWD_DEFINED` guards
- ABI signature-based GUID computation (matches real midl.exe)
- C vtable block with `BEGIN_INTERFACE`/`END_INTERFACE`, COBJMACROS
- Parameterized interface template specializations (`__FI` mangled names)
- IID seed signature comment blocks
- Runtimeclass metadata comments
- Correct name mangling (`_C` separator), type mapping (`int`, `double`, `::boolean`)

### NDR Proxy/Stub (_p.c)
- `NdrTypeFormatBuilder` ΓÇö FC_* type encoding for primitives, HSTRING, interfaces, GUIDs, structs
- `NdrProcFormatBuilder` ΓÇö Oi2 procedure headers, parameter descriptors
- `ProxyVtableBuilder` ΓÇö CINTERFACE_PROXY_VTABLE, ObjectStublessClient thunks
- `StubDispatchBuilder` ΓÇö MIDL_STUB_DESC, MIDL_SERVER_INFO, CInterfaceStubVtbl
- `DllPlumbingBuilder` ΓÇö DllGetClassObject/DllRegisterServer/etc exports

### Tests
- ~600 CodeGen unit tests
- ~900 Integration tests
- E2E MSVC compilation test (validates generated files compile with cl.exe)

## What's Remaining for NDR 100% Parity

### High Priority
1. **Runtime validation** ΓÇö Load our generated proxy DLL into a real COM process and verify marshaling actually works (call across apartment/process boundary)
2. **HSTRING wire format** ΓÇö Verify FC_USER_MARSHAL encoding matches what NDR runtime expects
3. **Interface pointer marshaling** ΓÇö Verify FC_IP + IID encoding works with NDR's interface pointer marshaler
4. **Complex struct marshaling** ΓÇö FC_BOGUS_STRUCT for structs containing pointers/strings

### Medium Priority
5. **Array parameter support** ΓÇö NDR conformant/varying arrays for `T[]` params
6. **Generic interface proxy/stubs** ΓÇö Parameterized interface instantiations need their own proxy vtables
7. **Async method support** ΓÇö IAsyncOperation/IAsyncAction marshaling
8. **DllData.c generation** ΓÇö Merge multiple proxy files into one DLL

### Lower Priority
9. **Byte-level parity with real midl _p.c** ΓÇö Compare our format strings byte-by-byte against real midl output (hard because midl ICEs on WinRT without /nomidl which suppresses _p.c)
10. **NDR64 support** ΓÇö 64-bit NDR transfer syntax (may be needed for some scenarios)
11. **Custom marshaling attributes** ΓÇö `[custom]` marshaling for specific types

## Alternative Approach (per Jon Wiswall)
Instead of NDR byte strings, generate direct C++ marshaling code using flatbuffers/protobuf for same-machine scenarios. This would be more efficient than NDR's interpreted approach. See the conversation in the session notes for full context.
