---
id: ai-events
type: event-catalog
version: 1
status: active
tags:
- messaging
- masstransit
- rabbitmq
related:
- ai-services
- ai-databases
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Catalog command and event contracts in `ECommerce.BuildingBlocks.Contracts`.

# Broker

- id: `broker-rabbitmq`
- provider: CloudAMQP-compatible RabbitMQ
- env: `RabbitMq__ConnectionString`
- library: MassTransit 8.5.1
- outbox: [[03_DATABASES#OrderingDb]], [[03_DATABASES#InventoryDb]], [[03_DATABASES#PaymentDb]], [[03_DATABASES#ShippingDb]], [[03_DATABASES#NotificationDb]], [[03_DATABASES#OrderingSagaDb]]
- delivery guarantees: the shared registration enables the EF bus outbox and applies the EF consumer outbox to every generated receive endpoint. Consumer processing uses inbox duplicate detection and retries transient faults after 100 ms, 500 ms, and 1 second before the endpoint `_error` queue.
- business idempotency: Notification persists each consumed contract `MessageId` and enforces uniqueness per delivery channel, so an event replay remains idempotent after the transport inbox window expires. Other workflow services additionally guard repeated work through order-scoped state and unique constraints.
- monitoring: every outbox-owning runtime polls its local `OutboxMessage` table and exports pending-count, oldest-age, and poll-error OpenTelemetry instruments. Trusted runtime verification also compares RabbitMQ `_error` and `_skipped` queue message totals before and after a scenario through the CloudAMQP-enabled management HTTP API.
- consumer queues: kebab-case names are prefixed by the owning service (`ordering`, `ordering-saga`, `inventory`, `payment`, `shipping`, or `notification`). Event consumers in different services therefore receive independent copies instead of competing on a shared queue.

# Message Contracts

## OrderSubmitted

- id: `event-order-submitted`
- type: event
- producer: [[02_SERVICES#Ordering]]
- consumers: [[02_SERVICES#OrderingSaga]], [[02_SERVICES#Notification]]

## ReserveInventory

- id: `command-reserve-inventory`
- type: command
- producer: [[02_SERVICES#OrderingSaga]]
- consumer: [[02_SERVICES#Inventory]]

## InventoryReserved

- id: `event-inventory-reserved`
- type: event
- producer: [[02_SERVICES#Inventory]]
- consumer: [[02_SERVICES#OrderingSaga]]

## InventoryReservationFailed

- id: `event-inventory-reservation-failed`
- type: event
- producer: [[02_SERVICES#Inventory]]
- consumer: [[02_SERVICES#OrderingSaga]]

## AuthorizePayment

- id: `command-authorize-payment`
- type: command
- producer: [[02_SERVICES#OrderingSaga]]
- consumer: [[02_SERVICES#Payment]]

## PaymentAuthorized

- id: `event-payment-authorized`
- type: event
- producer: [[02_SERVICES#Payment]]
- consumers: [[02_SERVICES#OrderingSaga]], [[02_SERVICES#Notification]]

## PaymentFailed

- id: `event-payment-failed`
- type: event
- producer: [[02_SERVICES#Payment]]
- consumers: [[02_SERVICES#OrderingSaga]], [[02_SERVICES#Notification]]

## CreateShipment

- id: `command-create-shipment`
- type: command
- producer: [[02_SERVICES#OrderingSaga]]
- consumer: [[02_SERVICES#Shipping]]

## ShipmentCreated

- id: `event-shipment-created`
- type: event
- producer: [[02_SERVICES#Shipping]]
- consumers: [[02_SERVICES#OrderingSaga]], [[02_SERVICES#Notification]]

## ShipmentFailed

- id: `event-shipment-failed`
- type: event
- producer: [[02_SERVICES#Shipping]]
- consumers: [[02_SERVICES#OrderingSaga]], [[02_SERVICES#Notification]]

## ReleaseInventory

- id: `command-release-inventory`
- type: command
- producer: [[02_SERVICES#OrderingSaga]]
- consumer: [[02_SERVICES#Inventory]]

## RefundPayment

- id: `command-refund-payment`
- type: command
- producer: [[02_SERVICES#OrderingSaga]]
- consumer: [[02_SERVICES#Payment]]

## OrderConfirmed

- id: `event-order-confirmed`
- type: event
- producer: [[02_SERVICES#OrderingSaga]]
- consumer: [[02_SERVICES#Ordering]]

## OrderCancelled

- id: `event-order-cancelled`
- type: event
- producer: [[02_SERVICES#OrderingSaga]]
- consumer: [[02_SERVICES#Ordering]]

# Order Workflow

`OrderSubmitted -> ReserveInventory -> InventoryReserved -> AuthorizePayment -> PaymentAuthorized -> CreateShipment -> ShipmentCreated -> OrderConfirmed`.

Failure paths:

- inventory failure: `InventoryReservationFailed -> OrderCancelled`; no compensation is required because stock was not reserved.
- payment failure: `PaymentFailed -> ReleaseInventory -> OrderCancelled`; the stock reservation reaches `Released`.
- shipment failure: `ShipmentFailed -> RefundPayment + ReleaseInventory + OrderCancelled`.

The runtime verification tool exercises the success, inventory-failure, payment-failure, and shipping-failure paths. Shipping failure uses a valid address with postal code `00000`, which the configured mock shipping provider rejects after Ordering validation; this reaches the real saga compensation path without bypassing production validation.
