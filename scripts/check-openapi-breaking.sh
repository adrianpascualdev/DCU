#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CONTRACT_REL="specs/001-g1-identidad-contrato-mock/contracts/openapi.v1.yaml"
CONTRACT="$ROOT_DIR/$CONTRACT_REL"
BASE_REF="${BASE_REF:-origin/001-g1-identidad-contrato-mock}"
REDOCLLY_VERSION="2.83.0"
OASDIFF_IMAGE="${OASDIFF_IMAGE:-tufin/oasdiff:latest}"

if [ ! -f "$CONTRACT" ]; then
    echo "No se encontró el contrato normativo en $CONTRACT" >&2
    exit 1
fi

work_dir="$(mktemp -d)"
trap 'rm -rf "$work_dir"' EXIT

echo "Validando $CONTRACT_REL con @redocly/cli ${REDOCLLY_VERSION}"
npx --yes "@redocly/cli@${REDOCLLY_VERSION}" lint "$CONTRACT"

if ! git -C "$ROOT_DIR" cat-file -e "${BASE_REF}:${CONTRACT_REL}" 2>/dev/null; then
    echo "Aviso: no existe $CONTRACT_REL en $BASE_REF; se omite oasdiff." >&2
    exit 0
fi

git -C "$ROOT_DIR" show "${BASE_REF}:${CONTRACT_REL}" > "$work_dir/base.yaml"
cp "$CONTRACT" "$work_dir/revision.yaml"

if command -v oasdiff >/dev/null 2>&1; then
    echo "Comprobando cambios incompatibles con oasdiff"
    oasdiff breaking "$work_dir/base.yaml" "$work_dir/revision.yaml"
elif docker info >/dev/null 2>&1; then
    echo "Comprobando cambios incompatibles con oasdiff (Docker)"
    docker run --rm --volume "$work_dir:/specs:ro" "$OASDIFF_IMAGE" \
        breaking /specs/base.yaml /specs/revision.yaml
else
    echo "Se necesita oasdiff en PATH o Docker para completar la comprobación." >&2
    echo "Instálalo con: go install github.com/oasdiff/oasdiff@latest" >&2
    exit 1
fi

echo "OK: el contrato es válido y no introduce cambios incompatibles frente a $BASE_REF"
