# ECommerce Microservices

Production-oriented e-commerce backend built with .NET 10, independent microservices, RabbitMQ through MassTransit, database-per-service persistence, saga orchestration, transactional outbox, localization, observability, and OAuth 2.0/OIDC-ready security.

Detailed documentation starts at [docs/README.md](./docs/README.md). Turkish documentation starts at [docs/tr/README.md](./docs/tr/README.md). AI-first graph-ready documentation starts at [docs/ai/README.md](./docs/ai/README.md).

## Phase

Current phase: Phase 2.

Completed:

- Validated local .NET SDK `10.0.301`.
- Created `ECommerce.sln`.
- Created the requested root configuration files.
- Created the requested source and test folder structure.
- Created BuildingBlocks class library projects.
- Added message contracts for the first order workflow.
- Added shared error, result, paging, and API error response models.
- Added English and Turkish error message localization.
- Added MassTransit RabbitMQ configuration helpers.
- Added MassTransit EF Core PostgreSQL outbox registration helper.
- Added PostgreSQL EF Core registration helper.
- Added OpenTelemetry registration helper.
- Added OAuth 2.0/OIDC-ready JWT Bearer and current user abstractions.
- Created Catalog service projects.
- Added Catalog domain models for products, translations, images, categories, and brands.
- Added Catalog application DTOs, product service, repository abstraction, and image provider abstraction.
- Added Catalog PostgreSQL EF Core DbContext, mappings, repository implementation, and design-time DbContext factory.
- Added Catalog REST endpoints under `/api/v1/products`.
- Added `Accept-Language` based localized Catalog API errors for English and Turkish.
- Applied Catalog migration to Neon `catalog_db`.
- Created Basket service projects.
- Added Redis-backed active basket abstraction with local in-memory fallback when Redis is not configured.
- Added PostgreSQL basket checkout snapshot/history persistence.
- Added Basket REST endpoints under `/api/v1/baskets`.
- Applied Basket migration to Neon `basket_db`.
- Created Ordering service projects.
- Added order aggregate, order item model, and order lifecycle status.
- Added Ordering REST endpoints under `/api/v1/orders`.
- Added `OrderSubmitted` publishing through MassTransit with EF Core transactional outbox.
- Applied Ordering migration to Neon `ordering_db`.
- Created Inventory service projects.
- Added stock item and stock reservation models.
- Added idempotent `ReserveInventory` and `ReleaseInventory` consumers.
- Added `InventoryReserved` and `InventoryReservationFailed` publishing through MassTransit with EF Core transactional outbox.
- Applied Inventory migration to Neon `inventory_db`.
- Created Payment service projects.
- Added payment aggregate, transaction audit model, and payment lifecycle status.
- Added idempotent `AuthorizePayment` and `RefundPayment` consumers.
- Added `PaymentAuthorized` and `PaymentFailed` publishing through MassTransit with EF Core transactional outbox.
- Applied Payment migration to Neon `payment_db`.
- Created Shipping service projects.
- Added shipment aggregate and shipment lifecycle status.
- Added idempotent `CreateShipment` consumer.
- Added `ShipmentCreated` and `ShipmentFailed` publishing through MassTransit with EF Core transactional outbox.
- Applied Shipping migration to Neon `shipping_db`.
- Created Notification service projects.
- Added PostgreSQL notification history persistence.
- Added SignalR notifications hub at `/hubs/notifications`.
- Added order, payment, and shipment event consumers for realtime notification delivery.
- Applied Notification migration to Neon `notification_db`.
- Created Ordering Saga Worker projects.
- Added durable order workflow state in `ordering_saga_db`.
- Added workflow orchestration from order submission through inventory, payment, shipping, confirmation, and cancellation compensation.
- Added shipping address fields to submitted orders and order workflow messages.
- Applied Ordering address migration to Neon `ordering_db`.
- Applied Ordering Saga migration to Neon `ordering_saga_db`.
- Created Ocelot API Gateway project.
- Added gateway routes for Catalog, Basket, Ordering, Inventory, service health checks, and Notification SignalR hub.
- Added gateway observability and OIDC-ready security hooks.
- Verified gateway startup and local health endpoint at `http://localhost:5080/health/live`.
- Created Identity service projects.
- Added user and role persistence backed by PostgreSQL.
- Added user registration and profile lookup endpoints under `/api/v1/users`.
- Added password hashing through framework Identity password hasher.
- Applied Identity migration to Neon `identity_db`.
- Added Identity routes to Ocelot API Gateway.
- Added Inventory stock item lookup and upsert endpoints under `/api/v1/inventory/items`.
- Added Inventory routes to Ocelot API Gateway.
- Added a shared Dockerfile for service container builds.
- Added Docker Compose local orchestration for API services, saga worker, gateway, and Redis.
- Added Docker-specific Ocelot routing using service DNS names.
- Added local validation and gateway smoke test scripts.
- Added repeatable EF Core migration runner script.
- Added first contract, Inventory, and Ordering unit tests.

## Initial Infrastructure Direction

- Catalog: PostgreSQL on Neon for product, translation, category, brand, and image metadata. Cloudinary or equivalent external image provider for product images.
- Basket: Redis for active temporary basket state and PostgreSQL on Neon for basket history and checkout snapshots.
- Ordering: PostgreSQL on Neon for order consistency, history, and relational querying.
- Ordering Saga Worker: PostgreSQL on Neon for durable saga state.
- Inventory: PostgreSQL on Neon for transactional stock reservations.
- Payment: PostgreSQL on Neon for durable audit records and idempotent payment attempts.
- Shipping: PostgreSQL on Neon for shipment records.
- Notification: PostgreSQL initially for simple notification history; MongoDB can be considered later if notification payloads become document-heavy.
- Identity: PostgreSQL initially while keeping the service replaceable by a managed identity provider later.
- API Gateway: no primary database. Redis can be added later for rate limiting or distributed gateway features.

## Local Configuration

Copy `.env.example` to `.env` for local Docker-based development and fill in managed Neon, CloudAMQP, Cloudinary, and authentication settings. Real secrets must stay out of source control.

CloudAMQP can be configured with `RabbitMq__ConnectionString` as a single `amqps://` URI, or with the split `RabbitMq__Host`, `RabbitMq__Port`, `RabbitMq__Username`, `RabbitMq__Password`, `RabbitMq__VirtualHost`, and `RabbitMq__UseSsl` values.

Auth0 access tokens use `Auth__RoleClaimType` (default `https://ecommerce.local/claims/roles`) for API roles. Catalog and Inventory mutations plus arbitrary Identity user lookup require `Admin`. Set `RuntimeChecks__AccessToken` to an admin token before running a full smoke or workflow probe; health-only smoke checks do not require it.

Initial PostgreSQL databases:

- `catalog_db`
- `basket_db`
- `ordering_db`
- `ordering_saga_db`
- `inventory_db`
- `payment_db`
- `shipping_db`
- `notification_db`
- `identity_db`

## Validation

```powershell
dotnet --version
dotnet sln ECommerce.sln list
dotnet build ECommerce.sln
dotnet test ECommerce.sln
.\scripts\validate-local.ps1
.\scripts\check-runtime-env.ps1
```

## Continuous Integration

`.github/workflows/ci.yml` runs on every push and pull request without application secrets. Independent jobs restore/build/test the .NET solution, install/lint/build the Next.js frontend, and run PowerShell syntax, Docker Compose, and repository secret-pattern validation. After those quality gates pass, one BuildKit-backed Compose job builds all ten .NET application images with shared layer cache. Container publishing, migrations, and deployment remain separate future release concerns.

## Database Migrations

```powershell
.\scripts\run-migrations.ps1
.\scripts\run-migrations.ps1 -Service catalog
```

## Local Orchestration

```powershell
Copy-Item .env.example .env
docker compose config
docker compose up --build
.\scripts\start-local.ps1
```

The compose setup uses managed Neon PostgreSQL and CloudAMQP values from `.env`, starts Redis locally for Basket, and sends .NET traces, metrics, and structured logs through OpenTelemetry Collector. Open `http://localhost:16686` for Jaeger, `http://localhost:9090` for Prometheus, `http://localhost:3100` for the Loki API, and `http://localhost:3001` for the provisioned Grafana dashboards.

Docker services use `Observability__OtlpEndpoint=http://otel-collector:4317`. For .NET services started directly on the host, use `http://localhost:4317` while the Collector container is running.

Grafana uses anonymous Viewer access in local development and provisions Prometheus, Loki, Jaeger, the `ECommerce HTTP Overview` dashboard, and the `ECommerce Logs` dashboard automatically. Loki authentication is also disabled locally. Do not expose this unauthenticated setup as a production deployment or send secrets/personal data in log fields.

For managed telemetry without running the local observability containers, store `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf`, and the secret `OTEL_EXPORTER_OTLP_HEADERS` in Infisical, then start a service through the wrapper:

```powershell
.\scripts\run-with-secrets.ps1 dotnet run --project src/Services/Catalog/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj
```

Standard OTEL variables select direct Grafana Cloud export. Without them, direct host runs have no telemetry exporter; Docker Compose retains its local Collector default. Never put the authorization header in `.env`, source control, screenshots, or command output.

Trace and structured-log attributes pass through the shared sensitive-data redaction processors by default. Set `Observability__RedactionEnabled=false` only for controlled diagnostics; secret values must never be written directly into log message bodies. The beginner-oriented Turkish reference is available at `output/pdf/grafana-opentelemetry-sifirdan-rehber.pdf`.

After the gateway and services are running:

```powershell
.\scripts\smoke-test.ps1
.\scripts\smoke-test.ps1 -SkipWorkflowProbe
.\scripts\workflow-check.ps1
.\scripts\workflow-check.ps1 -Scenario inventory-failure
.\scripts\workflow-check.ps1 -Scenario payment-failure
.\scripts\workflow-check.ps1 -Scenario shipping-failure
.\scripts\wait-for-runtime.ps1 -TimeoutSeconds 240
```

The secretless `.github/workflows/ci.yml` runs on pushes and pull requests. The separate `Runtime integration` workflow is manual and uses a protected GitHub environment plus Infisical OIDC to run migrations and saga workflow probes without storing a long-lived Infisical credential in GitHub.

The workflow check defaults to `all`, which runs success, inventory-failure, payment-failure, and shipping-failure scenarios. Shipping failure uses the valid postal code `00000`, rejected by the configurable mock shipping provider, and verifies payment refund plus inventory release.

## Next Phase

Next phase is synchronizing Identity roles with Auth0 authorization roles, adding customer-resource ownership enforcement, and expanding automated integration tests around the order workflow.
