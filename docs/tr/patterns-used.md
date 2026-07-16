# Kullanilan Patternler

Bu sayfa projede kullanilan mimari ve kod patternlerini anlatir.

## Clean Architecture Benzeri Katmanlama

Servisler `Api`, `Application`, `Domain`, `Infrastructure` seklinde ayrilir.

Amac:

- is kurallarini framework detaylarindan ayirmak
- test edilebilirligi artirmak
- database veya messaging detaylarini domain'den uzak tutmak

## Repository Pattern

Database erisimini interface arkasina alir.

Ornek:

- `IProductRepository`
- `IOrderRepository`
- `IInventoryRepository`
- `IPaymentRepository`

Application katmani repository interface'ini bilir; EF Core implementasyonu Infrastructure katmanindadir.

## Database Per Service

Her servis kendi database'ine sahiptir. Bu, mikroservislerde en onemli prensiplerden biridir.

Avantaj:

- servisler bagimsiz gelisir
- tablo paylasimi azalir
- deploy ve ownership daha net olur

Dezavantaj:

- raporlama ve cross-service query daha zor olur
- consistency icin event-driven yaklasim gerekir

## Outbox Pattern

Database'e yazma ve mesaj yayinlamayi guvenilir hale getirir.

Bu projede:

- Ordering siparis yazip `OrderSubmitted` yayinlarken kullanir.
- Messaging yapan servislerde MassTransit EF outbox altyapisi vardir.

## Inbox Pattern

Consumer'in ayni mesaji tekrar islemesini engeller.

Bu projede:

- Inventory, Payment, Shipping, Notification ve Saga consumer'lari MassTransit inbox state desteginden yararlanir.

## Saga Pattern

Birden fazla servise yayilan is akisini yonetir.

Bu projede:

- `OrderingSaga Worker` siparis surecini koordine eder.
- Stok, odeme, kargo ve compensation adimlarini yonetir.

## Compensation Pattern

Saga'da hata olursa onceki basarili adimlari geri almak icin kullanilir.

Ornek:

- Odeme basarili, kargo basarisiz: `RefundPayment`
- Stok ayrildi, odeme basarisiz: `ReleaseInventory`

## API Gateway Pattern

Client tek bir gateway ile konusur. Gateway istegi ilgili servise iletir.

Bu projede:

- `ECommerce.ApiGateway`
- Ocelot
- `ocelot.json`
- `ocelot.Docker.json`

## Event-Driven Architecture

Servisler birbirlerini direkt cagirmak yerine event ve command mesajlariyla haberlesir.

Bu projede siparis workflow'u bunun ana ornegidir.

## Localization Pattern

Hata mesajlari TR/EN destekleyecek sekilde merkezi localizer uzerinden yonetilir.

Kod dili Ingilizce kalir; kullaniciya donen mesajlar kulture gore degisebilir.

## Health Check Pattern

Servislerin ayakta olup olmadigini anlamak icin health endpoint'leri kullanilir.

Smoke test ve operasyon scriptleri bu endpoint'leri kontrol eder.
