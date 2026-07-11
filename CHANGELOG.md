# Changelog

All notable changes to Tooltip AI will be documented in this file.

## [Unreleased]

### Fixed
- WindowsUIAutomationService: replaced broken P/Invoke with real COM Interop via UIAutomationCore.dll
- DI chain: reflection-based platform resolution in Service Program.cs
- Namespace disambiguation for ElementInfo and SoftwareCategory in Core
- LicenseWindow/PrivacySettingsWindow OnClosed method for WinUI 3 compatibility
- Removed inflated metrics and fake claims from documentation

### Added
- UIAutomationInterop.cs with IUIAutomation/IUIAutomationElement COM interfaces
- 19 unit tests for UIA service and control type mapping
- Build scripts for Windows (BUILD-AND-RUN.bat, test-tooltip-windows.ps1)

## [1.1.0] - 2026-07-08

### Added
- Authentication system with login/signup UI
- Azure backend deployment workflow with custom domain support

## [1.0.0] - 2026-07-08

### Added
- Initial release — .NET 8 desktop application
- Mouse hook (WH_MOUSE_LL) for cursor tracking
- Named Pipe IPC between Service and UI
- WinUI 3 click-through overlay
- Software category classifier (16 categories)
- Local context enricher (rule-based)
- Backend API (licensing, context cache, plugins)
