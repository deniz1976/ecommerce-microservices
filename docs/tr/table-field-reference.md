# Tablo Alan Rehberi

Bu sayfa tablolardaki alanlarin genel olarak ne tuttugunu anlatir. Alan isimleri kodla uyumlu kalmasi icin Ingilizce yazilir.

## Catalog

`products`

- `Id`: urun kimligi.
- `Sku`: stok kodu.
- `Slug`: URL dostu urun adi.
- `Price`: urun fiyati.
- `Currency`: para birimi.
- `Status`: urun aktif/pasif durumu.
- `BrandId`: marka referansi.
- `CategoryId`: kategori referansi.

`product_translations`

- `ProductId`: urun referansi.
- `Culture`: `tr` veya `en`.
- `Name`: urun adi.
- `Description`: urun aciklamasi.

`product_images`

- `ProductId`: urun referansi.
- `Url`: gorsel adresi.
- `AltText`: gorsel alternatif metni.
- `SortOrder`: siralama.

## Basket

`basket_checkout_snapshots`

- `Id`: snapshot kimligi.
- `CustomerId`: musteri kimligi.
- `Currency`: para birimi.
- `TotalAmount`: checkout anindaki toplam tutar.
- `CreatedAtUtc`: olusturma zamani.

`basket_checkout_snapshot_items`

- `SnapshotId`: checkout snapshot referansi.
- `ProductId`: urun kimligi.
- `ProductName`: checkout anindaki urun adi.
- `Sku`: stok kodu.
- `Quantity`: adet.
- `UnitPrice`: birim fiyat.

## Ordering

`orders`

- `Id`: siparis kimligi.
- `CustomerId`: musteri kimligi.
- `Status`: siparis durumu.
- `Currency`: para birimi.
- `TotalAmount`: toplam tutar.
- `ShippingAddress...`: kargo adres alanlari.
- `CreatedAtUtc`: olusturma zamani.

`order_items`

- `OrderId`: siparis referansi.
- `ProductId`: urun kimligi.
- `ProductName`: siparis anindaki urun adi.
- `Sku`: stok kodu.
- `Quantity`: adet.
- `UnitPrice`: birim fiyat.

## Inventory

`inventory_items`

- `ProductId`: urun kimligi.
- `Sku`: stok kodu.
- `QuantityOnHand`: eldeki stok.
- `ReservedQuantity`: ayrilmis stok.

`stock_reservations`

- `OrderId`: siparis kimligi.
- `ProductId`: urun kimligi.
- `Quantity`: ayrilan adet.
- `Status`: rezervasyon durumu.

## Payment

`payments`

- `Id`: payment kimligi.
- `OrderId`: siparis kimligi.
- `CustomerId`: musteri kimligi.
- `Amount`: tutar.
- `Currency`: para birimi.
- `Status`: odeme durumu.

`payment_transactions`

- `PaymentId`: payment referansi.
- `Type`: authorize/refund gibi hareket tipi.
- `Amount`: hareket tutari.
- `Status`: hareket durumu.
- `CreatedAtUtc`: olusturma zamani.

## Shipping

`shipments`

- `Id`: shipment kimligi.
- `OrderId`: siparis kimligi.
- `CustomerId`: musteri kimligi.
- `TrackingNumber`: takip numarasi.
- `Status`: kargo durumu.
- `Address...`: kargo adresi.

## Notification

`notification_records`

- `Id`: notification kimligi.
- `CustomerId`: musteri kimligi.
- `Title`: bildirim basligi.
- `Message`: bildirim metni.
- `Channel`: SignalR/email/sms gibi kanal.
- `CreatedAtUtc`: olusturma zamani.

## Identity

`users`

- `Id`: kullanici kimligi.
- `Email`: email.
- `PasswordHash`: hash'lenmis password.
- `FirstName`: ad.
- `LastName`: soyad.
- `Status`: kullanici durumu.

`user_roles`

- `UserId`: kullanici referansi.
- `Role`: rol adi.

## Ordering Saga

`order_workflows`

- `OrderId`: siparis kimligi.
- `CustomerId`: musteri kimligi.
- `Status`: workflow durumu.
- `CurrentStep`: bulunulan adim.
- `FailureReason`: hata nedeni.

`order_workflow_items`

- `OrderWorkflowId`: workflow referansi.
- `ProductId`: urun kimligi.
- `Sku`: stok kodu.
- `Quantity`: adet.

## Outbox / Inbox Tablolari

Bu tablolar MassTransit tarafindan messaging guvenilirligi icin kullanilir.

Genel olarak:

- message id
- consumer id
- lock id
- processed time
- delivered time
- payload

gibi alanlar tutabilir.
