# Main Business Flows

## Product Browsing Flow

```text
Client
  -> API Gateway
  -> Catalog API
  -> catalog_db
```

Example:

```text
GET /gateway/catalog/products
```

Gateway forwards to:

```text
GET /api/v1/products
```

Catalog reads:

- products
- translations
- images
- categories
- brands

## Basket Flow

```text
Client
  -> API Gateway
  -> Basket API
  -> Redis
```

When customer adds an item:

```text
PUT /gateway/baskets/{customerId}/items
```

Basket stores the active basket in Redis.

Why Redis:

- Active basket is temporary.
- It changes often.
- It should be fast.

When customer checks out:

```text
POST /gateway/baskets/{customerId}/checkout
```

Basket stores a durable snapshot in PostgreSQL:

```text
basket_checkout_snapshots
basket_checkout_snapshot_items
```

## Order Creation Flow

```text
Client
  -> API Gateway
  -> Ordering API
  -> ordering_db
  -> RabbitMQ
```

Client calls:

```text
POST /gateway/orders
```

Ordering does:

```text
1. Validate order request
2. Create Order aggregate
3. Add OrderItems
4. Save order data
5. Publish OrderSubmitted through outbox
```

Important:

The order is created with status:

```text
Submitted
```

It is not fully confirmed yet. Confirmation happens after inventory, payment, and shipping succeed.

## Full Happy Path Order Workflow

```text
Client
  -> Ordering API
  -> OrderSubmitted
  -> Saga Worker
  -> ReserveInventory
  -> Inventory API
  -> InventoryReserved
  -> Saga Worker
  -> AuthorizePayment
  -> Payment API
  -> PaymentAuthorized
  -> Saga Worker
  -> CreateShipment
  -> Shipping API
  -> ShipmentCreated
  -> Saga Worker
  -> OrderConfirmed
  -> Notification API
```

Step by step:

1. Ordering creates order.
2. Ordering publishes `OrderSubmitted`.
3. Saga Worker consumes `OrderSubmitted`.
4. Saga Worker creates `order_workflows` record.
5. Saga Worker publishes `ReserveInventory`.
6. Inventory reserves stock.
7. Inventory publishes `InventoryReserved`.
8. Saga Worker publishes `AuthorizePayment`.
9. Payment authorizes payment.
10. Payment publishes `PaymentAuthorized`.
11. Saga Worker publishes `CreateShipment`.
12. Shipping creates shipment.
13. Shipping publishes `ShipmentCreated`.
14. Saga Worker marks workflow as complete and publishes `OrderConfirmed`.
15. Notification service can notify the customer.

## Inventory Failure Flow

```text
ReserveInventory
  -> InventoryReservationFailed
  -> Saga Worker
  -> OrderCancelled
```

Reason:

- Stock is not enough.
- Product does not exist in inventory.

Result:

- Payment is not attempted.
- Shipment is not created.
- Order workflow is cancelled.

## Payment Failure Flow

```text
AuthorizePayment
  -> PaymentFailed
  -> Saga Worker
  -> ReleaseInventory
  -> OrderCancelled
```

Reason:

- Payment authorization fails.

Compensation:

- Inventory stock reservation must be released.

## Shipment Failure Flow

```text
CreateShipment
  -> ShipmentFailed
  -> Saga Worker
  -> RefundPayment
  -> ReleaseInventory
  -> OrderCancelled
```

Reason:

- Shipping cannot create a shipment.

Compensation:

- Refund payment.
- Release inventory.
- Cancel workflow.

## Notification Flow

Notification service consumes events such as:

- `OrderSubmitted`
- `PaymentAuthorized`
- `PaymentFailed`
- `ShipmentCreated`
- `ShipmentFailed`

Then it:

```text
1. creates notification record in notification_db
2. sends realtime message through SignalR
```

SignalR hub:

```text
/hubs/notifications
```

Client group format:

```text
customer:{customerId}
```

Client method:

```text
notificationReceived
```

## Identity Flow

Current identity flow:

```text
Client
  -> API Gateway
  -> Identity API
  -> identity_db
```

Register:

```text
POST /gateway/users
```

Lookup:

```text
GET /gateway/users/{id}
```

Current status:

- Password is hashed.
- User role is stored.
- Login/token endpoint is not implemented yet.
