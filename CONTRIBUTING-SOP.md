# Development SOP — Feature & Bug Fix Checklist

When adding a feature or fixing a bug, consider its impact across **all** of these aspects:

## 1. Authoring Experience
- **Lexer**: New keywords? New token types?
- **Parser**: New syntax nodes? Error recovery? Forward declarations?
- **TextMate grammar**: Syntax highlighting for new keywords/constructs (VS Code + VS)

## 2. Semantic Analysis
- **Binder**: New symbols? Type resolution? Attribute extraction?
- **Validation**: New diagnostics? Update `AttributeValidator`, `VersionPropagator`?
- **Type system**: WinRT type constraints? Struct field restrictions?

## 3. Diagnostics
- **Error codes**: Assign new MIDL1xxx codes, add to `DiagnosticDescriptors.cs`
- **Positions**: Accurate source positions via green tree FullWidth
- **Code actions**: Quick fixes for new diagnostics?
- **Did-you-mean**: Suggestions for misspelled names?

## 4. Contextual Autocomplete
- **Attribute completions**: Filtered by target (`target_runtimeclass`, `target_method`, etc.)
- **Type completions**: Context-aware (struct body = value types only, etc.)
- **Snippet completions**: Attribute argument snippets with placeholders
- **SDK completions**: Dynamic from WinMD providers

## 5. Signature Help
- **Method/constructor signatures**: Parameter names and types
- **Attribute arguments**: Show expected args for `[composable(IFactory, access, Contract, version)]`

## 6. Code Lens / Hover / Go-to-Definition
- **Hover**: Keyword docs, symbol info, external type markers
- **Go-to-definition**: Within file, cross-file, into WinMD
- **Code lens**: Show synthesized interface names (future)

## 7. WinMD Emission
- **Types**: TypeDef rows with correct flags
- **Attributes**: Custom attributes on ALL targets (types, methods, properties, events, params, fields)
- **InterfaceImpl**: For `requires` chains and implemented interfaces
- **MethodImpl**: For runtimeclass stub → interface method mappings
- **Default suppression**: Don't emit defaults (e.g. `threading(both)`)

## 8. Header / IID Emission
- **C/C++ header**: Correct ABI types (`IInspectable*` for parameterized types)
- **IID file**: `_i.c` with correct GUIDs
- **Forward declarations**: External types

## 9. Proxy/Stub Code Generation
- **Wire format**: Correct serialization for new types
- **Interface pointer marshaling**: Typed IID + IID_IUnknown fallback
- **Delegate proxy/stub**: Cross-process Invoke
- **Signature folding**: New shapes get folded

## 10. Decompiler
- **Round-trip**: Compile → WinMD → decompile → recompile should work
- **Attribute display**: Show all non-default attributes
- **Versioned blocks**: Group by contract version
- **Default suppression**: Don't show `threading(both)`, `marshaling_behavior(agile)`

## 11. Extension Parity (VS Code ↔ VS)
- **VS Code**: Full LSP features, TextMate grammar, snippets, settings
- **VS**: Traditional VSSDK, ILanguageClient, ISmartIndent (no onTypeFormatting)
- **VS limitations**: No markdown in hover, no onTypeFormatting
- **Both**: Same server binary, same diagnostics, same completions

## 12. Testing
- **Unit tests**: Lexer, parser, analysis, codegen at each layer
- **Integration tests**: CLI, LSP handlers, WinMD parity
- **E2E tests**: In-proc + out-of-proc proxy/stub marshaling
- **Parity fixtures**: Golden-file comparison against MIDL.exe output
- **Negative tests**: Invalid inputs produce correct errors

---

## Agent Workflow Rules

When dispatching background agents:
1. **Always use a dedicated worktree + branch** for each agent to avoid conflicts
2. **Use C# partial classes** for new functionality to minimize merge conflicts
3. **Never use regex to edit source code** — use the `edit` tool with exact string matching
4. **Never hardcode IIDs** — always use generated `_i.c` files
5. **Each agent must run tests** before committing

## Build & Test Commands

```bash
# Build
dotnet build

# Test all
dotnet test

# Test specific project
dotnet test tests/Midl.CodeGen.Tests
dotnet test tests/Midl.Analysis.Tests
dotnet test tests/Midl.Integration.Tests

# E2E (requires MSVC dev prompt)
tests/e2e-custom-marshal/comprehensive/build-and-test.ps1
tests/e2e-custom-marshal/outofproc/build-and-test.ps1

# Package extensions
scripts/package-vsix.ps1        # VS Code
scripts/package-vs-vsix.ps1     # VS

# Publish optimized binary
scripts/publish-optimized.ps1
```
