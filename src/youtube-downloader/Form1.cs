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
        private VideoMetadata _currentMetadata;
        private CancellationTokenSource _downloadCts;
        private CancellationTokenSource _analyzeCts;
        private string _lastDownloadedFile;
        private AppSettings _settings;

        public Form1()
        {
            InitializeComponent();
        }

        private ComboBox _cmbCookies;

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
            
            // Programmatically add Cookies option to grpOptions
            grpOptions.Size = new Size(grpOptions.Size.Width, grpOptions.Size.Height + 40);
            
            Label lblCookies = new Label();
            lblCookies.Text = "Tarayıcı Çerezleri:";
            lblCookies.Location = new Point(lblQualityDesc.Location.X, lblQualityDesc.Location.Y + 35);
            lblCookies.AutoSize = true;
            lblCookies.Font = lblQualityDesc.Font;
            grpOptions.Controls.Add(lblCookies);

            _cmbCookies = new ComboBox();
            _cmbCookies.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbCookies.Items.AddRange(new object[] { "Yok", "Chrome", "Edge", "Firefox", "Opera", "Brave", "Safari", "Vivaldi" });
            _cmbCookies.SelectedItem = string.IsNullOrWhiteSpace(_settings.BrowserForCookies) ? "Yok" : _settings.BrowserForCookies;
            _cmbCookies.Location = new Point(cmbQuality.Location.X, cmbQuality.Location.Y + 35);
            _cmbCookies.Size = cmbQuality.Size;
            _cmbCookies.Font = cmbQuality.Font;
            _cmbCookies.SelectedIndexChanged += (s, ev) => 
            { 
                _settings.BrowserForCookies = _cmbCookies.SelectedItem.ToString(); 
                _engine.BrowserForCookies = _settings.BrowserForCookies;
                SettingsService.Save(_settings); 
            };
            grpOptions.Controls.Add(_cmbCookies);
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
                MessageBox.Show("Lütfen bir video bağlantısı (URL) girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUrl.Focus();
                return;
            }

            SetUiState(isAnalyzing: true);
            lblStatus.Text = "Video analiz ediliyor, lütfen bekleyin...";
            lblStatus.ForeColor = Color.Blue;

            _analyzeCts?.Cancel();
            _analyzeCts = new CancellationTokenSource();

            try
            {
                _currentMetadata = await _engine.GetVideoInfoAsync(url, _analyzeCts.Token);

                lblVideoTitle.Text = _currentMetadata.Title;
                lblVideoInfo.Text = $"Kanal: {_currentMetadata.Uploader}  |  Süre: {_currentMetadata.FormattedDuration}";

                // Küçük resmi yükle
                if (!string.IsNullOrEmpty(_currentMetadata.ThumbnailUrl))
                {
                    LoadThumbnailAsync(_currentMetadata.ThumbnailUrl);
                }

                // Çözünürlükleri dinamik doldur (Yalnızca mevcut olanlar!)
                PopulateAvailableResolutions(_currentMetadata.AvailableResolutions);

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

            if (availableHeights != null && availableHeights.Count > 0)
            {
                foreach (int height in availableHeights)
                {
                    string label = GetResolutionLabel(height);
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
                lblQualityDesc.Text = "Ses Formatı: MP3 (320 kbps En Yüksek Kalite)";
            }
            else
            {
                cmbQuality.Enabled = true;
                lblQualityDesc.Text = "Video Çözünürlüğü:";
            }
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
            var request = new DownloadRequest
            {
                Url = url,
                OutputDirectory = outDir,
                IsAudioOnly = rbAudio.Checked,
                TargetResolution = selectedQuality?.Resolution,
                AudioFormat = "mp3"
            };

            SetUiState(isDownloading: true);
            lblStatus.Text = "İndirme başlatılıyor...";
            lblStatus.ForeColor = Color.DarkBlue;
            progressBar1.Value = 0;
            lblProgressPercent.Text = "%0";
            lblProgressDetails.Text = "Hazırlanıyor...";

            _downloadCts = new CancellationTokenSource();

            var progress = new Progress<DownloadProgressReport>(report =>
            {
                if (report == null) return;

                int pVal = Math.Min(100, Math.Max(0, (int)report.Percentage));
                progressBar1.Value = pVal;
                lblProgressPercent.Text = $"%{pVal}";

                var details = new List<string>();
                if (!string.IsNullOrEmpty(report.Speed)) details.Add($"Hız: {report.Speed}");
                if (!string.IsNullOrEmpty(report.Eta)) details.Add($"Kalan: {report.Eta}");
                if (!string.IsNullOrEmpty(report.DownloadedBytes) || !string.IsNullOrEmpty(report.TotalBytes))
                {
                    details.Add($"{report.DownloadedBytes} / {report.TotalBytes}");
                }

                if (details.Count > 0)
                {
                    lblProgressDetails.Text = string.Join("  |  ", details);
                }

                if (!string.IsNullOrEmpty(report.StatusMessage))
                {
                    lblStatus.Text = report.StatusMessage;
                }
            });

            try
            {
                string resultFile = await _engine.DownloadAsync(request, progress, _downloadCts.Token);
                _lastDownloadedFile = resultFile;

                progressBar1.Value = 100;
                lblProgressPercent.Text = "%100";
                lblStatus.Text = "Tebrikler! İndirme başarıyla tamamlandı.";
                lblStatus.ForeColor = Color.DarkGreen;
                lblProgressDetails.Text = "Dosya hazır: " + Path.GetFileName(resultFile ?? "");

                btnOpenFile.Enabled = !string.IsNullOrEmpty(_lastDownloadedFile) && File.Exists(_lastDownloadedFile);
                btnOpenFolder.Enabled = true;

                MessageBox.Show("Video başarıyla indirildi!\n\nDosya: " + (resultFile ?? outDir), "İndirme Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "İndirme işlemi iptal edildi.";
                lblStatus.ForeColor = Color.DarkOrange;
                lblProgressDetails.Text = "İptal edildi.";
            }
            catch (BotVerificationException ex)
            {
                lblStatus.Text = "YouTube Bot Koruması: Oturum açmanız gerekiyor.";
                lblStatus.ForeColor = Color.DarkOrange;
                lblProgressDetails.Text = "Bot doğrulaması hatası.";
                Logger.Log("İndirme bot doğrulaması hatası: " + ex.Message);
                MessageBox.Show(ex.Message, "Doğrulama Gerekli", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "İndirme başarısız oldu!";
                lblStatus.ForeColor = Color.DarkRed;
                lblProgressDetails.Text = "Hata oluştu.";
                Logger.LogError("İndirme hatası", ex);
                MessageBox.Show($"İndirme sırasında hata oluştu:\n\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetUiState(isDownloading: false);
            }
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
