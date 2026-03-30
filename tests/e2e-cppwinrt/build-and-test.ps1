<#
.SYNOPSIS
    Build and run the C++/WinRT E2E test — validates WinMIDL .winmd output
    works with the cppwinrt toolchain.

.DESCRIPTION
    Pipeline: IDL -> WinMIDL -> .winmd -> cppwinrt -> projection headers -> compile -> run

    1. Compiles IDL to .winmd with WinMIDL
    2. Runs cppwinrt.exe to generate C++/WinRT projection headers
    3. Compiles a C++ test client against the projected headers
    4. Runs the test client
    5. Reports results

    Exit code 0 = pass, non-zero = failure.
#>
param(
    [switch]$Verbose,
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$scriptDir = $PSScriptRoot
$repoRoot = Resolve-Path "$scriptDir/../.."
$outDir = "$scriptDir/generated"
$buildDir = "$scriptDir/build"

$passCount = 0
$failCount = 0
$skipCount = 0

function Write-Step($msg) { Write-Host "`n>> $msg" -ForegroundColor Cyan }

function Check($condition, $name) {
    if ($condition) {
        Write-Host "  PASS: $name" -ForegroundColor Green
        $script:passCount++
    } else {
        Write-Host "  FAIL: $name" -ForegroundColor Red
        $script:failCount++
    }
}

# ── Step 1: Discover tools ──────────────────────────────────────────────
Write-Step "Discovering tools"

# Find cppwinrt.exe from Windows SDK
$sdkRoot = $null
$sdkVersion = $null
$sdkRegPath = "HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots"
if (Test-Path $sdkRegPath) {
    $sdkRoot = (Get-ItemProperty $sdkRegPath -Name KitsRoot10 -ErrorAction SilentlyContinue).KitsRoot10
    if ($sdkRoot) {
        $sdkVersions = Get-ChildItem "$sdkRoot\bin" -Directory |
            Where-Object { $_.Name -match '^\d+\.' } | Sort-Object Name -Descending
        if ($sdkVersions) { $sdkVersion = $sdkVersions[0].Name }
    }
}
if (-not $sdkRoot -or -not $sdkVersion) {
    Write-Host "SKIP: Windows SDK not found" -ForegroundColor Yellow
    exit 0
}

$cppwinrt = "$sdkRoot\bin\$sdkVersion\x64\cppwinrt.exe"
if (-not (Test-Path $cppwinrt)) {
    Write-Host "SKIP: cppwinrt.exe not found at $cppwinrt" -ForegroundColor Yellow
    exit 0
}

# Find cl.exe — prefer PATH, fall back to vswhere
$clPath = Get-Command cl.exe -ErrorAction SilentlyContinue
if ($clPath) {
    $cl = $clPath.Source
    Write-Host "  cl.exe: $cl (from PATH)"
} else {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (-not (Test-Path $vswhere)) {
        $vswhere = "${env:ProgramFiles}\Microsoft Visual Studio\Installer\vswhere.exe"
    }
    if (-not (Test-Path $vswhere)) {
        Write-Host "SKIP: cl.exe not on PATH and vswhere not found" -ForegroundColor Yellow
        exit 0
    }
    $vsPath = & $vswhere -latest -property installationPath -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64
    if (-not $vsPath) {
        Write-Host "SKIP: No Visual Studio with C++ tools found" -ForegroundColor Yellow
        exit 0
    }
    $vcToolsVersion = (Get-Content "$vsPath\VC\Auxiliary\Build\Microsoft.VCToolsVersion.default.txt").Trim()
    $vcToolsDir = "$vsPath\VC\Tools\MSVC\$vcToolsVersion"
    $cl = "$vcToolsDir\bin\Hostx64\x64\cl.exe"
    if (-not (Test-Path $cl)) {
        Write-Host "SKIP: cl.exe not found at $cl" -ForegroundColor Yellow
        exit 0
    }
    Write-Host "  cl.exe: $cl (via vswhere)"
}

# SDK include/lib paths
$sdkIncVersions = Get-ChildItem "$sdkRoot\Include" -Directory |
    Where-Object { $_.Name -match '^\d+\.' } | Sort-Object Name -Descending
$sdkIncVersion = $sdkIncVersions[0].Name

$cppwinrtInc = "$sdkRoot\Include\$sdkIncVersion\cppwinrt"
$sdkLibUm = "$sdkRoot\Lib\$sdkIncVersion\um\x64"
$unionMetadata = "$sdkRoot\UnionMetadata\$sdkVersion"

Write-Host "  cppwinrt: $cppwinrt"
Write-Host "  SDK: $sdkVersion"
Write-Host "  CppWinRT headers: $cppwinrtInc"

# ── Step 2: Generate .winmd from IDL ────────────────────────────────────
Write-Step "Compiling IDL -> .winmd with WinMIDL"

if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }
if (Test-Path $buildDir) { Remove-Item $buildDir -Recurse -Force }
New-Item -ItemType Directory -Path $outDir -Force | Out-Null
New-Item -ItemType Directory -Path $buildDir -Force | Out-Null

$cliProject = "$repoRoot/src/Midl.Cli"
$idlFile = "$scriptDir/TestCppWinRT.idl"

if (-not $SkipBuild) {
    & dotnet build $cliProject -v q --nologo 2>&1 | ForEach-Object { if ($Verbose) { Write-Host "  $_" } }
    if ($LASTEXITCODE -ne 0) {
        Write-Host "FAIL: dotnet build failed" -ForegroundColor Red
        exit 1
    }
}

& dotnet run --no-build --project $cliProject -- `
    /winrt `
    /winmd TestCppWinRT.winmd `
    /h TestCppWinRT.h `
    /iid TestCppWinRT_i.c `
    /out $outDir `
    /nologo `
    $idlFile 2>&1 | ForEach-Object { if ($Verbose) { Write-Host "  $_" } }

if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne 1) {
    Write-Host "FAIL: WinMIDL CLI failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit 1
}

Check (Test-Path "$outDir/TestCppWinRT.winmd") "WinMD generated"
Check (Test-Path "$outDir/TestCppWinRT.h") "Header generated"

# ── Step 3: Generate C++/WinRT projection ───────────────────────────────
Write-Step "Running cppwinrt.exe -> projection headers"

$projDir = "$outDir/projection"
New-Item -ItemType Directory -Path $projDir -Force | Out-Null

$cppwinrtOutput = & $cppwinrt -in "$outDir/TestCppWinRT.winmd" -ref $unionMetadata -out $projDir 2>&1
$cppwinrtExitCode = $LASTEXITCODE

if ($Verbose -and $cppwinrtOutput) {
    $cppwinrtOutput | ForEach-Object { Write-Host "  $_" }
}

Check ($cppwinrtExitCode -eq 0) "cppwinrt succeeded"
Check (Test-Path "$projDir/winrt/TestCppWinRT.h") "Projection header generated"

if ($cppwinrtExitCode -ne 0) {
    Write-Host "FAIL: cppwinrt failed - cannot continue" -ForegroundColor Red
    Write-Host $cppwinrtOutput
    exit 1
}

# ── Step 4: Compile C++ test client ─────────────────────────────────────
Write-Step "Compiling TestClient.cpp"

$testClientSrc = "$scriptDir/TestClient.cpp"
$testClientExe = "$buildDir/TestClient.exe"

$clOutput = & $cl /nologo /std:c++17 /EHsc /MD /W3 `
    /DWIN32 /D_WINDOWS /DUNICODE /D_UNICODE `
    /I"$projDir" /I"$cppwinrtInc" `
    /Fe"$testClientExe" `
    /Fo"$buildDir/TestClient.obj" `
    $testClientSrc `
    /link /LIBPATH:"$sdkLibUm" windowsapp.lib 2>&1
$clExitCode = $LASTEXITCODE

if ($Verbose -and $clOutput) {
    $clOutput | ForEach-Object { Write-Host "  $_" }
}

Check ($clExitCode -eq 0) "C++ compilation succeeded"

if ($clExitCode -ne 0) {
    Write-Host "FAIL: Compilation failed - output:" -ForegroundColor Red
    $clOutput | ForEach-Object { Write-Host "  $_" }
    exit 1
}

# ── Step 5: Run test ────────────────────────────────────────────────────
Write-Step "Running TestClient.exe"

$testOutput = & $testClientExe 2>&1
$testExitCode = $LASTEXITCODE

$testOutput | ForEach-Object { Write-Host "  $_" }

Check ($testExitCode -eq 0) "TestClient exited with code 0"
Check ($testOutput -match "PASS") "TestClient reports PASS"

# ── Results ─────────────────────────────────────────────────────────────
Write-Host "`n" -NoNewline
Write-Host ("=" * 60)
$total = $passCount + $failCount + $skipCount
Write-Host "Results: $passCount passed, $failCount failed, $skipCount skipped (of $total)" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Green" })
Write-Host ("=" * 60)

exit $failCount
