---
id: service-basket
type: service
version: 1
status: active
tags:
- microservice
- basket
- redis
related:
- database-basket
- store-redis-basket
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Own active basket state and checkout snapshot history.

# Responsibilities

- Read active basket.
- Add/update basket items.
- Persist checkout snapshots.
- Use Redis for mutable active basket data.

# Dependencies

- [[../../../docs/ai/03_DATABASES#BasketDb]]
- [[../../../docs/ai/03_DATABASES#Redis]]
- Identity `GET /api/v1/auth/me` for trusted Auth0 `sub` to local customer `Guid` resolution.
- StackExchange.Redis

# Database

See [[../../../docs/ai/03_DATABASES#BasketDb]] and [[../../../docs/ai/03_DATABASES#Redis]].

# APIs

See [[../../../docs/ai/05_APIS#Basket API]].

# Events Published

None.

# Events Consumed

None.

# Important Classes

- `BasketEndpoints`
- `BasketService`
- `RedisActiveBasketStore`
- `BasketHistoryRepository`
- `BasketDbContext`

# Folder Structure

- `ECommerce.Basket.Api`
- `ECommerce.Basket.Application`
- `ECommerce.Basket.Domain`
- `ECommerce.Basket.Infrastructure`

# Configuration

- `ConnectionStrings__BasketDb`
- `Redis__ConnectionString`
- `Redis__BasketTtlHours`
- `IdentityClient__BaseUrl`
- `IdentityClient__TimeoutSeconds`

Redis configuration is validated during startup and is required. The service has no process-memory fallback.

# Design Decisions

Active basket is in Redis; checkout history is in PostgreSQL. Missing Redis configuration fails startup to prevent silent data loss.

Every Basket route requires the shared `AuthenticatedUser` policy and the shared customer ownership authorizer. Normal users may access only the local customer `Guid` resolved by forwarding their Bearer token to Identity `/api/v1/auth/me`; `Admin` or the narrow `customer:act` automation permission may act for another customer. Identity resolution fails closed.

# Future Improvements

- Add basket expiration policy documentation.
- Add basket merge behavior for authenticated sessions.
