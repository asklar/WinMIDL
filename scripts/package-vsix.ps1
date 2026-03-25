<#
.SYNOPSIS
    Build and package the VS Code extension VSIX with bundled server binary.

.DESCRIPTION
    Publishes Midl.Cli as NativeAOT (or self-contained fallback), then bundles
    it into the VS Code extension and packages the VSIX.

.PARAMETER SkipBuild
    Skip the CLI publish step (use existing publish/win-x64/Midl.Cli.exe)

.PARAMETER Version
    Override the extension version (default: read from package.json)

.PARAMETER OutputPath
    Output path for the VSIX (default: publish/midl-language.vsix)
#>

[CmdletBinding()]
param(
    [switch]$SkipBuild,
    [string]$Version,
    [string]$OutputPath
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

# Step 2: Bundle into extension
Write-Host ""
Write-Host "Bundling server binaries into extension..." -ForegroundColor Cyan

# Bundle x64
$x64Dir = Join-Path $extensionDir "server" "win-x64"
New-Item -ItemType Directory -Force -Path $x64Dir | Out-Null
Copy-Item $serverExe -Destination $x64Dir -Force
Write-Host "  Copied x64 to $x64Dir"

# Bundle ARM64
$arm64Exe = Join-Path $publishDir "win-arm64" "Midl.Cli.exe"
if (Test-Path $arm64Exe) {
    $arm64SizeMB = [math]::Round((Get-Item $arm64Exe).Length / 1MB, 1)
    $arm64Dir = Join-Path $extensionDir "server" "win-arm64"
    New-Item -ItemType Directory -Force -Path $arm64Dir | Out-Null
    Copy-Item $arm64Exe -Destination $arm64Dir -Force
    Write-Host "  Copied ARM64 to $arm64Dir ($arm64SizeMB MB)"
} else {
    Write-Host "  ARM64 binary not found — skipping" -ForegroundColor Yellow
}

# Step 3: Set version if specified
if ($Version) {
    Write-Host ""
    Write-Host "Setting extension version to $Version..." -ForegroundColor Cyan
    Push-Location $extensionDir
    $pkg = Get-Content package.json | ConvertFrom-Json
    $pkg.version = $Version -replace '-.*$', ''
    $pkg | ConvertTo-Json -Depth 10 | Set-Content package.json
    Pop-Location
}

# Step 4: Package VSIX
Write-Host ""
Write-Host "Packaging VSIX..." -ForegroundColor Cyan
Push-Location $extensionDir

# Ensure dependencies are installed
if (-not (Test-Path "node_modules")) {
    Write-Host "  Installing npm dependencies..."
    npm ci --silent 2>&1 | Out-Null
}

# Determine output path
if (-not $OutputPath) {
    $OutputPath = Join-Path $publishDir "midl-language.vsix"
}
New-Item -ItemType Directory -Force -Path (Split-Path $OutputPath) | Out-Null

$result = npx @vscode/vsce package -o $OutputPath 2>&1
$packExitCode = $LASTEXITCODE
Pop-Location

if ($packExitCode -ne 0) {
    Write-Error "VSIX packaging failed: $result"
    exit 1
}

# Step 5: Report
if (Test-Path $OutputPath) {
    $vsixSize = [math]::Round((Get-Item $OutputPath).Length / 1MB, 2)
    Write-Host ""
    Write-Host "=======================================" -ForegroundColor Cyan
    Write-Host "  VSIX packaged!" -ForegroundColor Green
    Write-Host "=======================================" -ForegroundColor Cyan
    Write-Host "  Output: $OutputPath"
    Write-Host "  Size:   $vsixSize MB"
    Write-Host ""
    Write-Host "Install with:" -ForegroundColor DarkGray
    Write-Host "  code --install-extension `"$OutputPath`"" -ForegroundColor DarkGray
} else {
    Write-Error "VSIX not found at expected location: $OutputPath"
    exit 1
}
