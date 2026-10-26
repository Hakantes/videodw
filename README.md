# 🎬 YouTube ve Video İndirici v2.1.0

[![Release](https://img.shields.io/badge/Release-v2.1.0-blue.svg)](https://github.com/Hakantes/videodw/releases)
[![Platform Windows](https://img.shields.io/badge/Platform-Windows%2010%2F11%20(x64)-0078D6.svg)](https://github.com/Hakantes/videodw/releases)
[![Platform macOS](https://img.shields.io/badge/Platform-macOS%20Apple%20Silicon%20(ARM64)-000000.svg)](https://github.com/Hakantes/videodw/releases)
[![License](https://img.shields.io/badge/License-GPLv3%20%2F%20Open%20Source-green.svg)](LICENSE)

Windows (.NET Framework 4.8.1 WinForms) ve macOS Apple Silicon (.NET 8 Avalonia UI) için geliştirilmiş; **yt-dlp**, **FFmpeg** ve **ffprobe** destekli, canlı indirme ilerlemesi, anlık hız ve kalan süre (ETA) göstergeli, indirme öncesi dosya boyutu tahmini yapabilen, **H.264/AAC** uyumlu ve güvenli **RSA-2048** otomatik güncelleme mimarisine sahip masaüstü video ve ses indirici.

---

## 📥 İndirme Bağlantıları (Downloads)

Uygulamanın derlemelerini aşağıdaki resmi bağlantılardan indirebilirsiniz:

> **⚠️ Dağıtım ve Yayın Notu:** Aşağıdaki doğrudan indirme bağlantıları, GitHub Releases sayfasında resmi `v2.1.0` sürümü yayımlandığında (veya taslak release onaylandığında) aktifleşecektir. Şu anda yerel ortamda üretilmiş ve doğrulanmış dağıtım dosyaları projenin `artifacts/` klasöründe yer almaktadır. Tüm geçmiş ve güncel sürümler için her zaman [GitHub Releases](https://github.com/Hakantes/videodw/releases) sayfasını ziyaret edebilirsiniz.

| Platform | Dağıtım Türü | Dosya Adı | Açıklama | GitHub Release Bağlantısı |
| :--- | :--- | :--- | :--- | :---: |
| 🪟 **Windows** | **Kurulum Sihirbazı (Önerilen)** | `VideoDownloader-Setup.exe` | Tam kurulum; masaüstü kısayolu, yt-dlp & FFmpeg dahil | [İndir (Setup)](https://github.com/Hakantes/videodw/releases/download/v2.1.0/VideoDownloader-Setup.exe) |
| 🪟 **Windows** | **Taşınabilir (Portable ZIP)** | `YouTubeDownloader_v2.1.0_Windows.zip` | Kurulumsuz; USB bellekte veya klasörde doğrudan çalışır | [İndir (ZIP)](https://github.com/Hakantes/videodw/releases/download/v2.1.0/YouTubeDownloader_v2.1.0_Windows.zip) |
| 🍏 **macOS** | **Taşınabilir Arşiv (ZIP - Hazır)** | `YouTubeDownloader_v2.1.0_macOS_ARM64.zip` | Apple Silicon (M1/M2/M3/M4); yerel `.app` bundle | [İndir (ZIP)](https://github.com/Hakantes/videodw/releases/download/v2.1.0/YouTubeDownloader_v2.1.0_macOS_ARM64.zip) |
| 🍏 **macOS** | **Kolay Kurulum İmajı (DMG)** | `YouTubeDownloader_v2.1.0_macOS_ARM64.dmg` | Uygulamalar klasörüne sürükle-bırak *(Bu Release'e daha sonra eklenecektir)* | *(Yakında Eklenecek)* |

---

## 🔒 Dosya Bütünlüğü ve Güvenlik (SHA-256 Checksums)

Yerel derlemede üretilmiş ve doğrulanmış paketlerin SHA-256 sağlama özetleri aşağıdadır:

| Dosya Adı | Boyut (Bayt) | SHA-256 Sağlama Özeti | Durum |
| :--- | :--- | :--- | :---: |
| **`VideoDownloader-Setup.exe`** | 115.271.500 | `d14896f1a55f144be533fa9be790e27e7baaac8d5faa1a164664ad8f03d4df74` | Doğrulandı |
| **`YouTubeDownloader_v2.1.0_Windows.zip`** | 150.975.530 | `69c2859cf8c5d00afa76ef6570fe48ac892ed53278516a635658eaf5a8511438` | Doğrulandı |
| **`YouTubeDownloader_v2.1.0_macOS_ARM64.zip`** | 39.916.702 | `31fabf2a3552f4d8883ef3ba005deef9c81d53184d9affea34c799c5f8841bde` | Doğrulandı |

> **💡 macOS Paket Notu:** Apple'ın tescilli `hdiutil` aracı yalnızca macOS üzerinde çalıştığından, DMG disk imajı daha sonra oluşturulup aynı Release altına eklenecektir. Şu anda doğrudan kullanıma hazır ve doğrulanmış paket **`YouTubeDownloader_v2.1.0_macOS_ARM64.zip`** (38,07 MB) paketidir. *(Not: macOS ARM64 sürümü çapraz derleme ile paketlenmiştir; fiziksel Mac donanımı üzerinde kurulum ve test süreci devam etmektedir).*

---

## 🚀 Kurulum ve Başlangıç

### 🪟 Windows Kurulumu
1. **Kurulum Sihirbazı İle (Önerilen):**
   - `VideoDownloader-Setup.exe` dosyasını indirip çalıştırın.
   - Kurulum sihirbazındaki adımları izleyin (*İleri > Kur*).
   - Kurulum paketi `yt-dlp`, `FFmpeg` ve `ffprobe` araçlarını otomatik olarak içerir; harici hiçbir yazılım yüklemeniz gerekmez.
2. **Taşınabilir (Portable) Sürüm İle:**
   - `YouTubeDownloader_v2.1.0_Windows.zip` arşivini bir klasöre çıkartın.
   - Klasör içindeki `youtube dowload.exe` dosyasına çift tıklayarak doğrudan kullanın.

### 🍏 macOS Apple Silicon Kurulumu
1. **DMG veya ZIP Paketini İndirin:**
   - `YouTubeDownloader_v2.1.0_macOS_ARM64.dmg` dosyasını açıp uygulamayı **Uygulamalar (`Applications`)** klasörüne sürükleyin (veya ZIP arşivini açıp `.app` dosyasını taşıyın).
2. **İlk Açılışta Gatekeeper İzni:**
   - Bağımsız açık kaynaklı bir proje olduğundan (Apple'ın \$99/yıl ücretli Developer ID sertifikası yerine güvenli SHA-256 imzası kullanılmaktadır) macOS ilk açılışta *"Geliştirici doğrulanamadı"* uyarısı verir.
   - **Çözüm (Önerilen):** Finder'da uygulamaya **Control tuşuna basarak tıklayın (Sağ Tık)** ve **Aç (`Open`)** deyin. Çıkan pencerede **Aç** butonuna basın. macOS bu tercihi hatırlar.
   - *(Detaylı macOS güvenlik rehberi için [docs/MACOS_KURULUM.md](docs/MACOS_KURULUM.md) belgesini inceleyin).*
3. **Motorları Yükleyin (Homebrew):**
   - macOS Terminalini açıp şu komutu çalıştırın:
     ```bash
     brew install yt-dlp ffmpeg
     ```

---

## 🔄 Otomatik Güncelleme Mimarisi (v2.1.0)

Uygulama, hem Windows hem de macOS üzerinde modern ve güvenli bir güncelleme sistemi içerir:

- **Arka Plan Denetimi:** Uygulama açılışında günde en fazla 1 kez sessizce `update-manifest.json` dosyasını kontrol eder. Kullanıcıyı gereksiz bildirimlerle rahatsız etmez veya GitHub API sınırını tüketmez.
- **Manuel Kontrol:** İstenildiği an sağ üst köşedeki *"🔄 Güncellemeleri Denetle"* seçeneğiyle doğrudan sorgulama yapılabilir.
- **Kriptografik Güvenlik (RSA-2048 + SHA-256):** İndirilen manifest, geliştiricinin özel anahtarıyla imzalanmış ve uygulamaya gömülü RSA-2048 açık anahtarıyla doğrulanır. İmzası geçersiz, sürümü düşürülmüş (downgrade) veya hash'i eşleşmeyen hiçbir paket kuruluma alınmaz.
- **Platform Yalıtımı:** Windows yalnızca Windows kurulum paketlerini, macOS ise yalnızca Apple Silicon ARM64 paketlerini indirebilir; çapraz paket hataları engellenmiştir.
- **Ayarların Korunumu:** Windows ayarları `%APPDATA%\YouTubeVideoDownloader\settings.json`, macOS ayarları `~/Library/Application Support/YouTubeVideoDownloader/settings.json` altında saklandığından güncellemelerde hiçbir tercih sıfırlanmaz.

---

## ✨ Temel Özellikler

### 1. 📊 Canlı İlerleme, Anlık Hız ve ETA
- **Gerçek Zamanlı İlerleme:** Transfer edilen gerçek bayt verilerine göre anlık artan kademeli ilerleme çubuğu (%0 ➔ %12 ➔ %45 ➔ %88 ➔ %100).
- **Önceden Boyut Tahmini:** Video analiz edildiğinde seçilen çözünürlük ve profile göre tahmini dosya boyutu indirme başlamadan hesaplanır (`~145,6 MB`).
- **EMA Hız Filtresi:** Ağ dalgalanmalarını yumuşatan Üstel Hareketli Ortalama (EMA) destekli canlı transfer hızı (`8,45 MB/s`).
- **Tahmini Kalan Süre (ETA):** Sıfıra bölme korumalı anlık kalan süre göstergesi (`Kalan: 00:35`).

### 2. 🎯 Windows Media Player & QuickTime Uyumluluğu
- **Windows Uyumlu Profil (H.264 / AAC):** YouTube'un yeni nesil AV1 kodeki yerine donanımsal destekli H.264 (`avc1`) video ve AAC (`mp4a`) ses akışlarını birleştirir. Eski bilgisayarlarda, Windows Media Player'da, arabalarda ve televizyonlarda **siyah ekran olmadan** %100 sorunsuz oynatılır.
- **En İyi Kalite Profili (AV1 / VP9):** 4K/2K çözünürlüklerde kaynakta bulunan en yüksek bit hızına sahip akışları indirir.
- **Yalnızca Ses (MP3):** Seçilen videoyu doğrudan 320 kbps yüksek kaliteli MP3 dosyasına dönüştürür.

### 3. 🔍 Gerçek Medya Doğrulaması (`ffprobe`)
- İndirme tamamlandığında dosya sadece uzantısına bakılarak başarılı sayılmaz; entegre `ffprobe` aracıyla video ve ses akışları taranır. Bozuk veya eksik dosyalar anında tespit edilir.

---

## 🖥️ Desteklenen Platformlar

| İşletim Sistemi | Destek Durumu | Mimari | Gereksinimler |
| :--- | :---: | :---: | :--- |
| **Windows 11 / 10** | **Resmi Destek** | x64 / x86 | .NET Framework 4.8.1 (Windows ile hazır gelir) |
| **macOS Apple Silicon** | **Resmi Destek (MVP)** | ARM64 (M1/M2/M3/M4) | macOS 11.0+, Homebrew (`yt-dlp`, `ffmpeg`) |
| **macOS Intel (x86_64)** | *Desteklenmiyor* | Intel x64 | Bu sürümde Intel Mac derlemesi bulunmamaktadır |

> **⚠️ Donanım Testi Notu:** macOS ARM64 uygulaması .NET 8 Release ortamında temiz derlenmiş ve test edilmiştir. Arkadaşınızın veya son kullanıcının fiziksel Apple Silicon Mac donanımında ilk çalıştırma testi: **BEKLENİYOR / BLOCKED**.

---

## 🛠️ Kaynak Koddan Derleme (Geliştiriciler İçin)

### Windows Uygulamasını Derleme (.NET 4.8.1)
```powershell
# NuGet paketlerini geri yükle
nuget restore "src/youtube-downloader/packages.config" -SolutionDirectory .

# Release derlemesi yap
msbuild "src/youtube-downloader/youtube dowload.csproj" /p:Configuration=Release /t:Build

# 44 Testten oluşan süiti çalıştır
csc /target:exe /out:src/youtube-downloader/bin/Release/TestRunner.exe tests/TestRunner.cs /reference:"src/youtube-downloader/bin/Release/youtube dowload.exe","src/youtube-downloader/bin/Release/System.Text.Json.dll"
./src/youtube-downloader/bin/Release/TestRunner.exe
```

### macOS Uygulamasını Derleme ve Paketleme (.NET 8 Avalonia)
```bash
# macOS üzerinde veya Linux/CI ortamında:
chmod +x scripts/*.sh
./scripts/build-macos-app.sh "2.1.0" Release

# Mac üzerinde DMG oluşturmak için:
./scripts/create-dmg.sh "2.1.0"
```

---

## 📋 Sürüm Geçmişi

### v2.1.0
- 🍏 **macOS Apple Silicon (ARM64) Desteği:** Avalonia UI ve .NET 8 tabanlı yerel masaüstü uygulaması eklendi.
- 🔄 **Güvenli Otomatik Güncelleme:** RSA-2048 PKCS#1 SHA-256 dijital imzalı ve SHA-256 paket doğrulamalı güncelleme yöneticisi eklendi.
- 🛡️ **Inno Setup & Portable Desteği:** Kurulu sürümler için sessiz güncelleme (`/SP- /SILENT /NORESTART`), taşınabilir sürümler için bağımsız helper süreci geliştirildi.
- 🧪 **44/44 Kapsamlı Test Süiti:** Güncelleme manifesti, tahrifat engelleme, downgrade koruması ve hız sınırları için otomatik testler eklendi.

### v2.0.3
- ⚡ **Canlı İlerleme ve EMA Hız Göstergesi:** Gerçek zamanlı bayt transferine dayalı kademeli yüzde artışı eklendi.
- 📦 **İndirme Öncesi Boyut Tahmini:** Video çözünürlüğüne göre yaklaşık dosya boyutu gösterimi sağlandı.

### v2.0.2
- 🎬 **H.264/AAC Profili:** Windows Media Player siyah ekran hatası giderildi.
- 🔍 **ffprobe Akış Doğrulaması:** Dosya bütünlüğü teyit mekanizması eklendi.

---

## 📜 Lisans ve Üçüncü Taraf Bildirimleri

- **yt-dlp:** [The Unlicense](https://github.com/yt-dlp/yt-dlp/blob/master/LICENSE) (Kamu Malı / Public Domain)
- **FFmpeg / ffprobe:** [GPL v3.0](https://www.gnu.org/licenses/gpl-3.0.html)
- **Avalonia UI:** [MIT License](https://github.com/AvaloniaUI/Avalonia/blob/master/licence.md)
- **AngleSharp:** [MIT License](https://github.com/AngleSharp/AngleSharp/blob/devel/LICENSE)
- **YoutubeExplode:** [LGPL v3.0 License](https://github.com/Tyrrrz/YoutubeExplode/blob/master/License.txt)
- **System.Text.Json / CommunityToolkit:** [MIT License](https://github.com/dotnet/runtime/blob/main/LICENSE.TXT)
