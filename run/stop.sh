#!/bin/bash
set -euo pipefail
source "$(dirname "$0")/_compose.sh"

# Sin -v: el volumen con la base de datos se conserva
compose down
echo "API detenida (los datos se conservan)."
