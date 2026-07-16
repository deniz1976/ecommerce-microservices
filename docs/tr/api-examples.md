# API Ornekleri

Bu sayfa temel endpoint kullanimlarini gosterir. Portlar lokal Docker Compose varsayimina gore gateway uzerindendir.

## Gateway Health

```powershell
Invoke-WebRequest http://localhost:5080/health/live
```

## User Register

```powershell
$body = @{
  email = "customer@example.com"
  password = "StrongPassword123!"
  firstName = "Test"
  lastName = "Customer"
} | ConvertTo-Json

Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:5080/api/v1/users" `
  -ContentType "application/json" `
  -Body $body
```

## Inventory Upsert

```powershell
$productId = "11111111-1111-1111-1111-111111111111"

$body = @{
  sku = "SKU-001"
  quantityOnHand = 20
} | ConvertTo-Json

Invoke-RestMethod `
  -Method Put `
  -Uri "http://localhost:5080/api/v1/inventory/items/$productId" `
  -ContentType "application/json" `
  -Body $body
```

## Inventory Get

```powershell
Invoke-RestMethod `
  -Method Get `
  -Uri "http://localhost:5080/api/v1/inventory/items/$productId"
```

## Order Create

```powershell
$body = @{
  customerId = "22222222-2222-2222-2222-222222222222"
  currency = "TRY"
  shippingAddressLine1 = "Adres satiri 1"
  shippingCity = "Istanbul"
  shippingCountry = "TR"
  items = @(
    @{
      productId = $productId
      productName = "Sample Product"
      sku = "SKU-001"
      quantity = 1
      unitPrice = 100
    }
  )
} | ConvertTo-Json -Depth 5

Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:5080/api/v1/orders" `
  -ContentType "application/json" `
  -Body $body
```

## Basket Get

```powershell
$customerId = "22222222-2222-2222-2222-222222222222"

Invoke-RestMethod `
  -Method Get `
  -Uri "http://localhost:5080/api/v1/baskets/$customerId"
```

## Basket Add Item

```powershell
$body = @{
  productId = $productId
  productName = "Sample Product"
  sku = "SKU-001"
  quantity = 2
  unitPrice = 100
} | ConvertTo-Json

Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:5080/api/v1/baskets/$customerId/items" `
  -ContentType "application/json" `
  -Body $body
```

## SignalR Notification Hub

Hub route:

```text
/hubs/notifications
```

Client group mantigi:

```text
customer:{customerId:N}
```

Client method:

```text
notificationReceived
```
