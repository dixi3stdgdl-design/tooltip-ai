# LemonSqueezy Webhook Setup Guide

## Overview

This guide explains how to set up LemonSqueezy webhooks for Tooltip AI to automatically handle payments, license generation, and subscription management.

## Prerequisites

1. A LemonSqueezy account (https://www.lemonsqueezy.com)
2. A store created in LemonSqueezy
3. Products and variants configured
4. Backend deployed with HTTPS

## Step 1: Create Products in LemonSqueezy

### Pro Plan (Monthly)
- Name: Tooltip AI Pro (Monthly)
- Price: $4.99/month
- Billing: Monthly

### Pro Plan (Annual)
- Name: Tooltip AI Pro (Annual)
- Price: $3.99/month (billed annually)
- Billing: Annual

### Business Plan (Monthly)
- Name: Tooltip AI Business (Monthly)
- Price: $14.99/user/month
- Billing: Monthly

### Business Plan (Annual)
- Name: Tooltip AI Business (Annual)
- Price: $11.99/user/month (billed annually)
- Billing: Annual

## Step 2: Get Your Store ID

1. Go to **Settings > General** in LemonSqueezy dashboard
2. Copy your **Store ID**

## Step 3: Get Variant IDs

1. Go to **Products** in LemonSqueezy dashboard
2. Click on each product
3. Copy the **Variant ID** for each plan

## Step 4: Configure Webhook Endpoint

### In LemonSqueezy Dashboard:

1. Go to **Settings > Webhooks**
2. Click **Add Endpoint**
3. Enter your webhook URL: `https://your-domain.com/api/webhooks/lemonsqueezy`
4. Select the following events:
   - `order_created`
   - `order_updated`
   - `order_expired`
   - `subscription_created`
   - `subscription_updated`
   - `subscription_cancelled`
   - `subscription_resumed`
   - `subscription_expired`
5. Copy the **Webhook Secret**

## Step 5: Configure Backend

### Update appsettings.json:

```json
{
  "LemonSqueezy": {
    "WebhookSecret": "YOUR_WEBHOOK_SECRET",
    "StoreId": "YOUR_STORE_ID",
    "ApiKey": "YOUR_API_KEY"
  }
}
```

### Update pricing.html:

Replace the placeholder values in the LemonSqueezy configuration:

```javascript
const LEMON_CONFIG = {
    store: 'YOUR_STORE_ID',
    variants: {
        pro_monthly: 'YOUR_PRO_MONTHLY_VARIANT_ID',
        pro_annual: 'YOUR_PRO_ANNUAL_VARIANT_ID',
        business_monthly: 'YOUR_BUSINESS_MONTHLY_VARIANT_ID',
        business_annual: 'YOUR_BUSINESS_ANNUAL_VARIANT_ID'
    }
};
```

## Step 6: Test Webhooks

### Local Testing with ngrok:

```bash
# Install ngrok
npm install -g ngrok

# Start your backend
dotnet run

# Expose local server
ngrok http 5000

# Use the ngrok URL in LemonSqueezy webhook settings
# Example: https://abc123.ngrok.io/api/webhooks/lemonsqueezy
```

### Test Webhook Payload:

```bash
curl -X POST https://your-domain.com/api/webhooks/test \
  -H "Content-Type: application/json" \
  -d '{"test": true}'
```

## Step 7: Verify Webhook Signature

The webhook controller verifies the `X-Signature` header using HMAC-SHA256. Make sure:

1. The webhook secret is correctly configured
2. The secret matches what's in LemonSqueezy dashboard
3. The signature verification is enabled (not in dev mode)

## Webhook Events Handled

| Event | Action |
|-------|--------|
| `order_created` | Generate license key for completed orders |
| `order_updated` | Handle refunds (revoke license) |
| `order_expired` | Log expiration |
| `subscription_created` | Generate license for active subscriptions |
| `subscription_updated` | Handle status changes (active, past_due) |
| `subscription_cancelled` | Schedule deactivation at period end |
| `subscription_resumed` | Reactivate license |
| `subscription_expired` | Deactivate license |

## Troubleshooting

### Webhooks not received:
1. Check webhook URL is correct and accessible
2. Verify HTTPS is working
3. Check LemonSqueezy webhook logs for errors

### Signature verification fails:
1. Ensure webhook secret matches
2. Check for extra whitespace in the secret
3. Verify the body is not modified before verification

### License not generated:
1. Check backend logs for errors
2. Verify the order status is "completed"
3. Ensure variant ID mapping is correct

## Production Checklist

- [ ] Webhook secret is secure (not in source control)
- [ ] HTTPS is enabled
- [ ] Webhook endpoint is accessible
- [ ] Variant IDs are correctly mapped
- [ ] License generation is working
- [ ] Subscription lifecycle is handled
- [ ] Error logging is configured
