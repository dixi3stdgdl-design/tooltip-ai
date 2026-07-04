#!/bin/bash
# ============================================================
# Tooltip AI — Build Script for macOS ARM64 (Apple Silicon)
# ============================================================
set -e

echo ""
echo "╔══════════════════════════════════════════════════════════╗"
echo "║       Tooltip AI — macOS ARM64 Build                    ║"
echo "║       Author: Octavio Garcia (MiMo Team)               ║"
echo "╚══════════════════════════════════════════════════════════╝"
echo ""

# ─── Configuration ──────────────────────────────────────────
PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUTPUT_DIR="$PROJECT_ROOT/publish-mac-arm64"
APP_NAME="Tooltip AI"
BUNDLE_ID="com.mimo.tooltipai"
VERSION="1.0.0"

# ─── Check prerequisites ───────────────────────────────────
echo "[1/8] Checking prerequisites..."

if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found. Install from https://dotnet.microsoft.com/download"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo "  .NET SDK: $DOTNET_VERSION"

if ! command -v dotnet &> /dev/null; then
    echo "ERROR: dotnet command not found"
    exit 1
fi

# ─── Clean previous builds ─────────────────────────────────
echo "[2/8] Cleaning previous builds..."
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# ─── Restore packages ──────────────────────────────────────
echo "[3/8] Restoring NuGet packages..."
cd "$PROJECT_ROOT"
dotnet restore TooltipAI.Platform.Mac/TooltipAI.Platform.Mac.csproj

# ─── Build Core (portable) ─────────────────────────────────
echo "[4/8] Building TooltipAI.Core (portable)..."
dotnet build TooltipAI.Core/TooltipAI.Core.csproj \
    -c Release \
    --no-restore

# ─── Build Platform.Mac ────────────────────────────────────
echo "[5/8] Building TooltipAI.Platform.Mac (ARM64)..."
dotnet publish TooltipAI.Platform.Mac/TooltipAI.Platform.Mac.csproj \
    -c Release \
    -r osx-arm64 \
    --self-contained \
    -p:PublishTrimmed=true \
    -p:PublishSingleFile=true \
    -p:AssemblyName=tooltipai \
    -o "$OUTPUT_DIR"

# ─── Create .app bundle ────────────────────────────────────
echo "[6/8] Creating macOS .app bundle..."
APP_BUNDLE="$OUTPUT_DIR/$APP_NAME.app"
mkdir -p "$APP_BUNDLE/Contents/MacOS"
mkdir -p "$APP_BUNDLE/Contents/Resources"

# Move binary to bundle
mv "$OUTPUT_DIR/tooltipai" "$APP_BUNDLE/Contents/MacOS/tooltipai"
chmod +x "$APP_BUNDLE/Contents/MacOS/tooltipai"

# Create Info.plist
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
    <key>CFBundleSignature</key>
    <string>????</string>
    <key>LSMinimumSystemVersion</key>
    <string>12.0</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSSupportsAutomaticTermination</key>
    <false/>
</dict>
</plist>
PLIST

# Create entitlements for Accessibility
cat > "$OUTPUT_DIR/tooltipai.entitlements" << ENTITLEMENTS
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>com.apple.security.app-sandbox</key>
    <false/>
    <key>com.apple.security.device.accessibility</key>
    <true/>
</dict>
</plist>
ENTITLEMENTS

# ─── Create DMG ────────────────────────────────────────────
echo "[7/8] Creating DMG installer..."
DMG_NAME="TooltipAI-mac-arm64.dmg"
hdiutil create -volname "$APP_NAME" \
    -srcfolder "$APP_BUNDLE" \
    -ov -format UDZO \
    "$OUTPUT_DIR/$DMG_NAME"

# ─── Generate SHA-256 hashes ───────────────────────────────
echo "[8/8] Generating integrity hashes..."
cd "$OUTPUT_DIR"
shasum -a 256 "$DMG_NAME" > "$DMG_NAME.sha256"
shasum -a 256 "$APP_BUNDLE/Contents/MacOS/tooltipai" > "tooltipai-arm64.sha256"

echo ""
echo "╔══════════════════════════════════════════════════════════╗"
echo "║  BUILD COMPLETE                                         ║"
echo "╠══════════════════════════════════════════════════════════╣"
echo "║  App Bundle: $APP_BUNDLE"
echo "║  DMG:        $OUTPUT_DIR/$DMG_NAME"
echo "║  Hashes:     $OUTPUT_DIR/*.sha256"
echo "║                                                          ║"
echo "║  Next steps:                                             ║"
echo "║  1. Code sign: codesign --sign \"Developer ID\" $APP_BUNDLE"
echo "║  2. Notarize:  xcrun notarytool submit $OUTPUT_DIR/$DMG_NAME"
echo "║  3. Staple:    xcrun stapler staple $OUTPUT_DIR/$DMG_NAME"
echo "╚══════════════════════════════════════════════════════════╝"
echo ""
