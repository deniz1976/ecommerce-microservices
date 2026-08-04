param(
    [string]$GatewayBaseUrl = "http://localhost:5080",
    [Guid]$ProductId = [Guid]::Empty,
    [int]$TimeoutSeconds = 60
)

$ErrorActionPreference = "Stop"

# Windows PowerShell 5.1 does not automatically load this assembly when the
# script first encounters HttpClient types in function parameter declarations.
Add-Type -AssemblyName System.Net.Http

. (Join-Path $PSScriptRoot "jwt-claim-validation.ps1")

function Read-RequiredEnvironmentValue {
    param(
        [Parameter(Mandatory)]
        [string]$Name
    )

    $value = [Environment]::GetEnvironmentVariable($Name)
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw "$Name is required"
    }

    return $value.Trim()
}

function Assert-SellerAccessTokenClaims {
    param(
        [Parameter(Mandatory)]
        [string]$AccessToken,
        [Parameter(Mandatory)]
        [string]$Authority,
        [Parameter(Mandatory)]
        [string]$Audience,
        [Parameter(Mandatory)]
        [string]$RoleClaimType
    )

    $tokenName = "Seller Auth0 access token"
    $claims = Read-JwtPayloadClaims -AccessToken $AccessToken -TokenName $tokenName
    Assert-JwtCoreClaims `
        -Claims $claims `
        -ExpectedIssuer "$($Authority.TrimEnd('/'))/" `
        -ExpectedAudience $Audience `
        -TokenName $tokenName

    $roleProperty = $claims.PSObject.Properties[$RoleClaimType]
    $roles = if ($null -eq $roleProperty) { @() } else { @($roleProperty.Value) }
    if ($roles -cnotcontains "Seller") {
        throw "$tokenName does not contain the Seller role"
    }
    if ($roles -ccontains "Admin") {
        throw "$tokenName must not contain the Admin role"
    }
}

function Read-SuccessJsonAsync {
    param(
        [Parameter(Mandatory)]
        [System.Net.Http.HttpResponseMessage]$Response
    )

    if (-not $Response.IsSuccessStatusCode) {
        throw "Seller media probe received HTTP $([int]$Response.StatusCode)"
    }

    $payload = $Response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    if ([string]::IsNullOrWhiteSpace($payload)) {
        return $null
    }

    return $payload | ConvertFrom-Json
}

function Invoke-GetJson {
    param(
        [Parameter(Mandatory)]
        [System.Net.Http.HttpClient]$Client,
        [Parameter(Mandatory)]
        [string]$Path,
        [Parameter(Mandatory)]
        [Threading.CancellationToken]$CancellationToken
    )

    $response = $Client.GetAsync($Path, $CancellationToken).GetAwaiter().GetResult()
    try {
        return Read-SuccessJsonAsync -Response $response
    }
    finally {
        $response.Dispose()
    }
}

function Invoke-UploadImage {
    param(
        [Parameter(Mandatory)]
        [System.Net.Http.HttpClient]$Client,
        [Parameter(Mandatory)]
        [Guid]$TargetProductId,
        [Parameter(Mandatory)]
        [string]$FileName,
        [Parameter(Mandatory)]
        [byte[]]$Content,
        [Parameter(Mandatory)]
        [Threading.CancellationToken]$CancellationToken
    )

    $multipart = [System.Net.Http.MultipartFormDataContent]::new()
    $fileContent = [System.Net.Http.ByteArrayContent]::new($Content)
    $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::new("image/png")
    $multipart.Add($fileContent, "file", $FileName)

    try {
        $response = $Client.PostAsync(
            "/gateway/catalog/products/$TargetProductId/images",
            $multipart,
            $CancellationToken).GetAwaiter().GetResult()
        try {
            return Read-SuccessJsonAsync -Response $response
        }
        finally {
            $response.Dispose()
        }
    }
    finally {
        $multipart.Dispose()
    }
}

function Invoke-SetMainImage {
    param(
        [Parameter(Mandatory)]
        [System.Net.Http.HttpClient]$Client,
        [Parameter(Mandatory)]
        [Guid]$TargetProductId,
        [Parameter(Mandatory)]
        [Guid]$ImageId,
        [Parameter(Mandatory)]
        [Threading.CancellationToken]$CancellationToken
    )

    $content = [System.Net.Http.ByteArrayContent]::new([byte[]]::new(0))
    try {
        $response = $Client.PutAsync(
            "/gateway/catalog/products/$TargetProductId/images/$ImageId/main",
            $content,
            $CancellationToken).GetAwaiter().GetResult()
        try {
            return Read-SuccessJsonAsync -Response $response
        }
        finally {
            $response.Dispose()
        }
    }
    finally {
        $content.Dispose()
    }
}

function Invoke-DeleteImage {
    param(
        [Parameter(Mandatory)]
        [System.Net.Http.HttpClient]$Client,
        [Parameter(Mandatory)]
        [Guid]$TargetProductId,
        [Parameter(Mandatory)]
        [Guid]$ImageId,
        [Parameter(Mandatory)]
        [Threading.CancellationToken]$CancellationToken
    )

    $response = $Client.DeleteAsync(
        "/gateway/catalog/products/$TargetProductId/images/$ImageId",
        $CancellationToken).GetAwaiter().GetResult()
    try {
        if (-not $response.IsSuccessStatusCode -and
            $response.StatusCode -ne [System.Net.HttpStatusCode]::NotFound) {
            throw "Seller media cleanup received HTTP $([int]$response.StatusCode)"
        }
    }
    finally {
        $response.Dispose()
    }
}

function Find-OwnedProduct {
    param(
        [Parameter(Mandatory)]
        [System.Net.Http.HttpClient]$Client,
        [Parameter(Mandatory)]
        [object[]]$Stores,
        [Parameter(Mandatory)]
        [Threading.CancellationToken]$CancellationToken
    )

    foreach ($store in $Stores) {
        $storeId = [Guid]$store.id
        $page = Invoke-GetJson `
            -Client $Client `
            -Path "/gateway/catalog/manage/products?storeId=$storeId&pageNumber=1&pageSize=20" `
            -CancellationToken $CancellationToken

        $candidate = @($page.items) |
            Where-Object { @($_.images).Count -le 6 } |
            Select-Object -First 1
        if ($null -ne $candidate) {
            return $candidate
        }
    }

    throw "No owned product with capacity for two temporary images was found"
}

if ($TimeoutSeconds -le 0) {
    throw "TimeoutSeconds must be a positive integer"
}
$gatewayUri = $null
if (-not [Uri]::TryCreate($GatewayBaseUrl, [UriKind]::Absolute, [ref]$gatewayUri) -or
    ($gatewayUri.Scheme -ne [Uri]::UriSchemeHttp -and
     $gatewayUri.Scheme -ne [Uri]::UriSchemeHttps)) {
    throw "GatewayBaseUrl must be an absolute HTTP or HTTPS URL"
}

$accessToken = Read-RequiredEnvironmentValue -Name "SellerChecks__AccessToken"
$authority = Read-RequiredEnvironmentValue -Name "Auth__Authority"
$audience = Read-RequiredEnvironmentValue -Name "Auth__Audience"
$roleClaimType = [Environment]::GetEnvironmentVariable("Auth__RoleClaimType")
if ([string]::IsNullOrWhiteSpace($roleClaimType)) {
    $roleClaimType = "https://ecommerce.local/claims/roles"
}

Assert-SellerAccessTokenClaims `
    -AccessToken $accessToken `
    -Authority $authority `
    -Audience $audience `
    -RoleClaimType $roleClaimType

$client = [System.Net.Http.HttpClient]::new()
$cancellationSource = [Threading.CancellationTokenSource]::new(
    [TimeSpan]::FromSeconds($TimeoutSeconds))
$uploadedImageIds = [Collections.Generic.List[Guid]]::new()
$selectedProductId = [Guid]::Empty
$cleanupFailure = $null

try {
    $client.BaseAddress = [Uri]::new($gatewayUri.GetLeftPart([UriPartial]::Authority))
    $client.DefaultRequestHeaders.Authorization =
        [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $accessToken)
    $client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en")

    $stores = @(
        Invoke-GetJson `
        -Client $client `
        -Path "/gateway/catalog/stores/mine" `
        -CancellationToken $cancellationSource.Token |
            Where-Object { $null -ne $_ }
    )
    if ($stores.Count -eq 0) {
        throw "No store owned by the Seller token was found"
    }
    if ($stores.Count -eq 0) {
        throw "The Seller user does not own a store"
    }

    if ($ProductId -eq [Guid]::Empty) {
        $product = Find-OwnedProduct `
            -Client $client `
            -Stores $stores `
            -CancellationToken $cancellationSource.Token
    }
    else {
        $product = Invoke-GetJson `
            -Client $client `
            -Path "/gateway/catalog/manage/products/$ProductId" `
            -CancellationToken $cancellationSource.Token
    }

    $ownedStoreIds = @($stores | ForEach-Object { [Guid]$_.id })
    if ($null -eq $product.storeId -or
        $ownedStoreIds -cnotcontains [Guid]$product.storeId) {
        throw "The selected product is not owned by the Seller user"
    }
    if (@($product.images).Count -gt 6) {
        throw "The selected product does not have capacity for two temporary images"
    }

    $selectedProductId = [Guid]$product.id
    $pngBytes = [Convert]::FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=")

    Write-Output "Seller ownership resolved; starting temporary media lifecycle."
    $firstImage = Invoke-UploadImage `
        -Client $client `
        -TargetProductId $selectedProductId `
        -FileName "seller-probe-one.png" `
        -Content $pngBytes `
        -CancellationToken $cancellationSource.Token
    $uploadedImageIds.Add([Guid]$firstImage.id)

    $secondImage = Invoke-UploadImage `
        -Client $client `
        -TargetProductId $selectedProductId `
        -FileName "seller-probe-two.png" `
        -Content $pngBytes `
        -CancellationToken $cancellationSource.Token
    $uploadedImageIds.Add([Guid]$secondImage.id)
    $probeImageIds = @([Guid]$firstImage.id, [Guid]$secondImage.id)

    $mainImage = Invoke-SetMainImage `
        -Client $client `
        -TargetProductId $selectedProductId `
        -ImageId ([Guid]$secondImage.id) `
        -CancellationToken $cancellationSource.Token
    if (-not $mainImage.isMain) {
        throw "Catalog did not mark the selected temporary image as main"
    }

    foreach ($imageId in @($uploadedImageIds)) {
        Invoke-DeleteImage `
            -Client $client `
            -TargetProductId $selectedProductId `
            -ImageId $imageId `
            -CancellationToken $cancellationSource.Token
        [void]$uploadedImageIds.Remove($imageId)
    }

    $productAfterCleanup = Invoke-GetJson `
        -Client $client `
        -Path "/gateway/catalog/manage/products/$selectedProductId" `
        -CancellationToken $cancellationSource.Token
    $remainingIds = @($productAfterCleanup.images | ForEach-Object { [Guid]$_.id })
    if ($probeImageIds | Where-Object { $remainingIds -contains $_ }) {
        throw "Temporary image metadata remained after deletion"
    }

    Write-Output "Seller ownership and media lifecycle probe passed."
}
finally {
    foreach ($imageId in @($uploadedImageIds)) {
        try {
            Invoke-DeleteImage `
                -Client $client `
                -TargetProductId $selectedProductId `
                -ImageId $imageId `
                -CancellationToken ([Threading.CancellationToken]::None)
        }
        catch {
            $cleanupFailure = "One or more temporary images could not be cleaned up"
        }
    }

    $client.Dispose()
    $cancellationSource.Dispose()
    $accessToken = $null
    if ($null -ne $cleanupFailure) {
        throw $cleanupFailure
    }
}
