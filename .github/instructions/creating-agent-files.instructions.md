---
applyTo: '**/*.agent.md'
description: 'Directrices para crear archivos de agentes personalizados para GitHub Copilot'
---

# Directrices para archivos de agentes personalizados

Crear archivos de agentes personalizados efectivos y mantenibles que proporcionen experiencia especializada para tareas de desarrollo específicas en GitHub Copilot.

## Contexto del proyecto

- Audiencia objetivo: Desarrolladores que crean agentes personalizados para GitHub Copilot
- Formato de archivo: Markdown con frontmatter YAML
- Convención de nomenclatura de archivos: minúsculas con guiones (por ejemplo, `test-specialist.agent.md`)
- Ubicación en el repositorio: `.github/agents/`
- Ubicación en organización o empresa: `agents/` en la raíz de un repositorio `.github-private`
- Ubicación en el espacio de trabajo de VS Code: `.github/agents/` en el espacio de trabajo actual
- Ubicación a nivel de usuario de VS Code: el directorio de agentes personalizados activo para el perfil actual
- Propósito: Definir agentes especializados con experiencia, herramientas e instrucciones adaptadas para tareas específicas
- Documentación oficial:
  - [Crear agentes personalizados](https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-custom-agents)
  - [Configuración de agentes personalizados](https://docs.github.com/en/copilot/reference/custom-agents-configuration)
  - [Agentes personalizados en VS Code](https://code.visualstudio.com/docs/copilot/customization/custom-agents)
  - [Usar agentes integrados y personalizados con GitHub Copilot en Visual Studio](https://learn.microsoft.com/visualstudio/ide/copilot-specialized-agents?view=visualstudio)

## Frontmatter obligatorio

Todo archivo de agente debe incluir frontmatter YAML con los siguientes campos:

```yaml
---
description: 'Brief description of the agent purpose and capabilities'
name: 'Agent Display Name'
tools: ['read', 'edit', 'search']
model: 'Claude Sonnet 4.5'
target: 'vscode'
---
```

## Propiedades principales del frontmatter

### `description` (obligatorio)

- Cadena entre comillas simples que indica claramente el propósito y el dominio de experiencia del agente
- Mantenerla concisa, específica y orientada a la acción
- La descripción es la señal principal de descubrimiento en los selectores de agentes e interfaces de configuración
- Ejemplo: `'Focuses on test coverage, quality, and testing best practices'`

### `name` (opcional)

- Nombre visible del agente en la interfaz de usuario
- Si se omite, el valor predeterminado es el nombre del archivo sin `.md` ni `.agent.md`
- Usar title case y ser descriptivo
- Ejemplo: `'Testing Specialist'`

### `tools` (opcional)

- Lista de nombres o alias de herramientas que el agente puede usar
- Admite una cadena separada por comas o un array YAML
- Si se omite, el agente tiene acceso a todas las herramientas disponibles
- Usar el conjunto mínimo de herramientas necesarias para la tarea del agente
- Consultar [Configuración de herramientas](#configuración-de-herramientas) más adelante para más detalles

### `model` (recomendado)

- Especifica qué modelo de IA debe usar el agente cuando el host admite selección de modelo
- Si se omite, se usa el selector de modelo activo o el valor predeterminado del host
- Para mayor compatibilidad, preferir una cadena de modelo única
- En VS Code, también se puede usar una lista de modelos con prioridad cuando el host lo admite
- Ejemplos:
  - `'Claude Sonnet 4.5'`
  - `'GPT-5.2'`
  - `['Claude Opus 4.5', 'GPT-5.2']`

### `target` (opcional)

- Restringe el agente a un entorno de destino específico
- Valores admitidos: `'vscode'` o `'github-copilot'`
- Si se omite, el agente está disponible en cualquier entorno que pueda cargarlo
- Usar esta propiedad cuando el comportamiento del agente depende de herramientas o propiedades específicas del entorno

## Propiedades adicionales del frontmatter

### `argument-hint` (opcional, agentes personalizados de IDE)

- Agrega orientación breve en la interfaz de usuario sobre cómo invocar el agente
- Mantenerla concisa y orientada a la tarea
- Ejemplo: `'Provide a feature request or refactoring target'`

### `agents` (opcional, agentes personalizados de IDE)

- Restringe a qué otros agentes puede delegar este agente cuando usa la herramienta `agent`
- Acepta nombres explícitos, `'*'` para todos los agentes disponibles, o `[]` para ninguno
- Usar esta propiedad en agentes orquestadores para que la delegación sea predecible y auditable

### `user-invocable` (opcional)

- Booleano que controla si el agente aparece en el selector de agentes
- Valor predeterminado: `true`
- Establecer en `false` para agentes auxiliares que solo deben ser llamados por otros agentes o por herramientas

### `disable-model-invocation` (opcional)

- Booleano que controla si el agente puede ser seleccionado automáticamente por rutas de invocación dirigidas por modelo
- Valor predeterminado: `false`
- Usar cuando el agente solo deba ejecutarse mediante selección explícita del usuario o un paso de orquestación controlado
- Esta propiedad reemplaza el comportamiento antiguo de `infer` en las nuevas configuraciones

### `metadata` (opcional, GitHub.com)

- Objeto con pares nombre-valor para anotación
- Ejemplo: `metadata: { category: 'testing', version: '1.0' }`
- VS Code y otros agentes personalizados de IDE lo ignoran

### `mcp-servers` (opcional, GitHub.com y flujos de trabajo de agentes en la nube)

- Configura servidores MCP y herramientas con ámbito de agente directamente en el archivo de agente
- VS Code y otros agentes personalizados de IDE no lo usan
- Usar esta propiedad solo cuando el agente necesite herramientas externas dedicadas más allá de la configuración a nivel de repositorio
- Consultar [Configuración del servidor MCP](#configuración-del-servidor-mcp) más adelante para más detalles

### `handoffs` (opcional, agentes personalizados de IDE)

- Habilitan flujos de trabajo guiados que transicionan entre agentes con acciones sugeridas como siguiente paso
- GitHub.com ignora esta propiedad actualmente, por lo que debe usarse para flujos de trabajo centrados en IDE
- Consultar [Configuración de handoffs](#configuración-de-handoffs) más adelante para más detalles

### `hooks` (vista previa, VS Code)

- Los hooks con ámbito de agente pueden ejecutar comandos cuando el agente está activo
- Tratar esta funcionalidad como avanzada y usarla solo cuando el equipo la haya habilitado y probado explícitamente
- Preferir instrucciones u herramientas ordinarias a menos que los hooks resuelvan un problema claro del flujo de trabajo

## Configuración de handoffs

Los handoffs crean flujos de trabajo secuenciales guiados que transicionan entre agentes especializados. Usarlos cuando una tarea se beneficia de un handoff revisado por humanos entre fases como planificación, implementación, pruebas o revisión.

### Patrones de handoff comunes

- Planificación a implementación
- Implementación a revisión
- Escribir pruebas que fallen para luego implementar código que las pase
- Investigación a documentación
- Revisión de seguridad a remediación

### Estructura del frontmatter para handoffs

Definir los handoffs en el frontmatter YAML con el campo `handoffs`:

```yaml
---
description: 'Brief description of the agent'
name: 'Agent Name'
tools: ['search', 'read']
handoffs:
  - label: 'Start Implementation'
    agent: 'implementation'
    prompt: 'Now implement the plan outlined above.'
    send: false
  - label: 'Code Review'
    agent: 'code-review'
    prompt: 'Please review the implementation for quality and security issues.'
    send: false
---
```

### Propiedades de handoff

| Propiedad | Tipo | Obligatorio | Descripción |
| --- | --- | --- | --- |
| `label` | string | Sí | Texto que se muestra en el botón de handoff |
| `agent` | string | Sí | Identificador del agente de destino o nombre base del archivo |
| `prompt` | string | No | Texto del prompt para el agente de destino |
| `send` | boolean | No | Si es `true`, el prompt se envía automáticamente; el valor predeterminado es `false` |
| `model` | string o lista | No | Invalidación de modelo opcional para el handoff de destino en los hosts que lo admiten |

### Comportamiento de los handoffs

- Los botones de handoff aparecen como acciones sugeridas para el siguiente paso tras completar una respuesta
- Al seleccionar un handoff, se conserva el contexto de la conversación mientras se cambia el agente activo
- Si `prompt` está presente, se rellena previamente para el siguiente agente
- Si `send: true`, el prompt se envía automáticamente
- Mantener los handoffs centrados en el siguiente paso lógico en lugar de en todos los pasos posibles

### Directrices de configuración de handoffs

#### Cuándo usar handoffs

- Flujos de trabajo de varios pasos con límites de fase claros
- Puertas de calidad entre planificación, implementación y revisión
- Flujos de desarrollador estructurados donde el usuario debe aprobar cada fase
- Transiciones especializadas entre agentes de investigación, codificación, pruebas o documentación

#### Prácticas recomendadas

- Usar etiquetas claras y orientadas a la acción
- Mantener los prompts cortos y específicos al trabajo completado
- Limitar cada agente a los dos o tres pasos siguientes más relevantes
- Verificar que todos los agentes de destino existan
- Preferir `send: false` a menos que el siguiente paso sea extremadamente predecible

### Ejemplo de flujo de trabajo

**Agente de planificación** (`planner.agent.md`):

```yaml
---
description: 'Generate an implementation plan for new features or refactoring'
name: 'Planner'
tools: ['search', 'read']
handoffs:
  - label: 'Implement Plan'
    agent: 'implementer'
    prompt: 'Implement the plan outlined above.'
    send: false
---
```

**Agente de implementación** (`implementer.agent.md`):

```yaml
---
description: 'Implement code based on a plan or specification'
name: 'Implementer'
tools: ['read', 'edit', 'search', 'execute']
handoffs:
  - label: 'Review Implementation'
    agent: 'reviewer'
    prompt: 'Review this implementation for code quality, security, and best practices.'
    send: false
---
```

**Agente de revisión** (`reviewer.agent.md`):

```yaml
---
description: 'Review code for quality, security, and best practices'
name: 'Reviewer'
tools: ['read', 'search']
handoffs:
  - label: 'Back to Planning'
    agent: 'planner'
    prompt: 'Review the feedback above and decide whether a new plan is needed.'
    send: false
---
```

### Notas de compatibilidad de handoffs

- VS Code admite handoffs en los flujos de trabajo actuales de agentes personalizados
- GitHub.com ignora la propiedad `handoffs`
- El soporte en otros IDEs puede variar según la versión del producto, así que validar el comportamiento antes de estandarizarlo

## Configuración de herramientas

### Estrategias de especificación de herramientas

**Habilitar todas las herramientas** omitiendo la propiedad o usando:

```yaml
tools: ['*']
```

**Habilitar herramientas específicas**:

```yaml
tools: ['read', 'edit', 'search', 'execute']
```

**Habilitar herramientas de servidores MCP**:

```yaml
tools: ['read', 'edit', 'github/*', 'playwright/navigate']
```

**Deshabilitar todas las herramientas**:

```yaml
tools: []
```

### Alias de herramientas estándar

Todos los alias no distinguen entre mayúsculas y minúsculas.

| Alias | Nombres alternativos | Categoría | Descripción |
| --- | --- | --- | --- |
| `execute` | `shell`, `Bash`, `powershell` | Ejecución de shell | Ejecutar comandos en el shell apropiado |
| `read` | `Read`, `NotebookRead`, `view` | Lectura de archivos | Leer el contenido de archivos |
| `edit` | `Edit`, `MultiEdit`, `Write`, `NotebookEdit` | Edición de archivos | Editar y modificar archivos |
| `search` | `Grep`, `Glob`, `search` | Búsqueda de código | Buscar archivos o texto en archivos |
| `agent` | `custom-agent`, `Task` | Invocación de agentes | Invocar otros agentes personalizados |
| `web` | `WebSearch`, `WebFetch` | Acceso web | Obtener contenido web y realizar búsquedas web |
| `todo` | `TodoWrite` | Gestión de tareas | Crear y administrar listas de tareas estructuradas donde sea compatible |

### Herramientas integradas del servidor MCP

**Servidor MCP de GitHub**:

```yaml
tools: ['github/*']
tools: ['github/get_file_contents', 'github/search_repositories']
```

**Servidor MCP de Playwright**:

```yaml
tools: ['playwright/*']
tools: ['playwright/navigate', 'playwright/screenshot']
```

### Prácticas recomendadas para la selección de herramientas

- Aplicar el principio de mínimo privilegio
- Limitar `execute` a menos que el agente necesite acceso real al shell
- Mantener la lista de herramientas lo suficientemente reducida como para que el propósito del agente sea obvio
- Los nombres de herramientas varían entre los hosts de GitHub Copilot, por lo que hay que verificarlos en el entorno de destino
- En IDEs, preferir el diálogo integrado de configuración de herramientas cuando se necesiten los nombres exactos de las herramientas expuestas por ese host

## Invocación de subagentes y orquestación

Los agentes pueden invocar otros agentes con la herramienta `agent` para coordinar flujos de trabajo de varios pasos.

### Frontmatter obligatorio para orquestadores

- Agregar `agent` a la lista de `tools`
- Agregar una lista de permitidos en `agents` cuando se quiera limitar qué agentes pueden invocarse
- Asegurarse de que el orquestador tenga todas las herramientas que cualquier agente delegado vaya a necesitar

```yaml
---
name: 'Feature Builder'
description: 'Coordinate research and implementation through specialized agents'
tools: ['read', 'search', 'edit', 'agent']
agents: ['Researcher', 'Implementer']
---
```

### Enfoque recomendado de orquestación

- Definir el flujo de trabajo en lenguaje natural
- Delegar cada paso al agente más especializado
- Pasar solo el contexto compartido mínimo, como rutas, identificadores y ubicaciones de salida
- Requerir que cada subagente lea y siga su propio archivo `.agent.md`

### Patrón de prompt

```text
This phase must be performed as the agent "<AGENT_NAME>" defined in "<AGENT_SPEC_PATH>".

IMPORTANT:
- Read and apply the entire .agent.md spec.
- Work on "<WORK_UNIT_NAME>" with base path: "<BASE_PATH>".
- Perform the necessary reads and writes under this base path.
- Return a clear summary with actions taken, files produced or modified, and issues.
```

### Estructura básica de pasos

1. Descripción del paso
2. Identidad del agente: `agentName` y `agentSpecPath`
3. Contexto mínimo: rutas, IDs, nombre del entorno
4. Resultados esperados: archivos a crear o actualizar
5. Resumen de retorno: resumen corto y estructurado del trabajo completado

### Reglas clave

- Pasar variables en los prompts, no grandes bloques de contenido de archivo copiado
- Mantener las tareas de los subagentes pequeñas y específicas
- Ejecutar los pasos secuencialmente cuando las salidas alimentan pasos posteriores
- Detener o ramificar explícitamente cuando un paso obligatorio falla
- Evitar pipelines profundas para procesamiento masivo o conjuntos de datos muy grandes

## Estructura del prompt del agente

El contenido Markdown debajo del frontmatter define el comportamiento, la experiencia y las restricciones del agente.

Los prompts bien estructurados suelen incluir:

1. Identidad y rol del agente
2. Responsabilidades principales
3. Enfoque y metodología
4. Directrices y restricciones
5. Expectativas de salida

### Prácticas recomendadas para redactar prompts

- Ser específico y directo
- Definir límites claros
- Explicar la experiencia de dominio que el agente debe aplicar
- Centrarse en el comportamiento y la toma de decisiones, no en eslóganes
- Usar encabezados y listas cortas para que el prompt sea fácilmente escaneable
- Mantener el prompt completo dentro del límite documentado de 30.000 caracteres

## Definición y extracción de variables

Usar variables cuando el agente necesite adaptarse a datos proporcionados por el usuario, como nombres de proyectos, rutas, identificadores de funcionalidades o modos.

### Patrón de declaración de variables

```markdown
# Agent Name

## Dynamic parameters

- **projectName**: Project or repository identifier
- **basePath**: Root path the agent should work under
- **outputDirectory**: Where generated outputs should be written
```

### Estrategia de resolución de variables

1. Extraer valores explícitos del prompt del usuario
2. Inferir valores del archivo o contexto del espacio de trabajo actual cuando sea fiable
3. Leer archivos de configuración solo cuando sea necesario
4. Preguntar al usuario solo cuando la variable faltante bloquee el comportamiento correcto

### Prácticas recomendadas para variables

- Documentar explícitamente las variables obligatorias, opcionales y derivadas
- Usar nombres descriptivos como `projectName`, `basePath` y `outputDirectory`
- Indicar los valores válidos o las restricciones cuando una variable se limita a un enumerado o formato
- Pasar valores concretos ya resueltos a los subagentes en lugar de marcadores de posición sin resolver

## Configuración del servidor MCP

Los servidores MCP amplían un agente con herramientas adicionales. Usarlos con cuidado y solo cuando el agente necesite capacidades más allá del repositorio y el conjunto de herramientas integrado.

### Formato de configuración

```yaml
---
name: 'my-custom-agent'
description: 'Agent with MCP integration'
tools: ['read', 'edit', 'custom-mcp/tool-1']
mcp-servers:
  custom-mcp:
    type: 'local'
    command: 'some-command'
    args: ['--arg1', '--arg2']
    tools: ['*']
    env:
      ENV_VAR_NAME: ${{ secrets.COPILOT_MCP_ENV_VAR_VALUE }}
---
```

### Propiedades del servidor MCP

- `type`: generalmente `'local'`; GitHub asigna la compatibilidad de estilo `stdio` a su tipo local
- `command`: ejecutable utilizado para iniciar el servidor
- `args`: argumentos del comando
- `tools`: herramientas a habilitar desde ese servidor
- `env`: variables de entorno y valores respaldados por secretos

### Variables de entorno y secretos

Los patrones admitidos incluyen:

```yaml
env:
  VAR_NAME: $COPILOT_MCP_ENV_VAR_VALUE
  VAR_NAME_ALT: ${COPILOT_MCP_ENV_VAR_VALUE}
  VAR_NAME_WITH_DEFAULT: ${COPILOT_MCP_ENV_VAR_VALUE:-default}
  SECRET_VAR: ${{ secrets.COPILOT_MCP_ENV_VAR_VALUE }}
  CONFIG_VAR: ${{ vars.COPILOT_MCP_ENV_VAR_VALUE }}
```

### Prácticas recomendadas para MCP

- No codificar secretos de forma estática
- Limitar las herramientas MCP expuestas al conjunto mínimo útil
- Documentar por qué el agente necesita cada servidor externo
- Validar la configuración del repositorio o entorno antes de depender de flujos de trabajo respaldados por MCP

## Organización y nomenclatura de archivos

### Agentes a nivel de repositorio

- Ubicación: `.github/agents/`
- Ámbito: disponible solo en el repositorio o espacio de trabajo actual

### Agentes a nivel de organización o empresa

- Crear en `.github-private/agents/` y publicar en el directorio raíz `agents/` para uso compartido
- Ámbito: disponible en todos los repositorios de esa organización o empresa

### Convenciones de nomenclatura

- Usar minúsculas con guiones: `test-specialist.agent.md`
- Elegir un nombre de archivo que refleje el propósito del agente
- Caracteres permitidos en el nombre de archivo: `.`, `-`, `_`, `a-z`, `A-Z`, `0-9`
- El nombre de archivo se convierte en el nombre predeterminado del agente si se omite `name`

## Procesamiento y comportamiento del agente

### Control de versiones

- Las versiones del agente están vinculadas a los SHA de los commits de Git para el archivo
- Diferentes ramas o etiquetas pueden exponer diferentes comportamientos del agente
- Las interacciones de las solicitudes de incorporación de cambios continúan usando la misma versión del agente para mantener la consistencia

### Conflictos de nombres

Prioridad de mayor a menor:

1. Agente a nivel de repositorio
2. Agente a nivel de organización
3. Agente a nivel de empresa

Las configuraciones de nivel inferior invalidan las de nivel superior cuando los nombres colisionan.

### Procesamiento de herramientas

- Sin propiedad `tools` significa que todas las herramientas están habilitadas
- `tools: []` deshabilita todas las herramientas
- Una lista específica habilita solo las herramientas listadas
- Las herramientas no reconocidas se ignoran para que los perfiles entre entornos puedan degradarse de forma segura

### Orden de procesamiento de MCP

1. Servidores MCP predefinidos
2. Configuración de MCP con ámbito de agente
3. Configuración de MCP a nivel de repositorio

Cada capa posterior puede invalidar la configuración de las anteriores.

## Lista de verificación para la creación de agentes

### Frontmatter

- [ ] `description` está presente, es específica y va entre comillas simples
- [ ] `name` está presente cuando se necesita un nombre visible distinto del nombre de archivo
- [ ] `tools` están configuradas intencionalmente u omitidas intencionalmente
- [ ] `model` se especifica cuando el host admite anclaje de modelos y el comportamiento es importante
- [ ] `target` se establece si el agente es específico del entorno
- [ ] `user-invocable` se establece en `false` para agentes que solo funcionan como auxiliares
- [ ] `disable-model-invocation` se establece cuando la invocación automática sería arriesgada
- [ ] `agents` se establece en orquestadores que solo deben delegar a una lista de permitidos conocida
- [ ] `handoffs` están presentes solo cuando el flujo de trabajo del siguiente paso es real y está probado

### Contenido del prompt

- [ ] La identidad y el rol son claros
- [ ] Las responsabilidades son explícitas
- [ ] Las restricciones y los no-objetivos están documentados
- [ ] Las expectativas de salida son concretas
- [ ] Las instrucciones son específicas y orientadas a la acción
- [ ] Los límites del ámbito son obvios
- [ ] La longitud del prompt se mantiene dentro de los límites de la plataforma

### Estructura del archivo

- [ ] El nombre de archivo sigue la convención de minúsculas con guiones
- [ ] El archivo se coloca en el directorio correcto
- [ ] La extensión es `.agent.md`

### Control de calidad

- [ ] El agente tiene un propósito claramente diferenciado
- [ ] La lista de herramientas es mínima y justificada
- [ ] Las propiedades específicas del entorno se validan en el host de destino
- [ ] Los escenarios representativos se han probado manualmente
- [ ] Los enlaces a la documentación están actualizados
- [ ] Las implicaciones de seguridad están abordadas cuando el agente puede editar, ejecutar o llamar a sistemas externos

## Patrones de agentes comunes

### Especialista en pruebas

- Propósito: Mejorar la cobertura y la calidad de las pruebas
- Herramientas: Generalmente acceso amplio a herramientas de lectura, edición, búsqueda y ejecución relacionadas con pruebas
- Enfoque: Analizar brechas de cobertura, escribir pruebas, evitar cambios en producción a menos que se solicite explícitamente

### Planificador de implementación

- Propósito: Crear planes técnicos y especificaciones
- Herramientas: Generalmente `read`, `search` y `edit`
- Enfoque: Analizar requisitos, redactar planes, evitar la implementación

### Revisor de código

- Propósito: Revisar la calidad del código, la seguridad y las regresiones
- Herramientas: Generalmente `read` y `search`
- Enfoque: Producir hallazgos, no cambios de código

### Especialista en refactorización

- Propósito: Mejorar la estructura y la mantenibilidad de forma segura
- Herramientas: Generalmente `read`, `search` y `edit`
- Enfoque: Analizar patrones primero, luego refactorizar de forma mínima y validar

### Auditor de seguridad

- Propósito: Identificar vulnerabilidades y configuraciones arriesgadas
- Herramientas: Generalmente `read`, `search` y `web`
- Enfoque: Analizar el código, comparar con patrones seguros, informar hallazgos con severidad

## Errores comunes que evitar

### Errores de frontmatter

- Falta de `description`
- Sintaxis YAML no válida
- Suposiciones de propiedades no admitidas sin validación del host
- Nombres de herramientas copiados de otro host sin verificar el host actual

### Problemas de configuración de herramientas

- Conceder acceso excesivo
- Olvidar las herramientas necesarias para el propósito del agente
- Omitir `agent` o `agents` en orquestadores que necesitan control de delegación
- Olvidar los espacios de nombres MCP como `server-name/tool`

### Problemas de contenido del prompt

- Instrucciones vagas o contradictorias
- Ausencia de límites de ámbito
- Ausencia de expectativas de salida
- Prompts excesivamente largos que entierran la tarea real
- Ejemplos que no coinciden con las restricciones indicadas

### Problemas de organización

- El nombre de archivo no refleja el propósito del agente
- Directorio incorrecto para el ámbito previsto
- Caracteres no admitidos en el nombre de archivo
- Nombres duplicados que dependen de la precedencia de forma accidental en lugar de intencional

## Pruebas y validación

### Pruebas manuales

1. Crear el archivo `.agent.md` con frontmatter válido.
2. Recargar el host o reabrir la interfaz de configuración de agentes.
3. Seleccionar el agente en el selector.
4. Probar con prompts representativos.
5. Verificar el acceso a herramientas, los handoffs y el comportamiento de delegación donde corresponda.
6. Confirmar que la salida coincide con el propósito declarado del agente.

### Pruebas de integración

- Probar el agente con los tipos de archivo que se espera que maneje
- Verificar los nombres de herramientas y el acceso a herramientas en el host real
- Validar el comportamiento cuando falta el contexto esperado
- Probar casos extremos y rutas de error
- Verificar de extremo a extremo cualquier handoff o flujo orquestado

### Controles de calidad

- Revisar la lista de verificación anterior
- Comparar con ejemplos funcionales en la biblioteca de personalización o en `awesome-copilot`
- Obtener revisión por pares para agentes con herramientas potentes o integraciones externas
- Documentar cualquier requisito de configuración inusual

## Recursos adicionales

### Documentación oficial

- [Crear agentes personalizados](https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-custom-agents)
- [Configuración de agentes personalizados](https://docs.github.com/en/copilot/reference/custom-agents-configuration)
- [Agentes personalizados en VS Code](https://code.visualstudio.com/docs/copilot/customization/custom-agents)
- [Usar agentes integrados y personalizados con GitHub Copilot en Visual Studio](https://learn.microsoft.com/visualstudio/ide/copilot-specialized-agents?view=visualstudio)
- [Ampliar el agente en la nube de Copilot con MCP](https://docs.github.com/en/copilot/how-tos/use-copilot-agents/cloud-agent/extend-cloud-agent-with-mcp)

### Recursos de la comunidad

- [Colección de agentes Awesome Copilot](https://github.com/github/awesome-copilot/tree/main/agents)
- [Ejemplos de agentes en la biblioteca de personalización](https://docs.github.com/en/copilot/tutorials/customization-library/custom-agents)
- [Tutorial de tu primer agente personalizado](https://docs.github.com/en/copilot/tutorials/customization-library/custom-agents/your-first-custom-agent)

### Archivos relacionados

- [Directrices para archivos de prompts](./creating-prompt-files.instructions.md)
- [Directrices para archivos de instrucciones](./creating-instruction-files.instructions.md)

## Notas de compatibilidad de versiones

### GitHub.com y el agente en la nube de Copilot

- Admite los campos de configuración estándar documentados en GitHub Docs
- Ignora las propiedades específicas de IDE como `argument-hint` y `handoffs`
- Usa la configuración de GitHub.com y del repositorio para el comportamiento del agente en la nube
- Admite `mcp-servers` con ámbito de agente en escenarios de agentes en la nube

### VS Code y agentes personalizados de IDE

- Admite la selección de modelos para agentes personalizados
- Admite propiedades orientadas a IDE como `argument-hint`, `agents` y `handoffs`
- Usa nombres de herramientas específicos del host, por lo que siempre hay que validar los nombres de herramientas en el IDE de destino
- Puede admitir funcionalidades que GitHub.com ignora, por lo que no se debe asumir paridad entre entornos

Al crear agentes para múltiples entornos, centrarse primero en las propiedades comunes, luego agregar propiedades específicas del entorno solo cuando estén probadas y sean genuinamente necesarias.
