function Read-JwtPayloadClaims {
    param(
        [Parameter(Mandatory)]
        [string]$AccessToken,
        [Parameter(Mandatory)]
        [string]$TokenName
    )

    $segments = $AccessToken.Split('.')
    if ($segments.Length -ne 3) {
        throw "$TokenName does not use the expected three-segment JWT format"
    }

    try {
        $payloadSegment = $segments[1].Replace('-', '+').Replace('_', '/')
        $payloadSegment += '=' * ((4 - ($payloadSegment.Length % 4)) % 4)
        $payloadJson = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payloadSegment))
        return $payloadJson | ConvertFrom-Json
    }
    catch {
        throw "$TokenName payload cannot be decoded"
    }
}

function Assert-JwtCoreClaims {
    param(
        [Parameter(Mandatory)]
        [psobject]$Claims,
        [Parameter(Mandatory)]
        [string]$ExpectedIssuer,
        [Parameter(Mandatory)]
        [string]$ExpectedAudience,
        [Parameter(Mandatory)]
        [string]$TokenName
    )

    if ($Claims.iss -cne $ExpectedIssuer) {
        throw "$TokenName issuer does not match the expected issuer"
    }

    $tokenAudiences = @($Claims.aud)
    if ($tokenAudiences.Count -eq 0 -or $tokenAudiences -cnotcontains $ExpectedAudience) {
        throw "$TokenName audience does not include the expected audience"
    }

    [long]$expiresAt = 0
    if ($null -eq $Claims.exp -or
        -not [long]::TryParse([string]$Claims.exp, [ref]$expiresAt) -or
        $expiresAt -le [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()) {
        throw "$TokenName is expired or has no valid expiration"
    }
}

function Get-JwtGrantedPermissions {
    param(
        [Parameter(Mandatory)]
        [psobject]$Claims
    )

    $grantedPermissions = @()
    if ($Claims.scope -is [string]) {
        $grantedPermissions += @(
            $Claims.scope -split '\s+' |
                Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
        )
    }
    if ($null -ne $Claims.permissions) {
        $grantedPermissions += @($Claims.permissions)
    }

    return @($grantedPermissions | Sort-Object -Unique)
}

function Assert-RuntimeAccessTokenClaims {
    param(
        [Parameter(Mandatory)]
        [string]$AccessToken,
        [Parameter(Mandatory)]
        [string]$Authority,
        [Parameter(Mandatory)]
        [string]$Audience
    )

    $tokenName = "Runtime Auth0 access token"
    $claims = Read-JwtPayloadClaims -AccessToken $AccessToken -TokenName $tokenName
    Assert-JwtCoreClaims `
        -Claims $claims `
        -ExpectedIssuer "$($Authority.TrimEnd('/'))/" `
        -ExpectedAudience $Audience `
        -TokenName $tokenName

    $grantedPermissions = @(Get-JwtGrantedPermissions -Claims $claims)
    $expectedPermissions = @("customer:act", "inventory:write")

    if ($grantedPermissions.Count -ne $expectedPermissions.Count -or
        (Compare-Object -ReferenceObject $expectedPermissions -DifferenceObject $grantedPermissions).Count -ne 0) {
        throw "Runtime Auth0 access token must contain exactly inventory:write and customer:act"
    }
}
