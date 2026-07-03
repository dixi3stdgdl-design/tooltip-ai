#!/bin/bash
# TooltipAI — Verify Backend Deployment + Domain + SSL
# Usage: ./verify-backend.sh

set -euo pipefail

API="https://api.tooltip-ai.com"
WEBAPP="https://tooltip-ai.azurewebsites.net"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

pass() { echo -e "  ${GREEN}✓${NC} $1"; }
fail() { echo -e "  ${RED}✗${NC} $1"; }
warn() { echo -e "  ${YELLOW}⚠${NC} $1"; }

echo "╔══════════════════════════════════════════════════╗"
echo "║  TooltipAI Backend — Verification                ║"
echo "╚══════════════════════════════════════════════════╝"
echo ""

# ── 1. Custom Domain Health ────────────────────────────────────
echo "[1/5] Custom Domain: $API"
if curl -sf "$API/health" > /dev/null 2>&1; then
  pass "Health endpoint reachable"
  RESP=$(curl -sf "$API/health")
  echo "      Response: $RESP"
else
  fail "Health endpoint unreachable — check DNS configuration"
fi
echo ""

# ── 2. SSL Certificate ────────────────────────────────────────
echo "[2/5] SSL Certificate"
CERT_INFO=$(curl -sI "$API" 2>/dev/null | grep -i "strict-transport-security" || true)
if [ -n "$CERT_INFO" ]; then
  pass "HSTS enabled"
else
  warn "HSTS header not found"
fi

TLS=$(curl -sI "$API" 2>/dev/null | grep -i "SSL" || true)
if echo "$TLS" | grep -qi "TLSv1.2\|TLSv1.3"; then
  pass "TLS 1.2+ verified"
else
  warn "Could not verify TLS version"
fi
echo ""

# ── 3. License API ────────────────────────────────────────────
echo "[3/5] License API"
LICENSE_RESP=$(curl -sf -X POST "$API/api/license/validate" \
  -H "Content-Type: application/json" \
  -d '{"machineId":"verify-test","licenseKey":"FREE-001","appVersion":"1.0.0"}' 2>/dev/null || echo '{"error":"failed"}')
if echo "$LICENSE_RESP" | grep -q '"valid"'; then
  pass "License validation working"
  echo "      Response: $LICENSE_RESP"
else
  fail "License API failed"
fi
echo ""

# ── 4. Plugin Registry ────────────────────────────────────────
echo "[4/5] Plugin Registry"
PLUGINS_RESP=$(curl -sf "$API/api/plugins" 2>/dev/null || echo '{"error":"failed"}')
if echo "$PLUGINS_RESP" | grep -q '"id"'; then
  pass "Plugin registry working"
  PLUGIN_COUNT=$(echo "$PLUGINS_RESP" | grep -o '"id"' | wc -l)
  echo "      Plugins available: $PLUGIN_COUNT"
else
  fail "Plugin registry failed"
fi
echo ""

# ── 5. Context Cache ──────────────────────────────────────────
echo "[5/5] Context Cache"
# Set a value
curl -sf -X POST "$API/api/context" \
  -H "Content-Type: application/json" \
  -d '{"key":"verify-test","value":"hello","source":"verify-script"}' > /dev/null 2>&1

# Get it back
CTX_RESP=$(curl -sf "$API/api/context/verify-test" 2>/dev/null || echo '{"error":"failed"}')
if echo "$CTX_RESP" | grep -q '"hello"'; then
  pass "Context cache read/write working"
else
  warn "Context cache returned unexpected response"
fi

STATS=$(curl -sf "$API/api/context/stats" 2>/dev/null || echo '{"error":"failed"}')
if echo "$STATS" | grep -q '"total_entries"'; then
  pass "Context stats endpoint working"
  echo "      Stats: $STATS"
else
  warn "Context stats failed"
fi
echo ""

# ── Summary ────────────────────────────────────────────────────
echo "╔══════════════════════════════════════════════════╗"
echo "║  VERIFICATION COMPLETE                           ║"
echo "╚══════════════════════════════════════════════════╝"
