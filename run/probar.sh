#!/usr/bin/env bash
# Ejecuta las pruebas automáticas de integración dentro de Docker. El resultado queda en bin/resultados.trx
source "$(dirname "$0")/_comun.sh"

mkdir -p "$RAIZ/bin"
compose --profile pruebas run --rm --build pruebas
echo "Resultados guardados en bin/resultados.trx"
