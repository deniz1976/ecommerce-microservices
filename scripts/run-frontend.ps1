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
    "--prefix",
    $frontendRoot,
    "run",
    "dev"
)

& $runner `
    -Environment $Environment `
    -SecretPath $SecretPath `
    -Command $frontendCommand
