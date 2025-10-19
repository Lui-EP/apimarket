# ========= Build =========
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Cachea restore
COPY AgroMarketApi.csproj ./
RUN dotnet restore

# Copia el resto y limpia bin/obj por si entraron
COPY . .
RUN rm -rf ./bin ./obj

# Publica
RUN dotnet publish -c Release -o /app --no-restore

# ========= Runtime =========
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Render usa $PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
ENTRYPOINT ["dotnet", "AgroMarketApi.dll"]
