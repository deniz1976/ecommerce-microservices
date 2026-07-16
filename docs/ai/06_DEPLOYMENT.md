---
id: ai-deployment
type: deployment
version: 1
status: active
tags:
- docker
- runtime
- operations
related:
- ai-services
- ai-security
- ai-dependencies
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Describe runtime and deployment configuration actually present in this repository.

# Runtime Units

Docker Compose defines:

- `api-gateway`
- `catalog-api`
- `basket-api`
- `ordering-api`
- `inventory-api`
- `payment-api`
- `shipping-api`
- `notification-api`
- `identity-api`
- `ordering-saga-worker`
- `redis`
- `otel-collector`
- `jaeger`
- `prometheus`
- `loki`
- `grafana`

# Docker

- file: `docker-compose.yml`
- build file: `Dockerfile`
- ignore file: `.dockerignore`
- default container port: `8080`
- host ports: see [[05_APIS#API Catalog]]

# Observability

- collector config: `deploy/observability/otel-collector.yaml`
- Prometheus config: `deploy/observability/prometheus.yml`
- Loki config: `deploy/observability/loki.yaml`
- Grafana provisioning: `deploy/observability/grafana/provisioning`
- Grafana dashboard: `deploy/observability/grafana/dashboards/ecommerce-http.json`
- collector image: `otel/opentelemetry-collector-contrib:0.156.0`
- Jaeger image: `jaegertracing/jaeger:2.19.0`
- Prometheus image: `prom/prometheus:v3.13.0`
- Loki image: `grafana/loki:3.7.3`
- Grafana image: `grafana/grafana:13.1.0`
- Docker service OTLP endpoint: `http://otel-collector:4317`
- host-process OTLP endpoint: `http://localhost:4317`
- OTLP/gRPC host port: `4317`
- OTLP/HTTP host port: `4318`
- Collector health extension: `http://localhost:13133`
- Collector Prometheus exporter: `http://localhost:8889/metrics`
- Jaeger UI: `http://localhost:16686`
- Prometheus UI: `http://localhost:9090`
- Loki HTTP API: `http://localhost:3100`
- Grafana UI: `http://localhost:3001`
- trace flow: instrumented .NET runtime -> OTLP receiver -> batch processor -> Jaeger OTLP receiver.
- metrics flow: ASP.NET Core, HTTP client, .NET runtime, and service meters -> OTLP receiver -> batch processor -> Collector Prometheus exporter -> Prometheus -> Grafana.
- logs flow: structured .NET `ILogger` records with trace/span context -> OTLP receiver -> batch processor -> Loki native OTLP/HTTP endpoint -> Grafana.
- trace storage: Jaeger uses its built-in in-memory development storage; traces are lost when the container restarts.
- metrics/log/dashboard storage: named volumes `prometheus-data`, `loki-data`, and `grafana-data` survive container restart/removal until the volumes are deleted.
- Grafana provisioning: Prometheus is the default data source; Loki and Jaeger are also provisioned, trace-to-log navigation uses service resource labels plus trace id, and `ECommerce HTTP Overview` plus `ECommerce Logs` are loaded automatically.

`Observability__OtlpEndpoint` defaults to the Collector service address in Docker Compose. When running .NET services directly on the host, set it to `http://localhost:4317`. `Observability__RedactionEnabled` defaults to `true` and applies the shared trace-tag and structured-log-attribute processors before export. Grafana anonymous Viewer access and Loki's disabled authentication are enabled only for this local development stack. Production backends, authentication, TLS, message-body redaction, and retention remain environment-specific TODOs.

## Managed Grafana Cloud

The shared observability registration supports the standard OpenTelemetry environment variables:

- `OTEL_EXPORTER_OTLP_ENDPOINT`: Grafana Cloud base OTLP endpoint.
- `OTEL_EXPORTER_OTLP_PROTOCOL`: `http/protobuf` for the selected direct-cloud setup.
- `OTEL_EXPORTER_OTLP_HEADERS`: secret Grafana Cloud authorization header; store only in Infisical or the deployment secret manager.

Configuration precedence is:

1. A non-empty `OTEL_EXPORTER_OTLP_ENDPOINT` enables standard OTLP configuration and lets the .NET exporter read the standard protocol and header variables.
2. Otherwise, a non-empty `Observability__OtlpEndpoint` enables the repository-specific local Collector configuration.
3. When both endpoints are empty, providers remain registered but no OTLP exporter is enabled; services run normally without a telemetry backend.

Infisical `dev` at `/` contains the three standard OTLP keys. Run a host service through `scripts/run-with-secrets.ps1` to inject them. Docker Compose also forwards the standard keys when they exist; standard configuration takes precedence over its local Collector default.

Live ingestion was verified from an Infisical-injected `ECommerce.ApiGateway` process: six `/health/live` traces were visible in the Grafana Cloud Tempo data source under service name `ECommerce.ApiGateway`.

# Secrets Injection

Preferred development flow uses Infisical:

- script: `scripts/run-with-secrets.ps1`
- command form: `.\scripts\run-with-secrets.ps1 <command>`

See [[07_SECURITY#Secrets]].

# Scripts

- `scripts/check-runtime-env.ps1`: validates required env vars.
- `scripts/run-migrations.ps1`: applies EF Core migrations and stops immediately when any service migration fails; callers on clean machines must restore `ECommerce.sln` dependencies first.
- `scripts/smoke-test.ps1`: gateway health plus basic user/inventory/order probe; the full probe reads `RuntimeChecks__AccessToken`, while `-SkipWorkflowProbe` needs no token.
- `scripts/wait-for-runtime.ps1`: retries the health-only smoke test until the gateway and all downstream APIs are reachable or a bounded timeout expires.
- `scripts/request-runtime-access-token.ps1`: exchanges the Infisical-injected Auth0 M2M client ID/secret for a short-lived API token with requested scope `inventory:write`; masks and persists the token through `GITHUB_ENV` without printing it.
- `scripts/workflow-check.ps1`: end-to-end order workflow verification; reads `RuntimeChecks__AccessToken`, accepts `-Scenario`, and reports success or failure through the process exit code.
- `scripts/start-local.ps1`: Docker local startup helper.
- `scripts/validate-local.ps1`: repo validation.
- `scripts/load-env.ps1`: private local `.env` loader retained for local-only experiments.

# Runtime Checks

- project: `tools/ECommerce.RuntimeChecks`
- purpose: create orders and verify success plus deterministic compensation state across Ordering, Saga, Inventory, Payment, Shipping, and Notification.
- authorization: Inventory seeding accepts either an Auth0 user token with the `Admin` role or an M2M token with exact permission `inventory:write`. Trusted CI stores `RuntimeChecks__Auth0ClientId` and secret `RuntimeChecks__Auth0ClientSecret` in Infisical and generates `RuntimeChecks__AccessToken` at job runtime; local manual probes may still inject a valid access token directly.
- CLI scenarios: `all` (default), `success`, `inventory-failure`, `payment-failure`, `shipping-failure`.
- `success`: expects confirmed order, completed saga, authorized payment, created shipment, and notification persistence.
- `inventory-failure`: orders a product without an inventory row and expects failed reservation plus cancelled saga/order.
- `payment-failure`: orders a zero-price item, expects failed payment, cancelled saga/order, released inventory reservation, and both order/payment notifications.
- `shipping-failure`: uses the mock-provider rejected postal code `00000` and expects failed shipment, refunded payment, released inventory reservation, cancelled saga/order, and order/payment/shipment notifications.
- configuration: `ShippingProvider__RejectedPostalCodes__0` defaults to `00000`; keep the runtime scenario input synchronized when overriding this list.

# CI/CD

GitHub Actions workflow: `.github/workflows/ci.yml`.

- triggers: every push, every pull request, and manual `workflow_dispatch`.
- permissions: read-only repository contents.
- concurrency: a newer run for the same workflow/ref cancels the older run.
- backend job: reads `global.json`, restores the solution, builds Release with no second restore, and runs all .NET tests.
- frontend job: uses Node.js 24 plus the committed npm lockfile, then runs `npm ci`, lint, and production build.
- repository-validation job: parses every PowerShell script, validates Docker Compose, and scans tracked repository content for secret-like patterns through `scripts/validate-local.ps1 -SkipBuild`.
- container-build job: runs only after the three quality jobs succeed and uses `docker compose build` with BuildKit to build all ten .NET application images in one runner, allowing their common solution restore layer to be reused.
- secret policy: the CI workflow needs no application, database, broker, Auth0, Infisical, or Grafana credential.

Trusted runtime workflow: `.github/workflows/runtime-integration.yml`.

- trigger: manual `workflow_dispatch` only; inputs select the Infisical environment, scenario, and scenario timeout.
- protection boundary: the job targets the `runtime-integration` GitHub environment and never runs for pull requests.
- secret injection: `Infisical/secrets-action` exchanges GitHub's short-lived OIDC token for environment-scoped secrets; no long-lived Infisical credential is stored in GitHub.
- configuration: GitHub environment variables `INFISICAL_IDENTITY_ID` and `INFISICAL_PROJECT_SLUG` identify the Infisical machine identity and project; both are non-secret identifiers.
- execution: validate required variables and the non-sensitive GitHub OIDC issuer/audience/subject claims without logging the JWT, obtain a short-lived Auth0 `inventory:write` M2M token, restore solution dependencies and the local EF tool, apply all migrations with fail-fast exit-code handling, build/start the application Compose graph, wait for health, and execute the selected saga scenario.
- cleanup: print bounded container diagnostics only on failure and always remove containers and local volumes.
- Infisical OIDC trust is restricted to GitHub's immutable owner/repository identity plus the exact environment. For this repository the subject is `repo:deniz1976@96434352/ecommerce-microservices@1302913896:environment:runtime-integration`; the numeric IDs remain stable if either display name changes.

# TODO

- Add protected container publish pipeline.
- Add container image tagging strategy.
- Add production deployment manifests.
- Validate Grafana Cloud from the selected hosting platform.
