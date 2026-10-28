# YouTube Downloader

[![Release](https://img.shields.io/badge/Release-v2.1.0-blue.svg)](https://github.com/Hakantes/videodw/releases)
[![Platform Windows](https://img.shields.io/badge/Platform-Windows%2010%2F11%20(x64)-0078D6.svg)](https://github.com/Hakantes/videodw/releases)
[![Platform macOS](https://img.shields.io/badge/Platform-macOS%20Apple%20Silicon%20(ARM64)-000000.svg)](https://github.com/Hakantes/videodw/releases)
[![License](https://img.shields.io/badge/License-GPLv3-green.svg)](LICENSE)

YouTube Downloader, Windows ve macOS için geliştirilmiş açık kaynaklı bir video ve ses indirme uygulamasıdır. Arka planda `yt-dlp` ve `FFmpeg` motorlarını kullanarak yüksek çözünürlüklü video indirme, ses dönüştürme ve format uyumluluğu sağlar.

---

## Özellikler

- **Çoklu Çözünürlük ve Format:** 4K, 2K, 1080p, 720p ve 480p video indirme; 320 kbps yüksek kaliteli MP3 ses ayıklama.
- **Evrensel Oynatma Uyumluluğu:** Windows Media Player ve QuickTime ile sorunsuz çalışan H.264 video ve AAC ses birleştirme profili.
- **Canlı İndirme Göstergeleri:** Gerçek zamanlı ilerleme yüzdesi, anlık transfer hızı ve tahmini kalan süre (ETA).
- **İndirme Öncesi Boyut Tahmini:** Seçilen kaliteye göre tahmini dosya boyutunu indirme başlamadan görüntüleme.
- **Medya Doğrulaması:** İndirilen dosyaların bütünlüğünü `ffprobe` ile doğrulama.
- **Otomatik Güncelleme:** RSA-2048 imzalı manifest ve SHA-256 özet denetimi ile güvenli sürüm kontrolü.

---

## İndirme Bağlantıları

> **Not:** v2.1.0 sürümü GitHub üzerinde taslak olarak hazır durumdadır. İndirme bağlantıları sürüm yayımlandığında etkinleşir. Tüm sürümler için [GitHub Releases](https://github.com/Hakantes/videodw/releases) sayfasını inceleyebilirsiniz.

| Platform | Format | Dosya | Açıklama | İndir |
| :--- | :--- | :--- | :--- | :---: |
| **Windows** | Setup (Önerilen) | `VideoDownloader-Setup.exe` | Kurulum sihirbazı; masaüstü kısayolu, yt-dlp ve FFmpeg dahildir | [İndir](https://github.com/Hakantes/videodw/releases/download/v2.1.0/VideoDownloader-Setup.exe) |
| **Windows** | Portable ZIP | `YouTubeDownloader_v2.1.0_Windows.zip` | Kurulum gerektirmez, doğrudan çalıştırılabilir | [İndir](https://github.com/Hakantes/videodw/releases/download/v2.1.0/YouTubeDownloader_v2.1.0_Windows.zip) |
| **macOS** | DMG (Önerilen) | `YouTubeDownloader_v2.1.0_macOS_ARM64.dmg` | Apple Silicon (M1/M2/M3/M4); sürükle-bırak kurulum | [İndir](https://github.com/Hakantes/videodw/releases/download/v2.1.0/YouTubeDownloader_v2.1.0_macOS_ARM64.dmg) |
| **macOS** | Portable ZIP | `YouTubeDownloader_v2.1.0_macOS_ARM64.zip` | Apple Silicon (M1/M2/M3/M4); `.app` paketi | [İndir](https://github.com/Hakantes/videodw/releases/download/v2.1.0/YouTubeDownloader_v2.1.0_macOS_ARM64.zip) |

### Dosya Bütünlüğü (SHA-256)

| Dosya Adı | Boyut (Bayt) | SHA-256 Özeti |
| :--- | :--- | :--- |
| `VideoDownloader-Setup.exe` | 115.271.500 | `d14896f1a55f144be533fa9be790e27e7baaac8d5faa1a164664ad8f03d4df74` |
| `YouTubeDownloader_v2.1.0_Windows.zip` | 150.975.530 | `69c2859cf8c5d00afa76ef6570fe48ac892ed53278516a635658eaf5a8511438` |
| `YouTubeDownloader_v2.1.0_macOS_ARM64.dmg` | 45.741.596 | `6ac5abddbc1f07ae3924573d3fc403f9aaf90001d1ed3109f61df54a5d06bc82` |
| `YouTubeDownloader_v2.1.0_macOS_ARM64.zip` | 39.916.702 | `31fabf2a3552f4d8883ef3ba005deef9c81d53184d9affea34c799c5f8841bde` |

---

## Kurulum

### Windows
- **Kurulum Sihirbazı:** `VideoDownloader-Setup.exe` dosyasını çalıştırıp adımları tamamlayın. `yt-dlp` ve `FFmpeg` pakete dahil olduğundan ek bir yazılım kurmanız gerekmez.
- **Taşınabilir Sürüm:** `YouTubeDownloader_v2.1.0_Windows.zip` dosyasını çıkartın ve `youtube dowload.exe` uygulamasını çalıştırın.

### macOS (Apple Silicon M1/M2/M3/M4)
1. **Kurulum:** `YouTubeDownloader_v2.1.0_macOS_ARM64.dmg` dosyasını açın ve `YouTube Downloader.app` uygulamasını **Applications** klasörüne sürükleyin.
2. **İlk Açılış İzni:** Açık kaynaklı ve bağımsız bir yazılım olduğundan ilk açılışta macOS güvenlik uyarısı verebilir. Finder'da uygulamaya **Sağ tıklayın (veya Control + Tık)** ve **Aç** seçeneğini seçin; gelen iletişim kutusunda **Aç** butonuna tıklayın.
3. **Motor Kurulumu:** Terminal uygulamasını açarak indirme araçlarını yükleyin:
   ```bash
   brew install yt-dlp ffmpeg
   ```
   Ayrıntılı adımlar ve karantina yönetimi için [docs/MACOS_KURULUM.md](docs/MACOS_KURULUM.md) belgesini inceleyebilirsiniz.

> **Önemli Not:** macOS ARM64 sürümü çapraz derleme ile oluşturulmuştur ve CI ortamında paketlenmiştir. Fiziksel Apple Silicon cihazda son kullanıcı testi henüz tamamlanmamıştır.

---

## Kullanım

1. Video veya oynatma listesi bağlantısını URL alanına yapıştırın.
2. Çözünürlük ve format seçeneğini belirleyin (geniş uyumluluk için H.264 / AAC önerilir).
3. **İndir** butonuna tıklayın. İndirme ilerlemesi, hızı ve kalan süresi ekranda gösterilir.

---

## Sistem Gereksinimleri

| Platform | Mimari | Gereksinimler | Durum |
| :--- | :--- | :--- | :--- |
| **Windows 10 / 11** | x64 | .NET Framework 4.8.1 (Windows ile gelir) | Destekleniyor |
| **macOS 11.0+** | Apple Silicon (ARM64) | Homebrew (`yt-dlp`, `ffmpeg`) | Destekleniyor (Donanım testi beklemede) |
| **macOS (Intel)** | x86_64 | - | Desteklenmiyor |

---

## Sorun Giderme

| Sorun | Olası Neden | Çözüm |
| :--- | :--- | :--- |
| macOS: *"Geliştirici doğrulanamadı"* | Apple Developer imzası bulunmaması | Finder'da uygulamaya sağ tıklayıp **Aç** deyin veya Terminal'de `xattr -cr "/Applications/YouTube Downloader.app"` komutunu çalıştırın. |
| macOS: *"yt-dlp / FFmpeg bulunamadı"* | Gerekli komut satırı araçlarının kurulu olmaması | Terminal'de `brew install yt-dlp ffmpeg` komutunu çalıştırın. |
| Windows: Medya oynatıcıda siyah ekran | Videonun AV1 kodekiyle indirilmesi | İndirme menüsünden `H.264 / AAC` uyumlu profili seçin. |
| Windows SmartScreen uyarısı | Yeni derlenmiş açık kaynak yazılım | "Ek bilgi" bağlantısına ve ardından "Yine de çalıştır" butonuna tıklayın. |

---

## Derleme (Geliştiriciler İçin)

### Windows (.NET Framework 4.8.1)
```powershell
nuget restore "src/youtube-downloader/packages.config" -SolutionDirectory .
msbuild "src/youtube-downloader/youtube dowload.csproj" /p:Configuration=Release /t:Build
```

### macOS (.NET 8 / Avalonia UI)
```bash
./scripts/build-macos-app.sh "2.1.0" Release
./scripts/create-dmg.sh "2.1.0"
```

---

## Lisans ve Bağımlılıklar

Bu proje [GPL-3.0](LICENSE) lisansı ile dağıtılmaktadır. Projede kullanılan üçüncü taraf bileşenler:

- [yt-dlp](https://github.com/yt-dlp/yt-dlp) — The Unlicense
- [FFmpeg](https://ffmpeg.org/) — GPL v3.0 / LGPL v2.1
- [Avalonia UI](https://github.com/AvaloniaUI/Avalonia) — MIT License
- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) — LGPL v3.0
- [AngleSharp](https://github.com/AngleSharp/AngleSharp) — MIT License
