#!/bin/bash

# Script para probar el webhook de LemonSqueezy localmente
# Uso: ./test-webhook.sh [backend-url]

BACKEND_URL="${1:-http://localhost:5000}"
WEBHOOK_URL="$BACKEND_URL/api/webhooks/lemonsqueezy"

echo "=== Probando Webhook de LemonSqueezy ==="
echo "URL: $WEBHOOK_URL"
echo ""

# Test 1: Health check
echo "1. Probando health check..."
curl -s "$BACKEND_URL/health" | jq . 2>/dev/null || curl -s "$BACKEND_URL/health"
echo ""

# Test 2: Test endpoint
echo "2. Probando test endpoint..."
curl -s -X POST "$BACKEND_URL/api/webhooks/test" \
  -H "Content-Type: application/json" \
  -d '{"test": true}' | jq . 2>/dev/null || echo "OK"
echo ""

# Test 3: order_created event
echo "3. Probando evento order_created..."
ORDER_PAYLOAD='{
  "meta": {
    "event_name": "order_created",
    "webhook_id": "test-123"
  },
  "data": {
    "id": "order-test-001",
    "type": "orders",
    "attributes": {
      "user_email": "test@example.com",
      "variant_id": "pro-monthly-variant",
      "status": "completed",
      "total": 499
    }
  }
}'

curl -s -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -d "$ORDER_PAYLOAD" | jq . 2>/dev/null || echo "Sent"
echo ""

# Test 4: subscription_created event
echo "4. Probando evento subscription_created..."
SUB_PAYLOAD='{
  "meta": {
    "event_name": "subscription_created",
    "webhook_id": "test-456"
  },
  "data": {
    "id": "sub-test-001",
    "type": "subscriptions",
    "attributes": {
      "user_email": "subscriber@example.com",
      "variant_id": "business-monthly-variant",
      "status": "active"
    }
  }
}'

curl -s -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -d "$SUB_PAYLOAD" | jq . 2>/dev/null || echo "Sent"
echo ""

# Test 5: subscription_cancelled event
echo "5. Probando evento subscription_cancelled..."
CANCEL_PAYLOAD='{
  "meta": {
    "event_name": "subscription_cancelled",
    "webhook_id": "test-789"
  },
  "data": {
    "id": "sub-test-001",
    "type": "subscriptions",
    "attributes": {
      "ends_at": "2026-08-06T00:00:00Z"
    }
  }
}'

curl -s -X POST "$WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -d "$CANCEL_PAYLOAD" | jq . 2>/dev/null || echo "Sent"
echo ""

echo "=== Pruebas completadas ==="
echo "Revisa los logs del backend para ver los resultados."
