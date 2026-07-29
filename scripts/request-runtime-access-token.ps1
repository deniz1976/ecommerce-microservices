param()

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/jwt-claim-validation.ps1"

$authority = $env:Auth__Authority
$audience = $env:Auth__Audience
$clientId = $env:RuntimeChecks__Auth0ClientId
$clientSecret = $env:RuntimeChecks__Auth0ClientSecret

$missing = @(
    if ([string]::IsNullOrWhiteSpace($authority)) { "Auth__Authority" }
    if ([string]::IsNullOrWhiteSpace($audience)) { "Auth__Audience" }
    if ([string]::IsNullOrWhiteSpace($clientId)) { "RuntimeChecks__Auth0ClientId" }
    if ([string]::IsNullOrWhiteSpace($clientSecret)) { "RuntimeChecks__Auth0ClientSecret" }
)

if ($missing.Count -gt 0) {
    throw "Missing runtime token configuration: $($missing -join ', ')"
}

$tokenEndpoint = "$($authority.TrimEnd('/'))/oauth/token"
$response = Invoke-RestMethod -Method Post -Uri $tokenEndpoint -ContentType "application/x-www-form-urlencoded" -Body @{
    grant_type = "client_credentials"
    client_id = $clientId
    client_secret = $clientSecret
    audience = $audience
    scope = "inventory:write customer:act"
}

$accessToken = $response.access_token

if ([string]::IsNullOrWhiteSpace($accessToken)) {
    throw "Auth0 token endpoint returned an empty access token"
}

Assert-RuntimeAccessTokenClaims `
    -AccessToken $accessToken `
    -Authority $authority `
    -Audience $audience

[Environment]::SetEnvironmentVariable("RuntimeChecks__AccessToken", $accessToken, "Process")

if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_ENV)) {
    Write-Output "::add-mask::$accessToken"
    Add-Content -LiteralPath $env:GITHUB_ENV -Value "RuntimeChecks__AccessToken=$accessToken" -Encoding utf8
}

Write-Host "Runtime Auth0 access token acquired with exact issuer, audience, expiration, and permissions"
