using System;
using System.Collections.Generic;

namespace youtube_dowload
{
    public class VideoMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string Uploader { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string WebpageUrl { get; set; } = string.Empty;
        public List<int> AvailableResolutions { get; set; } = new List<int>();

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
    
    public class BotVerificationException : Exception 
    { 
        public BotVerificationException(string message) : base(message) { } 
    }
}
