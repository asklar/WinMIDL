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

# Step 1: Build the server binary (unless skipped)
if (-not $SkipBuild) {
    Write-Host "Building server binary..." -ForegroundColor Cyan
    & "$PSScriptRoot\publish-optimized.ps1" -SkipTests
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Server binary build failed."
        exit 1
    }
    Write-Host ""
}

# Verify binary exists
$rid = "win-x64"
$serverExe = Join-Path $publishDir $rid "Midl.Cli.exe"
if (-not (Test-Path $serverExe)) {
    Write-Error "Server binary not found at $serverExe. Run without -SkipBuild or build manually first."
    exit 1
}
$sizeMB = [math]::Round((Get-Item $serverExe).Length / 1MB, 1)
Write-Host "Server binary: $serverExe ($sizeMB MB)" -ForegroundColor Green

# Step 2: Bundle into extension
Write-Host ""
Write-Host "Bundling server binary into extension..." -ForegroundColor Cyan
$serverDir = Join-Path $extensionDir "server" $rid
New-Item -ItemType Directory -Force -Path $serverDir | Out-Null
Copy-Item $serverExe -Destination $serverDir -Force
Write-Host "  Copied to $serverDir"

# Also bundle ARM64 if available
$arm64Exe = Join-Path $publishDir "win-arm64" "Midl.Cli.exe"
if (Test-Path $arm64Exe) {
    $arm64Dir = Join-Path $extensionDir "server" "win-arm64"
    New-Item -ItemType Directory -Force -Path $arm64Dir | Out-Null
    Copy-Item $arm64Exe -Destination $arm64Dir -Force
    Write-Host "  Copied ARM64 to $arm64Dir"
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
