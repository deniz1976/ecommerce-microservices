[CmdletBinding()]
param(
    [string]$Environment = "staging",
    [string]$SecretPath = "/"
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path $PSScriptRoot -Parent
$frontendRoot = Join-Path $repositoryRoot "src\Frontend"
$runner = Join-Path $PSScriptRoot "run-with-secrets.ps1"
$frontendCommand = @(
    "npm",
    "run",
    "dev"
)

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
