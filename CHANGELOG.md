# Changelog

All notable changes to Tooltip AI will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]

## [1.1.0] - 2026-07-08

### Added
- Authentication system with login/signup UI
- Azure backend deployment workflow with custom domain support

### Changed
- Simplified auth — removed JWT middleware, keep in-memory auth

### Fixed
- Health/root endpoint JSON serialization with concrete record types
- Root endpoint syntax error with multi-line array
- Source-gen serialization issues returning plain text JSON

## [1.0.0] - 2026-07-08

### Added
- Initial release — .NET 8 desktop application
- In-memory authentication store
- Health and root monitoring endpoints
