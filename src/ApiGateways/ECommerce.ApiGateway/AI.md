---
id: service-api-gateway
type: service
version: 1
status: active
tags:
- gateway
- ocelot
- backend
related:
- ai-apis
- ai-deployment
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Expose a single HTTP entry point for the microservices through Ocelot.

# Responsibilities

- Route gateway paths to service HTTP endpoints.
- Expose gateway health endpoints.
- Load environment-specific Ocelot configuration.
- Enforce global Bearer authentication with an explicit public-route allow-list.

# Dependencies

- Ocelot
- [[07_SECURITY#Authentication]]
- [[06_DEPLOYMENT#Secrets Injection]]

# Database

None.

# APIs

See [[../../../docs/ai/05_APIS#Gateway]].

# Events Published

None.

# Events Consumed

None.

# Important Classes

- `Program.cs`
- `GatewayConfigurationExtensions`
- `GatewayServiceCollectionExtensions`
- `GatewayHealthCheckExtensions`
- `ocelot.json`
- `ocelot.Docker.json`

# Folder Structure

- root: host and Ocelot configuration
- `Configuration`: route configuration and service registration extensions
- `Middleware`: gateway-specific pipeline extensions
- `Properties`: launch profiles

# Configuration

- `Auth__Authority`
- `Auth__Audience`
- `Observability__ServiceNamespace`
- `Observability__OtlpEndpoint`
- `Cors__AllowedOrigins__0` and subsequent indexed values
- Ocelot route files

# Design Decisions

See [[../../../docs/ai/09_DECISIONS#decision-authorization-default-deny]] and [[../../../docs/ai/09_DECISIONS#decision-local-trace-pipeline]].

The gateway runs shared authentication without ASP.NET fallback authorization because Ocelot selects and authorizes its routes inside the Ocelot pipeline. Applying the service fallback before `UseOcelot` would treat the not-yet-selected endpoint as protected and return `401` even for gateway health and anonymous allow-list routes.

# Future Improvements

- Add rate and abuse controls per route.
- Add rate limiting.
- Add request correlation headers.
