# Research: contrato de identidad y mock (fase A)

**Feature**: `001-g1-identidad-contrato-mock` | **Fecha**: 2026-11-24

Este documento consolida las decisiones técnicas de la fase A. No repite las investigaciones de compatibilidad ya registradas en `docs/g1/diagnostico-tecnico.md` ni las clarificaciones de la especificación. No queda ningún `NEEDS CLARIFICATION`: las cuestiones de contrato se cerraron con D-002, D-003, D-004, D-005 y D-017, y las de stack se verificaron en el spike documentado en los ADR.

## R-001: Motor de protocolo del mock

- **Decisión**: usar OpenIddict 7.8.0 (`OpenIddict.AspNetCore` + `OpenIddict.EntityFrameworkCore`) como motor OAuth 2.0/OIDC del mock, con ASP.NET Core Minimal APIs.
- **Rationale**: evita implementar protocolo o criptografía a mano, mantiene el mismo camino que la fase B y permite que el mock sea sustituible por el servidor real sin cambiar a los consumidores. OpenIddict cubre descubrimiento, JWKS, autorización, token, userinfo, revocación y logout.
- **Alternativas consideradas**:
  - Mock con endpoints OAuth simulados: más simple, pero se desviaría del protocolo real y crearía falsa confianza.
  - Servidor OAuth casero: prohibido por el usuario y por la constitución.
  - Proveedor externo: descartado explícitamente.
  - OpenIddict 8.0: solo preview en la fecha de decisión; no se adopta.

## R-002: Autenticación de personas en el mock

- **Decisión**: autenticación de desarrollo mediante una cookie de sesión de persona y un selector accesible. El parámetro `mock_persona` se acepta solo en `Development` y `Testing` y solo tras pasar por `/connect/authorize`. No hay contraseñas fijas ni endpoint de emisión de tokens alternativo.
- **Rationale**: el flujo de autorización sigue siendo real (código + PKCE + `/connect/token`); el selector solo sustituye la pantalla de login, no la autorización. Cumple FR-031 y FR-036 y permite automatización determinista.
- **Alternativas consideradas**:
  - Usuarios de Identity con contraseñas sembradas: añade secretos y no aporta fidelidad al contrato de la fase A.
  - Token de desarrollo directo: sería un atajo de autorización y está prohibido.
  - Selector solo por CLI: no cubre la prueba E2E de navegador ni accesibilidad.

## R-003: Contrato OpenAPI 3.1 y Swagger UI

- **Decisión**: generar el documento con `Microsoft.AspNetCore.OpenApi` 10.0.15, versionar una copia normativa en `contracts/openapi.v1.yaml` y servir Swagger UI con `Swashbuckle.AspNetCore.SwaggerUI` 10.3.1 en `/swagger` solo en desarrollo y pruebas.
- **Rationale**: .NET 10 genera OpenAPI 3.1 por defecto; Swashbuckle se usa solo como visor, tal como se verificó y se decidió en ADR-0001. La copia versionada permite revisar cambios y validar a los consumidores sin arrancar el mock.
- **Alternativas consideradas**:
  - Generar el YAML a mano sin contrastarlo con el runtime: se desviaría con facilidad.
  - Usar el paquete completo de Swashbuckle para generar: duplica la generación y no es necesario.
  - Scalar: preferido por la skill interna, pero la constitución y el usuario exigen Swagger UI.

## R-004: Validación del contrato y detección de cambios incompatibles

- **Decisión**: pruebas de contrato con xUnit v3, `WebApplicationFactory` y validación de respuestas JSON contra los esquemas OpenAPI 3.1 con `JsonSchema.Net`. En CI, `oasdiff` compara `contracts/openapi.v1.yaml` con la versión base y falla ante cambios incompatibles sin versión nueva o nota de migración.
- **Rationale**: valida forma y no solo código de estado; `oasdiff` aporta una comprobación estándar de rupturas. El contrato es el artefacto central de la fase A.
- **Alternativas consideradas**:
  - Pruebas solo de código de estado: no detectan cambios de forma.
  - Comparación manual del YAML: no es repetible ni apta para CI.
  - Aprobar automáticamente cualquier cambio aditivo: requiere revisión humana para permisos y datos sensibles.

## R-005: Persistencia y stores de OpenIddict

- **Decisión**: EF Core InMemory 10.0.15 con `EnsureCreated()`, `DisableBulkOperations()` y seeding idempotente. Sin migraciones. Los stores de OpenIddict para aplicaciones, autorizaciones y tokens usan el mismo `DbContext` que el estado del mock.
- **Rationale**: es la persistencia académica solicitada y la combinación verificada en el spike. `DisableBulkOperations()` es obligatoria porque InMemory no soporta `ExecuteUpdate`/`ExecuteUpdateAsync`; sin ella la revocación falla.
- **Alternativas consideradas**:
  - SQLite: cumple migraciones, pero no es la persistencia solicitada; queda como alternativa si no se ratifica ADR-0002.
  - SQL Server/PostgreSQL: añade infraestructura y no aporta valor a la fase A.
  - Stores en memoria propios de OpenIddict sin EF: menos fiel a la fase B y no ejercita el `DbContext` previsto.

## R-006: Modelo de sesión y vínculo con tokens

- **Decisión**: crear una entidad `Session` al autenticar la persona, incluir su `sessionId` en el claim `sid` y mantener una tabla de vínculo `SessionToken` entre sesión y tokens de OpenIddict. `/me` y las operaciones de identidad revalidan el estado de la sesión; el logout y la revocación marcan la sesión y revocan sus refresh tokens.
- **Rationale**: el contrato exige `sessionId`, listado de sesiones y revocación selectiva. Un JWT no se puede invalidar retroactivamente sin un mecanismo verificable; la revalidación en G1 y la revocación de refresh respetan esa realidad y la documentan.
- **Alternativas consideradas**:
  - Revocar por sujeto con OpenIddict: revoca todas las sesiones del usuario, no una concreta.
  - Denylist de JWT: añade estado compartido y no está en v1 (D-005).
  - Introspección por petición: fuera de v1 y con más latencia.

## R-007: PKCE, refresco y rotación

- **Decisión**: OpenIddict valida Authorization Code + PKCE S256, emite access token, ID token y refresh token rotativo; se rechazan `plain`, implicit flow, password grant y `client_credentials` para identidad de usuario. La reutilización de un refresh revocado revoca la familia.
- **Rationale**: es el flujo aprobado y el comportamiento estándar de OpenIddict; no se implementa criptografía ni lógica de protocolo propia.
- **Alternativas consideradas**:
  - PKCE `plain`: prohibido por la especificación.
  - Refresh no rotativo: mayor ventana de exposición y menos trazabilidad.
  - Sin refresh: peor UX y no cubre el escenario de sesión prolongada.

## R-008: Errores de API y errores OAuth

- **Decisión**: `application/problem+json` con `type`, `title`, `status`, `detail`, `instance`, `code` y `traceId` para la API de identidad; errores OAuth estándar (`error`, `error_description`, `error_uri`, `state`) para el protocolo. Los códigos de negocio no se reutilizan en OAuth.
- **Rationale**: cumple RFC 9457, la constitución y FR-038/FR-039; separa el contrato de negocio del protocolo.
- **Alternativas consideradas**:
  - Un único formato para todo: confunde a clientes OIDC estándar.
  - Mensajes libres sin código estable: impide pruebas y rompe consumidores.

## R-009: Rate limiting, CORS y CSRF

- **Decisión**: usar el rate limiter y el middleware antiforgery de ASP.NET Core. Límites por endpoint en autorización, token, login del mock, reset y resumen. CORS por lista de orígenes y sin comodín con credenciales. Las escrituras con cookie exigen antiforgery; el modo Bearer no lo requiere.
- **Rationale**: son mecanismos del framework, no criptografía ni lógica casera; cubren FR-041, FR-042 y FR-045.
- **Alternativas consideradas**:
  - Middleware propio: más superficie de error.
  - Sin CSRF en modo cookie: inseguro.
  - Comodín CORS con credenciales: prohibido.

## R-010: Determinismo y control del tiempo

- **Decisión**: usar `TimeProvider` en la lógica del mock y fixtures con fechas relativas al inicio de la prueba. El endpoint de reset restaura personas, sesiones y tokens de forma idempotente. Las pruebas se aíslan por colección o por instancia.
- **Rationale**: cumple NFR-003 y evita pruebas dependientes del reloj o del orden de ejecución.
- **Alternativas consideradas**:
  - Fechas fijas absolutas: caducan y obligan a mantenimiento.
  - Estado compartido entre pruebas: produce fallos intermitentes.

## R-011: Selector de personas y accesibilidad

- **Decisión**: interfaz React + TypeScript + Vite, servida como build estático desde el mock. Selector con navegación por teclado, foco visible, etiquetas, contraste y mensajes de error; sin almacenar tokens en el navegador. Pruebas de accesibilidad con axe-core inyectado en Playwright.
- **Rationale**: cumple la restricción de frontend, NFR-005 y el escenario US6.4. El selector no necesita estado de negocio ni acceso directo a datos.
- **Alternativas consideradas**:
  - HTML estático: más simple, pero incumple la restricción de React + TypeScript y dificulta reutilizar componentes en fase B.
  - Selector dentro de Swagger: no representa el flujo de navegador real ni permite probar accesibilidad del login.

## R-012: Estrategia de pruebas

- **Decisión**:
  - Contrato: xUnit v3 + `WebApplicationFactory`, una prueba de éxito y una de error por operación, validación contra el esquema y casos de audiencia/permiso.
  - E2E: Playwright 1.64.1 en Chromium en cada PR; Firefox o WebKit en la ejecución programada.
  - Accesibilidad: axe-core sobre el selector y las páginas de error.
  - Compatibilidad: prueba de humo con SQLite en CI para detectar diferencias de proveedor (mitigación ADR-0002).
- **Rationale**: cubre FR-046 a FR-048 y SC-002, SC-005 y SC-008; separa la validación de forma de la de comportamiento.
- **Alternativas consideradas**:
  - Solo pruebas unitarias: no detectan desviaciones del contrato.
  - Solo E2E: lentas y con poca cobertura de errores.
  - Un único navegador: no cumple el criterio de ejecución programada.

## R-013: Claves, secretos y configuración

- **Decisión**: clave de firma de desarrollo efímera generada al arrancar; cliente de desarrollo con `redirect_uri` exacta y secreto solo si es confidencial, inyectado por configuración de desarrollo y nunca comprometido. Escaneo de secretos en CI. En producción se gestionarán claves fuera del repositorio (fase B).
- **Rationale**: cumple NFR-004 y FR-040; evita claves privadas en el repositorio.
- **Alternativas consideradas**:
  - Clave fija en `appsettings.Development.json`: se compromete y se reutiliza.
  - Certificado autofirmado versionado: mismo problema y difícil rotación.

## R-014: Fuera de alcance de la fase A

- **Decisión**: no se implementan Identity para usuarios reales, registro, contraseñas, verificación, recuperación, 2FA, introspección, contacto de terceros, administración de usuarios ni endpoints de negocio de G2/G3/G4.
- **Rationale**: la fase A entrega contrato y mock; la fase B implementa el servidor real. Mantener el límite evita resolver decisiones pendientes por implementación.
- **Alternativas consideradas**:
  - Adelantar Identity en el mock: añade secretos y no aporta al contrato.
  - Incluir introspección: contradice D-005 para v1.
  - Incluir contacto de terceros: contradice D-017.

## Trazabilidad de decisiones

| Decisión | Requisitos afectados |
| --- | --- |
| R-001, R-007 | FR-029 a FR-037, FR-055, US2 |
| R-002, R-011 | FR-031, FR-032, NFR-005, US5, US6 |
| R-003, R-004 | FR-001 a FR-006, FR-046, FR-048, SC-002, SC-006 |
| R-005 | NFR-003, ADR-0002, restricción técnica 4 |
| R-006 | FR-018 a FR-024, FR-034, SC-010 |
| R-008 | FR-038 a FR-040 |
| R-009 | FR-041 a FR-045 |
| R-010 | NFR-003, FR-033 |
| R-012 | FR-046 a FR-048, SC-005, SC-008 |
| R-013 | NFR-004, FR-040, SC-009 |
| R-014 | D-005, D-017, FR-054, FR-057 |

## Conclusión

No quedan incógnitas técnicas que impidan el diseño de la fase A. Las únicas condiciones abiertas son de gobernanza, no de viabilidad: ratificación de ADR-0002 (InMemory) y de ADR-0001 (arquitectura de fase B), y cierre de D-001 y D-016 antes de la integración. Ninguna se resuelve implícitamente en este plan.
