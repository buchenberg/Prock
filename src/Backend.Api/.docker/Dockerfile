# Build stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src

# Copy solution file and all project files for restore
COPY ["Prock.sln", "./"]
COPY ["src/Backend.Api/Backend.Api.csproj", "src/Backend.Api/"]
COPY ["src/Backend.Core/Backend.Core.csproj", "src/Backend.Core/"]
COPY ["src/Backend.Infrastructure/Backend.Infrastructure.csproj", "src/Backend.Infrastructure/"]
COPY ["src/Shared.Contracts/Shared.Contracts.csproj", "src/Shared.Contracts/"]

# Restore dependencies
RUN dotnet restore "src/Backend.Api/Backend.Api.csproj"

# Copy all source code
COPY ["src/", "src/"]

# Build the application (restore will happen automatically)
RUN dotnet build "src/Backend.Api/Backend.Api.csproj" -c $configuration -o /app/build

# Publish stage
FROM build AS publish
ARG configuration=Release
RUN dotnet publish "src/Backend.Api/Backend.Api.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application
COPY --from=publish /app/publish .

# Copy configuration files
COPY ["src/Backend.Api/appsettings.Docker.json", "./appsettings.json"]

# Set ownership
RUN chown -R appuser:appuser /app
USER appuser

# Expose ports
EXPOSE 5001 80 443

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5001/health || exit 1

# Environment configuration
ENV ASPNETCORE_URLS=http://+:5001
ENV ASPNETCORE_ENVIRONMENT=Docker

ENTRYPOINT ["dotnet", "Backend.Api.dll"]
