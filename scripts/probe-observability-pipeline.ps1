[CmdletBinding()]
param(
    [ValidateRange(60, 300)]
    [int]$TimeoutSeconds = 180,
    [switch]$KeepRunning
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$probeServiceName = "ECommerce.ObservabilityProbe"
$composeTouched = $false
$probeSucceeded = $false
$collectorHealthHostPort = if ($env:OTEL_COLLECTOR_HEALTH_HOST_PORT) {
    $env:OTEL_COLLECTOR_HEALTH_HOST_PORT
}
else {
    "14133"
}
$prometheusHostPort = if ($env:PROMETHEUS_HOST_PORT) {
    $env:PROMETHEUS_HOST_PORT
}
else {
    "19090"
}
$startTimeUnixNano = [string](
    [DateTimeOffset]::UtcNow.AddMinutes(-1).ToUnixTimeMilliseconds() * 1000000)

function Invoke-Compose {
    param(
        [Parameter(Mandatory)]
        [string[]]$Arguments
    )

    & docker compose @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "docker compose $($Arguments -join ' ') failed with exit code $LASTEXITCODE"
    }
}

function Wait-Until {
    param(
        [Parameter(Mandatory)]
        [scriptblock]$Condition,
        [Parameter(Mandatory)]
        [string]$Description
    )

    $deadline = [DateTimeOffset]::UtcNow.AddSeconds($TimeoutSeconds)
    do {
        try {
            if (& $Condition) {
                return
            }
        }
        catch {
            if ([DateTimeOffset]::UtcNow -ge $deadline) {
                throw
            }
        }

        Start-Sleep -Seconds 2
    }
    while ([DateTimeOffset]::UtcNow -lt $deadline)

    throw "Timed out waiting for $Description"
}

function Test-HttpOk {
    param(
        [Parameter(Mandatory)]
        [string]$Uri
    )

    $response = Invoke-WebRequest -Uri $Uri -UseBasicParsing -TimeoutSec 5
    return $response.StatusCode -eq 200
}

function Send-ProbeMetrics {
    param(
        [Parameter(Mandatory)]
        [long]$CounterValue
    )

    $timeUnixNano = [string](
        [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds() * 1000000)
    $resourceAttributes = @(
        @{
            key = "service.name"
            value = @{ stringValue = $probeServiceName }
        }
    )
    $metrics = @(
        New-GaugeMetric "ecommerce.identity.role_reconciliation.pending" 1 $timeUnixNano
        New-GaugeMetric `
            "ecommerce.identity.role_reconciliation.oldest.age" `
            360 `
            $timeUnixNano `
            -Unit "s"
        New-CounterMetric "ecommerce.identity.role_reconciliation.successes" $CounterValue $timeUnixNano
        New-CounterMetric "ecommerce.identity.role_reconciliation.failures" $CounterValue $timeUnixNano
        New-GaugeMetric "ecommerce.catalog.product_image_deletion.pending" 1 $timeUnixNano
        New-GaugeMetric `
            "ecommerce.catalog.product_image_deletion.oldest.age" `
            360 `
            $timeUnixNano `
            -Unit "s"
        New-CounterMetric "ecommerce.catalog.product_image_deletion.successes" $CounterValue $timeUnixNano
        New-CounterMetric "ecommerce.catalog.product_image_deletion.failures" $CounterValue $timeUnixNano
    )
    $payload = @{
        resourceMetrics = @(
            @{
                resource = @{ attributes = $resourceAttributes }
                scopeMetrics = @(
                    @{
                        scope = @{ name = "ECommerce.ObservabilityProbe" }
                        metrics = $metrics
                    }
                )
            }
        )
    } | ConvertTo-Json -Depth 20 -Compress

    Invoke-RestMethod `
        -Uri "http://localhost:4318/v1/metrics" `
        -Method Post `
        -ContentType "application/json" `
        -Body $payload `
        -TimeoutSec 10 | Out-Null
}

function New-GaugeMetric {
    param(
        [Parameter(Mandatory)]
        [string]$Name,
        [Parameter(Mandatory)]
        [long]$Value,
        [Parameter(Mandatory)]
        [string]$TimeUnixNano,
        [string]$Unit = ""
    )

    $metric = @{
        name = $Name
        gauge = @{
            dataPoints = @(
                @{
                    timeUnixNano = $TimeUnixNano
                    asInt = [string]$Value
                }
            )
        }
    }
    if (-not [string]::IsNullOrWhiteSpace($Unit)) {
        $metric.unit = $Unit
    }

    return $metric
}

function New-CounterMetric {
    param(
        [Parameter(Mandatory)]
        [string]$Name,
        [Parameter(Mandatory)]
        [long]$Value,
        [Parameter(Mandatory)]
        [string]$TimeUnixNano
    )

    return @{
        name = $Name
        sum = @{
            aggregationTemporality = 2
            isMonotonic = $true
            dataPoints = @(
                @{
                    startTimeUnixNano = $startTimeUnixNano
                    timeUnixNano = $TimeUnixNano
                    asInt = [string]$Value
                }
            )
        }
    }
}

function Get-PrometheusQueryResult {
    param(
        [Parameter(Mandatory)]
        [string]$Query
    )

    $encodedQuery = [Uri]::EscapeDataString($Query)
    $response = Invoke-RestMethod `
        -Uri "http://localhost:$prometheusHostPort/api/v1/query?query=$encodedQuery" `
        -TimeoutSec 10
    if ($response.status -ne "success") {
        throw "Prometheus query failed"
    }

    return @($response.data.result)
}

function Test-MetricValue {
    param(
        [Parameter(Mandatory)]
        [string]$MetricName,
        [Parameter(Mandatory)]
        [double]$ExpectedValue
    )

    $result = @(
        Get-PrometheusQueryResult `
            "$MetricName{service_name=`"$probeServiceName`"}"
    )
    if ($result.Count -ne 1) {
        Write-Verbose "$MetricName expected one series but found $($result.Count)"
        return $false
    }

    [double]$actualValue = $result[0].value[1]
    if ($actualValue -ne $ExpectedValue) {
        Write-Verbose "$MetricName expected $ExpectedValue but found $actualValue"
        return $false
    }

    return $true
}

function Test-AlertRulesPending {
    $requiredRules = @(
        "ECommerceIdentityRoleReconciliationBacklogStale",
        "ECommerceIdentityRoleReconciliationFailures",
        "ECommerceCatalogImageDeletionBacklogStale",
        "ECommerceCatalogImageDeletionFailures"
    )
    $response = Invoke-RestMethod `
        -Uri "http://localhost:$prometheusHostPort/api/v1/rules?type=alert" `
        -TimeoutSec 10
    $rules = @($response.data.groups | ForEach-Object { $_.rules })

    foreach ($requiredRule in $requiredRules) {
        $rule = $rules | Where-Object { $_.name -eq $requiredRule }
        if ($null -eq $rule -or $rule.health -ne "ok") {
            return $false
        }

        $probeAlert = @(
            $rule.alerts |
                Where-Object { $_.labels.service_name -eq $probeServiceName }
        )
        if ($probeAlert.Count -ne 1 -or
            $probeAlert[0].state -notin @("pending", "firing")) {
            return $false
        }
    }

    return $true
}

function Test-GrafanaProvisioning {
    $health = Invoke-RestMethod `
        -Uri "http://localhost:3001/api/health" `
        -TimeoutSec 10
    if ($health.database -ne "ok") {
        return $false
    }

    $dashboards = @(
        Invoke-RestMethod `
            -Uri "http://localhost:3001/api/search?type=dash-db" `
            -TimeoutSec 10
    )
    $uids = @($dashboards | ForEach-Object { $_.uid })
    if ("ecommerce-messaging" -notin $uids -or
        "ecommerce-catalog-cleanup" -notin $uids) {
        return $false
    }

    $dataSource = Invoke-RestMethod `
        -Uri "http://localhost:3001/api/datasources/uid/prometheus/health" `
        -TimeoutSec 10
    return $dataSource.status -eq "OK"
}

try {
    Set-Location $repositoryRoot

    $runningContainers = @(& docker compose ps --quiet)
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to inspect Docker Compose state"
    }

    if ($runningContainers.Count -ne 0) {
        throw "The observability probe requires a clean project Compose state"
    }

    .\scripts\validate-observability.ps1
    $composeTouched = $true
    Invoke-Compose -Arguments @("up", "--detach", "grafana")

    Wait-Until `
        -Description "OpenTelemetry Collector health" `
        -Condition { Test-HttpOk "http://localhost:$collectorHealthHostPort/" }
    Wait-Until `
        -Description "Prometheus readiness" `
        -Condition { Test-HttpOk "http://localhost:$prometheusHostPort/-/ready" }
    Wait-Until `
        -Description "Grafana provisioning" `
        -Condition { Test-GrafanaProvisioning }

    Send-ProbeMetrics -CounterValue 0
    Wait-Until `
        -Description "the initial observability probe scrape" `
        -Condition {
            (Test-MetricValue `
                "ecommerce_identity_role_reconciliation_pending" 1) -and
            (Test-MetricValue `
                "ecommerce_catalog_product_image_deletion_pending" 1) -and
            (Test-MetricValue `
                "ecommerce_identity_role_reconciliation_failures_total" 0) -and
            (Test-MetricValue `
                "ecommerce_catalog_product_image_deletion_failures_total" 0)
        }

    Send-ProbeMetrics -CounterValue 1
    Wait-Until `
        -Description "all eight exported metric series" `
        -Condition {
            (Test-MetricValue `
                "ecommerce_identity_role_reconciliation_pending" 1) -and
            (Test-MetricValue `
                "ecommerce_identity_role_reconciliation_oldest_age_seconds" 360) -and
            (Test-MetricValue `
                "ecommerce_identity_role_reconciliation_successes_total" 1) -and
            (Test-MetricValue `
                "ecommerce_identity_role_reconciliation_failures_total" 1) -and
            (Test-MetricValue `
                "ecommerce_catalog_product_image_deletion_pending" 1) -and
            (Test-MetricValue `
                "ecommerce_catalog_product_image_deletion_oldest_age_seconds" 360) -and
            (Test-MetricValue `
                "ecommerce_catalog_product_image_deletion_successes_total" 1) -and
            (Test-MetricValue `
                "ecommerce_catalog_product_image_deletion_failures_total" 1)
        }
    Wait-Until `
        -Description "the four probe alert series" `
        -Condition { Test-AlertRulesPending }

    Write-Host "Observability pipeline probe completed"
    $probeSucceeded = $true
}
finally {
    if ($composeTouched -and (-not $KeepRunning -or -not $probeSucceeded)) {
        & docker compose down --volumes --remove-orphans
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Observability probe completed, but Docker Compose cleanup failed."
        }
    }
    elseif ($composeTouched -and $probeSucceeded) {
        Write-Host "Observability containers were left running for follow-up verification."
    }
}
