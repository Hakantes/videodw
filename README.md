# 🎬 YouTube ve Video İndirici v2.0.3 (Windows)

Modern Windows Forms (.NET Framework 4.8.1) mimarisinde geliştirilmiş, **yt-dlp**, **FFmpeg** ve **ffprobe** destekli, gerçek zamanlı canlı ilerleme/hız göstergeli, indirme öncesi tahmini dosya boyutu hesaplayan, yüksek kaliteli ve Windows Media Player ile %100 uyumlu video ve ses indirme uygulaması.

---

## ⚡ Hızlı ve Kolay Kurulum

Uygulamayı kullanmak için iki kolay seçenek bulunmaktadır:

### 📦 Seçenek 1: Otomatik Kurulum Sihirbazı (Önerilen)
Arkadaşlarınıza göndermek veya bilgisayarınıza zahmetsizce kurmak için en pratik yöntemdir:

1. [Releases](https://github.com/Hakantes/videodw/releases/tag/v2.0.3) sayfasından [VideoDownloader-Setup.exe](https://github.com/Hakantes/videodw/releases/download/v2.0.3/VideoDownloader-Setup.exe) dosyasını indirin.
2. İndirdiğiniz kurulum dosyasına çift tıklayın.
3. Kurulum sihirbazındaki adımları izleyin (*İleri > Kur*).
4. Kurulum tamamlandığında masaüstünüze ve Başlat Menünüze kısayol eklenir; uygulama hemen açılmaya hazırdır!

> **💡 Not:** Kurulum paketi `yt-dlp`, `FFmpeg` ve `ffprobe` motorlarını otomatik olarak kendi içinde barındırır. Bilgisayarınıza **Python, FFmpeg, ffprobe veya harici hiçbir araç kurmanız gerekmez**.

---

### 💼 Seçenek 2: Kurulumsuz Taşınabilir Sürüm (Portable)
Herhangi bir kurulum yapmadan, USB bellekte veya istediğiniz klasörde doğrudan çalıştırmak isterseniz:

1. [Releases](https://github.com/Hakantes/videodw/releases/tag/v2.0.3) sayfasından [YouTubeDownloader_v2.0.3_Windows.zip](https://github.com/Hakantes/videodw/releases/download/v2.0.3/YouTubeDownloader_v2.0.3_Windows.zip) dosyasını indirin.
2. ZIP arşivini bir klasöre çıkartın.
3. Klasör içindeki **`youtube dowload.exe`** dosyasına çift tıklayarak doğrudan kullanın.

---

## 🔒 Dosya Bütünlüğü ve Güvenlik (SHA-256 Checksums)

İndirdiğiniz dosyaların orijinalliğini doğrulamak için aşağıdaki SHA-256 karma değerlerini kullanabilirsiniz:

* **`VideoDownloader-Setup.exe`:**
  ```text
  F98FB65EB3AD7D3A8D51212914D5940BF7AC139504565F266396CBFA4095B583
  ```
* **`YouTubeDownloader_v2.0.3_Windows.zip`:**
  ```text
  13F0AAFEEC1CCCC04C16F7F94EFAE319262D0BFF69E85DD31141D7810E3D063A
  ```

---

## 🖥️ Sistem Gereksinimleri

* **İşletim Sistemi:** Windows 10 veya Windows 11 (64-bit / 32-bit uyumlu)
* **.NET Sürümü:** .NET Framework 4.8.1 *(Windows 11 ve güncel Windows 10 sistemlerinde hazır gelir; eksikse kurulum sihirbazı otomatik olarak resmi Microsoft sayfasına yönlendirir)*
* **İnternet Bağlantısı:** Video indirmek için aktif internet bağlantısı.

---

## 📊 Canlı İndirme İlerlemesi, Hız ve Boyut Göstergesi (v2.0.3)

Uygulama, indirme sırasında kullanıcıyı her aşamada şeffaf biçimde bilgilendirir:

### 1. İndirme Öncesi Tahmini Dosya Boyutu
- Video analiz edildiğinde seçilen çözünürlük, profil ve ses durumuna göre dosya boyutu henüz indirme başlamadan hesaplanır (örneğin: `Tahmini Boyut: ~145,6 MB`).
- Video ve ses ayrı indirilecekse her iki akışın boyutu birlikte hesaplanır; H.264 dönüştürmesi gerekiyorsa dönüştürme uyarısıyla birlikte sunulur.

### 2. Gerçek Zamanlı ve Kademeli İlerleme Çubuğu
- İndirme başladığı andan itibaren progress bar ve yüzde göstergesi anlık olarak artar (%0 ➔ %12 ➔ %37 ➔ %68 ➔ %94 ➔ %100).
- Zamanlayıcıyla yapay artırma veya uydurma yüzde üretilmez; yt-dlp motorundan okunan gerçek transfer verileri kullanılır.
- Çok akışlı indirmelerde (video ve ses ayrı indirildiğinde) ses akışı video akışının üzerine yazılmaz; her iki akış birleştirilerek toplam kümülatif yüzde hesaplanır.

### 3. İndirilen Boyut ve Toplam Boyut
- Anlık olarak indirilen veri ve dosyanın toplam boyutu Türkçe biçimlendirme ile sunulur:
  `İndirilen: 125,40 MB / 500,00 MB (%25)`
- Sunucunun toplam boyutu önceden bildirmediği canlı veya parçalı akışlarda sahte tahminler yapılmaz:
  `İndirilen: 125,40 MB | Toplam boyut hesaplanıyor`
- Boyut birimleri otomatik olarak `B`, `KB`, `MB`, `GB` cinsinden iki ondalık basamakla gösterilir.

### 4. Anlık İndirme Hızı (EMA Yumuşatmalı)
- Canlı indirme hızı `B/sn`, `KB/sn`, `MB/sn` cinsinden gösterilir:
  `Hız: 8,45 MB/sn`
- Anlık paket gecikmelerinden dolayı hızın aşırı dalgalanmasını önlemek için **Üstel Hareketli Ortalama (EMA)** filtresi uygulanır.

### 5. Tahmini Kalan Süre (ETA)
- Kalan veri miktarı ve anlık hız kullanılarak kalan süre hesaplanır:
  `Kalan süre: 00:42` (veya 1 saatten uzunsa `01:15:30`)
- İndirme tamamlandığında:
  `İndirme tamamlandı`

### 6. Çok Aşamalı İndirme Yönetimi
- **Video indiriliyor...** (Görüntü akışı indirilirken)
- **Ses indiriliyor...** (Ses akışı indirilirken)
- **Ses ve video birleştiriliyor...** (FFmpeg kayıpsız birleştirme yaparken)
- **Dosya doğrulanıyor...** (ffprobe ile akış ve kodek denetimi yapılırken)
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
csc /target:exe /out:src/youtube-downloader/bin/Release/TestRunner.exe tests/TestRunner.cs /reference:"src/youtube-downloader/bin/Release/youtube dowload.exe","src/youtube-downloader/bin/Release/System.Text.Json.dll","src/youtube-downloader/bin/Release/System.Memory.dll","src/youtube-downloader/bin/Release/System.Buffers.dll","src/youtube-downloader/bin/Release/Microsoft.Bcl.AsyncInterfaces.dll",System.Core.dll,System.dll,System.Windows.Forms.dll
./src/youtube-downloader/bin/Release/TestRunner.exe
```

---

## 📋 Değişiklik Özeti

### v2.0.3 Yenilikleri
* ⚡ **Canlı İlerleme ve Hız Sistemi:** yt-dlp `--progress-template` JSON mimarisiyle sıfırdan yapılandırıldı; yüzde doğrudan %100'e atlamadan canlı olarak güncellenir.
* 📦 **İndirme Öncesi Boyut Tahmini:** URL analiz edildiğinde seçili kalite ve profile göre beklenen dosya boyutu indirme başlamadan gösterilir.
* 📊 **Gerçek İndirilen ve Toplam Boyut:** `İndirilen: 125,40 MB / 500,00 MB (%25)` ve dinamik birim (`B`, `KB`, `MB`, `GB`) dönüşümü eklendi.
* 🚀 **Anlık Hız ve EMA Filtreleme:** Hız dalgalanmalarını yumuşatan Exponential Moving Average destekli `Hız: 8,45 MB/sn` göstergesi eklendi.
* ⏱️ **Akıllı Kalan Süre (ETA):** Sıfıra bölme ve negatif değer korumalı anlık kalan süre göstergesi eklendi.
* 🔄 **Çok Aşamalı İndirme Yönetimi:** Video indiriliyor, ses indiriliyor, birleştiriliyor, doğrulanıyor ve tamamlandı durumları ayrıştırıldı; birleştirme ve doğrulama sırasında yapay %99 sıçramaları engellendi.
* 🛡️ **UI Thread Güvenliği ve Throttling:** WinForms arayüzünün donmasını önleyen olay birleştirme ve güvenli marshal mekanizması sağlandı.
* 🧪 **38/38 Otomatik Test Kapsamı:** Tüm ilerleme, hız, birim, çok akışlı kümülatif geçiş, süre sınırları ve hata durumları için test senaryoları doğrulandı.

### v2.0.2
* 🎬 Windows Uyumlu MP4 (H.264/AAC) Profili ve WMP siyah ekran sorunu çözümü.
* 🔍 ffprobe ile gerçek medya doğrulama entegrasyonu.

---

## 📜 Lisans ve Üçüncü Taraf Bildirimleri (Third-Party Notices)

Bu yazılım açık kaynak kodlu bileşenler içermekte veya bunlarla birlikte dağıtılmaktadır:

* **yt-dlp:** [The Unlicense](https://github.com/yt-dlp/yt-dlp/blob/master/LICENSE) (Kamu Malı / Public Domain)
* **FFmpeg / ffprobe:** [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html) (Gyan.dev GPL release derlemesi)
* **AngleSharp:** [MIT License](https://github.com/AngleSharp/AngleSharp/blob/devel/LICENSE)
* **YoutubeExplode:** [LGPL v3.0 License](https://github.com/Tyrrrz/YoutubeExplode/blob/master/License.txt)
* **System.Text.Json / Microsoft Bcl:** [MIT License](https://github.com/dotnet/runtime/blob/main/LICENSE.TXT)
* **NReco.VideoConverter.LT:** [NReco Component License](https://www.nrecosite.com/video_converter_net.aspx)
