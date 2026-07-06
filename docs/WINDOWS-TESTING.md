# Guia de Prueba en Windows

## Prerequisitos

1. **Windows 10/11**
2. **.NET 8 SDK** - Descargar desde https://dot.net/download
3. **Visual Studio 2022** (opcional) - Para debugging

## Paso 1: Clonar el Repositorio

```powershell
git clone https://github.com/dixi3stdgdl-design/tooltip-ai.git
cd tooltip-ai
```

## Paso 2: Ejecutar Script de Prueba

### Opcion A: PowerShell (Recomendado)
```powershell
.\scripts\test-windows.ps1
```

### Opcion B: Batch (Para usuarios basicos)
```cmd
scripts\test-windows.bat
```

### Opcion C: Manual
```powershell
# Restaurar paquetes
dotnet restore

# Compilar
dotnet build -c Release

# Ejecutar tests
dotnet test TooltipAI.Tests\TooltipAI.Tests.csproj
```

## Paso 3: Ejecutar el Backend

```powershell
dotnet run --project TooltipAI.Backend
```

El backend estara disponible en http://localhost:5000

## Paso 4: Probar el Webhook

En otra terminal:
```powershell
.\scripts\test-webhook.ps1 -BackendUrl http://localhost:5000
```

## Paso 5: Probar el Agent en Windows

### Build para Windows
```powershell
.\scripts\build-win-x64.sh
```

Esto crea una carpeta `publish-win-x64/` con los binarios.

### Ejecutar el Agent

1. Copiar la carpeta `publish-win-x64/` a cualquier lugar en Windows
2. Ejecutar `TooltipAI.Service.exe`
3. Abrir Excel, Chrome o VSCode
4. Pasar el mouse sobre elementos de la UI
5. Deberia aparecer un tooltip enriquecido

## Paso 6: Probar la Landing Page

```powershell
cd ..\landing\tooltip-ai
python -m http.server 8000
```

Abrir http://localhost:8000 en el navegador.

## Troubleshooting

### Error: "EnableWindowsTargeting"
Este error aparece al compilar en Linux. En Windows no deberia ocurrir.

### Error: "No se puede encontrar TooltipAI.Service.exe"
Verificar que el build se ejecuto correctamente:
```powershell
dotnet publish TooltipAI.Service\TooltipAI.Service.csproj -c Release -r win-x64 --self-contained
```

### El Agent no detecta elementos
1. Verificar que el servicio este corriendo
2. Ejecutar como administrador si es necesario
3. Revisar logs en `%AppData%\TooltipAI\Logs\`

### El tooltip no aparece
1. Verificar que la app este en la lista de apps soportadas
2. Revisar `rules.json` para ver si hay reglas para esa app
3. Probar con Excel, Chrome o VSCode (apps con mejor soporte)

## Apps con Mejor Soporte

| App | Soporte | Notas |
|-----|---------|-------|
| Microsoft Excel | Excelente | Reglas completas |
| Google Chrome | Excelente | Reglas completas |
| Visual Studio Code | Excelente | Reglas completas |
| Microsoft Word | Bueno | Misma familia que Excel |
| Microsoft PowerPoint | Bueno | Misma familia que Excel |
| Firefox | Bueno | Similar a Chrome |
| Edge | Bueno | Basado en Chromium |

## Archivos Importantes

| Archivo | Ubicacion | Descripcion |
|---------|-----------|-------------|
| rules.json | TooltipAI.Core/Rules/ | Reglas por app (42 reglas) |
| settings.json | %AppData%/TooltipAI/ | Configuracion del usuario |
| cache.db | %AppData%/TooltipAI/ | Cache de respuestas |
| consent.json | %AppData%/TooltipAI/ | Configuracion de privacidad |
| blacklist.json | %AppData%/TooltipAI/ | Lista negra de apps |
| usage.json | %AppData%/TooltipAI/ | Contador de uso |
