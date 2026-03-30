<#
.SYNOPSIS
    Build and run the comprehensive in-proc WinRT proxy/stub e2e test.

.DESCRIPTION
    This script can also be run via: dotnet test --filter "Category=E2E"
    The C# tests in Midl.Integration.Tests contain the equivalent logic.

    1. Runs our MIDL CLI to generate header, IID, and proxy code from the IDL
    2. Discovers MSVC via vswhere
    3. Compiles server DLL (ComprehensiveServer.dll)
    4. Compiles proxy DLL (TestComprehensiveProxy.dll) from generated code
    5. Registers COM classes and proxy/stub (per-user, no admin required)
    6. Runs test client
    7. Unregisters
    8. Reports results

    Exit code 0 = pass, non-zero = failure.
#>
param(
    [switch]$KeepRegistration,
    [switch]$Verbose,
    [switch]$Debug
)

$ErrorActionPreference = 'Stop'
$scriptDir = $PSScriptRoot
$repoRoot = Resolve-Path "$scriptDir/../../.."
$outDir = "$scriptDir/generated"
$buildDir = "$scriptDir/build"

# Ensure clean output
if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }
if (Test-Path $buildDir) { Remove-Item $buildDir -Recurse -Force }
New-Item -ItemType Directory -Path $outDir -Force | Out-Null
New-Item -ItemType Directory -Path $buildDir -Force | Out-Null

function Write-Step($msg) { Write-Host "`n>> $msg" -ForegroundColor Cyan }

# ── Step 1: Generate code from IDL ──────────────────────────────────────
Write-Step "Generating header + proxy code from TestComprehensive.idl"

# Kill any lingering Midl.Cli processes
Get-Process -Name "Midl.Cli" -ErrorAction SilentlyContinue |
    ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }
Start-Sleep -Milliseconds 500

$cliProject = "$repoRoot/src/Midl.Cli"
$idlFile = "$scriptDir/TestComprehensive.idl"

& dotnet run --project $cliProject -- `
    /winrt `
    /h TestComprehensive.h `
    /iid TestComprehensive_i.c `
    /cppproxy TestComprehensive_proxy.cpp `
    /noproxy `
    /out $outDir `
    /nologo `
    /no_dlldata `
    /arrays `
    $idlFile 2>&1 | ForEach-Object { if ($Verbose) { Write-Host "  $_" } }

# Exit code 1 = warnings only, which is acceptable
if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne 1) {
    Write-Host "FAIL: MIDL CLI failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit 1
}

foreach ($f in @("TestComprehensive.h", "TestComprehensive_i.c", "TestComprehensive_proxy.cpp")) {
    if (-not (Test-Path "$outDir/$f")) {
        Write-Host "FAIL: Expected generated file not found: $f" -ForegroundColor Red
        exit 1
    }
}
Write-Host "  Generated: TestComprehensive.h, TestComprehensive_i.c, TestComprehensive_proxy.cpp"

# The generated _i.c file has all IID definitions in MIDL_DEFINE_GUID format.
# We use it directly as the IID compilation unit.
Write-Host "  Using TestComprehensive_i.c for IID definitions"

# Parse IID name -> GUID mapping from the _i.c file for registration
$iidContent = Get-Content "$outDir/TestComprehensive_i.c"
$iidMap = @{}
$prevComment = $null
foreach ($line in $iidContent) {
    if ($line -match '^\s*//\s*\{([0-9a-fA-F-]+)\}\s*$') {
        $prevComment = $Matches[1]
    }
    elseif ($line -match 'MIDL_DEFINE_GUID\(IID\s*,\s*(IID___x_\S+)\s*,' -and $prevComment) {
        $iidMap[$Matches[1]] = "{$prevComment}"
        $prevComment = $null
    }
    else {
        $prevComment = $null
    }
}
Write-Host "  Found $($iidMap.Count) interface IIDs for registration"

# Extract CLSID_PSFactoryBuffer from the generated proxy
$proxyContent = Get-Content "$outDir/TestComprehensive_proxy.cpp" -Raw
if ($proxyContent -match 'CLSID_PSFactoryBuffer\s*=\s*\{\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*\{\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)') {
    $clsidPS = "{$($Matches[1])-$($Matches[2])-$($Matches[3])-$($Matches[4])$($Matches[5])-$($Matches[6])$($Matches[7])$($Matches[8])$($Matches[9])$($Matches[10])$($Matches[11])}"
    $clsidPS = $clsidPS.ToUpper()
} else {
    Write-Host "FAIL: Could not extract CLSID_PSFactoryBuffer from generated proxy" -ForegroundColor Red
    exit 1
}
Write-Host "  CLSID_PSFactoryBuffer: $clsidPS"

# ── Step 2: Discover MSVC ───────────────────────────────────────────────
Write-Step "Discovering MSVC toolchain"

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path $vswhere)) {
    $vswhere = "${env:ProgramFiles}\Microsoft Visual Studio\Installer\vswhere.exe"
}
if (-not (Test-Path $vswhere)) {
    Write-Host "SKIP: vswhere not found — MSVC not installed" -ForegroundColor Yellow
    exit 0
}
$vsPath = & $vswhere -latest -property installationPath -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64
if (-not $vsPath) {
    Write-Host "SKIP: No Visual Studio with C++ tools found" -ForegroundColor Yellow
    exit 0
}

$vcToolsVersion = Get-Content "$vsPath\VC\Auxiliary\Build\Microsoft.VCToolsVersion.default.txt" -ErrorAction Stop
$vcToolsVersion = $vcToolsVersion.Trim()
$vcToolsDir = "$vsPath\VC\Tools\MSVC\$vcToolsVersion"
$cl = "$vcToolsDir\bin\Hostx64\x64\cl.exe"
$link = "$vcToolsDir\bin\Hostx64\x64\link.exe"
if (-not (Test-Path $cl)) {
    Write-Host "SKIP: cl.exe not found at $cl" -ForegroundColor Yellow
    exit 0
}

$sdkRoot = $null
$sdkVersion = $null
$sdkRegPath = "HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots"
if (Test-Path $sdkRegPath) {
    $sdkRoot = (Get-ItemProperty $sdkRegPath -Name KitsRoot10 -ErrorAction SilentlyContinue).KitsRoot10
    if ($sdkRoot) {
        $sdkVersions = Get-ChildItem "$sdkRoot\Include" -Directory |
            Where-Object { $_.Name -match '^\d+\.' } | Sort-Object Name -Descending
        if ($sdkVersions) { $sdkVersion = $sdkVersions[0].Name }
    }
}
if (-not $sdkRoot -or -not $sdkVersion) {
    Write-Host "SKIP: Windows SDK not found" -ForegroundColor Yellow
    exit 0
}

Write-Host "  MSVC: $vcToolsVersion"
Write-Host "  SDK:  $sdkVersion"

$vcInclude = "$vcToolsDir\include"
$vcLib = "$vcToolsDir\lib\x64"
$sdkIncUcrt = "$sdkRoot\Include\$sdkVersion\ucrt"
$sdkIncShared = "$sdkRoot\Include\$sdkVersion\shared"
$sdkIncUm = "$sdkRoot\Include\$sdkVersion\um"
$sdkIncWinrt = "$sdkRoot\Include\$sdkVersion\winrt"
$sdkLibUcrt = "$sdkRoot\Lib\$sdkVersion\ucrt\x64"
$sdkLibUm = "$sdkRoot\Lib\$sdkVersion\um\x64"

$env:INCLUDE = "$vcInclude;$sdkIncUcrt;$sdkIncShared;$sdkIncUm;$sdkIncWinrt"
$env:LIB = "$vcLib;$sdkLibUcrt;$sdkLibUm"
$env:PATH = "$vcToolsDir\bin\Hostx64\x64;$sdkRoot\bin\$sdkVersion\x64;$env:PATH"

$commonFlags = @("/nologo", "/std:c++latest", "/EHsc", "/W3",
                  "/DWIN32", "/D_WINDOWS", "/DUNICODE", "/D_UNICODE",
                  "/Zi")

# ── Step 3: Compile IID definitions ─────────────────────────────────────
Write-Step "Compiling TestComprehensive_iid.obj"
$iidObj = "$buildDir\TestComprehensive_iid.obj"
# Compile as C (no /TP) so const has external linkage
$cFlags = @("/nologo", "/W3", "/DWIN32", "/D_WINDOWS", "/DUNICODE", "/D_UNICODE")
& $cl @cFlags /c /Fo"$iidObj" "$outDir\TestComprehensive_i.c" /I"$outDir" 2>&1 |
    ForEach-Object { Write-Host "  $_" }
if ($LASTEXITCODE -ne 0) { Write-Host "FAIL: IID compilation failed" -ForegroundColor Red; exit 1 }

# ── Step 4: Compile server DLL ──────────────────────────────────────────
Write-Step "Compiling ComprehensiveServer.dll"
$serverObj = "$buildDir\ComprehensiveServer.obj"
$serverDll = "$buildDir\ComprehensiveServer.dll"

& $cl @commonFlags /c /Fo"$serverObj" "$scriptDir\ComprehensiveServer.cpp" /I"$scriptDir" 2>&1 |
    ForEach-Object { Write-Host "  $_" }
if ($LASTEXITCODE -ne 0) { Write-Host "FAIL: Server compilation failed" -ForegroundColor Red; exit 1 }

& $link /nologo /DLL /DEBUG /OUT:"$serverDll" /DEF:"$scriptDir\ComprehensiveServer.def" `
    "$serverObj" "$iidObj" ole32.lib oleaut32.lib advapi32.lib user32.lib runtimeobject.lib 2>&1 |
    ForEach-Object { Write-Host "  $_" }
if ($LASTEXITCODE -ne 0) { Write-Host "FAIL: Server link failed" -ForegroundColor Red; exit 1 }
Write-Host "  -> $serverDll"

# ── Step 5: Compile proxy DLL ───────────────────────────────────────────
Write-Step "Compiling TestComprehensiveProxy.dll"
$proxyObj = "$buildDir\TestComprehensive_proxy.obj"
$proxyDll = "$buildDir\TestComprehensiveProxy.dll"

& $cl @commonFlags /c /Fo"$proxyObj" "$outDir\TestComprehensive_proxy.cpp" /I"$outDir" /I"$repoRoot/include" 2>&1 |
    ForEach-Object { Write-Host "  $_" }
if ($LASTEXITCODE -ne 0) { Write-Host "FAIL: Proxy compilation failed" -ForegroundColor Red; exit 1 }

& $link /nologo /DLL /DEBUG /OUT:"$proxyDll" /DEF:"$scriptDir\TestComprehensiveProxy.def" `
    "$proxyObj" ole32.lib oleaut32.lib advapi32.lib user32.lib runtimeobject.lib 2>&1 |
    ForEach-Object { Write-Host "  $_" }
if ($LASTEXITCODE -ne 0) { Write-Host "FAIL: Proxy link failed" -ForegroundColor Red; exit 1 }
Write-Host "  -> $proxyDll"

# ── Step 6: Compile test client ─────────────────────────────────────────
Write-Step "Compiling ComprehensiveClient.exe"
$clientObj = "$buildDir\ComprehensiveClient.obj"
$clientExe = "$buildDir\ComprehensiveClient.exe"

& $cl @commonFlags /c /Fo"$clientObj" "$scriptDir\ComprehensiveClient.cpp" /I"$scriptDir" 2>&1 |
    ForEach-Object { Write-Host "  $_" }
if ($LASTEXITCODE -ne 0) { Write-Host "FAIL: Client compilation failed" -ForegroundColor Red; exit 1 }

& $link /nologo /DEBUG /OUT:"$clientExe" "$clientObj" "$iidObj" `
    ole32.lib oleaut32.lib user32.lib runtimeobject.lib 2>&1 |
    ForEach-Object { Write-Host "  $_" }
if ($LASTEXITCODE -ne 0) { Write-Host "FAIL: Client link failed" -ForegroundColor Red; exit 1 }
Write-Host "  -> $clientExe"

# ── Step 7: Register COM classes (HKCU) ─────────────────────────────────
Write-Step "Registering COM classes (HKCU)"

$serverDllFull = (Resolve-Path $serverDll).Path
$proxyDllFull = (Resolve-Path $proxyDll).Path

$regBase = "HKCU:\Software\Classes"

# Runtimeclass CLSIDs → InprocServer32
$clsidMap = @{
    "{6F33328A-B1CF-1A52-8B6C-008019838105}" = "Animal"
    "{FD4F3844-4389-D352-BC91-3D348670F83E}" = "Shape"
    "{8AA503FE-EB8A-655F-9195-12D9800B2077}" = "Circle"
    "{05B429F5-72C9-E559-9D1D-2C09B701DEB4}" = "Document"
    "{FE8DF0E1-4F9D-4851-B494-0B41AB812A5D}" = "Calculator"
    "{A1B2C3D4-E5F6-4A7B-8C9D-0E1F2A3B4C59}" = "Notifier"
    "{B2C3D4E5-F6A7-4B8C-9D0E-1F2A3B4C5D6E}" = "EventSource"
    "{C3D4E5F6-A7B8-4C9D-0E1F-2A3B4C5D6E7F}" = "PropertyBag"
    "{D4E5F6A7-B8C9-4D0E-1F2A-3B4C5D6E7F80}" = "OptionalValues"
    "{E1F2A3B4-C5D6-4E7F-8091-A2B3C4D5E6F7}" = "TypeShowcase"
    "{F2A3B4C5-D6E7-4F80-91A2-B3C4D5E6F7A8}" = "ComplexOps"
    "{A3B4C5D6-E7F8-4091-A2B3-C4D5E6F7A8B9}" = "MathHelpers"
    "{B4C5D6E7-F8A9-40B1-C2D3-E4F5A6B7C8D9}" = "Observable"
    "{C5D6E7F8-A9B0-41C2-D3E4-F5A6B7C8D9EA}" = "CollectionOps"
    "{D6E7F8A9-B0C1-42D3-E4F5-A6B7C8D9EAFB}" = "ArrayOps"
}

foreach ($clsid in $clsidMap.Keys) {
    $keyPath = "$regBase\CLSID\$clsid\InprocServer32"
    New-Item -Path $keyPath -Force | Out-Null
    Set-ItemProperty -Path $keyPath -Name "(Default)" -Value $serverDllFull
    Set-ItemProperty -Path $keyPath -Name "ThreadingModel" -Value "Apartment"
    Write-Host "  CLSID $clsid → $($clsidMap[$clsid])"
}

# PSFactoryBuffer CLSID → proxy DLL
$psKeyPath = "$regBase\CLSID\$clsidPS\InprocServer32"
New-Item -Path $psKeyPath -Force | Out-Null
Set-ItemProperty -Path $psKeyPath -Name "(Default)" -Value $proxyDllFull
Write-Host "  PSFactory $clsidPS"

# Each interface IID → ProxyStubClsid32
foreach ($iidName in $iidMap.Keys) {
    $iidGuid = $iidMap[$iidName]
    $iidKeyPath = "$regBase\Interface\$iidGuid\ProxyStubClsid32"
    New-Item -Path $iidKeyPath -Force | Out-Null
    Set-ItemProperty -Path $iidKeyPath -Name "(Default)" -Value $clsidPS
    if ($Verbose) { Write-Host "  IID $iidGuid → $iidName" }
}
Write-Host "  Registered $($iidMap.Count) interface IIDs"

Write-Host "  Registration complete"

# ── Step 8: Run test ────────────────────────────────────────────────────
Write-Step "Running ComprehensiveClient.exe"
Write-Host ""

$testExitCode = 0
try {
    if ($Debug) {
        Write-Host "  Launching under WinDbg (registration preserved)..." -ForegroundColor Yellow
        Write-Host "  Set breakpoints, then 'g' to continue." -ForegroundColor Yellow
        Write-Host ""
        $KeepRegistration = $true
        Start-Process -FilePath "windbg" -ArgumentList "-g -G `"$clientExe`""
        Write-Host "  WinDbg launched. Run the script again without -Debug to unregister." -ForegroundColor Yellow
        Write-Host "  Or: .\build-and-test.ps1 -KeepRegistration  (to just unregister later)"
        exit 0
    }

    $stdoutFile = "$buildDir\test_stdout.txt"
    $stderrFile = "$buildDir\test_stderr.txt"
    $proc = Start-Process -FilePath $clientExe -Wait -PassThru `
        -RedirectStandardOutput $stdoutFile `
        -RedirectStandardError $stderrFile
    $testExitCode = $proc.ExitCode
    if (Test-Path $stdoutFile) { Get-Content $stdoutFile }
    if (Test-Path $stderrFile) {
        $errContent = Get-Content $stderrFile -Raw
        if ($errContent -and $errContent.Trim()) { Write-Host $errContent }
    }
} catch {
    Write-Host "FAIL: ComprehensiveClient.exe threw an exception: $_" -ForegroundColor Red
    $testExitCode = 1
}

# ── Step 9: Unregister ──────────────────────────────────────────────────
if (-not $KeepRegistration) {
    Write-Step "Cleaning up registration"
    foreach ($clsid in $clsidMap.Keys) {
        Remove-Item "$regBase\CLSID\$clsid" -Recurse -Force -ErrorAction SilentlyContinue
    }
    Remove-Item "$regBase\CLSID\$clsidPS" -Recurse -Force -ErrorAction SilentlyContinue
    foreach ($iidGuid in $iidMap.Values) {
        Remove-Item "$regBase\Interface\$iidGuid" -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Host "  Unregistered"
}

# ── Summary ─────────────────────────────────────────────────────────────
Write-Host ""
if ($testExitCode -eq 0) {
    Write-Host "SUCCESS: All comprehensive proxy/stub e2e tests passed!" -ForegroundColor Green
} else {
    Write-Host "FAILURE: Some tests failed (exit code $testExitCode)" -ForegroundColor Red
}

exit $testExitCode
