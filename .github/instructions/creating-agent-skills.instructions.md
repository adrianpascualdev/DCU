---
applyTo: '**/skills/**/SKILL.md'
description: 'Directrices para crear Agent Skills de alta calidad para GitHub Copilot'
---

# Directrices para archivos de Agent Skills

Instrucciones para crear Agent Skills eficaces y portables que mejoran GitHub Copilot con capacidades especializadas, flujos de trabajo y recursos empaquetados.

## ¿Qué son los Agent Skills?

Los Agent Skills son carpetas autónomas con instrucciones y recursos empaquetados que enseñan a los agentes de IA capacidades especializadas. A diferencia de las instrucciones personalizadas (que definen estándares de codificación), las skills habilitan flujos de trabajo específicos de tareas que pueden incluir scripts, ejemplos, plantillas y datos de referencia.

Características clave:

- **Portables**: funcionan en VS Code, Copilot CLI y el agente de codificación de Copilot
- **Carga progresiva**: solo se cargan cuando son relevantes para la solicitud del usuario
- **Recursos empaquetados**: pueden incluir scripts, plantillas y ejemplos junto a las instrucciones
- **Bajo demanda**: se activan automáticamente según la relevancia del prompt

## Estructura de directorios

Las skills se almacenan en ubicaciones específicas:

| Ubicación | Ámbito | Recomendación |
| --- | --- | --- |
| `.github/skills/<skill-name>/` | Proyecto/repositorio | Recomendado para skills de proyecto |
| `.claude/skills/<skill-name>/` | Proyecto/repositorio | Legado, por compatibilidad hacia atrás |
| `~/.github/skills/<skill-name>/` | Personal (usuario) | Recomendado para skills personales |
| `~/.claude/skills/<skill-name>/` | Personal (usuario) | Legado, por compatibilidad hacia atrás |

Cada skill **debe** tener su propio subdirectorio que contenga como mínimo un archivo `SKILL.md`.

## Formato requerido de SKILL.md

### Frontmatter (obligatorio)

```yaml
---
name: webapp-testing
description: 'Toolkit for testing local web applications using Playwright. Use when asked to verify frontend functionality, debug UI behavior, capture browser screenshots, check for visual regressions, or view browser console logs. Supports Chrome, Firefox, and WebKit browsers.'
license: Complete terms in LICENSE.txt
---
```

| Campo | Obligatorio | Restricciones |
| --- | --- | --- |
| `name` | Sí | Minúsculas, guiones en lugar de espacios, máx. 64 caracteres (p. ej., `webapp-testing`) |
| `description` | Sí | 10-1024 caracteres, capacidades Y casos de uso claros, entre comillas simples |
| `license` | No | Referencia a LICENSE.txt (p. ej., `Complete terms in LICENSE.txt`) o identificador SPDX |

### Buenas prácticas para la descripción

**CRÍTICO**: El campo `description` es el mecanismo PRINCIPAL para el descubrimiento automático de skills. Copilot lee ÚNICAMENTE el `name` y la `description` para decidir si cargar una skill. Si la descripción es vaga, la skill nunca se activará.

**Qué incluir en la descripción:**

1. **QUÉ** hace la skill (capacidades)
2. **CUÁNDO** usarla (disparadores concretos, escenarios, tipos de archivos o solicitudes del usuario)
3. **Palabras clave** que los usuarios podrían mencionar en sus prompts

**Descripción correcta:**

```yaml
description: 'Toolkit for testing local web applications using Playwright. Use when asked to verify frontend functionality, debug UI behavior, capture browser screenshots, check for visual regressions, or view browser console logs. Supports Chrome, Firefox, and WebKit browsers.'
```

**Descripción incorrecta:**

```yaml
description: 'Web testing helpers'
```

La descripción incorrecta falla porque:

- Sin disparadores específicos (¿cuándo debería Copilot cargar esta skill?)
- Sin palabras clave (¿qué prompts de usuario coincidirían?)
- Sin capacidades (¿qué puede hacer realmente?)

### Contenido del cuerpo

El cuerpo contiene instrucciones detalladas que Copilot carga DESPUÉS de activar la skill. Secciones recomendadas:

| Sección | Propósito |
| --- | --- |
| `# Title` | Descripción breve de lo que habilita esta skill |
| `## Cuándo usar esta skill` | Lista de escenarios que refuerza los disparadores de la descripción |
| `## Prerequisites` | Herramientas, dependencias y configuración de entorno requeridas (si aplica) |
| `## Flujos de trabajo paso a paso` | Pasos numerados para procedimientos repetibles (build, despliegue, configuración) |
| `## Gotchas` | Advertencias proactivas sobre comportamientos no obvios ("nunca hagas X porque Y") |
| `## Troubleshooting` | Soluciones reactivas a problemas conocidos ("si ves X, prueba Y") |
| `## References` | Enlaces a documentación empaquetada o recursos externos |

No toda skill necesita todas las secciones. Omitir `## Prerequisites` si no hay dependencias externas. Omitir `## Flujos de trabajo paso a paso` si la skill es meramente consultiva. Incluir `## Gotchas` siempre que la skill involucre herramientas externas, APIs o comportamiento específico de plataforma.

Para los principios de calidad del contenido (qué incluir y qué omitir), ver [Escribir skills de alto impacto](#escribir-skills-de-alto-impacto) más adelante.

### Redactar cada sección

**`# Title`** - Una frase que indica qué habilita la skill. Evitar redacciones genéricas; ser específico sobre el dominio.

**`## Cuándo usar esta skill`** - Lista con viñetas de escenarios concretos que refuerzan los disparadores de la descripción. Ayuda a Copilot a confirmar que cargó la skill correcta.

```markdown
## Cuándo usar esta skill

- La persona usuaria pide probar una aplicación web en un navegador
- La persona usuaria necesita capturar pantallas para pruebas de regresión visual
- La persona usuaria quiere depurar el comportamiento frontend con logs de consola del navegador
```

**`## Prerequisites`** - Incluir solo si la skill requiere herramientas, servicios o configuración que Copilot no puede asumir disponibles. Listar los comandos de instalación exactos.

```markdown
## Prerequisites

- [Playwright](https://playwright.dev/) installed: `npm install -D @playwright/test`
- At least one browser engine installed: `npx playwright install chromium`
```

**`## Flujos de trabajo paso a paso`** - Pasos numerados para procedimientos repetibles donde la secuencia importa, como build, despliegue o configuración de entorno. Describir QUÉ lograr en cada etapa, sin rutas de archivo o números de línea fijos; los pasos deben ser adaptables a distintas estructuras de proyecto. Para flujos complejos, más de 5 pasos, dividir en archivos `references/` y enlazarlos.

```markdown
## Flujos de trabajo paso a paso

### Deploy to Staging

1. Build the project: `npm run build`
2. Run pre-deploy validation: `npm run validate`
3. Deploy to staging: `npm run deploy -- --env staging`
4. Verify the health endpoint returns 200
```

**`## Gotchas`** - Advertencias proactivas que previenen errores. Documentar valores por defecto no obvios, peculiaridades de API, comportamiento específico de versión y trampas comunes. Poner en negrita la restricción clave y luego explicar el motivo.

```markdown
## Gotchas

- **Never** call `billing.charge()` without checking `user.hasPaymentMethod` first -
  the SDK throws an unrecoverable error instead of returning a failure.
- The `currency` field expects ISO 4217 codes, not display names.
  Copilot often writes "dollars" instead of "USD".
```

**`## Troubleshooting`** - Soluciones reactivas a problemas conocidos, presentadas como tabla de síntoma -> solución. Cada fila debe ser autónoma y accionable.

```markdown
## Troubleshooting

| Issue | Solution |
| ------- | ---------- |
| Plugin won't connect | Check servers are running (`npm run start:all`) |
| Browser blocks localhost | Allow local network access, or try a different browser |
| Tool execution times out | Ensure the plugin UI is open and shows "Connected" |
```

**`## References`** - Enlaces a documentación empaquetada en `references/`, documentación externa o skills relacionadas. Usar rutas relativas para archivos empaquetados.

## Empaquetado de recursos

Las skills pueden incluir archivos adicionales a los que Copilot accede bajo demanda:

### Tipos de recursos admitidos

| Carpeta | Propósito | ¿Se carga en contexto? | Archivos de ejemplo |
| --- | --- | --- | --- |
| `scripts/` | Automatización ejecutable que realiza operaciones concretas | Al ejecutarse | `helper.py`, `validate.sh`, `build.ts` |
| `references/` | Documentación que el agente de IA lee para tomar decisiones | Sí, cuando se referencia | `api_reference.md`, `schema.md`, `workflow_guide.md` |
| `assets/` | **Archivos estáticos usados TAL CUAL** en la salida (no los modifica el agente) | No | `logo.png`, `brand-template.pptx`, `custom-font.ttf` |
| `templates/` | **Código inicial/scaffolds que el agente de IA MODIFICA** y sobre los que construye | Sí, cuando se referencia | `viewer.html` (insertar algoritmo), `hello-world/` (extender) |

### Ejemplo de estructura de directorios

```text
.github/skills/my-skill/
|- SKILL.md              # Obligatorio: instrucciones principales
|- LICENSE.txt           # Recomendado: términos de licencia (Apache 2.0 habitual)
|- scripts/              # Opcional: automatización ejecutable
|  |- helper.py          # Script Python
|  \- helper.ps1         # Script PowerShell
|- references/           # Opcional: documentación cargada en contexto
|  |- api_reference.md
|  |- workflow-setup.md      # Flujo detallado (>5 pasos)
|  \- workflow-deployment.md
|- assets/               # Opcional: archivos estáticos usados tal cual en la salida
|  |- baseline.png       # Imagen de referencia para comparación
|  \- report-template.html
\- templates/            # Opcional: código inicial que el agente modifica
   |- scaffold.py        # Scaffold que el agente personaliza
   \- config.template    # Plantilla de configuración que el agente completa
```

> **LICENSE.txt**: al crear una skill, descargar el [texto de la licencia Apache 2.0](https://www.apache.org/licenses/LICENSE-2.0.txt) y guardarlo como `LICENSE.txt`. Actualizar el año y propietario del copyright en la sección de apéndice.

### Distinción clave: assets frente a templates

Los **assets** son recursos estáticos **consumidos sin modificación** en la salida:

- Un `logo.png` que se incrusta en un documento generado
- Un `report-template.html` copiado como formato de salida
- Un `custom-font.ttf` aplicado al renderizado de texto

Las **templates** son código inicial/scaffolds que **el agente de IA modifica activamente**:

- Un `scaffold.py` donde el agente inserta lógica
- Un `config.template` donde el agente rellena valores según los requisitos del usuario
- Un directorio de proyecto `hello-world/` que el agente extiende con nuevas funcionalidades

**Regla general**: si el agente lee y construye sobre el contenido del archivo -> `templates/`. Si el archivo se usa tal cual en la salida -> `assets/`.

### Referenciar recursos en SKILL.md

Usar rutas relativas para referenciar archivos dentro del directorio de la skill:

```markdown
## Available Scripts

Run the [helper script](./scripts/helper.py) to automate common tasks.

See [API reference](./references/api_reference.md) for detailed documentation.

Use the [scaffold](./templates/scaffold.py) as a starting point.
```

## Arquitectura de carga progresiva

Las skills usan carga en tres niveles para mayor eficiencia:

| Nivel | Qué se carga | Cuándo |
| --- | --- | --- |
| 1. Descubrimiento | Solo `name` y `description` | Siempre (metadatos ligeros) |
| 2. Instrucciones | Cuerpo completo de `SKILL.md` | Cuando la solicitud coincide con la descripción |
| 3. Recursos | Scripts, ejemplos, documentación | Solo cuando Copilot los referencia |

Esto significa:

- Se pueden instalar muchas skills sin consumir contexto
- Solo el contenido relevante se carga por tarea
- Los recursos no se cargan hasta que se necesiten explícitamente

## Directrices de contenido

### Directivas de herramienta explícitas

Los Agent Skills son ejecutados por asistentes de codificación de IA que interactúan con el workspace mediante herramientas. Mapear las acciones abstractas a usos concretos de herramientas para que el asistente sepa exactamente *cómo* ejecutar la skill:

- Indicar al agente que use la herramienta `read_file` al referenciar archivos de `templates/` o `references/` para cargarlos en contexto.
- Indicar al agente que use la herramienta `run_in_terminal` al ejecutar scripts empaquetados o comandos de despliegue, en lugar de mostrar el comando al usuario.
- Para procedimientos complejos de múltiples pasos, instruir al agente para que planifique y realice el seguimiento de su progreso (usando herramientas como `manage_todo_list` si está disponible) para que no omita ni alucine pasos.

### Estilo de redacción

- Usar modo imperativo: "Ejecutar", "Crear", "Configurar" (no "Deberías ejecutar")
- Ser específico y accionable
- Incluir comandos exactos con parámetros
- Mostrar salidas esperadas donde sea útil
- Mantener las secciones enfocadas y escaneables

### Requisitos de scripts

Al incluir scripts, preferir lenguajes multiplataforma:

| Lenguaje | Caso de uso |
| --- | --- |
| Python | Automatización compleja, procesamiento de datos |
| pwsh | Scripts PowerShell Core |
| Node.js | Tooling basado en JavaScript |
| Bash/Shell | Tareas de automatización simples |

Buenas prácticas:

- Incluir documentación de ayuda/uso (flag `--help`)
- Gestionar errores de forma adecuada con mensajes claros
- No almacenar credenciales ni secretos
- Usar rutas relativas siempre que sea posible

### Cuándo empaquetar scripts

Incluir scripts en la skill cuando:

- El mismo código sería reescrito repetidamente por el agente
- La fiabilidad determinista es crítica (p. ej., manipulación de archivos, llamadas a API)
- La lógica compleja se beneficia de estar pre-probada en lugar de generada cada vez
- La operación tiene un propósito autónomo que puede evolucionar de forma independiente
- La testeabilidad importa: los scripts se pueden probar unitariamente y validar
- Se prefiere comportamiento predecible sobre generación dinámica

Los scripts facilitan la evolución: incluso operaciones simples se benefician de implementarse como scripts cuando pueden crecer en complejidad, necesitar comportamiento consistente entre invocaciones o requerir extensibilidad futura.

### Consideraciones de seguridad

- Los scripts dependen de los helpers de credenciales existentes (sin almacenamiento de credenciales)
- Incluir flags `--force` solo para operaciones destructivas
- Advertir a los usuarios antes de acciones irreversibles
- Documentar cualquier operación de red o llamada externa

## Escribir skills de alto impacto

### Centrarse en lo que Copilot no sabe

No incluir información que Copilot ya conoce de sus datos de entrenamiento: sintaxis estándar del lenguaje, uso común de librerías o comportamiento de API bien documentado. Cada línea de una skill debe enseñar algo que Copilot de otro modo haría mal u omitiría. Si la información está en la primera página de la documentación oficial, omitirla. Centrarse en convenciones internas, valores por defecto no obvios, peculiaridades específicas de versión y flujos de trabajo específicos del dominio que cambien el comportamiento de Copilot.

### Conciencia del presupuesto de contexto

Todas las descripciones de skills comparten una porción limitada de la ventana de contexto disponible durante el descubrimiento. La descripción compite con cada otra skill instalada por la atención de Copilot. Mantener las descripciones concisas y densas en palabras clave: apuntar al texto más corto que aun así comunique QUÉ, CUÁNDO y las PALABRAS CLAVE relevantes. Las descripciones verbosas no solo desperdician el presupuesto propio; reducen la visibilidad de todas las demás skills del sistema.

### Los Gotchas son el contenido de mayor señal

La sección `## Gotchas` es consistentemente la parte más valiosa de cualquier skill: advertencias proactivas que previenen errores antes de que ocurran. Esto es distinto de `## Troubleshooting`, que proporciona correcciones reactivas después de que algo salga mal. Tratar los gotchas como una sección viva: cada vez que Copilot produzca un resultado incorrecto, añadir un gotcha. Poner en negrita la restricción clave y luego explicar el motivo (p. ej., "**Never** call `X()` without checking `Y` first - the SDK throws an unrecoverable error").

### Preferir directrices flexibles sobre pasos rígidos

Usar pasos numerados solo para procedimientos concretos y repetibles (build, despliegue, configuración de entorno) donde la secuencia realmente importa. Para tareas abiertas (depuración, refactorización, revisión de código), proporcionar criterios de decisión e información de referencia en su lugar: Copilot necesita flexibilidad para adaptarse a la situación específica del usuario.

```markdown
# ❌ Demasiado rígido
1. Open the file at src/api/handlers.ts
2. Find the function named processOrder
3. Add a try-catch block around lines 45-60

# ✅ Flexible
When fixing error handling in API handlers:
- Ensure all database operations have proper error handling
- Use the project's ErrorHandler utility (see ./references/error-handling.md)
- Log errors with enough context to debug in production
```

### Usar divulgación progresiva para skills grandes

Si el `SKILL.md` supera ~200 líneas, considerar dividir el contenido detallado en subdirectorios. Esto reduce el consumo de contexto: Copilot carga solo las instrucciones principales inicialmente y obtiene el material de referencia bajo demanda.

```markdown
## Archivos de referencia

- `references/api.md` - firmas completas de funciones y tipos de retorno
- `references/error-codes.md` - todos los códigos de error que puede devolver este servicio
- `scripts/validate.sh` - ejecútalo tras los cambios para verificar la corrección

Lee estos archivos según lo necesites para tu tarea actual. No los leas todos por adelantado.
```

## Patrones comunes

### Patrón de tabla de parámetros

Documentar los parámetros con claridad:

```markdown
| Parameter | Required | Default | Description |
| ----------- | ---------- | --------- | ------------- |
| `--input` | Yes | - | Input file or URL to process |
| `--action` | Yes | - | Action to perform |
| `--verbose` | No | `false` | Enable verbose output |
```

### Patrón de ejecución de flujo de trabajo

Al ejecutar flujos de trabajo de múltiples pasos, crear una lista TODO donde cada paso referencia la documentación relevante:

```markdown
## TODO
- [ ] Step 1: Configure environment - see [workflow-setup.md](./references/workflow-setup.md#environment)
- [ ] Step 2: Build project - see [workflow-setup.md](./references/workflow-setup.md#build)
- [ ] Step 3: Deploy to staging - see [workflow-deployment.md](./references/workflow-deployment.md#staging)
- [ ] Step 4: Run validation - see [workflow-deployment.md](./references/workflow-deployment.md#validation)
- [ ] Step 5: Deploy to production - see [workflow-deployment.md](./references/workflow-deployment.md#production)
```

Esto garantiza la trazabilidad y permite reanudar los flujos de trabajo si se interrumpen.

## Lista de verificación de validación

Antes de publicar una skill:

- [ ] `SKILL.md` tiene frontmatter válido con `name` y `description`
- [ ] `name` está en minúsculas con guiones, <=64 caracteres
- [ ] `description` indica claramente **QUÉ** hace, **CUÁNDO** usarla y las **PALABRAS CLAVE** relevantes
- [ ] `description` es concisa y densa en palabras clave (respeta el presupuesto de contexto)
- [ ] El cuerpo se centra en información que Copilot no conocería de sus datos de entrenamiento
- [ ] El cuerpo incluye cuándo usarla, prerrequisitos (si aplica) e instrucciones principales
- [ ] Sección `## Gotchas` presente si la skill involucra comportamiento no obvio, peculiaridades de API o trampas comunes
- [ ] Cuerpo de SKILL.md inferior a 500 líneas (considerar dividir en `references/` a ~200 líneas; 500 es el máximo absoluto)
- [ ] Flujos grandes (>5 pasos) divididos en la carpeta `references/` con enlaces claros desde SKILL.md
- [ ] Los scripts incluyen documentación de ayuda y gestión de errores
- [ ] Rutas relativas usadas para todas las referencias a recursos
- [ ] Sin credenciales ni secretos hardcodeados

## Recursos relacionados

- [Agent Skills Specification](https://agentskills.io/)
- [VS Code Agent Skills Documentation](https://code.visualstudio.com/docs/copilot/customization/agent-skills)
- [Reference Skills Repository](https://github.com/anthropics/skills)
- [Awesome Copilot Skills](https://github.com/github/awesome-copilot/blob/main/docs/README.skills.md)
