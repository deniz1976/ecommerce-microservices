# RabbitMQ Topolojisi

Bu sayfa RabbitMQ tarafinda mesajlarin nasil aktigini anlatir.

## Temel Kavramlar

Exchange:

Mesajin ilk gittigi RabbitMQ noktasidir. Mesaj exchange'e gelir, exchange onu ilgili queue'lara yonlendirir.

Queue:

Mesajlarin consumer tarafindan okunana kadar bekledigi kuyruktur.

Consumer:

Queue'dan mesaj okuyup isleyen kod parcasidir.

Publisher:

Mesaj yayinlayan taraftir.

## Bu Projedeki Ana Mesajlar

Command'ler:

- `ReserveInventory`
- `ReleaseInventory`
- `AuthorizePayment`
- `RefundPayment`
- `CreateShipment`

Event'ler:

- `OrderSubmitted`
- `InventoryReserved`
- `InventoryReservationFailed`
- `PaymentAuthorized`
- `PaymentFailed`
- `ShipmentCreated`
- `ShipmentFailed`
- `OrderConfirmed`
- `OrderCancelled`

## Siparis Akisi

Ordering API:

- `OrderSubmitted` yayinlar.

OrderingSaga Worker:

- `OrderSubmitted` dinler.
- `ReserveInventory` gonderir.
- `InventoryReserved` dinler.
- `AuthorizePayment` gonderir.
- `PaymentAuthorized` dinler.
- `CreateShipment` gonderir.
- `ShipmentCreated` dinler.
- `OrderConfirmed` yayinlar.

Inventory API:

- `ReserveInventory` dinler.
- `ReleaseInventory` dinler.
- `InventoryReserved` veya `InventoryReservationFailed` yayinlar.

Payment API:

- `AuthorizePayment` dinler.
- `RefundPayment` dinler.
- `PaymentAuthorized` veya `PaymentFailed` yayinlar.

Shipping API:

- `CreateShipment` dinler.
- `ShipmentCreated` veya `ShipmentFailed` yayinlar.

Notification API:

- cesitli order/payment/shipment event'lerini dinler.
- notification record olusturur.
- SignalR ile client'a bildirir.

## Neden Queue'lar Servise Ait?

Her consumer kendi queue'sundan okur. Boylece Notification ve Saga ayni event'i ayri ayri alabilir. Bir event birden fazla servisi ilgilendiriyorsa her servis kendi queue'su uzerinden o event'i isler.

## Retry ve Hata Durumlari

MassTransit retry ve error queue davranislarini yonetebilir. Production'da dead-letter queue ve error queue takibi cok onemlidir.

Bakilmasi gerekenler:

- biriken queue var mi
- consumer calisiyor mu
- error queue'da mesaj var mi
- ayni mesaj surekli retry oluyor mu

## CloudAMQP

CloudAMQP managed RabbitMQ saglar. Uygulama `RabbitMq__ConnectionString` environment variable'i ile baglanir.

Gercek connection string dosyalara yazilmamalidir.
