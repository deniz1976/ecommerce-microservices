param(
    [switch]$SkipContainerValidation
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$observabilityRoot = Join-Path $repositoryRoot "deploy/observability"
$dashboardRoot = Join-Path $observabilityRoot "grafana/dashboards"
$prometheusConfig = Join-Path $observabilityRoot "prometheus.yml"
$prometheusRules = Join-Path $observabilityRoot "prometheus-rules"
$collectorConfig = Join-Path $observabilityRoot "otel-collector.yaml"
$dashboardUids = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$requiredDashboardExpressions = @(
    "ecommerce_identity_role_reconciliation_pending",
    "ecommerce_identity_role_reconciliation_oldest_age_seconds",
    "ecommerce_identity_role_reconciliation_successes_total",
    "ecommerce_identity_role_reconciliation_failures_total",
    "ecommerce_catalog_product_image_deletion_pending",
    "ecommerce_catalog_product_image_deletion_oldest_age_seconds",
    "ecommerce_catalog_product_image_deletion_successes_total",
    "ecommerce_catalog_product_image_deletion_failures_total"
)
$requiredAlertNames = @(
    "ECommerceIdentityRoleReconciliationBacklogStale",
    "ECommerceIdentityRoleReconciliationFailures",
    "ECommerceCatalogImageDeletionBacklogStale",
    "ECommerceCatalogImageDeletionFailures"
)
$dashboardExpressions = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)

Get-ChildItem -LiteralPath $dashboardRoot -Filter *.json | ForEach-Object {
    $dashboard = Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json
    $uid = [string]$dashboard.uid

    if ([string]::IsNullOrWhiteSpace($uid)) {
        throw "Grafana dashboard '$($_.Name)' must declare a non-empty uid"
    }

    if (-not $dashboardUids.Add($uid)) {
        throw "Grafana dashboard uid '$uid' is duplicated"
    }

    foreach ($panel in $dashboard.panels) {
        foreach ($target in $panel.targets) {
            $expression = [string]$target.expr
            foreach ($requiredExpression in $requiredDashboardExpressions) {
                if ($expression.IndexOf(
                        $requiredExpression,
                        [System.StringComparison]::Ordinal) -ge 0) {
                    [void]$dashboardExpressions.Add($requiredExpression)
                }
            }
        }
    }
}

foreach ($requiredExpression in $requiredDashboardExpressions) {
    if (-not $dashboardExpressions.Contains($requiredExpression)) {
        throw "Grafana dashboards must query required metric '$requiredExpression'"
    }
}

$prometheusRuleContent = Get-ChildItem -LiteralPath $prometheusRules -Filter *.yml |
    ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw } |
    Out-String

foreach ($requiredAlertName in $requiredAlertNames) {
    if ($prometheusRuleContent.IndexOf(
            "alert: $requiredAlertName",
            [System.StringComparison]::Ordinal) -lt 0) {
        throw "Prometheus rules must declare required alert '$requiredAlertName'"
    }
}

Write-Host "Grafana dashboard and Prometheus rule validation completed"

if ($SkipContainerValidation) {
    return
}

$composeJson = docker compose config --format json | Out-String

if ($LASTEXITCODE -ne 0) {
    throw "Unable to resolve Docker Compose configuration"
}

$compose = $composeJson | ConvertFrom-Json
$prometheusImage = [string]$compose.services.prometheus.image
$collectorImage = [string]$compose.services.PSObject.Properties["otel-collector"].Value.image

if ([string]::IsNullOrWhiteSpace($prometheusImage) -or [string]::IsNullOrWhiteSpace($collectorImage)) {
    throw "Observability image versions could not be resolved from Docker Compose"
}

docker run --rm `
    --volume "${prometheusConfig}:/etc/prometheus/prometheus.yml:ro" `
    --volume "${prometheusRules}:/etc/prometheus/rules:ro" `
    --entrypoint promtool `
    $prometheusImage `
    check config /etc/prometheus/prometheus.yml

if ($LASTEXITCODE -ne 0) {
    throw "Prometheus configuration validation failed"
}

docker run --rm `
    --volume "${collectorConfig}:/etc/otelcol-contrib/config.yaml:ro" `
    $collectorImage `
    validate --config=/etc/otelcol-contrib/config.yaml

if ($LASTEXITCODE -ne 0) {
    throw "OpenTelemetry Collector configuration validation failed"
}

Write-Host "Observability container configuration validation completed"
