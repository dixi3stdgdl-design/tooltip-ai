# Tooltip AI — Code Obfuscation Guide

## Overview

Tooltip AI uses [Obfuscar](https://github.com/lextudio/obfuscar) to protect the core business logic in `TooltipAI.Core.dll` from reverse engineering.

## Protection Features

| Feature | Description |
|---------|-------------|
| **Renamer** | Renames classes/methods to unprintable unicode characters |
| **String Encryption** | AES-256 encryption for all string literals |
| **Control Flow** | Inserts opaque predicates and goto-based control flow mazes |
| **Anti-Tamper** | Detects runtime code modification |
| **Anti-Debug** | Detects attached debuggers |

## Excluded Types

The following types are excluded from obfuscation to maintain JSON serialization compatibility:

- `TooltipAI.Core.Interfaces.*` — All interfaces (DI/serialization)
- `TooltipAI.Core.Models.*` — All data models (JSON contracts)
- `TooltipAI.Core.Auth.*` — Authentication types (token serialization)
- `TooltipAI.Core.Translate.*` — Translation models (API contract)
- `LicenseService` — License validation
- `HybridAiService` — AI service orchestration

## Quick Start

### Local Build with Obfuscation

```bash
# Make scripts executable
chmod +x scripts/*.sh

# Run obfuscation pipeline
./scripts/obfuscate.sh

# Or build all platforms with obfuscation
./scripts/build-all.sh --obfuscate
```

### Docker Build with Obfuscation

```bash
# Build without obfuscation (default)
docker build -t tooltipai-backend .

# Build with obfuscation
docker build --build-arg OBFUSCATE=true -t tooltipai-backend .
```

## Build Pipeline

### 1. Compile Release Build

```bash
dotnet publish TooltipAI.sln -c Release -o publish-clean
```

### 2. Run Obfuscation

```bash
./scripts/obfuscate.sh
```

This will:
1. Create a clean copy of the build output
2. Run Obfuscar on `TooltipAI.Core.dll`
3. Replace the clean DLL with the obfuscated version
4. Generate SHA-256 hashes for integrity verification

### 3. Verify Integrity

```bash
cat SHA256-HASHES.txt
```

## File Structure

```
tooltip-ai/
├── obfuscar.xml              # Obfuscar configuration
├── scripts/
│   ├── obfuscate.sh          # Obfuscation pipeline
│   └── build-all.sh          # Multi-platform build
├── TooltipAI.Backend/
│   └── Dockerfile            # Docker build (supports --build-arg OBFUSCATE=true)
├── publish-clean/            # Original build output (gitignored)
├── publish-obfuscated/       # Obfuscated output (gitignored)
├── publish-final/            # Final distribution (gitignored)
└── SHA256-HASHES.txt         # Integrity hashes (gitignored)
```

## Security Notes

1. **Keys**: The obfuscation key and salt are in `obfuscar.xml`. For production, use environment variables or a secrets manager.

2. **SHA-256 Hashes**: Always verify the hash of `TooltipAI.Core.dll` before deployment to ensure the binary hasn't been tampered with.

3. **Azure Deployment**: The Dockerfile generates a hash file at `/app/SHA256-HASH.txt` inside the container.

4. **CI/CD Integration**:
   ```yaml
   # GitHub Actions example
   - name: Build with obfuscation
     run: docker build --build-arg OBFUSCATE=true -t tooltipai-backend .
   
   - name: Verify hash
     run: docker run --rm tooltipai-backend cat /app/SHA256-HASH.txt
   ```

## Troubleshooting

### "Obfuscar not found"

```bash
dotnet tool install --global Obfuscar.Console --version 2.2.30
export PATH="$PATH:$HOME/.dotnet/tools"
```

### "DLL not found after obfuscation"

Check `obfuscation.log` for errors. Common issues:
- Missing dependency DLLs
- Invalid XML configuration
- Type exclusion conflicts

### "JSON serialization broken"

If Azure backend returns empty objects, check that the affected types are excluded in `obfuscar.xml`:

```xml
<Exclude type="YourNamespace.YourModel" />
```

## License

Internal use only. Part of Tooltip AI commercial software.
