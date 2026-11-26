# Cobertura del contrato G1

Matriz de las 19 operaciones declaradas en `contracts/openapi.v1.yaml`. Una operación solo se considera cerrada cuando dispone de un caso de éxito y uno de error validados contra el esquema. Las pruebas unitarias de servicio se registran como cobertura parcial, no como sustitución de la prueba HTTP contractual.

| Operación | Responsable | Éxito | Error | Estado local |
| --- | --- | --- | --- | --- |
| `getOpenIdConfiguration` | DEV2 | Pendiente | Pendiente | Bloqueada por implementación OIDC |
| `getJsonWebKeySet` | DEV2 | Pendiente | Pendiente | Bloqueada por implementación OIDC |
| `authorize` | DEV2 | Pendiente | Pendiente | Bloqueada por implementación OIDC |
| `exchangeToken` | DEV2 | Pendiente | Pendiente | Bloqueada por implementación OIDC |
| `getUserInfo` | DEV2/DEV3 | Pendiente | Pendiente | Bloqueada por OIDC y fixtures |
| `revokeToken` | DEV2 | Pendiente | Pendiente | Bloqueada por implementación OIDC |
| `logoutOidc` GET | DEV2 | Pendiente | Pendiente | Bloqueada por implementación OIDC |
| `logoutOidcPost` POST | DEV2 | Pendiente | Pendiente | Bloqueada por implementación OIDC |
| `getCurrentIdentity` | DEV3 | Pendiente | Pendiente | Bloqueada por `/me` y fixtures |
| `getUserSummary` | DEV3/DEV4 | Pendiente | Parcial en `AuthorizationTests` | Falta endpoint DEV3 |
| `lookupUsers` | DEV3/DEV4 | Pendiente | Parcial en `AuthorizationTests` | Falta endpoint DEV3 |
| `listSessions` | DEV4 | Parcial en `SessionTests` | Parcial | Falta validación HTTP contra esquema |
| `revokeSession` | DEV4 | Parcial en `SessionTests` | Parcial: sesión ajena/no encontrada | Falta validación HTTP contra esquema |
| `revokeAllSessions` | DEV4 | Parcial en `SessionTests` | Pendiente | Falta validación HTTP contra esquema |
| `logoutCurrentSession` | DEV4 | Parcial en `SessionTests` | Pendiente | Falta validación HTTP contra esquema |
| `listRoles` | DEV4 | `CatalogTests` | Parcial: autorización común | Falta validar el 401 contra esquema |
| `listPermissions` | DEV4 | `CatalogTests` | Parcial: autorización común | Falta validar el 401 contra esquema |
| `listMockPersonas` | DEV3 | Pendiente | Pendiente | Bloqueada por fixtures |
| `resetMock` | DEV3 | Pendiente | Pendiente | Bloqueada por fixtures |

## Respuestas sin cuerpo

El contrato ya incluye `x-success-example` con cuerpo `null` para:

- `DELETE /api/v1/identity/sessions/{sessionId}`.
- `POST /api/v1/identity/sessions/revoke-all`.
- `POST /api/v1/identity/logout`.
- Las operaciones OIDC sin cuerpo que correspondan deben verificarse al integrar el trabajo de DEV2.

## Cobertura transversal disponible

- `ErrorMappingTests`: esquema RFC 9457, códigos estables y separación de errores OAuth.
- `AuthorizationTests`: denegación por defecto, permisos desconocidos, traducción 401/403 y protección frente a la omisión de antiforgery mediante una cabecera Bearer falsa.
- `SecurityHeadersAndSecretScanTests`: detección de claves privadas y tokens reales.
- `PipelineIntegrationTests`: correlación, cabeceras, CSP, CORS, antiforgery y `Retry-After`.
- `SwaggerAvailabilityTests`: publicación del contrato y validación condicional por `ETag`.
- `AuditTests`: auditoría minimizada sin tokens ni datos personales.
