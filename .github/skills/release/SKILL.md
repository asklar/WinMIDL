---
name: 'release'
description: 'Automate a WinMIDL release: build, test, changelog, tag, package VS Code + VS extensions with NativeAOT binaries. Use when the user wants to cut a new release.'
---

# Release Skill

Automates the full WinMIDL release pipeline. When invoked, performs all steps sequentially and stops on any failure.

## Prerequisites
- Must be run from the repo root (`E:\midl` or equivalent)
- `dotnet` must be on PATH
- `pwsh` (PowerShell 7+) must be available for packaging scripts
- For VS extension: MSBuild must be available (via Visual Studio)

## Steps

### Step 1: Determine Version

```powershell
$latestTag = git describe --tags --abbrev=0 2>$null
# Parse version: v0.1.7 → major=0, minor=1, patch=7
# Bump patch: → 0.1.8
# Allow user to override if they specify a version
```

If the user specified a version, use that. Otherwise bump the patch number from the latest tag. Confirm the version with the user before proceeding.

### Step 2: Build & Test

Run all non-E2E tests:

```powershell
dotnet test --filter "Category!=E2E" -v q
```

**STOP if any tests fail.** Report the failure and do not proceed.

### Step 3: Generate Changelog

Get commits since the last tag:

```powershell
git log --oneline $latestTag..HEAD
```

Categorize each commit by its prefix or content:
- **Features**: commits containing "Add", "Implement", "feat", "new"
- **Bug Fixes**: commits containing "Fix", "fix", "bug"
- **Tests**: commits containing "test", "Test", "Add.*test"
- **Documentation**: commits containing "doc", "README", "TODO"
- **Other**: everything else

Write to `CHANGELOG.md` at the repo root. If the file exists, prepend the new version section. If not, create it. Format:

```markdown
## v0.1.8 (2026-03-30)

### Features
- Add XDC doc comments support (#commit-summary)

### Bug Fixes  
- Fix sourcemap positions for #define directives

### Tests
- Add comprehensive diagnostic position tests
```

After writing the changelog, **show the diff to the user for review** using the `show_file` tool with `diff: true` on `CHANGELOG.md`. Wait for the user to confirm before proceeding. If they request changes, edit accordingly and show the diff again.

### Step 4: Bump Version & Sync Changelog

Update version strings in two files:

1. **`editors/vscode/package.json`**: Change `"version": "X.Y.Z"` to the new version
2. **`editors/vs/Midl.VsExtension/source.extension.vsixmanifest`**: Change `Version="X.Y.Z"` in the `<Identity>` element

Copy the root `CHANGELOG.md` into `editors/vscode/CHANGELOG.md` so the VS Code marketplace shows the real changelog:

```powershell
Copy-Item CHANGELOG.md editors/vscode/CHANGELOG.md -Force
```

### Step 5: Commit & Tag

```powershell
git add CHANGELOG.md editors/vscode/CHANGELOG.md editors/vscode/package.json editors/vs/Midl.VsExtension/source.extension.vsixmanifest
git commit -m "Release v$version

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
git tag "v$version"
```

### Step 6: Push

```powershell
git push origin main --tags
```

### Step 7: Package Extensions

Run the existing packaging scripts:

```powershell
# VS Code extension — platform-specific VSIXes (NativeAOT, one binary per VSIX)
pwsh -ExecutionPolicy Bypass -File scripts/package-vsix.ps1 -Version $version -Target "win32-x64,win32-arm64"

# VS extension (NativeAOT, bundled into VSIX)  
pwsh -ExecutionPolicy Bypass -File scripts/package-vs-vsix.ps1 -Version $version
```

These scripts:
- Build NativeAOT binaries for x64 and ARM64
- Package **separate platform-specific VSIXes** for VS Code (one per target)
- Package the VS VSIX
- Output to `publish/midl-language-win32-x64.vsix`, `publish/midl-language-win32-arm64.vsix`, and `publish/Midl.VsExtension.vsix`

### Step 7b: Package CLI Distribution Zip

Create a zip containing the CLI binaries and guide for standalone distribution:

```powershell
# Create staging directory
$staging = "publish/winmidl-$version"
New-Item -ItemType Directory -Force $staging/x64, $staging/arm64 | Out-Null

# Copy binaries
Copy-Item publish/win-x64/Midl.Cli.exe $staging/x64/
Copy-Item publish/win-arm64/Midl.Cli.exe $staging/arm64/

# Copy CLI guide
Copy-Item docs/cli-guide.md $staging/

# Create zip
Compress-Archive -Path $staging/* -DestinationPath "publish/winmidl-$version.zip" -Force
Remove-Item -Recurse $staging
```

Output: `publish/winmidl-0.1.13.zip` containing:
```
x64/Midl.Cli.exe
arm64/Midl.Cli.exe
cli-guide.md
```

Note: Runtime headers (`proxy_helpers.h`, `wil_compat.h`) are embedded in the compiler binary and extracted automatically to the output directory when generating proxy code.

### Step 7c: Package and Publish NuGet Tool

Pack and push the dotnet tool package to nuget.org. This enables `dotnet tool install -g WinMIDL` and `dnx winmidl`.

```powershell
dotnet pack src/Midl.Cli/Midl.Cli.csproj -c Release -p:Version=$version -o publish/nuget --nologo -v q
dotnet nuget push "publish/nuget/WinMIDL.$version.nupkg" --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

The NuGet API key should be set in the `NUGET_API_KEY` environment variable. If not set, prompt the user for it. The key can be created at https://www.nuget.org/account/apikeys (scope: "Push new packages and package versions", package: "WinMIDL").

### Step 8: Report

Print the results:

```
✅ Release v0.1.14 complete!

Artifacts:
  NuGet:           publish/nuget/WinMIDL.0.1.14.nupkg (X KB) → nuget.org
  CLI zip:         publish/winmidl-0.1.14.zip (X.X MB)
  VS Code (x64):   publish/midl-language-win32-x64.vsix (X.X MB)
  VS Code (arm64): publish/midl-language-win32-arm64.vsix (X.X MB)
  VS:              publish/Midl.VsExtension.vsix (X.X MB)

Install:
  dnx winmidl /winrt myfile.idl                    # one-shot (no install)
  dotnet tool install -g WinMIDL                    # permanent global install

Upload manually:
  VS Code: https://marketplace.visualstudio.com/manage/publishers/AlexanderSklarMSFT
  VS:      https://marketplace.visualstudio.com/manage/publishers/AlexanderSklarMSFT
```

## Error Handling

- If tests fail → stop, report which tests failed
- If NativeAOT build fails → stop, report the error
- If packaging fails → stop, report the error
- If git push fails → report (tag was created locally, can retry)

## Notes

- The version bump commit includes the changelog — it's part of the release commit
- The git tag points to the release commit
- NativeAOT binaries are ~5.5 MB each (x64 + ARM64)
- VS Code VSIXes are ~3 MB each (one binary per platform VSIX)
- VS VSIX is ~4.5 MB
- NuGet tool package is ~600 KB (framework-dependent, requires .NET 10 runtime)
- Platform-specific VSIXes use `vsce package --target win32-x64` etc.
- The marketplace merges platform VSIXes into one listing; Mac/Linux users won't see the extension unless we add those targets
- The `Co-authored-by: Copilot` trailer is always included in the commit
