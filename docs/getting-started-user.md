# Tooltip AI — User Guide

Smart tooltips for Windows, macOS, and Linux. Real-time contextual information in every application.

---

## Installation

### Windows

1. Download the latest installer from [Releases](https://github.com/dixi3stdgdl-design/tooltip-ai/releases).
2. Run the `.msi` or `.exe` installer and follow the prompts.
3. Tooltip AI will start automatically in the system tray.

### macOS

1. Download the `.dmg` for your architecture (Apple Silicon or Intel).
2. Open the `.dmg` and drag **Tooltip AI** to your Applications folder.
3. On first launch, macOS will prompt for Accessibility permissions:
   - Go to **System Preferences > Privacy & Security > Accessibility**.
   - Click the lock icon and enter your password.
   - Enable **Tooltip AI** in the list.
4. Tooltip AI will appear in the menu bar.

### Linux

Linux support is planned. Check the repository for updates.

---

## First Launch

1. Tooltip AI starts in the system tray (Windows) or menu bar (macOS).
2. Move your mouse over any UI element — a tooltip appears with contextual information about that element.
3. The tooltip shows the element name, type, and enriched context (keyboard shortcuts, usage tips, etc.).

---

## Configuration

Settings are stored at:

- **Windows:** `%APPDATA%\TooltipAI\settings.json`
- **macOS:** `~/Library/Application Support/TooltipAI/settings.json`

### Settings Reference

| Setting | Default | Description |
|---------|---------|-------------|
| `IsEnabled` | `true` | Enable or disable tooltips globally |
| `ShowAiContext` | `true` | Show AI-enriched context in tooltips |
| `TooltipDelayMs` | `100` | Delay before tooltip appears (milliseconds) |
| `TooltipMaxWidth` | `400` | Maximum tooltip width in pixels |
| `TooltipMaxHeight` | `250` | Maximum tooltip height in pixels |
| `Theme` | `"System"` | Visual theme (`"System"`, `"Light"`, `"Dark"`) |
| `Language` | `"en"` | Interface language |
| `EnableNotifications` | `true` | Show desktop notifications |
| `EnableSound` | `false` | Play sound on tooltip |
| `EnableTelemetry` | `false` | Send anonymous usage data |

### Editing Settings

Edit `settings.json` directly in any text editor. Changes are applied automatically — no restart required.

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+Shift+T` | Toggle Tooltip AI on/off |

Additional shortcuts may be shown in tooltips for the application you are currently using (e.g., `Ctrl+S` for Save, `Ctrl+P` for Command Palette).

---

## Pricing Tiers

| Tier | Price | Features |
|------|-------|----------|
| **Free** | $0 | 10 tooltips/day, basic element info |
| **Pro** | $4.99/mo | Unlimited, 50+ languages, AI enrichment |
| **Business** | $14.99/user/mo | Admin dashboard, analytics, SSO |
| **Enterprise** | $5k/year | On-premise, compliance, dedicated support |

---

## Troubleshooting

### Tooltips don't appear

- **Windows:** Ensure Tooltip AI is running (check system tray). If blocked by antivirus, add an exception for `TooltipAI.Service.exe`.
- **macOS:** Verify Accessibility permissions are granted in **System Preferences > Privacy & Security > Accessibility**.

### Tooltip is slow or delayed

Increase `TooltipDelayMs` in settings to give the UI automation layer more time, or decrease it for faster response on high-performance hardware.

### "Failed to create CGEventTap" (macOS)

This means Accessibility permissions are not granted. Go to **System Preferences > Privacy & Security > Accessibility** and enable Tooltip AI.

### Backend connection errors

If using Pro/Business features, ensure the backend API is reachable. Check `ApiEndpoint` and `ApiKey` in your settings file.

### Reset to defaults

Delete `settings.json` or set all values to defaults. Tooltip AI will recreate it on next launch.

---

## Support

- **Issues:** [GitHub Issues](https://github.com/dixi3stdgdl-design/tooltip-ai/issues)
- **Email:** hello@mimo.dev
- **GitHub:** [@dixi3stdgdl-design](https://github.com/dixi3stdgdl-design)
