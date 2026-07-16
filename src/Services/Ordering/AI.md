---
id: service-ordering
type: service
version: 1
status: active
tags:
- microservice
- ordering
- outbox
related:
- database-ordering
- event-order-submitted
- event-order-confirmed
- event-order-cancelled
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Own order records and order status.

# Responsibilities

- Create orders.
- Store order items and shipping address.
- Publish `OrderSubmitted`.
- Consume `OrderConfirmed` and `OrderCancelled` to update order status.

# Dependencies

- [[../../../docs/ai/03_DATABASES#OrderingDb]]
- [[../../../docs/ai/04_EVENTS#Message Contracts]]
- MassTransit EF outbox/inbox

# Database

See [[../../../docs/ai/03_DATABASES#OrderingDb]].

# APIs

See [[../../../docs/ai/05_APIS#Ordering API]].

# Events Published

- [[../../../docs/ai/04_EVENTS#OrderSubmitted]]

# Events Consumed

- [[../../../docs/ai/04_EVENTS#OrderConfirmed]]
- [[../../../docs/ai/04_EVENTS#OrderCancelled]]

# Important Classes

- `OrderEndpoints`
- `OrderService`
- `OrderStatusService`
- `MassTransitOrderSubmittedPublisher`
- `OrderConfirmedConsumer`
- `OrderCancelledConsumer`
- `OrderingDbContext`

# Folder Structure

- `ECommerce.Ordering.Api`
- `ECommerce.Ordering.Application`
- `ECommerce.Ordering.Domain`
- `ECommerce.Ordering.Infrastructure`

# Configuration

- `ConnectionStrings__OrderingDb`
- `RabbitMq__ConnectionString`

# Design Decisions

Order workflow orchestration is delegated to [[../OrderingSaga/AI#Purpose]].

# Future Improvements

- Add order status history.
- Add query/read model for workflow progress.
