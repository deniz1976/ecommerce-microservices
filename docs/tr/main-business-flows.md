# Ana Is Akislari

Bu sayfa projedeki temel is akislarini sade sekilde anlatir.

## Urun Listeleme

1. Client gateway'e istek atar.
2. Ocelot istegi `Catalog API` servisine yonlendirir.
3. Catalog `catalog_db` icinden urunleri okur.
4. Dil bilgisine gore TR veya EN translation secilir.
5. Sonuc client'a doner.

Bu akis senkrondur, RabbitMQ kullanmaz.

## Sepete Urun Ekleme

1. Client gateway uzerinden `Basket API` endpoint'ine gelir.
2. Basket aktif sepeti Redis'ten okur.
3. Urun sepete eklenir veya miktari artirilir.
4. Guncel sepet Redis'e yazilir.

Aktif sepet PostgreSQL yerine Redis'tedir, cunku hizli ve sik degisen bir veridir.

## Basket Checkout Snapshot

1. Musteri checkout adimina gelir.
2. Basket aktif sepeti Redis'ten okur.
3. Sepetin o anki hali PostgreSQL'e snapshot olarak yazilir.
4. Daha sonra sepet degisse bile checkout gecmisi kaybolmaz.

Bu henuz siparisin tamamlandigi anlamina gelmez; sadece sepet gecmisidir.

## Siparis Olusturma

1. Client `Ordering API` uzerinden siparis olusturur.
2. Ordering `orders` ve `order_items` tablolarina kayit atar.
3. `OrderSubmitted` event'i outbox'a yazilir.
4. MassTransit event'i RabbitMQ'ya yayinlar.
5. Saga workflow baslar.

## Siparis Saga Akisi

Basarili akis:

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

Anlami:

1. Siparis geldi.
2. Stok ayrildi.
3. Odeme onaylandi.
4. Kargo kaydi olusturuldu.
5. Siparis tamamlandi.

## Stok Basarisiz Olursa

```text
OrderSubmitted
  -> ReserveInventory
  -> InventoryReservationFailed
  -> OrderCancelled
```

Stok yoksa odeme veya kargo adimina gecilmez.

## Odeme Basarisiz Olursa

```text
InventoryReserved
  -> AuthorizePayment
  -> PaymentFailed
  -> ReleaseInventory
  -> OrderCancelled
```

Stok ayrildigi icin compensation olarak stok serbest birakilir.

## Kargo Basarisiz Olursa

```text
PaymentAuthorized
  -> CreateShipment
  -> ShipmentFailed
  -> RefundPayment
  -> ReleaseInventory
  -> OrderCancelled
```

Odeme basarili oldugu icin refund gerekir. Stok da serbest birakilir.

## Bildirim Akisi

Notification servisi bazi event'leri dinler:

- `OrderSubmitted`
- `PaymentAuthorized`
- `PaymentFailed`
- `ShipmentCreated`
- `ShipmentFailed`

Event geldiginde:

1. Notification record PostgreSQL'e yazilir.
2. SignalR ile ilgili customer group'una canli bildirim gonderilir.

## Kullanici Kaydi

1. Client `Identity API` uzerinden user register istegi atar.
2. Password hash'lenir.
3. User ve role bilgisi `identity_db` icine yazilir.

Login ve token uretimi henuz sonraki adimdir.
