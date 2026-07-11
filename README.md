# Tooltip AI

**Tooltips inteligentes para Windows. Contexto real-time en cada aplicacion.**

App de escritorio que detecta el elemento bajo el cursor y muestra informacion contextual relevante. Powered by UI Automation + Accessibility APIs nativas.

---

## Plataformas

| Plataforma | Estado |
|------------|--------|
| **Windows x64** | En desarrollo (Alpha) |
| **macOS** | Stub (pendiente) |
| **Linux** | No soportado |

---

## Arquitectura

```
┌─────────────────────────────────────────┐
│  TooltipAI.Core (Portable - .NET 8)     │
│  Modelos + Interfaces + Logica pura     │
└───────────────────┬─────────────────────┘
                    │
        ┌───────────┴───────────┐
        │                       │
┌───────▼───────┐     ┌────────▼────────┐
│  Platform.Win │     │  Platform.Mac   │
│  WH_MOUSE_LL  │     │  CGEventTap     │
│  UI Automation│     │  AXUIElement    │
└───────────────┘     └─────────────────┘
```

---

## Stack Tecnico

| Componente | Windows |
|------------|---------|
| **Mouse Hook** | WH_MOUSE_LL (P/Invoke) |
| **UI Extraction** | IUIAutomation via COM Interop |
| **Render** | WinUI 3 (click-through overlay) |
| **IPC** | Named Pipe |
| **Backend** | ASP.NET Core |

---

## Quick Start

### Windows
```powershell
git clone https://github.com/dixi3stdgdl-design/tooltip-ai.git
cd tooltip-ai

# Build individual projects (no macOS workload needed):
dotnet build TooltipAI.Core\TooltipAI.Core.csproj -c Release
dotnet build TooltipAI.Platform.Win\TooltipAI.Platform.Win.csproj -c Release
dotnet build TooltipAI.Service\TooltipAI.Service.csproj -c Release
dotnet build TooltipAI.Tray\TooltipAI.Tray.csproj -c Release
dotnet build TooltipAI.UI\TooltipAI.UI.csproj -c Release

# Run
dotnet run --project TooltipAI.Tray -c Release
# In another terminal:
dotnet run --project TooltipAI.UI -c Release
```

---

## Que hace

Cuando pasas el mouse sobre un elemento de cualquier app:
1. Captura el elemento bajo el cursor via UI Automation
2. Identifica tipo de control (Button, Edit, Slider, etc.)
3. Genera contexto enriquecido (hints, shortcuts, tips)
4. Muestra tooltip glassmorphic click-through junto al cursor

---

## Estado actual

- Mouse hook funcional (WH_MOUSE_LL)
- UIA service recien arreglado (COM Interop real)
- Named pipe IPC funcional
- WinUI 3 overlay funcional
- Backend API funcional (licencias, contexto, plugins)
- Tests: 207 pasando

**Pendiente**: probar end-to-end en Windows, medir latencia real, integrar AI real.

---

## Licencia

- **TooltipAI.Core** — Propietaria (cerrada)
- **Service + UI + Tray** — MIT
- **Backend** — MIT

---

## Contacto

**Octavio Garcia** — MiMo Team  
GitHub: [@dixi3stdgdl-design](https://github.com/dixi3stdgdl-design)  
Email: Dixstdgdl@hotmail.com
