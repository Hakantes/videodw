using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YouTubeDownloader.Mac.Models;

namespace YouTubeDownloader.Mac.Services
{
    public class MacDownloadEngine
    {
        public string YtDlpPath { get; private set; } = string.Empty;
        public string FFmpegPath { get; private set; } = string.Empty;

        public MacDownloadEngine()
        {
            ResolveToolPaths();
        }

        public void ResolveToolPaths()
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            string ytDlpExe = isWindows ? "yt-dlp.exe" : "yt-dlp";
            string ffmpegExe = isWindows ? "ffmpeg.exe" : "ffmpeg";

            YtDlpPath = FindBinary(ytDlpExe);
            FFmpegPath = FindBinary(ffmpegExe);

            // Ensure unix executable permissions if on Unix/macOS
            if (!isWindows)
            {
                EnsureExecutable(YtDlpPath);
                EnsureExecutable(FFmpegPath);
            }
        }

        private static void EnsureExecutable(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "chmod",
                    ArgumentList = { "+x", filePath },
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(3000);
            }
            catch { }
        }

        private static string FindBinary(string name)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 1. App tools directory
            string localTool = Path.Combine(baseDir, "tools", name);
            if (File.Exists(localTool)) return localTool;

            // 2. Base directory
            string directTool = Path.Combine(baseDir, name);
            if (File.Exists(directTool)) return directTool;

            // 3. macOS Homebrew Apple Silicon
            string brewSilicon = Path.Combine("/opt/homebrew/bin", name);
            if (File.Exists(brewSilicon)) return brewSilicon;

            // 4. macOS Homebrew Intel / Linux
            string usrLocal = Path.Combine("/usr/local/bin", name);
            if (File.Exists(usrLocal)) return usrLocal;

            // 5. System PATH search
            string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            char sep = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
            foreach (var dir in pathEnv.Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    string candidate = Path.Combine(dir.Trim(), name);
                    if (File.Exists(candidate)) return candidate;
                }
                catch { }
            }

            return name; // Fallback to bare command name
        }

        public bool AreDependenciesReady(out string message)
        {
            bool hasYtDlp = !string.IsNullOrEmpty(YtDlpPath) && (File.Exists(YtDlpPath) || IsInSystemPath(YtDlpPath));
            bool hasFFmpeg = !string.IsNullOrEmpty(FFmpegPath) && (File.Exists(FFmpegPath) || IsInSystemPath(FFmpegPath));

            if (!hasYtDlp && !hasFFmpeg)
            {
                message = "yt-dlp ve FFmpeg bulunamadı. Lütfen Homebrew ile kurun: 'brew install yt-dlp ffmpeg'";
                return false;
            }
            if (!hasYtDlp)
            {
                message = "yt-dlp bulunamadı. Lütfen kurun: 'brew install yt-dlp'";
                return false;
            }
            if (!hasFFmpeg)
            {
                message = "FFmpeg bulunamadı (Video birleştirme ve MP3 için gereklidir): 'brew install ffmpeg'";
                return false;
            }

            message = "Tüm araçlar hazır.";
            return true;
        }

        private static bool IsInSystemPath(string binaryName)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = binaryName,
                    ArgumentList = { "--version" },
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using var proc = Process.Start(psi);
                return proc != null && proc.WaitForExit(3000) && proc.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<VideoMetadata> GetVideoInfoAsync(string url, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Video bağlantısı boş olamaz.");

            var psi = new ProcessStartInfo
            {
                FileName = YtDlpPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            psi.ArgumentList.Add("--dump-single-json");
            psi.ArgumentList.Add("--no-warnings");
            psi.ArgumentList.Add("--no-playlist");
            psi.ArgumentList.Add(url.Trim());

            using var process = new Process { StartInfo = psi };
            var stdoutSb = new StringBuilder();
            var stderrSb = new StringBuilder();

            process.OutputDataReceived += (s, e) => { if (e.Data != null) stdoutSb.AppendLine(e.Data); };
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) stderrSb.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using (ct.Register(() => { try { process.Kill(); } catch { } }))
            {
                await Task.Run(() => process.WaitForExit(), ct);
            }

            if (process.ExitCode != 0)
            {
                string err = stderrSb.ToString().Trim();
                throw new InvalidOperationException($"Video bilgisi alınamadı (Çıkış kodu {process.ExitCode}): {err}");
            }

            string json = stdoutSb.ToString();
            return ParseVideoInfoJson(json, url);
        }

        private VideoMetadata ParseVideoInfoJson(string json, string url)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var meta = new VideoMetadata
            {
                Url = url,
                Title = root.TryGetProperty("title", out var t) ? (t.GetString() ?? "Video") : "Video",
                Channel = root.TryGetProperty("uploader", out var u) ? (u.GetString() ?? "") : "",
                ThumbnailUrl = root.TryGetProperty("thumbnail", out var th) ? (th.GetString() ?? "") : ""
            };

            if (root.TryGetProperty("duration", out var durProp))
            {
                if (durProp.ValueKind == JsonValueKind.Number && durProp.TryGetDouble(out double sec))
                {
                    meta.Duration = TimeSpan.FromSeconds(Math.Max(0, sec));
                }
            }

            // Parse available formats and estimate sizes
            var formatsList = new List<JsonElement>();
            if (root.TryGetProperty("formats", out var fmtArray) && fmtArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in fmtArray.EnumerateArray())
                {
                    formatsList.Add(el);
                }
            }

            // Find best audio size for combination
            long bestAudioSize = 0;
            foreach (var f in formatsList)
            {
                string vcodec = f.TryGetProperty("vcodec", out var vc) ? vc.GetString() ?? "none" : "none";
                string acodec = f.TryGetProperty("acodec", out var ac) ? ac.GetString() ?? "none" : "none";
                if (vcodec == "none" && acodec != "none")
                {
                    long sz = GetFormatSize(f, meta.Duration);
                    if (sz > bestAudioSize) bestAudioSize = sz;
                }
            }

            var resolutions = new[] { 1080, 720, 480, 360 };
            foreach (var res in resolutions)
            {
                // Find matching format
                JsonElement? matchingFormat = null;
                foreach (var f in formatsList)
                {
                    if (f.TryGetProperty("height", out var hProp) && hProp.ValueKind == JsonValueKind.Number)
                    {
                        if (hProp.TryGetInt32(out int h) && h == res)
                        {
                            matchingFormat = f;
                            break;
                        }
                    }
                }

                long? approxSize = null;
                string formattedSize = "Boyut hesaplanamadı";

                if (matchingFormat.HasValue)
                {
                    long vSize = GetFormatSize(matchingFormat.Value, meta.Duration);
                    if (vSize > 0)
                    {
                        long totalEst = vSize + bestAudioSize;
                        approxSize = totalEst;
                        formattedSize = $"~{DownloadProgressReport.FormatBytes(totalEst)}";
                    }
                }

                meta.AvailableFormats.Add(new FormatOption
                {
                    Resolution = res,
                    Label = $"{res}p (MP4 - macOS / QuickTime Uyumlu)",
                    Extension = "mp4",
                    ApproxSizeBytes = approxSize,
                    FormattedSize = formattedSize,
                    IsAudioOnly = false
                });
            }

            // Best quality option
            meta.AvailableFormats.Insert(0, new FormatOption
            {
                Resolution = null,
                Label = "En İyi Kalite (Orijinal Kaynak Çözünürlüğü)",
                Extension = "mp4",
                ApproxSizeBytes = null,
                FormattedSize = "Otomatik",
                IsAudioOnly = false
            });

            // Audio only option
            long audioOnlyEst = bestAudioSize > 0 ? bestAudioSize : (long)(meta.Duration.TotalSeconds * 24000);
            meta.AvailableFormats.Add(new FormatOption
            {
                Resolution = null,
                Label = "Yalnızca Ses (MP3 Yüksek Kalite)",
                Extension = "mp3",
                ApproxSizeBytes = audioOnlyEst > 0 ? audioOnlyEst : (long?)null,
                FormattedSize = audioOnlyEst > 0 ? $"~{DownloadProgressReport.FormatBytes(audioOnlyEst)}" : "Boyut hesaplanamadı",
                IsAudioOnly = true
            });

            return meta;
        }

        private static long GetFormatSize(JsonElement f, TimeSpan duration)
        {
            if (f.TryGetProperty("filesize", out var fs) && fs.ValueKind == JsonValueKind.Number && fs.TryGetInt64(out long s) && s > 0)
                return s;

            if (f.TryGetProperty("filesize_approx", out var fsa) && fsa.ValueKind == JsonValueKind.Number && fsa.TryGetInt64(out long sa) && sa > 0)
                return sa;

            if (f.TryGetProperty("tbr", out var tbr) && tbr.ValueKind == JsonValueKind.Number && tbr.TryGetDouble(out double kbps) && kbps > 0 && duration.TotalSeconds > 0)
            {
                return (long)(kbps * 1024.0 / 8.0 * duration.TotalSeconds);
            }

            return 0;
        }

        public async Task<DownloadResult> DownloadAsync(
            string url,
            FormatOption format,
            string outputDirectory,
            IProgress<DownloadProgressReport> progress,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory) || !Directory.Exists(outputDirectory))
            {
                outputDirectory = MacSettingsService.GetDefaultDownloadDirectory();
                if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);
            }

            var psi = new ProcessStartInfo
            {
                FileName = YtDlpPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            string outTemplate = Path.Combine(outputDirectory, "%(title)s.%(ext)s");
            psi.ArgumentList.Add("-o");
            psi.ArgumentList.Add(outTemplate);
            psi.ArgumentList.Add("--newline");
            psi.ArgumentList.Add("--no-warnings");
            psi.ArgumentList.Add("--progress-template");
            psi.ArgumentList.Add("download:PROG:%(progress.downloaded_bytes)s:%(progress.total_bytes)s:%(progress.total_bytes_estimate)s:%(progress.speed)s:%(progress.eta)s");

            if (!string.IsNullOrEmpty(FFmpegPath) && File.Exists(FFmpegPath))
            {
                psi.ArgumentList.Add("--ffmpeg-location");
                psi.ArgumentList.Add(Path.GetDirectoryName(FFmpegPath) ?? FFmpegPath);
            }

            if (format.IsAudioOnly)
            {
                psi.ArgumentList.Add("-x");
                psi.ArgumentList.Add("--audio-format");
                psi.ArgumentList.Add("mp3");
                psi.ArgumentList.Add("--audio-quality");
                psi.ArgumentList.Add("0");
            }
            else if (format.Resolution.HasValue)
            {
                // Format specifier for resolution with MP4/AAC container
                int res = format.Resolution.Value;
                psi.ArgumentList.Add("-f");
                psi.ArgumentList.Add($"bestvideo[height<={res}][ext=mp4]+bestaudio[ext=m4a]/bestvideo[height<={res}]+bestaudio/best[height<={res}]/best");
                psi.ArgumentList.Add("--merge-output-format");
                psi.ArgumentList.Add("mp4");
            }
            else
            {
                // Best quality
                psi.ArgumentList.Add("-f");
                psi.ArgumentList.Add("bestvideo+bestaudio/best");
                psi.ArgumentList.Add("--merge-output-format");
                psi.ArgumentList.Add("mp4");
            }

            psi.ArgumentList.Add(url.Trim());

            using var process = new Process { StartInfo = psi };
            string lastDestinationFile = string.Empty;
            var stderrSb = new StringBuilder();

            process.OutputDataReceived += (s, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;

                if (e.Data.StartsWith("download:PROG:"))
                {
                    var report = ParseProgressLine(e.Data, format.ApproxSizeBytes);
                    if (report != null && progress != null)
                    {
                        progress.Report(report);
                    }
                }
                else if (e.Data.Contains("[download] Destination:") || e.Data.Contains("[Merger] Merging formats into"))
                {
                    var match = Regex.Match(e.Data, @"""([^""]+)""");
                    if (match.Success)
                    {
                        lastDestinationFile = match.Groups[1].Value;
                    }
                    else
                    {
                        int idx = e.Data.LastIndexOf(':');
                        if (idx >= 0 && idx < e.Data.Length - 1)
                        {
                            lastDestinationFile = e.Data.Substring(idx + 1).Trim();
                        }
                    }
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data)) stderrSb.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            try
            {
                using (ct.Register(() => { try { process.Kill(); } catch { } }))
                {
                    await Task.Run(() => process.WaitForExit(), ct);
                }
            }
            catch (OperationCanceledException)
            {
                return new DownloadResult { WasCancelled = true, ErrorMessage = "İndirme kullanıcı tarafından iptal edildi." };
            }

            if (ct.IsCancellationRequested)
            {
                return new DownloadResult { WasCancelled = true, ErrorMessage = "İndirme iptal edildi." };
            }

            if (process.ExitCode != 0)
            {
                return new DownloadResult
                {
                    Success = false,
                    ErrorMessage = $"İndirme başarısız oldu (Hata kodu {process.ExitCode}): {stderrSb.ToString().Trim()}"
                };
            }

            // Report 100% completion
            progress?.Report(new DownloadProgressReport
            {
                Stage = DownloadStage.Completed,
                Percentage = 100.0,
                StatusMessage = "İndirme ve işleme tamamlandı."
            });

            return new DownloadResult
            {
                Success = true,
                FilePath = lastDestinationFile
            };
        }

        private static DownloadProgressReport? ParseProgressLine(string line, long? estimatedTotalBytes)
        {
            // Format: download:PROG:downloaded_bytes:total_bytes:total_bytes_estimate:speed:eta
            string[] parts = line.Split(':');
            if (parts.Length < 7) return null;

            long? downloaded = long.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out long d) && d >= 0 ? d : (long?)null;
            long? total = long.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out long t) && t > 0 ? t : (long?)null;
            long? estTotal = long.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out long et) && et > 0 ? et : (long?)null;
            double? speed = double.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out double sp) && sp >= 0 ? sp : (double?)null;
            TimeSpan? eta = int.TryParse(parts[6], NumberStyles.Any, CultureInfo.InvariantCulture, out int etaSec) && etaSec >= 0 ? TimeSpan.FromSeconds(etaSec) : (TimeSpan?)null;

            long? effectiveTotal = total ?? estTotal ?? estimatedTotalBytes;
            double pct = 0;
            if (downloaded.HasValue && effectiveTotal.HasValue && effectiveTotal.Value > 0)
            {
                pct = Math.Min(99.0, Math.Max(0.0, (double)downloaded.Value / effectiveTotal.Value * 100.0));
            }

            return new DownloadProgressReport
            {
                DownloadedBytes = downloaded,
                TotalBytes = effectiveTotal,
                SpeedBytesPerSec = speed,
                Eta = eta,
                Percentage = pct,
                StatusMessage = $"İndiriliyor... %{pct:F0}"
            };
        }
    }
}
