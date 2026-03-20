# TODO — MIDL Compiler Rewrite Status

> **Last updated**: 2026-03-20
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
- [x] `--decompile` mode (WinMD to IDL decompilation)
- [x] LSP server (JSON-RPC over stdio, no external NuGet deps)
- [x] VS Code extension (TextMate grammar, 19 snippets, LSP client, F5 debug)

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
- [x] Constructors
- [x] `static` methods and properties
- [x] `protected` and `overridable` members
- [x] Versioned blocks `[contract(C, N)] { ... }`
- [x] Generic/parameterized types (`IVector<T>`, `IMap<K,V>`)
- [x] Modern base types (Int32, String, Boolean, Guid, Object, Char, Single, Double, UInt8...)
- [x] `declare { }` blocks
- [x] `import` directive (parsed, not yet functional for IDL-to-IDL)
- [x] User-defined `attribute` declarations (`ParseAttributeDeclaration`, `AttributeTypeSymbol`, validation, WinMD round-trip)

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
- [x] Auto-numbering on name collision (suffix 2, 3...)
- [x] `IsExclusive` / `ExclusiveToClass` on synthesized interfaces

### Attribute Extraction
- [x] `AttributeExtractor` — parses attribute arguments into typed `AttributeData`
- [x] `[uuid]` extraction → InterfaceSymbol.Uuid
- [x] `[contract(name, version)]` → ContractName, ContractVersion
- [x] `[version(N)]` → Version
- [x] `[threading(model)]` → ThreadingModel
- [x] `[marshaling_behavior(behavior)]` → MarshalingBehavior
- [x] `[deprecated("msg",...)]` → DeprecatedMessage
- [x] `[method_name("name")]` → MethodNameOverride
- [x] `[overload("name")]` → OverloadGroup
- [x] `[default_overload]` → IsDefaultOverload
- [x] `[return_name("name")]` → ReturnNameOverride
- [x] `[remote_async]` → IsRemoteAsync
- [x] `[default_interface]` → IsDefaultInterface
- [x] `[contractversion(N)]` → ApiContractSymbol.ContractVersion

### Attribute Pipeline (complete)
- [x] `AttributeExtractor` — parses all attribute arguments into typed `AttributeData`
- [x] `AttributeValidator` — semantic checks (MIDL1020–1028): threading, marshaling, composable access, deprecation type, exclusiveto rules, `[default_interface]` empty class synthesis
- [x] `VersionPropagator` — contract/version flows: class→interface→member, validation (MIDL1014–1017)
- [x] WinMD attribute emission — 13 custom attributes: ExclusiveToAttribute, ActivatableAttribute, StaticAttribute, ComposableAttribute, ThreadingAttribute, MarshalingBehaviorAttribute, DeprecatedAttribute, ContractVersionAttribute, OverloadAttribute, DefaultOverloadAttribute, OverridableAttribute, ProtectedAttribute, DefaultAttribute
- [x] Synthesized interfaces get `tdNotPublic` flag + `ExclusiveToAttribute` in WinMD
- [x] WinMD PropertyMap/EventMap/MethodSemantics tables for properties and events
- [x] WinMD MethodImpl table — class method stubs back-reference synthesized interface methods
- [x] WinMD InterfaceImpl for `requires` clauses on interfaces
- [x] WinMD-to-IDL decompiler (`WinMdDecompiler`)

### Diagnostics (32 codes)
- [x] MIDL1001–MIDL1013: Semantic errors (undefined type, duplicate, namespace required, etc.)
- [x] MIDL1014–MIDL1017: Version/contract errors (interface version, contract family, undefined contract, enum member version)
- [x] MIDL1020–MIDL1030: Attribute validation (threading, marshaling, composable, exclusiveto, deprecated, attributeusage, custom attribute target/arg count)
- [x] MIDL1100–MIDL1102: Syntax errors (skipped tokens, error chars, missing tokens)
- [x] MIDL2001–MIDL2005: Strict mode warnings (classic constructs, lowercase types)
- [x] Diagnostic positions tracked via `TokenPositionMap` (no more position=0)
- [x] `position` parameter is **required** on all diagnostic factory methods

### LSP Features
- [x] Real-time diagnostics (syntax + semantic)
- [x] Keyword + type completions (including external WinMD types)
- [x] Hover (keyword docs, symbol info, external type markers)
- [x] Go-to-definition (within file)
- [x] Find all references
- [x] Signature help
- [x] Semantic tokens
- [x] Document formatting
- [x] Document symbols / outline
- [x] Code actions / quick fixes (int→Int32, boolean→Boolean, etc.)
- [x] `midl.strictMode` setting
- [x] Command palette: Toggle Strict Mode, Restart Server, Show Server Log

### Testing (330+ tests)
- [x] Lexer: 29 tests (tokens, trivia, edge cases)
- [x] Parser: 58 tests (structures, error recovery, completeness, classic MIDL)
- [x] Analysis: 112 tests (binding, synthesis, WinMD round-trip, extraction, go-to-def, imports)
- [x] CodeGen: 51 tests (WinMD emitter, header, IID, DllData, tables, decompiler)
- [x] Integration: 94 tests (CLI, diagnostics, formatting, semantic tokens, fixtures, code actions)
- [x] IDL fixture suite (15 files with `.expected.json` + `.expected.tree` sidecars)
- [x] End-to-end quick fix tests (apply fix → verify output IDL)
- [x] Diagnostic position tests (squigglies on correct token, not in comments)

### Documentation
- [x] README.md
- [x] CONTRIBUTING.md
- [x] docs/architecture.md (7 Mermaid diagrams)
- [x] docs/testing.md (VS Code extension build/test instructions)
- [x] docs/diagnostics.md (all 21 error codes)
- [x] editors/vscode/README.md (marketplace README)
- [x] editors/vscode/CHANGELOG.md

---

## 🔄 In Progress

*(nothing currently in progress)*

---

## ❌ Not Yet Implemented

### Critical for Production
- [ ] **`import "file.idl"` (IDL-to-IDL imports)** — currently only `.winmd` references work
- [ ] **Read custom attributes from imported `.winmd`** — `[uuid]`, `[contract]`, `[deprecated]`, `[exclusiveto]` on imported types are ignored
- [ ] **Read parameterized interfaces from `.winmd`** — generic type arguments not decoded from imports
- [ ] **PIID algorithm** — deterministic IID generation for parameterized type instances (SHA-1/RFC 4122 v5)
- [ ] **`partial runtimeclass` merging** — parsed but not merged across declarations
- [ ] **Composition factory encoding in WinMD** — controlling `IInspectable*` and non-delegating `IInspectable**` omitted from metadata `.ctor`
- [ ] **WinMD array encoding** — array length param omitted from MethodDefSig
- [ ] **Reserved parameter name auto-generation** — `result`, `operation`, `value` for retval params
- [ ] **`>>` vs `> >` disambiguation in parser** — currently `>>` is one token; nested generics need space

### Important
- [ ] **C preprocessor integration** — `cl.exe /E` invocation or built-in subset
- [ ] **`importlib` dispatch in parser** — recognized but not dispatched in classic mode
- [ ] **35 attribute-context keywords** not in lexer keyword table (work as identifiers, but no SyntaxKind)
- [ ] **NDR format string generation** (NDR20 + NDR64) — stub skeleton only
- [ ] **C proxy/stub code generation** (_c.c, _s.c) — skeleton only
- [ ] **Type library (.tlb) generation** — requires COM `ICreateTypeLib2`
- [ ] **ACF grammar** — placeholder only
- [ ] **~110 more CLI switches** — 30 of ~140 implemented
- [ ] **Velocity/Feature API staging** — `[feature(Name)]` → `[experimental]` transform
- [ ] **Contract history validation** (MIDL5132) — interface contracts must be in class's contract family
- [ ] **Case-insensitive namespace/type uniqueness validation**
- [ ] **Struct field restriction validation** — only value types, strings, `IReference<T>`
- [ ] **Unicode 3.0 identifier validation** — currently ASCII only
- [ ] **Incremental parsing for LSP** — currently full reparse on every edit
- [ ] **Cross-file analysis in LSP** — currently single-file only
- [ ] **WinMD file naming rules** — namespace matching, case-insensitive
- [ ] **`[contentproperty]`** — parsed but not semantically processed
- [ ] **`[allowforweb]`** — parsed but not semantically processed
- [ ] **`[fastabi]`** — parsed but not semantically processed

### Nice to Have
- [ ] **NativeAOT publish** — single-file `midl.exe`
- [ ] **Rename symbol** in LSP
- [ ] **WinMD inspection** — go-to-def into imported `.winmd` types
- [ ] **Formatting** — more sophisticated (preserve user formatting, align attributes)
- [ ] **Code lens** — show interface synthesis results inline
- [ ] **Extension icon** — custom icon for the VS Code extension
- [ ] **Test with Windows product IDL corpus** (T2 testing)
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
