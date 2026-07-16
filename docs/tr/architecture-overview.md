# Mimari Genel Bakis

Bu proje e-commerce domain'i icin hazirlanmis .NET 10 mikroservis backend'idir. Ana hedef, servislerin birbirinden bagimsiz gelistirilebilmesi, her servisin kendi verisini sahiplenmesi ve uzun is akislarinin guvenilir messaging ile yonetilmesidir.

## Neden Mikroservis?

E-commerce sisteminde urun katalogu, sepet, siparis, stok, odeme, kargo, bildirim ve kimlik gibi farkli is alanlari vardir. Bunlari tek uygulamada toplamak ilk basta kolay gorunur, ama zamanla her degisiklik tum sistemi etkiler.

Mikroservis yaklasiminda:

- `Catalog` sadece urun katalogundan sorumludur.
- `Basket` aktif sepet ve sepet gecmisinden sorumludur.
- `Ordering` siparis kaydini yonetir.
- `Inventory` stok rezervasyonu yapar.
- `Payment` odeme surecini yonetir.
- `Shipping` kargo kaydi olusturur.
- `Notification` bildirimleri saklar ve SignalR ile canli iletir.
- `Identity` kullanici ve rol bilgisini tutar.
- `OrderingSaga Worker` uzun siparis is akisini koordine eder.

## Ana Mimari

```text
Client
  |
  v
API Gateway
  |
  +--> HTTP APIs
  |
  v
Services
  |
  +--> PostgreSQL databases
  +--> Redis
  +--> RabbitMQ
```

Client once `API Gateway` ile konusur. Gateway istegi ilgili servise yonlendirir. Servisler kendi database'lerine yazar. Servisler arasi uzun sureclerde RabbitMQ kullanilir.

## Katmanlar

Servisler genel olarak su katmanlara ayrilir:

- `Api`: HTTP endpoint'leri, request/response, middleware.
- `Application`: use case'ler, service siniflari, port/interface tanimlari.
- `Domain`: is kurallari ve entity'ler.
- `Infrastructure`: database, Redis, RabbitMQ, external provider implementasyonlari.

Bu ayrim sayesinde is kurallari framework detaylarindan uzak tutulur.

## Shared Building Blocks

`src/BuildingBlocks` altinda ortak altyapi projeleri vardir:

- `Contracts`: command, event, result ve error modelleri.
- `EventBus`: MassTransit ve RabbitMQ ayarlari.
- `Persistence`: PostgreSQL connection string normalizasyonu.
- `Security`: current user ve auth ayarlari.
- `Localization`: TR/EN hata mesaji altyapisi.
- `Observability`: OpenTelemetry hazirligi.

## Veri Sahipligi

Her servis kendi database'inin sahibidir. Ornegin Ordering servisi Inventory tablosuna dogrudan bakmaz. Stok gerekiyorsa RabbitMQ uzerinden `ReserveInventory` command'i gonderilir ve sonuc `InventoryReserved` veya `InventoryReservationFailed` event'i ile gelir.

Bu yaklasim daha fazla kod gerektirir, ama servis bagimsizligini korur.

## Senkron ve Asenkron Iletisim

Senkron iletisim HTTP ile yapilir. Ornegin client gateway uzerinden urunleri listelemek ister.

Asenkron iletisim RabbitMQ ile yapilir. Ornegin siparis olusunca:

1. Ordering `OrderSubmitted` event'i yayinlar.
2. Saga bu event'i alir.
3. Saga `ReserveInventory` command'i gonderir.
4. Inventory sonucu event olarak yayinlar.
5. Saga sonraki adima gecer.

## Guvenilir Mesajlasma

RabbitMQ kullanan servislerde outbox/inbox patternleri vardir. Outbox, database kaydi ile mesaj yayinlamayi guvenilir hale getirir. Inbox, ayni mesaj tekrar gelirse tekrar islemeyi engeller.

## Deployment Yaklasimi

Lokal gelistirmede Docker Compose kullanilir. Production ortamda servislerin container olarak calismasi, PostgreSQL/Redis/RabbitMQ gibi altyapi servislerinin managed olarak kullanilmasi daha dogru olur.
