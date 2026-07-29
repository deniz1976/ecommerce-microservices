param()

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/jwt-claim-validation.ps1"

function ConvertTo-Base64Url {
    param(
        [Parameter(Mandatory)]
        [string]$Value
    )

    $encoded = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($Value))
    return $encoded.TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

function New-TestJwt {
    param(
        [Parameter(Mandatory)]
        [hashtable]$Claims
    )

    $header = ConvertTo-Base64Url '{"alg":"none","typ":"JWT"}'
    $payload = ConvertTo-Base64Url ($Claims | ConvertTo-Json -Compress)
    return "$header.$payload.test-signature"
}

function Assert-Rejected {
    param(
        [Parameter(Mandatory)]
        [scriptblock]$Action,
        [Parameter(Mandatory)]
        [string]$ExpectedMessage
    )

    try {
        & $Action
    }
    catch {
        if ($_.Exception.Message -cne $ExpectedMessage) {
            throw "Expected rejection '$ExpectedMessage' but received '$($_.Exception.Message)'"
        }

        return
    }

    throw "Expected runtime token validation to reject: $ExpectedMessage"
}

$authority = "https://runtime-check.example.test"
$audience = "https://api.example.test"
$futureExpiration = [DateTimeOffset]::UtcNow.AddMinutes(5).ToUnixTimeSeconds()
$validClaims = @{
    iss = "$authority/"
    aud = $audience
    exp = $futureExpiration
    scope = "inventory:write customer:act"
    permissions = @("customer:act", "inventory:write")
}

$validToken = New-TestJwt $validClaims
Assert-RuntimeAccessTokenClaims `
    -AccessToken $validToken `
    -Authority $authority `
    -Audience $audience

$wrongIssuerClaims = $validClaims.Clone()
$wrongIssuerClaims.iss = "https://other.example.test/"
Assert-Rejected {
    Assert-RuntimeAccessTokenClaims `
        -AccessToken (New-TestJwt $wrongIssuerClaims) `
        -Authority $authority `
        -Audience $audience
} "Runtime Auth0 access token issuer does not match the expected issuer"

$wrongAudienceClaims = $validClaims.Clone()
$wrongAudienceClaims.aud = "https://other-api.example.test"
Assert-Rejected {
    Assert-RuntimeAccessTokenClaims `
        -AccessToken (New-TestJwt $wrongAudienceClaims) `
        -Authority $authority `
        -Audience $audience
} "Runtime Auth0 access token audience does not include the expected audience"

$expiredClaims = $validClaims.Clone()
$expiredClaims.exp = [DateTimeOffset]::UtcNow.AddMinutes(-5).ToUnixTimeSeconds()
Assert-Rejected {
    Assert-RuntimeAccessTokenClaims `
        -AccessToken (New-TestJwt $expiredClaims) `
        -Authority $authority `
        -Audience $audience
} "Runtime Auth0 access token is expired or has no valid expiration"

$missingPermissionClaims = $validClaims.Clone()
$missingPermissionClaims.scope = "inventory:write"
$missingPermissionClaims.permissions = @("inventory:write")
Assert-Rejected {
    Assert-RuntimeAccessTokenClaims `
        -AccessToken (New-TestJwt $missingPermissionClaims) `
        -Authority $authority `
        -Audience $audience
} "Runtime Auth0 access token must contain exactly inventory:write and customer:act"

$additionalPermissionClaims = $validClaims.Clone()
$additionalPermissionClaims.permissions = @("customer:act", "inventory:write", "unexpected:permission")
Assert-Rejected {
    Assert-RuntimeAccessTokenClaims `
        -AccessToken (New-TestJwt $additionalPermissionClaims) `
        -Authority $authority `
        -Audience $audience
} "Runtime Auth0 access token must contain exactly inventory:write and customer:act"

Assert-Rejected {
    Assert-RuntimeAccessTokenClaims `
        -AccessToken "not-a-jwt" `
        -Authority $authority `
        -Audience $audience
} "Runtime Auth0 access token does not use the expected three-segment JWT format"

Write-Host "Runtime token claim validation tests completed"
