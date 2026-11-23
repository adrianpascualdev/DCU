---
applyTo: "**/Extensions/**/*.cs, **/*Extensions.cs"
description: "Directrices para crear métodos de extensión siguiendo los patrones establecidos para el registro de servicios, APIs fluidas y utilidades de dominio en C#."
---

# Directrices de desarrollo de métodos de extensión

Instrucciones para crear métodos de extensión siguiendo los patrones arquitectónicos establecidos en C#.

## Contexto del proyecto

- **Framework objetivo**: .NET 10 con C# 14 salvo que se indique lo contrario en el proyecto. En caso de usar una versión anterior, seguir las convenciones de esa versión específica. En caso de trabajar con varios Framework hacer seguir las convenciones de la versión menor para que sea compatible.
- **Convención**: las extensiones son descubribles y siguen patrones de API fluida
- **Estrategia de espacios de nombres**: usar espacios de nombres de Microsoft para las extensiones de inyección de dependencias y habilitar el autodescubrimiento.

## Validación de parámetros (obligatorio)

Todos los métodos de extensión públicos **deben** validar el parámetro `this` usando `ArgumentNullException.ThrowIfNull` como primera sentencia. Este es un patrón obligatorio para garantizar un comportamiento fail-fast y mensajes de error claros.

> **Nota**: en métodos de extensión se usa `ArgumentNullException.ThrowIfNull` (y familia `ArgumentException.ThrowIfNullOrWhiteSpace`, etc.) en lugar de `Guard` de `CommunityToolkit.Diagnostics`. La razón es que es el patrón estándar de .NET, no añade una dependencia externa y es el que usa Microsoft en su propio código de extensiones. `Guard` se reserva para servicios de dominio y lógica de negocio. Ver `/.github/instructions/coding-style-csharp.instructions.md` para la estrategia completa de validación por capa.

```csharp
public static IServiceCollection AddMyService(this IServiceCollection services)
{
    ArgumentNullException.ThrowIfNull(services);

    // implementación
    return services;
}
```

### Reglas de validación

- **Validar siempre el parámetro `this`** en los métodos de extensión públicos
- **Validar otros parámetros de referencia no nulos** que sean necesarios para que el método funcione
- **Usar `ArgumentNullException.ThrowIfNull`** (no comprobaciones manuales de nulo con `throw new ArgumentNullException`)
- **Colocar la validación en el punto de entrada del método**, antes de cualquier otra lógica
- **Incluir `<exception cref="ArgumentNullException">` en la documentación XML** para los parámetros validados

### Ejemplo de validación con múltiples parámetros

```csharp
public static IServiceCollection AddMyService(this IServiceCollection services, IConfiguration configuration, string sectionName)
{
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);
    ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

    // Implementation
    return services;
}
```

### Métodos de implementación privados

Al usar el patrón de múltiples sobrecargas, la validación debe ocurrir en cada método público antes de delegar a la implementación privada. El método privado `Inner*` no necesita validación redundante:

```csharp
// El método público valida
public static IServiceCollection AddMyService(this IServiceCollection services)
{
    ArgumentNullException.ThrowIfNull(services);
    return InnerAddMyService(services, null);
}

// El método privado confía en los llamadores (todos son internos)
private static IServiceCollection InnerAddMyService(IServiceCollection services, string? key)
{
    // Sin validación redundante
    // implementación
    return services;
}
```

## Convenciones de espacios de nombres

### Extensiones de IServiceCollection

Colocar las extensiones en espacios de nombres de Microsoft para el autodescubrimiento:

```csharp
// BIEN - Autodescubrible sin directiva using explícita
#pragma warning disable IDE0130 // El espacio de nombres no coincide con la estructura de carpetas
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130 // El espacio de nombres no coincide con la estructura de carpetas
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMyService(this IServiceCollection services)
        {
            // implementación
            return services;
        }
    }
}
```

```csharp
// BIEN - Para extensiones de host builder
#pragma warning disable IDE0130 // El espacio de nombres no coincide con la estructura de carpetas
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130 // El espacio de nombres no coincide con la estructura de carpetas
{
    public static class IHostApplicationBuilderExtensions
    {
        public static IHostApplicationBuilder AddMyService(this IHostApplicationBuilder builder, string assemblyName)
        {
            // implementación
            return builder;
        }
    }
}
```

### Extensiones específicas de dominio

Colocarlas en el espacio de nombres del dominio para importación explícita:

```csharp
namespace {ProjectName}.Extensions
{
    public static class ChatMessageContentExtensions
    {
        public static bool HasReachedTokenLimit(this ChatMessageContent content)
        {
            // implementación
        }
    }
}
```

## Patrón de "Fluent API"

Salvo que se indique lo contrario, todos los métodos de extensión deben devolver el tipo extendido para permitir el encadenamiento:

```csharp
public static IServiceCollection AddManager(this IServiceCollection services, IConfiguration configuration)
{
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddManager(configuration)
            .AddSingleton<IManager, Manager>();

    return services;  // Permitir encadenamiento
}
```

### Patrón de uso

```csharp
builder.Services
       .AddHttpClient()
       .AddDistributedMemoryCache()
       .AddManager(builder.Configuration)
       .AddHealthChecks();
```

## Patrón de múltiples sobrecargas

Proporcionar múltiples sobrecargas que deleguen en una única implementación privada:

```csharp
public static class IServiceCollectionExtensions
{
    // Sobrecarga mínima
    public static IServiceCollection AddMyService(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(services);
        return InnerAddMyService(services, null, null, lifetime);
    }

    // Sobrecarga con clave
    public static IServiceCollection AddMyService(
        this IServiceCollection services,
        string? key,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(services);
        return InnerAddMyService(services, key, null, lifetime);
    }

    // Sobrecarga con acción
    public static IServiceCollection AddMyService(
        this IServiceCollection services,
        Action<MyType>? myTypeActions,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(services);
        return InnerAddMyService(services, null, (myType, _) => myTypeActions?.Invoke(myType), lifetime);
    }

    // Sobrecarga completa con acceso al proveedor de servicios
    public static IServiceCollection AddMyService(
        this IServiceCollection services,
        Action<MyType, IServiceProvider>? myTypeActions,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(services);
        return InnerAddMyService(services, null, myTypeActions, lifetime);
    }

    // Implementación privada única (sin validación redundante)
    private static IServiceCollection InnerAddMyService(
        IServiceCollection services,
        string? key,
        Action<MyType, IServiceProvider>? myTypeActions,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        // Implementación completa aquí
        return services;
    }
}
```

## Patrón de registro mediante factory

Para servicios que necesiten creación por instancia:

```csharp
public static IServiceCollection AddServiceFactory(this IServiceCollection services, string containerId)
{
    ArgumentNullException.ThrowIfNull(services);
    ArgumentException.ThrowIfNullOrWhiteSpace(containerId);

    services.AddSingleton<Func<string, Task<ServiceStore>>>(serviceProvider =>
    {
        var serviceOptions = serviceProvider.GetRequiredService<IOptionsMonitor<ServiceOptions>>().CurrentValue;
        var connectionString = $"AccountEndpoint={serviceOptions.Endpoint};AccountKey={serviceOptions.AuthKey};";

        return async conversationId =>
        {
            var serviceClient = new ServiceClient(connectionString);
            var databaseResponse = await serviceClient.CreateDatabaseIfNotExistsAsync(serviceOptions.Database);
            var containerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(containerId, "/id");

            return new ServiceStore(serviceClient, serviceOptions.Database, containerId, conversationId);
        };
    });

    return services;
}
```

Uso:

```csharp
// En el servicio
private readonly Func<string, Task<ServiceStore>> _serviceStoreFactory;

public async Task<ServiceStore> GetServiceStoreAsync(string conversationId)
{
    return await _serviceStoreFactory(conversationId);
}
```

## Extensiones de enlace de opciones

Crear extensiones para los patrones de opciones habituales:

```csharp
public static IServiceCollection AddMyServiceWithOptions<TOptions>(this IServiceCollection services, IConfiguration configuration, string? sectionName = null) where TOptions : class
{
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddOptionsWithValidateOnStart<TOptions>()
            .Bind(configuration.GetSection(sectionName ?? typeof(TOptions).Name))
            .ValidateDataAnnotations();

    services.AddSingleton<IMyService, MyService>();

    return services;
}
```

## Extensiones de configuración

```csharp

#pragma warning disable IDE0130 // El espacio de nombres no coincide con la estructura de carpetas
namespace Microsoft.Extensions.Configuration
#pragma warning restore IDE0130 // El espacio de nombres no coincide con la estructura de carpetas
{
    public static class IConfigurationExtensions
    {
        public static T GetRequiredValue<T>(this IConfiguration configuration, string key)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentException.ThrowIfNullOrWhiteSpace(key);

            var value = configuration.GetValue<T>(key);

            if (value is null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                throw new MissingConfigurationException($@"Falta la configuración requerida: '{key}'");
            }

            return value;
        }

        public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            var connectionString = configuration.GetConnectionString(name);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new MissingConfigurationException($@"Falta la cadena de conexión: '{name}'");
            }

            return connectionString;
        }
    }
}
```

## Convenciones de nomenclatura

| Tipo extendido | Nombre de la clase | Nombre del archivo |
| --- | --- | --- |
| `IServiceCollection` | `IServiceCollectionExtensions` | `IServiceCollectionExtensions.cs` |
| `IHostApplicationBuilder` | `IHostApplicationBuilderExtensions` | `IHostApplicationBuilderExtensions.cs` |
| `IConfiguration` | `IConfigurationExtensions` | `IConfigurationExtensions.cs` |
| `string` | `StringExtensions` | `StringExtensions.cs` |
| `ChatMessageContent` | `ChatMessageContentExtensions` | `ChatMessageContentExtensions.cs` |
| Tipo de dominio `Foo` | `FooExtensions` | `FooExtensions.cs` |

## Documentación XML

Incluir siempre documentación XML en los métodos de extensión públicos. Esta documentación debe estar en castellano (manteniendo identificadores de código en inglés):

```csharp
/// <summary>
/// Agrega los servicios de Manager a la colección de servicios.
/// </summary>
/// <param name="services">La colección de servicios donde se registrarán los servicios.</param>
/// <param name="managerActions">Acciones opcionales para configurar el manager.</param>
/// <param name="lifetime">Tiempo de vida del servicio manager. El valor predeterminado es Singleton.</param>
/// <returns>La colección de servicios para permitir encadenamiento.</returns>
/// <exception cref="ArgumentNullException">Se produce cuando services es null.</exception>
public static IServiceCollection AddManager( this IServiceCollection services, Action<Manager>? managerActions = null, ServiceLifetime lifetime = ServiceLifetime.Singleton)
{
    ArgumentNullException.ThrowIfNull(services);
    return InnerAddManager(services, null, managerActions, lifetime);
}
```

## Convenciones de clases estáticas

- Usar `static class` para todos los contenedores de extensiones
- Mantener las clases de extensión enfocadas en un único tipo o dominio
- Usar el modificador de parámetro `this` como primer parámetro
- Marcar los helpers internos como `private static`

### Regla de agrupación: un archivo por tipo extendido y dominio

Cuando varios métodos de extensión operan sobre el **mismo tipo** (`IServiceCollection`, `IHostApplicationBuilder`, `IConfiguration`, etc.) y pertenecen al **mismo dominio funcional**, deben agruparse en **una sola clase estática** dentro de **un único archivo**.

- La regla "un tipo por archivo" aplica a clases, interfaces y enums distintos; **no** obliga a dividir artificialmente una clase estática de extensiones en múltiples archivos.
- Criterio de agrupación: **mismo tipo extendido + mismo dominio funcional = una clase, un archivo**.
- Solo se justifica un archivo adicional cuando los métodos pertenecen a dominios claramente diferenciados (p. ej., `IServiceCollectionCacheExtensions.cs` frente a `IServiceCollectionMessagingExtensions.cs`).

```csharp
// CORRECTO: todos los métodos de IServiceCollection del dominio de pedidos en un solo archivo
// Archivo: Extensions/IServiceCollectionExtensions.cs
#pragma warning disable IDE0130 // El espacio de nombres no coincide con la estructura de carpetas
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130 // El espacio de nombres no coincide con la estructura de carpetas
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddOrderRepository(this IServiceCollection services) { ... }
        public static IServiceCollection AddOrderService(this IServiceCollection services) { ... }
        public static IServiceCollection AddOrderValidator(this IServiceCollection services) { ... }
    }
}
// INCORRECTO: dividir sin necesidad
// Extensions/IServiceCollectionOrderRepositoryExtensions.cs  ← evitar
// Extensions/IServiceCollectionOrderServiceExtensions.cs     ← evitar
// Extensions/IServiceCollectionOrderValidatorExtensions.cs   ← evitar
```

> **Ante la duda, preferir el enfoque más simple, preguntar al usuario o solicitar especificaciones más detalladas**

## Convenciones de ubicación y organización

- Todos los métodos de extensión deben declararse en archivos ubicados dentro de una carpeta `Extensions` del proyecto correspondiente.
- Todos los archivos que contengan métodos de extensión deben terminar obligatoriamente con el sufijo `Extensions.cs`.
- Todas las clases estáticas que contengan métodos de extensión deben terminar obligatoriamente con el sufijo `Extensions`.

### Regla general para tipos del propio proyecto

Cuando un método de extensión amplíe una `class` o un `record` definido en el propio proyecto, se deben cumplir todas estas reglas:

1. El archivo debe ubicarse dentro de la carpeta `Extensions` del proyecto.
2. Dentro de `Extensions`, se debe reproducir la misma jerarquía funcional de carpetas del tipo extendido.
3. El nombre del archivo debe ser `{TypeName}Extensions.cs`.
4. El nombre de la clase debe ser `{TypeName}Extensions`.
5. El namespace debe insertarse bajo `.Extensions` inmediatamente después del namespace raíz del proyecto.

Regla de transformación:

- Tipo original: `{ProjectRoot}.{Path}.{TypeName}`
- Archivo de extensión: `Extensions/{Path}/{TypeName}Extensions.cs`
- Namespace de extensión: `{ProjectRoot}.Extensions.{Path}`
- Clase de extensión: `{TypeName}Extensions`

Ejemplo:

```text
Project.Contracts
├─ Models
│  └─ Dtos
│     └─ CustomerDto.cs
└─ Extensions
   └─ Models
      └─ Dtos
         └─ CustomerDtoExtensions.cs
