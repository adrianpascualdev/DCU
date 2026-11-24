# Implementation Plan: Contrato de identidad y mock para G2, G3 y G4 (fase A)

**Branch**: `001-g1-identidad-contrato-mock` | **Date**: 2026-11-24 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-g1-identidad-contrato-mock/spec.md`, ya clarificada; ADR-0001 y ADR-0002; registro `docs/decisions/pending-decisions.md`.

**Alcance de este plan**: exclusivamente la **fase A** (contrato OpenAPI 3.1 + mock de identidad + Swagger UI + pruebas de contrato/E2E + guía de consumo). No se planifica ni se implementa el servidor OAuth real, el registro real de usuarios ni la persistencia de producción. ASP.NET Core Identity y OpenIddict quedan como arquitectura prevista para la fase B; en fase A OpenIddict se usa solo como motor de protocolo del mock, con personas ficticias y claves efímeras.

## Summary

Entregar un contrato OpenAPI 3.1 versionado y un mock de identidad fiel que permita a G2, G3 y G4 desarrollar y probar sus flujos sin esperar al servidor real. El mock reproduce la superficie OIDC/OAuth observable (descubrimiento, JWKS, Authorization Code + PKCE S256, token, userinfo, revocación y logout) y la API de identidad (`/me`, resumen y lote de usuarios, sesiones, logout, catálogos y fixtures de desarrollo), usando OpenIddict 7.8.0 sobre ASP.NET Core Minimal APIs y EF Core InMemory con `DisableBulkOperations()`.

La autorización se resuelve con códigos estables de rol y permiso; G1 no interpreta ámbitos de negocio (D-003) y no expone datos de contacto de terceros (D-017). El mock no introduce atajos de autorización: la única vía de emisión de tokens es `/connect/token` tras un código obtenido con PKCE. El contrato se publica antes de implementar el mock y se valida en CI con pruebas de esquema y detección de cambios incompatibles.

## Technical Context

**Language/Version**: C# 14 sobre .NET 10.0 (`net10.0`), SDK 10.0.404; TypeScript 5.x + React con Vite para el selector de personas; Node.js LTS solo como entorno de build del frontend.

**Primary Dependencies**:
- ASP.NET Core Minimal APIs (.NET 10).
- `OpenIddict.AspNetCore` 7.8.0 y `OpenIddict.EntityFrameworkCore` 7.8.0.
- `Microsoft.EntityFrameworkCore.InMemory` 10.0.15.
- `Microsoft.AspNetCore.OpenApi` 10.0.15 para generar el documento OpenAPI 3.1.
- `Swashbuckle.AspNetCore.SwaggerUI` 10.3.1 solo para la UI de Swagger; no genera el documento.
- Rate limiting, antiforgery, Data Protection y autenticación de ASP.NET Core (incluidos en el framework).
- `xunit.v3` 4.0.3, `Microsoft.Playwright` 1.64.1, `JsonSchema.Net` para validar respuestas contra OpenAPI 3.1 y `oasdiff` como herramienta de CI (no dependencia de runtime) para detectar cambios incompatibles.
- React + TypeScript + Vite: versiones exactas fijadas en `package-lock.json` al iniciar la implementación.

**Storage**: EF Core InMemory para el estado del mock y los stores de OpenIddict. `EnsureCreated()`; sin migraciones; `DisableBulkOperations()` obligatorio. Clave de firma de desarrollo efímera, generada al arrancar y nunca comprometida. Los datos se pierden al reiniciar y el mock lo advierte en logs, README y Swagger.

**Testing**: xUnit v3 para pruebas de contrato e integración HTTP con `WebApplicationFactory`; validación de respuestas contra el esquema OpenAPI 3.1; Playwright para el flujo de navegador (autorización, PKCE, `/me`, logout y revocación); axe-core inyectado en Playwright para accesibilidad; `oasdiff` para cambios incompatibles.

**Target Platform**: Linux y Windows en desarrollo y CI; Kestrel en `localhost`; HTTPS de desarrollo. El mock nunca se despliega en producción.

**Project Type**: servicio web (backend Minimal APIs) + interfaz React independiente para el selector de personas; artefactos de contrato y pruebas.

**Performance Goals**: `/me` con p95 inferior a 200 ms en local, excluido el primer arranque; pruebas de contrato por debajo de 5 minutos; E2E por debajo de 10 minutos en CI estándar.

**Constraints**:
- Solo Authorization Code + PKCE S256; prohibidos implicit flow, password grant, `client_credentials` para identidad de usuario y `code_challenge_method=plain`.
- Sin introspección en v1; los access tokens JWT expiran por TTL (5 min por defecto).
- Sin endpoint de contacto de terceros; `/connect/userinfo` solo devuelve datos del usuario autenticado (D-017).
- G1 no interpreta ámbitos de negocio; cada módulo resuelve los suyos con `userId` (D-003).
- Catálogo v1 provisional: 10 roles y 3 permisos propiedad de G1, deny-by-default (D-002).
- Valores de desarrollo/pruebas de D-004: código 5 min, access 5 min, ID token 5 min, refresh rotativo 8 h, inactividad 30 min, absoluta 12 h, tolerancia 2 min.
- Sin secretos, contraseñas fijas ni datos personales reales en el repositorio.
- Este comando de planificación no genera código de aplicación: la implementación del mock se ejecutará solo tras la aprobación del plan y de las excepciones pendientes.

**Scale/Scope**: 18 personas ficticias, 10 roles, 3 permisos de G1, 7 rutas OIDC/OAuth (8 operaciones), 9 rutas de identidad y 2 rutas de mock; 19 operaciones documentadas en 18 rutas, más la página del selector de personas.

## Constitution Check

*GATE: debe pasar antes de la fase 0 y revaluarse tras el diseño de la fase 1.*

| Principio / restricción | Evaluación | Estado |
| --- | --- | --- |
| I. Fidelidad al dominio y control del alcance | Solo se entrega identidad y contrato; no se invaden datos de G2/G3/G4 ni se inventan funcionalidades de negocio. | PASS |
| II. Arquitectura modular e independiente | G1 es propietario de identidad; los consumidores usan REST versionado y `userId`; el mock no lee datos de otros módulos. | PASS |
| III. Desarrollo guiado por contratos (API-First) | El contrato OpenAPI 3.1 se escribe antes del mock, con métodos, rutas, seguridad, esquemas, errores y versión; CI detecta rupturas. | PASS |
| IV. Seguridad, privacidad e identidad única (NON-NEGOTIABLE) | Denegación por defecto, permisos efectivos en servidor, PKCE S256, sesiones revocables, minimización, sin secretos y auditoría. El mock no es vía alternativa de autorización. | PASS con verificación obligatoria en pruebas |
| V. Integridad de datos y trazabilidad | Estado del mock determinista, reset idempotente y auditoría de eventos. InMemory no ofrece migraciones, restricciones ni recuperación. | **CONDICIONAL**: excepción ADR-0002 pendiente de ratificación |
| VI. Experiencia por rol, accesibilidad e idioma | Selector de personas accesible WCAG 2.1 AA, textos en castellano, códigos estables en inglés y estados de carga/vacío/error. | PASS |
| VII. Calidad comprobable y pruebas | xUnit para contrato e integración, Playwright para E2E, pruebas de acceso permitido y denegado, accesibilidad y regresión. | PASS |
| VIII. Simplicidad y mantenimiento | OpenIddict evita un servidor OAuth casero; no se añaden capas ni servicios innecesarios; las decisiones transversales están en ADR. | PASS |
| IX. Requisitos confirmados frente a decisiones pendientes | D-001, D-012 y D-016 siguen abiertas; D-004 y D-005 mantienen abiertos sus valores/mecanismos de producción. El mock las trata como configuración o como no soportadas, sin resolverlas implícitamente. | PASS |
| X. Integración progresiva con datos simulados | Mock fiel, sustituible por configuración de emisor/cliente/claves, con casos públicos, restringidos, vacíos, inválidos y de error. | PASS |
| Restricción técnica 1: React + TypeScript independiente | El selector se construye con React + TypeScript y build propio; el backend no depende de Node.js. | PASS |
| Restricción técnica 2: .NET 10, C# 14 y Minimal APIs | TFM `net10.0`, C# 14 y Minimal APIs; sin controladores MVC. | PASS |
| Restricción técnica 3: OpenAPI + Swagger UI | `Microsoft.AspNetCore.OpenApi` genera OpenAPI 3.1; `Swashbuckle.AspNetCore.SwaggerUI` sirve la UI; deshabilitada fuera de desarrollo. | PASS |
| Restricción técnica 4: migraciones versionadas | InMemory no soporta migraciones ni restricciones. Se aplica la excepción académica de ADR-0002, aún no ratificada. | **CONDICIONAL**: requiere ratificación previa a la implementación |
| Restricción técnica 7: xUnit y Playwright | xUnit v3 y Playwright 1.64.1, tal como exige la constitución. | PASS |
| Decisión D-017: contacto de terceros | No se publica endpoint de contacto ajeno; userinfo solo devuelve datos propios; G4 obtiene contactos de G3. | PASS |
| Decisión D-003: ámbitos | G1 emite roles/permisos y no interpreta ámbitos; cada módulo los resuelve con `userId`. | PASS |
| Decisión D-005: sin introspección en v1 | `/connect/introspect` no se publica ni se implementa; la revocación de sesión/refresh se prueba separada del TTL del JWT. | PASS |

**Puertas bloqueantes antes de implementar**:

1. Ratificación de ADR-0002 (excepción de InMemory) o sustitución por SQLite/SQL Server/PostgreSQL. Sin ello, la implementación incumpliría la restricción técnica 4.
2. Aprobación de esta especificación y del plan.
3. Aprobación de ADR-0001 para la arquitectura de la fase B; la fase A puede avanzar con OpenIddict como motor del mock, pero la decisión definitiva del servidor real sigue condicionada.
4. Ninguna decisión pendiente se resolverá por defecto en código; el mock las expone como configuración o como ausencia explícita.

**Reevaluación tras el diseño (fase 1)**: el contrato generado no añade endpoints de contacto, introspección ni grants prohibidos; la matriz sigue siendo deny-by-default; no aparecen nuevas violaciones. La única excepción sigue siendo la persistencia InMemory (ADR-0002). No se modifica la constitución.

### Decisiones respetadas en este plan

| Decisión | Cómo se respeta |
| --- | --- |
| D-002: catálogo provisional de 10 roles y matriz deny-by-default | El catálogo y la matriz se modelan como datos versionados (`CatalogSnapshot`, `Role`, `Permission`, `RolePermission`); no se amplían permisos de G2/G3/G4. |
| D-003: G1 emite roles/permisos y no interpreta ámbitos | `RoleAssignment.scopeRef` es opaco y no se valida como regla de negocio; `/me` devuelve roles y permisos efectivos, no ámbitos resueltos. |
| D-004: vigencias de desarrollo/pruebas | Sesión con inactividad de 30 min y absoluta de 12 h; access/ID token de 5 min; código de 5 min; refresh rotativo de 8 h; tolerancia de 2 min. Todos configurables. |
| D-005: sin introspección en v1 | El contrato no publica `/connect/introspect`; la revocación de sesión/refresh se prueba separada del TTL del JWT. |
| D-017: sin contacto de terceros | No existe endpoint de contacto; `/connect/userinfo` solo devuelve datos del usuario autenticado; G4 obtiene contactos de G3. |
| D-001, D-012 y D-016: abiertas | El contrato admite cookie y Bearer, no decide BFF/SPA, no muestra consentimiento explícito en el mock y declara `aud=identity-api` como provisional; no se decide la matriz definitiva de audiencias. |

## Project Structure

### Documentation (this feature)

```text
specs/001-g1-identidad-contrato-mock/
├── plan.md                       # Este archivo
├── research.md                   # Fase 0: decisiones técnicas y alternativas
├── data-model.md                 # Fase 1: entidades, campos y transiciones
├── quickstart.md                 # Fase 1: guía de validación ejecutable
├── contracts/
│   ├── README.md                 # Propiedad, versionado, errores y proceso de cambio
│   └── openapi.v1.yaml           # Contrato OpenAPI 3.1 de la fase A
├── spec.md                       # Especificación clarificada
└── tasks.md                      # Fase 2 (/speckit.tasks), NO se crea aquí
```

### Source Code (repository root)

```text
src/
├── G1.Identity.Mock/             # Host ASP.NET Core Minimal APIs del mock
│   ├── Auth/                     # Autenticación de persona (solo desarrollo) y wiring PKCE
│   ├── Endpoints/                # /me, usuarios, sesiones, catálogos, mock y errores
│   ├── Fixtures/                 # Personas, roles, permisos y matriz deterministas
│   ├── Persistence/              # DbContext InMemory, stores OpenIddict y auditoría
│   ├── OpenApi/                  # Transformadores, ejemplos y esquemas del documento
│   └── wwwroot/                  # Build del selector y activos de Swagger UI
└── G1.Identity.Mock.Ui/          # React + TypeScript + Vite (selector de personas)

tests/
├── G1.Identity.ContractTests/    # xUnit v3 + WebApplicationFactory + validación OpenAPI
├── G1.Identity.E2E/              # Playwright + axe-core
└── G1.Identity.CompatibilityTests/ # Prueba de humo con proveedor relacional (SQLite) en CI

docs/
└── g1/
    └── guia-consumo-identidad.md # Guía para G2, G3 y G4 (se crea en implementación)

G1.Identity.slnx                  # Solución de la fase A
Directory.Packages.props          # Versiones centralizadas y auditables
```

**Structure Decision**: se elige una estructura de servicio web con dos proyectos de la fase A (`G1.Identity.Mock` y `G1.Identity.Mock.Ui`) y tres de pruebas. No se crea una biblioteca compartida de DTOs: los consumidores generan clientes desde el contrato y las pruebas validan JSON contra el esquema, lo que evita acoplar a los módulos a tipos internos de G1 (Principio II). La prueba de compatibilidad con SQLite existe solo para detectar diferencias de proveedor, según la mitigación obligatoria del ADR-0002.

## Phase 0: Research (resumen)

Las decisiones y alternativas completas están en [research.md](./research.md). No queda ningún `NEEDS CLARIFICATION`: las dudas de contrato y mock se cerraron en la especificación (D-002, D-003, D-004, D-005 y D-017) y las de compatibilidad se verificaron en el diagnóstico técnico.

Decisiones principales:

1. **Motor del mock**: OpenIddict 7.8.0, no un servidor OAuth propio. Evita criptografía y protocolo manuales y mantiene el mismo camino que la fase B.
2. **Autenticación de personas**: cookie de desarrollo y selector accesible; `mock_persona` solo en Development/Testing. No hay contraseñas fijas ni emisión de tokens fuera de `/connect/token`.
3. **Contrato**: OpenAPI 3.1 generado con `Microsoft.AspNetCore.OpenApi`, versionado en `contracts/openapi.v1.yaml` y validado en CI; Swagger UI solo como visor.
4. **Persistencia**: EF Core InMemory con `DisableBulkOperations()` y `EnsureCreated()`; sin migraciones; datos volátiles y reset idempotente.
5. **Sesiones y revocación**: registro de sesión propio con `sid`, vínculo con tokens de OpenIddict y revocación de refresh; `/me` revalida el estado de la sesión. Los JWT de otros módulos expiran por TTL.
6. **Errores**: RFC 9457 con `code` y `traceId` para la API; errores OAuth estándar para el protocolo.
7. **Pruebas**: xUnit v3 + validación de esquema + `oasdiff`; Playwright + axe-core; prueba de humo con SQLite.

## Phase 1: Design & Contracts (resumen)

- **Entidades**: ver [data-model.md](./data-model.md). Incluye persona, rol, permiso, asignación, sesión, cliente, autorización, token, evento de auditoría y catálogo versionado.
- **Contrato**: ver [contracts/openapi.v1.yaml](./contracts/openapi.v1.yaml) y [contracts/README.md](./contracts/README.md). Cubre OIDC/OAuth, API de identidad y endpoints de mock; no incluye introspección ni contacto de terceros.
- **Validación**: ver [quickstart.md](./quickstart.md). Describe arranque, Swagger, flujo PKCE, llamadas a `/me`, sesiones, logout, reset, pruebas y E2E.

## Trazabilidad con requisitos

| Requisitos | Dónde se cubre |
| --- | --- |
| FR-001 a FR-006 (contrato y documentación) | `contracts/openapi.v1.yaml`, `contracts/README.md`, Swagger UI, CI de `oasdiff` |
| FR-007 a FR-012 (`/me`) | Esquema `MeResponse`, endpoint `/api/v1/identity/me`, pruebas de contrato |
| FR-013 a FR-017 (usuarios) | `/users/{userId}/summary`, `/users/lookup`, minimización y pruebas de denegación |
| FR-018 a FR-024 (sesiones y logout) | Entidad `Session`, endpoints de sesiones/logout, `sid` y auditoría |
| FR-025 a FR-028 (catálogo) | `Role`, `Permission`, catálogos versionados y tolerancia a códigos desconocidos |
| FR-029 a FR-037 (mock) | OpenIddict, selector, fixtures, reset, JWKS y sustitución por configuración |
| FR-038 a FR-045 (errores y seguridad) | `ProblemDetails`, errores OAuth, rate limiting, CORS, CSRF y cabeceras |
| FR-046 a FR-052 (pruebas y documentación) | `quickstart.md`, pruebas de contrato/E2E, guía de consumo y CHANGELOG |
| FR-053 a FR-058 (audiencia, introspección, refresco, catálogo y contacto) | Discovery/scopes, ausencia de introspección, refresh rotativo y D-017 |
| NFR-001 a NFR-010 | `quickstart.md`, objetivos de latencia, determinismo, accesibilidad y CI |
| SC-001 a SC-012 | Escenarios de `quickstart.md` y pruebas automatizadas |

## Riesgos y mitigaciones de la fase A

| Riesgo | Mitigación |
| --- | --- |
| El mock se desvía del servidor real | Contrato único, pruebas de esquema, `oasdiff` y prueba de sustitución por configuración |
| El mock se confunde con una vía de autorización | Solo Development/Testing, sin contraseñas fijas, sin tokens fuera de `/connect/token`, avisos visibles y pruebas de que `mock_persona` no emite tokens |
| InMemory oculta diferencias relacionales | Modelo agnóstico, validación en dominio y prueba de humo con SQLite en CI |
| Revocación de JWT no inmediata | Access token de 5 min, revocación de sesión/refresh y documentación explícita; sin promesa de 30 s |
| Fuga de datos de menores o contacto | Minimización, sin endpoint de contacto ajeno y pruebas de denegación |
| Claves o secretos en el repositorio | Clave efímera de desarrollo, escaneo de secretos y configuración fuera del código |
| OpenIddict 8.0 preview | Se fija 7.8.0 estable y se revisa la migración cuando exista versión estable |
| Cambios incompatibles en el contrato | Versionado `v1`/`v2`, `oasdiff` y nota de migración obligatoria |

## Complexity Tracking

| Violación | Por qué es necesaria | Alternativa simple rechazada |
| --- | --- | --- |
| InMemory en lugar de migraciones versionadas | Requisito académico explícito y arranque sin infraestructura; ADR-0002 documenta la excepción y sus límites | SQLite en archivo cumple migraciones, pero añade gestión de archivo y no es la persistencia solicitada; queda como alternativa si no se ratifica la excepción |
| OpenIddict dentro del mock | Evita un servidor OAuth casero y mantiene el mismo motor que la fase B; el mock solo aporta personas y datos ficticios | Un mock con endpoints OAuth simulados sería más simple, pero se desviaría del protocolo real y crearía una falsa confianza |
| Proyecto React separado para el selector | La constitución exige React + TypeScript y el selector debe ser accesible y reutilizable en la fase B | HTML estático sería más simple, pero incumpliría la restricción de frontend y duplicaría estilos/accesibilidad |
| Prueba de compatibilidad con SQLite | Mitigación obligatoria del ADR-0002 para detectar diferencias de proveedor | Confiar solo en InMemory ocultaría errores relacionales |

## Phase 2 (no ejecutada)

`/speckit.tasks` generará `tasks.md` a partir de este plan y de la especificación. No se ejecuta automáticamente: requiere aprobación explícita del plan y de las excepciones pendientes.
