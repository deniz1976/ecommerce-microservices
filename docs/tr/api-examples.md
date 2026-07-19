# API Ornekleri

Bu sayfa temel endpoint kullanimlarini gosterir. Portlar lokal Docker Compose varsayimina gore gateway uzerindendir.

Korumali ornekler gecerli bir Auth0 access token kullanir:

```powershell
$authenticatedHeaders = @{ Authorization = "Bearer $accessToken" }
```

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
  -Uri "http://localhost:5080/gateway/users" `
  -ContentType "application/json" `
  -Body $body
```

## Inventory Upsert

Bu istek `Admin` rolu veya `inventory:write` permission ister.

```powershell
$productId = "11111111-1111-1111-1111-111111111111"

$body = @{
  sku = "SKU-001"
  quantityOnHand = 20
} | ConvertTo-Json

Invoke-RestMethod `
  -Method Put `
  -Uri "http://localhost:5080/gateway/inventory/items/$productId" `
  -ContentType "application/json" `
  -Headers $authenticatedHeaders `
  -Body $body
```

## Inventory Get

```powershell
Invoke-RestMethod `
  -Method Get `
  -Uri "http://localhost:5080/gateway/inventory/items/$productId"
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
  -Uri "http://localhost:5080/gateway/orders" `
  -ContentType "application/json" `
  -Headers $authenticatedHeaders `
  -Body $body
```

## Basket Get

```powershell
$customerId = "22222222-2222-2222-2222-222222222222"

Invoke-RestMethod `
  -Method Get `
  -Uri "http://localhost:5080/gateway/baskets/$customerId" `
  -Headers $authenticatedHeaders
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
  -Method Put `
  -Uri "http://localhost:5080/gateway/baskets/$customerId/items" `
  -ContentType "application/json" `
  -Headers $authenticatedHeaders `
  -Body $body
```

## SignalR Notification Hub

Hub route:

```text
http://localhost:5080/gateway/hubs/notifications
```

SignalR baglantisi kurulurken Auth0 access token gonderilmelidir.

Client group mantigi:

```text
customer:{customerId:N}
```

Client method:

```text
notificationReceived
```
