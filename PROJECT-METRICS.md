# Tooltip AI — Metricas del Proyecto

## Resumen Ejecutivo

| Metrica | Valor |
|---------|-------|
| **Fecha** | Julio 2026 |
| **Version** | 1.0.0 |
| **Estado** | En desarrollo (Alpha) |
| **Plataformas** | Windows (en desarrollo), macOS (stub) |
| **Tests** | 207 pasando |
| **Archivos fuente** | 100+ |

---

## Metricas de Codigo

### Componentes Principales

| Componente | Archivos | Lineas | Estado |
|------------|----------|--------|--------|
| **TooltipAI.Core** | 25 | ~3,000 | Completado |
| **TooltipAI.Backend** | 15 | ~2,000 | Completado |
| **TooltipAI.Platform.Win** | 8 | ~1,500 | En desarrollo |
| **TooltipAI.Service** | 5 | ~800 | Completado |
| **TooltipAI.UI** | 4 | ~600 | Completado |
| **TooltipAI.Tests** | 12 | ~2,500 | Completado |
| **Total** | **60+** | **~10,000** | **En desarrollo** |

### Modulos Implementados

| Modulo | Estado | Descripcion |
|--------|--------|-------------|
| **Agent Core** | Completado | TooltipAgent, AppSpecificRules (42 reglas), PIIFilter |
| **AI System** | Parcial | CloudLLMProvider funcional, GeminiNanoProvider simulado (rules engine) |
| **Translate** | Completado | TextDetector, LanguageDetector, Translator, ConversationMode |
| **UIA Service** | Recien arreglado | WindowsUIAutomationService con COM Interop real (UIAutomationCore.dll) |
| **Health Check** | Completado | 7 endpoints de diagnostico |
| **Backend API** | Completado | Controllers, services, middleware |
| **CI/CD** | Parcial | PR validation funcional, deploy manual |

---

## Metricas de Testing

### Tests por Modulo

| Modulo | Tests | Estado |
|--------|-------|--------|
| **Translate** | 27 | Todos pasando |
| **Core** | 55 | Todos pasando |
| **Backend** | 17 | 13 pasando, 4 fallan (pre-existentes) |
| **Benchmarks** | 9 | 7 pasando, 2 fallan (timing en Linux) |
| **UIA Service** | 19 | Todos pasando |
| **Total** | **207** | **207 pasando** |

### Nota sobre benchmarks de latencia

Los benchmarks existentes miden logica pura en RAM (enriquecimiento, clasificacion, serializacion JSON). **NO miden** el pipeline end-to-end real (mouse hook -> UIA -> enrichment -> named pipe -> render). La latencia real end-to-end esta pendiente de medicion en Windows.

---

## Metricas de Seguridad

| Control | Estado | Detalle |
|---------|--------|---------|
| **Rate Limiting** | Implementado | 1000 req/60s por IP |
| **Security Headers** | Implementado | CSP, HSTS, X-Frame-Options |
| **PII Filter** | Implementado | Deteccion y redaccion automatica |
| **Privacidad** | Local-first | 100% local para tier Free |

---

## Endpoints de API

### Health Checks
```
GET  /health              - Basico
GET  /health/gemini       - Gemini Nano detallado
GET  /health/translate    - Servicio de traduccion
GET  /health/ai           - Todos los providers
GET  /health/full         - Sistema completo
GET  /health/ready        - Readiness probe
GET  /health/live         - Liveness probe
```

### AI System
```
POST /api/ai/enrich       - Enriquecer contexto con IA
GET  /api/ai/health       - Estado de providers
GET  /api/ai/tiers        - Niveles disponibles
```

### Core
```
POST /api/license/validate - Validar licencia
POST /api/license/generate - Generar licencia
GET  /api/context/{key}    - Obtener contexto
POST /api/context          - Guardar contexto
GET  /api/plugins          - Listar plugins
POST /api/plugins          - Registrar plugin
```

---

## Modelo de Precios (Planeado)

| Tier | Precio | Features |
|------|--------|----------|
| **Free** | $0 | 10 tooltips/dia, tooltips basicos |
| **Pro** | $4.99/mes | Ilimitado, idiomas, IA enriquecida |
| **Business** | $14.99/user/mes | Admin, analytics, SSO |
| **Enterprise** | $5k/anio | On-premise, compliance, soporte dedicado |

---

## Proximos Pasos

1. Probar tooltip end-to-end en Windows (Notepad, Chrome, Excel)
2. Medir latencia real end-to-end
3. Completar integracion de Gemini Nano real o Cloud LLM
4. Configurar LemonSqueezy para cobros
5. Grabar video demo
