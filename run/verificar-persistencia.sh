#!/usr/bin/env bash
# Demuestra que los datos sobreviven a reiniciar y a RECONSTRUIR los contenedores.
source "$(dirname "$0")/_http.sh"

NOMBRE="persistente$(date +%s)"
llamar POST /api/auth/registro "" "{\"nombreUsuario\":\"$NOMBRE\",\"correo\":\"$NOMBRE@ejemplo.com\",\"contrasena\":\"123456\"}"
esperar "Registrar $NOMBRE" 201
T="$(obtener_token "$NOMBRE" 123456)"
llamar POST /api/mensajes "$(obtener_token Alice 123456)" "{\"destinatarioNombreUsuario\":\"$NOMBRE\",\"asunto\":\"Sobrevivo\",\"contenido\":\"x\"}"
esperar "Alice le escribe a $NOMBRE" 201

echo "== Reiniciando contenedores (restart) =="
compose restart api >/dev/null; esperar_api
llamar GET /api/buzon "$(obtener_token "$NOMBRE" 123456)"; esperar "Buzón tras restart" 200; contiene "Mensaje conservado" "Sobrevivo"

echo "== Destruyendo y reconstruyendo contenedores (down + up --build) =="
compose down >/dev/null; compose up -d --build api >/dev/null 2>&1; esperar_api
llamar GET /api/buzon "$(obtener_token "$NOMBRE" 123456)"; esperar "Buzón tras reconstruir" 200; contiene "Mensaje conservado" "Sobrevivo"

terminar
