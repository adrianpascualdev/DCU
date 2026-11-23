---
applyTo: "**/Program.cs"
description: "Directrices para crear archivos Program.cs concisos delegando configuración en BuilderExtensions.cs y middleware en AppExtensions.cs, siguiendo convenciones de configuración, telemetría e inyección de dependencias."
---

# Directrices de Program.cs y extensiones de arranque

Instrucciones para crear archivos `Program.cs` concisos para ASP.NET, servicios en segundo plano, Aspire y aplicaciones .NET, delegando la configuración del builder en `BuilderExtensions.cs` y la configuración del middleware en `AppExtensions.cs`.

## Contexto del proyecto

- **Framework objetivo**: .NET 10 con C# 14
- **Infraestructura**: Azure PaaS (App Configuration, Key Vault, Application Insights)
- **Sistemas distribuidos**: Integración con sidecar de Dapr, orquestación de Aspire cuando se requiera
- **Combinar con instrucciones**: Seguir estas directrices junto con las siguientes instrucciones:
  - [Directrices de clases Options](/.github/instructions/coding-style-csharp-settings-classes.instructions.md) para gestión de configuración.
  - [Directrices de métodos de extensión](/.github/instructions/coding-style-csharp-extension-methods.instructions.md) para la estructura de las clases de extensión.

## Principio de delegación

`Program.cs` debe ser conciso y actuar como **orquestador de alto nivel**. Toda la lógica de configuración del builder (settings, telemetría, servicios, etc.) se delega a un método de extensión `Configure()` en `BuilderExtensions.cs`. La configuración del middleware de la aplicación se delega a métodos de extensión en `AppExtensions.cs`.

### Distribución de responsabilidades

| Archivo | Responsabilidad |
| --- | --- |
| `Program.cs` | Crear el builder, invocar extensiones, construir la app, ejecutarla |
| `BuilderExtensions.cs` | Configuración, telemetría, settings/options, registro de servicios |
| `AppExtensions.cs` | Middlewares, registro de endpoints, pipelines de petición |

## Estructura requerida de Program.cs

`Program.cs` debe contener únicamente:

1. Directivas `using` mínimas
2. Creación del builder
3. Invocación de `builder.Configure()` (extensión en `BuilderExtensions.cs`)
4. Construcción de la app (`builder.Build()`)
5. Invocación de middlewares y extensiones de app (`AppExtensions.cs`)
6. Ejecución de la aplicación

## Estructura requerida de BuilderExtensions.cs

El método `Configure()` debe seguir una estructura consistente basada en secciones con encabezados de comentario claros, delegando a métodos privados cuando una sección sea compleja:

1. Carga de configuración (Key Vault, Azure App Configuration)
2. Configuración de telemetría/logging
3. Registro de settings/options
4. Servicios de infraestructura (HTTP, caché, versionado, Swagger, CORS)
5. Servicios de aplicación (dominio, servicios de negocio)
6. Otros componentes (MassTransit, Feature Management, etc.)

## Directrices detalladas por archivo

Las directrices detalladas para cada archivo se encuentran en sus instrucciones dedicadas:

- [Directrices de BuilderExtensions.cs](/.github/instructions/coding-style-csharp-builder-extensions.instructions.md): estructura del método `Configure()`, secciones de configuración, telemetría, settings/options, servicios de infraestructura y dominio, Azure App Configuration, Key Vault y componentes como MassTransit.
- [Directrices de AppExtensions.cs](/.github/instructions/coding-style-csharp-app-extensions.instructions.md): middlewares, orden canónico del pipeline, registro de endpoints por reflexión o controladores.

## Plantillas por tipo de proyecto

### Plantilla Web API — Program.cs

```csharp
namespace Company.Project.Api
{
    /// <summary>
    /// Clase principal de la aplicación.
    /// </summary>
    public record Program
    {
        /// <summary>
        /// Punto de entrada de la aplicación.
        /// </summary>
        /// <param name="args">Argumentos de línea de comandos.</param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var app = builder
                .Configure()
                .Build();

            app.UseMiddlewares();
            app.UseRegisterEndpointDefinitions();

            try
            {
                app.Run();
            }
            catch (Exception e)
            {
                var logger = app.Services.GetService<ILogger<Program>>();
                logger?.LogCritical(e, "Error en el inicio de la Api");
            }
        }
    }
}
```

Ver [BuilderExtensions.cs](/.github/instructions/coding-style-csharp-builder-extensions.instructions.md) y [AppExtensions.cs](/.github/instructions/coding-style-csharp-app-extensions.instructions.md) para las plantillas de los archivos complementarios.

### Plantilla Worker en segundo plano — Program.cs

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Configure();

var host = builder.Build();
await host.RunAsync();
```

Ver [BuilderExtensions.cs para workers](/.github/instructions/coding-style-csharp-builder-extensions.instructions.md#plantilla-para-worker-en-segundo-plano) para la plantilla del archivo complementario.

## Patrones a seguir

- **Encadenamiento fluido**: `builder.Configure().Build()` en `Program.cs`.
- **Delegación total**: toda la configuración en `BuilderExtensions.cs`, todo el middleware en `AppExtensions.cs`.
- **Manejo de errores en `Program.cs`**: envolver `app.Run()` en try/catch para log crítico al inicio.
- **Patrones detallados de configuración, telemetría y servicios**: ver [BuilderExtensions.cs](/.github/instructions/coding-style-csharp-builder-extensions.instructions.md).
- **Patrones detallados de middlewares y endpoints**: ver [AppExtensions.cs](/.github/instructions/coding-style-csharp-app-extensions.instructions.md).

## Patrones a evitar

- **Evitar lógica de configuración en `Program.cs`**: delegar a `BuilderExtensions.cs`.
- **Evitar lógica de middleware inline en `Program.cs`**: delegar a `AppExtensions.cs`.
- **Evitar `Program.cs` con más de ~30 líneas**: si crece, falta delegación.
- **Evitar llamadas bloqueantes síncronas**: usar async/await en todo el flujo.

## Validación

- Asegurar que todas las secciones de configuración obligatorias estén presentes.
- Verificar la conectividad con Azure App Configuration antes del arranque.
- Usar `ValidateDataAnnotations()` para la validación de opciones al inicio.

## Referencias

- [ASP.NET Core Fundamentals](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/)
- [Azure App Configuration](https://learn.microsoft.com/en-us/azure/azure-app-configuration/)
- [Options Pattern in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
- [.NET Aspire Overview](https://learn.microsoft.com/en-us/dotnet/aspire/)
