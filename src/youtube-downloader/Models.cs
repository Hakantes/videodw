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

    public class DownloadProgressReport
    {
        public double Percentage { get; set; }
        public string Speed { get; set; } = string.Empty;
        public string Eta { get; set; } = string.Empty;
        public string TotalBytes { get; set; } = string.Empty;
        public string DownloadedBytes { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
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
