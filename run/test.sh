#!/bin/bash
# Prueba la API con peticiones HTTP reales (la API debe estar iniciada: ./run/start.sh)
set -euo pipefail

URL=http://localhost:5080
FALLOS=0

# comprobar <descripción> <esperado> <obtenido>
comprobar() {
  if [[ "$3" == *"$2"* ]]; then
    echo "OK    $1"
  else
    echo "FALLO $1 (esperado: $2, obtenido: $3)"
    FALLOS=$((FALLOS + 1))
  fi
}

# codigo <método> <ruta> [json]  -> imprime el código HTTP
codigo() {
  curl -s -o /dev/null -w '%{http_code}' -X "$1" "$URL$2" -H 'Content-Type: application/json' ${3:+-d "$3"}
}

curl -fs -o /dev/null "$URL/swagger/index.html" || { echo "La API no responde en $URL. Ejecuta ./run/start.sh"; exit 1; }
comprobar "Swagger accesible" 200 "$(codigo GET /swagger/index.html)"

RESP=$(curl -s -X POST "$URL/api/todos" -H 'Content-Type: application/json' \
  -d '{"title":"Aprender EF Core","description":"Leer migraciones","status":"Pending","dueDate":"2030-01-31T00:00:00"}')
ID=$(echo "$RESP" | grep -o '"id":[0-9]*' | cut -d: -f2)
comprobar "POST crea la tarea" '"title":"Aprender EF Core"' "$RESP"
comprobar "POST guarda dueDate" '"dueDate":"2030-01-31' "$RESP"

comprobar "GET lista incluye la tarea" '"title":"Aprender EF Core"' "$(curl -s "$URL/api/todos")"
comprobar "GET por id" '"description":"Leer migraciones"' "$(curl -s "$URL/api/todos/$ID")"

comprobar "PUT responde 204" 204 "$(codigo PUT /api/todos/$ID '{"title":"Dominar EF Core","description":"Practicar","status":"InProgress"}')"
GET=$(curl -s "$URL/api/todos/$ID")
comprobar "PUT cambia el título" '"title":"Dominar EF Core"' "$GET"
comprobar "PUT cambia la descripción" '"description":"Practicar"' "$GET"
comprobar "PUT cambia el estado" '"status":"InProgress"' "$GET"

comprobar "GET inexistente da 404" 404 "$(codigo GET /api/todos/99999)"
comprobar "PUT inexistente da 404" 404 "$(codigo PUT /api/todos/99999 '{"title":"x"}')"
comprobar "DELETE inexistente da 404" 404 "$(codigo DELETE /api/todos/99999)"

comprobar "DELETE responde 204" 204 "$(codigo DELETE /api/todos/$ID)"
comprobar "GET tras borrar da 404" 404 "$(codigo GET /api/todos/$ID)"

if [ "$FALLOS" -eq 0 ]; then
  echo "Todas las pruebas pasaron."
else
  echo "$FALLOS prueba(s) fallaron."
  exit 1
fi
