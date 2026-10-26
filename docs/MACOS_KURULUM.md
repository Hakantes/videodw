# 🍏 YouTube ve Video İndirici — macOS (Apple Silicon M1/M2/M3/M4) Kurulum ve Kullanım Kılavuzu

Bu belge, Apple Silicon (ARM64) mimarisine sahip macOS bilgisayarlarda **YouTube ve Video İndirici v2.1.0** uygulamasının ücretsiz ve güvenli kurulum adımlarını açıklar.

---

## 📌 Sistem Gereksinimleri

- **İşlemci:** Apple Silicon (M1, M2, M3, M4 veya üzeri ARM64)
- **İşletim Sistemi:** macOS 11.0 (Big Sur) veya daha yeni sürüm (macOS 12 Monterey, 13 Ventura, 14 Sonoma, 15 Sequoia %100 desteklenir)
- **Ek Bileşenler:** `yt-dlp` ve `ffmpeg` (Video birleştirme ve MP3 dönüştürme için)

---

## 🚀 1. Kurulum Adımları (3 Basit Adım)

### Adım 1: Uygulama Paketini İndirin ve Çıkarın
1. GitHub Releases sayfasından **`YouTubeDownloader_v2.1.0_macOS_ARM64.zip`** dosyasını indirin.
2. İndirilen ZIP dosyasına çift tıklayarak arşivden çıkarın.
3. Çıkan **`YouTube Downloader.app`** uygulamasını farenizle sürükleyip **Uygulamalar (`/Applications`)** klasörüne bırakın.

---

### Adım 2: Gatekeeper Güvenlik İznini Verin (Ücretsiz Dağıtım)

> [!NOTE]
> Uygulama açık kaynaklı ve topluluk için ücretsiz dağıtılmaktadır (Apple'ın yıllık \$99 geliştirici sertifikası yerine güvenli SHA-256 dijital doğrulaması kullanılmaktadır). Bu nedenle macOS ilk açılışta *"Geliştirici doğrulanamadı"* uyarısı gösterecektir.

Bu uyarıyı aşmanın iki kolay yolu vardır:

#### Seçenek A: Grafik Arayüz ile (Finder - Önerilen)
1. **Finder**'ı açın ve sol menüden **Uygulamalar (`Applications`)** klasörüne gidin.
2. `YouTube Downloader.app` üzerine **Control tuşuna basılı tutarak tıklayın** (veya sağ tıklayın).
3. Açılan menüden **Aç (`Open`)** seçeneğine tıklayın.
4. Ekrana gelen güvenlik uyarısı penceresinde **Aç (`Open`)** butonuna basın.
5. *macOS bu tercihi hatırlar ve sonraki açılışlarda doğrudan açılır.*

#### Seçenek B: Terminal ile (Hızlı Tek Komut)
Mac Terminal uygulamasını açın ve şu komutu yapıştırıp `Enter`a basın:
```bash
xattr -cr "/Applications/YouTube Downloader.app"
```
Bu komut, indirilen dosyadaki karantina özniteliğini temizler ve uygulamanın doğrudan açılmasını sağlar.

---

### Adım 3: İndirme Motorlarını Yükleyin (Homebrew ile)

macOS üzerinde en yüksek indirme hızı ve dönüştürme performansı için `yt-dlp` ve `ffmpeg` gereklidir:

1. Mac Terminal uygulamasını açın.
2. Mac'inizde Homebrew kuruluysa şu komutu çalıştırın:
   ```bash
   brew install yt-dlp ffmpeg
   ```
3. *(Homebrew kurulu değilse, önce şu resmi komutla kurabilirsiniz)*:
   ```bash
   /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
   ```

---

## 🔄 Otomatik Güncelleme Sistemi

macOS sürümü, Windows sürümüyle aynı merkezi ve güvenli güncelleme mekanizmasını paylaşır:

1. **Arka Plan Denetimi:** Uygulama açıldığında günde en fazla 1 kez arka planda GitHub Releases üzerinden yeni sürüm denetler.
2. **Kriptografik Güvenlik:** İndirilen manifest ve paketler RSA-2048 ve SHA-256 imzalarıyla doğrulanır; sahte veya bozuk güncellemeler engellenir.
3. **Manuel Güncelleme:** Sağ üst köşedeki **"🔄 Güncellemeleri Denetle"** butonuna tıklayarak dilediğiniz an kontrol sağlayabilirsiniz.
4. **Tek Tıkla Kurulum:** Yeni sürüm bulunduğunda **"✨ Yeni Sürüm Mevcut! Güncelle"** butonu belirir; tıklandığında güncel `.app` paketi indirilip otomatik olarak `/Applications` klasöründeki eski sürümle değiştirilir.

---

## 🛠 Sorun Giderme

| Sorun | Neden | Çözüm |
| :--- | :--- | :--- |
| *"Uygulama hasar görmüş"* uyarısı | macOS Gatekeeper karantinası | Terminal'de `xattr -cr "/Applications/YouTube Downloader.app"` çalıştırın. |
| *"yt-dlp veya FFmpeg bulunamadı"* uyarısı | Homebrew paketleri eksik | Terminal'de `brew install yt-dlp ffmpeg` komutunu çalıştırın. |
| İndirilen video QuickTime'da açılmıyor | Orijinal video AV1 codec kullanıyor | İndirme menüsünden `1080p (MP4 - macOS / QuickTime Uyumlu)` seçeneğini tercih edin. |
| Ayarlar sıfırlandı mı? | Ayar konumu korunur | Ayarlarınız `~/Library/Application Support/YouTubeVideoDownloader/settings.json` altında güvenle saklanır, güncellemelerde kaybolmaz. |
