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
| Basket | `AuthenticatedUser` | Customer ownership bekliyor |
| Ordering | `AuthenticatedUser` | Customer ownership bekliyor |
| Notification SignalR | `AuthenticatedUser` | Customer group ownership bekliyor |

## Onemli Sinir

Authentication, “gecerli token'i kim sundu?” sorusunu cevaplar. Kullanicinin bir customer kaydinin sahibi oldugunu tek basina kanitlamaz. Basket ve Ordering yerel Identity `Guid` degerini tutarken Auth0 token'i kullaniciyi harici `sub` ile tanir. Bu iki kimlik guvenli bicimde eslestirilene kadar authenticated bir kullanici bildigi baska bir customer kimligini isteyebilir. Notification group join icin de ayni sinir vardir.

Siradaki guvenlik asamasi, kaynak servislerinin kullanabilecegi guvenilir local-user mapping olusturmali; sonra owner-or-admin kurallari ve negatif testler eklenmelidir.

## Otomatik Denetim

Contract testleri sunlari dogrular:

- fallback policy authentication ister;
- Basket ve Ordering route group'lari `AuthenticatedUser` ister;
- Notification SignalR `AuthenticatedUser` ister;
- public servis route'lari acikca `AllowAnonymous` tanimlar;
- iki Ocelot konfigurasyonu da global Bearer authentication kullanir;
- gateway anonymous route'lari onayli allow-list ile birebir aynidir.

Bu testler normal solution test paketiyle calisir ve guvenlik temeli beklenmedik bicimde degisirse CI'i kirmizi yapar.
