# Data Model and Tables

## Database Strategy

The project uses database-per-service.

This means each service owns its own database and schema. Other services should not directly query those tables. If another service needs information, it should receive it through an API call or message.

Why:

- Clear ownership.
- Independent migrations.
- Smaller blast radius.
- Easier future scaling.
- Avoids hidden coupling through shared tables.

## Catalog Database

Database:

```text
catalog_db
```

Tables:

```text
brands
categories
category_translations
products
product_images
product_translations
```

### brands

Stores product brand metadata.

Example:

```text
Nike
Apple
Samsung
```

Relation:

```text
products.brand_id -> brands.id
```

### categories

Stores product categories.

Example:

```text
Shoes
Phones
Laptops
```

Relation:

```text
products.category_id -> categories.id
```

### category_translations

Stores category names in multiple languages.

Example:

```text
category_id = 1
language = tr
name = Ayakkabı

category_id = 1
language = en
name = Shoes
```

Why:

- The project is planned as Turkish and English.
- We do not hardcode display text in one language.

### products

Stores core product metadata.

Typical fields:

- id
- sku
- category_id
- brand_id
- price
- currency
- status
- created_at
- updated_at

### product_translations

Stores localized product name and description.

Relation:

```text
product_translations.product_id -> products.id
```

Example:

```text
language = tr
name = Kablosuz Kulaklık

language = en
name = Wireless Headphones
```

### product_images

Stores product image metadata.

Fields include:

- product_id
- public_id
- url
- secure_url
- width
- height
- format
- sort_order
- is_main

The actual image can live in Cloudinary or another object/image provider.

## Basket Database

Database:

```text
basket_db
```

Tables:

```text
basket_checkout_snapshots
basket_checkout_snapshot_items
```

### Active Basket Is Not Primarily PostgreSQL

The active basket is stored in Redis.

Why:

- Basket changes often.
- Basket state is temporary.
- Redis is fast for session-like data.

PostgreSQL stores checkout history after the customer checks out.

### basket_checkout_snapshots

Stores a basket snapshot at checkout time.

Meaning:

```text
This is what the customer had in the basket when checkout happened.
```

Typical fields:

- id
- customer_id
- currency
- total_amount
- checked_out_at

### basket_checkout_snapshot_items

Stores items that belonged to a checkout snapshot.

Relation:

```text
basket_checkout_snapshot_items.snapshot_id -> basket_checkout_snapshots.id
```

## Ordering Database

Database:

```text
ordering_db
```

Tables:

```text
orders
order_items
InboxState
OutboxMessage
OutboxState
```

### orders

Stores order header data.

Important fields:

- id
- customer_id
- currency
- status
- recipient_name
- address_line
- city
- country_code
- postal_code
- created_at
- updated_at

Status examples:

- Submitted
- Confirmed
- Cancelled

### order_items

Stores order lines.

Relation:

```text
order_items.order_id -> orders.id
```

Important fields:

- product_id
- product_name
- quantity
- unit_price
- total_price
- currency

### InboxState

MassTransit table used for inbox behavior.

Purpose:

- Helps prevent duplicate processing when the same message is delivered more than once.

### OutboxMessage

MassTransit table used for outbox messages.

Purpose:

- Stores messages that should be published after the local DB transaction succeeds.

### OutboxState

MassTransit table used to track outbox delivery state.

Purpose:

- Helps MassTransit know what has been delivered and what is pending.

## Ordering Saga Database

Database:

```text
ordering_saga_db
```

Tables:

```text
order_workflows
order_workflow_items
InboxState
OutboxMessage
OutboxState
```

### order_workflows

Stores one durable workflow record per order.

Meaning:

```text
Order X is currently at this workflow stage.
```

Possible workflow states include:

- Started
- InventoryReserved
- PaymentAuthorized
- ShipmentCreated
- Confirmed
- Cancelled

### order_workflow_items

Stores product lines related to a workflow.

Relation:

```text
order_workflow_items.order_workflow_id -> order_workflows.id
```

Why store this:

- The saga needs item data to reserve stock.
- It may need the same item data again for compensation.

## Inventory Database

Database:

```text
inventory_db
```

Tables:

```text
inventory_items
stock_reservations
InboxState
OutboxMessage
OutboxState
```

### inventory_items

Stores stock per product.

Important fields:

- product_id
- quantity_on_hand
- reserved_quantity
- updated_at

Available quantity is calculated as:

```text
quantity_on_hand - reserved_quantity
```

### stock_reservations

Stores reservation attempts per order and product.

Important fields:

- id
- order_id
- product_id
- quantity
- status
- failure_reason
- created_at

Why it matters:

- Prevents duplicate stock reservation for the same order.
- Acts as a practical inbox/idempotency record for inventory reservation.

Example:

```text
Order 123 reserves 2 units of Product ABC.
If the same message arrives again, Inventory sees an existing reservation and does not reserve stock twice.
```

## Payment Database

Database:

```text
payment_db
```

Tables:

```text
payments
payment_transactions
InboxState
OutboxMessage
OutboxState
```

### payments

Stores payment aggregate state.

Important fields:

- id
- order_id
- customer_id
- amount
- currency
- status
- authorized_at
- failed_at
- refunded_at

### payment_transactions

Stores audit records for payment actions.

Examples:

- authorize attempt
- authorization success
- authorization failure
- refund attempt

Why:

- Payment history matters for audit.
- Even in mock mode, the shape is ready for real providers later.

## Shipping Database

Database:

```text
shipping_db
```

Tables:

```text
shipments
InboxState
OutboxMessage
OutboxState
```

### shipments

Stores shipment records.

Important fields:

- id
- order_id
- customer_id
- recipient_name
- address_line
- city
- country_code
- postal_code
- tracking_number
- status
- created_at

Why:

- Shipping is a separate business capability.
- Shipping data is not stored directly inside the ordering service beyond the submitted address.

## Notification Database

Database:

```text
notification_db
```

Tables:

```text
notifications
InboxState
OutboxMessage
OutboxState
```

### notifications

Stores notification history.

Important fields:

- id
- customer_id
- type
- channel
- title
- message
- created_at

Why:

- Realtime SignalR messages are temporary.
- We still want durable notification history.

## Identity Database

Database:

```text
identity_db
```

Tables:

```text
users
user_roles
```

### users

Stores users.

Important fields:

- id
- email
- display_name
- password_hash
- status
- created_at
- updated_at

### user_roles

Stores user roles.

Relation:

```text
user_roles.user_id -> users.id
```

Example:

```text
Customer
Admin
```

Current status:

- Registration exists.
- Password hashing exists.
- Token login does not exist yet.

## Tables Created by MassTransit

Several service databases include:

```text
InboxState
OutboxMessage
OutboxState
```

These are not business tables. They are infrastructure tables.

They help with:

- safe message publishing
- duplicate message protection
- reliable event-driven workflows

Services with these tables:

- Ordering
- Ordering Saga
- Inventory
- Payment
- Shipping
- Notification

Catalog, Basket, and Identity currently do not need RabbitMQ consumers/publishers in the same way, so their database shapes are simpler.
