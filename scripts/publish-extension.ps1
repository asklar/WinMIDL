# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

<#
.SYNOPSIS
  Publish the MIDL VS Code extension to the Marketplace.

.DESCRIPTION
  Publishes using either Azure CLI credentials (default, no PAT needed)
  or a PAT. Requires 'az login' to have been run, or a fresh PAT.

.PARAMETER Pat
  Azure DevOps Personal Access Token. If omitted, uses Azure CLI credentials.

.PARAMETER VsixPath
  Path to a local .vsix file. If not provided, publishes from editors/vscode.

.EXAMPLE
  # Using Azure CLI credentials (recommended):
  az login
  .\publish-extension.ps1

  # Using a PAT:
  .\publish-extension.ps1 -Pat "your-pat-here"
#>
param(
    [string]$Pat,
    [string]$VsixPath
)

$ErrorActionPreference = 'Stop'

if ($VsixPath -and (Test-Path $VsixPath)) {
    Write-Host "Publishing from VSIX: $VsixPath"
    if ($Pat) {
        npx @vscode/vsce publish --packagePath $VsixPath -p $Pat
    } else {
        Write-Host "Using Azure CLI credentials (run 'az login' first)"
        npx @vscode/vsce publish --packagePath $VsixPath --azure-credential
    }
} else {
    Push-Location "$PSScriptRoot\..\editors\vscode"
    try {
        npm ci --silent
        if ($Pat) {
            Write-Host "Publishing with PAT..."
            npx @vscode/vsce publish -p $Pat
        } else {
            Write-Host "Publishing with Azure CLI credentials (run 'az login' first)..."
            npx @vscode/vsce publish --azure-credential
        }
    } finally {
        Pop-Location
    }
}

Write-Host "`nPublished successfully!" -ForegroundColor Green
