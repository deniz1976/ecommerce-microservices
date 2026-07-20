---
id: service-identity
type: service
version: 1
status: active
tags:
- microservice
- identity
- users
related:
- database-identity
- api-identity
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Own user registration, external Auth0 profile mapping, profile lookup, password hashing, local role persistence, and Auth0 self-service role synchronization.

# Responsibilities

- Register users.
- Sync Auth0-authenticated users into local profile storage.
- Persist Auth0 provider and subject values for stable external identity matching.
- Allow self-service `Customer` and `Seller` role selection.
- Synchronize authenticated onboarding role changes to Auth0 before committing the local role.
- Reserve `Admin` for controlled operational assignment outside public registration.
- Hash passwords.
- Return user profile data.
- Persist user roles.

# Dependencies

- [[../../../docs/ai/03_DATABASES#IdentityDb]]
- `Microsoft.Extensions.Identity.Core`
- Auth0 Management API through a dedicated least-privilege M2M application.

# Database

See [[../../../docs/ai/03_DATABASES#IdentityDb]].

# APIs

See [[../../../docs/ai/05_APIS#Identity API]].

Self-profile sync and role selection require an authenticated token. Looking up an arbitrary user id requires the shared `Admin` authorization policy.

# Events Published

None.

# Events Consumed

None.

# Important Classes

- `AuthEndpoints`
- `UserEndpoints`
- `UserService`
- `PasswordHashService`
- `UserRepository`
- `IdentityDbContext`

# Folder Structure

- `ECommerce.Identity.Api`
- `ECommerce.Identity.Application`
- `ECommerce.Identity.Domain`
- `ECommerce.Identity.Infrastructure`

# Configuration

- `ConnectionStrings__IdentityDb`
- `Auth__Authority`
- `Auth__Audience`
- `Auth__RoleClaimType`
- `Auth0Management__Enabled`
- `Auth0Management__Domain`
- `Auth0Management__ClientId`
- `Auth0Management__ClientSecret`
- `Auth0Management__CustomerRoleId`
- `Auth0Management__SellerRoleId`
- `Auth0Management__TimeoutSeconds`

# Design Decisions

Login and frontend registration are Auth0-centered. Identity validates Auth0 JWTs and maps authenticated external users to local profile records by `external_provider` and `external_subject`. Email is used as a fallback only to link an existing local user on first external login. The frontend does not collect local passwords.

Auth0 Login Flow adds `https://ecommerce.local/claims/email` and `https://ecommerce.local/claims/name` to custom API access tokens. Identity accepts these namespaced claims when standard profile claims are absent.

Public role selection replaces the `Customer` or `Seller` role atomically in Identity persistence so duplicate onboarding requests do not produce optimistic-concurrency failures.

`users.onboarding_completed_at` distinguishes a role selected by the user from an incomplete Auth0 profile. Completed users bypass role onboarding on future logins.

Public registration and Auth0 onboarding can only select `Customer` or `Seller`. `Admin` is intentionally not reachable through self-registration.

Public registration explicitly uses `AllowAnonymous`; the shared fallback policy protects any new Identity route unless it is deliberately opened.

The API authorization role is read from the namespaced Auth0 token claim configured by `Auth__RoleClaimType`. When `Auth0Management__Enabled=true`, authenticated onboarding removes the opposite self-service Auth0 role, assigns the selected role, and only then commits the same role locally. Auth0 failure returns a retryable service-unavailable response and leaves the local onboarding role unchanged. The frontend bypasses its token cache after a successful change so the next access token contains the new role claim.

The Management API client is separate from the runtime-test M2M client and receives only the scopes needed to update user role membership. `Admin` remains outside self-service synchronization and requires controlled operational assignment.

# Future Improvements

- Document frontend Auth0 login flow.
- Add reconciliation/outbox processing for the rare case where Auth0 succeeds but the following local database commit fails.
