# Scriptler ve Lokal Komutlar

Bu projede `scripts` klasoru lokal gelistirme ve kontrol islemlerini kolaylastirir.

## check-runtime-env.ps1

Amac:

Gerekli environment variable degerleri set edilmis mi kontrol eder.

Ne zaman kullanilir:

- Docker Compose oncesi
- migration oncesi
- RabbitMQ veya database baglanti hatasi suphelenildiginde

Komut:

```powershell
.\scripts\check-runtime-env.ps1
```

Bu script secret degerleri ekrana basmaz; sadece eksik olup olmadigini soyler.

`.env` dosyasi varsa kontrol oncesi onu mevcut process icine yukler.

## load-env.ps1

Amac:

`.env` dosyasindaki degerleri mevcut PowerShell process icine yukler.

Bu script sadece private lokal denemeler icin kalir. Ortak development secret kaynagi Infisical olmalidir.

Private `.env` varsa dockersiz servis calistirmadan once kullan:

```powershell
.\scripts\load-env.ps1
dotnet run --project src/Services/Catalog/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj
```

Tercih edilen ortak development akisi:

```powershell
infisical run -- dotnet run --project src/Services/Catalog/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj
```

## run-with-secrets.ps1

Amac:

Herhangi bir komutu Infisical uzerinden calistirir. Infisical secret'lari process environment variable olarak enjekte eder.

Ornek:

```powershell
.\scripts\run-with-secrets.ps1 dotnet run --project src/Services/Catalog/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj
```

Docker Compose:

```powershell
.\scripts\run-with-secrets.ps1 docker compose up -d
```

Infisical environment slug `dev` degilse `-Environment` ile ver:

```powershell
.\scripts\run-with-secrets.ps1 -Environment dev dotnet test ECommerce.sln
```

## run-migrations.ps1

Amac:

EF Core migration'larini ilgili database'lere uygular.

Tum servisler:

```powershell
.\scripts\run-migrations.ps1
```

Tek servis:

```powershell
.\scripts\run-migrations.ps1 -Service Catalog
```

Bu script PostgreSQL connection string'lerini environment variable uzerinden okur.

## smoke-test.ps1

Kelime anlami:

Smoke test, sistemin temel olarak ayakta olup olmadigini hizlica kontrol eden testtir. Derin test degildir; "ana yerlerden duman cikiyor mu" kontrolu gibi dusunulebilir.

Amac:

- gateway health check
- servis health check
- temel user register
- inventory seed
- order create

Komut:

```powershell
.\scripts\smoke-test.ps1
```

Sadece health check:

```powershell
.\scripts\smoke-test.ps1 -SkipWorkflowProbe
```

## wait-for-runtime.ps1

Amac:

Gateway ve tum alt servis health route'lari basarili cevap verene kadar health-only smoke testini tekrarlar. CI icinde sabit bir sure beklemekten daha guvenilirdir.

```powershell
.\scripts\wait-for-runtime.ps1 -TimeoutSeconds 240
```

Belirlenen sure dolarsa son health hatasiyla birlikte basarisiz olur. Gerektiginde `-GatewayBaseUrl` ve `-PollingIntervalSeconds` degistirilebilir.

## request-runtime-access-token.ps1

`Auth__Authority` ve `Auth__Audience` icin `inventory:write` isteyen kisa omurlu Auth0 Client Credentials token'i alir. `RuntimeChecks__Auth0ClientId` ve `RuntimeChecks__Auth0ClientSecret` degerlerini okur, token'i yazdirmaz ve mevcut process ya da sonraki GitHub Actions adimlari icin `RuntimeChecks__AccessToken` olarak aktarir.

Workflow checker gizli olmayan senaryo ve probe ilerlemesini yazdirir. Hata durumunda exception turunu ve mesajini gosterir; access token'i veya veritabani connection string'lerini yazdirmaz.

## start-local.ps1

Amac:

Lokal ortamda sistemi daha otomatik baslatmaktir.

Genel olarak:

1. Docker kontrolu yapar.
2. Environment kontrolu yapar.
3. Testleri calistirir.
4. Docker Compose ile servisleri kaldirir.
5. Smoke test calistirir.

Komut:

```powershell
.\scripts\start-local.ps1
```

## validate-local.ps1

Amac:

Repository'nin temel olarak saglikli olup olmadigini kontrol eder.

Yaptiklari:

- opsiyonel build
- PowerShell script parse kontrolu
- docker compose config kontrolu
- secret benzeri deger taramasi

Secret taramasi once `rg` kullanir; bu komut yoksa Git tarafindan izlenen dosyalari PowerShell `Select-String` ile tarar. `rg` icin exit code `1`, eslesme bulunmadigi anlamina geldigi icin script bu beklenen sonucu process exit code `0` olarak normalize eder.

Komut:

```powershell
.\scripts\validate-local.ps1
```

Build atlayarak:

```powershell
.\scripts\validate-local.ps1 -SkipBuild
```

## Temel .NET Komutlari

Build:

```powershell
dotnet build ECommerce.sln
```

Test:

```powershell
dotnet test ECommerce.sln
```

SDK kontrolu:

```powershell
dotnet --version
dotnet --list-sdks
```

## Docker Komutlari

Compose config kontrolu:

```powershell
docker compose config
```

Servisleri baslatma:

```powershell
docker compose up -d
```

Log okuma:

```powershell
docker compose logs api-gateway
```

Kapatma:

```powershell
docker compose down
```
