---
id: service-inventory
type: service
version: 1
status: active
tags:
- microservice
- inventory
- stock
related:
- database-inventory
- command-reserve-inventory
- event-inventory-reserved
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Own stock quantities and stock reservations.

# Responsibilities

- Upsert inventory items.
- Reserve stock for orders.
- Release reserved stock during compensation.
- Publish reservation success/failure events.

# Dependencies

- [[../../../docs/ai/03_DATABASES#InventoryDb]]
- [[../../../docs/ai/04_EVENTS#ReserveInventory]]
- [[../../../docs/ai/04_EVENTS#ReleaseInventory]]

# Database

See [[../../../docs/ai/03_DATABASES#InventoryDb]].

# APIs

See [[../../../docs/ai/05_APIS#Inventory API]].

Inventory item reads are public. Inventory item upsert requires the shared `InventoryWrite` authorization policy, which accepts either the `Admin` role or the narrow Auth0 `inventory:write` M2M permission.

# Events Published

- [[../../../docs/ai/04_EVENTS#InventoryReserved]]
- [[../../../docs/ai/04_EVENTS#InventoryReservationFailed]]

# Events Consumed

- [[../../../docs/ai/04_EVENTS#ReserveInventory]]
- [[../../../docs/ai/04_EVENTS#ReleaseInventory]]

# Important Classes

- `InventoryEndpoints`
- `InventoryService`
- `ReserveInventoryConsumer`
- `ReleaseInventoryConsumer`
- `InventoryRepository`
- `InventoryDbContext`

# Folder Structure

- `ECommerce.Inventory.Api`
- `ECommerce.Inventory.Application`
- `ECommerce.Inventory.Domain`
- `ECommerce.Inventory.Infrastructure`

# Configuration

- `ConnectionStrings__InventoryDb`
- `RabbitMq__ConnectionString`
- `Auth__RoleClaimType`
- Auth0 API permission: `inventory:write` for the trusted runtime-check M2M client only.

# Design Decisions

Inventory does not read orders directly; it reacts to commands.

# Future Improvements

- Add stock audit trail.
- Add concurrency-focused integration tests.
