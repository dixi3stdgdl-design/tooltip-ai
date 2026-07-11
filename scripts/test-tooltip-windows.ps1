# TooltipAI - Script de prueba completo para Windows
# Ejecutar en PowerShell como administrador (para el mouse hook)
# Uso: .\scripts\test-tooltip-windows.ps1

$ErrorActionPreference = "Continue"
$repoPath = Split-Path -Parent (Split-Path -Parent $PSCommandPath)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  TooltipAI - Build + Test + Run" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# 1. Build
Write-Host "[1/5] Building solution..." -ForegroundColor Yellow
Set-Location $repoPath
dotnet build -c Release 2>&1 | Select-String -Pattern "error|Error|Build succeeded|Build FAILED"
if ($LASTEXITCODE -ne 0) {
    Write-Host "BUILD FAILED - Check errors above" -ForegroundColor Red
    exit 1
}
Write-Host "Build OK" -ForegroundColor Green
Write-Host ""

# 2. Run tests
Write-Host "[2/5] Running tests..." -ForegroundColor Yellow
dotnet test TooltipAI.Tests/TooltipAI.Tests.csproj --filter "FullyQualifiedName!~IntegrationTests" --no-build -c Release 2>&1 | Select-String -Pattern "Passed|Failed|Total"
Write-Host ""

# 3. Publish executables
Write-Host "[3/5] Publishing executables..." -ForegroundColor Yellow
$publishDir = "$repoPath\publish"
if (Test-Path $publishDir) { Remove-Item -Recurse -Force $publishDir }

dotnet publish TooltipAI.Tray/TooltipAI.Tray.csproj -c Release -o "$publishDir\Tray" --self-contained false 2>&1 | Out-Null
dotnet publish TooltipAI.Service/TooltipAI.Service.csproj -c Release -o "$publishDir\Service" --self-contained false 2>&1 | Out-Null
dotnet publish TooltipAI.UI/TooltipAI.UI.csproj -c Release -o "$publishDir\UI" --self-contained false 2>&1 | Out-Null

Write-Host "Published to: $publishDir" -ForegroundColor Green
Write-Host ""

# 4. Copy Service exe to Tray directory (Tray looks for it in same folder)
Write-Host "[4/5] Copying Service to Tray directory..." -ForegroundColor Yellow
Copy-Item "$publishDir\Service\TooltipAI.Service.exe" "$publishDir\Tray\" -Force 2>$null
Copy-Item "$publishDir\Service\*.dll" "$publishDir\Tray\" -Force 2>$null
Copy-Item "$publishDir\Service\*.json" "$publishDir\Tray\" -Force 2>$null
# Copy Platform.Win DLL (needed for UIA)
Copy-Item "$publishDir\Service\TooltipAI.Platform.Win.dll" "$publishDir\Tray\" -Force 2>$null
Write-Host "OK" -ForegroundColor Green
Write-Host ""

# 5. Run
Write-Host "[5/5] Starting TooltipAI..." -ForegroundColor Yellow
Write-Host ""
Write-Host "  Tray:  $publishDir\Tray\TooltipAI.Tray.exe" -ForegroundColor White
Write-Host "  UI:    $publishDir\UI\TooltipAI.UI.exe" -ForegroundColor White
Write-Host ""
Write-Host "  1. Run Tray first (system tray icon appears)" -ForegroundColor Gray
Write-Host "  2. Run UI (overlay window appears)" -ForegroundColor Gray
Write-Host "  3. Hover mouse over any app element" -ForegroundColor Gray
Write-Host "  4. Tooltip should show real element name + type" -ForegroundColor Gray
Write-Host ""

$runTray = Read-Host "Start Tray now? (y/n)"
if ($runTray -eq "y") {
    Start-Process "$publishDir\Tray\TooltipAI.Tray.exe"
    Start-Sleep -Seconds 2
    Start-Process "$publishDir\UI\TooltipAI.UI.exe"
    Write-Host ""
    Write-Host "TooltipAI is running! Hover over elements to see tooltips." -ForegroundColor Green
    Write-Host "Press Enter to stop..." -ForegroundColor Gray
    Read-Host
}
