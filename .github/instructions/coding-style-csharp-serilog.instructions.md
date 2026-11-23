---
applyTo: "**/BuilderExtensions.cs;**/appsettings*.json"
description: "Directrices para configurar Serilog en proyectos ASP.NET Core con dos archivos de log de salida (detallado y operacional), enrichers de correlación y contexto, e integración en BuilderExtensions.cs."
---

# Directrices de configuración de Serilog

Instrucciones para configurar Serilog en proyectos ASP.NET Core siguiendo los patrones del repositorio: configuración basada en `appsettings.json`, dos archivos de salida con rolling diario, enrichment de correlación y contexto, e integración en `BuilderExtensions.cs`.

## Contexto del proyecto

- **Framework objetivo**: .NET 8 con C# 12 o superiores
- **Patrón de configuración**: `ReadFrom.Configuration` desde `appsettings.json`; sin configuración hardcodeada en código
- **Combinar con instrucciones**:
  - [Directrices de BuilderExtensions.cs](/.github/instructions/coding-style-csharp-builder-extensions.instructions.md) para la integración del logger en el builder

## Paquetes NuGet requeridos

Añadir las siguientes referencias al `.csproj` del proyecto de API:

| Paquete | Versión mínima | Propósito |
| ---------------------------------------- | -------------- | ------------------------------------------ |
| `Serilog` | 3.1.1 | Núcleo del framework de logging |
| `Serilog.AspNetCore` | 8.0.0 | Integración con ASP.NET Core y `UseSerilog()` |
| `Serilog.Enrichers.CorrelationId` | 3.0.1 | Añade `CorrelationId` al contexto del log |
| `Serilog.Extensions.Logging` | 8.0.0 | Puente con `Microsoft.Extensions.Logging` |
| `Serilog.Sinks.Debug` | 2.0.0 | Salida al depurador (Visual Studio / VS Code) |
| `Serilog.Sinks.File` | 5.0.0 | Escritura a ficheros con rolling diario |

## Formato de línea de log

Todo sink — archivos de salida y depuración — usa el mismo `outputTemplate`:

```text
{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Application}] [{UserData}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}
```

Campos del template:

| Campo | Formato | Ejemplo |
| ------------------- | ----------------------------- | -------------------------------------- |
| `Timestamp` | `yyyy-MM-dd HH:mm:ss.fff zzz` | `2026-05-15 10:23:45.123 +02:00` |
| `Level` | `u3` (3 letras mayúsculas) | `INF`, `WRN`, `ERR`, `VRB` |
| `Application` | propiedad estática | `Erp.Api` |
| `UserData` | propiedad dinámica (contexto) | usuario autenticado o `System` |
| `CorrelationId` | enriquecido automáticamente | GUID de la petición HTTP |
| `Message` | `lj` (literal + JSON) | cuerpo del mensaje de log |
| `Exception` | traza completa si existe | — |

## Dos archivos de salida obligatorios

Cada proyecto genera exactamente dos archivos de log con rolling diario bajo una ruta relativa al ejecutable (`.\..\Logs\{AppGroup}\{Environment}\`):

### Archivo 1: log detallado — `log_{projectSlug}_.log`

Captura **todos** los niveles desde `Verbose`. Destinado a diagnóstico y depuración profunda.

```json
{
  "Name": "File",
  "Args": {
    "path": "..\\Logs\\{AppGroup}\\{Environment}\\log_{projectSlug}_.log",
    "rollingInterval": "Day",
    "rollOnFileSizeLimit": true,
    "fileSizeLimitBytes": "50000000",
    "retainedFileCountLimit": 7,
    "restrictedToMinimumLevel": "Verbose",
    "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Application}] [{UserData}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"
  }
}
```

### Archivo 2: log operacional 24×7 — `log_{projectSlug}_24x7_.log`

Captura niveles desde `Information`. Destinado a monitorización continua en producción.

```json
{
  "Name": "File",
  "Args": {
    "path": "..\\Logs\\{AppGroup}\\{Environment}\\log_{projectSlug}_24x7_.log",
    "rollingInterval": "Day",
    "rollOnFileSizeLimit": true,
    "fileSizeLimitBytes": "50000000",
    "retainedFileCountLimit": 7,
    "restrictedToMinimumLevel": "Information",
    "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Application}] [{UserData}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"
  }
}
```

### Parámetros de rolling comunes

| Parámetro | Valor | Motivo |
| ------------------------ | ---------- | -------------------------------------------------- |
| `rollingInterval` | `Day` | Un archivo por día; el nombre incluye la fecha |
| `rollOnFileSizeLimit` | `true` | Protege el disco ante ráfagas de logs |
| `fileSizeLimitBytes` | 50 000 000 | 50 MB máximo por fichero antes de rotar |
| `retainedFileCountLimit` | 7 | Conserva los últimos 7 archivos (≈ 1 semana) |

## Estructura completa del nodo `Serilog` en `appsettings.json`

```json
"Serilog": {
  "Using": [
    "Serilog.Sinks.Debug",
    "Serilog.Sinks.Console",
    "Serilog.Sinks.File",
    "Serilog.Enrichers.CorrelationId"
  ],
  "MinimumLevel": "Debug",
  "WriteTo": [
    {
      "Name": "File",
      "Args": {
        "path": "..\\Logs\\{AppGroup}\\{Environment}\\log_{projectSlug}_.log",
        "rollingInterval": "Day",
        "rollOnFileSizeLimit": true,
        "fileSizeLimitBytes": "50000000",
        "retainedFileCountLimit": 7,
        "restrictedToMinimumLevel": "Verbose",
        "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Application}] [{UserData}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"
      }
    },
    {
      "Name": "File",
      "Args": {
        "path": "..\\Logs\\{AppGroup}\\{Environment}\\log_{projectSlug}_24x7_.log",
        "rollingInterval": "Day",
        "rollOnFileSizeLimit": true,
        "fileSizeLimitBytes": "50000000",
        "retainedFileCountLimit": 7,
        "restrictedToMinimumLevel": "Information",
        "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Application}] [{UserData}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"
      }
    },
    {
      "Name": "Debug",
      "Args": {
        "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Application}] [{UserData}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"
      }
    }
  ],
  "Properties": {
    "Application": "{ApplicationPropertyValue}",
    "UserData": "System"
  },
  "Enrich": [
    "WithCorrelationId",
    "FromLogContext"
  ]
}
```

Sustituciones en las rutas de los archivos:

| Marcador | Descripción | Ejemplo |
| ----------------------- | --------------------------------------------- | -------------- |
| `{AppGroup}` | Grupo de aplicación en el servidor de logs | `Erp` |
| `{Environment}` | Entorno de despliegue | `PRE`, `PRO` |
| `{projectSlug}` | Identificador corto del proyecto | `api`, `worker` |
| `{ApplicationPropertyValue}` | Valor de la propiedad `Application` en los logs | `Erp.Api` |

## Integración en `BuilderExtensions.cs`

El método `Configure()` principal llama a `ConfigureLogger()` como **paso 2** de la secuencia de configuración, inmediatamente tras `ConfigureKeyVault()`. El método es privado, estático y devuelve `WebApplicationBuilder` para permitir el encadenamiento fluido.

### Ejemplo correcto

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

### Ejemplo incorrecto — evitar

```csharp
// EVITAR: configuración de sinks hardcodeada en lugar de ReadFrom.Configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("log.log")
    .CreateLogger();

// EVITAR: omitir ClearProviders() antes de registrar Serilog
builder.Host.UseSerilog();
```

### Posición dentro de `Configure()`

```csharp
public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
{
    // 1. Key Vault
    builder.ConfigureKeyVault();

    // 2. Logger y settings
    builder.ConfigureLogger()
           .ConfigureSettings();

    // 3. Servicios de infraestructura y aplicación ...

    return builder;
}
```

## Enriquecimiento dinámico con `LogContext`

Para propagar valores por petición (p. ej. el usuario autenticado) al campo `UserData`, usa `LogContext.PushProperty` en el middleware de acceso antes de invocar el siguiente middleware de la cadena:

```csharp
using (LogContext.PushProperty("UserData", user))
{
    logger.LogInformation("{Method} - {Uri} from Host - {Ip}",
        context.Request.Method, context.Request.GetDisplayUrl(), ip);
    await next.Invoke(context);
}
```

La propiedad `UserData` definida en `appsettings.json > Serilog > Properties` actúa como valor por defecto (`"System"`) cuando no se ha empujado ningún valor al contexto en esa petición.

## Buenas prácticas

- **Centralizar la configuración en `appsettings.json`**: nunca configurar sinks, niveles ni enrichers directamente en código fuera de `ConfigureLogger()`.
- **Mantener el mismo `outputTemplate` en todos los sinks** para garantizar la parsabilidad uniforme de los logs.
- **No omitir `retainedFileCountLimit`**: sin este límite los archivos se acumulan indefinidamente en disco.
- **Ajustar el `MinimumLevel` por entorno** en el `appsettings.{Environment}.json` correspondiente cuando sea necesario reducir la verbosidad en producción.
- **No reutilizar la misma ruta de log** entre proyectos distintos del mismo servidor; usar nombres de archivo que incluyan el `projectSlug`.
