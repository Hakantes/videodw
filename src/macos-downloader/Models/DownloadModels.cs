using System;
using System.Collections.Generic;

namespace YouTubeDownloader.Mac.Models
{
    public enum DownloadProfile
    {
        BestQuality,
        WindowsMacCompatible,
        AudioOnlyMp3
    }

    public enum DownloadStage
    {
        Preparing,
        DownloadingVideo,
        DownloadingAudio,
        Merging,
        Converting,
        Completed,
        Failed,
        Cancelled
    }

    public class FormatOption
    {
        public string Label { get; set; } = string.Empty;
        public int? Resolution { get; set; }
        public string FormatId { get; set; } = string.Empty;
        public string VideoCodec { get; set; } = string.Empty;
        public string AudioCodec { get; set; } = string.Empty;
        public string Extension { get; set; } = "mp4";
        public long? ApproxSizeBytes { get; set; }
        public string FormattedSize { get; set; } = "Boyut hesaplanamadı";
        public bool IsAudioOnly { get; set; }

        public override string ToString() => Label;
    }

    public class VideoMetadata
    {
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string FormattedDuration => Duration.TotalHours >= 1
            ? Duration.ToString(@"hh\:mm\:ss")
            : Duration.ToString(@"mm\:ss");
        public string ThumbnailUrl { get; set; } = string.Empty;
        public List<FormatOption> AvailableFormats { get; set; } = new List<FormatOption>();
    }

    public class DownloadProgressReport
    {
        public DownloadStage Stage { get; set; } = DownloadStage.DownloadingVideo;
        public double Percentage { get; set; }
        public long? DownloadedBytes { get; set; }
        public long? TotalBytes { get; set; }
        public double? SpeedBytesPerSec { get; set; }
        public TimeSpan? Eta { get; set; }
        public string StatusMessage { get; set; } = string.Empty;

        public string FormattedSpeed
        {
            get
            {
                if (!SpeedBytesPerSec.HasValue || SpeedBytesPerSec.Value <= 0) return "Hız: -";
                double spd = SpeedBytesPerSec.Value;
                if (spd >= 1024 * 1024)
                    return $"Hız: {(spd / (1024 * 1024)):F1} MB/s";
                if (spd >= 1024)
                    return $"Hız: {(spd / 1024):F0} KB/s";
                return $"Hız: {spd:F0} B/s";
            }
        }

        public string FormattedEta
        {
            get
            {
                if (!Eta.HasValue || Eta.Value.TotalSeconds <= 0) return "Kalan: -";
                var t = Eta.Value;
                if (t.TotalHours >= 1)
                    return $"Kalan: {t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
                return $"Kalan: {t.Minutes:D2}:{t.Seconds:D2}";
            }
        }

        public string FormattedSizeProgress
        {
            get
            {
                if (DownloadedBytes.HasValue && TotalBytes.HasValue && TotalBytes.Value > 0)
                {
                    return $"{FormatBytes(DownloadedBytes.Value)} / {FormatBytes(TotalBytes.Value)}";
                }
                if (DownloadedBytes.HasValue)
                {
                    return FormatBytes(DownloadedBytes.Value);
                }
                return "-";
            }
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }
    }

    public class DownloadResult
    {
        public bool Success { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public bool WasCancelled { get; set; }
    }

    public class MacAppSettings
    {
        public string DownloadDirectory { get; set; } = string.Empty;
        public string LastQuality { get; set; } = "1080p";
        public bool IsAudioOnly { get; set; }
        public bool CheckForUpdatesOnStartup { get; set; } = true;
        public DateTime? LastUpdateCheckUtc { get; set; }
        public string UpdateChannel { get; set; } = "stable";

        public bool ShouldCheckForUpdates()
        {
            if (!CheckForUpdatesOnStartup) return false;
            if (!LastUpdateCheckUtc.HasValue) return true;
            return (DateTime.UtcNow - LastUpdateCheckUtc.Value).TotalHours >= 24.0;
        }
    }
}
