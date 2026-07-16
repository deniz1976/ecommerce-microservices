# Runtime and Operations

## Local Runtime

Local runtime uses:

- Docker Compose
- Redis Cloud or local Redis container fallback
- managed Neon PostgreSQL
- managed CloudAMQP RabbitMQ
- local .NET build
- Infisical for shared development secrets

Docker Compose file:

```text
docker-compose.yml
```

## Docker Services

Services in Docker Compose:

- `redis`
- `catalog-api`
- `basket-api`
- `ordering-api`
- `inventory-api`
- `payment-api`
- `shipping-api`
- `notification-api`
- `identity-api`
- `ordering-saga-worker`
- `api-gateway`

## Ports

Local exposed ports:

```text
api-gateway       5080
catalog-api       5283
basket-api        5041
ordering-api      5265
inventory-api     5054
payment-api       5004
shipping-api      5187
notification-api  5234
identity-api      5090
redis             6379
```

## API Gateway

Main local gateway:

```text
http://localhost:5080
```

Health:

```text
http://localhost:5080/health/live
http://localhost:5080/health/ready
```

Gateway service health routes:

```text
/gateway/health/catalog
/gateway/health/basket
/gateway/health/ordering
/gateway/health/inventory
/gateway/health/payment
/gateway/health/shipping
/gateway/health/notification
/gateway/health/identity
```

## Redis

Current local Redis:

```text
redis:7-alpine
```

Used by:

```text
Basket Service
```

Local connection inside Docker:

```text
redis:6379
```

Why Redis exists:

- Active basket state changes often.
- It is temporary.
- Redis is fast and simple for key/value state.

Production recommendation:

- Managed Redis provider.
- Not the same container as Basket.

Possible providers:

- Upstash Redis
- Redis Cloud
- Azure Cache for Redis
- AWS ElastiCache

## Secrets

Development secrets are managed in Infisical under the project environment selected in the Infisical dashboard.

Managed Grafana Cloud export uses `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_EXPORTER_OTLP_PROTOCOL`, and secret `OTEL_EXPORTER_OTLP_HEADERS` from Infisical. The standard variables take precedence over the Docker-local `Observability__OtlpEndpoint`; without either endpoint, services run with no OTLP exporter.

`Observability__RedactionEnabled` defaults to `true`. It masks sensitive trace tags and structured-log attributes before export. This does not sanitize arbitrary free-text log bodies, so application logs must never embed credentials or personal/payment secrets in message text.

Recommended local command style:

```powershell
infisical run -- dotnet run --project src/Services/Catalog/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj
```

Docker Compose can also receive values from the shell where `infisical run` is used:

```powershell
infisical run -- docker compose up -d
```

The repository still includes `.env.example` only as a list of required names. Real `.env` files are ignored and should not be committed.

Required secret names:

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
Redis__ConnectionString
RabbitMq__ConnectionString
Cloudinary__CloudName
Cloudinary__ApiKey
Cloudinary__ApiSecret
Auth__Authority
Auth__Audience
Auth__RequireHttpsMetadata
```

## PostgreSQL

Current provider:

```text
Neon
```

One database per service:

- `catalog_db`
- `basket_db`
- `ordering_db`
- `ordering_saga_db`
- `inventory_db`
- `payment_db`
- `shipping_db`
- `notification_db`
- `identity_db`

Connection strings are supplied through environment variables:

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
```

## RabbitMQ

Current provider:

```text
CloudAMQP
```

Preferred config:

```text
RabbitMq__ConnectionString
```

The app supports a single `amqps://` URI.

Alternative split config:

```text
RabbitMq__Host
RabbitMq__Port
RabbitMq__Username
RabbitMq__Password
RabbitMq__VirtualHost
RabbitMq__UseSsl
```

## Dockerfile

Root file:

```text
Dockerfile
```

It is generic. Each service passes build args:

```text
PROJECT
ASSEMBLY_NAME
```

This avoids one Dockerfile per service.

## Environment Variables

Docker Compose can load:

```text
.env
```

but `.env` is ignored by git.

Compose also maps host environment variables into containers through `x-managed-env`.

This lets us run without writing real secrets to tracked files.

## Health Checks

Every API has:

```text
/health/live
/health/ready
```

The gateway also exposes its own health endpoints.

## Current Runtime Blocker

Docker Desktop must be running.

If Docker Desktop is closed, this error can appear:

```text
dockerDesktopLinuxEngine pipe not found
```

That means:

```text
Docker engine is not available.
```

Fix:

```text
Start Docker Desktop, then run start-local.ps1 again.
```

## Recommended Runtime Startup

1. Set required environment variables.
2. Start Docker Desktop.
3. Run migrations if needed.
4. Start local runtime.
5. Run smoke test.

Commands:

```powershell
.\scripts\check-runtime-env.ps1
.\scripts\run-migrations.ps1
.\scripts\start-local.ps1
```

## Production Direction

Production should use:

- Container hosting platform.
- Managed PostgreSQL.
- Managed RabbitMQ.
- Managed Redis.
- Centralized logs.
- Metrics dashboards.
- Secret manager.
- GitHub Actions build/test/validation pipeline plus a future protected release/deployment pipeline.

Not recommended for production:

- Redis inside the same service container.
- Real secrets in appsettings files.
- Manual migration execution without release process.
- No dead-letter monitoring for RabbitMQ.

## Continuous Integration

`.github/workflows/ci.yml` runs secretless quality gates for every push and pull request. It builds and tests the .NET solution, lints and builds the Next.js frontend, validates PowerShell syntax and Docker Compose, and scans repository files for secret-like values. After those jobs pass, Compose/BuildKit builds all ten .NET application images with a shared build cache. The workflow intentionally does not run migrations, external workflow probes, image publishing, or deployment with production credentials.

## Trusted Runtime Integration

`.github/workflows/runtime-integration.yml` is a manually triggered managed-environment test. It targets the protected GitHub environment `runtime-integration`, obtains short-lived access to Infisical through GitHub OIDC, applies all EF migrations, starts the application Compose graph, waits for downstream health, and runs the selected success or compensation scenario.

After Infisical injection, the workflow exchanges `RuntimeChecks__Auth0ClientId` and secret `RuntimeChecks__Auth0ClientSecret` through Auth0 Client Credentials for a short-lived token requesting only `inventory:write`. The token is masked, exists only for the job, and does not grant Catalog or Identity administrator access.

Configure these non-secret variables on the GitHub `runtime-integration` environment:

```text
INFISICAL_IDENTITY_ID
INFISICAL_PROJECT_SLUG
```

In Infisical, grant the machine identity least-privilege read access to the required non-production environment and restrict its OIDC subject to:

```text
repo:<owner>/<repository>:environment:runtime-integration
```

The workflow is deliberately manual and serialized because it applies migrations and writes test records to the configured databases. It always tears down local runner containers; managed test records remain subject to the environment's retention policy.
