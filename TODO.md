# TODO — MIDL Compiler Rewrite Status

> **Last updated**: 2026-03-20T06:15Z
> This file tracks implementation status. Keep it current with every feature change.

## ✅ Implemented

### Core Pipeline
- [x] Hand-written error-tolerant lexer (69 keywords, all operators, trivia)
- [x] Error-recovering recursive-descent parser (lossless green tree, Roslyn pattern)
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
- [x] `enum` with `[flags]`
- [x] `apicontract`
- [x] `event` (modern syntax)
- [x] Properties (`{ get; set; }` and shorthand)
- [x] Constructors (default + parameterized)
- [x] `static` methods and properties
- [x] `protected` and `overridable` members
- [x] Versioned blocks `[contract(C, N)] { ... }`
- [x] Generic/parameterized types (`IVector<T>`, `IMap<K,V>`)
- [x] `>>` disambiguation in nested generics (`IAsyncOperation<IVector<String>>` works)
- [x] Modern base types (Int32, String, Boolean, Guid, Object, Char, Single, Double, UInt8...)
- [x] `declare { }` blocks
- [x] `import "file.idl"` (multi-file IDL compilation with transitive imports)
- [x] `partial runtimeclass` merging across declarations
- [x] User-defined `attribute` declarations with `[attributeusage]`, `[attributename]`, `[allowmultiple]`
- [x] Parameter passing: `out`, `ref const` (structs), `ref` (fill arrays), `T[]` arrays

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
- [x] `AttributeExtractor` — typed extraction of all attribute arguments
- [x] `AttributeValidator` — semantic checks (MIDL1020–1030)
- [x] `VersionPropagator` — contract/version flows + validation (MIDL1014–1017)
- [x] WinMD emission — 13 custom attributes (ExclusiveToAttribute, ActivatableAttribute, etc.)
- [x] Synthesized interfaces get `tdNotPublic` flag + `ExclusiveToAttribute`
- [x] WinMD PropertyMap/EventMap/MethodSemantics tables
- [x] WinMD MethodImpl table (class→interface back-references)
- [x] WinMD InterfaceImpl for `requires` chains
- [x] WinMD composition factory encoding (omit IInspectable* outer/inner params)
- [x] WinMD array param encoding (omit length param from MethodDefSig)
- [x] Reserved parameter name auto-generation (result/operation/value)
- [x] PIID algorithm (SHA-1/UUID v5 deterministic IIDs for parameterized types)
- [x] Custom attribute reading from imported `.winmd` (uuid, version, deprecated, exclusiveto, threading, marshaling)
- [x] Code action fixers for type name corrections (int→Int32, boolean→Boolean, etc.)

### Diagnostics (47 codes)
- [x] MIDL1001–MIDL1013: Semantic errors (undefined type, duplicate, namespace required, empty struct, case collision, write-only property, reserved param name, etc.)
- [x] MIDL1014–MIDL1017: Version/contract errors
- [x] MIDL1020–MIDL1030: Attribute validation (threading, marshaling, composable, exclusiveto, deprecated, attributeusage, custom attribute target/arg count)
- [x] MIDL1031–MIDL1032: Partial runtimeclass conflicts
- [x] MIDL1033–MIDL1038: Access/inheritance validation (protected/overridable on sealed, undefined requires, base not runtimeclass, sealed base, runtimeclass on requires list)
- [x] MIDL1100–MIDL1102: Syntax errors (skipped tokens, error chars, missing tokens with expected info)
- [x] MIDL2001–MIDL2005: Strict mode warnings (classic constructs, lowercase types, HRESULT, pointers)
- [x] All diagnostic positions tracked via `TokenPositionMap` — `position` is **required** (no defaults)
- [x] Diagnostics include `HelpUri` linking to MIDL3 docs
- [x] Diagnostics include `Data` dict for code action fixers

### LSP Features
- [x] Real-time diagnostics (syntax + semantic, including all 47 codes)
- [x] Keyword + type completions (including external WinMD types)
- [x] Hover (keyword docs, symbol info, external type markers)
- [x] Go-to-definition (within file)
- [x] Find all references
- [x] Signature help
- [x] Semantic tokens
- [x] Document formatting
- [x] Document symbols / outline
- [x] Code actions / quick fixes (type name corrections with `Data["replacement"]`)
- [x] `midl.strictMode` setting
- [x] Command palette: Toggle Strict Mode, Restart Server, Show Server Log

### Testing (449 tests)
- [x] Lexer: 50 tests (tokens, trivia, edge cases, preprocessor)
- [x] Parser: 62 tests (structures, error recovery, completeness, classic MIDL, generic disambiguation)
- [x] Analysis: 138 tests (binding, synthesis, WinMD round-trip, extraction, imports, access validation, version propagation, attribute validation, built-in types)
- [x] CodeGen: 55 tests (WinMD emitter, header, IID, DllData, tables, decompiler, encoding fixes)
- [x] Integration: 146 tests (CLI, diagnostics, formatting, semantic tokens, fixtures, code actions, 50 keyword coverage, OS fixtures)
- [x] IDL fixture suite (18 files with `.expected.json` + `.expected.tree` sidecars)
- [x] OS fixture test script (`scripts/test-os-fixtures.ps1` — 1186 files, 0 crashes)
- [x] End-to-end quick fix tests (apply fix → verify output → recompile → no warnings)
- [x] Diagnostic position tests (squigglies on correct token, not in comments)
- [x] WinMD round-trip test (IDL → WinMD → decompile → recompile)

### Infrastructure
- [x] GitHub Actions CI (build + test + NativeAOT + VS Code extension)
- [x] NativeAOT / single-file publish support
- [x] MidlTrace debug logging (`/debug` or `MIDL_DEBUG=1`)
- [x] `/strict` (default) / `/no_strict` / `/compat` mode switches
- [x] Auto-derived output filenames (input.idl → input.h, input_i.c, input.winmd)

### Documentation
- [x] README.md (with Mermaid pipeline diagram)
- [x] CONTRIBUTING.md (with dependency graph diagram)
- [x] TODO.md (this file)
- [x] docs/architecture.md (7 Mermaid diagrams)
- [x] docs/testing.md (VS Code extension build/test instructions)
- [x] docs/diagnostics.md (all diagnostic codes with examples and fixes)
- [x] editors/vscode/README.md (marketplace-style feature list)
- [x] editors/vscode/CHANGELOG.md

---

## 🔄 In Progress

- [ ] **Windows SDK auto-discovery** — read registry for SDK root, auto-add include paths + Windows.winmd reference
- [ ] **Performance benchmarking** — compare NativeAOT WinMIDL vs original midl.exe

---

## ❌ Not Yet Implemented

### WinRT Type System Validation
- [ ] **WinRT type compatibility** (MIDL1048) — method return/param types must be WinRT-valid (no raw pointers, no C structs outside namespace)
- [ ] **Struct field restrictions** (MIDL1005 enforcement) — only primitives, enums, structs, String, IReference<T>
- [ ] **No nested types** (MIDL1049) — types cannot be declared inside other types
- [ ] **No bidirectional parameters** (MIDL1050) — WinRT only allows `in` or `out`, not `[in,out]`
- [ ] **No arrays of arrays** (MIDL1051) — nested arrays not valid in WinRT
- [ ] **Property type consistency** (MIDL1052) — getter and setter must agree on type
- [ ] **Delegate return type** (MIDL1053) — must be void (or HRESULT in classic)
- [ ] **Interface member name uniqueness** (MIDL1054) — per interface
- [ ] **Arity-only overloading** (MIDL1055) — WinRT doesn't support overloading by type, only by param count
- [ ] **Deprecated base class** (MIDL1056) — derived must have redundant deprecation

### Important
- [ ] **`importlib` dispatch in parser** — recognized but not dispatched in classic mode
- [ ] **Read parameterized interfaces from `.winmd`** — generic type arguments not decoded from imports
- [ ] **NDR format string generation** (NDR20 + NDR64) — stub skeleton only
- [ ] **C proxy/stub code generation** (_c.c, _s.c) — skeleton only
- [ ] **Type library (.tlb) generation** — requires COM `ICreateTypeLib2`
- [ ] **ACF grammar** — placeholder only
- [ ] **~84 more CLI switches** — accept-and-ignore for drop-in compatibility
- [ ] **Velocity/Feature API staging** — `[feature(Name)]` → `[experimental]` transform
- [ ] **Incremental parsing for LSP** — currently full reparse on every edit
- [ ] **Cross-file analysis in LSP** — currently single-file only
- [ ] **WinMD file naming rules** — namespace matching, case-insensitive
- [ ] **`[contentproperty]`** — parsed but not semantically processed
- [ ] **`[allowforweb]`** — parsed but not semantically processed
- [ ] **`[fastabi]`** — parsed but not semantically processed

### Nice to Have
- [ ] **Rename symbol** in LSP
- [ ] **WinMD inspection** — go-to-def into imported `.winmd` types
- [ ] **Formatting** — more sophisticated (preserve user formatting, align attributes)
- [ ] **Code lens** — show interface synthesis results inline
- [ ] **Extension icon** — custom icon for the VS Code extension
- [ ] **Golden-file comparison** against original midl.exe output (T3 testing)
- [ ] **Drop-in replacement** test in razzle build (T4 testing)

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
