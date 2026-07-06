# ─── Build stage ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_VERSION=1.0.0
ARG BUILD_DATE=unknown
ARG OBFUSCATION=false

WORKDIR /src

COPY TooltipAI.Backend/TooltipAI.Backend.csproj ./TooltipAI.Backend/
RUN dotnet restore TooltipAI.Backend/TooltipAI.Backend.csproj

COPY TooltipAI.Backend/ ./TooltipAI.Backend/
RUN dotnet publish TooltipAI.Backend/TooltipAI.Backend.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained \
    -o /app/publish \
    --no-restore \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true

# ─── Final stage — Minimal runtime image ───────────────────────
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS final

LABEL maintainer="Tooltip AI Team"
LABEL version="${BUILD_VERSION}"
LABEL build-date="${BUILD_DATE}"

# Create non-root user
RUN addgroup -S tooltipai && adduser -S tooltipai -G tooltipai

WORKDIR /app

# Copy published binaries (self-contained, no .NET runtime needed)
COPY --from=build /app/publish .

# Set ownership and permissions
RUN chown -R tooltipai:tooltipai /app && \
    chmod -R 550 /app

# Switch to non-root user
USER tooltipai

# Expose only port 8080
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

# Health check — 30s interval, 3s timeout, 3 retries
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Entry point — self-contained, no .NET runtime needed
ENTRYPOINT ["./TooltipAI.Backend"]
