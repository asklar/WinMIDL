<#
.SYNOPSIS
    Build and run the C++/WinRT E2E test suite — validates WinMIDL .winmd
    output works with the cppwinrt toolchain for both in-proc and out-of-proc.

.DESCRIPTION
    Pipeline:
      IDL -> WinMIDL -> .winmd + .h + _i.c + _proxy.cpp
      .winmd -> cppwinrt -> projection headers + component headers
      Compile + run in-proc test (direct winrt::make, no registration)
      Compile + run out-of-proc test (CoCreateInstance, proxy/stub, LRPC)

    Exit code 0 = pass, non-zero = failure.
#>
param(
    [switch]$Verbose,
    [switch]$SkipBuild,
    [switch]$KeepRegistration,
    [switch]$InProcOnly
)

$ErrorActionPreference = 'Stop'
$scriptDir = $PSScriptRoot
$repoRoot = Resolve-Path "$scriptDir/../.."
$outDir = "$scriptDir/generated"
$buildDir = "$scriptDir/build"

$passCount = 0
$failCount = 0

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

# Detect target architecture: use VS dev cmd env if available, else OS arch
$targetArch = $env:VSCMD_ARG_TGT_ARCH
if (-not $targetArch) {
    $targetArch = if ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq [System.Runtime.InteropServices.Architecture]::Arm64) { "arm64" } else { "x64" }
}
$hostDir = if ($targetArch -eq "arm64") { "Hostarm64\arm64" } else { "Hostx64\x64" }

# Find cl.exe — prefer PATH, fall back to vswhere
$vcInclude = $null
$vcLib = $null
$link = $null
$clPath = Get-Command cl.exe -ErrorAction SilentlyContinue
if ($clPath) {
    $cl = $clPath.Source
    Write-Host "  cl.exe: $cl (from PATH)"
    Write-Host "  Target arch: $targetArch"
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
    $cl = "$vcToolsDir\bin\$hostDir\cl.exe"
    $link = "$vcToolsDir\bin\$hostDir\link.exe"
    $vcInclude = "$vcToolsDir\include"
    $vcLib = "$vcToolsDir\lib\$targetArch"
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
$sdkIncUcrt = "$sdkRoot\Include\$sdkIncVersion\ucrt"
$sdkLibUm = "$sdkRoot\Lib\$sdkIncVersion\um\$targetArch"
$sdkLibUcrt = "$sdkRoot\Lib\$sdkIncVersion\ucrt\$targetArch"
$unionMetadata = "$sdkRoot\UnionMetadata\$sdkVersion"

# Full SDK include/lib paths needed for proxy compilation (windows.h etc.)
$sdkIncUm = "$sdkRoot\Include\$sdkIncVersion\um"
$sdkIncShared = "$sdkRoot\Include\$sdkIncVersion\shared"
$sdkIncWinrt = "$sdkRoot\Include\$sdkIncVersion\winrt"

# Common lib paths for linking
$libPaths = @()
$libPaths += "/LIBPATH:$sdkLibUm"
$libPaths += "/LIBPATH:$sdkLibUcrt"
if ($vcLib) { $libPaths += "/LIBPATH:$vcLib" }

Write-Host "  cppwinrt: $cppwinrt"
Write-Host "  SDK: $sdkVersion"
Write-Host "  CppWinRT headers: $cppwinrtInc"
Write-Host "  Union metadata: $unionMetadata"
if ($vcInclude) { Write-Host "  VC Include: $vcInclude" }
if ($vcLib) { Write-Host "  VC Lib: $vcLib" }

# ── Step 2: Generate .winmd from IDL ────────────────────────────────────
Write-Step "Compiling IDL -> .winmd + header + proxy with WinMIDL"

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
    /cppproxy TestCppWinRT_proxy.cpp `
    /noproxy `
    /no_dlldata `
    /out $outDir `
    /nologo `
    $idlFile 2>&1 | ForEach-Object { if ($Verbose) { Write-Host "  $_" } }

if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne 1) {
    Write-Host "FAIL: WinMIDL CLI failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit 1
}

Check (Test-Path "$outDir/TestCppWinRT.winmd") "WinMD generated"
Check (Test-Path "$outDir/TestCppWinRT.h") "Header generated"
Check (Test-Path "$outDir/TestCppWinRT_i.c") "IID file generated"
Check (Test-Path "$outDir/TestCppWinRT_proxy.cpp") "Proxy code generated"

# ── Step 3: Generate C++/WinRT projection + component headers ──────────
Write-Step "Running cppwinrt.exe -> projection + component headers"

$projDir = "$outDir/cppwinrt"
New-Item -ItemType Directory -Path $projDir -Force | Out-Null

$cppwinrtArgs = @("-in", "$outDir/TestCppWinRT.winmd", "-ref", $unionMetadata,
                  "-out", $projDir, "-component", "-overwrite", "-verbose")
$cppwinrtOutput = & $cppwinrt @cppwinrtArgs 2>&1
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

# Find generated component files
$widgetGH = $null
foreach ($candidate in @(
    "$projDir/TestCppWinRT/Widget.g.h",
    "$projDir/Widget.g.h"
)) {
    if (Test-Path $candidate) { $widgetGH = $candidate; break }
}

$widgetGCpp = $null
foreach ($candidate in @(
    "$projDir/TestCppWinRT/Widget.g.cpp",
    "$projDir/Widget.g.cpp"
)) {
    if (Test-Path $candidate) { $widgetGCpp = $candidate; break }
}

$moduleGCpp = $null
foreach ($candidate in @(
    "$projDir/module.g.cpp"
)) {
    if (Test-Path $candidate) { $moduleGCpp = $candidate; break }
}

if (-not $widgetGH) {
    Write-Host "FAIL: Widget.g.h not found in cppwinrt output" -ForegroundColor Red
    Write-Host "  Contents of $projDir :"
    Get-ChildItem $projDir -Recurse | ForEach-Object { Write-Host "    $($_.FullName)" }
    exit 1
}

Write-Host "  Widget.g.h: $widgetGH"
if ($widgetGCpp) { Write-Host "  Widget.g.cpp: $widgetGCpp" }
if ($moduleGCpp) { Write-Host "  module.g.cpp: $moduleGCpp" }

# Determine the directory containing Widget.g.h (for /I include path)
$componentDir = Split-Path $widgetGH -Parent

# Create a stub pch.h if module.g.cpp references it
if ($moduleGCpp -and (Test-Path $moduleGCpp)) {
    $moduleContent = Get-Content $moduleGCpp -Raw
    if ($moduleContent -match '#include\s+"pch\.h"') {
        $pchStub = "$projDir/pch.h"
        if (-not (Test-Path $pchStub)) {
            Set-Content -Path $pchStub -Value "// stub pch.h`n#pragma once"
        }
    }
}

# ── Step 4: Build & run in-proc test ────────────────────────────────────
Write-Step "Compiling in-proc test (TestInProc.exe)"

$commonFlags = @("/nologo", "/std:c++latest", "/EHsc", "/MD", "/W3", "/Zi",
                 "/DWIN32", "/D_WINDOWS", "/DUNICODE", "/D_UNICODE",
                 "/D_SILENCE_CLANG_COROUTINE_MESSAGE")

# Include paths:
#   $projDir       → winrt/TestCppWinRT.h, module.g.cpp, Widget.g.h
#   $scriptDir     → Widget.h, Widget.cpp
#   $cppwinrtInc   → C++/WinRT SDK base headers
#   $sdkIncUm/Shared/Winrt/Ucrt → windows.h, winstring.h, etc. (for server/proxy)
#   $vcInclude     → MSVC standard headers (when not in dev cmd)
$includeFlags = @()
foreach ($dir in @($projDir, $componentDir, $scriptDir, $cppwinrtInc, $sdkIncUm, $sdkIncShared, $sdkIncWinrt, $sdkIncUcrt)) {
    $includeFlags += "/I"
    $includeFlags += $dir
}
if ($vcInclude) {
    $includeFlags += "/I"
    $includeFlags += $vcInclude
}

# Source files for in-proc
$inprocSources = @(
    "$scriptDir/Widget.cpp",
    "$scriptDir/TestInProc.cpp"
)
if ($moduleGCpp) {
    $inprocSources += $moduleGCpp
}

$inprocExe = "$buildDir/TestInProc.exe"
$inprocObjs = @()

# Compile each source to object file
foreach ($src in $inprocSources) {
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($src)
    $objFile = "$buildDir/${baseName}.obj"
    $inprocObjs += $objFile

    Write-Host "  Compiling $([System.IO.Path]::GetFileName($src))..."
    $output = & $cl @commonFlags @includeFlags /c /Fo"$objFile" $src 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "FAIL: Compilation of $([System.IO.Path]::GetFileName($src)) failed:" -ForegroundColor Red
        $output | ForEach-Object { Write-Host "    $_" }
        exit 1
    }
    if ($Verbose) { $output | ForEach-Object { Write-Host "    $_" } }
}

# Link
Write-Host "  Linking TestInProc.exe..."
$linkOutput = & $cl /nologo /Fe"$inprocExe" @inprocObjs /link /DEBUG `
    @libPaths windowsapp.lib ole32.lib 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Link failed:" -ForegroundColor Red
    $linkOutput | ForEach-Object { Write-Host "    $_" }
    exit 1
}
if ($Verbose) { $linkOutput | ForEach-Object { Write-Host "    $_" } }

Check (Test-Path $inprocExe) "TestInProc.exe built"

Write-Step "Running in-proc tests"

$stdoutFile = "$buildDir/inproc_stdout.txt"
$stderrFile = "$buildDir/inproc_stderr.txt"
$inprocProc = Start-Process -FilePath $inprocExe -Wait -PassThru `
    -RedirectStandardOutput $stdoutFile `
    -RedirectStandardError $stderrFile
$inprocExitCode = $inprocProc.ExitCode

if (Test-Path $stdoutFile) { Get-Content $stdoutFile | ForEach-Object { Write-Host "  $_" } }
if (Test-Path $stderrFile) {
    $errContent = Get-Content $stderrFile -Raw
    if ($errContent -and $errContent.Trim()) {
        Write-Host "  STDERR:" -ForegroundColor Yellow
        Write-Host $errContent
    }
}

Check ($inprocExitCode -eq 0) "In-proc tests passed (exit code $inprocExitCode)"

if ($InProcOnly) {
    Write-Host "`n"
    Write-Host ("=" * 60)
    Write-Host "Results: $passCount passed, $failCount failed" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Green" })
    Write-Host ("=" * 60)
    exit $failCount
}

# ── Step 5: Build out-of-proc components ────────────────────────────────
Write-Step "Building out-of-proc components"

# 5a: Compile proxy DLL
Write-Host "  Compiling TestCppWinRTProxy.dll..."
$proxyObj = "$buildDir/TestCppWinRT_proxy.obj"
$proxyDll = "$buildDir/TestCppWinRTProxy.dll"

$proxyIncludes = @("/I", "$outDir", "/I", "$repoRoot/include", "/I", "$sdkIncUm", "/I", "$sdkIncShared", "/I", "$sdkIncWinrt", "/I", "$sdkIncUcrt")
if ($vcInclude) { $proxyIncludes += "/I"; $proxyIncludes += $vcInclude }
$output = & $cl @commonFlags @proxyIncludes /c /Fo"$proxyObj" "$outDir/TestCppWinRT_proxy.cpp" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Proxy compilation failed:" -ForegroundColor Red
    $output | ForEach-Object { Write-Host "    $_" }
    exit 1
}
if ($Verbose) { $output | ForEach-Object { Write-Host "    $_" } }

$output = & $cl /nologo /LD /Fe"$proxyDll" $proxyObj /link /DEBUG `
    /DEF:"$scriptDir/TestCppWinRTProxy.def" `
    @libPaths `
    ole32.lib oleaut32.lib rpcrt4.lib advapi32.lib user32.lib runtimeobject.lib 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Proxy DLL link failed:" -ForegroundColor Red
    $output | ForEach-Object { Write-Host "    $_" }
    exit 1
}
if ($Verbose) { $output | ForEach-Object { Write-Host "    $_" } }

Check (Test-Path $proxyDll) "TestCppWinRTProxy.dll built"

# 5b: Compile server EXE
Write-Host "  Compiling CppWinRTServer.exe..."
$serverSources = @("$scriptDir/ServerMain.cpp", "$scriptDir/Widget.cpp")
if ($moduleGCpp) { $serverSources += $moduleGCpp }
$serverObjs = @()

foreach ($src in $serverSources) {
    $baseName = "server_$([System.IO.Path]::GetFileNameWithoutExtension($src))"
    $objFile = "$buildDir/${baseName}.obj"
    $serverObjs += $objFile

    $output = & $cl @commonFlags @includeFlags /c /Fo"$objFile" $src 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "FAIL: Server compilation of $([System.IO.Path]::GetFileName($src)) failed:" -ForegroundColor Red
        $output | ForEach-Object { Write-Host "    $_" }
        exit 1
    }
    if ($Verbose) { $output | ForEach-Object { Write-Host "    $_" } }
}

$serverExe = "$buildDir/CppWinRTServer.exe"
$output = & $cl /nologo /Fe"$serverExe" @serverObjs /link /DEBUG `
    /SUBSYSTEM:CONSOLE `
    @libPaths `
    windowsapp.lib ole32.lib oleaut32.lib runtimeobject.lib 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Server link failed:" -ForegroundColor Red
    $output | ForEach-Object { Write-Host "    $_" }
    exit 1
}
if ($Verbose) { $output | ForEach-Object { Write-Host "    $_" } }

Check (Test-Path $serverExe) "CppWinRTServer.exe built"

# 5c: Compile out-of-proc client EXE
Write-Host "  Compiling TestOutOfProc.exe..."
$clientObj = "$buildDir/TestOutOfProc.obj"
$clientExe = "$buildDir/TestOutOfProc.exe"

# Client only needs projection headers + SDK (for windows.h used by CoCreateInstance)
$clientIncludes = @("/I", "$projDir", "/I", "$cppwinrtInc", "/I", "$sdkIncUm", "/I", "$sdkIncShared", "/I", "$sdkIncUcrt", "/I", "$sdkIncWinrt")
if ($vcInclude) { $clientIncludes += "/I"; $clientIncludes += $vcInclude }

$output = & $cl @commonFlags @clientIncludes /c /Fo"$clientObj" "$scriptDir/TestOutOfProc.cpp" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Client compilation failed:" -ForegroundColor Red
    $output | ForEach-Object { Write-Host "    $_" }
    exit 1
}
if ($Verbose) { $output | ForEach-Object { Write-Host "    $_" } }

$output = & $cl /nologo /Fe"$clientExe" $clientObj /link /DEBUG `
    /SUBSYSTEM:CONSOLE `
    @libPaths `
    windowsapp.lib ole32.lib oleaut32.lib 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Client link failed:" -ForegroundColor Red
    $output | ForEach-Object { Write-Host "    $_" }
    exit 1
}
if ($Verbose) { $output | ForEach-Object { Write-Host "    $_" } }

Check (Test-Path $clientExe) "TestOutOfProc.exe built"

# ── Step 6: Register proxy DLL and server EXE (HKCU) ───────────────────
Write-Step "Registering COM classes (HKCU, no admin needed)"

$serverExePath = (Resolve-Path $serverExe).Path
$proxyDllPath = (Resolve-Path $proxyDll).Path

$CLSID_Widget = "{A7B8C9D0-E1F2-4A3B-8C5D-6E7F8091A2B3}"
$CLSID_WidgetFactory = "{B8C9D0E1-F2A3-5B4C-9D6E-7F8091A2B3C4}"
$regBase = "HKCU:\Software\Classes"

# Register Widget CLSID → LocalServer32
$clsidKey = "$regBase\CLSID\$CLSID_Widget\LocalServer32"
New-Item -Path $clsidKey -Force | Out-Null
Set-ItemProperty -Path $clsidKey -Name "(Default)" -Value $serverExePath
Write-Host "  CLSID $CLSID_Widget → LocalServer32 (instances)"

# Register WidgetFactory CLSID → LocalServer32 (same server EXE)
$factoryKey = "$regBase\CLSID\$CLSID_WidgetFactory\LocalServer32"
New-Item -Path $factoryKey -Force | Out-Null
Set-ItemProperty -Path $factoryKey -Name "(Default)" -Value $serverExePath
Write-Host "  CLSID $CLSID_WidgetFactory → LocalServer32 (factory)"

# Parse IID → GUID mapping from the generated _i.c
$iidContent = Get-Content "$outDir/TestCppWinRT_i.c"
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

# Extract CLSID_PSFactoryBuffer from generated proxy
$proxyContent = Get-Content "$outDir/TestCppWinRT_proxy.cpp" -Raw
if ($proxyContent -match 'CLSID_PSFactoryBuffer\s*=\s*\{\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*\{\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+)\s*\}') {
    $a = [uint32]($Matches[1])
    $b = [uint16]($Matches[2])
    $c = [uint16]($Matches[3])
    $d = @(4..11 | ForEach-Object { [byte]($Matches[$_]) })
    $lastSeg = (($d[2..7] | ForEach-Object { $_.ToString("X2") }) -join '')
    $CLSID_PSFactory = "{$($a.ToString("X8"))-$($b.ToString("X4"))-$($c.ToString("X4"))-$($d[0].ToString("X2"))$($d[1].ToString("X2"))-$lastSeg}"
} else {
    Write-Host "FAIL: Could not extract CLSID_PSFactoryBuffer from generated proxy" -ForegroundColor Red
    exit 1
}
Write-Host "  CLSID_PSFactory: $CLSID_PSFactory"

# Register PSFactoryBuffer CLSID → InprocServer32
$psKey = "$regBase\CLSID\$CLSID_PSFactory\InprocServer32"
New-Item -Path $psKey -Force | Out-Null
Set-ItemProperty -Path $psKey -Name "(Default)" -Value $proxyDllPath
Set-ItemProperty -Path $psKey -Name "ThreadingModel" -Value "Both"
Write-Host "  PSFactory → InprocServer32"

# Register each IID → ProxyStubClsid32
foreach ($iidName in $iidMap.Keys) {
    $iidGuid = $iidMap[$iidName]
    $iidKey = "$regBase\Interface\$iidGuid\ProxyStubClsid32"
    New-Item -Path $iidKey -Force | Out-Null
    Set-ItemProperty -Path $iidKey -Name "(Default)" -Value $CLSID_PSFactory
    if ($Verbose) { Write-Host "    IID $iidGuid → $iidName" }
}
Write-Host "  Registered $($iidMap.Count) interface IIDs"

# Register WinRT activation (for RoGetActivationFactory — needed for static methods)
# ActivatableClassId → Server CLSID → LocalServer32 path
$activationBase = "$regBase\ActivatableClassId\TestCppWinRT.Widget"
New-Item -Path $activationBase -Force | Out-Null
Set-ItemProperty -Path $activationBase -Name "ActivationType" -Value 1 -Type DWord  # 1 = out-of-proc
Set-ItemProperty -Path $activationBase -Name "Server" -Value $CLSID_Widget
Write-Host "  WinRT activation: TestCppWinRT.Widget → Server $CLSID_Widget"

# ── Step 7: Run out-of-proc test ────────────────────────────────────────
Write-Step "Starting server and running out-of-proc tests"

# Kill stale server processes
Get-Process -Name "CppWinRTServer" -ErrorAction SilentlyContinue |
    ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }
Start-Sleep -Milliseconds 500

$serverProc = Start-Process -FilePath $serverExePath -PassThru -NoNewWindow
Write-Host "  Server PID: $($serverProc.Id)"
Start-Sleep -Seconds 2

if ($serverProc.HasExited) {
    Write-Host "FAIL: Server exited prematurely with code $($serverProc.ExitCode)" -ForegroundColor Red
    $failCount++
} else {
    # Run client
    $oopStdout = "$buildDir/oop_stdout.txt"
    $oopStderr = "$buildDir/oop_stderr.txt"
    $clientProc = Start-Process -FilePath $clientExe -Wait -PassThru `
        -RedirectStandardOutput $oopStdout `
        -RedirectStandardError $oopStderr
    $oopExitCode = $clientProc.ExitCode

    if (Test-Path $oopStdout) { Get-Content $oopStdout | ForEach-Object { Write-Host "  $_" } }
    if (Test-Path $oopStderr) {
        $errContent = Get-Content $oopStderr -Raw
        if ($errContent -and $errContent.Trim()) {
            Write-Host "  STDERR:" -ForegroundColor Yellow
            Write-Host $errContent
        }
    }

    Check ($oopExitCode -eq 0) "Out-of-proc tests passed (exit code $oopExitCode)"
}

# Kill server
if (-not $serverProc.HasExited) {
    Stop-Process -Id $serverProc.Id -Force -ErrorAction SilentlyContinue
    Write-Host "  Server killed"
}

# ── Step 8: Cleanup registration ────────────────────────────────────────
if (-not $KeepRegistration) {
    Write-Step "Cleaning up registration"
    Remove-Item "$regBase\CLSID\$CLSID_Widget" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "$regBase\CLSID\$CLSID_WidgetFactory" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "$regBase\CLSID\$CLSID_PSFactory" -Recurse -Force -ErrorAction SilentlyContinue
    foreach ($iidGuid in $iidMap.Values) {
        Remove-Item "$regBase\Interface\$iidGuid" -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Host "  Unregistered"
}

# ── Summary ─────────────────────────────────────────────────────────────
Write-Host "`n"
Write-Host ("=" * 60)
Write-Host "Results: $passCount passed, $failCount failed" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Green" })
Write-Host ("=" * 60)

exit $failCount
