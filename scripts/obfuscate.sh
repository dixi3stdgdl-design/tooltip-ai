#!/bin/bash
# ============================================================
# Tooltip AI — Code Obfuscation Pipeline
# ============================================================
# Ofusca TooltipAI.Core.dll con Obfuscar
# Uso: ./obfuscate.sh [--skip-build] [--docker]
#
# Pasos:
#   1. Compila en modo Release
#   2. Crea copia limpia
#   3. Ejecuta ofuscación
#   4. Sustituye DLL ofuscada
#   5. Valida SHA-256
# ============================================================

set -euo pipefail

# ─── Configuración ─────────────────────────────────────────
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
BUILD_DIR="$ROOT_DIR/publish-clean"
OBFUSCATED_DIR="$ROOT_DIR/publish-obfuscated"
FINAL_DIR="$ROOT_DIR/publish-final"
LOG_FILE="$ROOT_DIR/obfuscation.log"
HASH_FILE="$ROOT_DIR/SHA256-HASHES.txt"

# Proyectos
CORE_PROJECT="$ROOT_DIR/TooltipAI.Core/TooltipAI.Core.csproj"
BACKEND_PROJECT="$ROOT_DIR/TooltipAI.Backend/TooltipAI.Backend.csproj"

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

log() { echo -e "${CYAN}[$(date +%H:%M:%S)]${NC} $1" | tee -a "$LOG_FILE"; }
ok() { echo -e "${GREEN}[OK]${NC} $1" | tee -a "$LOG_FILE"; }
warn() { echo -e "${YELLOW}[WARN]${NC} $1" | tee -a "$LOG_FILE"; }
fail() { echo -e "${RED}[FAIL]${NC} $1" | tee -a "$LOG_FILE"; exit 1; }

# ─── Inicializar log ───────────────────────────────────────
echo "=== Tooltip AI Obfuscation — $(date) ===" > "$LOG_FILE"

# ─── Parsear argumentos ────────────────────────────────────
SKIP_BUILD=false
DOCKER_MODE=false

for arg in "$@"; do
    case $arg in
        --skip-build) SKIP_BUILD=true ;;
        --docker) DOCKER_MODE=true ;;
    esac
done

# ═══════════════════════════════════════════════════════════
# PASO 1: Compilar en modo Release
# ═══════════════════════════════════════════════════════════
if [ "$SKIP_BUILD" = false ]; then
    log "PASO 1/5: Compilando proyecto completo en modo Release..."
    
    rm -rf "$BUILD_DIR"
    mkdir -p "$BUILD_DIR"
    
    # Compilar Solution completa
    dotnet publish "$ROOT_DIR/TooltipAI.sln" \
        -c Release \
        -o "$BUILD_DIR" \
        /p:PublishTrimmed=false \
        /p:PublishSingleFile=false \
        2>&1 | tee -a "$LOG_FILE"
    
    ok "Compilación completada"
else
    log "PASO 1/5: Saltando compilación (--skip-build)"
    if [ ! -d "$BUILD_DIR" ]; then
        fail "Directorio $BUILD_DIR no existe. Ejecuta sin --skip-build primero."
    fi
fi

# Verificar que existe la DLL
if [ ! -f "$BUILD_DIR/TooltipAI.Core.dll" ]; then
    fail "TooltipAI.Core.dll no encontrada en $BUILD_DIR"
fi

# ═══════════════════════════════════════════════════════════
# PASO 2: Crear copia limpia para ofuscación
# ═══════════════════════════════════════════════════════════
log "PASO 2/5: Preparando copia limpia..."

# Crear directorio temporal con solo la DLL a ofuscar
TEMP_OBFUSCATE="$ROOT_DIR/.obfuscate-temp"
rm -rf "$TEMP_OBFUSCATE"
mkdir -p "$TEMP_OBFUSCATE"

cp "$BUILD_DIR/TooltipAI.Core.dll" "$TEMP_OBFUSCATE/"
cp "$BUILD_DIR/TooltipAI.Core.pdb" "$TEMP_OBFUSCATE/" 2>/dev/null || true

# Copiar dependencias necesarias para Obfuscar
for dll in "$BUILD_DIR"/Microsoft.Extensions.*.dll "$BUILD_DIR"/System.*.dll; do
    [ -f "$dll" ] && cp "$dll" "$TEMP_OBFUSCATE/" 2>/dev/null || true
done

ok "Copia limpia creada en $TEMP_OBFUSCATE"

# ═══════════════════════════════════════════════════════════
# PASO 3: Ejecutar ofuscación con Obfuscar
# ═══════════════════════════════════════════════════════════
log "PASO 3/5: Ejecutando ofuscación..."

# Verificar si Obfuscar está instalado
if ! command -v obfuscar &> /dev/null; then
    # Intentar con dotnet tool
    if ! dotnet tool list -g | grep -q obfuscar; then
        log "Instalando Obfuscar..."
        dotnet tool install --global Obfuscar.Console --version 2.2.30
    fi
    OBFUSCAR_CMD="dotnet obfuscar"
else
    OBFUSCAR_CMD="obfuscar"
fi

# Crear configuración temporal con paths correctos
TEMP_CONFIG="$TEMP_OBFUSCATE/obfuscar.xml"
cat > "$TEMP_CONFIG" << XMLCONFIG
<?xml version="1.0" encoding="utf-8"?>
<Obfuscator>
  <Input file="TooltipAI.Core.dll" />
  <OutputDirectory>output</OutputDirectory>
  
  <LogFile path="obfuscation.log" />
  
  <Module>
    <Attribute xml-name="Obfuscator.ConfigurationAttribute" feature="true" />
    <Attribute xml-name="Obfuscator.Attribute" feature="true" />
  </Module>
  
  <StringEncryption enabled="true" dynamic="true" aes="true"
                    key="2026-TooltipAI-Obfuscation-Key-Production"
                    salt="TooltipAI-Core-Salt-v1" />
  
  <ControlFlow enabled="true" />
  
  <Rules>
    <ExcludeNamespace name="TooltipAI.Core.Interfaces" />
    <ExcludeNamespace name="TooltipAI.Core.Models" />
    <ExcludeNamespace name="TooltipAI.Core.Auth" />
    <ExcludeNamespace name="TooltipAI.Core.Translate" />
    <Exclude type="TooltipAI.Core.Services.LicenseService" />
    <Exclude type="TooltipAI.Core.Services.HybridAiService" />
  </Rules>
  
  <AntiTamper enabled="true" />
  <AntiDebug enabled="true" />
</Obfuscator>
XMLCONFIG

cd "$TEMP_OBFUSCATE"
$OBFUSCAR_CMD obfuscar.xml 2>&1 | tee -a "$LOG_FILE"
cd "$ROOT_DIR"

if [ ! -f "$TEMP_OBFUSCATE/output/TooltipAI.Core.dll" ]; then
    fail "Ofuscación falló — DLL ofuscada no generada"
fi

ok "Ofuscación completada"

# ═══════════════════════════════════════════════════════════
# PASO 4: Sustituir DLL ofuscada
# ═══════════════════════════════════════════════════════════
log "PASO 4/5: Sustituyendo DLL ofuscada..."

# Crear directorio final
rm -rf "$FINAL_DIR"
cp -r "$BUILD_DIR" "$FINAL_DIR"

# Reemplazar DLL limpia con ofuscada
cp "$TEMP_OBFUSCATE/output/TooltipAI.Core.dll" "$FINAL_DIR/TooltipAI.Core.dll"

# Calcular hashes ANTES de reemplazar (DLL limpia)
HASH_CLEAN=$(sha256sum "$BUILD_DIR/TooltipAI.Core.dll" | cut -d' ' -f1)
HASH_OBFUSCATED=$(sha256sum "$FINAL_DIR/TooltipAI.Core.dll" | cut -d' ' -f1)

ok "DLL sustituida"

# ═══════════════════════════════════════════════════════════
# PASO 5: Validación SHA-256
# ═══════════════════════════════════════════════════════════
log "PASO 5/5: Generando hashes SHA-256..."

cat > "$HASH_FILE" << EOF
# Tooltip AI — SHA-256 Hashes
# Generado: $(date)
# Build: $(dotnet --version 2>/dev/null || echo "unknown")

## DLL Original (limpia)
TooltipAI.Core.dll (clean): $HASH_CLEAN

## DLL Ofuscada
TooltipAI.Core.dll (obfuscated): $HASH_OBFUSCATED

## Todos los binarios finales
EOF

# Hashes de todos los archivos en publish-final
for f in "$FINAL_DIR"/*.dll "$FINAL_DIR"/*.exe; do
    if [ -f "$f" ]; then
        fname=$(basename "$f")
        hash=$(sha256sum "$f" | cut -d' ' -f1)
        echo "$fname: $hash" >> "$HASH_FILE"
    fi
done

ok "Hashes generados en $HASH_FILE"

# ═══════════════════════════════════════════════════════════
# LIMPIEZA
# ═══════════════════════════════════════════════════════════
log "Limpiando archivos temporales..."
rm -rf "$TEMP_OBFUSCATE"

# ═══════════════════════════════════════════════════════════
# RESUMEN
# ═══════════════════════════════════════════════════════════
echo ""
echo "╔══════════════════════════════════════════════════════════╗"
echo "║       Tooltip AI — Ofuscación Completada                ║"
echo "╠══════════════════════════════════════════════════════════╣"
echo "║  DLL Original:    $BUILD_DIR/TooltipAI.Core.dll"
echo "║  DLL Ofuscada:    $FINAL_DIR/TooltipAI.Core.dll"
echo "║  Hash Clean:      ${HASH_CLEAN:0:16}..."
echo "║  Hash Obfuscated: ${HASH_OBFUSCATED:0:16}..."
echo "║  Log:             $LOG_FILE"
echo "║  Hashes:          $HASH_FILE"
echo "╚══════════════════════════════════════════════════════════╝"
echo ""
log "Ofuscación completada exitosamente"
