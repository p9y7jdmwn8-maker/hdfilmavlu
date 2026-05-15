# HDFilmAvlu

C# / ASP.NET Core MVC ile hazirlanmis modern video ve film izleme sitesi.

## Ozellikler

- Ana sayfa
- Film ve dizi listeleme sayfalari
- Kategori filtreleri
- Yil, IMDb puani ve sureye gore siralama
- Sag ve sol reklam alanlari
- Otomatik kayan one cikan video alani
- Video detay/izleme sayfasi
- Admin girisi
- Admin panelinden kategori ekleme
- Admin panelinden video ve kapak gorseli yukleme
- 30, 45 ve 60 dakika video sureleri icin alanlar

## Calistirma

Bu klasorde yerel .NET 8 SDK kuruludur. VS Code ile klasoru actiktan sonra terminalde:

```powershell
.\.dotnet\dotnet.exe restore
.\.dotnet\dotnet.exe run
```

VS Code terminali yeniden acildiginda `.vscode/settings.json` sayesinde `dotnet run` komutu da calisir.

Tarayicida terminalde yazan `http://localhost:...` adresini acin.

## Admin

Varsayilan yerel giris:

```text
Kullanici adi: admin
Sifre: admin123
```

Canli yayinda bu bilgileri Render environment variables ile degistirin:

```text
Admin__Username
Admin__Password
```

## Render Yayini

Render uzerinde Docker Web Service olarak yayinlanir.

Temel ayarlar:

```text
Runtime: Docker
Dockerfile Path: ./Dockerfile
Branch: main
```

Environment variables:

```text
Admin__Username=admin
Admin__Password=guclu-bir-sifre
DISABLE_HTTPS_REDIRECT=true
ASPNETCORE_ENVIRONMENT=Production
```

Ayrintilar icin `DEPLOYMENT.md` dosyasina bakin.

## Duzenlenecek Yerler

- Kategori eklemek icin admin panelini kullanin.
- Reklam HTML kodlari `Views/Shared/_Layout.cshtml` dosyasindaki sol ve sag reklam alanlarina eklenir.
- Yuklenen videolar `wwwroot/uploads/videos` klasorune kaydedilir.
- Yuklenen kapaklar `wwwroot/uploads/posters` klasorune kaydedilir.

