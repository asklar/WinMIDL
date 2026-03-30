# Changelog

## v0.1.8 (2026-03-30)

### Features
- XDC (XML Doc Comments) support: `///` doc comments, `/doc` CLI switch for XDC XML output, hover shows summaries, `///` autocomplete for XML tag skeletons
- Syntax highlighting for doc comments: `///` prefix, XML tags, and content rendered in distinct colors
- MIDL3020 quality diagnostic for missing documentation comments
- Array params (`T[]`) merged to main: parser, header, WinMD, proxy/stub, LSP (gated behind `/arrays`)
- C++/WinRT E2E tests: IDL → .winmd → cppwinrt → projection headers → in-proc (39 assertions) + out-of-proc (25 assertions) with cross-process static methods via factory CLSID
- Inactive `#ifdef` region graying in editors via semantic tokens + Unnecessary diagnostic tag
- `midl.defines` setting in VS Code and VS for preprocessor defines (live update without restart)
- `#ifdef`/`#ifndef`/`#if`/`#endif` folding ranges
- Namespace type count code lens (e.g., "1 enum, 1 struct, 2 classes")
- `/diag` CLI flag to show Info-level quality diagnostics
- MIDL3017: redundant default attributes (`[threading(both)]`, `[marshaling_behavior(agile)]`)
- MIDL3018: obsolete attributes (`[dualapipartition]`, `[allowforweb]`, `[webhosthidden]`)
- MIDL3019: all-static class should be `static runtimeclass`
- MIDL3005 improved: suppressed for `*EventArgs` types, all-static classes, `// [obtained-from:]` comments
- `/release` Copilot skill for automated releases

### Bug Fixes
- Fix sourcemap positions for `#define` directives (preprocessor segment flushing)
- Fix MIDL1077 squiggle to highlight type reference, not parameter name
- Fix backslash `\` line continuation position preservation
- Fix `/doc` and `/diag` switches being parsed as `/D` defines
- Fix semantic token cache invalidation when defines change
- Fix `workspace/semanticTokens/refresh` sent as request (not notification)
- Fix E2E build scripts for ARM64 and cross-compile environments (`VSCMD_ARG_TGT_ARCH`)
- Fix VS Code config change to include arrays and defines settings

### Tests
- ~50 diagnostic position tests covering all preprocessor directives
- LF-only line ending position tests
- MIDL2006 `#include` position test
- 13 diagnostic codes position-verified (MIDL1009/1034-1038/1042/1044/1062/1077/2006/3003)
- 9 DiagnosticsPublisher integration tests
- Array E2E: 21 in-proc + 4 out-of-proc cross-process array marshaling
- C++/WinRT E2E: 64 assertions (in-proc + out-of-proc with proxy/stub)
- XDC emitter: 6 golden XML tests
- Doc comment: 20+ tests (lexer, binder, emitter, diagnostics, semantic tokens)
- Both array configurations tested (with/without `WITH_ARRAYS`)

### Other
- Cleaned up 11 merged worktrees + 50 stale branches
- Updated TODO.md with current status
- v0.1.7 tagged and released (previous session)
