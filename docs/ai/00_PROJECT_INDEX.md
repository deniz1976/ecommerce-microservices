---
id: ai-project-index
type: project-index
version: 1
status: active
tags:
- repository
- dotnet
- microservices
related:
- ai-architecture
- ai-services
- ai-databases
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Index the repository for AI agents and future graph generation.

# Repository Map

- `ECommerce.sln`: .NET 10 solution.
- `src/ApiGateways/ECommerce.ApiGateway`: Ocelot API gateway.
- `src/BuildingBlocks`: shared contracts and infrastructure helpers.
- `src/Services`: business services and the ordering saga worker.
- `src/Frontend`: Next.js frontend for authentication, registration, role onboarding, and the administrator overview. See [[../../src/Frontend/AI#Purpose]].
- `tests`: unit and contract tests.
- `tools/ECommerce.RuntimeChecks`: runtime workflow verification tool.
- `scripts`: PowerShell runtime, migration, smoke, validation, and secret-wrapper scripts.
- `.github/workflows/ci.yml`: secretless backend, frontend, Compose, script, and repository validation pipeline.
- `docs`: human documentation.
- `docs/ai`: graph-ready AI memory documentation.

# Solution Projects

See [[12_DEPENDENCIES#Project Catalog]].

# Runtime Entry Points

- [[02_SERVICES#ApiGateway]]
- [[02_SERVICES#Catalog]]
- [[02_SERVICES#Basket]]
- [[02_SERVICES#Ordering]]
- [[02_SERVICES#Inventory]]
- [[02_SERVICES#Payment]]
- [[02_SERVICES#Shipping]]
- [[02_SERVICES#Notification]]
- [[02_SERVICES#Identity]]
- [[02_SERVICES#OrderingSaga]]
- [[06_DEPLOYMENT#Runtime Checks]]

# External Systems

- Neon PostgreSQL: see [[03_DATABASES#Database Catalog]].
- CloudAMQP RabbitMQ: see [[04_EVENTS#Broker]].
- Redis Cloud: see [[03_DATABASES#Redis]].
- Cloudinary: see [[07_SECURITY#Secrets]] and [[02_SERVICES#Catalog]].
- Auth0: see [[07_SECURITY#Authentication]].
- Infisical: see [[07_SECURITY#Secrets]].
- OpenTelemetry Collector, Jaeger, Prometheus, Loki, and Grafana: local telemetry pipeline; see [[06_DEPLOYMENT#Observability]].
- Grafana Cloud: managed OTLP backend selected for secret-injected development and future deployment; see [[06_DEPLOYMENT#Managed Grafana Cloud]].

# Current Validation Commands

- `dotnet build ECommerce.sln`
- `dotnet test ECommerce.sln`
- `cd src\Frontend && npm run lint`
- `cd src\Frontend && npm run build`
- `.\scripts\validate-local.ps1 -SkipBuild`
- `.\scripts\smoke-test.ps1`
- `.\scripts\run-with-secrets.ps1 .\scripts\workflow-check.ps1`
