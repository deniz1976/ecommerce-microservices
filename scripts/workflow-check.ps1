param(
    [string]$GatewayBaseUrl = "http://localhost:5080",
    [int]$TimeoutSeconds = 60,
    [ValidateSet("all", "success", "inventory-failure", "payment-failure", "shipping-failure")]
    [string]$Scenario = "all"
)

$ErrorActionPreference = "Stop"

dotnet run --project tools/ECommerce.RuntimeChecks/ECommerce.RuntimeChecks.csproj -- `
    --gateway $GatewayBaseUrl `
    --timeout-seconds $TimeoutSeconds `
    --scenario $Scenario
