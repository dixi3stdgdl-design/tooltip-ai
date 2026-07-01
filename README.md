# Tooltip AI

**Tooltips inteligentes para Windows. Contexto real-time en cada aplicación.**

App de escritorio Windows que detecta el elemento bajo el cursor y muestra información contextual relevante usando AI. Powered by UI Automation + WinUI 3.

---

## Soporte el desarrollo

[![Pro](https://img.shields.io/badge/Pro-$4.99%2Fmes-2563EB?style=for-the-badge&logo=windows&logoColor=white)](https://link.mercadopago.com.mx/mimomobile)
[![Business](https://img.shields.io/badge/Business-$9.99%2Fmes-16A34A?style=for-the-badge&logo=windows&logoColor=white)](https://link.mercadopago.com.mx/mimomobile)

## Modelo de Negocio

| Tier | Precio | Features |
|------|--------|----------|
| **Free** | $0 | 50 consultas/día, tooltips básicos |
| **Pro** | $4.99/mes | Ilimitado, historial, personalización |
| **Business** | $9.99/mes | API, soporte, actualizaciones prioritarias |

---

## Funcionalidades

- **Detección en tiempo real** — Detecta el elemento UI bajo el cursor
- **Tooltips contextuales** — Muestra información relevante vía AI
- **UI Automation** — Lee propiedades de controles Windows nativos
- **System Tray** — Se ejecuta en segundo plano sin molestar
- **WinUI 3** — Interfaz moderna con Fluent Design

---

## Arquitectura

```
┌─────────────────────────────────┐
│    TooltipAI.UI (WinUI 3)       │
│    Interfaz moderna Windows     │
└───────────┬─────────────────────┘
            │
┌───────────▼─────────────────────┐
│    TooltipAI.Core               │
│    UIA Service, Models          │
└───────────┬─────────────────────┘
            │
┌───────────▼─────────────────────┐
│    TooltipAI.Service            │
│    Background monitoring        │
└───────────┬─────────────────────┘
            │
┌───────────▼─────────────────────┐
│    TooltipAI.Tray               │
│    System tray icon             │
└─────────────────────────────────┘
```

---

## Stack Técnico

| Componente | Tecnología |
|------------|------------|
| Framework | .NET 8 |
| UI | WinUI 3 (Fluent Design) |
| Detección | UI Automation (P/Invoke) |
| Monitoring | Background Service |
| Tray | System Tray App |

---

## Proyectos

| Proyecto | Descripción |
|----------|-------------|
| `TooltipAI.Core` | Modelos, interfaces, lógica central |
| `TooltipAI.Service` | Servicio de monitoreo en segundo plano |
| `TooltipAI.UI` | Interfaz WinUI 3 |
| `TooltipAI.Tray` | Icono en system tray |
| `TooltipAI.Core.Tests` | Tests unitarios |

---

## Build

### Requisitos
- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 (con workload .NET Desktop)

### Compilar
```bash
dotnet build
```

### Publicar
```bash
dotnet publish -r win-x64 --self-contained true -c Release
```

---

## Licencia

MIT License
