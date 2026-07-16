# ECommerce Mikroservis Dokumantasyonu

Bu klasor, projeyi Turkce ve sade bir dille anlatir. Kod, servis, tablo, endpoint, command ve event isimleri Ingilizce tutulur; aciklamalar Turkcedir. Bunun sebebi projenin kodlama dilinin Ingilizce olmasi, ama ekip ve ogrenme surecinde Turkce anlatima da ihtiyac duymamizdir.

## Okuma Sirasi

1. [Mimari Genel Bakis](./architecture-overview.md)
2. [Servisler ve Sorumluluklari](./services-and-responsibilities.md)
3. [Veri Modeli ve Tablolar](./data-model-and-tables.md)
4. [Messaging, RabbitMQ, Inbox ve Outbox](./messaging-inbox-outbox.md)
5. [Ana Is Akislari](./main-business-flows.md)
6. [Scriptler ve Lokal Komutlar](./scripts-and-local-commands.md)
7. [Runtime ve Operasyon](./runtime-and-operations.md)
8. [API Ornekleri](./api-examples.md)
9. [Diyagramlar](./diagrams.md)
10. [RabbitMQ Topolojisi](./rabbitmq-topology.md)
11. [Sifirdan Lokal Kurulum](./local-setup-from-zero.md)
12. [Servis Detaylari](./service-deep-dive.md)
13. [Tablo Alan Rehberi](./table-field-reference.md)
14. [Sozluk](./glossary.md)
15. [Kullanilan Patternler](./patterns-used.md)
16. [Sorun Giderme](./troubleshooting.md)
17. [Mimari Karar Kaydi](./architecture-decision-log.md)
18. [Eksikler ve Iyilestirmeler](./current-gaps-and-improvements.md)

## Proje Tek Paragrafta

Bu proje .NET 10 ile hazirlanan bir e-commerce backend sistemidir. Sistem tek buyuk uygulama yerine, her is yeteneginin kendi servisine ve kendi veritabanina sahip oldugu mikroservis mimarisiyle kurulur. Servisler gerekli oldugunda HTTP ile konusur; siparis, stok rezervasyonu, odeme, kargo ve bildirim gibi uzun is akislarinda RabbitMQ uzerinden command ve event mesajlari kullanir. PostgreSQL kalici veriyi, Redis aktif sepeti, RabbitMQ servisler arasi mesajlari, Ocelot gateway dis dunyaya acilan API kapisini, MassTransit ise messaging, consumer, inbox ve outbox islerini yonetir.

## Ana Bilesenler

```text
Client
  |
  v
Ocelot API Gateway
  |
  +--> Catalog API
  +--> Basket API
  +--> Ordering API
  +--> Inventory API
  +--> Identity API
  +--> Notification SignalR Hub

RabbitMQ
  |
  +--> Ordering Saga Worker
  +--> Inventory API
  +--> Payment API
  +--> Shipping API
  +--> Notification API

PostgreSQL
  |
  +--> her servis icin ayri database

Redis
  |
  +--> aktif sepet verisi
```

## Mevcut Durum

Hazir olanlar:

- Solution yapisi ve shared building block projeleri.
- Catalog, Basket, Ordering, Inventory, Payment, Shipping, Notification, Identity, Ordering Saga Worker ve API Gateway.
- Her servis icin ayri PostgreSQL database yaklasimi.
- Redis destekli aktif basket yapisi ve lokal in-memory fallback.
- RabbitMQ ve MassTransit ile messaging.
- Tum servisler icin EF Core migration dosyalari.
- Transactional outbox destegi.
- Consumer servislerinde MassTransit inbox state destegi.
- Ocelot gateway routing.
- SignalR notification hub.
- Docker Compose ile lokal calistirma altyapisi.
- Smoke test ve validation scriptleri.
- Ilk unit ve contract testleri.

Henuz tamamlanmayanlar:

- Gercek login ve token uretimi.
- Production Redis provider bilgisi.
- Kapsamli integration test paketi.
- Korumali container publish/deployment pipeline'i; secret gerektirmeyen build/test/validation CI mevcuttur.
- Production seviyesinde observability dashboardlari.
