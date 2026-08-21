param(
    [string]$GatewayBaseUrl = "http://localhost:15080",
    [ValidateRange(1, 1800)]
    [int]$TimeoutSeconds = 180,
    [ValidateRange(1, 60)]
    [int]$PollingIntervalSeconds = 5
)

$ErrorActionPreference = "Stop"
$deadline = [DateTimeOffset]::UtcNow.AddSeconds($TimeoutSeconds)
$attempt = 0
$lastError = $null

while ([DateTimeOffset]::UtcNow -lt $deadline) {
    $attempt++

    try {
        .\scripts\smoke-test.ps1 -GatewayBaseUrl $GatewayBaseUrl -SkipWorkflowProbe
        Write-Host "Runtime became ready after $attempt attempt(s)"
        exit 0
    }
    catch {
        $lastError = $_.Exception.Message
        Write-Host "Runtime is not ready yet (attempt $attempt): $lastError"
        Start-Sleep -Seconds $PollingIntervalSeconds
    }
}

throw "Runtime did not become ready within $TimeoutSeconds seconds. Last error: $lastError"
