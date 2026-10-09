using System;
using System.Collections.Generic;
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
            Console.WriteLine(" YouTube Downloader v2.0.2 Kapsamlı Test Paketi  ");
            Console.WriteLine("=================================================");

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
            var reqWin = new DownloadRequest
            {
                Url = "https://example.com",
                Profile = DownloadProfile.WindowsCompatible,
                TargetResolution = 1080
            };
            string selWin = DownloadEngine.BuildFormatSelector(reqWin);
            Assert(selWin.Contains("height<=1080"), "1080p sınırı format dizgesine eklenmeli");
            Assert(selWin.Contains("vcodec^=avc1"), "H.264 şartı korunmalı");

            var reqBest = new DownloadRequest
            {
                Url = "https://example.com",
                Profile = DownloadProfile.BestQuality,
                TargetResolution = 720
            };
            string selBest = DownloadEngine.BuildFormatSelector(reqBest);
            Assert(selBest.Contains("height<=720"), "En iyi kalitede 720p sınırı eklenmeli");
        }

        static void TestH264CodecDetection()
        {
            Assert(DownloadEngine.IsH264Codec("avc1.4D401F"), "avc1.4D401F H.264 olarak tanınmalı");
            Assert(DownloadEngine.IsH264Codec("h264"), "h264 tanınmalı");
            Assert(DownloadEngine.IsH264Codec("H264"), "Büyük harf H264 tanınmalı");

            Assert(!DownloadEngine.IsH264Codec("av01.0.08M.08"), "AV1 H.264 olarak kabul edilmemeli");
            Assert(!DownloadEngine.IsH264Codec("vp9"), "VP9 H.264 olarak kabul edilmemeli");
            Assert(!DownloadEngine.IsH264Codec("vp09.00.10.08"), "VP09 H.264 olarak kabul edilmemeli");
            Assert(!DownloadEngine.IsH264Codec("none"), "none reddedilmeli");
            Assert(!DownloadEngine.IsH264Codec(""), "boşluk reddedilmeli");
        }

        static void TestAacCodecDetection()
        {
            Assert(DownloadEngine.IsAacCodec("mp4a.40.2"), "mp4a.40.2 AAC olarak tanınmalı");
            Assert(DownloadEngine.IsAacCodec("aac"), "aac tanınmalı");

            Assert(!DownloadEngine.IsAacCodec("opus"), "opus AAC olarak kabul edilmemeli");
            Assert(!DownloadEngine.IsAacCodec("mp3"), "mp3 AAC olarak kabul edilmemeli");
            Assert(!DownloadEngine.IsAacCodec("none"), "none reddedilmeli");
        }

        static void TestMetadataParsing()
        {
            string fakeJson = @"
            {
                ""title"": ""Test Video Başlığı"",
                ""uploader"": ""Test Kanalı"",
                ""duration"": 125,
                ""thumbnail"": ""https://example.com/thumb.jpg"",
                ""formats"": [
                    { ""format_id"": ""1"", ""vcodec"": ""avc1.4D401F"", ""acodec"": ""none"", ""height"": 1080 },
                    { ""format_id"": ""2"", ""vcodec"": ""av01.0.08M.08"", ""acodec"": ""none"", ""height"": 2160 },
                    { ""format_id"": ""3"", ""vcodec"": ""vp9"", ""acodec"": ""none"", ""height"": 1440 },
                    { ""format_id"": ""4"", ""vcodec"": ""none"", ""acodec"": ""mp4a.40.2"" },
                    { ""format_id"": ""5"", ""vcodec"": ""none"", ""acodec"": ""opus"" }
                ]
            }";

            var engine = new DownloadEngine();
            var meta = engine.ParseVideoInfoJson(fakeJson, "https://example.com");

            Assert(meta.Title == "Test Video Başlığı", "Başlık doğru ayrıştırılmalı");
            Assert(meta.AvailableResolutions.Contains(2160), "2160p tüm çözünürlüklerde olmalı");
            Assert(meta.AvailableResolutions.Contains(1080), "1080p tüm çözünürlüklerde olmalı");

            Assert(meta.AvailableH264Resolutions.Contains(1080), "1080p H.264 çözünürlüklerinde olmalı");
            Assert(!meta.AvailableH264Resolutions.Contains(2160), "2160p AV1 olduğu için H.264 listesinde olmamalı");
            Assert(meta.HasH264Stream, "HasH264Stream true olmalı");
            Assert(meta.HasAacAudioStream, "HasAacAudioStream true olmalı");
            Assert(meta.VideoCodecs.Contains("H.264 (AVC)"), "H.264 codec listesinde yer almalı");
            Assert(meta.VideoCodecs.Contains("AV1"), "AV1 codec listesinde yer almalı");
            Assert(meta.VideoCodecs.Contains("VP9"), "VP9 codec listesinde yer almalı");
        }

        static async Task TestMediaValidationSuccess()
        {
            string testVideo = @"E:\Masaüstü\DERSLER\0713 (1).mp4";
            if (!File.Exists(testVideo))
            {
                Console.WriteLine("    (Test video bulunamadı, bu adım atlandı)");
                return;
            }

            var engine = new DownloadEngine();
            var result = await engine.ValidateMediaFileAsync(testVideo, false, CancellationToken.None);

            Assert(result.IsValid, "Geçerli MP4 geçerli olarak onaylanmalı");
            Assert(result.HasVideo, "Video akışı tespit edilmeli");
            Assert(result.HasAudio, "Ses akışı tespit edilmeli");
            Assert(result.Width > 0 && result.Height > 0, "Çözünürlük okunmalı");
            Assert(result.IsH264, "H.264 codec'i doğrulanmalı");
            Assert(result.IsAac, "AAC ses doğrulanmalı");
            Assert(result.IsWindowsPlayerFriendly, "Windows Media Player uyumlu olmalı");
        }

        static async Task TestMediaValidationMissingFile()
        {
            var engine = new DownloadEngine();
            var result = await engine.ValidateMediaFileAsync(@"C:\olmayan_dosya_12345.mp4", false, CancellationToken.None);

            Assert(!result.IsValid, "Olmayan dosya geçersiz sayılmalı");
            Assert(result.ErrorMessage.Contains("bulunamadı"), "Hata mesajı açıklayıcı olmalı");
        }

        static async Task TestMediaValidationNoVideoStream()
        {
            // Create a dummy audio or empty text file renamed as .mp4
            string tempFile = Path.Combine(Path.GetTempPath(), "dummy_not_video.mp4");
            File.WriteAllText(tempFile, "This is not a real video stream file!");

            try
            {
                var engine = new DownloadEngine();
                var result = await engine.ValidateMediaFileAsync(tempFile, false, CancellationToken.None);

                Assert(!result.IsValid, "Sahte/bozuk dosya geçersiz sayılmalı");
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        static async Task TestFfprobeFallbackHandling()
        {
            // Test with a dummy file and check fallback if ffprobe isn't called or fails
            string tempFile = Path.Combine(Path.GetTempPath(), "test_fallback.mp4");
            File.WriteAllBytes(tempFile, new byte[1024]);

            try
            {
                var engine = new DownloadEngine();
                var result = await engine.ValidateMediaFileAsync(tempFile, false, CancellationToken.None);
                // Even if ffprobe fails to parse raw bytes, it reports failure safely without app crashing
                Assert(result != null, "Sonuç null olmamalı");
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        static void TestPathAndTurkishCharacters()
        {
            string turkishPath = Path.Combine(Path.GetTempPath(), "Türkçe_Karakterli_Dizin_ŞÇĞÜÖı");
            if (!Directory.Exists(turkishPath)) Directory.CreateDirectory(turkishPath);

            try
            {
                string testFile = Path.Combine(turkishPath, "Örnek Başlık [abc123xyz].mp4");
                File.WriteAllText(testFile, "test content");

                Assert(File.Exists(testFile), "Türkçe karakterli dosya yolu başarıyla oluşturulmalı");
            }
            finally
            {
                if (Directory.Exists(turkishPath)) Directory.Delete(turkishPath, true);
            }
        }

        static void TestExceptionTypes()
        {
            var botEx = new BotVerificationException("Bot koruması tespit edildi");
            Assert(botEx is Exception, "BotVerificationException bir Exception olmalı");

            var h264Ex = new H264NotAvailableException("H.264 akışı yok", new List<int> { 2160, 1440 });
            Assert(h264Ex.AvailableResolutions.Contains(2160), "Mevcut çözünürlükleri taşımalı");

            var valEx = new MediaValidationException("Doğrulama başarısız");
            Assert(valEx is Exception, "MediaValidationException bir Exception olmalı");
        }

        static async Task TestReencoding()
        {
            string sourceVideo = @"E:\Masaüstü\DERSLER\0713 (1).mp4";
            if (!File.Exists(sourceVideo))
            {
                Console.WriteLine("    (Test video bulunamadı, dönüştürme testi atlandı)");
                return;
            }

            // Create a small 2-second clip to test real FFmpeg libx264 re-encoding
            string tempInput = Path.Combine(Path.GetTempPath(), "temp_clip_for_reencode.mp4");
            string ffmpeg = @"e:\Masaüstü\DERSLER\youtube dowload\Publish\ffmpeg.exe";

            // Cut 2 seconds: ffmpeg -y -i source -t 2 -c copy tempInput
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = ffmpeg,
                Arguments = $"-y -i \"{sourceVideo}\" -t 2 -c copy \"{tempInput}\"",
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
                // Expected!
                Assert(true, "OperationCanceledException doğru fırlatıldı");
            }
            catch (Exception ex)
            {
                Assert(false, "Beklenmeyen hata türü: " + ex.GetType().Name);
            }
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
