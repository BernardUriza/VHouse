# Phase 8: Multi-Cloud Production Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install additional dependencies for multi-cloud operations
RUN apt-get update && apt-get install -y \
    curl \
    wget \
    unzip \
    postgresql-client \
    && rm -rf /var/lib/apt/lists/*

# Install cloud CLI tools
RUN curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip" \
    && unzip awscliv2.zip \
    && ./aws/install \
    && rm -rf aws awscliv2.zip

RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["VHouse/VHouse.csproj", "VHouse/"]
RUN dotnet restore "VHouse/VHouse.csproj"
COPY . .
WORKDIR "/src/VHouse"
RUN dotnet build "VHouse.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VHouse.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN useradd -r -g daemon vhouse
USER vhouse

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "VHouse.dll"]