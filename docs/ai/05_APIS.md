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

## Frontend Client

- id: `api-frontend-client`
- owner: frontend
- path: `src/Frontend`
- framework: Next.js App Router
- API base env: `NEXT_PUBLIC_API_BASE_URL`
- Auth0 public env: `NEXT_PUBLIC_AUTH0_DOMAIN`, `NEXT_PUBLIC_AUTH0_CLIENT_ID`, `NEXT_PUBLIC_AUTH0_AUDIENCE`
- calls: `/gateway/users`, `/gateway/auth/me`, `/gateway/auth/me/role`

## Catalog API

- id: `api-catalog`
- owner: [[02_SERVICES#Catalog]]
- direct base: `http://localhost:5283`
- gateway routes: `/gateway/catalog/products`, `/gateway/catalog/products/{everything}`
- service routes: `/api/v1/products`
- methods in code: `GET`, `POST`, `PUT`
- public routes: product `GET` routes.
- protected routes: product `POST` and `PUT` require the `Admin` authorization policy.
- response note: product `status` is serialized as the numeric .NET `ProductStatus` enum (`0` Draft, `1` Active, `2` Inactive, `3` Archived).

## Basket API

- id: `api-basket`
- owner: [[02_SERVICES#Basket]]
- direct base: `http://localhost:5041`
- gateway route: `/gateway/baskets/{everything}`
- service routes: `/api/v1/baskets/{customerId}`, `/items`, `/checkout`

## Ordering API

- id: `api-ordering`
- owner: [[02_SERVICES#Ordering]]
- direct base: `http://localhost:5265`
- gateway routes: `/gateway/orders`, `/gateway/orders/{everything}`
- service routes: `/api/v1/orders`

## Inventory API

- id: `api-inventory`
- owner: [[02_SERVICES#Inventory]]
- direct base: `http://localhost:5054`
- gateway route: `/gateway/inventory/{everything}`
- service routes: `/api/v1/inventory/items/{productId}`
- public routes: item `GET`.
- protected routes: item `PUT` requires the `Admin` authorization policy.

## Identity API

- id: `api-identity`
- owner: [[02_SERVICES#Identity]]
- direct base: `http://localhost:5090`
- gateway routes: `/gateway/users`, `/gateway/users/{everything}`, `/gateway/auth/{everything}`
- service routes: `/api/v1/users`, `/api/v1/auth/me`, `/api/v1/auth/me/role`
- public routes: `POST /gateway/users` supports self-registration as `Customer` or `Seller` only.
- authenticated-user routes: `GET /gateway/auth/me` and `PUT /gateway/auth/me/role` require a valid Bearer token from [[07_SECURITY#Authentication]].
- admin route: `GET /gateway/users/{id}` requires the `Admin` authorization policy.

## Notification SignalR

- id: `api-notification-signalr`
- owner: [[02_SERVICES#Notification]]
- direct base: `http://localhost:5234`
- gateway route: `/gateway/hubs/notifications/{everything}`
- hub: `/hubs/notifications`
- client method: `notificationReceived`

## Health APIs

- id: `api-health`
- gateway live: `/health/live`
- gateway ready: `/health/ready`
- service health routes: `/gateway/health/catalog`, `/gateway/health/basket`, `/gateway/health/ordering`, `/gateway/health/inventory`, `/gateway/health/payment`, `/gateway/health/shipping`, `/gateway/health/notification`, `/gateway/health/identity`

# TODO

- Add request/response schemas for every endpoint.
- Add authenticated customer ownership checks for Basket, Ordering, and Notification resources.
