param()

$ErrorActionPreference = "Stop"

$enabled = $env:Auth0Management__Enabled
$domain = $env:Auth0Management__Domain
$clientId = $env:Auth0Management__ClientId
$clientSecret = $env:Auth0Management__ClientSecret
$customerRoleId = $env:Auth0Management__CustomerRoleId
$sellerRoleId = $env:Auth0Management__SellerRoleId

$missing = @(
    if ([string]::IsNullOrWhiteSpace($enabled)) { "Auth0Management__Enabled" }
    if ([string]::IsNullOrWhiteSpace($domain)) { "Auth0Management__Domain" }
    if ([string]::IsNullOrWhiteSpace($clientId)) { "Auth0Management__ClientId" }
    if ([string]::IsNullOrWhiteSpace($clientSecret)) { "Auth0Management__ClientSecret" }
    if ([string]::IsNullOrWhiteSpace($customerRoleId)) { "Auth0Management__CustomerRoleId" }
    if ([string]::IsNullOrWhiteSpace($sellerRoleId)) { "Auth0Management__SellerRoleId" }
)

if ($missing.Count -gt 0) {
    throw "Missing Auth0 Management API configuration: $($missing -join ', ')"
}

if ($enabled -cne "true") {
    throw "Auth0Management__Enabled must be exactly 'true' in the managed runtime environment"
}

if ($domain -notmatch '^[A-Za-z0-9](?:[A-Za-z0-9.-]*[A-Za-z0-9])?$' -or $domain.Contains('..')) {
    throw "Auth0Management__Domain must be a host name without scheme, port, path, or trailing slash"
}

if ($customerRoleId -notmatch '^rol_[A-Za-z0-9]+$' -or $sellerRoleId -notmatch '^rol_[A-Za-z0-9]+$') {
    throw "Auth0 Management Customer and Seller role IDs must use the expected rol_ format"
}

if ($customerRoleId -ceq $sellerRoleId) {
    throw "Auth0 Management Customer and Seller role IDs must be different"
}

$response = Invoke-RestMethod `
    -Method Post `
    -Uri "https://$domain/oauth/token" `
    -ContentType "application/x-www-form-urlencoded" `
    -Body @{
        grant_type = "client_credentials"
        client_id = $clientId
        client_secret = $clientSecret
        audience = "https://$domain/api/v2/"
        scope = "update:users"
    }

$accessToken = $response.access_token

if ([string]::IsNullOrWhiteSpace($accessToken)) {
    throw "Auth0 Management token endpoint returned an empty access token"
}

$segments = $accessToken.Split('.')
if ($segments.Length -lt 2) {
    throw "Auth0 Management token is not a JWT and its granted scope cannot be verified"
}

$payloadSegment = $segments[1].Replace('-', '+').Replace('_', '/')
$payloadSegment += '=' * ((4 - ($payloadSegment.Length % 4)) % 4)
$payloadJson = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payloadSegment))
$claims = $payloadJson | ConvertFrom-Json

$grantedScopes = @()
if ($claims.scope -is [string]) {
    $grantedScopes += @($claims.scope -split '\s+' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}
if ($null -ne $claims.permissions) {
    $grantedScopes += @($claims.permissions)
}
$grantedScopes = @($grantedScopes | Sort-Object -Unique)

if ($grantedScopes.Count -ne 1 -or $grantedScopes[0] -cne "update:users") {
    throw "Auth0 Management token must contain exactly the update:users scope"
}

Write-Host "Auth0 Management API access verified with exact scope update:users"
