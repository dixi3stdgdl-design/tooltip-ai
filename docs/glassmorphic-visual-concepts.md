# Tooltip AI — Glassmorphic Visual Concepts
## 4 Diseños de Interfaz Manos Libres

---

## 1. Quantum Pill (Isla Dinámica)

**Inspiración:** Dynamic Island de Apple  
**Uso:** Usuarios rápidos que quieren confirmación visual sin perder foco

### Layout
- Píldora simétrica horizontal
- Border-radius: 24px
- Posición: 15px debajo del cursor (eje Y)

### Estética
- Cristal esmerilado oscuro (Mica, 80% blur)
- Borde milimétrico 0.5px gradiente neón: azul → violeta
- Texto interior centrado

### Comportamiento por Voz
- Trigger In: Expansión horizontal sutil
- Texto: Ecualizador de línea fina (waveform) reaccionando a dB
- Trigger Out: Contrae y desaparece

### Render
```
┌─────────────────────────────────────┐
│  ╭─────────────────────────────╮    │
│  │ ▎▎▎▎▎▎▎▎▎▎▎▎▎▎▎▎▎▎▎▎▎▎▎▎▎ │    │  ← Waveform
│  ╰─────────────────────────────╯    │
│         (15px below cursor)         │
└─────────────────────────────────────┘
```

---

## 2. Contextual Aura (Foco Perimetral)

**Inspiración:** Interfaces de ciencia ficción  
**Uso:** Efecto más limpio, integrado en el SO

### Layout
- Marco de brillo difuminado alrededor del elemento UI
- Sin fondo sólido
- Tipografía flotante sobre el elemento

### Estética
- Sombreado radial translúcido e hiper-desenfocado
- Borde perimetral con glow azul
- Tipografía limpia flotando en el aire

### Comportamiento por Voz
- Trigger In: Marco "respira" (pulsación opacidad 40-90%)
- Speaking: Glow más intenso
- Ejecución: Brillo se desplaza como destello hacia el centro

### Render
```
┌─────────────────────────────────────┐
│                                     │
│    ╭───── Glow ─────╮              │
│    │  ┌───────────┐  │              │
│    │  │  BUTTON   │  │  ← Aura     │
│    │  │  EXPORT   │  │              │
│    │  └───────────┘  │              │
│    ╰─────────────────╯              │
│                                     │
│    "Exporta como PDF"  ← Floating   │
│                                     │
└─────────────────────────────────────┘
```

---

## 3. TheBlade (Panel Asimétrico)

**Inspiración:** Paneles premium de diseño  
**Uso:** Enterprise, entornos corporativos

### Layout
- Panel vertical: 280px × 140px
- Esquinas suavizadas (border-radius: 12px)
- Anclado al lado opuesto del cursor

### Estética
- Cristal esmerilado claro (frosted glass puro)
- Tipografía Inter/Segoe UI Variable
- Gris oscuro para contexto, negro profundo para acción

### Comportamiento por Voz
- Trigger In: Barra de progreso translúcida se llena izq → der
- Processing: Muestra micro-icono del producto destino
- Ejecución: Confirmación visual

### Render
```
┌─────────────────────────────────────┐
│  ┌─────────────────────────────┐    │
│  │  Exportar                   │    │
│  │                             │    │
│  │  Botón que permite          │    │
│  │  exportar la hoja actual    │    │
│  │  en múltiples formatos.     │    │
│  │                             │    │
│  │  → Exportar como PDF        │    │
│  │  ████████████░░░░░░░░░░░░░ │    │  ← Progress
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
```

---

## 4. GazeBracket (La Mira Táctica)

**Inspiración:** HUDs militares, cyberpunk minimal  
**Uso:** Developers, hardware modesto, rendimiento extremo

### Layout
- Cuatro esquineros [ ] encuadran el elemento
- Línea de texto translúcida debajo
- Sin texturas pesadas

### Estética
- Monocromático (blanco puro 75% opacidad)
- Estética técnica/cyberpunk
- Composición GDI nativa, cero overhead

### Comportamiento por Voz
- Trigger In: Corchetes se cierran magnéticamente (2px inward)
- Ejecución: Parpadeo único de confirmación
- Trigger Out: Corchetes se abren y desaparecen

### Render
```
┌─────────────────────────────────────┐
│                                     │
│    ┌ ─ ─ ─ ─ ─ ─ ─ ┐              │
│      ┌───────────┐                  │
│    │ │  BUTTON   │ │  ← Brackets   │
│      │  EXPORT   │                  │
│    │ └───────────┘ │                │
│      └ ─ ─ ─ ─ ─ ─ ┘              │
│    "Export"  ← Translucent          │
│                                     │
└─────────────────────────────────────┘
```

---

## Archivos Implementados

| Concepto | Archivo | Renderizador |
|----------|---------|--------------|
| **Quantum Pill** | `QuantumPillRenderer.cs` | GDI+ Graphics |
| **Contextual Aura** | `ContextualAuraRenderer.cs` | GDI+ GraphicsPath |
| **TheBlade** | `TheBladeRenderer.cs` | GDI+ LinearGradientBrush |
| **GazeBracket** | `GazeBracketRenderer.cs` | GDI+ DrawLine |
| **Config/State** | `GlassmorphicStyle.cs` | Modelos compartidos |
| **Renderer Base** | `GlassmorphicRenderer.cs` | Win32 + DWM + GDI+ |

---

## Métricas de Render

| Concepto | GPU | RAM | Tiempo render |
|----------|-----|-----|---------------|
| **Quantum Pill** | Bajo | ~1MB | <1ms |
| **Contextual Aura** | Medio | ~2MB | <2ms |
| **TheBlade** | Medio | ~3MB | <2ms |
| **GazeBracket** | Mínimo | ~0.5MB | <0.5ms |

---

**Documento técnico interno.**  
**Para desarrollo: Dixstdgdl@hotmail.com**
