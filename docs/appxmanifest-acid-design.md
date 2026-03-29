# Design: AppxManifest ACID Generation

## Goal

Generate `ActivatableClassId` (ACID) entries for AppxManifest.xml from the compilation's runtimeclass symbols, enabling packaged WinRT component registration without manual manifest authoring.

## Background

Packaged WinRT components register their activatable classes in the AppxManifest:

```xml
<Extensions>
  <Extension Category="windows.activatableClass.inProcessServer">
    <InProcessServer>
      <Path>MyComponent.dll</Path>
      <ActivatableClass ActivatableClassId="MyNamespace.MyClass"
                        ThreadingModel="both" />
    </InProcessServer>
  </Extension>
</Extensions>
```

Currently, developers must author these entries manually or rely on build system tooling (e.g., MSBuild targets in C++/WinRT). WinMIDL can generate these directly from the IDL, since it already knows which runtimeclasses are activatable and what threading model they use.

## CLI Interface

```bash
# In-proc component
Midl.Cli /winrt /manifest manifest.xml /server_dll MyComponent.dll input.idl

# Out-of-proc server
Midl.Cli /winrt /manifest manifest.xml /server_exe MyServer.exe input.idl
```

### New switches

| Switch | Description |
|--------|-------------|
| `/manifest <file>` | Emit AppxManifest ACID fragment |
| `/server_dll <name>` | Server binary name for in-proc registration |
| `/server_exe <name>` | Server binary name for out-of-proc registration |

If neither `/server_dll` nor `/server_exe` is specified, infer from the input filename (e.g., `Widget.idl` → `Widget.dll`).

## Output Format

### In-proc fragment
```xml
<Extensions>
  <Extension Category="windows.activatableClass.inProcessServer">
    <InProcessServer>
      <Path>MyComponent.dll</Path>
      <ActivatableClass ActivatableClassId="MyNamespace.Widget"
                        ThreadingModel="both" />
      <ActivatableClass ActivatableClassId="MyNamespace.Helper"
                        ThreadingModel="sta" />
    </InProcessServer>
  </Extension>
</Extensions>
```

### Out-of-proc fragment
```xml
<Extensions>
  <Extension Category="windows.activatableClass.outOfProcessServer">
    <OutOfProcessServer ServerName="MyServer">
      <Path>MyServer.exe</Path>
      <ActivatableClass ActivatableClassId="MyNamespace.Widget" />
    </OutOfProcessServer>
  </Extension>
</Extensions>
```

## Design Decisions

### Which classes to include
Only runtimeclasses that are **activatable** — i.e., those with:
- At least one constructor, OR
- An explicit `[activatable]` attribute, OR
- An explicit `[composable]` attribute

Static-only classes and non-activatable classes are excluded.

### Threading model
Derived from the `[threading]` attribute on the runtimeclass:
- `[threading(sta)]` → `ThreadingModel="STA"`
- `[threading(mta)]` → `ThreadingModel="MTA"`
- `[threading(both)]` → `ThreadingModel="both"`
- No attribute → `ThreadingModel="both"` (WinRT default)

Note: out-of-proc servers don't specify `ThreadingModel` in the manifest.

### Namespace handling
The `ActivatableClassId` is the fully-qualified runtimeclass name (e.g., `MyApp.Controls.Widget`).

### Multiple IDL files
When compiling multiple inputs (via `winmidl.json`), all activatable classes from all inputs are collected into a single manifest fragment.

## Implementation

### 1. CommandLineParser (src/Midl.Cli/CommandLineParser.cs)
Add `ManifestFile`, `ServerDll`, `ServerExe` properties and parse the new switches.

### 2. ManifestEmitter (src/Midl.CodeGen/Manifest/ManifestEmitter.cs)
New emitter class:
```csharp
public static class ManifestEmitter
{
    public static string EmitFragment(
        Compilation compilation, 
        string serverPath, 
        bool isOutOfProc = false)
    {
        // Walk compilation.GetAllTypes()
        // Filter to activatable RuntimeClassSymbols
        // Emit XML with correct threading model
    }
}
```

### 3. CompilationDriver (src/Midl.Cli/CompilationDriver.cs)
Call `ManifestEmitter.EmitFragment()` when `/manifest` is specified, write to output file.

### 4. winmidl.json support
Add optional `manifest` field:
```jsonc
{
    "inputs": ["Widget.idl"],
    "manifest": "AppxManifest.fragment.xml",
    "serverDll": "MyComponent.dll"
}
```

## Tests

### Unit tests (Midl.CodeGen.Tests)
- `ManifestEmitter_SingleClass_EmitsCorrectXml`
- `ManifestEmitter_MultipleClasses_EmitsAllEntries`
- `ManifestEmitter_ThreadingModel_Sta`
- `ManifestEmitter_ThreadingModel_Both_Default`
- `ManifestEmitter_StaticClass_Excluded`
- `ManifestEmitter_OutOfProc_NoThreadingModel`
- `ManifestEmitter_ComposableClass_Included`

### Integration tests (Midl.Integration.Tests)
- `CliSwitch_Manifest_GeneratesFile`
- `CliSwitch_ServerDll_AppearsInPath`
- `CliSwitch_ManifestWithoutServerDll_InfersFromInput`

## Future Extensions

- Full manifest mode (generate complete AppxManifest.xml, not just fragment)
- Merge mode (patch existing AppxManifest.xml with new entries)
- MSBuild target integration (auto-generate manifest during build)
- Support for `ProxyStub` registration entries alongside ACID entries
