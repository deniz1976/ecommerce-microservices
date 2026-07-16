# RabbitMQ Topology

## Important Note

The project does not manually declare every RabbitMQ exchange and queue in code. MassTransit creates topology from message contracts and consumers.

That means the exact runtime names should be inspected in CloudAMQP after the services start.

Still, we can document the expected logical topology.

## Core Idea

```text
Published message type -> RabbitMQ exchange -> bound consumer queues
```

Command or event contracts live in:

```text
src/BuildingBlocks/ECommerce.BuildingBlocks.Contracts
```

Consumers live in service infrastructure/API worker projects.

## Endpoint Naming

MassTransit configuration uses:

```csharp
registration.SetEndpointNameFormatter(
    new KebabCaseEndpointNameFormatter(endpointNamePrefix, includeNamespace: false));
```

Endpoint names are generated in kebab-case and prefixed by the owning service.

Examples:

```text
OrderingSaga OrderSubmittedConsumer -> ordering-saga-order-submitted
Notification OrderSubmittedConsumer -> notification-order-submitted
Inventory ReserveInventoryConsumer -> inventory-reserve-inventory
```

The prefix is required for publish/subscribe fan-out. Without it, same-named consumers in Saga and Notification would share one queue and compete for each event instead of both receiving a copy.

## Event Contracts

Events:

- `OrderSubmitted`
- `InventoryReserved`
- `InventoryReservationFailed`
- `PaymentAuthorized`
- `PaymentFailed`
- `ShipmentCreated`
- `ShipmentFailed`
- `OrderConfirmed`
- `OrderCancelled`

Meaning:

```text
Something already happened.
```

## Command Contracts

Commands:

- `ReserveInventory`
- `ReleaseInventory`
- `AuthorizePayment`
- `RefundPayment`
- `CreateShipment`

Meaning:

```text
Please do this action.
```

## Expected Consumer Mapping

### Ordering Saga Worker

Consumes:

- `OrderSubmitted`
- `InventoryReserved`
- `InventoryReservationFailed`
- `PaymentAuthorized`
- `PaymentFailed`
- `ShipmentCreated`
- `ShipmentFailed`

Publishes:

- `ReserveInventory`
- `AuthorizePayment`
- `CreateShipment`
- `ReleaseInventory`
- `RefundPayment`
- `OrderConfirmed`
- `OrderCancelled`

### Inventory API

Consumes:

- `ReserveInventory`
- `ReleaseInventory`

Publishes:

- `InventoryReserved`
- `InventoryReservationFailed`

### Payment API

Consumes:

- `AuthorizePayment`
- `RefundPayment`

Publishes:

- `PaymentAuthorized`
- `PaymentFailed`

### Shipping API

Consumes:

- `CreateShipment`

Publishes:

- `ShipmentCreated`
- `ShipmentFailed`

### Notification API

Consumes:

- `OrderSubmitted`
- `PaymentAuthorized`
- `PaymentFailed`
- `ShipmentCreated`
- `ShipmentFailed`

Stores:

- `notifications`

Pushes:

- SignalR message `notificationReceived`

## Why We Let MassTransit Manage Topology

Benefits:

- Less manual RabbitMQ setup.
- Queue and exchange names follow message/consumer types.
- Consumers can be added without changing publisher code.
- Local and production topology are generated consistently.
- EF inbox/outbox integration works naturally.

Tradeoff:

- You need to inspect RabbitMQ UI to see exact topology after runtime startup.
- Documentation is logical unless verified against the live broker.

## What To Inspect In CloudAMQP

After Docker runtime starts, check:

- Exchanges
- Queues
- Bindings
- Consumer count
- Ready messages
- Unacked messages
- Dead-lettered messages if configured later

Useful questions:

- Does each expected consumer queue exist?
- Are consumers connected?
- Are messages stuck in ready state?
- Are there repeated redeliveries?
- Are outbox tables draining?

## Exchange Type

MassTransit RabbitMQ publish topology normally creates exchanges for message types and binds queues to them. We are not manually choosing direct/topic/fanout in our application code.

Practical explanation:

```text
Publish event -> every interested consumer receives a copy through its queue.
Send command -> target command consumer receives the command.
```

## Future Improvements

- Document actual CloudAMQP topology screenshots.
- Add explicit retry policies.
- Add dead-letter strategy.
- Add an environment prefix if multiple deployments later share one RabbitMQ virtual host.
- Add message TTL decisions.
- Add monitoring for ready/unacked/dead-letter messages.
