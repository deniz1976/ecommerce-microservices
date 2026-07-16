# Sorun Giderme

Bu sayfa sik karsilasilan hatalari ve nasil kontrol edilecegini anlatir.

## Docker Desktop Calismiyor

Belirti:

```text
dockerDesktopLinuxEngine pipe not found
```

Anlami:

Docker Desktop kapali veya Docker engine hazir degil.

Cozum:

1. Docker Desktop'i ac.
2. Engine tamamen calisana kadar bekle.
3. Komutu tekrar calistir.

Kontrol:

```powershell
docker version
```

## Runtime Environment Variable Eksik

Belirti:

```text
Missing required environment variables
```

Kontrol:

```powershell
.\scripts\check-runtime-env.ps1
```

Eksik degerleri PowerShell session icinde set et:

```powershell
$env:ConnectionStrings__CatalogDb = "<catalog-db-connection-string>"
$env:RabbitMq__ConnectionString = "<managed-rabbitmq-uri>"
```

Secret degerleri repository icindeki dosyalara yazma.

## Migration Hata Veriyor

Komut:

```powershell
.\scripts\run-migrations.ps1
```

Muhtemel nedenler:

- database adi yanlis
- password yanlis
- Neon database olusturulmamis
- SSL parametreleri eksik
- network sorunu var
- credential rotate edilmis

Kontrol et:

```text
Database var mi?
Connection string dogru database'e mi gidiyor?
SSL ayarlari var mi?
```

## RabbitMQ Baglantisi Hata Veriyor

Muhtemel nedenler:

- `RabbitMq__ConnectionString` set edilmemis
- username/password yanlis
- virtual host yanlis
- CloudAMQP instance kapali veya pause durumda

Kontrol:

```powershell
.\scripts\check-runtime-env.ps1
```

Sonra CloudAMQP dashboard'a bak.

## Redis Baglantisi Hata Veriyor

Lokal Docker Compose icinde Redis su servisle gelir:

```text
redis:7-alpine
```

Basket container icinden su adrese baglanir:

```text
redis:6379
```

Kontrol:

```powershell
docker compose ps
docker compose logs redis
```

Basket Docker disinda Redis olmadan calisirsa in-memory fallback kullanabilir. Bu development icin iyidir, production icin dogru degildir.

## Smoke Test Hata Veriyor

Komut:

```powershell
.\scripts\smoke-test.ps1
```

Kontrol:

```powershell
docker compose ps
docker compose logs api-gateway
docker compose logs ordering-api
```

Sadece health check calistirmak icin:

```powershell
.\scripts\smoke-test.ps1 -SkipWorkflowProbe
```

## Gateway Route Calismiyor

Gateway health:

```powershell
Invoke-WebRequest http://localhost:5080/health/live
```

Eger servis direkt calisiyor ama gateway uzerinden calismiyorsa:

- `ocelot.json` kontrol et
- `ocelot.Docker.json` kontrol et
- port ve container service name degerlerini kontrol et

## Testler Hata Veriyor

Komut:

```powershell
dotnet test ECommerce.sln
```

Restore hatasi varsa:

```powershell
dotnet restore ECommerce.sln
```

Package versiyonlari genel olarak `Directory.Packages.props` icinde tutulur.

## Secret Scan Hata Veriyor

`validate-local.ps1` repository icinde secret benzeri deger arar.

Cozum:

1. Gercek secret'i tracked dosyadan sil.
2. Dosyada placeholder kullan.
3. Gercek degeri environment variable olarak ver.

Kotu:

```text
RabbitMq__ConnectionString=<real-secret-written-to-file>
```

Iyi:

```text
RabbitMq__ConnectionString=
```

veya:

```powershell
$env:RabbitMq__ConnectionString = "<managed-rabbitmq-uri>"
```
