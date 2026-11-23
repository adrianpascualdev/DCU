# Directrices detalladas de revisión

Usa esta referencia cuando necesites la lista completa de comprobación que respalda la skill `code-review`.

## Modelo de prioridad

| Prioridad | Áreas de foco |
| --- | --- |
| `CRITICAL` | Seguridad, corrección, breaking changes, pérdida de datos |
| `IMPORTANT` | Problemas graves de calidad, pruebas ausentes en rutas críticas, problemas evidentes de rendimiento, deriva arquitectónica |
| `SUGGESTION` | Legibilidad, optimización sin impacto funcional, incidencias menores de buenas prácticas, huecos de documentación |

## Principios básicos de revisión

- Sé específico y referencia archivos y líneas exactos siempre que sea posible
- Explica por qué importa el problema y qué puede salir mal
- Sugiere la corrección práctica más pequeña cuando sea evidente
- Agrupa preocupaciones relacionadas en lugar de repetir el mismo punto en varios hallazgos
- Prioriza defectos y riesgos concretos frente a comentarios generales de estilo
- Mantén una revisión pragmática; no toda sugerencia debe bloquear el cambio

## Lista de comprobación de revisión

### Calidad de código

- Comprueba si los nombres son descriptivos y encajan con el dominio
- Comprueba si funciones y métodos están centrados y tienen un tamaño razonable
- Busca lógica duplicada que debería vivir en un solo sitio
- Comprueba si la lógica compleja puede simplificarse con guard clauses o extracciones
- Busca números mágicos o cadenas mágicas que deberían ser constantes con nombre
- Comprueba si el manejo de errores es explícito y significativo
- Busca fallos silenciosos, excepciones tragadas o valores de retorno ignorados

### Seguridad

- Comprueba si hay secretos expuestos, tokens, contraseñas o información personal identificable
- Comprueba que toda entrada externa se valide y sanee antes de usarse
- Comprueba que el acceso a base de datos use consultas parametrizadas en lugar de concatenar cadenas
- Comprueba que la entrada de usuario renderizada se escape o sanee para evitar XSS
- Comprueba que autenticación y autorización se apliquen antes de acciones privilegiadas
- Comprueba que la criptografía use librerías consolidadas y valores seguros por defecto
- Comprueba si decisiones de dependencia o configuración debilitan la postura de seguridad

### Pruebas

- Comprueba si el comportamiento nuevo o modificado tiene cobertura
- Comprueba si las rutas críticas y los flujos de dinero, identidad o datos están probados
- Comprueba si los nombres de prueba describen el escenario y el resultado esperado
- Comprueba si las pruebas cubren condiciones límite, valores vacíos, valores nulos y casos de error
- Comprueba si las aserciones son específicas en lugar de verificaciones genéricas de truthiness
- Comprueba si las pruebas son deterministas y están aisladas del estado externo

### Rendimiento

- Comprueba si hay consultas N+1, I/O redundante o trabajo caro repetido dentro de bucles
- Comprueba si los conjuntos de resultados grandes deberían paginarse o streamearse
- Comprueba si el trabajo caro debería cachearse o memorizarse en la capa adecuada
- Comprueba si conexiones, streams y otros recursos se liberan correctamente
- Comprueba si algoritmos y estructuras de datos son adecuados para el volumen de datos

### Arquitectura

- Comprueba si las responsabilidades están separadas con limpieza entre capas y módulos
- Comprueba si la lógica de alto nivel depende de detalles de bajo nivel que no debería conocer
- Comprueba si el cambio introduce acoplamiento fuerte que dificulte pruebas o reutilización
- Comprueba si el código sigue patrones ya establecidos en el codebase

### Documentación

- Comprueba si las APIs públicas están documentadas donde el proyecto espere XML docs o JSDoc
- Comprueba si la lógica poco obvia necesita un comentario breve y explicativo
- Comprueba si cambios de setup o comportamiento de cara a la persona usuaria requieren actualizar el README
- Comprueba si los breaking changes están documentados con claridad

## Comprobaciones específicas por lenguaje

### C# y VB.NET

- Usa `PascalCase` para tipos, métodos y propiedades
- Usa `camelCase` para variables locales y parámetros
- En C#, prefiere `var` cuando el tipo sea evidente por la parte derecha
- Usa `nameof` en lugar de cadenas literales para referencias a miembros cuando esté disponible
- Usa `string.Empty` en lugar de `""` cuando esa sea la convención del codebase
- Usa `Async` y `Await` correctamente y pasa cancellation tokens donde la API lo soporte
- Valida la vida útil de recursos desechables con `using` o patrones equivalentes
- En EF6 y consultas de solo lectura, prefiere `AsNoTracking()` cuando evite tracking innecesario
- En Dapper, usa consultas parametrizadas con objetos anónimos o `DynamicParameters`

### TypeScript y React

- Evita `any`; usa `unknown` o tipos precisos
- Prefiere `const` frente a `let` y no uses `var`
- Usa interfaces o tipos explícitos para props y formas públicas
- Prefiere componentes funcionales y hooks frente a class components
- Asegura que los event handlers estén tipados explícitamente
- Asegura que los elementos interactivos sean accesibles por teclado y estén bien etiquetados
- Comprueba si nombres de props y handlers siguen las convenciones del proyecto

### jQuery y jQuery UI

- Prefiere eventos delegados para contenido dinámico
- Prefiere `$.ajax()` con manejo explícito de errores frente a helpers abreviados cuando el comportamiento importe
- Cachea selecciones jQuery repetidas en lugar de volver a consultar el DOM dentro de bucles
- Asegura que los handlers se desenlacen antes de volver a enlazarlos si existe riesgo de registro duplicado
- Usa Widget Factory para comportamiento jQuery UI con estado
- Destruye widgets antes de eliminar sus elementos del DOM

### Blazor

- Mantén los componentes centrados y de tamaño razonable
- Usa `@code` inline o code-behind, pero no ambos para la misma superficie lógica
- Prefiere parámetros explícitos del componente frente a comportamiento cascading oculto cuando la claridad importe
- Libera referencias JS interop y handlers de evento en `Dispose` o `DisposeAsync`

## Plantilla de reporte

Usa esta estructura al escribir hallazgos:

```text
**[SEVERITY] Categoría: título breve**

Descripción detallada del problema.

**Por qué importa:**
Explica el impacto o el modo de fallo.

**Corrección sugerida:**
Describe la corrección práctica más pequeña.
```

## Caso sin hallazgos

Si no descubres hallazgos, dilo explícitamente. Después menciona cualquier incertidumbre residual, como rutas sin probar, validación en runtime ausente o áreas que no hayas podido verificar con el contexto disponible.
