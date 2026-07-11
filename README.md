# Tooltip AI

Smart tooltips for Windows. When you hover over any UI element, Tooltip AI shows you what it does, its keyboard shortcuts, and contextual tips — without leaving your workflow.

---

## What it does

Hover over a button in Chrome → Tooltip AI tells you "Back button — Alt+Left".  
Hover over a cell in Excel → Tooltip AI tells you "Data cell — F2 to edit".  
Hover over a terminal in VS Code → Tooltip AI tells you "Integrated terminal — Ctrl+`".

The tooltip appears as a glassmorphic overlay next to your cursor. It's click-through (doesn't steal focus) and disappears when you move away.

---

## How it works

1. **Mouse hook** (WH_MOUSE_LL) tracks your cursor position
2. **UI Automation** (IUIAutomation via COM Interop) identifies the element under your cursor — its name, type (Button, Edit, Slider, etc.), and properties
3. **Context enricher** generates hints, shortcuts, and tips based on the element and app category
4. **Named Pipe** sends the data to the UI overlay
5. **WinUI 3 overlay** renders a glassmorphic tooltip next to your cursor

Everything runs locally. No data leaves your machine.

---

## Status

**Alpha** — The core pipeline works: mouse hook → UIA element detection → context enrichment → named pipe → WinUI 3 overlay.

What works:
- Mouse hook captures cursor position
- UIA service extracts real element data (name, control type, class, automation ID)
- Context enricher generates relevant hints per control type and app category
- Named Pipe IPC between Service and UI processes
- WinUI 3 click-through overlay with glassmorphic styling
- 207 tests passing

What's in progress:
- End-to-end latency measurement on real Windows hardware
- Gemini Nano integration (currently simulated with rules engine)
- macOS support (stub only)
- Monetization (LemonSqueezy integration)

---

## Build (Windows)

```powershell
git clone https://github.com/dixi3stdgdl-design/tooltip-ai.git
cd tooltip-ai

# Build each project individually (no macOS workload needed):
dotnet build TooltipAI.Core\TooltipAI.Core.csproj -c Release
dotnet build TooltipAI.Platform.Win\TooltipAI.Platform.Win.csproj -c Release
dotnet build TooltipAI.Service\TooltipAI.Service.csproj -c Release
dotnet build TooltipAI.Tray\TooltipAI.Tray.csproj -c Release
dotnet build TooltipAI.UI\TooltipAI.UI.csproj -c Release
```

## Run

```powershell
# Terminal 1 — starts the system tray + background service:
dotnet run --project TooltipAI.Tray -c Release

# Terminal 2 — starts the overlay window:
dotnet run --project TooltipAI.UI -c Release
```

Then hover over elements in any app. The tooltip appears next to your cursor.

---

## Architecture

```
TooltipAI.Tray          System tray icon, launches Service
    │
    ▼
TooltipAI.Service       Background process
    ├── MouseHookService     WH_MOUSE_LL global hook
    ├── UIAutomationService  IUIAutomation COM Interop
    ├── HybridAiService      Local context enrichment
    ├── NamedPipeService     IPC to UI
    └── SoftwareCategoryClassifier  App categorization

TooltipAI.UI            WinUI 3 overlay window
    ├── NamedPipeClient      Receives TooltipData
    └── TooltipOverlay       Glassmorphic rendering

TooltipAI.Core          Shared library (portable .NET 8)
    ├── Models              ElementInfo, TooltipData, SoftwareCategory
    ├── Interfaces          IUIAutomationService, IAIService
    └── Services            Enricher, Classifier, Cache, Settings
```

---

## Project structure

```
tooltip-ai/
├── TooltipAI.Core/           Shared models, interfaces, logic
├── TooltipAI.Platform.Win/   Windows UIA + renderers
├── TooltipAI.Service/        Background service (mouse hook + UIA)
├── TooltipAI.UI/             WinUI 3 overlay
├── TooltipAI.Tray/           System tray launcher
├── TooltipAI.Backend/        ASP.NET Core API
├── TooltipAI.Tests/          xUnit tests
├── scripts/                  Build and test scripts
└── docs/                     Documentation
```

---

## License

- **TooltipAI.Core** — Proprietary (closed source)
- **Service + UI + Tray** — MIT
- **Backend** — MIT

---

## Contact

**Octavio Garcia** — MiMo Team  
GitHub: [@dixi3stdgdl-design](https://github.com/dixi3stdgdl-design)  
Email: Dixstdgdl@hotmail.com
