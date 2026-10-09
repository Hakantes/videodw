using System;
using System.Diagnostics;
using System.IO;

namespace youtube_dowload
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static readonly string _logDir;
        private static readonly string _logPath;

        static Logger()
        {
            try
            {
                _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(_logDir))
                {
                    Directory.CreateDirectory(_logDir);
                }
                _logPath = Path.Combine(_logDir, "downloader.log");
            }
            catch
            {
                // Fallback to local
                _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "downloader.log");
            }
        }

        public static string LogFilePath => _logPath;

        public static void Log(string message)
        {
            try
            {
                lock (_lock)
                {
                    string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                    File.AppendAllText(_logPath, entry);
                }
            }
            catch
            {
                // Silently ignore logging failures to avoid app crash
            }
        }

        public static void LogError(string message, Exception ex)
        {
            Log($"[HATA] {message}: {ex?.Message}{Environment.NewLine}{ex?.StackTrace}");
        }

        public static void OpenLog()
        {
            try
            {
                if (File.Exists(_logPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _logPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                Log($"Log dosyası açılamadı: {ex.Message}");
            }
        }
    }
}
