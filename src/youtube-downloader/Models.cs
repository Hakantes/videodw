using System;
using System.Collections.Generic;
using System.Linq;

namespace youtube_dowload
{
    public enum DownloadProfile
    {
        WindowsCompatible,   // Profil B: Windows Uyumlu MP4 (H.264/AAC öncelikli)
        BestQuality,         // Profil A: En İyi Kalite (Orijinal codec - AV1/VP9/AVC)
        ForceReencodeH264    // İsteğe bağlı: H.264'e Dönüştür (FFmpeg libx264 ile yeniden kodla)
    }

    public class FormatMetadata
    {
        public string FormatId { get; set; } = string.Empty;
        public string Ext { get; set; } = string.Empty;
        public string VCodec { get; set; } = string.Empty;
        public string ACodec { get; set; } = string.Empty;
        public int? Height { get; set; }
        public int? Width { get; set; }
        public long? Filesize { get; set; }
        public long? FilesizeApprox { get; set; }
        public double? Tbr { get; set; }
        public double? Vbr { get; set; }
        public double? Abr { get; set; }
        public double? Fps { get; set; }

        public bool HasVideo => !string.IsNullOrEmpty(VCodec) && !VCodec.Equals("none", StringComparison.OrdinalIgnoreCase);
        public bool HasAudio => !string.IsNullOrEmpty(ACodec) && !ACodec.Equals("none", StringComparison.OrdinalIgnoreCase);
        public bool IsH264 => DownloadEngine.IsH264Codec(VCodec);
        public bool IsAac => DownloadEngine.IsAacCodec(ACodec);

        public long? EffectiveFilesize
        {
            get
            {
                if (Filesize.HasValue && Filesize.Value > 0) return Filesize.Value;
                if (FilesizeApprox.HasValue && FilesizeApprox.Value > 0) return FilesizeApprox.Value;
                return null;
            }
        }
    }

    public class SizeEstimationResult
    {
        public long? TotalSizeBytes { get; set; }
        public long? VideoSizeBytes { get; set; }
        public long? AudioSizeBytes { get; set; }
        public bool IsApproximate { get; set; }
        public bool NeedsReencoding { get; set; }
        public string VideoFormatId { get; set; }
        public string AudioFormatId { get; set; }
        public string VideoCodec { get; set; }
        public string AudioCodec { get; set; }
        public int? SelectedResolution { get; set; }
        public string FormattedSize { get; set; } = "Boyut hesaplanamadı";
        public string Explanation { get; set; } = string.Empty;
    }

    public class VideoMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string Uploader { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string WebpageUrl { get; set; } = string.Empty;
        public List<int> AvailableResolutions { get; set; } = new List<int>();
        public List<int> AvailableH264Resolutions { get; set; } = new List<int>();
        public List<string> VideoCodecs { get; set; } = new List<string>();
        public List<string> AudioCodecs { get; set; } = new List<string>();
        public List<FormatMetadata> Formats { get; set; } = new List<FormatMetadata>();
        public bool HasH264Stream => AvailableH264Resolutions != null && AvailableH264Resolutions.Count > 0;
        public bool HasAacAudioStream { get; set; }

        public string FormattedDuration
        {
            get
            {
                if (Duration.TotalHours >= 24)
                {
                    int totalHours = (int)Math.Floor(Duration.TotalHours);
                    return $"{totalHours}:{Duration.Minutes:D2}:{Duration.Seconds:D2}";
                }
                if (Duration.TotalHours >= 1)
                    return Duration.ToString(@"hh\:mm\:ss");
                return Duration.ToString(@"mm\:ss");
            }
        }

        public long? GetFormatSizeBytes(FormatMetadata fmt)
        {
            if (fmt == null) return null;
            if (fmt.EffectiveFilesize.HasValue) return fmt.EffectiveFilesize.Value;
            if (fmt.Tbr.HasValue && fmt.Tbr.Value > 0 && Duration.TotalSeconds > 0)
            {
                return (long)(fmt.Tbr.Value * 1000.0 / 8.0 * Duration.TotalSeconds);
            }
            return null;
        }

        public SizeEstimationResult EstimateSize(DownloadProfile profile, int? targetResolution, bool isAudioOnly)
        {
            var result = new SizeEstimationResult();
            try
            {
                if (Formats == null || Formats.Count == 0)
                {
                    result.FormattedSize = "Boyut hesaplanamadı";
                    result.Explanation = "Format verisi mevcut değil.";
                    return result;
                }

            if (isAudioOnly)
            {
                var audioFormats = Formats.Where(f => f.HasAudio && !f.HasVideo).ToList();
                if (audioFormats.Count == 0)
                    audioFormats = Formats.Where(f => f.HasAudio).ToList();

                var bestAudio = audioFormats
                    .OrderByDescending(f => f.Abr ?? f.Tbr ?? 0)
                    .ThenByDescending(f => f.EffectiveFilesize ?? 0)
                    .FirstOrDefault();

                if (bestAudio != null)
                {
                    result.AudioFormatId = bestAudio.FormatId;
                    result.AudioCodec = bestAudio.ACodec;
                    long? aSize = GetFormatSizeBytes(bestAudio);
                    result.AudioSizeBytes = aSize;
                    result.TotalSizeBytes = aSize;
                    result.IsApproximate = !bestAudio.Filesize.HasValue;

                    if (aSize.HasValue && aSize.Value > 0)
                    {
                        string baseSizeStr = DownloadProgressReport.FormatBytes(aSize.Value);
                        result.FormattedSize = result.IsApproximate ? $"~{baseSizeStr} (Tahmini)" : baseSizeStr;
                    }
                    else
                    {
                        result.FormattedSize = "Boyut hesaplanamadı";
                    }
                    result.Explanation = $"Ses formatı: {DownloadEngine.NormalizeAudioCodecName(bestAudio.ACodec)}";
                }
                else
                {
                    result.FormattedSize = "Boyut hesaplanamadı";
                }
                return result;
            }

            var videoFormats = Formats.Where(f => f.HasVideo).ToList();
            if (videoFormats.Count == 0)
            {
                result.FormattedSize = "Boyut hesaplanamadı";
                return result;
            }

            int maxAvailable = videoFormats.Where(f => f.Height.HasValue).Select(f => f.Height.Value).DefaultIfEmpty(0).Max();
            int desiredRes = targetResolution.HasValue && targetResolution.Value > 0 ? targetResolution.Value : maxAvailable;
            result.SelectedResolution = desiredRes;

            FormatMetadata chosenVideo = null;
            bool needsReencode = false;

            if (profile == DownloadProfile.WindowsCompatible)
            {
                // 1. Yerel H.264 ara (Height <= desiredRes)
                var h264Candidates = videoFormats
                    .Where(f => f.IsH264 && f.Height.HasValue && f.Height.Value <= desiredRes)
                    .OrderByDescending(f => f.Height.Value)
                    .ThenByDescending(f => f.Tbr ?? f.Vbr ?? 0)
                    .ThenByDescending(f => f.EffectiveFilesize ?? 0)
                    .ToList();

                if (h264Candidates.Count > 0)
                {
                    chosenVideo = h264Candidates.First();
                    needsReencode = false;
                }
                else
                {
                    // Yerel H.264 yoksa en iyi kaliteden FFmpeg libx264 ile dönüştürülecek
                    var anyCandidates = videoFormats
                        .Where(f => f.Height.HasValue && f.Height.Value <= desiredRes)
                        .OrderByDescending(f => f.Height.Value)
                        .ThenByDescending(f => f.Tbr ?? f.Vbr ?? 0)
                        .ToList();

                    chosenVideo = anyCandidates.FirstOrDefault() ?? videoFormats.OrderByDescending(f => f.Height ?? 0).FirstOrDefault();
                    needsReencode = true;
                }
            }
            else if (profile == DownloadProfile.BestQuality)
            {
                var candidates = videoFormats
                    .Where(f => f.Height.HasValue && f.Height.Value <= desiredRes)
                    .OrderByDescending(f => f.Height.Value)
                    .ThenByDescending(f => f.Tbr ?? f.Vbr ?? 0)
                    .ThenByDescending(f => f.EffectiveFilesize ?? 0)
                    .ToList();

                chosenVideo = candidates.FirstOrDefault() ?? videoFormats.OrderByDescending(f => f.Height ?? 0).FirstOrDefault();
                needsReencode = false;
            }
            else // ForceReencodeH264
            {
                var candidates = videoFormats
                    .Where(f => f.Height.HasValue && f.Height.Value <= desiredRes)
                    .OrderByDescending(f => f.Height.Value)
                    .ThenByDescending(f => f.Tbr ?? f.Vbr ?? 0)
                    .ToList();

                chosenVideo = candidates.FirstOrDefault() ?? videoFormats.OrderByDescending(f => f.Height ?? 0).FirstOrDefault();
                needsReencode = chosenVideo != null && (!chosenVideo.IsH264);
            }

            if (chosenVideo == null)
            {
                result.FormattedSize = "Boyut hesaplanamadı";
                return result;
            }

            result.VideoFormatId = chosenVideo.FormatId;
            result.VideoCodec = chosenVideo.VCodec;
            result.NeedsReencoding = needsReencode;

            if (chosenVideo.HasAudio)
            {
                long? size = GetFormatSizeBytes(chosenVideo);
                result.TotalSizeBytes = size;
                result.VideoSizeBytes = size;
                result.AudioSizeBytes = 0;
                result.IsApproximate = !chosenVideo.Filesize.HasValue || needsReencode;
            }
            else
            {
                FormatMetadata chosenAudio = null;
                if (profile == DownloadProfile.WindowsCompatible)
                {
                    chosenAudio = Formats.Where(f => f.HasAudio && !f.HasVideo && f.IsAac)
                        .OrderByDescending(f => f.Abr ?? f.Tbr ?? 0)
                        .FirstOrDefault();
                }

                if (chosenAudio == null)
                {
                    chosenAudio = Formats.Where(f => f.HasAudio && !f.HasVideo)
                        .OrderByDescending(f => f.Abr ?? f.Tbr ?? 0)
                        .FirstOrDefault();
                }

                if (chosenAudio == null)
                {
                    chosenAudio = Formats.Where(f => f.HasAudio)
                        .OrderByDescending(f => f.Abr ?? f.Tbr ?? 0)
                        .FirstOrDefault();
                }

                long? vSize = GetFormatSizeBytes(chosenVideo);
                long? aSize = chosenAudio != null ? GetFormatSizeBytes(chosenAudio) : (long?)0;

                result.VideoSizeBytes = vSize;
                result.AudioSizeBytes = aSize;
                if (chosenAudio != null)
                {
                    result.AudioFormatId = chosenAudio.FormatId;
                    result.AudioCodec = chosenAudio.ACodec;
                }

                if (vSize.HasValue && aSize.HasValue)
                {
                    result.TotalSizeBytes = vSize.Value + aSize.Value;
                }
                else if (vSize.HasValue)
                {
                    result.TotalSizeBytes = vSize.Value;
                }
                else
                {
                    result.TotalSizeBytes = null;
                }

                result.IsApproximate = (!chosenVideo.Filesize.HasValue || (chosenAudio != null && !chosenAudio.Filesize.HasValue) || needsReencode);
            }

            if (result.TotalSizeBytes.HasValue && result.TotalSizeBytes.Value > 0)
            {
                string formattedBytes = DownloadProgressReport.FormatBytes(result.TotalSizeBytes.Value);
                if (needsReencode)
                {
                    result.FormattedSize = $"~{formattedBytes} (Tahmini - Dönüştürme)";
                }
                else if (result.IsApproximate)
                {
                    result.FormattedSize = $"~{formattedBytes} (Tahmini)";
                }
                else
                {
                    result.FormattedSize = formattedBytes;
                }
            }
            else
            {
                result.FormattedSize = "Boyut hesaplanamadı";
            }

            string vName = DownloadEngine.NormalizeVideoCodecName(chosenVideo.VCodec);
            string aName = chosenVideo.HasAudio ? "(Gömülü ses)" : (result.AudioCodec != null ? DownloadEngine.NormalizeAudioCodecName(result.AudioCodec) : "Ses");
            result.Explanation = $"{desiredRes}p • Video: {vName} • Ses: {aName}";

            return result;
            }
            catch
            {
                result.FormattedSize = "Boyut hesaplanamadı";
                return result;
            }
        }
    }

    public class DownloadRequest
    {
        public string Url { get; set; }
        public string OutputDirectory { get; set; }
        public bool IsAudioOnly { get; set; }
        public int? TargetResolution { get; set; }
        public string AudioFormat { get; set; } = "mp3";
        public DownloadProfile Profile { get; set; } = DownloadProfile.WindowsCompatible;
        public bool AllowReencodeIfH264Missing { get; set; } = false;
        public long? EstimatedTotalBytes { get; set; }
        public long? EstimatedAudioBytes { get; set; }
    }

    public enum DownloadStage
    {
        Starting,
        DownloadingVideo,
        DownloadingAudio,
        MergingStreams,
        ExtractingAudio,
        Reencoding,
        Validating,
        Completed,
        Failed,
        Cancelled
    }

    public class DownloadProgressReport
    {
        public double Percentage { get; set; }
        public string Speed { get; set; } = string.Empty;
        public string Eta { get; set; } = string.Empty;
        public string TotalBytes { get; set; } = string.Empty;
        public string DownloadedBytes { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;

        // Structured live metrics
        public long DownloadedBytesRaw { get; set; }
        public long? TotalBytesRaw { get; set; }
        public long? EstimatedTotalBytes { get; set; }
        public double SpeedBytesPerSec { get; set; }
        public DownloadStage Stage { get; set; } = DownloadStage.Starting;

        // User-friendly formatted fields
        public string FormattedSizeProgress
        {
            get
            {
                string dl = FormatBytes(DownloadedBytesRaw);
                long? total = TotalBytesRaw.HasValue && TotalBytesRaw.Value > 0
                    ? TotalBytesRaw.Value
                    : (EstimatedTotalBytes.HasValue && EstimatedTotalBytes.Value > 0 ? EstimatedTotalBytes.Value : (long?)null);

                if (total.HasValue && total.Value > 0)
                {
                    string totalStr = FormatBytes(total.Value);
                    int pct = Math.Min(100, Math.Max(0, (int)Math.Round(Percentage)));
                    return $"İndirilen: {dl} / {totalStr} (%{pct})";
                }
                return $"İndirilen: {dl} | Toplam boyut hesaplanıyor";
            }
        }

        public string FormattedSpeed => FormatSpeed(SpeedBytesPerSec);

        public string FormattedEta
        {
            get
            {
                if (Stage == DownloadStage.Completed) return "İndirme tamamlandı";
                long? total = TotalBytesRaw.HasValue && TotalBytesRaw.Value > 0
                    ? TotalBytesRaw.Value
                    : (EstimatedTotalBytes.HasValue && EstimatedTotalBytes.Value > 0 ? EstimatedTotalBytes.Value : (long?)null);

                if (!total.HasValue || total.Value <= 0 || SpeedBytesPerSec <= 10)
                    return "Kalan süre hesaplanıyor...";

                long remaining = Math.Max(0, total.Value - DownloadedBytesRaw);
                return FormatEta(remaining, SpeedBytesPerSec);
            }
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes < 0) bytes = 0;
            var tr = new System.Globalization.CultureInfo("tr-TR");
            if (bytes < 1024)
                return $"{bytes} B";
            double kb = bytes / 1024.0;
            if (kb < 1024)
                return $"{kb.ToString("F2", tr)} KB";
            double mb = kb / 1024.0;
            if (mb < 1024)
                return $"{mb.ToString("F2", tr)} MB";
            double gb = mb / 1024.0;
            return $"{gb.ToString("F2", tr)} GB";
        }

        public static string FormatSpeed(double bytesPerSec)
        {
            if (bytesPerSec <= 0 || double.IsNaN(bytesPerSec) || double.IsInfinity(bytesPerSec))
                return "Hız hesaplanıyor...";

            var tr = new System.Globalization.CultureInfo("tr-TR");
            if (bytesPerSec < 1024)
                return $"{bytesPerSec.ToString("F2", tr)} B/sn";
            double kb = bytesPerSec / 1024.0;
            if (kb < 1024)
                return $"{kb.ToString("F2", tr)} KB/sn";
            double mb = kb / 1024.0;
            if (mb < 1024)
                return $"{mb.ToString("F2", tr)} MB/sn";
            double gb = mb / 1024.0;
            return $"{gb.ToString("F2", tr)} GB/sn";
        }

        public static string FormatEta(long remainingBytes, double bytesPerSec)
        {
            if (remainingBytes <= 0) return "00:00";
            if (bytesPerSec <= 10 || double.IsNaN(bytesPerSec) || double.IsInfinity(bytesPerSec))
                return "Kalan süre hesaplanıyor...";

            double seconds = remainingBytes / bytesPerSec;
            if (seconds <= 0 || double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds > 86400 * 7)
                return "Kalan süre hesaplanıyor...";

            var ts = TimeSpan.FromSeconds(seconds);
            if (ts.TotalHours >= 1)
                return ts.ToString(@"hh\:mm\:ss");
            return ts.ToString(@"mm\:ss");
        }
    }

    public class MediaValidationResult
    {
        public bool IsValid { get; set; }
        public bool HasVideo { get; set; }
        public bool HasAudio { get; set; }
        public string VideoCodec { get; set; } = string.Empty;
        public string AudioCodec { get; set; } = string.Empty;
        public string Container { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public TimeSpan Duration { get; set; }
        public long FileSizeBytes { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public bool IsH264 => !string.IsNullOrEmpty(VideoCodec) &&
            (VideoCodec.IndexOf("h264", StringComparison.OrdinalIgnoreCase) >= 0 ||
             VideoCodec.IndexOf("avc", StringComparison.OrdinalIgnoreCase) >= 0);

        public bool IsAac => !string.IsNullOrEmpty(AudioCodec) &&
            (AudioCodec.IndexOf("aac", StringComparison.OrdinalIgnoreCase) >= 0 ||
             AudioCodec.IndexOf("mp4a", StringComparison.OrdinalIgnoreCase) >= 0);

        public bool IsWindowsPlayerFriendly => IsH264 && IsAac;
    }

    public class DownloadResult
    {
        public bool Success { get; set; }
        public string FilePath { get; set; }
        public MediaValidationResult Validation { get; set; }
        public bool WasReencoded { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
    }

    public class BotVerificationException : Exception
    {
        public BotVerificationException(string message) : base(message) { }
    }

    public class H264NotAvailableException : Exception
    {
        public List<int> AvailableResolutions { get; }
        public H264NotAvailableException(string message, List<int> availableResolutions = null) : base(message)
        {
            AvailableResolutions = availableResolutions ?? new List<int>();
        }
    }

    public class MediaValidationException : Exception
    {
        public MediaValidationException(string message) : base(message) { }
    }
}
