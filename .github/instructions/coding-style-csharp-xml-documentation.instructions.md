---
applyTo: "**/*.cs"
description: "Directrices para redactar documentación XML en código C#, cubriendo tipos públicos, métodos, propiedades, uso de inheritdoc y documentación de excepciones."
---

# Directrices de documentación XML en C\#

Directrices para redactar comentarios de documentación XML en código C#, garantizando que IntelliSense, GitHub Copilot y herramientas de generación de documentación dispongan de información completa y precisa sobre la API pública.

## Contexto del proyecto

- **Idioma de la documentación**: castellano.
- **Objetivo**: que cualquier consumidor del ensamblado (directo o vía NuGet) obtenga documentación útil a través de IntelliSense y de los servicios del lenguaje del IDE.
- **Política del repositorio**: usar `<inheritdoc />` cuando el contrato documentado ya existe en una interfaz o clase base, y complementar la implementación solo con la documentación específica que no forme parte de ese contrato.

## Principios generales

- Escribir comentarios para explicar el *por qué*, no el *qué*, cuando sea necesario.
- Eliminar el código comentado antes de hacer commit.

## Reglas de idioma

- Toda la documentación XML (`<summary>`, `<remarks>`, `<param>`, `<returns>`, `<exception>`, `<example>`) debe estar en castellano.
- Los identificadores de código referenciados dentro de la documentación (`<see cref="..."/>`, `<paramref name="..."/>`) mantienen su nombre original en inglés.
- **DEBE** seguir las reglas de localización y uso de recursos del archivo `/.github/instructions/coding-style-csharp-localization.instructions.md`, especialmente para mensajes de validación destinados al usuario final.

## Tipos públicos

Toda clase, interfaz, enum, struct o record público **DEBE** tener un `<summary>` propio que describa su propósito.

```csharp
/// <summary>
/// Representa los datos de un pedido del ERP.
/// </summary>
public class OrderDto : IOrder
```

### Cuando el tipo implementa una única interfaz

Se permite `<inheritdoc cref="IOrder"/>` solo si el `<summary>` de la interfaz describe adecuadamente al tipo concreto. Si la clase aporta un matiz distinto (por ejemplo, "versión serializable" o "versión para colas"), se debe escribir un `<summary>` explícito.

## Métodos y propiedades que implementan interfaces

### Regla general

Se acepta `<inheritdoc />` en métodos y propiedades que implementan una interfaz, **siempre que la interfaz tenga documentación XML completa** (`<summary>`, `<param>`, `<returns>`). En caso de devolver el motodio un tipo Task o void, no es necesario un `<returns>`.

```csharp
/// <inheritdoc />
public async Task<ICustomer> GetCustomerByCode(string code)
```

### Cuándo NO usar `<inheritdoc />`

- Si la interfaz **no tiene documentación XML** o está incompleta, se debe documentar directamente en la implementación.
- Si el método de la implementación tiene **comportamiento adicional significativo** no descrito en la interfaz (por ejemplo, caché, transformaciones, validaciones extra), se debe añadir un `<summary>` explícito o complementar con `<remarks>`.

## Excepciones: regla de complemento obligatorio

Cuando una implementación lanza excepciones que **no están documentadas en la interfaz**, se **DEBEN** documentar con `<exception>` en la implementación, incluso si se usa `<inheritdoc />` para el resto.

```csharp
/// <inheritdoc />
/// <exception cref="ErpException">
/// Se produce cuando el ERP devuelve un error al obtener el cliente.
/// </exception>
/// <exception cref="ArgumentException">
/// Se produce cuando <paramref name="code"/> es nulo o vacío.
/// </exception>
public async Task<ICustomer> GetCustomerByCode(string code)
```

### Razonamiento

Como regla práctica del repositorio, la implementación debe conservar la documentación heredada mediante `<inheritdoc />` y añadir solo las excepciones específicas que introduce el comportamiento concreto. Documentar las excepciones en la implementación:

- Informa al consumidor de errores específicos que solo ocurren en la implementación concreta.
- Permite que herramientas de análisis estático (como Rider, ReSharper o SonarQube) detecten excepciones no gestionadas.
- Complementa sin duplicar: la documentación heredada cubre `<summary>`, `<param>` y `<returns>`; las `<exception>` añadidas cubren el comportamiento específico de la implementación.

## Propiedades de DTOs y modelos públicos

### Propiedades que implementan una interfaz

Se acepta `<inheritdoc />` si la interfaz documenta la propiedad con un `<summary>` claro.

### Propiedades exclusivas del DTO

Toda propiedad pública que **no exista en la interfaz** (es decir, es propia del DTO) **DEBE** tener un `<summary>` explícito.

```csharp
/// <summary>
/// Identificador del pedido externo a NAV.
/// </summary>
public string Code { get; set; }
```

## Constructores

Los constructores públicos **DEBEN** tener un `<summary>` que indique su propósito y un `<param>` por cada parámetro.

```csharp
/// <summary>
/// Inicializa una nueva instancia para llamadas con inyección de dependencias.
/// </summary>
/// <param name="httpClient">Cliente HTTP configurado.</param>
/// <param name="logger">Logger del tipo concreto.</param>
/// <param name="settings">Opciones de configuración del proxy del ERP.</param>
public CustomersProxy(HttpClient httpClient, ILogger<CustomersProxy> logger, IOptions<ErpProxySettings> settings)
    : base(httpClient, logger, settings)
{
}
```

## Interfaces

Las interfaces son la **fuente de verdad** de la documentación para las implementaciones que usen `<inheritdoc />`. Por tanto, toda interfaz pública **DEBE** documentar completamente:

- `<summary>` en la interfaz.
- `<summary>`, `<param>` en cada método. En caso de devolver el método un tipo Task o void, no es requisito incluir un `<returns>`, en caso contrario sí.
- `<summary>` en cada propiedad.
- `<exception>` solo cuando la interfaz establezca un contrato de excepciones que toda implementación deba cumplir.

Si la interfaz no documenta un miembro de forma suficiente, la implementación no debe usar `<inheritdoc />` como sustituto de una documentación ausente.

```csharp
/// <summary>
/// Proxy de gestión de clientes.
/// </summary>
public interface ICustomerProxy : IProxy
{
    /// <summary>
    /// Obtiene los datos de un cliente por su código ERP.
    /// </summary>
    /// <param name="code">Código del cliente en el ERP.</param>
    /// <returns>Los datos del cliente.</returns>
    Task<ICustomer> GetCustomerByCode(string code);

    /// <summary>
    ///     Cambia el cliente del pedido.
    /// </summary>
    /// <param name="customerCode">Nuevo cliente de facturación</param>
    /// <param name="erpReportCode">Número de Informe del Pedido.</param>
    Task ChangeSellTo(string customerCode, string erpReportCode);
}
```

## Enums

Cada enum público **DEBE** tener un `<summary>` en el tipo y en cada valor.

```csharp
/// <summary>
/// Tipo de línea de venta.
/// </summary>
public enum LineType
{
    /// <summary>
    /// Se asocia a un producto o servicio.
    /// </summary>
    Product = 0,

    /// <summary>
    /// La línea es un comentario.
    /// </summary>
    CodeComment = 1
}
```

## Etiquetas opcionales pero recomendadas

- `<remarks>`: para detalles adicionales, referencias a campos externos o notas de implementación.
- `<example>` y `<code>`: para ejemplos de uso, especialmente en APIs públicas consumidas por terceros.
- `<see cref="..."/>`: para referenciar tipos o métodos relacionados.
- `<seealso cref="..."/>`: para referencias complementarias.

## Antipatrones

| Antipatrón | Corrección |
| --- | --- |
| `<inheritdoc />` en un tipo cuya interfaz no tiene documentación | Escribir `<summary>` explícito |
| `<summary>` vacío (`/// <summary> </summary>`) | Escribir una descripción funcional |
| `<returns></returns>` vacío | Describir qué devuelve el método o eliminar la etiqueta si es `void`/`Task` |
| Duplicar toda la documentación de la interfaz en la implementación | Usar `<inheritdoc />` y complementar solo con `<exception>` o `<remarks>` |
| Documentación en inglés cuando el resto del proyecto está en castellano | Mantener castellano en toda la documentación XML |
| Omitir `<exception>` en implementaciones que lanzan excepciones no contractuales | Añadir `<exception>` junto a `<inheritdoc />` |

## Cumplimiento forzado en el proyecto Contract

El proyecto `Contract` tiene configurado el warning **CS1591** como **error de compilación**. Cualquier tipo público o miembro público sin `<summary>` impide el build.

El `.csproj` de `Contract` debe contener siempre:

```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<WarningsAsErrors>$(WarningsAsErrors);CS1591</WarningsAsErrors>
```

Nunca eliminar ni suprimir estas propiedades. El resto de proyectos (`Api`, `Service`, `Data`) no tienen esta restricción.
