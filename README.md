# 🎬 YouTube ve Video İndirici v2.0.1 (Windows)

Modern Windows Forms (.NET Framework 4.8.1) mimarisinde geliştirilmiş, **yt-dlp** ve **FFmpeg** destekli, yüksek kaliteli video ve ses indirme uygulaması.

---

## ⚡ Hızlı ve Kolay Kurulum

Uygulamayı kullanmak için iki kolay seçenek bulunmaktadır:

### 📦 Seçenek 1: Otomatik Kurulum Sihirbazı (Önerilen)
Arkadaşlarınıza göndermek veya bilgisayarınıza zahmetsizce kurmak için en pratik yöntemdir:

1. [Releases](https://github.com/Hakantes/videodw/releases/tag/v2.0.1) sayfasından [VideoDownloader-Setup.exe](https://github.com/Hakantes/videodw/releases/download/v2.0.1/VideoDownloader-Setup.exe) dosyasını indirin.
3. İndirdiğiniz kurulum dosyasına çift tıklayın.
4. Kurulum sihirbazındaki adımları izleyin (*İleri > Kur*).
5. Kurulum tamamlandığında masaüstünüze ve Başlat Menünüze kısayol eklenir; uygulama hemen açılmaya hazırdır!

> **💡 Not:** Kurulum paketi `yt-dlp` ve `FFmpeg` motorlarını otomatik olarak kendi içinde barındırır. Bilgisayarınıza **Python, FFmpeg veya harici hiçbir araç kurmanız gerekmez**.

---

### 💼 Seçenek 2: Kurulumsuz Taşınabilir Sürüm (Portable)
Herhangi bir kurulum yapmadan, USB bellekte veya istediğiniz klasörde doğrudan çalıştırmak isterseniz:

1. [Releases](https://github.com/Hakantes/videodw/releases/tag/v2.0.1) sayfasından **`YouTubeDownloader_v2.0.1_Windows.zip`** dosyasını indirin.
2. ZIP arşivini bir klasöre çıkartın.
3. Klasör içindeki **`youtube dowload.exe`** dosyasına çift tıklayarak doğrudan kullanın.

---

## 🔒 Dosya Bütünlüğü ve Güvenlik (SHA-256 Checksums)

İndirdiğiniz dosyaların orijinalliğini doğrulamak için aşağıdaki SHA-256 karma değerlerini kullanabilirsiniz:

* **`VideoDownloader-Setup.exe`:**
  ```text
  41CDF5DA9F2BB66D2A1BA0C1E5CB1112B4FFA840DEF9EF343316529CD3C82A95
  ```
* **`YouTubeDownloader_v2.0.1_Windows.zip`:**
  ```text
  0D39FE03C620A739D625947A57F201583D1614F93D56C3BD4C4917E9612B0CEC
  ```

---

## 🖥️ Sistem Gereksinimleri

* **İşletim Sistemi:** Windows 10 veya Windows 11 (64-bit / 32-bit uyumlu)
* **.NET Sürümü:** .NET Framework 4.8.1 *(Windows 11 ve güncel Windows 10 sistemlerinde hazır gelir; eksikse kurulum sihirbazı otomatik olarak resmi Microsoft sayfasına yönlendirir)*
* **İnternet Bağlantısı:** Video indirmek için aktif internet bağlantısı.

---

## 🚀 Öne Çıkan Özellikler

1. **Gelişmiş Video ve Ses İndirme Motoru:**
   - **yt-dlp** (güncel YouTube API değişikliklerine ve bot korumalarına tam uyumlu).
   - YouTube haricinde Vimeo, Twitter/X, Dailymotion, Facebook, Instagram vb. platformları da destekler.
   - YouTube bot koruması ve oturum gereksinimleri için **Tarayıcı Çerezleri** (Chrome, Edge, Firefox, Brave vb.) seçeneği.
   - Tek tıkla indirme motorunu güncelleme (`yt-dlp -U`) desteği.

2. **Dinamik Çözünürlük ve Format Seçimi:**
   - Video analiz edildiğinde mevcut gerçek çözünürlükler tespit edilir (4K Ultra HD 2160p, 2K Quad HD 1440p, Full HD 1080p, HD 720p, 480p, 360p).
   - Kaynakta bulunmayan sahte çözünürlükler listelenmez.
   - "En Yüksek Kalite (Otomatik)" modu ile en yüksek video ve ses akışı otomatik seçilir.

3. **Gerçek Kalite ve FFmpeg Entegrasyonu:**
   - 1080p, 2K ve 4K videolarda ayrı gelen yüksek kaliteli video ve ses akışları **FFmpeg** ile kayıpsız olarak MP4 kapsayıcısına birleştirilir (lossless remux).
   - Gereksiz yeniden kodlama (re-encode) yapılmaz, orijinal kalite korunur.
   - Yalnızca ses istendiğinde FFmpeg ile yüksek kaliteli **MP3 (320 kbps)** dönüştürmesi yapılır.

4. **Modern ve Kararlı Masaüstü Arayüzü:**
   - Video başlığı, süresi, kanalı ve önizleme küçük resmi (thumbnail).
   - Canlı ilerleme çubuğu (ProgressBar), indirme yüzdesi, anlık indirme hızı ve tahmini kalan süre (ETA).
   - Güvenli iptal etme (Cancel) desteği (arka plan süreçleri askıda kalmaz).
   - İndirme tamamlandığında tek tıkla **Dosyayı Aç** ve **Klasörde Göster** butonları.
   - Pano (Clipboard) entegrasyonu ("📋 Yapıştır").
   - İndirme klasörü seçimi ve ayarların kalıcı olarak saklanması.

5. **Güvenilirlik ve Hata Yönetimi:**
   - Windows geçersiz dosya adı karakterleri otomatik temizlenir.
   - Aynı isimli videoların çakışmaması için `[VideoID]` şablonu ve `--no-overwrites` koruması.
   - Arayüz asla donmaz (`async/await` ve arka plan akış yönetimi).
   - Detaylı hata ve olay kayıtları için `logs/downloader.log` mekanizması.

---

## 🛠️ Geliştirici ve Derleme Bilgileri

Kaynak koddan kendiniz derlemek isterseniz:

```powershell
# Paketleri geri yükle
nuget restore "src/youtube-downloader/youtube dowload.sln"

# Release modunda derle
msbuild "src/youtube-downloader/youtube dowload.sln" /p:Configuration=Release
```

Kurulum dosyasını derlemek için Inno Setup 6 ile:
```powershell
iscc "installer.iss"
```

---

## 📋 Değişiklik Özeti (v2.0 -> v2.0.1)

* 🛠️ **YouTube Bot Koruması Çözümü:** `Sign in to confirm you're not a bot` hatası için özel algılama ve tarayıcı çerezleri (`--cookies-from-browser`) desteği eklendi.
* 📦 **Inno Setup Kurulum Sihirbazı:** Tek tıkla kurulan `VideoDownloader-Setup.exe` paketi oluşturuldu.
* 🛡️ **Dosya Çakışma Koruması:** İndirilen dosya adlarına `[VideoID]` etiketi eklenerek aynı isimli videoların birbirini ezmesi önlendi.
* ⚡ **Süreç İptal İyileştirmesi:** İptal butonuna basıldığında tüm alt süreçlerin (FFmpeg ve yt-dlp) anında sonlandırılması sağlandı.
