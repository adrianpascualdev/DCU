---
name: netfx-csharp
description: "Directrices de codificación para C# en proyectos .NET Framework 4.7.2, net472. Úsala con cualquier archivo C# o .csproj de un proyecto que apunte a net472 o .NET Framework 4.7.2, con `.csproj` tradicional de MSBuild, estructura heredada no SDK, flujos `msbuild /t:rebuild`, entradas explícitas `<Compile Include=...>`, `packages.config` o `PackageReference`, `App.config`, `Web.config`, ASP.NET MVC 5, ASP.NET Web API 2, WPF, WinForms, WCF, Entity Framework 6 y restricciones del lenguaje C# 7.3, además de rutas y comandos específicos de Windows. IMPORTANTE: coding-style-csharp.instructions.md sigue siendo SIEMPRE válido para proyectos .NET Framework; esta skill añade restricciones específicas de net472 sobre ella. NO LA USES para proyectos dirigidos a .NET 5 o posterior ni a ninguna versión de .NET Core."
argument-hint: "Se carga automáticamente al trabajar en proyectos C# dirigidos a .NET Framework 4.7.2"
version: "1.0.0"
category: "Legacy"
compatibility: "Requiere C# clásico sobre .NET Framework 4.7.2"
---

# Directrices de C# para .NET Framework 4.7.2

Directrices complementarias para proyectos C# dirigidos a **.NET Framework 4.7.2**.

## Dependencias

**Antes de aplicar cualquier guía de esta skill**, lee y aplica la guía base de estilo de C#:

```path
.github/instructions/coding-style-csharp.instructions.md
```

Esta skill es puramente **aditiva**: añade restricciones específicas de net472, APIs del framework y funcionalidades de lenguaje no disponibles sobre ese archivo base. Todas las convenciones generales de C# definidas allí, como nombres, formato, LINQ, comprobaciones de null y orden de miembros, siguen siendo **plenamente válidas y obligatorias** para proyectos .NET Framework 4.7.2 y aplican sin excepción. Lee y aplica siempre ambos archivos juntos.

Esta skill cubre solo restricciones, patrones y APIs recomendadas específicas del framework que difieren de .NET moderno o no existen allí.

## Cuándo usarla

Carga esta skill cuando detectes cualquiera de estas señales en el workspace:

- Un archivo `.csproj` contiene `<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>` o `<TargetFramework>net472</TargetFramework>`
- El proyecto usa `App.config` o `Web.config` como archivo principal de configuración
- Los paquetes NuGet se gestionan con `packages.config`
- El proyecto referencia `System.Web`, `System.Web.Mvc` o `System.Web.Http`
- La persona usuaria pregunta por ASP.NET MVC 5, ASP.NET Web API 2, WCF, WinForms, WPF para .NET Framework o Entity Framework 6

No uses esta skill para proyectos dirigidos a .NET 5, .NET 6, .NET 7, .NET 8, .NET 9 o posterior, ni a ninguna versión de .NET Core.

## Contexto del proyecto

- **Framework objetivo**: .NET Framework 4.7.2 (`net472`)
- **Versión máxima de C#**: C# 7.3, la predeterminada al apuntar a net472; no fuerces una versión superior
- **Formato de proyecto**: `.csproj` tradicional de MSBuild, no SDK-style salvo que el proyecto se haya adaptado explícitamente
- **Gestión de paquetes**: `packages.config` o `PackageReference`; sigue el formato que ya use el proyecto y no migres entre formatos salvo petición explícita

## Gestión del archivo de proyecto

### Estructura de proyecto no SDK-style

Los proyectos tradicionales de .NET Framework usan el formato MSBuild, que se comporta de forma distinta a los proyectos SDK-style:

- **Inclusión explícita de archivos**: los archivos fuente nuevos deben añadirse al archivo de proyecto con una entrada `<Compile Include="..." />`. Estos proyectos no incluyen archivos automáticamente desde el árbol de directorios. Ejemplo: `<Compile Include="Path\To\NewFile.cs" />`
- **Sin imports implícitos**: namespaces, referencias y ensamblados comunes no se infieren igual que en proyectos SDK-style
- **Configuración de build**: espera secciones `<PropertyGroup>` explícitas para Debug y Release
- **Rutas de salida**: espera valores explícitos para `<OutputPath>` y `<IntermediateOutputPath>`
- **Declaración del framework objetivo**: estos proyectos suelen usar `<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>` en lugar de `<TargetFramework>`. Conserva el formato existente salvo que te pidan migrarlo

## Gestión de paquetes NuGet

- No intentes instalar ni actualizar paquetes NuGet automáticamente en proyectos .NET Framework
- Si hace falta cambiar un paquete, indica a la persona usuaria que use Visual Studio NuGet Package Manager o Package Manager Console
- Cuando recomiendes un paquete, verifica que soporte .NET Framework 4.7.2 o .NET Standard 2.0

## Consideraciones del entorno

- Prefiere rutas de Windows con barras invertidas al describir operaciones de archivo en este entorno
- Prefiere comandos apropiados para Windows al sugerir pasos de terminal sobre .NET Framework
- Ten en cuenta el comportamiento del sistema de archivos de Windows al razonar sobre flujos heredados de build y despliegue

## Restricciones de versión del lenguaje

Mantente dentro de C# 7.3. Las siguientes funcionalidades de C# 8.0 o posterior **no están disponibles** en .NET Framework 4.7.2 y no deben usarse salvo que el archivo de proyecto fuerce una `<LangVersion>` superior:

- Implementaciones por defecto de interfaces, C# 8
- Nullable reference types, `?` sobre tipos de referencia y `#nullable enable`; existe soporte parcial de compilador, pero faltan tipos de runtime en net472, así que evítalos salvo adopción consciente del equipo
- `await foreach` e `IAsyncEnumerable<T>`, C# 8 y .NET Core 3.0+
- `IAsyncDisposable` y `await using`, C# 8
- Records e inicializadores `init`, C# 9
- Top-level statements, C# 9
- Directivas globales `using`, C# 10
- File-scoped namespaces, C# 10; usa siempre namespaces con bloque tradicional
- Miembros `required`, C# 11
- Primary constructors, C# 12

Funcionalidades de C# 7.x disponibles y recomendadas:

- Value tuples y elementos de tupla con nombre
- Pattern matching con `is` y `switch`
- Declaraciones inline con `out var`
- Local functions
- Throw expressions
- Expression-bodied members, en métodos, propiedades, constructores y finalizers
- `ref` returns y locals

## Configuración

.NET Framework 4.7.2 no usa `appsettings.json`. Usa `App.config`, para escritorio, consola, WPF o WinForms, o `Web.config`, para ASP.NET, y accede a la configuración mediante `ConfigurationManager`.

```csharp
using System.Configuration;

var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
var apiUrl = ConfigurationManager.AppSettings["ApiUrl"];
```

Las sustituciones específicas por entorno requieren transformaciones en tiempo de build, como `App.Release.config` o `Web.Release.config`, usando `SlowCheetah` o transformaciones de `Web.config`, no cambios de archivo en runtime.

### Assembly binding redirects

Los binding redirects en `App.config` o `Web.config` son necesarios cuando distintos paquetes dependen de versiones distintas del mismo ensamblado. Revisa y actualiza siempre las secciones `<dependentAssembly>` al actualizar paquetes NuGet.

## Inyección de dependencias

.NET Framework 4.7.2 no tiene contenedor DI integrado. Usa uno de terceros que el proyecto ya referencie. Algunas opciones habituales son:

- **Unity** (`Unity.Container`)
- **Autofac**
- **Ninject**
- **Castle Windsor**

No introduzcas un contenedor DI nuevo en un proyecto existente salvo aprobación explícita del equipo. Registra y resuelve servicios en el composition root, por ejemplo `Application_Start` en `Global.asax`, `Main` en una aplicación de consola o la clase `App` en WPF.

## Logging

.NET Framework 4.7.2 no incluye `ILogger<T>` de forma nativa. Usa una librería de logging consolidada que el proyecto ya referencie:

- **Serilog**
- `System.Diagnostics.Trace` o `TraceSource`, para escenarios ligeros y solo framework

Envuelve la librería elegida detrás de una abstracción para poder sustituirla. Las abstracciones `Microsoft.Extensions.Logging`, como `ILogger<T>`, pueden usarse en .NET Framework 4.7.2 si el proyecto añadió el paquete NuGet; prefiérelas en código nuevo cuando el equipo esté de acuerdo.

## Comunicación HTTP

`HttpClient` es el cliente HTTP recomendado. Como .NET Framework 4.7.2 no incluye `IHttpClientFactory`, gestiona cuidadosamente su ciclo de vida:

- Crea una única instancia `static` o de larga vida por endpoint base para evitar socket exhaustion
- No instancies `HttpClient` dentro de métodos de vida corta ni dentro de bloques `using`

```csharp
// BUENO - una sola instancia compartida
private static readonly HttpClient HttpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.example.com")
};
```

`HttpWebRequest` está disponible, pero solo debería usarse en escenarios avanzados, como esquemas concretos de autenticación no soportados por `HttpClient`.

## Acceso a datos

### ADO.NET

Usa `SqlConnection`, `SqlCommand` y `SqlDataReader` para acceso a datos sin abstracciones. Envuelve siempre las conexiones en sentencias `using`.

```csharp
using (var connection = new SqlConnection(connectionString))
using (var command = new SqlCommand("SELECT Id, Name FROM Products WHERE IsActive = 1", connection))
{
    connection.Open();
    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            // Procesa cada fila
        }
    }
}
```

### Entity Framework

Usa **Entity Framework 6.x**, paquete NuGet `EntityFramework`, y no EF Core, que no apunta a net472. Sigue el patrón DbContext y define modelos con Data Annotations o Fluent API.

## Programación asíncrona

`async` y `await`, junto con Task Parallel Library, están plenamente soportados, con estas notas específicas de net472:

- `Task.Run` es la forma correcta de mover trabajo CPU-bound al thread pool
- `Task.FromResult`, `Task.CompletedTask` y `Task.Delay` están disponibles
- `SynchronizationContext`, en WPF, WinForms y ASP.NET clásico, puede causar deadlocks; usa siempre `ConfigureAwait(false)` en código de librería
- Evita patrones sync-over-async como `.Result`, `.Wait()` y `.GetAwaiter().GetResult()`, porque pueden bloquearse bajo el synchronization context clásico
- `IAsyncEnumerable<T>` no está disponible; usa `IEnumerable<T>`, `IObservable<T>` con Rx.NET o patrones personalizados de enumeración asíncrona

## ASP.NET, MVC 5 y Web API 2

Al trabajar en proyectos web:

- Usa **ASP.NET MVC 5**, `System.Web.Mvc`, para UI web, no ASP.NET Core MVC
- Usa **ASP.NET Web API 2**, `System.Web.Http`, para APIs REST, no ASP.NET Core Web API
- Registra rutas en `RouteConfig.cs`, dentro de `Application_Start`, o mediante attribute routing con `[Route]` y `[RoutePrefix]`
- Usa `ActionResult` en MVC e `IHttpActionResult` en Web API 2 como tipos de retorno
- Aplica filtros con `[Authorize]`, `[ValidateAntiForgeryToken]` y filtros personalizados basados en `ActionFilterAttribute` o `ActionFilter`
- Accede a personas usuarias autenticadas mediante `HttpContext.Current.User` o la propiedad `User` del controlador

## WPF y WinForms

- Usa code-behind con patrones MVVM en WPF o MVP en WinForms
- En WPF, enlaza a propiedades del view-model con `INotifyPropertyChanged`; no manipules controles UI directamente desde hilos en segundo plano
- Lleva las actualizaciones de UI al hilo del dispatcher usando `Dispatcher.Invoke` en WPF o `Control.Invoke` y `Control.BeginInvoke` en WinForms

```csharp
// WPF - mover al hilo de UI
Application.Current.Dispatcher.Invoke(() =>
{
    StatusLabel.Content = "Listo";
});
```

## WCF

Usa `System.ServiceModel` para clientes y hosts de servicios SOAP. Genera o actualiza referencias de servicio con **Add Service Reference** o `svcutil.exe`. Cierra o aborta siempre los canales proxy en un bloque `finally`; no confíes solo en `using`, porque `IDisposable.Dispose` sobre un canal faulted lanza excepción.

```csharp
var proxy = new MyServiceClient();
try
{
    proxy.DoWork();
    proxy.Close();
}
catch (Exception)
{
    proxy.Abort();
    throw;
}
```

## Consideraciones de seguridad

- Usa `System.Security.Cryptography` para operaciones criptográficas y prefiere `Aes` frente a `RijndaelManaged`, hoy obsoleto
- Usa `SecureString` con moderación, porque ofrece protección limitada y no está disponible en todos los caminos de interoperabilidad
- Usa `System.Web.Security.Membership` o `FormsAuthentication` solo en proyectos ASP.NET heredados; no los introduzcas en código nuevo
- Guarda secretos en secciones cifradas de configuración o en un almacén externo de secretos; nunca en `App.config` en texto plano
- Valida toda entrada externa con reglas de allow-list y rechaza o sanea antes de usar

## Gestión de hilos

- Prefiere `Task.Run` y `async` o `await` frente a crear objetos `Thread` manualmente
- Usa `ThreadPool.QueueUserWorkItem` solo para cargas fire-and-forget sin valor de retorno ni cancelación estructurada
- Usa `CancellationToken` para señalizar cancelación cooperativa en tareas largas

## Otras trampas y buenas prácticas

### Manejo de fecha y hora

- Prefiere `DateTimeOffset` frente a `DateTime` para marcas temporales absolutas
- Cuando necesites `DateTime`, establece o conserva correctamente `DateTimeKind`, como `Utc` o `Local`
- Usa `CultureInfo.InvariantCulture` en lógica de serialización y parseo

### Manejo de cadenas

- Usa `StringBuilder` para concatenación repetida de cadenas en bucles o rutas calientes
- Especifica siempre `StringComparison` en comparaciones y búsquedas de cadenas

### Ciclo de vida de recursos

- Implementa `IDisposable` correctamente cuando tu tipo posea recursos no administrados o dependencias desechables
- Envuelve recursos desechables en bloques `using` para que la limpieza ocurra incluso si se lanzan excepciones
- Ten presentes las asignaciones en large object heap para objetos de más de unos 85 KB

### Manejo de excepciones

- Captura tipos de excepción específicos en lugar de bloques amplios `Exception` cuando puedas recuperarte con sentido
- No tragues excepciones silenciosamente; regístralas, tradúcelas o relánzalas

## Serialización

- Usa `System.Runtime.Serialization.Json.DataContractJsonSerializer` o `Newtonsoft.Json`, `Json.NET`, para JSON
- Usa `System.Xml.Serialization.XmlSerializer` o `DataContractSerializer` para XML
- `System.Text.Json` no está disponible en net472

## Ensamblado y despliegue

- Firma ensamblados con strong name, `<SignAssembly>true</SignAssembly>`, solo cuando lo requiera un despliegue en GAC o interoperabilidad COM
- Establece `Copy Local` a `False` para ensamblados instalados en GAC y así evitar conflictos de versión
- Usa despliegue estilo `xcopy` en aplicaciones de escritorio o Web Deploy y MSDeploy en aplicaciones ASP.NET

## Referencias

- [.NET Framework 4.7.2 release notes](https://learn.microsoft.com/en-us/dotnet/framework/whats-new/#v472)
- [ASP.NET MVC 5 documentation](https://learn.microsoft.com/en-us/aspnet/mvc/overview/getting-started/introduction/getting-started)
- [ASP.NET Web API 2 documentation](https://learn.microsoft.com/en-us/aspnet/web-api/overview/getting-started-with-aspnet-web-api/tutorial-your-first-web-api)
