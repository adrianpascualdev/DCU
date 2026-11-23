---
name: apply-nuget-instructions
description: Aplica las directrices de empaquetado NuGet sobre todos los proyectos .csproj del workspace o sobre los que indique la persona usuaria.
disable-model-invocation: true
argument-hint: '[opcional] rutas, nombres o patrones de uno o varios .csproj; si se omite, se revisarán todos los .csproj del workspace'
---
# Aplicar directrices de empaquetado NuGet

## Directiva principal

Aplica y haz cumplir las reglas definidas en [coding-style-csharp-nuget-packaging.instructions.md](../instructions/coding-style-csharp-nuget-packaging.instructions.md) y trata ese archivo como la fuente de verdad.

## Alcance y precondiciones

- Interpreta cualquier texto adicional que acompañe la invocación `/apply-nuget-instructions` como una lista opcional de rutas, nombres de proyecto o patrones de archivos `.csproj`.
- Si la persona usuaria no aporta filtros, localiza todos los archivos `.csproj` del workspace y evalúalos.
- Si la persona usuaria aporta varios objetivos, acepta listas separadas por espacios, comas o saltos de línea.
- Si un objetivo indicado es ambiguo o no existe, acláralo antes de editar.
- Trabaja únicamente sobre los `.csproj` objetivo y sobre los archivos auxiliares necesarios para que esos proyectos cumplan la instrucción enlazada.
- Lee siempre el contenido actual de cada archivo antes de modificar nada.
- Haz cambios mínimos y preserva el estilo existente.
- Si un proyecto no entra en el ámbito de la instrucción enlazada y la persona usuaria no ha pedido adaptarlo explícitamente, indícalo y no fuerces cambios fuera de alcance.

## Entradas

- Los objetivos opcionales escritos por la persona usuaria junto al comando.
- El contenido actual de cada `.csproj` objetivo.
- La instrucción base enlazada y los archivos relacionados que esa instrucción exija crear, completar o actualizar.

## Flujo de trabajo

1. Lee la instrucción base enlazada y úsala como fuente de verdad para todos los cambios.
2. Determina el alcance:
   - Si hay filtros proporcionados por la persona usuaria, resuélvelos a uno o varios `.csproj` concretos.
   - Si no hay filtros, busca todos los `.csproj` del workspace.
3. Inspecciona cada proyecto objetivo y verifica qué requisitos de la instrucción ya cumple y cuáles faltan.
4. Aplica solo los cambios necesarios para que cada proyecto objetivo cumpla la instrucción, incluyendo cuando proceda propiedades de documentación XML, README empaquetado, estructura `Skills`, `Build` y `BuildTransitive`, archivos `.targets` y entradas `ItemGroup` del `.csproj`.
5. Cuando debas crear o actualizar `.targets`, rutas empaquetadas o prefijos internos, deriva los nombres del `PackageId` real del proyecto y evita cualquier nombre hardcodeado ajeno a ese proyecto.
6. Si faltan archivos o carpetas obligatorios, créalos siguiendo exactamente la instrucción base.
7. Si la persona usuaria indicó varios proyectos, procesa todos los válidos dentro del mismo encargo y separa claramente los omitidos o bloqueados.
8. Valida los archivos modificados con la comprobación más estrecha disponible.

## Expectativas de salida

- Aplica los cambios directamente en los archivos necesarios; no te limites a describirlos.
- Resume al final qué proyectos se actualizaron, cuáles ya cumplían la instrucción y cuáles quedaron fuera de alcance.
- Indica cualquier ambigüedad, bloqueo o validación no ejecutada.
- Si no hay ningún `.csproj` elegible, indícalo con claridad y detente sin editar.
- Si un requisito no puede aplicarse con seguridad porque falta información crítica, formula una pregunta concreta y detente antes de introducir cambios dudosos.

## Aseguramiento de calidad

- No contradigas ni dupliques innecesariamente la instrucción base; aplícala.
- No modifiques archivos no relacionados.
- No inventes nombres de paquete, rutas o archivos; dedúcelos del proyecto.
- Si un proyecto ya cumple la instrucción, no lo reformatees ni lo toques.
- Conserva el estilo y la estructura de cada `.csproj`, Markdown o `.targets` que toques.
- Mantén la solución general para los proyectos que apliquen estas directrices: evita nombres hardcodeados salvo cuando correspondan al `PackageId` real del proyecto procesado.
