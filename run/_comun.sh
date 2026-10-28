#!/usr/bin/env bash
# Funciones compartidas por los scripts de run/. No se ejecuta directamente.
set -euo pipefail

RAIZ="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export COMPOSE_FILE="$RAIZ/src/back/docker-compose.yml"
URL="${URL:-http://localhost:8080}"

compose() { docker compose "$@"; }

esperar_api() {
  echo -n "Esperando a la API en $URL "
  for _ in $(seq 1 60); do
    if curl -fs -o /dev/null "$URL/swagger/v1/swagger.json"; then echo " lista."; return 0; fi
    echo -n "."; sleep 1
  done
  echo; echo "La API no respondió. Revisa los logs: docker compose -f $COMPOSE_FILE logs api" >&2
  return 1
}
