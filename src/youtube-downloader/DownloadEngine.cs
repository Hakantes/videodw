using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace youtube_dowload
{
    public class DownloadEngine
    {
        private string _ytDlpPath;
        private string _ffmpegPath;
        private string _ffprobePath;

        public string BrowserForCookies { get; set; }

        public DownloadEngine()
        {
            ResolveDependencies();
        }

        public string YtDlpPath => _ytDlpPath;
        public string FFmpegPath => _ffmpegPath;
        public string FFprobePath => _ffprobePath;

        public void ResolveDependencies()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 1. yt-dlp resolution
            string[] possibleYtDlpPaths = new[]
            {
                Path.Combine(baseDir, "yt-dlp.exe"),
                Path.Combine(baseDir, "tools", "yt-dlp.exe"),
                @"e:\Masaüstü\DERSLER\youtube dowload\Publish\yt-dlp.exe",
                @"e:\Masaüstü\DERSLER\youtube dowload\yt-dlp.exe"
            };

            _ytDlpPath = possibleYtDlpPaths.FirstOrDefault(File.Exists);

            if (string.IsNullOrEmpty(_ytDlpPath))
            {
                _ytDlpPath = FindInPath("yt-dlp.exe");
            }

            // 2. FFmpeg resolution
            string[] possibleFfmpegPaths = new[]
            {
                Path.Combine(baseDir, "ffmpeg.exe"),
                Path.Combine(baseDir, "tools", "ffmpeg.exe"),
                @"e:\Masaüstü\DERSLER\youtube dowload\Publish\ffmpeg.exe",
                @"C:\Users\Hakan Ateş\AppData\Local\Microsoft\WinGet\Packages\Gyan.FFmpeg_Microsoft.Winget.Source_8wekyb3d8bbwe\ffmpeg-9.0.2-full_build\bin\ffmpeg.exe"
            };

            _ffmpegPath = possibleFfmpegPaths.FirstOrDefault(File.Exists);

            if (string.IsNullOrEmpty(_ffmpegPath))
            {
                _ffmpegPath = FindInPath("ffmpeg.exe");
            }

            // 3. ffprobe resolution
            string ffmpegDir = !string.IsNullOrEmpty(_ffmpegPath) && File.Exists(_ffmpegPath)
                ? Path.GetDirectoryName(_ffmpegPath)
                : string.Empty;

            string[] possibleFfprobePaths = new[]
            {
                Path.Combine(baseDir, "ffprobe.exe"),
                Path.Combine(baseDir, "tools", "ffprobe.exe"),
                !string.IsNullOrEmpty(ffmpegDir) ? Path.Combine(ffmpegDir, "ffprobe.exe") : null,
                @"e:\Masaüstü\DERSLER\youtube dowload\Publish\ffprobe.exe",
                @"C:\Users\Hakan Ateş\AppData\Local\Microsoft\WinGet\Packages\Gyan.FFmpeg_Microsoft.Winget.Source_8wekyb3d8bbwe\ffmpeg-9.0.2-full_build\bin\ffprobe.exe"
            }.Where(p => !string.IsNullOrEmpty(p)).ToArray();

            _ffprobePath = possibleFfprobePaths.FirstOrDefault(File.Exists);

            if (string.IsNullOrEmpty(_ffprobePath))
            {
                _ffprobePath = FindInPath("ffprobe.exe");
            }

            Logger.Log($"Bağımlılıklar: yt-dlp: '{_ytDlpPath}', ffmpeg: '{_ffmpegPath}', ffprobe: '{_ffprobePath}'");
        }

        private static string FindInPath(string fileName)
        {
            try
            {
                string pathEnv = Environment.GetEnvironmentVariable("PATH");
                if (string.IsNullOrEmpty(pathEnv)) return null;

                string[] paths = pathEnv.Split(Path.PathSeparator);
                foreach (string p in paths)
                {
                    string full = Path.Combine(p.Trim(), fileName);
                    if (File.Exists(full)) return full;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"FindInPath hata ({fileName}): {ex.Message}");
            }
            return null;
        }

        public bool AreDependenciesReady(out string missingMessage)
        {
            ResolveDependencies();
            if (string.IsNullOrEmpty(_ytDlpPath) || !File.Exists(_ytDlpPath))
            {
                missingMessage = "yt-dlp.exe bulunamadı. Lütfen uygulamanın klasöründe yt-dlp.exe olduğundan emin olun.";
                return false;
            }
            if (string.IsNullOrEmpty(_ffmpegPath) || !File.Exists(_ffmpegPath))
            {
                missingMessage = "ffmpeg.exe bulunamadı. FFmpeg olmadan yüksek çözünürlüklü video birleştirme ve MP3 dönüştürme yapılamaz.";
                return false;
            }
            if (string.IsNullOrEmpty(_ffprobePath) || !File.Exists(_ffprobePath))
            {
                Logger.Log("Uyarı: ffprobe.exe bulunamadı. Medya doğrulama özellikleri kısıtlı çalışabilir.");
            }
            missingMessage = null;
            return true;
        }

        public async Task<VideoMetadata> GetVideoInfoAsync(string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Lütfen geçerli bir video URL'si girin.");

            if (!File.Exists(_ytDlpPath))
                throw new FileNotFoundException("yt-dlp.exe bulunamadı: " + _ytDlpPath);

            int maxRetries = 3;
            int delayMs = 1500;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                Logger.Log($"Video analizi başlatılıyor: {url} (Deneme {attempt})");

                string args = "--dump-single-json --no-playlist --no-warnings ";
                if (!string.IsNullOrWhiteSpace(BrowserForCookies) && BrowserForCookies != "Yok")
                {
                    args += $"--cookies-from-browser {BrowserForCookies.ToLower()} ";
                }
                args += $"\"{url.Trim()}\"";

                var psi = new ProcessStartInfo
                {
                    FileName = _ytDlpPath,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using (var process = new Process { StartInfo = psi })
                {
                    var stdout = new StringBuilder();
                    var stderr = new StringBuilder();

                    process.OutputDataReceived += (s, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
                    process.ErrorDataReceived += (s, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    using (cancellationToken.Register(() => KillProcess(process)))
                    {
                        await Task.Run(() => process.WaitForExit());
                    }

                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException("Video analizi kullanıcı tarafından iptal edildi.");

                    if (process.ExitCode == 0)
                    {
                        string json = stdout.ToString();
                        return ParseVideoInfoJson(json, url);
                    }

                    string err = stderr.ToString().Trim();
                    Logger.Log($"Video analiz hatası (ExitCode {process.ExitCode}): {err}");

                    if (err.Contains("Sign in to confirm you're not a bot") || err.Contains("bot"))
                    {
                        throw new BotVerificationException("YouTube bot olmadığınızı doğrulamak için oturum açmanızı istiyor. Ayarlardan tarayıcı çerezlerini aktif edip tekrar deneyin.");
                    }

                    if (attempt == maxRetries)
                    {
                        throw new Exception(string.IsNullOrWhiteSpace(err) ? "Video bilgileri alınamadı." : err);
                    }

                    await Task.Delay(delayMs, cancellationToken);
                    delayMs *= 2;
                }
            }

            throw new Exception("Bilinmeyen bir hata oluştu.");
        }

        public VideoMetadata ParseVideoInfoJson(string json, string url)
        {
            using (var doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;

                string title = root.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : "Bilinmeyen Video";
                string uploader = root.TryGetProperty("uploader", out var uploaderProp) ? uploaderProp.GetString() :
                                 (root.TryGetProperty("channel", out var chProp) ? chProp.GetString() : "Bilinmeyen Kanal");
                double durationSec = root.TryGetProperty("duration", out var durProp) ? (durProp.ValueKind == JsonValueKind.Number ? durProp.GetDouble() : 0) : 0;
                string thumbnail = root.TryGetProperty("thumbnail", out var thumbProp) ? thumbProp.GetString() : string.Empty;

                var resolutions = new HashSet<int>();
                var h264Resolutions = new HashSet<int>();
                var videoCodecs = new HashSet<string>();
                var audioCodecs = new HashSet<string>();
                bool hasAac = false;

                if (root.TryGetProperty("formats", out var formatsProp) && formatsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var fmt in formatsProp.EnumerateArray())
                    {
                        string vcodec = fmt.TryGetProperty("vcodec", out var vcProp) ? (vcProp.GetString() ?? "none") : "none";
                        string acodec = fmt.TryGetProperty("acodec", out var acProp) ? (acProp.GetString() ?? "none") : "none";

                        bool hasVideo = !string.IsNullOrEmpty(vcodec) && vcodec != "none";
                        bool hasAudio = !string.IsNullOrEmpty(acodec) && acodec != "none";

                        if (hasVideo)
                        {
                            string displayVCodec = NormalizeVideoCodecName(vcodec);
                            videoCodecs.Add(displayVCodec);

                            if (fmt.TryGetProperty("height", out var hProp) && hProp.ValueKind == JsonValueKind.Number)
                            {
                                int height = hProp.GetInt32();
                                if (height >= 144)
                                {
                                    resolutions.Add(height);
                                    if (IsH264Codec(vcodec))
                                    {
                                        h264Resolutions.Add(height);
                                    }
                                }
                            }
                        }

                        if (hasAudio)
                        {
                            string displayACodec = NormalizeAudioCodecName(acodec);
                            audioCodecs.Add(displayACodec);
                            if (IsAacCodec(acodec))
                            {
                                hasAac = true;
                            }
                        }
                    }
                }

                var sortedResolutions = resolutions.OrderByDescending(r => r).ToList();
                var sortedH264Resolutions = h264Resolutions.OrderByDescending(r => r).ToList();

                return new VideoMetadata
                {
                    Title = title,
                    Uploader = uploader,
                    Duration = TimeSpan.FromSeconds(durationSec),
                    ThumbnailUrl = thumbnail,
                    WebpageUrl = url,
                    AvailableResolutions = sortedResolutions,
                    AvailableH264Resolutions = sortedH264Resolutions,
                    VideoCodecs = videoCodecs.OrderBy(c => c).ToList(),
                    AudioCodecs = audioCodecs.OrderBy(c => c).ToList(),
                    HasAacAudioStream = hasAac
                };
            }
        }

        public static bool IsH264Codec(string codec)
        {
            if (string.IsNullOrWhiteSpace(codec) || codec.Equals("none", StringComparison.OrdinalIgnoreCase)) return false;
            return codec.StartsWith("avc1", StringComparison.OrdinalIgnoreCase) ||
                   codec.IndexOf("h264", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   codec.IndexOf("avc", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsAacCodec(string codec)
        {
            if (string.IsNullOrWhiteSpace(codec) || codec.Equals("none", StringComparison.OrdinalIgnoreCase)) return false;
            return codec.StartsWith("mp4a", StringComparison.OrdinalIgnoreCase) ||
                   codec.IndexOf("aac", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string NormalizeVideoCodecName(string codec)
        {
            if (string.IsNullOrWhiteSpace(codec) || codec.Equals("none", StringComparison.OrdinalIgnoreCase)) return "Yok";
            if (IsH264Codec(codec)) return "H.264 (AVC)";
            if (codec.StartsWith("av01", StringComparison.OrdinalIgnoreCase) || codec.IndexOf("av1", StringComparison.OrdinalIgnoreCase) >= 0) return "AV1";
            if (codec.StartsWith("vp09", StringComparison.OrdinalIgnoreCase) || codec.IndexOf("vp9", StringComparison.OrdinalIgnoreCase) >= 0) return "VP9";
            return codec;
        }

        public static string NormalizeAudioCodecName(string codec)
        {
            if (string.IsNullOrWhiteSpace(codec) || codec.Equals("none", StringComparison.OrdinalIgnoreCase)) return "Yok";
            if (IsAacCodec(codec)) return "AAC";
            if (codec.IndexOf("opus", StringComparison.OrdinalIgnoreCase) >= 0) return "Opus";
            if (codec.IndexOf("mp3", StringComparison.OrdinalIgnoreCase) >= 0) return "MP3";
            return codec;
        }

        public static string BuildFormatSelector(DownloadRequest request)
        {
            if (request == null) return "bestvideo+bestaudio/best";

            if (request.IsAudioOnly)
            {
                return "ba/b";
            }

            if (request.Profile == DownloadProfile.WindowsCompatible)
            {
                if (request.TargetResolution.HasValue && request.TargetResolution.Value > 0)
                {
                    int res = request.TargetResolution.Value;
                    return $"bestvideo[height<={res}][vcodec^=avc1]+bestaudio[acodec^=mp4a]/bestvideo[height<={res}][vcodec*=avc]+bestaudio[acodec*=aac]/bestvideo[height<={res}][vcodec^=avc1]+bestaudio/best[height<={res}][vcodec^=avc1]/best[height<={res}][vcodec*=avc]";
                }
                else
                {
                    return "bestvideo[vcodec^=avc1]+bestaudio[acodec^=mp4a]/bestvideo[vcodec*=avc]+bestaudio[acodec*=aac]/bestvideo[vcodec^=avc1]+bestaudio/best[vcodec^=avc1]/best[vcodec*=avc]";
                }
            }

            // BestQuality or ForceReencodeH264
            if (request.TargetResolution.HasValue && request.TargetResolution.Value > 0)
            {
                int res = request.TargetResolution.Value;
                return $"bestvideo[height<={res}]+bestaudio/best[height<={res}]/best";
            }

            return "bestvideo+bestaudio/best";
        }

        public async Task<DownloadResult> DownloadAsync(
            DownloadRequest request,
            IProgress<DownloadProgressReport> progress,
            CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.OutputDirectory))
                throw new ArgumentException("İndirme klasörü belirtilmelidir.");

            if (!Directory.Exists(request.OutputDirectory))
                Directory.CreateDirectory(request.OutputDirectory);

            if (!File.Exists(_ytDlpPath))
                throw new FileNotFoundException("yt-dlp.exe bulunamadı: " + _ytDlpPath);

            var sbArgs = new StringBuilder();

            // Progress template
            sbArgs.Append("--newline ");
            sbArgs.Append("--progress-template \"PROG:[%(progress._percent_str)s]|%(progress._speed_str)s|%(progress._eta_str)s|%(progress._total_bytes_str)s|%(progress._downloaded_bytes_str)s\" ");
            sbArgs.Append("--print \"after_move:FINAL_FILE:%(filepath)s\" ");
            sbArgs.Append("--no-playlist ");
            sbArgs.Append("--no-warnings ");
            sbArgs.Append("--windows-filenames ");

            // Performance & stability
            sbArgs.Append("--concurrent-fragments 4 ");
            sbArgs.Append("--retries 10 ");
            sbArgs.Append("--fragment-retries 10 ");
            sbArgs.Append("--socket-timeout 30 ");

            // FFmpeg location
            if (!string.IsNullOrEmpty(_ffmpegPath) && File.Exists(_ffmpegPath))
            {
                string ffmpegDir = Path.GetDirectoryName(_ffmpegPath);
                sbArgs.Append($"--ffmpeg-location \"{ffmpegDir}\" ");
            }

            // Format selection
            if (request.IsAudioOnly)
            {
                sbArgs.Append("-x --audio-format mp3 --audio-quality 0 ");
            }
            else
            {
                string format = BuildFormatSelector(request);
                sbArgs.Append($"-f \"{format}\" --merge-output-format mp4 ");
                sbArgs.Append("--postprocessor-args \"Merger:-c copy\" ");
            }

            // Browser Cookies
            if (!string.IsNullOrWhiteSpace(BrowserForCookies) && BrowserForCookies != "Yok")
            {
                sbArgs.Append($"--cookies-from-browser {BrowserForCookies.ToLower()} ");
            }

            // Output template with sanitize and unique ID to prevent collisions
            string outTemplate = Path.Combine(request.OutputDirectory, "%(title)s [%(id)s].%(ext)s");
            sbArgs.Append($"--no-overwrites -o \"{outTemplate}\" ");

            // Target URL
            sbArgs.Append($"\"{request.Url.Trim()}\"");

            string arguments = sbArgs.ToString();
            Logger.Log($"İndirme komutu başlatılıyor: {_ytDlpPath} {arguments}");

            var psi = new ProcessStartInfo
            {
                FileName = _ytDlpPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            string downloadedFilePath = null;

            using (var process = new Process { StartInfo = psi })
            {
                var stderr = new StringBuilder();

                process.OutputDataReceived += (s, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data)) return;

                    string line = e.Data;

                    if (line.StartsWith("FINAL_FILE:"))
                    {
                        downloadedFilePath = line.Substring("FINAL_FILE:".Length).Trim();
                        Logger.Log($"Tamamlanan dosya: {downloadedFilePath}");
                        return;
                    }

                    if (line.StartsWith("PROG:"))
                    {
                        var report = ParseProgressLine(line);
                        if (report != null && progress != null)
                        {
                            progress.Report(report);
                        }
                        return;
                    }

                    if (line.Contains("[Merger]") || line.Contains("Merging formats"))
                    {
                        progress?.Report(new DownloadProgressReport
                        {
                            Percentage = 99,
                            StatusMessage = "Video ve ses FFmpeg ile birleştiriliyor..."
                        });
                    }
                    else if (line.Contains("[ExtractAudio]") || (line.Contains("Destination:") && line.EndsWith(".mp3")))
                    {
                        progress?.Report(new DownloadProgressReport
                        {
                            Percentage = 99,
                            StatusMessage = "Ses MP3 formatına dönüştürülüyor..."
                        });
                    }
                    else if (line.Contains("has already been downloaded"))
                    {
                        progress?.Report(new DownloadProgressReport
                        {
                            Percentage = 100,
                            StatusMessage = "Dosya zaten indirilmiş."
                        });
                    }
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data)) return;
                    stderr.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                using (cancellationToken.Register(() => KillProcess(process)))
                {
                    await Task.Run(() => process.WaitForExit());
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Log("İndirme iptal edildi.");
                    throw new OperationCanceledException("İndirme işlemi iptal edildi.");
                }

                if (process.ExitCode != 0)
                {
                    string err = stderr.ToString().Trim();
                    Logger.Log($"İndirme hatası (ExitCode {process.ExitCode}): {err}");

                    if (err.Contains("Sign in to confirm you're not a bot") || err.Contains("bot"))
                    {
                        throw new BotVerificationException("YouTube bot olmadığınızı doğrulamak için oturum açmanızı istiyor. Ayarlardan tarayıcı çerezlerini aktif edip tekrar deneyin.");
                    }

                    if (err.Contains("Requested format is not available") && request.Profile == DownloadProfile.WindowsCompatible)
                    {
                        if (request.AllowReencodeIfH264Missing)
                        {
                            Logger.Log("Kaynakta H.264 bulunamadı; kullanıcı izniyle En İyi Kalite indirilip FFmpeg ile H.264'e dönüştürülecek.");
                            var fallbackRequest = new DownloadRequest
                            {
                                Url = request.Url,
                                OutputDirectory = request.OutputDirectory,
                                IsAudioOnly = false,
                                TargetResolution = request.TargetResolution,
                                Profile = DownloadProfile.BestQuality
                            };

                            var intermediateResult = await DownloadAsync(fallbackRequest, progress, cancellationToken);
                            if (intermediateResult.Success && !string.IsNullOrEmpty(intermediateResult.FilePath))
                            {
                                string reencodedPath = await ReencodeToH264Async(intermediateResult.FilePath, progress, cancellationToken);
                                var postValidation = await ValidateMediaFileAsync(reencodedPath, false, cancellationToken);
                                return new DownloadResult
                                {
                                    Success = true,
                                    FilePath = reencodedPath,
                                    Validation = postValidation,
                                    WasReencoded = true,
                                    StatusMessage = "İndirme ve H.264 dönüştürmesi başarıyla tamamlandı."
                                };
                            }
                        }
                        else
                        {
                            throw new H264NotAvailableException("Seçilen çözünürlük veya video için kaynakta yerel H.264 (AVC) akışı bulunamadı (yalnızca AV1 veya VP9 mevcut).");
                        }
                    }

                    throw new Exception(string.IsNullOrWhiteSpace(err) ? "İndirme sırasında bir hata oluştu." : err);
                }

                // If output path wasn't captured via FINAL_FILE tag, attempt finding it
                if (string.IsNullOrEmpty(downloadedFilePath) || !File.Exists(downloadedFilePath))
                {
                    downloadedFilePath = FindLatestDownloadedFile(request.OutputDirectory);
                }

                if (string.IsNullOrEmpty(downloadedFilePath) || !File.Exists(downloadedFilePath))
                {
                    throw new FileNotFoundException("İndirilen dosya diskte bulunamadı.");
                }

                bool wasReencoded = false;

                // Handle ForceReencodeH264 profile or non-H264 conversion
                if (request.Profile == DownloadProfile.ForceReencodeH264 && !request.IsAudioOnly)
                {
                    var preVal = await ValidateMediaFileAsync(downloadedFilePath, false, cancellationToken);
                    if (preVal.IsH264 && preVal.IsAac)
                    {
                        Logger.Log("Dosya zaten H.264 ve AAC formatında, gereksiz yeniden kodlama atlandı.");
                    }
                    else
                    {
                        downloadedFilePath = await ReencodeToH264Async(downloadedFilePath, progress, cancellationToken);
                        wasReencoded = true;
                    }
                }
                else if (request.Profile == DownloadProfile.WindowsCompatible && !request.IsAudioOnly)
                {
                    // If video was merged but audio is Opus, quickly convert audio to AAC (video stream copied)
                    var checkVal = await ValidateMediaFileAsync(downloadedFilePath, false, cancellationToken);
                    if (checkVal.IsH264 && !checkVal.IsAac && !string.IsNullOrEmpty(checkVal.AudioCodec))
                    {
                        Logger.Log($"H.264 video için ses formatı '{checkVal.AudioCodec}' tespit edildi; AAC formatına dönüştürülüyor...");
                        downloadedFilePath = await ConvertAudioToAacAsync(downloadedFilePath, progress, cancellationToken);
                    }
                }

                // Final media validation with ffprobe
                var validation = await ValidateMediaFileAsync(downloadedFilePath, request.IsAudioOnly, cancellationToken);
                if (!validation.IsValid)
                {
                    Logger.Log($"Çıktı doğrulama uyarısı/hatası: {validation.ErrorMessage}");
                    throw new MediaValidationException(validation.ErrorMessage);
                }

                Logger.Log($"İndirme ve doğrulama başarıyla tamamlandı: {downloadedFilePath} (Video: {validation.VideoCodec}, Ses: {validation.AudioCodec})");

                return new DownloadResult
                {
                    Success = true,
                    FilePath = downloadedFilePath,
                    Validation = validation,
                    WasReencoded = wasReencoded,
                    StatusMessage = "İndirme ve medya doğrulaması başarıyla tamamlandı."
                };
            }
        }

        public async Task<string> ReencodeToH264Async(
            string inputFilePath,
            IProgress<DownloadProgressReport> progress,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(inputFilePath) || !File.Exists(inputFilePath))
                throw new FileNotFoundException("Dönüştürülecek dosya bulunamadı: " + inputFilePath);

            if (string.IsNullOrEmpty(_ffmpegPath) || !File.Exists(_ffmpegPath))
                throw new FileNotFoundException("ffmpeg.exe bulunamadı: " + _ffmpegPath);

            string dir = Path.GetDirectoryName(inputFilePath) ?? AppDomain.CurrentDomain.BaseDirectory;
            string nameWithoutExt = Path.GetFileNameWithoutExtension(inputFilePath);
            string tempOutputFile = Path.Combine(dir, $"{nameWithoutExt}_h264_temp.mp4");

            if (File.Exists(tempOutputFile))
            {
                try { File.Delete(tempOutputFile); } catch { }
            }

            Logger.Log($"FFmpeg ile H.264 dönüştürme başlatılıyor: '{inputFilePath}' -> '{tempOutputFile}'");

            progress?.Report(new DownloadProgressReport
            {
                Percentage = 99,
                StatusMessage = "Video Windows uyumlu H.264 formatına dönüştürülüyor (Lütfen bekleyin)..."
            });

            string arguments = $"-y -i \"{inputFilePath}\" -c:v libx264 -preset faster -crf 20 -c:a aac -b:a 192k -movflags +faststart \"{tempOutputFile}\"";

            var psi = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (var process = new Process { StartInfo = psi })
            {
                var stderr = new StringBuilder();
                process.ErrorDataReceived += (s, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

                process.Start();
                process.BeginErrorReadLine();

                using (cancellationToken.Register(() => KillProcess(process)))
                {
                    await Task.Run(() => process.WaitForExit());
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    if (File.Exists(tempOutputFile)) { try { File.Delete(tempOutputFile); } catch { } }
                    throw new OperationCanceledException("H.264 dönüştürme işlemi iptal edildi. Orijinal dosya korundu.");
                }

                if (process.ExitCode != 0)
                {
                    if (File.Exists(tempOutputFile)) { try { File.Delete(tempOutputFile); } catch { } }
                    string err = stderr.ToString().Trim();
                    Logger.Log($"FFmpeg dönüştürme hatası: {err}");
                    throw new Exception("H.264 dönüştürmesi başarısız oldu. Orijinal dosya korundu: " + (string.IsNullOrWhiteSpace(err) ? "Bilinmeyen FFmpeg hatası" : err));
                }

                // Swap files
                try
                {
                    File.Delete(inputFilePath);
                    File.Move(tempOutputFile, inputFilePath);
                    Logger.Log($"H.264 dönüştürme başarıyla tamamlandı: '{inputFilePath}'");
                    return inputFilePath;
                }
                catch (Exception ex)
                {
                    Logger.Log($"Dosya değiştirme uyarısı: {ex.Message}. Dönüştürülen dosya: '{tempOutputFile}'");
                    return tempOutputFile;
                }
            }
        }

        public async Task<string> ConvertAudioToAacAsync(
            string inputFilePath,
            IProgress<DownloadProgressReport> progress,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(inputFilePath) || !File.Exists(inputFilePath)) return inputFilePath;
            if (string.IsNullOrEmpty(_ffmpegPath) || !File.Exists(_ffmpegPath)) return inputFilePath;

            string dir = Path.GetDirectoryName(inputFilePath) ?? AppDomain.CurrentDomain.BaseDirectory;
            string nameWithoutExt = Path.GetFileNameWithoutExtension(inputFilePath);
            string tempOutputFile = Path.Combine(dir, $"{nameWithoutExt}_aac_temp.mp4");

            if (File.Exists(tempOutputFile))
            {
                try { File.Delete(tempOutputFile); } catch { }
            }

            progress?.Report(new DownloadProgressReport
            {
                Percentage = 99,
                StatusMessage = "Ses akışı Windows uyumlu AAC formatına dönüştürülüyor..."
            });

            // Video is untouched (copy), only audio is converted to AAC
            string arguments = $"-y -i \"{inputFilePath}\" -c:v copy -c:a aac -b:a 192k -movflags +faststart \"{tempOutputFile}\"";

            var psi = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (var process = new Process { StartInfo = psi })
            {
                process.Start();
                using (cancellationToken.Register(() => KillProcess(process)))
                {
                    await Task.Run(() => process.WaitForExit());
                }

                if (process.ExitCode == 0 && File.Exists(tempOutputFile))
                {
                    try
                    {
                        File.Delete(inputFilePath);
                        File.Move(tempOutputFile, inputFilePath);
                        return inputFilePath;
                    }
                    catch
                    {
                        return tempOutputFile;
                    }
                }
                else
                {
                    if (File.Exists(tempOutputFile)) { try { File.Delete(tempOutputFile); } catch { } }
                    return inputFilePath;
                }
            }
        }

        public async Task<MediaValidationResult> ValidateMediaFileAsync(
            string filePath,
            bool isAudioOnly,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return new MediaValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Çıktı dosyası diskte bulunamadı."
                };
            }

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length == 0)
            {
                return new MediaValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Çıktı dosyasının boyutu 0 bayt (boş dosya)."
                };
            }

            if (string.IsNullOrEmpty(_ffprobePath) || !File.Exists(_ffprobePath))
            {
                ResolveDependencies();
            }

            if (string.IsNullOrEmpty(_ffprobePath) || !File.Exists(_ffprobePath))
            {
                Logger.Log("UYARI: ffprobe.exe bulunamadı; temel dosya varlığı ve boyutu doğrulandı.");
                return new MediaValidationResult
                {
                    IsValid = true,
                    HasVideo = !isAudioOnly,
                    HasAudio = true,
                    FileSizeBytes = fileInfo.Length,
                    Container = Path.GetExtension(filePath),
                    ErrorMessage = "ffprobe bulunamadığı için akış bilgileri denetlenemedi, ancak dosya oluşturuldu."
                };
            }

            string arguments = $"-v error -show_entries format=format_name,duration,size:stream=index,codec_name,codec_type,width,height -of json \"{filePath}\"";

            var psi = new ProcessStartInfo
            {
                FileName = _ffprobePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (var process = new Process { StartInfo = psi })
            {
                var stdout = new StringBuilder();
                var stderr = new StringBuilder();

                process.OutputDataReceived += (s, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
                process.ErrorDataReceived += (s, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                using (cancellationToken.Register(() => KillProcess(process)))
                {
                    await Task.Run(() => process.WaitForExit());
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException("Doğrulama işlemi iptal edildi.");
                }

                if (process.ExitCode != 0)
                {
                    string err = stderr.ToString().Trim();
                    Logger.Log($"ffprobe hatası: {err}");
                    return new MediaValidationResult
                    {
                        IsValid = false,
                        FileSizeBytes = fileInfo.Length,
                        ErrorMessage = $"ffprobe dosyayı okuyamadı (bozuk veya tamamlanmamış medya): {err}"
                    };
                }

                string json = stdout.ToString();
                try
                {
                    using (var doc = JsonDocument.Parse(json))
                    {
                        var root = doc.RootElement;
                        bool hasVideo = false;
                        bool hasAudio = false;
                        string videoCodec = string.Empty;
                        string audioCodec = string.Empty;
                        int width = 0;
                        int height = 0;
                        double duration = 0;
                        string container = string.Empty;

                        if (root.TryGetProperty("streams", out var streams) && streams.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var s in streams.EnumerateArray())
                            {
                                string type = s.TryGetProperty("codec_type", out var tProp) ? tProp.GetString() : "";
                                string name = s.TryGetProperty("codec_name", out var nProp) ? nProp.GetString() : "";

                                if (type == "video" && !hasVideo)
                                {
                                    hasVideo = true;
                                    videoCodec = name ?? "";
                                    width = s.TryGetProperty("width", out var wProp) ? wProp.GetInt32() : 0;
                                    height = s.TryGetProperty("height", out var hProp) ? hProp.GetInt32() : 0;
                                }
                                else if (type == "audio" && !hasAudio)
                                {
                                    hasAudio = true;
                                    audioCodec = name ?? "";
                                }
                            }
                        }

                        if (root.TryGetProperty("format", out var fmt))
                        {
                            container = fmt.TryGetProperty("format_name", out var fn) ? fn.GetString() ?? "" : "";
                            if (fmt.TryGetProperty("duration", out var durProp))
                            {
                                double.TryParse(durProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out duration);
                            }
                        }

                        if (!isAudioOnly && !hasVideo)
                        {
                            return new MediaValidationResult
                            {
                                IsValid = false,
                                HasAudio = hasAudio,
                                AudioCodec = audioCodec,
                                FileSizeBytes = fileInfo.Length,
                                ErrorMessage = "Doğrulama Hatası: İndirilen dosyada hiçbir video akışı bulunamadı (yalnızca ses veya bozuk dosya)."
                            };
                        }

                        if (!isAudioOnly && (width <= 0 || height <= 0))
                        {
                            return new MediaValidationResult
                            {
                                IsValid = false,
                                HasVideo = true,
                                VideoCodec = videoCodec,
                                FileSizeBytes = fileInfo.Length,
                                ErrorMessage = "Doğrulama Hatası: Video çözünürlük bilgisi geçersiz (0x0)."
                            };
                        }

                        if (isAudioOnly && !hasAudio)
                        {
                            return new MediaValidationResult
                            {
                                IsValid = false,
                                FileSizeBytes = fileInfo.Length,
                                ErrorMessage = "Doğrulama Hatası: İndirilen dosyada ses akışı bulunamadı."
                            };
                        }

                        return new MediaValidationResult
                        {
                            IsValid = true,
                            HasVideo = hasVideo,
                            HasAudio = hasAudio,
                            VideoCodec = videoCodec,
                            AudioCodec = audioCodec,
                            Width = width,
                            Height = height,
                            Duration = TimeSpan.FromSeconds(duration),
                            FileSizeBytes = fileInfo.Length,
                            Container = container
                        };
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"ffprobe JSON ayrıştırma hatası: {ex.Message}");
                    return new MediaValidationResult
                    {
                        IsValid = false,
                        FileSizeBytes = fileInfo.Length,
                        ErrorMessage = "ffprobe çıktısı ayrıştırılamadı: " + ex.Message
                    };
                }
            }
        }

        private static string FindLatestDownloadedFile(string directory)
        {
            try
            {
                var dirInfo = new DirectoryInfo(directory);
                var file = dirInfo.GetFiles()
                    .Where(f => !f.Name.EndsWith(".part") && !f.Name.EndsWith(".ytdl") && !f.Name.EndsWith(".temp"))
                    .OrderByDescending(f => f.LastWriteTime)
                    .FirstOrDefault();
                return file?.FullName;
            }
            catch
            {
                return null;
            }
        }

        private static DownloadProgressReport ParseProgressLine(string line)
        {
            try
            {
                string data = line.Substring("PROG:".Length);
                string[] parts = data.Split('|');
                if (parts.Length < 3) return null;

                string rawPercent = parts[0].Trim('[', ']', ' ', '%');
                double pct = 0;
                if (!string.IsNullOrEmpty(rawPercent))
                {
                    double.TryParse(rawPercent, NumberStyles.Any, CultureInfo.InvariantCulture, out pct);
                }

                string speed = parts.Length > 1 ? parts[1].Trim() : "";
                string eta = parts.Length > 2 ? parts[2].Trim() : "";
                string total = parts.Length > 3 ? parts[3].Trim() : "";
                string downloaded = parts.Length > 4 ? parts[4].Trim() : "";

                return new DownloadProgressReport
                {
                    Percentage = pct,
                    Speed = string.IsNullOrWhiteSpace(speed) || speed == "NA" ? "" : speed,
                    Eta = string.IsNullOrWhiteSpace(eta) || eta == "NA" ? "" : eta,
                    TotalBytes = string.IsNullOrWhiteSpace(total) || total == "NA" ? "" : total,
                    DownloadedBytes = string.IsNullOrWhiteSpace(downloaded) || downloaded == "NA" ? "" : downloaded,
                    StatusMessage = $"İndiriliyor... %{pct:F1}"
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> UpdateEngineAsync()
        {
            if (!File.Exists(_ytDlpPath))
                throw new FileNotFoundException("yt-dlp.exe bulunamadı.");

            Logger.Log("yt-dlp güncellemesi başlatılıyor...");

            var psi = new ProcessStartInfo
            {
                FileName = _ytDlpPath,
                Arguments = "-U",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (var process = new Process { StartInfo = psi })
            {
                var output = new StringBuilder();
                process.OutputDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };
                process.ErrorDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.Run(() => process.WaitForExit());

                string result = output.ToString().Trim();
                Logger.Log($"Güncelleme çıktısı: {result}");
                return result;
            }
        }

        private static void KillProcess(Process process)
        {
            try
            {
                if (process != null && !process.HasExited)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "taskkill",
                        Arguments = $"/F /T /PID {process.Id}",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    })?.WaitForExit(3000);

                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Process sonlandırma hatası: " + ex.Message);
            }
        }
    }
}
