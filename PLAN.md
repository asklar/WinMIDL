# MIDL Compiler Rewrite — Comprehensive Implementation Plan

## 1. Problem Statement

Rewrite the Microsoft Interface Definition Language (MIDL) compiler as a modern, incremental, editor-friendly toolchain. The new compiler must:

- **100% feature parity** for pure MIDL3 syntax (runtimeclass, delegate, namespace, contracts, parameterized types, WinRT attributes, etc.)
- **Best-effort parity** for mixed MIDL3 + classic MIDL + embedded C code
- **Error-recovering parser** for on-the-fly partial-file analysis (LSP requirement)
- **Language Server Protocol (LSP)** with syntax highlighting, autocomplete, diagnostics, go-to-definition, hover info
- **VS Code extension** integrating the LSP
- Produce all existing output formats: C proxy/stubs, headers, IID files, dlldata, WinMD metadata, type libraries

Target directory: `E:\midl`

---

## 2. Language Choice: C# / .NET 10

### Decision

**C# on .NET 10** is the implementation language. A thin **TypeScript** shell wraps the VS Code extension.

### Rationale

| Requirement | C# / .NET 10 Fit |
|---|---|
| **Error-recovering parser** | ★★★ Roslyn **invented** the lossless red-green syntax tree pattern; we follow the same architecture |
| **LSP implementation** | ★★★ `Microsoft.VisualStudio.LanguageServer.Protocol` NuGet; C# Dev Kit proves the pattern at scale |
| **VS Code extension** | ★★★ Thin TS shell spawns .NET LSP binary via stdio (same as C# Dev Kit, Razor, Bicep, PowerShell) |
| **WinMD (.winmd) output** | ★★★ `System.Reflection.Metadata` (SRM) — first-class, low-level ECMA-335 reader + writer; trivial |
| **WinMD (.winmd) reading** | ★★★ SRM reads imported `.winmd` for type resolution; no extra dependencies |
| **C header/stub generation** | ★★★ `StringBuilder`, string interpolation, `Span<char>` — excellent |
| **NDR format strings** | ★★★ `Span<byte>`, `BinaryWriter`, `BitConverter` — .NET 10 perf is excellent |
| **Type library (.tlb)** | ★★★ COM interop via `System.Runtime.InteropServices` — natural |
| **Performance** | ★★★ .NET 10 has NativeAOT, PGO, frozen objects; MIDL files are small — GC is irrelevant |
| **Developer accessibility** | ★★★ Windows team developers can read, maintain, and contribute immediately |
| **Cross-platform** | ★★★ .NET 10 runs on Windows, Linux, macOS |
| **Ecosystem** | ★★★ NuGet for everything; `System.Collections.Immutable` for persistent data structures |

### Key .NET 10 / C# 14 Features We Leverage
- **NativeAOT** — compile the LSP server and CLI to a single native binary (fast startup, no .NET runtime dependency for end users)
- **`System.Reflection.Metadata`** — zero-allocation ECMA-335 metadata reader/writer
- **`System.Collections.Immutable`** — persistent immutable collections for the red-green tree
- **`System.Text.Json`** — LSP protocol JSON serialization
- **Records and pattern matching** — clean AST node definitions
- **`Span<T>` / `Memory<T>`** — zero-copy parsing and code generation
- **Source generators** — generate typed AST accessors from syntax kind definitions

---

## 3. Architecture Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                        VS Code Extension                         │
│  (TypeScript — thin shell, spawns LSP, TextMate grammar for     │
│   basic highlighting, configuration, snippets)                   │
└──────────────────────┬───────────────────────────────────────────┘
                       │ LSP (stdio / pipe)
┌──────────────────────▼───────────────────────────────────────────┐
│                  LSP Server (C# / .NET 10 NativeAOT)             │
│  ┌─────────────┐  ┌──────────────┐  ┌────────────────────────┐  │
│  │  Document    │  │  Diagnostics │  │  Completions / Hover / │  │
│  │  Manager     │  │  Engine      │  │  Go-to-def / Rename    │  │
│  └──────┬──────┘  └──────┬───────┘  └────────────┬───────────┘  │
│         │                │                        │              │
│  ┌──────▼────────────────▼────────────────────────▼───────────┐  │
│  │        Incremental Analysis Engine (Roslyn-style)          │  │
│  │  - File text → Token stream (cached)                       │  │
│  │  - Token stream → Green tree (cached, error-recovering)    │  │
│  │  - Green tree → Red tree (on-demand, lazy)                 │  │
│  │  - Red tree → Semantic model (cached, incremental)         │  │
│  └──────┬─────────────────────────────────────────────────────┘  │
│         │                                                        │
│  ┌──────▼─────────────────────────────────────────────────────┐  │
│  │                    Core Compiler Pipeline                   │  │
│  │                                                             │  │
│  │  ┌─────────┐  ┌─────────┐  ┌──────────┐  ┌─────────────┐  │  │
│  │  │  Lexer  │→│ Parser  │→│ Semantic │→│  Code Gen   │  │  │
│  │  │(error-  │  │(error-  │  │ Analysis │  │ (on-demand) │  │  │
│  │  │ tolerant)│  │recovery)│  │(partial) │  │             │  │  │
│  │  └─────────┘  └─────────┘  └──────────┘  └─────────────┘  │  │
│  │                                                             │  │
│  │  ┌──────────────────────────────────────────────────────┐  │  │
│  │  │              Output Backends                          │  │  │
│  │  │  • C headers (.h)     • C proxy/stubs (_c.c, _s.c)  │  │  │
│  │  │  • IID file (_i.c)    • DllData (dlldata.c)          │  │  │
│  │  │  • WinMD (.winmd)     • Type library (.tlb)          │  │  │
│  │  │  • NDR format strings • Contract data                │  │  │
│  │  └──────────────────────────────────────────────────────┘  │  │
│  └─────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 4. Error-Recovering Parser Design

This is the **most critical architectural decision** for LSP quality. The parser must produce a useful syntax tree even when the input is incomplete or contains errors.

### Approach: Lossless Concrete Syntax Tree (CST) with Error Nodes

Following the **Roslyn** red-green tree pattern (which Roslyn invented for the C#/VB compilers):

1. **Lossless Green Tree**: Every character from the source file (including whitespace, comments, errors) is represented in an immutable "green" tree. Nothing is thrown away. Green nodes are reusable and share structure across edits.
2. **Lazy Red Tree**: A thin "red" wrapper provides parent pointers and absolute positions on-demand, without storing them in the immutable green tree. This is the API consumers use.
3. **Error recovery strategies**:
   - **Synchronization tokens**: On error, skip tokens until a known synchronization point (`}`, `;`, `]`, keyword like `interface`, `runtimeclass`, `namespace`, `struct`, `enum`)
   - **Expected-token insertion**: If a `{` is expected but not found, insert a synthetic `MissingToken` node and continue
   - **Bracket matching**: Track `[]`, `{}`, `()` nesting depth; use mismatched brackets as recovery points
   - **Attribute recovery**: `[` starts an attribute list; if parsing fails inside, skip to `]` and continue
   - **Statement recovery**: On error inside a method/field declaration, skip to `;` or `}`
   - **Declaration recovery**: On error at top level, skip to next `interface`/`runtimeclass`/`namespace`/`struct`/`enum`/`delegate`/`contract` keyword
4. **Error nodes**: Unparseable token sequences become `SkippedTokensTrivia` nodes in the green tree, preserving source text
5. **Partial AST access**: Typed AST wrappers use nullable properties (`SyntaxToken?`, `TypeSyntax?`) for missing children — consumers always get a tree, never null

### Why not tree-sitter?

Tree-sitter is excellent for syntax highlighting but has limitations for semantic analysis:
- Grammar is defined in JavaScript DSL, harder to maintain complex MIDL semantics
- Error recovery is automatic but not always semantically meaningful
- No access to symbol tables or type information during parsing
- Best used as a **secondary** parser for fast syntax highlighting in the VS Code extension

### Parser Technology Stack

```
Source text
    │
    ▼
┌─────────────────────────┐
│  Lexer (hand-written)    │  ← Error-tolerant: invalid chars become ERROR tokens
│  - All MIDL tokens       │  ← Context-aware: keyword vs identifier disambiguation
│  - C tokens (subset)     │  ← Produces SyntaxToken + SyntaxTrivia (whitespace, comments)
└─────────┬───────────────┘
          ▼
┌─────────────────────────┐
│  Parser (recursive       │  ← Error recovery at every production
│  descent, hand-          │  ← Produces immutable green tree (Roslyn pattern)
│  written)                │  ← Incremental: re-parse only changed regions
└─────────┬───────────────┘
          ▼
┌─────────────────────────┐
│  Green Tree (immutable)  │  ← Lossless, includes errors, trivia
│                          │  ← Structurally shared across edits
└─────────┬───────────────┘
          ▼
┌─────────────────────────┐
│  Red Tree (lazy wrapper) │  ← Parent pointers, absolute positions
│  + Typed syntax nodes    │  ← Strongly typed accessors (records)
│                          │  ← Nullable properties for missing nodes
└─────────┬───────────────┘
          ▼
┌─────────────────────────┐
│  Semantic Model          │  ← Symbol table, type resolution, validation
│  (incremental, cached)   │  ← Produces diagnostics
└─────────────────────────┘
```

---

## 5. Feature Parity Catalog

### 5.1 MIDL3 / WinRT Features (100% parity required)

| # | Feature | Status | Notes |
|---|---------|--------|-------|
| 1 | `namespace` declarations (nested, dotted) | Must have | Core MIDL3 |
| 2 | `runtimeclass` (sealed, unsealed, static) | Must have | Core MIDL3 |
| 3 | `interface` declarations (classic + modern syntax) | Must have | Methods, properties, events |
| 4 | `delegate` declarations | Must have | Typed callbacks |
| 5 | `struct` declarations (with per-field versioning) | Must have | Value types |
| 6 | `enum` declarations (with `[flags]`, per-value versioning) | Must have | Including `[v1_enum]` |
| 7 | `apicontract` declarations | Must have | Versioning contracts |
| 8 | Parameterized interfaces (`IVector<T>`, `IMap<K,V>`, etc.) | Must have | ~15 predefined pinterfaces |
| 9 | Modern base types (`Int32`, `String`, `Boolean`, `Guid`, `Object`, `Char`, `Single`, `Double`, `UInt8`...) | Must have | Map to classic types |
| 10 | Modern method syntax (`Type Method(params)` without HRESULT wrapping) | Must have | Core MIDL3 |
| 11 | Modern property syntax (`Type Prop { get; set; }`) | Must have | Read/write/static properties |
| 12 | Modern event syntax (`event DelegateType EventName`) | Must have | Add/remove auto-generated |
| 13 | Constructor syntax (`ClassName(params)`) | Must have | Default + factory activation |
| 14 | `static` methods and properties | Must have | Separate static interface |
| 15 | `protected` and `overridable` members | Must have | Composition pattern |
| 16 | `[activatable]` attribute (default + factory) | Must have | Type activation |
| 17 | `[composable]` attribute (public/protected) | Must have | Composition |
| 18 | `[contract(Name, Version)]` versioning | Must have | API contracts |
| 19 | `[deprecated]` attribute | Must have | Deprecation info |
| 20 | `[threading]` and `[marshaling_behavior]` | Must have | Threading model |
| 21 | `[exclusiveto]` attribute | Must have | Interface ownership |
| 22 | `[overload]` / `[default_overload]` / `[method_name]` | Must have | Method disambiguation |
| 23 | `[interface_name]` / `[static_name]` / `[constructor_name]` / `[protected_name]` / `[overridable_name]` | Must have | Explicit naming |
| 24 | `[default_interface]` / `[default]` | Must have | Default interface |
| 25 | `[internal]` / `[experimental]` / `[feature]` | Must have | Visibility control |
| 26 | `[fastabi]` attribute | Must have | Fast ABI |
| 27 | `[from_contract]` with ranges | Must have | Platform mapping |
| 28 | `[uuid]` on interfaces, delegates | Must have | GUID assignment |
| 29 | `[version]` / `[contractversion]` | Must have | Versioning |
| 30 | Custom attributes (`[attributeusage]`, `[attributename]`, `[allowmultiple]`) | Must have | Custom metadata |
| 31 | `declare { }` blocks | Must have | Forward pinterface instantiation |
| 32 | `import` statements (IDL files + WinMD files) | Must have | Module system |
| 33 | `[eventadd]` / `[eventremove]` on classic interface methods | Must have | Classic event pattern |
| 34 | `[remote_async]` / `[remote_sync]` | Must have | Async classification |
| 35 | WinMD output (ECMA-335 metadata) | Must have | Primary output |
| 36 | C/C++ header generation (for WinRT types) | Must have | Consumer headers |
| 37 | Interface synthesization (auto-generate `I<Class>`, `I<Class>Factory`, `I<Class>Statics`, `I<Class>Protected`, `I<Class>Overrides`) | Must have | Core MIDL3 compiler behavior — see [synthesizing-interfaces](https://learn.microsoft.com/en-us/uwp/midl-3/synthesizing-interfaces) |
| 38 | `[return_name]` attribute | Must have | Control output parameter names in ABI |
| 39 | `[contentproperty]` attribute | Must have | XAML content property |
| 40 | `[allowforweb]` attribute | Must have | WebView interop |
| 41 | `static runtimeclass` (no instances, static-only) | Must have | Static class pattern |
| 42 | `partial runtimeclass` (split across files) | Must have | XAML codegen scenario |
| 43 | `unsealed runtimeclass` (composable/derivable) | Must have | Inheritance support |
| 44 | Versioned member blocks: `[contract(C, N)] { members... }` inside runtimeclass | Must have | Per-member versioning via block syntax |
| 45 | `interface ... requires ...` syntax | Must have | Interface requirements |
| 46 | `>>` vs `> >` handling in nested generics | Must have | Parser must handle `IAsyncOperation<IVector<T> >` |
| 47 | Parameter passing: `out`, `ref const` (structs), `ref` (fill arrays) | Must have | Full parameter semantics |
| 48 | Conformant arrays: pass/fill/receive patterns (`T[]`, `ref T[]`, `out T[]`) | Must have | Array parameter conventions |
| 49 | User-defined `attribute` types (`[attributeusage]` + struct-like body) | Must have | Custom attribute definitions |
| 50 | Attribute scope blocks: `[attr] { decls... }` applies attribute to all enclosed | Must have | Bulk attribute application |
| 51 | Implicit property read-write: `Type Name;` without `{ get; set; }` implies read-write | Must have | Shorthand syntax |
| 52 | Property setter addition for versioning: separate `Type Name { set; }` after existing `{ get; }` | Must have | Binary-stable property evolution |
| 53 | Shorthand for `Windows.Foundation.Collections` parameterized types (e.g. `IVector<T>` without full namespace) | Must have | Namespace resolution for well-known types |
| 54 | MIDL 2.0 + MIDL 3.0 coexistence in same file (but not same entity) | Must have | Mixed-mode files |
| 55 | HRESULT-returning delegate ambiguity resolution (classic vs modern based on parameter syntax) | Must have | Delegate compatibility |
| 56 | `/reference` switch for WinMD imports | Must have | Cross-file type resolution |
| 57 | Reserved keywords list (complete — see [reserved-keywords](https://learn.microsoft.com/en-us/uwp/midl-3/reserved-keywords)) | Must have | ~200+ reserved words in and out of `[]` |
| 58 | Auto-numbering of synthesized interface/method names (suffix 2, 3, ...) when name collisions occur | Must have | ABI name collision avoidance |
| 59 | Parameterized type IID generation (PIID algorithm: SHA-1 hash per RFC 4122 v5, with `wrt_pinterface_namespace` GUID `11f47ad5-7b73-42c0-abae-878b1e16adee`) | Must have | See [WinRT type system](https://learn.microsoft.com/en-us/uwp/winrt-cref/winrt-type-system#guid-generation-for-parameterized-types) — type signature grammar for base types, interfaces, delegates, structs, enums, runtime classes, interface groups |
| 60 | Type signatures for PIID: base type codes (`i4`, `u4`, `f8`, `b1`, `c2`, `string`, `g16`, etc.), `rc(name;default_iface)`, `struct(name;fields)`, `enum(name;underlying)`, `delegate(guid)`, `pinterface(piid;args)` | Must have | Deterministic IID generation |
| 61 | WinMD file format compliance: ECMA-335 PE + CLI metadata, `tdWindowsRuntime` flag on types, version string `"Windows Runtime 1.2"`, TypeDef redirection pattern | Must have | See [WinMD files](https://learn.microsoft.com/en-us/uwp/winrt-cref/winmd-files) |
| 62 | WinMD encoding rules: enums extend `System.Enum`, structs extend `System.ValueType`, delegates extend `System.MulticastDelegate`, classes extend `System.Object` or composable base | Must have | ECMA-335 base class markers |
| 63 | WinMD method encoding: HRESULT return stripped, `[out,retval]` becomes return type in metadata | Must have | ABI→projection mapping |
| 64 | WinMD property encoding: `get_`/`put_` accessor methods, PropertyMap, MethodSemantics tables | Must have | Property metadata |
| 65 | WinMD event encoding: `add_`/`remove_` methods, `EventRegistrationToken` return/param, EventMap, MethodSemantics tables | Must have | Event metadata |
| 66 | WinMD array encoding: array length param omitted from MethodDefSig, direction inferred from array param direction + BYREF marker | Must have | Conformant array metadata |
| 67 | WinMD composition factory encoding: controlling `IInspectable*` and non-delegating `IInspectable**` omitted from metadata `.ctor` | Must have | Composition activation |
| 68 | WinMD activation encoding: `ActivatableAttribute` (direct + factory), `ComposableAttribute` (public/protected), `StaticAttribute` on TypeDef | Must have | Activation metadata |
| 69 | WinMD class methods: copy MethodDef from interface with MethodImpl table back-reference, remove Abstract flag, add Runtime ImplFlag, Final on non-overridable | Must have | Class method encoding |
| 70 | WinMD namespace/file naming rules: all types under namespace matching file name, case-insensitive file match, case-sensitive namespace match | Must have | File composition rules |
| 71 | `ExclusiveToAttribute` on private interfaces | Must have | Private interface binding |
| 72 | `OverridableAttribute` / `ProtectedAttribute` on InterfaceImpl rows | Must have | Composition visibility |
| 73 | `VersionAttribute` on types, interfaces, enum values, InterfaceImpl rows | Must have | Versioning metadata |
| 74 | `DefaultAttribute` on exactly one InterfaceImpl row for classes with member interfaces | Must have | Default interface designation |
| 75 | `FlagsAttribute` on UInt32 enums (required), absent on Int32 enums (required) | Must have | Enum kind enforcement |
| 76 | WinRT identifier grammar: Unicode 3.0 Lu/Ll/Lt/Lm/Lo/Nl start, Mn/Mc/Nd/Pc continuation, ZWNJ/ZWJ formatting chars | Must have | Identifier validation |
| 77 | Case-insensitive namespace/type uniqueness (no two types differing only by case) | Must have | WinRT naming rules |
| 78 | Struct field restrictions: only primitives, enums, other structs, `String`, and `IReference<T>` | Must have | Value type constraints |
| 79 | No nested types in WinRT | Must have | Type system constraint |
| 80 | No `in/out` (bidirectional) parameters — only `in` or `out` | Must have | Parameter direction rules |
| 81 | Method overloading by arity (not by type); `[default_overload]` required when same-arity overloads exist | Must have | Weak-typing language compat |
| 82 | `interface ... requires ...` → InterfaceImpl rows in metadata | Must have | Interface requirement encoding |
| 83 | Composable class hierarchy: `unsealed` → no `sealed` flag in metadata, `Extends` points to composable base TypeRef | Must have | Composition chain |
| 84 | Reserved parameter names: `value` (constructor retval), `baseInterface`/`innerInterface` (unsealed ctor, non-overridable), `operation` (IAsyncXxx return), `result` (all other returns); overridable via `[return_name]` | Must have | ABI parameter naming |
| 85 | Nullable value types via `Windows.Foundation.IReference<T>` (e.g. `IReference<Int32>` for nullable Int32) | Must have | Optional value pattern |
| 86 | `[threading(both)]` and `[marshaling_behavior(agile)]` are defaults — compiler should not require them; should warn if applied to non-activatable types | Must have | Default attribute inference |
| 87 | Contract attribute on interface references in runtimeclass base list: `runtimeclass C : BaseClass, [contract(UAC, 8)] IClosable` | Must have | Per-interface versioning on base list |
| 88 | `declare { }` blocks for forcing proxy stub generation of parameterized interface instantiations | Must have | Explicit pinterface declaration |
| 89 | Velocity/Feature API staging: `[feature(Feature_Name)]` attribute; build postprocessing transforms to `[experimental]` (DisabledByDefault), deletes API (AlwaysDisabled), or unrestricts (AlwaysEnabled) based on Velocity key | Should have | Windows-internal feature gating |
| 90 | Contract history validation: interfaces in a runtimeclass must use contracts in the runtimeclass's own contract history (error MIDL5132 if contract family mismatch) | Must have | Contract consistency check |
| 91 | MIDL2/MIDL3 mode detection: case of type names (`boolean` vs `Boolean`) determines which syntax mode the compiler uses for an entity; each interface/runtimeclass must be entirely one mode | Must have | Mixed-mode disambiguation |

### 5.2 Classic MIDL Features (best-effort parity)

| # | Feature | Priority | Notes |
|---|---------|----------|-------|
| 84 | `[object]` COM interface declarations | High | COM interop |
| 85 | `[uuid]` GUID on interfaces | High | Interface identity |
| 86 | Pointer attributes: `[ref]`, `[unique]`, `[ptr]` | High | Marshaling semantics |
| 87 | Directional attributes: `[in]`, `[out]`, `[in,out]`, `[retval]` | High | Parameter semantics |
| 88 | Size attributes: `[size_is]`, `[length_is]`, `[first_is]`, `[last_is]`, `[max_is]`, `[min_is]` | High | Array/buffer sizing |
| 89 | `[string]`, `[bstring]` | High | String types |
| 90 | `[range(min,max)]` | High | Value constraints |
| 91 | `[switch_is]`, `[switch_type]` for discriminated unions | High | Union support |
| 92 | `typedef` declarations | High | Type aliases |
| 93 | `struct`, `union`, `enum` (classic syntax) | High | C-style types |
| 94 | Encapsulated unions (NIDL switch syntax) | High | `switch(type) { case: ... }` |
| 48 | `coclass` declarations | High | COM class |
| 49 | `dispinterface` declarations | Medium | OLE Automation |
| 50 | `library` blocks | Medium | Type library scope |
| 51 | `module` declarations | Medium | DLL exports |
| 52 | `importlib()` for type libraries | Medium | TLB import |
| 53 | ACF (Application Configuration File) grammar | Medium | Separate attribute file |
| 54 | `[transmit_as]`, `[wire_marshal]`, `[represent_as]`, `[user_marshal]` | Medium | Custom marshaling |
| 55 | `[call_as]` | Medium | Function redirection |
| 56 | `[context_handle]`, `[implicit_handle]`, `[auto_handle]`, `[explicit_handle]` | Medium | RPC handles |
| 57 | `[callback]`, `[idempotent]`, `[broadcast]`, `[maybe]`, `[message]` | Medium | RPC semantics |
| 58 | `[async]` (classic RPC async) | Medium | Async RPC |
| 59 | `[encode]` / `[decode]` (pickling/serialization) | Medium | Data serialization |
| 60 | `[endpoint]` specifications | Medium | RPC endpoints |
| 61 | `[local]` attribute | High | No-marshaling interface |
| 62 | `cpp_quote()` | High | Pass-through C code |
| 63 | `#pragma` support (pack, warning, midl import) | Medium | Preprocessor integration |
| 64 | `[system_handle]` with access flags | Medium | NT handle types |
| 65 | OLE Automation attributes (`[dual]`, `[oleautomation]`, `[propget]`, `[propput]`, `[propputref]`, `[optional]`, `[vararg]`, `[hidden]`, `[restricted]`, etc.) | Medium | Automation compat |
| 66 | `[custom(GUID, value)]` attributes | Medium | Custom metadata |
| 67 | `[id(N)]` dispatch IDs | Medium | DISPID |
| 68 | `[helpstring]`, `[helpcontext]`, `[helpfile]` | Low | Documentation attributes |
| 69 | C proxy/stub generation (_c.c, _s.c) | Medium | RPC stubs |
| 70 | IID file generation (_i.c) | High | GUID definitions |
| 71 | DllData generation (dlldata.c) | Medium | Proxy registration |
| 72 | NDR format string generation (NDR20 + NDR64) | Low (complex) | Wire format |
| 73 | Type library (.tlb) generation | Low (complex) | OLE typelibs |
| 74 | Platform targeting (x86, x64, ARM, ARM64) | High | Multi-platform |
| 75 | `/robust` parameter validation | Medium | Runtime checks |
| 76 | Extension/Host interface stubs | Medium | Encapsulated marshaling |
| 77 | `[partial_ignore]` | Low | Partial marshaling |
| 78 | `[force_allocate]` | Low | Memory control |
| 79 | SAL annotation generation | Low | Static analysis |
| 80 | Pipe types | Low | Streaming |
| 81 | SAFEARRAY support | Low | Automation arrays |
| 82 | Embedded C code tolerance (mixed IDL/C) | High | Real-world files |

### 5.3 Command-Line Compatibility

| # | Feature | Priority | Notes |
|---|---------|----------|-------|
| 83 | All ~140 command-line switches | High | Feature parity |
| 84 | Response file (`@file`) support | High | Build system compat |
| 85 | C preprocessor invocation (`/cpp_cmd`) | High | `cl.exe /E` pipeline |
| 86 | `/env` platform targeting | High | x86/x64/ARM/ARM64 |
| 87 | `/target` version targeting (NT40→WIN11) | High | Feature gating |
| 88 | `/protocol` transfer syntax (DCE/NDR64/both) | Medium | Wire format |
| 89 | `/Oicf` optimization mode | Medium | Stub generation |
| 90 | `/error` runtime checking flags | Medium | Validation |
| 91 | `/robust` / `/no_robust` | Medium | Parameter validation |
| 92 | `/winrt` mode (currently spawns midlrt.exe) | High | WinRT compilation |
| 93 | Warning levels (`/W0`..`/W6`, `/WX`) | High | Diagnostics |
| 94 | `/syntax_check` / `/Zs` | High | Parse-only mode |

### 5.4 LSP / Editor Features (new)

| # | Feature | Priority | Notes |
|---|---------|----------|-------|
| 95 | Syntax highlighting (TextMate grammar + semantic tokens) | Must have | Core editor experience |
| 96 | Real-time diagnostics (errors/warnings as you type) | Must have | Error recovery enables this |
| 97 | Autocomplete: keywords, types, attributes, members | Must have | Context-aware |
| 98 | Hover: type info, attribute docs, method signatures | Must have | Quick documentation |
| 99 | Go-to-definition (interfaces, types, namespaces) | Must have | Navigation |
| 100 | Find all references | Should have | Refactoring support |
| 101 | Document symbols / outline | Must have | Structure view |
| 102 | Signature help (method parameters) | Should have | Parameter hints |
| 103 | Code actions (quick fixes for common errors) | Should have | Productivity |
| 104 | Formatting | Nice to have | Code style |
| 105 | Rename symbol | Nice to have | Refactoring |
| 106 | Workspace-wide analysis (multi-file) | Should have | Cross-file refs |
| 107 | WinMD file inspection (go-to-def into metadata) | Should have | Imported types |

---

## 6. Implementation Phases

### Phase 0: Project Bootstrap
- Set up .NET 10 solution at `E:\midl` with `global.json` pinning SDK version
- Create projects: `Midl.Syntax`, `Midl.Lexer`, `Midl.Parser`, `Midl.Analysis`, `Midl.CodeGen`, `Midl.Lsp`, `Midl.Cli`
- `Directory.Build.props` with shared settings (LangVersion=14, NativeAOT for CLI/LSP, nullable enabled)
- VS Code extension scaffold (TypeScript)
- CI/CD pipeline (dotnet build + test)
- Test infrastructure (Verify snapshot testing for parser, golden-file testing for codegen)

### Phase 1: Lexer + Error-Recovering Parser (MIDL3 focus)
- Hand-written lexer with `SlidingTextWindow` (Roslyn pattern) for efficient scanning
- All MIDL/WinRT tokens, keywords, literals; context-aware keyword disambiguation
- Error-recovering recursive-descent parser producing lossless green tree
- Red tree wrapper with parent pointers and typed syntax node accessors (C# records)
- **Target**: Parse any valid MIDL3 file; produce useful tree for invalid files
- **Test**: Parse all `MidlRT\unittest\*.idl` and `MidlRT\FunctionalTests\*.idl` files; snapshot tests for error recovery

### Phase 2: Semantic Analysis (MIDL3 focus)
- Symbol table with namespace-aware name resolution
- Type resolution (modern types → classic types mapping)
- Attribute validation (contract versioning, activation, composition)
- Parameterized interface instantiation
- Forward reference resolution
- Semantic diagnostics
- **Test**: Validate all MIDL3 test IDL files produce correct semantic model

### Phase 3: LSP Server + VS Code Extension
- LSP server using `Microsoft.VisualStudio.LanguageServer.Protocol` NuGet
- Published as NativeAOT single-file binary (fast startup, no runtime dependency)
- TextMate grammar for basic syntax highlighting
- Semantic token provider for rich highlighting
- Diagnostics (from parser errors + semantic errors)
- Document symbols / outline
- Hover information
- Go-to-definition (within file, then cross-file)
- Autocomplete (keywords, types, attributes, members)
- **Test**: Manual testing with real MIDL3 files in VS Code

### Phase 4: WinMD Output
- **Reading imported WinMD**: Use `System.Reflection.Metadata` (SRM) — the same API the .NET SDK uses; zero extra dependencies
- **Writing WinMD output**: Use `System.Reflection.Metadata.MetadataBuilder` — first-class, low-level, zero-allocation ECMA-335 writer
- **Reference implementation**: CsWinRT's `WinRT.SourceGenerator` (in `microsoft/CsWinRT`, `src/Authoring/WinRT.SourceGenerator`) is a Roslyn source generator that writes `.winmd` files from C# using `MetadataBuilder`. The companion `cswinmd` tool (`src/Authoring/cswinmd`) drives it from the CLI. This is the best existing C# example of writing WinMD files and should be studied closely for encoding patterns.
- Emit runtimeclass, interface, delegate, struct, enum metadata
- Custom attribute emission (all WinRT attributes: activation, composition, threading, contracts, deprecation, etc.)
- Type reference resolution across WinMD imports (using SRM reader)
- Parameterized interface metadata
- Contract versioning metadata
- **Validation**: Feed output to `windows-bindgen` to verify Rust bindings generate correctly (see [Rafael Rivera's article](https://withinrafael.com/2023/01/18/generating-metadata-for-the-windows-crate))
- **Test**: Binary-compare WinMD output against original midlrt.exe output

### Phase 5: C/C++ Header Generation
- C header output for WinRT types (MIDL_INTERFACE macros, vtables)
- Async header generation
- Interface UUID definitions
- Include guards, pragma once
- Modern type → classic type name mapping in output
- **Test**: Golden-file comparison against original midl output

### Phase 6: Classic MIDL Support
- Extend lexer/parser for full classic MIDL grammar
- ACF grammar support
- COM interface attributes ([object], pointer attrs, handles, etc.)
- Struct/union/enum classic syntax
- Coclass, dispinterface, library, module
- cpp_quote pass-through
- **Test**: Parse Windows SDK IDL files

### Phase 7: Classic Code Generation
- C proxy/stub generation (_c.c, _s.c)
- IID file generation (_i.c)
- DllData generation
- NDR format string generation (NDR20 + NDR64)
- Type library (.tlb) generation (via COM ICreateTypeLib2)
- Platform-specific code generation (x86, x64, ARM, ARM64)
- **Test**: Golden-file comparison against original midl output

### Phase 8: Command-Line Interface + Build System Integration
- Full command-line switch compatibility (~140 switches) using `System.CommandLine` or hand-rolled parser
- Response file support
- C preprocessor integration
- Warning level system
- `/winrt` mode (no longer needs separate midlrt.exe — unified compiler)
- Exit code compatibility
- NativeAOT publish for single-file `midl.exe` (no .NET runtime required on target)
- **Test**: Drop-in replacement for midl.exe in Windows build

### Phase 9: Advanced LSP Features
- Find all references (workspace-wide)
- Rename symbol
- Code actions / quick fixes
- Signature help
- Formatting
- WinMD file inspection (go-to-def into imported types via SRM reader)
- Incremental re-analysis on edit (scoped invalidation)
- **Test**: Stress testing with large IDL workspaces

---

## 7. Solution / Project Structure

```
E:\midl\
├── Midl.sln                          # Solution file
├── global.json                       # .NET 10 SDK version pin
├── Directory.Build.props             # Shared build properties (LangVersion, NativeAOT, etc.)
├── src/
│   ├── Midl.Syntax/                  # Syntax kinds, green/red tree infrastructure
│   │   ├── SyntaxKind.cs             # Enum of all node/token types
│   │   ├── GreenNode.cs              # Immutable green tree nodes
│   │   ├── SyntaxNode.cs             # Red tree wrapper (parent, position)
│   │   ├── SyntaxToken.cs            # Token with leading/trailing trivia
│   │   ├── SyntaxTrivia.cs           # Whitespace, comments, skipped tokens
│   │   └── Generated/                # Source-generated typed syntax nodes
│   ├── Midl.Lexer/                   # Tokenizer (error-tolerant)
│   │   ├── Lexer.cs                  # Lexer state machine
│   │   ├── TokenKinds.cs             # Token classification
│   │   ├── KeywordTable.cs           # Context-aware keyword lookup
│   │   └── SlidingTextWindow.cs      # Efficient text scanning (Roslyn pattern)
│   ├── Midl.Parser/                  # Error-recovering parser
│   │   ├── Parser.cs                 # Core parser (recursive descent)
│   │   ├── Parser.Declarations.cs    # Top-level declarations
│   │   ├── Parser.Interfaces.cs      # Interface/runtimeclass parsing
│   │   ├── Parser.Types.cs           # Type declarations
│   │   ├── Parser.Attributes.cs      # Attribute list parsing
│   │   ├── Parser.Expressions.cs     # Expression parsing
│   │   ├── Parser.Acf.cs             # ACF grammar
│   │   ├── Parser.Recovery.cs        # Error recovery strategies
│   │   └── SyntaxFactory.cs          # Green node construction
│   ├── Midl.Analysis/                # Semantic analysis
│   │   ├── Compilation.cs            # Top-level compilation unit
│   │   ├── SymbolTable.cs            # Namespace-aware symbol resolution
│   │   ├── Symbols/                  # Symbol types (InterfaceSymbol, TypeSymbol, etc.)
│   │   ├── Binding/                  # Name binding and type resolution
│   │   ├── TypeChecker.cs            # Type checking and validation
│   │   ├── AttributeValidator.cs     # Attribute validation rules
│   │   ├── ContractResolver.cs       # Contract versioning logic
│   │   ├── PInterfaceInstantiator.cs # Parameterized interface instantiation
│   │   └── Diagnostics/              # Diagnostic descriptors and emission
│   ├── Midl.CodeGen/                 # Code generation backends
│   │   ├── WinMd/                    # WinMD emitter (System.Reflection.Metadata)
│   │   │   ├── WinMdEmitter.cs
│   │   │   ├── TypeDefEmitter.cs
│   │   │   └── CustomAttributeEmitter.cs
│   │   ├── Header/                   # C/C++ header generation
│   │   ├── Stub/                     # C proxy/stub generation
│   │   ├── Iid/                      # IID file generation
│   │   ├── Ndr/                      # NDR format string generation
│   │   ├── Tlb/                      # Type library generation (COM interop)
│   │   └── DllData/                  # DllData generation
│   ├── Midl.Lsp/                     # Language Server
│   │   ├── MidlLanguageServer.cs     # LSP server entry point
│   │   ├── Handlers/                 # Per-request handlers
│   │   │   ├── CompletionHandler.cs
│   │   │   ├── DiagnosticsHandler.cs
│   │   │   ├── HoverHandler.cs
│   │   │   ├── GotoDefinitionHandler.cs
│   │   │   └── SemanticTokensHandler.cs
│   │   ├── DocumentManager.cs        # Open document tracking + incremental updates
│   │   └── WorkspaceAnalyzer.cs      # Cross-file analysis
│   └── Midl.Cli/                     # Command-line interface
│       ├── Program.cs                # Entry point
│       ├── CommandLineParser.cs      # ~140 switches
│       └── CompilationDriver.cs      # Pipeline orchestration
├── editors/
│   └── vscode/                       # VS Code extension
│       ├── package.json
│       ├── src/
│       │   └── extension.ts          # Spawn .NET LSP binary via stdio
│       ├── syntaxes/
│       │   └── midl.tmLanguage.json  # TextMate grammar
│       ├── language-configuration.json
│       └── snippets/
│           └── midl.json             # Code snippets
└── tests/
    ├── Midl.Lexer.Tests/
    ├── Midl.Parser.Tests/            # Snapshot tests for error recovery
    ├── Midl.Analysis.Tests/
    ├── Midl.CodeGen.Tests/           # Golden-file comparison tests
    ├── fixtures/                     # Test IDL files (from MidlRT tests)
    ├── golden/                       # Expected output files
    └── Midl.Integration.Tests/       # End-to-end tests
```

---

## 7b. Test Strategy

### Test Tiers

| Tier | Scope | What | When |
|------|-------|------|------|
| **T0: Unit** | Per-project | Lexer token tests, parser snapshot tests, symbol resolution tests, individual codegen unit tests | Every PR, <2 min |
| **T1: Internal corpus** | MidlRT test files | Parse + analyze all `MidlRT\unittest\*.idl` and `MidlRT\FunctionalTests\*.idl` files (~200+ IDL files) from the existing MIDL compiler test suite | Every PR, <5 min |
| **T2: Windows product IDLs** | OS repo IDL corpus | A curated collection of real Windows product IDL files spanning WinRT, classic MIDL, mixed-mode, and edge cases. Provided separately (multi-GB OS repo). Tests: parse without crash, produce same diagnostics, produce identical WinMD/headers | Nightly / weekly |
| **T3: Golden-file codegen** | Output comparison | For a subset of T1+T2 IDLs, compare our output (WinMD, headers, stubs, IID files) byte-for-byte against original `midl.exe` / `midlrt.exe` output | Nightly |
| **T4: Drop-in replacement** | Build system | Replace `midl.exe` in a Windows razzle build of a selected component and verify the build succeeds with identical output | Milestone gate |

### Test Corpus Sources

1. **MidlRT unit tests** (external fixture directory, ~200+ IDL files) covering:
   - Activatable, composable, delegate, event, overload, contract, async, deprecation, struct, enum, pinterface, static, constructor, namespace, custom attribute, and many edge cases
   - Positive and negative test cases (expected errors)
   - Mixed MIDL2 + MIDL3 coexistence files

2. **MidlRT functional tests** (external fixture directory) — metadata validation tests

3. **Curated real-world IDL corpus** (external fixture directory) — real-world IDL files, including:
   - Public WinRT namespaces (Windows.Foundation, Windows.UI.Xaml, Windows.ApplicationModel, WinUI, etc.)
   - APIs using `[feature]`, `[experimental]` attributes
   - APIs using every contract, versioning, and activation pattern
   - Mixed classic MIDL + MIDL3 files
   - Files with `cpp_quote()`, `import`, `importlib`, embedded C code
   - *Note: These will be collected and provided as a test fixture directory*

4. **Windows SDK published IDLs** (`%WindowsSdkDir%\Include\<version>\winrt\*.idl`) — the public WinRT API surface

5. **Error recovery corpus** (hand-crafted) — intentionally malformed IDL files testing:
   - Missing closing braces, brackets, semicolons
   - Incomplete declarations (user is mid-typing)
   - Partial attribute lists
   - Unterminated strings and comments
   - Files with cursor positions simulating active editing

### Per-Phase Test Plan

| Phase | Test approach |
|-------|--------------|
| **Phase 1: Lexer + Parser** | T0: snapshot tests for every syntax construct; T1: parse all MidlRT test IDLs without crash; error recovery corpus |
| **Phase 2: Semantic** | T0: symbol resolution, type checking, attribute validation unit tests; T1: semantic analysis of MidlRT IDLs matches expected diagnostics |
| **Phase 3: LSP** | Manual VS Code testing; automated LSP protocol tests (completion, hover, go-to-def) against fixture files |
| **Phase 4: WinMD** | T3: binary-compare WinMD output against `midlrt.exe` for T1 corpus; feed output to `windows-bindgen` to verify Rust bindings generate |
| **Phase 5: Headers** | T3: diff headers against original `midl.exe` output for T1 corpus |
| **Phase 6: Classic MIDL** | T2: parse Windows product IDLs (classic + mixed); T1: parse base MIDL test files |
| **Phase 7: Classic codegen** | T3: golden-file comparison for stubs, IIDs, dlldata against original output |
| **Phase 8: CLI** | T4: drop-in replacement in razzle build; T2: process Windows product IDLs end-to-end |
| **Phase 9: Advanced LSP** | Stress testing with large multi-file IDL workspaces from T2 corpus |

---

## 8. Key Design Decisions

### 8.1 Lossless Red-Green Tree (Roslyn pattern)
Every byte of source is preserved in the immutable green tree. This enables:
- Exact source reconstruction from tree
- Error nodes don't lose source text
- Comments and whitespace preserved (important for formatting)
- Structural sharing across incremental edits (green nodes are immutable and reusable)
- Red tree provides lazy parent pointers without storing them in the shared green nodes
- Uses `System.Collections.Immutable` for persistent data structures

### 8.2 Incremental Computation
LSP must respond in <100ms to keystrokes. We achieve this via:
- Memoized compilation stages (text → tokens → green tree → semantic model)
- Re-lex only the changed text span; re-parse only affected green tree nodes
- Semantic model invalidation scoped to changed declarations
- `ConcurrentDictionary` for thread-safe caching across LSP requests

### 8.3 C Preprocessor Strategy
- Phase 1-3: Require pre-preprocessed input (user runs `cl /E` separately)
- Phase 8: Integrate preprocessing (invoke `cl.exe` or implement subset)
- LSP mode: Use lightweight preprocessor that handles `#define`/`#include`/`#ifdef` for editor responsiveness

### 8.4 Mixed IDL/C Code Strategy
- Lexer: Tokenize C keywords and constructs alongside IDL
- Parser: C declarations parsed as "opaque C blocks" in early phases
- `cpp_quote()` content: Preserved as raw strings, not parsed
- Gradually increase C grammar coverage as needed

---

## 9. Risk Assessment

| Risk | Mitigation |
|------|------------|
| WinMD ECMA-335 complexity | `System.Reflection.Metadata` handles this natively — it's the same API the .NET SDK uses. No complexity gap. |
| NDR format string fidelity | Binary-compare output against original midl.exe; extensive golden-file tests |
| Mixed C/IDL parsing | Treat C regions as opaque initially; increase fidelity over time |
| Parameterized interface correctness | Port all ~15 predefined pinterface definitions as test fixtures |
| Error recovery quality | Invest in extensive test corpus of partially-typed files; Roslyn's patterns are proven at massive scale |
| Build system integration | NativeAOT single-file publish for CLI; exact CLI compatibility; drop-in replacement testing |
