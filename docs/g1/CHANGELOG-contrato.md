# Historial del contrato de identidad G1

## 1.0.0

Versión de `info.version` del [contrato normativo][openapi], OpenAPI `3.1.0`,
revisado el **24 de noviembre de 2026** (`x-reviewed-at`). Es la referencia
inicial de **fase A** para G2, G3 y G4. El catálogo y la audiencia de acceso
de G1 son provisionales. La versión del documento no implica que todas las
operaciones estén disponibles en el host.

### Superficie definida

- OIDC/OAuth: descubrimiento y JWKS, Authorization Code con PKCE S256,
  canje y refresh rotativo, userinfo propio, revocación y logout del
  proveedor. La única vía contractual de emisión es `/connect/token`.
- API `/api/v1/identity`: identidad propia (`/me`), resúmenes mínimos y
  consulta por lote, sesiones propias, logout de API y catálogos.
- Fixtures de personas y reset sin token para `Development`/`Testing`.
  Datos ficticios y volátiles, según la excepción de persistencia anotada
  en `x-persistence-exception`.

En la rama revisada, Swagger/OpenAPI, sesiones/logout de API y catálogos ya
están registrados en el host. La [matriz de cobertura][cobertura] detalla qué
falta para el recorrido autenticado y la validación de las operaciones. La
versión no incluye servidor OAuth real, registro o verificación reales,
recuperación, 2FA, administración de usuarios, introspección ni contacto de
terceros.

### Estado de las decisiones

| Estado | Decisiones que afectan a esta versión |
| --- | --- |
| Resueltas para fase A | **D-002:** catálogo provisional de 10 roles y 3 permisos de G1, con denegación por defecto. **D-003:** cada módulo resuelve sus ámbitos por `userId`. **D-004:** vigencias aprobadas solo para desarrollo y pruebas. **D-017:** G1 expone contacto propio; G4 obtiene el de terceros de G3, pendiente de su contrato. |
| Alcance parcial | **D-005:** v1 no ofrece introspección. La revocación de sesión y refresh debe ser efectiva en G1 en la siguiente validación; un JWT ya emitido puede conservar validez hasta `exp` fuera de G1. La estrategia de producción sigue abierta. |
| Excepción ratificada | **D-009 / [ADR-0002][adr2]:** el equipo G1 ratificó InMemory para desarrollo, demostración y pruebas académicas el 25 de noviembre de 2026. La persistencia definitiva y las migraciones siguen pendientes. |
| Abiertas | **D-001:** BFF o SPA. **D-012:** consentimiento de primera parte. **D-014:** duración definitiva de la convivencia. **D-016:** audiencias definitivas; en v1 `identity-api` es provisional para el access token de G1 y el ID token usa el `client_id`. |
| Propuesta | **[ADR-0001][adr1]:** arquitectura del servidor propio OAuth/OIDC con ASP.NET Core Identity y OpenIddict, pendiente de ratificar para fase B. |

El [registro de decisiones][decisiones] conserva los estados y propuestas
completos. Esta versión no aprueba nuevas relaciones tutor–menor, roles
adicionales ni navegación común.

## Política de cambios

1. Propón los cambios en la especificación y el contrato y acuerda su
   alcance con G1 y los consumidores afectados.
2. Publica los cambios aditivos compatibles en `v1` y anúncialos aquí.
   Los consumidores deben ignorar campos desconocidos y tratar los permisos
   desconocidos como no concedidos.
3. Si el cambio rompe compatibilidad, publica `/api/v2`, prepara una nota
   y plan de migración, avisa a G2/G3/G4 y acuerda un periodo explícito de
   convivencia. El [contrato][contrato] cita un semestre o la duración del
   curso como mínimo recomendado; la ventana definitiva sigue abierta en
   [D-014][d014]. Esta entrada no anuncia una `v2` ni una retirada de `v1`.
4. Valida OpenAPI 3.1, ejecuta las pruebas de contrato y compara con
   `oasdiff`. La puerta de CI está prevista para detectar rupturas no
   versionadas. Actualiza el README del contrato, la [guía de consumo][guia],
   este historial y los ejemplos según el cambio.

**Revisión pendiente:** DEV1 debe confirmar la exactitud contractual de la
entrada `1.0.0` y de la política de versionado (T063/FR-050).

[openapi]: ../../specs/001-g1-identidad-contrato-mock/contracts/openapi.v1.yaml
[contrato]: ../../specs/001-g1-identidad-contrato-mock/contracts/README.md
[cobertura]: ../../tests/G1.Identity.ContractTests/README.md
[decisiones]: ../decisions/pending-decisions.md
[adr1]: ../decisions/ADR-0001-servidor-oauth-propio-identity-openiddict.md
[adr2]: ../decisions/ADR-0002-inmemory-persistencia-academica.md
[d014]: ../decisions/pending-decisions.md#d-014-política-de-deprecación-de-versiones
[guia]: guia-consumo-identidad.md
