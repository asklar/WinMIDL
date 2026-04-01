# Changelog

## v0.1.11 (2026-04-01)

### Features
- Add "Show references" toggle to class diagram — dotted arrows for type references in method params, return types, and properties
- Add Ctrl+mousewheel zoom in class diagram webview
- Refine reference arrow styling (open arrowhead, dimmer color)

### Bug Fixes
- Fix #58: extension no longer activates on non-Windows platforms or projects without IDL files
- Add lazy activation events (onLanguage:midl, workspaceContains)
- Make findServerBinary platform-aware with graceful error + "Open Settings" button

### Other
- Package platform-specific VSIXes (win32-x64, win32-arm64) — Mac/Linux users no longer see a broken extension
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
