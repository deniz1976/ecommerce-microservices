param(
    [string]$Service = "all",
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

$migrations = @(
    @{
        Name = "catalog"
        Connection = "ConnectionStrings__CatalogDb"
        Project = "src/Services/Catalog/ECommerce.Catalog.Infrastructure/ECommerce.Catalog.Infrastructure.csproj"
        Startup = "src/Services/Catalog/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj"
    },
    @{
        Name = "basket"
        Connection = "ConnectionStrings__BasketDb"
        Project = "src/Services/Basket/ECommerce.Basket.Infrastructure/ECommerce.Basket.Infrastructure.csproj"
        Startup = "src/Services/Basket/ECommerce.Basket.Api/ECommerce.Basket.Api.csproj"
    },
    @{
        Name = "ordering"
        Connection = "ConnectionStrings__OrderingDb"
        Project = "src/Services/Ordering/ECommerce.Ordering.Infrastructure/ECommerce.Ordering.Infrastructure.csproj"
        Startup = "src/Services/Ordering/ECommerce.Ordering.Api/ECommerce.Ordering.Api.csproj"
    },
    @{
        Name = "ordering-saga"
        Connection = "ConnectionStrings__OrderingSagaDb"
        Project = "src/Services/OrderingSaga/ECommerce.OrderingSaga.Infrastructure/ECommerce.OrderingSaga.Infrastructure.csproj"
        Startup = "src/Services/OrderingSaga/ECommerce.OrderingSaga.Worker/ECommerce.OrderingSaga.Worker.csproj"
    },
    @{
        Name = "inventory"
        Connection = "ConnectionStrings__InventoryDb"
        Project = "src/Services/Inventory/ECommerce.Inventory.Infrastructure/ECommerce.Inventory.Infrastructure.csproj"
        Startup = "src/Services/Inventory/ECommerce.Inventory.Api/ECommerce.Inventory.Api.csproj"
    },
    @{
        Name = "payment"
        Connection = "ConnectionStrings__PaymentDb"
        Project = "src/Services/Payment/ECommerce.Payment.Infrastructure/ECommerce.Payment.Infrastructure.csproj"
        Startup = "src/Services/Payment/ECommerce.Payment.Api/ECommerce.Payment.Api.csproj"
    },
    @{
        Name = "shipping"
        Connection = "ConnectionStrings__ShippingDb"
        Project = "src/Services/Shipping/ECommerce.Shipping.Infrastructure/ECommerce.Shipping.Infrastructure.csproj"
        Startup = "src/Services/Shipping/ECommerce.Shipping.Api/ECommerce.Shipping.Api.csproj"
    },
    @{
        Name = "notification"
        Connection = "ConnectionStrings__NotificationDb"
        Project = "src/Services/Notification/ECommerce.Notification.Infrastructure/ECommerce.Notification.Infrastructure.csproj"
        Startup = "src/Services/Notification/ECommerce.Notification.Api/ECommerce.Notification.Api.csproj"
    },
    @{
        Name = "identity"
        Connection = "ConnectionStrings__IdentityDb"
        Project = "src/Services/Identity/ECommerce.Identity.Infrastructure/ECommerce.Identity.Infrastructure.csproj"
        Startup = "src/Services/Identity/ECommerce.Identity.Api/ECommerce.Identity.Api.csproj"
    }
)

$selected = if ($Service -eq "all") {
    $migrations
} else {
    $migrations | Where-Object { $_.Name -eq $Service }
}

if (-not $selected) {
    $names = ($migrations | ForEach-Object { $_.Name }) -join ", "
    throw "Unknown service '$Service'. Valid values: all, $names"
}

foreach ($migration in $selected) {
    $connectionString = [Environment]::GetEnvironmentVariable($migration.Connection, "Process")

    if ([string]::IsNullOrWhiteSpace($connectionString)) {
        throw "Missing $($migration.Connection)"
    }

    Write-Host "Applying $($migration.Name) migrations"
    dotnet ef database update --project $migration.Project --startup-project $migration.Startup
}

Write-Host "Migrations completed"
