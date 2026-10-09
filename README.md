# YouTube ve Video İndirici v2.0 (Windows Masaüstü Uygulaması)

Modern Windows Forms (.NET Framework 4.8.1) mimarisinde geliştirilmiş, **yt-dlp** ve **FFmpeg** destekli yüksek performanslı video ve ses indirme uygulaması.

---

## 🚀 Öne Çıkan Özellikler

1. **Gelişmiş Video ve Ses İndirme Motoru:**
   - **yt-dlp** (güncel YouTube API değişikliklerine ve bot korumalarına tam uyumlu).
   - YouTube haricinde Vimeo, Twitter/X, Dailymotion, Facebook, Instagram vb. platformları da destekler.
   - Tek tıkla indirme motorunu güncelleme (`yt-dlp -U`) desteği.

2. **Dinamik Çözünürlük ve Format Seçimi:**
   - Video analiz edildiğinde mevcut gerçek çözünürlükler tespit edilir (4K Ultra HD 2160p, 2K Quad HD 1440p, Full HD 1080p, HD 720p, 480p, 360p).
   - Kaynakta bulunmayan sahte çözünürlükler listelenmez.
   - "En Yüksek Kalite (Otomatik)" modu ile en yüksek video ve ses akışı otomatik seçilir.

3. **Gerçek Kalite ve FFmpeg Entegrasyonu:**
   - 1080p, 2K ve 4K videolarda ayrı gelen yüksek kaliteli video ve ses akışları **FFmpeg** ile kayıpsız olarak MP4 kapsayıcısına birleştirilir (remux).
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
   - Aynı isimli dosyaların üzerine yazılması engellenir.
   - Arayüz asla donmaz (`async/await` ve arka plan akış yönetimi).
   - Detaylı hata ve olay kayıtları için `logs/downloader.log` mekanizması.

---

## 📁 Dağıtım ve Çalıştırma

### Bağımsız Çalıştırma (Taşınabilir Sürüm):
`Publish` klasörü altındaki `youtube dowload.exe` dosyasını çift tıklayarak doğrudan çalıştırabilirsiniz.
Klasör içeriği:
- `youtube dowload.exe` (Ana uygulama)
- `yt-dlp.exe` (İndirme motoru)
- `ffmpeg.exe` (Video ve ses birleştirici)
- Gerekli .NET kütüphaneleri

> **Not:** Uygulama Windows 10 ve Windows 11 üzerinde yerel olarak çalışır, ek bir Python veya geliştirme ortamı kurulumu **gerektirmez**.

---

## 🛠️ Geliştirici ve Derleme Bilgileri

- **Geliştirme Dili:** C# (.NET Framework 4.8.1)
- **Arayüz:** Windows Forms (WinForms)
- **Çözüm Dosyası:** `youtube dowload.sln`
- **Derleme Komutu:**
  ```powershell
  msbuild "youtube dowload.sln" /p:Configuration=Release
  ```

---

## 📋 Değişiklik Özeti (v1.0 -> v2.0)

| Özellik | Eski Sürüm | Yeni Sürüm (v2.0) |
|---|---|---|
| **İndirme Motoru** | YoutubeExplode 6.4 (Cipher hatası veriyordu) | Güncel yt-dlp CLI motoru (Hatasız) |
| **FFmpeg Entegrasyonu** | Kısmi / Kodlanmamış | Tam otomatik remux ve MP3 dönüştürme |
| **Çözünürlük Seçenekleri** | Yok / Sabit | 4K, 2K, 1080p, 720p, 480p, 360p dinamik |
| **Önizleme** | Yok | Küçük resim, kanal, süre ve başlık |
| **İndirme Klasörü** | Hardcoded (`C:\Users\...\ABC`) | Kullanıcı seçimi + Kalıcı saklama |
| **İptal Mekanizması** | Yok | CancellationToken & Process Tree Kill |
| **Canlı Hız & ETA** | Yok | Anlık hız, kalan süre ve indirilen boyut |
| **Hata & Loglama** | Sadece MessageBox | `logs/downloader.log` + Log butonu |
| **Motor Güncelleme** | Yok | Tek tıkla güncelleme butonu |
