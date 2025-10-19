# ========= Build =========
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copiar solución/proyecto para cachear restore
COPY ApiAgricola.sln .          # si no tienes .sln, elimina esta línea
COPY AgroMarketApi.csproj ./
RUN dotnet restore

# Copiar el resto (sin bin/obj gracias a .dockerignore)
COPY . .
# Doble seguro por si alguien volvió a subirlos
RUN rm -rf ./bin ./obj

RUN dotnet publish -c Release -o /app --no-restore

# ========= Runtime =========
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Render usa $PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
EXPOSE 8080

ENTRYPOINT ["dotnet", "AgroMarketApi.dll"]
