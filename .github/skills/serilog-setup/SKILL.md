---
name: serilog-setup
description: Configura Serilog en un proyecto ASP.NET Core generando la sección Serilog en appsettings.json y el método ConfigureLogger en BuilderExtensions.cs, produciendo dos archivos de log de salida: detallado (Verbose) y operacional 24×7 (Information).
disable-model-invocation: true
argument-hint: '<projectSlug> <applicationProperty> <logBasePath>'
---
# Configurar Serilog con dos archivos de log

## Directiva principal

Aplica las directrices definidas en [coding-style-csharp-serilog.instructions.md](../instructions/coding-style-csharp-serilog.instructions.md) y úsalas como fuente de verdad para todos los cambios.

Genera la configuración completa de Serilog para el proyecto objetivo produciendo exactamente **dos archivos de log de salida**:

1. `log_{projectSlug}_.log` — archivo detallado que captura todos los niveles desde `Verbose`.
2. `log_{projectSlug}_24x7_.log` — archivo operacional que captura niveles desde `Information`.

## Alcance y precondiciones

- Trabaja únicamente en el proyecto objetivo indicado por la persona usuaria.
- Localiza el `appsettings.json` principal del proyecto y actualiza o añade el nodo `Serilog` completo.
- Localiza `BuilderExtensions.cs` del proyecto y añade o actualiza el método privado `ConfigureLogger()`.
- Lee el contenido actual de cada archivo antes de modificarlo.
- No modifiques nodos de `appsettings.json` ajenos al nodo `Serilog`.
- No refactorices `BuilderExtensions.cs` más allá de añadir o actualizar `ConfigureLogger()` y su invocación en `Configure()`.

## Entradas

| Variable | Descripción | Ejemplo |
| ------------------------------- | ---------------------------------------------------- | -------------------- |
| `${input:projectSlug}` | Identificador del proyecto usado en los nombres de archivo de log | `api`, `worker` |
| `${input:applicationProperty}` | Valor de la propiedad `Application` en los logs | `Erp.Api` |
| `${input:logBasePath}` | Ruta relativa base desde el ejecutable para los archivos de log | `..\Logs\Erp\PRE` |

Si la persona usuaria no proporciona alguno de estos valores junto al comando, solicítalo antes de generar ningún cambio.

## Flujo de trabajo

1. Lee la instrucción base enlazada y úsala como fuente de verdad.
2. Recoge los tres valores de entrada (`projectSlug`, `applicationProperty`, `logBasePath`).
3. Construye las rutas definitivas de los dos archivos de log:
   - Detallado: `{logBasePath}\log_{projectSlug}_.log`
   - Operacional: `{logBasePath}\log_{projectSlug}_24x7_.log`
4. Localiza `appsettings.json` en el proyecto objetivo y aplica el nodo `Serilog` completo con las rutas anteriores.
5. Localiza `BuilderExtensions.cs` en el proyecto objetivo y añade o actualiza el método privado `ConfigureLogger()`.
6. Verifica que `ConfigureLogger()` se invoca desde el método público `Configure()` en la posición correcta (paso 2, tras `ConfigureKeyVault()`); si no existe la invocación, añádela.
7. Verifica que los paquetes NuGet requeridos están referenciados en el `.csproj` del proyecto; si faltan, indícalo en el resumen final sin añadirlos automáticamente.

## Expectativas de salida

- Aplica los cambios directamente en `appsettings.json` y `BuilderExtensions.cs`; no te limites a mostrar código.
- El nodo `Serilog` en `appsettings.json` debe contener:
  - Los dos sinks `File` con los niveles `Verbose` e `Information` respectivamente y las rutas construidas.
  - El sink `Debug` con el mismo `outputTemplate`.
  - `Properties.Application` con el valor de `${input:applicationProperty}`.
  - `Properties.UserData` con el valor por defecto `System`.
  - `Enrich` con `WithCorrelationId` y `FromLogContext`.
- El método `ConfigureLogger()` en `BuilderExtensions.cs` debe:
  - Llamar a `builder.Logging.ClearProviders()`.
  - Crear `Log.Logger` con `ReadFrom.Configuration(builder.Configuration)`.
  - Registrar Serilog con `builder.Host.UseSerilog()`.
  - Devolver `WebApplicationBuilder` para permitir el encadenamiento fluido.
- Ambos archivos de log deben usar exactamente el mismo `outputTemplate`:
  `{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Application}] [{UserData}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}`

## Aseguramiento de calidad

- No introduzcas marcadores de posición ni valores ficticios; usa las rutas reales construidas a partir de las entradas proporcionadas.
- Si ya existe un nodo `Serilog` en `appsettings.json`, muéstrale a la persona usuaria los cambios concretos antes de sobreescribir valores que ya tenían contenido distinto.
- No modifiques otros nodos de `appsettings.json` (p. ej. `Logging`, `ErpSettings`, `SecuritySettings`).
- No reformatees el resto de `BuilderExtensions.cs`; preserva el estilo existente.
- Comprueba que el `outputTemplate` es idéntico en los tres sinks (File ×2 y Debug); si difieren, corrígelos para que sean el mismo.
- Resume al final: archivos modificados, rutas de log generadas y paquetes NuGet verificados o ausentes.
