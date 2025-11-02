# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["API-PDF/API-PDF.csproj", "API-PDF/"]
RUN dotnet restore "API-PDF/API-PDF.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/API-PDF"
RUN dotnet build "API-PDF.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "API-PDF.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create necessary directories
RUN mkdir -p /app/temp/pdfs && \
    mkdir -p /app/pdfs/fallback && \
    chmod -R 777 /app/temp && \
    chmod -R 777 /app/pdfs

# Copy published app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080
EXPOSE 8081

# Set environment variables (defaults)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/api/s3/health || exit 1

ENTRYPOINT ["dotnet", "API-PDF.dll"]
