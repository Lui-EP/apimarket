# ========= Build =========
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copiamos solución y proyecto para cachear el restore
COPY AgroMarketApi.csproj ./

# Restaurar dependencias
RUN dotnet restore

# Copiar todo y publicar
COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# ========= Runtime =========
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Render provee $PORT; escucha en todas las interfaces
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Para pruebas locales (opcional): expone 8080
EXPOSE 8080

# Cambia el nombre del dll si tu assembly se llama distinto
ENTRYPOINT ["dotnet", "AgroMarketApi.dll"]
