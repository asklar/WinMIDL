<#
.SYNOPSIS
    Compile a generated proxy/stub .cpp file into a DLL using MSVC.

.PARAMETER ProxyCpp
    Path to the generated proxy/stub .cpp file.

.PARAMETER DefFile
    Optional .def file for DLL exports. If not provided, looks for one next to the .cpp.

.PARAMETER OutputDir
    Output directory (default: same directory as the .cpp file).

.EXAMPLE
    .\scripts\build-proxy-dll.ps1 TestWidget_proxy.cpp
    .\scripts\build-proxy-dll.ps1 -ProxyCpp out\MyApi_proxy.cpp -OutputDir out\bin
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory, Position = 0)]
    [string]$ProxyCpp,

    [string]$DefFile,
    [string]$OutputDir
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $ProxyCpp)) {
    Write-Error "Proxy source not found: $ProxyCpp"
    exit 1
}

$ProxyCpp = Resolve-Path $ProxyCpp
$baseName = [IO.Path]::GetFileNameWithoutExtension($ProxyCpp)
$srcDir = Split-Path $ProxyCpp

if (-not $OutputDir) { $OutputDir = $srcDir }
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$OutputDir = Resolve-Path $OutputDir

# Auto-discover .def file
if (-not $DefFile) {
    $candidate = Join-Path $srcDir "$baseName.def"
    if (Test-Path $candidate) { $DefFile = $candidate }
}

# --- Find MSVC via vswhere ---
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path $vswhere)) {
    Write-Error "vswhere not found. Install Visual Studio with C++ workload."
    exit 1
}

$vsPath = & $vswhere -prerelease -latest -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
if (-not $vsPath) {
    Write-Error "No Visual Studio with C++ tools found."
    exit 1
}

$vcVersion = (Get-ChildItem "$vsPath\VC\Tools\MSVC" -Directory | Sort-Object Name -Descending | Select-Object -First 1).Name
$vcTools = "$vsPath\VC\Tools\MSVC\$vcVersion"
$clExe = "$vcTools\bin\Hostx64\x64\cl.exe"
$linkExe = "$vcTools\bin\Hostx64\x64\link.exe"

if (-not (Test-Path $clExe)) {
    Write-Error "cl.exe not found at $clExe"
    exit 1
}

# --- Find Windows SDK ---
$sdkRoot = "${env:ProgramFiles(x86)}\Windows Kits\10"
$sdkVer = Get-ChildItem "$sdkRoot\Include" -Directory |
    Where-Object { $_.Name -match '^\d+\.' -and (Test-Path "$($_.FullName)\um\windows.h") } |
    Sort-Object Name -Descending | Select-Object -First 1
if (-not $sdkVer) {
    Write-Error "Windows SDK not found."
    exit 1
}
$sv = $sdkVer.Name

# --- Build environment ---
$includePaths = @(
    "$vcTools\include",
    "$sdkRoot\Include\$sv\ucrt",
    "$sdkRoot\Include\$sv\um",
    "$sdkRoot\Include\$sv\shared",
    "$sdkRoot\Include\$sv\winrt",
    $srcDir
)
$libPaths = @(
    "$vcTools\lib\x64",
    "$sdkRoot\Lib\$sv\ucrt\x64",
    "$sdkRoot\Lib\$sv\um\x64"
)

$includeArgs = $includePaths | ForEach-Object { "/I"; $_ }
$libArgs = $libPaths | ForEach-Object { "/LIBPATH:$_" }

$objFile = Join-Path $OutputDir "$baseName.obj"
$dllFile = Join-Path $OutputDir "$baseName.dll"

Write-Host "Building proxy/stub DLL" -ForegroundColor Cyan
Write-Host "  Source:  $ProxyCpp"
Write-Host "  Output:  $dllFile"
Write-Host "  MSVC:    $vcVersion"
Write-Host "  SDK:     $sv"
Write-Host ""

# --- Compile ---
Write-Host "Compiling..." -ForegroundColor Yellow
$clArgs = @(
    "/nologo", "/c", "/EHsc", "/std:c++latest",
    "/DWIN32", "/D_WINDOWS", "/DUNICODE", "/D_UNICODE",
    "/Fo$objFile"
) + $includeArgs + @($ProxyCpp)

$env:PATH = "$vcTools\bin\Hostx64\x64;$env:PATH"
$clResult = & $clExe @clArgs 2>&1
$clExit = $LASTEXITCODE

if ($clExit -ne 0) {
    Write-Host $clResult -ForegroundColor Red
    Write-Error "Compilation failed (exit code $clExit)"
    exit 1
}
Write-Host "  Compiled OK" -ForegroundColor Green

# --- Link ---
Write-Host "Linking..." -ForegroundColor Yellow
$linkArgs = @(
    "/nologo", "/DLL",
    "/OUT:$dllFile"
) + $libArgs + @(
    "ole32.lib", "oleaut32.lib", "uuid.lib", "runtimeobject.lib", "advapi32.lib",
    $objFile
)

if ($DefFile -and (Test-Path $DefFile)) {
    $linkArgs += "/DEF:$DefFile"
    Write-Host "  Using .def: $DefFile" -ForegroundColor DarkGray
}

$linkResult = & $linkExe @linkArgs 2>&1
$linkExit = $LASTEXITCODE

if ($linkExit -ne 0) {
    Write-Host $linkResult -ForegroundColor Red
    Write-Error "Linking failed (exit code $linkExit)"
    exit 1
}

# --- Report ---
$dllSize = [math]::Round((Get-Item $dllFile).Length / 1KB, 1)
Write-Host "  Linked OK" -ForegroundColor Green
Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  Proxy/stub DLL built!" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  DLL:  $dllFile ($dllSize KB)"
Write-Host ""
Write-Host "Register with:" -ForegroundColor DarkGray
Write-Host "  regsvr32 `"$dllFile`"" -ForegroundColor DarkGray

# Clean up .obj
Remove-Item $objFile -EA SilentlyContinue
