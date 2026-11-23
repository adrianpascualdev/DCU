---
applyTo: "**"
description: "Aplica estas instrucciones solo al generar mensajes de commit para cambios de código fuente y control de configuración. Ignóralas en cualquier otro contexto. Sigue la especificación 'Conventional Commits' para mantener historiales legibles y consistentes, y para respaldar la generación automática de changelogs y los flujos de integración continua."
---

# Directrices de Conventional Commits

Directrices para redactar mensajes de commit siguiendo la especificación [Conventional Commits](https://www.conventionalcommits.org/) con el fin de mantener un historial legible, automatizar la generación de changelogs y facilitar la integración continua.

## Contexto del proyecto

- **Alcance**: Todos los commits del proyecto, incluidos los commits automatizados generados por GitHub Copilot u otras herramientas
- **Idioma**: Todos los mensajes de commit deben escribirse en inglés

## Formato del mensaje de commit

El mensaje de commit debe estructurarse así:

```text
<type> - [optional scope]: <description>
```

### Componentes

- **type**: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`, `revert`
- **scope** (opcional): Parte del código afectada, por ejemplo `api`, `domain`, `infrastructure` o `tests`
- **description**: Descripción breve en imperativo, sin mayúscula inicial y sin punto final
- **Límite de caracteres**: La primera línea no debe superar los 72 caracteres

### Ejemplos

- `feat - api: add order endpoint`
- `fix - (domain): correct order validation logic`
- `test - (order): add unit tests for order creation`
- `chore: update dependencies`

## Buenas prácticas

- **Un commit = un cambio lógico**: Cada commit debe representar una única unidad de trabajo atómica.
- **Usa `scope`**: Especifica la capa o funcionalidad afectada para aportar contexto.
- **Breaking changes**: Añade `!` después del tipo o del scope y detalla el cambio en el cuerpo del commit.

## Referencias

- [Conventional Commits v1.0.0](https://www.conventionalcommits.org/)
