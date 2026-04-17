# WinMIDL CLI Guide

A modern MIDL compiler that produces Windows Metadata, C/C++ headers, and custom proxy/stub code from a single ~6 MB native executable — no .NET runtime required.

---

## Installation

### Option 1: dotnet tool (requires .NET 10 SDK)

```bash
# One-shot execution (no install)
dnx winmidl /winrt Widget.idl

# Global install
dotnet tool install -g WinMIDL
winmidl /winrt Widget.idl
```

### Option 2: Standalone binary (no .NET required)

Download the latest `winmidl-<version>.zip` from the [Releases](https://github.com/asklar/WinMIDL/releases) page. Contains a single self-contained NativeAOT binary per architecture — no installer needed.

### Option 3: VS Code / Visual Studio extensions

Search for "MIDL" in the extension marketplace, or install from the [VS Code Marketplace](https://marketplace.visualstudio.com/items?itemName=AlexanderSklarMSFT.midl3-language-server).

---

## Quick Start

```bash
# Compile a MIDL file → header + IIDs
Midl.Cli /winrt /h Widget.h /iid Widget_i.c Widget.idl

# Generate Windows Metadata
Midl.Cli /winmd Widget.winmd Widget.idl

# Generate proxy/stub code for COM marshaling
Midl.Cli /winrt /h Widget.h /iid Widget_i.c /cppproxy Widget_proxy.cpp Widget.idl

# Decompile a .winmd back to IDL
Midl.Cli --decompile Windows.Foundation.winmd

# Syntax check only (no output)
Midl.Cli /Zs Widget.idl
```

---

## Outputs

| Artifact | Extension | Switch |
|----------|-----------|--------|
| Windows Metadata | `.winmd` | `/winmd` |
| C / C++ headers | `.h` | `/h` |
| IID definitions | `_i.c` | `/iid` |
| Custom proxy/stub | `_proxy.cpp` | `/cppproxy` |
| DllData | `dlldata.c` | (automatic) |
| Activation manifest | `.manifest` / `.appxmanifest` | `--manifest` |
| XML doc comments | `.xdc` | `/doc` |

---

## Command-Line Reference

```
Midl.Cli [switches] <input.idl>
```

### Compilation

| Switch | Description |
|--------|-------------|
| `/winrt` | Enable Windows Runtime (MIDL3) mode |
| `/winmd <file>` | Emit Windows Metadata (`.winmd`; implies `/winrt`) |
| `/h <file>` | Emit C/C++ header |
| `/iid <file>` | Emit IID definitions (`_i.c`) |
| `/cppproxy <file>` | Emit custom proxy/stub C++ code |
| `/doc <file>` | Emit XML doc comments (`.xdc`) |
| `--manifest <file>` | Generate activation manifest |
| `--server_dll <name>` | Server DLL name for manifest generation |
| `/out <dir>` | Set output directory |

### Preprocessor & Configuration

| Switch | Description |
|--------|-------------|
| `/D <name>=<val>` | Define a preprocessor macro |
| `/I <dir>` | Add an include search path |
| `/reference <.winmd>` | Add a WinMD reference |

### Diagnostics

| Switch | Description |
|--------|-------------|
| `/Zs` | Syntax check only — no output files |
| `/diag` | Show info-level diagnostics (code quality hints) |
| `/W<n>` | Set warning level |
| `/WX` | Treat warnings as errors |
| `--analyze` | Analyze and report diagnostics |

### Tools

| Switch | Description |
|--------|-------------|
| `--decompile <.winmd>` | Decompile a WinMD back to IDL |
| `--dump-winmd <.winmd>` | Dump WinMD metadata tables |
| `--lsp` | Start as an LSP server (stdio) |
| `/nologo` | Suppress the banner |
| `/?` | Show full help |

---

## Generating Windows Metadata (WinMD)

```bash
# Generate a .winmd file (implicitly enables /winrt mode)
Midl.Cli /winmd output.winmd input.idl

# Generate both header and WinMD
Midl.Cli /winrt /h output.h /winmd output.winmd input.idl

# With SDK references for resolving Windows.Foundation types
Midl.Cli /winmd output.winmd /reference "%WindowsSdkDir%\UnionMetadata\10.0.22621.0\Windows.winmd" input.idl
```

### Inspecting WinMD Files

```bash
# Dump metadata tables (types, methods, attributes)
Midl.Cli --dump-winmd output.winmd

# Decompile back to IDL
Midl.Cli --decompile output.winmd

# Decompile a specific namespace
Midl.Cli --decompile output.winmd --namespace Windows.Foundation
```

---

## Custom Proxy/Stub Code Generation

WinMIDL generates custom C++ proxy/stub code for COM marshaling, replacing traditional MIDL-generated NDR stubs with compiled, type-safe serialization.

### Generating Proxy Code

```bash
Midl.Cli /winrt /h Widget.h /iid Widget_i.c /cppproxy Widget_proxy.cpp Widget.idl
```

This produces:

| File | Content |
|------|---------|
| `Widget.h` | C/C++ header with interface declarations |
| `Widget_i.c` | GUIDs (IIDs, CLSIDs) as constexpr inline constants |
| `Widget_proxy.cpp` | Proxy and stub implementations |
| `dlldata.c` | DLL registration plumbing |

### Building a Proxy/Stub DLL

The generated code requires the `include/winmidl/` headers from the WinMIDL distribution:

```bash
# Compile the proxy/stub DLL (MSVC, from a Developer Command Prompt)
cl /nologo /std:c++latest /EHsc /LD ^
    Widget_proxy.cpp Widget_i.c dlldata.c ^
    /I path/to/winmidl/include ^
    ole32.lib oleaut32.lib rpcrt4.lib ^
    /link /DEF:proxy.def /OUT:WidgetProxy.dll
```

Example `proxy.def`:
```def
LIBRARY WidgetProxy
EXPORTS
    DllGetClassObject   PRIVATE
    DllCanUnloadNow     PRIVATE
    DllRegisterServer   PRIVATE
    DllUnregisterServer PRIVATE
```

### Registering the Proxy DLL

```bash
# Register for in-process marshaling (requires admin)
regsvr32 WidgetProxy.dll

# Or register in HKCU (no admin required)
# The generated code supports per-user COM registration
```

### Supported Marshaling Patterns

- **Primitives** — all WinRT value types, HSTRING, GUID, EventRegistrationToken
- **Structs** — blittable (single memcpy) and non-blittable (field-by-field), with compile-time layout verification via `static_assert`
- **Enums** — treated as blittable (4-byte `INT32`/`UINT32`)
- **Interfaces** — typed IID marshaling with `IID_IUnknown` fallback for system objects
- **Delegates** — typed callback proxies with event registration
- **Out parameters** — including interface out params with cleanup
- **Properties** — get/set with full round-trip
- **Events** — add/remove with `EventRegistrationToken`
- **Arrays** — pass, fill, and receive array patterns
- **Generic collections** — `IVector<T>`, `IMap<K,V>`, `IReference<T>`, `EventHandler<T>`, etc.
- **Multiple interfaces** — QI, `GetIids`, inheritance chains
- **Nullable struct fields** — `IReference<T>` in structs uses inline `[hasValue][T]` encoding via `winrt::box_value`/`unbox_value`

---

## Project Configuration (`winmidl.json`)

For multi-file projects, create a `winmidl.json` in your project root:

```jsonc
{
    "inputs": ["src/Widget.idl", "src/Helper.idl"],
    "references": ["lib/MyLibrary.winmd"],
    "defines": ["FEATURE_X"],
    "includePaths": ["include"],
    "noSdk": false
}
```

This enables cross-file symbol resolution for both the CLI and the LSP (editor extensions).

For lightweight per-file hints, use `// @ref: other.idl` at the top of your source.

---

## Doc Comments

Both XML (XDC) and Doxygen-style comments are supported:

```idl
/// <summary>A widget that does things.</summary>
/// <param name="name">The widget name</param>
runtimeclass Widget { ... }

/** @brief A widget that does things.
 *  @param name The widget name
 */
runtimeclass Widget { ... }

//! @brief A short doc comment
runtimeclass Widget { ... }
```

Supported Doxygen tags: `@brief`/`\brief`, `@param`/`\param` (including `@param[in]`), `@return`/`\returns`, `@see`, `@note`, `@deprecated`.

Generate XML doc comment files:
```bash
Midl.Cli /winrt /doc Widget.xdc Widget.idl
```

---

## Editor Extensions

WinMIDL includes editor extensions for VS Code and Visual Studio that use the same compiler binary as an LSP server:

- **VS Code**: Install from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=AlexanderSklarMSFT.midl3-language-server)
- **Visual Studio**: Install the `.vsix` from the [Releases](https://github.com/asklar/WinMIDL/releases) page

Features: real-time diagnostics, IntelliSense, go-to-definition (including into SDK types), semantic highlighting, code actions, formatting, doc comment support, class diagrams.
