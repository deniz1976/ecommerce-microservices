[CmdletBinding(PositionalBinding = $false)]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "staging")]
    [string]$Environment,
    [ValidateSet(
        "all",
        "catalog",
        "basket",
        "ordering",
        "ordering-saga",
        "inventory",
        "payment",
        "shipping",
        "notification",
        "identity"
    )]
    [string]$Service = "all",
    [string]$SecretPath = "/"
)

$ErrorActionPreference = "Stop"

$secretRunner = Join-Path $PSScriptRoot "run-with-secrets.ps1"
$migrationRunner = Join-Path $PSScriptRoot "run-migrations.ps1"
$migrationCommand = @(
    $migrationRunner,
    "-Service",
    $Service
)

Write-Host "Applying '$Service' migrations to the Infisical '$Environment' environment"

& $secretRunner `
    -Environment $Environment `
    -SecretPath $SecretPath `
    -Command $migrationCommand

Write-Host "'$Environment' migration run completed"
