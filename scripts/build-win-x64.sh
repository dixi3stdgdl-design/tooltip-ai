#!/bin/bash
# ============================================================
# Tooltip AI — Build Script for Windows x64
# ============================================================
set -e

echo ""
echo "╔══════════════════════════════════════════════════════════╗"
echo "║       Tooltip AI — Windows x64 Build                    ║"
echo "║       Author: Octavio Garcia (MiMo Team)               ║"
echo "╚══════════════════════════════════════════════════════════╝"
echo ""

PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUTPUT_DIR="$PROJECT_ROOT/publish-win-x64"

echo "[1/6] Checking prerequisites..."
DOTNET_VERSION=$(dotnet --version)
echo "  .NET SDK: $DOTNET_VERSION"

echo "[2/6] Cleaning previous builds..."
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

echo "[3/6] Restoring NuGet packages..."
cd "$PROJECT_ROOT"
dotnet restore TooltipAI.Service/TooltipAI.Service.csproj

echo "[4/6] Building TooltipAI.Core (portable)..."
dotnet build TooltipAI.Core/TooltipAI.Core.csproj -c Release --no-restore

echo "[5/6] Building TooltipAI.Service (Windows x64)..."
dotnet publish TooltipAI.Service/TooltipAI.Service.csproj \
    -c Release \
    -r win-x64 \
    --self-contained \
    -p:PublishTrimmed=true \
    -p:PublishSingleFile=true \
    -o "$OUTPUT_DIR"

echo "[6/6] Generating integrity hashes..."
cd "$OUTPUT_DIR"
shasum -a 256 *.exe *.dll > hashes-sha256.txt 2>/dev/null || \
    sha256sum *.exe *.dll > hashes-sha256.txt 2>/dev/null || \
    echo "No binaries to hash"

echo ""
echo "BUILD COMPLETE: $OUTPUT_DIR"
echo ""
