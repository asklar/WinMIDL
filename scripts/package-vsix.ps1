<#
.SYNOPSIS
    Build and package the VS Code extension VSIX with bundled server binary.

.DESCRIPTION
    Publishes Midl.Cli as NativeAOT (or self-contained fallback), then bundles
    it into the VS Code extension and packages platform-specific VSIXes.

.PARAMETER SkipBuild
    Skip the CLI publish step (use existing publish/win-x64/Midl.Cli.exe)

.PARAMETER Version
    Override the extension version (default: read from package.json)

.PARAMETER OutputPath
    Output directory for the VSIXes (default: publish/)

.PARAMETER Target
    Platform targets to build. Comma-separated. Default: win32-x64,win32-arm64
    Valid: win32-x64, win32-arm64, universal (legacy single VSIX with both binaries)
#>

[CmdletBinding()]
param(
    [switch]$SkipBuild,
    [string]$Version,
    [string]$OutputPath,
    [string]$Target = "win32-x64,win32-arm64"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path $PSScriptRoot -Parent
$extensionDir = Join-Path $repoRoot "editors" "vscode"
$publishDir = Join-Path $repoRoot "publish"

# Step 1: Build server binaries (unless skipped)
if (-not $SkipBuild) {
    Write-Host "Building server binaries..." -ForegroundColor Cyan

    # Build x64
    Write-Host "  Building win-x64..." -ForegroundColor Yellow
    & "$PSScriptRoot\publish-optimized.ps1" -SkipTests -Runtime win-x64
    if ($LASTEXITCODE -ne 0) {
        Write-Error "x64 build failed."
        exit 1
    }

    # Cross-compile ARM64
    Write-Host "  Building win-arm64 (cross-compile)..." -ForegroundColor Yellow
    & "$PSScriptRoot\publish-optimized.ps1" -SkipTests -Runtime win-arm64
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ARM64 build failed — VSIX will be x64-only" -ForegroundColor Yellow
    }
    Write-Host ""
}

# Verify x64 binary exists
$serverExe = Join-Path $publishDir "win-x64" "Midl.Cli.exe"
if (-not (Test-Path $serverExe)) {
    Write-Error "Server binary not found at $serverExe. Run without -SkipBuild or build manually first."
    exit 1
}
$sizeMB = [math]::Round((Get-Item $serverExe).Length / 1MB, 1)
Write-Host "Server binary (x64): $serverExe ($sizeMB MB)" -ForegroundColor Green

# Step 3: Set version — from parameter, git tag, or package.json
if (-not $Version) {
    # Infer from latest git tag (e.g. v0.1.5 → 0.1.5)
    $latestTag = git describe --tags --abbrev=0 2>$null
    if ($latestTag -match '^v?(\d+\.\d+\.\d+)') {
        $Version = $Matches[1]
        Write-Host "Inferred version $Version from git tag $latestTag" -ForegroundColor Cyan
    }
}
if ($Version) {
    Write-Host ""
    Write-Host "Setting extension version to $Version..." -ForegroundColor Cyan
    Push-Location $extensionDir
    $pkg = Get-Content package.json | ConvertFrom-Json
    $pkg.version = $Version -replace '-.*$', ''
    $pkg | ConvertTo-Json -Depth 10 | Set-Content package.json
    Pop-Location

    # Sync changelog from repo root into extension directory
    Copy-Item "$repoRoot\CHANGELOG.md" "$extensionDir\CHANGELOG.md" -Force
}

# Step 4: Package VSIX(es)
Write-Host ""
Write-Host "Packaging VSIX(es)..." -ForegroundColor Cyan
Push-Location $extensionDir

# Ensure dependencies are installed
if (-not (Test-Path "node_modules")) {
    Write-Host "  Installing npm dependencies..."
    npm ci --silent 2>&1 | Out-Null
}

# Determine output directory
if (-not $OutputPath) {
    $OutputPath = $publishDir
}
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

$targets = $Target -split ','
$vsixFiles = @()

foreach ($t in $targets) {
    $t = $t.Trim()

    if ($t -eq "universal") {
        # Legacy: single VSIX with both binaries (no --target flag)
        # Bundle both architectures
        $x64Dir = Join-Path $extensionDir "server" "win-x64"
        New-Item -ItemType Directory -Force -Path $x64Dir | Out-Null
        Copy-Item (Join-Path $publishDir "win-x64" "Midl.Cli.exe") -Destination $x64Dir -Force
        $arm64Exe = Join-Path $publishDir "win-arm64" "Midl.Cli.exe"
        if (Test-Path $arm64Exe) {
            $arm64Dir = Join-Path $extensionDir "server" "win-arm64"
            New-Item -ItemType Directory -Force -Path $arm64Dir | Out-Null
            Copy-Item $arm64Exe -Destination $arm64Dir -Force
        }
        $vsixPath = Join-Path $OutputPath "midl-language.vsix"
        $result = npx @vscode/vsce package -o $vsixPath 2>&1
    } else {
        # Platform-specific VSIX: bundle only the matching binary
        # Map vsce target to NativeAOT RID
        $rid = switch ($t) {
            "win32-x64"    { "win-x64" }
            "win32-arm64"  { "win-arm64" }
            "darwin-x64"   { "osx-x64" }
            "darwin-arm64" { "osx-arm64" }
            "linux-x64"    { "linux-x64" }
            "linux-arm64"  { "linux-arm64" }
            default        { Write-Warning "Unknown target: $t"; continue }
        }
        $isWinTarget = $t.StartsWith("win32")
        $exeName = if ($isWinTarget) { "Midl.Cli.exe" } else { "Midl.Cli" }
        $srcExe = Join-Path $publishDir $rid $exeName

        if (-not (Test-Path $srcExe)) {
            Write-Warning "  Binary not found for $t at $srcExe — skipping"
            continue
        }

        # Clean server dir, bundle only this platform's binary
        $serverDir = Join-Path $extensionDir "server"
        if (Test-Path $serverDir) { Remove-Item $serverDir -Recurse -Force }
        $targetDir = Join-Path $serverDir $rid
        New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
        Copy-Item $srcExe -Destination $targetDir -Force

        $vsixPath = Join-Path $OutputPath "midl-language-$t.vsix"
        $result = npx @vscode/vsce package --target $t -o $vsixPath 2>&1
    }

    $packExitCode = $LASTEXITCODE
    if ($packExitCode -ne 0) {
        Write-Error "VSIX packaging failed for $t`: $result"
        Pop-Location
        exit 1
    }

    if (Test-Path $vsixPath) {
        $vsixSize = [math]::Round((Get-Item $vsixPath).Length / 1MB, 2)
        $vsixFiles += @{ Target = $t; Path = $vsixPath; Size = $vsixSize }
        Write-Host "  ✅ $t → $vsixPath ($vsixSize MB)" -ForegroundColor Green
    }
}

Pop-Location

# Clean up bundled server dir
$serverDir = Join-Path $extensionDir "server"
if (Test-Path $serverDir) { Remove-Item $serverDir -Recurse -Force }

# Step 5: Report
Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  VSIX packaging complete!" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Cyan
foreach ($v in $vsixFiles) {
    Write-Host "  $($v.Target): $($v.Path) ($($v.Size) MB)"
}
Write-Host ""
Write-Host "Install with:" -ForegroundColor DarkGray
foreach ($v in $vsixFiles) {
    Write-Host "  code --install-extension `"$($v.Path)`"" -ForegroundColor DarkGray
}
