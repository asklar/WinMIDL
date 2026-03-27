# build-and-test.ps1 — Build & test out-of-proc WinRT custom marshal E2E
#
# 1. Generate proxy code from IDL via our CLI
# 2. Compile server EXE, proxy DLL, client EXE
# 3. Register proxy DLL + server EXE in HKCU
# 4. Start server process
# 5. Run client process
# 6. Kill server
# 7. Unregister
# 8. Report results
#
# Exit code: 0 = pass, 1 = fail

param(
    [string]$VcToolsDir,  # e.g. "C:\Program Files\Microsoft Visual Studio\...\VC\Tools\MSVC\14.x"
    [string]$WinSdkInclude,  # e.g. "C:\Program Files (x86)\Windows Kits\10\Include\10.0.xxxxx.0"
    [string]$WinSdkLib       # e.g. "C:\Program Files (x86)\Windows Kits\10\Lib\10.0.xxxxx.0"
)

$ErrorActionPreference = 'Continue'

$testDir = $PSScriptRoot
$repoRoot = (Resolve-Path "$testDir/../../..").Path
$genDir = "$testDir/generated"
$buildDir = "$testDir/build"

# ---------- Locate MSVC + Windows SDK ----------

function Find-VsWhere {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) { return $vswhere }
    $vswhere = "${env:ProgramFiles}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) { return $vswhere }
    return $null
}

function Find-MsvcTools {
    if ($VcToolsDir -and (Test-Path $VcToolsDir)) {
        return $VcToolsDir
    }
    $vswhere = Find-VsWhere
    if (-not $vswhere) { return $null }
    $installPath = & $vswhere -latest -property installationPath -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 2>$null
    if (-not $installPath) { return $null }
    $vcToolsVersion = Get-Content "$installPath\VC\Auxiliary\Build\Microsoft.VCToolsVersion.default.txt" -ErrorAction SilentlyContinue
    if (-not $vcToolsVersion) { return $null }
    $vcToolsVersion = $vcToolsVersion.Trim()
    return "$installPath\VC\Tools\MSVC\$vcToolsVersion"
}

function Find-WinSdk {
    $sdkBase = "${env:ProgramFiles(x86)}\Windows Kits\10"
    if (-not (Test-Path $sdkBase)) { return $null, $null }
    $versions = Get-ChildItem "$sdkBase\Include" -Directory | Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' } | Sort-Object Name -Descending
    if ($versions.Count -eq 0) { return $null, $null }
    $sdkVer = $versions[0].Name
    return "$sdkBase\Include\$sdkVer", "$sdkBase\Lib\$sdkVer"
}

$vcTools = Find-MsvcTools
if (-not $vcTools) {
    Write-Host "SKIP: MSVC toolchain not found"
    exit 0
}

if (-not $WinSdkInclude -or -not $WinSdkLib) {
    $sdkInc, $sdkLib = Find-WinSdk
    if (-not $WinSdkInclude) { $WinSdkInclude = $sdkInc }
    if (-not $WinSdkLib) { $WinSdkLib = $sdkLib }
}

if (-not $WinSdkInclude -or -not $WinSdkLib) {
    Write-Host "SKIP: Windows SDK not found"
    exit 0
}

# Determine target arch
$arch = if ([Environment]::Is64BitOperatingSystem) { "x64" } else { "x86" }
$cl = "$vcTools\bin\Hostx64\$arch\cl.exe"
$link = "$vcTools\bin\Hostx64\$arch\link.exe"

if (-not (Test-Path $cl)) {
    Write-Host "SKIP: cl.exe not found at $cl"
    exit 0
}

# Build include/lib paths
$vcInc = "$vcTools\include"
$vcLib = "$vcTools\lib\$arch"
$umInc = "$WinSdkInclude\um"
$sharedInc = "$WinSdkInclude\shared"
$ucrtInc = "$WinSdkInclude\ucrt"
$winrtInc = "$WinSdkInclude\winrt"
$umLib = "$WinSdkLib\um\$arch"
$ucrtLib = "$WinSdkLib\ucrt\$arch"

$includes = @("/I`"$vcInc`"", "/I`"$umInc`"", "/I`"$sharedInc`"", "/I`"$ucrtInc`"", "/I`"$winrtInc`"", "/I`"$genDir`"", "/I`"$repoRoot\include`"")
$libpaths = @("/LIBPATH:`"$vcLib`"", "/LIBPATH:`"$umLib`"", "/LIBPATH:`"$ucrtLib`"")

Write-Host "=== Out-of-Proc Custom Marshal E2E Test ===" -ForegroundColor Cyan
Write-Host "MSVC: $vcTools"
Write-Host "SDK Include: $WinSdkInclude"
Write-Host "SDK Lib: $WinSdkLib"
Write-Host "Arch: $arch"
Write-Host ""

# ---------- Step 0: Kill stale processes ----------

Get-Process -Name "Midl.Cli" -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.Id -Force }
Get-Process -Name "CalcServer" -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.Id -Force }

# ---------- Step 1: Generate code from IDL ----------

Write-Host "--- Step 1: Generate proxy code from IDL ---" -ForegroundColor Yellow

New-Item -ItemType Directory -Force $genDir | Out-Null
New-Item -ItemType Directory -Force $buildDir | Out-Null

$cliProject = "$repoRoot/src/Midl.Cli/Midl.Cli.csproj"

$cliOutput = & dotnet run --project $cliProject --no-build -- `
    /winrt /nologo `
    /h TestCalculator.h `
    /iid TestCalculator_i.c `
    /cppproxy TestCalculator_proxy.cpp `
    /noproxy /no_dlldata `
    /out $genDir `
    "$testDir/TestCalculator.idl" 2>&1

$cliOutput | Write-Host

if (-not (Test-Path "$genDir/TestCalculator_proxy.cpp")) {
    Write-Host "FAIL: Proxy code not generated" -ForegroundColor Red
    exit 1
}

Write-Host "  Generated: TestCalculator.h, TestCalculator_proxy.cpp" -ForegroundColor Green

# ---------- Step 2: Compile ----------

Write-Host ""
Write-Host "--- Step 2: Compile server, proxy DLL, client ---" -ForegroundColor Yellow

$commonFlags = @("/nologo", "/EHsc", "/std:c++latest", "/W3", "/D", "WIN32_LEAN_AND_MEAN", "/D", "_WIN32_WINNT=0x0A00")

# 2a: Compile proxy DLL
Write-Host "  Compiling TestCalculatorProxy.dll..."
$proxyObj = "$buildDir/TestCalculator_proxy.obj"
& $cl @commonFlags @includes /c /Fo"$proxyObj" "$genDir/TestCalculator_proxy.cpp" 2>&1 | Write-Host
if ($LASTEXITCODE -ne 0) { Write-Host "FAIL: Proxy compilation failed" -ForegroundColor Red; exit 1 }

& $link /nologo /DLL /OUT:"$buildDir/TestCalculatorProxy.dll" /DEF:"$testDir/TestCalculatorProxy.def" `
    @libpaths $proxyObj ole32.lib oleaut32.lib rpcrt4.lib advapi32.lib user32.lib windowsapp.lib 2>&1 | Write-Host
if ($LASTEXITCODE -ne 0) { Write-Host "FAIL: Proxy DLL link failed" -ForegroundColor Red; exit 1 }

Write-Host "  -> TestCalculatorProxy.dll" -ForegroundColor Green

# 2b: Compile server EXE
Write-Host "  Compiling CalcServer.exe..."
$serverObj = "$buildDir/CalcServer.obj"
& $cl @commonFlags @includes /c /Fo"$serverObj" "$testDir/CalcServer.cpp" 2>&1 | Write-Host
if ($LASTEXITCODE -ne 0) { Write-Host "FAIL: Server compilation failed" -ForegroundColor Red; exit 1 }

& $link /nologo /SUBSYSTEM:CONSOLE /OUT:"$buildDir/CalcServer.exe" `
    @libpaths $serverObj ole32.lib oleaut32.lib windowsapp.lib 2>&1 | Write-Host
if ($LASTEXITCODE -ne 0) { Write-Host "FAIL: Server link failed" -ForegroundColor Red; exit 1 }

Write-Host "  -> CalcServer.exe" -ForegroundColor Green

# 2c: Compile client EXE
Write-Host "  Compiling CalcClient.exe..."
$clientObj = "$buildDir/CalcClient.obj"
& $cl @commonFlags @includes /c /Fo"$clientObj" "$testDir/CalcClient.cpp" 2>&1 | Write-Host
if ($LASTEXITCODE -ne 0) { Write-Host "FAIL: Client compilation failed" -ForegroundColor Red; exit 1 }

& $link /nologo /SUBSYSTEM:CONSOLE /OUT:"$buildDir/CalcClient.exe" `
    @libpaths $clientObj ole32.lib oleaut32.lib windowsapp.lib 2>&1 | Write-Host
if ($LASTEXITCODE -ne 0) { Write-Host "FAIL: Client link failed" -ForegroundColor Red; exit 1 }

Write-Host "  -> CalcClient.exe" -ForegroundColor Green

# ---------- Step 3: Register proxy DLL and server EXE ----------

Write-Host ""
Write-Host "--- Step 3: Register proxy DLL + server ---" -ForegroundColor Yellow

$proxyDllPath = (Resolve-Path "$buildDir/TestCalculatorProxy.dll").Path
$serverExePath = (Resolve-Path "$buildDir/CalcServer.exe").Path

# Extract IIDs and CLSIDs dynamically from generated code
$iidContent = Get-Content "$genDir/TestCalculator_i.c" -Raw

function Extract-Guid($content, $name) {
    if ($content -match "$name,0x([0-9a-fA-F]+),0x([0-9a-fA-F]+),0x([0-9a-fA-F]+),0x([0-9a-fA-F]+),0x([0-9a-fA-F]+),0x([0-9a-fA-F]+),0x([0-9a-fA-F]+),0x([0-9a-fA-F]+),0x([0-9a-fA-F]+),0x([0-9a-fA-F]+),0x([0-9a-fA-F]+)") {
        return "{$($Matches[1])-$($Matches[2])-$($Matches[3])-$($Matches[4])$($Matches[5])-$($Matches[6])$($Matches[7])$($Matches[8])$($Matches[9])$($Matches[10])$($Matches[11])}".ToUpper()
    }
    return $null
}

$IID_ICalculator = Extract-Guid $iidContent "IID___x_TestApp_OutOfProc_ICalculator,"
$IID_ICalculator2 = Extract-Guid $iidContent "IID___x_TestApp_OutOfProc_ICalculator2,"

if (-not $IID_ICalculator) {
    Write-Host "FAIL: Could not extract IID_ICalculator from generated code" -ForegroundColor Red
    exit 1
}

# CLSID_Calculator — extract from server source (must match)
$CLSID_Calculator = "{B340F2FC-5C14-FD5E-A7F2-BE8DC2D67C04}"

# Read the proxy CLSID from the generated code
$proxyContent = Get-Content "$genDir/TestCalculator_proxy.cpp" -Raw

if ($proxyContent -match 'CLSID_PSFactoryBuffer\s*=\s*\{\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*\{\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+),\s*(0x[0-9a-fA-F]+)\s*\}') {
    $a = [uint32]($Matches[1])
    $b = [uint16]($Matches[2])
    $c = [uint16]($Matches[3])
    $d = @(4..11 | ForEach-Object { [byte]($Matches[$_]) })
    $lastSeg = (($d[2..7] | ForEach-Object { $_.ToString("X2") }) -join '')
    $CLSID_PSFactory = "{$($a.ToString("X8"))-$($b.ToString("X4"))-$($c.ToString("X4"))-$($d[0].ToString("X2"))$($d[1].ToString("X2"))-$lastSeg}"
} else {
    Write-Host "FAIL: Could not parse CLSID_PSFactoryBuffer from generated proxy" -ForegroundColor Red
    exit 1
}

Write-Host "  CLSID_Calculator:    $CLSID_Calculator"
Write-Host "  CLSID_PSFactory:     $CLSID_PSFactory"
Write-Host "  IID_ICalculator:     $IID_ICalculator"
Write-Host "  IID_ICalculator2:    $IID_ICalculator2"

# Register under HKCU (no admin needed)
$regBase = "HKCU:\Software\Classes"

# 3a: Register Calculator CLSID → LocalServer32 (EXE server)
$clsidKey = "$regBase\CLSID\$CLSID_Calculator\LocalServer32"
New-Item -Path $clsidKey -Force | Out-Null
Set-ItemProperty -Path $clsidKey -Name "(Default)" -Value $serverExePath
Write-Host "  Registered Calculator LocalServer32" -ForegroundColor Green

# 3b: Register proxy/stub factory CLSID → InprocServer32
$psClsidKey = "$regBase\CLSID\$CLSID_PSFactory\InprocServer32"
New-Item -Path $psClsidKey -Force | Out-Null
Set-ItemProperty -Path $psClsidKey -Name "(Default)" -Value $proxyDllPath
Set-ItemProperty -Path $psClsidKey -Name "ThreadingModel" -Value "Both"
Write-Host "  Registered PSFactory InprocServer32" -ForegroundColor Green

# 3c: Register IID_ICalculator → ProxyStubClsid32
$iidKey = "$regBase\Interface\$IID_ICalculator\ProxyStubClsid32"
New-Item -Path $iidKey -Force | Out-Null
Set-ItemProperty -Path $iidKey -Name "(Default)" -Value $CLSID_PSFactory
Write-Host "  Registered ICalculator ProxyStubClsid32" -ForegroundColor Green

# 3d: Register IID_ICalculator2 → ProxyStubClsid32 (synthesized default interface)
if ($IID_ICalculator2) {
    $iid2Key = "$regBase\Interface\$IID_ICalculator2\ProxyStubClsid32"
    New-Item -Path $iid2Key -Force | Out-Null
    Set-ItemProperty -Path $iid2Key -Name "(Default)" -Value $CLSID_PSFactory
    Write-Host "  Registered ICalculator2 ProxyStubClsid32" -ForegroundColor Green
}

# ---------- Step 4: Start server process ----------

Write-Host ""
Write-Host "--- Step 4: Start server process ---" -ForegroundColor Yellow

$serverProc = Start-Process -FilePath $serverExePath -PassThru -NoNewWindow
Write-Host "  Server PID: $($serverProc.Id)"
Start-Sleep -Seconds 2  # Give server time to register class factory

if ($serverProc.HasExited) {
    Write-Host "FAIL: Server exited prematurely with code $($serverProc.ExitCode)" -ForegroundColor Red
    # Cleanup registration
    Remove-Item -Path "$regBase\CLSID\$CLSID_Calculator" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "$regBase\CLSID\$CLSID_PSFactory" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "$regBase\Interface\$IID_ICalculator" -Recurse -Force -ErrorAction SilentlyContinue
    if ($IID_ICalculator2) { Remove-Item -Path "$regBase\Interface\$IID_ICalculator2" -Recurse -Force -ErrorAction SilentlyContinue }
    exit 1
}

# ---------- Step 5: Run client ----------

Write-Host ""
Write-Host "--- Step 5: Run client ---" -ForegroundColor Yellow

$clientExePath = (Resolve-Path "$buildDir/CalcClient.exe").Path
$clientProc = Start-Process -FilePath $clientExePath -PassThru -NoNewWindow -Wait
$clientExit = $clientProc.ExitCode

Write-Host ""

# ---------- Step 6: Kill server ----------

Write-Host "--- Step 6: Kill server ---" -ForegroundColor Yellow

if (-not $serverProc.HasExited) {
    Stop-Process -Id $serverProc.Id -Force -ErrorAction SilentlyContinue
    Write-Host "  Server killed" -ForegroundColor Green
} else {
    Write-Host "  Server already exited" -ForegroundColor Green
}

# ---------- Step 7: Unregister ----------

Write-Host ""
Write-Host "--- Step 7: Unregister ---" -ForegroundColor Yellow

Remove-Item -Path "$regBase\CLSID\$CLSID_Calculator" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$regBase\CLSID\$CLSID_PSFactory" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$regBase\Interface\$IID_ICalculator" -Recurse -Force -ErrorAction SilentlyContinue
if ($IID_ICalculator2) {
    Remove-Item -Path "$regBase\Interface\$IID_ICalculator2" -Recurse -Force -ErrorAction SilentlyContinue
}
Write-Host "  Registry entries removed" -ForegroundColor Green

# ---------- Step 8: Report ----------

Write-Host ""
Write-Host "--- Results ---" -ForegroundColor Yellow

if ($clientExit -eq 0) {
    Write-Host "ALL TESTS PASSED" -ForegroundColor Green
    exit 0
} else {
    Write-Host "TESTS FAILED (exit code $clientExit)" -ForegroundColor Red
    exit 1
}
