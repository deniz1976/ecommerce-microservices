---
id: frontend-web-client
type: frontend
version: 1
status: active
tags:
- nextjs
- frontend
- auth0
- admin
related:
- service-api-gateway
- service-identity
- service-catalog
- api-frontend-client
owners:
- frontend
last_reviewed: 2026-07-14
graph_ready: true
---

# Purpose

Provide the TR/EN Next.js web client for Auth0 login, local role onboarding, and role-aware authenticated experiences.

# Responsibilities

- Authenticate browser users through Auth0 Authorization Code with PKCE.
- Read the local Identity profile through the API Gateway.
- Route an authenticated `Admin` to the administration overview at `/`.
- Display a live read-only Catalog summary for the administrator.

# Dependencies

- [[../../docs/ai/01_ARCHITECTURE#Core Components]]
- [[../../docs/ai/05_APIS#Frontend Client]]
- Auth0 SPA SDK.

# Database

The frontend does not own a database. See [[../../docs/ai/03_DATABASES#IdentityDb]] and [[../../docs/ai/03_DATABASES#CatalogDb]].

# APIs

- `GET /gateway/auth/me`
- `PUT /gateway/auth/me/role`
- `GET /gateway/catalog/products`

The Catalog product `status` field is the numeric .NET `ProductStatus` enum and is mapped to localized UI labels in the typed client.

# Events Published

None.

# Events Consumed

None.

# Important Classes

- `HomeShell`
- `AdminDashboard`
- `getProfile`
- `getCatalogProducts`

# Folder Structure

- `app`: App Router pages and global styling.
- `components/auth`: authentication and onboarding components.
- `components/admin`: administrator workspace components.
- `lib/api`: typed API gateway clients.
- `lib/i18n`: TR/EN dictionaries and provider.

# Configuration

- `NEXT_PUBLIC_API_BASE_URL`
- `NEXT_PUBLIC_AUTH0_DOMAIN`
- `NEXT_PUBLIC_AUTH0_CLIENT_ID`
- `NEXT_PUBLIC_AUTH0_AUDIENCE`

# Design Decisions

The initial administrator overview uses only the existing profile and public Catalog query. It does not model a personal seller store because Catalog currently has no seller or store ownership field. See [[../../docs/ai/09_DECISIONS#decision-role-aware-frontend-entry]].

# Future Improvements

- Add server-enforced admin API authorization.
- Add seller/store ownership before implementing seller-specific store dashboards.
- Add dedicated administrator metrics endpoints instead of client-side summary values.
