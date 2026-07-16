# Eksikler ve Iyilestirmeler

Bu proje saglam bir temel kurdu, ama production seviyesine yaklasmak icin tamamlanmasi gereken alanlar var.

## Identity ve Auth

Mevcut durum:

- user register var
- password hash var
- user ve role tablolari var

Eksikler:

- login endpoint
- JWT token uretimi
- refresh token
- role-based authorization
- gateway seviyesinde auth enforcement

## Redis Production Provider

Mevcut durumda Docker Compose lokal Redis kullanir. Production icin Redis'in ayri ve managed bir servis olmasi daha dogrudur.

Aranabilecek cozumler:

- Upstash Redis
- Redis Cloud
- Azure Cache for Redis
- Railway/Render benzeri managed Redis secenekleri

## Integration Tests

Unit ve contract testleri var, ama tum workflow'u gercek PostgreSQL, RabbitMQ ve Redis ile test eden integration test seti henuz yok.

Eklenmeli:

- order happy path integration test
- inventory failure test
- payment failure compensation test
- gateway route testleri

## CI/CD

Secret gerektirmeyen GitHub Actions build/test/validation pipeline'i eklendi.

Mevcut adimlar:

- restore
- build
- test
- secret scan
- tum application container image'lari icin gated Compose/BuildKit build

Eklenebilecek guvenli release adimlari:

- migration validation
- image push
- deploy

## Observability

OpenTelemetry altyapisi hazir, ama production dashboard ve collector kurulumu yok.

Eklenebilecekler:

- OpenTelemetry Collector
- Prometheus
- Grafana
- Jaeger veya Tempo
- centralized logging

## Real Payment Provider

Payment servisi su an mock davranir. Production icin iyzico, Stripe veya baska provider entegrasyonu gerekir.

Dikkat edilmesi gerekenler:

- idempotency key
- webhook handling
- refund state
- audit log
- PCI uyumlulugu

## Catalog Admin ve Image Upload

Catalog servisinde urun modeli var, ama tam admin deneyimi ve gorsel upload sureci production seviyesinde degil.

Eklenebilir:

- admin authorization
- Cloudinary upload
- image validation
- variant support
- search/filter gelistirmeleri

## Order Status Sync

Saga `OrderConfirmed` veya `OrderCancelled` event'leri yayinlar. Ordering tarafinda bu event'leri dinleyip order status guncelleyen consumer eklemek sonraki mantikli adimdir.

## Notification Delivery

SignalR anlik bildirim var. Daha sonra email, SMS veya push notification kanallari eklenebilir.

## Resilience

Eklenebilecekler:

- retry policy ince ayari
- circuit breaker
- timeout policy
- dead-letter queue takibi
- poison message handling

## Security Hardening

Eklenmeli:

- rate limiting
- CORS politikasi
- security headers
- structured audit logs
- secret rotation sureci

## Documentation

Ingilizce ve Turkce dokumantasyon var. Bundan sonra her buyuk teknik degisiklikte iki dilde de guncelleme yapilmali.
