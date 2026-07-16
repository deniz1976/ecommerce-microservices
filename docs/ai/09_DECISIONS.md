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
- decision: messaging services use MassTransit EF outbox/inbox.
- reason: reliable message publishing and duplicate detection.
- tuning: `QueryDelay` is 5 seconds.
- related: [[03_DATABASES#OrderingDb]], [[04_EVENTS#Broker]]

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
- status: active
- decision: JWT authentication uses shared named policies, while authorization remains endpoint-level opt-in.
- reason: protect privileged mutations and user-directory reads without blocking health checks and explicitly public queries.
- consequence: Catalog and Inventory privileged mutations plus arbitrary Identity user lookup require the namespaced Auth0 `Admin` role claim; full runtime probes require an admin access token.
- related: [[07_SECURITY#Authorization]]

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
- consequence: the Infisical machine identity must trust only the exact repository/environment subject and have least-privilege access to the selected secret environment; the workflow mutates test data in the configured managed databases and therefore remains serialized and manual.
- related: [[06_DEPLOYMENT#CI/CD]], [[07_SECURITY#Secrets]], [[10_ROADMAP#Runtime Verification]]

# TODO

- Add decision dates once ADR workflow is formalized.
