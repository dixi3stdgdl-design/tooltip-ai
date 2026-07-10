# TooltipAI Troubleshooting Guide

## Common Issues

### 1. Application Won't Start

**Symptoms:** Application crashes on launch or shows no window.

**Solutions:**
- Check if another instance is already running in Task Manager
- Delete `%APPDATA%\TooltipAI\settings.json` to reset corrupted settings
- Run as Administrator if UI Automation access is denied
- Check Windows Event Viewer for .NET runtime errors

### 2. Tooltips Not Appearing

**Symptoms:** Hovering over UI elements shows no tooltip overlay.

**Solutions:**
- Verify tooltip feature is enabled in Privacy Settings
- Check if the application is in the App Blacklist
- Ensure UI Automation is available (some apps don't support it)
- Try restarting the application

**Debug Steps:**
1. Enable Debug mode in Settings
2. Check logs at `%APPDATA%\TooltipAI\Logs\tooltip_ai_*.log`
3. Look for "Element not found" or "UIA not available" messages

### 3. Cache Issues

**Symptoms:** Tooltips show stale data or performance is slow.

**Solutions:**
- Clear cache: `DELETE FROM cache;` in `%APPDATA%\TooltipAI\cache.db`
- Restart the application to reset in-memory cache
- Check cache stats via Admin API: `GET /api/admin/metrics`

### 4. License Problems

**Symptoms:** "Trial expired" message or license validation fails.

**Solutions:**
- Verify license key format is correct (Base64 encoded)
- Check system clock is correct (license validation uses UTC time)
- Delete `%APPDATA%\TooltipAI\license.dat` to restart trial
- Contact support if purchased license isn't working

### 5. AI Enrichment Not Working

**Symptoms:** Tooltips show basic info only, no AI-generated context.

**Solutions:**
- Verify AI enrichment is enabled in Privacy Settings
- Check API key is configured in Settings (for cloud AI)
- Ensure network connectivity for cloud AI providers
- Check Gemini Nano is installed for local AI

### 6. Gaze Tracking Issues

**Symptoms:** Eye tracking not detecting focus or voice commands not working.

**Solutions:**
- Enable Windows Eye Control in Windows Settings
- Calibrate eye tracking via Windows Eye Control settings
- Check microphone permissions for voice commands
- Verify audio capture is working (check volume levels)

**Debug Steps:**
1. Check logs for "GazeTracker" or "AudioCapture" errors
2. Verify Windows Eye Control API is available
3. Test microphone in Windows Sound Settings

### 7. Performance Issues

**Symptoms:** Application is slow or uses excessive CPU/memory.

**Solutions:**
- Check cache hit rate (should be >80%)
- Reduce tooltip delay in Settings
- Disable unnecessary AI enrichment
- Clear old log files from `%APPDATA%\TooltipAI\Logs\`

**Monitor Metrics:**
- Memory usage: Task Manager → TooltipAI process
- Cache stats: `GET /api/admin/health`
- CPU usage: Check `CPUUsagePercent` in health endpoint

### 8. Plugin Issues

**Symptoms:** Plugins not loading or causing errors.

**Solutions:**
- Verify plugin is compatible with current app version
- Check plugin hash matches registry
- Remove and reinstall problematic plugin
- Check plugin logs for specific errors

## Debug Mode

### Enabling Debug Mode

1. Open TooltipAI Settings
2. Navigate to Advanced section
3. Enable "Debug Mode"
4. Restart application

### Debug Mode Features

- Verbose logging to `%APPDATA%\TooltipAI\Logs\`
- Console window for real-time log output
- UI Automation tree inspection
- Cache hit/miss visualization

### Debug Log Location

```
%APPDATA%\TooltipAI\Logs\tooltip_ai_YYYYMMDD.log
```

Log files are rotated daily and kept for 7 days.

### Viewing Logs

```powershell
# PowerShell - view latest log
Get-Content "$env:APPDATA\TooltipAI\Logs\tooltip_ai_*.log" -Tail 100

# Filter for errors
Select-String -Path "$env:APPDATA\TooltipAI\Logs\tooltip_ai_*.log" -Pattern "Error"
```

## Log File Locations

| File | Location | Purpose |
|------|----------|---------|
| Application Logs | `%APPDATA%\TooltipAI\Logs\tooltip_ai_*.log` | Daily structured logs |
| Cache Database | `%APPDATA%\TooltipAI\cache.db` | SQLite cache |
| Settings | `%APPDATA%\TooltipAI\settings.json` | App configuration |
| License | `%APPDATA%\TooltipAI\license.dat` | License data |
| Consent | `%APPDATA%\TooltipAI\consent.json` | User consent state |
| Usage | `%APPDATA%\TooltipAI\usage.json` | Usage tracking |
| Blacklist | `%APPDATA%\TooltipAI\blacklist.json` | App blacklist |

## Health Check Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /health` | Basic health check (200 OK) |
| `GET /ready` | Readiness probe |
| `GET /api/admin/health` | Detailed system metrics |
| `GET /api/admin/metrics` | Usage metrics |

## Performance Tuning

### Cache Configuration

- **Max Entries:** 1000 (configurable in code)
- **Default TTL:** 5 minutes
- **LRU Eviction:** Enabled (evicts least recently used)
- **Hit Rate Target:** >80%

### Memory Management

- Gaze tracker uses ring buffer for audio (RAM only, no disk)
- Cache uses SQLite with WAL mode for concurrent access
- Log queue flushes every 5 seconds or at 10,000 entries

### Rate Limiting

- Default: 1000 requests per 60 seconds per IP
- Configurable in `Program.cs`

## Getting Help

1. Check this troubleshooting guide
2. Review logs at `%APPDATA%\TooltipAI\Logs\`
3. Run health check: `GET /api/admin/health`
4. File an issue on GitHub with log excerpts
5. Contact support with debug mode logs enabled
