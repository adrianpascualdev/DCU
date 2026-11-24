---

description: "Task list for feature implementation"
---

# Tasks: Contrato de identidad y mock para G2, G3 y G4 — fase A

**Input**: Design documents from `/specs/001-g1-identidad-contrato-mock/`

**Prerequisites**: `plan.md` (required), `spec.md` (required for user stories), `research.md`, `data-model.md`, `contracts/`, `.specify/memory/constitution.md`

**Tests**: se incluyen porque la feature los exige explícitamente (US6, FR-046 a FR-048 y SC-002/SC-005/SC-008).

**Organization**: tareas agrupadas por user story para implementación y prueba independientes. Solo fase A: contrato OpenAPI 3.1 + mock funcional + Swagger UI + pruebas. No se implementa Identity real, registro ni servidor OAuth de producción.

## Convenciones y herramientas

- **Skills de `.github/skills/`**: OpenCode **no las descubre automáticamente**. Comprobado en esta sesión: el registro de skills solo contiene `opencode` y `report`, y la carga de `dotnet-api-architecture` devuelve `Unable to load skill dotnet-api-architecture`. GitHub Copilot sí las carga por `name`/`description`. Por tanto, cada tarea que las necesita incluye la ruta explícita `.github/skills/<nombre>/SKILL.md`; el agente debe leerla antes de ejecutar la tarea. El detalle se documenta en T008.
- **Instrucciones obligatorias**: `.github/instructions/` y `.github/copilot-instructions.md`. Especialmente `coding-style-csharp.instructions.md`, `coding-style-csharp-program.instructions.md`, `coding-style-csharp-builder-extensions.instructions.md`, `coding-style-csharp-app-extensions.instructions.md`, `coding-style-csharp-settings-classes.instructions.md`, `coding-style-csharp-localization.instructions.md`, `coding-style-csharp-resource-files.instructions.md`, `coding-style-csharp-constants.instructions.md`, `coding-style-react-typescript.instructions.md`, `coding-style-css-theming.instructions.md`, `coding-style-frontend-design-system.instructions.md`, `coding-style-markdown.instructions.md`, `dotnet-environment-configuration.instructions.md`, `wow-spec-driven-development.instructions.md`, `wow-no-dummy-no-placholder-code.instructions.md` y `wow-follow-up-question.instructions.md`.
- **Idioma**: textos para personas en castellano; identificadores de código en inglés. Sin valores de relleno ni marcadores de posición.
- **Estilo**: `.editorconfig` manda (CRLF, 4 espacios; 2 espacios en JSON/csproj/props).
- **Excepción documentada**: `.github/instructions/coding-style-csharp-project-structure.instructions.md` prefiere Scalar para proyectos nuevos, pero la constitución y el mandato del usuario exigen Swagger UI. Se aplica Swagger UI y se registra la excepción en T059; no se modifica la instrucción.
- **Skill referenciada ausente**: `coding-style-css-theming.instructions.md` y `coding-style-frontend-design-system.instructions.md` citan `myadmon-theme`, que no existe en `.github/skills/`. La T029 define tokens locales concretos y accesibles hasta que se aporte la skill.
- **Excepción pendiente**: EF Core InMemory incumple la restricción técnica 4 de la constitución (migraciones versionadas) hasta que se ratifique ADR-0002. Ninguna tarea de implementación puede ejecutarse antes de esa ratificación.

## Formato: `[ID] [P?] [Story] Description`

- **[P]**: archivos distintos, sin dependencias de tareas incompletas.
- **[Story]**: obligatorio en fases de user story; ausente en Setup, Foundational y Polish.
- Toda tarea incluye una ruta de archivo exacta.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: solución, proyectos, versiones, entornos y CI antes de cualquier historia.

- [ ] T001 Crear la solución y la estructura de carpetas de `plan.md` en `G1.Identity.slnx`, `src/`, `tests/` y `docs/g1/` (Skill: `dotnet-api-architecture` en `.github/skills/dotnet-api-architecture/SKILL.md`)
- [ ] T002 Fijar `global.json` con SDK `10.0.404` y `Directory.Packages.props` con OpenIddict 7.8.0, EF Core InMemory 10.0.15, `Microsoft.AspNetCore.OpenApi` 10.0.15, `Swashbuckle.AspNetCore.SwaggerUI` 10.3.1, `xunit.v3` 4.0.3, `Microsoft.Playwright` 1.64.1 y `JsonSchema.Net`; usar la CLI `dotnet` para añadir paquetes (Skill: `nuget-package-manager` en `.github/skills/nuget-package-manager/SKILL.md`)
- [ ] T003 [P] Crear `src/G1.Identity.Mock/G1.Identity.Mock.csproj` con `net10.0`, C# 14, nullable activado y sin controladores MVC (Skill: `dotnet-api-architecture`)
- [ ] T004 [P] Crear `src/G1.Identity.Mock.Ui/package.json`, `package-lock.json`, `tsconfig.json` y `vite.config.ts` con React 19, TypeScript estricto y build propio; sin tokens en el navegador (Instrucciones: `coding-style-react-typescript.instructions.md`, `coding-style-css-theming.instructions.md`)
- [ ] T005 [P] Crear `tests/G1.Identity.ContractTests/G1.Identity.ContractTests.csproj`, `tests/G1.Identity.E2E/G1.Identity.E2E.csproj` y `tests/G1.Identity.CompatibilityTests/G1.Identity.CompatibilityTests.csproj` con las versiones centralizadas de T002
- [ ] T006 Crear `.github/workflows/ci-g1-identity.yml` con restore/build/test, `npx @redocly/cli lint`, `oasdiff breaking`, escaneo de secretos e instalación de navegadores Playwright; sin publicar artefactos del mock
- [ ] T007 [P] Crear `src/G1.Identity.Mock/Properties/launchSettings.json` con HTTPS `7180` y HTTP `5180` en `Development`, y `src/G1.Identity.Mock/appsettings.json` / `appsettings.Development.json` con issuer, cliente, scopes y TTL de D-004; sin secretos (Instrucción: `dotnet-environment-configuration.instructions.md`)
- [ ] T008 [P] Crear `docs/g1/herramientas-y-skills.md` documentando que OpenCode no descubre `.github/skills/`, la evidencia de la carga fallida y la asignación de skills por tarea; incluir la ausencia de `myadmon-theme` (Instrucción: `coding-style-markdown.instructions.md`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: infraestructura que BLOCKA todas las historias. No se inicia ninguna US hasta completarla.

- [ ] T009 Crear `src/G1.Identity.Mock/Settings/MockOptions.cs`, `OpenIdConnectOptions.cs`, `SessionOptions.cs` y `CorsOptions.cs` con Options pattern, validación fail-fast y los valores de D-004: «código 5 min, access 5 min, ID token 5 min, refresh rotativo 8 h, inactividad 30 min, absoluta 12 h, tolerancia 2 min» (Instrucciones: `coding-style-csharp-settings-classes.instructions.md`, `coding-style-csharp-localization.instructions.md`)
- [ ] T010 Crear `src/G1.Identity.Mock/Persistence/G1IdentityDbContext.cs` con EF Core InMemory, `EnsureCreated()`, `DisableBulkOperations()` y sin migraciones; advertencia de volatilidad en logs (Skill: `dotnet-api-architecture`; ADR-0002)
- [ ] T011 [P] Crear `src/G1.Identity.Mock/Persistence/Entities/MockPersona.cs`, `Role.cs`, `Permission.cs`, `RolePermission.cs` y `RoleAssignment.cs` cumpliendo literalmente: «`key` y `userId` son únicos.», «Un `userId` no cambia entre reinicios ni entre resets.», «`email` y `phone` nunca se exponen en el resumen de otro usuario.», «Una persona `inactive` o `locked` no puede completar la autorización; el error es genérico.», «Una asignación revocada no contribuye a `roles` ni a `permissions`.» y «`scopeRef` se transporta sin validación de negocio; el módulo propietario lo resuelve.»
- [ ] T012 [P] Crear `src/G1.Identity.Mock/Persistence/Entities/Session.cs`, `SessionToken.cs`, `AuditEvent.cs` y `CatalogSnapshot.cs` cumpliendo literalmente: «Una sesión `revoked` o `expired` no permite `/me`, ni refresh, ni operaciones de identidad.», «`expiresAt` nunca supera `absoluteExpiresAt`.», «El listado solo devuelve sesiones del usuario autenticado y nunca IP ni secretos.», «La sesión actual se marca con `current: true` en la respuesta.» y «No se registran tokens, contraseñas, correos completos ni datos de salud.»
- [ ] T013 [P] Crear `src/G1.Identity.Mock/Constants.cs` y `src/G1.Identity.Mock/Resources/IdentityMessages.resx` con los códigos de error estables de `contracts/README.md` y textos en castellano (Instrucciones: `coding-style-csharp-constants.instructions.md`, `coding-style-csharp-resource-files.instructions.md`)
- [ ] T014 Crear `src/G1.Identity.Mock/BuilderExtensions.cs`, `AppExtensions.cs` y `Program.cs` con pipeline Minimal API: correlación, CORS sin comodín con credenciales, antiforgery, rate limiting, `X-Correlation-ID` y manejador de excepciones (Skill: `dotnet-api-architecture`; Instrucciones: `coding-style-csharp-program.instructions.md`, `coding-style-csharp-app-extensions.instructions.md`, `coding-style-csharp-builder-extensions.instructions.md`)
- [ ] T015 Configurar OpenIddict 7.8.0 en `src/G1.Identity.Mock/Auth/OpenIddictConfiguration.cs`: solo `authorization_code` + `refresh_token`, PKCE S256 obligatorio, scopes `openid profile email offline_access identity.api`, clave RSA efímera, sin introspección, sin implicit, password ni `client_credentials` (Skills: `dotnet-api-architecture` y `microsoft-docs-and-code-consultant` en `.github/skills/microsoft-docs-and-code-consultant/SKILL.md`, este último solo si el MCP de Microsoft Learn está disponible)
- [ ] T016 Configurar la autenticación de persona del mock en `src/G1.Identity.Mock/Auth/MockPersonaAuthentication.cs` con cookie `__Host-g1.session` HttpOnly/Secure/SameSite=Lax y antiforgery en escrituras; el parámetro `mock_persona` solo se acepta en `Development`/`Testing` y no emite tokens por sí mismo (Skill: `dotnet-api-architecture`)
- [ ] T017 Crear `src/G1.Identity.Mock/Fixtures/SeedData.cs` con 18 personas ficticias, 10 roles, 3 permisos de G1 y la matriz deny-by-default exacta de `data-model.md`; seeding idempotente
- [ ] T018 Crear `src/G1.Identity.Mock/Endpoints/ErrorMapping.cs` para `application/problem+json` (RFC 9457) con `code` y `traceId`, separado de los errores OAuth estándar

**Checkpoint**: base lista; las seis historias pueden comenzar. ADR-0002 debe estar ratificado antes de ejecutar estas tareas.

---

## Phase 3: User Story 1 — Contrato publicado y navegable (Priority: P1) 🎯 MVP

**Goal**: que G2, G3 y G4 puedan generar clientes y ver formas exactas de respuesta y error desde `/swagger`.

**Independent Test**: arrancar el mock, abrir `/swagger`, validar `openapi.v1.yaml` y ejecutar una operación desde la UI sin módulos consumidores.

### Tests for User Story 1

- [ ] T019 [P] [US1] Crear `tests/G1.Identity.ContractTests/OpenApiDocumentTests.cs`: validar `specs/001-g1-identidad-contrato-mock/contracts/openapi.v1.yaml` como OpenAPI 3.1 y comprobar que `/openapi/v1.json` expone `info.version`, seguridad, esquemas y ejemplos de `/api/v1/identity/me`
- [ ] T020 [P] [US1] Crear `tests/G1.Identity.ContractTests/SwaggerAvailabilityTests.cs`: `/swagger` responde en Development/Testing y `404` en otros entornos; añadir `scripts/check-openapi-breaking.sh` con `oasdiff` y `@redocly/cli`

### Implementation for User Story 1

- [ ] T021 [P] [US1] Crear los transformadores en `src/G1.Identity.Mock/OpenApi/` para security schemes `cookieAuth`/`bearerAuth`, etiquetas, ejemplos, `Cache-Control`, `ETag` y versión 3.1 (Skill: `dotnet-api-architecture`)
- [ ] T022 [US1] Implementar `src/G1.Identity.Mock/Endpoints/DocsEndpoints.cs` con `/swagger` y `/openapi/v1.json` restringidos por entorno (Skill: `dotnet-api-architecture`)
- [ ] T023 [US1] Sincronizar `specs/001-g1-identidad-contrato-mock/contracts/openapi.v1.yaml` con el documento runtime y crear `scripts/export-openapi.sh` para regenerarlo
- [ ] T024 [US1] Ejecutar la validación de US1 con `specs/001-g1-identidad-contrato-mock/quickstart.md` secciones 2 y 3

**Checkpoint**: US1 funciona de forma aislada; contrato navegable y verificable en CI.

---

## Phase 4: User Story 2 — Sesión simulada mediante Authorization Code + PKCE (Priority: P1)

**Goal**: obtener tokens del mock con el mismo flujo OIDC que la fase B; sin atajos de autorización.

**Independent Test**: completar `/connect/authorize` → `/connect/token` → `/me` con un cliente de prueba y verificar token, JWKS, `nonce` y refresh rotativo.

### Tests for User Story 2

- [ ] T025 [P] [US2] Crear `tests/G1.Identity.ContractTests/PkceFlowTests.cs`: éxito con S256, rechazo de `plain`, código reutilizado, `code_verifier` incorrecto, `state`/`nonce` alterados y `redirect_uri` no registrada
- [ ] T026 [P] [US2] Crear `tests/G1.Identity.ContractTests/TokenValidationTests.cs`: firma con JWKS, `iss`, `aud=identity-api`, `nonce`, `exp` y rotación de refresh con invalidación del anterior

### Implementation for User Story 2

- [ ] T027 [P] [US2] Registrar el cliente `cli_mock_spa` en `src/G1.Identity.Mock/Fixtures/MockClients.cs` con `redirect_uri` exacta, scopes de D-004 y PKCE obligatorio
- [ ] T028 [US2] Implementar `src/G1.Identity.Mock/Endpoints/OidcEndpoints.cs` delegando descubrimiento, JWKS, autorización, token y userinfo a OpenIddict; sin emisión manual de tokens (Skill: `dotnet-api-architecture`)
- [ ] T029 [P] [US2] Crear `src/G1.Identity.Mock.Ui/src/components/PersonaSelector.tsx`, `src/G1.Identity.Mock.Ui/src/App.tsx` y `src/G1.Identity.Mock.Ui/src/styles/tokens.css` con selector accesible, foco visible y tokens locales concretos WCAG 2.1 AA; nunca almacenar tokens (Instrucciones: `coding-style-react-typescript.instructions.md`, `coding-style-css-theming.instructions.md`, `coding-style-frontend-design-system.instructions.md`)
- [ ] T030 [US2] Servir el build de `src/G1.Identity.Mock.Ui/dist` desde `src/G1.Identity.Mock/wwwroot/` y la ruta de selección en `src/G1.Identity.Mock/Endpoints/MockPersonaEndpoints.cs`
- [ ] T031 [US2] Añadir los claims `sid`, `roles`, `permissions`, `email_verified` y `phone_verified` en `src/G1.Identity.Mock/Auth/ClaimsTransformation.cs` como fotografía al emitir; la firma la hace OpenIddict (Skill: `dotnet-api-architecture`)
- [ ] T032 [US2] Ejecutar la validación de US2 con `specs/001-g1-identidad-contrato-mock/quickstart.md` secciones 4, 5 y 6

**Checkpoint**: US2 funciona de forma aislada; ningún token se emite fuera de `/connect/token`.

---

## Phase 5: User Story 3 — Autorización por rol y permiso, con errores explícitos (Priority: P1)

**Goal**: distinguir sesión ausente, inválida, permiso insuficiente y recurso no visible sin filtrar datos.

**Independent Test**: invocar `/me`, resúmenes y catálogos con las personas del mock y tokens inválidos, comprobando 401/403/404/400/200.

### Tests for User Story 3

- [ ] T033 [P] [US3] Crear `tests/G1.Identity.ContractTests/AuthorizationTests.cs` con un caso de éxito y uno de error por operación, incluidos `identity.session.required`, `identity.session.invalid`, `identity.permission.denied`, `identity.user.not_found` y `identity.request.invalid`
- [ ] T034 [P] [US3] Crear `tests/G1.Identity.ContractTests/InvalidTokenTests.cs` con token caducado, revocado, firma inválida, `alg=none`, emisor incorrecto y audiencia incorrecta

### Implementation for User Story 3

- [ ] T035 [P] [US3] Implementar `src/G1.Identity.Mock/Auth/PermissionPolicies.cs` con denegación por defecto, mapeo a `ProblemDetails` y trato de códigos desconocidos como no concedidos (Skill: `dotnet-api-architecture`)
- [ ] T036 [P] [US3] Implementar `GET /api/v1/identity/me` en `src/G1.Identity.Mock/Endpoints/IdentityEndpoints.cs` con `Cache-Control: no-store`, `X-Correlation-ID` y el esquema `MeResponse`
- [ ] T037 [P] [US3] Implementar `/users/{userId}/summary` y `/users/lookup` en `src/G1.Identity.Mock/Endpoints/IdentityEndpoints.cs`: «Lote de `lookup` de 1 a 100 identificadores, deduplicado» y «`Cache-Control: no-store` en identidad y sesiones»; sin correo, teléfono ni datos del tutor
- [ ] T038 [US3] Implementar `/catalog/roles` y `/catalog/permissions` en `src/G1.Identity.Mock/Endpoints/CatalogEndpoints.cs` con `ETag`, `catalogVersion` y `provisional: true`
- [ ] T039 [US3] Ejecutar la validación de US3 con `specs/001-g1-identidad-contrato-mock/quickstart.md` secciones 5 y 8

**Checkpoint**: US3 funciona de forma aislada; la matriz provisional D-002 se aplica sin interpretar ámbitos D-003.

---

## Phase 6: User Story 4 — Logout y revocación de sesiones (Priority: P2)

**Goal**: cerrar la sesión actual y revocar otras sin afectar a sesiones que deben permanecer activas.

**Independent Test**: abrir dos sesiones de la misma persona, revocar una y comprobar que la otra sigue válida; verificar que el refresh revocado no se canjea.

### Tests for User Story 4

- [ ] T040 [P] [US4] Crear `tests/G1.Identity.ContractTests/SessionTests.cs` con logout, `DELETE /sessions/{sessionId}`, `revoke-all` con `keepCurrentSession=true`, sesión ajena (`404`, no `403`) e idempotencia

### Implementation for User Story 4

- [ ] T041 [P] [US4] Definir `src/G1.Identity.Mock/Abstractions/ISessionService.cs` e implementar `src/G1.Identity.Mock/Services/SessionService.cs`: `sid`, `lastSeenAt`, transiciones `active`/`revoked`/`expired`, vínculo `SessionToken` y revocación de refresh (Instrucciones: `coding-style-csharp-service-implementation.instructions.md`, `coding-style-csharp-xml-documentation.instructions.md`)
- [ ] T042 [US4] Implementar `GET /api/v1/identity/sessions`, `DELETE /api/v1/identity/sessions/{sessionId}` y `POST /api/v1/identity/sessions/revoke-all` en `src/G1.Identity.Mock/Endpoints/SessionEndpoints.cs`
- [ ] T043 [US4] Implementar `/connect/logout` GET y POST en `src/G1.Identity.Mock/Endpoints/OidcEndpoints.cs` con `post_logout_redirect_uri` de coincidencia exacta y cierre de sesión OpenIddict
- [ ] T044 [US4] Implementar `POST /api/v1/identity/logout` en `src/G1.Identity.Mock/Endpoints/SessionEndpoints.cs`; documentar que el access token sigue válido hasta `exp` fuera de G1 (D-005)
- [ ] T045 [US4] Registrar `logout`, `session_revoked` y `token_revoked` en `src/G1.Identity.Mock/Services/AuditService.cs` sin tokens ni datos personales
- [ ] T046 [US4] Ejecutar la validación de US4 con `specs/001-g1-identidad-contrato-mock/quickstart.md` sección 7

**Checkpoint**: US4 funciona de forma aislada; revocación inmediata en G1 y refresh, TTL de 5 min para JWT.

---

## Phase 7: User Story 5 — Personas y casos límite reproducibles (Priority: P2)

**Goal**: fixtures deterministas, públicos y reiniciables para que todos los equipos prueben lo mismo.

**Independent Test**: ejecutar `POST /api/v1/identity/mock/reset` dos veces y comprobar que personas, roles y permisos vuelven al estado base.

### Tests for User Story 5

- [ ] T047 [P] [US5] Crear `tests/G1.Identity.ContractTests/MockFixturesTests.cs`: 18 personas, `fixtureVersion` estable, reset idempotente y `404` fuera de Development/Testing

### Implementation for User Story 5

- [ ] T048 [P] [US5] Implementar `GET /api/v1/identity/mock/personas` y `POST /api/v1/identity/mock/reset` en `src/G1.Identity.Mock/Endpoints/MockPersonaEndpoints.cs` con paginación `page`/`pageSize` máximo 100
- [ ] T049 [US5] Añadir los casos `token.expired`, `token.revoked`, `token.wrong-audience` y `token.invalid-signature` en `src/G1.Identity.Mock/Fixtures/EdgeCaseTokens.cs`; ninguno emite tokens fuera de `/connect/token`
- [ ] T050 [US5] Aplicar en `src/G1.Identity.Mock/Auth/MockPersonaAuthentication.cs` el error genérico para personas inactivas/bloqueadas, permisos limitados para correo sin verificar y resumen minimizado para menores (D-017)
- [ ] T051 [US5] Ejecutar la validación de US5 con `specs/001-g1-identidad-contrato-mock/quickstart.md` sección 9

**Checkpoint**: US5 funciona de forma aislada; los datos son ficticios y volátiles.

---

## Phase 8: User Story 6 — Pruebas de contrato y E2E (Priority: P3)

**Goal**: detectar divergencias del mock respecto al contrato antes de que afecten a G2, G3 o G4.

**Independent Test**: ejecutar las suites de contrato, E2E, accesibilidad y compatibilidad sin interfaces de módulos consumidores.

### Tests for User Story 6

- [ ] T052 [P] [US6] Completar la matriz de cobertura en `tests/G1.Identity.ContractTests/README.md`: las 19 operaciones de `contracts/openapi.v1.yaml` con al menos una prueba de éxito y una de error validadas contra el esquema
- [ ] T053 [P] [US6] Crear `tests/G1.Identity.E2E/AuthorizationFlowSpecs.cs` con Playwright 1.64.1: autorización, PKCE, `/me`, logout y revocación en Chromium
- [ ] T054 [P] [US6] Crear `tests/G1.Identity.E2E/AccessibilitySpecs.cs` con axe-core sobre `PersonaSelector.tsx` y las páginas de error; navegación solo con teclado
- [ ] T055 [P] [US6] Crear `tests/G1.Identity.CompatibilityTests/ProviderSmokeTests.cs` con SQLite para detectar diferencias de proveedor (mitigación obligatoria de ADR-0002)
- [ ] T056 [US6] Integrar en `.github/workflows/ci-g1-identity.yml` los pasos de `oasdiff breaking`, `@redocly/cli lint`, escaneo de secretos y navegadores adicionales en ejecución programada
- [ ] T057 [US6] Ejecutar la validación de US6 con `specs/001-g1-identidad-contrato-mock/quickstart.md` secciones 10 y 11

**Checkpoint**: US6 funciona de forma aislada; CI falla ante rupturas no versionadas.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: gobernanza, seguridad, calidad y documentación final de la fase A.

- [ ] T058 [P] Completar `docs/g1/guia-consumo-identidad.md` con generación de clientes, PKCE, `aud` provisional D-016, D-003 y D-017, y aviso de revocación de JWT
- [ ] T059 [P] Registrar en `docs/g1/herramientas-y-skills.md` la excepción Swagger UI frente a la preferencia Scalar de `.github/instructions/coding-style-csharp-project-structure.instructions.md` y la excepción InMemory de ADR-0002, sin modificar `.specify/memory/constitution.md`
- [ ] T060 [P] Revisar cabeceras, CORS, antiforgery, rate limiting y logs en `src/G1.Identity.Mock/BuilderExtensions.cs` y `AppExtensions.cs` (Skill: `code-review` en `.github/skills/code-review/SKILL.md`)
- [ ] T061 [P] Medir `/me` con `tests/G1.Identity.ContractTests/PerformanceTests.cs` y verificar p95 inferior a 200 ms en local (SC-007)
- [ ] T062 [P] Verificar castellano en textos, inglés en identificadores y recursos `.resx` en `src/G1.Identity.Mock/Resources/` y `src/G1.Identity.Mock.Ui/src/`
- [ ] T063 Crear `docs/g1/CHANGELOG-contrato.md` con `1.0.0`, estado provisional y política de cambios incompatibles de `contracts/README.md`
- [ ] T064 [P] Ejecutar revisión final del diff con `code-review` en `.github/skills/code-review/SKILL.md`, sin generar cambios de aplicación nuevos
- [ ] T065 Ejecutar `specs/001-g1-identidad-contrato-mock/quickstart.md` completo y registrar resultados en `docs/g1/validacion-fase-a.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sin dependencias; T002 bloquea T003-T005; T008 es documental y paralela.
- **Foundational (Phase 2)**: depende de Setup; T010 bloquea T011/T012/T017; T014 bloquea todos los endpoints; T015 y T016 bloquean US2; T009 bloquea la configuración de TTL.
- **User Stories (Phase 3-8)**: dependen de Foundational. US1 y US2 son P1 y pueden avanzar en paralelo; US3 depende de los claims de US2 para `/me` con token; US4 depende de la sesión creada en US2; US5 depende de fixtures y de la autenticación de US2; US6 depende de que las historias anteriores existan para cubrir todas las operaciones.
- **Polish (Phase 9)**: depende de US1-US6.

### User Story Dependencies

- **US1 (P1)**: después de Foundational; sin dependencias de otras historias. MVP.
- **US2 (P1)**: después de Foundational; sin dependencias de US1.
- **US3 (P1)**: necesita T031 de US2 para `/me` con Bearer; el resto es independiente.
- **US4 (P2)**: necesita T028/T031 de US2 para crear sesiones y `sid`.
- **US5 (P2)**: necesita T016/T017 y T028 para los casos de token.
- **US6 (P3)**: necesita las implementaciones de US1-US5 para cubrir las 19 operaciones.

### Within Each User Story

1. Pruebas primero; deben fallar antes de la implementación.
2. Contratos/esquemas y entidades antes que servicios.
3. Servicios antes que endpoints.
4. Implementación antes de la verificación con `quickstart.md`.

### Parallel Opportunities

- Setup: T003, T004, T005, T007 y T008 son paralelos tras T001/T002.
- Foundational: T011, T012 y T013 son paralelos tras T010; T015 y T016 son paralelos.
- US1: T019 y T020 en paralelo; T021 en paralelo con ellos.
- US2: T025, T026 y T027 en paralelo; T029 en paralelo con T028.
- US3: T033, T034, T035, T036 y T037 en paralelo tras T031.
- US4: T040 y T041 en paralelo.
- US5: T047 y T048 en paralelo.
- US6: T052, T053, T054 y T055 en paralelo.
- Polish: T058 a T062 y T064 son paralelos.

## Parallel Example: User Story 2

```bash
# Pruebas en paralelo
Task: "Crear tests/G1.Identity.ContractTests/PkceFlowTests.cs"
Task: "Crear tests/G1.Identity.ContractTests/TokenValidationTests.cs"
Task: "Registrar cli_mock_spa en src/G1.Identity.Mock/Fixtures/MockClients.cs"

# Implementación en paralelo
Task: "Implementar OidcEndpoints.cs delegando en OpenIddict"
Task: "Crear PersonaSelector.tsx, App.tsx y tokens.css"
```

---

## Implementation Strategy

### MVP First (US1 + US2)

1. Completar Phase 1 y Phase 2. **Parada obligatoria**: ratificar ADR-0002.
2. Completar Phase 3 (US1): contrato navegable y CI de rupturas. Validar de forma aislada.
3. Completar Phase 4 (US2): flujo PKCE y `/me`. Validar de forma aislada.
4. **STOP and VALIDATE**: ejecutar `quickstart.md` secciones 2 a 6. Este es el primer entregable útil para G2, G3 y G4.

### Incremental Delivery

1. Setup + Foundational → base lista.
2. US1 → contrato y Swagger.
3. US2 → sesión fiel con OpenIddict.
4. US3 → errores y permisos explícitos.
5. US4 → logout y revocación.
6. US5 → fixtures deterministas.
7. US6 → automatización y detección de divergencias.
8. Polish → gobernanza y documentación.

### Parallel Team Strategy

- Desarrollador A: US1 y US6 (contrato y CI).
- Desarrollador B: US2 y US3 (protocolo e identidad).
- Desarrollador C: US4 y US5 (sesiones y fixtures).
- Todas las historias se integran sobre el contrato inmutable `contracts/openapi.v1.yaml`.

---

## Notes

- `[P]` = archivos distintos y sin dependencias; verificar antes de marcar.
- `[Story]` = trazabilidad con `spec.md`; no se añade en Setup, Foundational ni Polish.
- Cada historia tiene prueba independiente y checkpoint; no se omiten.
- No se genera código de aplicación en este comando: solo este `tasks.md`.
- No se ejecuta implementación sin ratificación de ADR-0002 y aprobación explícita del listado.

## Bloqueos pendientes (no resueltos por estas tareas)

| Bloqueo | Estado | Impacto |
| --- | --- | --- |
| ADR-0002 (InMemory frente a migraciones de la constitución) | Pendiente de ratificación | Bloquea Phase 2 y, por tanto, todas las historias |
| ADR-0001 (servidor OAuth real con Identity + OpenIddict) | Propuesto | Bloquea la fase B; no bloquea el mock de fase A |
| D-001 (BFF/SPA), D-012 (consentimiento), D-016 (audiencias) | Abiertas | No se resuelven en código; el contrato las declara provisionales |
| D-004 (valores de producción) y D-005 (revocación de JWT en producción) | Parciales | Solo se aplican los valores de desarrollo aprobados |
| D-003 (verificación con G2) y D-017 (contrato de contacto G3-G4) | Resueltas para fase A; integración en fase C | No se añaden endpoints de ámbitos ni contacto de terceros |
| Skill `myadmon-theme` referenciada por instrucciones de frontend | Ausente en `.github/skills/` | La T029 define tokens locales concretos; se necesita la skill para alineación visual definitiva |
| OpenCode no descubre `.github/skills/` | Verificado | Las skills se asocian por ruta y deben leerse explícitamente con OpenCode |
