#!/usr/bin/env bash
# T030: genera el build del selector y lo publica donde el host lo sirve en Development/Testing.
# Node se usa solo aquí y en CI; el host en runtime nodepende de Node.
set -euo pipefail

raiz="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ui="$raiz/src/G1.Identity.Mock.Ui"
destino="$raiz/src/G1.Identity.Mock/wwwroot/selector"

if [[ ! -f "$ui/package.json" || ! -f "$ui/package-lock.json" ]]; then
    echo "No se encontró el proyecto UI con package-lock.json en $ui" >&2
    exit 1
fi

npm ci --prefix "$ui"
npm run build --prefix "$ui"

if [[ ! -f "$ui/dist/index.html" ]]; then
    echo "La compilación no generó $ui/dist/index.html" >&2
    exit 1
fi

mkdir -p "$destino"
find "$destino" -mindepth 1 -maxdepth 1 -exec rm -rf {} +
cp -R "$ui/dist/." "$destino/"
echo "Selector publicado en $destino"
