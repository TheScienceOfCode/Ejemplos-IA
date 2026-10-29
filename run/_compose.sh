# Usa Docker si existe; si no, Podman (mismo comando "compose")
cd "$(dirname "${BASH_SOURCE[0]}")/.." || exit 1
if command -v docker >/dev/null; then
  compose() { docker compose "$@"; }
elif command -v podman >/dev/null; then
  compose() { podman compose "$@"; }
else
  echo "Error: se necesita Docker (o Podman) con Compose" >&2
  exit 1
fi
