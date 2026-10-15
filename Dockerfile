# ==============================================================================
# KrishiBondhu Decision Engine - Multi-Stage Production Dockerfile
# Target: .NET 10 (LTS) ASP.NET Core Blazor Web App
# ==============================================================================

# ------------------------------------------------------------------------------
# Build Stage
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for optimized Docker layer caching
COPY ["src/KrishiBondhu.Domain/KrishiBondhu.Domain.csproj", "src/KrishiBondhu.Domain/"]
COPY ["src/KrishiBondhu.Application/KrishiBondhu.Application.csproj", "src/KrishiBondhu.Application/"]
COPY ["src/KrishiBondhu.Infrastructure/KrishiBondhu.Infrastructure.csproj", "src/KrishiBondhu.Infrastructure/"]
COPY ["src/KrishiBondhu.Web/KrishiBondhu.Web.csproj", "src/KrishiBondhu.Web/"]

# Restore dependencies for the web entrypoint project
RUN dotnet restore "src/KrishiBondhu.Web/KrishiBondhu.Web.csproj"

# Copy the rest of the application source code
COPY src/ src/

# Build and publish release binaries
WORKDIR "/src/src/KrishiBondhu.Web"
RUN dotnet publish "KrishiBondhu.Web.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ------------------------------------------------------------------------------
# Runtime Stage
# ------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published application binaries
COPY --from=build /app/publish .

# Create directory structure for complaint evidence/receipt file uploads
RUN mkdir -p /app/uploads/complaints

# Configure environment variables
# Port 10000 as requested (can also be overridden via docker run -e ASPNETCORE_URLS=... or PORT=...)
ENV ASPNETCORE_URLS=http://+:10000
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
EXPOSE 10000

# Set entrypoint
ENTRYPOINT ["dotnet", "KrishiBondhu.Web.dll"]
