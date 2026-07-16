# Mimari Karar Kaydi

Bu sayfa projede alinan ana teknik kararlarin nedenlerini ozetler.

## Karar: .NET 10 Kullanimi

Neden:

- modern runtime
- guncel C# ozellikleri
- uzun vadeli gelistirme icin yeni platform

Risk:

- bazi kutuphanelerde erken donem uyumluluk problemi olabilir

Mevcut kontrol:

- console test basarili
- solution build/test basarili

## Karar: Mikroservis Mimarisi

Neden:

- is alanlarini ayirmak
- her servisin kendi verisine sahip olmasi
- messaging ve saga gibi production yaklasimlarini ogrenmek

Bedel:

- daha fazla proje ve dosya
- daha fazla operasyon ihtiyaci
- distributed consistency zorlugu

## Karar: Database Per Service

Neden:

- servis bagimsizligi
- veri sahipliginin net olmasi
- mikroservis best practice'e uygunluk

Bedel:

- join yapmak kolay degil
- raporlama icin ayrica read model gerekebilir

## Karar: PostgreSQL ve Neon

Neden:

- guclu relational database
- cloud uzerinde kolay baslangic
- EF Core ile iyi uyum

## Karar: Redis Sadece Basket Icin

Neden:

- aktif basket hizli degisir
- key-value model basket icin uygun
- PostgreSQL'e gereksiz sik yazmayi azaltir

Not:

Checkout snapshot yine PostgreSQL'de saklanir.

## Karar: RabbitMQ ve MassTransit

Neden:

- servisler arasi asenkron iletisim
- command/event modeline uygunluk
- MassTransit ile consumer, retry, outbox/inbox destegi

## Karar: Saga Worker

Neden:

- siparis sureci tek servise ait degil
- stok, odeme ve kargo adimlari ayri servislerde
- hata durumunda compensation gerekiyor

## Karar: Ocelot Gateway

Neden:

- .NET ekosisteminde basit gateway kurulumu
- route konfigurasyonu kolay
- lokal gelistirme icin yeterli

Alternatif:

- YARP
- cloud API gateway
- Kubernetes ingress

## Karar: Secret'lari Dosyaya Yazmamak

Neden:

- repository'ye secret girerse sızıntı riski olur
- git history'den temizlemek zordur
- production'da secret manager kullanmak daha dogrudur

Bu yuzden connection string'ler environment variable ile verilir.

## Karar: TR/EN Dokumantasyon

Neden:

- kod dili Ingilizce kalmali
- ekip ve ogrenme sureci Turkce aciklamadan faydalanir
- proje hem teknik hem ogretici hale gelir
