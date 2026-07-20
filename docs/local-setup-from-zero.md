# Local Setup From Zero

## Prerequisites

Install:

- .NET SDK 10.0.301 or compatible .NET 10 SDK.
- Docker Desktop.
- Git.
- PowerShell.

Check .NET:

```powershell
dotnet --version
dotnet --list-sdks
```

Check Docker:

```powershell
docker version
docker compose version
```

## Restore .NET Tooling

The project uses local `dotnet-ef`.

```powershell
dotnet tool restore
```

## Build and Test

```powershell
dotnet build ECommerce.sln
dotnet test ECommerce.sln
```

## Required Runtime Values

Required:

```text
ConnectionStrings__CatalogDb
ConnectionStrings__BasketDb
ConnectionStrings__OrderingDb
ConnectionStrings__OrderingSagaDb
ConnectionStrings__InventoryDb
ConnectionStrings__PaymentDb
ConnectionStrings__ShippingDb
ConnectionStrings__NotificationDb
ConnectionStrings__IdentityDb
RabbitMq__ConnectionString
```

Optional for now:

```text
Cloudinary__CloudName
Cloudinary__ApiKey
Cloudinary__ApiSecret
Auth__Authority
Auth__Audience
```

To enable Auth0 `Customer`/`Seller` onboarding synchronization, add these values to Infisical and run Identity through the secret wrapper:

```text
Auth0Management__Enabled=true
Auth0Management__Domain=<tenant host without https://>
Auth0Management__ClientId=<dedicated Management API M2M client id>
Auth0Management__ClientSecret=<secret>
Auth0Management__CustomerRoleId=<Auth0 Customer role id>
Auth0Management__SellerRoleId=<Auth0 Seller role id>
Auth0Management__TimeoutSeconds=10
```

Do not reuse `ecommerce-runtime-integration`; create a separate Auth0 Machine-to-Machine application and authorize only the Management API user-role membership scopes.

## Set Runtime Variables Without Writing Secrets to Files

Preferred development flow is Infisical:

```powershell
infisical login
infisical init
infisical run -- .\scripts\check-runtime-env.ps1
infisical run -- dotnet run --project src/Services/Catalog/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj
```

The repository also includes a wrapper:

```powershell
.\scripts\run-with-secrets.ps1 dotnet run --project src/Services/Catalog/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj
.\scripts\run-with-secrets.ps1 docker compose up -d
```

PowerShell process variables can still be used for quick local tests:

```powershell
$env:ConnectionStrings__CatalogDb = "postgresql://..."
$env:ConnectionStrings__BasketDb = "postgresql://..."
$env:ConnectionStrings__OrderingDb = "postgresql://..."
$env:ConnectionStrings__OrderingSagaDb = "postgresql://..."
$env:ConnectionStrings__InventoryDb = "postgresql://..."
$env:ConnectionStrings__PaymentDb = "postgresql://..."
$env:ConnectionStrings__ShippingDb = "postgresql://..."
$env:ConnectionStrings__NotificationDb = "postgresql://..."
$env:ConnectionStrings__IdentityDb = "postgresql://..."
$env:RabbitMq__ConnectionString = "amqps://..."
```

Check values exist without printing secrets:

```powershell
.\scripts\check-runtime-env.ps1
```

## Apply Migrations

All services:

```powershell
.\scripts\run-migrations.ps1
```

Single service:

```powershell
.\scripts\run-migrations.ps1 -Service inventory
```

## Start Docker Runtime

Start Docker Desktop first.

Then:

```powershell
.\scripts\start-local.ps1
```

If you want to start without smoke test:

```powershell
.\scripts\start-local.ps1 -SkipSmokeTest
```

If you already ran tests:

```powershell
.\scripts\start-local.ps1 -SkipBuild
```

## Run Smoke Test Manually

```powershell
.\scripts\smoke-test.ps1
```

Health only:

```powershell
.\scripts\smoke-test.ps1 -SkipWorkflowProbe
```

## Stop Docker Runtime

```powershell
docker compose down
```

## Common Problems

### Docker engine pipe not found

Meaning:

```text
Docker Desktop is not running.
```

Fix:

```text
Open Docker Desktop and wait until it says the engine is running.
```

### Missing runtime environment variable

Run:

```powershell
.\scripts\check-runtime-env.ps1
```

Then set the missing variables in the current PowerShell session.

### Database migrations fail

Check:

- connection string points to the right database
- Neon database exists
- SSL parameters are included
- password is correct
- network is reachable

### RabbitMQ connection fails

Check:

- CloudAMQP URI is correct
- URI starts with `amqps://`
- virtual host exists
- username and password are correct

## Minimal Daily Workflow

```powershell
dotnet test ECommerce.sln
.\scripts\validate-local.ps1 -SkipBuild
.\scripts\check-runtime-env.ps1
.\scripts\start-local.ps1
```
