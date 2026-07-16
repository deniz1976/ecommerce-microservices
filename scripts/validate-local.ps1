param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

if (-not $SkipBuild) {
    dotnet build ECommerce.sln
}

Get-ChildItem scripts -Filter *.ps1 | ForEach-Object {
    $errors = $null
    $tokens = $null
    [System.Management.Automation.Language.Parser]::ParseFile($_.FullName, [ref]$tokens, [ref]$errors) | Out-Null

    if ($errors.Count -gt 0) {
        $errors | ForEach-Object { $_.Message }
        throw "PowerShell syntax check failed for $($_.Name)"
    }
}

docker compose config | Out-Null

$patterns = "postgresql://[^\s]+:[^\s]+@|amqps://[^\s]+:[^\s]+@|npg_[A-Za-z0-9]{12,}|ep-[a-z0-9-]+-pooler"
$ripgrep = Get-Command rg -ErrorAction SilentlyContinue

if ($null -ne $ripgrep) {
    $matches = rg $patterns . -g "!**/bin/**" -g "!**/obj/**" -g "!.git/**" -g "!scripts/validate-local.ps1"

    if ($LASTEXITCODE -gt 1) {
        throw "Secret scan failed"
    }
}
else {
    $trackedFiles = @(git ls-files)

    if ($LASTEXITCODE -ne 0) {
        throw "Unable to enumerate tracked files for the secret scan"
    }

    $scanFiles = @($trackedFiles | Where-Object { $_ -ne "scripts/validate-local.ps1" })
    $matches = @(
        Select-String -LiteralPath $scanFiles -Pattern $patterns -List |
            ForEach-Object { $_.Path }
    )
}

if ($matches) {
    $matches
    throw "Secret-like value found in repository files"
}

$global:LASTEXITCODE = 0
Write-Host "Local validation completed"
