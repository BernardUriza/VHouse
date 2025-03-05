# Usa la imagen base de .NET 8 para producción
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Usa la imagen SDK de .NET 8 para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["VHouse.csproj", "./"]
RUN dotnet restore "VHouse.csproj"
COPY . .
RUN dotnet publish "VHouse.csproj" -c Release -o /app/publish

# Construye la imagen final con solo los archivos necesarios
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VHouse.dll"]
