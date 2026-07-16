---
id: ai-architecture
type: architecture
version: 1
status: active
tags:
- architecture
- microservices
- event-driven
related:
- ai-services
- ai-events
- ai-databases
- decision-database-per-service
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Describe the actual architecture of this .NET 10 e-commerce microservices repository.

# Architecture Style

The system uses a Next.js frontend, independent .NET services with database-per-service persistence, Ocelot gateway routing, MassTransit/RabbitMQ messaging, EF Core persistence, Redis-backed active basket state, and saga orchestration for order workflow.

# Core Components

- [[02_SERVICES#ApiGateway]] routes external HTTP traffic.
- `src/Frontend` contains the Next.js web client. Login and registration use Auth0 Authorization Code with PKCE; the local Identity profile and `Customer` or `Seller` role are synchronized after authentication. An authenticated `Admin` receives the initial administrator overview, which reads its live product summary from Catalog. The client supports TR/EN localization and dark/light themes. See [[../../src/Frontend/AI#Responsibilities]].
- [[02_SERVICES#Ordering]] creates orders and publishes `OrderSubmitted`.
- [[02_SERVICES#OrderingSaga]] coordinates inventory, payment, shipping, confirmation, and cancellation.
- [[02_SERVICES#Inventory]] reserves and releases stock.
- [[02_SERVICES#Payment]] authorizes and refunds payments.
- [[02_SERVICES#Shipping]] creates shipments.
- [[02_SERVICES#Notification]] records notifications and exposes SignalR.
- [[02_SERVICES#Identity]] registers and reads users.
- [[02_SERVICES#Catalog]] owns product catalog.
- [[02_SERVICES#Basket]] owns active baskets and checkout snapshots.

# Communication

- HTTP through [[02_SERVICES#ApiGateway]] for client-facing APIs.
- Browser clients are admitted by the API Gateway's configured `Cors:AllowedOrigins`; development allows `http://localhost:3000`.
- RabbitMQ through MassTransit for commands and events. See [[04_EVENTS#Message Contracts]].
- PostgreSQL through EF Core for durable service-owned data. See [[03_DATABASES#Database Catalog]].
- Redis through StackExchange.Redis for active basket state. See [[03_DATABASES#Redis]].
- OTLP/gRPC traces, metrics, and structured `ILogger` records from .NET runtime units to the OpenTelemetry Collector; the local Docker collector batches traces to Jaeger, exposes Prometheus-format metrics for Prometheus/Grafana, and forwards logs over OTLP/HTTP to Loki.
- When standard `OTEL_EXPORTER_OTLP_*` variables are injected, .NET runtime units bypass the local Collector and export traces, metrics, and logs directly to the managed Grafana Cloud OTLP/HTTP endpoint.
- Trace tags and structured-log attributes pass through shared sensitive-key redaction processors before either local or managed export. Redaction is enabled by default through `Observability__RedactionEnabled`.

# Shared Libraries

See [[12_DEPENDENCIES#BuildingBlocks]].

# Architectural Boundaries

Services do not share business tables. Cross-service workflow is expressed through contracts in `ECommerce.BuildingBlocks.Contracts`. Shared code is limited to contracts, infrastructure helpers, security, localization, persistence, observability, and event bus setup.

# Known Constraints

- Endpoint authorization is policy-based and intentionally opt-in so health and public query endpoints remain anonymous. Privileged Catalog, Inventory, and Identity operations require the Auth0 `Admin` role claim.
- Identity database roles are not yet synchronized automatically into Auth0 access-token role claims.
- Basket, Ordering, and Notification customer-resource ownership enforcement is still TODO.
- Payment provider behavior is mock/in-process.
- Shipping uses a provider abstraction backed by a configurable in-process mock provider.
- Local Jaeger storage is in-memory and intended for development diagnostics, not durable production retention.
- Local Loki, Prometheus, and Grafana use Docker volumes, but the stack has no production retention, backup, authentication, or high-availability design.
- Direct SDK-to-cloud export is the initial managed quickstart path; basic SDK attribute redaction is active, while production-scale retry, body/content redaction, sampling, and routing through a managed Collector/Alloy deployment remain TODOs.
- Runtime workflow verification exists, but a full integration-test suite is still TODO.
