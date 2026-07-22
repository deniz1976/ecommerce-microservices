param(
    [long]$BaselineMessageCount = -1
)

$ErrorActionPreference = "Stop"

$connectionString = [Environment]::GetEnvironmentVariable("RabbitMq__ConnectionString", "Process")

if ([string]::IsNullOrWhiteSpace($connectionString)) {
    throw "RabbitMq__ConnectionString is required"
}

$connectionUri = [Uri]$connectionString
$credentials = $connectionUri.UserInfo.Split(":", 2)

if ($credentials.Length -ne 2) {
    throw "RabbitMq__ConnectionString must contain a username and password"
}

$username = [Uri]::UnescapeDataString($credentials[0])
$password = [Uri]::UnescapeDataString($credentials[1])
$virtualHost = [Uri]::UnescapeDataString($connectionUri.AbsolutePath.TrimStart("/"))

if ([string]::IsNullOrWhiteSpace($virtualHost)) {
    $virtualHost = "/"
}

$encodedVirtualHost = [Uri]::EscapeDataString($virtualHost)
$managementUri = "https://$($connectionUri.Host)/api/queues/$encodedVirtualHost"
$authorizationBytes = [Text.Encoding]::UTF8.GetBytes("$username`:$password")
$authorization = [Convert]::ToBase64String($authorizationBytes)
$response = Invoke-RestMethod -Method Get -Uri $managementUri -Headers @{ Authorization = "Basic $authorization" }
$queues = if ($response.PSObject.Properties.Name -contains "items") {
    @($response.items)
}
else {
    @($response)
}
$errorQueues = @($queues | Where-Object { $_.name -like "*_error" -or $_.name -like "*_skipped" })
$messageCount = if ($errorQueues.Count -eq 0) {
    0L
}
else {
    [long](($errorQueues | Measure-Object -Property messages -Sum).Sum)
}

foreach ($queue in $errorQueues) {
    Write-Host "RabbitMQ failure queue '$($queue.name)' contains $($queue.messages) messages."
}

Write-Host "RabbitMQ failure queue total: $messageCount"

if ($BaselineMessageCount -ge 0 -and $messageCount -gt $BaselineMessageCount) {
    throw "RabbitMQ failure queues increased from $BaselineMessageCount to $messageCount messages during runtime verification."
}

Write-Output $messageCount
