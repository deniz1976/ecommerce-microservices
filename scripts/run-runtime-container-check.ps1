[CmdletBinding()]
param(
    [ValidateSet(
        "all",
        "basket-checkout",
        "success",
        "inventory-failure",
        "payment-failure",
        "payment-decline",
        "notification-signalr",
        "seller-authorization",
        "shipping-failure")]
    [string]$Scenario = "all",
    [ValidateRange(30, 600)]
    [int]$TimeoutSeconds = 180,
    [switch]$IncludeObservability,
    [switch]$SkipBuild,
    [switch]$KeepRunning
)

$ErrorActionPreference = "Stop"
$composeTouched = $false
$verificationSucceeded = $false

function Invoke-Compose {
    param(
        [Parameter(Mandatory)]
        [string[]]$ComposeArguments
    )

    & docker compose @ComposeArguments
    if ($LASTEXITCODE -ne 0) {
        throw "docker compose $($ComposeArguments -join ' ') failed with exit code $LASTEXITCODE"
    }
}

try {
    .\scripts\check-runtime-env.ps1

    $baselineOutput = @(
        .\scripts\check-rabbitmq-error-queues.ps1
    )
    [long]$failureQueueBaseline = $baselineOutput[-1]

    .\scripts\request-runtime-access-token.ps1

    $env:CatalogClient__TimeoutSeconds = "15"
    $env:CATALOG_API_HOST_PORT = "15283"
    $env:ORDERING_API_HOST_PORT = "15265"
    $env:SHIPPING_API_HOST_PORT = "15187"
    $env:NOTIFICATION_API_HOST_PORT = "15234"
    $env:DemoPayment__Scenario = if ($Scenario -eq "payment-decline") {
        "Decline"
    }
    else {
        "Success"
    }

    if ($IncludeObservability) {
        $env:OTEL_EXPORTER_OTLP_ENDPOINT = ""
        $env:OTEL_EXPORTER_OTLP_PROTOCOL = ""
        $env:OTEL_EXPORTER_OTLP_HEADERS = ""
        $env:Observability__OtlpEndpoint = "http://otel-collector:4317"

        .\scripts\validate-observability.ps1
        $composeTouched = $true
        Invoke-Compose -ComposeArguments @("up", "--detach", "grafana")
    }

    $composeTouched = $true
    $applicationArguments = @("up")
    if (-not $SkipBuild) {
        $applicationArguments += "--build"
    }

    $applicationArguments += @(
        "--detach",
        "api-gateway",
        "ordering-saga-worker"
    )
    Invoke-Compose -ComposeArguments $applicationArguments
    .\scripts\wait-for-runtime.ps1 -TimeoutSeconds 300
    .\scripts\workflow-check.ps1 `
        -Scenario $Scenario `
        -TimeoutSeconds $TimeoutSeconds
    .\scripts\check-rabbitmq-error-queues.ps1 `
        -BaselineMessageCount $failureQueueBaseline | Out-Null

    Write-Host "Runtime container verification '$Scenario' completed."
    $verificationSucceeded = $true
}
finally {
    if ($composeTouched -and (-not $KeepRunning -or -not $verificationSucceeded)) {
        & docker compose down --volumes --remove-orphans
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Runtime verification completed, but Docker Compose cleanup failed."
        }
    }
    elseif ($composeTouched -and $verificationSucceeded) {
        Write-Host "Runtime containers were left running for follow-up verification."
    }
}
