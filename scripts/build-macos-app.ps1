param(
    [string]$Version = "2.1.0",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
Write-Host "=== YouTube Downloader macOS Apple Silicon Paketleyici ===" -ForegroundColor Cyan
Write-Host "Sürüm: $Version | Yapılandırma: $Configuration" -ForegroundColor Gray

$rootDir = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $rootDir "src\macos-downloader\YouTubeDownloader.Mac.csproj"
$publishDir = Join-Path $rootDir "artifacts\publish\osx-arm64"
$appBundleDir = Join-Path $rootDir "artifacts\YouTube Downloader.app"
$zipOutput = Join-Path $rootDir "artifacts\YouTubeDownloader_v${Version}_macOS_ARM64.zip"

# 1. dotnet publish osx-arm64
Write-Host "`n[1/4] .NET 8 Apple Silicon (osx-arm64) Self-Contained derleniyor..." -ForegroundColor Yellow
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
dotnet publish $projectPath -r osx-arm64 -c $Configuration --self-contained true /p:PublishSingleFile=true -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish başarısız oldu!"
    exit 1
}

# 2. .app Bundle Hiyerarşisi Oluşturma
Write-Host "`n[2/4] 'YouTube Downloader.app' bundle klasör yapısı oluşturuluyor..." -ForegroundColor Yellow
if (Test-Path $appBundleDir) { Remove-Item $appBundleDir -Recurse -Force }

$contentsDir = Join-Path $appBundleDir "Contents"
$macOsDir = Join-Path $contentsDir "MacOS"
$resourcesDir = Join-Path $contentsDir "Resources"

New-Item -ItemType Directory -Path $macOsDir -Force | Out-Null
New-Item -ItemType Directory -Path $resourcesDir -Force | Out-Null

# Info.plist kopyala
$plistSource = Join-Path $rootDir "src\macos-downloader\Info.plist"
Copy-Item $plistSource (Join-Path $contentsDir "Info.plist") -Force

# İkili dosyaları MacOS klasörüne kopyala
Get-ChildItem -Path $publishDir | ForEach-Object {
    Copy-Item $_.FullName (Join-Path $macOsDir $_.Name) -Force -Recurse
}

# 3. ZIP Arşivi Oluşturma
Write-Host "`n[3/4] ZIP paketi sıkıştırılıyor..." -ForegroundColor Yellow
if (Test-Path $zipOutput) { Remove-Item $zipOutput -Force }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($appBundleDir, $zipOutput, [System.IO.Compression.CompressionLevel]::Optimal, $true)

# 4. Hash ve Boyut Raporu
Write-Host "`n[4/4] Paket özeti ve SHA-256 hesaplanıyor..." -ForegroundColor Yellow
$fileItem = Get-Item $zipOutput
$hash = (Get-FileHash -Path $zipOutput -Algorithm SHA256).Hash.ToLowerInvariant()
$sizeMb = [Math]::Round($fileItem.Length / (1024 * 1024), 2)

Write-Host "`n=======================================================" -ForegroundColor Green
Write-Host " macOS Apple Silicon Paketi Başarıyla Hazırlandı!" -ForegroundColor Green
Write-Host "=======================================================" -ForegroundColor Green
Write-Host "Dosya Adı:   $($fileItem.Name)"
Write-Host "Dosya Yolu:  $($fileItem.FullName)"
Write-Host "Boyut:       $sizeMb MB ($($fileItem.Length) bayt)"
Write-Host "SHA-256:     $hash"
Write-Host "`nmacOS DMG Oluşturma Komutu (Mac Terminal):" -ForegroundColor Cyan
Write-Host "  hdiutil create -volname ""YouTube Downloader"" -srcfolder ""YouTube Downloader.app"" -ov -format UDZO ""YouTubeDownloader_v${Version}_macOS_ARM64.dmg""" -ForegroundColor Gray
Write-Host "`nGatekeeper İzni (Mac Terminal):" -ForegroundColor Cyan
Write-Host "  xattr -cr ""/Applications/YouTube Downloader.app""" -ForegroundColor Gray
