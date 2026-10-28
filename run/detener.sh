#!/usr/bin/env bash
# Detiene la API conservando los datos. Con --borrar-datos también elimina el volumen de la base de datos.
source "$(dirname "$0")/_comun.sh"

if [[ "${1:-}" == "--borrar-datos" ]]; then
  compose --profile pruebas down -v
  echo "Contenedores detenidos y base de datos ELIMINADA."
else
  compose --profile pruebas down
  echo "Contenedores detenidos. La base de datos se conserva (usa --borrar-datos para eliminarla)."
fi
