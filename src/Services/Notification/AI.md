---
id: service-notification
type: service
version: 1
status: active
tags:
- microservice
- notification
- signalr
related:
- database-notification
- api-notification-signalr
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Own notification history and real-time SignalR delivery.

# Responsibilities

- Consume order, payment, and shipment events.
- Store notification records.
- Publish live notifications to SignalR groups.

# Dependencies

- [[../../../docs/ai/03_DATABASES#NotificationDb]]
- [[../../../docs/ai/04_EVENTS#Message Contracts]]
- SignalR

# Database

See [[../../../docs/ai/03_DATABASES#NotificationDb]].

# APIs

See [[../../../docs/ai/05_APIS#Notification SignalR]].

# Events Published

None.

# Events Consumed

- [[../../../docs/ai/04_EVENTS#OrderSubmitted]]
- [[../../../docs/ai/04_EVENTS#PaymentAuthorized]]
- [[../../../docs/ai/04_EVENTS#PaymentFailed]]
- [[../../../docs/ai/04_EVENTS#ShipmentCreated]]
- [[../../../docs/ai/04_EVENTS#ShipmentFailed]]

# Important Classes

- `NotificationsHub`
- `NotificationService`
- `SignalRLiveNotificationPublisher`
- event consumer classes under `Messaging`
- `NotificationDbContext`

# Folder Structure

- `ECommerce.Notification.Api`
- `ECommerce.Notification.Application`
- `ECommerce.Notification.Domain`
- `ECommerce.Notification.Infrastructure`

# Configuration

- `ConnectionStrings__NotificationDb`
- `RabbitMq__ConnectionString`

# Design Decisions

Notification history is durable; live SignalR delivery is best-effort.
MassTransit receive endpoints use the `notification-` service prefix, giving Notification its own event subscription instead of competing with same-named Saga consumers.

SignalR connections require the shared `AuthenticatedUser` policy. Joining a customer group is not yet treated as proof of ownership; authenticated identity-to-local-customer mapping must be added before group membership is considered fully authorized.

# Future Improvements

- Add email/SMS/push channels.
- Add notification read state.
