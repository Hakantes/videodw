using System;
using System.Collections.Generic;

namespace youtube_dowload
{
    public enum DownloadProfile
    {
        WindowsCompatible,   // Profil B: Windows Uyumlu MP4 (H.264/AAC öncelikli)
        BestQuality,         // Profil A: En İyi Kalite (Orijinal codec - AV1/VP9/AVC)
        ForceReencodeH264    // İsteğe bağlı: H.264'e Dönüştür (FFmpeg libx264 ile yeniden kodla)
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
        public bool HasH264Stream => AvailableH264Resolutions != null && AvailableH264Resolutions.Count > 0;
        public bool HasAacAudioStream { get; set; }

        public string FormattedDuration
        {
            get
            {
                if (Duration.TotalHours >= 1)
                    return Duration.ToString(@"hh\:mm\:ss");
                return Duration.ToString(@"mm\:ss");
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
        public double SpeedBytesPerSec { get; set; }
        public DownloadStage Stage { get; set; } = DownloadStage.Starting;

        // User-friendly formatted fields
        public string FormattedSizeProgress
        {
            get
            {
                string dl = FormatBytes(DownloadedBytesRaw);
                if (TotalBytesRaw.HasValue && TotalBytesRaw.Value > 0)
                {
                    string total = FormatBytes(TotalBytesRaw.Value);
                    int pct = Math.Min(100, Math.Max(0, (int)Math.Round(Percentage)));
                    return $"İndirilen: {dl} / {total} (%{pct})";
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
                if (!TotalBytesRaw.HasValue || TotalBytesRaw.Value <= 0 || SpeedBytesPerSec <= 10)
                    return "Kalan süre hesaplanıyor...";

                long remaining = TotalBytesRaw.Value - DownloadedBytesRaw;
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
