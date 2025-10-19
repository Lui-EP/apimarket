# ========= Build =========
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Restaurar el PROYECTO (no la solución)
COPY AgroMarketApi.csproj ./
RUN dotnet restore AgroMarketApi.csproj

# Copiar el resto del código (bin/obj ya están ignorados por .dockerignore)
COPY . .

# Publicar el PROYECTO explícitamente
RUN dotnet publish AgroMarketApi.csproj -c Release -o /app --no-restore

# ========= Runtime =========
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
ENTRYPOINT ["dotnet", "AgroMarketApi.dll"]
