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
registration.SetKebabCaseEndpointNameFormatter();
```

So endpoint names are generated in kebab-case.

Examples:

```text
OrderSubmittedConsumer -> order-submitted
ReserveInventoryConsumer -> reserve-inventory
PaymentAuthorizedConsumer -> payment-authorized
```

The exact queue names can include MassTransit conventions, but this is the naming direction.

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
- Add queue naming conventions per environment.
- Add message TTL decisions.
- Add monitoring for ready/unacked/dead-letter messages.
