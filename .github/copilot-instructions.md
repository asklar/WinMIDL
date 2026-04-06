# Copilot Instructions — WinMIDL

## Build & Test

```bash
dotnet build                                    # Build all projects
dotnet test                                     # Run all ~1,900 tests (xUnit)
dotnet test tests/Midl.Analysis.Tests           # Single project
dotnet test --filter "FullyQualifiedName~Parse" # Single test by name pattern
scripts/publish-optimized.ps1                   # NativeAOT binary + smoke test
scripts/package-vsix.ps1                        # VS Code VSIX (auto-infers version from git tag)
scripts/package-vs-vsix.ps1                     # VS VSIX
```

E2E proxy/stub tests require an MSVC x64 dev prompt:
```powershell
tests/e2e-custom-marshal/comprehensive/build-and-test.ps1   # In-proc (89 tests)
tests/e2e-custom-marshal/outofproc/build-and-test.ps1       # Cross-process (108 tests)
```

The VS extension must be built with MSBuild from VS, not `dotnet build`.

## Architecture

WinMIDL is a MIDL3 compiler with an LSP server, two editor extensions, and custom proxy/stub codegen.

**Pipeline**: IDL source → Lexer → Parser (lossless green tree) → Binder → InterfaceSynthesizer → VersionPropagator → AttributeValidator → CodeGen (WinMD / Header / IID / Proxy)

**Key projects** (in `src/`):
- `Midl.Syntax` — shared token/node types
- `Midl.Lexer` — hand-written lexer, built-in C preprocessor
- `Midl.Parser` — error-recovering recursive-descent parser (Roslyn green tree pattern)
- `Midl.Analysis` — binder, symbol table, type resolver, validators, WinMD reader
- `Midl.CodeGen` — WinMD emitter, C header, IID, proxy/stub generators, decompiler
- `Midl.Lsp` — JSON-RPC LSP server (no external NuGet deps)
- `Midl.Cli` — CLI entry point, NativeAOT-compatible

**Extensions** (in `editors/`):
- `editors/vscode/` — VS Code extension (TextMate grammar, LSP client, TypeScript)
- `editors/vs/Midl.VsExtension/` — Visual Studio extension (traditional VSSDK, ILanguageClient)

**Proxy/stub** (in `src/Midl.CodeGen/Proxy/`):
- Generates direct C++ proxy/stub code (no NDR) for COM cross-apartment/cross-process marshaling
- Payload format: simple packed binary with null-terminated HSTRING
- Interface pointer marshaling: typed IID first, IID_IUnknown fallback for system objects
- Runtime helpers in `include/winmidl/proxy_helpers.h`

## Conventions

### Feature/bug checklist (see CONTRIBUTING-SOP.md)

Every change must be evaluated across 12 aspects: lexer/parser, analysis, diagnostics, autocomplete, signature help, hover/goto-def, WinMD emission (on ALL targets: types, methods, properties, events, params, fields), header/IID, proxy/stub codegen, decompiler round-trip, VS Code ↔ VS parity, and testing.

### Compilation modes

- `CompilationMode.Strict` — MIDL3 only, classic constructs produce errors
- `CompilationMode.Compat` — allows classic MIDL2 constructs (coclass, typedef, etc.)
- `WITH_COMPAT` define — enables compat-mode code paths
- `WINDOWS_INTERNAL` define — enables internal features (feature_name, feature gates)

### Diagnostics

- `MIDL1xxx` — semantic errors and warnings
- `MIDL2xxx` — strict-mode errors (classic constructs, C-only types)
- `MIDL3xxx` — code quality hints (naming, empty types, etc.)
- All codes defined in `src/Midl.Analysis/Diagnostics/DiagnosticDescriptors.cs`

### Interface synthesis

Runtimeclasses get synthesized exclusive interfaces:
- Instance members → `I<Class>` (default interface)
- Constructors → `I<Class>Factory`
- Static members → `I<Class>Statics`
- Protected → `I<Class>Protected`
- Overridable → `I<Class>Overrides`

### Parameterized types

`IReference<T>`, `IVector<T>`, `IMap<K,V>`, `EventHandler<T>` etc. emit as `IInspectable*` in generated headers and proxy code. PIIDs computed via UUID v5.

### WinMD attribute emission

All attributes must be emitted to WinMD. Well-known attributes use dedicated `Add*Attribute` methods in `CustomAttributeEmitter.cs`. Everything else uses `AddGenericCustomAttribute` via `EmitRemainingAttributes`. Default values (`threading(both)`, `marshaling_behavior(agile)`) are suppressed in decompiled output.

### Proxy/stub marshaling

- `CoMarshalInterface`/`CoUnmarshalInterface` use the typed IID (needed for delegate callbacks)
- Falls back to `IID_IUnknown` for system WinRT objects (PropertyValue, etc.)
- `MSHCTX_LOCAL` for both inproc and cross-process
- Object type → `IID_IUnknown` (not `IID_IInspectable` — no proxy/stub for IInspectable)

### Agent workflow

- Background agents get their own dedicated git worktree + branch
- Use C# partial classes to minimize merge conflicts
- Never use regex to edit source code — use exact string matching
- Never hardcode IIDs — use generated `_i.c` files
- Every agent must run tests before committing

### VS extension limitations

- VS doesn't render markdown in LSP hover — hover handler strips markdown for VS
- VS doesn't support LSP `onTypeFormatting` — use native `ISmartIndent` instead
- VS built-in C++ project system hijacks `.idl` files — use `.idlrt` as primary extension
- VS extension uses traditional VSSDK (AsyncPackage + ILanguageClient), not VisualStudio.Extensibility SDK

### Testing patterns

- `CompilationHelper.Compile(source)` — compile IDL without SDK refs
- `CompilationHelper.Compile(source, new[] { sdk.WindowsWinMdPath })` — with SDK
- `CompileAndEmit(source)` — compile + emit WinMD bytes
- `OpenMetadataReader(bytes)` — read WinMD metadata
- E2E tests use `Check(condition, "TestName")` pattern with pass/fail counting
