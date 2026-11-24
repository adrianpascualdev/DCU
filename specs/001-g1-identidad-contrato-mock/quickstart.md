# Quickstart: validación del contrato y del mock (fase A)

**Feature**: `001-g1-identidad-contrato-mock` | **Fecha**: 2026-11-24

Guía ejecutable para comprobar que el contrato y el mock cumplen los escenarios de la fase A. No sustituye a las pruebas automatizadas ni incluye código de implementación: describe comandos, resultados esperados y criterios de aceptación.

## 1. Requisitos previos

- .NET SDK 10.0.404 (`dotnet --version`).
- Node.js LTS y npm (solo para construir el selector React).
- `curl`, `openssl` y, opcionalmente, `jq`.
- Puertos libres `7180` (HTTPS) y `5180` (HTTP).
- Certificado de desarrollo confiable: `dotnet dev-certs https --trust`.
- Navegador Chromium instalado por Playwright para las pruebas E2E.

> **Aviso**: el mock usa EF Core InMemory. Todos los datos, sesiones y tokens se pierden al reiniciar. No usar datos reales. ADR-0002 sigue pendiente de ratificación formal: esta guía describe la validación prevista, pero las tareas de implementación no deben ejecutarse hasta que se ratifique.

## 2. Arranque

```bash
# 1. Restaurar y compilar
dotnet restore G1.Identity.slnx
dotnet build G1.Identity.slnx

# 2. Construir el selector React (una vez o tras cambios)
cd src/G1.Identity.Mock.Ui
npm ci
npm run build
cd ../../..

# 3. Arrancar el mock
dotnet run --project src/G1.Identity.Mock
```

Resultado esperado:

- El mock escucha en `https://localhost:7180` y `http://localhost:5180`.
- Los logs advierten de que la persistencia es InMemory y los datos son ficticios.
- No se publica ningún endpoint fuera del contrato; `/health` no forma parte de la fase A.

## 3. Swagger UI y contrato

1. Abrir `https://localhost:7180/swagger`.
2. Comprobar que carga el documento OpenAPI 3.1 y que aparecen las etiquetas `OIDC`, `Identity`, `Sessions`, `Catalog` y `Mock`.
3. Abrir `https://localhost:7180/openapi/v1.json` y verificar `openapi: 3.1.0`.
4. Comparar el documento con `specs/001-g1-identidad-contrato-mock/contracts/openapi.v1.yaml`.

Resultado esperado: Swagger UI solo está disponible en Development/Testing; en otro entorno responde `404`.

## 4. Flujo Authorization Code + PKCE

Este flujo demuestra que el mock no usa atajos: la persona se autentica, pero los tokens solo se emiten canjeando un código con PKCE.

```bash
# Variables de ejemplo
ISSUER=https://localhost:7180
CLIENT_ID=cli_mock_spa
REDIRECT_URI=https://localhost:3000/callback
STATE=$(openssl rand -hex 8)
NONCE=$(openssl rand -hex 8)

# 1. Generar code_verifier y code_challenge S256
VERIFIER=$(openssl rand -base64 64 | tr -d '=+/' | cut -c1-64)
CHALLENGE=$(printf %s "$VERIFIER" | openssl dgst -sha256 -binary | openssl base64 -A | tr '+/' '-_' | tr -d '=')

# 2. Autorizar con una persona ficticia (solo desarrollo)
curl -sS -k -c cookies.txt -D headers.txt -o /dev/null \
  "$ISSUER/connect/authorize?response_type=code&client_id=$CLIENT_ID&redirect_uri=$(python3 -c "import urllib.parse,sys;print(urllib.parse.quote(sys.argv[1],safe=''))" "$REDIRECT_URI")&scope=openid%20profile%20email%20identity.api&state=$STATE&nonce=$NONCE&code_challenge=$CHALLENGE&code_challenge_method=S256&mock_persona=coach.ana"

# 3. Extraer el código del Location
CODE=$(grep -i '^location:' headers.txt | sed -E 's/.*[?&]code=([^&]+).*/\1/' | tr -d '\r')
echo "code=$CODE"
```

Resultado esperado: `Location` apunta a `https://localhost:3000/callback?code=...&state=...` y el `state` coincide.

```bash
# 4. Canjear el código (única vía de emisión de tokens)
curl -sS -k -X POST "$ISSUER/connect/token" \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  --data-urlencode grant_type=authorization_code \
  --data-urlencode code="$CODE" \
  --data-urlencode redirect_uri="$REDIRECT_URI" \
  --data-urlencode client_id="$CLIENT_ID" \
  --data-urlencode code_verifier="$VERIFIER" | tee token.json
```

Resultado esperado:

- `access_token`, `id_token`, `token_type=Bearer`, `expires_in=300` y `scope`.
- `refresh_token` si se solicitó `offline_access`.
- Repetir el canje con el mismo código devuelve `400 invalid_grant`.
- Un `code_verifier` incorrecto devuelve `400 invalid_grant`.
- El access token lleva `aud=identity-api` y el ID token `aud=client_id`; se validan por separado con JWKS.

## 5. Llamada a `/me`

```bash
ACCESS_TOKEN=$(jq -r .access_token token.json)
curl -sS -k "$ISSUER/api/v1/identity/me" \
  -H "Authorization: Bearer $ACCESS_TOKEN" | jq
```

Resultado esperado: `userId`, `displayName`, `email`, `emailVerified`, `phoneVerified`, `roles`, `permissions`, `sessionId`, `authenticatedAt`, `expiresAt` y `authenticationMethods`. La cabecera incluye `Cache-Control: no-store`.

Comprobaciones de denegación:

```bash
# Sin credencial -> 401 identity.session.required
curl -sS -k -i "$ISSUER/api/v1/identity/me" | head -20

# Token manipulado -> 401 identity.session.invalid
curl -sS -k -i "$ISSUER/api/v1/identity/me" -H "Authorization: Bearer token.invalido" | head -20
```

## 6. Userinfo y ausencia de contacto de terceros

```bash
curl -sS -k "$ISSUER/connect/userinfo" -H "Authorization: Bearer $ACCESS_TOKEN" | jq
```

Resultado esperado: solo `sub`, `name`, `email` y `email_verified` del usuario autenticado. No existe endpoint de contacto de terceros; cualquier intento de obtener contacto ajeno devuelve `404` de ruta.

## 7. Sesiones, logout y revocación

```bash
# Listar sesiones propias
curl -sS -k "$ISSUER/api/v1/identity/sessions" \
  -H "Authorization: Bearer $ACCESS_TOKEN" | jq

# Revocar una sesión concreta (usar un sessionId que no sea el actual)
curl -sS -k -i -X DELETE "$ISSUER/api/v1/identity/sessions/ses_OTRA" \
  -H "Authorization: Bearer $ACCESS_TOKEN"

# Revocar todas menos la actual
curl -sS -k -i -X POST "$ISSUER/api/v1/identity/sessions/revoke-all" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"keepCurrentSession":true}'

# Logout de la sesión actual
curl -sS -k -i -X POST "$ISSUER/api/v1/identity/logout" \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

Resultado esperado:

- `204` en revocación y logout.
- Tras el logout, `/me` con el mismo access token devuelve `401` en G1 porque la sesión está revocada.
- Un `sessionId` ajeno o inexistente devuelve `404`, no `403`.
- El refresh token de la sesión revocada ya no se canjea.

> Matiz importante: otros módulos que validen el JWT localmente seguirán aceptándolo hasta su `exp` (5 min). La revocación inmediata solo se garantiza en G1 y sobre refresh tokens.

## 8. Catálogos y permisos

```bash
curl -sS -k "$ISSUER/api/v1/identity/catalog/roles" \
  -H "Authorization: Bearer $ACCESS_TOKEN" | jq

curl -sS -k "$ISSUER/api/v1/identity/catalog/permissions" \
  -H "Authorization: Bearer $ACCESS_TOKEN" | jq
```

Resultado esperado:

- 10 roles y 3 permisos de G1.
- `catalogVersion` y `provisional: true`.
- `ETag` en la respuesta.
- Un permiso no concedido se deniega; los códigos desconocidos se tratan como no concedidos.

## 9. Personas y reset del mock

```bash
curl -sS -k "$ISSUER/api/v1/identity/mock/personas" | jq
curl -sS -k -i -X POST "$ISSUER/api/v1/identity/mock/reset"
```

Resultado esperado:

- 15 personas ficticias con sus roles, permisos, estados y etiquetas; las claves `token.*` son 4 escenarios de token y no aparecen como personas.
- `fixtureVersion` estable.
- `204` en reset y estado inicial restaurado; la operación es idempotente y no requiere token en v1.
- Fuera de `Development`/`Testing` ambos endpoints devuelven `404`.

## 10. Pruebas automatizadas

```bash
# Contrato e integración (incluye validación de esquema OpenAPI 3.1)
dotnet test tests/G1.Identity.ContractTests

# Compatibilidad de proveedor (SQLite) según ADR-0002
dotnet test tests/G1.Identity.CompatibilityTests

# E2E de navegador
pwsh tests/G1.Identity.E2E/bin/Debug/net10.0/playwright.ps1 install chromium
dotnet test tests/G1.Identity.E2E
```

Resultado esperado:

- Cada operación documentada tiene al menos una prueba de éxito y una de error.
- El flujo Playwright recorre autorización, PKCE, `/me`, logout y revocación.
- Las comprobaciones de accesibilidad del selector pasan.
- La suite de contrato termina en menos de 5 minutos y la E2E en menos de 10.

## 11. Validación del contrato en CI

```bash
# Validar estructura OpenAPI 3.1
npx @redocly/cli lint specs/001-g1-identidad-contrato-mock/contracts/openapi.v1.yaml

# Detectar cambios incompatibles contra la rama base
oasdiff breaking specs/001-g1-identidad-contrato-mock/contracts/openapi.v1.yaml \
  origin/main:specs/001-g1-identidad-contrato-mock/contracts/openapi.v1.yaml
```

Resultado esperado: sin errores de validación y sin rupturas no versionadas.

## 12. Criterios de aceptación cubiertos

| Criterio | Comprobación | Tareas que lo cubren |
| --- | --- | --- |
| SC-001 | Un consumidor completa la guía y obtiene `200` de `/me` en 15 minutos o menos. | T024, T058, T065 |
| SC-002 | Las 19 operaciones tienen ejemplo de éxito (`examples` o `x-success-example`) y prueba de error; CI en verde. | T019, T052, T056 |
| SC-003 | Cambiar emisor, cliente y claves no requiere cambios en el código de negocio del consumidor (prueba de humo). | T072; T055 aporta la mitigación de proveedor |
| SC-004 | Los errores devuelven el código y esquema documentados, sin trazas ni secretos. | T018, T033, T034, T068 |
| SC-005 | El flujo Playwright pasa en Chromium en cada PR y en otro navegador en la ejecución programada. | T053, T056, T057 |
| SC-006 | El documento valida como OpenAPI 3.1 y no tiene rupturas sin versión ni nota de migración. | T020, T023, T056 |
| SC-007 | `/me` responde con p95 inferior a 200 ms en local, excluido el primer arranque. | T061 |
| SC-008 | El selector pasa las comprobaciones de accesibilidad y se usa solo con teclado. | T029, T054 |
| SC-009 | El escaneo de secretos no encuentra claves privadas, contraseñas ni tokens reales. | T006, T066 |
| SC-010 | La revocación de sesión/refresh se refleja en la siguiente validación de G1; el access token deja de aceptarse como máximo al expirar su TTL. Se miden por separado. | T026, T040, T041 |
| SC-011 | El access token usa `aud=identity-api` y el ID token `aud=client_id`; un consumidor sin esa audiencia configurada rechaza el token. | T026, T067 |
| SC-012 | No existe endpoint de contacto ajeno y userinfo solo devuelve los datos del usuario autenticado. | T052, T071 |

## 13. Problemas frecuentes

| Síntoma | Causa probable | Acción |
| --- | --- | --- |
| `401` inmediato tras canjear el código | Reloj desajustado o `code_verifier` incorrecto | Sincronizar reloj; regenerar verifier/challenge. |
| `invalid_grant` al canjear | Código ya usado, caducado o `redirect_uri` distinta | Repetir el flujo con una URI exacta. |
| La revocación falla con `ExecuteUpdate` | Falta `DisableBulkOperations()` | Añadirlo en la configuración de OpenIddict. |
| Las sesiones desaparecen | Reinicio del mock o reset | Es el comportamiento esperado de InMemory. |
| Swagger UI no carga | Entorno distinto de Development/Testing | Arrancar en Development o habilitar la UI solo para pruebas. |
| Playwright no encuentra el navegador | Navegadores no instalados | Ejecutar `playwright.ps1 install`. |
| El selector no aparece | Ya hay cookie de persona o `mock_persona` válido | Borrar cookies o usar una ventana privada. |

## 14. Qué no valida esta guía

- El servidor OAuth real de la fase B.
- El registro real, contraseñas, verificación, recuperación o 2FA.
- La persistencia relacional, migraciones o copias de seguridad.
- La revocación inmediata de access tokens JWT en módulos externos.
- La matriz definitiva de audiencias (D-016), el modo BFF/SPA (D-001) ni la política de consentimiento de producción (D-012).
