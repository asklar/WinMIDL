<#
.SYNOPSIS
    Performance benchmark: WinMIDL vs original midl.exe

.DESCRIPTION
    Compiles IDL files with both compilers and compares timing.
    Uses NativeAOT binary for fair comparison. Run publish-optimized.ps1 first.

.PARAMETER WinMidlPath
    Explicit path to WinMIDL binary. Auto-detected if not specified.

.PARAMETER IdlFiles
    Path(s) to IDL files to benchmark. Defaults to tests/fixtures/valid/*.idl

.PARAMETER Iterations
    Number of iterations per file (default: 5)

.PARAMETER PublishFirst
    Build NativeAOT release binary before benchmarking

.PARAMETER OriginalMidl
    Path to original midl.exe (auto-detected from Windows SDK)
#>
[CmdletBinding()]
param(
    [string]$WinMidlPath,
    [string[]]$IdlFiles,
    [int]$Iterations = 5,
    [switch]$PublishFirst,
    [string]$OriginalMidl
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot

# ── Helpers ──────────────────────────────────────────────────────────────────

function Write-Header([string]$text) {
    $line = '═' * 70
    Write-Host "`n╔$line╗" -ForegroundColor Cyan
    Write-Host "║ $($text.PadRight(68)) ║" -ForegroundColor Cyan
    Write-Host "╚$line╝" -ForegroundColor Cyan
}

function Get-Stats([double[]]$values) {
    $sorted = $values | Sort-Object
    $min = $sorted[0]
    $max = $sorted[-1]
    $avg = ($sorted | Measure-Object -Average).Average
    $mid = [math]::Floor($sorted.Count / 2)
    $median = if ($sorted.Count % 2 -eq 0) {
        ($sorted[$mid - 1] + $sorted[$mid]) / 2
    } else {
        $sorted[$mid]
    }
    [PSCustomObject]@{ Min = $min; Max = $max; Avg = $avg; Median = $median }
}

function Format-Ms([double]$ms) {
    '{0,8:N1}ms' -f $ms
}

# ── Locate original midl.exe ────────────────────────────────────────────────

function Find-OriginalMidl {
    if ($OriginalMidl -and (Test-Path $OriginalMidl)) {
        return $OriginalMidl
    }

    $searchRoots = @()
    if ($env:WindowsSdkDir) {
        $searchRoots += "$env:WindowsSdkDir\bin"
    }
    $searchRoots += 'C:\Program Files (x86)\Windows Kits\10\bin'

    foreach ($root in $searchRoots) {
        if (-not (Test-Path $root)) { continue }
        $found = Get-ChildItem -Path $root -Recurse -Filter 'midl.exe' -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match 'x64[\\/]midl\.exe$' } |
            Where-Object { $_.Directory.Parent.Name -ne '10.0.26546.0' } |
            Sort-Object { $_.Directory.Parent.Name } -Descending |
            Select-Object -First 1
        if ($found) { return $found.FullName }
    }
    return $null
}

# ── Build WinMIDL ───────────────────────────────────────────────────────────

function Build-WinMidl {
    Write-Header 'Building WinMIDL (NativeAOT Release)'

    $publishDir = Join-Path $repoRoot 'publish\win-x64'

    # Try NativeAOT publish first
    Write-Host 'Attempting NativeAOT publish...' -ForegroundColor Yellow
    $aotResult = & dotnet publish "$repoRoot\src\Midl.Cli" -c Release -r win-x64 `
        -p:PublishAot=true -o $publishDir 2>&1
    $aotExe = Join-Path $publishDir 'Midl.Cli.exe'

    if ($LASTEXITCODE -eq 0 -and (Test-Path $aotExe)) {
        Write-Host "NativeAOT build succeeded: $aotExe" -ForegroundColor Green
        return $aotExe
    }

    # Fall back to Release build
    Write-Host 'NativeAOT failed, falling back to Release build...' -ForegroundColor Yellow
    & dotnet publish "$repoRoot\src\Midl.Cli" -c Release -r win-x64 -o $publishDir 2>&1 | Out-Null

    if ($LASTEXITCODE -eq 0 -and (Test-Path $aotExe)) {
        Write-Host "Release build succeeded: $aotExe" -ForegroundColor Green
        return $aotExe
    }

    # Last resort: just build
    Write-Host 'Publish failed, falling back to dotnet build...' -ForegroundColor Yellow
    & dotnet build "$repoRoot\src\Midl.Cli" -c Release 2>&1 | Out-Null
    $buildExe = Get-ChildItem -Path "$repoRoot\src\Midl.Cli\bin\Release" -Recurse -Filter 'Midl.Cli.exe' |
        Select-Object -First 1
    if ($buildExe) {
        Write-Host "Build succeeded: $($buildExe.FullName)" -ForegroundColor Green
        return $buildExe.FullName
    }

    Write-Error 'Failed to build WinMIDL. Run with -PublishFirst or build manually.'
    return $null
}

# ── Find WinMIDL binary ────────────────────────────────────────────────────

function Find-WinMidl {
    # Explicit path takes priority
    if ($WinMidlPath -and (Test-Path $WinMidlPath)) { return $WinMidlPath }

    # Check AOT published binary (preferred — smallest, fastest)
    $aotExe = Join-Path $repoRoot 'publish\aot-x64\Midl.Cli.exe'
    if (Test-Path $aotExe) { return $aotExe }

    # Check standard published binary
    $publishExe = Join-Path $repoRoot 'publish\win-x64\Midl.Cli.exe'
    if (Test-Path $publishExe) { return $publishExe }

    # Check Release build output
    $buildExe = Get-ChildItem -Path "$repoRoot\src\Midl.Cli\bin\Release" -Recurse -Filter 'Midl.Cli.exe' -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($buildExe) { return $buildExe.FullName }

    return $null
}

# ── Benchmark a single file ────────────────────────────────────────────────

function Measure-Compilation([string]$exe, [string]$file, [string[]]$extraArgs, [int]$runs) {
    $times = @()
    for ($i = 0; $i -lt $runs; $i++) {
        $elapsed = Measure-Command {
            & $exe /Zs /nologo $extraArgs $file 2>&1 | Out-Null
        }
        $times += $elapsed.TotalMilliseconds
    }
    return $times
}

# ══════════════════════════════════════════════════════════════════════════════
# Main
# ══════════════════════════════════════════════════════════════════════════════

Write-Header 'WinMIDL Performance Benchmark'

# Build if requested
if ($PublishFirst) {
    $winmidlExe = Build-WinMidl
    if (-not $winmidlExe) { exit 1 }
} else {
    $winmidlExe = Find-WinMidl
    if (-not $winmidlExe) {
        Write-Host 'WinMIDL binary not found. Run with -PublishFirst or build manually.' -ForegroundColor Red
        Write-Host "Expected at: $(Join-Path $repoRoot 'publish\win-x64\Midl.Cli.exe')" -ForegroundColor Yellow
        exit 1
    }
}

# Locate original midl.exe
$origMidl = Find-OriginalMidl
$hasOriginal = $null -ne $origMidl

if ($hasOriginal) {
    Write-Host "Original midl.exe: $origMidl" -ForegroundColor Green
} else {
    Write-Host 'Original midl.exe not found. Benchmarking WinMIDL only.' -ForegroundColor Yellow
}

Write-Host "WinMIDL:           $winmidlExe" -ForegroundColor Green
Write-Host "Iterations:        $Iterations" -ForegroundColor Gray

# Resolve IDL files
if (-not $IdlFiles) {
    $fixtureDir = Join-Path $repoRoot 'tests\fixtures'
    $validDir = Join-Path $fixtureDir 'valid'
    $errorDir = Join-Path $fixtureDir 'errors'
    $externalDir = Join-Path $fixtureDir 'external'

    $validFiles = @()
    $errorFiles = @()

    if (Test-Path $validDir) {
        $validFiles = Get-ChildItem -Path $validDir -Filter '*.idl' | Select-Object -ExpandProperty FullName
    }
    if (Test-Path $errorDir) {
        $errorFiles = Get-ChildItem -Path $errorDir -Filter '*.idl' | Select-Object -ExpandProperty FullName
    }
    if (Test-Path $externalDir) {
        $validFiles += Get-ChildItem -Path $externalDir -Filter '*.idl' | Select-Object -ExpandProperty FullName
    }

    $IdlFiles = $validFiles + $errorFiles
}

if (-not $IdlFiles -or $IdlFiles.Count -eq 0) {
    Write-Host 'No IDL files found to benchmark.' -ForegroundColor Red
    exit 1
}

# Separate valid vs error files for reporting
if (-not $validFiles) { $validFiles = $IdlFiles }
if (-not $errorFiles) { $errorFiles = @() }

Write-Host "Files:             $($IdlFiles.Count) total ($($validFiles.Count) valid, $($errorFiles.Count) error)" -ForegroundColor Gray

# ── Cold start test ─────────────────────────────────────────────────────────

Write-Header 'Cold Start Test (first invocation)'

$firstFile = $IdlFiles[0]
$coldWinmidl = (Measure-Command { & $winmidlExe /Zs /nologo /no_strict $firstFile 2>&1 | Out-Null }).TotalMilliseconds

if ($hasOriginal) {
    $coldOrig = (Measure-Command { & $origMidl /Zs /nologo /winrt $firstFile 2>&1 | Out-Null }).TotalMilliseconds
    Write-Host ("  WinMIDL cold start:  {0,8:N1}ms" -f $coldWinmidl)
    Write-Host ("  Original cold start: {0,8:N1}ms" -f $coldOrig)
    $coldRatio = if ($coldWinmidl -gt 0) { $coldOrig / $coldWinmidl } else { 0 }
    Write-Host ("  Ratio:               {0:N1}x" -f $coldRatio)
} else {
    Write-Host ("  WinMIDL cold start:  {0,8:N1}ms" -f $coldWinmidl)
}

# ── Per-file benchmark ──────────────────────────────────────────────────────

Write-Header "Per-File Benchmark ($Iterations iterations each)"

$results = @()

foreach ($file in $IdlFiles) {
    $name = [IO.Path]::GetFileName($file)
    $category = if ($file -in $errorFiles) { 'ERR' } else { 'OK ' }
    Write-Host "  [$category] $name" -ForegroundColor Gray -NoNewline

    $winTimes = Measure-Compilation $winmidlExe $file @('/no_strict') $Iterations
    $winStats = Get-Stats $winTimes

    $origStats = $null
    if ($hasOriginal) {
        $origTimes = Measure-Compilation $origMidl $file @('/winrt') $Iterations
        $origStats = Get-Stats $origTimes
    }

    $ratio = if ($origStats -and $winStats.Median -gt 0) {
        $origStats.Median / $winStats.Median
    } else { 0 }

    $results += [PSCustomObject]@{
        File        = $name
        WinMedian   = $winStats.Median
        WinMin      = $winStats.Min
        WinMax      = $winStats.Max
        WinAvg      = $winStats.Avg
        OrigMedian  = if ($origStats) { $origStats.Median } else { $null }
        OrigMin     = if ($origStats) { $origStats.Min } else { $null }
        OrigMax     = if ($origStats) { $origStats.Max } else { $null }
        OrigAvg     = if ($origStats) { $origStats.Avg } else { $null }
        Ratio       = $ratio
    }

    Write-Host " done" -ForegroundColor DarkGray
}

# ── Results table ───────────────────────────────────────────────────────────

Write-Header 'Results (median times)'

$nameWidth = ($results | ForEach-Object { $_.File.Length } | Measure-Object -Maximum).Maximum
$nameWidth = [math]::Max($nameWidth, 4)

if ($hasOriginal) {
    $border  = '╔' + ('═' * ($nameWidth + 2)) + '╦═══════════╦═══════════╦═════════╗'
    $header  = '║ ' + 'File'.PadRight($nameWidth) + ' ║  WinMIDL  ║ Original  ║  Ratio  ║'
    $sep     = '╠' + ('═' * ($nameWidth + 2)) + '╬═══════════╬═══════════╬═════════╣'
    $bottom  = '╚' + ('═' * ($nameWidth + 2)) + '╩═══════════╩═══════════╩═════════╝'

    Write-Host $border -ForegroundColor Cyan
    Write-Host $header -ForegroundColor Cyan
    Write-Host $sep -ForegroundColor Cyan

    foreach ($r in $results) {
        $winStr  = '{0,7:N1}ms' -f $r.WinMedian
        $origStr = '{0,7:N1}ms' -f $r.OrigMedian
        $ratStr  = '{0,5:N1}x' -f $r.Ratio
        $color = if ($r.Ratio -ge 1) { 'Green' } else { 'Red' }
        Write-Host ("║ {0} ║ {1} ║ {2} ║ {3}  ║" -f $r.File.PadRight($nameWidth), $winStr, $origStr, $ratStr) -ForegroundColor $color
    }

    Write-Host $bottom -ForegroundColor Cyan
} else {
    $border  = '╔' + ('═' * ($nameWidth + 2)) + '╦═══════════╦═══════════╦═══════════╗'
    $header  = '║ ' + 'File'.PadRight($nameWidth) + ' ║   Min     ║  Median   ║    Max    ║'
    $sep     = '╠' + ('═' * ($nameWidth + 2)) + '╬═══════════╬═══════════╬═══════════╣'
    $bottom  = '╚' + ('═' * ($nameWidth + 2)) + '╩═══════════╩═══════════╩═══════════╝'

    Write-Host $border -ForegroundColor Cyan
    Write-Host $header -ForegroundColor Cyan
    Write-Host $sep -ForegroundColor Cyan

    foreach ($r in $results) {
        $minStr = '{0,7:N1}ms' -f $r.WinMin
        $medStr = '{0,7:N1}ms' -f $r.WinMedian
        $maxStr = '{0,7:N1}ms' -f $r.WinMax
        Write-Host ("║ {0} ║ {1} ║ {2} ║ {3} ║" -f $r.File.PadRight($nameWidth), $minStr, $medStr, $maxStr) -ForegroundColor Green
    }

    Write-Host $bottom -ForegroundColor Cyan
}

# ── Batch test ──────────────────────────────────────────────────────────────

Write-Header 'Batch Test (all files sequentially)'

$batchWinTimes = @()
for ($i = 0; $i -lt $Iterations; $i++) {
    $elapsed = Measure-Command {
        foreach ($file in $IdlFiles) {
            & $winmidlExe /Zs /nologo /no_strict $file 2>&1 | Out-Null
        }
    }
    $batchWinTimes += $elapsed.TotalMilliseconds
}
$batchWinStats = Get-Stats $batchWinTimes
Write-Host ("  WinMIDL batch ({0} files x{1}): median {2:N1}ms, min {3:N1}ms, max {4:N1}ms" -f `
    $IdlFiles.Count, $Iterations, $batchWinStats.Median, $batchWinStats.Min, $batchWinStats.Max)

if ($hasOriginal) {
    $batchOrigTimes = @()
    for ($i = 0; $i -lt $Iterations; $i++) {
        $elapsed = Measure-Command {
            foreach ($file in $IdlFiles) {
                & $origMidl /Zs /nologo /winrt $file 2>&1 | Out-Null
            }
        }
        $batchOrigTimes += $elapsed.TotalMilliseconds
    }
    $batchOrigStats = Get-Stats $batchOrigTimes
    Write-Host ("  Original batch ({0} files x{1}): median {2:N1}ms, min {3:N1}ms, max {4:N1}ms" -f `
        $IdlFiles.Count, $Iterations, $batchOrigStats.Median, $batchOrigStats.Min, $batchOrigStats.Max)

    $batchRatio = if ($batchWinStats.Median -gt 0) { $batchOrigStats.Median / $batchWinStats.Median } else { 0 }
    Write-Host ("  Batch ratio: {0:N1}x" -f $batchRatio) -ForegroundColor $(if ($batchRatio -ge 1) { 'Green' } else { 'Red' })
}

# ── Valid vs Error Performance ───────────────────────────────────────────────

if ($validFiles.Count -gt 0 -and $errorFiles.Count -gt 0) {
    Write-Header 'Valid vs Error File Performance'

    $validResults = $results | Where-Object { $_.File -in ($validFiles | ForEach-Object { [IO.Path]::GetFileName($_) }) }
    $errorResults = $results | Where-Object { $_.File -in ($errorFiles | ForEach-Object { [IO.Path]::GetFileName($_) }) }

    $avgValid = if ($validResults.Count -gt 0) { ($validResults | Measure-Object -Property WinMedian -Average).Average } else { 0 }
    $avgError = if ($errorResults.Count -gt 0) { ($errorResults | Measure-Object -Property WinMedian -Average).Average } else { 0 }

    Write-Host ("  Valid files ({0}):   avg {1:N1}ms" -f $validResults.Count, $avgValid) -ForegroundColor Green
    Write-Host ("  Error files ({0}):   avg {1:N1}ms" -f $errorResults.Count, $avgError) -ForegroundColor Yellow

    if ($avgValid -gt 0) {
        $errorOverhead = (($avgError - $avgValid) / $avgValid) * 100
        $sign = if ($errorOverhead -ge 0) { '+' } else { '' }
        Write-Host ("  Error overhead:     {0}{1:N0}%" -f $sign, $errorOverhead) -ForegroundColor $(if ($errorOverhead -gt 20) { 'Red' } else { 'Gray' })
    }
}

# ── Summary ─────────────────────────────────────────────────────────────────

Write-Header 'Summary'

$avgWinMedian = ($results | Measure-Object -Property WinMedian -Average).Average
Write-Host ("  Average WinMIDL compilation: {0:N1}ms" -f $avgWinMedian)

if ($hasOriginal) {
    $avgOrigMedian = ($results | Where-Object { $null -ne $_.OrigMedian } | Measure-Object -Property OrigMedian -Average).Average
    $avgRatio = ($results | Where-Object { $_.Ratio -gt 0 } | Measure-Object -Property Ratio -Average).Average
    Write-Host ("  Average original midl.exe:   {0:N1}ms" -f $avgOrigMedian)
    Write-Host ("  Average speedup:             {0:N1}x" -f $avgRatio) -ForegroundColor $(if ($avgRatio -ge 1) { 'Green' } else { 'Red' })
}

Write-Host ''
