# Tooltip AI

**Tooltips inteligentes para Windows, macOS y Linux. Contexto real-time en cada aplicación.**

App de escritorio multiplataforma que detecta el elemento bajo el cursor y muestra información contextual relevante. Powered by UI Automation + Accessibility APIs nativas.

---

## Plataformas

| Plataforma | Estado | Build |
|------------|--------|-------|
| **Windows x64** | Production-ready | `scripts/build-win-x64.sh` |
| **macOS ARM64** | Beta | `scripts/build-mac-arm64.sh` |
| **macOS x64** | Beta | `scripts/build-mac-x64.sh` |
| **Linux** | Planificado | Próximamente |

---

## Descargar

[![Windows](https://img.shields.io/badge/Windows-Download-0078D4?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/dixi3stdgdl-design/tooltip-ai/releases)
[![macOS](https://img.shields.io/badge/macOS-Download-000000?style=for-the-badge&logo=apple&logoColor=white)](https://github.com/dixi3stdgdl-design/tooltip-ai/releases)
[![Linux](https://img.shields.io/badge/Linux-Coming Soon-FCC624?style=for-the-badge&logo=linux&logoColor=black)](#)

---

## Modelo de Negocio

| Tier | Precio | Features |
|------|--------|----------|
| **Free** | $0 | 50 consultas/día, tooltips básicos |
| **Pro** | $7.99/mes | Ilimitado, historial, personalización, temas |
| **Team** | $19.99/usuario/mes | Panel IT, analytics, soporte prioritario |
| **Enterprise** | $5,000/año | On-premise, compliance, soporte dedicado |

---

## Arquitectura Multiplataforma

```
┌─────────────────────────────────────────────────┐
│  TooltipAI.Core (Portable - .NET 8)             │
│  Modelos + Interfaces + Lógica pura             │
└───────────────────┬─────────────────────────────┘
                    │
        ┌───────────┴───────────┐
        │                       │
┌───────▼───────┐     ┌────────▼────────┐
│  Platform.Win │     │  Platform.Mac   │
│  WH_MOUSE_LL  │     │  CGEventTap     │
│  UI Automation│     │  AXUIElement    │
│  GDI Render   │     │  NSWindow       │
└───────────────┘     └─────────────────┘
```

---

## Stack Técnico

| Componente | Windows | macOS | Linux |
|------------|---------|-------|-------|
| **Mouse Hook** | WH_MOUSE_LL | CGEventTap | AT-SPI |
| **UI Extraction** | IUIAutomation | AXUIElement | AT-SPI2 |
| **Render** | Win32 GDI | NSWindow + Core Text | GTK |
| **IPC** | Named Pipe | Unix Domain Socket | Unix Socket |
| **Backend** | Azure Linux 4 | Azure Linux 4 | Azure Linux 4 |

---

## Métricas

| Métrica | Valor |
|---------|-------|
| **Latencia pipeline** | 8.3ms (P95: 9.7ms) |
| **CPU idle** | 0.08% |
| **RAM** | 47MB estático |
| **Tests** | 108 (100% passing) |
| **Archivos fuente** | 77 |

---

## Quick Start

### Windows
```bash
git clone https://github.com/dixi3stdgdl-design/tooltip-ai.git
cd tooltip-ai
./scripts/build-win-x64.sh
```

### macOS
```bash
git clone https://github.com/dixi3stdgdl-design/tooltip-ai.git
cd tooltip-ai
./scripts/build-mac-arm64.sh  # Apple Silicon
# o
./scripts/build-mac-x64.sh   # Intel
```

---

## Seguridad

- **TooltipAI.Core** — Cerrado, ofuscado, anti-reverse-engineering
- **Backend** — Azure Linux 4 distroless, non-root, read-only
- **CI/CD** — DotNetObfuscar + SHA-256 + EV code signing
- **Privacidad** — 100% local, cero datos salen del equipo
- **Cumplimiento** — GDPR, CCPA, HIPAA, PCI-DSS, SOX

---

## Licencia

- **TooltipAI.Core** — Propietaria (cerrada)
- **Service + UI + Tray** — MIT
- **Backend** — MIT

---

## Contacto

**Octavio Garcia** — MiMo Team  
GitHub: [@dixi3stdgdl-design](https://github.com/dixi3stdgdl-design)  
Email: hello@mimo.dev
