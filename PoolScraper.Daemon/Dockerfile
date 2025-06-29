# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-stage
WORKDIR /src

# Restore dependencies first for layer caching
COPY ["PoolScraper.Daemon/PoolScraper.Daemon.csproj", "PoolScraper.Daemon/"]
COPY ["CommonUtils/CommonUtils.csproj", "CommonUtils/"]
COPY ["PoolScraper.Lib/PoolScraper.Lib.csproj", "PoolScraper.Lib/"]

RUN dotnet restore "PoolScraper.Daemon/PoolScraper.Daemon.csproj"

# Copy everything else and build
COPY . .

RUN dotnet publish "PoolScraper.Daemon/PoolScraper.Daemon.csproj" -c Release -o /app/publish \
    /p:UseAppHost=false \
    /p:GenerateRuntimeConfigurationFiles=true

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published content
COPY --from=build-stage /app/publish .

ENTRYPOINT ["dotnet", "PoolScraper.Daemon.dll"]