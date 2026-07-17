# Scripts and Local Commands

## Why There Are PowerShell Scripts

The project has several moving parts:

- .NET solution
- PostgreSQL migrations
- Docker Compose
- Redis
- RabbitMQ
- Gateway health checks
- end-to-end smoke checks
- secret scanning

PowerShell scripts make these repeated tasks less error-prone.

## Script Summary

```text
scripts/check-runtime-env.ps1
scripts/load-env.ps1
scripts/run-with-secrets.ps1
scripts/run-migrations.ps1
scripts/smoke-test.ps1
scripts/start-local.ps1
scripts/validate-local.ps1
```

## check-runtime-env.ps1

Purpose:

```text
Checks whether required runtime environment variables are set.
```

It does not print secret values. It only prints missing variable names.

If `.env` exists, it is loaded into the current process before the check.

Checks required variables:

- `ConnectionStrings__CatalogDb`
- `ConnectionStrings__BasketDb`
- `ConnectionStrings__OrderingDb`
- `ConnectionStrings__OrderingSagaDb`
- `ConnectionStrings__InventoryDb`
- `ConnectionStrings__PaymentDb`
- `ConnectionStrings__ShippingDb`
- `ConnectionStrings__NotificationDb`
- `ConnectionStrings__IdentityDb`
- `RabbitMq__ConnectionString`

Command:

```powershell
.\scripts\check-runtime-env.ps1
```

With optional variables:

```powershell
.\scripts\check-runtime-env.ps1 -IncludeOptional
```

Optional variables include Cloudinary and Auth config.

## load-env.ps1

Purpose:

```text
Loads .env values into the current PowerShell process.
```

This remains available for private local-only experiments. Shared development secrets should come from Infisical.

Use it before running services without Docker when a private `.env` exists:

```powershell
.\scripts\load-env.ps1
dotnet run --project src/Services/Catalog/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj
```

Preferred shared development flow:

```powershell
infisical run -- dotnet run --project src/Services/Catalog/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj
```

## run-with-secrets.ps1

Purpose:

```text
Runs any command through Infisical so secrets are injected as environment variables.
```

Example:

```powershell
.\scripts\run-with-secrets.ps1 dotnet run --project src/Services/Catalog/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj
```

Docker Compose:

```powershell
.\scripts\run-with-secrets.ps1 docker compose up -d
```

Use `-Environment` if the Infisical environment slug is not `dev`:

```powershell
.\scripts\run-with-secrets.ps1 -Environment dev dotnet test ECommerce.sln
```

## run-migrations.ps1

Purpose:

```text
Runs EF Core database migrations for all service databases.
```

Command:

```powershell
.\scripts\run-migrations.ps1
```

Run only one service:

```powershell
.\scripts\run-migrations.ps1 -Service catalog
```

Supported services:

- `catalog`
- `basket`
- `ordering`
- `ordering-saga`
- `inventory`
- `payment`
- `shipping`
- `notification`
- `identity`

What it does:

```text
dotnet ef database update
```

for each service's infrastructure project and startup project.

It can read `.env` if present, but secrets should not be committed.

## smoke-test.ps1

Purpose:

```text
Runs a small end-to-end runtime check through the API Gateway.
```

Smoke test means:

```text
A quick check that the system is alive and a basic business path works.
```

It is not a full test suite.

Command:

```powershell
.\scripts\smoke-test.ps1
```

What it checks:

1. Gateway health.
2. Service health routes through gateway.
3. User registration.
4. Inventory stock seed through gateway.
5. Order creation through gateway.

Health-only mode:

```powershell
.\scripts\smoke-test.ps1 -SkipWorkflowProbe
```

Custom gateway:

```powershell
.\scripts\smoke-test.ps1 -GatewayBaseUrl http://localhost:5080
```

## wait-for-runtime.ps1

Purpose:

```text
Waits until the gateway and every downstream service health route respond successfully.
```

It repeatedly calls the health-only smoke test, making it safer than a fixed startup sleep in CI:

```powershell
.\scripts\wait-for-runtime.ps1 -TimeoutSeconds 240
```

The command fails after the bounded timeout and reports the last health error. `-GatewayBaseUrl` and `-PollingIntervalSeconds` can be overridden when needed.

## request-runtime-access-token.ps1

Requests a short-lived Auth0 Client Credentials token with `inventory:write` from `Auth__Authority` and `Auth__Audience`. It reads `RuntimeChecks__Auth0ClientId` and `RuntimeChecks__Auth0ClientSecret`, never prints the token, and exports it as `RuntimeChecks__AccessToken` for the current process or subsequent GitHub Actions steps.

The workflow checker prints non-sensitive scenario and probe progress. On failure it prints the exception type and message, but never prints the access token or database connection strings.
Its direct PostgreSQL probes accept both Neon-style `postgresql://` URIs and native Npgsql connection strings by using the shared persistence normalizer.

## start-local.ps1

Purpose:

```text
Starts the local Docker runtime in a repeatable way.
```

Command:

```powershell
.\scripts\start-local.ps1
```

What it does:

1. Checks Docker engine.
2. Checks required runtime env variables.
3. Runs `dotnet test ECommerce.sln`.
4. Runs `docker compose up --build -d`.
5. Waits briefly.
6. Runs smoke test.

Skip build/test:

```powershell
.\scripts\start-local.ps1 -SkipBuild
```

Skip smoke test:

```powershell
.\scripts\start-local.ps1 -SkipSmokeTest
```

Current note:

If Docker Desktop is not running, this script fails early. That is expected.

## validate-local.ps1

Purpose:

```text
Runs local repository validation.
```

Command:

```powershell
.\scripts\validate-local.ps1
```

The secret scan prefers `rg`; when it is unavailable, the script scans Git-tracked files with PowerShell `Select-String`. An `rg` exit code of `1` means that no match was found, so the script normalizes that expected result to process exit code `0`.

Skip build:

```powershell
.\scripts\validate-local.ps1 -SkipBuild
```

What it checks:

1. Optional solution build.
2. PowerShell syntax for all scripts.
3. `docker compose config`.
4. Secret-like values are not present in repository files.

The secret scan covers credential-bearing PostgreSQL/RabbitMQ URIs, Neon identifiers, and ODBC/Npgsql-style connection-string literals. Tests should build synthetic connection strings at runtime instead of committing even fake credential-shaped literals.

Why it exists:

- We do not want accidental secrets committed.
- We want broken scripts caught early.
- We want Docker Compose syntax checked.

## Common Local Commands

Restore tools:

```powershell
dotnet tool restore
```

Build:

```powershell
dotnet build ECommerce.sln
```

Run tests:

```powershell
dotnet test ECommerce.sln
```

Validate:

```powershell
.\scripts\validate-local.ps1
```

Apply migrations:

```powershell
.\scripts\run-migrations.ps1
```

Start Docker runtime:

```powershell
.\scripts\start-local.ps1
```

Smoke test:

```powershell
.\scripts\smoke-test.ps1
```

## Why Secrets Are Not Written to Files

The repository is staged in git.

If a real password or connection string is written to a tracked file, it can accidentally enter git history.

Safer approach:

```text
Use process environment variables for local runs.
Use secret manager or CI/CD secrets in production.
Keep .env ignored by git.
Keep .env.example empty.
```

The project has `.gitignore` entries for:

```text
.env
.env.*
```

except:

```text
.env.example
```

which contains only empty placeholders.
