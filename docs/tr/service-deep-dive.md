# Servis Detaylari

Bu sayfa servislerin ic yapisini biraz daha detayli anlatir.

## Catalog

Katmanlar:

- `ECommerce.Catalog.Api`
- `ECommerce.Catalog.Application`
- `ECommerce.Catalog.Domain`
- `ECommerce.Catalog.Infrastructure`

Domain entity'leri:

- `Product`
- `Category`
- `Brand`
- `ProductImage`
- `ProductTranslation`
- `CategoryTranslation`

Catalog cok dilli urun bilgisi icin translation tablolarini kullanir.

## Basket

Katmanlar:

- `ECommerce.Basket.Api`
- `ECommerce.Basket.Application`
- `ECommerce.Basket.Domain`
- `ECommerce.Basket.Infrastructure`

Ana fikir:

- aktif sepet Redis'te
- checkout snapshot PostgreSQL'de
- Redis yoksa development icin in-memory fallback

Domain:

- `Basket`
- `BasketItem`
- `BasketCheckoutSnapshot`
- `BasketCheckoutSnapshotItem`

## Ordering

Katmanlar:

- `ECommerce.Ordering.Api`
- `ECommerce.Ordering.Application`
- `ECommerce.Ordering.Domain`
- `ECommerce.Ordering.Infrastructure`

Domain:

- `Order`
- `OrderItem`
- `OrderStatus`

Ordering siparisi kaydeder ve `OrderSubmitted` event'i yayinlar. Workflow'u kendisi yonetmez; bu is Saga tarafindadir.

## Inventory

Katmanlar:

- `ECommerce.Inventory.Api`
- `ECommerce.Inventory.Application`
- `ECommerce.Inventory.Domain`
- `ECommerce.Inventory.Infrastructure`

Domain:

- `InventoryItem`
- `StockReservation`
- `StockReservationStatus`

Consumer'lar:

- `ReserveInventoryConsumer`
- `ReleaseInventoryConsumer`

Inventory stok bilgisinin tek sahibidir.

## Payment

Katmanlar:

- `ECommerce.Payment.Api`
- `ECommerce.Payment.Application`
- `ECommerce.Payment.Domain`
- `ECommerce.Payment.Infrastructure`

Domain:

- `Payment`
- `PaymentTransaction`
- `PaymentStatus`
- `PaymentTransactionType`

Consumer'lar:

- `AuthorizePaymentConsumer`
- `RefundPaymentConsumer`

Su an mock odeme davranisi vardir.

## Shipping

Katmanlar:

- `ECommerce.Shipping.Api`
- `ECommerce.Shipping.Application`
- `ECommerce.Shipping.Domain`
- `ECommerce.Shipping.Infrastructure`

Domain:

- `Shipment`
- `ShipmentStatus`

Consumer:

- `CreateShipmentConsumer`

Shipping shipment kaydi ve tracking number uretiminden sorumludur.

## Notification

Katmanlar:

- `ECommerce.Notification.Api`
- `ECommerce.Notification.Application`
- `ECommerce.Notification.Domain`
- `ECommerce.Notification.Infrastructure`

Domain:

- `NotificationRecord`
- `NotificationChannel`

SignalR:

- Hub: `/hubs/notifications`
- Client method: `notificationReceived`
- Group format: `customer:{customerId:N}`

Notification hem kalici bildirim gecmisi tutar hem de canli bildirim gonderir.

## Identity

Katmanlar:

- `ECommerce.Identity.Api`
- `ECommerce.Identity.Application`
- `ECommerce.Identity.Domain`
- `ECommerce.Identity.Infrastructure`

Domain:

- `User`
- `UserRole`
- `UserStatus`

Mevcut:

- register
- user get
- password hashing

Eksik:

- login
- JWT
- refresh token

## OrderingSaga Worker

Katmanlar:

- `ECommerce.OrderingSaga.Worker`
- `ECommerce.OrderingSaga.Application`
- `ECommerce.OrderingSaga.Domain`
- `ECommerce.OrderingSaga.Infrastructure`

Domain:

- `OrderWorkflow`
- `OrderWorkflowItem`
- `OrderWorkflowStatus`

Worker HTTP API degildir. Arka planda RabbitMQ mesajlarini dinler ve workflow'u yonetir.

## API Gateway

Proje:

- `ECommerce.ApiGateway`

Teknoloji:

- Ocelot

Dosyalar:

- `ocelot.json`
- `ocelot.Docker.json`

Gateway client icin tek giris noktasi saglar.
