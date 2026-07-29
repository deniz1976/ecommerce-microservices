param()

$ErrorActionPreference = "Stop"

.\scripts\request-runtime-access-token.ps1

Push-Location .\src\Frontend
try {
    npm run check:notification-signalr
    if ($LASTEXITCODE -ne 0) {
        throw "Notification SignalR check failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}
