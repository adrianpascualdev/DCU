# Contrato de identidad G1 — v1 (fase A)

- **Propietario**: equipo G1 (Infraestructura).
- **Consumidores**: G2 (Gestión Deportiva), G3 (Socios y Cuotas) y G4 (Comunicaciones y Equipaciones).
- **Estado**: contrato de la fase A, ejecutado por el mock de identidad. No es todavía el servidor OAuth real.
- **Fecha de revisión**: 2026-11-24.
- **Especificación**: [`../spec.md`](../spec.md).
- **Documento normativo**: [`openapi.v1.yaml`](./openapi.v1.yaml), OpenAPI 3.1.

## 1. Qué cubre este contrato

1. La superficie OIDC/OAuth observable: descubrimiento, JWKS, autorización, token, userinfo, revocación y logout.
2. La API de identidad que consumen G2, G3 y G4: `/me`, resumen y lote de usuarios, sesiones, logout y catálogos.
3. Los endpoints exclusivos de desarrollo: personas y reset del mock. `mock/reset` no usa token en v1: solo existe en `Development`/`Testing` y devuelve `404` en otros entornos.

No cubre: registro real, contraseñas, verificación real, recuperación, 2FA, administración de usuarios, introspección, datos de negocio de otros módulos ni contacto de terceros.

## 2. Reglas de uso

- **Autenticación**: Authorization Code + PKCE S256 exclusivamente. Prohibidos implicit flow, password grant, `client_credentials` para identidad de usuario y `code_challenge_method=plain`.
- **Audiencia**: `identity-api` es provisional y solo identifica la API de G1. Cada API consumidora DEBE validar `aud` y aplicar su propia política; un token de G1 no se acepta por defecto en G2, G3 o G4 (D-016).
- **Scopes**: `openid`, `profile`, `email`, `offline_access` e `identity.api`. El teléfono no se publica en v1 porque no existe scope `phone`.
- **Roles y permisos**: `/me` devuelve `roles` y `permissions` efectivos. G1 no interpreta ámbitos de negocio; cada módulo resuelve su ámbito con `userId` y sus propios datos (D-003).
- **Contacto**: no existe endpoint de contacto de terceros. `/connect/userinfo` solo devuelve datos del usuario autenticado; G4 obtiene los contactos de G3 (D-017).
- **Revocación**: la sesión y los refresh tokens se revocan de inmediato en G1. Un access token JWT sigue siendo aceptado por otros módulos hasta su `exp` (5 min por defecto). No se promete revocación inmediata del JWT.
- **Introspección**: `/connect/introspect` no forma parte de v1.
- **Datos**: todos los fixtures son ficticios. El mock se ejecuta solo en desarrollo y pruebas.

## 3. Versionado y cambios

- La versión funcional vive en la ruta (`/api/v1/...`) y en `info.version`.
- Los cambios aditivos compatibles se publican dentro de `v1` y se anuncian en el CHANGELOG.
- Un cambio incompatible requiere `/api/v2`, nota de migración y periodo de convivencia (D-014 pendiente de ratificación; mínimo recomendado: un semestre o la duración del curso).
- Los consumidores DEBEN ignorar campos desconocidos y tratar códigos de permiso desconocidos como no concedidos.
- La copia versionada `openapi.v1.yaml` se compara en CI con `oasdiff`; una ruptura sin versión nueva o nota de migración hace fallar el pipeline.

## 4. Códigos de error de la API

Formato `application/problem+json` (RFC 9457):

| Código | HTTP | Significado |
| --- | --- | --- |
| `identity.session.required` | 401 | No se ha enviado credencial. |
| `identity.session.invalid` | 401 | Credencial caducada, revocada o inválida. |
| `identity.permission.denied` | 403 | Identidad válida sin permiso suficiente. |
| `identity.user.not_found` | 404 | Usuario inexistente o no visible. |
| `identity.request.invalid` | 400 | Cuerpo, parámetros o cabeceras inválidos. |
| `identity.rate_limited` | 429 | Límite de peticiones superado. |
| `identity.internal_error` | 500 | Error no expuesto al cliente. |

Los errores OAuth usan el formato estándar y no reutilizan estos códigos:

`invalid_request`, `invalid_client`, `invalid_grant`, `unauthorized_client`, `unsupported_grant_type`, `invalid_scope`, `access_denied`, `server_error`, `temporarily_unavailable`.

## 5. Cabeceras

- `X-Correlation-ID`: se acepta y se devuelve; si falta, el servidor genera uno.
- `Cache-Control: no-store` en identidad, sesiones y userinfo.
- `ETag` en catálogos, con validación por el cliente.
- `Retry-After` en respuestas `429`.
- `WWW-Authenticate: Bearer` en `401` de userinfo y de la API cuando corresponda.

## 6. Proceso de cambio

1. Proponer el cambio en la especificación y en este contrato.
2. Ejecutar validación OpenAPI 3.1, pruebas de contrato y `oasdiff`.
3. Si es incompatible, abrir `v2`, escribir la nota de migración y avisar a G2, G3 y G4.
4. Actualizar la guía de consumo, el CHANGELOG y los ejemplos.
5. Aprobar con el propietario de G1 y los consumidores afectados.

## 7. Generación y verificación

- El documento se genera en runtime con `Microsoft.AspNetCore.OpenApi` y se publica en `/openapi/v1.json`.
- `openapi.v1.yaml` es la copia normativa revisable; CI comprueba que no diverge del runtime.
- Swagger UI se sirve en `/swagger` solo en desarrollo y pruebas.
- La validación de respuestas usa el esquema OpenAPI 3.1 con `JsonSchema.Net`.
- Las respuestas sin cuerpo (`204`, RFC 7009 `200` vacío, redirecciones) documentan su ejemplo de éxito con la extensión `x-success-example`; las respuestas con cuerpo usan `examples` estándar. Así las 19 operaciones tienen ejemplo de éxito y de error sin inventar cuerpos para respuestas vacías.

## 8. Claims de los tokens

El contrato no decodifica los JWT, pero los consumidores necesitan conocer sus claims. La audiencia depende del tipo de token: el access token se emite para `identity-api` y el ID token para el `client_id` que inició el flujo. No se usa una sola audiencia para ambos.

**Access token**:

| Claim | Contenido |
| --- | --- |
| `iss` | Emisor OIDC del mock. |
| `aud` | `identity-api` (provisional, D-016). |
| `sub` | `userId` opaco. |
| `client_id` | Cliente que obtuvo el token. |
| `scope` | Scopes concedidos. |
| `exp`, `iat`, `nbf` | Vigencia y tolerancia de reloj. |
| `jti` | Identificador único del token. |
| `sid` | `sessionId` de la sesión. |
| `roles` | Códigos de rol efectivos. |
| `permissions` | Códigos de permiso efectivos. |
| `email_verified` | Estado de verificación del correo. |
| `phone_verified` | Estado de verificación del teléfono. |

**ID token**: `iss`, `aud` (el `client_id`), `sub`, `exp`, `iat`, `auth_time`, `nonce`, `amr` y `sid`. No incluye datos de terceros.

Los claims de rol y permiso son una fotografía del momento de emisión: si el usuario pierde un rol, el token anterior sigue vigente hasta su `exp`. La ventana máxima es el TTL del access token (5 min por defecto).
