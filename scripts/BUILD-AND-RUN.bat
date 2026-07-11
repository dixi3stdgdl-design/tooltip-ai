@echo off
title TooltipAI - Build and Run
echo ============================================
echo   TooltipAI - Build + Run
echo ============================================
echo.

cd /d "%~dp0\.."

echo [1] Building projects...
dotnet build TooltipAI.Core\TooltipAI.Core.csproj -c Release
if %ERRORLEVEL% NEQ 0 ( echo CORE FAILED & pause & exit /b 1 )
dotnet build TooltipAI.Platform.Win\TooltipAI.Platform.Win.csproj -c Release
if %ERRORLEVEL% NEQ 0 ( echo PLATFORM.WIN FAILED & pause & exit /b 1 )
dotnet build TooltipAI.Service\TooltipAI.Service.csproj -c Release
if %ERRORLEVEL% NEQ 0 ( echo SERVICE FAILED & pause & exit /b 1 )
dotnet build TooltipAI.Tray\TooltipAI.Tray.csproj -c Release
if %ERRORLEVEL% NEQ 0 ( echo TRAY FAILED & pause & exit /b 1 )
dotnet build TooltipAI.UI\TooltipAI.UI.csproj -c Release
if %ERRORLEVEL% NEQ 0 ( echo UI FAILED & pause & exit /b 1 )
echo [OK] All projects built
echo.

echo [2] Publishing...
dotnet publish TooltipAI.Tray\TooltipAI.Tray.csproj -c Release -o publish\Tray --self-contained false
dotnet publish TooltipAI.Service\TooltipAI.Service.csproj -c Release -o publish\Service --self-contained false
dotnet publish TooltipAI.UI\TooltipAI.UI.csproj -c Release -o publish\UI --self-contained false

echo [3] Copying Service to Tray directory...
copy /Y publish\Service\TooltipAI.Service.exe publish\Tray\ >nul
copy /Y publish\Service\*.dll publish\Tray\ >nul
copy /Y publish\Service\*.json publish\Tray\ >nul
copy /Y publish\Service\TooltipAI.Platform.Win.dll publish\Tray\ >nul

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
