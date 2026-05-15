# HDFilmAvlu Yayınlama

Bu proje ASP.NET Core 8 MVC uygulamasıdır. SQLite kullanır.

## Hazır Çıktı

Yayın paketi oluşturmak için:

```powershell
.\scripts\publish.ps1
```

Çıktılar:

- `deploy-output\hdfilmavlu`
- `deploy-output\hdfilmavlu-release.zip`

## Admin Bilgisi

Varsayılan geliştirme girişi:

- Kullanıcı: `admin`
- Şifre: `admin123`

Yayında mutlaka ortam değişkeniyle değiştirin:

```text
Admin__Username=admin
Admin__Password=Güçlü_Bir_Şifre
```

## Kalıcı Klasörler

Şu klasörler yazılabilir ve kalıcı olmalıdır:

- `App_Data`
- `wwwroot/uploads`

SQLite veritabanı burada oluşur:

```text
App_Data/hdfilmavlu.db
```

## Docker ile Yayınlama

Build:

```powershell
docker build -t hdfilmavlu .
```

Run:

```powershell
docker run -p 8080:8080 `
  -e Admin__Username=admin `
  -e Admin__Password=Güçlü_Bir_Şifre `
  -v hdfilmavlu-data:/app/App_Data `
  -v hdfilmavlu-uploads:/app/wwwroot/uploads `
  hdfilmavlu
```

Adres:

```text
http://localhost:8080
```

## Hosting Notu

Public yayına almak için bir hosting hesabı veya VPS gerekir. Bana sağlayıcı paneli ya da SSH/FTP bilgileri verilmeden dışarıya yükleme işlemini tamamlayamam; ama bu paket doğrudan yayınlanmaya hazırdır.
