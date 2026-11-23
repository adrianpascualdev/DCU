---
name: skill-maker
description: 'Crea nuevas Agent Skills para GitHub Copilot a partir de prompts o duplicando esta plantilla. Úsala cuando te pidan crear una skill, preparar la base de una skill o construir capacidades especializadas con recursos empaquetados. Genera archivos SKILL.md con frontmatter correcto, estructura de directorios y carpetas opcionales como scripts, references o assets.'
---

# Plantilla para crear skills

Una meta-skill para crear nuevas Agent Skills. Úsala cuando necesites preparar la estructura de una nueva skill, generar un archivo `SKILL.md` o ayudar a entender la especificación de Agent Skills.

## Cuándo usar esta skill

- La persona usuaria pide "create a skill", "make a new skill" o "scaffold a skill"
- La persona usuaria quiere añadir una capacidad especializada a su configuración de GitHub Copilot
- La persona usuaria necesita ayuda para estructurar una skill con recursos empaquetados
- La persona usuaria quiere duplicar esta plantilla como punto de partida

## Requisitos previos

- Entender qué debe lograr la skill
- Contar con una descripción clara, rica en palabras clave y orientada a capacidades y disparadores
- Saber qué recursos empaquetados se necesitan, como scripts, referencias, assets o plantillas

## Cómo crear una nueva skill

### Paso 1: crea el directorio de la skill

Crea una carpeta nueva con un nombre en minúsculas y separado por guiones:

```text
skills/<skill-name>/
└── SKILL.md          # Required
```

### Paso 2: genera `SKILL.md` con frontmatter

Toda skill necesita frontmatter YAML con `name` y `description`:

```yaml
---
name: <skill-name>
description: '<What it does>. Use when <specific triggers, scenarios, keywords users might say>.'
---
```

#### Requisitos de los campos del frontmatter

| Campo | Obligatorio | Restricciones |
| --- | --- | --- |
| `name` | **Sí** | Entre 1 y 64 caracteres, solo minúsculas, números o guiones y debe coincidir con el nombre de la carpeta |
| `description` | **Sí** | Entre 1 y 1024 caracteres y debe describir QUÉ hace y CUÁNDO usarla |
| `license` | No | Nombre de la licencia o referencia a un `LICENSE.txt` incluido |
| `compatibility` | No | Entre 1 y 500 caracteres con requisitos de entorno cuando sean necesarios |
| `metadata` | No | Pares clave-valor para propiedades adicionales |
| `allowed-tools` | No | Lista separada por espacios de herramientas preaprobadas, experimental |

#### Buenas prácticas para `description`

**CRÍTICO**: `description` es el mecanismo PRINCIPAL para el descubrimiento automático de skills. Incluye:

1. **QUÉ** hace la skill, es decir, sus capacidades
2. **CUÁNDO** usarla, es decir, disparadores, escenarios o tipos de archivo
3. **Palabras clave** que la persona usuaria podría mencionar en los prompts

**Ejemplo bueno:**

```yaml
description: 'Toolkit for testing local web applications using Playwright. Use when asked to verify frontend functionality, debug UI behavior, capture browser screenshots, or view browser console logs. Supports Chrome, Firefox, and WebKit.'
```

**Ejemplo malo:**

```yaml
description: 'Web testing helpers'
```

### Paso 3: redacta el cuerpo de la skill

Después del frontmatter, añade instrucciones en Markdown. Las secciones recomendadas son:

| Sección | Propósito |
| --- | --- |
| `# Título` | Descripción breve |
| `## Cuándo usar esta skill` | Refuerza los disparadores de la descripción |
| `## Requisitos previos` | Herramientas y dependencias necesarias |
| `## Flujos paso a paso` | Pasos numerados para las tareas |
| `## Resolución de problemas` | Problemas habituales y soluciones |
| `## Referencias` | Enlaces a documentación incluida |

### Paso 4: añade directorios opcionales si hacen falta

| Carpeta | Propósito | Cuándo usarla |
| --- | --- | --- |
| `scripts/` | Código ejecutable, Python, Bash o JS | Automatización que realiza operaciones |
| `references/` | Documentación que el agente lee | Referencias de API, esquemas o guías |
| `assets/` | Archivos estáticos usados tal cual | Imágenes, fuentes o plantillas |
| `templates/` | Código base que el agente modifica | Scaffolds para extender |

## Ejemplo: estructura completa de una skill

```text
my-awesome-skill/
├── SKILL.md                    # Instrucciones obligatorias
├── LICENSE.txt                 # Archivo de licencia opcional
├── scripts/
│   └── helper.py               # Automatización ejecutable
├── references/
│   ├── api-reference.md        # Documentación detallada
│   └── examples.md             # Ejemplos de uso
├── assets/
│   └── diagram.png             # Recursos estáticos
└── templates/
    └── starter.ts              # Base de código
```

## Inicio rápido: duplica esta plantilla

1. Copia la carpeta `make-skill-template/`.
2. Renómbrala con el nombre de tu skill, en minúsculas y con guiones.
3. Actualiza `SKILL.md`:
   - Cambia `name:` para que coincida con el nombre de la carpeta.
   - Redacta una `description:` rica en palabras clave.
   - Sustituye el contenido del cuerpo por tus instrucciones.
   - Añade los campos extra de frontmatter que necesites o te hayan pedido.
4. Añade los recursos empaquetados que hagan falta.
5. Ejecuta `npm install` dentro de la carpeta `scripts/` para instalar dependencias antes de ejecutar scripts.
6. Valida con `npm run skill:validate` desde la carpeta `scripts/`.

## Lista de validación

- [ ] El nombre de la carpeta está en minúsculas y usa guiones
- [ ] El campo `name` coincide exactamente con el nombre de la carpeta
- [ ] `description` tiene entre 10 y 1024 caracteres
- [ ] `description` explica QUÉ hace y CUÁNDO usarla
- [ ] `description` está envuelta en comillas simples
- [ ] El cuerpo del documento tiene menos de 500 líneas
- [ ] Cada asset empaquetado ocupa menos de 5 MB

## Resolución de problemas

| Problema | Solución |
| --- | --- |
| La skill no se detecta | Mejora `description` con más palabras clave y disparadores |
| La validación falla en `name` | Asegura minúsculas, sin guiones consecutivos y que coincida con la carpeta |
| `description` es demasiado corta | Añade capacidades, disparadores y palabras clave |
| No se encuentran los assets | Usa rutas relativas desde la raíz de la skill |

## Referencias

- Especificación oficial de Agent Skills: <https://agentskills.io/specification>
