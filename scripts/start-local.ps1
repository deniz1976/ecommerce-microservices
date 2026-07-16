param(
    [switch]$SkipBuild,
    [switch]$SkipSmokeTest
)

$ErrorActionPreference = "Stop"

docker version | Out-Null

.\scripts\check-runtime-env.ps1

if (-not $SkipBuild) {
    dotnet test ECommerce.sln
}

docker compose up --build -d

if (-not $SkipSmokeTest) {
    Start-Sleep -Seconds 10
    .\scripts\smoke-test.ps1
}

Write-Host "Local runtime started"
