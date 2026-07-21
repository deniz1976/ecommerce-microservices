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
- Store notification records with durable source-event idempotency.
- Publish live notifications to SignalR groups.

# Dependencies

- [[../../../docs/ai/03_DATABASES#NotificationDb]]
- [[../../../docs/ai/04_EVENTS#Message Contracts]]
- SignalR
- Identity `GET /api/v1/auth/me` for trusted Auth0 `sub` to local customer `Guid` resolution.

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
- `IdentityClient__BaseUrl`
- `IdentityClient__TimeoutSeconds`

# Design Decisions

Notification history is durable; live SignalR delivery is best-effort.
MassTransit receive endpoints use the `notification-` service prefix, giving Notification its own event subscription instead of competing with same-named Saga consumers.

Every consumed business event copies its contract `MessageId` into `notifications.source_message_id`. The unique `(source_message_id, channel)` index prevents a replayed event from creating or broadcasting the same channel notification again after MassTransit's inbox duplicate-detection window expires. Existing notification rows receive migration-only source identifiers because their original event identifiers were not retained.

SignalR connections require the shared `AuthenticatedUser` policy. Joining or leaving a customer group additionally resolves the caller through Identity `/api/v1/auth/me` and permits only the matching local customer `Guid`; `Admin` or the narrow `customer:act` automation permission may act for another customer. Browser query-string tokens are forwarded only from the notification hub path, matching the JWT handler restriction. Identity resolution fails closed.

# Future Improvements

- Add email/SMS/push channels.
- Add notification read state.
- Add durable dispatch state if live channel delivery must become stronger than best-effort SignalR.
