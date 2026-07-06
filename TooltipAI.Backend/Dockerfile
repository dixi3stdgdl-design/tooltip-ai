# ─── Build stage ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy csproj and restore as distinct layers
COPY TooltipAI.Backend/TooltipAI.Backend.csproj ./TooltipAI.Backend/
RUN dotnet restore TooltipAI.Backend/TooltipAI.Backend.csproj -r linux-x64

# Copy everything else and build
COPY TooltipAI.Backend/ ./TooltipAI.Backend/
RUN dotnet publish TooltipAI.Backend/TooltipAI.Backend.csproj \
    -c Release \
    -r linux-x64 \
    --self-contained \
    -o /app/publish \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true

# ─── Final stage — Minimal runtime image ───────────────────────
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS final

LABEL maintainer="Tooltip AI Team"

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

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["./TooltipAI.Backend"]
