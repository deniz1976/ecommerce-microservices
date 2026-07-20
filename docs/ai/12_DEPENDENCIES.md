---
id: ai-dependencies
type: dependency-catalog
version: 1
status: active
tags:
- dependencies
- packages
- projects
related:
- ai-project-index
- ai-decisions
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Catalog project and package dependencies visible in this repository.

# Project Catalog

## BuildingBlocks

- `ECommerce.BuildingBlocks.Contracts`
- `ECommerce.BuildingBlocks.EventBus`
- `ECommerce.BuildingBlocks.Localization`
- `ECommerce.BuildingBlocks.Observability`
- `ECommerce.BuildingBlocks.Persistence`: shared EF/Npgsql registration, managed PostgreSQL URI normalization, and runtime-built local design-time connection defaults.
- `ECommerce.BuildingBlocks.Security`

## Services

- Catalog: Api, Application, Domain, Infrastructure
- Basket: Api, Application, Domain, Infrastructure
- Ordering: Api, Application, Domain, Infrastructure
- Inventory: Api, Application, Domain, Infrastructure
- Payment: Api, Application, Domain, Infrastructure
- Shipping: Api, Application, Domain, Infrastructure
- Notification: Api, Application, Domain, Infrastructure
- Identity: Api, Application, Domain, Infrastructure
- OrderingSaga: Worker, Application, Domain, Infrastructure

## Tests and Tools

- `ECommerce.ContractTests`
- `ECommerce.Inventory.UnitTests`
- `ECommerce.Ordering.UnitTests`
- `ECommerce.Shipping.UnitTests`
- `ECommerce.RuntimeChecks`: executable runtime verifier; depends on `ECommerce.BuildingBlocks.Persistence` for the same Neon/Npgsql connection-string normalization used by service runtimes.

## Frontend

- `src/Frontend`: Next.js 16, React 19, TypeScript, Tailwind CSS 4, Auth0 SPA SDK, ESLint.
- package manager: npm with `package-lock.json`.

# Package Catalog

Central package management is enabled in `Directory.Packages.props`.

Key packages:

- MassTransit 8.5.1
- MassTransit.EntityFrameworkCore 8.5.1
- MassTransit.RabbitMQ 8.5.1
- Microsoft.EntityFrameworkCore 10.0.9
- Npgsql 10.0.3
- Npgsql.EntityFrameworkCore.PostgreSQL 10.0.2
- Ocelot 24.1.0
- StackExchange.Redis 3.0.11
- OpenTelemetry packages 1.16.0
- OpenTelemetry runtime instrumentation 1.16.0
- xUnit packages for tests

# Runtime Images

- `otel/opentelemetry-collector-contrib:0.156.0`
- `jaegertracing/jaeger:2.19.0`
- `prom/prometheus:v3.13.0`
- `grafana/loki:3.7.3`
- `grafana/grafana:13.1.0`
- Jaeger storage mode: in-memory local development.
- Loki, Prometheus, and Grafana storage: named local Docker volumes.

# Workflow Actions

- `Infisical/secrets-action@v1.0.16`: OIDC-authenticated runtime secret injection using the current GitHub Actions Node runtime.

# TODO

- Generate exact project-reference graph automatically.
