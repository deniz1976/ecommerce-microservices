# API Security Baseline

## Default-Deny Rule

All service APIs use a shared fallback authorization policy. An endpoint requires an authenticated user unless it deliberately declares `AllowAnonymous` or a stricter named policy. Missing Auth0 configuration registers a rejecting Bearer handler, so a protected endpoint does not become public by configuration omission.

Ocelot repeats this boundary at the gateway. Both gateway route files apply global `Bearer` authentication. Public routes override the global rule with `AuthenticationOptions.AllowAnonymous`.

The gateway registers shared authentication with `AddOidcReadyAuthentication` but does not register the service authorization policies. This prevents ASP.NET minimal hosting from automatically inserting fallback authorization before Ocelot, where no route has been selected and even approved anonymous and health routes would be rejected. Ocelot performs gateway route authorization; downstream services use `AddOidcReadySecurity` and independently retain their ASP.NET fallback policy.

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
| Basket | `AuthenticatedUser` plus ownership | Identity-resolved owner, `Admin`, or `customer:act` |
| Ordering | `AuthenticatedUser` plus ownership | Identity-resolved owner, `Admin`, or `customer:act` |
| Notification SignalR | `AuthenticatedUser` plus ownership | Identity-resolved customer group, `Admin`, or `customer:act` |

## Customer Ownership

Authentication alone does not prove that a caller owns a customer resource. Basket, Ordering, and Notification use the shared ownership authorizer, which forwards the original Bearer token to Identity `/api/v1/auth/me` and compares the returned local user `Guid` with the requested customer identifier. Lookup failure denies access. Order-by-id hides another customer's existing order as not found.

`Admin` and the exact `customer:act` permission may bypass the owner comparison. `customer:act` exists for trusted runtime automation, must not be granted to normal users, and does not grant unrelated administrator operations. Notification query-string tokens are forwarded only from the direct hub path.

## Automated Enforcement

Contract tests verify:

- the fallback policy requires authentication;
- Basket and Ordering route groups require `AuthenticatedUser`;
- Notification SignalR requires `AuthenticatedUser`;
- the shared ownership authorizer allows the Identity-resolved owner and rejects another customer;
- `Admin` and `customer:act` delegation bypass Identity lookup;
- Basket, Ordering, and Notification source guards require ownership checks and registration;
- public service routes explicitly declare `AllowAnonymous`;
- both Ocelot configurations use global Bearer authentication;
- the gateway anonymous routes exactly match the approved allow-list.

These tests run with the normal solution test suite and therefore fail CI when the security baseline changes unexpectedly.
