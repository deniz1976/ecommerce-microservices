---
id: ai-decisions
type: decision-log
version: 1
status: active
tags:
- adr
- decisions
- architecture
related:
- ai-architecture
- ai-security
- ai-deployment
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Track architectural decisions as stable graph nodes.

# Decision Records

## decision-database-per-service

- id: `decision-database-per-service`
- status: active
- decision: each service owns its own PostgreSQL database.
- reason: service autonomy and clear data ownership.
- consequences: cross-service queries require APIs, events, or future read models.
- related: [[03_DATABASES#Database Catalog]]

## decision-event-driven-order-workflow

- id: `decision-event-driven-order-workflow`
- status: active
- decision: order workflow is coordinated through RabbitMQ/MassTransit and saga state.
- reason: inventory, payment, shipping, and notification are separate services.
- related: [[04_EVENTS#Order Workflow]], [[02_SERVICES#OrderingSaga]]

## decision-transactional-outbox

- id: `decision-transactional-outbox`
- status: active
- decision: messaging services use the MassTransit EF bus outbox for scoped producers and the EF consumer outbox on every generated receive endpoint.
- reason: reliable message publishing, duplicate detection, and atomic consumer database/message processing. Enabling only the bus outbox leaves consumer database updates outside the inbox/outbox transaction and can surface concurrency faults during compensation.
- tuning: delivery `QueryDelay` is 5 seconds, duplicate detection is 10 minutes, and receive endpoints retry after 100 ms, 500 ms, and 1 second before using the `_error` queue.
- related: [[03_DATABASES#OrderingDb]], [[04_EVENTS#Broker]]

## decision-service-owned-consumer-queues

- id: `decision-service-owned-consumer-queues`
- status: active
- decision: every MassTransit receive endpoint uses a kebab-case queue name prefixed by its owning service.
- reason: consumer class names such as `OrderSubmittedConsumer`, `PaymentAuthorizedConsumer`, and `ShipmentCreatedConsumer` exist in both OrderingSaga and Notification; unprefixed names would attach them to one competing-consumer queue instead of separate event subscriptions.
- consequence: published events fan out to service-owned queues such as `ordering-saga-order-submitted` and `notification-order-submitted`; command queues are also service-prefixed for consistent ownership.
- related: [[04_EVENTS#Broker]], [[02_SERVICES#OrderingSaga]], [[02_SERVICES#Notification]]

## decision-infisical-secrets

- id: `decision-infisical-secrets`
- status: active
- decision: shared development secrets come from Infisical.
- reason: avoid tracked `.env` secrets.
- related: [[07_SECURITY#Secrets]]

## decision-masstransit-v8

- id: `decision-masstransit-v8`
- status: active
- decision: use MassTransit 8.5.1.
- reason: avoid MassTransit v9 runtime license requirement in this repository.
- related: [[12_DEPENDENCIES#Package Catalog]]

## decision-authorization-opt-in

- id: `decision-authorization-opt-in`
- status: superseded
- decision: JWT authentication uses shared named policies, while authorization remains endpoint-level opt-in.
- reason: protect privileged mutations and user-directory reads without blocking health checks and explicitly public queries.
- consequence: Catalog mutations and arbitrary Identity user lookup require the namespaced Auth0 `Admin` role claim; Inventory upsert accepts `Admin` or the narrow `inventory:write` permission used by trusted runtime verification.
- related: [[07_SECURITY#Authorization]]

## decision-authorization-default-deny

- id: `decision-authorization-default-deny`
- status: active
- decision: service APIs require an authenticated user through a shared fallback policy unless an endpoint explicitly uses `AllowAnonymous`; Ocelot independently applies global Bearer authentication with the same public allow-list.
- reason: endpoint-level opt-in allowed new business routes to be exposed when authorization metadata was forgotten.
- consequence: health checks, Catalog reads, Inventory reads, and Identity registration are explicitly public; Basket, Ordering, and Notification SignalR require authentication and separately enforce customer ownership. Service hosts register authentication and ASP.NET fallback authorization, while the gateway registers authentication only and defers route authorization to Ocelot so minimal hosting cannot apply the service fallback before route selection. Contract tests verify the registration boundary, fallback policy, endpoint metadata, and both gateway configurations.
- related: [[07_SECURITY#Authorization]], [[05_APIS#API Catalog]]

## decision-auth0-centered-login

- id: `decision-auth0-centered-login`
- status: active
- decision: login is handled by Auth0 instead of issuing local passwords/JWTs from Identity.
- reason: reduce custom auth security burden and align with production identity-provider practice.
- consequence: Identity stores local profile records and maps Auth0 token `sub` values through `/api/v1/auth/me`.
- related: [[07_SECURITY#Authentication]], [[02_SERVICES#Identity]]

## decision-role-aware-frontend-entry

- id: `decision-role-aware-frontend-entry`
- status: active
- decision: the frontend selects the first authenticated workspace from the local Identity role; `Admin` opens the administrator overview.
- reason: role-aware navigation can start without adding a duplicate frontend identity store or an unowned cross-service database.
- consequence: the current administrator overview uses the existing public Catalog query and does not claim seller-specific ownership data.
- related: [[01_ARCHITECTURE#Core Components]], [[05_APIS#Frontend Client]], [[../../src/Frontend/AI#Design Decisions]]

## decision-shipping-provider-boundary

- id: `decision-shipping-provider-boundary`
- status: active
- decision: Shipping application logic depends on `IShippingProvider`; Infrastructure supplies the current configurable mock provider.
- reason: isolate carrier behavior from shipment persistence and exercise provider rejection plus saga compensation without invalid API payloads or production-only test hooks.
- consequence: postal codes in `ShippingProvider__RejectedPostalCodes` create durable failed shipments and publish the normal `ShipmentFailed` contract.
- related: [[02_SERVICES#Shipping]], [[04_EVENTS#Order Workflow]], [[06_DEPLOYMENT#Runtime Checks]]

## decision-local-trace-pipeline

- id: `decision-local-trace-pipeline`
- status: active
- decision: Docker development sends OTLP/gRPC traces from all instrumented .NET runtime units to a shared OpenTelemetry Collector, which batches and exports them to Jaeger.
- reason: keep application instrumentation backend-neutral while providing a searchable local distributed-trace UI.
- consequence: Docker uses `Observability__OtlpEndpoint=http://otel-collector:4317`; Jaeger UI is exposed on host port `16686` and stores traces only in memory.
- related: [[01_ARCHITECTURE#Communication]], [[06_DEPLOYMENT#Observability]], [[12_DEPENDENCIES#Runtime Images]]

## decision-local-metrics-pipeline

- id: `decision-local-metrics-pipeline`
- status: active
- decision: instrumented .NET services export ASP.NET Core, HTTP client, runtime, and service metrics over OTLP to the shared Collector; Prometheus scrapes the Collector exporter and Grafana reads Prometheus through provisioned configuration.
- reason: preserve backend-neutral application instrumentation while providing queryable local operational metrics and a versioned starter dashboard.
- consequence: Prometheus and Grafana use named local Docker volumes; Grafana anonymous Viewer access and the current retention model are development-only.
- related: [[01_ARCHITECTURE#Communication]], [[06_DEPLOYMENT#Observability]], [[07_SECURITY#Local Observability Access]]

## decision-local-logs-pipeline

- id: `decision-local-logs-pipeline`
- status: active
- decision: shared .NET observability captures structured `ILogger` records through OpenTelemetry, preserves trace/span context, and sends them through the Collector to Loki's native OTLP/HTTP endpoint; Grafana provisions Loki and Jaeger correlation plus a starter logs dashboard.
- reason: centralize application logs without coupling services to a logging backend or adding a second application logging API.
- consequence: local Loki uses unauthenticated single-binary filesystem storage in the `loki-data` volume; production requires durable storage, retention, authentication, TLS, and sensitive-field redaction policies.
- related: [[01_ARCHITECTURE#Communication]], [[06_DEPLOYMENT#Observability]], [[07_SECURITY#Local Observability Access]]

## decision-grafana-cloud-direct-otlp

- id: `decision-grafana-cloud-direct-otlp`
- status: active
- decision: use Grafana Cloud Free as the initial managed observability backend and send all three signals directly from the existing .NET OpenTelemetry SDKs through standard OTLP environment variables; retain the local Docker observability stack as optional development infrastructure.
- reason: avoid requiring Loki, Prometheus, Jaeger, Grafana, or a Collector on the developer machine while keeping backend-neutral instrumentation and a zero-cost managed starting point.
- consequence: Infisical owns the authorization header; standard OTLP configuration overrides the local Collector endpoint when both are injected. Direct SDK export has weaker centralized retry, redaction, enrichment, and sampling controls than a production Collector/Alloy pipeline.
- related: [[01_ARCHITECTURE#Communication]], [[06_DEPLOYMENT#Managed Grafana Cloud]], [[07_SECURITY#Secrets]]

## decision-sdk-telemetry-redaction

- id: `decision-sdk-telemetry-redaction`
- status: active
- decision: run trace tags and structured-log attributes through shared sensitive-key redaction processors before local or managed OTLP export; enable the processors by default.
- reason: direct SDK-to-cloud export bypasses a central Collector, so common credential, personal-data, URL-query, and database-statement fields need a defense-in-depth boundary inside every .NET runtime unit.
- consequence: matching values become `[REDACTED]`; arbitrary free-text log bodies and nested content remain the application's responsibility, and production Collector/Alloy policies are still required for centralized allow-listing and content transformation.
- related: [[01_ARCHITECTURE#Communication]], [[06_DEPLOYMENT#Observability]], [[07_SECURITY#Telemetry Data Handling]]

## decision-secretless-pull-request-ci

- id: `decision-secretless-pull-request-ci`
- status: active
- decision: run backend build/tests, frontend lint/build, PowerShell/Compose validation, repository secret-pattern scanning, and a gated build of all application container images in GitHub Actions without injecting runtime application secrets.
- reason: pull requests need deterministic quality gates while untrusted or accidental code changes must not receive database, broker, identity-provider, Infisical, or Grafana credentials.
- consequence: the container job waits for code quality gates and builds all services in one Compose/BuildKit runner so the common restore layer can be reused; external-service workflow probes, migrations, image publishing, and deployment require separate trusted release jobs with environment protection and machine identity.
- related: [[06_DEPLOYMENT#CI/CD]], [[07_SECURITY#Secrets]]

## decision-oidc-runtime-integration

- id: `decision-oidc-runtime-integration`
- status: active
- decision: run managed end-to-end workflow probes only through a manually triggered, GitHub-environment-protected job that obtains Infisical secrets with GitHub OIDC.
- reason: database migrations and saga probes need trusted managed-service credentials, but pull requests and long-lived CI credentials must not receive them.
- consequence: the Infisical machine identity must trust only the exact repository/environment subject and have least-privilege access to Infisical `staging`; callers cannot select another secret environment. `staging` imports shared `dev` values but locally overrides all nine database connections to the Neon `runtime-integration` child branch, so managed test mutations are isolated from `production`. The workflow remains serialized and manual, and every database override is mandatory to prevent fallback to an imported development connection.
- related: [[06_DEPLOYMENT#CI/CD]], [[07_SECURITY#Secrets]], [[10_ROADMAP#Runtime Verification]]

## decision-runtime-m2m-permission

- id: `decision-runtime-m2m-permission`
- status: active
- decision: trusted runtime verification obtains a short-lived Auth0 Client Credentials token with only `inventory:write` and `customer:act`; the shared `Admin` policy remains role-only.
- reason: storing expiring administrator user tokens is unreliable, while granting an M2M client the full administrator role would exceed the workflow's needs to seed inventory and create orders for its isolated test customer.
- consequence: Inventory upsert uses `inventory:write`; customer ownership checks accept `customer:act`. Auth0 must define and grant both exact permissions only to the dedicated runtime M2M application, normal users must never receive `customer:act`, and the client secret remains only in Infisical.
- related: [[05_APIS#Inventory API]], [[06_DEPLOYMENT#Runtime Checks]], [[07_SECURITY#Authorization]]

## decision-customer-resource-ownership

- id: `decision-customer-resource-ownership`
- status: active
- decision: Basket, Ordering, and Notification forward the caller's original Bearer token to Identity `/api/v1/auth/me`, resolve the external Auth0 identity to the local user `Guid`, and enforce owner-or-explicit-delegate access at every customer resource boundary.
- reason: customer resources store a local Identity `Guid`, while Auth0 access tokens identify the caller by external `sub`; trusting a route or request-body `customerId` allows cross-customer access.
- consequence: the three services depend synchronously on the Identity API through `IdentityClient__BaseUrl` and fail closed when resolution fails. `Admin` and exact `customer:act` permission bypass lookup; another customer's order-by-id is hidden as not found. SignalR query tokens are forwarded only from `/hubs/notifications`. Contract tests cover mapping, failure, delegation, query-token restriction, and endpoint adoption.
- related: [[05_APIS#Basket API]], [[05_APIS#Ordering API]], [[05_APIS#Notification SignalR]], [[07_SECURITY#Authorization]]

## decision-auth0-role-synchronization

- id: `decision-auth0-role-synchronization`
- status: active
- decision: authenticated `Customer` and `Seller` onboarding uses a dedicated least-privilege Auth0 Management API M2M client to update Auth0 membership before committing the same role to Identity; the frontend then forces a non-cached token refresh.
- reason: downstream role policies evaluate the signed Auth0 access-token claim, while the local profile is the application's onboarding source of truth. Updating only PostgreSQL creates an authorization mismatch.
- consequence: Auth0 rejection, timeout, or malformed token response fails closed with `503` and no local role commit. The Management client is separate from runtime automation and its secret stays in Infisical. Auth0 success followed by local database failure remains a rare cross-system consistency case requiring later reconciliation/outbox processing.
- related: [[02_SERVICES#Identity]], [[05_APIS#Identity API]], [[07_SECURITY#Authorization]]

# TODO

- Add decision dates once ADR workflow is formalized.
