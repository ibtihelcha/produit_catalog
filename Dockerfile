# 1. Image de base pour l'exécution
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

EXPOSE 8080

USER app


# 2. Image SDK pour compiler
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copie du fichier projet
COPY ["src/ProductCatalog.Api/ProductCatalog.Api.csproj", "ProductCatalog.Api/"]

# Restauration des dépendances
RUN dotnet restore "ProductCatalog.Api/ProductCatalog.Api.csproj"

# Copie du code source
COPY src/ProductCatalog.Api/ ProductCatalog.Api/

WORKDIR "/src/ProductCatalog.Api"

# Compilation
RUN dotnet build "ProductCatalog.Api.csproj" -c Release -o /app/build


# 3. Publication
FROM build AS publish

RUN dotnet publish "ProductCatalog.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false


# 4. Image finale
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ProductCatalog.Api.dll"]