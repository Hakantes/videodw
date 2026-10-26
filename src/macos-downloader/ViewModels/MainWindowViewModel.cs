using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YouTubeDownloader.Mac.Models;
using YouTubeDownloader.Mac.Services;

namespace YouTubeDownloader.Mac.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly MacDownloadEngine _engine = new MacDownloadEngine();
        private readonly MacUpdateManager _updateManager = new MacUpdateManager();
        private readonly MacAppSettings _settings;
        private CancellationTokenSource? _downloadCts;
        private MacUpdateCheckResult? _pendingUpdate;

        [ObservableProperty]
        private string _url = string.Empty;

        [ObservableProperty]
        private bool _isAnalyzing;

        [ObservableProperty]
        private bool _isDownloading;

        [ObservableProperty]
        private bool _canCancel;

        [ObservableProperty]
        private string _statusMessage = "Hazır. Video bağlantısını yapıştırıp 'Analiz Et'e tıklayın.";

        [ObservableProperty]
        private string _videoTitle = string.Empty;

        [ObservableProperty]
        private string _videoChannel = string.Empty;

        [ObservableProperty]
        private string _videoDuration = string.Empty;

        [ObservableProperty]
        private bool _hasMetadata;

        [ObservableProperty]
        private ObservableCollection<FormatOption> _availableFormats = new ObservableCollection<FormatOption>();

        [ObservableProperty]
        private FormatOption? _selectedFormat;

        [ObservableProperty]
        private string _downloadDirectory = string.Empty;

        [ObservableProperty]
        private double _progressPercentage = 0;

        [ObservableProperty]
        private string _progressPercentText = "%0";

        [ObservableProperty]
        private string _progressDetails = "İndirme bekleniyor.";

        [ObservableProperty]
        private bool _hasUpdate;

        [ObservableProperty]
        private string _updateText = string.Empty;

        [ObservableProperty]
        private string _lastDownloadedFile = string.Empty;

        [ObservableProperty]
        private bool _canOpenFile;

        [ObservableProperty]
        private bool _dependenciesOk = true;

        [ObservableProperty]
        private string _dependenciesMessage = string.Empty;

        public MainWindowViewModel()
        {
            _settings = MacSettingsService.Load();
            DownloadDirectory = _settings.DownloadDirectory;

            // Check dependencies
            if (!_engine.AreDependenciesReady(out string depMsg))
            {
                DependenciesOk = false;
                DependenciesMessage = depMsg;
                StatusMessage = "Bağımlılık uyarısı: " + depMsg;
            }

            // Silent update check in background
            if (_settings.ShouldCheckForUpdates())
            {
                Task.Delay(3000).ContinueWith(_ => CheckForUpdatesInternalAsync(manual: false));
            }
        }

        [RelayCommand]
        public async Task AnalyzeAsync()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                StatusMessage = "Lütfen geçerli bir video URL'si girin.";
                return;
            }

            IsAnalyzing = true;
            StatusMessage = "Video analiz ediliyor, kalite ve format bilgileri alınıyor...";
            HasMetadata = false;
            AvailableFormats.Clear();

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var meta = await _engine.GetVideoInfoAsync(Url, cts.Token);

                VideoTitle = meta.Title;
                VideoChannel = meta.Channel;
                VideoDuration = meta.FormattedDuration;
                HasMetadata = true;

                foreach (var f in meta.AvailableFormats)
                {
                    AvailableFormats.Add(f);
                }

                if (AvailableFormats.Count > 0)
                {
                    SelectedFormat = AvailableFormats[0];
                }

                StatusMessage = "Analiz tamamlandı. İndirmek istediğiniz kaliteyi seçin.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Analiz hatası: " + ex.Message;
            }
            finally
            {
                IsAnalyzing = false;
            }
        }

        [RelayCommand]
        public async Task DownloadAsync()
        {
            if (SelectedFormat == null)
            {
                StatusMessage = "Lütfen bir indirme formatı seçin.";
                return;
            }

            IsDownloading = true;
            CanCancel = true;
            CanOpenFile = false;
            ProgressPercentage = 0;
            ProgressPercentText = "%0";
            ProgressDetails = "İndirme başlatılıyor...";
            StatusMessage = "İndirme devam ediyor...";

            _downloadCts = new CancellationTokenSource();

            var progress = new Progress<DownloadProgressReport>(report =>
            {
                ProgressPercentage = report.Percentage;
                ProgressPercentText = $"%{report.Percentage:F0}";
                ProgressDetails = $"{report.FormattedSizeProgress}   •   {report.FormattedSpeed}   •   {report.FormattedEta}";
                if (!string.IsNullOrEmpty(report.StatusMessage))
                {
                    StatusMessage = report.StatusMessage;
                }
            });

            try
            {
                var result = await _engine.DownloadAsync(
                    Url,
                    SelectedFormat,
                    DownloadDirectory,
                    progress,
                    _downloadCts.Token);

                if (result.Success)
                {
                    ProgressPercentage = 100;
                    ProgressPercentText = "%100";
                    StatusMessage = "Tebrikler! İndirme başarıyla tamamlandı.";
                    LastDownloadedFile = result.FilePath;
                    CanOpenFile = File.Exists(result.FilePath);
                    ProgressDetails = "Tamamlandı: " + Path.GetFileName(result.FilePath);
                }
                else if (result.WasCancelled)
                {
                    StatusMessage = "İndirme iptal edildi.";
                    ProgressDetails = "İptal edildi.";
                }
                else
                {
                    StatusMessage = "Hata: " + result.ErrorMessage;
                    ProgressDetails = "İndirme başarısız oldu.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Beklenmeyen hata: " + ex.Message;
                ProgressDetails = "Hata.";
            }
            finally
            {
                IsDownloading = false;
                CanCancel = false;
                _downloadCts = null;
            }
        }

        [RelayCommand]
        public void CancelDownload()
        {
            if (_downloadCts != null && !_downloadCts.IsCancellationRequested)
            {
                StatusMessage = "İptal ediliyor...";
                _downloadCts.Cancel();
            }
        }

        [RelayCommand]
        public void OpenFile()
        {
            if (!string.IsNullOrEmpty(LastDownloadedFile) && File.Exists(LastDownloadedFile))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = LastDownloadedFile,
                        UseShellExecute = true
                    });
                }
                catch { }
            }
        }

        [RelayCommand]
        public void OpenFolder()
        {
            try
            {
                string dir = Directory.Exists(DownloadDirectory)
                    ? DownloadDirectory
                    : MacSettingsService.GetDefaultDownloadDirectory();

                if (!string.IsNullOrEmpty(LastDownloadedFile) && File.Exists(LastDownloadedFile))
                {
                    if (OperatingSystem.IsMacOS())
                    {
                        Process.Start("open", $"-R \"{LastDownloadedFile}\"");
                        return;
                    }
                    else if (OperatingSystem.IsWindows())
                    {
                        Process.Start("explorer.exe", $"/select,\"{LastDownloadedFile}\"");
                        return;
                    }
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true
                });
            }
            catch { }
        }

        [RelayCommand]
        public async Task CheckUpdatesManualAsync()
        {
            StatusMessage = "Güncellemeler denetleniyor...";
            await CheckForUpdatesInternalAsync(manual: true);
        }

        private async Task CheckForUpdatesInternalAsync(bool manual)
        {
            try
            {
                var result = await _updateManager.CheckForUpdatesAsync(isManual: manual);

                _settings.LastUpdateCheckUtc = DateTime.UtcNow;
                MacSettingsService.Save(_settings);

                if (result.IsUpdateAvailable)
                {
                    _pendingUpdate = result;
                    HasUpdate = true;
                    UpdateText = $"✨ Yeni Sürüm v{result.NewVersion} Mevcut! Güncelle";
                    StatusMessage = $"Yeni sürüm mevcut (v{result.NewVersion}). Güncellemek için butona tıklayın.";
                }
                else if (manual)
                {
                    StatusMessage = result.IsError
                        ? result.StatusMessage
                        : $"Uygulamanız güncel (v{_updateManager.CurrentVersion}).";
                }
            }
            catch (Exception ex)
            {
                if (manual) StatusMessage = "Güncelleme denetimi hatası: " + ex.Message;
            }
        }

        [RelayCommand]
        public async Task ApplyUpdateAsync()
        {
            if (_pendingUpdate == null) return;

            StatusMessage = $"Güncelleme paketi indiriliyor: v{_pendingUpdate.NewVersion}...";
            IsDownloading = true;

            var progress = new Progress<double>(pct =>
            {
                ProgressPercentage = pct;
                ProgressPercentText = $"%{pct:F0}";
                ProgressDetails = $"Güncelleme indiriliyor... %{pct:F0}";
            });

            try
            {
                string downloadedFile = await _updateManager.DownloadUpdateAsync(_pendingUpdate, progress, CancellationToken.None);
                StatusMessage = "Güncelleme doğrulandı, kuruluyor...";
                MacUpdateManager.ApplyUpdateMac(downloadedFile);
            }
            catch (Exception ex)
            {
                StatusMessage = "Güncelleme uygulanamadı: " + ex.Message;
                IsDownloading = false;
            }
        }
    }
}
