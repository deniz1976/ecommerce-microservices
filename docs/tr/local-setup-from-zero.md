# Sifirdan Lokal Kurulum

Bu sayfa yeni bir makinede projeyi calistirmak icin genel adimlari anlatir.

## 1. Gerekenleri Kur

Gerekli araclar:

- .NET 10 SDK
- Docker Desktop
- Git
- PowerShell

Kontrol:

```powershell
dotnet --version
docker version
git --version
```

## 2. Repository Klasorune Gir

```powershell
cd C:\Users\deniz\Desktop\microservice
```

## 3. SDK Kontrolu

```powershell
dotnet --list-sdks
```

Projede .NET 10 kullaniliyor.

## 4. Secret Degerlerini Infisical'a Ekle

Her servis icin PostgreSQL connection string gerekir.

Ornek isimler:

```text
ConnectionStrings__CatalogDb
ConnectionStrings__BasketDb
ConnectionStrings__OrderingDb
ConnectionStrings__InventoryDb
ConnectionStrings__PaymentDb
ConnectionStrings__ShippingDb
ConnectionStrings__NotificationDb
ConnectionStrings__IdentityDb
ConnectionStrings__OrderingSagaDb
RabbitMq__ConnectionString
```

Auth0 `Customer`/`Seller` onboarding rol senkronizasyonunu acmak icin Infisical'a su anahtarlari da ekle:

```text
Auth0Management__Enabled=true
Auth0Management__Domain=<https:// olmadan tenant hostu>
Auth0Management__ClientId=<ayri Management API M2M client id>
Auth0Management__ClientSecret=<secret>
Auth0Management__CustomerRoleId=<Auth0 Customer role id>
Auth0Management__SellerRoleId=<Auth0 Seller role id>
Auth0Management__TimeoutSeconds=10
```

`ecommerce-runtime-integration` uygulamasini tekrar kullanma. Auth0 Management API icin ayri bir Machine-to-Machine uygulama olustur ve yalniz user-role membership yetkilerini ver.

Redis icin production connection string daha sonra verilecek. Lokal Docker Compose icinde `redis:6379` kullanilir.

Tercih edilen kullanim Infisical'dir:

```powershell
infisical login
infisical init
infisical run -- .\scripts\check-runtime-env.ps1
infisical run -- dotnet run --project src/Services/Catalog/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj
```

Projede bunun icin wrapper script de var:

```powershell
.\scripts\run-with-secrets.ps1 dotnet run --project src/Services/Catalog/ECommerce.Catalog.Api/ECommerce.Catalog.Api.csproj
.\scripts\run-with-secrets.ps1 docker compose up -d
```

Gecici lokal test icin PowerShell process environment variable da kullanilabilir.

## 5. Ortami Kontrol Et

```powershell
.\scripts\check-runtime-env.ps1
```

Eksik deger varsa script soyler.

## 6. Migration Calistir

```powershell
.\scripts\run-migrations.ps1
```

Bu komut her servisin database schema'sini olusturur veya gunceller.

## 7. Build ve Test

```powershell
dotnet build ECommerce.sln
dotnet test ECommerce.sln
```

## 8. Docker Desktop'i Ac

Docker Desktop tamamen calisiyor olmali.

Kontrol:

```powershell
docker version
```

## 9. Docker Compose ile Baslat

```powershell
docker compose up -d
```

## 10. Smoke Test Calistir

```powershell
.\scripts\smoke-test.ps1
```

Sadece health check:

```powershell
.\scripts\smoke-test.ps1 -SkipWorkflowProbe
```

## 11. Loglara Bak

```powershell
docker compose logs api-gateway
docker compose logs ordering-saga-worker
```

## 12. Kapat

```powershell
docker compose down
```
