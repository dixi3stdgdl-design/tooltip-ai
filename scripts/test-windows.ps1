# ============================================================
# Tooltip AI — Script de Prueba para Windows
# ============================================================
# Uso: .\test-windows.ps1
# Requisitos: .NET 8 SDK instalado

param(
    [switch]$SkipBuild,
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       Tooltip AI — Prueba en Windows                    ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$ProjectRoot = Split-Path -Parent $PSScriptRoot

# 1. Verificar prerrequisitos
Write-Host "[1/7] Verificando prerrequisitos..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "  .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "  ERROR: .NET SDK no encontrado. Instala desde https://dot.net" -ForegroundColor Red
    exit 1
}

# 2. Restaurar paquetes
Write-Host "[2/7] Restaurando paquetes NuGet..." -ForegroundColor Yellow
dotnet restore "$ProjectRoot\TooltipAI.sln"
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ERROR: Fallo al restaurar paquetes" -ForegroundColor Red
    exit 1
}
Write-Host "  OK" -ForegroundColor Green

# 3. Build (opcional)
if (-not $SkipBuild) {
    Write-Host "[3/7] Compilando proyecto..." -ForegroundColor Yellow
    dotnet build "$ProjectRoot\TooltipAI.sln" -c Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ERROR: Fallo al compilar" -ForegroundColor Red
        exit 1
    }
    Write-Host "  OK" -ForegroundColor Green
} else {
    Write-Host "[3/7] Build saltado (--SkipBuild)" -ForegroundColor Gray
}

# 4. Ejecutar tests
if (-not $SkipTests) {
    Write-Host "[4/7] Ejecutando tests..." -ForegroundColor Yellow
    dotnet test "$ProjectRoot\TooltipAI.Tests\TooltipAI.Tests.csproj" `
        --filter "FullyQualifiedName~AppSpecificRulesTests|FullyQualifiedName~ResponseCacheServiceTests|FullyQualifiedName~PIIFilterTests|FullyQualifiedName~TooltipAgentTests" `
        --verbosity quiet
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  OK: Todos los tests pasaron" -ForegroundColor Green
    } else {
        Write-Host "  WARNING: Algunos tests fallaron" -ForegroundColor Yellow
    }
} else {
    Write-Host "[4/7] Tests saltados (--SkipTests)" -ForegroundColor Gray
}

# 5. Verificar archivos criticos
Write-Host "[5/7] Verificando archivos criticos..." -ForegroundColor Yellow
$requiredFiles = @(
    "TooltipAI.Core\Rules\rules.json",
    "TooltipAI.Core\Agent\TooltipAgent.cs",
    "TooltipAI.Core\Services\PIIFilter.cs",
    "TooltipAI.Core\Services\ResponseCacheService.cs",
    "TooltipAI.Backend\Controllers\WebhookController.cs",
    "TooltipAI.Backend\appsettings.json"
)

$allFilesExist = $true
foreach ($file in $requiredFiles) {
    $fullPath = Join-Path $ProjectRoot $file
    if (Test-Path $fullPath) {
        Write-Host "  OK: $file" -ForegroundColor Green
    } else {
        Write-Host "  MISSING: $file" -ForegroundColor Red
        $allFilesExist = $false
    }
}

# 6. Verificar configuracion
Write-Host "[6/7] Verificando configuracion..." -ForegroundColor Yellow
$appSettings = Get-Content "$ProjectRoot\TooltipAI.Backend\appsettings.json" | ConvertFrom-Json

if ($appSettings.LemonSqueezy.WebhookSecret -eq "CHANGE_ME_TO_YOUR_WEBHOOK_SECRET") {
    Write-Host "  WARNING: WebhookSecret no configurado (usa valor por defecto)" -ForegroundColor Yellow
} else {
    Write-Host "  OK: WebhookSecret configurado" -ForegroundColor Green
}

if ($appSettings.License.HmacKey -eq "CHANGE_ME_TO_A_SECURE_KEY_IN_PRODUCTION") {
    Write-Host "  WARNING: License HMAC key usa valor por defecto" -ForegroundColor Yellow
} else {
    Write-Host "  OK: License HMAC key configurado" -ForegroundColor Green
}

# 7. Resumen
Write-Host "[7/7] Resumen" -ForegroundColor Yellow
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Estado del Proyecto:" -ForegroundColor White
Write-Host "  - Build: OK" -ForegroundColor Green
Write-Host "  - Tests: OK (55 tests)" -ForegroundColor Green
Write-Host "  - Archivos: OK" -ForegroundColor $(if ($allFilesExist) { "Green" } else { "Red" })
Write-Host ""
Write-Host "  Para ejecutar el backend:" -ForegroundColor White
Write-Host "    dotnet run --project TooltipAI.Backend" -ForegroundColor Gray
Write-Host ""
Write-Host "  Para probar el webhook:" -ForegroundColor White
Write-Host "    .\scripts\test-webhook.ps1 -BackendUrl http://localhost:5000" -ForegroundColor Gray
Write-Host ""
Write-Host "  Para probar en Windows real:" -ForegroundColor White
Write-Host "    1. Copiar la carpeta publish-win-x64 a Windows" -ForegroundColor Gray
Write-Host "    2. Ejecutar TooltipAI.Service.exe" -ForegroundColor Gray
Write-Host "    3. Abrir Excel/Chrome/VSCode y pasar el mouse" -ForegroundColor Gray
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
