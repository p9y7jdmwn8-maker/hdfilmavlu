# StreamPanel Video Sitesi

C# / ASP.NET Core MVC ile hazırlanmış modern video sitesi başlangıç projesi.

## Özellikler

- Ana sayfa
- Kategori filtreleri
- Sağ ve sol reklam alanları
- Video detay/izleme sayfası
- Yönetimden kategori ekleme
- Yönetimden video ve kapak görseli yükleme
- 30, 45 ve 60 dakika süre seçenekleri

## Çalıştırma

Bu klasörde yerel .NET 8 SDK kuruludur. VS Code ile klasörü açtıktan sonra terminalde şu komutları kullanabilirsiniz:

```powershell
.\.dotnet\dotnet.exe restore
.\.dotnet\dotnet.exe run
```

VS Code terminali yeniden açıldığında `.vscode/settings.json` sayesinde `dotnet run` komutu da çalışır.

Tarayıcıda terminalde yazan `http://localhost:....` adresini açın.

## Yayınlama

Yayın paketi oluşturmak için:

```powershell
.\scripts\publish.ps1
```

Ayrıntılar için `DEPLOYMENT.md` dosyasına bakın.

## Düzenlenecek Yerler

- Kategori eklemek için sitede `Kategoriler` sayfasını kullanın.
- Reklam HTML kodlarını `Views/Shared/_Layout.cshtml` dosyasındaki `Sol reklam alanı` ve `Sağ reklam alanı` bölümlerine yerleştirin.
- Yüklenen videolar `wwwroot/uploads/videos` klasörüne kaydedilir.
- Yüklenen kapaklar `wwwroot/uploads/posters` klasörüne kaydedilir.
