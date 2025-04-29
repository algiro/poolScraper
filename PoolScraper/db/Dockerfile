# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-stage
WORKDIR /src

# Restore dependencies first for layer caching
COPY ["PoolScraper.csproj", "./"]
RUN dotnet restore "PoolScraper.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "PoolScraper.csproj" -c Release -o /app/publish \
    /p:UseAppHost=false \
    /p:GenerateRuntimeConfigurationFiles=true

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published content
COPY --from=build-stage /app/publish .

# Ensure proper permissions and clean entrypoint
RUN chmod -R 755 /app/wwwroot
ENTRYPOINT ["dotnet", "PoolScraper.dll"]
