# ITAssetManagement - Docker & Run Guide

Bu repository, IT demirbaş ve zimmet yönetimi projelerinden oluşur.

Projeler:
- ITAssetManagement.API - Backend API
- ITAssetManagement.Web - Web uygulaması (MVC Views)

Önkoşullar
- Docker ve docker-compose yüklü olmalı
- (Geliştirme) Visual Studio 2026 veya dotnet 10 SDK

Ekran Görüntüleri
Aşağıdaki görseller uygulamadan alınıp `docs/` klasörüne eklenmelidir:

| Giriş | Envanter Listesi | Zimmet Yönetimi |
|-------|------------------|-----------------|
| ![Giriş](docs/giris.png) | ![Envanter](docs/envanter.png) | ![Zimmet](docs/zimmet.png) |

Hızlı Başlatma (Docker)
1. `.env.example` dosyasını `.env` olarak kopyalayın ve `SA_PASSWORD` ile `JWT_KEY` değerlerini değiştirin.
2. Kök dizinde terminal açın (PowerShell):
   docker compose up --build

3. Servisler ayağa kalktığında:
   - API: http://localhost:7040 (Swagger: http://localhost:7040/swagger)
   - Web: http://localhost:5000 (Compose: 5000 -> 80)

Ortam Değişkenleri
- SA_PASSWORD ve JWT_KEY zorunludur ve `.env` dosyasından okunur. Eksikse compose başlamaz.
- `.env` dosyası `.gitignore` ile repo dışında tutulur; şablon olarak `.env.example` kullanılır.
- SA_PASSWORD değiştirildiğinde SQL Server volume'u eski şifreyle oluşturulmuş olabilir; bu durumda `docker compose down -v` ile volume silinmelidir.
- API konteyneri ConnectionStrings__DefaultConnection ve Jwt__Key çevresel değişkenlerini kullanır.
- Web konteyneri ApiBaseUrl ile API adresini kullanır (`http://api:80/api/`).

Notlar
- İlk açılışta API, veritabanını migration ile otomatik oluşturur.
- Kamera ile QR okutma özelliği mobil uygulamada geliştirilecektir. Bu depoda barkoda göre cihaz sorgulama uç noktası (`GET api/Demirbaslar/Barkod/{barkod}`) hazırdır.

Docker ile geliştirme
- Eğer Visual Studio içinde debug yapıyorsanız, Dockerfile ve docker-compose dosyalarını Visual Studio'ya import edebilirsiniz.

Sorun giderme
- Eğer API konteyneri DB bağlantısı hatası veriyorsa, sqlserver konteynerinin tamamen hazır olduğunu (healthcheck) bekleyin veya connection string bilgilerini kontrol edin.

