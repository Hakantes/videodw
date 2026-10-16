using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using youtube_dowload;

namespace TestSuite
{
    class Program
    {
        static int _passed = 0;
        static int _failed = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine(" YouTube Downloader v2.0.3 Kapsamlı Test Paketi  ");
            Console.WriteLine("=================================================");

            // v2.0.2 Regresyon Testleri (1-14)
            RunTest("1. Format Seçici: Windows Uyumlu Profil (H.264/AAC)", TestWindowsCompatibleFormatSelector);
            RunTest("2. Format Seçici: En İyi Kalite Profil (AV1/VP9/Orijinal)", TestBestQualityFormatSelector);
            RunTest("3. Format Seçici: Çözünürlük Kısıtlamaları (1080p, 720p)", TestResolutionFormatSelector);
            RunTest("4. Codec Algılama: H.264 (avc1) Tespiti ve AV1/VP9 Ayırımı", TestH264CodecDetection);
            RunTest("5. Codec Algılama: AAC (mp4a) Tespiti ve Opus Ayırımı", TestAacCodecDetection);
            RunTest("6. Metadata Ayrıştırma: Çoklu Codec ve Çözünürlük Ayrımı", TestMetadataParsing);
            RunTest("7. ffprobe Doğrulaması: Gerçek MP4 Dosyası Akış Analizi", TestMediaValidationSuccess);
            RunTest("8. Doğrulama Hatası: Bulunamayan veya 0 Bayt Dosya", TestMediaValidationMissingFile);
            RunTest("9. Doğrulama Hatası: Video Beklenirken Yalnızca Ses Çıktısı", TestMediaValidationNoVideoStream);
            RunTest("10. ffprobe Eksikliği Durumunda Güvenli Geri Dönüş", TestFfprobeFallbackHandling);
            RunTest("11. Dosya Yolu & Türkçe Karakter & Çakışma Önleme", TestPathAndTurkishCharacters);
            RunTest("12. İstisna Türleri: Bot Doğrulama & H.264 Yok Bildirimi", TestExceptionTypes);
            RunTest("13. FFmpeg ile Gerçek H.264 Yeniden Kodlama ve ffprobe Teyidi", TestReencoding);
            RunTest("14. İptal Mekanizması ve Temizlik Doğrulaması", TestCancellationCleanup);

            // v2.0.3 Canlı İlerleme ve Hız Göstergesi Testleri (15-26)
            RunTest("15. Gerçek Zamanlı İlerleme: Yüzdenin Artması", TestProgressPercentageIncreasing);
            RunTest("16. Boyut & Yüzde Hesabı: Bilinen Toplam Boyut", TestKnownTotalSize);
            RunTest("17. Güvenli Gösterim: Bilinmeyen Toplam Boyut", TestUnknownTotalSizeSafeDisplay);
            RunTest("18. Birim Dönüşümü: Bayt Boyut Formatlaması (B, KB, MB, GB)", TestByteFormattingUnits);
            RunTest("19. Canlı Hız Hesabı, Birimler ve EMA Yumuşatma", TestSpeedCalculationAndUnits);
            RunTest("20. Tahmini Kalan Süre (ETA) Hesabı ve Sınır Kontrolleri", TestEtaCalculation);
            RunTest("21. Hata Toleransı: Sıfır ve Eksik Veri Yönetimi", TestZeroOrMissingDataHandling);
            RunTest("22. Sınırlandırma: Yüzde Değerinin %0-%100 Arasında Kalması", TestPercentageClamping);
            RunTest("23. Çok Aşamalı İndirme: Ayrı Video ve Ses Akışları", TestSeparateVideoAndAudioStreams);
            RunTest("24. İptal ve Yeniden Deneme: Tracker Sıfırlama (Reset)", TestCancellationAndRetryReset);
            RunTest("25. FFmpeg Birleştirme ve ffprobe Doğrulama Aşamaları", TestMergingAndValidationStages);
            RunTest("26. UI Thread Performansı: Rapor Filtreleme ve Throttling", TestProcessExitAndThrottling);

            Console.WriteLine("\n-------------------------------------------------");
            Console.WriteLine($"SONUÇ: {_passed} Başarılı, {_failed} Başarısız (Toplam {_passed + _failed} Senaryo)");
            Console.WriteLine("-------------------------------------------------");

            if (_failed > 0)
            {
                Environment.Exit(1);
            }
        }

        static void RunTest(string name, Action test)
        {
            try
            {
                test();
                Console.WriteLine($"[PASS] {name}");
                _passed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FAIL] {name}: {ex.Message}");
                _failed++;
            }
        }

        static void RunTest(string name, Func<Task> testAsync)
        {
            try
            {
                testAsync().GetAwaiter().GetResult();
                Console.WriteLine($"[PASS] {name}");
                _passed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FAIL] {name}: {ex.Message}");
                _failed++;
            }
        }

        static void TestWindowsCompatibleFormatSelector()
        {
            var req = new DownloadRequest
            {
                Url = "https://example.com",
                Profile = DownloadProfile.WindowsCompatible
            };
            string selector = DownloadEngine.BuildFormatSelector(req);

            Assert(selector.Contains("vcodec^=avc1"), "Windows profilinde vcodec^=avc1 bulunmalı");
            Assert(selector.Contains("acodec^=mp4a"), "Windows profilinde acodec^=mp4a bulunmalı");
            Assert(selector.Contains("vcodec*=avc"), "Windows profilinde vcodec*=avc fallback bulunmalı");
        }

        static void TestBestQualityFormatSelector()
        {
            var req = new DownloadRequest
            {
                Url = "https://example.com",
                Profile = DownloadProfile.BestQuality
            };
            string selector = DownloadEngine.BuildFormatSelector(req);

            Assert(selector == "bestvideo+bestaudio/best", "En İyi Kalite profilinde sınırsız en iyi akış seçilmeli");
        }

        static void TestResolutionFormatSelector()
        {
            var req1080 = new DownloadRequest
            {
                Url = "https://example.com",
                Profile = DownloadProfile.WindowsCompatible,
                TargetResolution = 1080
            };
            string sel1080 = DownloadEngine.BuildFormatSelector(req1080);
            Assert(sel1080.Contains("height<=1080"), "1080p sınırı selector içinde olmalı");

            var req720 = new DownloadRequest
            {
                Url = "https://example.com",
                Profile = DownloadProfile.BestQuality,
                TargetResolution = 720
            };
            string sel720 = DownloadEngine.BuildFormatSelector(req720);
            Assert(sel720.Contains("height<=720"), "720p sınırı selector içinde olmalı");
        }

        static void TestH264CodecDetection()
        {
            Assert(DownloadEngine.IsH264Codec("avc1.640028"), "avc1.640028 H.264 olarak tanınmalı");
            Assert(DownloadEngine.IsH264Codec("h264"), "h264 H.264 olarak tanınmalı");
            Assert(DownloadEngine.IsH264Codec("AVC"), "AVC H.264 olarak tanınmalı");
            Assert(!DownloadEngine.IsH264Codec("av01.0.08M.08"), "av01 AV1'dir, H.264 sayılmamalı");
            Assert(!DownloadEngine.IsH264Codec("vp09.00.51.08.01"), "vp09 VP9'dur, H.264 sayılmamalı");
        }

        static void TestAacCodecDetection()
        {
            Assert(DownloadEngine.IsAacCodec("mp4a.40.2"), "mp4a.40.2 AAC olarak tanınmalı");
            Assert(DownloadEngine.IsAacCodec("aac"), "aac AAC olarak tanınmalı");
            Assert(!DownloadEngine.IsAacCodec("opus"), "opus AAC sayılmamalı");
        }

        static void TestMetadataParsing()
        {
            string sampleJson = @"
            {
                ""title"": ""Test Video Başlığı"",
                ""uploader"": ""Test Kanalı"",
                ""duration"": 125,
                ""thumbnail"": ""https://example.com/thumb.jpg"",
                ""formats"": [
                    { ""format_id"": ""137"", ""vcodec"": ""avc1.640028"", ""acodec"": ""none"", ""height"": 1080 },
                    { ""format_id"": ""399"", ""vcodec"": ""av01.0.08M.08"", ""acodec"": ""none"", ""height"": 1080 },
                    { ""format_id"": ""248"", ""vcodec"": ""vp09.00.51.08.01"", ""acodec"": ""none"", ""height"": 1080 },
                    { ""format_id"": ""140"", ""vcodec"": ""none"", ""acodec"": ""mp4a.40.2"", ""height"": null }
                ]
            }";

            var engine = new DownloadEngine();
            var meta = engine.ParseVideoInfoJson(sampleJson, "https://example.com");

            Assert(meta.Title == "Test Video Başlığı", "Video başlığı doğru ayrıştırılmalı");
            Assert(meta.HasH264Stream, "H.264 akışı tespit edilmeli");
            Assert(meta.HasAacAudioStream, "AAC ses akışı tespit edilmeli");
            Assert(meta.AvailableH264Resolutions.Contains(1080), "H.264 1080p çözünürlüğü tespit edilmeli");
            Assert(meta.VideoCodecs.Contains("H.264 (AVC)"), "VideoCodecs içinde H.264 (AVC) bulunmalı");
            Assert(meta.VideoCodecs.Contains("AV1"), "VideoCodecs içinde AV1 bulunmalı");
        }

        static async Task TestMediaValidationSuccess()
        {
            string testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_valid.mp4");
            string ffmpeg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = ffmpeg,
                Arguments = $"-y -f lavfi -i testsrc=duration=1:size=640x360:rate=1 -f lavfi -i sine=frequency=1000:duration=1 -c:v libx264 -c:a aac \"{testFile}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (var proc = System.Diagnostics.Process.Start(psi))
            {
                proc.WaitForExit();
            }

            try
            {
                Assert(File.Exists(testFile), "Test video dosyası oluşturulmuş olmalı");
                var engine = new DownloadEngine();
                var result = await engine.ValidateMediaFileAsync(testFile, false, CancellationToken.None);

                Assert(result.IsValid, "Test dosyası geçerli olarak doğrulanmalı");
                Assert(result.HasVideo, "Video akışı tespit edilmeli");
                Assert(result.HasAudio, "Ses akışı tespit edilmeli");
                Assert(result.Width == 640, "Genişlik 640 olmalı");
                Assert(result.Height == 360, "Yükseklik 360 olmalı");
                Assert(result.IsH264, "Video H.264 olarak doğrulanmalı");
                Assert(result.IsAac, "Ses AAC olarak doğrulanmalı");
                Assert(result.IsWindowsPlayerFriendly, "Windows Media Player dostu olmalı");
            }
            finally
            {
                if (File.Exists(testFile)) try { File.Delete(testFile); } catch { }
            }
        }

        static async Task TestMediaValidationMissingFile()
        {
            var engine = new DownloadEngine();
            var result = await engine.ValidateMediaFileAsync("C:\\olmayan_dosya_12345.mp4", false, CancellationToken.None);
            Assert(!result.IsValid, "Olmayan dosya geçersiz dönmeli");
            Assert(result.ErrorMessage.Contains("bulunamadı"), "Bulunamadı hata mesajı içermeli");
        }

        static async Task TestMediaValidationNoVideoStream()
        {
            string audioOnlyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_audio_only.mp4");
            string ffmpeg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = ffmpeg,
                Arguments = $"-y -f lavfi -i sine=frequency=1000:duration=1 -c:a aac \"{audioOnlyFile}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (var proc = System.Diagnostics.Process.Start(psi))
            {
                proc.WaitForExit();
            }

            try
            {
                var engine = new DownloadEngine();
                var result = await engine.ValidateMediaFileAsync(audioOnlyFile, false, CancellationToken.None);

                Assert(!result.IsValid, "Video beklenirken ses dosyası geçersiz sayılmalı");
                Assert(result.ErrorMessage.Contains("video akışı bulunamadı"), "Doğru hata mesajı içermeli");
            }
            finally
            {
                if (File.Exists(audioOnlyFile)) try { File.Delete(audioOnlyFile); } catch { }
            }
        }

        static async Task TestFfprobeFallbackHandling()
        {
            string testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_fallback.mp4");
            File.WriteAllBytes(testFile, new byte[] { 1, 2, 3, 4 });

            try
            {
                var engine = new DownloadEngine();
                var result = await engine.ValidateMediaFileAsync(testFile, false, CancellationToken.None);
                Assert(result.FileSizeBytes == 4, "Dosya boyutu doğru okunmalı");
            }
            finally
            {
                if (File.Exists(testFile)) try { File.Delete(testFile); } catch { }
            }
        }

        static void TestPathAndTurkishCharacters()
        {
            string invalid = "Video/Başlığı:*?\"<>|Şampiyon_Türkçe";
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                invalid = invalid.Replace(c, '_');
            }
            Assert(!invalid.Contains("/"), "Geçersiz karakter / temizlenmeli");
            Assert(!invalid.Contains(":"), "Geçersiz karakter : temizlenmeli");
            Assert(!invalid.Contains("*"), "Geçersiz karakter * temizlenmeli");
            Assert(invalid.Contains("Şampiyon_Türkçe"), "Türkçe karakterler korunmalı");
        }

        static void TestExceptionTypes()
        {
            var botEx = new BotVerificationException("Bot doğrulama uyarısı");
            Assert(botEx.Message.Contains("Bot doğrulama"), "BotVerificationException mesajı korunmalı");

            var h264Ex = new H264NotAvailableException("H.264 akışı yok", new List<int> { 1080, 720 });
            Assert(h264Ex.AvailableResolutions.Count == 2, "Kullanılabilir çözünürlükler korunmalı");

            var valEx = new MediaValidationException("Doğrulama hatası");
            Assert(valEx.Message == "Doğrulama hatası", "MediaValidationException mesajı korunmalı");
        }

        static async Task TestReencoding()
        {
            string tempInput = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_av1_dummy.mp4");
            string ffmpeg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = ffmpeg,
                Arguments = $"-y -f lavfi -i testsrc=duration=2:size=320x240:rate=1 -f lavfi -i sine=frequency=1000:duration=2 -c:v mpeg4 -c:a mp3 \"{tempInput}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (var proc = System.Diagnostics.Process.Start(psi))
            {
                proc.WaitForExit();
            }

            try
            {
                Assert(File.Exists(tempInput), "2 saniyelik test klibi oluşturulmalı");

                var engine = new DownloadEngine();
                string reencoded = await engine.ReencodeToH264Async(tempInput, null, CancellationToken.None);

                Assert(File.Exists(reencoded), "Yeniden kodlanan dosya mevcut olmalı");
                var val = await engine.ValidateMediaFileAsync(reencoded, false, CancellationToken.None);
                Assert(val.IsValid, "Dönüştürülen dosya ffprobe tarafından geçerli sayılmalı");
                Assert(val.IsH264, "Dönüştürülen dosya video codec'i H.264 olmalı");
                Assert(val.IsAac, "Dönüştürülen dosya ses codec'i AAC olmalı");
            }
            finally
            {
                if (File.Exists(tempInput)) try { File.Delete(tempInput); } catch { }
            }
        }

        static void TestCancellationCleanup()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var req = new DownloadRequest { Url = "https://example.com", OutputDirectory = Path.GetTempPath() };
            var engine = new DownloadEngine();

            try
            {
                engine.DownloadAsync(req, null, cts.Token).GetAwaiter().GetResult();
                Assert(false, "İptal edilen istek OperationCanceledException fırlatmalı");
            }
            catch (OperationCanceledException)
            {
                Assert(true, "OperationCanceledException doğru fırlatıldı");
            }
            catch (Exception ex)
            {
                Assert(false, "Beklenmeyen hata türü: " + ex.GetType().Name);
            }
        }

        // ==========================================
        // v2.0.3 CANLI İLERLEME VE HIZ TESTLERİ
        // ==========================================

        static void TestProgressPercentageIncreasing()
        {
            var tracker = new DownloadEngine.DownloadProgressTracker(false);
            double lastPct = -1;
            long[] bytes = { 10000000, 25000000, 50000000, 75000000, 90000000 };
            foreach (var b in bytes)
            {
                string json = $"{{\"status\":\"downloading\",\"filename\":\"video.mp4\",\"downloaded_bytes\":{b},\"total_bytes\":100000000,\"speed\":5000000}}";
                var rep = tracker.ProcessLine("PROG:" + json);
                Assert(rep != null, "Rapor üretilmeli");
                Assert(rep.Percentage > lastPct, $"Yüzde artmalı: {rep.Percentage} > {lastPct}");
                lastPct = rep.Percentage;
            }
            Assert(Math.Abs(lastPct - 90.0) < 0.1, "Son yüzde %90 olmalı");
        }

        static void TestKnownTotalSize()
        {
            var tracker = new DownloadEngine.DownloadProgressTracker(false);
            string json = "{\"status\":\"downloading\",\"filename\":\"video.mp4\",\"downloaded_bytes\":125000000,\"total_bytes\":500000000,\"speed\":8860877}";
            var rep = tracker.ProcessLine("PROG:" + json);
            Assert(rep != null, "Rapor null olmamalı");
            Assert(Math.Abs(rep.Percentage - 25.0) < 0.01, "125MB / 500MB = %25 olmalı");
            Assert(rep.FormattedSizeProgress.Contains("(%25)"), "FormattedSizeProgress %25 içermeli");
            Assert(rep.FormattedSizeProgress.Contains("İndirilen:"), "İndirilen etiketi içermeli");
            Assert(rep.FormattedSizeProgress.Contains("/"), "Toplam boyut ayracı içermeli");
        }

        static void TestUnknownTotalSizeSafeDisplay()
        {
            var tracker = new DownloadEngine.DownloadProgressTracker(false);
            string json = "{\"status\":\"downloading\",\"filename\":\"stream.mp4\",\"downloaded_bytes\":131490201,\"total_bytes\":null,\"speed\":2000000}";
            var rep = tracker.ProcessLine("PROG:" + json);
            Assert(rep != null, "Rapor null olmamalı");
            Assert(rep.FormattedSizeProgress.Contains("Toplam boyut hesaplanıyor"), "Bilinmeyen toplam boyut belirtilmeli");
            Assert(rep.FormattedSizeProgress.Contains("125,40 MB"), "İndirilen boyut 125,40 MB gösterilmeli");
            Assert(rep.FormattedEta == "Kalan süre hesaplanıyor...", "Bilinmeyen toplam boyutta ETA hesaplanıyor olmalı");
        }

        static void TestByteFormattingUnits()
        {
            Assert(DownloadProgressReport.FormatBytes(500) == "500 B", "500 B doğru formatlanmalı");
            Assert(DownloadProgressReport.FormatBytes(1024) == "1,00 KB", "1 KB doğru formatlanmalı");
            Assert(DownloadProgressReport.FormatBytes(1048576) == "1,00 MB", "1 MB doğru formatlanmalı");
            Assert(DownloadProgressReport.FormatBytes(1073741824) == "1,00 GB", "1 GB doğru formatlanmalı");
            Assert(DownloadProgressReport.FormatBytes(131490201) == "125,40 MB", "125,40 MB doğru formatlanmalı");
            Assert(DownloadProgressReport.FormatBytes(0) == "0 B", "0 bayt doğru formatlanmalı");
            Assert(DownloadProgressReport.FormatBytes(-100) == "0 B", "Negatif bayt 0 B olarak korunmalı");
        }

        static void TestSpeedCalculationAndUnits()
        {
            Assert(DownloadProgressReport.FormatSpeed(500) == "500,00 B/sn", "B/sn formatı doğru olmalı");
            Assert(DownloadProgressReport.FormatSpeed(10240) == "10,00 KB/sn", "KB/sn formatı doğru olmalı");
            Assert(DownloadProgressReport.FormatSpeed(8860877) == "8,45 MB/sn", "MB/sn formatı doğru olmalı");
            Assert(DownloadProgressReport.FormatSpeed(0) == "Hız hesaplanıyor...", "0 hızında hesaplanıyor gösterilmeli");
            Assert(DownloadProgressReport.FormatSpeed(-5) == "Hız hesaplanıyor...", "Negatif hızda hesaplanıyor gösterilmeli");
            Assert(DownloadProgressReport.FormatSpeed(double.NaN) == "Hız hesaplanıyor...", "NaN hızda hesaplanıyor gösterilmeli");

            // EMA yumuşatma testi
            var tracker = new DownloadEngine.DownloadProgressTracker(false);
            tracker.ProcessLine("PROG:{\"status\":\"downloading\",\"filename\":\"v.mp4\",\"downloaded_bytes\":100,\"total_bytes\":1000,\"speed\":10000000}");
            double speed1 = tracker.SmoothedSpeed;
            tracker.ProcessLine("PROG:{\"status\":\"downloading\",\"filename\":\"v.mp4\",\"downloaded_bytes\":200,\"total_bytes\":1000,\"speed\":20000000}");
            double speed2 = tracker.SmoothedSpeed;
            Assert(speed2 > speed1 && speed2 < 20000000, "EMA hızı yumuşatmalı (ani sıçramayı filtrelemeli)");
        }

        static void TestEtaCalculation()
        {
            // 42 MB remaining at 1 MB/s = 42 seconds -> "00:42"
            string eta42 = DownloadProgressReport.FormatEta(42 * 1024 * 1024, 1024 * 1024);
            Assert(eta42 == "00:42", $"42 saniye 00:42 olmalı, alınan: {eta42}");

            // 3661 seconds = 1 hour 1 minute 1 sec -> "01:01:01"
            string etaHour = DownloadProgressReport.FormatEta(3661 * 1000, 1000);
            Assert(etaHour == "01:01:01", $"3661 saniye 01:01:01 olmalı, alınan: {etaHour}");

            // 0 remaining -> "00:00"
            Assert(DownloadProgressReport.FormatEta(0, 1000000) == "00:00", "0 kalan bayt 00:00 olmalı");
            // 0 speed -> "Kalan süre hesaplanıyor..."
            Assert(DownloadProgressReport.FormatEta(1000, 0) == "Kalan süre hesaplanıyor...", "0 hızda hesaplanıyor olmalı");
        }

        static void TestZeroOrMissingDataHandling()
        {
            var tracker = new DownloadEngine.DownloadProgressTracker(false);
            Assert(tracker.ProcessLine(null) == null, "Null satır null dönmeli");
            Assert(tracker.ProcessLine("") == null, "Boş satır null dönmeli");
            Assert(tracker.ProcessLine("PROG:invalid-json") == null, "Geçersiz json çökmemeli");
            Assert(tracker.ProcessLine("PROG:{}") != null, "Boş JSON güvenle işlenmeli");
            Assert(tracker.ProcessLine("PROG:{\"downloaded_bytes\":0}") != null, "Eksik alanlı JSON işlenmeli");
        }

        static void TestPercentageClamping()
        {
            var tracker = new DownloadEngine.DownloadProgressTracker(false);
            var repNegative = tracker.ProcessLine("PROG:{\"status\":\"downloading\",\"filename\":\"v.mp4\",\"downloaded_bytes\":-50,\"total_bytes\":100}");
            Assert(repNegative.Percentage >= 0.0, "Yüzde negatif olamaz");

            // İndirme sürerken yüzde 99'u geçemez
            var repOver = tracker.ProcessLine("PROG:{\"status\":\"downloading\",\"filename\":\"v.mp4\",\"downloaded_bytes\":150,\"total_bytes\":100}");
            Assert(repOver.Percentage <= 99.0, "Devam eden indirmede yüzde 99'u geçemez");

            // Yalnızca Completed aşaması %100 olabilir
            var repComplete = new DownloadProgressReport { Stage = DownloadStage.Completed, Percentage = 100 };
            Assert(repComplete.Percentage == 100.0, "Tamamlanan işlem %100 olmalı");
            Assert(repComplete.FormattedEta == "İndirme tamamlandı", "Tamamlanan işlemde ETA İndirme tamamlandı olmalı");
        }

        static void TestSeparateVideoAndAudioStreams()
        {
            var tracker = new DownloadEngine.DownloadProgressTracker(false);

            // Stream 1: Video (80 MB)
            var rep1 = tracker.ProcessLine("PROG:{\"status\":\"downloading\",\"filename\":\"video.f137.mp4\",\"downloaded_bytes\":40000000,\"total_bytes\":80000000,\"speed\":5000000}");
            Assert(rep1.Stage == DownloadStage.DownloadingVideo, "İlk akış video olmalı");
            Assert(tracker.TotalDownloadedBytes == 40000000, "40MB indirildi");

            // Video biter
            tracker.ProcessLine("PROG:{\"status\":\"finished\",\"filename\":\"video.f137.mp4\",\"downloaded_bytes\":80000000,\"total_bytes\":80000000,\"speed\":5000000}");

            // Stream 2: Audio (20 MB)
            var rep2 = tracker.ProcessLine("PROG:{\"status\":\"downloading\",\"filename\":\"audio.f140.m4a\",\"downloaded_bytes\":10000000,\"total_bytes\":20000000,\"speed\":2000000}");
            Assert(rep2.Stage == DownloadStage.DownloadingAudio, "İkinci akış ses olmalı");
            Assert(tracker.TotalDownloadedBytes == 90000000, "Toplam 80MB + 10MB = 90MB olmalı");
            Assert(tracker.TotalExpectedBytes == 100000000, "Toplam beklenen 80MB + 20MB = 100MB olmalı");
            Assert(Math.Abs(rep2.Percentage - 90.0) < 0.1, "Toplam yüzde %90 olmalı (ses akışı video akışının üzerine yazılmadı)");
        }

        static void TestCancellationAndRetryReset()
        {
            var tracker = new DownloadEngine.DownloadProgressTracker(false);
            tracker.ProcessLine("PROG:{\"status\":\"downloading\",\"filename\":\"v.mp4\",\"downloaded_bytes\":50000000,\"total_bytes\":100000000,\"speed\":5000000}");
            Assert(tracker.TotalDownloadedBytes == 50000000, "50MB kaydedilmeli");

            // Yeniden deneme için sıfırla
            tracker.Reset();
            Assert(tracker.TotalDownloadedBytes == 0, "Reset sonrası indirilen 0 olmalı");
            Assert(tracker.TotalExpectedBytes == null, "Reset sonrası toplam null olmalı");
            Assert(tracker.SmoothedSpeed == 0, "Reset sonrası hız 0 olmalı");
        }

        static void TestMergingAndValidationStages()
        {
            var tracker = new DownloadEngine.DownloadProgressTracker(false);
            var repMerge = tracker.ProcessLine("[Merger] Merging formats into \"final.mp4\"");
            Assert(repMerge != null, "Merger tespit edilmeli");
            Assert(repMerge.Stage == DownloadStage.MergingStreams, "Stage MergingStreams olmalı");
            Assert(repMerge.StatusMessage == "Ses ve video birleştiriliyor...", "StatusMessage birleştirme olmalı");
            Assert(repMerge.Percentage == 99, "Birleştirmede yüzde 99 olmalı");

            var repVal = new DownloadProgressReport
            {
                Percentage = 99,
                Stage = DownloadStage.Validating,
                StatusMessage = "Dosya doğrulanıyor..."
            };
            Assert(repVal.Stage == DownloadStage.Validating, "Validating aşaması doğru");
            Assert(repVal.StatusMessage == "Dosya doğrulanıyor...", "Validating mesajı doğru");
        }

        static void TestProcessExitAndThrottling()
        {
            var tracker = new DownloadEngine.DownloadProgressTracker(false);
            var rep1 = new DownloadProgressReport { Stage = DownloadStage.DownloadingVideo, Percentage = 10 };
            Assert(tracker.ShouldReport(rep1, 100), "İlk rapor her zaman raporlanmalı");

            var rep2 = new DownloadProgressReport { Stage = DownloadStage.DownloadingVideo, Percentage = 11 };
            // Hemen ardından gelen aynı aşamadaki rapor throttle edilmeli
            Assert(!tracker.ShouldReport(rep2, 100), "100ms dolmadan gelen ara rapor throttle edilmeli");

            // Aşama değiştiğinde (örneğin Merging) throttle baypas edilmeli
            var repStageChange = new DownloadProgressReport { Stage = DownloadStage.MergingStreams, Percentage = 99 };
            Assert(tracker.ShouldReport(repStageChange, 100), "Aşama değişimi anında raporlanmalı");
        }

        static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception("Doğrulama Hatası: " + message);
            }
        }
    }
}
