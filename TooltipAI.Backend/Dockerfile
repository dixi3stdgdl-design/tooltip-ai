# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_VERSION=1.0.0
WORKDIR /src

COPY TooltipAI.Backend/TooltipAI.Backend.csproj ./TooltipAI.Backend/
RUN dotnet restore TooltipAI.Backend/TooltipAI.Backend.csproj

COPY TooltipAI.Backend/ ./TooltipAI.Backend/
RUN dotnet publish TooltipAI.Backend/TooltipAI.Backend.csproj -c Release -o /app/publish --no-restore

# Final stage — Azure Linux 4
FROM mcr.microsoft.com/azurelinux-beta/base/core:4.0 AS final

RUN tdnf install -y aspnetcore-runtime-8.0 curl && tdnf clean all

RUN groupadd -r tooltipai && useradd -r -g tooltipai -d /app -s /sbin/nologin tooltipai

WORKDIR /app
COPY --from=build /app/publish .

ARG BUILD_VERSION=1.0.0
LABEL version="${BUILD_VERSION}"

RUN chown -R tooltipai:tooltipai /app
USER tooltipai

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "TooltipAI.Backend.dll"]
