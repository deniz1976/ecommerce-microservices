[CmdletBinding()]
param(
    [string]$Environment = "staging",
    [string]$SecretPath = "/",
    [string]$ApiBaseUrl = "http://localhost:15080"
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path $PSScriptRoot -Parent
$frontendRoot = Join-Path $repositoryRoot "src\Frontend"
$runner = Join-Path $PSScriptRoot "run-with-secrets.ps1"
$frontendLauncher = Join-Path $PSScriptRoot "start-frontend-process.ps1"
$frontendCommand = @($frontendLauncher, "-ApiBaseUrl", $ApiBaseUrl)

Push-Location $frontendRoot
try {
    & $runner `
        -Environment $Environment `
        -SecretPath $SecretPath `
        -Command $frontendCommand
}
finally {
    Pop-Location
}
