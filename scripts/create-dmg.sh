#!/usr/bin/env bash
set -euo pipefail

# ==============================================================================
# YouTube Downloader — macOS Kolay Kurulum DMG Paketleyici
# Bu betik macOS üzerinde 'hdiutil' kullanarak Uygulamalar (/Applications)
# sürükle-bırak kısayoluna sahip sıkıştırılmış (UDZO) DMG paketi üretir.
# ==============================================================================

VERSION="${1:-2.1.0}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
APP_PATH="${ROOT_DIR}/artifacts/YouTube Downloader.app"
DMG_OUTPUT="${ROOT_DIR}/artifacts/YouTubeDownloader_v${VERSION}_macOS_ARM64.dmg"
STAGING_DIR="${ROOT_DIR}/artifacts/dmg_temp"

echo "=== YouTube Downloader DMG Paketleyici (v${VERSION}) ==="

# 1. Ortam Kontrolü
if ! command -v hdiutil &>/dev/null; then
    echo "UYARI: 'hdiutil' komutu yalnızca Apple macOS üzerinde yerel olarak çalışır."
    echo "Windows üzerinde gerçek Apple APFS/HFS+ DMG diski üretilemez."
    echo "Lütfen bu betiği bir Mac bilgisayarda veya GitHub Actions macos runner üzerinde çalıştırın."
    echo "Mevcut doğrulanmış ZIP paketi: ${ROOT_DIR}/artifacts/YouTubeDownloader_v${VERSION}_macOS_ARM64.zip"
    exit 1
fi

# 2. .app Bundle Kontrolü
if [ ! -d "${APP_PATH}" ]; then
    echo "Hata: '${APP_PATH}' bulunamadı!"
    echo "Lütfen önce './scripts/build-macos-app.sh ${VERSION}' çalıştırarak uygulamayı derleyin."
    exit 1
fi

# 3. Geçici Sahneleme Klasörü ve Applications Kısayolu
rm -rf "${STAGING_DIR}" "${DMG_OUTPUT}"
mkdir -p "${STAGING_DIR}"

echo "[1/3] Uygulama sahneleme dizinine kopyalanıyor..."
cp -R "${APP_PATH}" "${STAGING_DIR}/"

echo "[2/3] Sürükle-bırak için /Applications sembolik bağı oluşturuluyor..."
ln -s /Applications "${STAGING_DIR}/Applications"

# 4. DMG Oluşturma (UDZO: zlib sıkıştırmalı salt okunur disk imajı)
echo "[3/3] 'hdiutil' ile sıkıştırılmış DMG imajı üretiliyor..."
hdiutil create -volname "YouTube Downloader" \
               -srcfolder "${STAGING_DIR}" \
               -ov -format UDZO \
               "${DMG_OUTPUT}"

rm -rf "${STAGING_DIR}"

echo ""
echo "======================================================="
echo " macOS Apple Silicon DMG Paketi Başarıyla Oluşturuldu!"
echo "======================================================="
echo "Dosya:   ${DMG_OUTPUT}"
echo "Boyut:   $(du -h "${DMG_OUTPUT}" | cut -f1)"
echo -n "SHA-256: "
shasum -a 256 "${DMG_OUTPUT}" | cut -d' ' -f1
echo "======================================================="
