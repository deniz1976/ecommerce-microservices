# Messaging, RabbitMQ, Inbox ve Outbox

Bu projede servisler arasi uzun is akislarinda RabbitMQ kullanilir. HTTP daha cok client ile servis arasinda veya anlik sorgularda tercih edilir. Siparis gibi birden fazla servisi ilgilendiren sureclerde messaging daha guvenilir ve esnektir.

## Neden Messaging?

Siparis olustugunda stok ayrilmali, odeme alinmali, kargo olusturulmali ve bildirim gonderilmelidir. Bunlarin hepsini tek HTTP request icinde yapmak kirilgan olur.

Messaging ile:

- servisler birbirinden daha gevsek bagli olur
- gecici hata olursa mesaj tekrar denenebilir
- her servis kendi hizinda calisir
- uzun is akislari daha takip edilebilir olur

## Event ve Command Farki

Command bir servisten is yapmasini ister.

Ornek:

```text
ReserveInventory
AuthorizePayment
CreateShipment
RefundPayment
ReleaseInventory
```

Event ise olmus bir seyi bildirir.

Ornek:

```text
OrderSubmitted
InventoryReserved
PaymentAuthorized
ShipmentCreated
OrderCancelled
```

## Outbox Pattern

Problem sudur: servis database'e siparis yazdi, ama tam event yayinlayacakken uygulama crash oldu. Bu durumda siparis database'de var ama diger servislerin haberi yok.

Outbox bu problemi azaltir.

Akis:

1. Servis kendi business verisini database'e yazar.
2. Ayni transaction icinde yayinlanacak mesaj outbox tablosuna yazilir.
3. Transaction commit olur.
4. MassTransit outbox kaydini okuyup RabbitMQ'ya yayinlar.

Bu projede Ordering `OrderSubmitted` yayinlarken outbox kullanir. Inventory, Payment, Shipping ve Saga gibi messaging yapan servislerde de MassTransit outbox/inbox altyapisi vardir.

Ortak kayit hem Bus Outbox hem Consumer Outbox davranisini etkinlestirir. Bus Outbox consumer disindaki scoped publish/send islemlerini veritabanina yazar. Consumer Outbox ise her receive endpoint'te inbox kaydini, business verisi degisikligini ve consumer'in uretecegi yeni mesajlari tek bir islem birimi olarak tamamlar.

Receive endpoint'ler gecici hatalari 100 ms, 500 ms ve 1 saniye sonra yeniden dener. Hata kaliciysa MassTransit mesaji ilgili endpoint'in `_error` queue'suna tasir. Consumer Outbox basarisiz denemenin yarim business sonucu commit etmesini veya devam mesajlarini iki kez yayinlamasini engeller.

## Inbox Pattern

RabbitMQ gibi sistemlerde ayni mesaj bazi durumlarda tekrar gelebilir. Consumer ayni mesaji iki kez islerse stok iki kez dusulebilir veya iki kez refund yapilabilir.

Inbox bu riski azaltir.

Akis:

1. Consumer mesaji alir.
2. Mesaj kimligi inbox tarafinda kontrol edilir.
3. Daha once islenmediyse islenir.
4. Islendigi kaydedilir.
5. Ayni mesaj tekrar gelirse duplicate islem engellenir.

## Ornek: Ordering Outbox

Client siparis olusturur.

1. `Ordering API` `orders` tablosuna siparisi yazar.
2. `order_items` tablosuna urun satirlarini yazar.
3. `OrderSubmitted` mesajini outbox'a kaydeder.
4. MassTransit mesaji RabbitMQ'ya yayinlar.
5. `OrderingSaga Worker` mesaji alir.

Burada siparis kaydi ile event yayinlama birbirinden kopmaz.

## Ornek: Inventory Inbox

Saga `ReserveInventory` command'i gonderir.

1. `Inventory API` command'i alir.
2. Inbox bu mesajin daha once islenip islenmedigine bakar.
3. Stok yeterliyse `stock_reservations` kaydi olusturulur.
4. `InventoryReserved` event'i yayinlanir.
5. Ayni command tekrar gelirse duplicate rezervasyon riski azalir.

## RabbitMQ Bilesenleri

Exchange:

Mesajin ilk geldigi yerdir. Mesaji ilgili queue'lara yonlendirir.

Queue:

Consumer'in okuyacagi mesajlarin bekledigi kuyruktur.

Consumer:

Queue'dan mesaj alip isleyen kod parcasidir.

Publisher:

Mesaji RabbitMQ'ya gonderen taraftir.

## Bu Projede Neden MassTransit?

MassTransit RabbitMQ ile calismayi kolaylastirir. Consumer registration, retry, outbox, inbox, serialization ve endpoint naming gibi konularda hazir altyapi saglar.

Bu projede receive endpoint adlari servis prefix'i ile kebab-case uretilir. Ornegin ayni `OrderSubmitted` event'ini dinleyen Saga `ordering-saga-order-submitted`, Notification ise `notification-order-submitted` queue'suna sahiptir. Boylece event iki servise de ayri kopya olarak ulasir; consumer'lar tek queue uzerinde yarisa girmez.
