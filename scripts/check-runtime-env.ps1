param(
    [switch]$IncludeOptional,
    [string]$EnvFile = ".env"
)

$ErrorActionPreference = "Stop"

if (Test-Path $EnvFile) {
    Get-Content $EnvFile | ForEach-Object {
        $line = $_.Trim()

        if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith("#")) {
            return
        }

        $parts = $line.Split("=", 2)

        if ($parts.Length -eq 2) {
            [Environment]::SetEnvironmentVariable($parts[0].Trim(), $parts[1].Trim(), "Process")
        }
    }
}

$required = @(
    "ConnectionStrings__CatalogDb",
    "ConnectionStrings__BasketDb",
    "ConnectionStrings__OrderingDb",
    "ConnectionStrings__OrderingSagaDb",
    "ConnectionStrings__InventoryDb",
    "ConnectionStrings__PaymentDb",
    "ConnectionStrings__ShippingDb",
    "ConnectionStrings__NotificationDb",
    "ConnectionStrings__IdentityDb",
    "RabbitMq__ConnectionString"
)

$optional = @(
    "Cloudinary__CloudName",
    "Cloudinary__ApiKey",
    "Cloudinary__ApiSecret",
    "Auth__Authority",
    "Auth__Audience"
)

$missingRequired = $required | Where-Object { [string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($_, "Process")) }

if ($missingRequired.Count -gt 0) {
    Write-Host "Missing required environment variables:"
    $missingRequired | ForEach-Object { Write-Host $_ }
    exit 1
}

Write-Host "Required runtime environment variables are set"

if ($IncludeOptional) {
    $missingOptional = $optional | Where-Object { [string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($_, "Process")) }

    if ($missingOptional.Count -gt 0) {
        Write-Host "Missing optional environment variables:"
        $missingOptional | ForEach-Object { Write-Host $_ }
    }
}
