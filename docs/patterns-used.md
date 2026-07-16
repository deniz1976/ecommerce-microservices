# Patterns Used

This document lists the architectural and implementation patterns used in the project.

## Database Per Service

Used in:

- Catalog
- Basket
- Ordering
- Ordering Saga
- Inventory
- Payment
- Shipping
- Notification
- Identity

Meaning:

Each service owns its own database.

Why:

- Avoid shared database coupling.
- Keep service ownership clear.
- Allow independent schema changes.

Example:

```text
Ordering uses ordering_db.
Inventory uses inventory_db.
Ordering does not directly update inventory_db.
```

## Layered Architecture

Used in most services:

```text
Api
Application
Domain
Infrastructure
```

Meaning:

Code is separated by responsibility.

Why:

- API stays thin.
- Business logic stays testable.
- Infrastructure details stay replaceable.

Example:

```text
Ordering.Api receives HTTP request.
Ordering.Application runs use case.
Ordering.Domain protects business rules.
Ordering.Infrastructure writes PostgreSQL and publishes messages.
```

## Repository Pattern

Used in:

- Catalog
- Basket history
- Ordering
- Inventory
- Payment
- Shipping
- Notification
- Identity
- Ordering Saga

Meaning:

Application code depends on an interface, not directly on EF Core.

Example:

```text
IOrderRepository
OrderRepository
```

Why:

- Keeps application logic cleaner.
- Makes unit testing easier.
- Hides persistence details.

## DTO Pattern

Used in API and application models.

Examples:

- `CreateOrderRequest`
- `OrderResponse`
- `CreateProductRequest`
- `BasketResponse`

Meaning:

DTOs carry data in and out. They are not domain entities.

Why:

- Avoid exposing domain internals.
- Keep API contracts stable.

## Transactional Outbox

Used in:

- Ordering
- Ordering Saga
- Inventory
- Payment
- Shipping
- Notification

Meaning:

Outgoing messages are stored in the service database before being delivered to RabbitMQ.

Why:

- Prevent message loss.
- Keep database change and outgoing message coordinated.

Example:

```text
Ordering saves order and stores OrderSubmitted in OutboxMessage.
MassTransit later publishes OrderSubmitted.
```

## Inbox Pattern

Used through MassTransit EF tables and domain idempotency.

Tables:

```text
InboxState
```

Domain example:

```text
Inventory stock_reservations
```

Why:

- Prevent duplicate message processing.

Example:

```text
ReserveInventory arrives twice.
Inventory sees existing stock_reservation.
Inventory does not reserve twice.
```

## Saga Pattern

Used in:

```text
Ordering Saga Worker
```

Meaning:

Coordinates a workflow across services without one giant transaction.

Why:

- Order flow touches Inventory, Payment, Shipping, Notification.
- These services have separate databases.

Example:

```text
OrderSubmitted
  -> ReserveInventory
  -> AuthorizePayment
  -> CreateShipment
```

## Compensation Pattern

Used in:

```text
Ordering Saga Worker
```

Meaning:

When a later step fails, publish actions to undo earlier steps.

Example:

```text
Payment succeeded but shipping failed.
Saga publishes RefundPayment and ReleaseInventory.
```

## Event-Driven Architecture

Used through RabbitMQ messages.

Meaning:

Services react to events instead of tightly calling each other for every step.

Example:

```text
Notification service listens to PaymentAuthorized.
Payment service does not directly call Notification service.
```

Why:

- Decoupling.
- Better async workflows.
- Easier to add new subscribers later.

## API Gateway Pattern

Used in:

```text
ECommerce.ApiGateway
```

Meaning:

Clients call one gateway instead of many services directly.

Why:

- Cleaner client integration.
- Central routing.
- Future place for auth/rate limiting/cross-cutting concerns.

## Health Check Pattern

Used in every API.

Endpoints:

```text
/health/live
/health/ready
```

Why:

- Docker/runtime can check whether services are alive.
- Gateway can expose service readiness.

## Localization Pattern

Used in:

```text
BuildingBlocks.Localization
```

Meaning:

Error messages can be returned in Turkish or English.

Why:

- Project is planned for both TR and EN.

## Options Pattern

Used for configuration classes.

Examples:

- `RabbitMqOptions`
- `RedisOptions`
- `PostgresOptions`
- `AuthOptions`
- `ObservabilityOptions`

Meaning:

Configuration is mapped into typed classes.

Why:

- Less stringly typed config.
- Easier validation later.

## Dependency Injection

Used everywhere in .NET services.

Meaning:

Classes ask for interfaces in constructors. The framework provides implementations.

Example:

```text
OrderService receives IOrderRepository.
Runtime provides OrderRepository.
```

Why:

- Testability.
- Loose coupling.
- Clear dependencies.

## In-Memory Fallback

Used in Basket.

Meaning:

If Redis is not configured, Basket can use local memory for development.

Why:

- Easier local experimentation.

Not for production:

```text
In-memory state disappears when service restarts.
Multiple service instances would not share basket data.
```
