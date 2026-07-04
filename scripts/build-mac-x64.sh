#!/bin/bash
# ============================================================
# Tooltip AI — Build Script for macOS x64 (Intel)
# ============================================================
set -e

echo ""
echo "╔══════════════════════════════════════════════════════════╗"
echo "║       Tooltip AI — macOS x64 Build                      ║"
echo "║       Author: Octavio Garcia (MiMo Team)               ║"
echo "╚══════════════════════════════════════════════════════════╝"
echo ""

PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUTPUT_DIR="$PROJECT_ROOT/publish-mac-x64"
APP_NAME="Tooltip AI"
BUNDLE_ID="com.mimo.tooltipai"
VERSION="1.0.0"

echo "[1/8] Checking prerequisites..."
DOTNET_VERSION=$(dotnet --version)
echo "  .NET SDK: $DOTNET_VERSION"

echo "[2/8] Cleaning previous builds..."
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

echo "[3/8] Restoring NuGet packages..."
cd "$PROJECT_ROOT"
dotnet restore TooltipAI.Platform.Mac/TooltipAI.Platform.Mac.csproj

echo "[4/8] Building TooltipAI.Core (portable)..."
dotnet build TooltipAI.Core/TooltipAI.Core.csproj -c Release --no-restore

echo "[5/8] Building TooltipAI.Platform.Mac (x64)..."
dotnet publish TooltipAI.Platform.Mac/TooltipAI.Platform.Mac.csproj \
    -c Release \
    -r osx-x64 \
    --self-contained \
    -p:PublishTrimmed=true \
    -p:PublishSingleFile=true \
    -p:AssemblyName=tooltipai \
    -o "$OUTPUT_DIR"

echo "[6/8] Creating macOS .app bundle..."
APP_BUNDLE="$OUTPUT_DIR/$APP_NAME.app"
mkdir -p "$APP_BUNDLE/Contents/MacOS"
mkdir -p "$APP_BUNDLE/Contents/Resources"
mv "$OUTPUT_DIR/tooltipai" "$APP_BUNDLE/Contents/MacOS/tooltipai"
chmod +x "$APP_BUNDLE/Contents/MacOS/tooltipai"

cat > "$APP_BUNDLE/Contents/Info.plist" << PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>tooltipai</string>
    <key>CFBundleIdentifier</key>
    <string>$BUNDLE_ID</string>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleDisplayName</key>
    <string>Tooltip AI</string>
    <key>CFBundleVersion</key>
    <string>$VERSION</string>
    <key>CFBundleShortVersionString</key>
    <string>$VERSION</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>LSMinimumSystemVersion</key>
    <string>12.0</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
PLIST

echo "[7/8] Creating DMG installer..."
hdiutil create -volname "$APP_NAME" \
    -srcfolder "$APP_BUNDLE" \
    -ov -format UDZO \
    "$OUTPUT_DIR/TooltipAI-mac-x64.dmg"

echo "[8/8] Generating integrity hashes..."
cd "$OUTPUT_DIR"
shasum -a 256 TooltipAI-mac-x64.dmg > TooltipAI-mac-x64.dmg.sha256

echo ""
echo "BUILD COMPLETE: $OUTPUT_DIR"
echo "DMG: $OUTPUT_DIR/TooltipAI-mac-x64.dmg"
echo ""
