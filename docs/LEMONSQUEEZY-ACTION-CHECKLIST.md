# Checklist de Acciones - LemonSqueezy

## Accion Requerida por ti (Octavio)

### Paso 1: Crear Cuenta (5 minutos)
- [ ] Ir a https://www.lemonsqueezy.com
- [ ] Click "Get Started"
- [ ] Crear cuenta
- [ ] Verificar email

### Paso 2: Configurar Tienda (5 minutos)
- [ ] Ir a Settings > General
- [ ] Nombre: "Tooltip AI"
- [ ] Moneda: USD
- [ ] Website: https://tooltip-ai.com
- [ ] Copiar **Store ID**

### Paso 3: Crear Productos (15 minutos)

**Pro Monthly:**
- [ ] Products > New Product
- [ ] Name: "Tooltip AI Pro (Monthly)"
- [ ] Type: Subscription
- [ ] Price: $4.99
- [ ] Billing: Monthly
- [ ] Copiar **Variant ID**

**Pro Annual:**
- [ ] Products > New Product
- [ ] Name: "Tooltip AI Pro (Annual)"
- [ ] Type: Subscription
- [ ] Price: $47.88
- [ ] Billing: Annual
- [ ] Copiar **Variant ID**

**Business Monthly:**
- [ ] Products > New Product
- [ ] Name: "Tooltip AI Business (Monthly)"
- [ ] Type: Subscription
- [ ] Price: $14.99 per seat
- [ ] Billing: Monthly
- [ ] Copiar **Variant ID**

**Business Annual:**
- [ ] Products > New Product
- [ ] Name: "Tooltip AI Business (Annual)"
- [ ] Type: Subscription
- [ ] Price: $179.88 per seat
- [ ] Billing: Annual
- [ ] Copiar **Variant ID**

### Paso 4: Configurar Webhooks (10 minutos)
- [ ] Settings > Webhooks > Add Endpoint
- [ ] URL: `https://api.tooltip-ai.com/api/webhooks/lemonsqueezy`
- [ ] Seleccionar eventos: order_created, subscription_created, subscription_cancelled
- [ ] Copiar **Webhook Secret**

### Paso 5: Enviar Credenciales
Una vez tengas los IDs, envia:
- Store ID: _______________
- Variant IDs: _______________
- Webhook Secret: _______________

### Paso 6: Yo actualizo el codigo
Con las credenciales, yo actualizo:
- `appsettings.json`
- `pricing.html`
- Hago deploy a Azure

## Tiempo total estimado: ~35 minutos
