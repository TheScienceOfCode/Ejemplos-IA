#!/bin/bash
set -euo pipefail
source "$(dirname "$0")/_compose.sh"

compose build
echo "Imagen construida."
