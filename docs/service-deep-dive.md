# Service Deep Dive

This document explains how requests and messages move through the code. The goal is to connect folder names and file names to actual behavior.

## Common Service Shape

Most services follow this pattern:

```text
Api
  Program.cs
  Endpoint files

Application
  Use case services
  Request/response models
  Repository interfaces

Domain
  Business entities
  Status enums
  Business methods

Infrastructure
  DbContext
  EF configurations
  Repository implementations
  Message consumers/publishers
```

## Catalog Deep Dive

Main request:

```text
GET /gateway/catalog/products
```

Flow:

```text
Ocelot Gateway
  -> Catalog.Api ProductEndpoints
  -> Catalog.Application ProductService
  -> Catalog.Application IProductRepository
  -> Catalog.Infrastructure ProductRepository
  -> CatalogDbContext
  -> catalog_db
```

Important files:

- `Catalog.Api/Products/ProductEndpoints.cs`
- `Catalog.Api/Products/CatalogResults.cs`
- `Catalog.Application/Products/ProductService.cs`
- `Catalog.Application/Products/IProductRepository.cs`
- `Catalog.Application/Products/ProductMapper.cs`
- `Catalog.Domain/Product.cs`
- `Catalog.Infrastructure/Persistence/ProductRepository.cs`
- `Catalog.Infrastructure/Persistence/CatalogDbContext.cs`

What happens:

1. Endpoint receives HTTP request.
2. Endpoint calls `ProductService`.
3. `ProductService` validates and coordinates product use cases.
4. Repository reads or writes product aggregate data.
5. Mapper turns domain objects into response DTOs.

Domain objects:

- `Product`
- `ProductTranslation`
- `ProductImage`
- `Category`
- `CategoryTranslation`
- `Brand`

Important design choice:

Catalog stores multilingual product and category text through translation tables instead of hardcoding one language.

## Basket Deep Dive

Main requests:

```text
GET /gateway/baskets/{customerId}
PUT /gateway/baskets/{customerId}/items
POST /gateway/baskets/{customerId}/checkout
```

Flow for active basket:

```text
Ocelot Gateway
  -> Basket.Api BasketEndpoints
  -> Basket.Application BasketService
  -> IActiveBasketStore
  -> RedisActiveBasketStore
  -> Redis
```

Flow for checkout history:

```text
BasketService
  -> IBasketHistoryRepository
  -> BasketHistoryRepository
  -> BasketDbContext
  -> basket_db
```

Important files:

- `Basket.Api/Baskets/BasketEndpoints.cs`
- `Basket.Application/Baskets/BasketService.cs`
- `Basket.Application/Baskets/IActiveBasketStore.cs`
- `Basket.Application/Baskets/IBasketHistoryRepository.cs`
- `Basket.Domain/Basket.cs`
- `Basket.Domain/BasketItem.cs`
- `Basket.Domain/BasketCheckoutSnapshot.cs`
- `Basket.Infrastructure/Redis/RedisActiveBasketStore.cs`
- `Basket.Infrastructure/Redis/InMemoryActiveBasketStore.cs`
- `Basket.Infrastructure/Persistence/BasketHistoryRepository.cs`

What happens:

1. Active basket reads/writes go to Redis.
2. If Redis is not configured, local in-memory fallback is used.
3. Checkout creates durable PostgreSQL snapshot records.

Why this split exists:

- Redis is good for temporary active baskets.
- PostgreSQL is good for durable checkout history.

## Ordering Deep Dive

Main request:

```text
POST /gateway/orders
```

Flow:

```text
Ocelot Gateway
  -> Ordering.Api OrderEndpoints
  -> Ordering.Application OrderService
  -> Ordering.Domain Order
  -> IOrderRepository
  -> OrderRepository
  -> OrderingDbContext
  -> ordering_db
  -> IOrderSubmittedPublisher
  -> MassTransit outbox
  -> RabbitMQ
```

Important files:

- `Ordering.Api/Orders/OrderEndpoints.cs`
- `Ordering.Application/Orders/OrderService.cs`
- `Ordering.Application/Orders/IOrderRepository.cs`
- `Ordering.Application/Orders/IOrderSubmittedPublisher.cs`
- `Ordering.Domain/Order.cs`
- `Ordering.Domain/OrderItem.cs`
- `Ordering.Infrastructure/Persistence/OrderRepository.cs`
- `Ordering.Infrastructure/Messaging/MassTransitOrderSubmittedPublisher.cs`

What happens:

1. Endpoint receives create order request.
2. `OrderService.CreateAsync` validates customer, address, currency, and items.
3. `Order` aggregate is created.
4. `OrderItem` rows are added.
5. Repository adds order to EF Core.
6. Publisher publishes `OrderSubmitted`.
7. EF outbox stores outgoing message in `OutboxMessage`.
8. MassTransit later delivers message to RabbitMQ.

Important detail:

The order starts as `Submitted`, not `Confirmed`. Confirmation is a later workflow result.

## Ordering Saga Deep Dive

The saga is a worker, not an HTTP API.

Flow:

```text
RabbitMQ event
  -> Saga Worker consumer
  -> OrderWorkflowService
  -> OrderWorkflowRepository
  -> ordering_saga_db
  -> WorkflowCommandPublisher
  -> RabbitMQ command/event
```

Important files:

- `OrderingSaga.Worker/Program.cs`
- `OrderingSaga.Worker/Messaging/OrderSubmittedConsumer.cs`
- `OrderingSaga.Worker/Messaging/InventoryReservedConsumer.cs`
- `OrderingSaga.Worker/Messaging/PaymentAuthorizedConsumer.cs`
- `OrderingSaga.Application/Workflows/OrderWorkflowService.cs`
- `OrderingSaga.Application/Workflows/IWorkflowCommandPublisher.cs`
- `OrderingSaga.Domain/OrderWorkflow.cs`
- `OrderingSaga.Domain/OrderWorkflowStatus.cs`
- `OrderingSaga.Infrastructure/Messaging/MassTransitWorkflowCommandPublisher.cs`
- `OrderingSaga.Infrastructure/Persistence/OrderWorkflowRepository.cs`

What happens:

1. Saga consumes `OrderSubmitted`.
2. Saga creates workflow state.
3. Saga publishes `ReserveInventory`.
4. Saga consumes `InventoryReserved`.
5. Saga publishes `AuthorizePayment`.
6. Saga consumes `PaymentAuthorized`.
7. Saga publishes `CreateShipment`.
8. Saga consumes `ShipmentCreated`.
9. Saga publishes `OrderConfirmed`.

Failure handling:

- Inventory failure cancels workflow.
- Payment failure releases inventory and cancels workflow.
- Shipment failure refunds payment, releases inventory, and cancels workflow.

Why saga exists:

The order workflow spans multiple services and cannot be one database transaction.

## Inventory Deep Dive

HTTP flow:

```text
PUT /gateway/inventory/items/{productId}
  -> InventoryEndpoints
  -> InventoryService.UpsertAsync
  -> InventoryRepository
  -> inventory_db
```

Message flow:

```text
RabbitMQ ReserveInventory
  -> ReserveInventoryConsumer
  -> InventoryService.ReserveAsync
  -> InventoryRepository
  -> inventory_items / stock_reservations
  -> InventoryReserved or InventoryReservationFailed
```

Important files:

- `Inventory.Api/Inventory/InventoryEndpoints.cs`
- `Inventory.Application/Inventory/InventoryService.cs`
- `Inventory.Application/Inventory/IInventoryRepository.cs`
- `Inventory.Domain/InventoryItem.cs`
- `Inventory.Domain/StockReservation.cs`
- `Inventory.Infrastructure/Messaging/ReserveInventoryConsumer.cs`
- `Inventory.Infrastructure/Messaging/ReleaseInventoryConsumer.cs`
- `Inventory.Infrastructure/Persistence/InventoryRepository.cs`

What happens during reservation:

1. Check existing reservations for the order.
2. If already reserved, return success without changing stock.
3. If already failed, return the previous failure.
4. Check available stock.
5. Reserve stock by increasing `reserved_quantity`.
6. Write `stock_reservations`.
7. Publish success or failure event.

Important pattern:

`stock_reservations` acts as domain-level idempotency for stock reservation.

## Payment Deep Dive

Message flow:

```text
RabbitMQ AuthorizePayment
  -> AuthorizePaymentConsumer
  -> PaymentService.AuthorizeAsync
  -> PaymentRepository
  -> payments / payment_transactions
  -> PaymentAuthorized or PaymentFailed
```

Refund flow:

```text
RabbitMQ RefundPayment
  -> RefundPaymentConsumer
  -> PaymentService.RefundAsync
  -> payment_transactions
```

Important files:

- `Payment.Application/Payments/PaymentService.cs`
- `Payment.Application/Payments/IPaymentRepository.cs`
- `Payment.Domain/Payment.cs`
- `Payment.Domain/PaymentTransaction.cs`
- `Payment.Infrastructure/Messaging/AuthorizePaymentConsumer.cs`
- `Payment.Infrastructure/Messaging/RefundPaymentConsumer.cs`
- `Payment.Infrastructure/Persistence/PaymentRepository.cs`

Current behavior:

- Payment authorization is mocked.
- Audit records are still stored as if a real provider existed.

Future work:

- Stripe, Iyzico, PayPal, or another real payment provider integration.

## Shipping Deep Dive

Message flow:

```text
RabbitMQ CreateShipment
  -> CreateShipmentConsumer
  -> ShipmentService.CreateAsync
  -> ShipmentRepository
  -> shipments
  -> ShipmentCreated or ShipmentFailed
```

Important files:

- `Shipping.Application/Shipments/ShipmentService.cs`
- `Shipping.Application/Shipments/IShipmentRepository.cs`
- `Shipping.Domain/Shipment.cs`
- `Shipping.Infrastructure/Messaging/CreateShipmentConsumer.cs`
- `Shipping.Infrastructure/Persistence/ShipmentRepository.cs`

What happens:

1. Create shipment record.
2. Generate tracking number.
3. Store address and customer/order references.
4. Publish shipment result.

Future work:

- Real cargo provider integration.
- Tracking status updates.

## Notification Deep Dive

Message flow:

```text
RabbitMQ event
  -> Notification consumer
  -> NotificationService
  -> NotificationRepository
  -> notification_db
  -> SignalR live publisher
  -> connected clients
```

Important files:

- `Notification.Api/Hubs/NotificationsHub.cs`
- `Notification.Api/Hubs/SignalRLiveNotificationPublisher.cs`
- `Notification.Api/Messaging/*.cs`
- `Notification.Application/Notifications/NotificationService.cs`
- `Notification.Domain/NotificationRecord.cs`
- `Notification.Infrastructure/Persistence/NotificationRepository.cs`

What happens:

1. Notification service consumes business events.
2. It creates a notification record.
3. It saves notification history.
4. It pushes realtime message through SignalR if client is connected.

Client group:

```text
customer:{customerId}
```

SignalR method:

```text
notificationReceived
```

## Identity Deep Dive

Main requests:

```text
POST /gateway/users
GET /gateway/users/{id}
```

Flow:

```text
Ocelot Gateway
  -> Identity.Api UserEndpoints
  -> Identity.Application UserService
  -> PasswordHashService
  -> UserRepository
  -> identity_db
```

Important files:

- `Identity.Api/Users/UserEndpoints.cs`
- `Identity.Application/Users/UserService.cs`
- `Identity.Application/Users/IUserRepository.cs`
- `Identity.Application/Users/IPasswordHashService.cs`
- `Identity.Domain/User.cs`
- `Identity.Domain/UserRole.cs`
- `Identity.Infrastructure/Security/PasswordHashService.cs`
- `Identity.Infrastructure/Persistence/UserRepository.cs`

What happens during registration:

1. Normalize email.
2. Validate email, display name, and password length.
3. Check duplicate user.
4. Hash password.
5. Add `Customer` role.
6. Save user and role.

Current limitation:

Identity does not issue JWT tokens yet.

## Gateway Deep Dive

Important files:

- `ApiGateway/Program.cs`
- `ApiGateway/ocelot.json`
- `ApiGateway/ocelot.Docker.json`

What it does:

1. Starts Ocelot.
2. Loads environment-specific routing config.
3. Provides gateway health endpoints.
4. Routes external `/gateway/...` paths to internal service endpoints.

Why two Ocelot files:

- Local file routes to `localhost` ports.
- Docker file routes to Compose service names like `catalog-api`.

## BuildingBlocks Deep Dive

### Contracts

Defines shared messages and common models.

Examples:

- `OrderSubmitted`
- `ReserveInventory`
- `PaymentAuthorized`
- `Result`
- `Error`

### EventBus

Defines RabbitMQ and MassTransit setup.

Key behavior:

- Reads RabbitMQ options.
- Supports `RabbitMq__ConnectionString`.
- Registers consumers.
- Adds EF outbox.

### Persistence

Defines PostgreSQL setup helpers.

Key behavior:

- Normalizes Neon-style PostgreSQL URLs.
- Registers EF Core DbContexts.

### Security

Defines auth-ready abstractions.

Key behavior:

- JWT bearer config can be enabled when Authority and Audience are provided.
- `ICurrentUser` abstraction exists.

### Localization

Defines Turkish/English error localization.

Key behavior:

- Reads request language.
- Maps error codes to localized messages.

### Observability

Defines OpenTelemetry setup.

Key behavior:

- Adds tracing and metrics foundations.
- Can export to console or OTLP endpoint.
