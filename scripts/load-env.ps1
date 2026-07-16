param(
    [string]$EnvFile = ".env"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $EnvFile)) {
    throw "Environment file not found: $EnvFile"
}

Get-Content $EnvFile | ForEach-Object {
    $line = $_.Trim()

    if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith("#")) {
        return
    }

    $parts = $line.Split("=", 2)

    if ($parts.Length -eq 2) {
        Set-Item -Path "Env:$($parts[0].Trim())" -Value $parts[1].Trim()
    }
}

Write-Host "Environment variables loaded from $EnvFile"
