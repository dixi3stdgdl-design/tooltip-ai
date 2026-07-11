@echo off
title TooltipAI - Build and Run
echo ============================================
echo   TooltipAI - Build + Run
echo ============================================
echo.

cd /d "%~dp0\.."

echo [1] Building...
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo BUILD FAILED
    pause
    exit /b 1
)
echo [OK] Build succeeded
echo.

echo [2] Publishing...
dotnet publish TooltipAI.Tray\TooltipAI.Tray.csproj -c Release -o publish\Tray --self-contained false
dotnet publish TooltipAI.Service\TooltipAI.Service.csproj -c Release -o publish\Service --self-contained false
dotnet publish TooltipAI.UI\TooltipAI.UI.csproj -c Release -o publish\UI --self-contained false

echo [3] Copying files...
copy /Y publish\Service\TooltipAI.Service.exe publish\Tray\ >nul
copy /Y publish\Service\*.dll publish\Tray\ >nul
copy /Y publish\Service\*.json publish\Tray\ >nul

echo.
echo ============================================
echo   READY! Files at: publish\
echo ============================================
echo.
echo   1. Run: publish\Tray\TooltipAI.Tray.exe
echo   2. Run: publish\UI\TooltipAI.UI.exe
echo   3. Hover over app elements
echo.
echo Press any key to launch...
pause >nul

start "" "publish\Tray\TooltipAI.Tray.exe"
timeout /t 2 >nul
start "" "publish\UI\TooltipAI.UI.exe"

echo TooltipAI is running!
echo Press any key to exit...
pause >nul
