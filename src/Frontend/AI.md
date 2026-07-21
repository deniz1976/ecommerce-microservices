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
- Refresh the Auth0 access token without using the local cache after role onboarding.
- Route an authenticated `Admin` to the administration overview at `/`.
- Display a live read-only Catalog summary for the administrator.
- Route an authenticated `Seller` to an ownership-aware seller workspace.
- List and create stores owned by the current seller and show products filtered by the selected store.

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
- `GET /gateway/catalog/stores/mine`
- `POST /gateway/catalog/stores`
- `GET /gateway/catalog/references/categories`
- `GET /gateway/catalog/references/brands`
- `POST /gateway/catalog/products`

The Catalog product `status` field is the numeric .NET `ProductStatus` enum and is mapped to localized UI labels in the typed client.

# Events Published

None.

# Events Consumed

None.

# Important Classes

- `HomeShell`
- `AdminDashboard`
- `SellerDashboard`
- `StoreCreateForm`
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

The administrator overview uses only the existing profile and public Catalog query. The seller workspace uses authenticated store endpoints, never accepts an owner id, filters product search by the selected owned store, loads active category/brand references, and creates products against that store. Empty reference sets disable product creation instead of inventing identifiers. See [[../../docs/ai/09_DECISIONS#decision-catalog-seller-store-ownership]].

After Identity confirms a self-service role change in both Auth0 and local persistence, the onboarding client forces a silent token refresh. This prevents the existing cached JWT from continuing without the newly assigned namespaced role claim.

# Future Improvements

- Add server-enforced admin API authorization.
- Add product update and image-upload editing.
- Add dedicated administrator metrics endpoints instead of client-side summary values.
