#!/bin/bash
set -euo pipefail
source "$(dirname "$0")/_compose.sh"

compose up -d

echo -n "Esperando a la API"
for _ in $(seq 1 30); do
  if curl -fs -o /dev/null http://localhost:5080/swagger/v1/swagger.json; then
    echo
    echo "API lista. Swagger: http://localhost:5080/swagger"
    exit 0
  fi
  echo -n "."
  sleep 1
done
echo
echo "Error: la API no respondió a tiempo" >&2
compose logs api >&2
exit 1
