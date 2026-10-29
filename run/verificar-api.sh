#!/usr/bin/env bash
# Prueba el flujo completo con peticiones HTTP reales contra la API que está corriendo (run/iniciar.sh).
source "$(dirname "$0")/_http.sh"

SUFIJO="$(date +%s)"
echo "== Seguridad básica =="
llamar GET /api/buzon;                    esperar "Buzón sin token" 401
llamar POST /api/auth/login "" '{"nombreUsuario":"Alice","contrasena":"mala"}'; esperar "Login con contraseña incorrecta" 401

echo "== Registro =="
llamar POST /api/auth/registro "" "{\"nombreUsuario\":\"carla$SUFIJO\",\"correo\":\"carla$SUFIJO@ejemplo.com\",\"contrasena\":\"123456\"}"
esperar "Registrar a carla$SUFIJO" 201
no_contiene "No se devuelve el hash" "ash"
llamar POST /api/auth/registro "" "{\"nombreUsuario\":\"carla$SUFIJO\",\"correo\":\"otro$SUFIJO@ejemplo.com\",\"contrasena\":\"123456\"}"
esperar "Registro duplicado" 409
llamar POST /api/auth/registro "" '{"nombreUsuario":"x","correo":"malo","contrasena":"1"}'
esperar "Registro con datos inválidos" 400

echo "== Flujo Alice -> Bob =="
ALICE="$(obtener_token Alice 123456)"; BOB="$(obtener_token Bob 123456)"
llamar POST /api/mensajes "$ALICE" "{\"destinatarioNombreUsuario\":\"Bob\",\"asunto\":\"Hola $SUFIJO\",\"contenido\":\"¿Almorzamos?\"}"
esperar "Alice envía mensaje a Bob" 201
ID="$(echo "$CUERPO" | sed -n 's/.*"id":\([0-9]*\).*/\1/p')"
llamar POST /api/mensajes "$ALICE" '{"destinatarioNombreUsuario":"Nadie","asunto":"a","contenido":"b"}'
esperar "Enviar a usuario inexistente" 404
llamar GET /api/mensajes/enviados "$ALICE";  esperar "Enviados de Alice" 200; contiene "Aparece el mensaje" "Hola $SUFIJO"
llamar GET /api/buzon "$BOB";                esperar "Buzón de Bob" 200;       contiene "Aparece el mensaje" "Hola $SUFIJO"
llamar GET "/api/mensajes/$ID" "$BOB";       esperar "Bob lee el mensaje" 200; contiene "Aún sin leer" '"estaLeido":false'
llamar PUT "/api/mensajes/$ID/leido" "$BOB"; esperar "Bob lo marca como leído" 200; contiene "Quedó leído" '"estaLeido":true'

echo "== Autorización =="
llamar GET /api/buzon "$ALICE";                 esperar "Alice abre su buzón" 200; no_contiene "No ve el mensaje privado de Bob" "Hola $SUFIJO"
llamar PUT "/api/mensajes/$ID/leido" "$ALICE";      esperar "Alice (remitente) intenta marcar como leído" 404
CARLA="$(obtener_token "carla$SUFIJO" 123456)"
llamar GET "/api/mensajes/$ID" "$CARLA";            esperar "Un tercero intenta leer el mensaje" 404
llamar PUT "/api/mensajes/$ID/leido" "$CARLA";      esperar "Un tercero intenta marcarlo como leído" 404

terminar
