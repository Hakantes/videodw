param(
    [string]$ManifestPath = "update-manifest.json",
    [string]$PrivateKeyFile = "",
    [string]$PrivateKeyXml = ""
)

$ErrorActionPreference = "Stop"

# 1. RSA Özel Anahtarı Güvenli Biçimde Çözümle
# Sırasıyla: Parametre -> Ortam Değişkeni -> Parametre Dosyası -> Ortam Değişkeni Dosyası -> Yerel Gizli Anahtar
if ([string]::IsNullOrWhiteSpace($PrivateKeyXml)) {
    if (-not [string]::IsNullOrWhiteSpace($env:UPDATE_SIGNING_PRIVATE_KEY)) {
        $PrivateKeyXml = $env:UPDATE_SIGNING_PRIVATE_KEY
    } elseif (-not [string]::IsNullOrWhiteSpace($PrivateKeyFile) -and (Test-Path $PrivateKeyFile)) {
        $PrivateKeyXml = Get-Content -Path $PrivateKeyFile -Raw -Encoding UTF8
    } elseif (-not [string]::IsNullOrWhiteSpace($env:UPDATE_SIGNING_PRIVATE_KEY_PATH) -and (Test-Path $env:UPDATE_SIGNING_PRIVATE_KEY_PATH)) {
        $PrivateKeyXml = Get-Content -Path $env:UPDATE_SIGNING_PRIVATE_KEY_PATH -Raw -Encoding UTF8
    } else {
        $rootDir = Split-Path -Parent $PSScriptRoot
        $defaultLocalKey = Join-Path $rootDir "keys\update-signing-private.key"
        if (Test-Path $defaultLocalKey) {
            $PrivateKeyXml = Get-Content -Path $defaultLocalKey -Raw -Encoding UTF8
        }
    }
}

if ([string]::IsNullOrWhiteSpace($PrivateKeyXml)) {
    Write-Error "GÜVENLİK HATASI: Manifesti imzalamak için RSA özel anahtarı (private key) bulunamadı! Lütfen -PrivateKeyFile parametresi, UPDATE_SIGNING_PRIVATE_KEY ortam değişkeni veya git-ignore edilmiş 'keys/update-signing-private.key' dosyası ile anahtarı sağlayın. Özel anahtarlar açık depoda veya dağıtım paketlerinde ASLA saklanmamalıdır."
    exit 1
}

if (-not (Test-Path $ManifestPath)) {
    Write-Error "Manifest dosyası bulunamadı: $ManifestPath"
    exit 1
}

$jsonText = Get-Content -Path $ManifestPath -Raw -Encoding UTF8
$manifest = $jsonText | ConvertFrom-Json

# 2. Kanonik Dize (Canonical String) Oluşturma
# Format: Version|ReleaseDate|ChangelogUrl|pkg1:file:size:sha256;pkg2:...
$sb = New-Object System.Text.StringBuilder
[void]$sb.Append($manifest.version).Append("|")
[void]$sb.Append($manifest.releaseDate).Append("|")
[void]$sb.Append($manifest.changelogUrl).Append("|")

$pkgNames = @($manifest.packages.PSObject.Properties | Select-Object -ExpandProperty Name | Sort-Object)
foreach ($name in $pkgNames) {
    $pkg = $manifest.packages.$name
    [void]$sb.Append($name).Append(":")
    [void]$sb.Append($pkg.fileName).Append(":")
    [void]$sb.Append($pkg.size).Append(":")
    [void]$sb.Append($pkg.sha256).Append(";")
}

$canonicalString = $sb.ToString()
Write-Host "Kanonik Dize: $canonicalString" -ForegroundColor Cyan

# 3. RSA-2048 SHA-256 ile İmzala
$rsa = [System.Security.Cryptography.RSA]::Create()
$rsa.FromXmlString($PrivateKeyXml.Trim())
$dataBytes = [System.Text.Encoding]::UTF8.GetBytes($canonicalString)
$sigBytes = $rsa.SignData($dataBytes, [System.Security.Cryptography.HashAlgorithmName]::SHA256, [System.Security.Cryptography.RSASignaturePadding]::Pkcs1)
$sigBase64 = [Convert]::ToBase64String($sigBytes)

# 4. Genel Anahtarla Doğruluk Kontrolü
$pubRsa = [System.Security.Cryptography.RSA]::Create()
$pubRsa.FromXmlString($rsa.ToXmlString($false))
$verified = $pubRsa.VerifyData($dataBytes, $sigBytes, [System.Security.Cryptography.HashAlgorithmName]::SHA256, [System.Security.Cryptography.RSASignaturePadding]::Pkcs1)

if (-not $verified) {
    Write-Error "Kritik Hata: İmzalanan veri genel anahtarla doğrulanamadı!"
    exit 1
}

Write-Host "Dijital İmza Başarıyla Oluşturuldu ve Doğrulandı!" -ForegroundColor Green

# 5. Manifest JSON Dosyasını Güncelle
$manifest | Add-Member -MemberType NoteProperty -Name "signature" -Value $sigBase64 -Force
$updatedJson = $manifest | ConvertTo-Json -Depth 10
[System.IO.File]::WriteAllText($ManifestPath, $updatedJson, [System.Text.Encoding]::UTF8)

Write-Host "Manifest başarıyla mühürlendi: $ManifestPath" -ForegroundColor Green
