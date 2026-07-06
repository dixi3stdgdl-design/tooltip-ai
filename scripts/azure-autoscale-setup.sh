#!/bin/bash
# ============================================================
# Tooltip AI — Azure Auto-Scale Configuration
# ============================================================
# Configura auto-scale para Azure App Service
# Uso: ./azure-autoscale-setup.sh <resource-group> <app-name>

set -e

RESOURCE_GROUP="${1:-tooltip-ai-rg}"
APP_NAME="${2:-tooltip-ai}"

echo ""
echo "╔══════════════════════════════════════════════════════════╗"
echo "║       Tooltip AI — Azure Auto-Scale Setup               ║"
echo "╚══════════════════════════════════════════════════════════╝"
echo ""

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo "ERROR: Azure CLI not found. Install from https://docs.microsoft.com/cli/azure/install-azure-cli"
    exit 1
fi

# Check if logged in
if ! az account show &> /dev/null; then
    echo "Not logged in. Please run: az login"
    exit 1
fi

echo "Resource Group: $RESOURCE_GROUP"
echo "App Name: $APP_NAME"
echo ""

# Step 1: Upgrade to Standard tier (required for auto-scale)
echo "[1/5] Upgrading to Standard S1 tier..."
az webapp update \
    --name "$APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --sku S1 \
    --output none 2>/dev/null || echo "  (may already be on S1 or higher)"

# Step 2: Configure auto-scale profile
echo "[2/5] Creating auto-scale profile..."
az monitor autoscale create \
    --resource-group "$RESOURCE_GROUP" \
    --resource "$APP_NAME" \
    --resource-type "Microsoft.Web/sites" \
    --name "${APP_NAME}-autoscale" \
    --min-count 2 \
    --max-count 10 \
    --output none 2>/dev/null || echo "  (profile may already exist)"

# Step 3: Scale out rule (add instance when CPU > 70%)
echo "[3/5] Adding scale-out rule (CPU > 70%)..."
az monitor autoscale rule create \
    --resource-group "$RESOURCE_GROUP" \
    --autoscale-name "${APP_NAME}-autoscale" \
    --condition "Cpu > 70 avg 5m" \
    --scale out 1 \
    --cooldown 5 \
    --output none 2>/dev/null || echo "  (rule may already exist)"

# Step 4: Scale out rule (add instance when HTTP queue > 100)
echo "[4/5] Adding scale-out rule (HTTP Queue > 100)..."
az monitor autoscale rule create \
    --resource-group "$RESOURCE_GROUP" \
    --autoscale-name "${APP_NAME}-autoscale" \
    --condition "HttpQueueLength > 100 avg 5m" \
    --scale out 1 \
    --cooldown 5 \
    --output none 2>/dev/null || echo "  (rule may already exist)"

# Step 5: Scale in rule (remove instance when CPU < 30%)
echo "[5/5] Adding scale-in rule (CPU < 30%)..."
az monitor autoscale rule create \
    --resource-group "$RESOURCE_GROUP" \
    --autoscale-name "${APP_NAME}-autoscale" \
    --condition "Cpu < 30 avg 10m" \
    --scale in 1 \
    --cooldown 10 \
    --output none 2>/dev/null || echo "  (rule may already exist)"

echo ""
echo "========================================"
echo "Auto-scale configured successfully!"
echo "========================================"
echo ""
echo "Configuration:"
echo "  - Min instances: 2"
echo "  - Max instances: 10"
echo "  - Scale out: CPU > 70% OR HTTP Queue > 100"
echo "  - Scale in: CPU < 30%"
echo ""
echo "Estimated cost: $50-150/month (Standard S1)"
echo ""
echo "Verify with:"
echo "  az monitor autoscale show --resource-group $RESOURCE_GROUP --resource $APP_NAME --resource-type Microsoft.Web/sites"
echo ""
