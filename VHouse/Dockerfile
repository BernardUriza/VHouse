# 1️⃣ Usa el SDK para compilar y generar migraciones
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["VHouse.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

# 2️⃣ Crea una capa con el SDK solo para migraciones
FROM build AS migrations
WORKDIR /src
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# 3️⃣ Usa la imagen final SIN el SDK para producción
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Asegurar que el directorio de la base de datos existe
RUN mkdir -p /data && chmod -R 777 /data

# Configurar variables de entorno de Fly.io
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Copiar la app compilada desde la etapa de construcción
COPY --from=build /app .

# Comando de inicio de la app
ENTRYPOINT ["dotnet", "VHouse.dll"]
