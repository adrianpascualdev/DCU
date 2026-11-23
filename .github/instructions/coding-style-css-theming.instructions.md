---
applyTo: '**/*.css,**/*.scss,**/*.module.css,**/*.module.scss,**/*.razor.css,**/*.tsx,**/*.jsx,**/*.razor,**/*.razor.cs'
description: 'Convenciones de theming transversal para aplicaciones web. Obliga al uso de CSS custom properties (design tokens) en lugar de valores literales para garantizar consistencia visual y soporte de temas.'
---

# Directrices de theming para aplicaciones web

Convenciones para garantizar que todas las aplicaciones web con frontal soporten temas de estilo mediante CSS custom properties (design tokens), independientemente del stack frontend utilizado (Blazor, React+TypeScript, jQuery/jQuery UI).

## Contexto del proyecto

- **Alcance**: Toda aplicación con interfaz web debe consumir temas de estilo
- **Mecanismo**: CSS custom properties (`var(--token-name)`) como canal único de personalización visual
- **Stacks cubiertos**: Blazor (`.razor.css`), React+TypeScript (`.module.css`, `.module.scss`), jQuery/jQuery UI (`.css`)
- **Estado actual**: Los valores concretos de los design tokens se encuentran en la skill `myadmon-theme`

## Requisito obligatorio

Toda aplicación web con frontal **debe** estar preparada para funcionar con temas de estilo. El código de presentación no debe contener valores visuales literales; debe consumir tokens del tema a través de CSS custom properties.

## Uso de CSS custom properties

- Usar `var(--token-name)` para colores, espaciados, tipografía, sombras, bordes y radios
- Proporcionar un valor de respaldo cuando sea crítico para la funcionalidad: `var(--token-name, <fallback>)`
- No usar valores literales de color (`#fff`, `rgb(...)`, `red`), tamaño (`16px`, `1.5rem`) ni tipografía (`Arial`) directamente en componentes
- Declarar los tokens del tema en el nivel `:root` o en la capa de tema correspondiente

## Convención de nombres para tokens

Usar una estructura jerárquica con guiones dobles como prefijo CSS y guiones simples como separador semántico:

| Categoría | Patrón | Ejemplo |
| --- | --- | --- |
| Colores | `--color-{rol}` | `--color-primary`, `--color-surface` |
| Tipografía | `--font-{propiedad}` | `--font-family-base`, `--font-size-md` |
| Espaciado | `--spacing-{escala}` | `--spacing-sm`, `--spacing-lg` |
| Bordes | `--border-{propiedad}` | `--border-radius-md`, `--border-color` |
| Sombras | `--shadow-{nivel}` | `--shadow-sm`, `--shadow-lg` |
| Transiciones | `--transition-{propiedad}` | `--transition-duration`, `--transition-easing` |

## Soporte para múltiples temas

- El sistema debe contemplar múltiples temas (claro, oscuro, personalizados)
- Usar selectores de atributo (`[data-theme="dark"]`) o clases (`.theme-dark`) en el elemento raíz para alternar temas
- No asumir un único tema; diseñar los tokens para que sean intercambiables

## Compatibilidad por stack

### Blazor (`.razor.css`)

- Usar CSS custom properties dentro de los estilos aislados del componente
- No definir valores literales en estilos aislados; consumir siempre tokens del tema

### React + TypeScript (`.module.css`, `.module.scss`)

- Usar CSS custom properties dentro de los módulos CSS/SCSS
- No definir valores literales en módulos; consumir siempre tokens del tema

### jQuery / jQuery UI (`.css`)

- Usar CSS custom properties en las hojas de estilo
- Respetar la integración con las clases de tematización de jQuery UI (`ui-widget`, `ui-state-*`)

## Valores prohibidos en componentes

- Colores literales: `#ffffff`, `rgb(0, 0, 0)`, `hsl(...)`, `red`, `blue`
- Tamaños literales de fuente o espaciado sin justificación: `14px`, `2rem`, `24px`
- Familias tipográficas literales: `'Segoe UI'`, `Arial`, `sans-serif`
- Sombras literales: `0 2px 4px rgba(0,0,0,0.1)`

En su lugar, usar siempre el token correspondiente del tema.
