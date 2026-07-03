# Azure Linux 4 base — Microsoft-signed, supply chain secure
FROM mcr.microsoft.com/azurelinux-beta/base/core:4.0 AS base

# Install .NET 8 runtime from Microsoft repo
RUN tdnf install -y dotnet-runtime-8.0 aspnetcore-runtime-8.0 && \
    tdnf clean all

# Create non-root user
RUN groupadd -r tooltipai && useradd -r -g tooltipai -d /app -s /sbin/nologin tooltipai

WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_VERSION=1.0.0
WORKDIR /src

COPY ["TooltipAI.Backend.csproj", "."]
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Final stage — Azure Linux 4
FROM base AS final

COPY --from=build /app/publish .

# Build version label
ARG BUILD_VERSION
LABEL version="${BUILD_VERSION}"

RUN chown -R tooltipai:tooltipai /app
USER tooltipai

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "TooltipAI.Backend.dll"]
