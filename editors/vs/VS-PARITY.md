# VS Extension ↔ VS Code Extension Parity

Last updated from audit on `feature/vs-parity` branch.

## ✅ At Parity

| Feature | VS | VS Code | Notes |
|---------|-----|---------|-------|
| TextMate grammar | `midl.tmLanguage.json` | `syntaxes/midl.tmLanguage.json` | Identical files (same hash) |
| File associations | `.idl`, `.idlrt` | `.idl`, `.idlrt` | Both via content type definitions |
| Toggle Strict Mode | Tools menu command | Command palette | Both persist and notify server |
| Restart Server | Tools menu command | Command palette | |
| Show Server Log | Tools menu → Output window | Command palette → output channel | |
| Select SDK Version | Tools menu command | Command palette | |
| Strict MIDL3 Mode | Tools → Options → MIDL | `midl.strictMode` | |
| Code Quality Hints | Tools → Options → MIDL | `midl.codeQualityHints` | |
| Use Windows SDK | Tools → Options → MIDL | `midl.useWindowsSdk` | |
| Windows SDK Version | Tools → Options → MIDL | `midl.windowsSdkVersion` | |
| Flat SDK Completions | Tools → Options → MIDL | `midl.flatSdkTypeCompletions` | |
| Fetch SDK Docs | Tools → Options → MIDL | `midl.fetchSdkDocs` | |
| Server Path override | Tools → Options → MIDL | `midl.serverPath` | |
| `didChangeConfiguration` | Sent on Options Apply | Sent on settings change | VS includes all 6 LSP settings |
| Server discovery | Bundled → repo → fallback | Bundled → repo → workspace | Same search order |
| Smart indent | Brace-based (4-space) | Deferred to LSP/editor | VS has custom `ISmartIndent` |
| Code actions | Via LSP server | Via LSP server | No custom client handling needed |
| WinMD decompiler | `WinMdDecompiler.cs` | N/A | VS-only feature |

## ⚠️ Known Gaps (Platform Limitations)

### 1. Snippets — Bundled but not active in VS

**VS Code**: 19 snippets registered declaratively via `contributes.snippets` in `package.json`.
The editor natively loads and serves them as completions.

**VS**: The same `snippets.json` is bundled in the VSIX but VS has no declarative JSON snippet
mechanism. Options to close this gap:

- **Best**: Add snippet completion support to the LSP server itself (benefits both editors)
- **Alternative**: Convert to VS `.snippet` XML files + `ProvideLanguageCodeExpansion`
- **Alternative**: Implement `IAsyncCompletionSourceProvider` (risk of conflicting with LSP completions)

The snippet definitions are identical between both extensions (same file content).

### 2. Inline docs browser (`midl.inlineDocsBrowser`)

**VS Code**: Opens Microsoft Learn documentation in a side panel (`SimpleBrowser.Show`).

**VS**: Not applicable — VS has its own integrated documentation browser. The `fetchSdkDocs`
setting controls whether hover tooltips fetch docs from Learn, which works in both editors.

### 3. Hover markdown with trusted commands

**VS Code**: Hover middleware enables trusted command links (e.g., `midl.openDocs`) in
markdown hover content. Clicking a link in the hover opens documentation.

**VS**: VS's LSP hover rendering supports basic markdown but not trusted command URIs.
Hover content displays correctly but doc links are not clickable. This is a VS platform
limitation — the VS LSP client does not support `command:` URIs in hover markdown.

### 4. First-line language detection

**VS Code**: Uses `firstLine` regex `^\s*(namespace|import|\[|runtimeclass|interface|delegate|struct|enum|//)` 
to detect MIDL files without an extension.

**VS**: Relies solely on file extensions (`.idl`, `.idlrt`). VS does not support first-line
language detection through the content type system.

## Settings Mapping

| VS Code setting | VS Options property | Sent to LSP | Notes |
|----------------|--------------------|----|-------|
| `midl.serverPath` | `ServerPath` | No | Client-only, for server discovery |
| `midl.strictMode` | `StrictMode` | Yes | |
| `midl.codeQualityHints` | `CodeQualityHints` | Yes | |
| `midl.useWindowsSdk` | `UseWindowsSdk` | Yes | |
| `midl.windowsSdkVersion` | `WindowsSdkVersion` | Yes | |
| `midl.flatSdkTypeCompletions` | `FlatSdkTypeCompletions` | Yes | |
| `midl.fetchSdkDocs` | `FetchSdkDocs` | Yes | |
| `midl.inlineDocsBrowser` | — | No | VS Code-only (SimpleBrowser) |
