<#
.SYNOPSIS
  Publish the MIDL VS Code extension to the Marketplace.

.DESCRIPTION
  Downloads the latest VSIX from the GitHub release (or uses a local file)
  and publishes it using vsce. Requires a fresh Azure DevOps PAT with
  Marketplace (Manage) scope.

.PARAMETER Pat
  Azure DevOps Personal Access Token with Marketplace scope.

.PARAMETER VsixPath
  Path to a local .vsix file. If not provided, publishes from editors/vscode.

.EXAMPLE
  .\publish-extension.ps1 -Pat "your-pat-here"
#>
param(
    [Parameter(Mandatory)]
    [string]$Pat,

    [string]$VsixPath
)

$ErrorActionPreference = 'Stop'

if ($VsixPath -and (Test-Path $VsixPath)) {
    Write-Host "Publishing from VSIX: $VsixPath"
    npx @vscode/vsce publish --packagePath $VsixPath -p $Pat
} else {
    Push-Location "$PSScriptRoot\..\editors\vscode"
    try {
        Write-Host "Publishing from editors/vscode..."
        npm ci --silent
        npx @vscode/vsce publish -p $Pat
    } finally {
        Pop-Location
    }
}

Write-Host "`nPublished successfully!" -ForegroundColor Green
