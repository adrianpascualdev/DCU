---
applyTo: "**/BuilderExtensions.cs"
description: "Directrices para crear archivos BuilderExtensions.cs que centralicen la configuración del builder, manteniendo Program.cs conciso."
---

# Directrices de BuilderExtensions.cs

Instrucciones para crear archivos `BuilderExtensions.cs` que centralicen toda la configuración del builder (settings, telemetría, servicios, infraestructura), manteniendo `Program.cs` como orquestador de alto nivel.

## Contexto del proyecto

- **Framework objetivo**: .NET 10 con C# 14
- **Infraestructura**: Azure PaaS (App Configuration, Key Vault, Application Insights)
- **Combinar con instrucciones**:
  - [Directrices de Program.cs](/.github/instructions/coding-style-csharp-program.instructions.md) para la estructura del punto de entrada.
  - [Directrices de métodos de extensión](/.github/instructions/coding-style-csharp-extension-methods.instructions.md) para convenciones generales de extensiones.
  - [Directrices de clases Options](/.github/instructions/coding-style-csharp-settings-classes.instructions.md) para gestión de configuración.

## Principio general

`BuilderExtensions.cs` contiene un método de extensión público `Configure()` sobre `WebApplicationBuilder` (o `IHostApplicationBuilder` para workers) que actúa como punto de entrada único para toda la configuración. Internamente delega en métodos privados auxiliares para mantener cada sección cohesiva y legible.

## Namespace obligatorio

Usar `Microsoft.Extensions.DependencyInjection` para que el método `Configure()` sea autodescubrible desde `Program.cs` sin necesidad de directivas `using` adicionales.

```csharp
namespace Microsoft.Extensions.DependencyInjection
{
    public static class BuilderExtensions
    {
    }
}
```

## Estructura requerida del método `Configure()`

El método público `Configure()` debe seguir un orden de secciones con encabezados de comentario claros y delegar a métodos privados cuando una sección sea compleja:

1. Carga de configuración (Key Vault, Azure App Configuration)
2. Configuración de telemetría/logging
3. Registro de settings/options
4. Servicios de infraestructura (HTTP, caché, versionado, Swagger, CORS)
5. Servicios de aplicación (dominio, servicios de negocio)
6. Otros componentes (MassTransit, Feature Management, etc.)

### Ejemplo de estructura

```csharp
public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
{
    /* Leer Configuración */
    builder.ConfigureKeyVault();

    /* Configuración de telemetría */
    builder.ConfigureLogger()
           .ConfigureSettings();

    /* Servicios de infraestructura */
    builder.Services
        .AddHttpContextAccessor()
        .ConfigureApiVersioning()
        .ConfigureSwagger()
        .ConfigureCors()
        .AddHttpClient()
        .AddMemoryCache()
        .AddOutputCache()
        .AddExceptionHandler<GlobalExceptionHandler>()
        .AddProblemDetails();

    /* Feature Management */
    builder.Services.AddFeatureManagement()
        .AddFeatureFilter<EnvironmentFeatureFilter>();

    /* Servicios de aplicación */
    builder.Services.ConfigureServices();

    /* Message Broker */
    builder.ConfigureMassTransit();

    return builder;
}
```

## Secciones y métodos privados auxiliares

### Carga de configuración

Usar comentario estructurado `/* Leer Configuración */`. Delegar a métodos privados como `ConfigureKeyVault()` o `ConfigureAppConfiguration()`.

```csharp
/* Leer Configuración */

if (Debugger.IsAttached)
{
    builder.Configuration.AddJsonFile(@"appsettings.debug.json", optional: true, reloadOnChange: true);
}

builder.Configuration.AddJsonFile($@"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                     .AddJsonFile($@"appsettings.{Environment.UserName}.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables();
```

#### Orden de carga de configuración (prioridad)

1. `appsettings.json` (configuración base)
2. `appsettings.debug.json` (solo con debugger adjunto)
3. `appsettings.{Environment}.json` (específica de entorno)
4. `appsettings.{Username}.json` (específica de desarrollador)
5. Variables de entorno
6. Azure App Configuration (con jerarquía de etiquetas)

#### Azure App Configuration y Key Vault

```csharp
var appConfigurationConnectionString = builder.Configuration.GetConnectionString(@"AppConfig");
var useAppConfiguration = !string.IsNullOrWhiteSpace(appConfigurationConnectionString);

if (useAppConfiguration)
{
    var azureCredentials = new ChainedTokenCredential(new DefaultAzureCredential(), new EnvironmentCredential());

    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options.Connect(appConfigurationConnectionString)
               .ConfigureKeyVault(keyVault =>
               {
                   keyVault.SetCredential(azureCredentials);
               })
               .Select(KeyFilter.Any, LabelFilter.Null)
               .Select(KeyFilter.Any, assemblyName)
               .ConfigureRefresh(refreshOptions =>
               {
                   refreshOptions.Register(@"Sentinel", assemblyName, refreshAll: true);
                   refreshOptions.SetRefreshInterval(TimeSpan.FromSeconds(86400));
               });
    }, optional: false);

    builder.Services.AddAzureAppConfiguration();
}
```

### Configuración de telemetría/logging

Usar comentario estructurado `/* Configuración de telemetría */`. Delegar a un método privado `ConfigureLogger()`.

```csharp
private static WebApplicationBuilder ConfigureLogger(this WebApplicationBuilder builder)
{
    builder.Logging.ClearProviders();

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();

    builder.Host.UseSerilog();
    return builder;
}
```

Para proyectos con OpenTelemetry y Application Insights:

```csharp
builder.AddServiceDefaults(assemblyName);

builder.Services.AddVersionTelemetry();
builder.Services.AddAssemblyNameTelemetry(assemblyName ?? string.Empty);

builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = @"yyyy-MM-dd HH:mm:ss ";
    options.UseUtcTimestamp = true;
});

if (Debugger.IsAttached)
{
    builder.Logging.AddDebug();
}
```

### Registro de settings/options

Usar comentario estructurado `/* Leer Settings/Options */`. Delegar a un método privado `ConfigureSettings()`.

```csharp
private static void ConfigureSettings(this WebApplicationBuilder builder)
{
    builder.Services.Configure<MessageBrokerSettings>(builder.Configuration.GetSection(nameof(MessageBrokerSettings)));
    builder.Services.Configure<ErpSettings>(builder.Configuration.GetSection(nameof(ErpSettings)));
    builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection(nameof(SecuritySettings)));
}
```

Para proyectos con validación fail-fast, preferir `AddOptionsWithValidateOnStart<T>()`:

```csharp
builder.Services.AddOptionsWithValidateOnStart<AzureOpenAIOptions>()
    .Bind(builder.Configuration.GetSection(nameof(AzureOpenAIOptions)))
    .ValidateDataAnnotations();
```

### Servicios de infraestructura

Métodos privados auxiliares para cada subsistema de infraestructura:

- `ConfigureApiVersioning()` sobre `IServiceCollection`
- `ConfigureSwagger()` sobre `IServiceCollection`
- `ConfigureCors()` sobre `IServiceCollection`

```csharp
private static IServiceCollection ConfigureApiVersioning(this IServiceCollection services)
{
    services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ApiVersionReader = new HeaderApiVersionReader();
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });
    return services;
}
```

### Servicios de aplicación

Usar comentario estructurado `/* Servicios de aplicación */`. Delegar a un método privado `ConfigureServices()` sobre `IServiceCollection`.

```csharp
private static IServiceCollection ConfigureServices(this IServiceCollection services)
{
    services.AddSingleton<IAuthService, AuthService>();
    services.AddScoped<IReportingService, ReportingService>();

    services.AddScoped<IOrderServices, OrderServices>(provider =>
    {
        var settings = provider.GetRequiredService<IOptions<ErpSettings>>();
        var logger = provider.GetRequiredService<ILogger<OrderServices>>();
        return new OrderServices(logger, settings);
    });

    return services;
}
```

### Otros componentes

Componentes como MassTransit, Feature Management o servicios de correo se configuran en métodos privados independientes:

```csharp
private static void ConfigureMassTransit(this WebApplicationBuilder builder)
{
    var messageBroker = builder.Configuration
                            .GetSection(nameof(MessageBrokerSettings))
                            .Get<MessageBrokerSettings>()
                        ?? new MessageBrokerSettings();

    builder.Services.AddMassTransit(x =>
    {
        x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(prefix: environment, includeNamespace: false));
        x.SetInMemorySagaRepositoryProvider();

        // Consumers por dominio
        x.AddOfferConsumers();
        x.AddOrderConsumers();

        // Transporte: InMemory o Azure Service Bus
        if (messageBroker.PrioritizeInMemory)
        {
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        }
        else
        {
            x.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.Host(messageBroker.Host);
                cfg.ConfigureEndpoints(context);
            });
        }
    });
}
```

## Plantilla para Worker en segundo plano

Para workers, el método `Configure()` extiende `IHostApplicationBuilder`:

```csharp
public static IHostApplicationBuilder Configure(this IHostApplicationBuilder builder)
{
    var assemblyName = typeof(Program).Assembly.GetName().Name;
    builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());

    /* Leer Configuración */
    builder.ConfigureKeyVault();

    /* Configuración de logging */
    builder.AddServiceDefaults(assemblyName ?? string.Empty);

    /* Leer Settings/Options */
    builder.Services.AddHostedService<MyWorker>();

    /* Servicios de aplicación */
    builder.Services.AddSingleton<IMyService, MyService>();
    builder.Services.AddHostedService<MyWorker>();

    return builder;
}
```

## Patrones a seguir

- **Retornar siempre el builder** desde el método público `Configure()` para permitir encadenamiento fluido con `.Build()`.
- **Usar encadenamiento fluido** con indentación vertical alineada al registrar servicios.
- **Usar `ChainedTokenCredential`** con `DefaultAzureCredential` y `EnvironmentCredential` para autenticación de Azure.
- **Lanzar `ArgumentException`** cuando falten cadenas de conexión obligatorias.

## Patrones a evitar

- **Evitar lógica de configuración en `Program.cs`**: toda la configuración pertenece a `BuilderExtensions.cs`.
- **Evitar métodos públicos auxiliares**: solo `Configure()` debe ser público; el resto de métodos son privados.
- **Evitar cadenas de conexión hardcodeadas**: usar Azure App Configuration y Key Vault.
- **Evitar omitir comentarios de sección**: usar siempre comentarios estructurados.
- **Evitar mezclar registros de infraestructura y dominio**: separarlos en métodos privados distintos.

## Validación

- Verificar que `Configure()` retorna el builder.
- Verificar que cada sección tiene su comentario estructurado.
- Verificar que los métodos auxiliares son privados.
- Verificar que el namespace es `Microsoft.Extensions.DependencyInjection`.

## Referencias

- [ASP.NET Core Fundamentals](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/)
- [Azure App Configuration](https://learn.microsoft.com/en-us/azure/azure-app-configuration/)
- [Options Pattern in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
- [MassTransit Documentation](https://masstransit.io/documentation/concepts)
