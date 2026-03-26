# Visual Studio Extension — Status

This documents the VS extension status on branch `feature/vs-extension`.

## ✅ Done

### Core
- **Traditional VSSDK** extension (converted from OOP Extensibility SDK which didn't work)
- `AsyncPackage` + `ILanguageClient` (in-process, launches Midl.Cli.exe --lsp via stdio)
- `ServerDiscovery.cs` — finds bundled binary, auto-detects x64 vs ARM64 via `ProcessArchitecture`
- `.idl` and `.idlrt` file extensions registered (`.idlrt` recommended — VS C++ project system hijacks `.idl`)
- VSIX manifest targeting `[17.0,)` — works in VS 2022 and VS 2026

### Features
- **LSP integration** — diagnostics, completions, hover, F12, rename, code actions, semantic tokens, code lens
- **Smart indentation** — `ISmartIndent` MEF export (VS doesn't support LSP onTypeFormatting)
- **Plaintext hover** — detects client `contentFormat` capability, strips markdown for VS
- **TextMate grammar** — bundled `midl.tmLanguage.json` as embedded resource
- **Snippets** — bundled `snippets.json` (served via LSP completions)
- **Semantic token caching** — no re-lexing unchanged documents
- **Output window** — "MIDL Language Server" pane with live server stderr + trace logs

### Settings
- **Tools → Options → MIDL → General** — `DialogPage` with 6 settings:
  - Strict MIDL3 Mode, Code Quality Hints, Use Windows SDK, Windows SDK Version, Flat SDK Type Completions, Fetch SDK Docs
- **InitializationOptions** — reads persisted settings via `MidlPackage.Instance`
- **Live settings push** — `OnApply` sends `workspace/didChangeConfiguration` to server (no restart needed)

### Packaging
- `scripts/package-vs-vsix.ps1` — builds NativeAOT server + VS extension, produces VSIX
- Dual-arch: bundles both x64 (8.1 MB) and ARM64 (8.3 MB) NativeAOT binaries
- Total VSIX: ~6.8 MB compressed
- Icon, tags, description in manifest
- Published to VS Marketplace as "MIDL Language Support" by "Alexander Sklar"

### Bug fixes (also benefit VS Code)
- Fix duplicate MIDL1062 diagnostic (copy-paste bug in Binder.cs)
- Semantic tokens: use cached compilation instead of recompiling per request
- Workspace scan: exclude packages/, node_modules/, bin/, obj/; 100KB file size limit
- `workspace/didChangeConfiguration` support for live settings in both VS and VS Code
- Compile log includes file name for easier debugging

## 🔲 Remaining

### Should Have
1. **WinMD virtual document provider** — Port the `midl-winmd:` scheme for F12 into decompiled WinMD views. VS has different virtual document APIs than VS Code — needs investigation.
2. **CI pipeline** — Add VS extension build/package job to release.yml
3. **Quick fixes in VS** — Code actions work server-side but VS may not render them in all contexts

### Nice to Have
4. **Auto-restart on settings change** — currently some settings (like useWindowsSdk) take full effect after restarting the server
5. **Port 4 commands to VS menu** — Toggle Strict Mode, Restart Server, Show Log, Select SDK Version (commands are registered but may need menu placement tuning)
6. **Marketplace auto-publish** — script or CI step for VS Marketplace uploads

## Known Limitations
- VS doesn't render markdown in hover tooltips (tracked: [VSExtensibility#271](https://github.com/microsoft/VSExtensibility/issues/271))
- VS doesn't reliably apply `onTypeFormatting` edits — worked around with native `ISmartIndent`
- VS C++ project system claims `.idl` files — use `.idlrt` as primary extension in VS
