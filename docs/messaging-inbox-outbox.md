# Messaging, RabbitMQ, Inbox, and Outbox

## Why Messaging Exists

Some operations are too large to happen inside one HTTP request and one database transaction.

Example:

```text
Create order
Reserve stock
Authorize payment
Create shipment
Notify customer
```

These steps belong to different services. A single database transaction cannot safely cover all of them. Messaging lets each service do its own work and inform the rest of the system.

## RabbitMQ

RabbitMQ is the message broker.

In simple words:

```text
Services put messages into RabbitMQ.
Other services receive those messages when they are ready.
```

We use CloudAMQP as the managed RabbitMQ provider.

## MassTransit

MassTransit is the .NET library we use on top of RabbitMQ.

It gives us:

- typed messages
- consumer registration
- endpoint naming
- RabbitMQ configuration
- EF Core inbox/outbox integration
- retries and durable message handling patterns

Important shared file:

```text
src/BuildingBlocks/ECommerce.BuildingBlocks.EventBus/DependencyInjection.cs
```

## Message Types

The project uses two conceptual message types:

```text
Event
Command
```

### Event

An event says:

```text
Something already happened.
```

Examples:

- `OrderSubmitted`
- `InventoryReserved`
- `InventoryReservationFailed`
- `PaymentAuthorized`
- `PaymentFailed`
- `ShipmentCreated`
- `ShipmentFailed`
- `OrderConfirmed`
- `OrderCancelled`

### Command

A command says:

```text
Please do this.
```

Examples:

- `ReserveInventory`
- `ReleaseInventory`
- `AuthorizePayment`
- `RefundPayment`
- `CreateShipment`

## Main Message Chain

Happy path:

```text
Ordering API
  publishes OrderSubmitted

Ordering Saga Worker
  consumes OrderSubmitted
  publishes ReserveInventory

Inventory API
  consumes ReserveInventory
  publishes InventoryReserved

Ordering Saga Worker
  consumes InventoryReserved
  publishes AuthorizePayment

Payment API
  consumes AuthorizePayment
  publishes PaymentAuthorized

Ordering Saga Worker
  consumes PaymentAuthorized
  publishes CreateShipment

Shipping API
  consumes CreateShipment
  publishes ShipmentCreated

Ordering Saga Worker
  consumes ShipmentCreated
  publishes OrderConfirmed

Notification API
  consumes several events
  stores and sends notifications
```

## RabbitMQ Exchanges and Queues

MassTransit creates RabbitMQ topology based on message types and consumers.

In RabbitMQ terms:

- An exchange receives a published message.
- A queue stores messages for a consumer.
- A binding connects an exchange to a queue.

MassTransit usually creates message exchanges for published message types and receive endpoints for consumers.

Endpoint naming:

```text
KebabCaseEndpointNameFormatter(servicePrefix, includeNamespace: false)
```

That means consumer endpoints use kebab-case names prefixed by their owning service.

Example:

```text
OrderingSaga OrderSubmittedConsumer
```

becomes a receive endpoint similar to:

```text
ordering-saga-order-submitted
```

Notification's consumer for the same event uses `notification-order-submitted`. Separate queues are essential: a published event is copied to both service subscriptions instead of being delivered to only one competing consumer.

Exact names may include MassTransit conventions, but the important idea is:

```text
Message type exchange -> consumer queue
```

## Exchange Type

MassTransit with RabbitMQ commonly uses RabbitMQ exchanges for publish/subscribe routing. For normal event publishing, the topology behaves like type-based publish/subscribe.

You do not manually define direct/topic/fanout exchanges in our code right now. MassTransit manages the topology.

Why we chose this:

- Less boilerplate.
- Strong typed message contracts.
- Consumers can be added later without changing publishers.
- Good default for event-driven microservices.

## Outbox Pattern

Outbox solves this problem:

```text
What if the service saves data to PostgreSQL but crashes before publishing the event?
```

Example:

```text
Ordering creates an order.
Ordering must publish OrderSubmitted.
```

Without outbox:

```text
1. Save order to DB
2. Publish event to RabbitMQ

If step 1 succeeds and step 2 fails, the order exists but the workflow never starts.
```

With outbox:

```text
1. Save order to DB
2. Save outgoing event to OutboxMessage in the same DB transaction
3. MassTransit later publishes the event to RabbitMQ
```

In our project:

```text
OrderingDbContext + MassTransit EF Outbox
```

creates:

```text
OutboxMessage
OutboxState
```

So Ordering can safely store the order and the outgoing `OrderSubmitted` message.

## Inbox Pattern

Inbox solves this problem:

```text
What if the same message arrives twice?
```

Message brokers can redeliver messages. That is normal in distributed systems.

Example:

```text
Inventory receives ReserveInventory for Order 123.
If it receives the same message twice, it must not reserve stock twice.
```

Inbox/idempotency makes processing safe.

MassTransit EF integration creates:

```text
InboxState
```

Also, Inventory has domain-level protection:

```text
stock_reservations
```

Inventory checks if the order already has a reservation.

If yes:

```text
Do not reserve stock again.
Return the previous result.
```

## Where Outbox Is Applied

Outbox is registered through:

```text
registration.AddPostgresEntityFrameworkOutbox<TDbContext>()
```

Used in:

- Ordering API
- Ordering Saga Worker
- Inventory API
- Payment API
- Shipping API
- Notification API

## Where Inbox Is Applied

MassTransit EF tables include `InboxState` in consumer service databases.

Used by:

- Ordering Saga Worker
- Inventory API
- Payment API
- Shipping API
- Notification API

## Why Inbox and Outbox Are Inside Each Service Database

We do not create a central inbox/outbox service.

Correct approach:

```text
Ordering Service
  ordering_db
    orders
    OutboxMessage
    InboxState

Inventory Service
  inventory_db
    inventory_items
    stock_reservations
    OutboxMessage
    InboxState
```

Why:

- The message state belongs to that service's transaction.
- The service can keep its consistency boundary.
- No distributed transaction is needed.
- No central bottleneck.
- No central service that every microservice depends on.

## Example from This Project

### Ordering Outbox

File:

```text
src/Services/Ordering/ECommerce.Ordering.Application/Orders/OrderService.cs
```

Flow:

```text
OrderService.CreateAsync
  creates Order
  repository.Add(order)
  publisher.PublishAsync(order)
  repository.SaveChangesAsync()
```

Because Ordering has EF outbox, the publish is captured and stored safely with the database work.

### Inventory Inbox and Idempotency

File:

```text
src/Services/Inventory/ECommerce.Inventory.Application/Inventory/InventoryService.cs
```

Flow:

```text
ReserveAsync
  checks existing reservations for order
  if Reserved already exists -> success without reserving again
  if Failed already exists -> failure without retrying business logic
  otherwise reserves stock and records stock_reservations
```

This protects Inventory from duplicate reservation commands.

## Current Messaging Limitations

Current implementation is good for a first production-style foundation, but improvements remain:

- Explicit retry policies per consumer.
- Dead-letter queue strategy.
- Environment-specific queue prefixes if multiple deployments share one RabbitMQ virtual host.
- More integration tests with RabbitMQ test container.
- More explicit idempotency records for every command type.
- Better saga timeout handling.
