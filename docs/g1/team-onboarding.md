# Guía de incorporación — equipo G1 (fase A)

- **Fecha**: 2026-11-24
- **Audiencia**: DEV1–DEV5
- **Objetivo**: clonar, configurar herramientas, usar Spec Kit y skills, ejecutar tareas asignadas y trabajar mediante pull requests sin conflictos
- **Reparto**: [`team-assignment.md`](./team-assignment.md)
- **Tareas**: [`specs/001-g1-identidad-contrato-mock/tasks.md`](../../specs/001-g1-identidad-contrato-mock/tasks.md)

## 1. Requisitos previos

| Herramienta | Versión / comprobación | Para qué |
| --- | --- | --- |
| Git | `git --version` | Ramas, PR y sincronización |
| .NET SDK | `10.0.404` (`dotnet --version`) | API Minimal y pruebas xUnit v3 |
| Node.js y npm | LTS (`node --version`, `npm --version`) | Build del selector React |
| Python y `uv`/`uvx` | Según instalación de Spec Kit | Comandos `/speckit.*` |
| OpenCode | V2 | Comandos Spec Kit y carga de skills del repositorio |
| GitHub Copilot en VS Code | Extensión instalada | Instrucciones y skills `.github/` |
| Playwright | `pwsh tests/G1.Identity.E2E/bin/Debug/net10.0/playwright.ps1 install chromium` | E2E de navegador |
| `@redocly/cli` y `oasdiff` | `npx @redocly/cli@latest lint ...` | Validación del contrato en local |

Extensiones de VS Code recomendadas: C# Dev Kit, ESLint, markdownlint, PowerShell. La lista completa está en el [`README.md`](../../README.md).

> **Puerta**: ADR-0002 está pendiente de ratificación formal. Puedes clonar, leer y preparar documentación, pero **no ejecutes tareas de implementación** hasta que se ratifique. No se modifica `.specify/memory/constitution.md`.

## 2. Clonar y crear tu rama

```bash
git clone https://github.com/adrianpascualdev/DCU.git
cd DCU
git fetch origin
```

Si la rama de integración ya existe en remoto:

```bash
git switch -c dev1/infra-contract origin/001-g1-identidad-contrato-mock   # DEV1
git switch -c dev2/oauth-oidc origin/001-g1-identidad-contrato-mock       # DEV2
git switch -c dev3/users-persistence origin/001-g1-identidad-contrato-mock # DEV3
git switch -c dev4/authz-audit origin/001-g1-identidad-contrato-mock      # DEV4
git switch -c dev5/ui-e2e origin/001-g1-identidad-contrato-mock           # DEV5
```

Si todavía no existe en remoto, DEV1 la crea una vez desde `main`:

```bash
git switch -c 001-g1-identidad-contrato-mock origin/main
git push -u origin 001-g1-identidad-contrato-mock
```

El resto crea su rama desde esa rama de integración. **No trabajes directamente en `main` ni en la rama de integración.**

## 3. Verificar el entorno

```bash
dotnet --version                 # 10.0.404
dotnet restore G1.Identity.slnx
dotnet build G1.Identity.slnx
cd src/G1.Identity.Mock.Ui && npm ci && npm run build && cd ../../..
npx --yes @redocly/cli@latest lint specs/001-g1-identidad-contrato-mock/contracts/openapi.v1.yaml
```

Antes de implementar, lee en este orden:

1. `.specify/memory/constitution.md` (autoridad máxima; no se edita).
2. `specs/001-g1-identidad-contrato-mock/spec.md`.
3. `specs/001-g1-identidad-contrato-mock/plan.md`.
4. `specs/001-g1-identidad-contrato-mock/contracts/README.md` y `openapi.v1.yaml`.
5. `specs/001-g1-identidad-contrato-mock/tasks.md` y `docs/g1/team-assignment.md`.
6. Instrucciones `.github/instructions/` y `.github/copilot-instructions.md` aplicables a tus archivos.

## 4. Skills e instrucciones: OpenCode y GitHub Copilot

Las skills originales viven en `.github/skills/` y **no se copian ni se modifican**. GitHub Copilot las descubre automáticamente por `name`/`description`, igual que las instrucciones de `.github/instructions/`.

OpenCode V2 las descubre desde la raíz del repositorio gracias a `opencode.jsonc`, que ya está incluido:

```jsonc
{
  "$schema": "https://opencode.ai/config.json",
  "skills": ["./.github/skills"],
}
```

**Comprobación de que funciona**: abre OpenCode en la raíz del repositorio y pide al agente que cargue la skill `dotnet-api-architecture`; debe devolver el contenido de `.github/skills/dotnet-api-architecture/SKILL.md`. Si no responde, revisa que `opencode.jsonc` esté en la raíz y que no exista una configuración de usuario que sobrescriba `skills`.

| Skill | Quién la usa en fase A | Cuándo |
| --- | --- | --- |
| `dotnet-api-architecture` | DEV1, DEV2, DEV3, DEV4 | Proyectos, `Program.cs`, endpoints y servicios Minimal API |
| `nuget-package-manager` | DEV1 | Añadir o fijar paquetes con la CLI `dotnet` |
| `microsoft-docs-and-code-consultant` | DEV2 | Solo si el MCP de Microsoft Learn está disponible; verificar OpenIddict/OIDC |
| `code-review` | DEV1, DEV4 y revisores de PR | Antes de abrir PR y en la revisión final T064 |
| `skill-maker` | Nadie en fase A | Solo si se crea una skill nueva, con aprobación previa |
| `netfx-csharp`, `azure-infrastructure`, `serilog-setup`, `apply-nuget-instructions` | Nadie en fase A | No aplican al stack `net10.0` de esta fase |

La instrucción `wow-follow-up-question.instructions.md` exige declarar el nivel de certeza y preguntar si falta información. La documentación de esta fase se revisó con ese criterio; las decisiones abiertas (D-001, D-012, D-016) **no se inventan**.

## 5. Usar Spec Kit

Los comandos Spec Kit de este repositorio están en `.opencode/commands/`. Se ejecutan **desde OpenCode en la raíz del repositorio**:

| Comando | Uso recomendado |
| --- | --- |
| `/speckit.specify` | Nueva especificación (fase B o C, cuando se autorice) |
| `/speckit.clarify` | Cerrar ambigüedades de una spec antes de planificar |
| `/speckit.plan` | Generar `plan.md` y artefactos de diseño |
| `/speckit.tasks` | Generar `tasks.md` a partir del plan aprobado |
| `/speckit.analyze` | Comprobar consistencia entre spec, plan, contratos y tareas |
| `/speckit.checklist` | Listas de revisión de calidad de requisitos |
| `/speckit.implement` | Ejecutar tareas de un `tasks.md` aprobado |

**Importante para la fase A**: `spec.md`, `plan.md`, `tasks.md`, contratos y documentación ya existen y han sido revisados. No vuelvas a ejecutar `/speckit.tasks` ni `/speckit.implement` sobre esta rama sin acuerdo explícito: sobrescribiría el reparto DEV1–DEV5 y las correcciones aplicadas. Usa `/speckit.analyze` solo para verificar, y propón cambios por PR.

Regla de gobernanza: si cambias un requisito, actualiza primero `spec.md` y el contrato; después el plan y las tareas; y solo entonces el código. Ningún comando debe dar por ratificada ADR-0002 ni modificar la constitución.

## 6. Ejecutar una tarea asignada

1. Localiza el ID en `tasks.md` y confirma que el responsable `[DEVn]` es el tuyo.
2. Comprueba la rama indicada y la propiedad de archivos en `team-assignment.md` §4. Si el archivo es de otro DEV, coordinad un PR conjunto; no lo edites directamente.
3. Verifica dependencias y el marcador `[P]`. `[P]` significa archivos distintos y sin dependencias pendientes; `P0`–`P3` es prioridad.
4. Escribe primero la prueba correspondiente; debe fallar antes de la implementación.
5. Implementa solo lo descrito en la tarea. Sin valores de relleno, sin endpoints fuera del contrato, sin secretos y sin decisiones pendientes resueltas por defecto.
6. Ejecuta tus pruebas y actualiza la documentación de tu código en el mismo PR. Cada integrante responde por las pruebas y la documentación de lo que implementa.
7. Marca la casilla `- [x] Txxx` **solo en el PR de esa tarea**, nunca en la rama de otro compañero.
8. Usa mensajes de commit Conventional Commits con el ID de la tarea, por ejemplo `feat(g1): add PKCE flow tests (T025)`.

Comandos habituales:

```bash
dotnet test tests/G1.Identity.ContractTests
dotnet test tests/G1.Identity.CompatibilityTests
dotnet test tests/G1.Identity.E2E
npx --yes @redocly/cli@latest lint specs/001-g1-identidad-contrato-mock/contracts/openapi.v1.yaml
```

## 7. Flujo de pull request

```bash
git fetch origin
git rebase origin/001-g1-identidad-contrato-mock   # o merge, según acuerdo del equipo
git add .
git commit -m "feat(g1): descripción corta (Txxx)"
git push -u origin devN/rama
```

Abre el PR contra `001-g1-identidad-contrato-mock` (nunca contra `main` directamente desde una rama DEV). La descripción del PR debe indicar:

- ID de tarea y responsable.
- Archivos cambiados y por qué no solapan con otros DEV.
- Pruebas ejecutadas y resultado (incluidas las de tu propio código).
- Documentación actualizada (`docs/g1/...`, XML docs o README según corresponda).
- Requisitos/SC afectados y si el contrato cambió (si cambió, revisión de DEV1 y aviso a consumidores).
- Confirmación de que ADR-0002 sigue pendiente y de que no se modificó la constitución ni se resolvieron D-001/D-012/D-016.

Antes de pedir revisión, carga la skill `code-review` y aplica los hallazgos de gravedad alta. El revisor comprueba propiedad de archivos, pruebas, documentación y trazabilidad. Tras el merge, sincroniza tu rama:

```bash
git fetch origin
git switch 001-g1-identidad-contrato-mock
git pull --ff-only
git switch devN/rama
git rebase origin/001-g1-identidad-contrato-mock
```

## 8. Reglas de oro

- ADR-0002 pendiente: no implementar hasta su ratificación formal; no simular aprobación ni editar la constitución.
- El contrato `contracts/openapi.v1.yaml` es la fuente de verdad; un cambio incompatible exige `v2` y nota de migración.
- `mock/reset` no usa token en v1 y solo existe en `Development`/`Testing`; las claves `token.*` son escenarios, no personas.
- El único emisor de tokens es `/connect/token` con PKCE S256; la UI nunca guarda tokens.
- No se exponen contactos de terceros ni introspección en v1; las audiencias son provisionales (D-016).
- Cada desarrollador prueba y documenta su propio código; un PR sin pruebas o sin documentación se rechaza.
- Las fases B y C del documento de reparto son previsión, no tareas de la fase A.

## 9. Problemas frecuentes

| Síntoma | Causa probable | Acción |
| --- | --- | --- |
| OpenCode no carga `dotnet-api-architecture` | `opencode.jsonc` ausente o configuración de usuario que sobrescribe `skills` | Verificar el archivo en la raíz y volver a cargar la skill |
| Copilot no aplica una convención | Instrucción `.github/instructions/` no applicable al patrón del archivo | Revisar el `applyTo` de la instrucción; no duplicar reglas |
| Conflicto en `SessionEndpoints.cs` | T042 y T044 editan el mismo archivo | Secuenciar T042 antes de T044 dentro de DEV4 |
| Conflicto en `IdentityEndpoints.cs` | T036 y T037 editan el mismo archivo | Secuenciar T036 antes de T037 dentro de DEV3 |
| `@redocly/cli` avisa de `localhost` o 4XX/2XX en descubrimiento/JWKS/authorize | Avisos esperados del contrato de fase A | No son errores; revisar el resto de hallazgos |
| La revocación falla con `ExecuteUpdate` | Falta `DisableBulkOperations()` | Añadirlo en la configuración de OpenIddict (T010/T015) |
| El selector no aparece | Ya hay cookie de persona o `mock_persona` válido | Borrar cookies o usar ventana privada |

## 10. Referencias

- [`team-assignment.md`](./team-assignment.md) — reparto, ramas, propiedad de archivos y entregables.
- [`specs/001-g1-identidad-contrato-mock/tasks.md`](../../specs/001-g1-identidad-contrato-mock/tasks.md) — T001–T072 con responsable, prioridad y `[P]`.
- [`specs/001-g1-identidad-contrato-mock/quickstart.md`](../../specs/001-g1-identidad-contrato-mock/quickstart.md) — validación y cobertura SC.
- [`docs/decisions/pending-decisions.md`](../decisions/pending-decisions.md) — D-001 a D-017, con ADR-0002 pendiente.
- [Documentación de OpenCode V2](https://opencode.ai/v2/docs/) — configuración y skills.
