---

description: "Task list for feature implementation — fase A, reparto G1 DEV1-DEV5"
---

# Tasks: Contrato de identidad y mock para G2, G3 y G4 — fase A

**Input**: Design documents from `/specs/001-g1-identidad-contrato-mock/`

**Prerequisites**: `plan.md` (required), `spec.md` (required for user stories), `research.md`, `data-model.md`, `contracts/`, `.specify/memory/constitution.md`

**Tests**: se incluyen porque la feature los exige explícitamente (US6, FR-046 a FR-048 y SC-001 a SC-012).

**Organization**: tareas agrupadas por user story para implementación y prueba independientes. Solo fase A: contrato OpenAPI 3.1 + mock funcional + Swagger UI + pruebas. No se implementa Identity real para usuarios, registro ni servidor OAuth de producción. El reparto previsto de las fases B y C se documenta al final, **fuera de este listado ejecutable**.

## Convenciones y herramientas

- **Skills**: las skills originales permanecen en `.github/skills/` y siguen siendo descubiertas por GitHub Copilot. OpenCode las descubre desde la raíz del repositorio gracias a `opencode.jsonc` (`"skills": ["./.github/skills"]`), verificado cargando `dotnet-api-architecture`. No se copian ni se modifican las skills; la compatibilidad con Copilot no se altera. La verificación operativa se documenta en T008 y T059.
- **Uso de skills**: el agente carga la skill relevante con su herramienta de skills cuando el nombre aparece en la tarea. Las rutas `.github/skills/<nombre>/SKILL.md` se indican solo como referencia para revisiones humanas y para otros agentes.
- **Instrucciones obligatorias**: `.github/instructions/` y `.github/copilot-instructions.md`. Especialmente `coding-style-csharp.instructions.md`, `coding-style-csharp-program.instructions.md`, `coding-style-csharp-builder-extensions.instructions.md`, `coding-style-csharp-app-extensions.instructions.md`, `coding-style-csharp-settings-classes.instructions.md`, `coding-style-csharp-localization.instructions.md`, `coding-style-csharp-resource-files.instructions.md`, `coding-style-csharp-constants.instructions.md`, `coding-style-react-typescript.instructions.md`, `coding-style-markdown.instructions.md`, `dotnet-environment-configuration.instructions.md`, `wow-spec-driven-development.instructions.md`, `wow-no-dummy-no-placholder-code.instructions.md`, `wow-conventional-commits.instructions.md` y `wow-follow-up-question.instructions.md`.
- **Idioma**: textos para personas en castellano; identificadores de código en inglés. Sin valores de relleno ni marcadores de posición.
- **Estilo**: `.editorconfig` manda (CRLF, 4 espacios; 2 espacios en JSON/csproj/props).
- **Excepción documentada**: `.github/instructions/coding-style-csharp-project-structure.instructions.md` prefiere Scalar para proyectos nuevos, pero la constitución y el mandato del usuario exigen Swagger UI. Se aplica Swagger UI y se registra la excepción en T059; no se modifica la instrucción.
- **Skill referenciada ausente**: `coding-style-css-theming.instructions.md` y `coding-style-frontend-design-system.instructions.md` citan `myadmon-theme`, que no existe en `.github/skills/`. La T029 define tokens locales concretos y accesibles hasta que se aporte la skill.
- **Excepción ratificada**: ADR-0002 fue aprobado por equipo G1 el 2026-11-25 para desarrollo, demostración y pruebas. La excepción no cubre datos reales ni producción: la persistencia definitiva, el proveedor relacional y la prueba de CI con proveedor relacional (T055) siguen pendientes. Las tareas solo se marcan como completadas cuando están implementadas y verificadas.

## Formato: `[ID] [P?] [Prioridad] [DEVn] [Story?] Descripción`

- **[P]**: la tarea usa archivos distintos y no depende de tareas incompletas; verificar antes de marcar.
- **Prioridad**: `P0` (bloqueante), `P1` (MVP), `P2` (calidad y cierre), `P3` (automatización avanzada). No se confunde con `[P]`.
- **[DEVn]**: responsable único de la tarea, de sus pruebas y de su documentación.
- **[Story]**: obligatorio en fases de user story; ausente en Setup, Foundational y Polish.
- **Rama**: cada responsable trabaja en su rama DEV y abre PR contra la rama de integración `001-g1-identidad-contrato-mock`. No se hace push directo a esa rama ni a `main`.
- Toda tarea incluye una ruta de archivo exacta y un entregable verificable.

| Responsable | Rama | Foco | Tareas |
| --- | --- | --- | --- |
| DEV1 | `dev1/infra-contract` | Arquitectura, scaffolding, contrato, Swagger, CI e integración | T001-T003, T005-T009, T013-T014, T019-T024, T030, T056, T059, T064-T065 |
| DEV2 | `dev2/oauth-oidc` | OAuth/OIDC, OpenIddict, emisión y validación de tokens | T015, T025-T028, T031-T032, T043, T049, T067 |
| DEV3 | `dev3/users-persistence` | Usuarios, selectors, fixtures, persistencia y minimización | T010-T012, T016-T017, T036-T037, T047-T048, T050-T051, T055, T071 |
| DEV4 | `dev4/authz-audit` | Autorización, políticas, catálogos, sesiones y auditoría | T018, T033-T035, T038-T042, T044-T046, T052, T060, T066, T068-T069 |
| DEV5 | `dev5/ui-e2e` | React, accesibilidad, E2E, guía de consumo y compatibilidad | T004, T029, T053-T054, T057-T058, T061-T063, T070, T072 |

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: solución, proyectos, versiones, entornos y CI antes de cualquier historia. Responsable principal: DEV1; DEV5 prepara el frontend.

- [x] T001 [P0] [DEV1] Crear la solución y la estructura de carpetas de `plan.md` en `G1.Identity.slnx`, `src/`, `tests/` y `docs/g1/` (Skill: `dotnet-api-architecture` en `.github/skills/dotnet-api-architecture/SKILL.md`). **Rama**: `dev1/infra-contract`. **Entregable**: solución compilable vacía.
- [x] T002 [P0] [DEV1] Fijar `global.json` con SDK `10.0.404` y `Directory.Packages.props` con OpenIddict 7.8.0, EF Core InMemory 10.0.15, `Microsoft.AspNetCore.OpenApi` 10.0.15, `Swashbuckle.AspNetCore.SwaggerUI` 10.3.1, `xunit.v3` 4.0.3, `Microsoft.Playwright` 1.64.1 y `JsonSchema.Net`; usar la CLI `dotnet` para añadir paquetes (Skill: `nuget-package-manager`). **Rama**: `dev1/infra-contract`. **Entregable**: versiones centralizadas sin avisos NU190x.
- [x] T003 [P] [P0] [DEV1] Crear `src/G1.Identity.Mock/G1.Identity.Mock.csproj` con `net10.0`, C# 14, nullable activado y sin controladores MVC (Skill: `dotnet-api-architecture`). **Rama**: `dev1/infra-contract`. **Entregable**: host Minimal API que arranca.
- [ ] T004 [P] [P1] [DEV5] Crear `src/G1.Identity.Mock.Ui/package.json`, `package-lock.json`, `tsconfig.json` y `vite.config.ts` con React 19, TypeScript estricto y build propio; sin tokens en el navegador (Instrucciones: `coding-style-react-typescript.instructions.md`, `coding-style-css-theming.instructions.md`). **Rama**: `dev5/ui-e2e`. **Entregable**: `npm ci && npm run build` en verde.
- [x] T005 [P] [P0] [DEV1] Crear `tests/G1.Identity.ContractTests/G1.Identity.ContractTests.csproj`, `tests/G1.Identity.E2E/G1.Identity.E2E.csproj` y `tests/G1.Identity.CompatibilityTests/G1.Identity.CompatibilityTests.csproj` con las versiones centralizadas de T002. **Rama**: `dev1/infra-contract`. **Entregable**: tres proyectos de prueba descubribles por `dotnet test`.
- [x] T006 [P0] [DEV1] Crear `.github/workflows/ci-g1-identity.yml` con restore/build/test, `npx @redocly/cli lint`, `oasdiff breaking`, escaneo de secretos e instalación de navegadores Playwright; sin publicar artefactos del mock. **Rama**: `dev1/infra-contract`. **Entregable**: pipeline rojo si falta contrato o hay secretos; SC-006 y SC-009.
- [x] T007 [P] [P0] [DEV1] Crear `src/G1.Identity.Mock/Properties/launchSettings.json` con HTTPS `7180` y HTTP `5180` en `Development`, y `src/G1.Identity.Mock/appsettings.json` / `appsettings.Development.json` con issuer, cliente, scopes y TTL de D-004; sin secretos (Instrucción: `dotnet-environment-configuration.instructions.md`). **Rama**: `dev1/infra-contract`. **Entregable**: arranque reproducible sin secretos.
- [x] T008 [P] [P0] [DEV1] Verificar y documentar en `docs/g1/herramientas-y-skills.md` la integración de skills: `opencode.jsonc` registra `.github/skills/`, `dotnet-api-architecture` carga correctamente en OpenCode y GitHub Copilot conserva `.github/skills/` y `.github/instructions/` sin cambios; incluir la ausencia de `myadmon-theme` (Instrucción: `coding-style-markdown.instructions.md`). **Rama**: `dev1/infra-contract`. **Entregable**: evidencia de carga y procedimiento de verificación.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: infraestructura que BLOCKA todas las historias. No se inicia ninguna US hasta completarla.

- [x] T009 [P0] [DEV1] Crear `src/G1.Identity.Mock/Settings/MockOptions.cs`, `OpenIdConnectOptions.cs`, `SessionOptions.cs` y `CorsOptions.cs` con Options pattern, validación fail-fast y los valores de D-004: «código 5 min, access 5 min, ID token 5 min, refresh rotativo 8 h, inactividad 30 min, absoluta 12 h, tolerancia 2 min» (Instrucciones: `coding-style-csharp-settings-classes.instructions.md`, `coding-style-csharp-localization.instructions.md`). **Rama**: `dev1/infra-contract`. **Entregable**: configuración enlazada y validada.
- [x] T010 [P0] [DEV3] Crear `src/G1.Identity.Mock/Persistence/G1IdentityDbContext.cs` con EF Core InMemory, `EnsureCreated()`, `DisableBulkOperations()` y sin migraciones; advertencia de volatilidad en logs (Skill: `dotnet-api-architecture`; ADR-0002). **Rama**: `dev3/users-persistence`. **Entregable**: revocación OpenIddict sin `ExecuteUpdate`.
- [x] T011 [P] [P0] [DEV3] Crear `src/G1.Identity.Mock/Persistence/Entities/MockPersona.cs`, `Role.cs`, `Permission.cs`, `RolePermission.cs` y `RoleAssignment.cs` cumpliendo literalmente: «`key` y `userId` son únicos.», «Un `userId` no cambia entre reinicios ni entre resets.», «`email` y `phone` nunca se exponen en el resumen de otro usuario.», «Una persona `inactive` o `locked` no puede completar la autorización; el error es genérico.», «Una asignación revocada no contribuye a `roles` ni a `permissions`.» y «`scopeRef` se transporta sin validación de negocio; el módulo propietario lo resuelve.». **Rama**: `dev3/users-persistence`. **Entregable**: entidades con invariantes documentados.
- [ ] T012 [P] [P0] [DEV3] Crear `src/G1.Identity.Mock/Persistence/Entities/Session.cs`, `SessionToken.cs`, `AuditEvent.cs` y `CatalogSnapshot.cs` cumpliendo literalmente: «Una sesión `revoked` o `expired` no permite `/me`, ni refresh, ni operaciones de identidad.», «`expiresAt` nunca supera `absoluteExpiresAt`.», «El listado solo devuelve sesiones del usuario autenticado y nunca IP ni secretos.», «La sesión actual se marca con `current: true` en la respuesta.» y «No se registran tokens, contraseñas, correos completos ni datos de salud.». **Rama**: `dev3/users-persistence`. **Entregable**: entidades de sesión y auditoría.
- [x] T013 [P] [P0] [DEV1] Crear `src/G1.Identity.Mock/Constants.cs` y `src/G1.Identity.Mock/Resources/IdentityMessages.resx` con los códigos de error estables de `contracts/README.md` y textos en castellano (Instrucciones: `coding-style-csharp-constants.instructions.md`, `coding-style-csharp-resource-files.instructions.md`). **Rama**: `dev1/infra-contract`. **Entregable**: sin cadenas mágicas de error en endpoints.
- [x] T014 [P0] [DEV1] Crear `src/G1.Identity.Mock/BuilderExtensions.cs`, `AppExtensions.cs` y `Program.cs` con pipeline Minimal API: correlación, CORS sin comodín con credenciales, antiforgery, rate limiting, CSP en Swagger UI, `X-Content-Type-Options: nosniff`, `X-Correlation-ID` y manejador de excepciones (Skill: `dotnet-api-architecture`; Instrucciones: `coding-style-csharp-program.instructions.md`, `coding-style-csharp-app-extensions.instructions.md`, `coding-style-csharp-builder-extensions.instructions.md`). **Rama**: `dev1/infra-contract`. **Entregable**: FR-041 a FR-045 implementados en el pipeline.
- [x] T015 [P0] [DEV2] Configurar OpenIddict 7.8.0 en `src/G1.Identity.Mock/Auth/OpenIddictConfiguration.cs`: solo `authorization_code` + `refresh_token`, PKCE S256 obligatorio, scopes `openid profile email offline_access identity.api`, clave RSA efímera, sin introspección, sin implicit, password ni `client_credentials` (Skills: `dotnet-api-architecture` y `microsoft-docs-and-code-consultant`, esta última solo si el MCP de Microsoft Learn está disponible). **Rama**: `dev2/oauth-oidc`. **Entregable**: descubrimiento y JWKS sin grants prohibidos.
- [x] T016 [P] [P0] [DEV3] Configurar la autenticación de persona del mock en `src/G1.Identity.Mock/Auth/MockPersonaAuthentication.cs` con cookie `__Host-g1.session` HttpOnly/Secure/SameSite=Lax y antiforgery en escrituras; el parámetro `mock_persona` solo se acepta en `Development`/`Testing` y no emite tokens por sí mismo (Skill: `dotnet-api-architecture`). **Rama**: `dev3/users-persistence`. **Entregable**: cookie de desarrollo sin emisión directa de JWT.
- [ ] T017 [P0] [DEV3] Crear `src/G1.Identity.Mock/Fixtures/SeedData.cs` con **15 personas ficticias**, 10 roles, 3 permisos de G1, la matriz deny-by-default exacta de `data-model.md` y los 4 escenarios `token.*` como casos de token, no como personas; seeding idempotente. **Rama**: `dev3/users-persistence`. **Entregable**: `fixtureVersion` estable y reset reproducible.
- [ ] T018 [P] [P0] [DEV4] Crear `src/G1.Identity.Mock/Endpoints/ErrorMapping.cs` para `application/problem+json` (RFC 9457) con `code` y `traceId`, separado de los errores OAuth estándar. **Rama**: `dev4/authz-audit`. **Entregable**: todos los errores de API con esquema documentado; SC-004.

**Checkpoint**: base lista; las seis historias pueden comenzar cuando sus propias dependencias estén listas. **ADR-0002 quedó ratificado por equipo G1 el 2026-11-25; la excepción se limita al alcance académico.**

---

## Phase 3: User Story 1 — Contrato publicado y navegable (Priority: P1) 🎯 MVP

**Goal**: que G2, G3 y G4 puedan generar clientes y ver formas exactas de respuesta y error desde `/swagger`.

**Independent Test**: arrancar el mock, abrir `/swagger`, validar `openapi.v1.yaml` y ejecutar una operación desde la UI sin módulos consumidores.

### Tests for User Story 1

- [x] T019 [P] [P1] [DEV1] [US1] Crear `tests/G1.Identity.ContractTests/OpenApiDocumentTests.cs`: validar `contracts/openapi.v1.yaml` como OpenAPI 3.1, comprobar `info.version`, seguridad, esquemas, `x-reviewed-at`, `x-persistence-exception` y que las 19 operaciones tienen ejemplo de éxito (`examples` o `x-success-example`) y error. **Rama**: `dev1/infra-contract`. **Entregable**: FR-001 a FR-006 y FR-005 con respuestas vacías documentadas.
- [x] T020 [P] [P1] [DEV1] [US1] Crear `tests/G1.Identity.ContractTests/SwaggerAvailabilityTests.cs`: `/swagger` responde en Development/Testing y `404` en otros entornos; añadir `scripts/check-openapi-breaking.sh` con `oasdiff` y `@redocly/cli`. **Rama**: `dev1/infra-contract`. **Entregable**: SC-006 verificado en local y CI.

### Implementation for User Story 1

- [x] T021 [P] [P1] [DEV1] [US1] Crear los transformadores en `src/G1.Identity.Mock/OpenApi/` para security schemes `cookieAuth`/`bearerAuth`, etiquetas, ejemplos, `Cache-Control`, `ETag` y versión 3.1 (Skill: `dotnet-api-architecture`). **Rama**: `dev1/infra-contract`. **Entregable**: documento runtime equivalente al normativo.
- [x] T022 [P1] [DEV1] [US1] Implementar `src/G1.Identity.Mock/Endpoints/DocsEndpoints.cs` con `/swagger` y `/openapi/v1.json` restringidos por entorno (Skill: `dotnet-api-architecture`). **Rama**: `dev1/infra-contract`. **Entregable**: UI navegable solo en desarrollo/pruebas.
- [x] T023 [P1] [DEV1] [US1] Sincronizar `contracts/openapi.v1.yaml` con el documento runtime y crear `scripts/export-openapi.sh` para regenerarlo, conservando `x-reviewed-at`, `x-persistence-exception` y los `x-success-example`. **Rama**: `dev1/infra-contract`. **Entregable**: diff vacío entre contrato y runtime.
- [x] T024 [P1] [DEV1] [US1] Ejecutar la validación de US1 con `quickstart.md` secciones 2 y 3 y registrar el resultado en `docs/g1/validacion-fase-a.md`. **Rama**: `dev1/infra-contract`. **Entregable**: SC-001 y SC-006 comprobados.

**Checkpoint**: US1 funciona de forma aislada; contrato navegable y verificable en CI.

---

## Phase 4: User Story 2 — Sesión simulada mediante Authorization Code + PKCE (Priority: P1)

**Goal**: obtener tokens del mock con el mismo flujo OIDC que la fase B; sin atajos de autorización.

**Independent Test**: completar `/connect/authorize` → `/connect/token` → `/me` con un cliente de prueba y verificar token, JWKS, `nonce` y refresh rotativo.

### Tests for User Story 2

- [ ] T025 [P] [P1] [DEV2] [US2] Crear `tests/G1.Identity.ContractTests/PkceFlowTests.cs`: éxito con S256, rechazo de `plain`, código reutilizado, `code_verifier` incorrecto, `state`/`nonce` alterados y `redirect_uri` no registrada. **Rama**: `dev2/oauth-oidc`. **Entregable**: FR-030 verificado.
- [ ] T026 [P] [P1] [DEV2] [US2] Crear `tests/G1.Identity.ContractTests/TokenValidationTests.cs`: firma con JWKS, `iss`, `aud=identity-api` en access token, `nonce` en ID token, `exp`, `jti`, `sid` y rotación de refresh con invalidación del anterior. **Rama**: `dev2/oauth-oidc`. **Entregable**: FR-034, FR-055 y SC-010 (TTL por separado).
- [x] T027 [P] [P1] [DEV2] [US2] Registrar el cliente `cli_mock_spa` en `src/G1.Identity.Mock/Fixtures/MockClients.cs` con `redirect_uri` exacta, scopes de D-004 y PKCE obligatorio. **Rama**: `dev2/oauth-oidc`. **Entregable**: cliente de desarrollo sin secreto en el repositorio.
- [ ] T067 [P] [P1] [DEV2] [US2] Crear `tests/G1.Identity.ContractTests/TokenAudienceTests.cs`: access token `aud=identity-api`, ID token `aud=client_id` y rechazo por una configuración de consumidor que no declara esa audiencia. **Rama**: `dev2/oauth-oidc`. **Entregable**: SC-011 y FR-053; D-016 sigue abierta.

### Implementation for User Story 2

- [ ] T028 [P1] [DEV2] [US2] Implementar `src/G1.Identity.Mock/Endpoints/OidcEndpoints.cs` delegando descubrimiento, JWKS, autorización, token, userinfo y revocación a OpenIddict; sin emisión manual de tokens (Skill: `dotnet-api-architecture`). **Rama**: `dev2/oauth-oidc`. **Entregable**: única vía de emisión en `/connect/token`.
- [ ] T029 [P] [P1] [DEV5] [US2] Crear `src/G1.Identity.Mock.Ui/src/components/PersonaSelector.tsx`, `src/G1.Identity.Mock.Ui/src/App.tsx` y `src/G1.Identity.Mock.Ui/src/styles/tokens.css` con selector accesible, foco visible y tokens locales concretos WCAG 2.1 AA; nunca almacenar tokens (Instrucciones: `coding-style-react-typescript.instructions.md`, `coding-style-css-theming.instructions.md`, `coding-style-frontend-design-system.instructions.md`). **Rama**: `dev5/ui-e2e`. **Entregable**: SC-008 y FR-031.
- [ ] T030 [P1] [DEV1] [US2] Servir el build de `src/G1.Identity.Mock.Ui/dist` desde `src/G1.Identity.Mock/wwwroot/` y configurar archivos estáticos en `src/G1.Identity.Mock/AppExtensions.cs`. **Rama**: `dev1/infra-contract`. **Entregable**: selector servido por el host sin dependencia de Node en runtime.
- [ ] T031 [P1] [DEV2] [US2] Añadir los claims `sid`, `roles`, `permissions`, `email_verified` y `phone_verified` en `src/G1.Identity.Mock/Auth/ClaimsTransformation.cs` como fotografía al emitir; la firma la hace OpenIddict (Skill: `dotnet-api-architecture`). **Rama**: `dev2/oauth-oidc`. **Entregable**: FR-007 y FR-034 en el token.
- [ ] T032 [P1] [DEV2] [US2] Ejecutar la validación de US2 con `quickstart.md` secciones 4, 5 y 6. **Rama**: `dev2/oauth-oidc`. **Entregable**: flujo reproducible sin atajos.

**Checkpoint**: US2 funciona de forma aislada; ningún token se emite fuera de `/connect/token`.

---

## Phase 5: User Story 3 — Autorización por rol y permiso, con errores explícitos (Priority: P1)

**Goal**: distinguir sesión ausente, inválida, permiso insuficiente y recurso no visible sin filtrar datos.

**Independent Test**: invocar `/me`, resúmenes y catálogos con las personas del mock y tokens inválidos, comprobando 401/403/404/400/200.

### Tests for User Story 3

- [ ] T033 [P] [P1] [DEV4] [US3] Crear `tests/G1.Identity.ContractTests/AuthorizationTests.cs` con un caso de éxito y uno de error por operación, incluidos `identity.session.required`, `identity.session.invalid`, `identity.permission.denied`, `identity.user.not_found` y `identity.request.invalid`. **Rama**: `dev4/authz-audit`. **Entregable**: SC-004.
- [ ] T034 [P] [P1] [DEV4] [US3] Crear `tests/G1.Identity.ContractTests/InvalidTokenTests.cs` con token caducado, revocado, firma inválida, `alg=none`, emisor incorrecto y audiencia incorrecta. **Rama**: `dev4/authz-audit`. **Entregable**: US3 escenarios 1-2.
- [ ] T069 [P] [P1] [DEV4] [US3] Crear `tests/G1.Identity.ContractTests/CatalogTests.cs`: 10 roles, 3 permisos de G1, `catalogVersion`, `provisional: true`, `ETag` y tolerancia a códigos desconocidos como no concedidos. **Rama**: `dev4/authz-audit`. **Entregable**: FR-025 a FR-028 y FR-056.
- [ ] T071 [P] [P1] [DEV3] [US3] Crear `tests/G1.Identity.ContractTests/ContactIsolationTests.cs`: no existe endpoint de contacto ajeno, `/connect/userinfo` devuelve solo datos propios e ignora cualquier `userId` de otra persona. **Rama**: `dev3/users-persistence`. **Entregable**: SC-012 y FR-057.

### Implementation for User Story 3

- [ ] T035 [P] [P1] [DEV4] [US3] Implementar `src/G1.Identity.Mock/Auth/PermissionPolicies.cs` con denegación por defecto, mapeo a `ProblemDetails` y trato de códigos desconocidos como no concedidos (Skill: `dotnet-api-architecture`). **Rama**: `dev4/authz-audit`. **Entregable**: matriz D-002 aplicada sin interpretar D-003.
- [ ] T036 [P] [P1] [DEV3] [US3] Implementar `GET /api/v1/identity/me` en `src/G1.Identity.Mock/Endpoints/IdentityEndpoints.cs` con `Cache-Control: no-store`, `X-Correlation-ID` y el esquema `MeResponse`. **Rama**: `dev3/users-persistence`. **Entregable**: FR-007 a FR-012.
- [ ] T037 [P1] [DEV3] [US3] Implementar `/users/{userId}/summary` y `/users/lookup` en `src/G1.Identity.Mock/Endpoints/IdentityEndpoints.cs`: «Lote de `lookup` de 1 a 100 identificadores, deduplicado» y «`Cache-Control: no-store` en identidad y sesiones»; sin correo, teléfono ni datos del tutor. **Rama**: `dev3/users-persistence`. **Depende de**: T036 (mismo archivo) y T035 (política). **Entregable**: FR-013 a FR-017.
- [ ] T038 [P1] [DEV4] [US3] Implementar `/catalog/roles` y `/catalog/permissions` en `src/G1.Identity.Mock/Endpoints/CatalogEndpoints.cs` con `ETag`, `catalogVersion` y `provisional: true`. **Rama**: `dev4/authz-audit`. **Entregable**: catálogo versionado.
- [ ] T039 [P1] [DEV4] [US3] Ejecutar la validación de US3 con `quickstart.md` secciones 5 y 8. **Rama**: `dev4/authz-audit`. **Entregable**: 401/403/404/400/200 documentados.

**Checkpoint**: US3 funciona de forma aislada; la matriz provisional D-002 se aplica sin interpretar ámbitos D-003.

---

## Phase 6: User Story 4 — Logout y revocación de sesiones (Priority: P2)

**Goal**: cerrar la sesión actual y revocar otras sin afectar a sesiones que deben permanecer activas.

**Independent Test**: abrir dos sesiones de la misma persona, revocar una y comprobar que la otra sigue válida; verificar que el refresh revocado no se canjea.

### Tests for User Story 4

- [ ] T040 [P] [P2] [DEV4] [US4] Crear `tests/G1.Identity.ContractTests/SessionTests.cs` con logout, `DELETE /sessions/{sessionId}`, `revoke-all` con `keepCurrentSession=true`, sesión ajena (`404`, no `403`) e idempotencia. **Rama**: `dev4/authz-audit`. **Entregable**: SC-010.
- [ ] T068 [P] [P2] [DEV4] [US4] Crear `tests/G1.Identity.ContractTests/AuditTests.cs`: eventos `logout`, `session_revoked` y `token_revoked` con `correlationId`, actor, objetivo, resultado y marca temporal, sin tokens ni datos personales. **Rama**: `dev4/authz-audit`. **Entregable**: FR-023, FR-044 y SC-004.

### Implementation for User Story 4

- [ ] T041 [P] [P2] [DEV4] [US4] Definir `src/G1.Identity.Mock/Abstractions/ISessionService.cs` e implementar `src/G1.Identity.Mock/Services/SessionService.cs`: `sid`, `lastSeenAt`, transiciones `active`/`revoked`/`expired`, vínculo `SessionToken` y revocación de refresh (Instrucciones: `coding-style-csharp-service-implementation.instructions.md`, `coding-style-csharp-xml-documentation.instructions.md`). **Rama**: `dev4/authz-audit`. **Entregable**: revocación efectiva en G1.
- [ ] T042 [P] [P2] [DEV4] [US4] Implementar `GET /api/v1/identity/sessions`, `DELETE /api/v1/identity/sessions/{sessionId}` y `POST /api/v1/identity/sessions/revoke-all` en `src/G1.Identity.Mock/Endpoints/SessionEndpoints.cs`. **Rama**: `dev4/authz-audit`. **Entregable**: FR-018 a FR-020.
- [ ] T043 [P] [P2] [DEV2] [US4] Implementar `/connect/logout` GET y POST en `src/G1.Identity.Mock/Endpoints/OidcEndpoints.cs` con `post_logout_redirect_uri` de coincidencia exacta y cierre de sesión OpenIddict. **Rama**: `dev2/oauth-oidc`. **Entregable**: FR-024 y FR-021 para el proveedor.
- [ ] T044 [P2] [DEV4] [US4] Implementar `POST /api/v1/identity/logout` en `src/G1.Identity.Mock/Endpoints/SessionEndpoints.cs`; documentar que el access token sigue válido hasta `exp` fuera de G1 (D-005). **Rama**: `dev4/authz-audit`. **Depende de**: T042 (mismo archivo). **Entregable**: FR-021.
- [ ] T045 [P2] [DEV4] [US4] Registrar `logout`, `session_revoked` y `token_revoked` en `src/G1.Identity.Mock/Services/AuditService.cs` sin tokens ni datos personales. **Rama**: `dev4/authz-audit`. **Entregable**: FR-023 y FR-044.
- [ ] T046 [P2] [DEV4] [US4] Ejecutar la validación de US4 con `quickstart.md` sección 7. **Rama**: `dev4/authz-audit`. **Entregable**: TTL y revocación medidos por separado.

**Checkpoint**: US4 funciona de forma aislada; revocación inmediata en G1 y refresh, TTL de 5 min para JWT.

---

## Phase 7: User Story 5 — Personas y casos límite reproducibles (Priority: P2)

**Goal**: fixtures deterministas, públicos y reiniciables para que todos los equipos prueben lo mismo.

**Independent Test**: ejecutar `POST /api/v1/identity/mock/reset` dos veces y comprobar que las 15 personas, los 4 casos de token, roles y permisos vuelven al estado base.

### Tests for User Story 5

- [ ] T047 [P] [P2] [DEV3] [US5] Crear `tests/G1.Identity.ContractTests/MockFixturesTests.cs`: 15 personas y 4 escenarios `token.*` no contados como personas, `fixtureVersion` estable, reset idempotente y `404` fuera de Development/Testing. **Rama**: `dev3/users-persistence`. **Entregable**: FR-032 y FR-033.

### Implementation for User Story 5

- [ ] T048 [P] [P2] [DEV3] [US5] Implementar `GET /api/v1/identity/mock/personas` y `POST /api/v1/identity/mock/reset` en `src/G1.Identity.Mock/Endpoints/MockPersonaEndpoints.cs` con paginación `page`/`pageSize` máximo 100; reset sin token y solo en Development/Testing. **Rama**: `dev3/users-persistence`. **Entregable**: FR-032 y FR-033.
- [ ] T049 [P] [P2] [DEV2] [US5] Añadir los casos `token.expired`, `token.revoked`, `token.wrong-audience` y `token.invalid-signature` en `src/G1.Identity.Mock/Fixtures/EdgeCaseTokens.cs`; se emiten únicamente a través de `/connect/token`. **Rama**: `dev2/oauth-oidc`. **Entregable**: FR-035 y FR-036.
- [ ] T050 [P] [P2] [DEV3] [US5] Aplicar en `src/G1.Identity.Mock/Auth/MockPersonaAuthentication.cs` el error genérico para personas inactivas/bloqueadas, permisos limitados para correo sin verificar y resumen minimizado para menores (D-017). **Rama**: `dev3/users-persistence`. **Entregable**: US5 escenarios 3-5.
- [ ] T051 [P2] [DEV3] [US5] Ejecutar la validación de US5 con `quickstart.md` sección 9. **Rama**: `dev3/users-persistence`. **Entregable**: datos ficticios y volátiles confirmados.

**Checkpoint**: US5 funciona de forma aislada; los datos son ficticios y volátiles.

---

## Phase 8: User Story 6 — Pruebas de contrato y E2E (Priority: P3)

**Goal**: detectar divergencias del mock respecto al contrato antes de que afecten a G2, G3 o G4.

**Independent Test**: ejecutar las suites de contrato, E2E, accesibilidad y compatibilidad sin interfaces de módulos consumidores.

### Tests for User Story 6

- [ ] T052 [P] [P3] [DEV4] [US6] Completar la matriz de cobertura en `tests/G1.Identity.ContractTests/README.md`: las 19 operaciones de `contracts/openapi.v1.yaml` con al menos una prueba de éxito y una de error validadas contra el esquema; incluir `x-success-example` para respuestas sin cuerpo. **Rama**: `dev4/authz-audit`. **Entregable**: SC-002.
- [ ] T053 [P] [P3] [DEV5] [US6] Crear `tests/G1.Identity.E2E/AuthorizationFlowSpecs.cs` con Playwright 1.64.1: autorización, PKCE, `/me`, logout y revocación en Chromium. **Rama**: `dev5/ui-e2e`. **Entregable**: SC-005.
- [ ] T054 [P] [P3] [DEV5] [US6] Crear `tests/G1.Identity.E2E/AccessibilitySpecs.cs` con axe-core sobre `PersonaSelector.tsx` y las páginas de error; navegación solo con teclado. **Rama**: `dev5/ui-e2e`. **Entregable**: SC-008 y NFR-005.
- [ ] T055 [P] [P3] [DEV3] [US6] Crear `tests/G1.Identity.CompatibilityTests/ProviderSmokeTests.cs` con SQLite para detectar diferencias de proveedor (mitigación obligatoria de ADR-0002). **Rama**: `dev3/users-persistence`. **Entregable**: evidencia de proveedor relacional sin cambiar el contrato.
- [ ] T070 [P] [P3] [DEV5] [US6] Crear `tests/G1.Identity.E2E/TokenStorageSpecs.cs`: la UI no guarda tokens en `localStorage`, `sessionStorage` ni cookies propias; el selector solo inicia el flujo con PKCE. **Rama**: `dev5/ui-e2e`. **Entregable**: FR-036 y soporte a D-001 sin decidirla.
- [ ] T072 [P] [P2] [DEV5] [US6] Crear `tests/G1.Identity.ContractTests/ProviderSubstitutionTests.cs`: cambiar emisor, cliente y claves sin cambiar el código de negocio del consumidor. **Rama**: `dev5/ui-e2e`. **Entregable**: SC-003; D-016 permanece abierta.

### Implementation for User Story 6

- [ ] T056 [P3] [DEV1] [US6] Integrar en `.github/workflows/ci-g1-identity.yml` los pasos de `oasdiff breaking`, `@redocly/cli lint`, escaneo de secretos y navegadores adicionales en ejecución programada. **Rama**: `dev1/infra-contract`. **Entregable**: SC-005, SC-006 y SC-009 en CI.
- [ ] T057 [P3] [DEV5] [US6] Ejecutar la validación de US6 con `quickstart.md` secciones 10 y 11. **Rama**: `dev5/ui-e2e`. **Entregable**: resultados en `docs/g1/validacion-fase-a.md`.

**Checkpoint**: US6 funciona de forma aislada; CI falla ante rupturas no versionadas.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: gobernanza, seguridad, calidad y documentación final de la fase A. Cada responsable documenta y prueba su propio código.

- [ ] T058 [P] [P2] [DEV5] Completar `docs/g1/guia-consumo-identidad.md` con generación de clientes, PKCE, `aud` provisional D-016, D-003 y D-017, y aviso de revocación de JWT; revisión de exactitud contractual por DEV1. **Rama**: `dev5/ui-e2e`. **Entregable**: SC-001 y FR-049; guía usable por otro módulo.
- [x] T059 [P] [P2] [DEV1] Registrar en `docs/g1/herramientas-y-skills.md` la excepción Swagger UI frente a la preferencia Scalar de `.github/instructions/coding-style-csharp-project-structure.instructions.md` y la excepción InMemory de ADR-0002, sin modificar `.specify/memory/constitution.md`; añadir la comprobación de skills de T008. **Rama**: `dev1/infra-contract`. **Entregable**: T059 como cierre de la integración de skills en OpenCode/Copilot.
- [ ] T060 [P] [P2] [DEV4] Revisar cabeceras, CORS, antiforgery, rate limiting, CSP y logs en `src/G1.Identity.Mock/BuilderExtensions.cs` y `AppExtensions.cs` (Skill: `code-review` en `.github/skills/code-review/SKILL.md`). **Rama**: `dev4/authz-audit`. **Entregable**: FR-041 a FR-045 y NFR-009 sin regresiones.
- [ ] T061 [P] [P2] [DEV5] Medir `/me` con `tests/G1.Identity.ContractTests/PerformanceTests.cs` y verificar p95 inferior a 200 ms en local, excluido el primer arranque (SC-007). **Rama**: `dev5/ui-e2e`. **Entregable**: medición registrada.
- [ ] T062 [P] [P2] [DEV5] Verificar castellano en textos, inglés en identificadores y recursos `.resx` en `src/G1.Identity.Mock/Resources/` y `src/G1.Identity.Mock.Ui/src/`. **Rama**: `dev5/ui-e2e`. **Entregable**: NFR-006 sin textos de relleno.
- [ ] T063 [P2] [DEV5] Crear `docs/g1/CHANGELOG-contrato.md` con `1.0.0`, estado provisional y política de cambios incompatibles de `contracts/README.md`; revisión por DEV1. **Rama**: `dev5/ui-e2e`. **Entregable**: FR-050.
- [ ] T064 [P3] [DEV1] Ejecutar revisión final del diff con `code-review` en `.github/skills/code-review/SKILL.md`, sin generar cambios de aplicación nuevos. **Rama**: `dev1/infra-contract`. **Depende de**: todas las tareas de fase A. **Entregable**: hallazgos cerrados o documentados.
- [ ] T065 [P2] [DEV1] Ejecutar `quickstart.md` completo y registrar resultados por SC en `docs/g1/validacion-fase-a.md`. **Rama**: `dev1/infra-contract`. **Entregable**: trazabilidad SC-001 a SC-012 cerrada.
- [ ] T066 [P] [P2] [DEV4] Crear `tests/G1.Identity.ContractTests/SecurityHeadersAndSecretScanTests.cs`: `X-Correlation-ID`, `Cache-Control: no-store`, `ETag`, `Retry-After`, `Content-Security-Policy` en Swagger UI, `X-Content-Type-Options: nosniff` y comprobación de que el repositorio no contiene claves ni tokens reales. **Rama**: `dev4/authz-audit`. **Entregable**: SC-009, FR-042, FR-043 y FR-045.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: sin dependencias; T002 bloquea T003-T005; T008 es documental y paralela.
- **Foundational (Phase 2)**: depende de Setup; T010 bloquea T011/T012/T017; T014 bloquea todos los endpoints; T015 bloquea US2; T016 bloquea US5 y el selector; T009 bloquea la configuración de TTL. **Puerta**: ADR-0002 ratificada por equipo G1 el 2026-11-25 para el alcance académico; la persistencia relacional sigue pendiente para datos reales.
- **User Stories (Phase 3-8)**: dependen de Foundational. US1 y US2 son P1 y pueden avanzar en paralelo; US3 depende de T031 para `/me` con Bearer y de T035 para permisos; US4 depende de la sesión creada en US2; US5 depende de T016/T017 y T028; US6 depende de que las historias anteriores existan para cubrir las 19 operaciones.
- **Polish (Phase 9)**: depende de US1-US6; T064 y T065 son las últimas.

### Dependencias entre responsables

| De | A | Dependencia | Por qué |
| --- | --- | --- | --- |
| DEV1 | DEV2/DEV3/DEV4/DEV5 | T001, T002, T005-T009, T013-T014 | Solución, versiones, settings, pipeline y errores compartidos |
| DEV3 | DEV2 | T010, T016 | DbContext y autenticación de persona antes del flujo OIDC |
| DEV2 | DEV3 | T028, T031 | Emisión y claims para `/me` con Bearer |
| DEV2 | DEV4 | T028, T043 | Sesión OpenIddict y logout OIDC antes de sesiones/auditoría |
| DEV4 | DEV3 | T035 | Política de permisos antes de resúmenes y catálogos |
| DEV3 | DEV4 | T017 | Fixtures con roles y matriz antes de pruebas de autorización |
| DEV5 | DEV1 | T004, T029 | Build del selector antes de servirlo en T030 |

### Within Each User Story

1. Pruebas primero; deben fallar antes de la implementación.
2. Contratos/esquemas y entidades antes que servicios.
3. Servicios antes que endpoints.
4. Implementación antes de la verificación con `quickstart.md`.
5. Cada responsable ejecuta las pruebas y escribe la documentación de su propio código; el PR no se considera listo sin ambos.

### Parallel Opportunities

- Setup: T003, T004, T005, T007 y T008 son paralelos tras T001/T002.
- Foundational: T011, T012, T013 y T016 son paralelos tras T010/T014; T015 y T018 son paralelos.
- US1: T019, T020 y T021 en paralelo.
- US2: T025, T026, T027, T029 y T067 en paralelo; T029 avanza sin esperar a T028.
- US3: T033, T034, T035, T036, T069 y T071 en paralelo; T037 va después de T036.
- US4: T040, T041, T042, T043 y T068 en paralelo; T044 va después de T042.
- US5: T047, T048, T049 y T050 en paralelo.
- US6: T052, T053, T054, T055, T070 y T072 en paralelo.
- Polish: T058, T059, T060, T061, T062 y T066 en paralelo; T064 y T065 al final.

## Parallel Example: User Story 2

```bash
# Rama DEV2, tras Foundational
Task: "Crear tests/G1.Identity.ContractTests/PkceFlowTests.cs"
Task: "Crear tests/G1.Identity.ContractTests/TokenValidationTests.cs"
Task: "Registrar cli_mock_spa en src/G1.Identity.Mock/Fixtures/MockClients.cs"
Task: "Crear tests/G1.Identity.ContractTests/TokenAudienceTests.cs"

# Rama DEV5, en paralelo con DEV2
Task: "Crear PersonaSelector.tsx, App.tsx y tokens.css"

# Integración posterior por DEV1
Task: "Servir el build desde AppExtensions.cs y wwwroot/"
```

---

## Implementation Strategy

### MVP First (US1 + US2)

1. Completar Phase 1 y Phase 2. **Puerta cumplida**: ADR-0002 ratificado por equipo G1 el 2026-11-25 dentro del alcance académico.
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

- **DEV1**: infraestructura, contrato, Swagger, CI, integración, gates y validación final. No edita archivos de negocio de otros DEV; integra sus PR.
- **DEV2**: protocolo OIDC/OAuth en `Auth/` y `Endpoints/OidcEndpoints.cs`; pruebas de token y audiencia.
- **DEV3**: entidades, persistencia, fixtures, autenticación de persona y endpoints de usuario; pruebas de fixtures y contacto.
- **DEV4**: políticas, catálogos, sesiones, auditoría y pruebas de autorización/seguridad.
- **DEV5**: selector React, accesibilidad, E2E, guía de consumo, CHANGELOG y sustitución por configuración.
- Todas las historias se integran sobre el contrato inmutable `contracts/openapi.v1.yaml`; un cambio del contrato exige PR de DEV1 con aviso a los consumidores.

### Propiedad de archivos (conflictos mínimos)

| Área | Propietario | Nota |
| --- | --- | --- |
| `G1.Identity.slnx`, `global.json`, `Directory.Packages.props`, `.github/workflows/`, `src/G1.Identity.Mock/Program.cs`, `BuilderExtensions.cs`, `AppExtensions.cs`, `Settings/`, `OpenApi/`, `contracts/` | DEV1 | Un solo editor por archivo; los demás proponen cambios vía PR revisado por DEV1 |
| `src/G1.Identity.Mock/Auth/OpenIddictConfiguration.cs`, `ClaimsTransformation.cs`, `Endpoints/OidcEndpoints.cs`, `Fixtures/MockClients.cs`, `Fixtures/EdgeCaseTokens.cs` | DEV2 | Incluye logout OIDC (T043) |
| `src/G1.Identity.Mock/Auth/MockPersonaAuthentication.cs`, `Persistence/`, `Fixtures/SeedData.cs`, `Endpoints/IdentityEndpoints.cs`, `Endpoints/MockPersonaEndpoints.cs` | DEV3 | `IdentityEndpoints.cs` se edita secuencialmente en T036 y T037 |
| `src/G1.Identity.Mock/Auth/PermissionPolicies.cs`, `Endpoints/CatalogEndpoints.cs`, `Endpoints/SessionEndpoints.cs`, `Services/`, `Abstractions/ISessionService.cs`, `Endpoints/ErrorMapping.cs` | DEV4 | `SessionEndpoints.cs` se edita secuencialmente en T042 y T044 |
| `src/G1.Identity.Mock.Ui/` y `tests/G1.Identity.E2E/` | DEV5 | El build se publica en `wwwroot/`, propiedad de DEV1, mediante T030 |
| `tests/G1.Identity.ContractTests/` | Por archivo: DEV1 (documento), DEV2 (token), DEV3 (fixtures/contacto), DEV4 (autorización/seguridad), DEV5 (sustitución/perf) | Cada prueba nueva se crea en un archivo propio |

---

## Trazabilidad de cobertura

| Requisitos / SC | Tareas |
| --- | --- |
| FR-001 a FR-006 (contrato y documentación) | T001, T019-T023, T006, T056, T063 |
| FR-007 a FR-012 (`/me`) | T031, T036, T052, T061 |
| FR-013 a FR-017 (usuarios) | T011, T037, T050, T071 |
| FR-018 a FR-024 (sesiones y logout) | T012, T040-T046, T068 |
| FR-025 a FR-028 (catálogo) | T011, T017, T038, T069 |
| FR-029 a FR-037 (mock) | T015-T017, T025-T032, T047-T051, T072 |
| FR-038 a FR-045 (errores y seguridad) | T013, T014, T018, T033, T060, T066 |
| FR-046 a FR-052 (pruebas y documentación) | T019, T052-T057, T058, T062-T065 |
| FR-053 a FR-058 (audiencia, introspección, refresco, catálogo y contacto) | T026, T067, T019, T040, T069, T071 |
| NFR-001 a NFR-010 | T004, T024, T029, T054, T061, T062, T065 |
| SC-001 a SC-012 | T024, T019, T052, T072, T033, T053, T023, T061, T054, T066, T026, T067, T071 (detalle en `quickstart.md` §12) |

---

## Notes

- Los IDs son estables: T066–T072 se añadieron para cerrar cobertura de FR/SC y aparecen en la fase que les corresponde, no al final del documento.
- `[P]` = archivos distintos y sin dependencias; verificar antes de marcar. `P0` a `P3` indican prioridad, no paralelismo.
- `[Story]` = trazabilidad con `spec.md`; no se añade en Setup, Foundational ni Polish.
- Cada historia tiene prueba independiente y checkpoint; no se omiten.
- No se genera código de aplicación en este comando: solo este `tasks.md`, `spec.md`, `plan.md`, contratos y documentación de equipo.
- ADR-0002 está ratificado para el alcance académico; no se ejecuta ninguna tarea fuera del listado aprobado ni se usa InMemory con datos reales.
- La responsabilidad de pruebas y documentación es de quien ejecuta cada tarea; el PR se rechaza si falta alguna.

## Bloqueos pendientes (no resueltos por estas tareas)

| Bloqueo | Estado | Impacto |
| --- | --- | --- |
| Persistencia relacional y migraciones (D-009) | ADR-0002 ratificado solo para el alcance académico (equipo G1, 2026-11-25) | Bloquea datos reales y producción; T055 y el plan de migración siguen pendientes |
| ADR-0001 (servidor OAuth real con Identity + OpenIddict) | Propuesto | Bloquea la fase B; no bloquea el mock de fase A |
| D-001 (BFF/SPA), D-012 (consentimiento), D-016 (audiencias) | Abiertas | No se resuelven en código; el contrato y el mock las declaran provisionales o configurables |
| D-004 (valores de producción) y D-005 (revocación de JWT en producción) | Parciales | Solo se aplican los valores de desarrollo aprobados |
| D-003 (verificación con G2) y D-017 (contrato de contacto G3-G4) | Resueltas para fase A; integración en fase C | No se añaden endpoints de ámbitos ni contacto de terceros |
| Skill `myadmon-theme` referenciada por instrucciones de frontend | Ausente en `.github/skills/` | La T029 define tokens locales concretos; se necesita la skill para alineación visual definitiva |
| Integración de skills en OpenCode | Resuelta | `opencode.jsonc` registra `.github/skills/`; evidencia y verificación en T008 y T059 |

---

## Fases posteriores (solo previsión; NO son tareas de la fase A)

El reparto siguiente orienta la planificación de nuevas especificaciones. **No debe ejecutarse ni marcarse en este `tasks.md`**: cada fase requerirá su propio `spec.md`, `plan.md` y `tasks.md` aprobados. El detalle y las dependencias están en `docs/g1/team-assignment.md`.

### Fase B — Servidor real (especificación posterior)

| Responsable previsto | Responsabilidad | Dependencias previas |
| --- | --- | --- |
| DEV1 | Arquitectura del servidor real, despliegue, CI/CD, contrato v2 si aplica | Contrato v1 estable y ADR-0001 ratificada |
| DEV2 | Identity + OpenIddict, login, emisión, refresco, logout y claves | D-001, D-004 (producción), D-005 y D-016 decididas |
| DEV3 | Registro, verificación, recuperación, 2FA y persistencia real | D-007, D-010, D-011 y persistencia relacional pendiente (ADR-0002 solo cubre el alcance académico) |
| DEV4 | Catálogo definitivo, asignación auditada, políticas y retención | D-002, D-006, D-008 y D-013 decididas |
| DEV5 | Cliente web definitivo, BFF o SPA según D-001, E2E de producción | D-001 y D-016 decididas |

### Fase C — Integración real (especificación posterior)

| Responsable previsto | Responsabilidad | Dependencias previas |
| --- | --- | --- |
| DEV1 | Coordinación de integración, versionado y gates entre módulos | Fase B completa |
| DEV2 | Validación de tokens y audiencias con G2, G3 y G4 | D-016 cerrada |
| DEV3 | Contrato de contacto con G3 para G4 y verificación de datos propios | D-017 y contrato G3-G4 |
| DEV4 | Verificación de ámbitos y denegación con entrenadores de uno y varios equipos | D-003 verificada con G2 |
| DEV5 | E2E cruzada G2/G3/G4, accesibilidad y sustitución final del mock | Todos los módulos desplegados en integración |
