# Script de prueba del webhook de LemonSqueezy (PowerShell)
# Uso: .\test-webhook.ps1 -BackendUrl http://localhost:5000

param(
    [string]$BackendUrl = "http://localhost:5000"
)

$WebhookUrl = "$BackendUrl/api/webhooks/lemonsqueezy"

Write-Host "=== Probando Webhook de LemonSqueezy ===" -ForegroundColor Green
Write-Host "URL: $WebhookUrl"
Write-Host ""

# Test 1: Health check
Write-Host "1. Probando health check..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$BackendUrl/health" -Method Get
    Write-Host "OK: $($health | ConvertTo-Json -Compress)" -ForegroundColor Green
} catch {
    Write-Host "ERROR: $_" -ForegroundColor Red
}
Write-Host ""

# Test 2: Test endpoint
Write-Host "2. Probando test endpoint..." -ForegroundColor Yellow
try {
    $test = Invoke-RestMethod -Uri "$BackendUrl/api/webhooks/test" -Method Post -Body '{"test":true}' -ContentType "application/json"
    Write-Host "OK: $($test | ConvertTo-Json -Compress)" -ForegroundColor Green
} catch {
    Write-Host "ERROR: $_" -ForegroundColor Red
}
Write-Host ""

# Test 3: order_created
Write-Host "3. Probando order_created..." -ForegroundColor Yellow
$orderPayload = @{
    meta = @{
        event_name = "order_created"
        webhook_id = "test-001"
    }
    data = @{
        id = "order-test-001"
        attributes = @{
            user_email = "test@example.com"
            variant_id = "pro-monthly"
            status = "completed"
            total = 499
        }
    }
} | ConvertTo-Json -Depth 10

try {
    $order = Invoke-RestMethod -Uri $WebhookUrl -Method Post -Body $orderPayload -ContentType "application/json"
    Write-Host "OK: $($order | ConvertTo-Json -Compress)" -ForegroundColor Green
} catch {
    Write-Host "ERROR: $_" -ForegroundColor Red
}
Write-Host ""

# Test 4: subscription_created
Write-Host "4. Probando subscription_created..." -ForegroundColor Yellow
$subPayload = @{
    meta = @{
        event_name = "subscription_created"
        webhook_id = "test-002"
    }
    data = @{
        id = "sub-test-001"
        attributes = @{
            user_email = "subscriber@example.com"
            variant_id = "business-monthly"
            status = "active"
        }
    }
} | ConvertTo-Json -Depth 10

try {
    $sub = Invoke-RestMethod -Uri $WebhookUrl -Method Post -Body $subPayload -ContentType "application/json"
    Write-Host "OK: $($sub | ConvertTo-Json -Compress)" -ForegroundColor Green
} catch {
    Write-Host "ERROR: $_" -ForegroundColor Red
}
Write-Host ""

Write-Host "=== Pruebas completadas ===" -ForegroundColor Green
Write-Host "Revisa los logs del backend para ver los resultados."
