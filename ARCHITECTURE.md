# TooltipAI Architecture

## System Overview

TooltipAI is a .NET 8 desktop application that provides context-aware tooltips for Windows applications using UI Automation, gaze tracking, and optional AI enrichment.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           TooltipAI System                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐                   │
│  │   Tray App   │    │   UI Layer   │    │   Backend    │                   │
│  │  (TrayIcon)  │◄──►│  (WPF/XAML)  │◄──►│  (ASP.NET)   │                   │
│  └──────────────┘    └──────────────┘    └──────────────┘                   │
│         │                   │                   │                           │
│         ▼                   ▼                   ▼                           │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                        Core Services                                │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐   │   │
│  │  │  Response   │ │  Settings   │ │  License    │ │  Consent    │   │   │
│  │  │  Cache      │ │  Service    │ │  Service    │ │  Manager    │   │   │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘   │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐   │   │
│  │  │  Usage      │ │  App        │ │  Logging    │ │  Crash      │   │   │
│  │  │  Metering   │ │  Blacklist  │ │  Service    │ │  Recovery   │   │   │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘   │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                        AI Layer (Optional)                          │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐                   │   │
│  │  │  Gemini     │ │  Cloud LLM  │ │  AI Router  │                   │   │
│  │  │  Nano       │ │  Provider   │ │  (Hybrid)   │                   │   │
│  │  └─────────────┘ └─────────────┘ └─────────────┘                   │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                     Platform Layer                                  │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐   │   │
│  │  │  Gaze       │ │  Audio      │ │  Speech     │ │  Action     │   │   │
│  │  │  Tracker    │ │  Capture    │ │  to Text    │ │  Executor   │   │   │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘   │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Component Descriptions

### Core Services

| Component | Description |
|-----------|-------------|
| **ResponseCacheService** | SQLite-based LRU cache with TTL for tooltip data. Max 1000 entries with hit/miss metrics. |
| **SettingsService** | JSON file-based app settings with FileSystemWatcher for live reload. |
| **LicenseService** | HMAC-signed license key validation with 14-day trial support. |
| **ConsentManager** | User consent tracking for AI enrichment, telemetry, and app blacklisting. |
| **UsageMeteringService** | Daily usage tracking with configurable limits. |
| **AppBlacklistService** | Process-based app exclusion list with live reload. |
| **LoggingService** | File-based structured logging with queue and periodic flush. |
| **CrashRecoveryService** | Exponential backoff retry logic with failure tracking. |

### AI Layer

| Component | Description |
|-----------|-------------|
| **GeminiNanoProvider** | Local AI provider using Gemini Nano for zero-cost inference. |
| **CloudLLMProvider** | Cloud-based LLM provider (Azure OpenAI) for complex queries. |
| **AIRouter** | Hybrid routing between local and cloud AI based on complexity. |

### Platform Layer

| Component | Description |
|-----------|-------------|
| **GazeTracker** | Eye tracking via Windows Eye Control API with dwell detection. |
| **AudioCapture** | WASAPI-exclusive audio capture in RAM ring buffer. |
| **SpeechToText** | Local speech-to-text processing without cloud APIs. |
| **ActionExecutor** | UI Automation pattern execution for native actions. |

### Backend Services

| Component | Description |
|-----------|-------------|
| **AuthService** | JWT-based authentication with refresh tokens. |
| **TelemetryAggregator** | In-memory telemetry event aggregation and metrics. |
| **PluginRegistryService** | Plugin registration and management. |
| **ContextCacheService** | Backend context caching for distributed scenarios. |

## Data Flow

### Tooltip Display Flow

```
1. Mouse Hover → IMouseHookService captures (x, y)
2. IUIAutomationService.GetElementFromPoint(x, y) → ElementInfo
3. ResponseCacheService.Get(key) → Cache Hit/Miss
4. [Cache Miss] IContextEnricher.GetEnrichedContext(element) → TooltipData
5. [Optional] AI Provider enriches context → Enhanced TooltipData
6. ResponseCacheService.Set(key, data) → Cache Updated
7. IGlassmorphicRenderer.Show(x, y, element) → Overlay Displayed
```

### Gaze Interaction Flow

```
1. IGazeTracker.FocusConfirmed → ElementInfo
2. IAudioCapture.VoiceStopped → byte[] audioBuffer
3. ISpeechToText.TranscribeAsync(audioBuffer) → voiceText
4. GazeContext built from element + voiceText
5. IActionExecutor.ExecuteActionAsync(element, actionToken) → bool
```

### Backend API Flow

```
1. Client Request → RequestLoggingMiddleware
2. SecurityHeadersMiddleware → Security Headers Added
3. RateLimitMiddleware → Rate Limit Check
4. Controller → Service Method
5. Response → Client
```

## Technology Stack

| Layer | Technology |
|-------|------------|
| **Runtime** | .NET 8 |
| **UI** | WPF (XAML) |
| **Backend** | ASP.NET Core |
| **Database** | SQLite (cache), JSON files (settings, licenses) |
| **AI** | Gemini Nano (local), Azure OpenAI (cloud) |
| **Audio** | WASAPI |
| **Speech** | Local STT engine |
| **Platform** | Windows Eye Control API, UI Automation |
| **Testing** | xUnit, FluentAssertions |

## Project Structure

```
TooltipAI/
├── TooltipAI.Core/           # Core services and interfaces
│   ├── AI/                   # AI providers (Gemini, Cloud, Router)
│   ├── Analytics/            # Event tracking
│   ├── Interfaces/           # Core interfaces
│   ├── Models/               # Data models
│   ├── Resources/            # Localized strings
│   ├── Rules/                # App-specific rules
│   ├── Services/             # Core services
│   └── Translate/            # Translation module
├── TooltipAI.Backend/        # ASP.NET Core backend
│   ├── Controllers/          # API controllers
│   ├── Middleware/            # Request pipeline
│   ├── Models/               # API models
│   └── Services/             # Backend services
├── TooltipAI.UI/             # WPF UI layer
│   ├── Views/                # Windows and views
│   └── Rendering/            # Visual renderers
├── TooltipAI.Platform.Win/   # Windows platform services
├── TooltipAI.Platform.Mac/   # macOS platform services
├── TooltipAI.Tray/           # System tray application
└── TooltipAI.Tests/          # Unit and integration tests
```
