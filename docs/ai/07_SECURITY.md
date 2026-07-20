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

Authorization is default-deny. The shared fallback policy requires an authenticated user whenever an endpoint has no more specific authorization metadata. When OIDC settings are absent, the registered Bearer scheme fails closed instead of silently disabling protection.

Every intentionally public service endpoint is marked with `AllowAnonymous`. Ocelot applies the global `Bearer` authentication scheme and uses a route-level anonymous allow-list for the same public surface. Health checks, Catalog reads, Inventory reads, and Identity registration are intentionally public.

Service hosts register authentication plus authorization through `AddOidcReadySecurity` and run the ASP.NET fallback authorization middleware. The API Gateway registers authentication alone through `AddOidcReadyAuthentication`; Ocelot's own pipeline applies its global Bearer rule and route allow-list. Keeping authorization services out of the gateway also prevents minimal hosting from auto-inserting fallback authorization before Ocelot, which would reject requests before route selection, including the gateway's own health checks.

SignalR may send its Bearer token through the `access_token` query parameter when browser transport restrictions prevent an Authorization header. The JWT handler accepts that query parameter only on `/hubs/notifications` and `/gateway/hubs/notifications`; other routes do not accept query-string tokens.

Shared policies from `ECommerce.BuildingBlocks.Security` are:

- `AuthenticatedUser`: any valid authenticated Auth0 access token.
- `Admin`: `Admin` role.
- `InventoryWrite`: `Admin` role or exact `inventory:write` permission from an Auth0 M2M token.
- customer delegation: `Admin` role or exact `customer:act` permission; this is evaluated by the shared ownership authorizer and is not a general user permission.
- `SellerOrAdmin`: `Seller` or `Admin` role.
- `CustomerOrAdmin`: `Customer` or `Admin` role.

JWT role evaluation reads the claim configured by `Auth__RoleClaimType`. The Auth0 API Login Flow must add the user's authorization roles to the default namespaced claim `https://ecommerce.local/claims/roles` as an array.

Currently enforced privileged operations are:

- Catalog product `POST` and `PUT`: `Admin`.
- Inventory item `PUT`: `Admin` role or `inventory:write` M2M permission.
- Identity user lookup by arbitrary id: `Admin`.

Authenticated baseline operations are:

- every Basket route: `AuthenticatedUser`;
- every Ordering route: `AuthenticatedUser`;
- Notification SignalR connection: `AuthenticatedUser`.

Authentication is not resource ownership. Basket, Ordering, and Notification therefore forward the caller's Bearer token to Identity `/api/v1/auth/me`, resolve Auth0 `sub` to the local user `Guid`, and compare that trusted value with the requested customer resource. Identity lookup errors fail closed. `Admin` and the dedicated runtime M2M `customer:act` permission are the only bypasses. The query-string SignalR token is forwarded only when the original path is `/hubs/notifications`.

Identity local roles and Auth0 authorization roles remain separate stores, but authenticated self-service role selection synchronizes `Customer` and `Seller` through a dedicated Auth0 Management API M2M client before committing the local role. Auth0 failure fails closed and leaves local onboarding unchanged. The browser then bypasses its token cache to obtain the updated role claim. A later local database failure after Auth0 success is still a cross-system consistency edge case and requires reconciliation/outbox work.

The Management M2M application is not the runtime-integration application. It receives only user-role membership scopes required by Auth0 (`update:users`, or the tenant's equivalent `create:role_members` and `delete:role_members` grants); it does not receive application/client administration permissions. `Admin` is never synchronized through the public onboarding path and remains a controlled operational assignment.

Current role model:

- `Admin`: reserved for controlled operational assignment, never created through public registration.
- `Seller`: self-service role for product and seller workflows.
- `Customer`: default self-service role for shopping workflows.

TODO:

- Add reconciliation/outbox handling for cross-system role synchronization.
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
- `Auth0Management__Enabled`
- `Auth0Management__Domain`
- `Auth0Management__ClientId` and secret `Auth0Management__ClientSecret` for the dedicated Management API M2M client
- `Auth0Management__CustomerRoleId` and `Auth0Management__SellerRoleId`
- `Auth0Management__TimeoutSeconds`
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
- Add full HTTP/SignalR integration tests for dynamic negative ownership cases; shared authorizer behavior and endpoint adoption already have contract tests.
