# Instrucciones de GitHub Copilot

## Directrices principales y jerarquía

Esta sección describe el orden absoluto de operaciones. Estas reglas tienen la máxima prioridad y no deben ser violadas.

1. **Primacía de las directivas del usuario**: Un mandato directo y explícito del usuario tiene la máxima prioridad. Si el usuario indica usar una herramienta específica, editar un archivo o realizar una búsqueda concreta, ese mandato **debe ejecutarse sin desviaciones**, incluso si otras reglas sugieren que no es necesario. Todas las demás instrucciones son subordinadas a una orden directa del usuario.
2. **Verificación fáctica sobre conocimiento interno**: Cuando una petición implique información que pueda depender de la versión, ser sensible al tiempo o requiera datos externos específicos (p. ej., documentación de librerías, mejores prácticas más recientes, detalles de APIs), priorizar el uso de herramientas para obtener la respuesta factual y actual en lugar de confiar en el conocimiento interno general.
3. **Adhesión a la filosofía**: En ausencia de una directiva directa del usuario o de la necesidad de verificación fáctica, deben seguirse todas las demás reglas siguientes sobre interacción, generación de código y modificaciones.
4. **Aclaración antes de implementar**: Sigue las instrucciones del archivo `/.github/instructions/wow-follow-up-question.instructions.md` para solicitar aclaraciones si no estás al menos 95% seguro sobre qué construir o documentar.


## Directrices de prioridad

Al generar código para este repositorio:

1. **Compatibilidad de versiones**: Detectar y respetar las versiones exactas de lenguaje, framework y bibliotecas presentes en el repositorio.
2. **Archivos de contexto**: Priorizar la orientación en `.github/instructions/*` y `.github/copilot-instructions.md`.
3. **Patrones del código base**: Si falta orientación, revisar y seguir los patrones ya establecidos en los archivos existentes.
4. **Calidad del código**: Priorizar mantenibilidad, legibilidad, rendimiento, seguridad, accesibilidad y testabilidad, tal como se refleja en el código y pruebas actuales.

## Política de idioma

Todas las instrucciones y prompts en este repositorio deben escribirse en castellano. Esto se aplica a:

- Todos los archivos de reglas e instrucciones en `/.github/instructions/`
- Todos los archivos de prompts en `/.github/prompts/`
- Todos los archivos de agentes personalizados en `/.github/agents/`
- Todos los archivos de skills de agentes que GitHub Copilot pueda cargar cuando sean relevantes para realizar tareas especializadas en `/.github/skills/<skill_name>/`
- Toda la documentación y comentarios de código destinados a colaboradores, salvo que el usuario solicite explícitamente lo contrario en un caso concreto.

Los textos y valores de cadena destinados a usuarios finales, así como la documentación dirigida a colaboradores de este repositorio, deben estar en castellano salvo que el usuario indique lo contrario. Pero los elementos relacionados con el desarrollo o el código, como namespaces, clases, métodos, variables e identificadores equivalentes, **DEBEN SIEMPRE** estar en inglés.

Al revisar cualquier código existente, cualquier código o documentación que viole esta política de identificadores en inglés **DEBE** permanecer tal cual para evitar romper funcionalidad, salvo que el usuario solicite explícitamente cambiarlo o refactorizarlo. No obstante, todo código y documentación nuevos deben adherirse estrictamente a las políticas de inglés y castellano.

### Reglas obligatorias por tipo de artefacto

- **Identificadores de código** (clases, métodos, propiedades, variables, namespaces, tipos): siempre en inglés y descriptivos
- **Código externo o generado**: si un identificador proviene de un contrato externo o código auto-generado y renombrarlo rompe trazabilidad o regeneración, conservar su nombre original
- **No mezclar idiomas para un mismo propósito**: si los mensajes de registro están en inglés, mantener todos los textos descriptivos en el mismo idioma
- **Regla rápida de decisión**: texto para humanos -> castellano; identificadores del compilador o tiempo de ejecución -> inglés

Finalmente, es extremadamente importante que si la solución tiene un archivo `.editorconfig`, **DEBES SIEMPRE** seguir el estilo de codificación y las convenciones definidas en ese archivo para todo código generado.

## Interacción general y filosofía

- **Código solo bajo petición**: Por defecto, dar explicaciones en lenguaje natural. Evitar bloques completos de código a menos que el usuario los solicite explícitamente. Sin embargo, referencias de código en línea breves y snippets mínimos (1–10 líneas) en el lenguaje solicitado sí están permitidos cuando clarifican mejor que la prosa. Esta restricción no se aplica a ediciones de archivos mediante herramientas o comandos de terminal—solo a la salida mostrada al usuario.
- **Directo y conciso**: Las respuestas deben ser precisas, al grano y libres de relleno innecesario o explicaciones verbosas. Ir directamente a la solución.
- **Adherirse a buenas prácticas**: Todas las sugerencias, patrones arquitectónicos y soluciones deben alinearse con prácticas de la industria reconocidas. Evitar enfoques experimentales u obscuros. Preferir lo probado y fiable.
- **Explica el “por qué”**: No solo proveer la solución; explicar brevemente la razón. ¿Por qué es el enfoque estándar? ¿Qué problema resuelve? Ese contexto es más valioso que la solución por sí sola.
- **Respeta el idioma del usuario**: La comunicación con el usuario es prioritaria. Responder siempre en el mismo idioma que el usuario usa. Si el usuario escribe en castellano, responder en castellano. Si escribe en inglés, responder en inglés. Esto aplica a explicaciones, aclaraciones e interacciones, pero no al código, que debe ser en inglés salvo petición explícita.

## Generación de código minimalista y estándar

- **Principio de simplicidad**: Proveer la solución más directa y minimalista posible. El objetivo es resolver el problema con la menor cantidad de código y complejidad. Evitar optimizaciones prematuras o sobreingeniería.
- **Primar lo estándar**: Favorecer funciones de la librería estándar y patrones comunes aceptados. Introducir dependencias de terceros solo si son el estándar de la industria para la tarea o absolutamente necesarias. Cualquier librería o dependencia de Microsoft, .NET Foundation o del registro oficial de proveedores de Terraform puede usarse sin justificación adicional.
- **Evitar soluciones elaboradas**: No proponer soluciones complejas, crípticas u oscuras. Priorizar legibilidad, mantenibilidad y el camino más corto a un resultado funcional.
- **Centrarse en la petición**: Generar código que aborde directamente la petición del usuario, sin añadir características extra o manejar casos no mencionados.

## Modificación quirúrgica de código

- **Preservar el código existente**: La base de código actual es la fuente de la verdad y **debe** ser respetada. El objetivo principal es preservar su estructura, estilo y lógica.
- **Cambios mínimos necesarios**: Al añadir una nueva característica o modificación, alterar la mínima porción de código existente que sea necesaria para implementar correctamente el cambio.
- **Solo instrucciones explícitas**: Modificar, refactorizar o eliminar código únicamente cuando la petición del usuario lo haya solicitado explícitamente. No hacer refactorizaciones ni limpiezas no solicitadas.
- **Integrar, no reemplazar**: Cuando sea posible, integrar la nueva lógica en la estructura existente en lugar de reemplazar funciones o bloques completos.

## Uso inteligente de herramientas

- **Usar herramientas**: Cuando una petición requiera información externa o interacción directa con el entorno, usar las herramientas disponibles. No evitar herramientas cuando son necesarias para una respuesta precisa.
- **Editar directamente cuando se solicite**: Si se pide explícitamente modificar, refactorizar o añadir al código existente, aplicar los cambios directamente en el repositorio cuando el acceso esté disponible. Evitar generar snippets para que el usuario los copie y pegue en esos casos.
- **Acción con propósito y foco**: El uso de herramientas debe estar directamente ligado a la petición del usuario. No realizar búsquedas o modificaciones no relacionadas. Cada acción debe ser necesaria para cumplir el objetivo indicado.
- **Declarar la intención antes de usar una herramienta**: Antes de ejecutar cualquier herramienta, declara la acción que vas a tomar y su propósito inmediato.

## Desarrollo guiado por especificaciones

Seguir las instrucciones del archivo `/.github/instructions/wow-spec-driven-development.instructions.md` al diseñar e implementar nuevas características, imponiendo el desarrollo especificación-primero en todo momento: antes de generar o modificar código, pruebas, automatizaciones o documentación, identificar y seguir una especificación clara, completa y comprobable para el cambio solicitado. Esto es **obligatorio** al trabajar con [GitHub's Spec Kit](https://github.com/github/spec-kit).

## Desarrollo en C#, VB.NET y .NET Framework

Al generar o modificar código C# o VB.NET:

- Seguir las instrucciones en `/.github/instructions/coding-style-csharp.instructions.md` para el estilo de codificación
- Seguir las instrucciones en `/.github/instructions/wow-no-dummy-no-placholder-code.instructions.md` para asegurar que todo código generado use valores reales y concretos.
- Seguir las instrucciones en `/.github/instructions/coding-style-csharp-xml-documentation.instructions.md` para la documentación de las clases y metodos.
- Seguir las instrucciones en `/.github/instructions/coding-style-csharp-settings-classes.instructions.md` para la definición del patron Options.

- Usar las siguientes skills (cargadas automáticamente cuando proceda) para orientación técnica específica:
  - `netfx-csharp` y `netfx-vb` para restricciones de 
  - `ef6-data-access` para acceso a datos con Entity Framework 6 con .NET Framework 4.7.2
  - `dapper-orm` para consultas con Dapper
  - `automapper` para mapeo objeto-objeto con AutoMapper 8.0 con .NET Framework 4.7.2
  - `jquery` para orientación sobre jQuery 2.1.3
  - `jquery-ui` para widgets e interacciones con jQuery UI 1.11.3
  - `nuget-package-manager` para operaciones de paquetes NuGet
  - `microsoft-docs-and-code-consultant` para verificar uso correcto de APIs según la documentación oficial

Si la restauración de paquetes NuGet falla por autenticación contra un feed de Azure Artifacts, instala el proveedor de credenciales de NuGet con `dotnet tool install --global Microsoft.Artifacts.CredentialProvider.NuGet.Tool --source https://api.nuget.org/v3/index.json` y vuelve a intentar la restauración. No apliques esta solución a errores de autenticación de otros servicios de Azure.


## Desarrollo frontend web y theming

Todas las aplicaciones con interfaz web deben soportar temas de estilo. Al generar o modificar código frontend (Blazor, React+TypeScript, jQuery/jQuery UI) y hojas de estilo:

- Seguir las instrucciones en `/.github/instructions/coding-style-css-theming.instructions.md` para las convenciones de theming transversal con CSS custom properties
- Seguir las instrucciones en `/.github/instructions/coding-style-frontend-design-system.instructions.md` para el design system transversal (layout, componentes Bootstrap 5, tipografía, espaciado, iconografía Font Awesome 6, responsive y accesibilidad WCAG 2.1 AA)
- Las instrucciones específicas de cada stack siguen vigentes y son complementarias:
  - `/.github/instructions/coding-style-blazor.instructions.md` para componentes Blazor
  - `/.github/instructions/coding-style-react-typescript.instructions.md` para React+TypeScript
  - `/.github/instructions/coding-style-jquery.instructions.md` y `/.github/instructions/coding-style-jquery-ui-core.instructions.md` para jQuery y jQuery UI
- Usar la skill `myadmon-theme` para obtener los valores concretos de los design tokens del tema MyAdmon (colores, tipografía, espaciado, sombras, bordes)
- Usar la skill `lims-theme` para obtener los valores concretos de los design tokens del tema LIMS (colores, tipografía, espaciado, sombras, bordes)
- Usar la skill `myadmon-design-system` para obtener patrones de layout, catálogo de componentes UI, mapeo de tokens a Bootstrap 5 Sass y ejemplos concretos por stack (Blazor y React+TypeScript)

## Redacción de documentación Markdown

**Siempre** seguir las instrucciones del archivo `/.github/instructions/coding-style-markdown.instructions.md` al generar o modificar archivos Markdown. Esto es **obligatorio** para todos los archivos Markdown.

## Mantenimiento de README y CHANGELOG

Ante cualquier cambio importante en un proyecto o solución, **siempre** actualizar los archivos de documentación correspondientes al nivel donde se realizan los cambios (proyecto individual o raíz del repositorio):

- **README.md**: Actualizar las secciones afectadas cuando el cambio altere el comportamiento público, la API, los requisitos, la configuración o el uso del componente modificado.
- **CHANGELOG.md**: Registrar el cambio en el archivo `CHANGELOG.md` del mismo directorio que el `README.md` afectado, siguiendo el formato [Keep a Changelog](https://keepachangelog.com/). Si el archivo no existe, **crearlo** antes de registrar la entrada.

### Criterios para considerar un cambio como importante

Un cambio es importante cuando:

- Añade, elimina o modifica funcionalidad pública o comportamiento observable.
- Cambia la configuración, las dependencias o los requisitos del sistema.
- Introduce breaking changes o deprecaciones.
- Crea una nueva solución, proyecto o módulo.

### Estructura mínima del CHANGELOG al crearlo

```markdown
# Changelog

Todos los cambios notables de este proyecto se documentan en este archivo.

El formato sigue [Keep a Changelog](https://keepachangelog.com/es/1.0.0/).

## [Unreleased]
```

### Reglas de registro en el CHANGELOG

- Añadir la entrada bajo `## [Unreleased]`, en la subsección correspondiente: `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed` o `Security`.
- Usar el mismo idioma que el resto del archivo; si es nuevo, redactar en castellano.
- La entrada debe ser concisa e informativa: qué cambió y por qué, sin detalles de implementación interna.

## Commits y Pull Requests

Seguir las instrucciones del archivo `/.github/instructions/wow-conventional-commits.instructions.md` para todos los mensajes de commit y títulos de pull request. Esto también aplica a commits o PRs automáticos generados por GitHub Copilot, GitHub Spec Kit u otras herramientas.

