# Veri Modeli ve Tablolar

Bu projede her servis kendi database'ine sahiptir. Bir servisin tablosuna baska servis dogrudan baglanmaz. Servisler veri ihtiyacini API veya RabbitMQ mesajlariyla karsilar.

## Catalog Database

Database: `catalog_db`

Ana tablolar:

- `brands`: marka bilgisi.
- `categories`: kategori agaci.
- `category_translations`: kategori adlarinin TR/EN karsiliklari.
- `products`: urun ana bilgisi.
- `product_images`: urun gorselleri.
- `product_translations`: urun ad/aciklama TR/EN karsiliklari.

Catalog, urun bilgisinin sahibidir. Stok veya siparis bilgisini burada tutmayiz.

## Basket Database

Database: `basket_db`

Ana tablolar:

- `basket_checkout_snapshots`: checkout anindaki sepet ozeti.
- `basket_checkout_snapshot_items`: snapshot icindeki urun satirlari.

Aktif sepet Redis'tedir. PostgreSQL tarafinda checkout gecmisi tutulur. Bunun nedeni aktif sepetin cok sik degismesi, ama checkout aninin kalici gecmis olarak saklanmasi gerektigidir.

## Ordering Database

Database: `ordering_db`

Ana tablolar:

- `orders`: siparis ana kaydi.
- `order_items`: siparis urun satirlari.
- MassTransit outbox/inbox tablolari.

Ordering siparis kaydini saklar. Siparis olustugunda `OrderSubmitted` event'i yayinlanir.

## Inventory Database

Database: `inventory_db`

Ana tablolar:

- `inventory_items`: urun bazli stok miktari.
- `stock_reservations`: siparis icin ayrilan stok kayitlari.
- MassTransit outbox/inbox tablolari.

Inventory, stok bilgisinin tek sahibidir.

## Payment Database

Database: `payment_db`

Ana tablolar:

- `payments`: siparise ait odeme kaydi.
- `payment_transactions`: authorize/refund gibi hareketlerin gecmisi. Kimlikler Payment domain tarafindan uretilir ve EF Core bunlari yeni audit kaydi olarak ekler.
- MassTransit outbox/inbox tablolari.

Payment su an mock davranir; gercek provider daha sonra eklenebilir.

## Shipping Database

Database: `shipping_db`

Ana tablolar:

- `shipments`: siparis icin kargo kaydi.
- MassTransit outbox/inbox tablolari.

Shipping, tracking number ve shipment status bilgisini tutar. Basarili shipment kayitlarinda tracking number benzersizdir; provider veya adres reddiyle olusan basarisiz kayitlarda `tracking_number` `NULL` tutulur. Boylece birden fazla basarisiz shipment ayni unique indekste cakisma olusturmaz.

## Notification Database

Database: `notification_db`

Ana tablolar:

- `notification_records`: kullaniciya gonderilen bildirimlerin gecmisi.
- MassTransit inbox tablolari.

SignalR anlik bildirim verir; PostgreSQL ise gecmisi saklar.

## Identity Database

Database: `identity_db`

Ana tablolar:

- `users`: kullanici hesaplari.
- `user_roles`: kullanici rolleri.

Password plain text tutulmaz; hash olarak saklanir.

## Ordering Saga Database

Database: `ordering_saga_db`

Ana tablolar:

- `order_workflows`: siparis saga durum kaydi.
- `order_workflow_items`: workflow icindeki urun satirlari.
- MassTransit outbox/inbox tablolari.

Saga database'i uzun is akisinin hangi adimda oldugunu tutar.

## MassTransit Tablolari

MassTransit outbox/inbox icin ek tablolar olusturur.

Genel anlamlari:

- outbox tablolari: yayinlanacak mesajlari guvenli sekilde saklar.
- inbox tablolari: islenmis mesajlari takip eder.
- lock/state alanlari: ayni mesajin ayni anda birden fazla worker tarafindan islenmesini engellemeye yardim eder.

Bu tablolar uygulama is verisi degil, messaging guvenilirligi icin altyapi verisidir.
