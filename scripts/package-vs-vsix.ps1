<#
.SYNOPSIS
    Build and package the Visual Studio extension VSIX with bundled NativeAOT server binary.

.DESCRIPTION
    Publishes Midl.Cli as NativeAOT, copies it into the VS extension project,
    then builds the VSIX using MSBuild.

.PARAMETER SkipBuild
    Skip the CLI publish step (use existing publish/win-x64/Midl.Cli.exe)

.PARAMETER Version
    Override the extension version (default: inferred from latest git tag)

.PARAMETER Configuration
    Build configuration (default: Release)

.PARAMETER OutputPath
    Output path for the VSIX (default: publish/Midl.VsExtension.vsix)
#>

[CmdletBinding()]
param(
    [switch]$SkipBuild,
    [string]$Version,
    [string]$Configuration = "Release",
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path $PSScriptRoot -Parent
$vsExtDir = Join-Path $repoRoot "editors" "vs" "Midl.VsExtension"
$publishDir = Join-Path $repoRoot "publish"

# Also check the main repo publish directory (worktrees share it)
$mainRepoPublish = "E:\midl\publish"
if ((Test-Path (Join-Path $mainRepoPublish "win-x64" "Midl.Cli.exe")) -and
    -not (Test-Path (Join-Path $publishDir "win-x64" "Midl.Cli.exe"))) {
    $publishDir = $mainRepoPublish
}

# Find MSBuild
$msbuild = & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -requires Microsoft.Component.MSBuild `
    -find "MSBuild\**\Bin\MSBuild.exe" 2>$null | Select-Object -First 1

if (-not $msbuild) {
    Write-Error "MSBuild not found. Install Visual Studio with the 'Desktop development with C++' or '.NET desktop development' workload."
    exit 1
}
Write-Host "MSBuild: $msbuild" -ForegroundColor DarkGray

# Step 1: Build NativeAOT server binary
if (-not $SkipBuild) {
    Write-Host "Building NativeAOT server binary..." -ForegroundColor Cyan
    & "$PSScriptRoot\publish-optimized.ps1" -SkipTests -Runtime win-x64
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Server build failed."
        exit 1
    }
}

$serverExe = Join-Path $publishDir "win-x64" "Midl.Cli.exe"
if (-not (Test-Path $serverExe)) {
    Write-Error "Server binary not found at $serverExe. Run without -SkipBuild."
    exit 1
}
$sizeMB = [math]::Round((Get-Item $serverExe).Length / 1MB, 1)
Write-Host "Server binary: $serverExe ($sizeMB MB)" -ForegroundColor Green

# Step 2: Copy server binaries into VS extension project
Write-Host ""
Write-Host "Bundling server binaries..." -ForegroundColor Cyan

# x64
$serverDir = Join-Path $vsExtDir "server" "win-x64"
New-Item -ItemType Directory -Force -Path $serverDir | Out-Null
Copy-Item $serverExe -Destination $serverDir -Force
Write-Host "  x64: $sizeMB MB"

# ARM64
$arm64Exe = Join-Path $publishDir "win-arm64" "Midl.Cli.exe"
if (Test-Path $arm64Exe) {
    $arm64Size = [math]::Round((Get-Item $arm64Exe).Length / 1MB, 1)
    $arm64Dir = Join-Path $vsExtDir "server" "win-arm64"
    New-Item -ItemType Directory -Force -Path $arm64Dir | Out-Null
    Copy-Item $arm64Exe -Destination $arm64Dir -Force
    Write-Host "  ARM64: $arm64Size MB"
} else {
    Write-Host "  ARM64 binary not found — x64 only" -ForegroundColor Yellow
}

# Step 3: Infer version from git tag if not specified
if (-not $Version) {
    $latestTag = git describe --tags --abbrev=0 2>$null
    if ($latestTag -match '^v?(\d+\.\d+\.\d+)') {
        $Version = $Matches[1]
        Write-Host "Inferred version $Version from git tag $latestTag" -ForegroundColor Cyan
    } else {
        $Version = "0.1.0"
        Write-Host "No git tag found, using default version $Version" -ForegroundColor Yellow
    }
}

# Step 4: Update version in manifest (only the Identity element)
$manifestPath = Join-Path $vsExtDir "source.extension.vsixmanifest"
if (Test-Path $manifestPath) {
    $manifest = Get-Content $manifestPath -Raw
    $manifest = $manifest -replace '(<Identity[^>]*Version=")[^"]*(")', "`${1}$Version`${2}"
    Set-Content $manifestPath $manifest -NoNewline
    Write-Host "  Version set to $Version"
}

# Step 5: Build VSIX with MSBuild (clean first to pick up manifest changes)
Write-Host ""
Write-Host "Building VS extension ($Configuration)..." -ForegroundColor Cyan
Push-Location $vsExtDir
& $msbuild /restore /t:Clean /p:Configuration=$Configuration /v:quiet 2>&1 | Out-Null
& $msbuild /restore /t:Build /p:Configuration=$Configuration /v:minimal 2>&1 | ForEach-Object { Write-Host "  $_" }
$buildExitCode = $LASTEXITCODE
Pop-Location

if ($buildExitCode -ne 0) {
    Write-Error "VS extension build failed."
    exit 1
}

# Step 5: Copy VSIX to output
$builtVsix = Join-Path $vsExtDir "bin" $Configuration "Midl.VsExtension.vsix"
if (-not (Test-Path $builtVsix)) {
    Write-Error "VSIX not found at $builtVsix"
    exit 1
}

if (-not $OutputPath) {
    $OutputPath = Join-Path $publishDir "Midl.VsExtension.vsix"
}
New-Item -ItemType Directory -Force -Path (Split-Path $OutputPath) | Out-Null
Copy-Item $builtVsix -Destination $OutputPath -Force

# Step 6: Report
$vsixSize = [math]::Round((Get-Item $OutputPath).Length / 1MB, 1)
Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  VS Extension packaged!" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  Output: $OutputPath"
Write-Host "  Size:   $vsixSize MB"
Write-Host "  Server: NativeAOT ($sizeMB MB)"
Write-Host ""
Write-Host "Install:" -ForegroundColor DarkGray
Write-Host "  Double-click the .vsix, or:" -ForegroundColor DarkGray
Write-Host "  VSIXInstaller.exe `"$OutputPath`"" -ForegroundColor DarkGray
