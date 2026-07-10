# LemonSqueezy - Configuracion Completa para Tooltip AI

## 1. Crear Cuenta

1. Ir a https://www.lemonsqueezy.com
2. Click "Get Started"
3. Crear cuenta con email corporativo
4. Verificar email

## 2. Configuracion de Tienda

### Datos de la Tienda
```
Store Name: Tooltip AI
Currency: USD
Country: Mexico
Website: https://tooltip-ai.com
Support Email: support@tooltip-ai.com
```

### Obtener Store ID
1. Ir a Settings > General
2. Copiar **Store ID** (formato: `store_xxxxx`)

## 3. Crear Productos

### 3.1 Pro Monthly
```
Name: Tooltip AI Pro (Monthly)
Type: Subscription
Price: $4.99 USD
Billing: Monthly
Description: Unlimited tooltips, AI enrichment, keyboard shortcuts, custom themes
```

### 3.2 Pro Annual
```
Name: Tooltip AI Pro (Annual)
Type: Subscription
Price: $47.88 USD (equivalent to $3.99/month, 20% off $4.99)
Billing: Annual
Description: Same as Pro Monthly with 20% discount
```

### 3.3 Business Monthly
```
Name: Tooltip AI Business (Monthly)
Type: Subscription
Price: $14.99 USD per seat
Billing: Monthly
Description: Team features, admin dashboard, SSO, analytics
```

### 3.3 Business Annual
```
Name: Tooltip AI Business (Annual)
Type: Subscription
Price: $179.88 USD per seat (equivalent to $14.99/month)
Billing: Annual
Description: Same as Business Monthly with 20% discount
```

## 4. Obtener Variant IDs

Para cada producto:
1. Ir a Products
2. Click en el producto
3. Copiar **Variant ID** (formato: `variant_xxxxx`)

### Mapeo de Variant IDs
```json
{
  "pro_monthly": "variant_xxxxx",
  "pro_annual": "variant_xxxxx",
  "business_monthly": "variant_xxxxx",
  "business_annual": "variant_xxxxx"
}
```

## 5. Configurar Webhooks

### 5.1 Crear Endpoint
1. Ir a Settings > Webhooks
2. Click "Add Endpoint"
3. URL: `https://api.tooltip-ai.com/api/webhooks/lemonsqueezy`
4. Seleccionar eventos:
   - [x] order_created
   - [x] order_updated
   - [x] subscription_created
   - [x] subscription_updated
   - [x] subscription_cancelled
   - [x] subscription_expired
5. Copiar **Webhook Secret**

### 5.2 Webhook Secret
```
Webhook Secret: whsec_xxxxx
```

## 6. Configurar en Backend

### appsettings.json
```json
{
  "LemonSqueezy": {
    "WebhookSecret": "whsec_xxxxx",
    "StoreId": "store_xxxxx",
    "ApiKey": "ls_xxxxx"
  }
}
```

### Environment Variables (Azure)
```bash
LemonSqueezy__WebhookSecret=whsec_xxxxx
LemonSqueezy__StoreId=store_xxxxx
LemonSqueezy__ApiKey=ls_xxxxx
```

## 7. Configurar en Landing Page

### pricing.html
```javascript
const LEMON_CONFIG = {
  store: 'store_xxxxx',
  variants: {
    pro_monthly: 'variant_xxxxx',
    pro_annual: 'variant_xxxxx',
    business_monthly: 'variant_xxxxx',
    business_annual: 'variant_xxxxx'
  }
};
```

## 8. Probar Webhooks

### Local con ngrok
```bash
# Terminal 1: Backend
dotnet run --project TooltipAI.Backend

# Terminal 2: ngrok
ngrok http 5000

# Copiar URL de ngrok y configurar en LemonSqueezy
```

### Script de prueba
```bash
./scripts/test-webhook.sh http://localhost:5000
```

## 9. Checklist

- [ ] Cuenta creada en LemonSqueezy
- [ ] Store ID copiado
- [ ] 4 productos creados
- [ ] Variant IDs copiados
- [ ] Webhook endpoint configurado
- [ ] Webhook Secret copiado
- [ ] appsettings.json actualizado
- [ ] pricing.html actualizado
- [ ] Webhook probado con ngrok
- [ ] Deploy a Azure
