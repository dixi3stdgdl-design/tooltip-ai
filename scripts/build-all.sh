#!/bin/bash
# ============================================================
# Tooltip AI — Build All Platforms
# ============================================================
# Compila para todas las plataformas con ofuscación
#
# Uso: ./build-all.sh [--obfuscate]
# ============================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
DIST_DIR="$ROOT_DIR/dist"

# Colores
GREEN='\033[0;32m'
CYAN='\033[0;36m'
NC='\033[0m'

log() { echo -e "${CYAN}[$(date +%H:%M:%S)]${NC} $1"; }
ok() { echo -e "${GREEN}[OK]${NC} $1"; }

# Parsear argumentos
OBFUSCATE=false
for arg in "$@"; do
    [ "$arg" = "--obfuscate" ] && OBFUSCATE=true
done

log "=== Tooltip AI Build All ==="
log "Obfuscation: $OBFUSCATE"

# Limpiar directorio de distribución
rm -rf "$DIST_DIR"
mkdir -p "$DIST_DIR"/{linux-x64,win-x64,osx-x64}

# ═══════════════════════════════════════════════════════════
# PASO 1: Compilar para todas las plataformas
# ═══════════════════════════════════════════════════════════
log "Compilando para Linux x64..."
dotnet publish "$ROOT_DIR/TooltipAI.Backend/TooltipAI.Backend.csproj" \
    -c Release -r linux-x64 --self-contained \
    -o "$DIST_DIR/linux-x64" \
    /p:PublishTrimmed=true /p:PublishSingleFile=true

log "Compilando para Windows x64..."
dotnet publish "$ROOT_DIR/TooltipAI.Platform.Win/TooltipAI.Platform.Win.csproj" \
    -c Release -r win-x64 --self-contained \
    -o "$DIST_DIR/win-x64" \
    /p:PublishTrimmed=true /p:PublishSingleFile=true

log "Compilando para macOS x64..."
dotnet publish "$ROOT_DIR/TooltipAI.Platform.Mac/TooltipAI.Platform.Mac.csproj" \
    -c Release -r osx-x64 --self-contained \
    -o "$DIST_DIR/osx-x64" \
    /p:PublishTrimmed=true /p:PublishSingleFile=true

ok "Compilación completada"

# ═══════════════════════════════════════════════════════════
# PASO 2: Ofuscar si se solicita
# ═══════════════════════════════════════════════════════════
if [ "$OBFUSCATE" = true ]; then
    log "Ejecutando ofuscación..."
    bash "$SCRIPT_DIR/obfuscate.sh" --skip-build
    
    # Copiar DLL ofuscada a todas las plataformas
    for platform in linux-x64 win-x64 osx-x64; do
        cp "$ROOT_DIR/publish-final/TooltipAI.Core.dll" \
           "$DIST_DIR/$platform/TooltipAI.Core.dll"
    done
    
    ok "Ofuscación aplicada a todas las plataformas"
fi

# ═══════════════════════════════════════════════════════════
# PASO 3: Generar hashes SHA-256
# ═══════════════════════════════════════════════════════════
log "Generando hashes SHA-256..."

HASH_FILE="$DIST_DIR/SHA256-HASHES.txt"
echo "# Tooltip AI — SHA-256 Hashes" > "$HASH_FILE"
echo "# Generado: $(date)" >> "$HASH_FILE"
echo "# Obfuscation: $OBFUSCATE" >> "$HASH_FILE"
echo "" >> "$HASH_FILE"

for platform in linux-x64 win-x64 osx-x64; do
    echo "## $platform" >> "$HASH_FILE"
    for f in "$DIST_DIR/$platform"/*.dll "$DIST_DIR/$platform"/*.exe; do
        if [ -f "$f" ]; then
            fname=$(basename "$f")
            hash=$(sha256sum "$f" | cut -d' ' -f1)
            echo "$fname: $hash" >> "$HASH_FILE"
        fi
    done
    echo "" >> "$HASH_FILE"
done

ok "Hashes generados en $HASH_FILE"

# ═══════════════════════════════════════════════════════════
# PASO 4: Crear paquetes de distribución
# ═══════════════════════════════════════════════════════════
log "Creando paquetes de distribución..."

cd "$DIST_DIR"

# Linux
tar -czf "TooltipAI-v1.0.0-linux-x64.tar.gz" -C linux-x64 .
ok "Linux: TooltipAI-v1.0.0-linux-x64.tar.gz"

# Windows
cd "$DIST_DIR"
zip -r "TooltipAI-v1.0.0-win-x64.zip" win-x64/
ok "Windows: TooltipAI-v1.0.0-win-x64.zip"

# macOS
cd "$DIST_DIR"
tar -czf "TooltipAI-v1.0.0-osx-x64.tar.gz" -C osx-x64 .
ok "macOS: TooltipAI-v1.0.0-osx-x64.tar.gz"

# ═══════════════════════════════════════════════════════════
# RESUMEN
# ═══════════════════════════════════════════════════════════
echo ""
echo "╔══════════════════════════════════════════════════════════╗"
echo "║       Tooltip AI — Build Completado                     ║"
echo "╠══════════════════════════════════════════════════════════╣"
echo "║  Linux:   $DIST_DIR/TooltipAI-v1.0.0-linux-x64.tar.gz"
echo "║  Windows: $DIST_DIR/TooltipAI-v1.0.0-win-x64.zip"
echo "║  macOS:   $DIST_DIR/TooltipAI-v1.0.0-osx-x64.tar.gz"
echo "║  Hashes:  $DIST_DIR/SHA256-HASHES.txt"
echo "║  Obfuscation: $OBFUSCATE"
echo "╚══════════════════════════════════════════════════════════╝"
echo ""
