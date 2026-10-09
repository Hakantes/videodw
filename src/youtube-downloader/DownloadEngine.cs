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
        
        public string BrowserForCookies { get; set; }

        public DownloadEngine()
        {
            ResolveDependencies();
        }

        public string YtDlpPath => _ytDlpPath;
        public string FFmpegPath => _ffmpegPath;

        public void ResolveDependencies()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 1. yt-dlp resolution
            string[] possibleYtDlpPaths = new[]
            {
                Path.Combine(baseDir, "yt-dlp.exe"),
                Path.Combine(baseDir, "tools", "yt-dlp.exe"),
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
                @"C:\Users\Hakan Ateş\AppData\Local\Microsoft\WinGet\Packages\Gyan.FFmpeg_Microsoft.Winget.Source_8wekyb3d8bbwe\ffmpeg-9.0.2-full_build\bin\ffmpeg.exe"
            };

            _ffmpegPath = possibleFfmpegPaths.FirstOrDefault(File.Exists);

            if (string.IsNullOrEmpty(_ffmpegPath))
            {
                _ffmpegPath = FindInPath("ffmpeg.exe");
            }

            Logger.Log($"Bağımlılıklar: yt-dlp: '{_ytDlpPath}', ffmpeg: '{_ffmpegPath}'");
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

                string args = $"--dump-single-json --no-playlist --no-warnings ";
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

                    // Bekle ve tekrar dene
                    await Task.Delay(delayMs, cancellationToken);
                    delayMs *= 2; // Artan bekleme süresi
                }
            }
            
            throw new Exception("Bilinmeyen bir hata oluştu.");
        }

        private VideoMetadata ParseVideoInfoJson(string json, string url)
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

                if (root.TryGetProperty("formats", out var formatsProp) && formatsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var fmt in formatsProp.EnumerateArray())
                    {
                        if (fmt.TryGetProperty("height", out var hProp) && hProp.ValueKind == JsonValueKind.Number)
                        {
                            int height = hProp.GetInt32();
                            // Filter valid heights
                            if (height >= 144)
                            {
                                resolutions.Add(height);
                            }
                        }
                    }
                }

                var sortedResolutions = resolutions.OrderByDescending(r => r).ToList();

                return new VideoMetadata
                {
                    Title = title,
                    Uploader = uploader,
                    Duration = TimeSpan.FromSeconds(durationSec),
                    ThumbnailUrl = thumbnail,
                    WebpageUrl = url,
                    AvailableResolutions = sortedResolutions
                };
            }
        }

        public async Task<string> DownloadAsync(
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

            // Progress template with distinctive tag
            sbArgs.Append("--newline ");
            sbArgs.Append("--progress-template \"PROG:[%(progress._percent_str)s]|%(progress._speed_str)s|%(progress._eta_str)s|%(progress._total_bytes_str)s|%(progress._downloaded_bytes_str)s\" ");
            sbArgs.Append("--print \"after_move:FINAL_FILE:%(filepath)s\" ");
            sbArgs.Append("--no-playlist ");
            sbArgs.Append("--no-warnings ");
            sbArgs.Append("--windows-filenames ");

            // Performance, Multi-fragment concurrency & Network stability
            sbArgs.Append("--concurrent-fragments 4 ");
            sbArgs.Append("--retries 10 ");
            sbArgs.Append("--fragment-retries 10 ");
            sbArgs.Append("--socket-timeout 30 ");
            sbArgs.Append("--postprocessor-args \"Merger:-c copy\" "); // Kayıpsız hızlı birleştirme

            // FFmpeg location
            if (!string.IsNullOrEmpty(_ffmpegPath) && File.Exists(_ffmpegPath))
            {
                string ffmpegDir = Path.GetDirectoryName(_ffmpegPath);
                sbArgs.Append($"--ffmpeg-location \"{ffmpegDir}\" ");
            }

            // Format selection
            if (request.IsAudioOnly)
            {
                // Only Audio -> Extract audio and convert to MP3 with high quality
                sbArgs.Append("-x --audio-format mp3 --audio-quality 0 ");
            }
            else
            {
                // Video selection
                if (!request.TargetResolution.HasValue || request.TargetResolution.Value <= 0)
                {
                    // Auto / Highest Quality
                    sbArgs.Append("-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/bestvideo+bestaudio/best\" --merge-output-format mp4 ");
                }
                else
                {
                    int res = request.TargetResolution.Value;
                    // Best video with height <= chosen resolution + best audio, merged to mp4
                    sbArgs.Append($"-f \"bestvideo[height<={res}][ext=mp4]+bestaudio[ext=m4a]/bestvideo[height<={res}]+bestaudio/best[height<={res}]\" --merge-output-format mp4 ");
                }
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

                    // Status line analysis
                    if (line.Contains("[Merger]") || line.Contains("Merging formats"))
                    {
                        progress?.Report(new DownloadProgressReport
                        {
                            Percentage = 99,
                            StatusMessage = "Video ve ses FFmpeg ile birleştiriliyor..."
                        });
                    }
                    else if (line.Contains("[ExtractAudio]") || line.Contains("Destination:") && line.EndsWith(".mp3"))
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

                    throw new Exception(string.IsNullOrWhiteSpace(err) ? "İndirme sırasında bir hata oluştu." : err);
                }

                Logger.Log("İndirme başarıyla tamamlandı.");
                return downloadedFilePath;
            }
        }

        private static DownloadProgressReport ParseProgressLine(string line)
        {
            try
            {
                // Format: PROG:[%(progress._percent_str)s]|%(progress._speed_str)s|%(progress._eta_str)s|%(progress._total_bytes_str)s|%(progress._downloaded_bytes_str)s
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
                    // Kill process tree on Windows
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
