[CmdletBinding(PositionalBinding = $false)]
param(
    [string]$Environment = "dev",
    [string]$SecretPath = "/",
    [Parameter(Position = 0, ValueFromRemainingArguments = $true)]
    [string[]]$Command
)

$ErrorActionPreference = "Stop"

if ($Command.Count -eq 0) {
    throw "Usage: .\scripts\run-with-secrets.ps1 [-Environment dev] [-SecretPath /] dotnet run --project <project.csproj>"
}

$infisical = Get-Command infisical -ErrorAction SilentlyContinue

if ($null -eq $infisical) {
    throw "Infisical CLI is not installed or not available in PATH. Install it, then run 'infisical login' and 'infisical init'."
}

$arguments = @(
    "run",
    "--env=$Environment",
    "--path=$SecretPath",
    "--"
)

if ($Command[0].EndsWith(".ps1", [StringComparison]::OrdinalIgnoreCase)) {
    $arguments += @(
        "powershell",
        "-NoProfile",
        "-ExecutionPolicy",
        "Bypass",
        "-File"
    )
}

$arguments += $Command

& infisical @arguments
