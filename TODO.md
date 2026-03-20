# TODO — MIDL Compiler Rewrite Status

> **Last updated**: 2026-03-20T11:30Z
> This file tracks implementation status. Keep it current with every feature change.

## ✅ Implemented

### Core Pipeline
- [x] Hand-written error-tolerant lexer (69 keywords, all operators, trivia, float literals, negative numbers)
- [x] Error-recovering recursive-descent parser (lossless green tree, Roslyn pattern, forward declarations)
- [x] Semantic analysis: symbol table, binder, type resolver
- [x] WinMD output via `System.Reflection.Metadata`
- [x] WinMD reference reading (`/reference path.winmd`)
- [x] C/C++ header generation
- [x] IID file generation (_i.c)
- [x] DllData skeleton generation
- [x] Proxy/stub skeleton generation
- [x] CLI with 30+ switches, response files, `/debug` tracing
- [x] `--analyze` mode (Rust-style rich console output with squigglies)
- [x] `--decompile` mode (WinMD to MIDL3 IDL — idiomatic properties, events, skips exclusive interfaces)
- [x] LSP server (JSON-RPC over stdio, no external NuGet deps)
- [x] VS Code extension (TextMate grammar, 19 snippets, LSP client, F5 debug)
- [x] Built-in C preprocessor (`#define`, `#ifdef`, `#include`, `#pragma`, macro expansion)
- [x] Built-in WinRT types (IInspectable, HSTRING, EventRegistrationToken, Windows.Foundation.*, Collections.*)
- [x] Win32 type aliases (DWORD→UInt32, BYTE→UInt8, GUID→Guid, HSTRING→String, etc.)
- [x] GitHub Actions CI pipeline (build, test, NativeAOT, VS Code extension)
- [x] NativeAOT / single-file publish support
- [x] OS test fixture reference via `MIDL_TEST_FIXTURES` env var (1186 files, 0 crashes)
- [x] `scripts/test-os-fixtures.ps1` test runner

### MIDL3 Syntax
- [x] `namespace` (nested, dotted)
- [x] `runtimeclass` (sealed, unsealed, static, partial)
- [x] `interface` with `requires`
- [x] `delegate`
- [x] `struct`
- [x] `enum` with `[flags]` (auto-sets UInt32 underlying type per WinRT spec)
- [x] `apicontract`
- [x] `event` (modern syntax)
- [x] Properties (`{ get; set; }` and shorthand)
- [x] Constructors (default + parameterized)
- [x] `static` methods and properties
- [x] `protected` and `overridable` members
- [x] Versioned blocks `[contract(C, N)] { ... }`
- [x] Generic/parameterized types (`IVector<T>`, `IMap<K,V>`)
- [x] `>>` disambiguation in nested generics (`IAsyncOperation<IVector<String>>` works)
- [x] All WinRT fundamental types: Boolean, Char16, Int16, Int32, Int64, UInt8, UInt16, UInt32, UInt64, Single, Double, String, Guid, Object
- [x] `declare { }` blocks
- [x] `import "file.idl"` (multi-file IDL compilation with transitive imports)
- [x] `partial runtimeclass` merging across declarations
- [x] User-defined `attribute` declarations with `[attributeusage]`, `[attributename]`, `[allowmultiple]`
- [x] Parameter passing: `out`, `ref const` (structs), `ref` (fill arrays), `T[]` arrays
- [x] Forward declarations (`interface IFoo;`, `struct S;`, `runtimeclass C;`, `attribute A;`, `enum E;`)
- [x] Float literals (`1.5`, `3.14f`) and negative numeric literals (`-42`, `-3.14`)

### Classic MIDL
- [x] `coclass` declarations
- [x] `library` blocks
- [x] `typedef`
- [x] `union` (both variants)
- [x] `dispinterface`
- [x] `cpp_quote()`
- [x] Classic interface syntax (HRESULT + `[in]`/`[out]`/`[retval]`)
- [x] Pointer types (`int*`, `IFoo**`)
- [x] Wide strings (`L"..."`)
- [x] `->` and `::` operators

### Interface Synthesis
- [x] Instance members → `I<Class>` exclusive interface
- [x] Non-default constructors → `I<Class>Factory`
- [x] Default constructor → `HasDefaultActivation`
- [x] Static members → `I<Class>Statics`
- [x] Protected members → `I<Class>Protected`
- [x] Overridable members → `I<Class>Overrides`
- [x] Default interface selection (first instance interface)
- [x] `[default_interface]` on empty classes → synthesizes empty interface
- [x] Auto-numbering on name collision (suffix 2, 3...)
- [x] `IsExclusive` / `ExclusiveToClass` on synthesized interfaces

### Attribute Pipeline
- [x] `AttributeExtractor` — typed extraction of all attribute arguments (String, Integer, Float, Guid, Identifier, TypeName)
- [x] `AttributeValidator` — semantic checks (MIDL1020–1030, MIDL1064–1070)
- [x] Attribute argument type validation (MIDL1064: string where int expected, etc.)
- [x] Attribute argument range validation (MIDL1066: value 256 out of range for UInt8)
- [x] Attribute argument count validation (MIDL1030)
- [x] Unknown attribute detection (MIDL1065)
- [x] `[attributeusage]` enforcement on types AND members (MIDL1029)
- [x] `[exclusiveto]` violation detection (MIDL1069)
- [x] Missing `[default]` interface warning (MIDL1070)
- [x] `VersionPropagator` — contract/version flows + validation (MIDL1014–1017)
- [x] WinMD emission — 13 custom attributes (ExclusiveToAttribute, ActivatableAttribute, etc.)
- [x] Synthesized interfaces get `tdNotPublic` flag + `ExclusiveToAttribute`
- [x] WinMD PropertyMap/EventMap/MethodSemantics tables
- [x] WinMD MethodImpl table (class→interface back-references)
- [x] WinMD InterfaceImpl for `requires` chains
- [x] PIID algorithm (SHA-1/UUID v5 deterministic IIDs for parameterized types)
- [x] Custom attribute reading from imported `.winmd` (uuid, version, deprecated, exclusiveto, threading, marshaling)
- [x] Code action fixers for type name corrections (int→Int32, boolean→Boolean, etc.)

### WinRT Type System Validation
- [x] Struct fields: value types only (MIDL1005)
- [x] Struct circular references (MIDL1045)
- [x] `[flags]` enum → UInt32 underlying type (auto-set)
- [x] Enum values must be integer literals (MIDL1067: rejects float/string)
- [x] Enum values range-checked (MIDL1068: Int32 or UInt32 range)
- [x] Delegate return type must be void (MIDL1053)
- [x] Interface member name uniqueness (MIDL1054)
- [x] Overload arity + `[default_overload]` (MIDL1055, MIDL1059)
- [x] Constructor arity per version (MIDL1060)
- [x] `[composable]` + `[activatable]` forbidden (MIDL1025)
- [x] Sealed class as base (MIDL1036)
- [x] Protected/overridable on sealed class (MIDL1033)
- [x] Void parameter type (MIDL1043)
- [x] Deprecated base class inheritance (MIDL1056)
- [x] Static + unsealed conflict (MIDL1061)
- [x] Event type must be delegate (MIDL1042)
- [x] Array of arrays forbidden (MIDL1051 — caught at parser level)
- [x] Pointers in strict mode (MIDL2004)
- [x] HRESULT return type in strict mode (MIDL2005)
- [x] Classic types in strict mode: BSTR, LPSTR, VARIANT (MIDL2003)
- [x] Lowercase types in strict mode: int→Int32, float→Single, etc. (MIDL2002)
- [x] Classic constructs in strict mode: coclass, library, typedef, etc. (MIDL2001)
- [x] Classic `[in]`/`[out]`/`[retval]` attributes in strict mode (MIDL2003)

### Diagnostics (91 codes)
- [x] MIDL1001–MIDL1013: Semantic errors (undefined type, duplicate, namespace required, empty struct, case collision, write-only property, reserved param name, etc.)
- [x] MIDL1014–MIDL1017: Version/contract errors
- [x] MIDL1020–MIDL1030: Attribute validation (threading, marshaling, composable, exclusiveto, deprecated, attributeusage, custom attribute target/arg count)
- [x] MIDL1031–MIDL1032: Partial runtimeclass conflicts
- [x] MIDL1033–MIDL1038: Access/inheritance validation
- [x] MIDL1039–MIDL1061: WinRT validation rules (15 rules)
- [x] MIDL1064–MIDL1073: Attribute arg types/ranges, unknown attrs, exclusiveto, default interface, enum values, per-namespace WinMD, protected accessor errors
- [x] MIDL1100–MIDL1102: Syntax errors (skipped tokens, error chars, missing tokens)
- [x] MIDL2001–MIDL2005: Strict mode **errors** (classic constructs, lowercase types, HRESULT, pointers, classic attrs)
- [x] MIDL3001–MIDL3016: Code quality hints (interface naming, empty types, constructors, setter-only, method name clash, namespace casing, attribute naming, delegate naming, async naming, PascalCase, property shorthand)
- [x] All diagnostic positions tracked via green tree FullWidth accumulation — accurate to the usage site
- [x] Code action fixers for all fixable diagnostics (type corrections, naming, property shorthand)

### LSP Features
- [x] Real-time diagnostics (syntax + semantic, all 91 codes)
- [x] **Context-aware autocomplete**:
  - Top-level: declaration keywords
  - Runtimeclass body: types + void + event + modifiers + constructor name
  - Interface body: types + void + event + requires
  - Struct body: value types only (no Object, IVector)
  - Attribute body: value types + enums
  - Enum body: empty (member names only)
  - After `requires`: interfaces only
  - After `:` in base list: interfaces + classes only
  - Inside `[...]`: applicable attributes filtered by `[attributeusage]`
  - Inside `import "..."`: `.idl` files from project/SDK (excludes current file)
- [x] Hover (keyword docs, symbol info, external type markers)
- [x] Go-to-definition (within file + cross-file)
- [x] Find all references
- [x] Signature help
- [x] Semantic tokens (including float literals)
- [x] Document formatting (blank line normalization, brace splitting, tight generics, property accessor inline, comment preservation)
- [x] Document symbols / outline (filters external/synthesized)
- [x] Code actions / quick fixes (type name corrections, naming conventions, property shorthand, constructor insertion)
- [x] `midl.strictMode` setting
- [x] `midl.codeQualityHints` setting (toggle code quality hints)
- [x] Command palette: Toggle Strict Mode, Restart Server, Show Server Log
- [x] Cross-file context via `winmidl.json` project file
- [x] Cross-file hints via `// @ref: file.idl` directives
- [x] Debounce (300ms) — avoids recompilation on every keystroke
- [x] Content-hash caching — skips recompilation if text unchanged

### Testing (651 tests)
- [x] Lexer: 57 tests (tokens, trivia, edge cases, preprocessor, float/negative literals)
- [x] Parser: 62 tests (structures, error recovery, completeness, classic MIDL, generic disambiguation, forward declarations)
- [x] Analysis: 258 tests (binding, synthesis, WinMD round-trip, extraction, imports, access validation, version propagation, attribute validation, built-in types, enum value validation, hex literals, naming conventions, custom attribute types/ranges, protected accessor validation)
- [x] CodeGen: 77 tests (WinMD emitter, header, IID, DllData, tables, decompiler, encoding fixes, per-namespace output)
- [x] Integration: 197 tests (CLI, diagnostics, formatting, semantic tokens, fixtures, code actions, keyword coverage, OS fixtures, contextual completions, import autocomplete)
- [x] IDL fixture suite (18 files with `.expected.json` + `.expected.tree` sidecars)
- [x] OS fixture test script (`scripts/test-os-fixtures.ps1` — 1186 files, 0 crashes)

### Infrastructure & Documentation
- [x] GitHub Actions CI (build + test + NativeAOT + VS Code extension)
- [x] NativeAOT / single-file publish
- [x] `winmidl.json` project file format ([docs/winmidl-json.md](docs/winmidl-json.md))
- [x] F5 dev loop: builds .NET server + TS extension in parallel
- [x] README.md, CONTRIBUTING.md, docs/architecture.md, docs/testing.md, docs/diagnostics.md, docs/winmidl-json.md

---

## ❌ Not Yet Implemented

### Important
- [ ] **`importlib` dispatch in parser** — recognized but not dispatched in classic mode
- [ ] **NDR format string generation** (NDR20 + NDR64) — stub skeleton only
- [ ] **C proxy/stub code generation** (_c.c, _s.c) — skeleton only
- [ ] **Type library (.tlb) generation** — requires COM `ICreateTypeLib2`
- [ ] **ACF grammar** — placeholder only
- [ ] **~84 more CLI switches** — accept-and-ignore for drop-in compatibility
- [ ] **Velocity/Feature API staging** — `[feature(Name)]` → `[experimental]` transform
- [ ] **`[contentproperty]`** — parsed but not semantically processed
- [ ] **`[allowforweb]`** — parsed but not semantically processed
- [ ] **`[fastabi]`** — parsed but not semantically processed

### Nice to Have
- [ ] **WinMD inspection** — go-to-def into imported `.winmd` types
- [ ] **Code lens** — show interface synthesis results inline
- [ ] **Extension icon** — custom icon for the VS Code extension
- [ ] **Golden-file comparison** against original midl.exe output (T3 testing)
- [ ] **Drop-in replacement** test in razzle build (T4 testing)
- [ ] **Rename symbol** refinement (cross-file rename)
- [ ] **Incremental parsing** — deferred; full reparse is <10ms, debounce handles perf

---

## Conventions for Future Work

When adding a feature:
1. **Parser**: Add syntax support + error recovery tests
2. **Binder**: Create symbols, extract attributes, emit diagnostics
3. **WinMD**: Emit correct metadata tables
4. **LSP**: Verify diagnostics flow, add completions/hover if relevant
5. **Tests**: Fixture IDL + expected diagnostics, unit tests at every layer
6. **VS Code**: Test in Extension Development Host
7. **TODO.md**: Update this file
8. **docs/diagnostics.md**: Add any new error codes
