# Azure Auto-Scale Guide for Tooltip AI Backend

## Overview

This guide configures auto-scaling for the Tooltip AI backend to handle 500+ concurrent users.

## Current Configuration

| Setting | Value |
|---------|-------|
| Rate Limit | 1000 requests/60s per IP |
| Min Instances | 2 |
| Max Instances | 10 |
| Scale Out | CPU > 70% OR HTTP Queue > 100 |
| Scale In | CPU < 30% for 10 min |
| Tier | Standard S1 |

## Estimated Cost

| Component | Cost/month |
|-----------|------------|
| Standard S1 (2 instances) | ~$70 |
| Auto-scale (up to 10) | ~$150 max |
| Redis Cache (optional) | ~$50 |
| **Total** | **$70-270** |

## Quick Setup

### Option 1: Azure CLI Script

```bash
./scripts/azure-autoscale-setup.sh <resource-group> <app-name>
```

### Option 2: Azure Portal

1. Go to Azure Portal > App Services > tooltip-ai
2. Click **Scale out (App Service plan)**
3. Select **Automatic**
4. Set minimum instances: 2
5. Set maximum instances: 10
6. Add scaling rules:
   - CPU > 70% → Add 1 instance
   - HTTP Queue > 100 → Add 1 instance
   - CPU < 30% → Remove 1 instance

### Option 3: ARM Template

```bash
az deployment group create \
  --resource-group <your-rg> \
  --template-file azure-autoscale.json \
  --parameters appServiceName=tooltip-ai
```

## Monitoring

### Check Current Instances

```bash
az webapp show --name tooltip-ai --resource-group <your-rg> --query "instanceCount"
```

### View Auto-Scale History

```bash
az monitor autoscale history list \
  --resource-group <your-rg> \
  --resource tooltip-ai \
  --resource-type Microsoft.Web/sites
```

### Metrics to Watch

| Metric | Warning | Critical |
|--------|---------|----------|
| CPU | > 70% | > 90% |
| Memory | > 80% | > 95% |
| HTTP Queue | > 50 | > 200 |
| Response Time | > 500ms | > 2000ms |

## Load Testing

Before a big launch, test with:

```bash
# Install Apache Bench
apt-get install apache2-utils

# Test 500 concurrent users
ab -n 10000 -c 500 https://api.tooltip-ai.com/health
```

## Troubleshooting

### Auto-scale not triggering
- Check if tier is Standard S1 or higher
- Verify metrics are being collected in Azure Monitor
- Check if rules are enabled

### Instances not scaling in
- Cooldown period may not have elapsed
- CPU may still be above threshold on other instances
- Manual scale-in override may be active

### High costs
- Reduce max instances
- Increase cooldown periods
- Optimize application code
