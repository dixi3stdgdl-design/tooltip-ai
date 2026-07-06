# ─── Build stage ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

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

# ─── Final stage — Minimal runtime image ───────────────────────
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS final

LABEL maintainer="Tooltip AI Team"

# Create non-root user
RUN addgroup -S tooltipai && adduser -S tooltipai -G tooltipai

WORKDIR /app

# Copy published binaries
COPY --from=build /app/publish .

# Set ownership
RUN chown -R tooltipai:tooltipai /app && chmod -R 550 /app

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
