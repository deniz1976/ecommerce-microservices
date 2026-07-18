---
id: service-ordering-saga
type: service
version: 1
status: active
tags:
- worker
- saga
- ordering
related:
- database-ordering-saga
- event-order-submitted
- decision-event-driven-order-workflow
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Coordinate the long-running order workflow across inventory, payment, shipping, and ordering.

# Responsibilities

- Consume `OrderSubmitted`.
- Send inventory, payment, and shipping commands.
- Persist workflow state.
- Publish `OrderConfirmed` or `OrderCancelled`.
- Publish compensation commands when failures occur.

# Dependencies

- [[../../../docs/ai/03_DATABASES#OrderingSagaDb]]
- [[../../../docs/ai/04_EVENTS#Order Workflow]]
- MassTransit

# Database

See [[../../../docs/ai/03_DATABASES#OrderingSagaDb]].

# APIs

None. This is a worker process.

# Events Published

- [[../../../docs/ai/04_EVENTS#ReserveInventory]]
- [[../../../docs/ai/04_EVENTS#AuthorizePayment]]
- [[../../../docs/ai/04_EVENTS#CreateShipment]]
- [[../../../docs/ai/04_EVENTS#ReleaseInventory]]
- [[../../../docs/ai/04_EVENTS#RefundPayment]]
- [[../../../docs/ai/04_EVENTS#OrderConfirmed]]
- [[../../../docs/ai/04_EVENTS#OrderCancelled]]

# Events Consumed

- [[../../../docs/ai/04_EVENTS#OrderSubmitted]]
- [[../../../docs/ai/04_EVENTS#InventoryReserved]]
- [[../../../docs/ai/04_EVENTS#InventoryReservationFailed]]
- [[../../../docs/ai/04_EVENTS#PaymentAuthorized]]
- [[../../../docs/ai/04_EVENTS#PaymentFailed]]
- [[../../../docs/ai/04_EVENTS#ShipmentCreated]]
- [[../../../docs/ai/04_EVENTS#ShipmentFailed]]

# Important Classes

- `OrderWorkflowService`
- `MassTransitWorkflowCommandPublisher`
- worker consumer classes under `Messaging`
- `OrderingSagaDbContext`
- `OrderWorkflow`

# Folder Structure

- `ECommerce.OrderingSaga.Worker`
- `ECommerce.OrderingSaga.Application`
- `ECommerce.OrderingSaga.Domain`
- `ECommerce.OrderingSaga.Infrastructure`

# Configuration

- `ConnectionStrings__OrderingSagaDb`
- `RabbitMq__ConnectionString`

# Design Decisions

MassTransit receive endpoints use the `ordering-saga-` service prefix. This gives Saga an independent subscription when Notification consumes the same published event type.

The shared EF consumer outbox makes each consumed workflow transition atomic with its inbox record and outgoing commands/events. Shipping failure compensation (`RefundPayment`, `ReleaseInventory`, and `OrderCancelled`) is therefore persisted as one consumer processing unit and receives bounded transient retries.

Saga orchestration is separated from Ordering API. See [[../../../docs/ai/09_DECISIONS#decision-event-driven-order-workflow]].

`tools/ECommerce.RuntimeChecks` verifies the success path, inventory cancellation, payment cancellation with inventory release, and shipping cancellation with payment refund plus inventory release.

# Future Improvements

- Add timeout handling.
- Add explicit workflow diagnostics API or read model.
