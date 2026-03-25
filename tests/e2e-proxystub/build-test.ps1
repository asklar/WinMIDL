<#
.SYNOPSIS
    End-to-end compilation test for generated MIDL headers and proxy/stub code.
.DESCRIPTION
    1. Runs our CLI to generate .h, _i.c, _p.c from TestWidget.idl
    2. Compiles a C++ file that #includes the .h
    3. Compiles _i.c (validates IID definitions)
    4. Compiles _p.c (validates proxy/stub code)
    5. Compiles a C file that #includes the .h (validates C block)
    Returns exit code 0 only if ALL files compile.
#>
param(
    [string]$OutDir = (Join-Path $PSScriptRoot "out"),
    [string]$CliProject = (Join-Path $PSScriptRoot "..\..\src\Midl.Cli\Midl.Cli.csproj")
)

$ErrorActionPreference = "Stop"
$script:failures = @()

function Write-Status($msg) { Write-Host "  $msg" -ForegroundColor Cyan }
function Write-Pass($msg) { Write-Host "  PASS: $msg" -ForegroundColor Green }
function Write-Fail($msg) { Write-Host "  FAIL: $msg" -ForegroundColor Red; $script:failures += $msg }

# --- Find cl.exe ---
function Find-ClExe {
    # Check if already in PATH (e.g., Developer Command Prompt)
    $cl = Get-Command cl.exe -ErrorAction SilentlyContinue
    if ($cl) { return $cl.Source }

    # Try vswhere
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $vsPath = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath 2>$null
        if ($vsPath) {
            $vcToolsVerFile = Join-Path $vsPath "VC\Auxiliary\Build\Microsoft.VCToolsVersion.default.txt"
            if (Test-Path $vcToolsVerFile) {
                $vcVer = (Get-Content $vcToolsVerFile).Trim()
                $cl = Join-Path $vsPath "VC\Tools\MSVC\$vcVer\bin\Hostx64\x64\cl.exe"
                if (Test-Path $cl) { return $cl }
            }
        }
    }

    return $null
}

function Get-MsvcEnv {
    # If we already have INCLUDE set, we're in a dev prompt
    if ($env:INCLUDE) { return @{ INCLUDE = $env:INCLUDE; LIB = $env:LIB } }

    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (-not (Test-Path $vswhere)) { return $null }

    $vsPath = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath 2>$null
    if (-not $vsPath) { return $null }

    # Source vcvarsall and capture environment
    $vcvars = Join-Path $vsPath "VC\Auxiliary\Build\vcvarsall.bat"
    if (-not (Test-Path $vcvars)) { return $null }

    $envOutput = cmd /c "`"$vcvars`" x64 >nul 2>&1 && set" 2>$null
    $envMap = @{}
    foreach ($line in $envOutput) {
        if ($line -match '^([^=]+)=(.*)$') {
            $envMap[$matches[1]] = $matches[2]
        }
    }

    return @{ INCLUDE = $envMap["INCLUDE"]; LIB = $envMap["LIB"]; PATH = $envMap["Path"] }
}

# --- Compile helper ---
function Invoke-Cl {
    param(
        [string]$ClExe,
        [string]$SourceFile,
        [string]$Description,
        [string[]]$ExtraArgs = @(),
        [hashtable]$Env = @{}
    )

    $args = @("/nologo", "/c", "/W3", "/WX-") + $ExtraArgs + @("/Fo$OutDir\", $SourceFile)
    Write-Status "cl.exe $Description ..."

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $ClExe
    $psi.Arguments = $args -join " "
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.CreateNoWindow = $true
    $psi.WorkingDirectory = $OutDir

    if ($Env.INCLUDE) { $psi.Environment["INCLUDE"] = $Env.INCLUDE }
    if ($Env.LIB) { $psi.Environment["LIB"] = $Env.LIB }
    if ($Env.PATH) { $psi.Environment["Path"] = $Env.PATH }

    $proc = [System.Diagnostics.Process]::Start($psi)
    $stdout = $proc.StandardOutput.ReadToEnd()
    $stderr = $proc.StandardError.ReadToEnd()
    $proc.WaitForExit(60000)

    if ($proc.ExitCode -eq 0) {
        Write-Pass $Description
        return $true
    } else {
        Write-Fail $Description
        if ($stdout) { Write-Host $stdout }
        if ($stderr) { Write-Host $stderr }
        return $false
    }
}

# --- Main ---
Write-Host "`n=== E2E Proxy/Stub Compilation Test ===" -ForegroundColor Yellow

# Prepare output directory
if (Test-Path $OutDir) { Remove-Item $OutDir -Recurse -Force }
New-Item $OutDir -ItemType Directory -Force | Out-Null

# Step 1: Generate files from IDL
Write-Host "`n--- Step 1: Generate .h, _i.c, _p.c ---" -ForegroundColor Yellow
$idlFile = Join-Path $PSScriptRoot "TestWidget.idl"
$dotnetArgs = @(
    "run", "--project", $CliProject, "--",
    "/winrt", "/nologo",
    "/h", "TestWidget.h",
    "/iid", "TestWidget_i.c",
    "/proxy", "TestWidget_p.c",
    "/dlldata", "dlldata.c",
    "/out", $OutDir,
    $idlFile
)

Write-Status "dotnet run -- midl /winrt TestWidget.idl ..."
$result = & dotnet @dotnetArgs 2>&1
$genExitCode = $LASTEXITCODE
if ($genExitCode -gt 1) {
    Write-Fail "Code generation (exit code $genExitCode)"
    Write-Host ($result | Out-String)
    exit 1
}
Write-Pass "Code generation"

# Verify files exist
foreach ($f in @("TestWidget.h", "TestWidget_i.c", "TestWidget_p.c")) {
    if (-not (Test-Path (Join-Path $OutDir $f))) {
        Write-Fail "Expected file not generated: $f"
        exit 1
    }
}

# Step 2: Find MSVC
Write-Host "`n--- Step 2: Find MSVC ---" -ForegroundColor Yellow
$clExe = Find-ClExe
if (-not $clExe) {
    Write-Host "  SKIP: cl.exe not found. Install Visual Studio C++ workload or run from Developer Command Prompt." -ForegroundColor Yellow
    exit 0
}
Write-Pass "Found cl.exe: $clExe"

$msvcEnv = Get-MsvcEnv
if (-not $msvcEnv -or -not $msvcEnv.INCLUDE) {
    Write-Host "  SKIP: Could not set up MSVC environment." -ForegroundColor Yellow
    exit 0
}

# Build include path: output dir (for generated .h) + SDK includes
$extraInclude = $OutDir

# Step 3: Compile C++ test
Write-Host "`n--- Step 3: Compile C++ test ---" -ForegroundColor Yellow
$cppFile = Join-Path $PSScriptRoot "test_cpp_include.cpp"
Invoke-Cl -ClExe $clExe -SourceFile $cppFile -Description "C++ header include test" `
    -ExtraArgs @("/EHsc", "/std:c++17", "/I$extraInclude") -Env $msvcEnv | Out-Null

# Step 4: Compile _i.c
Write-Host "`n--- Step 4: Compile _i.c ---" -ForegroundColor Yellow
$iidFile = Join-Path $OutDir "TestWidget_i.c"
Invoke-Cl -ClExe $clExe -SourceFile $iidFile -Description "IID file (_i.c)" `
    -ExtraArgs @("/TC", "/I$extraInclude") -Env $msvcEnv | Out-Null

# Step 5: Compile _p.c
Write-Host "`n--- Step 5: Compile _p.c ---" -ForegroundColor Yellow
$proxyFile = Join-Path $OutDir "TestWidget_p.c"
Invoke-Cl -ClExe $clExe -SourceFile $proxyFile -Description "Proxy/stub file (_p.c)" `
    -ExtraArgs @("/TC", "/I$extraInclude", "/DREGISTER_PROXY_DLL") -Env $msvcEnv | Out-Null

# Step 6: Compile C test
Write-Host "`n--- Step 6: Compile C test ---" -ForegroundColor Yellow
$cFile = Join-Path $PSScriptRoot "test_c_include.c"
Invoke-Cl -ClExe $clExe -SourceFile $cFile -Description "C header include test" `
    -ExtraArgs @("/TC", "/I$extraInclude") -Env $msvcEnv | Out-Null

# --- Summary ---
Write-Host "`n=== Results ===" -ForegroundColor Yellow
if ($script:failures.Count -eq 0) {
    Write-Host "  All compilation tests passed!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "  $($script:failures.Count) test(s) failed:" -ForegroundColor Red
    foreach ($f in $script:failures) { Write-Host "    - $f" -ForegroundColor Red }
    exit 1
}
