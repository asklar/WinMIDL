# Visual Studio 2026 Extension ╬ô├ç├╢ TODO

This documents the remaining work for the VS 2026 extension.
The scaffold lives on branch `feature/vs-extension`.

## What's Done (on the feature branch)

### Scaffold
- `editors/vs/Midl.VsExtension/` project with VisualStudio.Extensibility SDK
- `MidlExtension.cs` ╬ô├ç├╢ extension entry point with metadata
- `MidlLanguageServerProvider.cs` ╬ô├ç├╢ launches Midl.Cli.exe --lsp via stdio
- `ServerDiscovery.cs` ╬ô├ç├╢ finds bundled or dev-loop binary
- `.idlrt` file extension registered in VS Code `package.json`
- `Directory.Build.props` override for TFM

## What's Remaining

### Must Have
1. **Test .idl registration** ╬ô├ç├╢ Try registering `.idl` as a document type in VS. If VS's built-in MIDL claims it, fall back to `.idlrt` only.
2. **Bundle TextMate grammar** ╬ô├ç├╢ Copy `midl.tmLanguage.json` into the VSIX and register for syntax highlighting
3. **Verify LSP works** ╬ô├ç├╢ Install in VS 2026 Preview, open a `.idlrt` file, verify diagnostics/completions/hover work
4. **VSIX manifest** ╬ô├ç├╢ Configure API-version-based compatibility (`[17.0,)`)

### Should Have  
5. **Port commands** ╬ô├ç├╢ Toggle strict mode, restart server, show log, select SDK version
6. **Port settings** ╬ô├ç├╢ serverPath, strictMode, codeQualityHints, useWindowsSdk, windowsSdkVersion, flatSdkTypeCompletions, fetchSdkDocs
7. **Port snippets** ╬ô├ç├╢ 19 MIDL snippets in VS snippet XML format

### Nice to Have
8. **WinMD virtual document provider** ╬ô├ç├╢ Port the `midl-winmd:` scheme for decompiled WinMD views (highest risk ╬ô├ç├╢ VS has different APIs than VS Code for virtual documents)
9. **CI pipeline** ╬ô├ç├╢ Add VS extension build/package job to release.yml
10. **Marketplace publishing** ╬ô├ç├╢ VS Marketplace listing

## Key Risk
VS 2026 may claim `.idl` for its built-in MIDL support, preventing our extension from handling those files. Mitigation: `.idlrt` fallback, which is already registered in VS Code too.
