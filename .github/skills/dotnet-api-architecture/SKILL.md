---
name: dotnet-api-architecture
description: "Arquitectura obligatoria para proyectos de API REST con .NET y ASP.NET Core Minimal API. Usar cuando se cree un nuevo proyecto API, se añadan endpoints, filtros, transformadores OpenAPI o configuración de servicios. Define estructura de carpetas, patrones de Program.cs, registro de servicios, endpoints Minimal API y elementos opcionales (SignalR, middleware personalizado, HostedServices)."
---

# Arquitectura de API REST con .NET (Minimal API)

## Cuándo usar esta skill

- Crear un nuevo proyecto de API REST en .NET.
- Añadir endpoints, filtros o transformadores OpenAPI a una API existente.
- Configurar `Program.cs`, `BuilderExtensions.cs`, `AppExtensions.cs`, `ServiceCollectionExtensions.cs` o `Constants.cs`.
- Decidir si incorporar SignalR, middleware personalizado, `IHostedService` o un proyecto `Proxy`.

---

## Estructura de carpetas

```text
src/
├── Contract/
│   ├── Exceptions/                # Excepciones públicas que Service propaga hacia las capas superiores
│   ├── Model/                     # DTOs, modelos de petición/respuesta y enums
│   │   └── Enums/
│   ├── Proxy/                     # Interfaces de proxy (IXxxProxy)  ← solo si existe Proxy
│   ├── Queries/                   # Interfaces de queries (lecturas de datos)
│   ├── Actions/                   # Interfaces de actions (escrituras / operaciones atómicas de datos)
│   └── Services/                  # Interfaces de los servicios de negocio implementados en Service/Services/
│
├── Commons/                       # OPCIONAL — solo si hay constantes o recursos compartidos entre varios proyectos
│   ├── Resources/                 # Archivos .resx reutilizables por varios proyectos (clase generada public)
│   └── CommonConstants.cs         # Constantes compartidas (p. ej., rutas de endpoints usadas por Api y Proxy)
│
├── Data/                          # OBLIGATORIO si el proyecto usa Entity Framework Core
│   ├── Model/                     # Entidades sin equivalente en Domain (clases mapeadas a tablas, sin lógica)
│   ├── Configurations/            # IEntityTypeConfiguration<T> de las entidades de Model/ y de Domain
│   ├── Migrations/                # Migraciones de EF Core, no existe si no existe Entity Framework Core
│   ├── Extensions/                # ServiceCollectionExtensions (registro de DbContext)
│   ├── Settings/                  # Clases de opciones de Data (cadena de conexión, etc.; public sealed, registradas desde Api)
│   └── Constants.cs               # Constantes internas de Data
│
├── Service/                       # Equivale a la capa de Aplicación.
│   ├── Queries/                   # Implementaciones internal sealed de queries (lecturas con ADO.NET/Dapper, o EF Core sin tracking si existe Data)
│   ├── Actions/                   # Implementaciones internal sealed de actions (ADO.NET/Dapper con stored procedures, o EF Core si existe Data)
│   ├── Services/                  # Implementaciones internal sealed de servicios que usan la lógica de negocio (llama al Dominio)
│   ├── Hubs/                      # SignalR hubs  ← SOLO si el proyecto usa SignalR
│   ├── Extensions/                # ServiceCollectionExtensions (registro DI de queries, actions, servicios e IDbConnection si aplica)
│   ├── Properties/
│   ├── Settings/                  # Clases de opciones del servicio (public sealed, registradas desde Api/Extensions/BuilderExtensions.cs)
│   ├── Assembly.cs                # InternalsVisibleTo para proyectos de test
│   └── Constants.cs               # Constantes internas del servicio
│
├── Proxy/                         # OPCIONAL — cliente HTTP tipado para consumir la API
│   ├── Base/                      # ProxyBase (lógica HTTP compartida)
│   ├── Extensions/                # ServiceCollectionExtensions (registro DI)
│   └── Constants.cs               # Constantes internas del proxy (las rutas compartidas con Api se toman de Commons)
│
├── Infrastructure.{Capacidad}/    # OPCIONAL — un proyecto por capacidad técnica o integración externa (p. ej., Infrastructure.Email, Infrastructure.BlobStorage, Infrastructure.Cryptography)
│   ├── Contract/                  # Interfaces públicas de la capacidad (p. ej., IEmailSender, IFileValidator)
│   ├── Services/                  # Implementaciones internal sealed de interfaces de Contract
│   ├── Settings/                  # Clases de opciones de la integración (public sealed, registradas desde Api/Extensions/BuilderExtensions.cs)
│   ├── Extensions/                # ServiceCollectionExtensions (registro DI)
│   └── Constants.cs               # Constantes internas de la capacidad
│
├── Api/
│   ├── ApisRest/                  # Endpoints Minimal API (un archivo por recurso)
│   ├── Filters/                   # IEndpointFilter (validación, autorización granular)
│   ├── OpenApi/                   # IOpenApiDocumentTransformer
│   ├── Resources/                 # Archivos .resx con mensajes de usuario usados solo por Api (los reutilizables van en Commons)
│   ├── Settings/                  # Clases de opciones (Options Pattern, public sealed)
│   ├── Services/                  # Servicios exclusivos de la capa API (internal sealed; si los hay y no deben estar relacionados con el Dominio/Lógica de Negocio)
│   ├── Data/                      # IDesignTimeDbContextFactory<TContext> (internal sealed)  ← SOLO si existe Data
│   ├── Middleware/                # Middleware personalizado  ← SOLO si hay lógica transversal no resoluble con IEndpointFilter
│   ├── Consumers/                 # Consumidores de mensajes (MassTransit, etc.)  ← SOLO si hay mensajería
│   ├── HostedServices/            # IHostedService / BackgroundService  ← SOLO si hay tareas en segundo plano
│   ├── Extensions/
│   │   ├── BuilderExtensions.cs   # Configuración del host: configuración, telemetría, Settings, CORS, OpenAPI, autenticación
│   │   ├── AppExtensions.cs       # Pipeline del host: middlewares y registro de endpoints
│   │   ├── ServiceCollectionExtensions.cs  # Registro DI de servicios de la aplicación
│   │   └── MigrationExtensions.cs # ← SOLO si se aplican migraciones EF Core al arranque
│   ├── Properties/
│   ├── Constants.cs               # Constantes internas de la API
│   ├── Program.cs                 # Solo invoca builder.Configure() y las extensiones de AppExtensions
│   ├── appsettings.json
│   └── appsettings.Development.json
│
└── Domain/                        # OBLIGATORIO si las entidades tienen lógica de negocio; no se crea si solo hay consultas o CRUD simples. Dominio según DDD.
    ├── Aggregates/                # Carpeta con los Agregados de entidades, dentro de ella contendrá una carpeta por cada uno de los Agregados, siendo el nombre de las subcarpetas el Dominio o Entidad principal. P. ej. si la entidad principal es SalesOrder, la subcarpeta será SalesOrderAgg y dentro de ella estará SalesOrder.cs y SalesOrderLine.cs
    └── DomainEvents/              # Eventos orientados al Dominio.


```

### Elementos opcionales — cuándo añadirlos

| Elemento | Cuándo añadirlo |
| --- | --- |
| Proyecto `Proxy/` completo | Solo si hay consumidores .NET externos que necesitan un cliente HTTP tipado |
| `Contract/Proxy/` (interfaces) | Solo si existe el proyecto `Proxy` |
| Proyecto `Commons/` | Solo si hay constantes o recursos que deben compartir varios proyectos (p. ej., rutas de endpoints entre `Api` y `Proxy`) |
| `Commons/Resources/` | Solo si hay archivos `.resx` reutilizados por varios proyectos |
| Proyectos `Infrastructure.{Capacidad}/` | Uno por cada capacidad técnica o integración externa (cifrado, validación de archivos, correo, almacenamiento, colas, etc.) |
| Proyecto `Domain/` | Obligatorio si las entidades tienen lógica de negocio; no se crea si solo hay consultas o CRUD simples |
| Proyecto `Data/` completo | Obligatorio si el proyecto usa Entity Framework Core; no se crea en caso contrario |
| `Service/Hubs/` | Solo si el proyecto usa SignalR |
| `Api/Data/` | Solo si existe el proyecto `Data` (design-time factory de EF Core) |
| `Api/Middleware/` | Solo si hay lógica transversal que no puede resolverse con `IEndpointFilter` |
| `Api/HostedServices/` | Solo si hay `IHostedService` o `BackgroundService` |
| `Api/Consumers/` | Solo si hay consumidores de mensajes (MassTransit, etc.) |

---

## Dirección de dependencias

```text
Api → Service → Contract
Api → Data → Contract          ← solo si existe Data
Service → Data → Contract      ← solo si existe Data
Proxy → Contract
Api → Infrastructure.* → Contract       ← solo si existen proyectos Infrastructure (registro DI)
Service → Infrastructure.*              ← solo si existen proyectos Infrastructure (uso de sus interfaces de Contract/)
Service → Domain                        ← solo si existe Domain
Data → Domain                           ← solo si existen Data y Domain (EF Core mapea directamente las entidades de dominio)
Api, Service, Data, Infrastructure.*, Proxy, Domain → Commons   ← solo si existe Commons y el proyecto necesita sus constantes o recursos
```

`Commons` no referencia a ningún otro proyecto de la solución; `Domain` solo puede referenciar a `Commons`.

Cadenas completas según los proyectos existentes:

| Proyectos | Cadena |
|-----------|--------|
| Sin `Data` ni `Domain` | `Api → Service → Contract` |
| Con `Data` | `Api → Service → Data → Contract` |
| Con `Data` y `Domain` | `Api → Service → Data → Contract` y `Data → Domain`, `Service → Domain` |
| Con `Infrastructure.*` | Se añade `Api → Infrastructure.* → Contract` y `Service → Infrastructure.*` |

Orden de capas, de superior a inferior:

1. `Api`
2. `Service`
3. `Data` e `Infrastructure.*` (mismo nivel; no se referencian entre sí)
4. `Contract` y `Domain` (no se referencian entre sí)
5. `Commons` (base)

`Proxy` queda fuera de este orden: solo referencia a `Contract` (y a `Commons` si existe) y ningún proyecto de la solución lo referencia.

Un proyecto solo puede referenciar a proyectos de niveles inferiores y únicamente según las flechas del diagrama; nunca a uno de su mismo nivel ni de un nivel superior. La aplicación consumidora (implementada con .NET) usa la API exclusivamente a través del proyecto `Proxy`. La API podrá ser consumida por clientes no .NET, pero el proyecto `Proxy` solo se implementa si se solicita para consumidores .NET que se benefician de un cliente HTTP tipado.

---

## Responsabilidades por capa

### Contract

- Solo contratos públicos: interfaces, DTOs, enums, excepciones e interfaces de proxy.
- `Exceptions/` contiene las excepciones que `Service` lanza hacia las capas superiores (`Api`, consumidores del NuGet).
- Las interfaces de queries van en `Queries/` (p. ej., `ISearchImagesQuery`).
- Las interfaces de actions van en `Actions/` (p. ej., `IUpdateFileNameAction`).
- Sin implementaciones. Se publica como NuGet.
- Documentación XML **obligatoria** en todos los tipos públicos y sus miembros. El warning CS1591 está configurado como error de compilación en el `.csproj` para garantizar el cumplimiento.
- Ver `/.github/instructions/coding-style-csharp-xml-documentation.instructions.md` y `/.github/instructions/coding-style-csharp-nuget-packaging.instructions.md`.

### Commons *(opcional)*

- Contiene `CommonConstants.cs` con las constantes que comparten varios proyectos de la solución (p. ej., rutas de endpoints usadas por `Api` al mapear y por `Proxy` al invocar).
- Es también el lugar recomendado para los archivos de recursos (`Resources/*.resx`) que deban reutilizarse en varios proyectos; la clase generada debe ser `public` para que sea accesible desde ellos. Ver `/.github/instructions/coding-style-csharp-resource-files.instructions.md`.
- `CommonConstants` es `public static` con clases anidadas `public static`, siguiendo la sección "Constantes compartidas (Commons)" de `coding-style-csharp-constants.instructions.md`. El nombre distinto evita la ambigüedad con la clase `Constants` interna de cada proyecto.
- No referencia a ningún otro proyecto de la solución.
- Si existe `Proxy`, `Commons` se publica también como NuGet, ya que `Proxy` depende de él.
- Documentación XML **obligatoria** en todos los tipos públicos y sus miembros.

### Data *(obligatorio si se usa Entity Framework Core)*

- Contiene el `DbContext`, las entidades EF (`Model/`), sus configuraciones (`Configurations/`) y las migraciones (`Migrations/`).
- Las entidades de `Model/` representan tablas **sin equivalente en el dominio** y **no contienen lógica de negocio**.
- Los agregados de `Domain` se persisten directamente: su mapeo se define con Fluent API en `Configurations/` (`IEntityTypeConfiguration<T>`), usando setters privados, campos de respaldo y owned types. No se duplican en `Model/`.
- Las entidades de `Model/` son `public sealed`; el `DbContext` es `public sealed`; las configuraciones son `internal sealed`.
- `Extensions/ServiceCollectionExtensions.cs` registra el `DbContext` con `AddDbContext<TContext>(...)`.
- `Settings/` contiene sus propias clases de opciones (`public sealed`), p. ej. la cadena de conexión; ver [Configuración](#configuración).
- Referencia a `Contract` y, si existen, a `Domain` y `Commons`.
- `Service` referencia a `Data` para acceder al `DbContext` y a las entidades.
- `Api` referencia a `Data` para el design-time factory de EF Core (`IDesignTimeDbContextFactory<TContext>`). La implementación se coloca en `Api/Data/` (p. ej., `Api/Data/CertificateStoreDbContextFactory.cs`) y se declara `internal sealed`.
- Documentación XML **obligatoria** en todos los tipos `public` e `internal` y sus miembros (incluidas las configuraciones de `Configurations/`). El warning CS1591 está configurado como error de compilación en el `.csproj` para garantizar el cumplimiento en los tipos públicos.

### Service

- Implementaciones `internal sealed` de las interfaces de `Contract`.
- `Extensions/ServiceCollectionExtensions.cs` (`public static`, namespace DI) registra las queries, actions y servicios (y el `IDbConnection` si no existe `Data`). Al estar en el mismo ensamblado puede registrar los tipos `internal`; `Api` solo invoca este método y únicamente conoce las interfaces de `Contract`.
- Los paquetes NuGet que usa la capa de aplicación (p. ej., proxies de APIs externas) los referencia `Service` y se registran en su `Extensions/ServiceCollectionExtensions.cs`.
- `Queries/` contiene las implementaciones de queries: lecturas directas con ADO.NET u, opcionalmente, Dapper, optimizadas para rendimiento; si existe `Data`, también pueden usar EF Core en modo solo lectura.
- `Actions/` contiene las implementaciones de actions: operaciones atómicas con ADO.NET u, opcionalmente, Dapper, que pueden invocar procedimientos almacenados; si existe `Data`, también pueden usar EF Core a través del `DbContext`.
- `Services/` contiene los servicios de negocio que coordinan el `Domain` (cargan agregados, invocan su lógica y persisten el resultado); consumen las interfaces de `Infrastructure.*/Contract/` cuando lo necesitan.
- Queries y actions se inyectan directamente donde se necesiten; no pasan por un servicio intermedio. Esta regla no afecta a `Services/`, que sí coordina el dominio.
- Settings en `Settings/` (`public sealed`, para que `Api/Extensions/BuilderExtensions.cs` pueda registrarlas).
- `Assembly.cs` declara `InternalsVisibleTo` para los proyectos de test.
- `Hubs/` solo si el proyecto usa SignalR.
- Ver `/.github/instructions/coding-style-csharp-service-implementation.instructions.md` y `/.github/instructions/coding-style-csharp-settings-classes.instructions.md`.

### Infrastructure.{Capacidad} *(opcional)*

- Un proyecto por capacidad técnica o integración externa (cifrado, validación de archivos, correo, almacenamiento, colas, etc.).
- `Contract/` contiene las interfaces `public` de la capacidad; `Services/` sus implementaciones `internal sealed`.
- `Extensions/ServiceCollectionExtensions.cs` registra los servicios de la capacidad; `Api/Extensions/BuilderExtensions.cs` lo invoca y registra sus Settings.
- `Service` consume únicamente las interfaces de `Contract/`, nunca las implementaciones.

### Domain *(obligatorio si las entidades tienen lógica de negocio)*

- Contiene agregados (`Aggregates/`) y eventos de dominio (`DomainEvents/`) con la lógica de negocio.
- Las entidades de dominio son las que EF Core persiste directamente (mapeadas desde `Data/Configurations/`); no contienen atributos ni dependencias de EF Core.
- No todas las tablas tienen entidad de dominio: las que no tienen lógica viven solo en `Data/Model/`.
- No se crea si el proyecto solo realiza consultas para mostrar datos o CRUD simples.
- Solo puede referenciar a `Commons` (si existe), p. ej. para usar recursos compartidos en sus mensajes; lo referencian `Service` y `Data`.
- Documentación XML **obligatoria** en todos los tipos públicos y sus miembros, igual que en `Contract`.

### Documentación XML en implementaciones

- Todos los tipos y miembros `public` e `internal` de todos los proyectos llevan documentación XML, propia o heredada. CS1591 solo detecta los `public`; los `internal` se verifican en revisión de código.
- Las clases que implementan una interfaz documentada usan `/// <inheritdoc/>` en lugar de repetir la documentación; la interfaz es la única fuente.
- Solo se documentan explícitamente los miembros que no provienen de la interfaz.

---

## Acceso a datos

Todo el acceso a datos se centraliza en `Service/Queries/` (lecturas) y `Service/Actions/` (escrituras y operaciones atómicas). Las interfaces de `Contract` son agnósticas a la tecnología de acceso a datos.

### Patrón Queries / Actions

- **Queries** (`Contract/Queries/` → `Service/Queries/`): lecturas directas con ADO.NET u, opcionalmente, Dapper, optimizadas para rendimiento. Si existe `Data`, también pueden usar EF Core, siempre priorizando la máxima velocidad: `AsNoTracking()`, proyección con `Select` directamente a DTOs de `Contract` (sin cargar entidades completas) y nunca `SaveChangesAsync()`, ya que una query no persiste nada. Una interfaz y una implementación por consulta lógica.
- **Actions** (`Contract/Actions/` → `Service/Actions/`): operaciones atómicas con ADO.NET u, opcionalmente, Dapper, que pueden invocar procedimientos almacenados para escrituras. Una action solo devuelve datos cuando el procedimiento almacenado realiza previamente alguna modificación; las lecturas puras son siempre queries. Si existe `Data`, también pueden usar EF Core a través del `DbContext` (p. ej., para persistir agregados de `Domain`). Una interfaz y una implementación por operación.
- Ambos se inyectan directamente en los endpoints o en otros componentes que los necesiten; no pasan por un servicio intermedio. Los servicios de `Service/Services/` sí coordinan el `Domain` y pueden usar queries y actions.

> **Importante**: `Queries/` y `Actions/` son **exclusivamente** para operaciones de acceso a datos. Los servicios de infraestructura (cifrado, validación de archivos, notificaciones, etc.) que no acceden a datos se ubican en un proyecto `Infrastructure.{Capacidad}`: interfaces públicas en `Contract/` e implementaciones `internal sealed` en `Services/`.

### Configuración

- Cada proyecto declara sus propias clases Settings (Options Pattern) en su carpeta `Settings/` y se reflejan en `appsettings.json`.
- Si existe `Data`, la cadena de conexión y los parámetros de base de datos se declaran en `Data/Settings/`. Si no existe `Data` (solo ADO.NET/Dapper), se declaran en `Service/Settings/`.
- `Api/Extensions/BuilderExtensions.cs` registra las Settings de `Data` de una de estas dos formas:
  - Enlazándolas directamente a su sección de `appsettings.json`, como parte de la configuración de la API.
  - Mapeándolas desde una Settings de `Api/Settings/` que contiene esos valores.
- La conexión se registra en el `ServiceCollectionExtensions` del proyecto que la posee: `DbContext` en `Data/Extensions/`; `IDbConnection` en `Service/Extensions/`.

### Estrategias de conexión

El proyecto puede usar solo EF Core, solo ADO.NET/Dapper o ambos sobre la misma base de datos:

| Estrategia | Registro en `ServiceCollectionExtensions` | Qué recibe la query o action |
|------------|-------------------------------------------|------------------------------|
| Solo EF Core | `AddDbContext<TContext>(...)` (desde `Data/Extensions/`) | `TContext` |
| Solo ADO.NET/Dapper | `IDbConnection` como **Scoped** (desde `Service/Extensions/`) | `IDbConnection` |
| Ambos | Solo `AddDbContext<TContext>(...)`; **no** se registra un `IDbConnection` independiente | `TContext`; las que usan Dapper obtienen la conexión con `context.Database.GetDbConnection()` |

En el caso combinado se comparte una única conexión por petición, de modo que EF Core y Dapper participan en la misma transacción cuando es necesario.

### ADO.NET / Dapper

- Cada petición HTTP obtiene su propia conexión (Scoped); se libera al terminar el scope.
- La query o action no debe crear ni gestionar la conexión directamente: la recibe por constructor (`IDbConnection` o `TContext`, según la estrategia).

### Entity Framework Core

#### MigrationExtensions — gestión de migraciones al arranque

Cuando el proyecto aplica migraciones EF Core en tiempo de ejecución, encapsular toda la lógica en una clase `public static class MigrationExtensions` ubicada en `Api/Extensions/`. La clase expone un único método de extensión genérico sobre `WebApplication`:

```csharp
public static class MigrationExtensions
{
    public const string AutoMigrateConfigKey = "Database:AutoMigrate";

    public static async Task ApplyMigrationsAsync<TContext>(this WebApplication app,
        string autoMigrateConfigKey = AutoMigrateConfigKey)
        where TContext : DbContext
    {
        if (app.Configuration.GetSection(autoMigrateConfigKey).Value is null)
        {
            throw new InvalidOperationException(
                $"[MigrationExtensions] La clave de configuración '{autoMigrateConfigKey}' no existe. " +
                "Asegúrate de definirla en appsettings.json o en las variables de entorno.");
        }

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();

        if (app.Configuration.GetValue<bool>(autoMigrateConfigKey))
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            var pending = (await db.Database.GetPendingMigrationsAsync()).ToArray();
            if (pending.Length > 0 && !app.Environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    $"Hay {pending.Length} migraciones pendientes: {string.Join(", ", pending)}");
            }
        }
    }
}
```

Reglas de uso:

- La clave de configuración se externaliza como constante pública `AutoMigrateConfigKey` para evitar *magic strings* y facilitar su referencia desde tests.
- **Excepción a `coding-style-csharp-constants.instructions.md`**: `AutoMigrateConfigKey` se declara `public const` dentro de `MigrationExtensions` y no en `Constants.cs`, porque es el valor por defecto de un parámetro de un método público y debe ser visible para quien lo invoca.
- El parámetro `autoMigrateConfigKey` es opcional y usa `AutoMigrateConfigKey` como valor por defecto, de modo que la llamada habitual es `app.ApplyMigrationsAsync<MyDbContext>()`.
- Si la clave **no existe** en la configuración, se lanza una `InvalidOperationException` con prefijo `[MigrationExtensions]` para que sea inequívocamente identificable en los logs.
- Si `AutoMigrate = false` y existen migraciones pendientes **fuera de Development**, se lanza una excepción que impide el arranque, evitando inconsistencias de esquema en producción.
- La llamada se realiza desde `AppExtensions` (o equivalente), como **primera operación** del pipeline, antes de registrar middleware, autenticación y endpoints.

Ejemplo en `AppExtensions`:

```csharp
await app.ApplyMigrationsAsync<CertificateStoreDbContext>();
```

- Registrar el `DbContext` con `AddDbContext<TContext>(...)`. Por defecto ya es **Scoped**, lo que significa una instancia por petición HTTP. No cambiar el lifetime a Singleton ni Transient salvo necesidad justificada.
- Si conviven EF Core y Dapper, no registrar un `IDbConnection` independiente para la misma base de datos; ver [Estrategias de conexión](#estrategias-de-conexión).
- `DbContext` **no es thread-safe**: siempre hacer `await` de las llamadas asíncronas inmediatamente; nunca compartir una instancia entre hilos.
- Si el proyecto necesita crear múltiples unidades de trabajo en un solo scope (p. ej., Blazor Server), usar `AddDbContextFactory<TContext>(...)` en lugar de `AddDbContext`. La factoría inyecta `IDbContextFactory<TContext>` y cada `DbContext` creado debe liberarse con `using`.
- El `DbContext` se declara siempre en `Data/` y recibe las opciones por constructor `DbContextOptions<TContext>`. Nunca se declara en `Service/`.

### Combinación de estrategias

Se permite usar Dapper y EF Core sobre la misma base de datos cuando el patrón Queries/Actions lo justifique (p. ej., queries con Dapper para lecturas optimizadas y actions con EF Core para persistencia de entidades de dominio). En ese caso, la separación entre `Queries/` y `Actions/` hace explícita la frontera entre estrategias.

---

### Proxy *(opcional)*

- Cliente HTTP tipado que implementa las interfaces de `Contract/Proxy/`.
- `ProxyBase` (en `Base/`) centraliza serialización JSON y lectura de `ProblemDetails`.
- Implementaciones concretas son `internal sealed`; solo se exponen las interfaces de `Contract`.
- `Extensions/ServiceCollectionExtensions.cs` registra el `HttpClient` tipado con URL base y timeout.
- Las rutas de los endpoints se toman de `Commons/CommonConstants.cs`, compartidas con `Api`; `Proxy/Constants.cs` solo contiene constantes internas del proxy (`internal`).
- Se publica como NuGet. Ver `/.github/instructions/coding-style-csharp-nuget-packaging.instructions.md`.
- Es exclusivamente para consumidores externos: ningún proyecto de la solución referencia ni registra su propio `Proxy`. Los proxies de **otras** APIs externas sí pueden consumirse como paquetes NuGet; se registran desde `Service/Extensions/` como cualquier otro paquete que use la capa de aplicación.

### Api

- Endpoints Minimal API organizados en `ApisRest/` (un archivo por recurso).
- `Filters/` para `IEndpointFilter` (`internal sealed`).
- `OpenApi/` para `IOpenApiDocumentTransformer` (`internal sealed`).
- `Extensions/` contiene las tres extensiones de arranque, todas `public static` en el namespace `Microsoft.Extensions.DependencyInjection`:
  - `BuilderExtensions.cs`: configuración del host (configuración, telemetría, Settings, CORS, versionado, OpenAPI, autenticación, SignalR). Su método `Configure()` invoca los `ServiceCollectionExtensions` de `Api`, `Service`, `Data` e `Infrastructure.*`.
  - `AppExtensions.cs`: pipeline del host (migraciones, middlewares, `UseAuthentication`, `MapOpenApi`, `MapHub`, `Map{Recurso}Endpoints`).
  - `ServiceCollectionExtensions.cs`: registro DI exclusivo de `Api` (servicios de `Api/Services/` y `AddHostedService<...>()`).
- `Program.cs` solo invoca `builder.Configure()` y las extensiones de `AppExtensions`.
- `Constants.cs` agrupa constantes internas con clases anidadas `internal static`.
- Si existe `Proxy`, las rutas de los endpoints se declaran en `Commons/CommonConstants.cs` (compartidas con `Proxy`) y no en `Api/Constants.cs`.
- Ver `/.github/instructions/coding-style-csharp-program.instructions.md` y `/.github/instructions/coding-style-csharp-extension-methods.instructions.md`.

---

## Reglas de visibilidad

| Elemento | Visibilidad |
|----------|-------------|
| Tipos en `Contract` | `public` |
| Entidades en `Data/Model/` | `public sealed` |
| Configuraciones en `Data/Configurations/` | `internal sealed` |
| `DbContext` en `Data/` | `public sealed` |
| `ServiceCollectionExtensions` en `Data/` | `public static` (namespace DI) |
| Implementaciones en `Service` (`Queries/`, `Actions/`, `Services/`) | `internal sealed` |
| Interfaces en `Infrastructure.*/Contract/` | `public` |
| Implementaciones en `Infrastructure.*/Services/` | `internal sealed` |
| Settings en `Service/Settings/`, `Data/Settings/` e `Infrastructure.*/Settings/` | `public sealed` |
| Settings en `Api/Settings/` | `public sealed` |
| Servicios en `Api/Services/` | `internal sealed` |
| Agregados y eventos en `Domain` | `public` |
| Filtros en `Api/Filters/` | `internal sealed` |
| Transformadores en `Api/OpenApi/` | `internal sealed` |
| Design-time factory en `Api/Data/` | `internal sealed` |
| Middleware en `Api/Middleware/`, consumidores en `Api/Consumers/` y servicios en `Api/HostedServices/` | `internal sealed` |
| Hubs en `Service/Hubs/` | `internal sealed` |
| Clase generada de `Commons/Resources/` | `public` |
| Implementaciones en `Proxy` | `internal sealed` |
| `Constants` en `Service`, `Data`, `Infrastructure.*`, `Proxy` y `Api` (uso interno, según `coding-style-csharp-constants.instructions.md`; `Contract` no contiene `Constants`) | `internal static` |
| `CommonConstants` en `Commons` (compartidas entre proyectos) | `public static` |
| Clases de endpoints en `ApisRest/` | `public static` |
| `ServiceCollectionExtensions` | `public static` (namespace DI) |
| `BuilderExtensions` y `AppExtensions` | `public static` (namespace DI) |

---

## Api/ApisRest/ — reglas de endpoints

- Un archivo por recurso (p. ej., `ImageEndpoints.cs`).
- Método de extensión `Map{Recurso}Endpoints(this IEndpointRouteBuilder)`.
- Cada endpoint declara: `.WithName`, `.WithTags`, `.WithSummary`, `.WithDescription`, `.Produces<T>`, `.ProducesProblem`, `.ProducesValidationProblem`.
- Handlers como métodos `private static` en la misma clase.
- **Siempre** declarar `CancellationToken cancellationToken` como parámetro en todos los handlers. ASP.NET Core Minimal API lo resuelve automáticamente desde `HttpContext.RequestAborted`. 
- Propagar el `CancellationToken` a todas las llamadas asíncronas del handler.
- Usar `TypedResults` (no `Results`).
- Versionar con `NewApiVersionSet()` y `MapToApiVersion(...)`.

---

## CORS — comportamiento por entorno

Se configura en `BuilderExtensions` y se aplica en `AppExtensions`.

| Entorno | Comportamiento |
|---------|----------------|
| Development | `SetIsOriginAllowed` acepta cualquier puerto de `localhost` |
| Resto | `WithOrigins([...])` desde `CorsSettings.AllowedOrigins`; warning si la lista está vacía |

---

## Opcionales en el arranque

| Elemento | Dónde | Condición |
|----------|-------|-----------|
| `AddAuthentication/AddAuthorization` | `BuilderExtensions` | Solo si los endpoints requieren identidad |
| `UseAuthentication/UseAuthorization` | `AppExtensions` | Solo si los endpoints requieren identidad |
| `AddSignalR()` | `BuilderExtensions` | Solo si el proyecto usa SignalR |
| `MapHub<...>(...)` | `AppExtensions` | Solo si el proyecto usa SignalR |
| `UseMiddleware<...>()` | `AppExtensions` | Solo si existe `Api/Middleware/` |
| `AddHostedService<...>()` | `ServiceCollectionExtensions` | Solo si existe `Api/HostedServices/` |

---

## Documentación interactiva de la API — Scalar

Todos los proyectos API **deben** incorporar [Scalar](https://github.com/scalar/scalar) como explorador interactivo de la API en desarrollo:

- Paquete: `Scalar.AspNetCore`.
- Registrar OpenAPI con `builder.Services.AddOpenApi()` en `BuilderExtensions`.
- En el pipeline (solo en Development), desde `AppExtensions`: `app.MapOpenApi()` seguido de `app.MapScalarApiReference()`.
- Scalar reemplaza a Swagger UI; **no** instalar `Swashbuckle`.
- El bloque completo debe estar condicionado a `app.Environment.IsDevelopment()` para no exponer la documentación en producción.

---

## Instrucciones relacionadas

| Instrucción | Cuándo leerla |
| --- | --- |
| `/.github/instructions/coding-style-csharp-program.instructions.md` | Estructura detallada de `Program.cs` |
| `/.github/instructions/coding-style-csharp-builder-extensions.instructions.md` | Configuración del host en `BuilderExtensions.cs` |
| `/.github/instructions/coding-style-csharp-app-extensions.instructions.md` | Pipeline y endpoints en `AppExtensions.cs` |
| `/.github/instructions/coding-style-csharp-service-implementation.instructions.md` | Implementaciones `internal sealed` en `Service` |
| `/.github/instructions/coding-style-csharp-settings-classes.instructions.md` | Clases de Settings y Options Pattern |
| `/.github/instructions/coding-style-csharp-constants.instructions.md` | Constantes y clases anidadas |
| `/.github/instructions/coding-style-csharp-extension-methods.instructions.md` | `ServiceCollectionExtensions` |
| `/.github/instructions/coding-style-csharp-xml-documentation.instructions.md` | Documentación XML en castellano (obligatoria en tipos y miembros `public` e `internal` de todos los proyectos, propia o heredada con `<inheritdoc/>`) |
| `/.github/instructions/coding-style-csharp-resource-files.instructions.md` | Archivos `.resx`: en `Commons` si se reutilizan en varios proyectos; en `Api` si solo los usa la API |
| `/.github/instructions/coding-style-csharp-nuget-packaging.instructions.md` | Empaquetado NuGet de `Contract`, `Proxy` y `Commons` |