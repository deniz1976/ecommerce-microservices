# API Security Baseline

## Default-Deny Rule

All service APIs use a shared fallback authorization policy. An endpoint requires an authenticated user unless it deliberately declares `AllowAnonymous` or a stricter named policy. Missing Auth0 configuration registers a rejecting Bearer handler, so a protected endpoint does not become public by configuration omission.

Ocelot repeats this boundary at the gateway. Both gateway route files apply global `Bearer` authentication. Public routes override the global rule with `AuthenticationOptions.AllowAnonymous`.

The gateway does not run the service fallback authorization middleware before Ocelot. At that point no Ocelot route has been selected, so the fallback would reject even approved anonymous and health routes. Ocelot performs the gateway route authorization; downstream services independently retain their ASP.NET fallback policy.

## Public Allow-List

Only these business operations are intentionally public:

- Catalog product `GET` routes.
- Inventory item `GET` route.
- Identity user registration `POST` route.
- Service and gateway health checks.

Catalog creation/update, Inventory update, arbitrary Identity user lookup, Basket, Ordering, Auth profile operations, and Notification SignalR are protected.

For SignalR transport compatibility, the JWT handler accepts an `access_token` query parameter only on the direct and gateway notification hub paths. Normal HTTP APIs continue to require the Authorization header.

## Current Protected Surface

| Surface | Current requirement | Ownership status |
|---|---|---|
| Catalog writes | `Admin` | Seller/store ownership is not modeled |
| Inventory write | `InventoryWrite` | Operational resource |
| Identity arbitrary user read | `Admin` | Admin-only |
| Identity `/auth/me` | `AuthenticatedUser` | Derived from token `sub` |
| Basket | `AuthenticatedUser` | Customer ownership pending |
| Ordering | `AuthenticatedUser` | Customer ownership pending |
| Notification SignalR | `AuthenticatedUser` | Customer group ownership pending |

## Important Limitation

Authentication answers “who presented a valid token?” It does not prove that a caller owns a customer resource. Basket and Ordering store the local Identity `Guid`, while Auth0 tokens identify users with an external `sub`. Until those identifiers are securely resolved, an authenticated caller could still request another known customer identifier. Notification group joins have the same limitation.

The next security phase must create a trusted local-user mapping available to resource-owning services, then enforce owner-or-admin rules and negative tests.

## Automated Enforcement

Contract tests verify:

- the fallback policy requires authentication;
- Basket and Ordering route groups require `AuthenticatedUser`;
- Notification SignalR requires `AuthenticatedUser`;
- public service routes explicitly declare `AllowAnonymous`;
- both Ocelot configurations use global Bearer authentication;
- the gateway anonymous routes exactly match the approved allow-list.

These tests run with the normal solution test suite and therefore fail CI when the security baseline changes unexpectedly.
