#!/usr/bin/env bash
# Crea una migración nueva de EF Core sin instalar nada en tu máquina (usa el SDK dentro de Docker).
# Uso: run/nueva-migracion.sh NombreDeLaMigracion
# Primero cambia los modelos en src/back/Mensajeria.Api/Modelos y luego ejecuta este script.
source "$(dirname "$0")/_comun.sh"

NOMBRE="${1:?Uso: run/nueva-migracion.sh NombreDeLaMigracion}"
docker run --rm -e ASPNETCORE_ENVIRONMENT=Development -v "$RAIZ/src/back/Mensajeria.Api:/proyecto:Z" -w /proyecto \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c "
    dotnet restore >/dev/null && dotnet tool install --global dotnet-ef --version 10.0.14 >/dev/null &&
    export PATH=\"\$PATH:/root/.dotnet/tools\" &&
    dotnet ef migrations add '$NOMBRE'"
echo "Migración creada en src/back/Mensajeria.Api/Migrations (se aplicará sola al reiniciar la API)."
