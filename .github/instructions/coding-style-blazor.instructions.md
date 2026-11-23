---
applyTo: "**/*.razor,**/*.razor.cs,**/*.razor.css"
description: "Convenciones de estilo de codificación para proyectos Blazor en .NET con C#. Estas instrucciones aplican a componentes Razor y al código relacionado con componentes Blazor."
---

# Directrices de estilo para Blazor

Directrices para escribir código Blazor y C# limpio, mantenible y consistente siguiendo las convenciones de .NET.

## Contexto del proyecto

- **Frameworks objetivo**: .NET con ASP.NET usando Blazor y C#
- **Referencia de estilo**: [Blazor for ASP.NET Web Forms Developers](https://learn.microsoft.com/en-us/dotnet/architecture/blazor-for-web-forms-developers/)
- **Formato**: el archivo `.editorconfig` **SIEMPRE** tiene prioridad cuando esté presente

## Principios generales

- **Priorizar claridad sobre brevedad**: preferir código legible frente a atajos ingeniosos.
- **Mantener consistencia**: usar formato y convención de nombres uniformes en el códigobase.
- **Documentar**: escribir comentarios claros y concisos para todas las API públicas.
- **Usar características modernas**: aprovechar características modernas de C# solo cuando el usuario lo solicite explícitamente.

## Estilo y estructura de código Blazor

- Escribir código Blazor y C# idiomático y eficiente.
- Seguir las convenciones de .NET y Blazor.
- Usar componentes Razor para el desarrollo de UI basado en componentes.
- Preferir funciones en línea para componentes pequeños y mover la lógica compleja a code-behind o servicios.
- Usar `async/await` donde proceda para asegurar operaciones de UI no bloqueantes.

## Convenciones de nombres

- Aplicar `PascalCase` para nombres de componentes, métodos y miembros públicos.
- Usar `camelCase` para campos privados y variables locales.
- Prefijar nombres de interfaces con "I" (por ejemplo, `IUserService`).

## Pautas específicas de Blazor y .NET

- Utilizar las características de ciclo de vida de componentes de Blazor (por ejemplo, `OnInitializedAsync`, `OnParametersSetAsync`).
- Usar enlace de datos con `@bind` de forma efectiva.
- Aprovechar la Inyección de Dependencias (DI) para servicios en Blazor.
- Estructurar componentes y servicios siguiendo Separación de Responsabilidades.

## Manejo de errores y validación

- Implementar un manejo de errores adecuado para páginas Blazor y llamadas a APIs.
- Usar logging para rastrear errores en el backend y considerar capturar errores a nivel UI con `ErrorBoundary`.
- Implementar validación con `FluentValidation` o `DataAnnotations` en formularios.

## API de Blazor y optimización de rendimiento

- Elegir Blazor server-side o WebAssembly según los requisitos del proyecto.
- Usar métodos asíncronos (`async/await`) para llamadas a APIs o acciones de UI que puedan bloquear el hilo principal.
- Optimizar componentes Razor reduciendo renders innecesarios y usar `StateHasChanged()` con eficiencia.
- Minimizar el árbol de renderizado evitando re-renders salvo cuando sea necesario; usar `ShouldRender()` cuando proceda.
- Usar `EventCallbacks` para manejar interacciones de usuario y pasar solo los datos mínimos necesarios.

### Modos de renderizado (.NET 8+)

| Modo | Dónde se ejecuta | Recomendado para |
| --- | --- | --- |
| `Static` | Servidor (sin interactividad) | Páginas SEO, contenido de marketing |
| `InteractiveServer` | Servidor vía SignalR | Apps en tiempo real, clientes ligeros |
| `InteractiveWebAssembly` | Navegador vía WASM | Apps offline o con procesamiento en cliente |
| `InteractiveAuto` | Servidor primero, luego WASM | Enfoque híbrido |

### Aplicar modos de renderizado

```razor
@* Por componente *@
@rendermode InteractiveServer

@* O en App.razor de forma global *@
<Routes @rendermode="InteractiveAuto" />
```

## Estrategias de cache

- Implementar cache en memoria para datos de uso frecuente, especialmente en Blazor Server; usar `IMemoryCache` para soluciones ligeras.
- En Blazor WebAssembly, usar `localStorage` o `sessionStorage` para persistir estado entre sesiones.
- Considerar estrategias de cache distribuido (por ejemplo, Redis o SQL Server Cache) para aplicaciones que necesiten estado compartido entre múltiples usuarios.
- Cachear llamadas a APIs almacenando respuestas cuando los datos cambian raramente para evitar llamadas redundantes.

## Bibliotecas y gestión de estado

- Usar `CascadingParameters` y `EventCallbacks` de Blazor para compartir estado básico entre componentes.
- Implementar soluciones avanzadas (Fluxor, BlazorState) cuando la aplicación aumente en complejidad.
- Para persistencia cliente en WASM, considerar `Blazored.LocalStorage` o `Blazored.SessionStorage`.
- En Blazor Server, usar servicios con ámbito (`Scoped`) y el patrón `StateContainer` para gestionar estado por sesión minimizando re-renders.

## Diseño de API e integración

- Usar `HttpClient` u otros servicios apropiados para comunicarse con APIs externas o con el backend.
- Implementar manejo de errores en llamadas a APIs con `try-catch` y proporcionar retroalimentación adecuada en la UI.

## Pruebas y depuración en Visual Studio

- Realizar pruebas unitarias e integradas en Visual Studio Enterprise.
- Probar componentes y servicios Blazor con `xUnit`, `NUnit` o `MSTest`.
- Usar `Moq` o `NSubstitute` para simular dependencias en tests.
- Depurar problemas de UI con las herramientas del navegador y depurar backend con Visual Studio.
- Para perfilar y optimizar, usar las herramientas de diagnóstico de Visual Studio.

## Documentación de API y Swagger

- Usar Swagger/OpenAPI para documentar APIs backend.
- Asegurar documentación XML para modelos y métodos API para mejorar la documentación Swagger.

## Patrones de componentes

### Componente básico

```razor
@* Counter.razor *@
<button @onclick="IncrementCount">
    Clics: @count
</button>

@code {
    private int count = 0;

    [Parameter]
    public int InitialCount { get; set; } = 0;

    protected override void OnInitialized()
    {
        count = InitialCount;
    }

    private void IncrementCount() => count++;
}
```

### Parámetros y callbacks de eventos

```razor
@* Parent.razor *@
<ChildComponent Value="@value" ValueChanged="@OnValueChanged" />

@* ChildComponent.razor *@
@code {
    [Parameter] public string Value { get; set; } = "";
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    private async Task UpdateValue(string newValue)
    {
        await ValueChanged.InvokeAsync(newValue);
    }
}
```

### Persistencia de estado (.NET 8+)

```razor
@* Evitar doble fetch durante prerender + hydration *@
@code {
    [PersistentState]
    public List<Product> Products { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        // Solo obtener una vez, persistir durante prerender
        Products ??= await Http.GetFromJsonAsync<List<Product>>("api/products");
    }
}
```

## Patrón de acceso a datos para modo Auto

```csharp
// Interfaz compartida
public interface IProductService
{
    Task<IList<Product>> GetProductsAsync();
}

// Implementación servidor (acceso directo a BD)
public class ServerProductService : IProductService
{
    private readonly AppDbContext _db;
    public async Task<IList<Product>> GetProductsAsync()
        => await _db.Products.ToListAsync();
}

// Implementación cliente (llamada HTTP)
public class ClientProductService : IProductService
{
    private readonly HttpClient _http;
    public async Task<IList<Product>> GetProductsAsync()
        => await _http.GetFromJsonAsync<IList<Product>>("api/products");
}

// Registro
// Servidor: builder.Services.AddScoped<IProductService, ServerProductService>();
// Cliente: builder.Services.AddScoped<IProductService, ClientProductService>();
```

## Anti-patrones a evitar

| Anti-Patrón | Por qué es malo | Mejor enfoque |
| --- | --- | --- |
| Componentes grandes | Difíciles de mantener, renders lentos | Dividir en componentes más pequeños |
| Acceso directo a BD en WASM | No hay DB en el navegador | Usar API HTTP |
| Ignorar `ShouldRender` | Re-renders innecesarios | Usar override cuando convenga |
| JS interop síncrono en Server | Bloquea el circuito SignalR | Usar `IJSRuntime` asíncrono |
| Sin límites de errores | Un error puede romper la app | Usar `<ErrorBoundary>` |
| Olvidar estado de prerender | Doble llamada a API | Usar `[PersistentState]` |

## Mejores prácticas de rendimiento

1. **Virtualizar listas grandes:**

    ```razor
    <Virtualize Items="@products" Context="product">
        <ProductCard Product="@product" />
    </Virtualize>
    ```

2. **Usar `@key` para diffing de listas:**

    ```razor
    @foreach (var item in items)
    {
        <ItemComponent @key="item.Id" Item="@item" />
    }
    ```

3. **Debounce en eventos rápidos:**

    ```csharp
    private Timer? _debounceTimer;

    private void OnInput(ChangeEventArgs e)
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ => InvokeAsync(DoSearch), null, 300, Timeout.Infinite);
    }
    ```

4. **Cargar ensamblados perezosamente (WASM):**

    ```csharp
    var assemblies = await LazyAssemblyLoader.LoadAssembliesAsync(["MyHeavyFeature.wasm"]);
    ```

## JS Interop

### Llamar JavaScript desde `C#`

```csharp
@inject IJSRuntime JS

await JS.InvokeVoidAsync("alert", "¡Hola desde Blazor!");
var result = await JS.InvokeAsync<string>("prompt", "Introduce tu nombre:");
```

### Llamar C# desde JavaScript

```csharp
[JSInvokable]
public static string GetMessage() => "¡Hola desde C#!";
```

```javascript
DotNet.invokeMethodAsync('MyAssembly', 'GetMessage').then(result => console.log(result));
```

## Anti-patrones en Blazor

### Anti-patrones de diseño de componentes

#### Componentes monolíticos

**Problema:** crear componentes grandes que gestionan múltiples responsabilidades.

```razor
@* MALO: Un componente que hace de todo *@
@code {
    private List<Product> products = [];
    private List<Category> categories = [];
    private Cart cart = new();
    private User? user;
    private bool showFilters = false;
    private string searchTerm = "";
    private decimal minPrice;
    private decimal maxPrice;
    // ... 500 líneas más con responsabilidades mixtas
}
```

**Solución:** dividir en componentes enfocados con responsabilidad única.

```razor
@* BUENO: Compuesto por componentes más pequeños *@
<ProductPage>
    <ProductFilters />
    <ProductGrid Products="@products" />
    <CartSidebar />
</ProductPage>
```

#### Parameter drilling

**Problema:** pasar parámetros a través de muchos niveles de componentes.

```razor
@* MALO: Hacer drilling del usuario por varios niveles *@
<Layout User="@user">
    <Sidebar User="@user">
        <UserMenu User="@user">
            <Avatar User="@user" />
        </UserMenu>
    </Sidebar>
</Layout>
```

**Solución:** usar valores en cascada para datos ampliamente requeridos.

```razor
@* BUENO: Valor en cascada *@
<CascadingValue Value="@user">
    <Layout>
        <Sidebar>
            <UserMenu />  @* Accede al usuario vía [CascadingParameter] *@
        </Sidebar>
    </Layout>
</CascadingValue>
```

#### Objetos de parámetros mutables

**Problema:** modificar objetos de parámetros directamente, evitando detección de cambios.

```razor
@* MALO: Mutar objeto de parámetro *@
@code {
    [Parameter] public Product Product { get; set; } = default!;

    private void UpdatePrice()
    {
        Product.Price = 99.99m; // El padre no conocerá este cambio
    }
}
```

**Solución:** usar eventos para notificar al padre sobre cambios.

```razor
@* BUENO: Notificar al padre vía callback *@
@code {
    [Parameter] public Product Product { get; set; } = default!;
    [Parameter] public EventCallback<Product> ProductChanged { get; set; }

    private async Task UpdatePrice()
    {
        var updated = Product with { Price = 99.99m };
        await ProductChanged.InvokeAsync(updated);
    }
}
```

#### Olvidar `EditorRequired`

**Problema:** olvidar marcar parámetros obligatorios, dando lugar a errores en tiempo de ejecución.

```razor
@* MALO: No indicar que esto es obligatorio *@
@code {
    [Parameter] public Product Product { get; set; } = default!;
}
```

**Solución:** usar `EditorRequired` para parámetros obligatorios.

```razor
@* BUENO: El compilador avisa si no se proporciona *@
@code {
    [Parameter, EditorRequired] public Product Product { get; set; } = default!;
}
```

### Anti-patrones de gestión de estado

#### Estado global estático

**Problema:** usar campos estáticos para estado, provocando fuga de datos entre usuarios en modo Server.

```csharp
// MALO: Estado estático compartido entre TODOS los usuarios en Server mode
public static class AppState
{
    public static User? CurrentUser { get; set; }
    public static List<CartItem> Cart { get; } = [];
}
```

**Solución:** usar servicios con ámbito (`Scoped`).

```csharp
// BUENO: Scoped por circuito en Server mode
public class AppState
{
    public User? CurrentUser { get; set; }
    public List<CartItem> Cart { get; } = [];
}

// Registro
services.AddScoped<AppState>();
```

#### Olvidar anular suscripciones a eventos

**Problema:** fugas de memoria por suscripciones a eventos.

```razor
@* MALO: Nunca se desuscribe *@
@code {
    protected override void OnInitialized()
    {
        CartService.OnChange += StateHasChanged;
    }
}
```

**Solución:** implementar `IDisposable`.

```razor
@* BUENO: Limpiar suscripciones *@
@implements IDisposable

@code {
    protected override void OnInitialized()
    {
        CartService.OnChange += StateHasChanged;
    }

    public void Dispose()
    {
        CartService.OnChange -= StateHasChanged;
    }
}
```

#### Doble obtención de datos con prerender

**Problema:** obtener datos dos veces (prerender e interacción).

```razor
@* MALO: Obtiene dos veces *@
@code {
    private List<Product> products = [];

    protected override async Task OnInitializedAsync()
    {
        products = await Http.GetFromJsonAsync<List<Product>>("api/products") ?? [];
    }
}
```

**Solución:** usar `PersistentState` o `PersistentComponentState`.

```razor
@* BUENO: Los datos persisten durante el prerender *@
@code {
    [PersistentState]
    public List<Product> Products { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        if (Products.Count == 0)
        {
            Products = await Http.GetFromJsonAsync<List<Product>>("api/products") ?? [];
        }
    }
}
```

### Anti-patrones de modo de renderizado

#### Acceso directo a BD en componentes WASM

**Problema:** intentar usar DbContext en WebAssembly.

```csharp
// MALO: DbContext no funciona en el navegador
@inject AppDbContext Db

@code {
    protected override async Task OnInitializedAsync()
    {
        products = await Db.Products.ToListAsync(); // Fallará en WASM
    }
}
```

**Solución:** usar abstracción vía API HTTP.

```csharp
// BUENO: Funciona en Server y WASM
@inject IProductService ProductService

@code {
    protected override async Task OnInitializedAsync()
    {
        products = await ProductService.GetProductsAsync();
    }
}

// Implementación servidor usa DbContext
// Implementación cliente usa HttpClient
```

#### Ignorar límites de modo de renderizado

**Problema:** asumir que todos los componentes se ejecutan en el mismo modo.

```razor
@* MALO: El hijo asume el modo del padre *@
<InteractiveParent>
    <StaticChild />  @* Puede no comportarse como se espera *@
</InteractiveParent>
```

**Solución:** establecer modos explícitos y entender los límites.

```razor
@* BUENO: Modo de render explícito *@
<div>
    <StaticHeader />
    <InteractiveContent @rendermode="InteractiveServer" />
    <StaticFooter />
</div>
```

#### Modo Auto sin implementación dual

**Problema:** usar modo Auto sin soportar ambos entornos.

```csharp
// MALO: Solo funciona en servidor
services.AddScoped<IDataService, ServerOnlyDataService>();
```

**Solución:** registrar implementaciones específicas por entorno.

```csharp
// Proyecto servidor
services.AddScoped<IDataService, ServerDataService>();

// Proyecto cliente
services.AddScoped<IDataService, ClientDataService>();
```

### Anti-patrones de rendimiento

#### Falta de @key en listas

**Problema:** Blazor recrea todos los elementos de la lista al cambiar.

```razor
@* MALO: Sin key, diffing pobre *@
@foreach (var item in items)
{
    <ItemComponent Item="@item" />
}
```

**Solución:** usar `@key` para actualizaciones eficientes.

```razor
@* BUENO: Diffing eficiente *@
@foreach (var item in items)
{
    <ItemComponent @key="item.Id" Item="@item" />
}
```

#### Renderizar listas grandes sin virtualización

**Problema:** renderizar miles de ítems a la vez.

```razor
@* MALO: Renderiza los 10.000 ítems *@
@foreach (var item in allItems)
{
    <ItemRow Item="@item" />
}
```

**Solución:** usar componente `Virtualize`.

```razor
@* BUENO: Solo renderiza ítems visibles *@
<Virtualize Items="@allItems" Context="item">
    <ItemRow Item="@item" />
</Virtualize>
```

#### Re-renders innecesarios

**Problema:** componentes se re-renderizan cuando no es necesario.

```razor
@* MALO: Re-render en cada cambio del padre *@
<ExpensiveComponent Data="@unchangingData" />
```

**Solución:** anular `ShouldRender` para componentes costosos.

```razor
@* BUENO: Re-render controlado *@
@code {
    private object? previousData;

    [Parameter] public object? Data { get; set; }

    protected override bool ShouldRender()
    {
        var shouldRender = !ReferenceEquals(Data, previousData);
        previousData = Data;
        return shouldRender;
    }
}
```

#### Operaciones async bloqueantes

**Problema:** usar esperas síncronas que bloquean el hilo de render.

```csharp
// MALO: Bloquea el hilo
protected override void OnInitialized()
{
    var data = Http.GetFromJsonAsync<Data>("api/data").Result; // BLOQUEA!
}
```

**Solución:** usar patrones async adecuados.

```csharp
// BUENO: No bloqueante
protected override async Task OnInitializedAsync()
{
    var data = await Http.GetFromJsonAsync<Data>("api/data");
}
```

### JS Interop anti-patrones

#### Llamadas JS síncronas en Server mode

**Problema:** interop JS síncrono bloquea el circuito SignalR.

```csharp
// MALO: Bloquea en Server mode
var result = ((IJSInProcessRuntime)JS).Invoke<string>("getValue");
```

**Solución:** usar siempre interop JS asíncrono.

```csharp
// BUENO: No bloqueante
var result = await JS.InvokeAsync<string>("getValue");
```

#### Interop JS durante prerender

**Problema:** llamar JS durante prerender cuando no hay navegador.

```csharp
// MALO: Falla durante prerender
protected override async Task OnInitializedAsync()
{
    await JS.InvokeVoidAsync("initializeMap"); // No hay runtime JS durante prerender
}
```

**Solución:** llamar JS solo después del primer render interactivo.

```csharp
// BUENO: Solo cuando es interactivo
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await JS.InvokeVoidAsync("initializeMap");
    }
}
```

#### No disponer referencias de objetos JS

**Problema:** fugas de memoria por referencias de objetos JS.

```csharp
// MALO: Nunca dispone
private IJSObjectReference? module;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        module = await JS.InvokeAsync<IJSObjectReference>("import", "./module.js");
    }
}
```

**Solución:** implementar `IAsyncDisposable`.

```csharp
// BUENO: Limpieza adecuada
@implements IAsyncDisposable

private IJSObjectReference? module;

public async ValueTask DisposeAsync()
{
    if (module is not null)
    {
        await module.DisposeAsync();
    }
}
```

#### Datos grandes en interop JS

**Problema:** pasar objetos grandes por la serialización de interop JS.

```csharp
// MALO: Serializa el dataset completo
await JS.InvokeVoidAsync("processData", hugeDataSet);
```

**Solución:** usar streaming o pasar referencias.

```csharp
// BUENO: Stream de datos grandes
using var streamRef = new DotNetStreamReference(dataStream);
await JS.InvokeVoidAsync("processStream", streamRef);
```

### Manejo de formularios anti-patrones

#### Falta de validación

**Problema:** formularios sin validación.

```razor
@* MALO: Sin validación *@
<EditForm Model="@model" OnSubmit="Submit">
    <InputText @bind-Value="model.Email" />
    <button type="submit">Submit</button>
</EditForm>
```

**Solución:** añadir validadores.

```razor
@* BUENO: Con validación *@
<EditForm Model="@model" OnValidSubmit="Submit">
    <DataAnnotationsValidator />
    <ValidationSummary />

    <InputText @bind-Value="model.Email" />
    <ValidationMessage For="() => model.Email" />

    <button type="submit">Submit</button>
</EditForm>
```

#### No manejar el estado de envío de formularios

**Problema:** envíos dobles y falta de indicador de carga.

```razor
@* MALO: Se puede enviar múltiples veces *@
<button type="submit">Submit</button>
```

**Solución:** rastrear y mostrar el estado de envío.

```razor
@* BUENO: Previene doble envío, muestra estado *@
<button type="submit" disabled="@isSubmitting">
    @(isSubmitting ? "Submitting..." : "Submit")
</button>

@code {
    private bool isSubmitting = false;

    private async Task Submit()
    {
        isSubmitting = true;
        try
        {
            await SubmitFormAsync();
        }
        finally
        {
            isSubmitting = false;
        }
    }
}
```

### Anti-patrones de manejo de errores

#### Sin límites de errores

**Problema:** un error en un componente puede romper toda la aplicación.

```razor
@* MALO: Excepción no manejada rompe el circuito *@
<RiskyComponent />
```

**Solución:** envolver componentes riesgosos en `ErrorBoundary`.

```razor
@* BUENO: Errores contenidos *@
<ErrorBoundary>
    <ChildContent>
        <RiskyComponent />
    </ChildContent>
    <ErrorContent Context="ex">
        <p>Algo salió mal: @ex.Message</p>
    </ErrorContent>
</ErrorBoundary>
```

#### Tragarse excepciones

**Problema:** capturar excepciones sin manejo adecuado.

```csharp
// MALO: Fallo silencioso
try
{
    await SaveDataAsync();
}
catch
{
    // Se silenció - el usuario no sabe que falló
}
```

**Solución:** proporcionar retroalimentación y logging.

```csharp
// BUENO: Retroalimentación y logging
try
{
    await SaveDataAsync();
    message = "Saved successfully";
}
catch (Exception ex)
{
    Logger.LogError(ex, "Failed to save data");
    errorMessage = "Failed to save. Please try again.";
}
```

### Anti-patrones de seguridad

#### Autorización solo del lado cliente

**Problema:** confiar únicamente en cheques de seguridad del lado cliente.

```razor
@* MALO: Cliente solamente - fácilmente eludible *@
@if (isAdmin)
{
    <AdminPanel />
}
```

**Solución:** validar siempre en el servidor.

```csharp
// BUENO: Autorización del lado servidor
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    // El servidor valida cada petición
}
```

#### Exponer datos sensibles en el estado del componente

**Problema:** mantener secretos en el estado del componente visibles al usuario.

```razor
@* MALO: API key visible en el estado del navegador *@
@code {
    private string apiKey = "secret-api-key-12345";
}
```

**Solución:** mantener secretos solo en el servidor.

```csharp
// BUENO: Servicio servidor mantiene secretos
public class SecureService
{
    private readonly string _apiKey;

    public SecureService(IConfiguration config)
    {
        _apiKey = config["ApiKey"]!;
    }

    public async Task CallApiAsync()
    {
        // Usa _apiKey internamente, nunca expuesto al cliente
    }
}
```

#### Confiar en la entrada del cliente

**Problema:** usar entrada del cliente sin validación.

```csharp
// MALO: Uso directo de entrada del usuario
var userId = userIdFromClient;
var data = await Db.GetUserData(userId); // Puede acceder a datos de cualquier usuario
```

**Solución:** validar contra el usuario autenticado.

```csharp
// BUENO: Validar propiedad
var authenticatedUserId = GetAuthenticatedUserId();
if (requestedUserId != authenticatedUserId)
{
    throw new UnauthorizedAccessException();
}
```

## Entregar

- Componentes Blazor interactivos con modo de render adecuado
- Gestión eficiente del estado y flujo de datos
- Manejo correcto de escenarios de prerender
- Renderizado de listas con virtualización para rendimiento

## Validar

- Verificar que los componentes se rendericen correctamente en el modo elegido
- Verificar que el estado persista correctamente entre prerender/hydration
- Verificar ausencia de re-renders innecesarios (usar herramientas del navegador)
- Verificar que JS interop funcione en Server y WASM
- Verificar que `ErrorBoundary` capture fallos de componentes
- Verificar que el modo Auto funcione en ambos entornos

## Referencias

- [Blazor for ASP.NET Web Forms Developers](https://learn.microsoft.com/en-us/dotnet/architecture/blazor-for-web-forms-developers/)
- [Blazor Overview](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Render Modes](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes)
- [Performance Best Practices](https://learn.microsoft.com/en-us/aspnet/core/blazor/performance)
- [State Management](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management)
- [JS Interop](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/)
