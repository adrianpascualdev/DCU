# GitHub Copilot SpecKit

Este repositorio reúne instrucciones, skills y documentación de apoyo para trabajar con GitHub Copilot en Visual Studio Code con un enfoque de gobernanza y estandarización.

## Cómo empezar

Para usar el repositorio con eficacia:

1. Instala las extensiones de Visual Studio Code que necesites para tu flujo de trabajo.
2. Revisa las [instrucciones](README.md#instrucciones-de-github-copilot) y [skills](README.md#habilidades-de-github-copilot).
3. Conserva solo las [instrucciones](README.md#instrucciones-de-github-copilot) y [skills](README.md#skills-de-github-copilot) que aporten valor a la solución en la que estés trabajando.
4. Actualiza el archivo `copilot-instructions.md` según las necesidades de la solución y elimina las referencias a los archivos borrados.
5. Revisa los servidores [mcp](README.md#servidores-mcp)  y borra los que no te interese.
6. Revisa el archivo [`.vscode/settings.json`](README.md#configurar-el-comportamiento-del-workspace-para-vsCode) que configura el comportamiento de VS Code específicamente para este workspace
7. Puede empezar a usar [Spec Kit](README.md#recomendaciones-para-constituciones-con-github-spec-kit)

### Extensiones de VSCode recomendadas

#### Trabajar con .NET

- [.Net Install Tool](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.vscode-dotnet-runtime)
- [c#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

#### Trabajar con Web (JavaScript,HTML)

- [ESLint](https://marketplace.visualstudio.com/items?itemName=dbaeumer.vscode-eslint)
- [HTML Preview](https://marketplace.visualstudio.com/items?itemName=george-alisson.html-preview-vscode)
- [Color Picker](https://marketplace.visualstudio.com/items?itemName=anseki.vscode-color)

#### Trabajar con Python

- [Python](https://marketplace.visualstudio.com/items?itemName=ms-python.python)
- [Pylance](https://marketplace.visualstudio.com/items?itemName=ms-python.vscode-pylance)
- [Python Environment](https://marketplace.visualstudio.com/items?itemName=ms-python.vscode-python-envs)

#### Trabajar con JSON

- [json](https://marketplace.visualstudio.com/items?itemName=ZainChen.json): verlo estructurado como arbol
- [JSON](https://marketplace.visualstudio.com/items?itemName=Meezilla.json): Hacer operaciones con el JSON.

#### Trabajar con PowerShell

- [PowerShell](https://marketplace.visualstudio.com/items?itemName=ms-vscode.PowerShell)

#### Trabajar con SQL Server

- [SQL Server (mssql)](https://marketplace.visualstudio.com/items?itemName=ms-mssql.mssql)
- [SQL Database Projects](https://marketplace.visualstudio.com/items?itemName=ms-mssql.sql-database-projects-vscode)

#### Trabajar con Markdown

- [markdownlint](https://marketplace.visualstudio.com/items?itemName=DavidAnson.vscode-markdownlint)
- [Markdown PDF](https://marketplace.visualstudio.com/items?itemName=yzane.markdown-pdf): Pasar Markdown a PDF

#### Trabajar con XML

- [XML Tools](https://marketplace.visualstudio.com/items?itemName=DotJoshJohnson.xml)
- [XML](https://marketplace.visualstudio.com/items?itemName=redhat.vscode-xml)

#### Trabajar con Copilot

- [Web Search for Copilot](https://marketplace.visualstudio.com/items?itemName=ms-vscode.vscode-websearchforcopilot)

#### Utilidades Generalistas

- [ToDo Tree](https://marketplace.visualstudio.com/items?itemName=Gruntfuggly.todo-tree): Se amplian las etiquetas para el código.
- [Material Icon Theme](https://marketplace.visualstudio.com/items?itemName=PKief.material-icon-theme): Skin para la ventana del Explorador de archivos
- [Excel Viewer](https://marketplace.visualstudio.com/items?itemName=GrapeCity.gc-excelviewer)

### Instrucciones de GitHub Copilot

Las instrucciones personalizadas son archivos Markdown con frontmatter YAML que se aplican automáticamente según patrones de archivos (*glob*). Se ubicarán en la carpeta `.github/instructions/` de cada repositorio. En la implementación actual, los nombres siguen tres familias de prefijos:

| Prefijo | Descripción |
| :--- | :--- |
| `coding-style` | Instrucciones relacionadas con el estilo de codificación, convenciones y patrones de uso de tecnologías específicas. |
| `creating` | Meta-instrucciones para ayudar a GitHub Copilot a crear artefactos del propio ecosistema: agentes, prompts, instrucciones y skills. |
| `wow` | Instrucciones relacionadas con la forma recomendada de trabajar (*way of working*), gobierno y calidad transversal. |

#### Reglas transversales y de gobierno

| ID | Archivo | Aplica a | Propósito real |
| :--- | :--- | :--- | :--- |
| INS-00 | `wow-follow-up-question.instructions.md` | `**` | Impone que el asistente no avance si no alcanza al menos un 95% de certeza, obliga a mostrar siempre el porcentaje de confianza y a formular preguntas aclaratorias cuando sea necesario. |
| INS-01 | `wow-no-dummy-no-placholder-code.instructions.md` | `**` | Prohíbe código no ejecutable, valores ficticios o placeholders y exige usar valores concretos o parametrización idiomática según el lenguaje. |
| INS-02 | `wow-spec-driven-development.instructions.md` | `**` | Obliga a trabajar desde una especificación aprobada, con criterios de aceptación medibles, trazabilidad y actualización previa de la especificación antes de tocar la implementación. |
| INS-03 | `wow-conventional-commits.instructions.md` | `**` | Define el formato de mensajes de commit y títulos de PR según Conventional Commits. |

#### Estilo y tecnología

| ID | Archivo | Aplica a | Propósito real |
| :--- | :--- | :--- | :--- |
| INS-04 | `coding-style-csharp.instructions.md` | `**/*.cs`<br>`**/*.csproj` | Guía general de estilo para C# en .NET y .NET Framework: convenciones de nombres, organización de archivos, validación, LINQ, nulabilidad y compatibilidad entre líneas tecnológicas. |
| INS-05 | `coding-style-csharp-constants.instructions.md` | `**/*.cs`<br>`**/Constants.cs` | Directrices para organizar constantes y cadenas mágicas (*magic strings*) siguiendo los patrones establecidos de clases estáticas anidadas. |
| INS-06 | `coding-style-csharp-extension-methods.instructions.md` | `**/Extensions/**/*.cs`<br>`**/*Extensions.cs` | Directrices para crear métodos de extensión siguiendo los patrones establecidos para el registro de servicios, APIs fluidas y utilidades de dominio en C#. |
| INS-07 | `coding-style-csharp-localization.instructions.md` | `**/*.cs` | Directrices de idioma y localización para código C#: castellano en textos legibles, inglés en identificadores y uso obligatorio de archivos de recursos (`.resx`) para textos destinados al usuario. |
| INS-08 | `coding-style-csharp-nuget-packaging.instructions.md` | `**/*.csproj` | Directrices para configurar proyectos `.csproj` que generan paquetes NuGet, asegurando que la documentación XML, el README y los metadatos se empaquetan correctamente. |
| INS-09 | `coding-style-csharp-program.instructions.md` | `**/Program.cs` | Directrices para crear archivos `Program.cs` para aplicaciones ASP.NET, Aspire y .NET, siguiendo convenciones de configuración, telemetría e inyección de dependencias. |
| INS-10 | `coding-style-csharp-resource-files.instructions.md` | `**/Resources/**/*.resx`<br>`**/Resources/**/*.Designer.cs`<br>`**/*.csproj` | Directrices para organizar archivos de recursos (`.resx`), sus archivos generados (`.Designer.cs`) y su configuración en proyectos .NET. |
| INS-11 | `coding-style-csharp-service-implementation.instructions.md` | `**/Services/**/*.cs`<br>`**/Abstractions/**/*.cs` | Directrices para implementar clases de servicio e interfaces siguiendo patrones establecidos para inyección de dependencias, métodos factoría y manejo de resultados. |
| INS-12 | `coding-style-csharp-settings-classes.instructions.md` | `**/Settings/**/*.cs`<br>`**/*Settings.cs` | Directrices para crear clases de *settings* de configuración con el Patrón de Opciones, usando enlazado fuerte de tipos, validación, inmutabilidad y registro consistente. |
| INS-13 | `coding-style-csharp-xml-documentation.instructions.md` | `**/*.cs` | Directrices para redactar documentación XML en código C#, cubriendo tipos públicos, métodos, propiedades, uso de `inheritdoc` y documentación de excepciones. |
| INS-14 | `coding-style-vb.instructions.md` | `**/*.vb`<br>`**/*.vbproj` | Guía general de estilo para Visual Basic .NET: convenciones, `Option Strict`/`Option Explicit`, estructura, comentarios XML y organización del código. |
| INS-15 | `coding-style-blazor.instructions.md` | `**/*.razor`<br>`**/*.razor.cs`<br>`**/*.razor.css` | Patrones de Blazor moderno con C#: ciclo de vida, *render modes*, persistencia de estado, validación, rendimiento y pruebas. |
| INS-16 | `coding-style-react-typescript.instructions.md` | `**/*.tsx`<br>`**/*.ts`<br>`**/*.jsx`<br>`**/*.js` | Convenciones para React con TypeScript: estructura de componentes, *hooks*, tipado, acciones de formularios y patrones de React. |
| INS-17 | `coding-style-jquery.instructions.md` | `**/*.js`<br>`**/*.html`<br>`**/*.htm` | Guía general de jQuery: selectores, eventos, manipulación del DOM, Ajax, Deferred, plugins, rendimiento y seguridad. |
| INS-18 | `coding-style-jquery-ui-core.instructions.md` | `**/*.js`<br>`**/*.html`<br>`**/*.htm` | Guía general de jQuery UI: ciclo de vida de widgets, accesibilidad, *theming*, seguridad y rendimiento, independiente de la versión concreta. |
| INS-19 | `coding-style-markdown.instructions.md` | `**/*.md` | Convenciones para documentación con Markdown: encabezados, listas, tablas, bloques de código, enlaces, estilo editorial y preferencia por Mermaid cuando sea posible. |

#### Meta-instrucciones para crear artefactos de GitHub Copilot

| ID | Archivo | Aplica a | Propósito real |
| :--- | :--- | :--- | :--- |
| INS-20 | `creating-agent-files.instructions.md` | `**/*.agent.md` | Explica cómo crear archivos de agentes personalizados con *frontmatter* correcto, herramientas, modelos, *handoffs* y buenas prácticas de mantenibilidad. |
| INS-21 | `creating-agent-skills.instructions.md` | `**/skills/**/SKILL.md` | Explica cómo crear *skills* de alta calidad, con descripción optimizada para descubrimiento automático, recursos asociados y carga progresiva. |
| INS-22 | `creating-instruction-files.instructions.md` | `**/*.instructions.md` | Define la estructura esperada para nuevas instrucciones: *frontmatter*, secciones recomendadas, ejemplos y patrones a evitar. |
| INS-23 | `creating-prompt-files.instructions.md` | `**/*.prompt.md` | Define cómo crear *prompts* reutilizables: *frontmatter*, herramientas mínimas, flujo, expectativas de salida y validación. |

### Skills de GitHub Copilot

#### Skills

Los *skills* son carpetas autocontenidas que incluyen instrucciones y recursos auxiliares para tareas especializadas. Se ubicarán (por defecto) en `.github/skills/` de cada repositorio o a nivel organizativo según corresponda.

| ID | Carpeta | Propósito real |
| :--- | :--- | :--- |
| SKL-00 | `automapper/` | Guía específica para AutoMapper 8.0 en .NET Framework 4.7.2 y .NET Standard 2.0, incluyendo perfiles, `CreateMap`, `MapFrom`, `ProjectTo`, validación y extensibilidad. Incluye el recurso `references/api-reference.md`. |
| SKL-01 | `code-review/` | Guía de revisión de código con mentalidad de detección de defectos: prioriza bugs, riesgos de seguridad, regresiones, huecos de pruebas y deriva arquitectónica. Incluye `references/review-guidelines.md`. |
| SKL-02 | `dapper-orm/` | Guía específica para Dapper: consultas parametrizadas, métodos `Query*`, transacciones, multi-mapping, `DynamicParameters`, rendimiento y anti-patrones. Incluye `references/api-reference.md`. |
| SKL-03 | `ef6-data-access/` | Guía específica para Entity Framework 6: `DbContext`, modelado, Fluent API, rendimiento, concurrencia y pruebas. |
| SKL-04 | `jquery/` | Capa específica para jQuery 2.1.3: compatibilidad, APIs no disponibles en 3.x, restricciones de versión y *gotchas*. Complementa a `coding-style-jquery.instructions.md`. |
| SKL-05 | `jquery-ui/` | Capa específica para jQuery UI 1.11.3: widgets disponibles, incompatibilidades, restricciones de versión y cuatro referencias detalladas sobre *widgets*, *interactions*, *effects* y *widget factory*. |
| SKL-06 | `microsoft-docs-and-code-consultant/` | Guía para consultar documentación y ejemplos oficiales de Microsoft mediante las herramientas de Microsoft Learn. Es la única *skill* con dependencia explícita de un MCP concreto. |
| SKL-07 | `netfx-csharp/` | Restricciones y patrones específicos para proyectos C# en .NET Framework 4.7.2: formato clásico de `.csproj`, C# 7.3, `App.config`/`Web.config`, WCF, WinForms, WPF y demás consideraciones. |
| SKL-08 | `netfx-vb/` | Restricciones y patrones específicos para proyectos VB.NET en .NET Framework 4.7.2: `.vbproj` clásico, Visual Basic 15.x, configuración, COM interop y tecnologías *legacy* del ecosistema .NET Framework. |
| SKL-09 | `nuget-package-manager/` | Guía operativa para gestionar paquetes NuGet. |
| SKL-10 | `skill-maker/` | *Meta-skill* para crear nuevas *skills*. Incluye plantilla, *checklist*, ejemplos de estructura y una carpeta `scripts/` con archivos `.mjs` y `package.json` para validación. |

Observaciones importantes sobre el conjunto actual de skills:

- `automapper/`, `dapper-orm/`, `ef6-data-access/`, `netfx-csharp/` y `netfx-vb/` son **aditivas**: están pensadas para combinarse con las instrucciones base de C# o VB, no para sustituirlas.
- `jquery-ui/` no se ha materializado como cinco archivos `.instructions.md` distintos; en su lugar, se ha entregado como una skill con cuatro archivos de referencia (`effects-theming.md`, `interactions.md`, `widget-factory.md`, `widgets.md`) que solo se cargan cuando hacen falta.
- La única *meta-skill* entregada en esta iteración es `skill-maker/`. La creación de agentes, *prompts* e instrucciones queda cubierta por las *meta-instrucciones* `creating-*`, no por *skills* separadas.

### Servidores mcp

Están en `.vscode/mcp.json`. Recuerda activarlos para que puedan ser usados.

#### Recomendaciones de MCPs

Una adopción apropiada de GitHub Copilot en AITEX debe ir acompañada también de una estrategia de adopción de MCP (Model Context Protocol). # Recomendaciones de MCPs

Una adopción apropiada de GitHub Copilot en AITEX debe ir acompañada también de una estrategia de adopción de MCP (Model Context Protocol). A la luz de las instrucciones y skills efectivamente creadas en este repositorio, conviene distinguir entre dependencias directas, recomendaciones de alto valor y servicios complementarios.

##### Dependencia directa de los entregables actuales

- **MCP Microsoft Learn**: es el único MCP explícitamente requerido por una skill existente, `microsoft-docs-and-code-consultant/`. Esta skill define un flujo de trabajo basado en búsqueda de documentación, descarga de páginas completas y búsqueda de ejemplos oficiales para validar APIs, SDKs y patrones de Microsoft.

##### Recomendaciones de alto valor

- **Context7**: recomendable para ampliar la consulta de documentación actualizada de librerías y frameworks no Microsoft. No es una dependencia dura de las skills actuales, pero encaja muy bien con el enfoque de verificación factual que se busca reforzar con GitHub Copilot.
- **MCP Filesystem**: recomendable cuando se quiera portar estas personalizaciones a entornos donde el acceso al workspace no esté garantizado por la propia integración del editor. Facilita la lectura y exploración del repositorio sin recurrir a scripts auxiliares.

**NOTA**: Para hacer un uso efectivo de Context7, es necesario obtener una API Key registrándose en su plataforma. Su adopción es recomendable, pero no obligatoria para ejecutar las skills actualmente creadas.

##### Servicios complementarios o de evolución

- **MCP Memory**: opcional. Puede ayudar a mantener contexto entre sesiones, pero ninguna de las instrucciones o skills creadas depende de él de forma estricta.
- **MCP Azure DevOps**: recomendado como siguiente paso si AITEX quiere extender el uso de GitHub Copilot hacia backlog, repositorios, *pipelines* y trazabilidad en Azure DevOps. No es un prerrequisito de los entregables actuales.
- **MCP Playwright**: servidor de automatización de navegador basado en Playwright. Permite realizar capturas de pantalla, navegar páginas, interactuar con elementos y ejecutar código en el contexto del navegador. Útil para pruebas de interfaz y verificación visual.
- **MCP MarkItDown**: servidor de conversión de documentos a Markdown. Permite transformar archivos de diversos formatos (PDF, Word, Excel, PowerPoint, imágenes, audio, HTML, entre otros) a Markdown para su procesamiento por el agente.

**NOTA 1**: Los servidores `mcp-memory`, `mcp-filesystem` y `mcp-playwright` requieren tener Node.js instalado en el ordenador local y se descargan automáticamente desde npm al ejecutar `npx` según la configuración del `mcp.json`.

**NOTA 2**: Para hacer uso efectivo de `mcp-azure-devops` es necesario que, por proyecto, se indique el valor del parámetro de entrada `ado_org` correspondiente al nombre de la organización de Azure DevOps.

**NOTA 3**: El servidor `mcp-markitdown` requiere tener Python y `uvx` instalados en el ordenador local. Se descarga y ejecuta automáticamente mediante `uvx` según la configuración del `mcp.json`.

### Archivo `copilot-instructions.md` a nivel de repositorio

Cada repositorio de AITEX contendrá un archivo `.github/copilot-instructions.md` que actuará como punto de entrada principal para GitHub Copilot. En la versión actualmente creada, este archivo ya define de forma explícita:

- **Jerarquía de directivas**: prioridad de las órdenes directas del usuario, verificación factual por encima del conocimiento interno, adhesión a la filosofía del repositorio y obligación de aclarar antes de implementar.
- **Política de idioma**: todo el material técnico para desarrolladores debe mantenerse en inglés; el contenido orientado al usuario final puede permanecer en español cuando corresponda.
- **Filosofía de interacción**: respuestas directas y concisas, explicando brevemente el porqué, con preferencia por buenas prácticas reconocidas y soluciones estándar.
- **Generación de código minimalista**: se prioriza la simplicidad, el uso de librerías estándar y la evitación de sobre-ingeniería.
- **Modificación quirúrgica**: las ediciones deben ser mínimas, preservando estructura, estilo y lógica del código existente.
- **Uso inteligente de herramientas**: se exige usar herramientas cuando sean necesarias y declarar la intención antes de invocarlas.
- **Desarrollo guiado por especificación**: remite expresamente a `wow-spec-driven-development.instructions.md` como marco obligatorio para nuevas implementaciones.
- **Reglas específicas para .NET Framework 4.7.2**: enlaza con las instrucciones base de C# y VB y con las skills `netfx-csharp`, `netfx-vb`, `ef6-data-access`, `dapper-orm`, `automapper`, `jquery`, `jquery-ui`, `nuget-package-manager` y `microsoft-docs-and-code-consultant`.
- **Gobierno de Markdown y commits**: obliga a seguir `coding-style-markdown.instructions.md` para documentación y `wow-conventional-commits.instructions.md` para commits y títulos de pull request.

### Configurar el comportamiento del WorkSpace para VSCode

- `chat.mcp.access`: Permite el acceso a todos los servidores MCP (Model Context Protocol) disponibles.
- `chat.promptFilesRecommendations`: Habilita las recomendaciones de archivos de prompt del SpecKit (speckit.constitution, speckit.specify, speckit.plan, speckit.tasks, speckit.implement) para que aparezcan como sugerencias al usar Copilot Chat.
- `chat.tools.terminal.autoApprove`: Auto-aprueba la ejecución de scripts de terminal ubicados en .specify/scripts/bash/ y .specify/scripts/powershell/, evitando la confirmación manual cada vez.
- `chat.useAgentSkills`: Activa el uso de Agent Skills de GitHub Copilot (los archivos SKILL.md del workspace).
- `chat.mcp.autostart`: Arranca automáticamente los servidores MCP nuevos o desactualizados.
- `files.associations`: Asocia los archivos `*.chatmode.md`, `*.instructions.md`, `*.prompt.md` y `SKILL.md` con el lenguaje Markdown para que VS Code les aplique resaltado de sintaxis y funcionalidades de edición Markdown.

### Recomendaciones para constituciones con GitHub Spec Kit

Al adoptar GitHub Spec Kit, se recomienda seguir un enfoque de constitución progresiva, sobre todo para proyectos *legacy* o ya establecidos (*brownfield*). Desde ENCAMINA recomendamos algunas posibles o tentativas secciones para el archivo `constitution.md` de cada proyecto, que podrían evolucionar a medida que el equipo y la organización se familiarice con la herramienta y su potencial. Así, los siguientes puntos son posibles ejemplos de secciones sugeridas de la constitución de un proyecto, sin ser un requisito ni una plantilla rígida, sino referencias que pueden servir de inspiración para que el equipo de AITEX defina su propia constitución de acuerdo a sus necesidades y contexto específico de cada proyecto.

- Test-Driven Development: directrices estrictas que exigen desarrollo guiado por pruebas (TDD) como obligatorio. Escribir y aprobar pruebas antes de implementar, seguir el ciclo rojo‑verde‑refactor, no escribir código sin pruebas y cubrir pruebas unitarias, de integración y de contrato para garantizar corrección.

- Code Quality Standards: directrices que establecen los estándares de calidad de código que exigen legibilidad y mantenibilidad (nombres descriptivos, funciones pequeñas, separación de responsabilidades, DRY, SOLID), documentación pública, análisis estático sin advertencias, formateo automático, anotaciones de tipos y límites de complejidad antes de permitir un *merge*.

- User Experience: directrices de experiencia de usuario que exigen interfaces coherentes e intuitivas, patrones de interacción y formatos de salida predecibles, integración completa, presentación de datos estandarizada y mensajes de error claros (en el lenguaje establecido, como por ejemplo Español) para reducir la carga cognitiva y mejorar la eficiencia. Aquí se puede especificar detalles tales como formatos de datos y salidas consistentes (ISO 8601 para fechas, tablas alineadas, codificación por colores, etc.) para garantizar uniformidad en la experiencia de usuario.

- Performance: directrices que especifican los requerimientos de rendimiento con objetivos medibles tales como latencia, eficiencia de recursos, escalabilidad y operaciones, para asegurar una aplicación rápida y fiable a medida que crece su carga de trabajo. Los lineamientos deben expresarse de forma clara y cuantificable, por ejemplo: "*Las respuestas a interacciones de usuario deben tener una latencia inferior a 200 ms*", "*El uso de memoria no debe superar los 100 MB por sesión*" o "*La aplicación debe escalar horizontalmente para soportar al menos 1000 usuarios concurrentes sin degradación del rendimiento*". De esta forma, se establecen expectativas claras y medibles para el rendimiento de la aplicación.

- Reliability: directrices concisas que exigen validaciones *post‑deploy*, observabilidad completa con *logs*, métricas, trazas y SLIs/SLOs (Service Level Indicator / Service Level Objective)cuantificables, gestión de fallos (*timeouts*, patrones *circuit breaker*, reintentos con *backoff*, idempotencia y degradación elegante), control de dependencias y versiones con pruebas de contrato, pruebas regulares de carga y *chaos engineering*, y operaciones seguras con *feature flags*, *rollback* automático, actuaciones accionables para mantener disponibilidad y recuperación rápida.

- Observability: directrices que exigen instrumentar sistemas para entender su comportamiento mediante tres pilares: *logs* estructurados, métricas y trazas, para poder responder a preguntas tales como “*¿está funcionando?*” o “*¿por qué falló?*”.  Instrumentación consistente (propagación de contexto y *correlation IDs*), *logs* en formato estructurado, métricas clave (conteo, latencias, tasas de error) y trazas por petición permiten correlación y diagnóstico rápido.  Definir SLIs/SLOs, exponer *endpoints* de salud y evitar datos sensibles (data personal, identificable) en la telemetría son requisitos operativos.

- Development Workflow: directrices integradas en la filosofía *Spec‑Driven Development* (SDD) que establecen que todo desarrollo parte de una especificación que emplea PRs (*pull requests*) pequeños y con una clara definición de terminado (DoD, Definition of Done). En la disciplina SDD la implementación no comienza hasta que la especificación ha sido formalmente revisada y aprobada. Cada PR debe corresponder a una unidad lógica contenida y justificable. La definición de terminado (DoD) exige que el código cumpla los requisitos de toda su especificación, que pasen las pruebas unitarias y de integración pertinentes y requeridas, que no se introduzcan nuevos errores (regresiones), que la documentación pública se actualice ante cambios, y que al menos otro colaborador humano apruebe la revisión. Se siguen las reglas establecidas para la gestión de la configuración, tales como nombres de ramas y nomenclatura establecida para los *commits*, etc.

Así mismo, es importante que la organización adopte la práctica de marcar secciones de la constitución como `(NON-NEGOTIABLE)` y a la vez expresarlas en un lenguaje que indique la obligatoriedad de su cumplimiento. Por ejemplo, en lugar de decir "*Seguir las prácticas de TDD*", se podría decir "*Es obligatorio y se debe seguir TDD para todas las nuevas implementaciones*". De esta forma, se refuerza la idea de que estas prácticas no son meras recomendaciones, sino reglas (leyes) claras que deben cumplirse para garantizar la calidad y consistencia del código. En ese sentido, lingüisticamente hablando recomendamos la RFC-2119 como referencia para redactar las secciones de la constitución, ya que su estilo imperativo y directo es ideal para comunicar reglas claras:

- Versión en inglés: <https://www.ietf.org/rfc/rfc2119.txt>
- Versión en español: <https://www.rfc-es.org/rfc/rfc2119-es.txt>

El uso de mayúsculas y de negritas en la nomenclatura Markdown es muy recomendable para destacar la obligatoriedad de las reglas. Aunque pareciera un refuerzo visual, en el contexto de la IA generativa suele funcionar como un indicador adicional de la importancia de la norma, ayudando a que la IA le preste más atención y la considere como un requisito estricto en lugar de una sugerencia.

## Referencias

- [Descargar e instalar Node.js](https://nodejs.org/) — requerido por los servidores `mcp-memory`, `mcp-filesystem`, `mcp-playwright` y `mcp-azure-devops` (ejecutados vía `npx`).
- [Descargar e instalar Python](https://www.python.org/downloads/) — requerido por el servidor `mcp-markitdown` (ejecutado vía `uvx`) y por [GitHub Spec Kit](https://speckit.org/).
- [Instalar uv y uvx](https://docs.astral.sh/uv/getting-started/installation/) — gestor de paquetes y ejecutor de herramientas Python, requerido para ejecutar `mcp-markitdown` y [GitHub Spec Kit](https://speckit.org/).

## Contribuir

Si quieres mejorar este repositorio, prioriza cambios pequeños, centrados y alineados con las instrucciones de `.github/`. Si necesitas una referencia para redactar archivos `README`, puedes consultar estas [directrices](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops) y revisar estos ejemplos:

- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)
# DCU
