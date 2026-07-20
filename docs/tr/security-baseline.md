# API Guvenlik Temeli

## Varsayilan Kapali Kural

Butun servis API'leri ortak fallback authorization policy kullanir. Bir endpoint acikca `AllowAnonymous` veya daha dar bir named policy tanimlamadikca authenticated kullanici ister. Auth0 ayarlari eksikse Bearer handler istegi reddeder; eksik konfigurasyon korumali endpoint'i public yapmaz.

Ocelot ayni siniri gateway katmaninda tekrar uygular. Iki gateway route dosyasi da global `Bearer` authentication kullanir. Public route'lar bu kurali `AuthenticationOptions.AllowAnonymous` ile acikca ezer.

Gateway ortak authentication kaydini `AddOidcReadyAuthentication` ile yapar, fakat servis authorization policy'lerini kaydetmez. Boylece ASP.NET minimal hosting, Ocelot route'u secilmeden once fallback authorization middleware'ini otomatik ekleyemez; aksi durumda gateway'in kendi health endpoint'i dahil anonymous route'lar reddedilirdi. Gateway route authorization islemini Ocelot yapar; downstream servisler `AddOidcReadySecurity` ile kendi ASP.NET fallback policy'sini bagimsiz olarak korur.

## Public Allow-List

Yalnizca su islemler bilerek public tutulur:

- Catalog product `GET` route'lari.
- Inventory item `GET` route'u.
- Identity user registration `POST` route'u.
- Servis ve gateway health check'leri.

Catalog create/update, Inventory update, rastgele Identity user sorgusu, Basket, Ordering, Auth profile islemleri ve Notification SignalR korumalidir.

SignalR transport uyumlulugu icin JWT handler `access_token` query parametresini yalnizca dogrudan ve gateway notification hub path'lerinde kabul eder. Normal HTTP API'leri Authorization header istemeye devam eder.

## Mevcut Korumali Yuzey

| Yuzey | Mevcut gereksinim | Ownership durumu |
|---|---|---|
| Catalog write | `Admin` | Seller/store ownership modellenmedi |
| Inventory write | `InventoryWrite` | Operasyonel kaynak |
| Identity rastgele user okuma | `Admin` | Yalnizca admin |
| Identity `/auth/me` | `AuthenticatedUser` | Token `sub` degerinden uretilir |
| Basket | `AuthenticatedUser` ve ownership | Identity ile cozulmus owner, `Admin` veya `customer:act` |
| Ordering | `AuthenticatedUser` ve ownership | Identity ile cozulmus owner, `Admin` veya `customer:act` |
| Notification SignalR | `AuthenticatedUser` ve ownership | Identity ile cozulmus customer group, `Admin` veya `customer:act` |

## Customer Ownership

Authentication tek basina kullanicinin customer kaydina sahip oldugunu kanitlamaz. Basket, Ordering ve Notification ortak ownership authorizer ile asil Bearer token'i Identity `/api/v1/auth/me` endpoint'ine iletir ve donen yerel user `Guid` degerini istenen customer kimligiyle karsilastirir. Lookup hatasi erisimi reddeder. Order-by-id baska musteriye ait kaydi not found olarak gizler.

`Admin` ve tam `customer:act` izni owner karsilastirmasini atlayabilir. `customer:act` yalniz guvenilir runtime otomasyonu icindir, normal kullanicilara verilmemelidir ve ilgisiz admin islemlerini acmaz.

## Auth0 Role Senkronizasyonu

Authenticated onboarding yalniz `Customer` ve `Seller` rollerini senkronlar. Identity, ayri ve en az yetkili Auth0 Management API M2M uygulamasiyla once diger self-service rolunu kaldirir, secilen rolu ekler, sonra yerel veritabanini gunceller. Auth0 hatasi `503` dondurur ve yerel rol degismez. Basaridan sonra frontend eski claim'lerle devam etmemek icin cache disindan yeni token ister.

Management istemcisi runtime-integration uygulamasindan ayridir; secret Infisical'da kalir ve uygulama/client yonetimi ya da `Admin` atama yetkisi almamalidir. Auth0 basarili olup hemen ardindan veritabani yazimi basarisiz olursa calisacak reconciliation/outbox mekanizmasi sonraki istir.

## Otomatik Denetim

Contract testleri sunlari dogrular:

- fallback policy authentication ister;
- Basket ve Ordering route group'lari `AuthenticatedUser` ister;
- Notification SignalR `AuthenticatedUser` ister;
- Auth0 role senkronizasyonu dogru remove/assign cagrilarini yapar ve hata halinde yerel yazimdan once durur;
- public servis route'lari acikca `AllowAnonymous` tanimlar;
- iki Ocelot konfigurasyonu da global Bearer authentication kullanir;
- gateway anonymous route'lari onayli allow-list ile birebir aynidir.

Bu testler normal solution test paketiyle calisir ve guvenlik temeli beklenmedik bicimde degisirse CI'i kirmizi yapar.
