#!/usr/bin/env bash
# Utilidades HTTP para los scripts de verificación. No se ejecuta directamente.
source "$(dirname "${BASH_SOURCE[0]}")/_comun.sh"

FALLOS=0
CUERPO=""
CODIGO=""

# llamar METODO RUTA [TOKEN] [JSON]  -> deja la respuesta en $CODIGO y $CUERPO
llamar() {
  local metodo="$1" ruta="$2" token="${3:-}" json="${4:-}"
  local args=(-s -X "$metodo" -w $'\n%{http_code}' "$URL$ruta")
  [[ -n "$token" ]] && args+=(-H "Authorization: Bearer $token")
  [[ -n "$json" ]] && args+=(-H "Content-Type: application/json" -d "$json")
  local salida; salida="$(curl "${args[@]}")"
  CODIGO="${salida##*$'\n'}"
  CUERPO="${salida%$'\n'*}"
}

# esperar DESCRIPCION CODIGO_ESPERADO  (compara con $CODIGO de la última llamada)
esperar() {
  if [[ "$CODIGO" == "$2" ]]; then echo "  OK    $1 -> $CODIGO"
  else echo "  FALLA $1 -> esperaba $2 y llegó $CODIGO: $CUERPO"; FALLOS=$((FALLOS+1)); fi
}

# contiene DESCRIPCION TEXTO  (busca texto en $CUERPO)
contiene() {
  if [[ "$CUERPO" == *"$2"* ]]; then echo "  OK    $1"
  else echo "  FALLA $1 (no aparece '$2' en: $CUERPO)"; FALLOS=$((FALLOS+1)); fi
}

# no_contiene DESCRIPCION TEXTO
no_contiene() {
  if [[ "$CUERPO" != *"$2"* ]]; then echo "  OK    $1"
  else echo "  FALLA $1 (aparece '$2')"; FALLOS=$((FALLOS+1)); fi
}

# token USUARIO CONTRASENA -> imprime el JWT
obtener_token() {
  llamar POST /api/auth/login "" "{\"nombreUsuario\":\"$1\",\"contrasena\":\"$2\"}"
  [[ "$CODIGO" == 200 ]] || { echo "No se pudo hacer login con $1 ($CODIGO)" >&2; return 1; }
  echo "$CUERPO" | sed -n 's/.*"token":"\([^"]*\)".*/\1/p'
}

terminar() {
  echo
  if [[ $FALLOS -eq 0 ]]; then echo "TODO CORRECTO"; else echo "$FALLOS verificación(es) fallaron"; exit 1; fi
}
