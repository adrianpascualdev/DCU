---
applyTo: "**"
description: "Exige generación de código real y ejecutable. Sin marcadores de posición ni segmentos de relleno."
---

# Instrucciones de generación de código con valores reales en lugar de `Placeholders Instructions`

Es fundamental que todo el código generado en este repositorio se adhiera al principio de usar valores reales y concretos en lugar de marcadores de posición o segmentos de relleno. Esta directriz garantiza que todo el código sea inmediatamente ejecutable, funcional, comprobable y mantenible, mejorando la fiabilidad y la calidad de nuestra base de código.

## Contexto del proyecto

Esta directriz se aplica a todos los flujos de generación de código de este repositorio y abarca todos los lenguajes de programación y entornos de scripting, incluyendo, entre otros: TypeScript, JavaScript, Python, Bash, PowerShell, C#, Visual Basic, XML, HTML y archivos de configuración (tanto JSON como YAML).

## Objetivo

Garantizar que todo el código generado sea inmediatamente ejecutable y esté libre de marcadores de posición no funcionales o segmentos de relleno.

## Reglas fundamentales

1. **Solo código ejecutable**
   Todo el código generado debe ser válido y directamente ejecutable. Sin valores de marcador de posición ni código de «relleno» que rompa la lógica o la ejecución.

2. **Usar valores reales y concretos**
   Cuando un valor o parámetro sea conocido (codificado directamente, descubierto o definido por contexto), insertarlo directamente en el código.

3. **Usar sintaxis paramétrica solo cuando sea necesario**
   Usar parámetros únicamente cuando el código requiera legítimamente una entrada dinámica. Emplear el enfoque idiomático de cada lenguaje:
   - **Bash**: `${VARIABLE}` o `$VARIABLE` para variables de entorno
   - **Python**: parámetros de función, argumentos con nombre u objetos de configuración
   - **TypeScript/JavaScript**: parámetros de función, argumentos desestructurados u objetos de configuración
   - **C#**: parámetros de método, inyección de constructor, proveedores de configuración (`IConfiguration`, `IOptions<T>`) o valores de `appsettings.json`
   - **Visual Basic**: usar parámetros de función, variables de entorno o archivos de configuración en lugar de marcadores de posición
   - **HTML/XML**: usar contenido o atributos reales en lugar de marcadores de posición como `<!-- TODO -->` o `<PLACEHOLDER>`
   - **JSON/YAML**: usar valores reales o referencias a variables de entorno (p. ej., `${ENV_VAR}`) en lugar de marcadores de posición

4. **No generar bloques inutilizables o de relleno**
   Evitar segmentos como:
   - Comentarios genéricos: `# your_value_here`, `// TODO: fill this in`, `<!-- PLACEHOLDER -->`
   - Marcadores de pseudocódigo: `<TO_BE_FILLED>`, `[INSERT_VALUE]`, `***REPLACE_ME***`
   - Valores por defecto no funcionales: `"dummy"`, `"test"`, `"foo"`, `"bar"` (salvo que sean legítimamente necesarios para ejemplos o pruebas)
   - Líneas vacías o comentadas que no tengan ningún propósito funcional

   Cada línea debe tener un propósito claro y funcional.

5. **Exigir cumplimiento en todos los lenguajes**
   Aplicar estas instrucciones para cada lenguaje. Adoptar la forma idiomática de representar parámetros cuando sea necesario (p. ej., variables de entorno en Bash, argumentos de función en Python/TypeScript, etc.).
   - **TypeScript/JavaScript**: usar parámetros de función, variables de entorno (`process.env`) o módulos de configuración
   - **Python**: usar parámetros de función, variables de entorno (`os.environ`) o archivos de configuración
   - **Bash**: usar variables de entorno (`$VAR` o `${VAR}`) o argumentos de línea de comandos (`$1`, `$2`)
   - **C#**: usar parámetros de método (incluido `Main(string[] args)` para aplicaciones CLI), variables de entorno mediante `Environment.GetEnvironmentVariable` y clases de configuración/opciones (por ejemplo, el «Options Pattern» en .NET)
   - **Visual Basic**: equivalente a C#; usar parámetros de función, variables de entorno o archivos de configuración

6. **Autocorrección**
   Revisar sistemáticamente todo el código generado en busca de marcadores de posición ocultos o artefactos no funcionales antes de hacer commit, push o crear una pull request. Hacerse las siguientes preguntas:
   - ¿Puede ejecutarse este código sin modificaciones?
   - ¿Son todos los valores concretos o están correctamente parametrizados?
   - ¿Necesitaría otro desarrollador «rellenar» algo antes de usar este código?

## Resultado esperado

- Ninguna salida contiene marcadores de posición no funcionales ni variables de «relleno».
- Todo el código generado está listo para ejecutar o es utilizable como función, módulo o script real.
- El código sigue las mejores prácticas de su lenguaje en cuanto a configuración, inyección de dependencias y seguridad de tipos.
- La refactorización, la automatización y los flujos de trabajo en equipo se vuelven más fiables y menos propensos a errores.
- Generación de código consistente y de alta calidad en todas las herramientas y flujos de trabajo de desarrollo.

## Aplicabilidad

**IMPORTANTE**: En el caso que se use `C#`, la solución debe ser compatible con Visual Studio 2026 y VSCode. Todas las soluciones que agrupan proyectos deben usar el formato .slnx (XML Solution format). No se debe utilizar el formato clásico .sln basado en texto plano.

**NOTA**: Si un valor no se conoce en el momento de la generación pero es necesario, usar la forma paramétrica idiomática del lenguaje destino. **Nunca un marcador de posición ficticio**.

> **En caso de duda, preferir el enfoque más simple, preguntar al usuario o solicitar especificaciones más detalladas**
