# Architecture Overview

## What We Are Building

The project is an e-commerce backend. Instead of one large application that owns every feature, the backend is split into smaller services. Each service owns one business area and its own database.

This is the main idea:

```text
One business capability = one service = one database boundary
```

For example:

- Catalog owns products.
- Basket owns active and checked-out basket data.
- Ordering owns orders.
- Inventory owns stock.
- Payment owns payment records.
- Shipping owns shipments.
- Notification owns notification history and realtime delivery.
- Identity owns users and roles.

## Why Microservices Here

We chose microservices because the domain naturally separates into different business capabilities. Product catalog, basket, ordering, stock, payment, shipping, and notifications all change for different reasons.

Benefits:

- Each service can evolve independently.
- Each service can have its own database schema.
- Failures can be isolated better.
- Messaging can decouple long-running workflows.
- The system is closer to real production e-commerce architecture.

Tradeoffs:

- More moving parts.
- More configuration.
- Harder local runtime.
- More need for documentation.
- Eventual consistency instead of one simple database transaction.

## Layering Inside Each Service

Most services follow this shape:

```text
ECommerce.X.Api
ECommerce.X.Application
ECommerce.X.Domain
ECommerce.X.Infrastructure
```

### Api

The API project exposes HTTP endpoints or hosts the service process.

Examples:

- `ECommerce.Catalog.Api`
- `ECommerce.Ordering.Api`
- `ECommerce.Inventory.Api`

The API layer usually contains:

- `Program.cs`
- endpoint mapping
- request/response HTTP behavior
- health checks
- authentication middleware hooks

### Application

The application layer contains use cases and service logic.

Examples:

- `OrderService`
- `BasketService`
- `InventoryService`
- `PaymentService`

It decides what should happen, but avoids direct infrastructure details.

### Domain

The domain layer contains business objects and business rules.

Examples:

- `Order`
- `InventoryItem`
- `Payment`
- `Shipment`
- `Basket`

Domain objects know business rules such as:

- how order totals are calculated
- how stock is reserved
- how a shipment changes status
- how basket items are added or removed

### Infrastructure

The infrastructure layer contains technical implementation details.

Examples:

- EF Core DbContext
- repository implementations
- MassTransit consumers
- Redis store implementation
- Cloudinary image provider placeholder

## Shared Building Blocks

The `src/BuildingBlocks` folder contains reusable pieces used by multiple services.

### Contracts

Contains shared message contracts and common result/error models.

Examples:

- `OrderSubmitted`
- `ReserveInventory`
- `InventoryReserved`
- `AuthorizePayment`
- `PaymentAuthorized`
- `CreateShipment`
- `ShipmentCreated`

These contracts define the language services use when talking over RabbitMQ.

### EventBus

Contains MassTransit and RabbitMQ setup helpers.

Important files:

- `DependencyInjection.cs`
- `RabbitMqOptions.cs`
- `OutboxRegistrationExtensions.cs`

This is where RabbitMQ configuration and EF Core outbox registration are centralized.

### Persistence

Contains PostgreSQL registration helpers and Neon connection string normalization.

Important files:

- `DependencyInjection.cs`
- `PostgresConnectionString.cs`
- `PostgresOptions.cs`

### Security

Contains OIDC/JWT-ready security hooks.

Current status:

- Authentication wiring exists.
- Token issuer/login flow is not implemented yet.

### Localization

Contains English and Turkish error message support.

Current usage:

- API errors can be localized by `Accept-Language`.

### Observability

Contains OpenTelemetry registration helper.

Current status:

- Instrumentation is wired.
- Production dashboards/exporters are not finalized.

## API Gateway

The API Gateway is implemented with Ocelot.

Project:

```text
src/ApiGateways/ECommerce.ApiGateway
```

Purpose:

- One entry point for clients.
- Routes external paths to internal services.
- Keeps clients from needing to know every service port.

Example:

```text
Client calls:
/gateway/orders

Gateway forwards to:
Ordering API /api/v1/orders
```

## Database Per Service

Each service has its own PostgreSQL database.

Current databases:

- `catalog_db`
- `basket_db`
- `ordering_db`
- `ordering_saga_db`
- `inventory_db`
- `payment_db`
- `shipping_db`
- `notification_db`
- `identity_db`

This is intentional. Services should not directly read each other's tables.

## Messaging

RabbitMQ is used for async communication.

MassTransit sits on top of RabbitMQ and gives us:

- typed message contracts
- consumers
- endpoint naming
- retry-friendly message processing
- EF Core inbox/outbox tables
- transactional outbox publishing

## Redis

Redis is currently used by Basket only.

Purpose:

- Store active basket state quickly.
- Avoid writing every basket item change directly to PostgreSQL.

PostgreSQL still stores basket checkout history.

## Current Runtime Shape

Local development:

```text
Docker Compose
  API services
  Saga worker
  API gateway
  local Redis

Managed services
  Neon PostgreSQL
  CloudAMQP RabbitMQ
```

Future production:

```text
Container platform
  APIs and workers

Managed services
  PostgreSQL provider
  RabbitMQ provider
  Redis provider
  observability stack
```
