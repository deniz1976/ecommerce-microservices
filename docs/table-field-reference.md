# Table Field Reference

This document explains what the main tables are expected to store. It is not a full generated database schema, but it gives the practical meaning of the important fields.

## Catalog

### products

Purpose:

```text
Main product record.
```

Important fields:

- `id`: product identifier.
- `sku`: stock keeping unit, business product code.
- `category_id`: category reference.
- `brand_id`: brand reference.
- `price`: product price.
- `currency`: price currency such as USD or TRY.
- `status`: product lifecycle status.
- `created_at`: creation time.
- `updated_at`: last update time.

### product_translations

Purpose:

```text
Localized product name and description.
```

Important fields:

- `id`: translation identifier.
- `product_id`: product reference.
- `language`: language code such as `en` or `tr`.
- `name`: localized product name.
- `description`: localized product description.

### product_images

Purpose:

```text
Product image metadata.
```

Important fields:

- `id`: image identifier.
- `product_id`: product reference.
- `public_id`: provider-side image id.
- `url`: image URL.
- `secure_url`: HTTPS image URL.
- `width`: image width.
- `height`: image height.
- `format`: image format.
- `sort_order`: display order.
- `is_main`: whether this is the primary product image.

### categories

Purpose:

```text
Product category.
```

Important fields:

- `id`: category identifier.
- `parent_id`: optional parent category.
- `slug`: URL-friendly category name.
- `is_active`: whether category is active.

### category_translations

Purpose:

```text
Localized category name.
```

Important fields:

- `category_id`: category reference.
- `language`: language code.
- `name`: localized category name.

### brands

Purpose:

```text
Product brand.
```

Important fields:

- `id`: brand identifier.
- `name`: brand name.
- `slug`: URL-friendly brand name.
- `is_active`: whether brand is active.

## Basket

### basket_checkout_snapshots

Purpose:

```text
Durable snapshot of a basket at checkout time.
```

Important fields:

- `id`: snapshot identifier.
- `customer_id`: customer identifier.
- `currency`: basket currency.
- `total_amount`: total basket amount at checkout.
- `checked_out_at`: checkout time.

### basket_checkout_snapshot_items

Purpose:

```text
Items inside a basket checkout snapshot.
```

Important fields:

- `id`: item identifier.
- `snapshot_id`: checkout snapshot reference.
- `product_id`: product identifier.
- `product_name`: product name copied at checkout time.
- `quantity`: purchased quantity.
- `unit_price`: item unit price at checkout time.
- `total_price`: quantity multiplied by unit price.
- `currency`: item currency.

## Ordering

### orders

Purpose:

```text
Order header.
```

Important fields:

- `id`: order identifier.
- `customer_id`: customer that owns the order.
- `currency`: order currency.
- `status`: order status.
- `recipient_name`: shipping recipient.
- `address_line`: shipping street/address.
- `city`: shipping city.
- `country_code`: shipping country code.
- `postal_code`: shipping postal code.
- `created_at`: order creation time.
- `updated_at`: last order update time.

### order_items

Purpose:

```text
Products inside an order.
```

Important fields:

- `id`: order item identifier.
- `order_id`: order reference.
- `product_id`: product identifier.
- `product_name`: copied product name.
- `quantity`: ordered quantity.
- `unit_price`: price per unit.
- `total_price`: quantity multiplied by unit price.
- `currency`: item currency.

## Ordering Saga

### order_workflows

Purpose:

```text
Durable state of the distributed order workflow.
```

Important fields:

- `id`: workflow identifier.
- `order_id`: order being processed.
- `customer_id`: customer reference.
- `status`: current workflow status.
- `total_amount`: order total.
- `currency`: order currency.
- `recipient_name`: shipment recipient.
- `address_line`: shipment address.
- `city`: shipment city.
- `country_code`: shipment country.
- `postal_code`: shipment postal code.
- `created_at`: workflow creation time.
- `updated_at`: last workflow update time.

### order_workflow_items

Purpose:

```text
Order line data needed by the saga.
```

Important fields:

- `id`: workflow item identifier.
- `order_workflow_id`: workflow reference.
- `product_id`: product identifier.
- `product_name`: product name.
- `quantity`: quantity.
- `unit_price`: unit price.
- `currency`: currency.

## Inventory

### inventory_items

Purpose:

```text
Stock record per product.
```

Important fields:

- `product_id`: product identifier.
- `quantity_on_hand`: total stock.
- `reserved_quantity`: reserved stock.
- `updated_at`: last stock update time.

Derived value:

```text
available_quantity = quantity_on_hand - reserved_quantity
```

### stock_reservations

Purpose:

```text
Reservation record for order/product pairs.
```

Important fields:

- `id`: reservation identifier.
- `order_id`: order reference.
- `product_id`: product reference.
- `quantity`: reserved quantity.
- `status`: Reserved, Failed, or Released.
- `failure_reason`: why reservation failed, if it failed.
- `created_at`: reservation creation time.

Why important:

- Prevents duplicate stock reservation for repeated messages.

## Payment

### payments

Purpose:

```text
Payment state for an order.
```

Important fields:

- `id`: payment identifier.
- `order_id`: order reference.
- `customer_id`: customer reference.
- `amount`: payment amount.
- `currency`: payment currency.
- `status`: payment status.
- `authorized_at`: authorization time.
- `failed_at`: failure time.
- `refunded_at`: refund time.

### payment_transactions

Purpose:

```text
Audit trail of payment operations.
```

Important fields:

- `id`: transaction identifier.
- `payment_id`: payment reference.
- `type`: authorization/refund/etc.
- `amount`: transaction amount.
- `currency`: transaction currency.
- `provider_reference`: external provider reference when added later.
- `created_at`: transaction time.

## Shipping

### shipments

Purpose:

```text
Shipment record for an order.
```

Important fields:

- `id`: shipment identifier.
- `order_id`: order reference.
- `customer_id`: customer reference.
- `recipient_name`: recipient.
- `address_line`: address.
- `city`: city.
- `country_code`: country.
- `postal_code`: postal code.
- `tracking_number`: generated tracking number.
- `status`: shipment status.
- `created_at`: creation time.
- `updated_at`: update time.

## Notification

### notifications

Purpose:

```text
Notification history.
```

Important fields:

- `id`: notification identifier.
- `customer_id`: target customer.
- `type`: business notification type.
- `channel`: notification channel.
- `title`: notification title.
- `message`: notification body.
- `created_at`: creation time.

## Identity

### users

Purpose:

```text
Application user.
```

Important fields:

- `id`: user identifier.
- `email`: normalized email.
- `display_name`: display name.
- `password_hash`: hashed password.
- `status`: user status.
- `created_at`: creation time.
- `updated_at`: update time.

### user_roles

Purpose:

```text
Roles assigned to a user.
```

Important fields:

- `id`: role row identifier.
- `user_id`: user reference.
- `role`: role name.
- `created_at`: role assignment time.

## MassTransit Infrastructure Tables

### InboxState

Purpose:

```text
Tracks consumed messages for inbox/idempotency behavior.
```

Why:

- A message can be delivered more than once.
- Inbox state helps avoid duplicate processing.

### OutboxMessage

Purpose:

```text
Stores outgoing messages before delivery to RabbitMQ.
```

Why:

- Prevents losing events when DB save succeeds but broker publish fails.

### OutboxState

Purpose:

```text
Tracks outbox delivery state.
```

Why:

- MassTransit needs delivery coordination and state tracking.
