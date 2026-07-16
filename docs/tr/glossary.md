# Sozluk

Bu sayfa projede gecen teknik terimleri sade sekilde aciklar.

## Microservice

Bir sistemi tek buyuk uygulama yerine, kucuk ve bagimsiz servisler halinde bolme yaklasimidir. Bu projede `Catalog`, `Basket`, `Ordering`, `Inventory`, `Payment`, `Shipping`, `Notification` ve `Identity` ayri servislerdir.

## Monolith

Tum is kurallarinin, API'lerin ve veri erisiminin tek uygulamada toplandigi mimaridir. Baslangicta kolay olabilir, ama proje buyudukce deploy, test ve ekip ayrimi zorlasabilir.

## API Gateway

Client'in servislerle dogrudan tek tek konusmasi yerine, once tek bir giris noktasina gelmesini saglar. Bu projede gateway `Ocelot` ile kuruldu. Client `http://localhost:5080` uzerinden gateway'e gelir, gateway istegi ilgili servise yonlendirir.

## Service

Belirli bir is yeteneginden sorumlu uygulamadir. Ornegin `Inventory API` stok bilgisini, `Payment API` odeme surecini, `Catalog API` urun bilgisini yonetir.

## Database Per Service

Her servisin kendi veritabanina sahip olmasi prensibidir. `catalog_db`, `basket_db`, `ordering_db` gibi ayri databaseler bu yuzden var. Boylece servisler birbirinin tablolarina dogrudan baglanmaz.

## PostgreSQL

Kalici ve iliskisel verileri tuttugumuz veritabanidir. Siparisler, urunler, odeme kayitlari, kullanicilar ve bildirim gecmisi PostgreSQL'de saklanir.

## Neon

Managed PostgreSQL servisidir. Biz lokal PostgreSQL kurmak yerine cloud tarafinda Neon database kullaniyoruz.

## Redis

Hizli key-value veri deposudur. Bu projede aktif sepet verisini tutmak icin kullanilir. Sepet sik degistigi ve hizli okunup yazildigi icin Redis uygundur.

## RabbitMQ

Servisler arasinda mesaj tasiyan message broker'dir. Bir servis bir event veya command yayinlar, ilgili diger servisler bu mesaji alir.

## CloudAMQP

Managed RabbitMQ servisidir. RabbitMQ'yu kendi sunucumuzda kurmak yerine cloud uzerinden kullanmamizi saglar.

## Message Broker

Mesajlari ureten servis ile tuketen servis arasinda duran aracidir. Producer mesaji broker'a verir, consumer broker'dan alir.

## Event

Sistemde olmus bir seyi anlatan mesajdir. Gecmis zaman anlamindadir. Ornekler: `OrderSubmitted`, `InventoryReserved`, `PaymentAuthorized`, `ShipmentCreated`.

## Command

Bir servisten bir is yapmasini isteyen mesajdir. Emir gibi dusunulebilir. Ornekler: `ReserveInventory`, `AuthorizePayment`, `CreateShipment`, `RefundPayment`.

## Consumer

RabbitMQ'dan mesaj alan ve isleyen kod parcasidir. Ornegin `ReserveInventoryConsumer`, `ReserveInventory` command'ini alir ve stok rezervasyonu yapar.

## Producer / Publisher

Mesaj yayinlayan taraftir. Ornegin `Ordering API`, siparis olusunca `OrderSubmitted` event'i yayinlar.

## Exchange

RabbitMQ'da mesajin ilk geldigi yonlendirme noktasidir. Mesaj exchange'e gelir, exchange kurallara gore ilgili queue'lara dagitir.

## Queue

Consumer'in okuyacagi mesajlarin bekledigi kuyruktur. Her consumer veya consumer grubu genellikle kendi queue'sundan mesaj okur.

## Routing

Bir istegin veya mesajin nereye gidecegini belirleme isidir. Gateway HTTP routing yapar; RabbitMQ exchange ve queue yapisi mesaj routing yapar.

## Saga

Birden fazla servise yayilan uzun is akisini yoneten pattern'dir. Siparis surecinde stok, odeme, kargo ve iptal adimlari oldugu icin `OrderingSaga Worker` kullanilir.

## Compensation

Saga icinde bir adim basarisiz olursa onceki adimlari geri almak icin yapilan telafi islemidir. Odeme alindi ama kargo olusmadiysa `RefundPayment`; stok ayrildi ama odeme basarisiz olduysa `ReleaseInventory` gibi.

## Outbox Pattern

Veritabanina yazma ve mesaj yayinlama islemini guvenilir yapmak icin kullanilir. Servis once kendi database transaction'i icinde mesaj kaydini outbox tablosuna yazar. Daha sonra MassTransit bu kaydi RabbitMQ'ya yayinlar.

## Inbox Pattern

Ayni mesajin tekrar islenmesini engellemek icin kullanilir. Consumer gelen mesajin islenip islenmedigini kaydeder. Ayni mesaj tekrar gelirse duplicate islem yapilmaz.

## Idempotency

Ayni istegin veya mesajin birden fazla calissa bile sonucu bozmamasi demektir. Messaging sistemlerinde cok onemlidir, cunku dagitik sistemlerde mesaj tekrar gelebilir.

## CorrelationId

Ayni is akisini takip etmek icin kullanilan ortak takip kimligidir. Bir siparisin tum mesajlarinda ayni correlation id bulunursa log ve debug kolaylasir.

## CausationId

Bir mesaja hangi onceki islem veya mesajin sebep oldugunu gosterir. Olay zincirini anlamaya yardim eder.

## Migration

EF Core'un database schema degisikliklerini versiyonlu sekilde yonetmesidir. Yeni tablo, kolon veya index ekledigimizde migration olusturulur.

## DbContext

EF Core'da kod ile database arasindaki ana koprudur. Her servisin kendi `DbContext` sinifi vardir.

## Entity

Domain icinde kimligi olan is nesnesidir. Ornek: `Product`, `Order`, `Payment`, `Shipment`, `User`.

## Value Object

Kimlikten cok degeriyle anlam kazanan nesnedir. Bu projede basit tutuldu, ileride adres veya para degeri gibi yapilar value object'e donusebilir.

## DTO

API veya servis katmaninda veri tasimak icin kullanilan modeldir. Domain entity'yi dogrudan disari acmak yerine response/request modelleri kullanilir.

## Contract

Servisler arasi ortak mesaj veya API anlasmasidir. `ECommerce.BuildingBlocks.Contracts` projesi command, event ve ortak response modellerini tutar.

## Smoke Test

Sistemin temel olarak ayakta olup olmadigini hizlica kontrol eden testtir. Derin test degildir; "gateway calisiyor mu, servisler cevap veriyor mu, ana flow basliyor mu" gibi kontroller yapar.

## Health Check

Bir servisin ayakta ve cevap verebilir durumda olup olmadigini kontrol eden endpoint'tir.

## Docker

Uygulamalari container icinde calistirmaya yarar. Lokal ortamda servisleri ayni sekilde ayaga kaldirmak icin kullanilir.

## Docker Compose

Birden fazla container'i tek dosya ile birlikte calistirma aracidir. Bu projede gateway, servisler, worker ve Redis Compose ile ayaga kalkar.

## Environment Variable

Uygulamaya disaridan verilen ayardir. Connection string ve secret gibi bilgiler dosyaya yazilmaz, environment variable olarak verilir.

## Secret

Password, token, connection string gibi gizli degerlerdir. Repository'ye yazilmamalidir.

## SignalR

Server'dan client'a anlik bildirim gondermeyi saglayan .NET teknolojisidir. Bu projede Notification servisi siparis ve kargo durumlarini canli bildirmek icin SignalR hub kullanir.

## Observability

Sistemin icinde ne oldugunu anlamamizi saglayan log, metric ve trace yaklasimlarinin genel adidir.

## OpenTelemetry

Log, metric ve trace verilerini standart sekilde toplamaya yarayan acik standarttir.
