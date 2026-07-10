#!/bin/bash
# ============================================================
# Tooltip AI — Windows MSI Installer Build
# ============================================================
# Genera el paquete .msi para Microsoft Store
#
# Uso: ./build-msi.sh [--publish-dir PATH]
# ============================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
PUBLISH_DIR="${1:-$ROOT_DIR/publish-win-x64}"
INSTALLER_DIR="$ROOT_DIR/installer"
OUTPUT_DIR="$ROOT_DIR/dist"

# Colores
GREEN='\033[0;32m'
CYAN='\033[0;36m'
NC='\033[0m'

log() { echo -e "${CYAN}[$(date +%H:%M:%S)]${NC} $1"; }
ok() { echo -e "${GREEN}[OK]${NC} $1"; }

log "=== Tooltip AI MSI Build ==="

# Verificar que WiX está instalado
if ! command -v wix &> /dev/null; then
    log "Instalando WiX Toolset..."
    dotnet tool install --global wix
fi

# Verificar que existe el directorio de publicación
if [ ! -d "$PUBLISH_DIR" ]; then
    log "Directorio de publicación no encontrado: $PUBLISH_DIR"
    log "Ejecuta primero: dotnet publish -c Release -r win-x64 --self-contained -o $PUBLISH_DIR"
    exit 1
fi

# Crear directorio de salida
mkdir -p "$OUTPUT_DIR"

# Compilar MSI
log "Compilando MSI..."
cd "$INSTALLER_DIR"

wix build TooltipAI.wxs \
    -d PublishDir="$PUBLISH_DIR" \
    -o "$OUTPUT_DIR/TooltipAI-v1.0.0-win-x64.msi" \
    2>&1

ok "MSI generado: $OUTPUT_DIR/TooltipAI-v1.0.0-win-x64.msi"

# Generar hash SHA-256
log "Generando hash SHA-256..."
sha256sum "$OUTPUT_DIR/TooltipAI-v1.0.0-win-x64.msi" > "$OUTPUT_DIR/TooltipAI-v1.0.0-win-x64.msi.sha256"
ok "Hash generado"

echo ""
echo "╔══════════════════════════════════════════════════════════╗"
echo "║       MSI Build Completado                              ║"
echo "╠══════════════════════════════════════════════════════════╣"
echo "║  MSI:    $OUTPUT_DIR/TooltipAI-v1.0.0-win-x64.msi"
echo "║  Hash:   $OUTPUT_DIR/TooltipAI-v1.0.0-win-x64.msi.sha256"
echo "╚══════════════════════════════════════════════════════════╝"
