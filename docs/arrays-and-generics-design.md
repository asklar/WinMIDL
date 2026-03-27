# Design: Array Syntax and Custom Generics for MIDL3

## Overview

Two related language extensions that WinMIDL could enable:
1. **Array syntax** — first-class array parameter support in MIDL3 (currently only expressible in classic MIDL2)
2. **Custom generics** — user-defined parameterized interfaces and delegates (currently restricted to the built-in set shipped by Microsoft)

Both are made possible by our custom proxy/stub codegen — the traditional NDR-based marshaling was the main technical blocker for both features.

---

## Part 1: Array Syntax

### Current state

WinRT arrays at the ABI level use a parameter pair pattern:
```c
// PassArray (caller provides, callee reads)
HRESULT ProcessItems([in] UINT32 itemsSize, [in, size_is(itemsSize)] INT32* items);

// FillArray (caller allocates, callee fills)
HRESULT GetItems([in] UINT32 capacity, [out, size_is(capacity)] INT32* items, [out] UINT32* actual);

// ReceiveArray (callee allocates via CoTaskMemAlloc, caller frees)
HRESULT GetAllItems([out] UINT32* itemsSize, [out, size_is(, *itemsSize)] INT32** items);
```

This is only expressible in MIDL2 (with `size_is` attributes). MIDL3 has no array syntax — you must use `IVector<T>` instead.

### Proposed MIDL3 syntax

```idl
namespace MyApp
{
    interface IDataProcessor
    {
        // PassArray — caller sends data
        void ProcessItems(Int32[] items);

        // ReceiveArray — callee returns data
        Int32[] GetItems();

        // Out array — callee fills caller-provided buffer
        void ReadInto(out Int32[] buffer);

        // String arrays
        void SetNames(String[] names);
        String[] GetNames();

        // Struct arrays
        Point[] GetPolygon();
    }
}
```

### What needs to change

#### 1. Lexer/Parser
- Add `[]` suffix as a type modifier: `Int32[]`, `String[]`, `Point[]`
- Parse as a `TypeRef` with `IsArray = true` and `ElementType` pointing to the inner type
- Handle in parameter declarations, return types, and property types

#### 2. Analysis/Binder
- `ParameterSymbol.IsArray` property
- Array classification: determine PassArray vs ReceiveArray vs FillArray based on parameter direction
  - `in` array → PassArray: ABI becomes `(UINT32 count, T* items)`
  - `out` array → FillArray or ReceiveArray (could use a heuristic or attribute)
  - Return type array → ReceiveArray: ABI becomes `(UINT32* count, T** items)` as trailing retval
- Validate: array element types must be valid WinRT types
- Note: traditional WinRT disallows arrays of arrays — this was an NDR limitation. With our custom wire format, we could support jagged arrays (`Int32[][]`) by nesting the `[count][elements]` encoding. Whether to allow this is a design choice: it extends the WinRT type system beyond what existing projections expect.

#### 3. Header emitter
Expand `Int32[] items` into the ABI pair:
```cpp
// PassArray
HRESULT ProcessItems(UINT32 __itemsSize, INT32* items);
// ReceiveArray
HRESULT GetItems(UINT32* __returnSize, INT32** __returnItems);
```

#### 4. WinMD emitter
Emit the standard WinRT array metadata:
- Parameter with `[in]` + `System.Runtime.InteropServices.WindowsRuntime.ReadOnlyArrayAttribute`
- Parameter with `[out]` + `System.Runtime.InteropServices.WindowsRuntime.WriteOnlyArrayAttribute`
- The count parameter is implicit in metadata (no explicit param — inferred by the runtime projection)

#### 5. Proxy/stub codegen
Already implemented — `marshal_array` / `unmarshal_array` helpers handle the wire encoding:
```
[uint32 count][element × count]
```
The codegen just needs to detect `IsArray` on the TypeRef (which it already does) and emit the right calls.

#### 6. Decompiler
When reading WinMD files that have array parameters (from other compilers), detect the `ReadOnlyArrayAttribute` / `WriteOnlyArrayAttribute` and decompile back to `T[]` syntax.

### Wire format (already implemented)
```
[uint32 count][element × count]
```
- Fixed-size elements (Int32, Double, blittable structs): bulk `memcpy`
- Variable-size elements (String): per-element serialization
- Element count prefixed as UINT32

### Compatibility
- Consumers (C#, C++/WinRT) already support array parameters — the WinMD metadata is standard
- The syntax sugar (`T[]`) is purely a MIDL3 authoring convenience
- ABI is identical to what classic MIDL2 generates

---

## Part 2: Custom Generics

### Current state

WinRT restricts generic (parameterized) types to a fixed set shipped by Microsoft:
- `IVector<T>`, `IVectorView<T>`, `IMap<K,V>`, `IMapView<K,V>`, `IIterable<T>`, `IIterator<T>`
- `IReference<T>`, `IAsyncOperation<T>`, `EventHandler<T>`, `TypedEventHandler<S,A>`
- A few others

This restriction exists because:
1. **NDR can't generate proxy/stub code for arbitrary generic instantiations** — the proxy/stub for each instantiation must exist at compile time
2. **The parameterized IID algorithm** (UUID v5) requires knowing the type signature of each argument — which requires metadata resolution

WinMIDL removes blocker #1 since we generate direct C++ proxy/stub code. Blocker #2 is solved by our PIID computation.

### Proposed syntax

```idl
namespace MyApp
{
    // Custom parameterized interface
    interface IRepository<T>
    {
        T GetById(Int32 id);
        void Add(T item);
        T[] GetAll();
        Int32 Count { get; };
    }

    // Custom parameterized delegate
    delegate void ItemHandler<T>(T item, Int32 index);

    // Instantiation
    runtimeclass WidgetStore : IRepository<Widget>
    {
        WidgetStore();
        event ItemHandler<Widget> WidgetAdded;
    }

    // Multiple type parameters
    interface IPair<TKey, TValue>
    {
        TKey Key { get; };
        TValue Value { get; };
    }

    // Type constraints (future)
    // interface ISortable<T> requires IComparable<T> { ... }
}
```

### What needs to change

#### 1. Lexer/Parser
- Support `<T>`, `<T, U>` type parameter lists on `interface` and `delegate` declarations
- Parse type parameters as `GenericParameter` nodes
- Handle type argument lists in type references: `IRepository<Widget>`
- Distinguish between generic definitions (`interface IFoo<T>`) and instantiations (`IFoo<Widget>`)

#### 2. Analysis/Binder

##### Generic definitions
- New `GenericParameterSymbol` type representing `T`, `U`, etc.
- `InterfaceSymbol.TypeParameters: List<GenericParameterSymbol>`
- Type parameters are usable within the interface body as type references
- Validate: type parameters can only appear in parameter types, return types, and property types
- Validate: type parameters cannot be used as base types or in `requires` clauses (WinRT limitation)

##### Generic instantiations
- When encountering `IRepository<Widget>`, create a **bound generic type** that substitutes `T → Widget`
- Each instantiation produces a concrete interface with concrete method signatures
- The bound type's members have substituted types: `T GetById(Int32)` → `Widget GetById(Int32)`

##### IID computation
- Each instantiation has a unique IID computed via the UUID v5 PIID algorithm
- The base PIID comes from the generic definition's `[uuid]` attribute
- Type argument signatures use the standard WinRT type signature format
- For custom types: `rc(MyApp.Widget;{default-interface-iid})` or `{interface-iid}`
- For built-in types: `i4`, `string`, `f8`, etc.
- Already implemented in `PiidGenerator.cs`

#### 3. Header emitter

##### Generic definitions (for consumption by other code)
```cpp
// The generic definition itself is not instantiable in C++ — 
// only specific instantiations are.
// The header emits each used instantiation as a concrete interface.

// IRepository<Widget>
MIDL_INTERFACE("{computed-piid}")
__x_MyApp_IRepository_1___x_MyApp_Widget : public IInspectable
{
    virtual HRESULT GetById(INT32 id, __x_MyApp_IWidget** result) = 0;
    virtual HRESULT Add(__x_MyApp_IWidget* item) = 0;
    virtual HRESULT GetAll(UINT32* count, __x_MyApp_IWidget*** items) = 0;
    virtual HRESULT get_Count(INT32* value) = 0;
};
```

Each instantiation encountered in the compilation gets its own concrete C++ interface.

##### Generic definition metadata
The header should also emit a comment or documentation indicating the generic origin:
```cpp
// Generic: IRepository<T> instantiated as IRepository<Widget>
```

#### 4. WinMD emitter

.NET metadata natively supports generics:
- `TypeDef` for the generic definition: `IRepository`1` with `GenericParam` rows for `T`
- `TypeSpec` for each instantiation: `IRepository`1<Widget>` via `GenericInst` signature
- Method signatures reference `!0` (first type parameter) which gets substituted by the runtime
- The `[uuid]` attribute on the definition becomes the base PIID

This is standard ECMA-335 metadata — no WinRT-specific extensions needed.

#### 5. Proxy/stub codegen

This is where WinMIDL's custom proxy/stub shines. For each **instantiation** encountered:

```
IRepository<Widget> at compile time:
  → Generated proxy class CIRepository_Widget_Proxy
  → Generated stub class CStub_IRepository_Widget
  → IID: computed via UUID v5 from base PIID + Widget type signature
  → Methods: concrete types substituted (T → Widget*)
  → Marshaling: Widget is an interface pointer → CoMarshalInterface
```

The key insight: **we generate code per-instantiation, not per-definition**. The generic definition is a template; the codegen only runs for concrete instantiations it encounters.

##### What about instantiations we don't see at compile time?

If component A defines `IRepository<T>` and component B uses `IRepository<Foo>`, but we're only compiling A:
- A's WinMD contains the generic definition
- B's compiler (WinMIDL or another) generates the proxy/stub for `IRepository<Foo>` when compiling B
- Each consumer generates proxy/stub for the instantiations it uses
- IIDs are deterministic (UUID v5), so A and B agree on the IID without coordination

This is the same model as C++ templates — instantiation happens at the use site.

##### Marshaling unknown generic type arguments

When marshaling `T` in the proxy/stub, we need to know how to serialize `T` without knowing `T` at definition time. The solution:

**At instantiation time, `T` is known.** `IRepository<Widget>` knows `Widget` is a runtimeclass → interface pointer marshaling. `IRepository<Int32>` knows `Int32` → 4-byte memcpy. The codegen specializes per instantiation.

**What if T is itself a generic?** `IRepository<IVector<String>>` → `T` is `IVector<String>` → interface pointer marshaling with the PIID for `IVector<String>`. Fully resolved at compile time.

**Cross-process marshaling** works because:
- The proxy DLL registers each instantiation's IID → CLSID mapping
- `CoMarshalInterface` with the instantiation's IID finds the right proxy/stub
- The wire format is fully determined at compile time per instantiation

#### 6. Language projections (consumption)

##### C++/WinRT
`cppwinrt.exe` reads WinMD and generates C++ projections. It already handles the built-in generics. For custom generics:
- The `GenericParam` metadata is standard — cppwinrt should handle it
- Each `TypeSpec` instantiation generates a concrete projected type
- Risk: cppwinrt may have a hardcoded allowlist of generic types. Would need testing.

##### C#/WinRT
CsWinRT generates C# projections. .NET natively supports generics, so:
- The WinMD metadata maps directly to C# generic interfaces
- `IRepository<Widget>` → `IRepository<Widget>` in C#
- No special handling needed — it's standard .NET metadata

##### JavaScript
The Chakra/V8 projection would need to handle custom generics. It already handles the built-in ones, so the infrastructure exists. Risk: may have a hardcoded allowlist.

#### 7. Runtime registration

Each instantiation needs:
- Registry entry: `Interface\{instantiation-IID}\ProxyStubClsid32 → {proxy-dll-clsid}`
- The proxy DLL handles all instantiations from the same compilation

##### Discovery problem
If component A defines `IRepository<T>` and component B uses `IRepository<Foo>`, the proxy/stub for `IRepository<Foo>` lives in B's proxy DLL. A's proxy DLL doesn't know about `Foo`. This is correct — each consumer registers its own instantiations.

But what if component C also uses `IRepository<Foo>`? Both B and C would register the same IID. This is fine — COM uses "last writer wins" for registry entries, and since the wire format is deterministic, any proxy DLL that handles `IRepository<Foo>` will produce compatible marshaling.

### Risks and mitigations

| Risk | Severity | Mitigation |
|------|----------|------------|
| cppwinrt rejects custom generics | High | Test early; may need cppwinrt changes |
| CsWinRT rejects custom generics | Medium | Standard .NET metadata — likely works |
| Binary size explosion (many instantiations) | Medium | Signature folding reduces per-instantiation code |
| Slow compilation (many instantiations) | Low | Only instantiate what's used |
| Backwards compatibility with tools expecting closed generic set | Medium | Opt-in via compiler flag initially |
| WinRT type system validators reject custom generics | High | May need advocacy with platform team |

### Implementation phases

#### Phase 1: Array syntax
- Parser: `T[]` type modifier
- Binder: PassArray/ReceiveArray classification
- Header emitter: expand to ABI pair
- WinMD emitter: standard array metadata
- Decompiler: detect and round-trip
- Tests: full coverage

#### Phase 2: Custom generic definitions
- Parser: `<T>` type parameter lists
- Binder: GenericParameterSymbol, type substitution
- WinMD emitter: GenericParam rows
- Header emitter: per-instantiation concrete interfaces
- Tests: definition + instantiation round-trip

#### Phase 3: Custom generic proxy/stub
- Codegen: per-instantiation proxy/stub with type-substituted marshaling
- PIID computation: extend existing algorithm (already works)
- E2E test: custom generic interface marshaled across apartments
- PIID validation against RoGetParameterizedTypeInstanceIID

#### Phase 4: Cross-language validation
- Test consumption from C++/WinRT, C#/WinRT
- File issues with cppwinrt/cswinrt if needed
- Document any limitations

### Open questions

1. **Generic runtimeclasses.** WinRT only has generic interfaces and delegates today. However, generic runtimeclasses are a natural extension — the compiler would synthesize a generic default interface. For example:
   ```idl
   runtimeclass Repository<T>
   {
       Repository();
       T GetById(Int32 id);
       void Add(T item);
   }
   ```
   Would synthesize `interface IRepository<T> { T GetById(Int32); void Add(T); }` as the default interface, with `Repository<T> : IRepository<T>`. Each instantiation (`Repository<Widget>`) gets its own concrete default interface IID and proxy/stub. The runtimeclass metadata uses `GenericParam` rows on both the class and the synthesized interface.

2. **Type constraints?** MIDL3 has `requires` for interface inheritance. Could we extend this to generic constraints? e.g. `interface ISortable<T> requires IComparable<T>` — meaning T must implement IComparable<T>.

3. **Variance?** .NET supports covariance (`out T`) and contravariance (`in T`) on generic type parameters. WinRT's built-in generics don't use variance. Should we support it?

4. **Generic structs?** Less useful since structs are value types and can't be polymorphic, but `Pair<T1, T2>` could be handy.
