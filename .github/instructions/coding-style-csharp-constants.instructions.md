---
applyTo: "**/Constants.cs, **/*Constants.cs"
description: "Directrices para organizar constantes y cadenas mágicas (magics strings) siguiendo los patrones establecidos de clases estáticas anidadas."
---

# Directrices de organización de constantes

Instrucciones para organizar constantes y cadenas mágicas siguiendo los patrones arquitectónicos establecidos.

## Contexto del proyecto

- **Propósito**: centralizar cadenas mágicas, claves de configuración e identificadores
- **Patrón**: clases estáticas anidadas para organización jerárquica
- **Ámbito**: constantes específicas del proyecto (`internal`); las constantes compartidas entre varios proyectos van en el proyecto `Commons` (`public`).
- **Excepción**: **TODOS** los Settings/Options que se pasen a los Servicios, **SIEMPRE** debe de estar definido en el archivo appsettings.json oportuno en el proyecto de la aplicación que contiene los servicios y/o los proyectos de Tests. Seguir las instrucciones del archivo `coding-style-csharp-settings-classes.instructions.md` para la definición de los Settings/Options.

## Estructura básica

```csharp
namespace {ProjectName}.Services.{ServiceName}
{
    internal static class Constants
    {
        internal static class ConnectionStrings
        {
            internal const string TableStorage = @"TableStorage";
            internal const string CosmosDb = @"CosmosDb";
            internal const string AppConfig = @"AppConfig";
        }

        internal static class Settings
        {
            internal const string DefaultLocale = @"DefaultLocale";
            internal const string MaxRetries = @"MaxRetries";
        }

        internal static class TelemetryEvents
        {
            internal const string QueryExecuted = @"QueryExecuted";
            internal const string AgentInvoked = @"AgentInvoked";
        }
    }
}
```

## Categorías habituales

### Cadenas de conexión

```csharp
internal static class ConnectionStrings
{
    internal const string TableStorage = @"TableStorage";
    internal const string TableStorageResponses = @"TableStorageResponses";
    internal const string CosmosDb = @"CosmosDb";
    internal const string AppConfig = @"AppConfig";
    internal const string ApplicationInsights = @"ApplicationInsights";
}
```

### Parámetros de configuración

```csharp
internal static class Settings
{
    internal const string DefaultLocale = @"DefaultLocale";
    internal const string ApplicationId = @"ApplicationId";
    internal const string Sentinel = @"Sentinel";
}
```

### Eventos de telemetría

```csharp
internal static class TelemetryEvents
{
    internal const string RequestStarted = @"{ProjectName}RequestStarted";
    internal const string RequestCompleted = @"{ProjectName}RequestCompleted";
    internal const string RequestFailed = @"{ProjectName}RequestFailed";
    internal const string QueryExecuted = @"QueryExecuted";
    internal const string CacheHit = @"CacheHit";
    internal const string CacheMiss = @"CacheMiss";
}
```

### Cabeceras HTTP

```csharp
internal static class HttpHeaders
{
    internal const string ActivityId = @"X-Activity-Id";
    internal const string UserId = @"X-User-Id";
    internal const string CorrelationId = @"X-Correlation-Id";
    internal const string Locale = @"X-Locale";
}
```

### Nombres de tablas

```csharp
internal static class TableNames
{
    internal const string IntentCategories = @"IntentCategories";
    internal const string UserProfiles = @"UserProfiles";
    internal const string Responses = @"Responses";
}
```

## Constantes específicas de Bot

```csharp
internal static class Constants
{
    internal static class AdaptiveCardsActions
    {
        internal const string ActionIdentifier = @"action";
        internal const string SubmitFeedback = @"submitFeedback";
        internal const string SubmitContinueResponse = @"submitContinueResponse";
    }

    internal static class BotEvents
    {
        internal const string Welcome = @"webchat/welcome";
        internal const string StartConversation = @"startConversation";
    }

    internal static class BotInvokeActions
    {
        internal const string SubmitAction = @"message/submitAction";
    }

    internal static class BotIntents
    {
        internal const string AssistantError = @"AssistantError";
        internal const string UnauthorizedAccess = @"UnauthorizedAccess";
        internal const string Welcome = @"Welcome";
        internal const string Goodbye = @"Goodbye";
    }
}
```

## Constantes específicas de agente

```csharp
internal static class Constants
{
    internal static class Plugins
    {
        internal const string PluginDirectory = @"Plugins";

        internal static class DaxPlugin
        {
            internal const string Name = nameof(DaxPlugin);
        }
    }

    internal static class Parameters
    {
        internal const string Input = @"input";
        internal const string Context = @"context";
        internal const string Locale = @"locale";
    }

    internal static class AgentNames
    {
        internal const string DaxHelper = @"DaxHelper";
        internal const string PerformanceAnalyzer = @"PerformanceAnalyzer";
    }
}
```

## Constantes de AppHost

```csharp
internal static class Constants
{
    internal static class ConnectionStrings
    {
        internal const string AppConfig = @"ConnectionStrings:AppConfig";
    }

    internal static class Settings
    {
        internal static class AssistantServiceOptions
        {
            internal const string ApplicationId = @"AssistantServiceOptions:ApplicationId";
        }

        internal static class BotServiceOptions
        {
            internal const string ApplicationId = @"BotServiceOptions:ApplicationId";
        }

        internal static class DaxHelperServiceOptions
        {
            internal const string ApplicationId = @"DaxHelperServiceOptions:ApplicationId";
        }
    }
}
```

## Constantes compartidas (Commons)

Para constantes usadas en múltiples proyectos, colocarlas en el proyecto compartido `Commons`:

```csharp
namespace {ProjectName}.Commons;

public static class CommonConstants
{
    public static class EndpointGroupName
    {
        public const string AI = @"AI";
        public const string Health = @"Health";
        public const string Admin = @"Admin";
    }

    public static class Agents
    {
        public const string ActionName = @"Asks";
    }

    public static class HttpHeaders
    {
        public const string ActivityId = @"x-activity-id";
        public const string UserId = @"x-user-id";
        public const string CorrelationId = @"x-correlation-id";
        public const string Locale = @"x-locale";
        public const string ApiKey = @"x-api-key";
    }

    public static class ContentTypes
    {
        public const string Json = @"application/json";
        public const string AdaptiveCard = @"application/vnd.microsoft.card.adaptive";
    }
}
```

## Convenciones de nomenclatura

| Elemento | Convención | Ejemplo |
| --- | --- | --- |
| Clase | `internal static class` | `internal static class Constants` |
| Clase anidada | `internal static class` | `internal static class Settings` |
| Constante | `internal const string` | `internal const string MaxRetries` |
| Valor | Cadena verbatim `@""` | `@"TableStorage"` |

## Patrones de uso

### Claves de configuración

```csharp
// Reading configuration
var connectionString = builder.Configuration.GetConnectionString(Constants.ConnectionStrings.TableStorage);
var appId = builder.Configuration[Constants.Settings.ApplicationId];
```

### Registro de eventos de telemetría

```csharp
// Tracking events
telemetryClient.TrackEvent(Constants.TelemetryEvents.QueryExecuted, new Dictionary<string, string>
{
    [@"QueryType"] = queryType,
    [@"Duration"] = duration.ToString(),
});
```

### Adición de cabeceras HTTP

```csharp
// Adding headers
httpRequest.Headers.Add(Constants.HttpHeaders.ActivityId, activityId);
httpRequest.Headers.Add(Constants.HttpHeaders.UserId, userId);
```

## Ubicación de los archivos

- Específico del proyecto: `{Project}/Constants.cs`
- Compartido: `{ProjectName}.Commons/CommonConstants.cs`
- Valores por defecto de configuración: `{Project}/SettingsConstants.cs` (si está separado de Constants)

## Buenas prácticas

1. **Usar cadenas verbatim**: usar siempre `@""` para los valores constantes y evitar problemas con caracteres de escape
2. **Organización jerárquica**: agrupar las constantes relacionadas en clases anidadas
3. **Nombres descriptivos**: usar nombres claros y autodocumentados
4. **Visibilidad interna**: usar `internal` para las constantes específicas del proyecto
5. **Público para lo compartido**: usar `public` solo para las constantes en `Commons`
6. **Evitar duplicaciones**: referenciar las constantes de `Commons` en lugar de duplicarlas
7. **Documentar el propósito**: añadir comentarios XML para constantes no evidentes

> **Ante la duda, preferir el enfoque más simple, preguntar al usuario o solicitar especificaciones más detalladas**
