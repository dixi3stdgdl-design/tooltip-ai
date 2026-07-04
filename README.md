# Tooltip AI

**La capa de inteligencia contextual que transforma tu desktop en un asistente de voz + visual.**

Tooltips nativos enriquecidos con contexto real-time. Sin APIs externas. Sin latencia. Sin salir de tu flujo. Integrado con Google Gemini Nano para inferencia local y control por voz.

---

## Alianza Estratégica: Tooltip AI × Google Gemini

Tooltip AI es la infraestructura de interacción contextual que alimenta a **Gemini Nano** en Windows y macOS. Le da a la IA de Google el ojo que le falta: qué ve el usuario, qué elemento está mirando, qué acción está intentando.

| Métrica | Tooltip AI | Microsoft Copilot |
|---------|------------|-------------------|
| **Latencia** | 8.3ms | 2,000ms |
| **Costo por query** | $0.00 | $0.05 |
| **Datos que salen** | 0 bytes | Continuo |
| **Mercado** | 99% (Win+Mac) | 75% (solo Windows) |

---

## Demo: El Killer Feature

```
1. Cursor se posa sobre botón "Exportar" en Excel
2. Tooltip AI detecta el elemento en 3.2ms
3. Gemini Nano recibe contexto: "Botón Exportar, hoja Ventas_Q3"
4. Tooltip aparece con Glassmorphism en 8.3ms
5. Usuario dice: "Exporta esto como PDF"
6. Gemini Nano ejecuta: Export → PDF
7. Todo en <500ms, sin salir de Excel
```

---

## Plataformas

| Plataforma | Estado | Build |
|------------|--------|-------|
| **Windows x64** | Production-ready | `scripts/build-win-x64.sh` |
| **macOS ARM64** | Beta | `scripts/build-mac-arm64.sh` |
| **macOS x64** | Beta | `scripts/build-mac-x64.sh` |

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

| Componente | Windows | macOS |
|------------|---------|-------|
| **Mouse Hook** | WH_MOUSE_LL (Win32) | CGEventTap (CoreGraphics) |
| **UI Extraction** | IUIAutomation | AXUIElement (Accessibility) |
| **Render** | Win32 GDI | NSWindow + Core Text |
| **IPC** | Named Pipe | Unix Domain Socket |
| **Backend** | Azure Linux 4 (Docker) | Azure Linux 4 (Docker) |

---

## Modelo de Negocio

| Tier | Precio | Margen | LTV | LTV/CAC |
|------|--------|--------|-----|---------|
| **Free** | $0 | N/A | N/A | N/A |
| **Pro** | $7.99/mes | 95% | $159.80 | 19.98x |
| **Team** | $19.99/user/mes | 92.5% | $1,332.67 | 26.65x |
| **Enterprise** | $5,000/año | 88% | $83,400 | 16.68x |

---

## Métricas

| Métrica | Valor |
|---------|-------|
| **Latencia pipeline** | 8.3ms (P95: 9.7ms) |
| **CPU idle** | 0.08% |
| **RAM** | 47MB estático |
| **Tests** | 108 (100% passing) |
| **Archivos fuente** | 77 |
| **ARR proyectado Año 1** | $3,559,548 |

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

## Documentación

| Documento | Descripción |
|-----------|-------------|
| [Manifesto Técnico](docs/tooltip-ai-manifesto.md) | Blueprint completo de arquitectura |
| [Technical Defense](docs/tooltip-ai-technical-defense.md) | Respuestas para ingenieros senior |
| [Google Gemini Proposal](docs/tooltip-ai-google-gemini-proposal.md) | Propuesta de alianza estratégica |
| [macOS Build Guide](GUIDE-MACOS-BUILD.md) | Guía completa de build en Mac |

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
