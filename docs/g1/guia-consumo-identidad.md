# Guía de consumo de identidad G1

Para G2 (Gestión Deportiva), G3 (Socios y Cuotas) y G4 (Comunicaciones y
Equipaciones). La fuente normativa es [OpenAPI 3.1, versión `1.0.0`][openapi].
Consulta las [reglas del contrato][contrato] y su [CHANGELOG][changelog].

**Estado de la rama:** el host publica Swagger y el documento OpenAPI, y
registra sesiones, logout de la API y catálogos. Todavía no registra el
proveedor OIDC, `/me`, consultas de usuarios ni los endpoints de fixtures. Las
pruebas de sesiones no acreditan la integración con OpenIddict. Puedes generar
el cliente ya; el recorrido autenticado de esta guía describe el resultado
esperado una vez integrada la fase A. Véase la [matriz de pruebas][cobertura].

## Arranque local

Desde la raíz del repositorio, con el SDK .NET `10.0.404`:

```powershell
dotnet dev-certs https --trust
dotnet run --project src/G1.Identity.Mock --launch-profile Development
```

Abre [Swagger UI en local](https://localhost:7180/swagger) o consulta el
[documento del host](https://localhost:7180/openapi/v1.json). Ambos se sirven
solo en `Development` y `Testing`. Usa HTTPS con un certificado de desarrollo
de confianza para probar OIDC y cookies. El [quickstart][quickstart] detalla
el build de la UI; aún falta servirla desde el host y conectarla a OIDC.

Usa solo datos ficticios. La persistencia en memoria aprobada para el alcance
académico es volátil y los datos se pierden al reiniciar ([ADR-0002][adr2]).

## Generar un cliente desde OpenAPI

Fija la revisión de `openapi.v1.yaml` que consume tu módulo y la versión del
generador. Por ejemplo, con Node.js LTS, desde la raíz de tu proyecto
TypeScript, copia allí el documento normativo y ejecuta:

```powershell
npx --yes openapi-typescript@7.13.0 ./openapi.v1.yaml -o ./g1-identity.d.ts
npm install --save-exact openapi-fetch@0.17.0
```

El primer comando genera los tipos de rutas, parámetros y respuestas. El
segundo permite crear el cliente HTTP tipado; conserva el lockfile en el
proyecto consumidor. Este ejemplo recibe el access token de una biblioteca
OIDC y apunta al origen de G1 configurado en tu módulo:

```typescript
import createClient from 'openapi-fetch';
import type { paths } from './g1-identity';

export async function getCurrentIdentity(baseUrl: string, accessToken: string) {
    const client = createClient<paths>({ baseUrl });
    return client.GET('/api/v1/identity/me', {
        headers: { Authorization: `Bearer ${accessToken}` },
    });
}
```

Comprueba el estado HTTP antes de usar `data`; procesa `error` si la llamada
falla. Los tipos generados no validan tokens ni sustituyen a la biblioteca
OIDC, y no verifican por sí solos el JSON recibido.

## Configurar OIDC y PKCE S256

| Parámetro | Valor o regla |
| --- | --- |
| Emisor local | `https://localhost:7180` |
| Descubrimiento | `GET /.well-known/openid-configuration`; comprueba que `issuer` coincida con el configurado y usa los endpoints y `jwks_uri` anunciados. |
| Cliente del quickstart | `cli_mock_spa`, cliente público de prueba previsto; su registro en el proveedor está pendiente en esta rama. |
| URI de retorno del quickstart | `https://localhost:3000/callback`, para el ejemplo local; la URI usada debe coincidir exactamente con la registrada para el cliente. |
| Scopes | `openid profile email identity.api` para el ejemplo; añade `offline_access` si necesitas refresh. Son los scopes de G1, no permisos de otros módulos. |
| Flujo | `response_type=code` y `code_challenge_method=S256`; v1 no admite `plain`, flujo implícito, password grant ni `client_credentials` para identidad de usuario. |

1. Inicia `GET /connect/authorize` con el cliente, la URI registrada, los
   scopes, `state` y `nonce` nuevos y el challenge PKCE. S256 usa
   `BASE64URL(SHA256(code_verifier))` sin relleno; el verifier tiene de 43 a
   128 caracteres. Conserva el verifier, `state` y `nonce` solo durante la
   transacción en el cliente OIDC.
2. Si no hay sesión, el contrato prevé la selección de persona. En
   `Development`/`Testing`, `mock_persona` permite automatizar la selección;
   no emite tokens. Al recibir el retorno en la URI registrada, comprueba el
   `state` antes de usar el código. Si no coincide o hay error, aborta.
3. Canjea el código una sola vez en `POST /connect/token` como
   `application/x-www-form-urlencoded`, con `grant_type=authorization_code`,
   `client_id`, `code`, la misma `redirect_uri` y `code_verifier`. El cliente
   público no usa secreto. El proveedor solo emite tokens en ese endpoint.
4. Valida el ID token con la biblioteca OIDC, incluido el `nonce` de la
   transacción. Envía el access token como Bearer a la API de G1. No uses el
   ID token como credencial de API.

| Token | Uso y validación |
| --- | --- |
| ID token | Identifica al usuario ante el cliente OIDC. Valida firma con JWKS, `iss`, vigencia y `nonce`; `aud` corresponde al `client_id`. |
| Access token | Autoriza llamadas Bearer a la API destinataria. La API valida firma con JWKS, `iss`, vigencia (`exp` y `nbf` cuando corresponda), su audiencia y sus políticas de scopes y permisos. El contrato v1 de G1 usa `aud=identity-api`, **provisional por D-016**. |
| Refresh token | Opaco y rotativo. Se canjea en `/connect/token` con `grant_type=refresh_token`; sustituye el anterior por el nuevo. No sirve como Bearer para la API. |

No aceptes un access token en G2, G3 o G4 solo por su firma ni por el scope
`identity.api`. Cada API debe rechazarlo si no tiene esa audiencia
explícitamente autorizada. La matriz definitiva sigue abierta en
[D-016][d016]. El contrato contempla también `__Host-g1.session` (`HttpOnly`,
`Secure`, `SameSite=Lax`); las escrituras con cookie requieren antiforgery.
El ejemplo Bearer no decide BFF frente a SPA ([D-001][d001]).

### Prueba local de `/me` cuando se integre el proveedor

Con el host en ejecución, guarda este ejemplo como `g1-probar-me.mjs` fuera
del repositorio y ejecútalo con `node --use-system-ca g1-probar-me.mjs`
(Node.js 24 y certificado local de confianza). Usa el cliente y la URI del
quickstart una vez registrados; la prueba automatiza la persona `coach.ana`,
sin seguir la redirección ni guardar o mostrar tokens. Comprueba el canje
y la respuesta HTTP; no sustituye la validación de ambos JWT con una
biblioteca OIDC.

```javascript
import assert from 'node:assert/strict';
import { createHash, randomBytes } from 'node:crypto';

const issuer = 'https://localhost:7180';
const clientId = 'cli_mock_spa';
const redirectUri = 'https://localhost:3000/callback';
const state = randomBytes(16).toString('hex');
const nonce = randomBytes(16).toString('hex');
const verifier = randomBytes(32).toString('base64url');
const challenge = createHash('sha256').update(verifier).digest('base64url');

const authorize = new URL('/connect/authorize', issuer);
authorize.search = new URLSearchParams({
    response_type: 'code',
    client_id: clientId,
    redirect_uri: redirectUri,
    scope: 'openid profile email identity.api',
    state,
    nonce,
    code_challenge: challenge,
    code_challenge_method: 'S256',
    mock_persona: 'coach.ana',
}).toString();
const authorization = await fetch(authorize, { redirect: 'manual' });
assert.equal(authorization.status, 302, 'Se esperaba la redirección de autorización');
const location = authorization.headers.get('location');
assert(location, 'Falta la cabecera Location');
const callback = new URL(location, issuer);
assert(callback.origin + callback.pathname === redirectUri, 'El retorno no es el registrado');
assert(callback.searchParams.get('state') === state, 'El state no coincide');
assert(!callback.searchParams.has('error'), 'La autorización devolvió un error');
const code = callback.searchParams.get('code');
assert(code, 'Falta el código de autorización');

const exchange = await fetch(new URL('/connect/token', issuer), {
    method: 'POST',
    redirect: 'error',
    body: new URLSearchParams({
        grant_type: 'authorization_code',
        client_id: clientId,
        redirect_uri: redirectUri,
        code,
        code_verifier: verifier,
    }),
});
assert.equal(exchange.status, 200, 'Falló el canje de código');
const tokens = await exchange.json();
assert(typeof tokens.access_token === 'string' && tokens.access_token, 'Falta el access token');

const response = await fetch(new URL('/api/v1/identity/me', issuer), {
    redirect: 'error',
    headers: { Authorization: `Bearer ${tokens.access_token}` },
});
assert.equal(response.status, 200, 'No se obtuvo un 200 de /me');
assert.equal(response.headers.get('cache-control'), 'no-store');
const identity = await response.json();
assert(typeof identity.userId === 'string' && identity.userId, 'Falta userId');
assert(identity.roles.includes('coach'), 'La persona no tiene el rol esperado');
console.log('GET /api/v1/identity/me: 200');
```

Para comprobar el descubrimiento, `userinfo`, el refresh y las respuestas
negativas, usa los escenarios del [quickstart][quickstart]. No registres los
códigos, tokens ni secretos durante las pruebas.

## Identidad, permisos y contacto

- Usa `userId` de `/api/v1/identity/me` como referencia opaca. `roles` y
  `permissions` representan los códigos efectivos; no son el catálogo ni
  la matriz completa. Los claims del JWT son una fotografía al emitirlo.
- Consulta `GET /api/v1/identity/catalog/roles` y
  `GET /api/v1/identity/catalog/permissions`: v1 define 10 roles y 3 permisos
  propios de G1, con `catalogVersion: 1.0.0-provisional`, `provisional: true` y
  `ETag`. Ignora campos nuevos y trata los permisos desconocidos como no
  concedidos. La matriz aprobada para fase A está en la [especificación][spec].
- Para `GET /api/v1/identity/users/{userId}/summary` se requiere
  `identity.users.summary.read`; para `POST /api/v1/identity/users/lookup`,
  `identity.users.lookup`. Envía entre 1 y 100 identificadores únicos por
  lote y trata `users` y `missing` por separado. `identity.admin` está
  reservado para operaciones futuras; v1 no ofrece administración de usuarios.
- Según [D-003][d003], cada módulo resuelve los ámbitos con `userId` y sus
  propios datos: por ejemplo, G2 resuelve los equipos de un entrenador. Un
  rol de G1 no acredita por sí solo acceso a un recurso del módulo.
- Según [D-017][d017], `/me` expone el correo propio y `GET /connect/userinfo`
  devuelve claims del usuario autenticado: `sub`, `name` cuando corresponda,
  y `email` y `email_verified` con scope `email`. No hay scope `phone`.
  Los resúmenes de terceros no incluyen correo, teléfono ni datos del tutor.
  G4 obtiene el contacto de terceros de G3 cuando exista su contrato.

## Errores y cabeceras

La API de identidad usa `application/problem+json` (RFC 9457). Usa `code`
para decidir el tratamiento; `type`, `title`, `status` y `traceId` también
son obligatorios en OpenAPI. `detail`, `instance` y `errors` pueden aparecer.

| HTTP | `code` | Qué indica |
| --- | --- | --- |
| `400` | `identity.request.invalid` | Petición inválida; revisa parámetros, cuerpo o cabeceras. |
| `401` | `identity.session.required` | Falta la credencial. |
| `401` | `identity.session.invalid` | Credencial caducada, revocada o inválida. |
| `403` | `identity.permission.denied` | La identidad es válida, pero no tiene el permiso. |
| `404` | `identity.user.not_found` | Recurso inexistente o no visible. |
| `429` | `identity.rate_limited` | Límite superado; respeta `Retry-After`. |
| `500` | `identity.internal_error` | Fallo interno sin detalles del servidor. |

Los errores del protocolo OAuth usan `error` y, según la respuesta,
`error_description`, `error_uri` o `state`. Por ejemplo, un código reutilizado
o un verifier incorrecto produce `invalid_grant` en el canje. Comprueba el
estado y el `Content-Type` de cada respuesta: `userinfo` documenta
`WWW-Authenticate: Bearer` y `application/problem+json` para `401`, y los
`429` remiten al problema común. Usa `X-Correlation-ID` y `traceId` para
diagnóstico; respeta `Cache-Control: no-store` y no intentes leer JSON de un
`204` o del `200` vacío de `/connect/revoke`.

## Logout, revocación y expiración

- `POST /api/v1/identity/logout` revoca la sesión actual y sus refresh
  tokens. Para otras sesiones propias, usa
  `DELETE /api/v1/identity/sessions/{sessionId}` o envía
  `{"keepCurrentSession":true}` a `POST /api/v1/identity/sessions/revoke-all`
  para conservar la actual. La API devuelve `204` en éxito.
- `GET` o `POST /connect/logout` cierra la sesión del proveedor OIDC. Si
  envías `post_logout_redirect_uri`, debe coincidir exactamente con una URI
  registrada. `POST /connect/revoke` revoca un token mediante RFC 7009; su
  `200` vacío tampoco distingue un token ya inválido.
- En G1, la revocación de la sesión o del refresh debe surtir efecto en la
  siguiente validación: tras el logout, `/me` debe rechazar la sesión y el
  refresh revocado no puede volver a canjearse. Esto queda pendiente de
  comprobar con sesiones emitidas por el proveedor integrado.
- Un access token JWT ya emitido puede seguir siendo válido en otra API que
  lo acepte expresamente y lo valide de forma local hasta su `exp`. Su vida
  por defecto es de cinco minutos en desarrollo y pruebas. No hay
  introspección en v1 ni garantía de revocación instantánea del JWT fuera de
  G1 ([D-004][d004], [D-005][d005]). Comprueba la revocación y la caducidad
  por separado.

## Fixtures y sustitución del proveedor

En `Development` y `Testing`, `GET /api/v1/identity/mock/personas` publica
los 15 perfiles ficticios con `fixtureVersion` y paginación (`pageSize`
máximo 100). `POST /api/v1/identity/mock/reset` restaura personas, sesiones
y tokens, de forma idempotente, con `204` y sin token de reset. Ambas rutas
devuelven `404` fuera de esos entornos. Resetea solo tu instancia de pruebas.
Las cuatro claves `token.*` de la [especificación][spec] son escenarios de
token, no personas, y no crean una vía de emisión fuera de `/connect/token`.

Para cambiar de proveedor, conserva el cliente de negocio sobre el
contrato y configura por entorno el emisor OIDC, el cliente registrado y sus
credenciales si corresponde, las URI registradas y las claves públicas de
confianza obtenidas por descubrimiento/JWKS. No fijes una clave efímera en el
código ni heredes la audiencia del mock para otro despliegue. Repite las
pruebas de identidad, permisos, errores y rechazo de credenciales fuera de
la configuración esperada. La prueba de sustitución por configuración
(T072/SC-003) está pendiente; el servidor real no forma parte de fase A.

## Validación pendiente

- **DEV1:** revisión de exactitud contractual de esta guía (T058/FR-049) y
  del CHANGELOG (T063/FR-050).
- **G2, G3 o G4:** cuando estén integrados el registro del cliente, OIDC,
  fixtures y `/me`, seguir la guía desde un clon y registrar un `200` de
  `/me` en 15 minutos o menos (SC-001). Falta además ejecutar con el host
  integrado las comprobaciones de audiencias, contacto y revocación.

[openapi]: ../../specs/001-g1-identidad-contrato-mock/contracts/openapi.v1.yaml
[contrato]: ../../specs/001-g1-identidad-contrato-mock/contracts/README.md
[changelog]: CHANGELOG-contrato.md
[cobertura]: ../../tests/G1.Identity.ContractTests/README.md
[adr2]: ../decisions/ADR-0002-inmemory-persistencia-academica.md
[quickstart]: ../../specs/001-g1-identidad-contrato-mock/quickstart.md
[spec]: ../../specs/001-g1-identidad-contrato-mock/spec.md#55-superficie-del-contrato
[d001]: ../decisions/pending-decisions.md#d-001-bff-con-cookies-o-spa-pública-con-tokens
[d003]: ../decisions/pending-decisions.md#d-003-ámbitos-de-rol-y-propiedad-de-los-ámbitos
[d004]: ../decisions/pending-decisions.md#d-004-duración-de-tokens-refresco-y-sesión
[d005]: ../decisions/pending-decisions.md#d-005-estrategia-de-revocación
[d016]: ../decisions/pending-decisions.md#d-016-audiencias-definitivas-por-api
[d017]: ../decisions/pending-decisions.md#d-017-fuente-de-datos-de-contacto-para-g4
