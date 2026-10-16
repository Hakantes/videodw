# 🎬 YouTube ve Video İndirici v2.0.3 (Windows)

Modern Windows Forms (.NET Framework 4.8.1) mimarisinde geliştirilmiş, **yt-dlp**, **FFmpeg** ve **ffprobe** destekli, gerçek zamanlı canlı ilerleme/hız göstergeli, yüksek kaliteli ve Windows Media Player ile %100 uyumlu video ve ses indirme uygulaması.

---

## ⚡ Hızlı ve Kolay Kurulum

Uygulamayı kullanmak için iki kolay seçenek bulunmaktadır:

### 📦 Seçenek 1: Otomatik Kurulum Sihirbazı (Önerilen)
Arkadaşlarınıza göndermek veya bilgisayarınıza zahmetsizce kurmak için en pratik yöntemdir:

1. [Releases](https://github.com/Hakantes/videodw/releases/tag/v2.0.3) sayfasından **`VideoDownloader-Setup.exe`** dosyasını indirin.
2. İndirdiğiniz kurulum dosyasına çift tıklayın.
3. Kurulum sihirbazındaki adımları izleyin (*İleri > Kur*).
4. Kurulum tamamlandığında masaüstünüze ve Başlat Menünüze kısayol eklenir; uygulama hemen açılmaya hazırdır!

> **💡 Not:** Kurulum paketi `yt-dlp`, `FFmpeg` ve `ffprobe` motorlarını otomatik olarak kendi içinde barındırır. Bilgisayarınıza **Python, FFmpeg, ffprobe veya harici hiçbir araç kurmanız gerekmez**.

---

### 💼 Seçenek 2: Kurulumsuz Taşınabilir Sürüm (Portable)
Herhangi bir kurulum yapmadan, USB bellekte veya istediğiniz klasörde doğrudan çalıştırmak isterseniz:

1. [Releases](https://github.com/Hakantes/videodw/releases/tag/v2.0.3) sayfasından **`YouTubeDownloader_v2.0.3_Windows.zip`** dosyasını indirin.
2. ZIP arşivini bir klasöre çıkartın.
3. Klasör içindeki **`youtube dowload.exe`** dosyasına çift tıklayarak doğrudan kullanın.

---

## 🔒 Dosya Bütünlüğü ve Güvenlik (SHA-256 Checksums)

İndirdiğiniz dosyaların orijinalliğini doğrulamak için aşağıdaki SHA-256 karma değerlerini kullanabilirsiniz:

* **`VideoDownloader-Setup.exe`:**
  ```text
  1BC02649F07A5AD1F07160156058EFBA8DBE84658B2E90875F55E6DF89DFA222
  ```
* **`YouTubeDownloader_v2.0.3_Windows.zip`:**
  ```text
  DE665BBDA2B950455C7C6AABEEEA562772591406B80848199CC96E9CAB188912
  ```

---

## 🖥️ Sistem Gereksinimleri

* **İşletim Sistemi:** Windows 10 veya Windows 11 (64-bit / 32-bit uyumlu)
* **.NET Sürümü:** .NET Framework 4.8.1 *(Windows 11 ve güncel Windows 10 sistemlerinde hazır gelir; eksikse kurulum sihirbazı otomatik olarak resmi Microsoft sayfasına yönlendirir)*
* **İnternet Bağlantısı:** Video indirmek için aktif internet bağlantısı.

---

## 📊 Canlı İndirme İlerlemesi, Hız ve Boyut Göstergesi (v2.0.3)

Uygulama, indirme sırasında kullanıcıyı her aşamada şeffaf biçimde bilgilendirir:

### 1. Gerçek Zamanlı ve Kademeli İlerleme Çubuğu
- İndirme başladığı andan itibaren progress bar ve yüzde göstergesi anlık olarak artar (%0 ➔ %12 ➔ %37 ➔ %68 ➔ %94 ➔ %100).
- Zamanlayıcıyla yapay artırma veya uydurma yüzde üretilmez; yt-dlp motorundan okunan gerçek transfer verileri kullanılır.
- Çok akışlı indirmelerde (video ve ses ayrı indirildiğinde) ses akışı video akışının üzerine yazılmaz; her iki akış birleştirilerek toplam kümülatif yüzde hesaplanır.

### 2. İndirilen Boyut ve Toplam Boyut
- Anlık olarak indirilen veri ve dosyanın toplam boyutu Türkçe biçimlendirme ile sunulur:
  `İndirilen: 125,40 MB / 500,00 MB (%25)`
- Sunucunun toplam boyutu önceden bildirmediği canlı veya parçalı akışlarda sahte tahminler yapılmaz:
  `İndirilen: 125,40 MB | Toplam boyut hesaplanıyor`
- Boyut birimleri otomatik olarak `B`, `KB`, `MB`, `GB` cinsinden iki ondalık basamakla gösterilir.

### 3. Anlık İndirme Hızı (EMA Yumuşatmalı)
- Canlı indirme hızı `B/sn`, `KB/sn`, `MB/sn` cinsinden gösterilir:
  `Hız: 8,45 MB/sn`
- Anlık paket gecikmelerinden dolayı hızın aşırı dalgalanmasını önlemek için **Üstel Hareketli Ortalama (EMA)** filtresi uygulanır.

### 4. Tahmini Kalan Süre (ETA)
- Kalan veri miktarı ve anlık hız kullanılarak kalan süre hesaplanır:
  `Kalan süre: 00:42` (veya 1 saatten uzunsa `01:15:30`)
- İndirme tamamlandığında:
  `İndirme tamamlandı`

### 5. Çok Aşamalı İndirme Yönetimi
- **Video indiriliyor...** (Görüntü akışı indirilirken)
- **Ses indiriliyor...** (Ses akışı indirilirken)
- **Ses ve video birleştiriliyor...** (FFmpeg kayıpsız birleştirme yaparken, %99)
- **Dosya doğrulanıyor...** (ffprobe ile akış ve kodek denetimi yapılırken, %99)
- **İndirme tamamlandı** (Medya doğrulaması onaylandıktan sonra %100)

---

## 🎯 Windows Medya Oynatıcı Uyumluluğu & Profil Seçimi

### ⚠️ Siyah Ekran / Sadece Ses Çalma Sorunu Nedir? (Kapsayıcı vs. Kodek Ayrımı)
Bir dosyanın uzantısının `.mp4` olması, o dosyanın her Windows bilgisayarda varsayılan oynatıcıyla (Windows Media Player, Filmler ve TV) açılabileceği anlamına gelmez. `.mp4` yalnızca bir **kapsayıcıdır (container)**; içerisindeki asıl video verisi **AV1 (`av01`)**, **VP9 (`vp09`)** veya **H.264 (`avc1`)** kodekleriyle kodlanmış olabilir:
- YouTube, 1080p ve üzerindeki yüksek çözünürlüklerde bant genişliği tasarrufu için varsayılan olarak yeni nesil **AV1** kodekini sunar.
- Windows Media Player, sistemde Microsoft Store "AV1 Video Extension" eklentisi yüklü değilse AV1 videosunu çözemez; bu nedenle **yalnızca ses çalar ve ekran siyah kalır**.

### 🛡️ İki Akıllı İndirme Profili

1. **Windows Uyumlu MP4 (H.264 / AAC) — Varsayılan ve Önerilen:**
   - YouTube'dan doğrudan donanımsal olarak desteklenen yerel **H.264 (`avc1`)** video ve **AAC (`mp4a`)** ses akışlarını çeker.
   - Dosyayı yeniden kodlamadan (re-encoding olmadan), kayıpsız ve maksimum hızda birleştirir.
   - İndirilen video istisnasız tüm Windows sürümlerinde (Windows Media Player, Filmler ve TV), eski televizyonlarda, otomobil teyplerinde ve telefonlarda sorunsuz açılır.
   - *Yerel H.264 akışı bulunmuyorsa:* Kullanıcıya açık bir diyalog penceresi sunulur; kullanıcı isterse video FFmpeg ile standart H.264'e dönüştürülür veya En İyi Kalite profiliyle indirilir.

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
# Roslyn derleyicisi ile test paketini derle ve çalıştır
csc /target:exe /out:Publish/TestRunner.exe tests/TestRunner.cs /reference:Publish/AngleSharp.dll,Publish/Microsoft.Bcl.AsyncInterfaces.dll,Publish/System.Buffers.dll,Publish/System.Memory.dll,Publish/System.Numerics.Vectors.dll,Publish/System.Runtime.CompilerServices.Unsafe.dll,Publish/System.Text.Encoding.CodePages.dll,Publish/System.Text.Encodings.Web.dll,Publish/System.Text.Json.dll,Publish/System.Threading.Tasks.Extensions.dll,Publish/System.ValueTuple.dll,Publish/YoutubeExplode.dll,"Publish/youtube dowload.exe"
Copy-Item "Publish/youtube dowload.exe.config" "Publish/TestRunner.exe.config" -Force
./Publish/TestRunner.exe
```

---

## 📋 Değişiklik Özeti

### v2.0.3 Yenilikleri
* ⚡ **Canlı İlerleme ve Hız Sistemi:** yt-dlp `--progress-template` JSON mimarisiyle sıfırdan yapılandırıldı; yüzde doğrudan %100'e atlamadan canlı olarak güncellenir.
* 📦 **Gerçek İndirilen ve Toplam Boyut:** `İndirilen: 125,40 MB / 500,00 MB (%25)` ve dinamik birim (`B`, `KB`, `MB`, `GB`) dönüşümü eklendi.
* 🚀 **Anlık Hız ve EMA Filtreleme:** Hız dalgalanmalarını yumuşatan Exponential Moving Average destekli `Hız: 8,45 MB/sn` göstergesi eklendi.
* ⏱️ **Akıllı Kalan Süre (ETA):** Sıfıra bölme ve negatif değer korumalı anlık kalan süre göstergesi eklendi.
* 🔄 **Çok Aşamalı İndirme Yönetimi:** Video indiriliyor, ses indiriliyor, birleştiriliyor, doğrulanıyor ve tamamlandı durumları ayrıştırıldı.
* 🛡️ **UI Thread Güvenliği ve Throttling:** WinForms arayüzünün donmasını önleyen 100ms olay birleştirme ve güvenli marshal mekanizması sağlandı.
* 🧪 **26/26 Otomatik Test Kapsamı:** Tüm ilerleme, hız, birim, çok akışlı geçiş ve hata durumları için otomatik testler eklendi.

### v2.0.2
* 🎬 Windows Uyumlu MP4 (H.264/AAC) Profili ve WMP siyah ekran sorunu çözümü.
* 🔍 ffprobe ile gerçek medya doğrulama entegrasyonu.
