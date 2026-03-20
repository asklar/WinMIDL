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
    [switch]$SkipTests
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

# Ensure Windows SDK lib path is available for native linker
if (-not $env:LIB -or -not ($env:LIB -match "Windows Kits")) {
    try {
        $sdkRoot = (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots" -ErrorAction SilentlyContinue).KitsRoot10
        if ($sdkRoot) {
            $sdkVer = Get-ChildItem "$sdkRoot\Lib" -Directory -ErrorAction SilentlyContinue | 
                Where-Object { $_.Name -match '^\d+\.' } |
                Sort-Object Name -Descending | Select-Object -First 1
            if ($sdkVer) {
                $v = $sdkVer.Name
                $libPaths = @("$sdkRoot\Lib\$v\um\x64", "$sdkRoot\Lib\$v\ucrt\x64") -join ";"
                $env:LIB = if ($env:LIB) { "$libPaths;$env:LIB" } else { $libPaths }
                Write-Host "  SDK lib path: $libPaths" -ForegroundColor DarkGray
            }
        }
    } catch { }
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
    "/p:StripSymbols=true"
    "/p:OptimizationPreference=Speed"
    "/p:IlcInstructionSet=native"
    "/p:IlcOptimizationPreference=Speed"
    "/p:IlcFoldIdenticalMethodBodies=true"
    "/p:InvariantGlobalization=true"
    "/p:StackTraceSupport=false"
    "/p:UseSystemResourceKeys=true"
    "/p:DebugType=none"
    "/p:DebugSymbols=false"
)

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
