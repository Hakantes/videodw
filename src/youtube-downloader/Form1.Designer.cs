namespace youtube_dowload
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.txtUrl = new System.Windows.Forms.TextBox();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.btnPaste = new System.Windows.Forms.Button();
            this.grpVideoInfo = new System.Windows.Forms.GroupBox();
            this.lblVideoInfo = new System.Windows.Forms.Label();
            this.lblVideoTitle = new System.Windows.Forms.Label();
            this.picThumbnail = new System.Windows.Forms.PictureBox();
            this.grpOptions = new System.Windows.Forms.GroupBox();
            this.lblQualityDesc = new System.Windows.Forms.Label();
            this.cmbQuality = new System.Windows.Forms.ComboBox();
            this.rbAudio = new System.Windows.Forms.RadioButton();
            this.rbVideo = new System.Windows.Forms.RadioButton();
            this.lblProfileDesc = new System.Windows.Forms.Label();
            this.cmbProfile = new System.Windows.Forms.ComboBox();
            this.lblCookiesDesc = new System.Windows.Forms.Label();
            this.cmbCookies = new System.Windows.Forms.ComboBox();
            this.grpFolder = new System.Windows.Forms.GroupBox();
            this.btnBrowseFolder = new System.Windows.Forms.Button();
            this.txtFolderPath = new System.Windows.Forms.TextBox();
            this.grpProgress = new System.Windows.Forms.GroupBox();
            this.lblProgressDetails = new System.Windows.Forms.Label();
            this.lblProgressPercent = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnUpdateEngine = new System.Windows.Forms.Button();
            this.btnViewLogs = new System.Windows.Forms.Button();
            this.lblUrlPrompt = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.grpVideoInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picThumbnail)).BeginInit();
            this.grpOptions.SuspendLayout();
            this.grpFolder.SuspendLayout();
            this.grpProgress.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblUrlPrompt
            // 
            this.lblUrlPrompt.AutoSize = true;
            this.lblUrlPrompt.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblUrlPrompt.Location = new System.Drawing.Point(20, 15);
            this.lblUrlPrompt.Name = "lblUrlPrompt";
            this.lblUrlPrompt.Size = new System.Drawing.Size(149, 17);
            this.lblUrlPrompt.TabIndex = 0;
            this.lblUrlPrompt.Text = "Video Bağlantısı (URL):";
            // 
            // txtUrl
            // 
            this.txtUrl.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.txtUrl.Location = new System.Drawing.Point(23, 38);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(580, 25);
            this.txtUrl.TabIndex = 1;
            this.txtUrl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtUrl_KeyDown);
            // 
            // btnPaste
            // 
            this.btnPaste.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnPaste.Location = new System.Drawing.Point(609, 36);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(85, 29);
            this.btnPaste.TabIndex = 2;
            this.btnPaste.Text = "📋 Yapıştır";
            this.toolTip1.SetToolTip(this.btnPaste, "Panodaki bağlantıyı yapıştırır");
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Click += new System.EventHandler(this.btnPaste_Click);
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnAnalyze.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnalyze.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnAnalyze.ForeColor = System.Drawing.Color.White;
            this.btnAnalyze.Location = new System.Drawing.Point(700, 36);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(110, 29);
            this.btnAnalyze.TabIndex = 3;
            this.btnAnalyze.Text = "🔍 Analiz Et";
            this.toolTip1.SetToolTip(this.btnAnalyze, "Video bilgilerini ve mevcut kaliteleri çeker");
            this.btnAnalyze.UseVisualStyleBackColor = false;
            this.btnAnalyze.Click += new System.EventHandler(this.btnAnalyze_Click);
            // 
            // grpVideoInfo
            // 
            this.grpVideoInfo.Controls.Add(this.lblVideoInfo);
            this.grpVideoInfo.Controls.Add(this.lblVideoTitle);
            this.grpVideoInfo.Controls.Add(this.picThumbnail);
            this.grpVideoInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.grpVideoInfo.Location = new System.Drawing.Point(23, 73);
            this.grpVideoInfo.Name = "grpVideoInfo";
            this.grpVideoInfo.Size = new System.Drawing.Size(787, 138);
            this.grpVideoInfo.TabIndex = 4;
            this.grpVideoInfo.TabStop = false;
            this.grpVideoInfo.Text = "Video Bilgileri";
            // 
            // lblVideoInfo
            // 
            this.lblVideoInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblVideoInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblVideoInfo.Location = new System.Drawing.Point(220, 80);
            this.lblVideoInfo.Name = "lblVideoInfo";
            this.lblVideoInfo.Size = new System.Drawing.Size(550, 48);
            this.lblVideoInfo.TabIndex = 2;
            this.lblVideoInfo.Text = "Kanal: - | Süre: -";
            // 
            // lblVideoTitle
            // 
            this.lblVideoTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblVideoTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lblVideoTitle.Location = new System.Drawing.Point(220, 20);
            this.lblVideoTitle.Name = "lblVideoTitle";
            this.lblVideoTitle.Size = new System.Drawing.Size(550, 56);
            this.lblVideoTitle.TabIndex = 1;
            this.lblVideoTitle.Text = "Henüz bir video analiz edilmedi. Lütfen bağlantı girip 'Analiz Et' butonuna tıklayınız.";
            // 
            // picThumbnail
            // 
            this.picThumbnail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.picThumbnail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picThumbnail.Location = new System.Drawing.Point(15, 22);
            this.picThumbnail.Name = "picThumbnail";
            this.picThumbnail.Size = new System.Drawing.Size(190, 105);
            this.picThumbnail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picThumbnail.TabIndex = 0;
            this.picThumbnail.TabStop = false;
            // 
            // grpOptions
            // 
            this.grpOptions.Controls.Add(this.lblQualityDesc);
            this.grpOptions.Controls.Add(this.cmbQuality);
            this.grpOptions.Controls.Add(this.rbAudio);
            this.grpOptions.Controls.Add(this.rbVideo);
            this.grpOptions.Controls.Add(this.lblProfileDesc);
            this.grpOptions.Controls.Add(this.cmbProfile);
            this.grpOptions.Controls.Add(this.lblCookiesDesc);
            this.grpOptions.Controls.Add(this.cmbCookies);
            this.grpOptions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.grpOptions.Location = new System.Drawing.Point(23, 217);
            this.grpOptions.Name = "grpOptions";
            this.grpOptions.Size = new System.Drawing.Size(787, 100);
            this.grpOptions.TabIndex = 5;
            this.grpOptions.TabStop = false;
            this.grpOptions.Text = "İndirme Biçimi ve Uyumluluk Profili";
            // 
            // rbVideo
            // 
            this.rbVideo.AutoSize = true;
            this.rbVideo.Checked = true;
            this.rbVideo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.rbVideo.Location = new System.Drawing.Point(25, 27);
            this.rbVideo.Name = "rbVideo";
            this.rbVideo.Size = new System.Drawing.Size(101, 19);
            this.rbVideo.TabIndex = 0;
            this.rbVideo.TabStop = true;
            this.rbVideo.Text = "🎬 Video (MP4)";
            this.rbVideo.UseVisualStyleBackColor = true;
            this.rbVideo.CheckedChanged += new System.EventHandler(this.rbFormat_CheckedChanged);
            // 
            // rbAudio
            // 
            this.rbAudio.AutoSize = true;
            this.rbAudio.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.rbAudio.Location = new System.Drawing.Point(145, 27);
            this.rbAudio.Name = "rbAudio";
            this.rbAudio.Size = new System.Drawing.Size(138, 19);
            this.rbAudio.TabIndex = 1;
            this.rbAudio.Text = "🎵 Yalnızca Ses (MP3)";
            this.rbAudio.UseVisualStyleBackColor = true;
            this.rbAudio.CheckedChanged += new System.EventHandler(this.rbFormat_CheckedChanged);
            // 
            // lblQualityDesc
            // 
            this.lblQualityDesc.AutoSize = true;
            this.lblQualityDesc.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblQualityDesc.Location = new System.Drawing.Point(320, 29);
            this.lblQualityDesc.Name = "lblQualityDesc";
            this.lblQualityDesc.Size = new System.Drawing.Size(108, 15);
            this.lblQualityDesc.TabIndex = 2;
            this.lblQualityDesc.Text = "Video Çözünürlüğü:";
            // 
            // cmbQuality
            // 
            this.cmbQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbQuality.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.cmbQuality.FormattingEnabled = true;
            this.cmbQuality.Location = new System.Drawing.Point(435, 25);
            this.cmbQuality.Name = "cmbQuality";
            this.cmbQuality.Size = new System.Drawing.Size(335, 23);
            this.cmbQuality.TabIndex = 3;
            // 
            // lblProfileDesc
            // 
            this.lblProfileDesc.AutoSize = true;
            this.lblProfileDesc.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblProfileDesc.Location = new System.Drawing.Point(25, 65);
            this.lblProfileDesc.Name = "lblProfileDesc";
            this.lblProfileDesc.Size = new System.Drawing.Size(83, 15);
            this.lblProfileDesc.TabIndex = 4;
            this.lblProfileDesc.Text = "İndirme Profili:";
            // 
            // cmbProfile
            // 
            this.cmbProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProfile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.cmbProfile.FormattingEnabled = true;
            this.cmbProfile.Location = new System.Drawing.Point(115, 61);
            this.cmbProfile.Name = "cmbProfile";
            this.cmbProfile.Size = new System.Drawing.Size(190, 23);
            this.cmbProfile.TabIndex = 5;
            this.cmbProfile.SelectedIndexChanged += new System.EventHandler(this.cmbProfile_SelectedIndexChanged);
            // 
            // lblCookiesDesc
            // 
            this.lblCookiesDesc.AutoSize = true;
            this.lblCookiesDesc.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblCookiesDesc.Location = new System.Drawing.Point(320, 65);
            this.lblCookiesDesc.Name = "lblCookiesDesc";
            this.lblCookiesDesc.Size = new System.Drawing.Size(99, 15);
            this.lblCookiesDesc.TabIndex = 6;
            this.lblCookiesDesc.Text = "Tarayıcı Çerezleri:";
            // 
            // cmbCookies
            // 
            this.cmbCookies.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCookies.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.cmbCookies.FormattingEnabled = true;
            this.cmbCookies.Location = new System.Drawing.Point(435, 61);
            this.cmbCookies.Name = "cmbCookies";
            this.cmbCookies.Size = new System.Drawing.Size(335, 23);
            this.cmbCookies.TabIndex = 7;
            this.cmbCookies.SelectedIndexChanged += new System.EventHandler(this.cmbCookies_SelectedIndexChanged);
            // 
            // grpFolder
            // 
            this.grpFolder.Controls.Add(this.btnBrowseFolder);
            this.grpFolder.Controls.Add(this.txtFolderPath);
            this.grpFolder.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.grpFolder.Location = new System.Drawing.Point(23, 323);
            this.grpFolder.Name = "grpFolder";
            this.grpFolder.Size = new System.Drawing.Size(787, 62);
            this.grpFolder.TabIndex = 6;
            this.grpFolder.TabStop = false;
            this.grpFolder.Text = "Kayıt Klasörü";
            // 
            // btnBrowseFolder
            // 
            this.btnBrowseFolder.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnBrowseFolder.Location = new System.Drawing.Point(677, 23);
            this.btnBrowseFolder.Name = "btnBrowseFolder";
            this.btnBrowseFolder.Size = new System.Drawing.Size(93, 27);
            this.btnBrowseFolder.TabIndex = 1;
            this.btnBrowseFolder.Text = "📁 Gözat...";
            this.btnBrowseFolder.UseVisualStyleBackColor = true;
            this.btnBrowseFolder.Click += new System.EventHandler(this.btnBrowseFolder_Click);
            // 
            // txtFolderPath
            // 
            this.txtFolderPath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.txtFolderPath.Location = new System.Drawing.Point(25, 25);
            this.txtFolderPath.Name = "txtFolderPath";
            this.txtFolderPath.ReadOnly = true;
            this.txtFolderPath.Size = new System.Drawing.Size(645, 23);
            this.txtFolderPath.TabIndex = 0;
            // 
            // grpProgress
            // 
            this.grpProgress.Controls.Add(this.lblProgressDetails);
            this.grpProgress.Controls.Add(this.lblProgressPercent);
            this.grpProgress.Controls.Add(this.progressBar1);
            this.grpProgress.Controls.Add(this.lblStatus);
            this.grpProgress.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.grpProgress.Location = new System.Drawing.Point(23, 391);
            this.grpProgress.Name = "grpProgress";
            this.grpProgress.Size = new System.Drawing.Size(787, 105);
            this.grpProgress.TabIndex = 7;
            this.grpProgress.TabStop = false;
            this.grpProgress.Text = "İndirme ve Doğrulama Durumu";
            // 
            // lblProgressDetails
            // 
            this.lblProgressDetails.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblProgressDetails.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblProgressDetails.Location = new System.Drawing.Point(22, 77);
            this.lblProgressDetails.Name = "lblProgressDetails";
            this.lblProgressDetails.Size = new System.Drawing.Size(748, 20);
            this.lblProgressDetails.TabIndex = 3;
            this.lblProgressDetails.Text = "İndirilen: -  •  Hız: -  •  Kalan süre: -";
            // 
            // lblProgressPercent
            // 
            this.lblProgressPercent.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblProgressPercent.Location = new System.Drawing.Point(710, 49);
            this.lblProgressPercent.Name = "lblProgressPercent";
            this.lblProgressPercent.Size = new System.Drawing.Size(60, 20);
            this.lblProgressPercent.TabIndex = 2;
            this.lblProgressPercent.Text = "%0";
            this.lblProgressPercent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(25, 49);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(680, 22);
            this.progressBar1.TabIndex = 1;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblStatus.Location = new System.Drawing.Point(22, 25);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(40, 15);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Hazır.";
            // 
            // btnDownload
            // 
            this.btnDownload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(124)))), ((int)(((byte)(65)))));
            this.btnDownload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDownload.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnDownload.ForeColor = System.Drawing.Color.White;
            this.btnDownload.Location = new System.Drawing.Point(23, 503);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(180, 42);
            this.btnDownload.TabIndex = 8;
            this.btnDownload.Text = "⬇ İndirmeyi Başlat";
            this.toolTip1.SetToolTip(this.btnDownload, "Seçilen ayarlarla indirmeyi başlatır");
            this.btnDownload.UseVisualStyleBackColor = false;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(43)))), ((int)(((byte)(28)))));
            this.btnCancel.Enabled = false;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(215, 503);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 42);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "✖ İptal Et";
            this.toolTip1.SetToolTip(this.btnCancel, "Devam eden indirmeyi güvenle iptal eder");
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.Enabled = false;
            this.btnOpenFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnOpenFile.Location = new System.Drawing.Point(348, 503);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(110, 42);
            this.btnOpenFile.TabIndex = 10;
            this.btnOpenFile.Text = "▶ Dosyayı Aç";
            this.btnOpenFile.UseVisualStyleBackColor = true;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Enabled = false;
            this.btnOpenFolder.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnOpenFolder.Location = new System.Drawing.Point(468, 503);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(130, 42);
            this.btnOpenFolder.TabIndex = 11;
            this.btnOpenFolder.Text = "📂 Klasörde Göster";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            // 
            // btnUpdateEngine
            // 
            this.btnUpdateEngine.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnUpdateEngine.Location = new System.Drawing.Point(610, 503);
            this.btnUpdateEngine.Name = "btnUpdateEngine";
            this.btnUpdateEngine.Size = new System.Drawing.Size(100, 42);
            this.btnUpdateEngine.TabIndex = 12;
            this.btnUpdateEngine.Text = "🔄 Motoru Güncelle";
            this.toolTip1.SetToolTip(this.btnUpdateEngine, "yt-dlp motorunu en son sürüme günceller");
            this.btnUpdateEngine.UseVisualStyleBackColor = true;
            this.btnUpdateEngine.Click += new System.EventHandler(this.btnUpdateEngine_Click);
            // 
            // btnViewLogs
            // 
            this.btnViewLogs.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnViewLogs.Location = new System.Drawing.Point(718, 503);
            this.btnViewLogs.Name = "btnViewLogs";
            this.btnViewLogs.Size = new System.Drawing.Size(92, 42);
            this.btnViewLogs.TabIndex = 13;
            this.btnViewLogs.Text = "📝 Loglar";
            this.toolTip1.SetToolTip(this.btnViewLogs, "Hata ve işlem loglarını açar");
            this.btnViewLogs.UseVisualStyleBackColor = true;
            this.btnViewLogs.Click += new System.EventHandler(this.btnViewLogs_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(834, 560);
            this.Controls.Add(this.btnViewLogs);
            this.Controls.Add(this.btnUpdateEngine);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.btnOpenFile);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.grpProgress);
            this.Controls.Add(this.grpFolder);
            this.Controls.Add(this.grpOptions);
            this.Controls.Add(this.grpVideoInfo);
            this.Controls.Add(this.btnAnalyze);
            this.Controls.Add(this.btnPaste);
            this.Controls.Add(this.txtUrl);
            this.Controls.Add(this.lblUrlPrompt);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "YouTube ve Video İndirici v2.0.3 (FFmpeg & yt-dlp)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.grpVideoInfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picThumbnail)).EndInit();
            this.grpOptions.ResumeLayout(false);
            this.grpOptions.PerformLayout();
            this.grpFolder.ResumeLayout(false);
            this.grpFolder.PerformLayout();
            this.grpProgress.ResumeLayout(false);
            this.grpProgress.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblUrlPrompt;
        private System.Windows.Forms.TextBox txtUrl;
        private System.Windows.Forms.Button btnPaste;
        private System.Windows.Forms.Button btnAnalyze;
        private System.Windows.Forms.GroupBox grpVideoInfo;
        private System.Windows.Forms.PictureBox picThumbnail;
        private System.Windows.Forms.Label lblVideoTitle;
        private System.Windows.Forms.Label lblVideoInfo;
        private System.Windows.Forms.GroupBox grpOptions;
        private System.Windows.Forms.RadioButton rbVideo;
        private System.Windows.Forms.RadioButton rbAudio;
        private System.Windows.Forms.Label lblQualityDesc;
        private System.Windows.Forms.ComboBox cmbQuality;
        private System.Windows.Forms.Label lblProfileDesc;
        private System.Windows.Forms.ComboBox cmbProfile;
        private System.Windows.Forms.Label lblCookiesDesc;
        private System.Windows.Forms.ComboBox cmbCookies;
        private System.Windows.Forms.GroupBox grpFolder;
        private System.Windows.Forms.TextBox txtFolderPath;
        private System.Windows.Forms.Button btnBrowseFolder;
        private System.Windows.Forms.GroupBox grpProgress;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblProgressPercent;
        private System.Windows.Forms.Label lblProgressDetails;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnUpdateEngine;
        private System.Windows.Forms.Button btnViewLogs;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
