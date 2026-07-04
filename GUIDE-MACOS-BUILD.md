# Tooltip AI — Guía Completa de Build en macOS
## Paso a Paso: De Cero a DMG Firmado

**Autor:** Octavio Garcia  
**Fecha:** Julio 2026  
**Requisito previo:** Mac con macOS 12+ (Monterey o superior)

---

## PREREQUISITOS

### 1. Instalar .NET 8 SDK

```bash
# Opción A: Homebrew (recomendado)
brew install --cask dotnet-sdk

# Opción B: Descarga directa
# https://dotnet.microsoft.com/download/dotnet/8.0
# Seleccionar: macOS > ARM64 (Apple Silicon) o x64 (Intel)

# Verificar instalación
dotnet --version
# Debe mostrar: 8.0.x
```

### 2. Instalar herramientas Apple (para firma y notarization)

```bash
# Xcode Command Line Tools
xcode-select --install

# Verificar
xcodebuild -version
```

### 3. Clonar el repositorio

```bash
git clone https://github.com/dixi3stdgdl-design/tooltip-ai.git
cd tooltip-ai
```

---

## BUILD LOCAL (sin firma Apple)

### Paso 1: Dar permisos de ejecución a los scripts

```bash
chmod +x scripts/build-mac-arm64.sh
chmod +x scripts/build-mac-x64.sh
```

### Paso 2: Compilar para tu arquitectura

**Si tienes Apple Silicon (M1/M2/M3/M4):**
```bash
./scripts/build-mac-arm64.sh
```

**Si tienes Intel:**
```bash
./scripts/build-mac-x64.sh
```

### Paso 3: Ejecutar

```bash
# El app bundle se crea en publish-mac-arm64/ o publish-mac-x64/
open "publish-mac-arm64/Tooltip AI.app"
```

### Paso 4: Otorgar permisos (primera ejecución)

Cuando macOS pregunte, ve a:
- **System Preferences > Privacy & Security > Accessibility** → Agregar "Tooltip AI"
- **System Preferences > Privacy & Security > Screen Recording** → Agregar "Tooltip AI"

---

## BUILD PARA DISTRIBUCIÓN (con firma Apple)

### Paso 1: Obtener Developer ID

Necesitas una cuenta de Apple Developer ($99/año):
- https://developer.apple.com

### Paso 2: Code Sign

```bash
# Firmar el app bundle
codesign --sign "Developer ID Application: TU NOMBRE (TEAM_ID)" \
    --deep --force --verbose \
    "publish-mac-arm64/Tooltip AI.app"

# Verificar firma
codesign --verify --verbose=4 \
    "publish-mac-arm64/Tooltip AI.app"
```

### Paso 3: Crear entitlements

```bash
cat > tooltipai.entitlements << 'EOF'
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
EOF
```

### Paso 4: Firmar con entitlements

```bash
codesign --sign "Developer ID Application: TU NOMBRE (TEAM_ID)" \
    --entitlements tooltipai.entitlements \
    --deep --force --verbose \
    "publish-mac-arm64/Tooltip AI.app"
```

### Paso 5: Notarizar con Apple

```bash
# Crear archivo de credenciales
cat > credentials.txt << EOF
TEAM_ID
apple-id@example.com
@app-specific-password
EOF

# Subir para notarización
xcrun notarytool submit \
    "publish-mac-arm64/TooltipAI-mac-arm64.dmg" \
    --keychain-profile "notarytool-profile" \
    --wait

# O con credenciales directas
xcrun notarytool submit \
    "publish-mac-arm64/TooltipAI-mac-arm64.dmg" \
    --apple-id "apple-id@example.com" \
    --team-id "TEAM_ID" \
    --password "app-specific-password" \
    --wait
```

### Paso 6: Staple el certificado

```bash
xcrun stapler staple "publish-mac-arm64/TooltipAI-mac-arm64.dmg"
```

### Paso 7: Verificar

```bash
spctl --assess --type open --context context:primary-signature \
    "publish-mac-arm64/TooltipAI-mac-arm64.dmg"
# Debe mostrar: accepted
```

---

## BUILD COMO HOMEWORK (para distribución masiva)

### Crear fórmula Homebrew

```bash
# Crear repo tap
brew tap-new dixi3stdgdl-design/tooltip-ai

# Crear fórmula
cat > $(brew --repo dixi3stdgdl-design/tooltip-ai)/Formula/tooltip-ai.rb << 'RUBY'
class TooltipAi < Formula
  desc "Contextual intelligence for native tooltips"
  homepage "https://tooltip-ai.com"
  version "1.0.0"

  if Hardware::CPU.arm?
    url "https://github.com/dixi3stdgdl-design/tooltip-ai/releases/download/v1.0.0-mac/TooltipAI-mac-osx-arm64.dmg"
    sha256 "HASH_AQUI"
  else
    url "https://github.com/dixi3stdgdl-design/tooltip-ai/releases/download/v1.0.0-mac/TooltipAI-mac-osx-x64.dmg"
    sha256 "HASH_AQUI"
  end

  def install
    prefix.install "Tooltip AI.app"
    bin.create_symlink "#{prefix}/Tooltip AI.app/Contents/MacOS/tooltipai"
  end

  test do
    system "#{bin}/tooltipai", "--version"
  end
end
RUBY

# Push
cd $(brew --repo dixi3stdgdl-design/tooltip-ai)
git add .
git commit -m "Add tooltip-ai formula"
git push
```

### Usuarios instalan con:

```bash
brew tap dixi3stdgdl-design/tooltip-ai
brew install tooltip-ai
```

---

## CI/CD AUTOMÁTICO

El workflow de GitHub Actions (`build-macos.yml`) ejecuta automáticamente:

1. Build para ARM64 (Apple Silicon)
2. Build para x64 (Intel)
3. Crea DMG para cada arquitectura
4. Genera SHA-256 hashes
5. Sube artifacts a GitHub
6. Crea release con ambos DMGs

Para activar:
```bash
git push origin main
```

---

## TROUBLESHOOTING

### "CGEventTap creation failed"
```bash
# Verificar permisos de Accessibility
tccutil reset Accessibility com.mimo.tooltipai
# Volver a otorgar permisos en System Preferences
```

### "AXUIElement permission denied"
```bash
# Resetear permisos de Accessibility
tccutil reset Accessibility
# Abrir System Preferences > Privacy & Security > Accessibility
# Agregar la app manualmente
```

### ".NET SDK not found"
```bash
# Instalar .NET 8
brew install --cask dotnet-sdk
# Reiniciar terminal
```

### "Build fails with macOS SDK error"
```bash
# Asegurar que Xcode Command Line Tools están instalados
xcode-select --install
# O reinstalar
sudo rm -rf /Library/Developer/CommandLineTools
xcode-select --install
```

---

## COMANDOS RÁPIDOS (COPY-PASTE)

```bash
# Build completo (ARM64)
cd tooltip-ai && chmod +x scripts/build-mac-arm64.sh && ./scripts/build-mac-arm64.sh

# Build completo (Intel)
cd tooltip-ai && chmod +x scripts/build-mac-x64.sh && ./scripts/build-mac-x64.sh

# Ejecutar
open "publish-mac-arm64/Tooltip AI.app"

# Firma (reemplazar TEAM_ID)
codesign --sign "Developer ID Application: Octavio Garcia (TEAM_ID)" --deep --force "publish-mac-arm64/Tooltip AI.app"

# Notarización
xcrun notarytool submit "publish-mac-arm64/TooltipAI-mac-arm64.dmg" --apple-id "tu@email.com" --team-id "TEAM_ID" --password "app-specific-password" --wait

# Staple
xcrun stapler staple "publish-mac-arm64/TooltipAI-mac-arm64.dmg"
```
