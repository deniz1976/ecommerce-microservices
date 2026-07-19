---
id: ai-databases
type: database-catalog
version: 1
status: active
tags:
- database
- postgresql
- redis
related:
- ai-services
- decision-database-per-service
owners:
- backend
last_reviewed:
graph_ready: true
---

# Purpose

Describe database ownership, table groups, and runtime stores used by this repository.

# Database Catalog

## CatalogDb

- id: `database-catalog`
- env: `ConnectionStrings__CatalogDb`
- owner: [[02_SERVICES#Catalog]]
- tables: `brands`, `categories`, `category_translations`, `products`, `product_images`, `product_translations`

## BasketDb

- id: `database-basket`
- env: `ConnectionStrings__BasketDb`
- owner: [[02_SERVICES#Basket]]
- tables: `basket_checkout_snapshots`, `basket_checkout_snapshot_items`

## OrderingDb

- id: `database-ordering`
- env: `ConnectionStrings__OrderingDb`
- owner: [[02_SERVICES#Ordering]]
- tables: `orders`, `order_items`, `InboxState`, `OutboxMessage`, `OutboxState`

## OrderingSagaDb

- id: `database-ordering-saga`
- env: `ConnectionStrings__OrderingSagaDb`
- owner: [[02_SERVICES#OrderingSaga]]
- tables: `order_workflows`, `order_workflow_items`, `InboxState`, `OutboxMessage`, `OutboxState`

## InventoryDb

- id: `database-inventory`
- env: `ConnectionStrings__InventoryDb`
- owner: [[02_SERVICES#Inventory]]
- tables: `inventory_items`, `stock_reservations`, `InboxState`, `OutboxMessage`, `OutboxState`

## PaymentDb

- id: `database-payment`
- env: `ConnectionStrings__PaymentDb`
- owner: [[02_SERVICES#Payment]]
- tables: `payments`, `payment_transactions` (domain-generated identifiers configured as non-database-generated), `InboxState`, `OutboxMessage`, `OutboxState`

## ShippingDb

- id: `database-shipping`
- env: `ConnectionStrings__ShippingDb`
- owner: [[02_SERVICES#Shipping]]
- tables: `shipments`, `InboxState`, `OutboxMessage`, `OutboxState`
- `shipments.tracking_number`: nullable for failed shipments and unique when present; successful provider responses supply the value.

## NotificationDb

- id: `database-notification`
- env: `ConnectionStrings__NotificationDb`
- owner: [[02_SERVICES#Notification]]
- tables: `notifications`, `InboxState`, `OutboxMessage`, `OutboxState`

## IdentityDb

- id: `database-identity`
- env: `ConnectionStrings__IdentityDb`
- owner: [[02_SERVICES#Identity]]
- tables: `users`, `user_roles`
- `users`: local user profile records, password hashes for local registration, Auth0 mapping fields `external_provider` and `external_subject`, `onboarding_completed_at`, status, timestamps.
- `user_roles`: local role assignments per user.

## Redis

- id: `store-redis-basket`
- env: `Redis__ConnectionString`
- owner: [[02_SERVICES#Basket]]
- purpose: active basket state
- required: `true`; Basket validates Redis configuration at startup and has no in-memory fallback.

# Migration Ownership

Each service infrastructure project owns its EF Core migrations under `Persistence/Migrations`.

The Ordering, OrderingSaga, Inventory, Payment, Shipping, and Notification migration chains include `RemoveObsoleteOutboxBusName`. It synchronizes their MassTransit 8.5.1 model snapshots by removing the obsolete nullable `OutboxState.BusName` column and its `BusName, Created` index; domain tables and business data are unchanged.

Shipping migration `AllowNullTrackingNumberForFailedShipments` makes `shipments.tracking_number` nullable while retaining its unique index. Existing successful tracking numbers remain unchanged; failed shipments can store `NULL` without colliding with other failures.

# TODO

- Add column-level graph metadata for each table.
