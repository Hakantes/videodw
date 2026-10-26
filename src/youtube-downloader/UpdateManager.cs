using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace youtube_dowload
{
    public enum InstallType
    {
        InnoSetupInstalled,
        PortableZip
    }

    public class PackageInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Sha256 { get; set; } = string.Empty;
    }

    public class UpdateManifest
    {
        public string Version { get; set; } = string.Empty;
        public string ReleaseDate { get; set; } = string.Empty;
        public bool Mandatory { get; set; }
        public string ChangelogUrl { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public Dictionary<string, PackageInfo> Packages { get; set; } = new Dictionary<string, PackageInfo>(StringComparer.OrdinalIgnoreCase);
        public string Signature { get; set; } = string.Empty;

        public string GetCanonicalString()
        {
            var sb = new StringBuilder();
            sb.Append(Version ?? string.Empty).Append("|");
            sb.Append(ReleaseDate ?? string.Empty).Append("|");
            sb.Append(ChangelogUrl ?? string.Empty).Append("|");

            var sortedKeys = new List<string>(Packages.Keys);
            sortedKeys.Sort(StringComparer.Ordinal);
            foreach (var k in sortedKeys)
            {
                var pkg = Packages[k];
                sb.Append(k).Append(":")
                  .Append(pkg.FileName).Append(":")
                  .Append(pkg.Size).Append(":")
                  .Append(pkg.Sha256).Append(";");
            }
            return sb.ToString();
        }
    }

    public class UpdateCheckResult
    {
        public bool IsUpdateAvailable { get; set; }
        public Version CurrentVersion { get; set; }
        public Version NewVersion { get; set; }
        public string ReleaseNotes { get; set; } = string.Empty;
        public string ChangelogUrl { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long PackageSizeBytes { get; set; }
        public string ExpectedSha256 { get; set; } = string.Empty;
        public InstallType InstallType { get; set; }
        public bool IsSignatureVerified { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public bool IsError { get; set; }
    }

    public class UpdateManager
    {
        public const string DefaultManifestUrl = "https://raw.githubusercontent.com/Hakantes/videodw/master/update-manifest.json";
        public const string FallbackReleasesApiUrl = "https://api.github.com/repos/Hakantes/videodw/releases/latest";

        public const string RsaPublicKeyXml =
            "<RSAKeyValue><Modulus>v+2DmdXu1p8KzYYB5dd3KysfwISKSn3sNe1jgEg0TeFj0GqDkdaAFQXlUyFpsXuK6OqNas3npBTrvYD8gFOd2M1ahu8yOSD8iUVk5dG6KNiAxTLkaN7vjox5yTAiz1TRVS66k+JyXgU8VzzVEWpJZJ4M+NtpaF7fXtasIfNwzXIofJjCa61qkuUM6ojkXuPtjJ9LdUkqn57MX4x+9Nd4lrS/HVbXWVb2Rm1uq7WXYsBLu6KMpCRd35cf9LoyOF1GUj6Q/PN9uVt5Rfn/HOGnvYuwiXa+YDpX0KL1taPMF5UAvUiaaYyfceuMCwP6sTiVj+4GIkvkdEzEGQFtWYeOTQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        private static readonly HttpClient _httpClient = new HttpClient();

        static UpdateManager()
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "YouTubeVideoDownloader-Updater/2.1.0");
            }
        }

        public Version CurrentVersion { get; }

        public UpdateManager(Version currentVersion = null)
        {
            if (currentVersion != null)
            {
                CurrentVersion = currentVersion;
            }
            else
            {
                var ver = typeof(UpdateManager).Assembly.GetName().Version;
                CurrentVersion = new Version(ver.Major, ver.Minor, Math.Max(0, ver.Build));
            }
        }

        public static InstallType DetectInstallType()
        {
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');

                string[] rootKeys = { @"Software\Microsoft\Windows\CurrentVersion\Uninstall", @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall" };
                RegistryKey[] hives = { Registry.CurrentUser, Registry.LocalMachine };

                foreach (var hive in hives)
                {
                    foreach (var root in rootKeys)
                    {
                        using (var key = hive.OpenSubKey(root + @"\{E684D355-63F6-4A59-86BC-B368A91522F5}_is1"))
                        {
                            if (key != null)
                            {
                                string installLocation = (key.GetValue("InstallLocation") as string)?.TrimEnd('\\', '/');
                                if (!string.IsNullOrEmpty(installLocation) &&
                                    string.Equals(installLocation, appDir, StringComparison.OrdinalIgnoreCase))
                                {
                                    return InstallType.InnoSetupInstalled;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Kurulum türü tespiti hatası (varsayılan Portable): " + ex.Message);
            }

            return InstallType.PortableZip;
        }

        public static bool ShouldCheckAutomatically(AppSettings settings)
        {
            if (settings == null) return false;
            if (!settings.CheckForUpdatesOnStartup) return false;

            if (settings.LastUpdateCheckUtc.HasValue)
            {
                var diff = DateTime.UtcNow - settings.LastUpdateCheckUtc.Value;
                if (diff.TotalHours < 24.0)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<UpdateCheckResult> CheckForUpdatesAsync(
            bool isManual = false,
            string manifestUrl = DefaultManifestUrl,
            CancellationToken cancellationToken = default)
        {
            var installType = DetectInstallType();
            var result = new UpdateCheckResult
            {
                CurrentVersion = CurrentVersion,
                InstallType = installType
            };

            try
            {
                string jsonContent = null;
                try
                {
                    jsonContent = await _httpClient.GetStringAsync(manifestUrl);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Birincil manifest indirilemedi ({manifestUrl}): {ex.Message}. GitHub Releases API deneniyor...");
                }

                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    jsonContent = await FetchManifestFromGitHubReleasesAsync(FallbackReleasesApiUrl, cancellationToken);
                }

                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    result.IsError = true;
                    result.StatusMessage = "Güncelleme sunucusuna ulaşılamadı.";
                    return result;
                }

                var manifest = ParseManifest(jsonContent);
                if (manifest == null || string.IsNullOrWhiteSpace(manifest.Version))
                {
                    result.IsError = true;
                    result.StatusMessage = "Güncelleme manifesti geçersiz.";
                    return result;
                }

                // Verify cryptographic RSA signature
                bool isSignatureValid = VerifyManifestSignature(manifest);
                result.IsSignatureVerified = isSignatureValid;

                if (!isSignatureValid)
                {
                    result.IsError = true;
                    result.IsUpdateAvailable = false;
                    result.StatusMessage = "Güvenlik Hatası: Güncelleme manifestinin dijital imzası doğrulanamadı veya tahrif edilmiş!";
                    Logger.Log("GÜVENLİK İHLALİ: Manifest dijital imzası geçersiz! Güncelleme işlemi durduruldu.");
                    return result;
                }

                if (!Version.TryParse(manifest.Version, out var remoteVer))
                {
                    result.IsError = true;
                    result.StatusMessage = "Uzak sürüm numarası ayrıştırılamadı: " + manifest.Version;
                    return result;
                }

                result.NewVersion = remoteVer;
                result.ReleaseNotes = manifest.ReleaseNotes;
                result.ChangelogUrl = manifest.ChangelogUrl;

                // Downgrade protection: Only report update if remote > current
                if (remoteVer <= CurrentVersion)
                {
                    result.IsUpdateAvailable = false;
                    result.StatusMessage = $"En güncel sürümü kullanıyorsunuz (v{CurrentVersion}).";
                    return result;
                }

                // Choose package strictly based on Windows installation type
                PackageInfo pkg = null;
                string primaryKey = (installType == InstallType.InnoSetupInstalled) ? "windows-setup" : "windows-portable";
                string fallbackKey = (installType == InstallType.InnoSetupInstalled) ? "windows-portable" : "windows-setup";

                if (manifest.Packages.TryGetValue(primaryKey, out var p1) && p1 != null && !string.IsNullOrWhiteSpace(p1.Url))
                {
                    pkg = p1;
                }
                else if (manifest.Packages.TryGetValue(fallbackKey, out var p2) && p2 != null && !string.IsNullOrWhiteSpace(p2.Url))
                {
                    pkg = p2;
                }

                if (pkg == null || string.IsNullOrWhiteSpace(pkg.Url))
                {
                    result.IsError = true;
                    result.StatusMessage = "Windows için uygun güncelleme paketi bulunamadı.";
                    return result;
                }

                // Strict sanity check: ensure Windows never downloads macOS package
                if (pkg.FileName.IndexOf("mac", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    pkg.FileName.EndsWith(".dmg", StringComparison.OrdinalIgnoreCase))
                {
                    result.IsError = true;
                    result.StatusMessage = "Platform uyuşmazlığı hatası: macOS paketi Windows üzerinde çalıştırılamaz.";
                    return result;
                }

                result.IsUpdateAvailable = true;
                result.DownloadUrl = pkg.Url;
                result.FileName = pkg.FileName;
                result.PackageSizeBytes = pkg.Size;
                result.ExpectedSha256 = pkg.Sha256;
                result.StatusMessage = $"Yeni bir sürüm mevcut: v{remoteVer}";

                return result;
            }
            catch (OperationCanceledException)
            {
                result.IsError = true;
                result.StatusMessage = "Güncelleme denetimi iptal edildi.";
                return result;
            }
            catch (Exception ex)
            {
                Logger.Log("Güncelleme denetimi hatası: " + ex.Message);
                result.IsError = true;
                result.StatusMessage = "Güncelleme denetlenirken hata oluştu.";
                return result;
            }
        }

        private async Task<string> FetchManifestFromGitHubReleasesAsync(string releasesApiUrl, CancellationToken cancellationToken)
        {
            try
            {
                using (var req = new HttpRequestMessage(HttpMethod.Get, releasesApiUrl))
                {
                    req.Headers.Accept.ParseAdd("application/vnd.github+json");
                    using (var resp = await _httpClient.SendAsync(req, cancellationToken))
                    {
                        if (!resp.IsSuccessStatusCode)
                        {
                            Logger.Log($"GitHub Releases API yanıtı: {resp.StatusCode}");
                            return null;
                        }

                        string body = await resp.Content.ReadAsStringAsync();
                        using (var doc = JsonDocument.Parse(body))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("assets", out var assets) && assets.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var asset in assets.EnumerateArray())
                                {
                                    string name = asset.GetProperty("name").GetString();
                                    if (string.Equals(name, "update-manifest.json", StringComparison.OrdinalIgnoreCase))
                                    {
                                        string downloadUrl = asset.GetProperty("browser_download_url").GetString();
                                        return await _httpClient.GetStringAsync(downloadUrl);
                                    }
                                }
                            }

                            // If manifest asset not found, construct a basic manifest from release metadata
                            string tagName = root.GetProperty("tag_name").GetString() ?? "";
                            string version = tagName.TrimStart('v', 'V');
                            string releaseNotes = root.TryGetProperty("body", out var b) ? b.GetString() : "";
                            string htmlUrl = root.TryGetProperty("html_url", out var h) ? h.GetString() : "";

                            var fallbackManifest = new UpdateManifest
                            {
                                Version = version,
                                ReleaseNotes = releaseNotes,
                                ChangelogUrl = htmlUrl
                            };

                            if (root.TryGetProperty("assets", out var assetArray) && assetArray.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var asset in assetArray.EnumerateArray())
                                {
                                    string aName = asset.GetProperty("name").GetString() ?? "";
                                    string aUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                                    long aSize = asset.TryGetProperty("size", out var s) ? s.GetInt64() : 0;

                                    if (aName.EndsWith("-Setup.exe", StringComparison.OrdinalIgnoreCase) ||
                                        aName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                    {
                                        fallbackManifest.Packages["windows-setup"] = new PackageInfo
                                        {
                                            FileName = aName,
                                            Url = aUrl,
                                            Size = aSize
                                        };
                                    }
                                    else if (aName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
                                             aName.IndexOf("Windows", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        fallbackManifest.Packages["windows-portable"] = new PackageInfo
                                        {
                                            FileName = aName,
                                            Url = aUrl,
                                            Size = aSize
                                        };
                                    }
                                }
                            }

                            return JsonSerializer.Serialize(fallbackManifest);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("GitHub Releases API fallback hatası: " + ex.Message);
                return null;
            }
        }

        public static UpdateManifest ParseManifest(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<UpdateManifest>(json, options);
            }
            catch (Exception ex)
            {
                Logger.Log("Manifest JSON ayrıştırma hatası: " + ex.Message);
                return null;
            }
        }

        public static bool VerifyManifestSignature(UpdateManifest manifest)
        {
            if (manifest == null || string.IsNullOrWhiteSpace(manifest.Signature))
            {
                // Unsigned manifest (allowed with SHA-256 asset check, but flagged)
                return false;
            }

            try
            {
                string canonical = manifest.GetCanonicalString();
                byte[] data = Encoding.UTF8.GetBytes(canonical);
                byte[] sigBytes = Convert.FromBase64String(manifest.Signature);

                using (var rsa = RSA.Create())
                {
                    rsa.FromXmlString(RsaPublicKeyXml);
                    return rsa.VerifyData(data, sigBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("İmza doğrulama hatası: " + ex.Message);
                return false;
            }
        }

        public static string ComputeFileSha256(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;

            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = sha.ComputeHash(stream);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));
                }
                return sb.ToString();
            }
        }

        public async Task<string> DownloadUpdateAsync(
            UpdateCheckResult check,
            IProgress<double> progress,
            CancellationToken cancellationToken)
        {
            if (check == null || string.IsNullOrWhiteSpace(check.DownloadUrl))
                throw new ArgumentException("Geçersiz güncelleme indirme bilgisi.");

            string tempDir = Path.Combine(Path.GetTempPath(), "YouTubeVideoDownloader_Updates");
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            string targetFile = Path.Combine(tempDir, string.IsNullOrWhiteSpace(check.FileName) ? "UpdatePackage.bin" : check.FileName);
            string partFile = targetFile + ".part";

            if (File.Exists(partFile))
            {
                try { File.Delete(partFile); } catch { }
            }

            using (var response = await _httpClient.GetAsync(check.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength ?? (check.PackageSizeBytes > 0 ? check.PackageSizeBytes : (long?)null);

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(partFile, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                {
                    byte[] buffer = new byte[81920];
                    long totalRead = 0;
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        totalRead += bytesRead;

                        if (totalBytes.HasValue && totalBytes.Value > 0 && progress != null)
                        {
                            double pct = (double)totalRead / totalBytes.Value * 100.0;
                            progress.Report(Math.Min(100.0, pct));
                        }
                    }
                }
            }

            // Verify SHA-256 hash
            if (!string.IsNullOrWhiteSpace(check.ExpectedSha256))
            {
                string actualHash = ComputeFileSha256(partFile);
                if (!string.Equals(actualHash, check.ExpectedSha256.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Delete(partFile); } catch { }
                    throw new InvalidOperationException($"İndirilen dosyanın SHA-256 özeti eşleşmedi! Beklenen: {check.ExpectedSha256}, Hesaplanan: {actualHash}. Dosya silindi.");
                }
            }

            if (File.Exists(targetFile))
            {
                try { File.Delete(targetFile); } catch { }
            }

            File.Move(partFile, targetFile);
            Logger.Log($"Güncelleme başarıyla indirildi ve doğrulandı: {targetFile}");
            return targetFile;
        }

        public static void ApplyUpdate(string downloadedFilePath, InstallType installType)
        {
            if (string.IsNullOrEmpty(downloadedFilePath) || !File.Exists(downloadedFilePath))
                throw new FileNotFoundException("Güncelleme dosyası bulunamadı: " + downloadedFilePath);

            string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
            int currentPid = Process.GetCurrentProcess().Id;

            if (installType == InstallType.InnoSetupInstalled)
            {
                // Inno Setup silent update
                string args = $"/SP- /SILENT /SUPPRESSMSGBOXES /NORESTART /DIR=\"{appDir}\"";
                Logger.Log($"Inno Setup güncellemesi başlatılıyor: {downloadedFilePath} {args}");

                var psi = new ProcessStartInfo
                {
                    FileName = downloadedFilePath,
                    Arguments = args,
                    UseShellExecute = true
                };

                Process.Start(psi);
                Environment.Exit(0);
            }
            else
            {
                // Portable ZIP update helper
                string helperBatPath = Path.Combine(Path.GetTempPath(), $"vdw_update_{currentPid}.bat");
                string extractTemp = Path.Combine(Path.GetTempPath(), $"vdw_extract_{currentPid}");

                if (Directory.Exists(extractTemp))
                {
                    try { Directory.Delete(extractTemp, true); } catch { }
                }
                Directory.CreateDirectory(extractTemp);

                // Unpack zip using PowerShell
                string unzipCmd = $"Expand-Archive -Path '{downloadedFilePath}' -DestinationPath '{extractTemp}' -Force";
                var psPsi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{unzipCmd}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psPsi))
                {
                    p.WaitForExit();
                }

                // Batch helper waits for current process to exit, copies files, restarts, deletes itself
                string exeName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                string batContent = $@"@echo off
timeout /t 2 /nobreak >nul
:waitloop
tasklist /fi ""PID eq {currentPid}"" 2>nul | find ""{currentPid}"" >nul
if not errorlevel 1 (
    timeout /t 1 /nobreak >nul
    goto waitloop
)
xcopy ""{extractTemp}\*"" ""{appDir}\"" /E /Y /I /Q >nul
start """" ""{Path.Combine(appDir, exeName)}""
rd /s /q ""{extractTemp}"" >nul 2>nul
del ""%~f0"" >nul 2>nul
";
                File.WriteAllText(helperBatPath, batContent, Encoding.Default);

                var batPsi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{helperBatPath}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                Logger.Log($"Portable güncelleme yardımcısı başlatılıyor: {helperBatPath}");
                Process.Start(batPsi);
                Environment.Exit(0);
            }
        }
    }
}
