---
id: ai-security
type: security
version: 1
status: active
tags:
- security
- auth0
- infisical
related:
- ai-deployment
- ai-apis
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Describe authentication, authorization, secret handling, and security boundaries.

# Authentication

Auth0-compatible JWT validation is configured through `ECommerce.BuildingBlocks.Security`.

Config keys:

- `Auth__Authority`
- `Auth__Audience`
- `Auth__RequireHttpsMetadata`
- `Auth__RoleClaimType` (default: `https://ecommerce.local/claims/roles`)

The selected login model is Auth0-centered. Clients authenticate with Auth0, obtain an access token for `Auth__Audience`, and call protected API endpoints with `Authorization: Bearer <token>`.

The Next.js client has one public account-creation path: Auth0 Universal Login signup. It does not collect or persist passwords. After callback processing, new users continue to role onboarding and select `Customer` or `Seller`.

Identity persists `users.onboarding_completed_at` after the role selection succeeds. The frontend redirects only incomplete profiles to onboarding, so completed users are not prompted for a role on later logins.

The first protected profile sync endpoint is:

- `GET /gateway/auth/me`
- downstream: `GET /api/v1/auth/me`
- owner: [[02_SERVICES#Identity]]
- behavior: validates the Auth0 token, reads `sub`, `email`, and display-name claims, finds or creates the local user, stores the Auth0 provider and subject in [[03_DATABASES#IdentityDb]], and returns the local user profile.

Auth0 custom API access tokens do not include Google profile fields by default. The Auth0 Login Flow must add these namespaced access-token claims:

- `https://ecommerce.local/claims/email`
- `https://ecommerce.local/claims/name`

The Auth0 onboarding role endpoint is:

- `PUT /gateway/auth/me/role`
- downstream: `PUT /api/v1/auth/me/role`
- allowed self-service roles: `Customer`, `Seller`
- forbidden self-service role: `Admin`

# Authorization

Endpoint-level authorization is opt-in. There is no global fallback policy, so health checks and documented public query endpoints remain reachable without a token.

Shared policies from `ECommerce.BuildingBlocks.Security` are:

- `AuthenticatedUser`: any valid authenticated Auth0 access token.
- `Admin`: `Admin` role.
- `InventoryWrite`: `Admin` role or exact `inventory:write` permission from an Auth0 M2M token.
- `SellerOrAdmin`: `Seller` or `Admin` role.
- `CustomerOrAdmin`: `Customer` or `Admin` role.

JWT role evaluation reads the claim configured by `Auth__RoleClaimType`. The Auth0 API Login Flow must add the user's authorization roles to the default namespaced claim `https://ecommerce.local/claims/roles` as an array.

Currently enforced privileged operations are:

- Catalog product `POST` and `PUT`: `Admin`.
- Inventory item `PUT`: `Admin` role or `inventory:write` M2M permission.
- Identity user lookup by arbitrary id: `Admin`.

Identity local roles and Auth0 authorization roles are separate stores. An operational administrator must currently be assigned `Admin` in both systems. Self-service role selection updates only Identity; automatic synchronization to Auth0 token roles is TODO.

Current role model:

- `Admin`: reserved for controlled operational assignment, never created through public registration.
- `Seller`: self-service role for product and seller workflows.
- `Customer`: default self-service role for shopping workflows.

TODO:

- Synchronize Identity role changes with Auth0 authorization roles.
- Enforce ownership for Basket, Ordering, and Notification customer resources before treating role checks as sufficient protection.
- Apply `SellerOrAdmin` only after Catalog models seller/store ownership.

# Secrets

Infisical is the preferred shared development secret source.

Required secret keys include:

- all `ConnectionStrings__*Db`
- `Redis__ConnectionString`
- `RabbitMq__ConnectionString`
- `Cloudinary__CloudName`
- `Cloudinary__ApiKey`
- `Cloudinary__ApiSecret`
- `Auth__Authority`
- `Auth__Audience`
- `Auth__RequireHttpsMetadata`
- `RuntimeChecks__Auth0ClientId` for trusted runtime-token acquisition; identifier only, but managed with the runtime configuration.
- `RuntimeChecks__Auth0ClientSecret` for trusted runtime-token acquisition; secret.
- `OTEL_EXPORTER_OTLP_HEADERS` when managed Grafana Cloud export is enabled

`OTEL_EXPORTER_OTLP_ENDPOINT` and `OTEL_EXPORTER_OTLP_PROTOCOL` are configuration values; the header contains the Grafana Cloud credential and must remain secret. Do not print it during validation, store it in `.env`, or include it in screenshots. Use a telemetry-write token and rotate/revoke it from Grafana Cloud if exposed.

# Telemetry Data Handling

`ECommerce.BuildingBlocks.Observability` enables SDK-side redaction by default. Before export, trace tags and structured-log attributes whose keys indicate authorization, cookies, passwords, secrets, tokens, API keys, connection strings, payment-card fields, query strings, full URLs, HTTP targets, or database statements are replaced with `[REDACTED]`. Configure with `Observability__RedactionEnabled`; disabling it is restricted to controlled diagnostics.

The processor is defense in depth, not permission to log secrets. It does not rewrite arbitrary free-text log message bodies or every nested object. Application code must never place credentials, tokens, payment data, connection strings, or unnecessary personal data directly in message text. Production Collector/Alloy policy must add body/content redaction, allow-listing, sampling, and environment-specific retention review.

Real secrets must not be committed.

# Passwords

Identity stores password hashes through framework identity password hashing.

# External Providers

- Auth0: authentication authority.
- Auth0 `sub`: stable external user identifier mapped to Identity `users.external_subject`.
- Infisical: secret management.
- Cloudinary: catalog image provider configuration.
- Neon: managed PostgreSQL.
- CloudAMQP: managed RabbitMQ.
- Redis Cloud: managed Redis.

# Local Observability Access

Docker development exposes Jaeger on `16686`, Prometheus on `9090`, Loki on `3100`, Collector endpoints on `4317`, `4318`, `8889`, and `13133`, and Grafana on `3001`. Grafana permits anonymous Viewer access and Loki has authentication disabled for local development. These ports and unauthenticated access must not be exposed as a production security model; production requires authentication, TLS, network restrictions, retention controls, and Collector-side defense in depth beyond the active SDK attribute redaction.

# TODO

- Add frontend Auth0 login flow documentation.
- Add rate limiting.
- Add gateway auth policies.
