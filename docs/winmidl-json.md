# winmidl.json — Project Configuration

`winmidl.json` is an optional project file that configures multi-file compilation for WinMIDL. Place it in your project root directory. The LSP server discovers it automatically on startup and uses it to provide cross-file symbol resolution, diagnostics, and autocomplete.

---

## Example

```jsonc
{
    // IDL files to compile together
    "inputs": [
        "src/Widget.idl",
        "src/Helper.idl",
        "src/Delegates.idl"
    ],

    // WinMD references (external type libraries)
    "references": [
        "lib/MyLibrary.winmd"
    ],

    // Preprocessor definitions
    "defines": [
        "FEATURE_PREVIEW",
        "NTDDI_VERSION=0x0A00000C"
    ],

    // Additional include search paths for import/include directives
    "includePaths": [
        "include",
        "generated"
    ],

    // Set to true to skip auto-referencing the Windows SDK Windows.winmd
    "noSdk": false
}
```

> **Tip:** JSON comments (`//`) are supported.

---

## Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `inputs` | `string[]` | `[]` | IDL files to compile together. Paths are relative to the `winmidl.json` directory. All listed files share a single compilation context — types defined in one file are visible in the others. |
| `references` | `string[]` | `[]` | WinMD files to reference for external type resolution. Paths are relative to the `winmidl.json` directory. |
| `defines` | `string[]` | `[]` | Preprocessor macro definitions, equivalent to `/D` on the command line. |
| `includePaths` | `string[]` | `[]` | Search directories for `import "file.idl"` and `#include` directives, equivalent to `/I` on the command line. Paths are relative to the `winmidl.json` directory. |
| `noSdk` | `bool` | `false` | When `true`, the compiler does not automatically reference the Windows SDK `Windows.winmd`. Use this when building `Windows.winmd` itself or when targeting a non-Windows platform. |

All paths are resolved relative to the directory containing the `winmidl.json` file.

---

## How It Works

### LSP (VS Code Extension)

When the language server starts, it searches for `winmidl.json` in the workspace root. If found:

1. **Cross-file symbols** — All `inputs` files are compiled together, so types defined in `Helper.idl` are visible when editing `Widget.idl`.
2. **References** — External types from `.winmd` files are available for autocomplete, hover, and go-to-definition.
3. **Defines** — Preprocessor macros are applied consistently across all files.
4. **Include paths** — `import` directives resolve using the configured search paths.

### CLI

The CLI does not read `winmidl.json` automatically — it uses explicit command-line switches. However, you can use `winmidl.json` as a source of truth and invoke the CLI with the corresponding arguments:

```bash
# Equivalent of the example winmidl.json above
winmidl /winrt /D FEATURE_PREVIEW /I include /reference lib/MyLibrary.winmd src/Widget.idl src/Helper.idl src/Delegates.idl
```

---

## Alternative: `// @ref:` Directives

For lightweight cross-file hints without a project file, you can add `// @ref:` comments at the top of an IDL file:

```idl
// @ref: Helper.idl
// @ref: ../shared/Types.idl
// @ref: ../bin/ExtLib.winmd

namespace MyApp
{
    runtimeclass Widget
    {
        Widget();
        Helper GetHelper();       // Helper is defined in Helper.idl
        ExtLib.IService Service;  // IService is in ExtLib.winmd
    }
}
```

The LSP parses these directives and:
- **`.idl` references**: adds the referenced files' directories to the include path, enabling `import` resolution
- **`.winmd` references**: adds the files as WinMD type references, enabling resolution of external WinRT types

This is useful for quick prototyping or small projects that don't need a full `winmidl.json`.

**Priority order** (highest to lowest):
1. `winmidl.json` project file
2. `// @ref:` directives in source
3. `import "file.idl"` with auto-discovered include paths
4. Workspace-level `.winmd` file scanning

---

## Schema Reference

A JSON Schema for editor autocomplete in `winmidl.json` files:

```json
{
    "$schema": "http://json-schema.org/draft-07/schema#",
    "type": "object",
    "properties": {
        "inputs": {
            "type": "array",
            "items": { "type": "string" },
            "description": "IDL files to compile together"
        },
        "references": {
            "type": "array",
            "items": { "type": "string" },
            "description": "WinMD reference files"
        },
        "defines": {
            "type": "array",
            "items": { "type": "string" },
            "description": "Preprocessor definitions"
        },
        "includePaths": {
            "type": "array",
            "items": { "type": "string" },
            "description": "Include search paths"
        },
        "noSdk": {
            "type": "boolean",
            "default": false,
            "description": "Skip automatic Windows SDK reference"
        }
    },
    "additionalProperties": false
}
```
