# ECommerce Microservices

A production-oriented e-commerce platform built with .NET microservices and a modern Next.js storefront. The system supports product discovery, secure basket and checkout flows, distributed order processing, seller-owned catalog management, bilingual user experiences, and end-to-end observability.

All application timestamps are represented as UTC instants with `DateTimeOffset` and stored in PostgreSQL as `timestamp with time zone`. Time-zone conversion is handled only at the presentation boundary.

## Features

- Searchable and filterable product catalog with localized product content
- Product detail pages, categories, brands, pricing, stock, and image galleries
- Seller-owned product creation and editing with Cloudinary image management
- Redis-backed active baskets with server-authoritative catalog pricing
- Durable checkout snapshots and transactional outbox delivery
- Distributed order workflow across inventory, payment, shipping, and notifications
- Saga-based orchestration with idempotency and compensation flows
- Order history and lifecycle timeline
- Auth0 authentication with OAuth 2.0, OpenID Connect, JWT validation, roles, and ownership checks
- English and Turkish UI content and localized API error responses
- SignalR-based real-time notifications
- Health checks, metrics, traces, logs, dashboards, and RabbitMQ failure-queue monitoring
- Automated architecture, UTC, build, test, frontend, runtime, and observability validation

## Tech Stack

| Area | Technologies |
| --- | --- |
| Backend | C#, .NET 10, ASP.NET Core Minimal APIs |
| Frontend | Next.js 16, React 19, TypeScript 5.7, Tailwind CSS 4, shadcn |
| API Gateway | Ocelot |
| Persistence | Entity Framework Core 10, PostgreSQL, Npgsql |
| Cache | Redis |
| Messaging | RabbitMQ, MassTransit 8.5, transactional EF Core outbox |
| Workflow | MassTransit state-machine saga, idempotent consumers, compensating actions |
| Authentication | Auth0, OAuth 2.0, OpenID Connect, JWT bearer tokens |
| Media | Cloudinary |
| Real-time | ASP.NET Core SignalR |
| Observability | OpenTelemetry, Prometheus, Grafana, Loki, Jaeger |
| Secrets | Infisical |
| Infrastructure | Docker, Docker Compose |
| Testing and CI | xUnit, architecture checks, GitHub Actions |

## Architecture

The repository contains independently deployable services with database-per-service ownership:

- **Catalog** — products, translations, categories, brands, prices, and media
- **Basket** — active baskets, canonical pricing, checkout snapshots, and checkout handoff
- **Ordering** — orders, order items, lifecycle state, and customer order history
- **Inventory** — stock and reservation management
- **Payment** — payment authorization, refund processing, and transaction audit
- **Shipping** — shipment creation and delivery lifecycle
- **Notification** — persisted notifications and SignalR delivery
- **Identity** — application user profile and role persistence
- **Ordering Saga** — durable orchestration and failure compensation
- **API Gateway** — the public routing boundary for APIs and real-time connections
- **Frontend** — the customer and seller web experience

Services communicate asynchronously through RabbitMQ and MassTransit contracts. PostgreSQL transactional outboxes protect database-to-message consistency, while idempotent consumers make message redelivery safe. Redis is used for low-latency active basket state.

## Repository Structure

```text
src/
  BuildingBlocks/   Shared contracts and cross-cutting infrastructure
  Frontend/         Next.js storefront and seller experience
  Gateways/         Ocelot API Gateway
  Services/         Domain microservices and the saga worker
tests/              Unit, integration, architecture, and contract tests
deploy/             Observability and deployment configuration
scripts/            Local startup, validation, migration, and smoke-test tooling
```

## Local Development

### Prerequisites

- .NET SDK `10.0.301` or a compatible .NET 10 feature band
- Node.js and npm
- Docker Desktop with Docker Compose
- An Infisical project or equivalent environment configuration for required secrets

### Start the platform

```powershell
./scripts/start-local.ps1
```

The script validates the local environment, loads configured secrets, starts the infrastructure and services, waits for readiness, and runs runtime checks.

To start the stack directly with Docker Compose:

```powershell
docker compose up --build
```

### Run validation

```powershell
./scripts/validate-local.ps1
./scripts/smoke-test.ps1
```

Frontend-only validation:

```powershell
cd src/Frontend
npm install
npm run lint
npm run build
```

## Engineering Principles

- One C# type per source file
- SOLID boundaries and dependency inversion between domain, application, infrastructure, and API layers
- Database ownership per service
- UTC-only persistence for application instants
- Localized, stable error codes separated from user-facing messages
- Secure defaults with authenticated mutation endpoints and ownership enforcement
- At-least-once message delivery handled through idempotency
- Code, automated checks, and public documentation kept in sync
