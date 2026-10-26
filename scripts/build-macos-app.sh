#!/usr/bin/env bash
set -euo pipefail

VERSION="${1:-2.1.0}"
CONFIGURATION="${2:-Release}"

echo "=== YouTube Downloader macOS Apple Silicon Native Builder ==="
echo "Version: ${VERSION} | Configuration: ${CONFIGURATION}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
PROJECT_PATH="${ROOT_DIR}/src/macos-downloader/YouTubeDownloader.Mac.csproj"
PUBLISH_DIR="${ROOT_DIR}/artifacts/publish/osx-arm64"
APP_DIR="${ROOT_DIR}/artifacts/YouTube Downloader.app"
ZIP_OUTPUT="${ROOT_DIR}/artifacts/YouTubeDownloader_v${VERSION}_macOS_ARM64.zip"
DMG_OUTPUT="${ROOT_DIR}/artifacts/YouTubeDownloader_v${VERSION}_macOS_ARM64.dmg"

# 1. Publish osx-arm64
echo "[1/4] Publishing .NET 8 osx-arm64 self-contained single-file..."
rm -rf "${PUBLISH_DIR}"
dotnet publish "${PROJECT_PATH}" -r osx-arm64 -c "${CONFIGURATION}" --self-contained true /p:PublishSingleFile=true -o "${PUBLISH_DIR}"

# 2. Build .app Bundle
echo "[2/4] Constructing .app bundle structure..."
rm -rf "${APP_DIR}"
mkdir -p "${APP_DIR}/Contents/MacOS"
mkdir -p "${APP_DIR}/Contents/Resources"

cp "${ROOT_DIR}/src/macos-downloader/Info.plist" "${APP_DIR}/Contents/Info.plist"
cp -R "${PUBLISH_DIR}/"* "${APP_DIR}/Contents/MacOS/"
chmod +x "${APP_DIR}/Contents/MacOS/YouTubeDownloader.Mac"

# Ad-hoc codesign on macOS if codesign tool is present
if command -v codesign &>/dev/null; then
    echo "Applying ad-hoc signature (no Apple Developer ID needed)..."
    codesign --force --deep --sign - "${APP_DIR}" || true
fi

# 3. Create ZIP archive
echo "[3/4] Creating ZIP package..."
rm -f "${ZIP_OUTPUT}"
cd "${ROOT_DIR}/artifacts"
zip -r -q "${ZIP_OUTPUT}" "YouTube Downloader.app"

# 4. Optional DMG creation on macOS
if command -v hdiutil &>/dev/null; then
    echo "Creating DMG image..."
    rm -f "${DMG_OUTPUT}"
    hdiutil create -volname "YouTube Downloader" -srcfolder "${APP_DIR}" -ov -format UDZO "${DMG_OUTPUT}"
    shasum -a 256 "${DMG_OUTPUT}"
fi

shasum -a 256 "${ZIP_OUTPUT}"
echo "Build completed successfully!"
