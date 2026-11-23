---
applyTo: '**/*.razor,**/*.razor.cs,**/*.razor.css,**/*.tsx,**/*.ts,**/*.jsx,**/*.css,**/*.scss,**/*.module.css,**/*.module.scss'
description: 'Design system transversal basado en Bootstrap 5 y el tema Atlant/MyAdmon. Define layout, componentes, tipografía, espaciado, iconografía, responsive y accesibilidad WCAG 2.1 AA para proyectos Blazor y React+TypeScript.'
---

# Design system frontend — Atlant / MyAdmon

Directrices obligatorias de diseño de interfaz para garantizar uniformidad visual en todas las aplicaciones frontend, independientemente del stack (Blazor o React+TypeScript). El tema visual Atlant y el tema MyAdmon son el mismo; esta instrucción los trata como sinónimos.

## Contexto del proyecto

- **Framework CSS**: Bootstrap 5.x
- **Tema visual**: MyAdmon / Atlant (sidebar oscuro, paleta naranja, Open Sans)
- **Stacks cubiertos**: Blazor (`.razor`, `.razor.css`) y React+TypeScript (`.tsx`, `.module.css`, `.module.scss`)
- **Iconografía**: Font Awesome 6 Free
- **Accesibilidad**: WCAG 2.1 nivel AA
- **Tokens de diseño**: Definidos en la skill `myadmon-theme`; consumidos mediante CSS custom properties (`var(--token)`)

## Relación con otras instrucciones

- `coding-style-css-theming.instructions.md`: Define la obligación general de usar CSS custom properties. Esta instrucción la **extiende** con reglas de layout, componentes y patrones concretos.
- `coding-style-blazor.instructions.md` y `coding-style-react-typescript.instructions.md`: Definen convenciones de código por stack. Esta instrucción añade las **reglas visuales y de diseño** que ambos deben seguir.
- Skill `myadmon-theme`: Proporciona los **valores exactos** de los tokens. Esta instrucción indica **cuándo y cómo** aplicarlos.

## Estructura de layout

Toda aplicación debe seguir el layout estándar Sidebar + Header + Content:

```text
┌────────────────────────────────────────────────┐
│ Header (40px, fijo arriba)                     │
├──────┬─────────────────────────────────────────┤
│ Side │                                         │
│ bar  │          Content area                   │
│      │          (fluido, scrollable)            │
│ 200px│                                         │
│  o   │                                         │
│ 50px │                                         │
└──────┴─────────────────────────────────────────┘
```

### Reglas de layout

- El **header** tiene altura fija de `var(--header-height)` (40px) y posición fija en la parte superior
- El **sidebar** es colapsable: `var(--sidebar-width-expanded)` (200px) expandido, `var(--sidebar-width-collapsed)` (50px) colapsado (solo iconos)
- El **content area** ocupa el espacio restante con scroll vertical propio
- El sidebar y el header usan `var(--color-secondary)` como fondo y `var(--color-text-inverse)` como texto
- El content area usa `var(--color-background)` como fondo
- Usar las clases de grid de Bootstrap 5 (`container-fluid`, `row`, `col-*`) para organizar el contenido dentro del content area
- No usar layout fijo con anchos en píxeles dentro del content area; usar siempre el grid de Bootstrap

### Sidebar colapsable

- Estado expandido: muestra icono + texto de cada ítem de navegación
- Estado colapsado: muestra solo el icono; el texto se oculta
- La transición entre estados usa `var(--transition-duration)` y `var(--transition-easing)`
- El ítem activo se resalta con `var(--sidebar-item-active)` (color primario naranja)
- El hover de ítems usa `var(--sidebar-item-hover)`

## Sistema de grid y breakpoints

Usar los breakpoints estándar de Bootstrap 5:

| Nombre | Prefijo | Mínimo |
| --- | --- | --- |
| Extra small | (ninguno) | < 576px |
| Small | `sm` | ≥ 576px |
| Medium | `md` | ≥ 768px |
| Large | `lg` | ≥ 992px |
| Extra large | `xl` | ≥ 1200px |
| Extra extra large | `xxl` | ≥ 1400px |

### Reglas responsive

- Usar `container-fluid` como contenedor principal del content area
- Usar las clases `col-{breakpoint}-{n}` para columnas responsive
- En pantallas `< md`, el sidebar se convierte en un overlay (off-canvas) que se cierra al navegar
- En pantallas `≥ md`, el sidebar es visible (colapsado o expandido según preferencia del usuario)
- Los formularios pasan de 2 columnas a 1 columna en breakpoint `< md`
- Las tablas de datos usan scroll horizontal en pantallas `< lg`

## Componentes UI

Usar los componentes nativos de Bootstrap 5 personalizados con los tokens del tema MyAdmon. No crear componentes custom cuando Bootstrap 5 ya proporciona uno equivalente.

### Botones

- Botón primario: clase `btn btn-primary`, usa `var(--color-primary)` como fondo
- Botón secundario: clase `btn btn-secondary`, usa `var(--color-secondary)` como fondo
- Botón de peligro: clase `btn btn-danger`, usa `var(--color-danger)` como fondo
- Botón outline: clase `btn btn-outline-primary` para acciones secundarias
- Tamaños: `btn-sm`, `btn` (default), `btn-lg`
- Los botones de acción principal llevan icono Font Awesome a la izquierda del texto

### Tarjetas y paneles

- Usar `card` de Bootstrap con `var(--shadow-md)` como sombra por defecto
- Fondo de tarjeta: `var(--color-surface)`
- Cabecera de tarjeta: `card-header` con fondo `var(--color-surface-alt)` y peso `var(--font-weight-semibold)`
- Borde: `var(--border-width) var(--border-style) var(--color-border-light)`
- Radio de borde: `var(--border-radius-lg)`
- Padding interno: `var(--spacing-md)`

### Formularios

- Usar las clases de formulario de Bootstrap 5: `form-control`, `form-select`, `form-check`, `form-label`
- Layouts de formulario: 1 columna (`col-12`), 2 columnas (`col-md-6`), o con tabs (`nav-tabs`)
- Etiquetas: peso `var(--font-weight-semibold)`, tamaño `var(--font-size-sm)`, color `var(--color-text-secondary)`
- Inputs con foco: sombra `var(--shadow-sm)` y borde `var(--color-primary)`
- Validación: usar las clases `is-valid`, `is-invalid` con `valid-feedback` e `invalid-feedback`
- Los campos obligatorios se marcan con asterisco rojo (`var(--color-danger)`)

### Tablas

- Usar `table` de Bootstrap con clases `table-striped` y `table-hover`
- Cabecera: fondo `var(--color-surface-alt)`, peso `var(--font-weight-semibold)`
- Filas alternas: fondo `var(--color-surface-alt)`
- Borde: `var(--color-border-light)`
- Para tablas de datos complejas, usar DataTables o equivalente con scroll horizontal en móvil

### Modales

- Usar `modal` de Bootstrap
- Fondo del overlay: `rgba(0, 0, 0, 0.5)`
- Radio de borde del modal: `var(--border-radius-lg)`
- Sombra: `var(--shadow-lg)`
- Cabecera con título en `var(--font-weight-bold)` y botón de cierre
- Footer con botones alineados a la derecha

### Alertas y notificaciones

- Usar `alert` de Bootstrap con las variantes semánticas: `alert-success`, `alert-warning`, `alert-danger`, `alert-info`
- Los colores de alerta corresponden a los tokens: `var(--color-success)`, `var(--color-warning)`, `var(--color-danger)`, `var(--color-info)`
- Las alertas descartables usan `alert-dismissible` con botón de cierre

### Badges y etiquetas

- Usar `badge` de Bootstrap
- Tamaño de fuente: `var(--font-size-xs)`
- Radio de borde: `var(--border-radius-sm)`

### Breadcrumbs

- Usar `breadcrumb` de Bootstrap
- Color del enlace activo: `var(--color-link)`
- Color del ítem actual: `var(--color-text-primary)`

### Pestañas (tabs)

- Usar `nav-tabs` de Bootstrap
- Pestaña activa: borde inferior con `var(--color-primary)`
- Texto de pestaña: `var(--color-text-secondary)`, activo `var(--color-primary)`

## Tipografía

Seguir la escala tipográfica definida en los tokens del tema MyAdmon:

| Elemento | Token de tamaño | Token de peso | Uso |
| --- | --- | --- | --- |
| h1 | `--font-size-2xl` | `--font-weight-bold` | Título principal de página |
| h2 | `--font-size-xl` | `--font-weight-bold` | Título de sección |
| h3 | `--font-size-lg` | `--font-weight-semibold` | Título de panel o tarjeta |
| h4 | `--font-size-md` | `--font-weight-semibold` | Subtítulo |
| h5 | `--font-size-sm` | `--font-weight-semibold` | Etiqueta destacada |
| h6 | `--font-size-xs` | `--font-weight-semibold` | Texto auxiliar destacado |
| body | `--font-size-md` | `--font-weight-normal` | Texto general |
| small | `--font-size-sm` | `--font-weight-normal` | Texto auxiliar |
| caption | `--font-size-xs` | `--font-weight-normal` | Leyendas, badges |

### Reglas tipográficas

- Fuente base: `var(--font-family-base)` (Open Sans)
- Interlineado general: `var(--line-height-base)` (1.5)
- Interlineado de encabezados: `var(--line-height-tight)` (1.25)
- No usar fuentes distintas a la base salvo para código monoespaciado
- No usar tamaños de fuente literales; consumir siempre `var(--font-size-*)`

## Espaciado

Usar la escala de espaciado del tema MyAdmon para paddings, margins y gaps:

| Token | Valor | Uso típico |
| --- | --- | --- |
| `--spacing-xs` | 0.25rem | Separación entre elementos inline, gap de badges |
| `--spacing-sm` | 0.5rem | Padding de badges, gaps pequeños entre botones |
| `--spacing-md` | 1rem | Padding de paneles, formularios, margins entre campos |
| `--spacing-lg` | 1.5rem | Separación entre secciones dentro de un panel |
| `--spacing-xl` | 2rem | Separación entre paneles o cards |
| `--spacing-2xl` | 3rem | Márgenes de layout a nivel de página |

### Reglas de espaciado

- Usar las utilidades de spacing de Bootstrap (`p-*`, `m-*`, `gap-*`) cuando mapeen a la escala del tema
- Para valores que no mapeen a utilidades de Bootstrap, usar `var(--spacing-*)` directamente en CSS
- El espaciado entre filas de formulario es `var(--spacing-md)`
- El padding interno de cards y paneles es `var(--spacing-md)`
- La separación entre cards en un grid es `var(--spacing-xl)`

## Iconografía

Usar Font Awesome 6 Free como única librería de iconos.

### Reglas de iconografía

- Usar las clases `fa-solid` (iconos sólidos) como opción por defecto
- Usar `fa-regular` (iconos outline) solo cuando se necesite diferenciar estado (activo/inactivo)
- No mezclar Font Awesome con otras librerías de iconos
- Los iconos de navegación del sidebar usan tamaño base (`1em`)
- Los iconos en botones preceden al texto con un margen de `var(--spacing-xs)`
- Los iconos decorativos llevan `aria-hidden="true"`
- Los iconos con significado funcional (sin texto acompañante) llevan `aria-label` o `title`

### Iconos comunes del design system

| Acción | Icono | Clase |
| --- | --- | --- |
| Crear / Añadir | plus | `fa-solid fa-plus` |
| Editar | pen-to-square | `fa-solid fa-pen-to-square` |
| Eliminar | trash-can | `fa-solid fa-trash-can` |
| Guardar | floppy-disk | `fa-solid fa-floppy-disk` |
| Buscar | magnifying-glass | `fa-solid fa-magnifying-glass` |
| Configuración | gear | `fa-solid fa-gear` |
| Usuario | user | `fa-solid fa-user` |
| Cerrar sesión | right-from-bracket | `fa-solid fa-right-from-bracket` |
| Menú hamburguesa | bars | `fa-solid fa-bars` |
| Colapsar sidebar | angles-left | `fa-solid fa-angles-left` |
| Expandir sidebar | angles-right | `fa-solid fa-angles-right` |
| Notificaciones | bell | `fa-solid fa-bell` |
| Éxito | circle-check | `fa-solid fa-circle-check` |
| Advertencia | triangle-exclamation | `fa-solid fa-triangle-exclamation` |
| Error | circle-xmark | `fa-solid fa-circle-xmark` |
| Información | circle-info | `fa-solid fa-circle-info` |

## Accesibilidad (WCAG 2.1 AA)

### Contraste

- Texto normal: ratio de contraste ≥ 4.5:1 respecto al fondo
- Texto grande (≥ 18pt o ≥ 14pt bold): ratio ≥ 3:1
- Elementos de interfaz (bordes de inputs, iconos funcionales): ratio ≥ 3:1
- No transmitir información solo mediante color; complementar con iconos, texto o patrones

### Foco

- Todo elemento interactivo debe tener un indicador de foco visible
- Usar el outline por defecto del navegador o un outline custom con contraste ≥ 3:1
- No eliminar el outline con `outline: none` sin proporcionar un indicador alternativo equivalente
- El orden del foco (tab order) debe seguir el orden visual lógico de lectura

### Navegación por teclado

- Todos los controles deben ser operables con teclado (Tab, Enter, Escape, flechas)
- Los modales atrapan el foco (focus trap) mientras están abiertos
- Al cerrar un modal, el foco vuelve al elemento que lo abrió
- Los menús dropdown y el sidebar son navegables con flechas arriba/abajo

### ARIA

- Usar landmarks: `<header>`, `<nav>`, `<main>`, `<aside>`, `<footer>`
- El sidebar usa `<nav aria-label="Navegación principal">`
- Los paneles colapsables usan `aria-expanded` y `aria-controls`
- Los formularios usan `aria-describedby` para mensajes de validación
- Las tablas de datos usan `<caption>` o `aria-label`
- Las alertas usan `role="alert"` para que los lectores de pantalla las anuncien

### Imágenes y multimedia

- Toda imagen informativa lleva `alt` descriptivo
- Las imágenes decorativas llevan `alt=""`
- Los controles de vídeo/audio son accesibles por teclado

## Patrones de dashboard

Las páginas de dashboard siguen el patrón visual del tema Atlant:

- **Widgets de KPI**: Cards con valor numérico grande (`--font-size-2xl`, `--font-weight-bold`), etiqueta descriptiva, e icono de contexto
- **Paneles de gráficos**: Card con título en `card-header` y área de gráfico en `card-body`
- **Tablas resumen**: Dentro de cards, con `table-striped table-hover`
- **Grid de widgets**: Usar `row` con `col-md-6 col-xl-3` para 4 widgets por fila en desktop

## Compatibilidad por stack

### Blazor

- Usar las clases de Bootstrap directamente en el markup Razor
- Los estilos aislados (`.razor.css`) consumen tokens via `var(--token)`
- Los componentes de layout (sidebar, header) se implementan como componentes Razor compartidos
- Usar `NavLink` con clases Bootstrap para la navegación del sidebar

### React + TypeScript

- Usar las clases de Bootstrap en los componentes JSX mediante `className`
- Los módulos CSS (`.module.css`) consumen tokens via `var(--token)`
- Los componentes de layout se implementan como componentes funcionales en una carpeta `layout/`
- Usar React Router con clases Bootstrap para la navegación del sidebar
