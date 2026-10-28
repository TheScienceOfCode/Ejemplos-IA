#!/usr/bin/env bash
# Demuestra cómo las migraciones EVOLUCIONAN una base de datos existente sin borrar su información.
# Usa un proyecto y un volumen aparte ("mensajeria-demo"), así que no toca tus datos reales.
# Se ejecuta en el puerto 8081.
source "$(dirname "$0")/_comun.sh"
export COMPOSE_PROJECT_NAME=mensajeria-demo PUERTO=8081 URL=http://localhost:8081

migraciones() { compose logs api 2>&1 | grep "Migraciones en la base de datos" | tail -1 | sed 's/.*: //'; }

compose down -v >/dev/null 2>&1 || true

echo "== PASO 1: base nueva, solo con las migraciones 1 y 2 (usuarios, buzones y mensajes) =="
MIGRAR_HASTA=AddInboxesAndMessages compose up -d --build api >/dev/null 2>&1
esperar_api
echo "Migraciones aplicadas: $(migraciones)"

echo
echo "== PASO 2: registramos un usuario (la tabla Mensajes todavía no tiene la columna EstaLeido) =="
curl -s -X POST "$URL/api/auth/registro" -H "Content-Type: application/json" \
  -d '{"nombreUsuario":"Veterano","correo":"veterano@ejemplo.com","contrasena":"123456"}'
echo

echo
echo "== PASO 3: actualizamos la aplicación: se aplica la migración 3 (AddMessageReadStatus) =="
MIGRAR_HASTA= compose up -d --force-recreate api >/dev/null 2>&1
esperar_api
echo "Migraciones aplicadas: $(migraciones)"

echo
echo "== PASO 4: el usuario creado ANTES de la migración 3 sigue ahí y puede iniciar sesión =="
curl -s -o /dev/null -w "Login de Veterano -> HTTP %{http_code}\n" -X POST "$URL/api/auth/login" \
  -H "Content-Type: application/json" -d '{"nombreUsuario":"Veterano","contrasena":"123456"}'

compose down -v >/dev/null 2>&1
echo
echo "Demo terminada (el volumen de la demo se eliminó; tus datos reales no se tocaron)."
