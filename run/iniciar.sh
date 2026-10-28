#!/usr/bin/env bash
# Construye la imagen y levanta la API. Los datos persisten en el volumen "datos".
source "$(dirname "$0")/_comun.sh"

compose up -d --build api
esperar_api
echo
echo "Swagger:  $URL/swagger"
echo "Usuarios de prueba: Alice / 123456   y   Bob / 123456"
