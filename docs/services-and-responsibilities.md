# Services and Responsibilities

## API Gateway

Project:

```text
src/ApiGateways/ECommerce.ApiGateway
```

Technology:

- Ocelot

Responsibility:

- Exposes a single gateway URL.
- Routes client requests to backend services.
- Has health endpoints.
- Loads `ocelot.Docker.json` in Docker environment and `ocelot.json` locally.

Example routes:

- `/gateway/catalog/products`
- `/gateway/baskets/{everything}`
- `/gateway/orders`
- `/gateway/inventory/{everything}`
- `/gateway/users`
- `/gateway/hubs/notifications/{everything}`

## Catalog Service

Projects:

```text
src/Services/Catalog/ECommerce.Catalog.Api
src/Services/Catalog/ECommerce.Catalog.Application
src/Services/Catalog/ECommerce.Catalog.Domain
src/Services/Catalog/ECommerce.Catalog.Infrastructure
```

Database:

```text
catalog_db
```

Responsibility:

- Product metadata.
- Product translations.
- Product images.
- Categories.
- Brands.

HTTP endpoints:

- `GET /api/v1/products`
- `GET /api/v1/products/{id}`
- `POST /api/v1/products`
- `PUT /api/v1/products/{id}`

Important tables:

- `products`
- `product_translations`
- `product_images`
- `categories`
- `category_translations`
- `brands`

## Basket Service

Projects:

```text
src/Services/Basket/ECommerce.Basket.Api
src/Services/Basket/ECommerce.Basket.Application
src/Services/Basket/ECommerce.Basket.Domain
src/Services/Basket/ECommerce.Basket.Infrastructure
```

Database:

```text
basket_db
```

Redis:

```text
active basket state
```

Responsibility:

- Customer basket.
- Active basket item operations.
- Checkout snapshot history.

HTTP endpoints:

- `GET /api/v1/baskets/{customerId}`
- `PUT /api/v1/baskets/{customerId}/items`
- `DELETE /api/v1/baskets/{customerId}/items/{productId}`
- `DELETE /api/v1/baskets/{customerId}`
- `POST /api/v1/baskets/{customerId}/checkout`

Important tables:

- `basket_checkout_snapshots`
- `basket_checkout_snapshot_items`

Important Redis behavior:

- Uses `RedisActiveBasketStore` when Redis is configured.
- Uses `InMemoryActiveBasketStore` as local fallback.

## Ordering Service

Projects:

```text
src/Services/Ordering/ECommerce.Ordering.Api
src/Services/Ordering/ECommerce.Ordering.Application
src/Services/Ordering/ECommerce.Ordering.Domain
src/Services/Ordering/ECommerce.Ordering.Infrastructure
```

Database:

```text
ordering_db
```

Responsibility:

- Order creation.
- Order history.
- Order status.
- Publishes `OrderSubmitted` when an order is created.

HTTP endpoints:

- `POST /api/v1/orders`
- `GET /api/v1/orders/{id}`
- `GET /api/v1/orders/customer/{customerId}`

Important tables:

- `orders`
- `order_items`
- `InboxState`
- `OutboxMessage`
- `OutboxState`

Why it has outbox tables:

- Ordering publishes `OrderSubmitted`.
- The outbox keeps that event safe if RabbitMQ is temporarily unavailable.

## Ordering Saga Worker

Projects:

```text
src/Services/OrderingSaga/ECommerce.OrderingSaga.Worker
src/Services/OrderingSaga/ECommerce.OrderingSaga.Application
src/Services/OrderingSaga/ECommerce.OrderingSaga.Domain
src/Services/OrderingSaga/ECommerce.OrderingSaga.Infrastructure
```

Database:

```text
ordering_saga_db
```

Responsibility:

- Coordinates the long-running order workflow.
- Consumes order, inventory, payment, and shipment events.
- Publishes commands to the next service.
- Stores workflow state durably.

Important tables:

- `order_workflows`
- `order_workflow_items`
- `InboxState`
- `OutboxMessage`
- `OutboxState`

Main workflow:

```text
OrderSubmitted
  -> ReserveInventory
  -> InventoryReserved
  -> AuthorizePayment
  -> PaymentAuthorized
  -> CreateShipment
  -> ShipmentCreated
  -> OrderConfirmed
```

Failure workflow:

```text
InventoryReservationFailed
  -> OrderCancelled

PaymentFailed
  -> ReleaseInventory
  -> OrderCancelled

ShipmentFailed
  -> RefundPayment
  -> ReleaseInventory
  -> OrderCancelled
```

## Inventory Service

Projects:

```text
src/Services/Inventory/ECommerce.Inventory.Api
src/Services/Inventory/ECommerce.Inventory.Application
src/Services/Inventory/ECommerce.Inventory.Domain
src/Services/Inventory/ECommerce.Inventory.Infrastructure
```

Database:

```text
inventory_db
```

Responsibility:

- Stock records.
- Stock reservation.
- Stock release.

HTTP endpoints:

- `GET /api/v1/inventory/items/{productId}`
- `PUT /api/v1/inventory/items/{productId}`

Consumers:

- `ReserveInventoryConsumer`
- `ReleaseInventoryConsumer`

Publishes:

- `InventoryReserved`
- `InventoryReservationFailed`

Important tables:

- `inventory_items`
- `stock_reservations`
- `InboxState`
- `OutboxMessage`
- `OutboxState`

## Payment Service

Projects:

```text
src/Services/Payment/ECommerce.Payment.Api
src/Services/Payment/ECommerce.Payment.Application
src/Services/Payment/ECommerce.Payment.Domain
src/Services/Payment/ECommerce.Payment.Infrastructure
```

Database:

```text
payment_db
```

Responsibility:

- Payment authorization.
- Refund tracking.
- Payment audit history.

Consumers:

- `AuthorizePaymentConsumer`
- `RefundPaymentConsumer`

Publishes:

- `PaymentAuthorized`
- `PaymentFailed`

Important tables:

- `payments`
- `payment_transactions`
- `InboxState`
- `OutboxMessage`
- `OutboxState`

Current payment behavior:

- Mock authorization.
- No real payment gateway integration yet.

## Shipping Service

Projects:

```text
src/Services/Shipping/ECommerce.Shipping.Api
src/Services/Shipping/ECommerce.Shipping.Application
src/Services/Shipping/ECommerce.Shipping.Domain
src/Services/Shipping/ECommerce.Shipping.Infrastructure
```

Database:

```text
shipping_db
```

Responsibility:

- Shipment creation.
- Tracking number generation.
- Shipment status.

Consumers:

- `CreateShipmentConsumer`

Publishes:

- `ShipmentCreated`
- `ShipmentFailed`

Important tables:

- `shipments`
- `InboxState`
- `OutboxMessage`
- `OutboxState`

## Notification Service

Projects:

```text
src/Services/Notification/ECommerce.Notification.Api
src/Services/Notification/ECommerce.Notification.Application
src/Services/Notification/ECommerce.Notification.Domain
src/Services/Notification/ECommerce.Notification.Infrastructure
```

Database:

```text
notification_db
```

Realtime:

```text
SignalR hub at /hubs/notifications
```

Responsibility:

- Store notification history.
- Send realtime notifications to connected clients.

Consumers:

- `OrderSubmittedConsumer`
- `PaymentAuthorizedConsumer`
- `PaymentFailedConsumer`
- `ShipmentCreatedConsumer`
- `ShipmentFailedConsumer`

Important tables:

- `notifications`
- `InboxState`
- `OutboxMessage`
- `OutboxState`

## Identity Service

Projects:

```text
src/Services/Identity/ECommerce.Identity.Api
src/Services/Identity/ECommerce.Identity.Application
src/Services/Identity/ECommerce.Identity.Domain
src/Services/Identity/ECommerce.Identity.Infrastructure
```

Database:

```text
identity_db
```

Responsibility:

- User registration.
- User profile lookup.
- User role storage.

HTTP endpoints:

- `POST /api/v1/users`
- `GET /api/v1/users/{id}`

Important tables:

- `users`
- `user_roles`

Current identity status:

- Password hashing exists.
- User registration exists.
- Token/login flow is not implemented yet.
