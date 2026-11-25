# Herramientas y skills de G1

- **Fecha**: 2026-11-24
- **Responsable**: DEV1
- **Rama**: `dev1/infra-contract`
- **Tareas**: T008 (verificación inicial); T059 completará el registro de excepciones al cierre de la fase A

Este documento deja evidencia de cómo se descubren y cargan las skills del equipo, y describe el procedimiento para repetir la comprobación. No modifica las skills ni las instrucciones de GitHub Copilot.

## Registro de skills

| Skill | Ubicación | Uso en Setup |
| --- | --- | --- |
| `dotnet-api-architecture` | `.github/skills/dotnet-api-architecture/SKILL.md` | T001 y T003: estructura de la solución y del host Minimal API |
| `nuget-package-manager` | `.github/skills/nuget-package-manager/SKILL.md` | T002 y T005: alta y fijación de paquetes con la CLI `dotnet` |
| `code-review` | `.github/skills/code-review/SKILL.md` | T060 y T064: revisiones de seguridad y del diff final |
| `microsoft-docs-and-code-consultant` | `.github/skills/microsoft-docs-and-code-consultant/SKILL.md` | T015, solo si el MCP de Microsoft Learn está disponible |

Las skills originales permanecen en `.github/skills/` y no se copian ni se editan. GitHub Copilot las descubre por su `name` y `description`; OpenCode las descubre desde la raíz del repositorio.

## Integración en OpenCode

`opencode.jsonc` registra el directorio de skills:

```jsonc
{
  "$schema": "https://opencode.ai/config.json",
  "skills": ["./.github/skills"],
}
```

La verificación se realizó con OpenCode v2.1.4. Se cargaron `dotnet-api-architecture` y `nuget-package-manager` mediante la herramienta de skills de la sesión; ambas devolvieron su contenido íntegro, incluida la base directory `.github/skills/<nombre>`. Esto confirma que el registro relativo `./.github/skills` se resuelve correctamente desde la raíz del repositorio.

## Integración en GitHub Copilot

Los directorios `.github/skills/` y `.github/instructions/`, junto con `.github/copilot-instructions.md`, no presentan cambios en `git status --porcelain`. Copilot conserva por tanto el descubrimiento automático de skills y la aplicación de instrucciones por `applyTo`.

## Procedimiento de verificación

Para repetir la comprobación desde la raíz del repositorio:

```bash
# 1. Confirmar el registro de skills en OpenCode.
cat opencode.jsonc

# 2. Listar las skills descubiertas y comprobar que no hay cambios locales.
find .github/skills -maxdepth 1 -mindepth 1 -type d -printf '%f\n' | sort
git status --porcelain -- .github/skills .github/instructions .github/copilot-instructions.md

# 3. Cargar una skill desde la sesión de OpenCode y confirmar su contenido.
#    La herramienta de skills debe devolver el SKILL.md y su base directory.
```

El resultado esperado del paso 2 es una salida vacía en `git status`: cualquier modificación de las skills o instrucciones debe revisarse antes de continuar.

## Skill ausente

Las instrucciones `coding-style-css-theming.instructions.md` y `coding-style-frontend-design-system.instructions.md` citan `myadmon-theme`, que no existe en `.github/skills/`. La comprobación confirma su ausencia. Mientras no se aporte la skill, la T029 define tokens locales concretos y accesibles; no se inventa una skill equivalente ni se modifica la instrucción.

## Excepciones pendientes de registro formal

- La guía `dotnet-api-architecture` prefiere Scalar, pero la constitución y el mandato del usuario exigen Swagger UI con `Swashbuckle.AspNetCore.SwaggerUI`. Se aplica Swagger UI y la excepción se registrará formalmente en T059.
- EF Core InMemory incumple la restricción técnica 4 de la constitución. ADR-0002 fue ratificada por equipo G1 el 2026-11-25 como excepción académica y ya no bloquea Phase 2 dentro de ese alcance. La persistencia relacional y el plan de migración siguen pendientes antes de datos reales o producción.

## Resultado

| Comprobación | Resultado |
| --- | --- |
| `opencode.jsonc` registra `.github/skills/` | Correcto |
| `dotnet-api-architecture` cargada en OpenCode v2.1.4 | Correcto |
| `nuget-package-manager` cargada en OpenCode v2.1.4 | Correcto |
| `.github/skills/` y `.github/instructions/` sin cambios | Correcto |
| `myadmon-theme` ausente y documentado | Correcto |
