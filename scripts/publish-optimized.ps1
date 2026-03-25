<#
.SYNOPSIS
    Build the most optimized NativeAOT binary of WinMIDL.

.DESCRIPTION
    Publishes Midl.Cli as a NativeAOT single-file binary with full optimizations.
    Falls back to self-contained single-file if AOT fails.

.PARAMETER Configuration
    Build configuration (default: Release)

.PARAMETER Runtime
    Target runtime (default: win-x64)

.PARAMETER OutputDir
    Output directory (default: publish/)

.PARAMETER SkipTests
    Skip running tests before publishing
#>

[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputDir = "publish",
    [switch]$SkipTests,
    [switch]$Native
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path $PSScriptRoot -Parent

Write-Host "Building optimized WinMIDL binary" -ForegroundColor Cyan
Write-Host "  Configuration: $Configuration"
Write-Host "  Runtime: $Runtime"
Write-Host "  Output: $OutputDir/"
Write-Host ""

# Step 1: Run tests first (unless skipped)
if (-not $SkipTests) {
    Write-Host "Running tests..." -ForegroundColor Yellow
    Push-Location $repoRoot
    dotnet test -c $Configuration -v q 2>&1 | Select-String "Passed|Failed"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Tests failed. Fix tests before publishing."
        Pop-Location
        exit 1
    }
    Pop-Location
    Write-Host "  Tests passed." -ForegroundColor Green
    Write-Host ""
}

# Step 2: Try NativeAOT publish
$outDir = Join-Path $repoRoot $OutputDir $Runtime
$cliProject = Join-Path $repoRoot "src" "Midl.Cli"

# Determine target architecture from RID
$targetArch = if ($Runtime -match 'arm64') { 'arm64' } else { 'x64' }
$hostArch = 'x64'  # we're always building on x64

# Find a working Windows SDK version for the native linker
# Some SDK versions may be broken; probe for advapi32.lib to verify
$sdkVersion = $null
try {
    $sdkRoot = (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots" -ErrorAction SilentlyContinue).KitsRoot10
    if ($sdkRoot) {
        $sdkVersions = Get-ChildItem "$sdkRoot\Lib" -Directory -ErrorAction SilentlyContinue | 
            Where-Object { $_.Name -match '^\d+\.' } |
            Sort-Object Name -Descending
        foreach ($sv in $sdkVersions) {
            $v = $sv.Name
            $umPath = "$sdkRoot\Lib\$v\um\$targetArch"
            $ucrtPath = "$sdkRoot\Lib\$v\ucrt\$targetArch"
            if ((Test-Path "$umPath\advapi32.lib") -and (Test-Path "$ucrtPath\ucrt.lib")) {
                $sdkVersion = $v
                # Ensure both paths are in LIB
                $missing = @()
                if (-not $env:LIB -or $env:LIB -notmatch [regex]::Escape($umPath)) { $missing += $umPath }
                if (-not $env:LIB -or $env:LIB -notmatch [regex]::Escape($ucrtPath)) { $missing += $ucrtPath }
                if ($missing.Count -gt 0) {
                    $addPaths = $missing -join ";"
                    $env:LIB = if ($env:LIB) { "$addPaths;$env:LIB" } else { $addPaths }
                }
                Write-Host "  Using SDK $v ($targetArch)" -ForegroundColor DarkGray
                break
            }
        }
    }
} catch { }

# For cross-compilation, ensure the cross-compiler tools are on PATH
if ($targetArch -ne $hostArch) {
    try {
        $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
        $vsPath = & $vswhere -prerelease -latest -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
        if ($vsPath) {
            $vcVersion = (Get-ChildItem "$vsPath\VC\Tools\MSVC" -Directory | Sort-Object Name -Descending | Select-Object -First 1).Name
            $crossToolPath = "$vsPath\VC\Tools\MSVC\$vcVersion\bin\Host$hostArch\$targetArch"
            $hostToolPath = "$vsPath\VC\Tools\MSVC\$vcVersion\bin\Host$hostArch\$hostArch"
            if (Test-Path $crossToolPath) {
                $env:PATH = "$crossToolPath;$hostToolPath;$env:PATH"
                # Also add VC lib for target arch
                $vcLibPath = "$vsPath\VC\Tools\MSVC\$vcVersion\lib\$targetArch"
                if (Test-Path $vcLibPath) {
                    $env:LIB = if ($env:LIB) { "$vcLibPath;$env:LIB" } else { $vcLibPath }
                }
                Write-Host "  Cross-compile: $hostArch → $targetArch" -ForegroundColor DarkGray
            }
        }
    } catch { }
}

$instructionSet = if ($Native) { "native" } elseif ($targetArch -eq 'arm64') { "" } else { "x86-64-v2" }
if ($instructionSet) {
    if ($Native) {
        Write-Host "  Instruction set: native (optimized for this machine — NOT portable)" -ForegroundColor Yellow
    } else {
        Write-Host "  Instruction set: $instructionSet (portable)" -ForegroundColor DarkGray
    }
} else {
    Write-Host "  Instruction set: default ($targetArch)" -ForegroundColor DarkGray
}

Write-Host "Attempting NativeAOT publish..." -ForegroundColor Yellow
$aotArgs = @(
    "publish"
    $cliProject
    "-c", $Configuration
    "-r", $Runtime
    "--self-contained"
    "-o", $outDir
    "/p:PublishAot=true"
)
if ($instructionSet) {
    $aotArgs += "/p:IlcInstructionSet=$instructionSet"
}
# Pass the verified SDK um path so the linker can find advapi32.lib etc.
if ($sdkVersion -and $sdkRoot) {
    $umPath = "$sdkRoot\Lib\$sdkVersion\um\$targetArch"
    $aotArgs += "/p:LinkerArg=/LIBPATH:`"$umPath`""
}
# For cross-compilation, tell ILC to use the right tools
if ($targetArch -ne $hostArch) {
    $aotArgs += "/p:IlcUseEnvironmentalTools=true"
}

$aotResult = & dotnet @aotArgs 2>&1
$aotExitCode = $LASTEXITCODE

if ($aotExitCode -eq 0) {
    Write-Host "  NativeAOT publish succeeded!" -ForegroundColor Green
    $buildType = "NativeAOT"
} else {
    Write-Host "  NativeAOT failed, falling back to single-file self-contained..." -ForegroundColor Yellow
    
    $sfArgs = @(
        "publish"
        $cliProject
        "-c", $Configuration
        "-r", $Runtime
        "--self-contained"
        "-o", $outDir
        "/p:PublishSingleFile=true"
        "/p:DebugType=none"
        "/p:DebugSymbols=false"
        "/p:EnableCompressionInSingleFile=true"
        "/p:IncludeNativeLibrariesForSelfExtract=true"
    )
    
    & dotnet @sfArgs 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Both NativeAOT and single-file publish failed."
        exit 1
    }
    Write-Host "  Single-file publish succeeded." -ForegroundColor Green
    $buildType = "SingleFile"
}

# Step 3: Report results
$exe = Join-Path $outDir "Midl.Cli.exe"
if (-not (Test-Path $exe)) {
    $exe = Get-ChildItem $outDir -Filter "Midl.Cli*" | Select-Object -First 1 | ForEach-Object { $_.FullName }
}

if (Test-Path $exe) {
    $size = (Get-Item $exe).Length
    $sizeMB = [math]::Round($size / 1MB, 1)
    
    Write-Host ""
    Write-Host "=======================================" -ForegroundColor Cyan
    Write-Host "  Build complete!" -ForegroundColor Green
    Write-Host "=======================================" -ForegroundColor Cyan
    Write-Host "  Type:     $buildType"
    Write-Host "  Binary:   $exe"
    Write-Host "  Size:     $sizeMB MB"
    Write-Host ""
    
    # Quick smoke test
    Write-Host "Smoke test..." -ForegroundColor Yellow
    $version = & $exe /version /nologo 2>&1
    Write-Host "  Version: $version" -ForegroundColor Green
    
    # Cold start timing
    $cold = Measure-Command { & $exe /? /nologo 2>&1 | Out-Null }
    Write-Host "  Cold start: $([math]::Round($cold.TotalMilliseconds))ms" -ForegroundColor Green
    
    # Compile timing
    $testIdl = Join-Path $repoRoot "tests" "fixtures" "valid" "full_runtimeclass.idl"
    if (Test-Path $testIdl) {
        $warm = Measure-Command { & $exe /Zs /nologo $testIdl 2>&1 | Out-Null }
        Write-Host "  Compile (full_runtimeclass.idl): $([math]::Round($warm.TotalMilliseconds))ms" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "To benchmark against original midl.exe:" -ForegroundColor DarkGray
    Write-Host "  .\scripts\benchmark.ps1 -WinMidlPath `"$exe`"" -ForegroundColor DarkGray
} else {
    Write-Error "Binary not found at expected location."
    exit 1
}
