# Runtime ve Operasyon

Bu sayfa uygulamanin calisma anindaki ihtiyaclarini anlatir.

## Gerekli Altyapi

Sistemin calismasi icin gereken ana altyapi:

- .NET 10 SDK/runtime
- PostgreSQL databaseleri
- RabbitMQ
- Redis
- Docker Desktop
- Infisical

Lokal gelistirmede servisler Docker Compose ile calistirilabilir. PostgreSQL ve RabbitMQ icin managed servisler kullanilabilir.

## Environment Variable Yaklasimi

Connection string ve secret degerleri repository icindeki dosyalara yazilmaz. Development icin ortak kaynak Infisical'dir. Lokal gecici testte process environment variable da kullanilabilir.

Yonetilen Grafana Cloud aktarimi Infisical'daki `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_EXPORTER_OTLP_PROTOCOL` ve gizli `OTEL_EXPORTER_OTLP_HEADERS` degerlerini kullanir. Standart OTEL degiskenleri Docker-local `Observability__OtlpEndpoint` degerinden onceliklidir; iki endpoint de yoksa servisler OTLP exporter olmadan normal calisir.

`Observability__RedactionEnabled` varsayilan olarak `true` degerindedir. Hassas trace tag ve structured-log attribute degerlerini aktarimdan once maskeler. Serbest metin log govdelerini yeniden yazmaz; credential, token, kisisel veya odeme sirri log mesaji icine yerlestirilmemelidir.

Ornek isimler:

```text
ConnectionStrings__CatalogDb
ConnectionStrings__BasketDb
ConnectionStrings__OrderingDb
RabbitMq__ConnectionString
Redis__ConnectionString
```

Bu yaklasim daha guvenlidir, cunku repository'ye gercek secret girmez.

Infisical ile dockersiz servis calistirma:

```powershell
infisical run -- dotnet run --project src/Services/Catalog/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj
```

Infisical ile Docker Compose calistirma:

```powershell
infisical run -- docker compose up -d
```

Repository'de sadece `.env.example` bulunur. Gercek `.env` dosyalari ignore edilir ve commit edilmez.

## Docker Compose

`docker-compose.yml` servisleri birlikte calistirir.

Icerir:

- api-gateway
- catalog-api
- basket-api
- ordering-api
- inventory-api
- payment-api
- shipping-api
- notification-api
- identity-api
- ordering-saga-worker
- redis

PostgreSQL ve RabbitMQ managed olarak disaridan kullanilabilir.

## Health Check

Health check servis ayakta mi anlamak icin kullanilir.

Gateway:

```text
http://localhost:5080/health/live
```

Gateway uzerinden service health route'lari da vardir.

## Logging

Servis loglari Docker Compose ile okunabilir:

```powershell
docker compose logs ordering-api
docker compose logs ordering-saga-worker
```

Production ortamda centralized logging gerekir.

## Observability

OpenTelemetry altyapisi hazirlandi. Bu, ileride trace, metric ve log verilerinin standart sekilde toplanmasini saglar.

Production icin onerilenler:

- OpenTelemetry Collector
- Prometheus
- Grafana
- Jaeger veya Tempo

## Migration Operasyonu

Migration'lar uygulama yayinlamadan once calistirilmalidir.

```powershell
.\scripts\run-migrations.ps1
```

Production ortamda migration adimi CI/CD pipeline icinde kontrollu calistirilmalidir.

## Continuous Integration

`.github/workflows/ci.yml` her push ve pull request icin secret gerektirmeyen kalite kontrollerini calistirir. .NET solution build/test, Next.js lint/build, PowerShell syntax, Docker Compose ve repository secret-pattern kontrolleri ayri job'larda calisir. Bunlar basarili olduktan sonra Compose/BuildKit ayni runner ve ortak layer cache ile on .NET application image'ini build eder. Migration, harici workflow probe, image push ve deployment adimlari bu pull-request pipeline'ina dahil edilmez.

## Guvenilir Runtime Entegrasyonu

`.github/workflows/runtime-integration.yml` yalnizca elle tetiklenen managed-environment testidir. Korumali `runtime-integration` GitHub environment'ini kullanir, GitHub OIDC ile Infisical'dan kisa omurlu secret erisimi alir, solution bagimliliklarini ve yerel EF aracini restore eder, tum EF migration'larini uygular, application Compose graph'ini baslatir, servislerin hazir olmasini bekler ve secilen basari veya compensation senaryosunu calistirir. Migration script'i bir servis basarisiz oldugunda kismen migrate edilmis bir ortamla devam etmek yerine hemen durur.

Workflow her zaman Infisical `staging` environment'ini okur; calistiran kisi `dev` veya `prod` secemez. `staging` kok dizini ortak degerleri Infisical `dev` ortamindan import eder ve dokuz `ConnectionStrings__*Db` anahtarinin tamamini yerel degerlerle override eder. Bu override'lar Neon `production` dalinin `runtime-integration` child branch'ini hedefler. Tek bir Neon branch proje icindeki tum veritabanlarini tasidigi icin migration ve workflow kayitlari parent branch'ten yalitilir. Import ile dokuz yerel override birlikte korunmalidir; eksik bir override Development baglantisina geri duser.

Job, Infisical'a baglanmadan once GitHub OIDC token'indan yalnizca gizli olmayan `iss`, `aud` ve `sub` claim'lerini cozer, bu uc degeri yazdirir ve kesin repository/environment guven sinirini dogrular. JWT'nin kendisi loglanmaz.

Infisical injection sonrasinda workflow, `RuntimeChecks__Auth0ClientId` ve gizli `RuntimeChecks__Auth0ClientSecret` ile Auth0 Client Credentials akisini kullanir ve `inventory:write customer:act` isteyen kisa omurlu token alir. Ilk izin test stoklarini hazirlar; ikinci izin ownership denetiminden sonra izole workflow-check musterisi adina siparis olusturur. Token maskelenir, sadece job boyunca yasar ve Catalog ya da Identity administrator erisimi vermez. Auth0 API'de iki permission'i da tanimla ve yalniz runtime M2M uygulamasina ver.

Docker API container'lari `IdentityClient__BaseUrl=http://identity-api:8080` kullanir. Basket, Ordering ve Notification customer-resource erisiminden once authenticated kullaniciyi bu guvenilir internal Identity adresinden cozer; `IdentityClient__TimeoutSeconds` varsayilan olarak bes saniyelik fail-closed timeout uygular. Host uzerinden calistirmada varsayilan adres `http://localhost:5090` olur.

GitHub `runtime-integration` environment'ina secret olmayan su iki variable eklenmelidir:

```text
INFISICAL_IDENTITY_ID
INFISICAL_PROJECT_SLUG
```

Infisical machine identity sadece gereken non-production environment secret'larini okuyabilmeli ve OIDC subject su kesin degerle sinirlanmalidir:

```text
repo:deniz1976@96434352/ecommerce-microservices@1302913896:environment:runtime-integration
```

GitHub'in immutable OIDC subject kullandigi repolarda sahip ID'si ve repository ID'si `@` isaretinden sonra yer alir. Bu ID'ler bilerek kullanilir ve sahip ya da repository gorunen adi degisse bile sabit kalir. Farkli bir repository kurulurken diagnostic adiminin yazdigi kesin `OIDC subject` degeri kopyalanmalidir.

Workflow migration uygulayip managed test branch'ine kayit yazdigi icin bilerek manuel ve tekil calisir. Runner container'lari her durumda kaldirilir; Neon `runtime-integration` dalina yazilan kayitlar bu dal resetlenene, temizlenene veya silinene kadar kalir.

## Secret Yonetimi

Gercek secret degerleri:

- git'e yazilmaz
- README veya docs icine yazilmaz
- Dockerfile icine yazilmaz
- compose dosyasina hardcoded girilmez

Kullanilabilecek cozumler:

- Infisical
- cloud secret manager
- GitHub Actions secrets
- environment variables
- container platform secret store

## Redis Operasyonu

Lokal Redis Docker Compose icinde calisir. Production icin Redis'in ayri managed servis olmasi daha dogrudur.

Redis bu projede aktif basket verisini tutar. Redis giderse aktif sepet davranisi etkilenir.

## RabbitMQ Operasyonu

RabbitMQ servisler arasi command/event mesajlarini tasir.

Bakilmasi gerekenler:

- queue birikmesi
- dead-letter queue
- retry sayilari
- consumer calisiyor mu
- connection hata loglari

## PostgreSQL Operasyonu

Her servis ayri database kullanir. Backup, migration ve connection pool ayarlari servis bazinda dusunulmelidir.
