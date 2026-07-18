---
id: service-shipping
type: service
version: 1
status: active
tags:
- microservice
- shipping
- shipment
related:
- database-shipping
- command-create-shipment
- event-shipment-created
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Own shipment creation and tracking number state.

# Responsibilities

- Consume shipment creation commands.
- Delegate carrier acceptance and tracking-number creation through `IShippingProvider`.
- Create shipment records.
- Publish shipment success/failure events.

# Dependencies

- [[../../../docs/ai/03_DATABASES#ShippingDb]]
- [[../../../docs/ai/04_EVENTS#CreateShipment]]

# Database

See [[../../../docs/ai/03_DATABASES#ShippingDb]].

# APIs

No public shipping HTTP API is currently documented. The service hosts health endpoints.

# Events Published

- [[../../../docs/ai/04_EVENTS#ShipmentCreated]]
- [[../../../docs/ai/04_EVENTS#ShipmentFailed]]

# Events Consumed

- [[../../../docs/ai/04_EVENTS#CreateShipment]]

# Important Classes

- `ShipmentService`
- `IShippingProvider`
- `MockShippingProvider`
- `CreateShipmentConsumer`
- `ShipmentRepository`
- `ShippingDbContext`

# Folder Structure

- `ECommerce.Shipping.Api`
- `ECommerce.Shipping.Application`
- `ECommerce.Shipping.Domain`
- `ECommerce.Shipping.Infrastructure`

# Configuration

- `ConnectionStrings__ShippingDb`
- `RabbitMq__ConnectionString`
- `ShippingProvider__RejectedPostalCodes__0` and subsequent indexed values

# Design Decisions

MassTransit receive endpoints use the `shipping-` service prefix so queue ownership remains explicit and collision-free.

Shipping is driven by saga commands rather than direct order database reads. Application logic depends on a provider abstraction; Infrastructure supplies the current configurable mock provider. See [[../../../docs/ai/09_DECISIONS#decision-shipping-provider-boundary]].

The mock provider rejects configured postal codes (`00000` by default). A rejection is persisted as `ShipmentStatus.Failed` and follows the normal `ShipmentFailed` event and saga compensation path.

Successful shipments require a unique tracking number. Failed shipments persist `NULL` for `tracking_number`, allowing multiple independent provider failures without colliding on the unique tracking-number index.

# Future Improvements

- Add carrier integration.
- Add shipment status updates.
