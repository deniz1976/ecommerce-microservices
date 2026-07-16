# API Examples

This document shows example requests through the API Gateway. Gateway examples use:

```text
http://localhost:5080
```

## Health Checks

Gateway live check:

```powershell
Invoke-WebRequest http://localhost:5080/health/live
```

Service readiness through gateway:

```powershell
Invoke-WebRequest http://localhost:5080/gateway/health/catalog
Invoke-WebRequest http://localhost:5080/gateway/health/basket
Invoke-WebRequest http://localhost:5080/gateway/health/ordering
Invoke-WebRequest http://localhost:5080/gateway/health/inventory
Invoke-WebRequest http://localhost:5080/gateway/health/payment
Invoke-WebRequest http://localhost:5080/gateway/health/shipping
Invoke-WebRequest http://localhost:5080/gateway/health/notification
Invoke-WebRequest http://localhost:5080/gateway/health/identity
```

## Identity

Register user:

```powershell
$body = @{
    email = "customer@example.com"
    displayName = "Test Customer"
    password = "Password123!"
} | ConvertTo-Json

Invoke-RestMethod `
    -Method Post `
    -Uri http://localhost:5080/gateway/users `
    -ContentType "application/json" `
    -Body $body
```

Example response shape:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "email": "customer@example.com",
  "displayName": "Test Customer",
  "roles": ["Customer"]
}
```

Get user:

```powershell
Invoke-RestMethod http://localhost:5080/gateway/users/{userId}
```

## Catalog

Search products:

```powershell
Invoke-RestMethod http://localhost:5080/gateway/catalog/products
```

Create product:

```powershell
$categoryId = [Guid]::NewGuid()
$brandId = [Guid]::NewGuid()

$body = @{
    sku = "SKU-001"
    categoryId = $categoryId
    brandId = $brandId
    price = 99.90
    currency = "USD"
    status = 1
    translations = @(
        @{
            language = "en"
            name = "Wireless Headphones"
            description = "Bluetooth headphones"
        },
        @{
            language = "tr"
            name = "Kablosuz Kulaklık"
            description = "Bluetooth kulaklık"
        }
    )
    images = @()
} | ConvertTo-Json -Depth 20

Invoke-RestMethod `
    -Method Post `
    -Uri http://localhost:5080/gateway/catalog/products `
    -ContentType "application/json" `
    -Headers @{ "Accept-Language" = "en" } `
    -Body $body
```

Note:

The current Catalog create flow expects existing category and brand identifiers. Seed/admin tooling is a future improvement.

## Basket

Get basket:

```powershell
Invoke-RestMethod http://localhost:5080/gateway/baskets/{customerId}
```

Add basket item:

```powershell
$body = @{
    productId = "{productId}"
    productName = "Wireless Headphones"
    quantity = 1
    unitPrice = 99.90
    currency = "USD"
} | ConvertTo-Json

Invoke-RestMethod `
    -Method Put `
    -Uri http://localhost:5080/gateway/baskets/{customerId}/items `
    -ContentType "application/json" `
    -Body $body
```

Checkout basket:

```powershell
Invoke-RestMethod `
    -Method Post `
    -Uri http://localhost:5080/gateway/baskets/{customerId}/checkout
```

## Inventory

Seed or update stock:

```powershell
$body = @{
    quantityOnHand = 25
} | ConvertTo-Json

Invoke-RestMethod `
    -Method Put `
    -Uri http://localhost:5080/gateway/inventory/items/{productId} `
    -ContentType "application/json" `
    -Body $body
```

Get stock:

```powershell
Invoke-RestMethod http://localhost:5080/gateway/inventory/items/{productId}
```

Example response:

```json
{
  "productId": "00000000-0000-0000-0000-000000000000",
  "quantityOnHand": 25,
  "reservedQuantity": 0,
  "availableQuantity": 25,
  "updatedAt": "2026-07-03T00:00:00+00:00"
}
```

## Ordering

Create order:

```powershell
$body = @{
    customerId = "{customerId}"
    currency = "USD"
    recipientName = "Test Customer"
    addressLine = "Integration Avenue 1"
    city = "Istanbul"
    countryCode = "TR"
    postalCode = "34000"
    items = @(
        @{
            productId = "{productId}"
            productName = "Wireless Headphones"
            quantity = 1
            unitPrice = 99.90
            currency = "USD"
        }
    )
} | ConvertTo-Json -Depth 20

Invoke-RestMethod `
    -Method Post `
    -Uri http://localhost:5080/gateway/orders `
    -ContentType "application/json" `
    -Body $body
```

Get order:

```powershell
Invoke-RestMethod http://localhost:5080/gateway/orders/{orderId}
```

Get customer orders:

```powershell
Invoke-RestMethod http://localhost:5080/gateway/orders/customer/{customerId}
```

## Notifications

SignalR hub:

```text
http://localhost:5080/gateway/hubs/notifications
```

Client group:

```text
customer:{customerId}
```

Client method:

```text
notificationReceived
```

## Smoke Test Equivalent

The manual version of `smoke-test.ps1` is:

```text
1. Check gateway health.
2. Check service health through gateway.
3. Register a user.
4. Create inventory stock for a random product id.
5. Create an order for that product.
```

The script automates exactly that kind of small runtime confidence check.
