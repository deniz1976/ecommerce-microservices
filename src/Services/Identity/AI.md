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

Own user registration, external Auth0 profile mapping, profile lookup, password hashing, and role persistence.

# Responsibilities

- Register users.
- Sync Auth0-authenticated users into local profile storage.
- Persist Auth0 provider and subject values for stable external identity matching.
- Allow self-service `Customer` and `Seller` role selection.
- Reserve `Admin` for controlled operational assignment outside public registration.
- Hash passwords.
- Return user profile data.
- Persist user roles.

# Dependencies

- [[../../../docs/ai/03_DATABASES#IdentityDb]]
- `Microsoft.Extensions.Identity.Core`

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

# Design Decisions

Login and frontend registration are Auth0-centered. Identity validates Auth0 JWTs and maps authenticated external users to local profile records by `external_provider` and `external_subject`. Email is used as a fallback only to link an existing local user on first external login. The frontend does not collect local passwords.

Auth0 Login Flow adds `https://ecommerce.local/claims/email` and `https://ecommerce.local/claims/name` to custom API access tokens. Identity accepts these namespaced claims when standard profile claims are absent.

Public role selection replaces the `Customer` or `Seller` role atomically in Identity persistence so duplicate onboarding requests do not produce optimistic-concurrency failures.

`users.onboarding_completed_at` distinguishes a role selected by the user from an incomplete Auth0 profile. Completed users bypass role onboarding on future logins.

Public registration and Auth0 onboarding can only select `Customer` or `Seller`. `Admin` is intentionally not reachable through self-registration.

The API authorization role is read from the namespaced Auth0 token claim configured by `Auth__RoleClaimType`. Identity database roles are not yet synchronized automatically to Auth0 roles; operational administrators currently require controlled `Admin` assignment in both systems.

# Future Improvements

- Document frontend Auth0 login flow.
- Synchronize Identity role changes with Auth0 authorization roles.
