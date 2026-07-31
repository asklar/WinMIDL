# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

<#
.SYNOPSIS
    Compare WinMIDL output against original midl.exe (T3 golden-file testing)

.PARAMETER GenerateBaseline
    Generate baseline golden files from original midl.exe

.PARAMETER Compare
    Compare WinMIDL output against stored baselines

.PARAMETER FixturesDir
    Directory of IDL files (default: tests/fixtures/valid)

.PARAMETER WinMidlPath
    Path to WinMIDL binary (auto-detected from publish/ or dotnet run)

.PARAMETER GoldenDir
    Directory for golden files (default: tests/golden)
#>
[CmdletBinding(DefaultParameterSetName = 'Compare')]
param(
    [Parameter(ParameterSetName = 'Generate')]
    [switch]$GenerateBaseline,

    [Parameter(ParameterSetName = 'Compare')]
    [switch]$Compare,

    [string]$FixturesDir,
    [string]$WinMidlPath,
    [string]$GoldenDir
)

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot

# Default paths
if (-not $FixturesDir) { $FixturesDir = Join-Path $RepoRoot 'tests\fixtures\valid' }
if (-not $GoldenDir) { $GoldenDir = Join-Path $RepoRoot 'tests\golden' }

function Find-OriginalMidl {
    $sdkRoot = "${env:ProgramFiles(x86)}\Windows Kits\10"
    $binDir = Join-Path $sdkRoot 'bin'
    if (-not (Test-Path $binDir)) { return $null }

    # Prefer SDK versions that have winrt include + Windows.winmd (needed for pure MIDL3)
    $versions = Get-ChildItem -Path $binDir -Directory |
        Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' } |
        Sort-Object { [Version]$_.Name } -Descending

    foreach ($ver in $versions) {
        $midlPath = Join-Path $ver.FullName 'x64\midl.exe'
        $winrtInc = Join-Path $sdkRoot "Include\$($ver.Name)\winrt"
        $unionWinmd = Join-Path $sdkRoot "UnionMetadata\$($ver.Name)\Windows.winmd"
        if ((Test-Path $midlPath) -and (Test-Path $winrtInc) -and (Test-Path $unionWinmd)) {
            return $midlPath
        }
    }

    # Fallback: any midl.exe
    foreach ($ver in $versions) {
        $midlPath = Join-Path $ver.FullName 'x64\midl.exe'
        if (Test-Path $midlPath) { return $midlPath }
    }

    # Try PATH
    $inPath = Get-Command midl.exe -ErrorAction SilentlyContinue
    if ($inPath) { return $inPath.Source }

    return $null
}

function Find-WinMidl {
    if ($WinMidlPath -and (Test-Path $WinMidlPath)) {
        return $WinMidlPath
    }

    # Try published binary
    $publishDir = Join-Path $RepoRoot 'publish'
    if (Test-Path $publishDir) {
        $exe = Get-ChildItem -Path $publishDir -Filter 'midl.exe' -Recurse | Select-Object -First 1
        if ($exe) { return $exe.FullName }
    }

    # Use dotnet run approach
    return $null
}

function Invoke-WinMidlCompile {
    param(
        [string]$IdlPath,
        [string]$OutputDir,
        [string]$BaseName
    )

    $winmidl = Find-WinMidl
    $winmdPath = Join-Path $OutputDir "$BaseName.winmd"
    $headerPath = Join-Path $OutputDir "$BaseName.h"

    if ($winmidl) {
        $args = @(
            '/winrt',
            '/winmd', $winmdPath,
            '/h', $headerPath,
            '/nologo',
            $IdlPath
        )
        $proc = Start-Process -FilePath $winmidl -ArgumentList $args -NoNewWindow -Wait -PassThru `
            -RedirectStandardOutput (Join-Path $OutputDir 'stdout.txt') `
            -RedirectStandardError (Join-Path $OutputDir 'stderr.txt')
        return $proc.ExitCode -le 1  # 0=success, 1=warnings only, 2=errors
    }
    else {
        # Use dotnet run
        $cliProj = Join-Path $RepoRoot 'src\Midl.Cli\Midl.Cli.csproj'
        if (-not (Test-Path $cliProj)) {
            Write-Warning "WinMIDL binary not found and CLI project not available"
            return $false
        }
        $result = & dotnet run --project $cliProj --no-build -- /winrt /winmd $winmdPath /h $headerPath /nologo $IdlPath 2>&1
        return $LASTEXITCODE -le 1
    }
}

function Invoke-OriginalMidlCompile {
    param(
        [string]$MidlExe,
        [string]$IdlPath,
        [string]$OutputDir,
        [string]$BaseName
    )

    $winmdPath = Join-Path $OutputDir "$BaseName.winmd"
    $headerPath = Join-Path $OutputDir "$BaseName.h"
    $iidPath = Join-Path $OutputDir "${BaseName}_i.c"

    # Find SDK paths for /reference, /metadata_dir, /I
    # MidlExe path: .../Windows Kits/10/bin/10.0.XXXXX.0/x64/midl.exe
    $sdkVersion = Split-Path -Leaf (Split-Path -Parent (Split-Path -Parent $MidlExe))
    $sdkRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $MidlExe)))

    $unionMeta = Join-Path $sdkRoot "UnionMetadata\$sdkVersion"
    $windowsWinmd = Join-Path $unionMeta "Windows.winmd"
    $sdkInclude = Join-Path $sdkRoot "Include\$sdkVersion"
    $winrtInc = Join-Path $sdkInclude "winrt"

    # If this SDK version doesn't have the needed files, search for one that does
    if (-not (Test-Path $windowsWinmd) -or -not (Test-Path $winrtInc)) {
        $sdkBase = Join-Path $sdkRoot "UnionMetadata"
        if (Test-Path $sdkBase) {
            $altVer = Get-ChildItem $sdkBase -Directory | Where-Object {
                (Test-Path (Join-Path $_.FullName "Windows.winmd"))
            } | Sort-Object Name -Descending | Select-Object -First 1
            if ($altVer) {
                $v = $altVer.Name
                $unionMeta = $altVer.FullName
                $windowsWinmd = Join-Path $unionMeta "Windows.winmd"
                $sdkInclude = Join-Path $sdkRoot "Include\$v"
                $winrtInc = Join-Path $sdkInclude "winrt"
            }
        }
    }

    $args = @(
        '/winrt',
        '/nomidl',
        '/winmd', "`"$winmdPath`"",
        '/h', "`"$headerPath`"",
        '/iid', "`"$iidPath`"",
        '/nologo'
    )

    if (Test-Path $windowsWinmd) {
        $args += '/reference'
        $args += "`"$windowsWinmd`""
    }
    if (Test-Path $unionMeta) {
        $args += '/metadata_dir'
        $args += "`"$unionMeta`""
    }
    if (Test-Path $winrtInc) {
        $args += '/I'
        $args += "`"$winrtInc`""
    }

    $args += "`"$IdlPath`""

    $proc = Start-Process -FilePath $MidlExe -ArgumentList $args -NoNewWindow -Wait -PassThru `
        -RedirectStandardOutput (Join-Path $OutputDir 'stdout.txt') `
        -RedirectStandardError (Join-Path $OutputDir 'stderr.txt')

    return $proc.ExitCode -eq 0
}

function Compare-Headers {
    param(
        [string]$ExpectedPath,
        [string]$ActualPath
    )

    if (-not (Test-Path $ExpectedPath)) { return @{ Status = 'skip'; Reason = 'Expected header not found' } }
    if (-not (Test-Path $ActualPath)) { return @{ Status = 'skip'; Reason = 'Actual header not found' } }

    $ignorePatterns = @(
        '^\s*//\s*Generated by',
        '^\s*//\s*Version',
        '^\s*//\s*File created by',
        '^\s*//\s*MIDL compiler version',
        '^\s*\*\s*File created by MIDL',
        '^\s*\*\s*at ',
        '^\s*$'  # blank lines
    )

    $expected = (Get-Content $ExpectedPath) | Where-Object {
        $line = $_
        -not ($ignorePatterns | Where-Object { $line -match $_ })
    } | ForEach-Object { $_.Trim() }

    $actual = (Get-Content $ActualPath) | Where-Object {
        $line = $_
        -not ($ignorePatterns | Where-Object { $line -match $_ })
    } | ForEach-Object { $_.Trim() }

    $expectedStr = ($expected -join "`n").Trim()
    $actualStr = ($actual -join "`n").Trim()

    if ($expectedStr -eq $actualStr) {
        return @{ Status = 'identical' }
    }

    # Find first differences
    $diffs = @()
    $maxLines = [Math]::Max($expected.Count, $actual.Count)
    for ($i = 0; $i -lt $maxLines -and $diffs.Count -lt 5; $i++) {
        $eLine = if ($i -lt $expected.Count) { $expected[$i] } else { '<EOF>' }
        $aLine = if ($i -lt $actual.Count) { $actual[$i] } else { '<EOF>' }
        if ($eLine -ne $aLine) {
            $diffs += "Line $($i+1): expected='$eLine' actual='$aLine'"
        }
    }

    return @{ Status = 'differs'; Diffs = $diffs }
}

function Compare-IidFiles {
    param(
        [string]$ExpectedPath,
        [string]$ActualPath
    )

    if (-not (Test-Path $ExpectedPath)) { return @{ Status = 'skip'; Reason = 'Expected IID file not found' } }
    if (-not (Test-Path $ActualPath)) { return @{ Status = 'skip'; Reason = 'Actual IID file not found' } }

    $expected = (Get-Content $ExpectedPath -Raw).Trim()
    $actual = (Get-Content $ActualPath -Raw).Trim()

    if ($expected -eq $actual) {
        return @{ Status = 'identical' }
    }
    return @{ Status = 'differs'; Diffs = @('IID content mismatch (GUIDs must be identical)') }
}

function Compare-WinMd {
    param(
        [string]$ExpectedPath,
        [string]$ActualPath
    )

    if (-not (Test-Path $ExpectedPath)) { return @{ Status = 'skip'; Reason = 'Expected WinMD not found' } }
    if (-not (Test-Path $ActualPath)) { return @{ Status = 'skip'; Reason = 'Actual WinMD not found' } }

    # Dump both WinMDs using --dump-winmd
    $winmidl = Find-WinMidl
    $cliProj = Join-Path $RepoRoot 'src\Midl.Cli\Midl.Cli.csproj'

    if ($winmidl) {
        $expectedDump = & $winmidl --dump-winmd $ExpectedPath 2>$null
        $actualDump = & $winmidl --dump-winmd $ActualPath 2>$null
    }
    elseif (Test-Path $cliProj) {
        $expectedDump = & dotnet run --project $cliProj --no-build -- --dump-winmd $ExpectedPath 2>$null
        $actualDump = & dotnet run --project $cliProj --no-build -- --dump-winmd $ActualPath 2>$null
    }
    else {
        return @{ Status = 'skip'; Reason = 'WinMIDL not available for --dump-winmd' }
    }

    $expectedStr = ($expectedDump -join "`n").Trim()
    $actualStr = ($actualDump -join "`n").Trim()

    if ($expectedStr -eq $actualStr) {
        return @{ Status = 'identical' }
    }

    # Find first differences
    $expectedLines = $expectedStr -split "`n"
    $actualLines = $actualStr -split "`n"
    $diffs = @()
    $maxLines = [Math]::Max($expectedLines.Count, $actualLines.Count)
    for ($i = 0; $i -lt $maxLines -and $diffs.Count -lt 5; $i++) {
        $eLine = if ($i -lt $expectedLines.Count) { $expectedLines[$i] } else { '<EOF>' }
        $aLine = if ($i -lt $actualLines.Count) { $actualLines[$i] } else { '<EOF>' }
        if ($eLine -ne $aLine) {
            $diffs += "Line $($i+1): expected='$eLine' actual='$aLine'"
        }
    }

    return @{ Status = 'differs'; Diffs = $diffs }
}

# --- Main ---

Write-Host "=== WinMIDL Golden File Comparison (T3) ===" -ForegroundColor Cyan
Write-Host "Fixtures: $FixturesDir"
Write-Host "Golden:   $GoldenDir"

if (-not (Test-Path $FixturesDir)) {
    Write-Error "Fixtures directory not found: $FixturesDir"
    exit 1
}

$idlFiles = Get-ChildItem -Path $FixturesDir -Filter '*.idl'
if ($idlFiles.Count -eq 0) {
    Write-Warning "No .idl files found in $FixturesDir"
    exit 0
}

Write-Host "Found $($idlFiles.Count) IDL fixture(s)" -ForegroundColor Green

if ($GenerateBaseline) {
    # --- Generate Baseline Mode ---
    $origMidl = Find-OriginalMidl
    if (-not $origMidl) {
        Write-Error "Original midl.exe not found. Install Windows SDK or set PATH."
        exit 1
    }
    Write-Host "Using original midl.exe: $origMidl" -ForegroundColor Yellow

    foreach ($idl in $idlFiles) {
        $name = [System.IO.Path]::GetFileNameWithoutExtension($idl.Name)
        Write-Host "`n--- Generating baseline for: $name ---" -ForegroundColor Cyan

        $originalDir = Join-Path $GoldenDir "$name\original"
        New-Item -ItemType Directory -Path $originalDir -Force | Out-Null

        $success = Invoke-OriginalMidlCompile -MidlExe $origMidl -IdlPath $idl.FullName `
            -OutputDir $originalDir -BaseName $name

        if ($success) {
            Write-Host "  [OK] Original midl.exe compiled successfully" -ForegroundColor Green
        }
        else {
            Write-Host "  [WARN] Original midl.exe failed for $name" -ForegroundColor Yellow
            $stderr = Join-Path $originalDir 'stderr.txt'
            if (Test-Path $stderr) {
                Get-Content $stderr | Select-Object -First 5 | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkYellow }
            }
        }
    }

    Write-Host "`n=== Baseline generation complete ===" -ForegroundColor Green
}
else {
    # --- Compare Mode (default) ---
    $origMidl = Find-OriginalMidl
    $stats = @{ Total = 0; Identical = 0; Differs = 0; Skipped = 0; Errors = 0 }

    foreach ($idl in $idlFiles) {
        $name = [System.IO.Path]::GetFileNameWithoutExtension($idl.Name)
        Write-Host "`n--- Comparing: $name ---" -ForegroundColor Cyan
        $stats.Total++

        # Compile with WinMIDL
        $winmidlDir = Join-Path $GoldenDir "$name\winmidl"
        New-Item -ItemType Directory -Path $winmidlDir -Force | Out-Null

        $winmidlOk = Invoke-WinMidlCompile -IdlPath $idl.FullName -OutputDir $winmidlDir -BaseName $name
        if (-not $winmidlOk) {
            Write-Host "  [ERROR] WinMIDL compilation failed" -ForegroundColor Red
            $stats.Errors++
            continue
        }

        # Check for original baseline
        $originalDir = Join-Path $GoldenDir "$name\original"
        if (-not (Test-Path $originalDir)) {
            # If no baseline, try generating one on-the-fly
            if ($origMidl) {
                New-Item -ItemType Directory -Path $originalDir -Force | Out-Null
                $origOk = Invoke-OriginalMidlCompile -MidlExe $origMidl -IdlPath $idl.FullName `
                    -OutputDir $originalDir -BaseName $name
                if (-not $origOk) {
                    Write-Host "  [WARN] Original midl.exe failed, skipping comparison" -ForegroundColor Yellow
                    $stats.Skipped++
                    continue
                }
            }
            else {
                Write-Host "  [SKIP] No baseline and no original midl.exe" -ForegroundColor Yellow
                $stats.Skipped++
                continue
            }
        }

        # Compare headers
        $origHeader = Join-Path $originalDir "$name.h"
        $winmidlHeader = Join-Path $winmidlDir "$name.h"
        $headerResult = Compare-Headers -ExpectedPath $origHeader -ActualPath $winmidlHeader

        switch ($headerResult.Status) {
            'identical' { Write-Host "  [HEADER] Identical" -ForegroundColor Green }
            'differs' {
                Write-Host "  [HEADER] Differs" -ForegroundColor Yellow
                $headerResult.Diffs | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkYellow }
            }
            'skip' { Write-Host "  [HEADER] Skipped: $($headerResult.Reason)" -ForegroundColor DarkGray }
        }

        # Compare IID files
        $origIid = Join-Path $originalDir "${name}_i.c"
        $winmidlIid = Join-Path $winmidlDir "${name}_i.c"
        $iidResult = Compare-IidFiles -ExpectedPath $origIid -ActualPath $winmidlIid

        switch ($iidResult.Status) {
            'identical' { Write-Host "  [IID]    Identical" -ForegroundColor Green }
            'differs' {
                Write-Host "  [IID]    Differs" -ForegroundColor Yellow
                $iidResult.Diffs | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkYellow }
            }
            'skip' { Write-Host "  [IID]    Skipped: $($iidResult.Reason)" -ForegroundColor DarkGray }
        }

        # Compare WinMD structurally via --dump-winmd
        $origWinmd = Join-Path $originalDir "$name.winmd"
        $winmidlWinmd = Join-Path $winmidlDir "$name.winmd"

        $winmdResult = Compare-WinMd -ExpectedPath $origWinmd -ActualPath $winmidlWinmd

        switch ($winmdResult.Status) {
            'identical' { Write-Host "  [WINMD]  Identical" -ForegroundColor Green }
            'differs' {
                Write-Host "  [WINMD]  Differs" -ForegroundColor Yellow
                $winmdResult.Diffs | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkYellow }
            }
            'skip' { Write-Host "  [WINMD]  Skipped: $($winmdResult.Reason)" -ForegroundColor DarkGray }
        }

        # Tally result (header + IID + WinMD)
        $allIdentical = @($headerResult, $iidResult, $winmdResult) | Where-Object { $_.Status -eq 'identical' }
        $anyDiffers = @($headerResult, $iidResult, $winmdResult) | Where-Object { $_.Status -eq 'differs' }

        if ($anyDiffers.Count -gt 0) {
            $stats.Differs++
        }
        elseif ($allIdentical.Count -gt 0) {
            $stats.Identical++
        }
        else {
            $stats.Skipped++
        }
    }

    # Summary
    Write-Host "`n=== Summary ===" -ForegroundColor Cyan
    Write-Host "Total:     $($stats.Total)"
    Write-Host "Identical: $($stats.Identical)" -ForegroundColor Green
    Write-Host "Differs:   $($stats.Differs)" -ForegroundColor $(if ($stats.Differs -gt 0) { 'Yellow' } else { 'Green' })
    Write-Host "Skipped:   $($stats.Skipped)" -ForegroundColor DarkGray
    Write-Host "Errors:    $($stats.Errors)" -ForegroundColor $(if ($stats.Errors -gt 0) { 'Red' } else { 'Green' })
}
