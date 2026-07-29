param(
    [string]$Root = "."
)

$ErrorActionPreference = "Stop"
$resolvedRoot = Resolve-Path $Root
$violations = @()

$sourceFiles = Get-ChildItem -Path (Join-Path $resolvedRoot "src") -Recurse -File -Filter *.cs |
    Where-Object {
        $_.FullName -notmatch '[\\/](?:bin|obj|Migrations)[\\/]'
    }

$forbiddenLocalTimePattern = '\b(?:DateTime|DateTimeOffset)\.(?:Now|Today)\b|\.ToLocalTime\s*\('
foreach ($sourceFile in $sourceFiles) {
    $matches = @(Select-String -LiteralPath $sourceFile.FullName -Pattern $forbiddenLocalTimePattern)
    foreach ($match in $matches) {
        $relativePath = [System.IO.Path]::GetRelativePath($resolvedRoot, $sourceFile.FullName)
        $violations += "${relativePath}:$($match.LineNumber) uses a local-time API"
    }
}

$domainAndContractFiles = Get-ChildItem -Path (Join-Path $resolvedRoot "src") -Recurse -File -Filter *.cs |
    Where-Object {
        $_.FullName -notmatch '[\\/](?:bin|obj|Migrations)[\\/]' -and
        ($_.FullName -match '[\\/]Domain[\\/]' -or $_.FullName -match 'ECommerce\.BuildingBlocks\.Contracts')
    }

foreach ($sourceFile in $domainAndContractFiles) {
    $matches = @(Select-String -LiteralPath $sourceFile.FullName -Pattern '\bDateTime\b')
    foreach ($match in $matches) {
        $relativePath = [System.IO.Path]::GetRelativePath($resolvedRoot, $sourceFile.FullName)
        $violations += "${relativePath}:$($match.LineNumber) uses DateTime; persisted instants and contracts must use DateTimeOffset"
    }
}

$nonUtcDatabaseMatches = @(
    Get-ChildItem -Path (Join-Path $resolvedRoot "src") -Recurse -File -Include *.cs |
        Where-Object { $_.FullName -notmatch '[\\/](?:bin|obj)[\\/]' } |
        Select-String -Pattern 'timestamp without time zone'
)

foreach ($match in $nonUtcDatabaseMatches) {
    $relativePath = [System.IO.Path]::GetRelativePath($resolvedRoot, $match.Path)
    $violations += "${relativePath}:$($match.LineNumber) maps a timestamp without time zone"
}

if ($violations.Count -gt 0) {
    $violations | ForEach-Object { Write-Error $_ }
    throw "UTC time validation failed."
}

Write-Host "UTC time validation completed"
