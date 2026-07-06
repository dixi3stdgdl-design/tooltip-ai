@echo off
REM ============================================================
REM Tooltip AI — Script de Prueba para Windows (Batch)
REM ============================================================
REM Uso: test-windows.bat

echo.
echo ╔══════════════════════════════════════════════════════════╗
echo ║       Tooltip AI — Prueba en Windows                    ║
echo ╚══════════════════════════════════════════════════════════╝
echo.

REM Verificar .NET
echo [1/4] Verificando .NET SDK...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK no encontrado
    echo Descarga desde: https://dot.net/download
    pause
    exit /b 1
)
echo   OK

REM Restaurar paquetes
echo [2/4] Restaurando paquetes...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Fallo al restaurar
    pause
    exit /b 1
)
echo   OK

REM Compilar
echo [3/4] Compilando...
dotnet build -c Release --no-restore
if %errorlevel% neq 0 (
    echo ERROR: Fallo al compilar
    pause
    exit /b 1
)
echo   OK

REM Tests
echo [4/4] Ejecutando tests...
dotnet test TooltipAI.Tests\TooltipAI.Tests.csproj --verbosity quiet
echo.

echo ═══════════════════════════════════════════════════════════
echo   Build completado!
echo.
echo   Para ejecutar el backend:
echo     dotnet run --project TooltipAI.Backend
echo.
echo   Para probar el webhook:
echo     scripts\test-webhook.ps1
echo ═══════════════════════════════════════════════════════════
echo.
pause
