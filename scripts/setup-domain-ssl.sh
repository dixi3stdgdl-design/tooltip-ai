#!/bin/bash
# TooltipAI — Setup Domain + SSL for Backend API
# Requires: Azure CLI logged in, domain tooltip-ai.com registered
#
# Usage: ./setup-domain-ssl.sh [resource-group]
# Default resource-group: tooltip-ai-rg

set -euo pipefail

RG="${1:-tooltip-ai-rg}"
APP="tooltip-ai"
HOSTNAME="api.tooltip-ai.com"
CERT_NAME="tooltipai-prod-cert"

echo "╔══════════════════════════════════════════════════╗"
echo "║  TooltipAI — Domain + SSL Setup (Azure)         ║"
echo "╚══════════════════════════════════════════════════╝"
echo ""
echo "  Resource Group : $RG"
echo "  Web App        : $APP"
echo "  Custom Domain  : $HOSTNAME"
echo ""

# ── 0. Pre-flight checks ──────────────────────────────────────
if ! command -v az &>/dev/null; then
  echo "[!] Azure CLI not found. Installing..."
  curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
fi

if ! az account show &>/dev/null; then
  echo "[*] Logging in to Azure..."
  az login
fi

echo "[1/7] Verifying Web App exists..."
if ! az webapp show -g "$RG" -n "$APP" &>/dev/null; then
  echo "[!] Web App '$APP' not found in resource group '$RG'"
  echo "    Create it first: az webapp create -g $RG -n $APP --plan <app-service-plan>"
  exit 1
fi
echo "  ✓ Web App found"

# ── 1. Get outbound IPs ───────────────────────────────────────
echo ""
echo "[2/7] Getting Web App outbound IPs..."
IPS=$(az webapp show -g "$RG" -n "$APP" --query "outboundIpAddresses" -o tsv)
IP1=$(echo "$IPS" | cut -d',' -f1)
echo "  Outbound IPs: $IPS"
echo "  Primary IP:   $IP1"
echo ""

# ── 2. Enable container support ───────────────────────────────
echo "[3/7] Configuring Web App for containers..."
az webapp config set -g "$RG" -n "$APP" --linux-fx-version "DOCKER|mcr.microsoft.com/azurelinux-beta/base/core:4.0" 2>/dev/null || true
az webapp update -g "$RG" -n "$APP" --set httpsOnly=true
echo "  ✓ HTTPS enforced"

# ── 3. Add custom domain ──────────────────────────────────────
echo ""
echo "[4/7] Adding custom domain: $HOSTNAME..."
az webapp config hostname add -g "$RG" --webapp-name "$APP" --hostname "$HOSTNAME" 2>/dev/null || echo "  (may already be added)"
echo "  ✓ Domain added"

# ── 4. Create managed SSL certificate ─────────────────────────
echo ""
echo "[5/7] Creating managed SSL certificate..."
az webapp config ssl create \
  -g "$RG" \
  -n "$APP" \
  --hostname "$HOSTNAME" \
  --cert-name "$CERT_NAME" 2>/dev/null || echo "  (certificate may already exist)"
echo "  ✓ Certificate created"

# ── 5. Bind SSL certificate ───────────────────────────────────
echo ""
echo "[6/7] Binding SSL certificate..."
THUMB=$(az webapp config ssl list -g "$RG" -n "$APP" \
  --query "[?name=='$CERT_NAME'].thumbprint" -o tsv 2>/dev/null)

if [ -n "$THUMB" ]; then
  az webapp config ssl bind \
    -g "$RG" \
    -n "$APP" \
    --certificate-thumbnail-thumbprint "$THUMB" \
    --hostname "$HOSTNAME" \
    --ssl-type SNI
  echo "  ✓ SSL bound (thumbprint: ${THUMB:0:12}...)"
else
  echo "  ⚠ Could not get thumbprint. Check Azure Portal."
fi

# ── 6. Set TLS minimum version ────────────────────────────────
echo ""
echo "[7/7] Enforcing TLS 1.2 minimum..."
az webapp config set -g "$RG" -n "$APP" --min-tls-version 1.2
echo "  ✓ TLS 1.2+ enforced"

# ── Summary ────────────────────────────────────────────────────
echo ""
echo "╔══════════════════════════════════════════════════╗"
echo "║  SETUP COMPLETE                                  ║"
echo "╚══════════════════════════════════════════════════╝"
echo ""
echo "  DNS Record (add at your domain registrar):"
echo "  ┌──────────┬──────────────────────────────────────┐"
echo "  │ Type     │ CNAME                                │"
echo "  │ Host     │ api                                  │"
echo "  │ Value    │ $APP.azurewebsites.net               │"
echo "  │ TTL      │ 3600                                 │"
echo "  └──────────┴──────────────────────────────────────┘"
echo ""
echo "  Alternative (if CNAME not supported):"
echo "  ┌──────────┬──────────────────────────────────────┐"
echo "  │ Type     │ A                                    │"
echo "  │ Host     │ api                                  │"
echo "  │ Value    │ $IP1                                 │"
echo "  │ TTL      │ 3600                                 │"
echo "  └──────────┴──────────────────────────────────────┘"
echo ""
echo "  After DNS propagation (5-30 min), verify:"
echo "    curl -I https://$HOSTNAME/health"
echo ""
echo "  API Endpoints:"
echo "    GET  https://$HOSTNAME/health"
echo "    POST https://$HOSTNAME/api/license/validate"
echo "    GET  https://$HOSTNAME/api/plugins"
echo "    GET  https://$HOSTNAME/api/context/stats"
echo ""
echo "  Azure Portal:"
echo "    https://portal.azure.com/#@/resource/subscriptions/87A5E93DC4384FD192D1298AEEB4D9C7/resourceGroups/$RG/providers/Microsoft.Web/sites/$APP"
