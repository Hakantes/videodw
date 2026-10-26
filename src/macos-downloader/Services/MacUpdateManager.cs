using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using YouTubeDownloader.Mac.Models;

namespace YouTubeDownloader.Mac.Services
{
    public class MacPackageInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Sha256 { get; set; } = string.Empty;
    }

    public class MacUpdateManifest
    {
        public string Version { get; set; } = string.Empty;
        public string ReleaseDate { get; set; } = string.Empty;
        public bool Mandatory { get; set; }
        public string ChangelogUrl { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public Dictionary<string, MacPackageInfo> Packages { get; set; } = new Dictionary<string, MacPackageInfo>(StringComparer.OrdinalIgnoreCase);
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

    public class MacUpdateCheckResult
    {
        public bool IsUpdateAvailable { get; set; }
        public Version CurrentVersion { get; set; } = new Version(2, 1, 0);
        public Version NewVersion { get; set; } = new Version(2, 1, 0);
        public string ReleaseNotes { get; set; } = string.Empty;
        public string ChangelogUrl { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long PackageSizeBytes { get; set; }
        public string ExpectedSha256 { get; set; } = string.Empty;
        public bool IsSignatureVerified { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public bool IsError { get; set; }
    }

    public class MacUpdateManager
    {
        public const string DefaultManifestUrl = "https://raw.githubusercontent.com/Hakantes/videodw/master/update-manifest.json";
        public const string FallbackReleasesApiUrl = "https://api.github.com/repos/Hakantes/videodw/releases/latest";

        public const string RsaPublicKeyXml =
            "<RSAKeyValue><Modulus>v+2DmdXu1p8KzYYB5dd3KysfwISKSn3sNe1jgEg0TeFj0GqDkdaAFQXlUyFpsXuK6OqNas3npBTrvYD8gFOd2M1ahu8yOSD8iUVk5dG6KNiAxTLkaN7vjox5yTAiz1TRVS66k+JyXgU8VzzVEWpJZJ4M+NtpaF7fXtasIfNwzXIofJjCa61qkuUM6ojkXuPtjJ9LdUkqn57MX4x+9Nd4lrS/HVbXWVb2Rm1uq7WXYsBLu6KMpCRd35cf9LoyOF1GUj6Q/PN9uVt5Rfn/HOGnvYuwiXa+YDpX0KL1taPMF5UAvUiaaYyfceuMCwP6sTiVj+4GIkvkdEzEGQFtWYeOTQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        private static readonly HttpClient _httpClient = new HttpClient();

        static MacUpdateManager()
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "YouTubeVideoDownloader-Mac/2.1.0");
            }
        }

        public Version CurrentVersion { get; }

        public MacUpdateManager(Version? currentVersion = null)
        {
            CurrentVersion = currentVersion ?? new Version(2, 1, 0);
        }

        public async Task<MacUpdateCheckResult> CheckForUpdatesAsync(
            bool isManual = false,
            string manifestUrl = DefaultManifestUrl,
            CancellationToken ct = default)
        {
            var result = new MacUpdateCheckResult { CurrentVersion = CurrentVersion };

            try
            {
                string? json = null;
                try
                {
                    json = await _httpClient.GetStringAsync(manifestUrl, ct);
                }
                catch { }

                if (string.IsNullOrWhiteSpace(json))
                {
                    json = await FetchManifestFromGitHubReleasesAsync(FallbackReleasesApiUrl, ct);
                }

                if (string.IsNullOrWhiteSpace(json))
                {
                    result.IsError = true;
                    result.StatusMessage = "Güncelleme sunucusuna ulaşılamadı.";
                    return result;
                }

                var manifest = ParseManifest(json);
                if (manifest == null || string.IsNullOrWhiteSpace(manifest.Version))
                {
                    result.IsError = true;
                    result.StatusMessage = "Güncelleme manifesti geçersiz.";
                    return result;
                }

                result.IsSignatureVerified = VerifySignature(manifest);

                if (!result.IsSignatureVerified)
                {
                    result.IsError = true;
                    result.IsUpdateAvailable = false;
                    result.StatusMessage = "Güvenlik Hatası: Güncelleme manifestinin dijital imzası geçersiz veya bozulmuş!";
                    return result;
                }

                if (!Version.TryParse(manifest.Version, out var remoteVer))
                {
                    result.IsError = true;
                    result.StatusMessage = "Uzak sürüm numarası okunamadı: " + manifest.Version;
                    return result;
                }

                result.NewVersion = remoteVer;
                result.ReleaseNotes = manifest.ReleaseNotes;
                result.ChangelogUrl = manifest.ChangelogUrl;

                if (remoteVer <= CurrentVersion)
                {
                    result.IsUpdateAvailable = false;
                    result.StatusMessage = $"En güncel sürümü kullanıyorsunuz (v{CurrentVersion}).";
                    return result;
                }

                // Look strictly for macos-arm64 or macos-universal package
                MacPackageInfo? pkg = null;
                if (manifest.Packages.TryGetValue("macos-arm64", out var p1) && p1 != null && !string.IsNullOrWhiteSpace(p1.Url))
                {
                    pkg = p1;
                }
                else if (manifest.Packages.TryGetValue("macos-universal", out var p2) && p2 != null && !string.IsNullOrWhiteSpace(p2.Url))
                {
                    pkg = p2;
                }

                if (pkg == null || string.IsNullOrWhiteSpace(pkg.Url))
                {
                    result.IsError = true;
                    result.StatusMessage = "macOS Apple Silicon için uygun güncelleme paketi bulunamadı.";
                    return result;
                }

                // Strict sanity check: ensure macOS never attempts to download Windows packages
                if (pkg.FileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                    pkg.FileName.IndexOf("windows", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result.IsError = true;
                    result.StatusMessage = "Platform uyuşmazlığı hatası: macOS için geçersiz paket tespit edildi.";
                    return result;
                }

                result.IsUpdateAvailable = true;
                result.DownloadUrl = pkg.Url;
                result.FileName = pkg.FileName;
                result.PackageSizeBytes = pkg.Size;
                result.ExpectedSha256 = pkg.Sha256;
                result.StatusMessage = $"Yeni sürüm mevcut: v{remoteVer}";

                return result;
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.StatusMessage = "Güncelleme denetlenirken hata oluştu: " + ex.Message;
                return result;
            }
        }

        private static bool VerifySignature(MacUpdateManifest manifest)
        {
            if (string.IsNullOrWhiteSpace(manifest.Signature)) return false;

            try
            {
                string canonical = manifest.GetCanonicalString();
                byte[] data = Encoding.UTF8.GetBytes(canonical);
                byte[] signatureBytes = Convert.FromBase64String(manifest.Signature);

                using var rsa = RSA.Create();
                rsa.FromXmlString(RsaPublicKeyXml);
                return rsa.VerifyData(data, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            catch
            {
                return false;
            }
        }

        private static MacUpdateManifest? ParseManifest(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var manifest = new MacUpdateManifest
                {
                    Version = root.TryGetProperty("version", out var v) ? v.GetString() ?? "" : "",
                    ReleaseDate = root.TryGetProperty("releaseDate", out var rd) ? rd.GetString() ?? "" : "",
                    Mandatory = root.TryGetProperty("mandatory", out var m) && m.GetBoolean(),
                    ChangelogUrl = root.TryGetProperty("changelogUrl", out var cl) ? cl.GetString() ?? "" : "",
                    ReleaseNotes = root.TryGetProperty("releaseNotes", out var rn) ? rn.GetString() ?? "" : "",
                    Signature = root.TryGetProperty("signature", out var sig) ? sig.GetString() ?? "" : ""
                };

                if (root.TryGetProperty("packages", out var pkgs) && pkgs.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in pkgs.EnumerateObject())
                    {
                        var pEl = prop.Value;
                        manifest.Packages[prop.Name] = new MacPackageInfo
                        {
                            FileName = pEl.TryGetProperty("fileName", out var fn) ? fn.GetString() ?? "" : "",
                            Url = pEl.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "",
                            Size = pEl.TryGetProperty("size", out var s) && s.TryGetInt64(out long sz) ? sz : 0,
                            Sha256 = pEl.TryGetProperty("sha256", out var sh) ? sh.GetString() ?? "" : ""
                        };
                    }
                }

                return manifest;
            }
            catch
            {
                return null;
            }
        }

        private static async Task<string?> FetchManifestFromGitHubReleasesAsync(string apiUrl, CancellationToken ct)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                req.Headers.Accept.ParseAdd("application/vnd.github+json");
                using var resp = await _httpClient.SendAsync(req, ct);
                if (!resp.IsSuccessStatusCode) return null;

                string body = await resp.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (root.TryGetProperty("assets", out var assets) && assets.ValueKind == JsonValueKind.Array)
                {
                    foreach (var asset in assets.EnumerateArray())
                    {
                        string name = asset.GetProperty("name").GetString() ?? "";
                        if (string.Equals(name, "update-manifest.json", StringComparison.OrdinalIgnoreCase))
                        {
                            string downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                            return await _httpClient.GetStringAsync(downloadUrl, ct);
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        public async Task<string> DownloadUpdateAsync(
            MacUpdateCheckResult check,
            IProgress<double>? progress,
            CancellationToken ct)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "YouTubeVideoDownloader_Mac_Updates");
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            string targetFile = Path.Combine(tempDir, string.IsNullOrWhiteSpace(check.FileName) ? "update.zip" : check.FileName);
            string partFile = targetFile + ".part";

            if (File.Exists(partFile))
            {
                try { File.Delete(partFile); } catch { }
            }

            using (var resp = await _httpClient.GetAsync(check.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, ct))
            {
                resp.EnsureSuccessStatusCode();
                long? total = resp.Content.Headers.ContentLength ?? (check.PackageSizeBytes > 0 ? check.PackageSizeBytes : null);

                using var contentStream = await resp.Content.ReadAsStreamAsync(ct);
                using var fileStream = new FileStream(partFile, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

                byte[] buffer = new byte[81920];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                    totalRead += bytesRead;

                    if (total.HasValue && total.Value > 0 && progress != null)
                    {
                        double pct = (double)totalRead / total.Value * 100.0;
                        progress.Report(Math.Min(100.0, pct));
                    }
                }
            }

            // Verify SHA-256
            if (!string.IsNullOrWhiteSpace(check.ExpectedSha256))
            {
                string hash = ComputeFileSha256(partFile);
                if (!string.Equals(hash, check.ExpectedSha256.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    try { File.Delete(partFile); } catch { }
                    throw new InvalidOperationException($"SHA-256 doğrulaması başarısız! Beklenen: {check.ExpectedSha256}, Hesaplanan: {hash}");
                }
            }

            if (File.Exists(targetFile))
            {
                try { File.Delete(targetFile); } catch { }
            }

            File.Move(partFile, targetFile);
            return targetFile;
        }

        public static string ComputeFileSha256(string filePath)
        {
            using var sha = SHA256.Create();
            using var fs = File.OpenRead(filePath);
            byte[] hash = sha.ComputeHash(fs);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public static void ApplyUpdateMac(string downloadedPackagePath)
        {
            // On macOS: unzips or mounts DMG and swaps YouTube Downloader.app
            string appBundleDir = AppDomain.CurrentDomain.BaseDirectory;
            // Traverse up to find .app directory
            string appRoot = appBundleDir;
            while (!string.IsNullOrEmpty(appRoot) && !appRoot.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
            {
                string? parent = Path.GetDirectoryName(appRoot);
                if (parent == null || parent == appRoot) break;
                appRoot = parent;
            }

            int pid = Environment.ProcessId;
            string tempDir = Path.GetTempPath();
            string scriptPath = Path.Combine(tempDir, "apply_update_macos.sh");

            var scriptContent = new StringBuilder();
            scriptContent.AppendLine("#!/bin/bash");
            scriptContent.AppendLine($"echo 'Uygulama PID {pid} bekleniyor...'");
            scriptContent.AppendLine($"while kill -0 {pid} 2>/dev/null; do sleep 0.5; done");
            scriptContent.AppendLine("sleep 1");

            if (downloadedPackagePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                string extractDir = Path.Combine(tempDir, "extracted_update");
                scriptContent.AppendLine($"rm -rf \"{extractDir}\"");
                scriptContent.AppendLine($"mkdir -p \"{extractDir}\"");
                scriptContent.AppendLine($"unzip -q -o \"{downloadedPackagePath}\" -d \"{extractDir}\"");

                if (appRoot.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
                {
                    string appName = Path.GetFileName(appRoot);
                    string targetParent = Path.GetDirectoryName(appRoot) ?? "/Applications";
                    scriptContent.AppendLine($"rm -rf \"{appRoot}\"");
                    scriptContent.AppendLine($"cp -R \"{extractDir}/{appName}\" \"{targetParent}/\" || cp -R \"{extractDir}\"/*.app \"{targetParent}/\"");
                    scriptContent.AppendLine($"xattr -cr \"{appRoot}\" 2>/dev/null || true");
                    scriptContent.AppendLine($"open \"{appRoot}\"");
                }
            }
            else if (downloadedPackagePath.EndsWith(".dmg", StringComparison.OrdinalIgnoreCase))
            {
                scriptContent.AppendLine($"open \"{downloadedPackagePath}\"");
            }

            File.WriteAllText(scriptPath, scriptContent.ToString().Replace("\r\n", "\n"));

            var psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                ArgumentList = { scriptPath },
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(psi);
            Environment.Exit(0);
        }
    }
}
