# 🍏 YouTube ve Video İndirici — macOS (Apple Silicon M1/M2/M3/M4) Kurulum ve Güvenlik Kılavuzu

Bu belge, Apple Silicon mimarisine sahip macOS bilgisayarlarda **YouTube ve Video İndirici v2.1.0** uygulamasının kurulumunu, ilk açılış güvenlik izinlerini ve motor gereksinimlerini açıklar.

---

## 📌 Desteklenen Platform ve Sistem Gereksinimleri

- **İşlemci Mimarisi:** Apple Silicon (**M1, M2, M3, M4** veya üzeri ARM64).
  > **ℹ️ Intel Mac Durumu:** Bu sürüm doğrudan Apple Silicon (ARM64) mimarisi için optimize edilmiştir. Intel (x86_64) işlemcili eski Mac modelleri için bu sürümde resmi derleme ve test yapılmamıştır; resmi olarak desteklenmemektedir.
- **İşletim Sistemi:** macOS 11.0 (Big Sur) veya daha yeni bir sürüm (macOS 12 Monterey, 13 Ventura, 14 Sonoma, 15 Sequoia desteklenir).
- **Ek Bileşenler:** `yt-dlp` ve `ffmpeg` (Video indirme, birleştirme ve MP3 dönüştürme için).
- **Test Durumu:** .NET 8.0 `osx-arm64` Release derlemesi ve bundle yapısı Windows ortamında %100 doğrulanmıştır. Fiziksel Apple Silicon Mac donanımında son kullanıcı testi: **BEKLENİYOR / BLOCKED**.

---

## 🔒 Apple Developer Notarization ve Gatekeeper Bilgilendirmesi

Bu proje bağımsız, açık kaynaklı ve topluluk için tamamen ücretsiz bir yazılımdır:
- Apple'ın yıllık \$99 tutarındaki ücretli geliştirici hesabı ve Developer ID Notarization süreci **kullanılmamaktadır**.
- Bunun yerine indirme paketlerinin güvenliği ve orijinalliği **SHA-256 sağlama özetleri** ve **RSA-2048 dijital imzaları** ile korunmaktadır.
- Bu nedenle macOS, ilk açılışta Apple Notarization sunucusunda kayıtlı olmayan tüm bağımsız uygulamalarda olduğu gibi standart bir güvenlik uyarısı gösterecektir (*"Geliştirici doğrulanamadığı için açılamıyor"* veya *"macOS cannot verify the developer"*).
- **ÖNEMLİ GÜVENLİK UYARISI:** Gatekeeper'ı sistem genelinde devre dışı bırakacak komutları (`sudo spctl --master-disable` vb.) **KESİNLİKLE ÇALIŞTIRMAYIN**. Bu komut Mac'inizin tüm sistem güvenliğini savunmasız bırakır. Aşağıdaki tek seferlik standart macOS izin adımı yeterlidir ve tamamen güvenlidir.

---

## 🚀 Kurulum Yöntemleri

Kullanım tercihinize göre iki dağıtım formatından birini seçebilirsiniz:

> **ℹ️ Dağıtım Paketi Notu:** GitHub Release yayımlandığında hem DMG hem ZIP paketleri doğrudan indirilebilir olacaktır. Projeyi yerel ortamda derliyorsanız; Windows ortamında doğrudan üretilen ve doğrulanan paket `YouTubeDownloader_v2.1.0_macOS_ARM64.zip` paketidir. DMG paketi ise macOS ortamında `scripts/create-dmg.sh` ile veya GitHub Actions CI üzerinde üretilmektedir.

### Yöntem 1: DMG Kolay Kurulum İmajı (Önerilen)
1. GitHub Releases sayfasından **`YouTubeDownloader_v2.1.0_macOS_ARM64.dmg`** dosyasını indirin.
2. İndirilen DMG dosyasına çift tıklayarak sanal diski bağlayın.
3. Açılan pencerede **`YouTube Downloader.app`** simgesini yanındaki **`Applications` (Uygulamalar)** kısayol klasörüne sürükleyip bırakın.
4. Masaüstündeki veya Finder sol sütunundaki disk imajını sağ tıklayıp **Çıkar (Eject)** deyin.

---

### Yöntem 2: ZIP Taşınabilir Arşiv (Alternatif)
1. GitHub Releases sayfasından **`YouTubeDownloader_v2.1.0_macOS_ARM64.zip`** dosyasını indirin.
2. ZIP arşivine çift tıklayarak içeriği çıkartın.
3. Çıkan **`YouTube Downloader.app`** uygulamasını **Uygulamalar (`/Applications`)** klasörüne taşıyın.

---

## 🛡️ İlk Açılış ve Gatekeeper İzni (Yalnızca İlk Seferde)

Uygulama Apple tarafından ücretli mühürlenmediği için ilk açılışta macOS güvenlik onayı ister. Bu onay iki farklı şekilde verilebilir:

### 🟢 Seçenek A: Finder Grafik Arayüzü ile (Varsayılan ve Önerilen Yöntem)
1. **Finder**'ı açın ve sol menüden **Uygulamalar (`Applications`)** klasörüne gidin.
2. `YouTube Downloader.app` uygulaması üzerine **Control tuşuna basılı tutarak tıklayın** (veya farenin sağ tuşuyla tıklayın).
3. Açılan açılır menünün en üstündeki **Aç (`Open`)** seçeneğine tıklayın. *(Doğrudan çift tıklamayın; sağ tık > Aç menüsünü kullanın).*
4. Ekrana gelen güvenlik uyarısı iletişim kutusunda **Aç (`Open`)** butonuna basın.
5. *macOS bu tercihi kalıcı olarak kaydeder. Sonraki tüm kullanımlarda uygulama doğrudan çift tıklamayla veya Spotlight ile açılır.*

> **💡 Alternatif Grafik Yöntem:** Eğer doğrudan çift tıklayıp uyarı aldıysanız; Mac'inizde **Sistem Ayarları (`System Settings`) > Gizlilik ve Güvenlik (`Privacy & Security`)** menüsüne gidin. En altta *"YouTube Downloader engellendi"* bildirimini göreceksiniz. Yanındaki **Yine de Aç (`Open Anyway`)** butonuna tıklayarak onay verin.

---

### ⚪ Seçenek B: Terminal ile Karantina Temizleme (İleri Düzey Kullanıcılar İçin Alternatif)
Grafik arayüz yerine komut satırı kullanmak isterseniz, Mac Terminal uygulamasında yalnızca bu uygulamanın indirme karantinasını kaldırmak için şu komutu çalıştırabilirsiniz:
```bash
xattr -cr "/Applications/YouTube Downloader.app"
```
*(Bu komut sistem güvenliğine dokunmaz; yalnızca indirilen uygulamanın üzerindeki 'com.apple.quarantine' etiketini kaldırır).*

---

## ⚙️ İndirme Motorlarını Yükleme (Homebrew)

macOS üzerinde video formatlarını indirmek, birleştirmek ve MP3'e dönüştürmek için sistemde `yt-dlp` ve `ffmpeg` kurulu olmalıdır:

1. Mac **Terminal** uygulamasını açın.
2. Mac'inizde Homebrew kuruluysa şu tek komutu çalıştırın:
   ```bash
   brew install yt-dlp ffmpeg
   ```
3. *(Mac'inizde Homebrew henüz kurulu değilse, önce resmi Homebrew kurulum komutunu çalıştırabilirsiniz)*:
   ```bash
   /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
   ```
4. Uygulama açılışta `/opt/homebrew/bin/` yolundaki araçları otomatik olarak algılar. Motorlar hazır olduğunda arayüzde doğrudan indirme yapabilirsiniz.

---

## 🔄 Otomatik Güncelleme Sistemi ve Dosya Güvenliği

- **Denetim:** Uygulama açılışta günde en fazla 1 kez arka planda `update-manifest.json` dosyasını sorgular. İstenirse sağ üstteki **"🔄 Güncellemeleri Denetle"** butonuyla anında manuel kontrol yapılabilir.
- **Güvenlik Doğrulaması:** İndirilen güncelleme paketi RSA-2048 dijital imzası ve SHA-256 özetiyle doğrulanır. Bozuk veya tahrif edilmiş paketler kuruluma alınmaz ve silinir.
- **Uygulama Güncellemesi:** Yeni sürüm onaylandığında geçici bir kabuk betiği başlatılır, mevcut uygulamanın kapanması beklenir, `/Applications/YouTube Downloader.app` yeni sürümle değiştirilir, karantina etiketi temizlenir ve yeni sürüm başlatılır.
- **Kurtarma Davranışı:** İndirme veya doğrulama sırasında herhangi bir hata oluşursa çalışan mevcut uygulamaya dokunulmaz; kullanıcı mevcut sürümü kullanmaya devam eder.
- **Kullanıcı Ayarları:** Tercihleriniz `~/Library/Application Support/YouTubeVideoDownloader/settings.json` altında tutulur. Uygulama paketi değişse dahi indirme geçmişiniz ve tercihleriniz korunur.

---

## ❓ Sık Karşılaşılan Sorunlar ve Çözümleri

| Belirti / Hata | Olası Neden | Kesin Çözüm |
| :--- | :--- | :--- |
| *"Geliştirici doğrulanamadı"* | Apple Notarization bulunmaması | Finder'da uygulamaya **Sağ tıklayın (Control+Tık)** ve **Aç** deyin. |
| *"yt-dlp ve FFmpeg bulunamadı"* | Homebrew araçlarının eksik olması | Terminal'de `brew install yt-dlp ffmpeg` komutunu çalıştırın. |
| QuickTime'da video siyah ekran veriyor | Orijinal video AV1 kodeki içeriyor | İndirme menüsünden `1080p (MP4 - macOS / QuickTime Uyumlu)` seçin. |
| İndirilen video nereye kaydedildi? | Varsayılan indirme konumu | Varsayılan olarak `~/Downloads` (İndirilenler) klasörüne kaydedilir. Arayüzden klasörü değiştirebilirsiniz. |
