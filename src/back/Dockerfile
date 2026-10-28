# Etapas del Dockerfile:
#   build  -> restaura paquetes y compila (con el SDK de .NET, que solo vive dentro de Docker)
#   pruebas -> ejecuta las pruebas automáticas (docker compose --profile pruebas ...)
#   final  -> imagen liviana que ejecuta la API (es la última, así que es la que se construye por defecto)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Primero solo los .csproj: así Docker reutiliza la caché de paquetes si el código cambia
COPY Mensajeria.Api/Mensajeria.Api.csproj Mensajeria.Api/
COPY Mensajeria.Pruebas/Mensajeria.Pruebas.csproj Mensajeria.Pruebas/
RUN dotnet restore Mensajeria.Pruebas/Mensajeria.Pruebas.csproj
COPY . .
RUN dotnet publish Mensajeria.Api/Mensajeria.Api.csproj -c Release -o /publicado

FROM build AS pruebas
ENTRYPOINT ["dotnet", "test", "Mensajeria.Pruebas/Mensajeria.Pruebas.csproj", \
            "--logger", "trx;LogFileName=resultados.trx", "--results-directory", "/salida"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
# /datos es donde vive la base SQLite (se monta como volumen para no perderla)
RUN mkdir /datos && chown app:app /datos
COPY --from=build /publicado .
USER app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Mensajeria.Api.dll"]
