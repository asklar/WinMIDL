# Changelog

## v0.1.12 (2026-04-03)

### Features
- Add "MIDL: Report Issue" command — collects server diagnostics, opens pre-filled GitHub issue with consent dialog
- Add DiagnosticEventLog ring buffer (500 events) as server flight recorder
- Add `midl/diagnosticReport` LSP request with source-gen JSON serialization
- Add memory budget (512MB) and error-count heuristic to workspace scan — self-healing for large workspaces
- Detect non-MIDL3 files via keyword heuristic, evict high-error compilations from cache
- Report private bytes, working set, and GC heap memory in diagnostic report
- Split type counts into user-defined vs resolved (with WinMD source breakdown)

### Bug Fixes
- Fix #59: extension no longer activates on workspaces with IDL files unless an IDL file is opened
- Remove `workspaceContains` activation events; only activate on `onLanguage:midl`
- Fix E2E test: remove hardcoded ref path and unavailable SDK type
- Fix hardcoded path in VS packaging script

### Other
- Redact absolute paths to workspace-relative in diagnostic reports for privacy
- 22 new tests (ring buffer, MIDL3 detection, LSP integration, path redaction)

## v0.1.11 (2026-04-01)

### Features
- Add "Show references" toggle to class diagram — dotted arrows for type references
- Add Ctrl+mousewheel zoom in class diagram webview

### Bug Fixes
- Fix #58: extension no longer activates on non-Windows platforms or projects without IDL files
- Make findServerBinary platform-aware with graceful error + "Open Settings" button

### Other
- Package platform-specific VSIXes (win32-x64, win32-arm64)
- Update release skill for platform-specific packaging
- Update docs for v0.1.7–v0.1.10 features

## v0.1.10 (2026-04-01)

### Features
- Add interactive class diagram generation (MIDL: Show Class Diagram command)
- Graphviz-based UML layout via viz.js with color-coded type headers
- Drag-to-move nodes with live arrow updating
- Right-click "Copy as PNG" to clipboard
- Color-coded member names: methods (yellow), properties (blue), events (purple)
- Standard UML arrows: solid = extends, dashed = implements/requires

### Bug Fixes
- Fix deterministic diagram layout with fixed fdp random seed

## v0.1.9 (2026-03-31)

### Features
- **Manifest generation** (`--manifest`): Generate WinRT activation manifest fragments from `.winmd` files. Supports both SxS (`.manifest`) and AppX (`.appxmanifest`) formats for unpackaged and packaged apps.
- **Decompiled WinMD views**: Full semantic token highlighting, hover with type kind (runtimeclass/interface/enum/method/property), Microsoft Learn links, document symbols, and folding ranges.
- **Array syntax always enabled**: `T[]`, `out T[]`, `ref T[]` no longer require `/arrays` flag — standard WinRT syntax like MIDL.exe.
- **NativeAOT binary 45% smaller**: 10.0 MB → 5.5 MB by removing `System.Xml`, `System.Net.Http`, and `System.Text.RegularExpressions` dependencies.

### Improvements
- Hover doc fetching moved to VS Code extension (client-side) — descriptions appear on first hover instead of requiring re-hover.
- All regex replaced with hand-written string operations — faster startup, smaller binary.
- XDC doc comment parser uses lightweight `XmlTagParser` instead of `System.Xml.XmlDocument`.
- MIDL3021 diagnostic for malformed doc comments (unclosed/mismatched XML tags).

### Bug Fixes
- Fixed decompiled hover showing "type" for methods/properties instead of correct kind.
- Fixed decompiled hover not prepending enclosing namespace for Learn URLs.
- Fixed WinMdReader ThreadingAttribute/MarshalingBehavior enum value mapping (off-by-one).
- Fixed test regressions from arrays-always-enabled change.

## v0.1.8 (2026-03-30)

### Features
- **XML Doc Comments (`///`)**: Add `/// <summary>` comments above types and methods. Syntax highlighted with distinct colors for tags, content, and `///` prefix. Hover shows summaries. `///` autocomplete generates `<param>`/`<returns>` skeletons. New `/doc <file>` CLI switch outputs XDC XML documentation.
- **Array parameters (`T[]`)**: Pass, receive, and fill arrays in IDL methods. Supported across header, WinMD, and proxy/stub codegen. Gated behind `/arrays` flag.
- **Inactive `#ifdef` region graying**: Code inside false `#ifdef` blocks appears dimmed. Configure active defines via `midl.defines` setting — changes take effect immediately.
- **Preprocessor folding**: `#ifdef`/`#ifndef`/`#if`/`#endif` blocks are foldable in the editor.
- **Namespace type count code lens**: Shows type breakdown (e.g., "1 enum, 2 classes") above each namespace block.
- **`/diag` CLI flag**: Shows info-level quality hints from the command line.
- **C++/WinRT E2E validation**: Verified that emitted `.winmd` files work end-to-end with `cppwinrt.exe` projections, including cross-process static methods.

### Quality Diagnostics
- **MIDL3017**: `[threading(both)]` and `[marshaling_behavior(agile)]` are defaults — can be omitted.
- **MIDL3018**: `[dualapipartition]`, `[allowforweb]`, `[webhosthidden]` have no effect in modern WinRT.
- **MIDL3019**: Class with only static members suggests `static runtimeclass`.
- **MIDL3020**: Public type or method has no documentation comment.
- **MIDL3005** improved: No longer warns for `*EventArgs` types or types with `// [obtained-from:...]` comments.

### Bug Fixes
- Diagnostic squiggles now appear on the correct line after `#define`, `#ifdef`, and backslash line continuations.
- MIDL1077 squiggle highlights the type reference (e.g., `SoftwareBitmap`) instead of the parameter name.
- `/doc` and `/diag` switches no longer incorrectly parsed as `/D` preprocessor defines.
- Changing `midl.defines` in settings immediately updates syntax highlighting and diagnostics.

## v0.1.7 (2026-03-29)

### Features
- SourceMap: multi-file diagnostic position tracking (preprocessor integration, file attribution, multi-file publishing, `#ifdef` gap tracking)
- Add parse-based symbol lookup for WinMD navigation
- Add `/diag` flag for sourcemap position diagnostics

### Bug Fixes
- Fix backslash line continuation positions in preprocessor
- Fix MIDL1077 squiggle to highlight type reference, not parameter name
- Fix sourcemap positions for `#define` directives
- Fix preprocessor diagnostics being double-remapped through sourcemap
- Fix preprocessor error/warning positions (MIDL1074)
- Fix diagnostic position attribution for `#include`'d files
- Fix `#include` warning squiggle pointing to line 1 instead of actual position
- Fix diagnostic positions not updating on editor changes
- Fix diagnostic positions with multiple `#define`/`#ifdef` lines
- Fix sourcemap for macro substitution and `\r\n` newlines
- Fix F12 on delegates with long generic return types in decompiled WinMD

### Tests
- Add comprehensive diagnostic position tests across all layers (LF endings, preprocessor, `#include`, squiggles, end-to-end)

### Other
- Reframe README for end users with Midl.Cli binary
- Add design docs for C++/WinRT E2E tests and AppxManifest generation

## v0.1.6 (2026-03-28)

### Features
- Add `constructor_name`, `interface_name`, `static_name` attribute support with autocomplete and validation
- Add contextual autocomplete for attribute arguments
- Add F12 go-to-definition for attribute names
- Add signature help for attribute arguments
- Implement `textDocument/foldingRange` for LSP server
- Add WinMD attribute emission for 6 missing attributes
- Add decompiler support for `experimental`, `default_overload`, `allowforweb`, `contentproperty` attributes
- Support multiple `[static]` interfaces and generic custom attribute emission

### Bug Fixes
- Fix LSP server enabling trace to stderr permanently
- Fix infinite loop in `DetectAttributeArgContext` causing integration test hang
- Fix WinMD emission parity with MIDL.exe
- Fix CLR vs WinRT `AttributeTargets` flags decoding
- Fix `[feature]` on runtimeclass: skip SDK target validation for builtins
- Fix versioned block attrs propagated to member attribute lists
- Fix arg positions for all validators + deprecated on all targets
- Fix duplicate MIDL1093: report once on class, not per member
- Fix version arg squiggle: point at the invalid arg, not the attr name
- Fix threading/marshaling target validation + version arg validation
- Fix version validation: check major.minor range (0–65535 each)
- Fix 3 bugs: property/event attribute emission, deprecated on methods, composable protected access
- Fix `/winmd` implies `/winrt` mode
- Fix attribute emission on parameters, struct fields, and enum members
- Close 21 negative parity gaps with MIDL.exe
- Fix VS extension parity gaps with VS Code extension

### Tests
- Add arg count validation for threading/marshaling + contract version tests
- Add tests for `[feature]` on runtimeclass + CLR flag decoding
- Add position accuracy tests for MIDL1093 contract version exceeds
- Add SDK integration tests for threading/marshaling/version validation
- Add integration tests for CLI switches
- Add squiggle position accuracy tests
- Add attribute autocomplete and diagnostics audit tests
- Add completion context edge case tests
- Refactor: move E2E test logic from PowerShell to C#
- Move slow E2E tests to separate Midl.E2E.Tests project

### Other
- Pass cached compilation to all LSP handlers
- Track source positions on attribute arguments for accurate squiggles
- Decompile attribute types + fix threading SDK target conflict
- Package scripts: auto-infer version from latest git tag
- Support all VS editions: Community, Professional, Enterprise
- Exclude PDBs from both VSIXs

## v0.1.5 (2026-03-27)

### Features
- **Custom proxy/stub code generation** for WinRT COM interfaces
- Preprocessor `#`/`##`/variadic macros and enum expression evaluator
- Add `scanSubfolders` and `excludeDirectories` to `winmidl.json`
- Add Visual Studio 2026 extension scaffold with LSP integration
- Convert VS extension from VisualStudio.Extensibility SDK to traditional VSSDK
- Add VS extension features: commands, settings, grammar, snippets
- Add `ISmartIndent` for auto-indentation in VS
- Add `Tools > Options > MIDL` settings page in VS
- Support live settings changes via `workspace/didChangeConfiguration`
- Dynamic attribute completions from SDK and referenced WinMDs
- Resolve SDK attributes from WinMD for validation
- Add package-vs-vsix.ps1 for VS extension packaging
- Bundle both x64 and ARM64 server binaries in VS VSIX
- Support ARM64 cross-compilation in build scripts

### Bug Fixes
- Fix attribute completion: show only short names, not class names
- Fix publisher name to match VS Marketplace
- Fix `InitializationOptions` null handling and auto-load
- Fix duplicate MIDL1062 diagnostic: remove duplicated emit call
- Fix memory leak: don't compile in `SemanticTokensHandler`
- Fix semantic tokens recompilation: use cached compilation from analysis
- Fix workspace scan skips large files and non-source directories
- Fix `MissingToken` in `CheckStrictModeTokensRecursive` and advance token index
- Fix x64 NativeAOT: add VC lib path for native builds
- Strip markdown from hover for VS (plaintext-only LSP clients)

### Tests
- Add 10 tests for attribute completion and default inference
- Add tests for SDK attribute resolution (noexcept)

### Other
- Cache semantic token results to avoid re-lexing unchanged documents
- Add VS Output window logging and fix `onTypeFormatting`
- Add file name to compile logs, exclude `packages/` from workspace scan
- Bundle server binary in VSIX and add fallback discovery paths

## v0.1.4 (2026-03-24)

### Features
- Add package-vsix.ps1 for local VSIX packaging

### Bug Fixes
- Fix LSP crashes on untitled files and `@ref` directives

## v0.1.3 (2026-03-24)

### Features
- Full round-trip support for `ref const`, `out`, `ref` parameter modifiers
- Add VS Code Marketplace publish step to release pipeline

### Other
- Auto-publish extension to Marketplace using Azure CLI credentials
- Replace auto-publish with manual publish-extension.ps1 script

## v0.1.2 (2026-03-23)

### Other
- Extension version bump for Marketplace republish

## v0.1.1 (2026-03-23)

### Bug Fixes
- Fix README: cross-file resolution supported via `winmidl.json` and `@ref` directives
- Fix SVG layout: tighten padding, fix truncation in code-quality
- Fix diagnostics tooltip: widen for MIDL1001, add lightbulb
- Fix async-naming SVG spacing for GitHub rendering

### Other
- Add visual SVGs to VS Code extension README (SDK autocomplete, F12/decompile, Learn panel, async naming, rename)
- Render SVGs to PNG for Marketplace compatibility
- Remove dev/build sections from README, fix license
- Exclude cli-output.svg from VSIX package
- Extension ships NativeAOT binary, no .NET runtime needed

## v0.1.0 (2026-03-23)

### Features
- **First public release** — replaces midl3-language-server
- F12-in-F12 via custom LSP request `midl/decompileDefinition`
- F12 on namespace segments and within decompiled WinMD views
- F12 on built-in WinRT types with per-namespace virtual docs
- F12 on WinMD types and `@ref` to `.winmd` files
- CLR↔WinRT type projection mapping for decompiler
- Cross-file go-to-definition for imports and WinMD types
- Implement LSP rename handler (F2 rename symbol)
- Workspace-wide diagnostics and file watching
- LSP Protocol types with source-generated JSON serialization
- Add Code Lens for `@ref` directives showing type counts
- Open MS Learn docs in VS Code side panel
- Add MS Learn hover links
- SDK types in completion with dot notation drill-down
- SDK version picker: QuickPick command with 'none' option
- Auto-discover `Windows.winmd` from SDK for type completion
- Add `[Version]` on synthesized interfaces + Family access for protected stubs
- Optimize SDK discovery: skip registry when standard path exists

### Bug Fixes
- Fix F12 hang in decompiled views — skip analysis + optimize lookup
- Fix F12 on fully qualified built-in types (TypedEventHandler, IVector, etc.)
- Fix F12 on any segment of FQN resolves via lazy WinMD providers
- Fix F12 on middle namespace segments
- Fix F12 shares cached compilation with diagnostics/autocomplete
- Fix decompiler: groups members into versioned blocks
- Fix decompiler: shows base class + decoded version numbers
- Fix decompiler: shows overridable/protected modifiers on members
- Fix decompiler: shows static members from `[static()]` interfaces
- Fix decompiler: property shorthand uses PropertyAccessors for setter detection
- Fix decompiler: uses short names only for types in current namespace
- Fully qualify all cross-namespace types in decompilation
- Emit ALL custom attributes to WinMD, not just whitelisted ones
- Fix dot completion: insertText uses full dotted path, canonical casing, mid-typing
- Fix dot-after-type showing completions
- Fix Code Lens: correctly count types from directory `@ref`
- Fix hover for FQN, dot completion textEdit
- Fix hover command URI with isTrusted
- Fix protected/overridable WinMD emission on sealed classes
- Fix context-aware type completion for inheritance, requires, params, events
- Fix autocomplete ordering for `@ref` and import completions
- Fix `@ref` autocomplete triggers on `:` and `\` characters
- Fix NativeAOT: set LIB/PATH directly, skip broken vcvarsall SDK
- Fix C++ WinMD dumper: decode enum-typed custom attributes
- Fix decompiler emits duplicate members for overridable/protected
- Fix RunAsync breaks on EOF
- Fix icon proportions and remove rounded corners

### Tests
- Add tests for protected/overridable WinMD emission on sealed classes
- Add tests for did-you-mean suggestions and `[default]` base list
- Add tests for MIDL1078, MIDL1081, MIDL1083, MIDL1084
- Add e2e tests for SDK dot completion scenarios
- Regression tests for squiggle offset, generic decompile, F12 definition search

### Other
- Restore WinMD emitter parity (GUID algorithm, assembly name, version encoding, visibility)
- Restore LSP WinMD provider cache, compilation cache, code action context
- Restore parser perf optimizations (SingleChildGreenNode, inlining, pooling)
- Refactor: split CompletionHandler, GotoDefinitionHandler, MidlLanguageServer, extension.ts into focused modules
- esbuild bundling for VSIX — single JS file, no node_modules
- Switch CI/release to self-hosted runner with MSVC discovery
- Add NativeAOT optimization flags to release pipeline

## v0.0.4 (2026-03-22)

### Bug Fixes
- Fix WinMD parity with original midl.exe (C++ dumper validated)
- Fix 3 WinMD parity gaps: interface GUIDs, EventHandler projection, static classes

### Other
- C++ WinMD dumper, version encoding, `feature_name` gating, attr targets, experimental

## v0.0.3 (2026-03-22)

### Features
- Support `[default]` in interface lists and non-void delegate returns
- Emit MIDL1102 diagnostic for missing `@ref:` targets

### Bug Fixes
- Fix 5 critical WinMD emitter bugs found by parity comparison

### Other
- Optimize Parser hot paths to reduce allocation and call overhead
- Optimize compilation pipeline hot paths

## v0.0.2 (2026-03-20)

### Features
- Add function-like macro support to preprocessor
- Add TCHAR, OLECHAR, CLSID, REFIID and more Win32 aliases (compat mode)
- Add `/cpp_cmd cl.exe` external preprocessor option
- Add MIDL2006: warn about `#include` in strict mode

### Bug Fixes
- Fix TypeRef positions: use green tree offset instead of text lookup
- Fix LSP: remove double preprocessing that broke diagnostic positions
- Fix MIDL3016: don't suggest shorthand when already using shorthand
- Fix typedef of forward-declared enums/structs, mark imported types external
- Fix preprocessor: line continuation, comment stripping, recursive expansion, `/I` parsing
- Fix void params in compat mode, add preprocessor defines
- Fix global scope rules, duplicate imports, void param, `#undef`
- Fix cl.exe detection
- Fix MIDL2006 wording: `#include` inlines, import references

### Other
- Bind typedef declarations: inner enums/structs now registered
- Import idempotency: duplicate definitions from imports silently skipped
- Forward declarations skip if type already defined, recursive macro expansion
- Skip enum range validation in compat mode
- All C/Win32 type aliases are compat-only

### Tests
- Add test: multiple attributes in one bracket `[attr1, attr2(p1, p2)]`

## v0.0.1 (2026-03-20)

### Features
- **Initial implementation**: MIDL3 compiler rewrite in C# / .NET 10
- Lexer, parser (lossless green tree), binder, code generation (WinMD, C header, IID)
- LSP language server with diagnostics, autocomplete, hover, go-to-definition, formatting
- Interface synthesis for runtimeclasses (`IClassFactory`, `IClassStatics`, `IClassProtected`, `IClassOverrides`)
- Attribute extraction pipeline, `AttributeValidator`, `VersionPropagator`
- WinMD fidelity + decompiler (`--decompile`)
- C preprocessor + WinMD encoding fixes + `>>` disambiguation
- Built-in WinRT types + decompiler crash fix + round-trip test
- IDL imports, partial merge, PIID algorithm, WinMD attr reading
- Custom attribute support + CI + NativeAOT + OS fixture tests
- SDK auto-discovery + perf benchmarks
- Diagnostic sweep (MIDL1033–1047) + access/inheritance validation
- Context-aware attribute autocomplete in LSP
- Code quality hints (MIDL3001–3009) for LSP
- Add cross-file LSP context via `winmidl.json` and `@ref` directives
- Add MIDL1065 warning for unknown/undeclared attributes
- Support forward declarations (`type Name;`) to prevent cascading errors
- Add float literal support and type validation (MIDL1064)
- Context-aware import autocomplete for IDL files
- Fully contextual autocomplete based on cursor scope
- Validate enum member values: type and range checks (MIDL1067–1068)
- Add MIDL3010: attribute names should end with `Attribute`
- All strict mode violations are errors; add MIDL3011–3015 quality hints + fixers
- Add MIDL3016: property with get+set can use shorthand
- Add quick fix for MIDL3016: property shorthand
- Add internal keyword as proper modifier
- Error on protected accessor and protected interface members (MIDL1072–1073)
- Add debounce (300ms) and content-hash caching to LSP
- WinMD per-namespace file naming (MIDL1071)
- Improve document formatter: blank lines, spacing, generics
- Add command palette commands + fix LSP for untitled documents
- Add `--analyze` mode, IDL fixture tests, code action fixers
- Add `/no_strict` as alias for `/compat`
- Add comprehensive diagnostic reference doc (`docs/diagnostics.md`)
- NativeAOT working — 3.6 MB binary, 30ms compile time
- Bundle both x64+arm64 CLI binaries in VSIX
- Tag-triggered release with NativeAOT optimized binary

### Bug Fixes
- Fix: midl stops at first error, not crashes
- Fix attribute diagnostic position and add arg type validation (MIDL1064)
- Fix MissingToken positions in comments + code action fixers
- Fix document symbol crash: skip empty/external/synthesized names
- Fix interface naming hint, add `exclusiveto`/default-iface validation
- Fix pointer diagnostic: single squiggly, correct message, accurate position
- Fix `wchar_t` and `char` to suggest `Char16` (not `Char`)
- Fix: `internal` is an attribute `[internal]`, not a keyword modifier
- Fix: interface properties also check per-accessor protected for MIDL3016
- Fix CI pipeline (master→main)
- Fix release: create publish dir, add `.vscodeignore`
- Fix AOT: use Speed optimization

### Tests
- Add 50 keyword coverage tests (transitive import fix)
- Add 10 new formatting tests
- Add hex literal tests for unsigned type range validation
- Add Char16, negative literals, float tokens, and numeric range validation tests
- Add tests for protected accessor errors and read-only property
- Add end-to-end quick fix tests
- Add OS fixture test script + external fixtures
