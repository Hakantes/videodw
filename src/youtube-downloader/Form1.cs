using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace youtube_dowload
{
    public partial class Form1 : Form
    {
        private readonly DownloadEngine _engine = new DownloadEngine();
        private CancellationTokenSource _downloadCts;
        private CancellationTokenSource _analyzeCts;
        private VideoMetadata _currentMetadata;
        private string _lastDownloadedFile;
        private AppSettings _settings;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
            }
            catch { }

            _settings = SettingsService.Load();
            txtFolderPath.Text = _settings.DownloadFolder;
            rbAudio.Checked = _settings.IsAudioOnly;
            rbVideo.Checked = !_settings.IsAudioOnly;

            // Profil Seçimi
            cmbProfile.Items.Clear();
            cmbProfile.Items.Add("Windows Uyumlu MP4 (H.264)");
            cmbProfile.Items.Add("En İyi Kalite (Orijinal Codec)");
            cmbProfile.Items.Add("H.264'e Dönüştür (Yeniden Kodla)");

            if (_settings.LastProfile == "BestQuality")
                cmbProfile.SelectedIndex = 1;
            else if (_settings.LastProfile == "ForceReencodeH264")
                cmbProfile.SelectedIndex = 2;
            else
                cmbProfile.SelectedIndex = 0;

            // Çerez Seçimi
            cmbCookies.Items.Clear();
            cmbCookies.Items.AddRange(new object[] { "Yok", "Chrome", "Edge", "Firefox", "Opera", "Brave", "Safari", "Vivaldi" });
            cmbCookies.SelectedItem = string.IsNullOrWhiteSpace(_settings.BrowserForCookies) ? "Yok" : _settings.BrowserForCookies;
            _engine.BrowserForCookies = _settings.BrowserForCookies;

            PopulateDefaultQualityOptions();

            // Bağımlılık kontrolü
            if (!_engine.AreDependenciesReady(out string missingMessage))
            {
                lblStatus.Text = "Uyarı: " + missingMessage;
                lblStatus.ForeColor = Color.DarkRed;
                Logger.Log("Bağımlılık uyarısı: " + missingMessage);
            }
            else
            {
                lblStatus.Text = "Hazır. Video bağlantısını yapıştırıp 'Analiz Et'e tıklayın.";
            }
        }

        private void PopulateDefaultQualityOptions()
        {
            cmbQuality.Items.Clear();
            cmbQuality.Items.Add(new QualityItem("En Yüksek Kalite (Otomatik)", null));
            cmbQuality.Items.Add(new QualityItem("4K Ultra HD (2160p)", 2160));
            cmbQuality.Items.Add(new QualityItem("2K Quad HD (1440p)", 1440));
            cmbQuality.Items.Add(new QualityItem("Full HD (1080p)", 1080));
            cmbQuality.Items.Add(new QualityItem("HD (720p)", 720));
            cmbQuality.Items.Add(new QualityItem("SD (480p)", 480));
            cmbQuality.Items.Add(new QualityItem("Düşük (360p)", 360));
            cmbQuality.SelectedIndex = 0;
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                txtUrl.Text = Clipboard.GetText().Trim();
                btnAnalyze_Click(sender, e);
            }
            else
            {
                MessageBox.Show("Panoda geçerli bir metin bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void txtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnAnalyze_Click(sender, e);
            }
        }

        private async void btnAnalyze_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Lütfen geçerli bir video URL'si girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetUiState(isAnalyzing: true);
            lblStatus.Text = "URL analiz ediliyor...";
            lblStatus.ForeColor = Color.DarkBlue;
            lblProgressDetails.Text = "Video bilgileri ve tahmini boyut alınıyor...";

            _analyzeCts = new CancellationTokenSource();

            try
            {
                _currentMetadata = await _engine.GetVideoInfoAsync(url, _analyzeCts.Token);

                lblVideoTitle.Text = _currentMetadata.Title;
                string vCodecs = _currentMetadata.VideoCodecs.Count > 0 ? string.Join(", ", _currentMetadata.VideoCodecs) : "Belirsiz";
                string aCodecs = _currentMetadata.AudioCodecs.Count > 0 ? string.Join(", ", _currentMetadata.AudioCodecs) : "Belirsiz";
                lblVideoInfo.Text = $"Kanal: {_currentMetadata.Uploader}  |  Süre: {_currentMetadata.FormattedDuration}\r\nVideo Codec: {vCodecs}  |  Ses: {aCodecs}";

                // Küçük resmi yükle
                if (!string.IsNullOrEmpty(_currentMetadata.ThumbnailUrl))
                {
                    LoadThumbnailAsync(_currentMetadata.ThumbnailUrl);
                }

                // Çözünürlükleri profil durumuna göre doldur
                PopulateAvailableResolutions(_currentMetadata.AvailableResolutions);

                // Tahmini boyutu anında göster
                UpdateEstimatedSizeDisplay();

                lblStatus.Text = "Analiz tamamlandı. İndirmeye hazır.";
                lblStatus.ForeColor = Color.DarkGreen;
                btnDownload.Enabled = true;
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Analiz iptal edildi.";
                lblStatus.ForeColor = Color.Black;
            }
            catch (BotVerificationException ex)
            {
                lblStatus.Text = "YouTube Bot Koruması: Oturum açmanız gerekiyor.";
                lblStatus.ForeColor = Color.DarkOrange;
                Logger.Log("Bot doğrulaması hatası: " + ex.Message);
                MessageBox.Show(ex.Message, "Doğrulama Gerekli", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Analiz başarısız oldu!";
                lblStatus.ForeColor = Color.DarkRed;
                Logger.LogError("Video analizi sırasında hata", ex);
                MessageBox.Show($"Video bilgileri alınırken hata oluştu:\n\n{ex.Message}", "Analiz Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetUiState(isAnalyzing: false);
            }
        }

        private void PopulateAvailableResolutions(List<int> availableHeights)
        {
            cmbQuality.Items.Clear();
            cmbQuality.Items.Add(new QualityItem("En Yüksek Kalite (Otomatik)", null));

            bool isWinProfile = cmbProfile.SelectedIndex == 0;

            if (availableHeights != null && availableHeights.Count > 0)
            {
                foreach (int height in availableHeights)
                {
                    string label = GetResolutionLabel(height);
                    if (isWinProfile && _currentMetadata != null)
                    {
                        if (_currentMetadata.AvailableH264Resolutions.Contains(height))
                        {
                            label += " [H.264 Uyumlu]";
                        }
                        else
                        {
                            label += " [Yalnızca AV1/VP9]";
                        }
                    }
                    cmbQuality.Items.Add(new QualityItem(label, height));
                }
            }
            else
            {
                cmbQuality.Items.Add(new QualityItem("Standart Video (720p)", 720));
                cmbQuality.Items.Add(new QualityItem("Standart Video (360p)", 360));
            }

            cmbQuality.SelectedIndex = 0;
        }

        private string GetResolutionLabel(int height)
        {
            switch (height)
            {
                case 4320: return "8K Ultra HD (4320p)";
                case 2160: return "4K Ultra HD (2160p)";
                case 1440: return "2K Quad HD (1440p)";
                case 1080: return "Full HD (1080p)";
                case 720: return "HD (720p)";
                case 480: return "SD (480p)";
                case 360: return "360p";
                case 240: return "240p";
                case 144: return "144p";
                default: return $"{height}p";
            }
        }

        private async void LoadThumbnailAsync(string thumbnailUrl)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    byte[] data = await client.GetByteArrayAsync(thumbnailUrl);
                    using (var ms = new MemoryStream(data))
                    {
                        var oldImg = picThumbnail.Image;
                        picThumbnail.Image = Image.FromStream(ms);
                        oldImg?.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Küçük resim yüklenemedi: " + ex.Message);
            }
        }

        private void rbFormat_CheckedChanged(object sender, EventArgs e)
        {
            if (rbAudio.Checked)
            {
                cmbQuality.Enabled = false;
                cmbProfile.Enabled = false;
                lblQualityDesc.Text = "Ses Formatı: MP3 (320 kbps)";
            }
            else
            {
                cmbQuality.Enabled = true;
                cmbProfile.Enabled = true;
                lblQualityDesc.Text = "Video Çözünürlüğü:";
            }
            UpdateEstimatedSizeDisplay();
        }

        private void cmbProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProfile.SelectedIndex == 1)
                _settings.LastProfile = "BestQuality";
            else if (cmbProfile.SelectedIndex == 2)
                _settings.LastProfile = "ForceReencodeH264";
            else
                _settings.LastProfile = "WindowsCompatible";

            SettingsService.Save(_settings);

            if (_currentMetadata != null)
            {
                PopulateAvailableResolutions(_currentMetadata.AvailableResolutions);
            }
            UpdateEstimatedSizeDisplay();
        }

        private void cmbQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateEstimatedSizeDisplay();
        }

        private void UpdateEstimatedSizeDisplay()
        {
            try
            {
                if (_currentMetadata == null)
                {
                    lblEstimatedSize.Text = "Tahmini Boyut: -";
                    return;
                }

                var selectedQuality = cmbQuality.SelectedItem as QualityItem;
                DownloadProfile profile = DownloadProfile.WindowsCompatible;
                if (cmbProfile.SelectedIndex == 1) profile = DownloadProfile.BestQuality;
                else if (cmbProfile.SelectedIndex == 2) profile = DownloadProfile.ForceReencodeH264;

                bool isAudioOnly = rbAudio.Checked;
                var est = _currentMetadata.EstimateSize(profile, selectedQuality?.Resolution, isAudioOnly);

                string formatName = isAudioOnly
                    ? "MP3 (320 kbps)"
                    : (est.VideoCodec != null ? DownloadEngine.NormalizeVideoCodecName(est.VideoCodec) : "MP4 (H.264)");

                string qualityLabel = isAudioOnly ? "320 kbps" : (selectedQuality?.Label ?? "En Yüksek Kalite");

                lblEstimatedSize.Text = $"Tahmini Boyut: {est.FormattedSize}  •  Format: {formatName}  •  Kalite: {qualityLabel}";
                lblEstimatedSize.ForeColor = est.TotalSizeBytes.HasValue ? Color.FromArgb(16, 124, 65) : Color.DarkOrange;

                if (btnDownload.Enabled && progressBar1.Value == 0)
                {
                    lblProgressDetails.Text = $"İndirmeye hazır  •  Tahmini Boyut: {est.FormattedSize}  •  Format: {formatName}  •  Kalite: {qualityLabel}";
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Boyut güncelleme hatası: " + ex.Message);
                lblEstimatedSize.Text = "Tahmini Boyut: Boyut hesaplanamadı";
                lblEstimatedSize.ForeColor = Color.DarkOrange;
            }
        }

        private void cmbCookies_SelectedIndexChanged(object sender, EventArgs e)
        {
            _settings.BrowserForCookies = cmbCookies.SelectedItem?.ToString() ?? "Yok";
            _engine.BrowserForCookies = _settings.BrowserForCookies;
            SettingsService.Save(_settings);
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "İndirilen videoların kaydedileceği klasörü seçin:";
                fbd.SelectedPath = Directory.Exists(txtFolderPath.Text) ? txtFolderPath.Text : SettingsService.GetDefaultDownloadFolder();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtFolderPath.Text = fbd.SelectedPath;
                    _settings.DownloadFolder = fbd.SelectedPath;
                    SettingsService.Save(_settings);
                }
            }
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Lütfen bir video bağlantısı girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string outDir = txtFolderPath.Text.Trim();
            if (string.IsNullOrEmpty(outDir) || !Directory.Exists(outDir))
            {
                MessageBox.Show("Lütfen geçerli bir indirme klasörü seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedQuality = cmbQuality.SelectedItem as QualityItem;
            DownloadProfile profile = DownloadProfile.WindowsCompatible;
            if (cmbProfile.SelectedIndex == 1) profile = DownloadProfile.BestQuality;
            else if (cmbProfile.SelectedIndex == 2) profile = DownloadProfile.ForceReencodeH264;

            long? estimatedSize = null;
            long? estimatedAudioSize = null;
            if (_currentMetadata != null)
            {
                var est = _currentMetadata.EstimateSize(profile, selectedQuality?.Resolution, rbAudio.Checked);
                estimatedSize = est.TotalSizeBytes;
                estimatedAudioSize = est.AudioSizeBytes;
            }

            var request = new DownloadRequest
            {
                Url = url,
                OutputDirectory = outDir,
                IsAudioOnly = rbAudio.Checked,
                TargetResolution = selectedQuality?.Resolution,
                AudioFormat = "mp3",
                Profile = profile,
                AllowReencodeIfH264Missing = false,
                EstimatedTotalBytes = estimatedSize,
                EstimatedAudioBytes = estimatedAudioSize
            };

            // Profil B için ön denetim: Videoda hiç H.264 akışı yoksa kullanıcıya sor
            if (profile == DownloadProfile.WindowsCompatible && !request.IsAudioOnly && _currentMetadata != null && !_currentMetadata.HasH264Stream)
            {
                var ask = MessageBox.Show(
                    "Bu videoda kaynak tarafından sunulan yerel H.264 (AVC) akışı bulunmuyor (yalnızca AV1 veya VP9 mevcut).\n\n" +
                    "Windows Media Player ile tam uyumluluk için video FFmpeg ile H.264 formatına dönüştürülsün mü?\n\n" +
                    "• [Evet]: İndir ve H.264'e dönüştür (Windows Media Player uyumlu)\n" +
                    "• [Hayır]: Orijinal En İyi Kalite (AV1/VP9) olarak indir\n" +
                    "• [İptal]: İndirmeyi iptal et",
                    "H.264 Akışı Bulunamadı",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (ask == DialogResult.Cancel)
                {
                    return;
                }
                else if (ask == DialogResult.Yes)
                {
                    request.AllowReencodeIfH264Missing = true;
                }
                else if (ask == DialogResult.No)
                {
                    request.Profile = DownloadProfile.BestQuality;
                }
            }

            SetUiState(isDownloading: true);
            string startStageText = request.IsAudioOnly ? "Ses indiriliyor..." : "Video indiriliyor...";
            lblStatus.Text = startStageText;
            lblStatus.ForeColor = Color.DarkBlue;
            progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.Value = 0;
            lblProgressPercent.Text = "%0";

            string estStr = estimatedSize.HasValue ? DownloadProgressReport.FormatBytes(estimatedSize.Value) : "Hesaplanıyor...";
            lblProgressDetails.Text = $"İndirilen: 0 B / {estStr} (%0)   •   Hız: Başlatılıyor...   •   Kalan süre: Hesaplanıyor...";

            _downloadCts = new CancellationTokenSource();

            var progress = new Progress<DownloadProgressReport>(report => UpdateProgressUi(report));

            try
            {
                var downloadResult = await _engine.DownloadAsync(request, progress, _downloadCts.Token);
                HandleDownloadSuccess(downloadResult, outDir);
            }
            catch (H264NotAvailableException ex)
            {
                lblStatus.Text = "H.264 Akışı Yok: Dönüştürme veya En İyi Kalite seçin.";
                lblStatus.ForeColor = Color.DarkOrange;
                lblProgressDetails.Text = "Yerel H.264 akışı bulunamadı.";
                Logger.Log("H.264 bulunamadı: " + ex.Message);

                var choice = MessageBox.Show(
                    ex.Message + "\n\n" +
                    "Windows Media Player ile tam uyumluluk için videoyu FFmpeg ile H.264 formatına dönüştürerek indirmek ister misiniz?\n\n" +
                    "• [Evet]: İndir ve H.264'e dönüştür (Windows Media Player uyumlu)\n" +
                    "• [Hayır]: Orijinal En İyi Kalite (AV1/VP9) olarak indir\n" +
                    "• [İptal]: İndirmeyi iptal et",
                    "H.264 Akışı Bulunamadı",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (choice == DialogResult.Yes)
                {
                    request.AllowReencodeIfH264Missing = true;
                    try
                    {
                        lblStatus.Text = "En İyi Kalite indiriliyor ve H.264'e dönüştürülecek...";
                        var retryResult = await _engine.DownloadAsync(request, progress, _downloadCts.Token);
                        HandleDownloadSuccess(retryResult, outDir);
                    }
                    catch (Exception retryEx)
                    {
                        HandleDownloadError(retryEx);
                    }
                }
                else if (choice == DialogResult.No)
                {
                    request.Profile = DownloadProfile.BestQuality;
                    try
                    {
                        lblStatus.Text = "En İyi Kalite (Orijinal codec) ile indiriliyor...";
                        var retryResult = await _engine.DownloadAsync(request, progress, _downloadCts.Token);
                        HandleDownloadSuccess(retryResult, outDir);
                    }
                    catch (Exception retryEx)
                    {
                        HandleDownloadError(retryEx);
                    }
                }
            }
            catch (MediaValidationException ex)
            {
                lblStatus.Text = "Medya Doğrulama Hatası!";
                lblStatus.ForeColor = Color.DarkRed;
                lblProgressDetails.Text = "Çıktı doğrulanamadı.";
                Logger.LogError("Medya doğrulama hatası", ex);
                MessageBox.Show($"İndirilen medya dosyası doğrulanamadı:\n\n{ex.Message}", "Doğrulama Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OperationCanceledException)
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
                lblStatus.Text = "İndirme işlemi iptal edildi.";
                lblStatus.ForeColor = Color.DarkOrange;
                lblProgressDetails.Text = "İptal edildi.";
            }
            catch (BotVerificationException ex)
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
                lblStatus.Text = "YouTube Bot Koruması: Oturum açmanız gerekiyor.";
                lblStatus.ForeColor = Color.DarkOrange;
                lblProgressDetails.Text = "Bot doğrulaması hatası.";
                Logger.Log("İndirme bot doğrulaması hatası: " + ex.Message);
                MessageBox.Show(ex.Message, "Doğrulama Gerekli", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                HandleDownloadError(ex);
            }
            finally
            {
                SetUiState(isDownloading: false);
            }
        }

        private void UpdateProgressUi(DownloadProgressReport report)
        {
            if (report == null) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateProgressUi(report)));
                return;
            }

            bool hasKnownTotal = (report.TotalBytesRaw.HasValue && report.TotalBytesRaw.Value > 0) ||
                                 (report.EstimatedTotalBytes.HasValue && report.EstimatedTotalBytes.Value > 0);

            if (report.Stage == DownloadStage.Completed)
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
                progressBar1.Value = 100;
                lblProgressPercent.Text = "%100";
            }
            else if (hasKnownTotal || report.Percentage > 0)
            {
                int pVal = Math.Min(99, Math.Max(0, (int)Math.Round(report.Percentage)));
                progressBar1.Style = ProgressBarStyle.Blocks;
                progressBar1.Value = pVal;
                lblProgressPercent.Text = $"%{pVal}";
            }
            else
            {
                // Toplam boyut bilinmiyorsa sahte yüzde üretme: Belirsiz (Marquee) çubuk
                if (progressBar1.Style != ProgressBarStyle.Marquee)
                {
                    progressBar1.Style = ProgressBarStyle.Marquee;
                    progressBar1.MarqueeAnimationSpeed = 30;
                }
                lblProgressPercent.Text = "İndiriliyor...";
            }

            string sizeStr = report.FormattedSizeProgress;
            string speedStr = report.FormattedSpeed;
            string etaStr = report.FormattedEta;

            lblProgressDetails.Text = $"{sizeStr}   •   {speedStr}   •   {etaStr}";

            if (!string.IsNullOrEmpty(report.StatusMessage))
            {
                lblStatus.Text = report.StatusMessage;
            }
        }

        private void HandleDownloadSuccess(DownloadResult downloadResult, string outDir)
        {
            _lastDownloadedFile = downloadResult.FilePath;
            progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.Value = 100;
            lblProgressPercent.Text = "%100";
            lblStatus.Text = "Tebrikler! İndirme ve doğrulama başarıyla tamamlandı.";
            lblStatus.ForeColor = Color.DarkGreen;

            string fileName = Path.GetFileName(downloadResult.FilePath ?? "");
            string codecInfo = downloadResult.Validation != null
                ? $"Video: {downloadResult.Validation.VideoCodec.ToUpper()} | Ses: {downloadResult.Validation.AudioCodec.ToUpper()}"
                : "Doğrulandı";

            string finalSize = downloadResult.Validation != null && downloadResult.Validation.FileSizeBytes > 0
                ? DownloadProgressReport.FormatBytes(downloadResult.Validation.FileSizeBytes)
                : "";

            lblProgressDetails.Text = string.IsNullOrEmpty(finalSize)
                ? $"Dosya: {fileName} ({codecInfo})   •   İndirme tamamlandı"
                : $"İndirilen: {finalSize} / {finalSize} (%100)   •   Hız: Tamamlandı   •   İndirme tamamlandı";

            btnOpenFile.Enabled = !string.IsNullOrEmpty(_lastDownloadedFile) && File.Exists(_lastDownloadedFile);
            btnOpenFolder.Enabled = true;

            string infoMessage = $"Video başarıyla indirildi ve doğrulandı!\n\nDosya: {_lastDownloadedFile}";
            if (downloadResult.Validation != null && downloadResult.Validation.HasVideo)
            {
                infoMessage += $"\n\nVideo Codec: {downloadResult.Validation.VideoCodec}\nSes Codec: {downloadResult.Validation.AudioCodec}\nÇözünürlük: {downloadResult.Validation.Width}x{downloadResult.Validation.Height}";
                if (downloadResult.Validation.IsWindowsPlayerFriendly)
                {
                    infoMessage += "\n\n✔ Windows Media Player ile %100 uyumludur (H.264/AAC).";
                }
                else if (downloadResult.WasReencoded)
                {
                    infoMessage += "\n\n✔ FFmpeg ile H.264 formatına dönüştürüldü.";
                }
            }

            MessageBox.Show(infoMessage, "İndirme Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void HandleDownloadError(Exception ex)
        {
            progressBar1.Style = ProgressBarStyle.Blocks;
            lblStatus.Text = "İndirme başarısız oldu!";
            lblStatus.ForeColor = Color.DarkRed;
            lblProgressDetails.Text = "Hata oluştu.";
            Logger.LogError("İndirme hatası", ex);
            MessageBox.Show($"İndirme sırasında hata oluştu:\n\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_downloadCts != null && !_downloadCts.IsCancellationRequested)
            {
                var confirm = MessageBox.Show("Devam eden indirmeyi iptal etmek istiyor musunuz?", "İptal Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    lblStatus.Text = "İptal ediliyor...";
                    _downloadCts.Cancel();
                }
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_lastDownloadedFile) && File.Exists(_lastDownloadedFile))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _lastDownloadedFile,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("Dosya bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dosya açılamadı: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_lastDownloadedFile) && File.Exists(_lastDownloadedFile))
                {
                    Process.Start("explorer.exe", $"/select,\"{_lastDownloadedFile}\"");
                }
                else if (Directory.Exists(txtFolderPath.Text))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = txtFolderPath.Text,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Klasör açılamadı: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnUpdateEngine_Click(object sender, EventArgs e)
        {
            btnUpdateEngine.Enabled = false;
            lblStatus.Text = "İndirme motoru (yt-dlp) güncelleniyor...";
            try
            {
                string result = await _engine.UpdateEngineAsync();
                MessageBox.Show("Motor Güncelleme Sonucu:\n\n" + result, "Güncelleme", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblStatus.Text = "Motor güncellendi.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Güncelleme hatası:\n\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Güncelleme başarısız.";
            }
            finally
            {
                btnUpdateEngine.Enabled = true;
            }
        }

        private void btnViewLogs_Click(object sender, EventArgs e)
        {
            Logger.OpenLog();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_downloadCts != null && !_downloadCts.IsCancellationRequested)
            {
                var confirm = MessageBox.Show("Devam eden bir indirme var. Çıkmak istiyor musunuz?", "Çıkış Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                _downloadCts?.Cancel();
            }

            _settings.LastQuality = cmbQuality.SelectedItem?.ToString();
            _settings.IsAudioOnly = rbAudio.Checked;
            SettingsService.Save(_settings);
        }

        private void SetUiState(bool isAnalyzing = false, bool isDownloading = false)
        {
            if (isAnalyzing)
            {
                btnAnalyze.Enabled = false;
                btnPaste.Enabled = false;
                txtUrl.Enabled = false;
                btnDownload.Enabled = false;
            }
            else if (isDownloading)
            {
                btnDownload.Enabled = false;
                btnCancel.Enabled = true;
                btnAnalyze.Enabled = false;
                btnPaste.Enabled = false;
                txtUrl.Enabled = false;
                grpOptions.Enabled = false;
                btnBrowseFolder.Enabled = false;
                btnOpenFile.Enabled = false;
                btnOpenFolder.Enabled = false;
                btnUpdateEngine.Enabled = false;
            }
            else
            {
                btnAnalyze.Enabled = true;
                btnPaste.Enabled = true;
                txtUrl.Enabled = true;
                btnDownload.Enabled = true;
                btnCancel.Enabled = false;
                grpOptions.Enabled = true;
                btnBrowseFolder.Enabled = true;
                btnUpdateEngine.Enabled = true;
            }
        }

        private class QualityItem
        {
            public string Label { get; }
            public int? Resolution { get; }

            public QualityItem(string label, int? resolution)
            {
                Label = label;
                Resolution = resolution;
            }

            public override string ToString() => Label;
        }
    }
}
