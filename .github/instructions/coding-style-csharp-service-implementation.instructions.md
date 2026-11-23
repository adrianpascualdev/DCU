---
applyTo: "**/Services/**/*.cs, **/Abstractions/**/*.cs"
description: "Directrices para implementar clases de servicio e interfaces siguiendo patrones establecidos para inyección de dependencias, métodos factoría y manejo de resultados."
---

# Directrices de implementación de servicios

Instrucciones para implementar clases de servicio siguiendo patrones arquitectónicos establecidos.

## Contexto del proyecto

- **Framework objetivo**: .NET 10 con C# 14
- **Patrón**: diseño interface-first con inyección de dependencias
- **Manejo de errores**: patrón Result para propagación limpia de errores

## Reglas de idioma obligatorias

- La documentación XML (`<summary>`, `<remarks>`, `<param>`, `<returns>`, `<exception>`) debe estar en castellano y **DEBEN** seguir las instrucciones del archivo `/.github/instructions/coding-style-csharp-xml-documentation.instructions.md`.
- **DEBE** seguir las reglas de localización y uso de recursos del archivo `/.github/instructions/coding-style-csharp-localization.instructions.md`, especialmente para mensajes de validación destinados al usuario final.
- **Uso de namespaces**: Usar siempre namespaces tradicionales con llaves `{ }`. Los ejemplos de código extensos en esta guía utilizan la sintaxis de namespace con punto y coma por brevedad; el código generado real **DEBE** usar la forma de bloque conforme a `/.github/instructions/coding-style-csharp.instructions.md`.

## Diseño interface-first

Definir siempre una interfaz antes de implementar un servicio:

```csharp
namespace {ProjectName}.Abstractions
{
    /// <summary>
    /// Proporciona operaciones para gestionar datos de {domain}.
    /// </summary>
    public interface I{ServiceName}Service
    {
        /// <summary>
        /// Recupera un elemento por su identificador.
        /// </summary>
        /// <param name="id">El identificador único del elemento.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>El elemento si existe; en caso contrario, null.</returns>
        Task<Item?> GetByIdAsync(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Crea un nuevo elemento.
        /// </summary>
        /// <param name="item">El elemento que se va a crear.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Un resultado que indica éxito o error.</returns>
        Task<Result<Item>> CreateAsync(Item item, CancellationToken cancellationToken);
    }
}
```

## Por qué las implementaciones de servicios deben ser `internal sealed`

Las implementaciones de servicios (no las interfaces) deben declararse como `internal sealed` por razones arquitectónicas críticas:

### Principio central

**Solo las abstracciones son contratos. Las implementaciones son detalles de implementación.**

Hacer públicas (`public`) las implementaciones las convierte accidentalmente en contratos, impidiendo su evolución futura y aumentando el acoplamiento.

### Por qué `internal`

#### 1. Evita contratos públicos accidentales

Una implementación `public` se convierte en una promesa de que:

- Otros ensamblados pueden referenciarla directamente
- Su comportamiento pasa a ser una dependencia externa
- Los cambios de ruptura se vuelven costosos o imposibles

```csharp
// MAL - Filtra la implementación como parte de la API
public class OrderService : IOrderService
{
}

// BIEN - La implementación está oculta
internal sealed class OrderService : IOrderService
{
}
```

Una vez pública, no puedes:

- Cambiar libremente las dependencias del constructor
- Dividir la clase en múltiples servicios
- Sustituirla por una estrategia de implementación diferente
- Eliminarla sin romper consumidores externos

#### 2. Refuerza la inversión de dependencias

Los consumidores deben depender de **interfaces**, no de tipos concretos.

Si la implementación es `public`, los desarrolladores tenderán a:

- Omitir la interfaz
- Inyectar el tipo concreto directamente (`services.AddScoped<OrderService>()`)
- Crear acoplamiento en tiempo de compilación
- Dificultar las pruebas

Esto viola principios SOLID y crea una arquitectura frágil.

#### 3. Permite refactorizaciones seguras

Con implementaciones `internal`, puedes libremente:

- Renombrar clases
- Dividir responsabilidades
- Sustituir por decoradores o proxies
- Eliminar por completo

Sin provocar roturas entre ensamblados.

### Por qué `sealed`

#### 1. La herencia es un contrato no documentado

Permitir herencia implica:

- Que los métodos virtuales son puntos de extensión estables
- Que los invariantes de la clase base deben preservarse para siempre
- Que las subclases pueden alterar el comportamiento de formas inesperadas

La mayoría de los servicios **no están diseñados para herencia**.

```csharp
// MAL - Implica que la herencia está soportada
internal class PricingService : IPricingService
{
    protected virtual decimal CalculateBasePrice() { }
}

// BIEN - Cerrada para herencia
internal sealed class PricingService : IPricingService
{
    private decimal CalculateBasePrice() { }
}
```

#### 2. Evita problemas de clase base frágil

Las clases no selladas conducen a:

- Cambios de comportamiento sutiles por subclases
- Errores de orden de ejecución
- Violaciones de invariantes internos
- Mutaciones de estado impredecibles

Sellar la clase hace el comportamiento **cerrado y predecible**.

#### 3. Habilita optimizaciones del compilador

Usar `sealed` permite:

- Desvirtualización (llamadas directas en lugar de despacho virtual)
- Mejor inlining
- Menor sobrecarga en tiempo de ejecución

Estos beneficios son automáticos y significativos.

### Regla de decisión

**Usar siempre `internal sealed` salvo que aplique una de estas excepciones poco frecuentes:**

1. El tipo está diseñado explícitamente como punto de extensión del framework
2. El tipo forma parte de un SDK pensado para herencia de terceros
3. El tipo es una clase base abstracta documentada para plugins
4. **La herencia mejora de forma demostrable la calidad y reutilización del código** — al crear una familia de implementaciones relacionadas que comparten comportamiento, omitir `sealed` puede estar justificado

Para la excepción #4, deben cumplirse las siguientes condiciones:

- La clase base aporta implementación compartida sustancial (no solo una interfaz)
- Múltiples implementaciones concretas se benefician realmente de la jerarquía de herencia
- Los puntos de extensión virtual están claramente documentados y son intencionales
- El diseño mejora la mantenibilidad frente a alternativas de composición o delegación

**Cuando exista duda sobre omitir `sealed`:**

- **Preguntar al usuario** si la herencia es necesaria para el caso de uso específico
- **Revisar la especificación** para determinar si hay múltiples implementaciones relacionadas documentadas
- **Usar `sealed` por defecto** si la intención arquitectónica no es clara
- **Preferir composición sobre herencia** salvo que la herencia aporte beneficios claros y medibles

En todos los casos de excepción:

- Documentar explícitamente los contratos de herencia
- Preferir `public abstract` sobre clases concretas `public` cuando la visibilidad deba ser pública
- Versionar con cuidado y tratar los métodos virtuales como contratos estables

### Resumen del patrón

```csharp
// Contrato público - superficie de API estable
public interface IEmailSender
{
    Task SendAsync(Message message);
}

// Implementación interna - detalle oculto, sin herencia
internal sealed class SmtpEmailSender : IEmailSender
{
    public Task SendAsync(Message message)
    {
        // implementación
    }
}

// Registro DI: los consumidores solo ven la interfaz
services.AddScoped<IEmailSender, SmtpEmailSender>();
```

**Conclusión clave para generación de código:** Al crear implementaciones de servicios, usar por defecto `internal sealed class` salvo instrucción explícita en contrario. Esto es una buena práctica arquitectónica, no una preferencia de estilo.

## Patrón de implementación de servicios

```csharp
namespace {ProjectName}.Services;

internal sealed class {ServiceName}Service : I{ServiceName}Service
{
    private readonly ILogger<{ServiceName}Service> _logger;
    private readonly TelemetryClient _telemetryClient;
    private {ServiceName}Options _options;

    public {ServiceName}Service(ILogger<{ServiceName}Service> logger, TelemetryClient telemetryClient, IOptionsMonitor<{ServiceName}Options> optionsMonitor)
    {
        _logger = logger;
        _telemetryClient = telemetryClient;
        _options = optionsMonitor.CurrentValue;

        // Compatibilidad con recarga en caliente
        optionsMonitor.OnChange(newOptions => _options = newOptions);
    }

    public async Task<Item?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(@"Recuperando elemento {ItemId}", id);

        try
        {
            var item = await FetchItemAsync(id, cancellationToken);

            if (item is null)
            {
                _logger.LogWarning(@"No se encontró el elemento {ItemId}", id);
            }

            return item;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, @"Error al recuperar el elemento {ItemId}", id);
            throw;
        }
    }

    public async Task<Result<Item>> CreateAsync(Item item, CancellationToken cancellationToken)
    {
        _logger.LogInformation(@"Creando elemento {ItemId}", item.Id);

        try
        {
            await SaveItemAsync(item, cancellationToken);

            _telemetryClient.TrackEvent(@"ItemCreated", new Dictionary<string, string>
            {
                [@"ItemId"] = item.Id,
            });

            return Result<Item>.Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, @"Error al crear el elemento {ItemId}", item.Id);
            return Result<Item>.Fail(ex);
        }
    }
}
``` con `CommunityToolkit.Diagnostics`

Usar la clase `Guard` de `CommunityToolkit.Diagnostics` para validación de precondiciones en servicios de dominio y lógica de negocio. `Guard` proporciona una API expresiva y fail-fast para rangos, colecciones, tipos y booleanos, más allá de la simple comprobación de nulos.

> **Nota**: `Guard` es la herramienta de precondiciones para esta capa. Para métodos de extensión de infraestructura, usar `ArgumentNullException.ThrowIfNull`. Para validación de modelos de datos externos del usuario (requests de API, formularios, mensajes de cola), usar `FluentValidation`. Ver `/.github/instructions/coding-style-csharp.instructions.md` para la estrategia completa de validación por capa.

### Instalación

Añadir el paquete al proyecto. **Usar siempre la versión más reciente disponible**.

```xml
<PackageReference Include="CommunityToolkit.Diagnostics" Version="8.4.0" />
```

### Uso

```csharp
using CommunityToolkit.Diagnostics;

public void ProcessItem(Item item, string userId)
{
    Guard.IsNotNull(item);
    Guard.IsNotNullOrWhiteSpace(userId);

    // Uso seguro de item y userId
}

public async Task<Result> CreateAsync(Request request, CancellationToken cancellationToken)
{
    Guard.IsNotNull(request);
    Guard.IsNotNullOrEmpty(request.Items);
    Guard.IsGreaterThan(request.Timeout, 0);

    await DoWorkAsync(request, cancellationToken);
    return Result.Ok();
}
```

### Métodos de guarda comunes

```csharp
// Comprobaciones de nulo
Guard.IsNotNull(value);
Guard.IsNull(value);

// Comprobaciones de cadena
Guard.IsNotNullOrEmpty(text);
Guard.IsNotNullOrWhiteSpace(text);

// Comprobaciones numéricas
Guard.IsGreaterThan(value, threshold);
Guard.IsGreaterThanOrEqualTo(value, threshold);
Guard.IsLessThan(value, threshold);
Guard.IsLessThanOrEqualTo(value, threshold);
Guard.IsInRange(value, minimum, maximum);

// Comprobaciones de colección
Guard.IsNotEmpty(collection);
Guard.HasSizeEqualTo(collection, size);
Guard.HasSizeGreaterThan(collection, size);

// Comprobaciones booleanas
Guard.IsTrue(condition);
Guard.IsFalse(condition);

// Comprobaciones de tipo
Guard.IsOfType<TExpected>(value);
Guard.IsAssignableToType<TExpected>(value);
```

## Patrón Result

Usar el "Patrón Result" para operaciones que pueden fallar:

```csharp
namespace {ProjectName}.Services
{
    public class Result
    {
        protected Result(bool isSuccess, string? error = null, Exception? exception = null)
        {
            IsSuccess = isSuccess;
            Error = error;
            Exception = exception;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; }
        public Exception? Exception { get; }

        public static Result Ok() => new(true);
        public static Result Fail(string error) => new(false, error);
        public static Result Fail(Exception exception) => new(false, exception.Message, exception);
    }

    public class Result<T> : Result
    {
        protected Result(bool isSuccess, T? value = default, string? error = null, Exception? exception = null)
            : base(isSuccess, error, exception)
        {
            Value = value;
        }

        public T? Value { get; }

        public static Result<T> Ok(T value) => new(true, value);
        public static new Result<T> Fail(string error) => new(false, default, error);
        public static new Result<T> Fail(Exception exception) => new(false, default, exception.Message, exception);
    }
}
```

> **Nota**: Evitar operadores implícitos como `public static implicit operator Result<T>(T value) => Ok(value);` — pueden ocultar fallos al convertir silenciosamente `null` o valores predeterminados en resultados exitosos (p. ej., `Result<string> r = null;` crea `Ok(null)`).

### Uso del patrón Result

```csharp
public async Task<Result> ProcessAsync(Request request, CancellationToken cancellationToken)
{
    Guard.IsNotNull(request);
    Guard.IsNotNullOrWhiteSpace(request.Input);

    // Procesamiento
    try
    {
        await DoWorkAsync(request, cancellationToken);
        return Result.Ok();
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, @"La solicitud HTTP falló");
        return Result.Fail(ex);
    }
}

// El llamador gestiona el resultado
var result = await service.ProcessAsync(request, cancellationToken);

if (result.IsFailure)
{
    logger.LogError(@"El procesamiento falló: {Error}", result.Error);
    return BadRequest(result.Error);
}
```

## Patrón record de configuración de servicio

Para dependencias de servicio complejas, usar un record de configuración:

```csharp
namespace {ProjectName}.Services
{
    public sealed record {ServiceName}ServiceConfiguration(
        DaprClient DaprClient,
        IDistributedCache DistributedCache,
        IHttpClientFactory HttpClientFactory,
        ILogger<{ServiceName}Service> Logger,
        IOptionsMonitor<DistributedCacheEntryOptions> CacheOptions,
        IOptionsMonitor<{ServiceName}Options> ServiceOptions);
}
```

### Uso en el servicio

```csharp
internal sealed class {ServiceName}Service : I{ServiceName}Service
{
    private readonly {ServiceName}ServiceConfiguration _config;
    private {ServiceName}Options _options;

    public {ServiceName}Service({ServiceName}ServiceConfiguration config)
    {
        _config = config;
        _options = config.ServiceOptions.CurrentValue;
        config.ServiceOptions.OnChange(newOptions => _options = newOptions);
    }
}
```

## Patrón factoría para creación de clientes

```csharp
namespace {ProjectName}.Services;

public static class {ClientName}Factory
{
    public static TClient CreateClient<TClient, TOptions>(TOptions options)
        where TClient : class
        where TOptions : class
    {
        Guard.IsNotNull(options);

        var clientOptions = ConfigureClientOptions(options);

        return CreateClientInstance<TClient, TOptions>(options, clientOptions);
    }

    private static TClientOptions ConfigureClientOptions<TOptions>(TOptions options)
        where TOptions : class
    {
        // Configurar políticas de reintento, tiempos de espera, cabeceras personalizadas, etc.
        var clientOptions = new TClientOptions
        {
            // Establecer tiempos de espera, políticas de reintento, etc.
        };

        // Añadir cabeceras o políticas de autenticación si es necesario

        return clientOptions;
    }

    private static TClient CreateClientInstance<TClient, TOptions>(
        TOptions options,
        TClientOptions clientOptions)
        where TClient : class
        where TOptions : class
    {
        Guard.IsNotNull(clientOptions);

        // Elegir el método de autenticación según las opciones
        if (options.UseTokenAuthentication)
        {
            var credentials = BuildCredentialChain(options);
            return new TClient(options.Endpoint, credentials, clientOptions);
        }

        Guard.IsNotNullOrWhiteSpace(options.ApiKey);
        return new TClient(options.Endpoint, new ApiKeyCredential(options.ApiKey), clientOptions);
    }

    private static ChainedTokenCredential BuildCredentialChain<TOptions>(TOptions options)
        where TOptions : class
    {
        var credentials = new List<TokenCredential>();

        // Añadir credenciales de entidad de servicio si están configuradas
        if (options.ServicePrincipalOptions is not null)
        {
            credentials.Add(new ClientSecretCredential(
                options.ServicePrincipalOptions.TenantId,
                options.ServicePrincipalOptions.ClientId,
                options.ServicePrincipalOptions.ClientSecret));
        }

        // Fallback a la credencial de Azure predeterminada
        credentials.Add(new DefaultAzureCredential());
        return new ChainedTokenCredential(credentials.ToArray());
    }
}
```

### Ejemplo: factoría de cliente de Azure OpenAI

```csharp
namespace {ProjectName}.Services;

public static class OpenAIClientFactory
{
    public static AzureOpenAIClient CreateClient(AzureOpenAIOptions options)
    {
        Guard.IsNotNull(options);
        Guard.IsNotNull(options.Endpoint);

        var clientOptions = ConfigureClientOptions(options);

        return CreateClientInstance(options, clientOptions);
    }

    private static AzureOpenAIClientOptions ConfigureClientOptions(AzureOpenAIOptions options)
    {
        var clientOptions = new AzureOpenAIClientOptions(options.ServiceVersion)
        {
            RetryPolicy = new ClientRetryPolicy(maxRetries: 0),
            NetworkTimeout = TimeSpan.FromMinutes(5),
        };

        // Añadir cabeceras personalizadas si se proporciona la clave de API
        if (!string.IsNullOrWhiteSpace(options.Key))
        {
            clientOptions.AddPolicy(
                policy: new AddHeadersPolicy(new Dictionary<string, string>
                {
                    { @"Ocp-Apim-Subscription-Key", options.Key }
                }),
                position: PipelinePosition.PerCall);
        }

        return clientOptions;
    }

    private static AzureOpenAIClient CreateClientInstance(AzureOpenAIOptions options, AzureOpenAIClientOptions clientOptions)
    {
        Guard.IsNotNullOrWhiteSpace(options.Key);

        if (options.UseTokenCredentialAuthentication)
        {
            var credentials = BuildCredentialChain(options);
            return new AzureOpenAIClient(options.Endpoint, credentials, clientOptions);
        }

        return new AzureOpenAIClient(options.Endpoint, new ApiKeyCredential(options.Key), clientOptions);
    }

    private static ChainedTokenCredential BuildCredentialChain(AzureOpenAIOptions options)
    {
        var credentials = new List<TokenCredential>();

        if (options.TokenCredentialsOptions is not null)
        {
            credentials.Add(new ClientSecretCredential(
                options.TokenCredentialsOptions.TenantId,
                options.TokenCredentialsOptions.ClientId,
                options.TokenCredentialsOptions.ClientSecret));
        }

        credentials.Add(new DefaultAzureCredential());
        return new ChainedTokenCredential(credentials.ToArray());
    }
}
```

## Registro de función factoría

Registrar delegados factoría para creación por instancia:

```csharp
public static IServiceCollection AddMessageStoreFactory(this IServiceCollection services, string containerId)
{
    ArgumentNullException.ThrowIfNull(services);
    ArgumentException.ThrowIfNullOrWhiteSpace(containerId);

    services.AddSingleton<Func<string, CancellationToken, Task<IChatMessageStore>>>(sp =>
    {
        var cosmosOptions = sp.GetRequiredService<IOptionsMonitor<CosmosOptions>>().CurrentValue;

        return async (conversationId, cancellationToken) =>
        {
            var client = new CosmosClient(cosmosOptions.ConnectionString);
            var database = await client.CreateDatabaseIfNotExistsAsync(cosmosOptions.Database, cancellationToken: cancellationToken);
            var container = await database.Database.CreateContainerIfNotExistsAsync(containerId, "/id", cancellationToken: cancellationToken);

            return new CosmosChatMessageStore(client, cosmosOptions.Database, containerId, conversationId);
        };
    });

    return services;
}
```

### Ejemplo de uso de función factoría

```csharp
public class ChatService
{
    private readonly Func<string, CancellationToken, Task<IChatMessageStore>> _storeFactory;

    public ChatService(Func<string, CancellationToken, Task<IChatMessageStore>> storeFactory)
    {
        _storeFactory = storeFactory;
    }

    public async Task<IChatMessageStore> GetStoreAsync(string conversationId, CancellationToken cancellationToken)
    {
        return await _storeFactory(conversationId, cancellationToken);
    }
}
```

## Patrón de servicio hospedado de calentamiento

Precargar datos al inicio:

```csharp
namespace {ProjectName}.Services
{
    internal sealed class {ServiceName}WarmUpHostedService : IHostedService
    {
        private readonly I{ServiceName}Service _service;
        private readonly ILogger<{ServiceName}WarmUpHostedService> _logger;

        public {ServiceName}WarmUpHostedService(
            I{ServiceName}Service service,
            ILogger<{ServiceName}WarmUpHostedService> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(@"Iniciando precalentamiento de {ServiceName}...", nameof({ServiceName}));

            try
            {
                await _service.InitializeAsync(cancellationToken);
                _logger.LogInformation(@"Precalentamiento de {ServiceName} completado", nameof({ServiceName}));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, @"Falló el precalentamiento de {ServiceName}; se inicializará en el primer uso", nameof({ServiceName}));
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
```

## Servicio de semáforo para limitación de tasa

```csharp
namespace {ProjectName}.Services
{
    public sealed class SemaphoreService
    {
        private readonly object _semaphoreLock = new();
        private SemaphoreSlim _semaphore;
        private int _limit;

        public SemaphoreService(IOptionsMonitor<SemaphoreServiceOptions> optionsMonitor)
        {
            _limit = optionsMonitor.CurrentValue.SemaphoreLimit;
            _semaphore = new SemaphoreSlim(1, _limit);

            optionsMonitor.OnChange(newOptions =>
            {
                if (newOptions.SemaphoreLimit != _limit)
                {
                    lock (_semaphoreLock)
                    {
                        _limit = newOptions.SemaphoreLimit;
                        _semaphore = new SemaphoreSlim(1, _limit);
                    }
                }
            });
        }

        public async Task WaitAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
        }

        public void Release()
        {
            _semaphore.Release();
        }
    }
}
```

## Selección de tiempo de vida de servicio

Elegir el tiempo de vida correcto del servicio es crucial para la estabilidad y el rendimiento de la aplicación. A continuación se presentan directrices para seleccionar entre tiempos de vida `Transient`, `Scoped` y `Singleton`.

### Servicios Transient

#### Cuándo usar transient

Usar `Transient` cuando **todas** las siguientes condiciones sean verdaderas:

1. **El servicio es sin estado** — no tiene campos mutables que afecten el comportamiento; cualquier estado se pasa por parámetros de método
2. **El servicio no administra recursos** — sin manejadores de archivo, sockets ni conexiones de base de datos
3. **El costo de creación es bajo** — sin lógica de inicialización pesada
4. **La identidad de instancia es irrelevante** — dos instancias son indistinguibles en comportamiento
5. **La biblioteca o el tipo requerido exige explícitamente uso transient**

#### Ejemplos típicos de transient

- Servicios de cálculo puro (calculadoras de precio, formateadores)
- Validadores y mapeadores
- Implementaciones de estrategia
- Handlers de comandos de una sola ejecución
- Procesadores ligeros

```csharp
builder.Services.AddTransient<IPriceCalculator, PriceCalculator>();
builder.Services.AddTransient<IOrderValidator, OrderValidator>();
builder.Services.AddTransient<IMessageFormatter, MessageFormatter>();
```

#### Antipatrones de transient

- El servicio captura estado accidentalmente
- El servicio depende de servicios scoped o singleton (provoca fugas de ámbito)
- El servicio se crea en rutas calientes y afecta el rendimiento
- El servicio administra recursos desechables

### Servicios Scoped

#### Cuándo usar scoped

Usar `Scoped` cuando **cualquiera** de las siguientes condiciones sea verdadera:

1. **El servicio posee estado por solicitud o por operación** — identidad de usuario, IDs de correlación, cachés específicas por solicitud
2. **El servicio coordina múltiples operaciones** — Unit of Work, límites transaccionales
3. **El servicio administra recursos desechables** — contextos de base de datos, sesiones ORM, conexiones ligadas a la solicitud
4. **Importa la consistencia dentro de una solicitud** — múltiples consumidores deben ver la misma instancia
5. **La biblioteca o el tipo requerido exige explícitamente uso scoped**

#### Ejemplos típicos de scoped

- `DbContext` de Entity Framework
- Implementaciones de Unit of Work
- Servicios de contexto de solicitud
- Servicios de dominio que coordinan repositorios
- Gestores transaccionales

```csharp
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRequestContext, RequestContext>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
```

#### Propiedades arquitectónicas de servicios scoped

- Proveen **aislamiento entre solicitudes**
- Evitan fuga de datos entre solicitudes
- Se alinean naturalmente con límites transaccionales
- Se liberan cuando finaliza el ámbito

#### Antipatrones de scoped

- Inyectar servicios scoped en singletons
- Tratar servicios scoped como thread-safe
- Guardar instancias scoped en campos estáticos
- Usar servicios scoped fuera de un ámbito

### Servicios Singleton

#### Cuándo usar singleton

Usar `Singleton` **solo cuando todas** las siguientes condiciones sean verdaderas:

1. **El servicio representa comportamiento o estado global** — proveedores de configuración, cachés de alcance global, planificadores en segundo plano
2. **El servicio es thread-safe por diseño** — estado inmutable o sincronización explícita
3. **El servicio no depende de servicios scoped** — sin `DbContext`, sin datos específicos de solicitud
4. **El tiempo de vida del servicio coincide con el tiempo de vida de la aplicación**
5. **La biblioteca o el tipo requerido exige explícitamente uso singleton**

#### Ejemplos típicos de singleton

- Cachés en memoria
- Accesores de configuración (`IOptions<T>`, `IOptionsMonitor<T>`)
- Recolectores de métricas y clientes de telemetría
- Pools de conexión
- Evaluadores de feature flags
- Factorías de cliente HTTP

```csharp
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
builder.Services.AddSingleton<IMetricsCollector, MetricsCollector>();
builder.Services.AddSingleton<IFeatureFlags, FeatureFlagService>();
builder.Services.AddSingleton<TelemetryClient>();
```

#### Riesgos de singleton (antipatrones críticos)

- Estado mutable compartido accidental
- Condiciones de carrera bajo carga
- Fugas de memoria por referencias retenidas
- Captura de servicios scoped mediante factorías
- Falta de thread-safety

### Reglas de dependencia por tiempo de vida

#### Dependencias permitidas

| Tiempo de vida de servicio | Puede depender de |
| --- | --- |
| Singleton | Solo Singleton |
| Scoped | Scoped, Singleton |
| Transient | Cualquier tiempo de vida |

#### Dependencias prohibidas

- **Singleton → Scoped** (siempre incorrecto)
- **Singleton → Transient que captura estado scoped**

Las violaciones provocan:

- Problemas de dependencias cautivas
- Excepciones en tiempo de ejecución
- Comportamiento indefinido
- Incidencias de producción difíciles de depurar

### Matriz de decisión de tiempo de vida de servicio

| Pregunta | Elegir |
| --- | --- |
| ¿Necesita estado por solicitud? | Scoped |
| ¿Representa coordinación global? | Singleton |
| ¿Es cálculo puro sin estado? | Transient |
| ¿Administra recursos desechables? | Scoped |
| ¿Debe ser thread-safe? | Singleton |
| ¿Es barato, sin estado y simple? | Transient |
| ¿Gestiona límites transaccionales? | Scoped |
| ¿Es de configuración o caché? | Singleton |
| ¿Es contexto de base de datos? | Scoped |
| ¿Es procesamiento en segundo plano? | Singleton (Hosted Service) |

### Consideraciones de rendimiento

- **Transient** — incurre en costo de asignación por resolución
- **Scoped** — amortiza costo por solicitud/ámbito frente a transient
- **Singleton** — minimiza asignaciones, pero incrementa el riesgo de contención

### Ejemplos de registro

```csharp
// Servicios hospedados para el ciclo de vida de inicio/apagado
builder.Services.AddHostedService<{ServiceName}WarmUpHostedService>();

// Ejemplos de Singleton
builder.Services.AddSingleton<IDistributedCache, RedisCache>();
builder.Services.AddSingleton<SemaphoreService>();

// Ejemplos de Scoped
builder.Services.AddScoped<I{ServiceName}Service, {ServiceName}Service>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Ejemplos de Transient
builder.Services.AddTransient<IValidator<Order>, OrderValidator>();
builder.Services.AddTransient<IMessageProcessor, MessageProcessor>();
```

> **Ante la duda, preferir el enfoque más simple, preguntar al usuario o solicitar especificaciones más detalladas**
