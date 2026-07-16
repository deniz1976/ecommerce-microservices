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
- Configure shared observability and auth-ready security.

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

See [[../../../docs/ai/09_DECISIONS#decision-authorization-opt-in]] and [[../../../docs/ai/09_DECISIONS#decision-local-trace-pipeline]].

# Future Improvements

- Add gateway-level authorization policies.
- Add rate limiting.
- Add request correlation headers.
