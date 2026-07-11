# Tooltip AI — User Guide

Smart tooltips for Windows. Hover over any UI element to see what it does.

---

## Current status

Tooltip AI is in **alpha**. The core functionality works: hover over elements and see contextual tooltips. There is no installer yet — you need to build from source.

---

## Build from source

### Requirements
- Windows 10/11
- .NET 8 SDK ([download](https://dotnet.microsoft.com/download/dotnet/8.0))

### Steps

```powershell
git clone https://github.com/dixi3stdgdl-design/tooltip-ai.git
cd tooltip-ai

dotnet build TooltipAI.Core\TooltipAI.Core.csproj -c Release
dotnet build TooltipAI.Platform.Win\TooltipAI.Platform.Win.csproj -c Release
dotnet build TooltipAI.Service\TooltipAI.Service.csproj -c Release
dotnet build TooltipAI.Tray\TooltipAI.Tray.csproj -c Release
dotnet build TooltipAI.UI\TooltipAI.UI.csproj -c Release
```

---

## Running

1. **Start the tray app:** `dotnet run --project TooltipAI.Tray -c Release`
   - A tray icon appears in the system tray
   - The background service starts automatically

2. **Start the overlay:** `dotnet run --project TooltipAI.UI -c Release`
   - A transparent overlay window appears (it follows your cursor)

3. **Hover over elements** in any app — the tooltip appears next to your cursor

---

## What you'll see

The tooltip shows:
- **App category** (DEV, AUDIO, BROWSER, TERMINAL, etc.)
- **Process name** and window title
- **Element info** — type (Button, Edit, Slider), name, class
- **Gesture hint** — what the element does ("Click to activate", "Type to input text")
- **Quality tip** — contextual advice per app category
- **Keyboard shortcut** — if available

---

## Troubleshooting

**Tooltip doesn't appear:**
- Make sure both Tray and UI are running
- Check if another app is blocking the mouse hook (run as administrator)

**Tooltip shows "Unknown" for everything:**
- The UIA service may not be able to read the element
- Some apps (games, DirectX) don't expose UI Automation elements

**Logs:**
- Check `%APPDATA%/TooltipAI/Logs/` for daily log files

---

## What's planned

- Real Gemini Nano on-device AI (currently rule-based)
- macOS support
- Installer (MSI/EXE)
- Microsoft Store listing
