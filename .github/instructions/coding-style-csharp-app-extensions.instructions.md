---
applyTo: "**/AppExtensions.cs"
description: "Directrices para crear archivos AppExtensions.cs que centralicen la configuración de middlewares y el registro de endpoints, manteniendo Program.cs conciso."
---

# Directrices de AppExtensions.cs

Instrucciones para crear archivos `AppExtensions.cs` que centralicen la configuración de middlewares y el registro de endpoints de la aplicación, manteniendo `Program.cs` como orquestador de alto nivel.

## Contexto del proyecto

- **Framework objetivo**: .NET 10 con C# 14
- **Combinar con instrucciones**:
  - [Directrices de Program.cs](/.github/instructions/coding-style-csharp-program.instructions.md) para la estructura del punto de entrada.
  - [Directrices de BuilderExtensions.cs](/.github/instructions/coding-style-csharp-builder-extensions.instructions.md) para la configuración del builder.
  - [Directrices de métodos de extensión](/.github/instructions/coding-style-csharp-extension-methods.instructions.md) para convenciones generales de extensiones.

## Principio general

`AppExtensions.cs` contiene métodos de extensión sobre `WebApplication` que encapsulan la configuración del pipeline de middlewares y el registro de endpoints. `Program.cs` invoca estos métodos sin conocer los detalles internos del pipeline.

## Namespace

Usar el namespace del proyecto (por ejemplo, `Company.Project.Api.Extensions`) ya que estos métodos son específicos de la aplicación y no de la infraestructura genérica.

```csharp
namespace Company.Project.Api.Extensions
{
    public static class AppExtensions
    {
    }
}
```

## Métodos requeridos

### `UseMiddlewares()`

Configura el pipeline de middlewares de la aplicación. Registra cada middleware personalizado y los middlewares estándar de ASP.NET Core en el orden correcto.

```csharp
/// <summary>
/// Registra los middlewares de la aplicación.
/// </summary>
/// <param name="app">Instancia de la aplicación web.</param>
/// <returns>La aplicación configurada para encadenamiento fluido.</returns>
public static WebApplication UseMiddlewares(this WebApplication app)
{
    app.UseMiddleware<LogAccessMiddleware>();
    app.UseMiddleware<AuthMiddleware>();
    return app;
}
```

#### Orden de middlewares

Respetar el orden canónico de ASP.NET Core cuando se incluyan middlewares estándar:

1. `UseExceptionHandler()` / `UseDeveloperExceptionPage()`
2. `UseRouting()`
3. `UseCors()`
4. `UseAuthentication()`
5. `UseAuthorization()`
6. Middlewares personalizados (logging, filtros, etc.)
7. `UseEndpoints()` / `MapControllers()`

Si los middlewares estándar se registran directamente en `Program.cs` (por ejemplo, `UseExceptionHandler()`, `UseCors()`), `UseMiddlewares()` se limita a los middlewares **personalizados** del proyecto.

### `UseRegisterEndpointDefinitions()`

Registra los endpoints de la aplicación. Puede usar descubrimiento por reflexión o registro manual.

#### Patrón con descubrimiento por reflexión

```csharp
/// <summary>
/// Registra los endpoints descubiertos por reflexión.
/// </summary>
/// <param name="app">Instancia de la aplicación web.</param>
/// <returns>La aplicación configurada para encadenamiento fluido.</returns>
public static WebApplication UseRegisterEndpointDefinitions(this WebApplication app)
{
    ArgumentNullException.ThrowIfNull(app);

    var endpointDefinitions = AssemblyReference.Assembly
        .GetTypes()
        .Where(t => t.IsAssignableTo(typeof(IEndpointDefinition)) && t is { IsAbstract: false, IsInterface: false })
        .Select(Activator.CreateInstance)
        .Cast<IEndpointDefinition>();

    foreach (var endpointDef in endpointDefinitions)
    {
        endpointDef.RegisterEndpoints(app);
    }

    return app;
}
```

#### Patrón con controladores

```csharp
public static WebApplication UseRegisterEndpointDefinitions(this WebApplication app)
{
    ArgumentNullException.ThrowIfNull(app);

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

    return app;
}
```

### Configuración condicional (opcional)

Para lógica condicional del pipeline (Swagger, Azure App Configuration), se pueden añadir métodos adicionales o aceptar el builder de configuración como parámetro:

```csharp
/// <summary>
/// Configura el pipeline condicional según el entorno.
/// </summary>
/// <param name="app">Instancia de la aplicación web.</param>
/// <param name="configuration">Configuración de la aplicación.</param>
/// <returns>La aplicación configurada para encadenamiento fluido.</returns>
public static WebApplication UseConditionalPipeline(this WebApplication app, IConfiguration configuration)
{
    ArgumentNullException.ThrowIfNull(app);
    ArgumentNullException.ThrowIfNull(configuration);

    if (configuration["Swagger:Visible"] == "1")
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    return app;
}
```

## Ejemplo completo

```csharp
using Company.Project.Api.Abstractions;
using Company.Project.Api.Middlewares;

namespace Company.Project.Api.Extensions
{
    /// <summary>
    /// Extensiones de configuración del middleware para la aplicación.
    /// </summary>
    public static class AppExtensions
    {
        /// <summary>
        /// Registra los endpoints descubiertos por reflexión.
        /// </summary>
        /// <param name="app">Instancia de la aplicación web.</param>
        /// <returns>La aplicación configurada para encadenamiento fluido.</returns>
        public static WebApplication UseRegisterEndpointDefinitions(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            var endpointDefinitions = AssemblyReference.Assembly
                .GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IEndpointDefinition)) && t is { IsAbstract: false, IsInterface: false })
                .Select(Activator.CreateInstance)
                .Cast<IEndpointDefinition>();

            foreach (var endpointDef in endpointDefinitions)
            {
                endpointDef.RegisterEndpoints(app);
            }

            return app;
        }

        /// <summary>
        /// Registra los middlewares personalizados de la aplicación.
        /// </summary>
        /// <param name="app">Instancia de la aplicación web.</param>
        /// <returns>La aplicación configurada para encadenamiento fluido.</returns>
        public static WebApplication UseMiddlewares(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            app.UseMiddleware<LogAccessMiddleware>();
            app.UseMiddleware<AuthMiddleware>();
            return app;
        }
    }
}
```

## Patrones a seguir

- **Retornar siempre `WebApplication`** para permitir encadenamiento fluido.
- **Separar middlewares personalizados de estándar**: los estándar (`UseExceptionHandler`, `UseCors`, `UseHttpsRedirection`) pueden quedarse en `Program.cs` si son una única línea.
- **Usar documentación XML** en todos los métodos públicos.
- **Registrar endpoints en un método dedicado**: no mezclar registro de endpoints con configuración de middlewares.

## Patrones a evitar

- **Evitar registrar servicios en `AppExtensions.cs`**: la configuración del contenedor de dependencias pertenece a `BuilderExtensions.cs`.
- **Evitar middlewares sin orden correcto**: respetar el orden canónico de ASP.NET Core.
- **Evitar métodos genéricos que mezclen middlewares y endpoints**: mantener cada responsabilidad en su propio método.
- **Evitar lógica de negocio en los métodos de extensión**: estos métodos solo configuran el pipeline.

## Validación

- Verificar que todos los métodos públicos retornan `WebApplication`.
- Verificar que el orden de middlewares respeta el pipeline canónico de ASP.NET Core.
- Verificar que no se registran servicios en `AppExtensions.cs`.
- Verificar que la documentación XML está presente en todos los métodos públicos.

## Referencias

- [ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [Routing in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing)
- [Minimal APIs Overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview)
