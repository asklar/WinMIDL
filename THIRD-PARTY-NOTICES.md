# Third Party Notices

This project may include third-party software or depend on third-party software during build, test, packaging, and editor-extension development. Microsoft is not the original author of the third-party software listed below. The original copyright notices and licenses for these components are available from their respective package sources.

The distributed WinMIDL command-line binary is a NativeAOT .NET application and does not bundle third-party components beyond the .NET runtime and libraries provided by Microsoft. `System.Reflection.Metadata` is provided as part of .NET for this project.

## Build, test, packaging, and extension development dependencies

The following direct package dependencies were found in project files at the time this notice was prepared.

### NuGet packages

| Package | Version(s) | Usage |
|---|---:|---|
| coverlet.collector | 6.0.0, 6.0.4 | Test coverage collection |
| Microsoft.NET.Test.Sdk | 17.8.0, 17.14.1 | .NET test infrastructure |
| Microsoft.VisualStudio.LanguageServer.Client | 17.14.60 | Visual Studio extension language client |
| Microsoft.VisualStudio.SDK | 17.14.40265 | Visual Studio extension SDK |
| Microsoft.VSSDK.BuildTools | 17.14.2120 | Visual Studio extension build tooling |
| StreamJsonRpc | 2.22.11 | Visual Studio language client dependency |
| Verify.Xunit | * | Snapshot verification tests |
| xunit | 2.5.3, 2.9.3 | Test framework |
| Xunit.SkippableFact | 1.* | Conditional xUnit tests |
| xunit.runner.visualstudio | 2.5.3, 3.1.4 | Visual Studio/xUnit test runner |

### npm packages

Direct dependencies for the VS Code extension in `editors/vscode/package.json`:

| Package | Version | Usage |
|---|---:|---|
| vscode-languageclient | ^9.0.1 | VS Code Language Server Protocol client |

Direct development dependencies for the VS Code extension:

| Package | Version | Usage |
|---|---:|---|
| @types/node | ^25.5.0 | Node.js TypeScript type definitions |
| @types/vscode | ^1.85.0 | VS Code extension API TypeScript definitions |
| esbuild | ^0.27.4 | VS Code extension bundling |
| typescript | ^5.3.0 | TypeScript compiler |
