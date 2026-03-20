<#
.SYNOPSIS
    Run MIDL compiler tests against OS IDL fixture files.

.DESCRIPTION
    Parses and compiles IDL files from the MidlRT unittest directory (or any directory
    of IDL files) and reports results. Does NOT copy files — references them in-place.

.PARAMETER FixturesDir
    Path to directory of IDL files. Defaults to the MidlRT unittest directory.

.PARAMETER Mode
    Compilation mode: 'strict' (default) or 'compat'.

.PARAMETER Filter
    Optional glob to filter which IDL files to test (e.g., "Contract.Positive.*").

.PARAMETER ParseOnly
    Only test lexing+parsing (no semantic analysis). Faster, catches parser crashes.

.PARAMETER StopOnCrash
    Stop immediately if any file causes a crash (non-diagnostic failure).

.EXAMPLE
    .\scripts\test-os-fixtures.ps1
    .\scripts\test-os-fixtures.ps1 -FixturesDir "E:\os\src\com\rpc\midl\MidlRT\unittest"
    .\scripts\test-os-fixtures.ps1 -Filter "MoIdl*" -Verbose
    .\scripts\test-os-fixtures.ps1 -Filter "*.Negative.*" -Mode compat
    .\scripts\test-os-fixtures.ps1 -ParseOnly
#>

[CmdletBinding()]
param(
    [string]$FixturesDir = "E:\os\src\com\rpc\midl\MidlRT\unittest",
    [ValidateSet('strict', 'compat')]
    [string]$Mode = "compat",
    [string]$Filter = "*.idl",
    [switch]$ParseOnly,
    [switch]$StopOnCrash
)

$ErrorActionPreference = "Continue"

# Find the MIDL CLI
$repoRoot = Split-Path $PSScriptRoot -Parent
$midlCli = Join-Path $repoRoot "src\Midl.Cli\bin\Debug\net10.0\Midl.Cli.exe"

if (-not (Test-Path $midlCli)) {
    Write-Host "Building MIDL compiler..." -ForegroundColor Yellow
    Push-Location $repoRoot
    dotnet build -v q 2>&1 | Out-Null
    Pop-Location
    if (-not (Test-Path $midlCli)) {
        Write-Error "Failed to build. Run 'dotnet build' from $repoRoot first."
        exit 1
    }
}

if (-not (Test-Path $FixturesDir)) {
    Write-Error "Fixtures directory not found: $FixturesDir"
    Write-Host "Set -FixturesDir to a directory of .idl files, e.g.:"
    Write-Host "  .\scripts\test-os-fixtures.ps1 -FixturesDir 'E:\os\src\com\rpc\midl\MidlRT\unittest'"
    exit 1
}

$idlFiles = Get-ChildItem $FixturesDir -Filter $Filter | Where-Object { $_.Extension -eq ".idl" }
$total = $idlFiles.Count

if ($total -eq 0) {
    Write-Warning "No .idl files found matching '$Filter' in $FixturesDir"
    exit 0
}

Write-Host "Testing $total IDL files from: $FixturesDir" -ForegroundColor Cyan
Write-Host "Mode: $Mode | ParseOnly: $ParseOnly | Filter: $Filter" -ForegroundColor DarkGray
Write-Host ""

$results = @{
    Passed    = 0
    Warnings  = 0
    Errors    = 0
    Crashed   = 0
}
$crashes = @()

$modeSwitch = if ($Mode -eq 'strict') { '/strict' } else { '/no_strict' }

$i = 0
foreach ($file in $idlFiles) {
    $i++
    $pct = [math]::Floor(($i / $total) * 100)
    
    try {
        $output = & $midlCli /Zs /nologo $modeSwitch $file.FullName 2>&1
        $exitCode = $LASTEXITCODE
        
        $errorCount = ($output | Select-String "error MIDL").Count
        $warnCount = ($output | Select-String "warning MIDL").Count
        
        if ($exitCode -gt 2) {
            # Crash (exit code > 2 indicates internal error)
            $results.Crashed++
            $crashes += $file.Name
            Write-Host "  CRASH  $($file.Name)" -ForegroundColor Red
            if ($StopOnCrash) {
                Write-Host "Stopping on crash." -ForegroundColor Red
                break
            }
        }
        elseif ($errorCount -gt 0) {
            $results.Errors++
            if ($VerbosePreference -eq 'Continue') {
                Write-Host "  ERROR  $($file.Name) ($errorCount errors)" -ForegroundColor Yellow
                $output | Select-String "error MIDL" | ForEach-Object { Write-Host "         $_" -ForegroundColor DarkYellow }
            }
        }
        elseif ($warnCount -gt 0) {
            $results.Warnings++
            if ($VerbosePreference -eq 'Continue') {
                Write-Host "  WARN   $($file.Name) ($warnCount warnings)" -ForegroundColor DarkYellow
            }
        }
        else {
            $results.Passed++
            if ($VerbosePreference -eq 'Continue') {
                Write-Host "  OK     $($file.Name)" -ForegroundColor Green
            }
        }
    }
    catch {
        $results.Crashed++
        $crashes += $file.Name
        Write-Host "  CRASH  $($file.Name): $_" -ForegroundColor Red
        if ($StopOnCrash) { break }
    }
    
    # Progress bar (non-verbose)
    if ($VerbosePreference -ne 'Continue' -and ($i % 50 -eq 0 -or $i -eq $total)) {
        Write-Host "`r  [$pct%] $i / $total files processed..." -NoNewline -ForegroundColor DarkGray
    }
}

Write-Host ""
Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  Results: $total files tested" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "  Passed (no errors):  $($results.Passed)" -ForegroundColor Green
Write-Host "  Warnings only:      $($results.Warnings)" -ForegroundColor Yellow
Write-Host "  Errors (expected):   $($results.Errors)" -ForegroundColor DarkYellow
Write-Host "  CRASHED:             $($results.Crashed)" -ForegroundColor $(if ($results.Crashed -gt 0) { 'Red' } else { 'Green' })
Write-Host ""

if ($crashes.Count -gt 0) {
    Write-Host "CRASHED FILES:" -ForegroundColor Red
    foreach ($c in $crashes) {
        Write-Host "  - $c" -ForegroundColor Red
    }
    Write-Host ""
}

# Separate positive vs negative files
$negativeFiles = $idlFiles | Where-Object { $_.Name -match '\.Negative\.' }
$positiveFiles = $idlFiles | Where-Object { $_.Name -notmatch '\.Negative\.' }

Write-Host "  File breakdown:" -ForegroundColor DarkGray
Write-Host "    Positive test files: $($positiveFiles.Count)" -ForegroundColor DarkGray
Write-Host "    Negative test files: $($negativeFiles.Count)" -ForegroundColor DarkGray
Write-Host ""

# The key metric: ZERO crashes
if ($results.Crashed -eq 0) {
    Write-Host "Pass: No crashes! Parser handled all $total files." -ForegroundColor Green
    exit 0
}
else {
    Write-Host "FAIL: $($results.Crashed) file(s) caused crashes." -ForegroundColor Red
    exit 1
}
