# Diagrams

## System Context

```mermaid
flowchart LR
    Client["Client"] --> Gateway["Ocelot API Gateway"]
    Gateway --> Catalog["Catalog API"]
    Gateway --> Basket["Basket API"]
    Gateway --> Ordering["Ordering API"]
    Gateway --> Inventory["Inventory API"]
    Gateway --> Identity["Identity API"]
    Gateway --> Notification["Notification API / SignalR"]

    Basket --> Redis["Redis"]

    Catalog --> CatalogDb["catalog_db"]
    Basket --> BasketDb["basket_db"]
    Ordering --> OrderingDb["ordering_db"]
    Inventory --> InventoryDb["inventory_db"]
    Identity --> IdentityDb["identity_db"]
    Notification --> NotificationDb["notification_db"]

    Ordering --> Rabbit["RabbitMQ"]
    Rabbit --> Saga["Ordering Saga Worker"]
    Rabbit --> Inventory
    Rabbit --> Payment["Payment API"]
    Rabbit --> Shipping["Shipping API"]
    Rabbit --> Notification

    Saga --> SagaDb["ordering_saga_db"]
    Payment --> PaymentDb["payment_db"]
    Shipping --> ShippingDb["shipping_db"]
```

## Order Workflow

```mermaid
sequenceDiagram
    participant Client
    participant Gateway
    participant Ordering
    participant RabbitMQ
    participant Saga
    participant Inventory
    participant Payment
    participant Shipping
    participant Notification

    Client->>Gateway: POST /gateway/orders
    Gateway->>Ordering: POST /api/v1/orders
    Ordering->>Ordering: Save order and outbox message
    Ordering-->>RabbitMQ: OrderSubmitted
    RabbitMQ-->>Saga: OrderSubmitted
    Saga-->>RabbitMQ: ReserveInventory
    RabbitMQ-->>Inventory: ReserveInventory
    Inventory->>Inventory: Reserve stock
    Inventory-->>RabbitMQ: InventoryReserved
    RabbitMQ-->>Saga: InventoryReserved
    Saga-->>RabbitMQ: AuthorizePayment
    RabbitMQ-->>Payment: AuthorizePayment
    Payment->>Payment: Authorize payment
    Payment-->>RabbitMQ: PaymentAuthorized
    RabbitMQ-->>Saga: PaymentAuthorized
    Saga-->>RabbitMQ: CreateShipment
    RabbitMQ-->>Shipping: CreateShipment
    Shipping->>Shipping: Create shipment
    Shipping-->>RabbitMQ: ShipmentCreated
    RabbitMQ-->>Saga: ShipmentCreated
    Saga-->>RabbitMQ: OrderConfirmed
    RabbitMQ-->>Notification: Order and shipment events
    Notification->>Notification: Store notification and push SignalR
```

## Payment Failure Compensation

```mermaid
sequenceDiagram
    participant Saga
    participant RabbitMQ
    participant Inventory
    participant Payment
    participant Notification

    Saga-->>RabbitMQ: AuthorizePayment
    RabbitMQ-->>Payment: AuthorizePayment
    Payment-->>RabbitMQ: PaymentFailed
    RabbitMQ-->>Saga: PaymentFailed
    Saga-->>RabbitMQ: ReleaseInventory
    RabbitMQ-->>Inventory: ReleaseInventory
    Saga-->>RabbitMQ: OrderCancelled
    RabbitMQ-->>Notification: PaymentFailed / OrderCancelled
```

## Shipment Failure Compensation

```mermaid
sequenceDiagram
    participant Saga
    participant RabbitMQ
    participant Inventory
    participant Payment
    participant Shipping
    participant Notification

    Saga-->>RabbitMQ: CreateShipment
    RabbitMQ-->>Shipping: CreateShipment
    Shipping-->>RabbitMQ: ShipmentFailed
    RabbitMQ-->>Saga: ShipmentFailed
    Saga-->>RabbitMQ: RefundPayment
    RabbitMQ-->>Payment: RefundPayment
    Saga-->>RabbitMQ: ReleaseInventory
    RabbitMQ-->>Inventory: ReleaseInventory
    Saga-->>RabbitMQ: OrderCancelled
    RabbitMQ-->>Notification: ShipmentFailed / OrderCancelled
```

## Database Ownership

```mermaid
flowchart TB
    Catalog["Catalog Service"] --> CatalogDb["catalog_db"]
    Basket["Basket Service"] --> BasketDb["basket_db"]
    Ordering["Ordering Service"] --> OrderingDb["ordering_db"]
    Saga["Ordering Saga Worker"] --> SagaDb["ordering_saga_db"]
    Inventory["Inventory Service"] --> InventoryDb["inventory_db"]
    Payment["Payment Service"] --> PaymentDb["payment_db"]
    Shipping["Shipping Service"] --> ShippingDb["shipping_db"]
    Notification["Notification Service"] --> NotificationDb["notification_db"]
    Identity["Identity Service"] --> IdentityDb["identity_db"]
```

## Catalog Tables

```mermaid
erDiagram
    brands ||--o{ products : has
    categories ||--o{ products : has
    categories ||--o{ category_translations : has
    products ||--o{ product_translations : has
    products ||--o{ product_images : has
```

## Basket Tables

```mermaid
erDiagram
    basket_checkout_snapshots ||--o{ basket_checkout_snapshot_items : has
```

## Ordering Tables

```mermaid
erDiagram
    orders ||--o{ order_items : has
```

## Saga Tables

```mermaid
erDiagram
    order_workflows ||--o{ order_workflow_items : has
```

## Inventory Tables

```mermaid
erDiagram
    inventory_items ||--o{ stock_reservations : reserves
```

## Payment Tables

```mermaid
erDiagram
    payments ||--o{ payment_transactions : has
```

## Identity Tables

```mermaid
erDiagram
    users ||--o{ user_roles : has
```

## Inbox and Outbox Per Service

```mermaid
flowchart LR
    Service["Service code"] --> Db["Own service database"]
    Db --> Business["Business tables"]
    Db --> Inbox["InboxState"]
    Db --> OutboxMessage["OutboxMessage"]
    Db --> OutboxState["OutboxState"]
    OutboxMessage --> Rabbit["RabbitMQ"]
    Rabbit --> Consumer["Consumer service"]
```
