---
applyTo: "**/Settings/**/*.cs, **/*Settings.cs"
description: "Directrices para crear clases de settings de configuración con el Patrón de Opciones, usando enlazado fuerte de tipos, validación, inmutabilidad y registro consistente."
---

# Directrices de desarrollo de clases de configuración

Directrices para diseñar clases de configuración siguiendo el Patrón de Opciones en .NET.

## Contexto del proyecto

- **Framework objetivo**: .NET 10 con C# 14
- **Patrón**: `IOptions<T>`, `IOptionsSnapshot<T>`, `IOptionsMonitor<T>`
- **Validación**: Data Annotations con validación fail-fast en el arranque

## Reglas de idioma obligatorias

- La documentación XML (`<summary>`, `<remarks>`, `<param>`, `<returns>`, `<exception>`) debe estar en castellano y **DEBE** seguir las instrucciones del archivo `/.github/instructions/coding-style-csharp-xml-documentation.instructions.md`.
- **DEBE** seguir las reglas de localización y uso de recursos del archivo `/.github/instructions/coding-style-csharp-localization.instructions.md`, especialmente para mensajes de validación destinados al usuario final.

## Cuándo crear una clase de configuración

Crear una clase de settings dedicada cuando:

1. Se requiere configuración externa (`appsettings.json`, variables de entorno, App Configuration, Key Vault)
2. Existen tres o más valores relacionados
3. Se necesita validación de formato, rango o obligatoriedad
4. Se requiere hot-reload de configuración
5. Se busca testabilidad mediante inyección de configuraciones alternativas

No crear clases de settings para:

- Valores únicos que pueden inyectarse como parámetro
- Un único endpoint o cadena de conexión aislada
- Constantes de compilación que no cambian
- Valores derivados de otra configuración (calcular en el servicio)

## Proceso recomendado

### Paso 1: identificar valores

Extraer de la especificación:

- Cadenas de conexión y endpoints
- Credenciales (marcar sensibles)
- Ajustes de comportamiento (timeouts, reintentos, tamaños)
- Feature flags
- Identificadores de recursos

### Paso 2: elegir tipos

| La especificación indica | Tipo recomendado | Nota |
| --- | --- | --- |
| URL, endpoint, address | `Uri` | Seguridad de tipo |
| connection string | `string` | Compatibilidad SDK |
| timeout, duration, interval | `double` o `TimeSpan` | Preferir `double` en segundos/minutos simples |
| count, limit, size | `int` | Añadir `[Range]` |
| percentage, ratio, probability | `double` | Rango `0.0` a `1.0` |
| enable, disable, use, allow | `bool` | Definir valor por defecto seguro |
| optional, if provided, when set | `T?` | Sin `required` |
| list of, multiple, collection | `IReadOnlyList<T>` | Colección inmutable |
| one of, mode, strategy | `enum` | Añadir `[EnumDataType]` |

### Paso 3: obligatorio u opcional

- **Obligatorio**: `required` + `[Required]`
- **Opcional con valor por defecto**: sin `required`, con default explícito
- **Opcional anulable**: `T?`, sin `[Required]`

### Paso 4: convenciones de nomenclatura

| Elemento | Convención | Ejemplo |
| --- | --- | --- |
| Clase | `{Feature}Settings` | `WeatherApiSettings` |
| Namespace | `{Project}.Services.{Area}.Settings` | `MyApp.Services.Storage.Settings` |
| Archivo | `{ClassName}.cs` | `WeatherApiSettings.cs` |
| Sección de configuración | Nombre de clase | `"WeatherApiSettings": {}` |
| Propiedades | PascalCase descriptivo | `MaxRetryCount`, `Endpoint` |

### Paso 5: validación

Para cada propiedad, definir:

- Valor mínimo y máximo válidos
- Formato requerido
- Si vacío/blancos es válido
- Dependencias entre propiedades

## Ejemplo trabajado

**Especificación**: servicio que consume una API externa de clima con URL base, API key, timeout, caché y concurrencia.

```csharp
namespace MyProject.Services.Weather.Settings
{
    /// <summary>
    /// Opciones de configuración para la integración con la API de Weather.
    /// </summary>
    /// <remarks>
    /// Vincular a la sección `WeatherApiSettings` en `appsettings.json`.
    /// </remarks>
    public sealed class WeatherApiSettings
    {
        [Required]
        public required Uri BaseUrl { get; init; }

        [Required]
        public required string ApiKey { get; init; }

        [Range(1, 120)]
        public double TimeoutSeconds { get; init; } = 30;

        public bool EnableCaching { get; init; } = true;

        [Range(1, 60)]
        public int CacheDurationMinutes { get; init; } = 5;

        [Range(1, 100)]
        public int MaxConcurrentRequests { get; init; } = 10;

        public Uri? ProxyUrl { get; init; }
    }
}
```

```json
{
  "WeatherApiSettings": {
    "BaseUrl": "https://api.weather.example.com/v2",
    "ApiKey": "managed-in-user-secrets-or-key-vault",
    "TimeoutSeconds": 30,
    "EnableCaching": true,
    "CacheDurationMinutes": 5,
    "MaxConcurrentRequests": 10,
    "ProxyUrl": null
  }
}
```

```csharp
builder.Services.AddOptionsWithValidateOnStart<WeatherApiSettings>()
                .Bind(builder.Configuration.GetSection(nameof(WeatherApiSettings)))
                .ValidateDataAnnotations();
```

## Convenciones clave

### Inmutabilidad

- Usar `required` + `init` en propiedades obligatorias
- Usar `init` con defaults en propiedades opcionales
- Evitar `set` público mutable

### Jerarquía de tipos

- Usar `sealed` por defecto en clases `Settings` .
- Usar herencia solo si existe una necesidad real y explícita.

## Atributos de validación recomendados

- `[Required]` para obligatorios
- `[Range]` para valores numéricos
- `[EnumDataType]` para enums
- Mensajes de error personalizados cuando aporten claridad.
- Los mensajes de validación (o de error) destinados al usuario deben obtenerse desde recursos (`.resx`) usando `ErrorMessageResourceType` y `ErrorMessageResourceName`
- Evitar `ErrorMessage` inline cuando el texto pueda llegar al usuario final.

```csharp
public sealed class ApiSettings
{
    [Required(
        ErrorMessageResourceType = typeof(ValidationMessages),
        ErrorMessageResourceName = nameof(ValidationMessages.EndpointRequired))]
    public required Uri Endpoint { get; init; }

    [Required(
        ErrorMessageResourceType = typeof(ValidationMessages),
        ErrorMessageResourceName = nameof(ValidationMessages.ApiKeyRequired))]
    [MinLength(
        32,
        ErrorMessageResourceType = typeof(ValidationMessages),
        ErrorMessageResourceName = nameof(ValidationMessages.ApiKeyMinLength))]
    public required string ApiKey { get; init; }

    [Range(
        1,
        300,
        ErrorMessageResourceType = typeof(ValidationMessages),
        ErrorMessageResourceName = nameof(ValidationMessages.TimeoutSecondsRange))]
    public double TimeoutSeconds { get; init; } = 30;
}
```

## Patrón de configuración anidada

Usar clases anidadas para configuraciones complejas:

```csharp
public sealed class AzureOpenAISettings
{
    [Required]
    public required Uri Endpoint { get; init; }

    [Required]
    public required string ChatModelDeploymentName { get; init; }

    public bool UseTokenCredentialAuthentication { get; init; }

    public TokenCredentialsSettings? TokenCredentialsSettings { get; init; }
}

public sealed class TokenCredentialsSettings
{
    [Required]
    public required string ClientId { get; init; }

    [Required]
    public required string ClientSecret { get; init; }

    [Required]
    public required string TenantId { get; init; }
}
```

## Registro y consumo

### Registro básico

```csharp
builder.Services.AddOptionsWithValidateOnStart<MySettings>()
                .Bind(builder.Configuration.GetSection(nameof(MySettings)))
                .ValidateDataAnnotations();
```

### Opciones con nombre

```csharp
builder.Services.AddOptionsWithValidateOnStart<AzureOpenAISettings>(@"Primary")
                .Bind(builder.Configuration.GetSection(@"AzureOpenAI:Primary"))
                .ValidateDataAnnotations();
```

### Recarga en caliente

Usar `IOptionsMonitor<T>` para servicios de larga vida:

```csharp
public sealed class MyService
{
    private MySettings _options;

    public MyService(IOptionsMonitor<MySettings> optionsMonitor)
    {
        _options = optionsMonitor.CurrentValue;
        optionsMonitor.OnChange(newOptions => _options = newOptions);
    }
}
```

| Interfaz | Tiempo de vida típico | Uso |
| --- | --- | --- |
| `IOptions<T>` | Singleton | Configuración estática |
| `IOptionsSnapshot<T>` | Scoped | Configuración por solicitud |
| `IOptionsMonitor<T>` | Singleton | Recarga en caliente |

## Seguridad

- No hardcodear secretos en clases Settings
- Almacenar secretos en Key Vault
- Evitar logs de propiedades sensibles
- Preferir `DefaultAzureCredential` o identidades administradas cuando aplique

## Organización de archivos

```text
Project/
├── Settings/
│   ├── AgentSettings.cs
│   ├── ResilienceSettings.cs
│   └── TelemetrySettings.cs
```

## Lista de verificación

- [ ] Clase `sealed` salvo necesidad explícita
- [ ] Propiedades obligatorias con `required init`
- [ ] Propiedades opcionales con defaults razonables
- [ ] Documentación XML clara
- [ ] Atributos de validación apropiados
- [ ] Registro con `AddOptionsWithValidateOnStart`
- [ ] Sección de configuración alineada con nombre de clase
- [ ] Datos sensibles fuera del código

## Referencias

- [Patrón Options en ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options). ***En esta guía se usa el sufijo `Settings` en lugar de `Options`.***

> **Ante la duda, preferir el enfoque más simple, preguntar al usuario o solicitar especificaciones más detalladas**
