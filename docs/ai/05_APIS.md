---
id: ai-apis
type: api-catalog
version: 1
status: active
tags:
- api
- gateway
- http
related:
- ai-services
- ai-deployment
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Catalog HTTP and SignalR APIs exposed by this repository.

# API Catalog

## Gateway

- id: `api-gateway-routes`
- owner: [[02_SERVICES#ApiGateway]]
- local base URL: `http://localhost:5080`
- docker base URL: `http://localhost:5080`
- config: `src/ApiGateways/ECommerce.ApiGateway/ocelot.json`, `ocelot.Docker.json`
- authentication: global `Bearer` authentication; only the documented public route allow-list bypasses it.

## Frontend Client

- id: `api-frontend-client`
- owner: frontend
- path: `src/Frontend`
- framework: Next.js App Router
- API base env: `NEXT_PUBLIC_API_BASE_URL`
- Auth0 public env: `NEXT_PUBLIC_AUTH0_DOMAIN`, `NEXT_PUBLIC_AUTH0_CLIENT_ID`, `NEXT_PUBLIC_AUTH0_AUDIENCE`
- calls: `/gateway/users`, `/gateway/auth/me`, `/gateway/auth/me/role`, `/gateway/catalog/products`, `/gateway/catalog/stores/mine`, `/gateway/catalog/stores`, `/gateway/catalog/references/categories`, `/gateway/catalog/references/brands`
- seller behavior: authenticated sellers list/create only their Identity-resolved stores, filter product search by the selected store, and create products from active category/brand references. Store creation sends name/slug only; ownership is derived server-side.

## Catalog API

- id: `api-catalog`
- owner: [[02_SERVICES#Catalog]]
- direct base: `http://localhost:5283`
- gateway routes: `/gateway/catalog/products`, `/gateway/catalog/products/{everything}`, `/gateway/catalog/stores`, `/gateway/catalog/stores/mine`, `/gateway/catalog/stores/{id}`, `/gateway/catalog/references/categories`, `/gateway/catalog/references/brands`
- service routes: `/api/v1/products`, `/api/v1/stores`, `/api/v1/catalog-references/categories`, `/api/v1/catalog-references/brands`
- methods in code: `GET`, `POST`, `PUT`
- public routes: product `GET` routes, store-by-id `GET`, and active category/brand reference `GET` routes.
- protected routes: product `POST`/`PUT`, store `POST`, and store `/mine` require `SellerOrAdmin`.
- ownership: store owner is derived from Identity `/api/v1/auth/me`. Seller product creation requires an owned `storeId`; seller update requires ownership of the product's store. Admin bypasses ownership and may create a platform product with null `storeId`.
- product search accepts optional `storeId`; product responses include nullable `storeId`.
- response note: product `status` is serialized as the numeric .NET `ProductStatus` enum (`0` Draft, `1` Active, `2` Inactive, `3` Archived).

## Basket API

- id: `api-basket`
- owner: [[02_SERVICES#Basket]]
- direct base: `http://localhost:5041`
- gateway route: `/gateway/baskets/{everything}`
- service routes: `/api/v1/baskets/{customerId}`, `/items`, `/checkout`
- protected routes: every Basket route requires `AuthenticatedUser` plus owner-or-privileged customer authorization.
- ownership status: normal users may access only the customer `Guid` resolved by Identity `/api/v1/auth/me`; `Admin` or `customer:act` may act for another customer.

## Ordering API

- id: `api-ordering`
- owner: [[02_SERVICES#Ordering]]
- direct base: `http://localhost:5265`
- gateway routes: `/gateway/orders`, `/gateway/orders/{everything}`
- service routes: `/api/v1/orders`
- protected routes: every Ordering route requires `AuthenticatedUser` plus owner-or-privileged customer authorization.
- ownership status: create and customer-list operations validate the requested customer `Guid`; another customer's order-by-id is hidden as not found. `Admin` or `customer:act` may act for another customer.

## Inventory API

- id: `api-inventory`
- owner: [[02_SERVICES#Inventory]]
- direct base: `http://localhost:5054`
- gateway route: `/gateway/inventory/{everything}`
- service routes: `/api/v1/inventory/items/{productId}`
- public routes: item `GET`.
- protected routes: item `PUT` requires `InventoryWrite`, accepting an `Admin` user role or the Auth0 `inventory:write` M2M permission.

## Identity API

- id: `api-identity`
- owner: [[02_SERVICES#Identity]]
- direct base: `http://localhost:5090`
- gateway routes: `/gateway/users`, `/gateway/users/{everything}`, `/gateway/auth/{everything}`
- service routes: `/api/v1/users`, `/api/v1/auth/me`, `/api/v1/auth/me/role`
- public routes: `POST /gateway/users` supports self-registration as `Customer` or `Seller` only.
- authenticated-user routes: `GET /gateway/auth/me` and `PUT /gateway/auth/me/role` require a valid Bearer token from [[07_SECURITY#Authentication]].
- role synchronization: `PUT /gateway/auth/me/role` synchronizes the selected `Customer` or `Seller` role to Auth0 before updating Identity persistence. Auth0 rejection, timeout, or malformed token response returns `503 Service Unavailable`; validation failures remain `400`.
- admin route: `GET /gateway/users/{id}` requires the `Admin` authorization policy.

## Notification SignalR

- id: `api-notification-signalr`
- owner: [[02_SERVICES#Notification]]
- direct base: `http://localhost:5234`
- gateway route: `/gateway/hubs/notifications/{everything}`
- hub: `/hubs/notifications`
- client method: `notificationReceived`
- protected route: the SignalR connection requires `AuthenticatedUser`; joining or leaving a customer group additionally requires owner-or-privileged authorization.
- ownership status: normal users may join only the group for the customer `Guid` resolved by Identity `/api/v1/auth/me`; `Admin` or `customer:act` may act for another customer.

## Health APIs

- id: `api-health`
- gateway live: `/health/live`
- gateway ready: `/health/ready`
- service health routes: `/gateway/health/catalog`, `/gateway/health/basket`, `/gateway/health/ordering`, `/gateway/health/inventory`, `/gateway/health/payment`, `/gateway/health/shipping`, `/gateway/health/notification`, `/gateway/health/identity`

# TODO

- Add request/response schemas for every endpoint.
- Add full HTTP/SignalR integration tests for negative customer ownership cases.
