# Guia de Prueba del Webhook de LemonSqueezy

## Prueba Local con ngrok

### 1. Instalar ngrok

```bash
# Mac
brew install ngrok

# Linux/Windows
npm install -g ngrok
```

### 2. Iniciar el Backend

```bash
cd src/tooltip-ai
dotnet run --project TooltipAI.Backend
```

### 3. Exponer el puerto con ngrok

```bash
ngrok http 5000
```

Copiar la URL publica (ej: `https://abc123.ngrok.io`)

### 4. Configurar en LemonSqueezy

1. Ir a **Settings > Webhooks**
2. Click **Add Endpoint**
3. Pegar la URL de ngrok + `/api/webhooks/lemonsqueezy`
4. Seleccionar eventos
5. Copiar el **Webhook Secret**

### 5. Actualizar appsettings.json

```json
{
  "LemonSqueezy": {
    "WebhookSecret": "EL_SECRET_QUE_COPIASTE"
  }
}
```

---

## Prueba con curl (Sin LemonSqueezy)

### Health Check
```bash
curl http://localhost:5000/health
```

### Test Endpoint
```bash
curl -X POST http://localhost:5000/api/webhooks/test \
  -H "Content-Type: application/json" \
  -d '{"test": true}'
```

### Simular order_created
```bash
curl -X POST http://localhost:5000/api/webhooks/lemonsqueezy \
  -H "Content-Type: application/json" \
  -d '{
    "meta": {
      "event_name": "order_created",
      "webhook_id": "test-001"
    },
    "data": {
      "id": "order-123",
      "attributes": {
        "user_email": "test@example.com",
        "variant_id": "pro-monthly",
        "status": "completed",
        "total": 499
      }
    }
  }'
```

### Simular subscription_created
```bash
curl -X POST http://localhost:5000/api/webhooks/lemonsqueezy \
  -H "Content-Type: application/json" \
  -d '{
    "meta": {
      "event_name": "subscription_created",
      "webhook_id": "test-002"
    },
    "data": {
      "id": "sub-456",
      "attributes": {
        "user_email": "user@example.com",
        "variant_id": "business-monthly",
        "status": "active"
      }
    }
  }'
```

### Simular subscription_cancelled
```bash
curl -X POST http://localhost:5000/api/webhooks/lemonsqueezy \
  -H "Content-Type: application/json" \
  -d '{
    "meta": {
      "event_name": "subscription_cancelled",
      "webhook_id": "test-003"
    },
    "data": {
      "id": "sub-456",
      "attributes": {
        "ends_at": "2026-08-06T00:00:00Z"
      }
    }
  }'
```

---

## Verificar en LemonSqueezy Dashboard

1. Ir a **Settings > Webhooks**
2. Click en el endpoint creado
3. Ver **Recent Deliveries**
4. Verificar que los eventos muestran **200 OK**

---

## Prueba Completa con Script

```bash
# Ejecutar todas las pruebas
./scripts/test-webhook.sh http://localhost:5000
```

---

## Troubleshooting

### Error 401 Unauthorized
- Verificar que el Webhook Secret sea correcto
- Asegurarse de que no haya espacios extra

### Error 500 Internal Server Error
- Revisar logs del backend
- Verificar que la BD este accesible

### No llegan eventos
- Verificar que ngrok este corriendo
- Revisar que la URL sea correcta en LemonSqueezy
- Verificar que los eventos esten seleccionados

### Webhook no verifica firma
- El secret debe coincidir exactamente
- No usar el signing secret de la URL, sino el de la configuracion
