---
applyTo: "**/*.cs,**/*.csproj"
description: "Convenciones de estilo de codificación para proyectos C# en .NET y .NET Framework. Estas instrucciones son siempre válidas también para .NET Framework; cuando el objetivo sea net472, carga además la skill `netfx-csharp` para las restricciones específicas del framework."
---

# Directrices de estilo para `C#`

Directrices para escribir código C# limpio, mantenible y consistente siguiendo las convenciones modernas de .NET.

> Estas instrucciones siguen siendo válidas tanto para proyectos modernos de .NET como para proyectos de .NET Framework. Si el proyecto tiene como objetivo .NET Framework 4.7.2 (`net472`), carga y aplica también la skill `netfx-csharp`. La skill es aditiva y aporta únicamente las restricciones específicas del framework y la guía de API que difieren de .NET moderno.

## Contexto del proyecto

- **Frameworks objetivo**: .NET y .NET Framework con C#
- **Referencia de estilo**: [Microsoft .NET C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- **Formato**: el archivo `.editorconfig` **SIEMPRE** tiene prioridad cuando está presente

## Principios generales

- **Claridad sobre brevedad**: preferir código legible antes que atajos ingeniosos.
- **Consistencia**: usar formato y nomenclatura uniformes en todo el código base.
- **Documentación**: escribir comentarios claros y concisos para las APIs públicas.
- **Características modernas**: Utilizar características modernas de C# solo cuando el usuario lo solicite explícitamente.

## Convenciones de nomenclatura

- Usa `PascalCase` para clases, métodos y propiedades.
- Usa `camelCase` para variables locales y parámetros de métodos.
- Usa `PascalCase` para constantes.
- Prefija las interfaces con `I` (por ejemplo, `IOrderService`).
- Usa nombres significativos y descriptivos; evita abreviaturas.
- Prefija los campos privados con guiones bajos (`_`).
- Las convenciones de nombres definidas en `.editorconfig` deben seguirse cuando existan y tienen prioridad sobre estas pautas.

## Formato

- Usa 4 espacios para la indentación (no tabuladores).
- Usa namespaces tradicionales para compatibilidad entre .NET y .NET Framework; evita namespaces con alcance de archivo a menos que el usuario lo solicite y la compatibilidad no sea un requisito.
- Añade una línea en blanco entre definiciones de métodos.
- Inserta una nueva línea antes de la llave de apertura de cualquier bloque de código (por ejemplo, después de `if`, `for`, `while`, `foreach`, `using`, `try`, etc.).
- Coloca las llaves de apertura en una nueva línea para métodos, propiedades y tipos (salvo que uses namespaces con alcance de archivo y se siga ese estilo).
- Usa *pattern matching* y *switch expressions* cuando sea apropiado.
- Usa `nameof` en lugar de literales de cadena al referirse a nombres de miembros.
- Usa `String.Empty` en lugar de `""` para cadenas vacías.
- Garantiza que se crean comentarios XML (`///`) para cualquier API pública. Cuando proceda, incluye `<example>` y `<code>` en la documentación.
- **Siempre** aplica el estilo de formateo definido en `.editorconfig`, que tiene prioridad sobre estas pautas.
- **DEBE** seguir las reglas de localización del archivo `/.github/instructions/coding-style-csharp-localization.instructions.md`, especialmente para mensajes de validación destinados al usuario final.

### Ejemplo: namespaces tradicionales vs file-scoped

Debido a que la organización combina .NET con .NET Framework, no se recomienda usar namespaces con alcance de archivo. En su lugar, **SIEMPRE** use namespaces tradicionales para garantizar compatibilidad.

```csharp
// MAL - No compatible con .NET Framework
namespace MyNamespace;

public class ExampleClass
{
    // ...código existente...
}

// BIEN - Compatible con .NET y .NET Framework
namespace MyNamespace
{
    public class ExampleClass
    {
        // ...código existente...
    }
}
```

Salvo que el usuario solicite explícitamente namespaces con alcance de archivo, prefiere namespaces tradicionales.

## Declaración de variables

- Usa `var` para declaraciones de variables locales cuando el tipo sea obvio.
- Prefiere tipos explícitos si mejora la claridad.

### Ejemplo: declaración de variables

```csharp
// MAL - Antes
int x = 1;
double y = 2.0;
string z = "Hello";
ProductBacklogItem item = new ProductBacklogItem("Test", "Test", 1, 1, 1);

// BIEN - Después
var x = 1;
var y = 2.0;
var z = "Hello";
var item = new ProductBacklogItem("Test", "Test", 1, 1, 1);
```

## Guía del lenguaje

### Arrays

- Usa la sintaxis de inicialización de arrays cuando declares y puebles un array en la misma línea.
- Prefiere `Array.Empty<T>()` en lugar de asignar un nuevo array de longitud cero.
- Mantén el tipo de elemento explícito en el lado izquierdo cuando mejore la claridad más que la inferencia con `new[]`.

```csharp
// BIEN
string[] letters = { "a", "b", "c" };
var identifiers = new[] { 1, 2, 3 };
var noLetters = Array.Empty<string>();

// MAL - población innecesaria paso a paso
var letters = new string[3];
letters[0] = "a";
letters[1] = "b";
letters[2] = "c";
```

### Creación de objetos

- Prefiere inicializadores de objetos y colecciones en lugar de un constructor sin parámetros seguido por asignaciones de propiedades.
- Usa una sintaxis de construcción compatible con .NET y .NET Framework a menos que el usuario solicite explícitamente sintaxis más moderna.
- Pasa dependencias requeridas a través de constructores; reserva setters mutables para estado opcional.

```csharp
// BIEN
var orderLog = new EventLog
{
    Log = "Application",
    Source = "Application Name",
    MachineName = "Computer Name"
};

// MAL - construcción separada de la inicialización
var secondOrderLog = new EventLog();
secondOrderLog.Log = "Application";
secondOrderLog.Source = "Application Name";
secondOrderLog.MachineName = "Computer Name";
```

### Factorías

- Las factorías que no mantienen estado ni dependencias deben declararse como clases `static`.
- Sus métodos de creación deben ser `static` y tener nombres descriptivos, normalmente `Create` o `Create{TypeName}`.
- Solo usar factorías de instancia cuando necesiten estado o dependencias inyectadas para construir el objeto.
- Mantener una factoría por archivo y ubicarla junto al tipo que construye o en la carpeta de la característica correspondiente.

```csharp
public static class PostChatMessageCommandFactory
{
    public static PostChatMessageCommand Create(
        string sessionId,
        OperationIdentity identity,
        string text,
        bool generate,
        MessageDocumentInput? document,
        DateTime requestedAtUtc)
    {
        return new PostChatMessageCommand(
            sessionId,
            identity,
            text,
            generate,
            document,
            requestedAtUtc);
    }
}

## Clases selladas

- Haz las clases `sealed` por defecto. Si una clase necesita ser heredada, márcala explícitamente como `virtual`.

## Uso de `nameof` con excepciones

- Al lanzar excepciones, usa `nameof` para hacer referencia al nombre del parámetro en lugar de codificarlo como literal.

### Ejemplo de `nameof`

```csharp
// MAL - Antes
throw new ArgumentNullException("parameterName");

// BIEN - Después
throw new ArgumentNullException(nameof(parameterName));
```

## Estructura del código

- **Siempre** usa un tipo por archivo (clase, interfaz, enum, etc.).
- Organiza los archivos por característica/dominio cuando sea posible.
- Agrupa las directivas `using` al comienzo del archivo, fuera del namespace, con líneas en blanco entre grupos.
- Todos los espacios de nombres `System` deben listarse primero y el resto en orden alfabético.
- Coloca tipos relacionados en el mismo namespace.
- Usa clases parciales solo cuando sea necesario (por ejemplo, para generación de código).
- Prefiere sentencias en una sola línea para métodos, propiedades y constructores en lugar de extenderlos en múltiples líneas.

### Orden de miembros

Se recomienda organizar los miembros dentro de un tipo en el siguiente orden:

1. Campos
2. Constructores
3. Propiedades e indexadores
4. Métodos
5. Eventos
6. Tipos anidados

Mantén los miembros relacionados juntos y haz que la interfaz pública sea fácil de inspeccionar antes de los detalles de implementación.

## Convenciones de comentarios

- Usa comentarios de documentación XML (`///`) para miembros y tipos públicos.
- Escribe comentarios para explicar el porqué, no el qué, cuando sea necesario.
- Sugiere eliminar el código comentado antes de hacer commit.
- Todos los comentarios y la documentación deben estar siempre en español. Términos específicos pueden permanecer en el idioma original si son ampliamente reconocidos y deben ir entre backticks (por ejemplo, `alvarán`).

## Comprobaciones de nulos y excepciones

- Usa *guard clauses* para la validación de argumentos.
- Usa `nameof` para los nombres de parámetros en excepciones.
- Usa excepciones para condiciones excepcionales, no para el flujo de control habitual.
- Prefiere el tipo de excepción más específico disponible, como `ArgumentException`, `ArgumentNullException`, `ArgumentOutOfRangeException`, `InvalidOperationException` y `NotSupportedException`.
- Envuelve recursos desechables en sentencias `using` cuando su duración sea limitada.
- Documenta las excepciones lanzadas en las API públicas cuando la excepción forme parte del contrato.
- Deriva las excepciones de dominio personalizadas de `Exception`, añade el sufijo `Exception` y mantén sus constructores mínimos y significativos.

```csharp
if (order is null)
{
    throw new ArgumentNullException(nameof(order));
}

using (var connection = new SqlConnection(connectionString))
{
    connection.Open();
    ProcessOrder(order, connection);
}
```

## Consultas LINQ

- Usa nombres significativos para las variables de rango.
- Aplica cláusulas `where` lo antes posible para que las cláusulas posteriores operen sobre un conjunto filtrado.
- Usa nombres descriptivos en `PascalCase` para los miembros proyectados en tipos anónimos.
- Prefiere la sintaxis de expresiones de consulta para joins, agrupamientos o consultas con múltiples cláusulas; usa la sintaxis de métodos para pipelines cortos y lineales.
- Materializa una consulta con `ToList()`, `ToArray()` u métodos similares solo en el límite donde se requiera una colección concreta.

```csharp
var customerOrders =
    from customer in customers
    join order in orders on customer.CustomerId equals order.CustomerId
    where order.Amount > 100
    select new
    {
        CustomerName = customer.Name,
        OrderId = order.Id
    };
```

## Características modernas de `C#`

Debido a que la organización usa tanto .NET como .NET Framework, aprovecha las características modernas de C# solo cuando el usuario lo solicite explícitamente. Si el proyecto tiene como objetivo .NET Framework 4.7.2, la skill `netfx-csharp` define el techo de lenguaje compatible con el framework y las excepciones específicas a la guía siguiente. Al usar características modernas, sigue estas pautas:

- Usa *pattern matching* y miembros con cuerpo de expresión cuando proceda.
- Prefiere inicializadores de objetos y colecciones.
- Evita usar primary constructors.
- Prefiere métodos asíncronos (`async`/`await`) para operaciones públicas ligadas a E/S o potencialmente largas.
- Incluye un parámetro `CancellationToken` cuando la cancelación pueda propagarse de forma significativa, especialmente en operaciones públicas ligadas a E/S o de larga duración.
- Inyecta dependencias mediante el contenedor de DI integrado.
- Usa capacidades de trazado y registro a través de `ILogger<T>`.

## Archivos de configuración

Para aplicaciones modernas de .NET que usan el host genérico o la canalización de configuración de ASP.NET Core, gestiona distintos archivos de ajustes para diferentes entornos y usuarios como sigue. Si el proyecto tiene como objetivo .NET Framework 4.7.2, usa `App.config` o `Web.config` en su lugar, según define la skill `netfx-csharp`:

```csharp
if (Debugger.IsAttached)
{
    builder.Configuration.AddJsonFile(@"appsettings.debug.json", optional: true, reloadOnChange: true);
}

builder.Configuration.AddJsonFile($@"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                     .AddJsonFile($@"appsettings.{Environment.UserName}.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables();
```

## Tipos de referencia anulables (nullable reference types)

- Declara las variables como no nulas y comprueba `null` en los puntos de entrada.
- Usa siempre `is null` o `is not null` en lugar de `== null` o `!= null` o la alternativa para .NET Framework.
- Confía en las anotaciones de nulabilidad de C# y no añadas comprobaciones de nulo cuando el sistema de tipos indica que un valor no puede ser nulo.

## Referencias

- [Microsoft .NET C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
