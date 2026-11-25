#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CONTRACT="$ROOT_DIR/specs/001-g1-identidad-contrato-mock/contracts/openapi.v1.yaml"
RUNTIME_COPY="$ROOT_DIR/src/G1.Identity.Mock/OpenApi/ContractDocument.json"
REDOCLLY_VERSION="2.83.0"
PORT="${PORT:-5191}"
MODE="${1:-check}"

usage() {
    cat <<'USO'
Uso: scripts/export-openapi.sh [generate|check|runtime|all]

  generate  Regenera la copia JSON incrustada desde el contrato normativo.
  check     Comprueba que la copia incrustada coincide con el contrato (por defecto).
  runtime   Arranca el mock en Testing y compara /openapi/v1.json con la copia incrustada.
  all       Ejecuta check y runtime.
USO
}

require_tools() {
    if ! command -v dotnet >/dev/null 2>&1; then
        export PATH="$HOME/.dotnet:$PATH"
    fi

    for tool in dotnet python3 curl npx; do
        if ! command -v "$tool" >/dev/null 2>&1; then
            echo "Falta la herramienta requerida: $tool" >&2
            exit 1
        fi
    done

    if [ ! -f "$CONTRACT" ]; then
        echo "No se encontró el contrato normativo en $CONTRACT" >&2
        exit 1
    fi
}

bundle_contract() {
    local output="$1"
    npx --yes "@redocly/cli@${REDOCLLY_VERSION}" bundle "$CONTRACT" --ext json --output "$output"
}

validate_document() {
    python3 - "$1" <<'PYTHON'
import json
import sys
from pathlib import Path

path = Path(sys.argv[1])
document = json.loads(path.read_text(encoding="utf-8"))
info = document.get("info", {})

def fail(message):
    raise SystemExit(f"{path}: {message}")

if document.get("openapi") != "3.1.0":
    fail("la versión publicada debe ser 3.1.0")
if info.get("version") != "1.0.0":
    fail("info.version debe ser 1.0.0")
if "x-reviewed-at" not in info or "x-persistence-exception" not in info:
    fail("faltan las extensiones de gobernanza")
if len(document.get("paths", {})) != 18:
    fail("el contrato debe contener 18 rutas")
if len(document.get("components", {}).get("schemas", {})) != 23:
    fail("el contrato debe contener 23 esquemas")

methods = {"get", "post", "put", "delete", "patch"}
operations = sum(
    1
    for item in document.get("paths", {}).values()
    for method in item
    if method.lower() in methods
)
if operations != 19:
    fail(f"el contrato debe contener 19 operaciones; se encontraron {operations}")
PYTHON
}

check_runtime_document() {
    local work_dir="$1"
    local dll="$ROOT_DIR/src/G1.Identity.Mock/bin/Release/net10.0/G1.Identity.Mock.dll"
    local url="http://127.0.0.1:${PORT}"
    local headers="$work_dir/runtime-headers.txt"
    local runtime_json="$work_dir/runtime.json"
    local app_log="$work_dir/app.log"
    local etag
    local status

    if [ ! -f "$dll" ]; then
        dotnet build "$ROOT_DIR/src/G1.Identity.Mock/G1.Identity.Mock.csproj" --configuration Release
    fi

    (
        cd "$ROOT_DIR/src/G1.Identity.Mock"
        exec env ASPNETCORE_ENVIRONMENT=Testing ASPNETCORE_URLS="$url" dotnet "$dll"
    ) >"$app_log" 2>&1 &
    local app_pid=$!

    cleanup_runtime() {
        kill "$app_pid" >/dev/null 2>&1 || true
        wait "$app_pid" >/dev/null 2>&1 || true
    }
    trap cleanup_runtime RETURN

    local ready=false
    for _ in $(seq 1 30); do
        if curl -fsS -D "$headers" "$url/openapi/v1.json" -o "$runtime_json" 2>/dev/null; then
            ready=true
            break
        fi
        if ! kill -0 "$app_pid" >/dev/null 2>&1; then
            cat "$app_log" >&2
            return 1
        fi
        sleep 1
    done

    if [ "$ready" != true ]; then
        echo "El mock no respondió en $url/openapi/v1.json" >&2
        cat "$app_log" >&2
        return 1
    fi

    validate_document "$runtime_json"

    python3 - "$RUNTIME_COPY" "$runtime_json" <<'PYTHON'
import json
import sys
from pathlib import Path

expected = json.loads(Path(sys.argv[1]).read_text(encoding="utf-8"))
actual = json.loads(Path(sys.argv[2]).read_text(encoding="utf-8"))
if expected != actual:
    print("El documento publicado diffs del contrato incrustado:", file=sys.stderr)
    print(json.dumps(actual, ensure_ascii=False, indent=2, sort_keys=True), file=sys.stderr)
    raise SystemExit(1)
PYTHON

    python3 - "$headers" <<'PYTHON'
import sys
from pathlib import Path

headers = {}
for line in Path(sys.argv[1]).read_text(encoding="utf-8").splitlines():
    if ":" not in line:
        continue
    name, value = line.split(":", 1)
    headers[name.strip().lower()] = value.strip()

if headers.get("cache-control") != "no-cache":
    raise SystemExit("El documento debe publicar Cache-Control: no-cache")
etag = headers.get("etag", "")
if not (etag.startswith('"') and etag.endswith('"') and len(etag) > 2):
    raise SystemExit("El documento debe publicar una ETag fuerte entre comillas")
print(etag)
PYTHON

    etag="$(python3 - "$headers" <<'PYTHON'
import sys
from pathlib import Path
for line in Path(sys.argv[1]).read_text(encoding="utf-8").splitlines():
    if line.lower().startswith("etag:"):
        print(line.split(":", 1)[1].strip())
        break
PYTHON
)"
    status="$(curl -sS -o /dev/null -w '%{http_code}' -H "If-None-Match: $etag" "$url/openapi/v1.json")"
    if [ "$status" != "304" ]; then
        echo "If-None-Match devolvió $status; se esperaba 304" >&2
        return 1
    fi

    curl -fsS "$url/swagger/index.html" -o "$work_dir/swagger.html"
    if ! grep -q 'swagger-ui-bundle.js' "$work_dir/swagger.html"; then
        echo "La página de Swagger UI no carga sus recursos" >&2
        return 1
    fi

    for asset in swagger-ui.css swagger-ui-bundle.js swagger-ui-standalone-preset.js index.js index.css; do
        if ! curl -fsS -o /dev/null "$url/swagger/$asset"; then
            echo "No se pudo descargar el recurso /swagger/$asset" >&2
            return 1
        fi
    done

    curl -fsS -D "$work_dir/swagger-headers.txt" -o "$work_dir/swagger-init.js" "$url/swagger/index.js"
    if ! grep -Fq '/openapi/v1.json' "$work_dir/swagger-init.js"; then
        echo "Swagger UI no apunta a /openapi/v1.json" >&2
        return 1
    fi
    if ! grep -Fqi "Content-Security-Policy: default-src 'self'; script-src 'self' 'unsafe-inline'" "$work_dir/swagger-headers.txt"; then
        echo "La CSP de Swagger UI no permite sus scripts" >&2
        return 1
    fi
}

require_tools

case "$MODE" in
    generate)
        bundle_contract "$RUNTIME_COPY"
        validate_document "$RUNTIME_COPY"
        echo "OK: copia incrustada regenerada desde $CONTRACT"
        ;;
    check)
        work_dir="$(mktemp -d)"
        trap 'rm -rf "$work_dir"' EXIT
        bundle_contract "$work_dir/contract.json"
        validate_document "$work_dir/contract.json"
        if ! diff -u "$RUNTIME_COPY" "$work_dir/contract.json"; then
            echo "La copia incrustada no coincide con el contrato; ejecuta generate" >&2
            exit 1
        fi
        echo "OK: contrato normativo, copia incrustada y metadatos coinciden"
        ;;
    runtime)
        work_dir="$(mktemp -d)"
        trap 'rm -rf "$work_dir"' EXIT
        check_runtime_document "$work_dir"
        echo "OK: documento publicado, Swagger UI, ETag/304 y CSP verificados"
        ;;
    all)
        work_dir="$(mktemp -d)"
        trap 'rm -rf "$work_dir"' EXIT
        bundle_contract "$work_dir/contract.json"
        validate_document "$work_dir/contract.json"
        diff -u "$RUNTIME_COPY" "$work_dir/contract.json"
        check_runtime_document "$work_dir"
        echo "OK: contrato, copia incrustada y documento publicado son equivalentes"
        ;;
    -h|--help|help)
        usage
        ;;
    *)
        usage >&2
        exit 1
        ;;
esac
