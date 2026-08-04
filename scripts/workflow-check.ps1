param(
    [string]$GatewayBaseUrl = "http://localhost:5080",
    [int]$TimeoutSeconds = 60,
    [ValidateSet("all", "basket-checkout", "success", "inventory-failure", "payment-failure", "payment-decline", "notification-signalr", "seller-authorization", "shipping-failure", "customer-cancellation")]
    [string]$Scenario = "all"
)

$ErrorActionPreference = "Stop"

dotnet run --project tools/ECommerce.RuntimeChecks/ECommerce.RuntimeChecks.csproj -- `
    --gateway $GatewayBaseUrl `
    --timeout-seconds $TimeoutSeconds `
    --scenario $Scenario
