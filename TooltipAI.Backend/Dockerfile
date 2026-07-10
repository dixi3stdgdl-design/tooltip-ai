# ============================================================
# Tooltip AI — Backend Dockerfile
# ============================================================
# Build normal:
#   docker build -t tooltipai-backend .
#
# Build con ofuscación:
#   docker build --build-arg OBFUSCATE=true -t tooltipai-backend .
# ============================================================

# ─── Build stage ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG OBFUSCATE=false

WORKDIR /src

# Copy csproj files first for better layer caching
COPY TooltipAI.Core/TooltipAI.Core.csproj ./TooltipAI.Core/
COPY TooltipAI.Backend/TooltipAI.Backend.csproj ./TooltipAI.Backend/

# Restore dependencies
RUN dotnet restore TooltipAI.Backend/TooltipAI.Backend.csproj -r linux-x64

# Copy all source code
COPY TooltipAI.Core/ ./TooltipAI.Core/
COPY TooltipAI.Backend/ ./TooltipAI.Backend/

# Publish
RUN dotnet publish TooltipAI.Backend/TooltipAI.Backend.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained \
    -o /app/publish \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true

# ─── Obfuscation stage (optional) ──────────────────────────
FROM build AS obfuscate

ARG OBFUSCATE=false

RUN if [ "$OBFUSCATE" = "true" ]; then \
        echo "=== Installing Obfuscar ===" && \
        dotnet tool install --global Obfuscar.Console --version 2.2.30 && \
        export PATH="$PATH:$HOME/.dotnet/tools" && \
        \
        echo "=== Preparing obfuscation ===" && \
        mkdir -p /obfuscate-temp/output && \
        cp /app/publish/TooltipAI.Core.dll /obfuscate-temp/ && \
        \
        # Copy dependencies
        for dll in /app/publish/Microsoft.Extensions.*.dll /app/publish/System.*.dll; do \
            [ -f "$dll" ] && cp "$dll" /obfuscate-temp/ 2>/dev/null || true; \
        done && \
        \
        # Create obfuscar config
        cat > /obfuscate-temp/obfuscar.xml << 'XMLCONFIG'
<?xml version="1.0" encoding="utf-8"?>
<Obfuscator>
  <Input file="TooltipAI.Core.dll" />
  <OutputDirectory>output</OutputDirectory>
  
  <Module>
    <Attribute xml-name="Obfuscator.ConfigurationAttribute" feature="true" />
    <Attribute xml-name="Obfuscator.Attribute" feature="true" />
  </Module>
  
  <StringEncryption enabled="true" dynamic="true" aes="true"
                    key="2026-TooltipAI-Obfuscation-Key-Production"
                    salt="TooltipAI-Core-Salt-v1" />
  
  <ControlFlow enabled="true" />
  
  <Rules>
    <ExcludeNamespace name="TooltipAI.Core.Interfaces" />
    <ExcludeNamespace name="TooltipAI.Core.Models" />
    <ExcludeNamespace name="TooltipAI.Core.Auth" />
    <ExcludeNamespace name="TooltipAI.Core.Translate" />
    <Exclude type="TooltipAI.Core.Services.LicenseService" />
    <Exclude type="TooltipAI.Core.Services.HybridAiService" />
  </Rules>
  
  <AntiTamper enabled="true" />
  <AntiDebug enabled="true" />
</Obfuscator>
XMLCONFIG
        \
        echo "=== Running obfuscation ===" && \
        cd /obfuscate-temp && \
        dotnet obfuscar obfuscar.xml && \
        \
        echo "=== Replacing DLL ===" && \
        cp /obfuscate-temp/output/TooltipAI.Core.dll /app/publish/TooltipAI.Core.dll && \
        echo "=== Obfuscation completed ===" || \
        (echo "=== Obfuscation failed, using clean DLL ===" && exit 1); \
    else \
        echo "=== Skipping obfuscation (OBFUSCATE=false) ==="; \
    fi

# ─── Final stage — Debian-based for compatibility ──────────
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0 AS final

LABEL maintainer="Tooltip AI Team"
LABEL version="1.0.0"

# Install wget for health check
RUN apt-get update && apt-get install -y --no-install-recommends wget && \
    rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r tooltipai && useradd -r -g tooltipai -m tooltipai

WORKDIR /app

# Copy published binaries
COPY --from=build /app/publish .

# Generate SHA-256 hash for integrity verification
RUN sha256sum TooltipAI.Core.dll > /app/SHA256-HASH.txt && \
    chown -R tooltipai:tooltipai /app

# Switch to non-root user
USER tooltipai

# Expose port
EXPOSE 8080

# Environment
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["./TooltipAI.Backend"]
