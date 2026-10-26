# YouTube Downloader v2.1.0 — Bağımsız Doğrulama ve Mac Test Raporu

**Tarih:** 26 Ekim 2026
**Hedef Sürüm:** v2.1.0 (Windows & macOS Apple Silicon)
**Durum:** DOĞRULAMA TAMAMLANDI — MAC DONANIM TESTİNE HAZIR (Yayımlama Beklemede)

---

## 1. YÖNETİCİ ÖZETİ VE MUTLAK KURAL DENETİMİ

Bu doğrulama çalışması, önceki oturumlarda geliştirilen kodların bağımsız bir güvenlik, kararlılık ve regresyon denetimidir. Projeye gereksiz özellik eklenmemiş, mevcut Windows uygulaması ve paketleri korunmuş, tespit edilen güvenlik açıkları giderilmiş ve testler gerçek verilerle çalıştırılmıştır.

### 🛡️ Kırmızı Çizgi ve Güvenlik Denetimi
- [x] **Git İşlemleri:** Depoya hiçbir `commit`, `push`, `tag` veya `GitHub Release` gönderilmemiştir (`git status` ile teyit edildi).
- [x] **Windows Uyumluluğu:** .NET Framework 4.8.1 WinForms projesi bozulmadan korunmuş; mevcut indirme, analiz, codec filtreleme ve canlı ilerleme özellikleri çalışmaya devam etmektedir.
- [x] **Özel Anahtar Güvenliği:** RSA özel anahtarı (private key) kaynak koddan, depodan ve paketleme betiklerinden tamamen arındırılmıştır. Anahtar, `.gitignore` ile korunan yerel `keys/update-signing-private.key` dosyasında tutulmaktadır. Repoda veya derlenen dosyalarda hiçbir özel anahtar bulunmamaktadır.
- [x] **Fiziksel Cihaz Testleri:** Apple Silicon donanımı gerektiren son çalıştırma adımları dürüstçe `BLOCKED (Fiziksel Mac Donanımı Gerekli)` olarak işaretlenmiştir.

---

## 2. WINDOWS REGRESYON KONTROLÜ

### 2.1. Temiz Release Derlemesi
- **Derleme Aracı:** MSBuild 17.14.23 (.NET Framework 4.8.1)
- **Komut:** `MSBuild.exe "src\youtube-downloader\youtube dowload.csproj" /p:Configuration=Release /t:Rebuild`
- **Sonuç:** `0 Hata, 0 Uyarı (Başarılı)`
- **Çıktı Dosyaları:** `src/youtube-downloader/bin/Release/youtube dowload.exe`

### 2.2. Kapsamlı Test Paketi Sonuçları
`tests/TestRunner.cs` test süiti çalıştırılmış ve **44 test senaryosunun tamamı başarıyla geçmiştir (`44 PASS, 0 FAIL`)**:
- **v2.0.2 Regresyon Testleri (1-14):** H.264/AAC ve En İyi Kalite profil seçimi, ffprobe doğrulaması, Türkçe karakter desteği, hata yönetimi ve iptal mekanizması: `PASS (14/14)`
- **v2.0.3 Canlı İlerleme ve Hız Testleri (15-26):** Gerçek zamanlı yüzde artışı, boyut hesaplamaları, hız yumuşatma (EMA), kalan süre (ETA) ve aşama geçişleri: `PASS (12/12)`
- **v2.0.3 Önceden Boyut Tahmini Testleri (27-38):** JSON ayrıştırma, storyboard FPS toleransı, çoklu akış kümülatif boyutu ve 24 saati aşan süreler: `PASS (12/12)`
- **v2.1.0 Otomatik Güncelleme ve Güvenlik Testleri (39-44):** Manifest ayrıştırma, RSA-2048 imza doğrulama, sürüm karşılaştırması/downgrade koruması, 24 saat hız sınırı, platform paket seçimi ve SHA-256 bütünlüğü: `PASS (6/6)`

### 2.3. Kurulum ve Güncelleme Yolları
| Kurulum Türü | Algılama Yöntemi | Güncelleme Mekanizması | Hata Durumunda Kurtarma |
| :--- | :--- | :--- | :--- |
| **Inno Setup (Kurulu)** | Registry anahtarı (`{E684D355-63F6-4A59-86BC-B368A91522F5}_is1`) ve `InstallLocation` eşleşmesi | `VideoDownloader-Setup.exe /SP- /SILENT /NORESTART` arka planda çalıştırılır. | Kurulum başlatılmadan önce SHA-256 doğrulanır. İndirme iptal edilirse veya hash tutmazsa dosya silinir; mevcut kurulum aynen çalışmaya devam eder. |
| **Portable ZIP (Taşınabilir)** | Registry kaydı bulunmadığında veya dizin uyuşmadığında otomatik devreye girer. | `%TEMP%\update_helper.bat` betiği başlatılır; uygulamanın PID'sinin kapanması beklenir, yeni dosyalar uygulama klasörüne kopyalanır ve EXE yeniden açılır. | Geçici klasörde ZIP açılırken hata olursa mevcut çalışan klasöre dokunulmaz. Kullanıcı uygulamayı kullanmaya devam eder. |

### 2.4. Kullanıcı Ayarlarının Korunumu
- Kullanıcı ayarları `%APPDATA%\YouTubeVideoDownloader\settings.json` altında tutulmaktadır.
- Uygulamanın kurulu olduğu program dizininden tamamen yalıtılmıştır.
- Güncelleme sırasında ister Inno Setup çalışsın ister Portable ZIP açılsın, `%APPDATA%` altındaki ayar dosyası hiçbir şekilde silinmez veya sıfırlanmaz.

### 2.5. Windows Kaynak Kodundaki Değişiklikler (Dosya Bazında)
1. **`src/youtube-downloader/SettingsService.cs`:**
   - `CheckForUpdatesOnStartup` (varsayılan `true`), `LastUpdateCheckUtc` (varsayılan `null`) ve `UpdateChannel` (`stable`) alanları eklendi.
2. **`src/youtube-downloader/UpdateManager.cs` (Yeni Dosya):**
   - Registry tabanlı kurulum tespiti, RSA-2048 SHA-256 genel anahtar doğrulaması, downgrade koruması, 24 saat hız sınırı denetimi, SHA-256 hash doğrulamalı paket indirme ve kurulum uygulama mantığı eklendi.
3. **`src/youtube-downloader/Form1.cs`:**
   - Açılışta 3 saniye gecikmeli, UI thread'i kilitlemeyen arka plan güncelleme denetimi eklendi.
   - Sağ üstteki `btnUpdateEngine` butonu context menu ile genişletildi: Hem `yt-dlp` motorunu güncelleme hem de *"✨ Uygulama Güncellemelerini Denetle..."* seçenekleri sunuldu.
   - İndirme sırasında ana ekrandaki ProgressBar ve yüzde etiketleri kullanılarak indirme ilerlemesi canlı yansıtıldı.
4. **`src/youtube-downloader/youtube dowload.csproj`:**
   - `UpdateManager.cs` derleme listesine eklendi.
5. **`tests/TestRunner.cs`:**
   - 39-44 numaralı otomatik güncelleme ve kriptografik güvenlik testleri eklendi.

---

## 3. GÜNCELLEME GÜVENLİĞİ VE KRİPTOGRAFİK DOĞRULAMA

### 3.1. RSA-2048 ve SHA-256 İmza Denetimi
- **Genel Anahtar (Public Key):** `UpdateManager.cs` ve `MacUpdateManager.cs` içerisine sabit XML formatında gömülmüştür. Yalnızca imza doğrulamak için kullanılır; imza üretme yeteneği yoktur.
- **Kanonik Mühür:** Manifest içerisindeki `Version|ReleaseDate|ChangelogUrl|pkg1:file:size:sha256;...` dizesi RSA-2048 PKCS#1 SHA-256 ile imzalanır.
- **Sıkı Güvenlik Kontrolü:** Yapılan denetimde imza geçersiz olduğunda güncellemenin durdurulması güvenceye alınmıştır. İmzası geçersiz veya bozulmuş bir manifest kesinlikle reddedilir (`IsError = true`, `IsUpdateAvailable = false`).
- **Tahrifat Koruması (Tamper-Proof):** Test 40 kapsamında doğrulanmıştır; manifestteki sürüm numarası veya paket SHA-256 değeri 1 karakter dahi değiştirilse imza doğrulaması başarısız olur ve güncelleme engellenir.

### 3.2. Platform ve Mimari Yalıtımı (Çapraz İndirme Engelleme)
Kaynak kod seviyesinde yapılan güvenlik incelemesinde tespit edilen olası çapraz paket seçimi giderilmiştir:
- **Windows İstemcisi:** Kesinlikle sadece `windows-setup` veya `windows-portable` paketlerini seçebilir. `mac`, `darwin`, `.dmg` veya `arm64` içeren paketler anında reddedilir.
- **macOS İstemcisi:** Kesinlikle sadece `macos-arm64` veya `macos-universal` paketlerini seçebilir. `.exe` veya `windows` paketleri anında reddedilir.

### 3.3. Hata Senaryoları ve Dayanıklılık
- **Downgrade Koruması:** `remoteVer <= CurrentVersion` kontrolü sayesinde sunucuda eski bir sürüm gösterilse dahi geriye dönük sürüm yüklenemez.
- **Bozuk Hash:** İndirilen paketin yerel SHA-256 özeti, manifestte belirtilen özetle birebir uyuşmazsa dosya derhal silinir (`File.Delete`) ve istisna fırlatılır.
- **Yarım İndirme:** İndirme işlemi doğrudan nihai dosya adına değil, `.part` uzantısıyla yapılır. İndirme kesilirse `.part` dosyası geçersiz sayılır; bir sonraki denemede temizlenip baştan başlanır.
- **Ağ ve GitHub Hataları:** GitHub API hız sınırına (rate limit) takılınırsa veya bağlantı koparsa uygulama çökmek yerine kullanıcıya anlaşılır durum bildirimi verir.

---

## 4. MACOS APPLE SILICON (ARM64) PAKET DOĞRULAMASI

### 4.1. Derleme ve Paketleme
- **Framework:** .NET 8.0 (LTS) — Avalonia UI 11.3 MVVM
- **Hedef Mimari:** `osx-arm64` (Apple Silicon M1/M2/M3/M4)
- **Derleme Komutu:** `dotnet publish src/macos-downloader/YouTubeDownloader.Mac.csproj -r osx-arm64 -c Release --self-contained true /p:PublishSingleFile=true -o artifacts/publish/osx-arm64`
- **Sonuç:** `0 Hata, 0 Uyarı (Başarılı)`

### 4.2. `.app` Bundle Mimarisi
`artifacts/YouTube Downloader.app` klasör yapısı incelendi:
```text
YouTube Downloader.app/
└── Contents/
    ├── Info.plist (CFBundleExecutable: YouTubeDownloader.Mac, ARM64, v2.1.0)
    ├── MacOS/
    │   ├── YouTubeDownloader.Mac (83.2 MB, Mach-O 64-bit ARM64 ikili dosyası)
    │   ├── libAvaloniaNative.dylib (macOS yerel pencere ve menü kütüphanesi)
    │   ├── libHarfBuzzSharp.dylib (Yazı tipi render kütüphanesi)
    │   └── libSkiaSharp.dylib (2D grafik motoru kütüphanesi)
    └── Resources/
```
- Çalışma zamanı için gereken tüm native dylib kütüphaneleri bundle içerisine dahil edilmiştir; sistemde .NET kurulu olması gerekmez (Self-Contained).

### 4.3. Motor Bağımlılıkları (Homebrew)
- Uygulama, video indirme ve dönüştürme için sistemdeki `yt-dlp` ve `ffmpeg` ikililerini kullanır.
- Homebrew'in standart Apple Silicon yolu olan `/opt/homebrew/bin/` dizini öncelikli olarak taranır.
- Eğer sistemde yt-dlp veya FFmpeg yoksa, arayüzde sarı uyarı çubuğu görüntülenir ve kullanıcı Homebrew kurulum komutuna yönlendirilir (`brew install yt-dlp ffmpeg`).

---

## 5. TEST DOĞRULAMA TABLOSU

| Test Grubu | Senaryo | Hedef | Gerçekleşen Sonuç | Karar |
| :--- | :--- | :--- | :--- | :---: |
| **Windows Derleme** | MSBuild Release Temiz Derleme | Windows | 0 Hata, 0 Uyarı | **PASS** |
| **Windows Regresyon** | v2.0.2 & v2.0.3 İndirme & Codec Testleri (1-38) | Windows | 38 Senaryo Başarılı | **PASS** |
| **Güncelleme Manifesti** | JSON Ayrıştırma ve Alan Doğrulama (Test 39) | Ortak | Manifest alanları eksiksiz | **PASS** |
| **Kriptografik Güvenlik** | RSA-2048 İmza ve Tahrifat Engelleme (Test 40) | Ortak | Sahte veri reddedildi | **PASS** |
| **Sürüm Kontrolü** | Downgrade ve Eşit Sürüm Koruması (Test 41) | Ortak | Sürüm düşürme engellendi | **PASS** |
| **Hız Sınırı (Rate Limit)**| 24 Saatlik Sessiz Denetim Mantığı (Test 42) | Ortak | Günde en fazla 1 kez sorgu | **PASS** |
| **Paket Seçimi** | Kurulum Türü ve Platform İzolasyonu (Test 43) | Ortak | Yanlış platform engellendi | **PASS** |
| **Bütünlük Denetimi** | SHA-256 Sağlama ve Bozuk Dosya İptali (Test 44) | Ortak | Bozuk hash silindi | **PASS** |
| **macOS Derleme** | .NET 8 Avalonia `osx-arm64` Derlemesi | macOS | 0 Hata, 0 Uyarı | **PASS** |
| **macOS Paketleme** | `.app` Bundle ve Dağıtım ZIP Üretimi | macOS | 38.07 MB ZIP üretildi | **PASS** |
| **macOS Gatekeeper** | Ücretsiz Dağıtım İzin Rehberi | macOS | Dokümantasyon hazırlandı | **PASS** |
| **Mac Donanım Testi** | Apple Silicon Üzerinde UI Başlatma | Mac M1/M2/M3 | Fiziksel cihaz bekleniyor | **BLOCKED** |
| **Mac Donanım Testi** | Apple Silicon Üzerinde Gerçek Video İndirme | Mac M1/M2/M3 | Fiziksel cihaz bekleniyor | **BLOCKED** |

---

## 6. ÜRETİLEN PAKETLER VE SAĞLAMA ÖZETLERİ (SHA-256)

| Paket Dosyası | Konum | Boyut | SHA-256 Özeti |
| :--- | :--- | :--- | :--- |
| **`VideoDownloader-Setup.exe`** | `VideoDownloader-Setup.exe` | 115.271.500 bayt (109,93 MB) | `d14896f1a55f144be533fa9be790e27e7baaac8d5faa1a164664ad8f03d4df74` |
| **`YouTubeDownloader_v2.1.0_Windows.zip`** | `YouTubeDownloader_v2.1.0_Windows.zip` | 150.975.530 bayt (143,98 MB) | `69c2859cf8c5d00afa76ef6570fe48ac892ed53278516a635658eaf5a8511438` |
| **`YouTubeDownloader_v2.1.0_macOS_ARM64.zip`** | `artifacts/YouTubeDownloader_v2.1.0_macOS_ARM64.zip` | 39.916.702 bayt (38,07 MB) | `31fabf2a3552f4d8883ef3ba005deef9c81d53184d9affea34c799c5f8841bde` |
| **`YouTube Downloader.app` (Bundle)** | `artifacts/YouTube Downloader.app` | ~103 MB (Açılmış) | Mach-O ARM64 + dylib Doğrulandı |
| **`update-manifest.json` (İmzalı)** | `update-manifest.json` | 1.832 bayt | RSA-2048 SHA-256 İmzası Doğrulandı |

---

## 7. ARKADAŞINIZIN APPLE SILICON MAC'İNDE TEST REHBERİ

Arkadaşınızın Mac'inde uygulamayı denemek için takip edilecek adımlar:

### Adım 1: Paketi Mac'e Kopyalayın
- Projedeki `artifacts/YouTubeDownloader_v2.1.0_macOS_ARM64.zip` dosyasını AirDrop, flash bellek veya bulut depolama ile Mac'e aktarın.
- ZIP dosyasına çift tıklayarak **`YouTube Downloader.app`** uygulamasını çıkarın.
- Uygulamayı **Uygulamalar (`/Applications`)** klasörüne sürükleyin.

### Adım 2: İlk Açılış ve Gatekeeper İzni (Ücretsiz Dağıtım)
Apple Developer sertifikası olmadan ücretsiz dağıtıldığı için macOS ilk açılışta güvenlik uyarısı verir. Bunu aşmak için:
- **Yöntem A (Grafik Arayüz):** Finder'da uygulamaya **Control tuşuna basarak tıklayın** (veya sağ tıklayın) ve **Aç (Open)** deyin. Çıkan pencerede tekrar **Aç** butonuna basın. (macOS bu tercihi hatırlar).
- **Yöntem B (Terminal):** Terminal'i açıp şu komutu çalıştırın:
  ```bash
  xattr -cr "/Applications/YouTube Downloader.app"
  ```

### Adım 3: Motor Kontrolü
Terminal'de Homebrew paketlerinin kurulu olduğunu doğrulayın:
```bash
brew install yt-dlp ffmpeg
```

### Adım 4: Doğrulama Testleri
1. **Pencere Açılışı:** Uygulama açıldığında Fluent temasının ve başlığın (`macOS ARM64 v2.1.0`) düzgün yüklendiğini görün.
2. **Video Analizi:** Bir YouTube linki yapıştırıp **Analiz Et** butonuna basın. Video başlığı, süresi ve format listesinin geldiğini teyit edin.
3. **İndirme ve İlerleme:** Kalite seçip **İndirmeyi Başlat** butonuna basın. İlerleme çubuğunun aktığını, MB/Hız/Kalan Süre bilgilerinin güncellendiğini doğrulayın.
4. **Dosyayı Açma:** İndirme bittiğinde **Dosyayı Aç** butonuna basarak videonun QuickTime Player ile sorunsuz açıldığını teyit edin.

---

## 8. SONUÇ VE ONAY BEKLENTİSİ

- Windows ve macOS projeleri temiz bir şekilde derlenmektedir.
- Windows tarafındaki mevcut özelliklerde sıfır kırılma ve sıfır regresyon vardır.
- Otomatik güncelleme mimarisi kriptografik RSA-2048 SHA-256 dijital mühür ile donatılmıştır.
- Özel anahtarlar açık kaynaklı repoda yer almayacak şekilde güvenli biçimde izole edilmiştir.
- Depoya hiçbir commit veya push yapılmamış, sonraki adımlar için kullanıcının onayı beklenmektedir.
