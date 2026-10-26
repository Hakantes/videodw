using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using YouTubeDownloader.Mac.Models;

namespace YouTubeDownloader.Mac.Services
{
    public static class MacSettingsService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public static string GetConfigDirectory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(home, "Library", "Application Support", "YouTubeVideoDownloader");
            }
            else
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appData, "YouTubeVideoDownloader");
            }
        }

        public static string GetSettingsFilePath()
        {
            return Path.Combine(GetConfigDirectory(), "settings.json");
        }

        public static string GetDefaultDownloadDirectory()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string downloads = Path.Combine(home, "Downloads");
            if (Directory.Exists(downloads)) return downloads;
            return Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        }

        public static MacAppSettings Load()
        {
            try
            {
                string filePath = GetSettingsFilePath();
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var settings = JsonSerializer.Deserialize<MacAppSettings>(json);
                    if (settings != null)
                    {
                        if (string.IsNullOrWhiteSpace(settings.DownloadDirectory) || !Directory.Exists(settings.DownloadDirectory))
                        {
                            settings.DownloadDirectory = GetDefaultDownloadDirectory();
                        }
                        return settings;
                    }
                }
            }
            catch (Exception)
            {
                // Fallback to default
            }

            return new MacAppSettings
            {
                DownloadDirectory = GetDefaultDownloadDirectory(),
                LastQuality = "1080p",
                IsAudioOnly = false,
                CheckForUpdatesOnStartup = true
            };
        }

        public static void Save(MacAppSettings settings)
        {
            try
            {
                string dir = GetConfigDirectory();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string filePath = GetSettingsFilePath();
                string json = JsonSerializer.Serialize(settings, _jsonOptions);
                File.WriteAllText(filePath, json);
            }
            catch (Exception)
            {
                // Non-critical, ignore save errors
            }
        }
    }
}
