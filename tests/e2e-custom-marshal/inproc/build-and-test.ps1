<#
.SYNOPSIS
    Build and run the in-proc WinRT proxy/stub e2e test.

.DESCRIPTION
    1. Runs WinMIDL CLI to generate header, IID, and C++ proxy code
    2. Discovers MSVC via vswhere
    3. Compiles server DLL + proxy DLL + client EXE
    4. Registers in HKCU (no admin needed)
    5. Runs test client
    6. Unregisters
    7. Reports results

    Exit code 0 = pass, non-zero = failure.
#>
param(
    [switch]$KeepRegistration,
    [switch]$RegisterOnly,
    [switch]$UnregisterOnly,
    [switch]$Verbose
)

$ErrorActionPreference = 'Stop'

$scriptDir  = $PSScriptRoot
$repoRoot   = (Resolve-Path "$scriptDir/../../..").Path
$outDir     = "$scriptDir/generated"
$buildDir   = "$scriptDir/build"

function Write-Step($msg) { Write-Host "`n>> $msg" -ForegroundColor Cyan }

# ---------------------------------------------------------------------------
# Helper: parse a C-style GUID initializer into a registry-format string.
# Accepts 11 hex values (d1, d2, d3, b1..b8) stripped of the 0x prefix.
# Returns e.g. "{3871B1F8-C7D1-7D56-BD7F-98C98895B576}"
# ---------------------------------------------------------------------------
function Format-Guid {
    param([string[]]$Parts)
    $h = $Parts | ForEach-Object { ($_ -replace '^0x','').Trim() }
    $d1 = $h[0].PadLeft(8,'0')
    $d2 = $h[1].PadLeft(4,'0')
    $d3 = $h[2].PadLeft(4,'0')
    $b  = $h[3..10] | ForEach-Object { $_.PadLeft(2,'0') }
    return "{$d1-$d2-$d3-$($b[0])$($b[1])-$($b[2])$($b[3])$($b[4])$($b[5])$($b[6])$($b[7])}".ToUpper()
}

# ---------------------------------------------------------------------------
# Helper: extract all IID definitions from a generated _i.c or proxy file.
# Returns a hashtable: name -> GUID string.
# ---------------------------------------------------------------------------
function Get-IidsFromFile($path) {
    $content = Get-Content $path -Raw
    $iids = @{}
    $regex = 'MIDL_DEFINE_GUID\(\s*IID\s*,\s*(\w+)\s*,' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*\)'
    foreach ($match in [regex]::Matches($content, $regex)) {
        $name = $match.Groups[1].Value
        $parts = @(2..12 | ForEach-Object { $match.Groups[$_].Value })
        $iids[$name] = Format-Guid -Parts $parts
    }
    return $iids
}

# ---------------------------------------------------------------------------
# Helper: extract CLSID_PSFactoryBuffer from the generated proxy.
# ---------------------------------------------------------------------------
function Get-ClsidPSFactory($path) {
    $content = Get-Content $path -Raw
    $regex = 'CLSID_PSFactoryBuffer\s*=\s*\{' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*\{' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)'
    if ($content -match $regex) {
        $parts = @(1..11 | ForEach-Object { $Matches[$_] })
        return Format-Guid -Parts $parts
    }
    return $null
}

# ---------------------------------------------------------------------------
# Helper: extract CLSID_Widget from WidgetServer.cpp source.
# ---------------------------------------------------------------------------
function Get-ClsidWidget($path) {
    $content = Get-Content $path -Raw
    $regex = 'CLSID_Widget\s*=\s*\{' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,\s*\{' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)\s*,' +
             '\s*0x([0-9a-fA-F]+)\s*,\s*0x([0-9a-fA-F]+)'
    if ($content -match $regex) {
        $parts = @(1..11 | ForEach-Object { $Matches[$_] })
        return Format-Guid -Parts $parts
    }
    return $null
}

# ---------------------------------------------------------------------------
# Handle -UnregisterOnly: clean up and exit
# ---------------------------------------------------------------------------
if ($UnregisterOnly) {
    Write-Step "Unregistering COM classes"
    $regBase = "HKCU:\Software\Classes"
    $clsidWidget = Get-ClsidWidget "$scriptDir\WidgetServer.cpp"
    $clsidPS = if (Test-Path "$outDir\TestWidget_proxy.cpp") { Get-ClsidPSFactory "$outDir\TestWidget_proxy.cpp" } else { $null }
    $iids = if (Test-Path "$outDir\TestWidget_i.c") { Get-IidsFromFile "$outDir\TestWidget_i.c" } else { @{} }
    if ($clsidWidget) { Remove-Item "$regBase\CLSID\$clsidWidget" -Recurse -Force -EA SilentlyContinue; Write-Host "  Removed CLSID $clsidWidget" }
    if ($clsidPS) { Remove-Item "$regBase\CLSID\$clsidPS" -Recurse -Force -EA SilentlyContinue; Write-Host "  Removed PS CLSID $clsidPS" }
    foreach ($entry in $iids.GetEnumerator()) { Remove-Item "$regBase\Interface\$($entry.Value)" -Recurse -Force -EA SilentlyContinue; Write-Host "  Removed IID $($entry.Key)" }
    Write-Host "  Done" -ForegroundColor Green
    exit 0
}

# ---------------------------------------------------------------------------
# Handle -RegisterOnly: build (if needed) + register, then exit
# ---------------------------------------------------------------------------

# Ensure output directories exist
if (-not $RegisterOnly) {
    if (Test-Path $outDir)   { Remove-Item $outDir -Recurse -Force }
    if (Test-Path $buildDir) { Remove-Item $buildDir -Recurse -Force }
}
New-Item -ItemType Directory -Path $outDir -Force | Out-Null
New-Item -ItemType Directory -Path $buildDir -Force | Out-Null

# ── Step 1: Generate code from IDL ─────────────────────────────────────────
Write-Step "Generating header + proxy code from TestWidget.idl"

# Kill any lingering Midl.Cli processes
Get-Process -Name "Midl.Cli" -ErrorAction SilentlyContinue |
    ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }
Start-Sleep -Milliseconds 500

$cliProject = "$repoRoot/src/Midl.Cli/Midl.Cli.csproj"
$idlFile    = "$scriptDir/TestWidget.idl"

& dotnet run --project $cliProject -- `
    /winrt `
    /h TestWidget.h `
    /iid TestWidget_i.c `
    /cppproxy TestWidget_proxy.cpp `
    /noproxy /no_dlldata `
    /out $outDir `
    /nologo `
    $idlFile 2>&1 | ForEach-Object { if ($Verbose) { Write-Host "  $_" } }

if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne 1) {
    Write-Host "FAIL: MIDL CLI failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit 1
}

foreach ($f in @("TestWidget.h", "TestWidget_i.c", "TestWidget_proxy.cpp")) {
    if (-not (Test-Path "$outDir/$f")) {
        Write-Host "FAIL: Expected generated file not found: $f" -ForegroundColor Red
        exit 1
    }
}

Write-Host "  Generated: TestWidget.h, TestWidget_i.c, TestWidget_proxy.cpp"

# ── Step 2: Discover MSVC ──────────────────────────────────────────────────
Write-Step "Discovering MSVC toolchain"

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path $vswhere)) {
    $vswhere = "${env:ProgramFiles}\Microsoft Visual Studio\Installer\vswhere.exe"
}
if (-not (Test-Path $vswhere)) {
    Write-Host "SKIP: vswhere not found - MSVC not installed" -ForegroundColor Yellow
    exit 0
}

$vsPath = & $vswhere -latest -property installationPath `
    -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64
if (-not $vsPath) {
    Write-Host "SKIP: No Visual Studio with C++ tools found" -ForegroundColor Yellow
    exit 0
}

# Find MSVC tools
$vcToolsVersion = (Get-Content "$vsPath\VC\Auxiliary\Build\Microsoft.VCToolsVersion.default.txt" `
    -ErrorAction Stop).Trim()
$vcToolsDir = "$vsPath\VC\Tools\MSVC\$vcToolsVersion"
$cl   = "$vcToolsDir\bin\Hostx64\x64\cl.exe"
$link = "$vcToolsDir\bin\Hostx64\x64\link.exe"

if (-not (Test-Path $cl)) {
    Write-Host "SKIP: cl.exe not found at $cl" -ForegroundColor Yellow
    exit 0
}

# Find Windows SDK
$sdkRoot    = $null
$sdkVersion = $null
$sdkRegPath = "HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots"
if (Test-Path $sdkRegPath) {
    $sdkRoot = (Get-ItemProperty $sdkRegPath -Name KitsRoot10 -ErrorAction SilentlyContinue).KitsRoot10
    if ($sdkRoot) {
        $sdkVersions = Get-ChildItem "$sdkRoot\Include" -Directory |
            Where-Object { $_.Name -match '^\d+\.' } |
            Sort-Object Name -Descending
        if ($sdkVersions) { $sdkVersion = $sdkVersions[0].Name }
    }
}
if (-not $sdkRoot -or -not $sdkVersion) {
    Write-Host "SKIP: Windows SDK not found" -ForegroundColor Yellow
    exit 0
}

Write-Host "  MSVC: $vcToolsVersion"
Write-Host "  SDK:  $sdkVersion"

# Build environment
$vcInclude    = "$vcToolsDir\include"
$vcLib        = "$vcToolsDir\lib\x64"
$sdkIncUcrt   = "$sdkRoot\Include\$sdkVersion\ucrt"
$sdkIncShared = "$sdkRoot\Include\$sdkVersion\shared"
$sdkIncUm     = "$sdkRoot\Include\$sdkVersion\um"
$sdkIncWinrt  = "$sdkRoot\Include\$sdkVersion\winrt"
$sdkLibUcrt   = "$sdkRoot\Lib\$sdkVersion\ucrt\x64"
$sdkLibUm     = "$sdkRoot\Lib\$sdkVersion\um\x64"

$env:INCLUDE = "$vcInclude;$sdkIncUcrt;$sdkIncShared;$sdkIncUm;$sdkIncWinrt"
$env:LIB     = "$vcLib;$sdkLibUcrt;$sdkLibUm"
$env:PATH    = "$vcToolsDir\bin\Hostx64\x64;$sdkRoot\bin\$sdkVersion\x64;$env:PATH"

$commonFlags = @(
    "/nologo", "/std:c++latest", "/EHsc", "/W3",
    "/DWIN32", "/D_WINDOWS", "/DUNICODE", "/D_UNICODE",
    "/Zi"
)

# ── Step 3: Compile IID definitions ────────────────────────────────────────
Write-Step "Compiling TestWidget_i.obj"

$iidObj = "$buildDir\TestWidget_i.obj"
& $cl @commonFlags /c /Fo"$iidObj" /TP /I"$outDir" "$outDir\TestWidget_i.c" 2>&1 |
    ForEach-Object { Write-Host "  $_" }
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: IID compilation failed" -ForegroundColor Red
    exit 1
}

# ── Step 4: Compile server DLL ─────────────────────────────────────────────
Write-Step "Compiling WidgetServer.dll"

$serverObj = "$buildDir\WidgetServer.obj"
$serverDll = "$buildDir\WidgetServer.dll"

& $cl @commonFlags /c /Fo"$serverObj" /I"$outDir" "$scriptDir\WidgetServer.cpp" 2>&1 |
    ForEach-Object { Write-Host "  $_" }
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Server compilation failed" -ForegroundColor Red
    exit 1
}

& $link /nologo /DLL /DEBUG /OUT:"$serverDll" /DEF:"$scriptDir\WidgetServer.def" `
    "$serverObj" "$iidObj" `
    ole32.lib oleaut32.lib advapi32.lib user32.lib runtimeobject.lib 2>&1 |
    ForEach-Object { Write-Host "  $_" }
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Server link failed" -ForegroundColor Red
    exit 1
}
Write-Host "  -> $serverDll"

# ── Step 5: Compile proxy DLL ──────────────────────────────────────────────
Write-Step "Compiling TestWidgetProxy.dll"

$proxyObj = "$buildDir\TestWidget_proxy.obj"
$proxyDll = "$buildDir\TestWidgetProxy.dll"

& $cl @commonFlags /c /Fo"$proxyObj" /I"$outDir" /I"$repoRoot\include" "$outDir\TestWidget_proxy.cpp" 2>&1 |
    ForEach-Object { Write-Host "  $_" }
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Proxy compilation failed" -ForegroundColor Red
    exit 1
}

& $link /nologo /DLL /DEBUG /OUT:"$proxyDll" /DEF:"$scriptDir\TestWidgetProxy.def" `
    "$proxyObj" `
    ole32.lib oleaut32.lib advapi32.lib user32.lib runtimeobject.lib 2>&1 |
    ForEach-Object { Write-Host "  $_" }
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Proxy link failed" -ForegroundColor Red
    exit 1
}
Write-Host "  -> $proxyDll"

# ── Step 6: Compile test client ────────────────────────────────────────────
Write-Step "Compiling TestClient.exe"

$clientObj = "$buildDir\TestClient.obj"
$clientExe = "$buildDir\TestClient.exe"

& $cl @commonFlags /c /Fo"$clientObj" /I"$outDir" "$scriptDir\TestClient.cpp" 2>&1 |
    ForEach-Object { Write-Host "  $_" }
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Client compilation failed" -ForegroundColor Red
    exit 1
}

& $link /nologo /DEBUG /OUT:"$clientExe" `
    "$clientObj" "$iidObj" `
    ole32.lib oleaut32.lib user32.lib runtimeobject.lib 2>&1 |
    ForEach-Object { Write-Host "  $_" }
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Client link failed" -ForegroundColor Red
    exit 1
}
Write-Host "  -> $clientExe"

# ── Step 7: Register DLLs (per-user, no admin) ────────────────────────────
Write-Step "Registering COM classes (HKCU)"

$serverDllFull = (Resolve-Path $serverDll).Path
$proxyDllFull  = (Resolve-Path $proxyDll).Path

# Extract GUIDs dynamically from generated and source files
$clsidWidget = Get-ClsidWidget "$scriptDir\WidgetServer.cpp"
if (-not $clsidWidget) {
    Write-Host "FAIL: Could not extract CLSID_Widget from WidgetServer.cpp" -ForegroundColor Red
    exit 1
}

$iids = Get-IidsFromFile "$outDir\TestWidget_i.c"
if ($iids.Count -eq 0) {
    Write-Host "FAIL: No IID definitions found in generated TestWidget_i.c" -ForegroundColor Red
    exit 1
}

$clsidPS = Get-ClsidPSFactory "$outDir\TestWidget_proxy.cpp"
if (-not $clsidPS) {
    Write-Host "FAIL: Could not extract CLSID_PSFactoryBuffer from proxy" -ForegroundColor Red
    exit 1
}

Write-Host "  CLSID_Widget:          $clsidWidget"
Write-Host "  CLSID_PSFactoryBuffer: $clsidPS"
foreach ($entry in $iids.GetEnumerator()) {
    Write-Host "  $($entry.Key): $($entry.Value)"
}

# Register Widget server CLSID -> InprocServer32
$regBase = "HKCU:\Software\Classes"
$serverKeyPath = "$regBase\CLSID\$clsidWidget\InprocServer32"
New-Item -Path $serverKeyPath -Force | Out-Null
Set-ItemProperty -Path $serverKeyPath -Name "(Default)" -Value $serverDllFull
Set-ItemProperty -Path $serverKeyPath -Name "ThreadingModel" -Value "Apartment"

# Register proxy/stub factory CLSID -> InprocServer32
$psKeyPath = "$regBase\CLSID\$clsidPS\InprocServer32"
New-Item -Path $psKeyPath -Force | Out-Null
Set-ItemProperty -Path $psKeyPath -Name "(Default)" -Value $proxyDllFull

# Register each IID -> ProxyStubClsid32
foreach ($entry in $iids.GetEnumerator()) {
    $iidGuid = $entry.Value
    $iidKeyPath = "$regBase\Interface\$iidGuid\ProxyStubClsid32"
    New-Item -Path $iidKeyPath -Force | Out-Null
    Set-ItemProperty -Path $iidKeyPath -Name "(Default)" -Value $clsidPS
}

Write-Host "  Registration complete"

if ($RegisterOnly) {
    Write-Host "`nRegistration done. Run the client manually:" -ForegroundColor Green
    Write-Host "  $clientExe" -ForegroundColor DarkGray
    Write-Host "Unregister with:" -ForegroundColor DarkGray
    Write-Host "  .\build-and-test.ps1 -UnregisterOnly" -ForegroundColor DarkGray
    exit 0
}

# ── Step 8: Run test ───────────────────────────────────────────────────────
Write-Step "Running TestClient.exe"
Write-Host ""

$testExitCode = 0
try {
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
    Write-Host "FAIL: TestClient.exe threw an exception: $_" -ForegroundColor Red
    $testExitCode = 1
}

# ── Step 9: Unregister ─────────────────────────────────────────────────────
if (-not $KeepRegistration) {
    Write-Step "Cleaning up registration"
    Remove-Item "$regBase\CLSID\$clsidWidget" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "$regBase\CLSID\$clsidPS" -Recurse -Force -ErrorAction SilentlyContinue
    foreach ($entry in $iids.GetEnumerator()) {
        Remove-Item "$regBase\Interface\$($entry.Value)" -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Host "  Unregistered"
}

# ── Summary ────────────────────────────────────────────────────────────────
Write-Host ""
if ($testExitCode -eq 0) {
    Write-Host "SUCCESS: All proxy/stub e2e tests passed!" -ForegroundColor Green
} else {
    Write-Host "FAILURE: Some tests failed (exit code $testExitCode)" -ForegroundColor Red
}

exit $testExitCode
