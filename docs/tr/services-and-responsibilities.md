# Servisler ve Sorumluluklari

Bu sayfa her servisin ne yaptigini ve ne yapmamasi gerektigini anlatir.

## Catalog API

Urun katalogundan sorumludur.

Tutar:

- product
- category
- brand
- product image
- product translation
- category translation

Yapar:

- urun olusturma
- urun guncelleme
- urun listeleme
- TR/EN metin destegi

Yapmaz:

- stok tutmaz
- fiyat tahsilati yapmaz
- siparis olusturmaz

## Basket API

Musterinin aktif sepetini ve checkout snapshot gecmisini yonetir.

Aktif sepet Redis'te tutulur. Checkout yapildiginda sepetin o anki hali PostgreSQL'e snapshot olarak yazilir.

Yapar:

- sepete urun ekleme
- sepetten urun silme
- sepet goruntuleme
- checkout snapshot kaydetme

Yapmaz:

- odeme almaz
- stok ayirmaz
- siparis workflow'unu yonetmez

## Ordering API

Siparis kaydini olusturur ve siparis durumunu tutar.

Yapar:

- order olusturma
- order item kaydetme
- shipping address saklama
- `OrderSubmitted` event'i yayinlama

Yapmaz:

- stok dusmez
- odeme cekmez
- kargo kaydi olusturmaz

## Inventory API

Stok bilgisinden ve stok rezervasyonundan sorumludur.

Yapar:

- inventory item upsert
- stok rezervasyonu
- rezervasyon iptali
- `InventoryReserved` veya `InventoryReservationFailed` event'i yayinlama

## Payment API

Odeme surecinden sorumludur. Su an mock odeme davranisi vardir; gercek provider henuz yoktur.

Yapar:

- payment authorization
- refund
- payment transaction audit
- `PaymentAuthorized` veya `PaymentFailed` event'i yayinlama

## Shipping API

Kargo kaydini olusturur.

Yapar:

- shipment olusturma
- tracking number uretme
- `ShipmentCreated` veya `ShipmentFailed` event'i yayinlama

## Notification API

Bildirim gecmisini tutar ve SignalR ile client'a canli bildirim gonderir.

Yapar:

- event consumer'lari ile bildirim olusturma
- notification history saklama
- SignalR hub uzerinden canli mesaj yayinlama

## Identity API

Kullanici ve rol bilgisini yonetir.

Yapar:

- user register
- password hash kaydetme
- user getirme

Henuz yapmaz:

- login
- JWT token uretimi
- refresh token

## OrderingSaga Worker

Siparisin uzun is akisini koordine eder.

Yapar:

- `OrderSubmitted` event'ini dinler
- `ReserveInventory` command'i gonderir
- stok basariliysa `AuthorizePayment` gonderir
- odeme basariliysa `CreateShipment` gonderir
- tum adimlar basariliysa `OrderConfirmed` yayinlar
- hata olursa compensation mesajlari yayinlar

## API Gateway

Client icin tek giris noktasidir.

Yapar:

- route yonlendirme
- servis endpoint'lerini tek port altinda toplama
- health route'lari
- SignalR route'u

Bu projede gateway Ocelot ile kuruldu.
