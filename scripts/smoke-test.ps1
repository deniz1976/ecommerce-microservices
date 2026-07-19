param(
    [string]$GatewayBaseUrl = "http://localhost:5080",
    [switch]$SkipWorkflowProbe
)

$ErrorActionPreference = "Stop"

function Invoke-JsonRequest {
    param(
        [string]$Method,
        [string]$Uri,
        [object]$Body = $null
    )

    $headers = @{
        "Accept-Language" = "en"
        "Content-Type" = "application/json"
    }

    if (-not [string]::IsNullOrWhiteSpace($accessToken)) {
        $headers["Authorization"] = "Bearer $accessToken"
    }

    if ($null -eq $Body) {
        return Invoke-RestMethod -Method $Method -Uri $Uri -Headers $headers
    }

    $json = $Body | ConvertTo-Json -Depth 20
    return Invoke-RestMethod -Method $Method -Uri $Uri -Headers $headers -Body $json
}

function Assert-StatusCode {
    param(
        [string]$Name,
        [string]$Uri
    )

    Write-Host "Checking $Name health at $Uri"

    try {
        $response = Invoke-WebRequest -Method Get -Uri $Uri -UseBasicParsing
    }
    catch {
        throw "$Name health check failed at '$Uri': $($_.Exception.Message)"
    }

    if ($response.StatusCode -lt 200 -or $response.StatusCode -gt 299) {
        throw "$Name health check at '$Uri' returned $($response.StatusCode)"
    }

    Write-Host "$Name OK"
}

$baseUrl = $GatewayBaseUrl.TrimEnd("/")

Assert-StatusCode "Gateway" "$baseUrl/health/live"

$services = @("catalog", "basket", "ordering", "inventory", "payment", "shipping", "notification", "identity")

foreach ($service in $services) {
    Assert-StatusCode $service "$baseUrl/gateway/health/$service"
}

if ($SkipWorkflowProbe) {
    Write-Host "Smoke test completed"
    exit 0
}

$accessToken = $env:RuntimeChecks__AccessToken

if ([string]::IsNullOrWhiteSpace($accessToken)) {
    throw "RuntimeChecks__AccessToken must contain an Auth0 access token with the Admin role for the workflow probe"
}

$suffix = [Guid]::NewGuid().ToString("N").Substring(0, 12)
$productId = [Guid]::NewGuid()

$user = Invoke-JsonRequest Post "$baseUrl/gateway/users" @{
    email = "smoke-$suffix@example.com"
    displayName = "Smoke Test $suffix"
    password = "SmokeTest123!"
}

Invoke-JsonRequest Put "$baseUrl/gateway/inventory/items/$productId" @{
    quantityOnHand = 25
} | Out-Null

$order = Invoke-JsonRequest Post "$baseUrl/gateway/orders" @{
    customerId = $user.id
    currency = "USD"
    recipientName = "Smoke Test"
    addressLine = "Integration Avenue 1"
    city = "Istanbul"
    countryCode = "TR"
    postalCode = "34000"
    items = @(
        @{
            productId = $productId
            productName = "Smoke Product"
            quantity = 1
            unitPrice = 10.50
            currency = "USD"
        }
    )
}

if ([string]::IsNullOrWhiteSpace($order.id)) {
    throw "Order response did not include an id"
}

Write-Host "User $($user.id) OK"
Write-Host "Inventory item $productId OK"
Write-Host "Order $($order.id) OK"
Write-Host "Smoke test completed"
