using System;
using System.IO;
using System.Text.Json;

namespace youtube_dowload
{
    public class AppSettings
    {
        public string DownloadFolder { get; set; } = string.Empty;
        public string LastQuality { get; set; } = "auto";
        public bool IsAudioOnly { get; set; } = false;
    }

    public static class SettingsService
    {
        private static readonly string SettingsFilePath;

        static SettingsService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = Path.Combine(appData, "YouTubeVideoDownloader");
            if (!Directory.Exists(folder))
            {
                try { Directory.CreateDirectory(folder); } catch { }
            }
            SettingsFilePath = Path.Combine(folder, "settings.json");
        }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        if (string.IsNullOrWhiteSpace(settings.DownloadFolder) || !Directory.Exists(settings.DownloadFolder))
                        {
                            settings.DownloadFolder = GetDefaultDownloadFolder();
                        }
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Ayar yükleme hatası: " + ex.Message);
            }

            return new AppSettings
            {
                DownloadFolder = GetDefaultDownloadFolder(),
                LastQuality = "auto",
                IsAudioOnly = false
            };
        }

        public static void Save(AppSettings settings)
        {
            try
            {
                if (settings == null) return;
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Logger.Log("Ayar kaydetme hatası: " + ex.Message);
            }
        }

        public static string GetDefaultDownloadFolder()
        {
            try
            {
                // Try Downloads folder
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string downloads = Path.Combine(userProfile, "Downloads");
                if (Directory.Exists(downloads)) return downloads;

                // Try MyVideos
                string videos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                if (Directory.Exists(videos)) return videos;

                // Try Desktop
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            catch
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }
    }
}
