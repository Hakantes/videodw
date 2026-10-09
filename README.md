# 🎬 YouTube ve Video İndirici v2.0.2 (Windows)

Modern Windows Forms (.NET Framework 4.8.1) mimarisinde geliştirilmiş, **yt-dlp**, **FFmpeg** ve **ffprobe** destekli, yüksek kaliteli ve Windows Media Player ile %100 uyumlu video ve ses indirme uygulaması.

---

## ⚡ Hızlı ve Kolay Kurulum

Uygulamayı kullanmak için iki kolay seçenek bulunmaktadır:

### 📦 Seçenek 1: Otomatik Kurulum Sihirbazı (Önerilen)
Arkadaşlarınıza göndermek veya bilgisayarınıza zahmetsizce kurmak için en pratik yöntemdir:

1. [Releases](https://github.com/Hakantes/videodw/releases/tag/v2.0.2) sayfasından **`VideoDownloader-Setup.exe`** dosyasını indirin.
2. İndirdiğiniz kurulum dosyasına çift tıklayın.
3. Kurulum sihirbazındaki adımları izleyin (*İleri > Kur*).
4. Kurulum tamamlandığında masaüstünüze ve Başlat Menünüze kısayol eklenir; uygulama hemen açılmaya hazırdır!

> **💡 Not:** Kurulum paketi `yt-dlp`, `FFmpeg` ve `ffprobe` motorlarını otomatik olarak kendi içinde barındırır. Bilgisayarınıza **Python, FFmpeg, ffprobe veya harici hiçbir araç kurmanız gerekmez**.

---

### 💼 Seçenek 2: Kurulumsuz Taşınabilir Sürüm (Portable)
Herhangi bir kurulum yapmadan, USB bellekte veya istediğiniz klasörde doğrudan çalıştırmak isterseniz:

1. [Releases](https://github.com/Hakantes/videodw/releases/tag/v2.0.2) sayfasından **`YouTubeDownloader_v2.0.2_Windows.zip`** dosyasını indirin.
2. ZIP arşivini bir klasöre çıkartın.
3. Klasör içindeki **`youtube dowload.exe`** dosyasına çift tıklayarak doğrudan kullanın.

---

## 🔒 Dosya Bütünlüğü ve Güvenlik (SHA-256 Checksums)

İndirdiğiniz dosyaların orijinalliğini doğrulamak için aşağıdaki SHA-256 karma değerlerini kullanabilirsiniz:

* **`VideoDownloader-Setup.exe`:**
  ```text
  1084E1007728E3011787E72F47BD01989B5C5A37EF1DB3C3E1A57BD337BBF19B
  ```
* **`YouTubeDownloader_v2.0.2_Windows.zip`:**
  ```text
  81E2D2707A5968E2439C07792D8774AA7E20AAC0D88BDB90FF59E3E2CF54DA4A
  ```

---

## 🖥️ Sistem Gereksinimleri

* **İşletim Sistemi:** Windows 10 veya Windows 11 (64-bit / 32-bit uyumlu)
* **.NET Sürümü:** .NET Framework 4.8.1 *(Windows 11 ve güncel Windows 10 sistemlerinde hazır gelir; eksikse kurulum sihirbazı otomatik olarak resmi Microsoft sayfasına yönlendirir)*
* **İnternet Bağlantısı:** Video indirmek için aktif internet bağlantısı.

---

## 🎯 Windows Medya Oynatıcı Uyumluluğu & Profil Seçimi

### ⚠️ Siyah Ekran / Sadece Ses Çalma Sorunu Nedir? (Kapsayıcı vs. Kodek Ayrımı)
Bir dosyanın uzantısının `.mp4` olması, o dosyanın her Windows bilgisayarda varsayılan oynatıcıyla (Windows Media Player, Filmler ve TV) açılabileceği anlamına gelmez. `.mp4` yalnızca bir **kapsayıcıdır (container)**; içerisindeki asıl video verisi **AV1 (`av01`)**, **VP9 (`vp09`)** veya **H.264 (`avc1`)** kodekleriyle kodlanmış olabilir:
- YouTube, 1080p ve üzerindeki yüksek çözünürlüklerde bant genişliği tasarrufu için varsayılan olarak yeni nesil **AV1** kodekini sunar.
- Windows Media Player, sistemde Microsoft Store "AV1 Video Extension" eklentisi yüklü değilse AV1 videosunu çözemez; bu nedenle **yalnızca ses çalar ve ekran siyah kalır**.

### 🛡️ v2.0.2 Çözümü: İki Akıllı İndirme Profili

Uygulama arayüzünde iki ayrı profil sunulmaktadır:

1. **Windows Uyumlu MP4 (H.264 / AAC) — Varsayılan ve Önerilen:**
   - YouTube'dan doğrudan donanımsal olarak desteklenen yerel **H.264 (`avc1`)** video ve **AAC (`mp4a`)** ses akışlarını çeker.
   - Dosyayı yeniden kodlamadan (re-encoding olmadan), kayıpsız ve maksimum hızda birleştirir.
   - İndirilen video istisnasız tüm Windows sürümlerinde (Windows Media Player, Filmler ve TV), eski televizyonlarda, otomobil teyplerinde ve telefonlarda sorunsuz açılır.
   - *Eğer seçilen çözünürlükte yerel H.264 akışı bulunmuyorsa:* Kullanıcıya açık bir diyalog penceresi sunulur; kullanıcı isterse video FFmpeg ile standart H.264'e dönüştürülür veya En İyi Kalite profiliyle indirilir. Asla AV1 akışı H.264 diye sahte etiketlenmez.

2. **En İyi Kalite (Best Quality - AV1/VP9):**
   - Kaynakta bulunan en yüksek bit hızına ve çözünürlüğe sahip akışları (4K/2K AV1 veya VP9) indirir.
   - VLC Media Player, MPC-HC, PotPlayer gibi güncel codec paketlerine sahip oynatıcıları tercih eden veya AV1 eklentisi kurulu kullanıcılar için idealdir.

---

## 🔍 Gerçek Medya Doğrulaması (`ffprobe`)

İndirme tamamlandığında dosya sadece uzantısına bakılarak "başarılı" kabul edilmez:
- Entegre **`ffprobe.exe`** aracı arka planda çalıştırılarak indirilen dosyanın akışları denetlenir.
- Video akışının gerçekten var olduğu, genişlik/yükseklik değerlerinin geçerli olduğu ve video/ses kodeklerinin sağlamlığı doğrulanır.
- Bozuk, eksik veya video akışı olmayan indirmeler anında tespit edilir.

---

## 🚀 Öne Çıkan Özellikler

1. **Gelişmiş Video ve Ses İndirme Motoru:**
   - **yt-dlp** (güncel YouTube API değişikliklerine ve bot korumalarına tam uyumlu).
   - YouTube haricinde Vimeo, Twitter/X, Dailymotion, Facebook, Instagram vb. platformları da destekler.
   - YouTube bot koruması ve oturum gereksinimleri için **Tarayıcı Çerezleri** (Chrome, Edge, Firefox, Brave vb.) seçeneği.
   - Tek tıkla indirme motorunu güncelleme (`yt-dlp -U`) desteği.

2. **Dinamik Çözünürlük ve Format Seçimi:**
   - Video analiz edildiğinde mevcut gerçek çözünürlükler tespit edilir (4K Ultra HD 2160p, 2K Quad HD 1440p, Full HD 1080p, HD 720p, 480p, 360p).
   - Profil seçiminize göre mevcut H.264 çözünürlükleri ayrı ve şeffaf biçimde listelenir.
   - Kaynakta bulunmayan sahte çözünürlükler listelenmez.

3. **FFmpeg & ffprobe Entegrasyonu:**
   - Yüksek kaliteli video ve ses akışları **FFmpeg** ile kayıpsız olarak MP4 kapsayıcısına birleştirilir (lossless remux).
   - İndirme tamamlandığında **ffprobe** ile akış ve kodek doğrulaması yapılır.
   - Yalnızca ses istendiğinde FFmpeg ile yüksek kaliteli **MP3 (320 kbps)** dönüştürmesi yapılır.

4. **Modern ve Kararlı Masaüstü Arayüzü:**
   - Profil seçimi (Windows Uyumlu MP4 vs En İyi Kalite) ve her profil için açıklama metinleri.
   - Video başlığı, süresi, kanalı, video kodekleri (`avc1`, `av01`, `vp09`) ve ses kodekleri detayı.
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

Birim ve entegrasyon testlerini çalıştırmak için:
```powershell
csc /target:exe /out:TestRunner.exe tests/TestRunner.cs /reference:Publish/AngleSharp.dll,Publish/Microsoft.Bcl.AsyncInterfaces.dll,Publish/System.Buffers.dll,Publish/System.Memory.dll,Publish/System.Numerics.Vectors.dll,Publish/System.Runtime.CompilerServices.Unsafe.dll,Publish/System.Text.Encoding.CodePages.dll,Publish/System.Text.Encodings.Web.dll,Publish/System.Text.Json.dll,Publish/System.Threading.Tasks.Extensions.dll,Publish/System.ValueTuple.dll,Publish/YoutubeExplode.dll,"Publish/youtube dowload.exe"
./TestRunner.exe
```

---

## 📋 Değişiklik Özeti (v2.0.1 -> v2.0.2)

* 🎬 **Windows Uyumlu MP4 (H.264/AAC) Profili:** Windows Media Player'daki siyah ekran / AV1 sorununu çözen yerel H.264/AAC akış seçici varsayılan profil olarak eklendi.
* ⚡ **En İyi Kalite (AV1/VP9) Profili:** Yüksek çözünürlük ve modern oynatıcılar için orijinal ham akış seçeneği korundu.
* 🔄 **Akıllı Yeniden Kodlama (Re-encode) Güvencesi:** Seçilen çözünürlükte yerel H.264 bulunmadığında kullanıcı onayıyla FFmpeg (`libx264` + `aac`) dönüştürmesi sunuldu; sessizce sahte etiketleme engellendi.
* 🔍 **ffprobe Medya Doğrulama:** İndirilen dosyaların geçerli video akışına ve doğru kodeklere sahip olduğunu doğrulayan `ValidateMediaFileAsync` eklendi.
* 📦 **Kurulum Paketine ffprobe Dahil Edildi:** Inno Setup kurulumu ve Portable ZIP paketine `ffprobe.exe` entegre edildi.
* 🧪 **14/14 Otomatik Test Kapsamı:** Format seçiciler, H.264 tespiti, ffprobe akış doğrulaması ve hata simülasyonları için kapsamlı test paketi eklendi.
