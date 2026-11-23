---
name: code-review
description: 'Revisa cambios en C#, VB.NET, TypeScript, React, Blazor, jQuery y jQuery UI para detectar errores, riesgos de seguridad, regresiones, pruebas ausentes, problemas de rendimiento y deriva arquitectónica. Úsala cuando te pidan revisar una pull request, inspeccionar un diff, auditar la calidad del código, encontrar fallos antes de hacer merge o producir hallazgos ordenados por severidad con referencias de archivo y correcciones sugeridas.'
version: '1.0.0'
category: 'Quality'
---

# Revisión de código

Revisa los cambios con mentalidad de detección de defectos. Prioriza bugs, riesgos de seguridad, regresiones, breaking changes y pruebas ausentes por encima de observaciones de estilo.

## Cuándo usar esta skill

- La persona usuaria pide una revisión de código, revisión de PR, revisión de diff, comprobación de merge readiness o evaluación de riesgos
- La persona usuaria quiere hallazgos, no trabajo de implementación
- El objetivo incluye C#, VB.NET, TypeScript, React, Blazor, jQuery o jQuery UI
- El propósito es identificar problemas de comportamiento, vulnerabilidades, regresiones de rendimiento, deriva arquitectónica o huecos de pruebas

## Enfoque de herramientas

- Usa `get_changed_files` para delimitar la superficie de revisión cuando el objetivo sea el worktree actual
- Usa `read_file` para inspeccionar los archivos tocados y solo el contexto cercano necesario para sostener un hallazgo
- Usa `grep_search` o `search_subagent` para localizar pruebas relacionadas, call sites o configuración solo cuando un hallazgo dependa de ello
- Usa `get_errors` cuando errores de build, lint o tipos formen parte de la superficie de revisión

## Flujo de revisión

1. Identifica la superficie exacta de revisión a partir del diff o de los archivos nombrados por la persona usuaria.
2. Lee solo el código tocado y el menor contexto cercano necesario para entender el comportamiento.
3. Evalúa los hallazgos en este orden: critical, important, suggestion.
4. Prioriza los problemas de comportamiento frente a los detalles estilísticos menores.
5. Verifica cada hallazgo contra el flujo de control real, los call sites cercanos o las pruebas relevantes antes de informarlo.
6. Deja de ampliar el alcance cuando ya tengas evidencia suficiente para hallazgos defendibles o evidencia suficiente de que no los hay.

## Requisitos de salida

- Empieza por los hallazgos y ordénalos por severidad
- Usa referencias de archivo precisas con números de línea siempre que sea posible
- Mantén cada hallazgo concreto, falsable y accionable
- Explica por qué importa el problema y sugiere una corrección cuando sea práctico
- Coloca las preguntas abiertas o supuestos después de los hallazgos, no antes
- Mantén el resumen de cambios breve y secundario
- Si no encuentras hallazgos, dilo explícitamente y menciona riesgos residuales o huecos de pruebas

## Modelo de severidad

| Severidad | Úsala cuando |
| --- | --- |
| `CRITICAL` | Haya una vulnerabilidad de seguridad, un bug de corrección, un breaking change o riesgo de pérdida de datos |
| `IMPORTANT` | Falten pruebas en una ruta crítica, exista un problema grave de calidad, un problema evidente de rendimiento o deriva arquitectónica |
| `SUGGESTION` | Se trate de legibilidad, mantenibilidad, documentación o una incidencia menor de buenas prácticas |

## Plantilla de hallazgo

```text
**[SEVERITY] Categoría: título breve**

Describe el problema de forma directa y referencia el archivo y la línea afectada.

**Por qué importa:**
Explica el impacto concreto o el modo de fallo.

**Corrección sugerida:**
Propón la corrección práctica más pequeña o el camino de decisión adecuado.
```

## Puntos a vigilar

- No empieces con un resumen cuando tengas hallazgos reales
- No informes bugs especulativos sin un modo de fallo o camino de código concreto
- No eleves preferencias menores de estilo por encima de corrección, seguridad o regresiones
- No señales pruebas ausentes salvo que el comportamiento haya cambiado o la ruta sea lo bastante crítica como para justificarlo
- No entierres el problema de mayor severidad detrás de elogios o contexto de fondo

## Material de referencia

Lee [review-guidelines.md](./references/review-guidelines.md) para consultar la lista detallada, las prioridades de revisión y las reglas específicas por lenguaje.
