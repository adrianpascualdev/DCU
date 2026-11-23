---
description: 'Directrices generales de estilo para escribir código jQuery correcto, idiomático, seguro y de alto rendimiento en archivos JavaScript y HTML.'
applyTo: '**/scripts/**/*.js, **/Scripts/**/*.js, **/*.html, **/*.htm, **/*.aspx, **/*.cshtml'
---

# Directrices de desarrollo para jQuery

Buenas prácticas generales para escribir código jQuery correcto, idiomático, seguro y de alto rendimiento. Estas directrices aplican independientemente de la versión concreta de jQuery en uso y abarcan selectores, eventos, manipulación del DOM, Ajax, Deferred/promesas, efectos, creación de plugins y rendimiento.

> **Guía por versión**: La skill `jquery` aporta restricciones específicas para jQuery 2.1.3 (URLs de CDN, soporte de navegadores, APIs obsoletas y funcionalidades no disponibles). Cargar siempre esa skill junto con este archivo cuando se trabaje en el proyecto.

## Instrucciones generales

- Encerrar el código de nivel de página dentro de `$(document).ready()` o el atajo `$(function() { ... })` para garantizar que el DOM esté listo antes de manipularlo.
- Usar `jQuery.noConflict()` cuando jQuery conviva con otras librerías que usan `$`.
- Envolver cada plugin y módulo independiente en una IIFE que reciba `jQuery` y nombre el parámetro `$`:

```javascript
(function($) {
    // Uso seguro de $ como jQuery aquí.
}(jQuery));
```

- Preferir `.on()` y `.off()` para enlazar y eliminar eventos — `.bind()`, `.delegate()` y `.live()` están obsoletos o eliminados.
- Usar `.prop()` para propiedades booleanas del DOM (`checked`, `disabled`, `selected`); usar `.attr()` para atributos HTML.
- Insertar contenido proporcionado por usuarios con `.text()` — nunca `.html()` — para prevenir XSS.
- Usar `jQuery.parseHTML(htmlString, document, false)` para parsear cadenas HTML de forma segura; el tercer argumento `false` impide la ejecución de scripts inline.

## Buenas prácticas para selectores

- Preferir selectores de ID (`$("#myId")`) para búsquedas de un solo elemento — se mapean a `getElementById` nativo y son los más rápidos.
- Cachear selecciones jQuery en variables prefijadas con `$` para evitar consultas DOM repetidas:

```javascript
var $sidebar = $(".sidebar");
$sidebar.addClass("active");
$sidebar.find(".item").show();
```

- Usar `.find()` desde un padre cacheado en lugar de selectores descendientes largos:

```javascript
// Correcto
var $container = $("#results");
$container.find(".row");

// Incorrecto — re-consulta cada vez
$("#results .row");
```

- Evitar pseudo-selectores de extensión de jQuery (`:has()`, `:contains()`, `:animated`, `:visible`, `:hidden`) cuando los selectores CSS estándar son suficientes — los selectores de extensión evitan el motor nativo `querySelectorAll` y son más lentos.
- Evitar el selector universal `$('*')`; es extremadamente lento en DOMs grandes.
- Al combinar múltiples selectores, preferir llamadas separadas o `.add()` en lugar de un mega-selector separado por comas si las partes individuales pueden cachearse.

## Eventos

- Adjuntar eventos con `.on()` y eliminarlos con `.off()`:

```javascript
$element.on("click", handler);
$element.off("click", handler);
```

- Usar delegación de eventos para elementos que se añaden dinámicamente:

```javascript
// Delegar click para cualquier .item presente o futuro dentro de #list
$("#list").on("click", ".item", function(event) {
    $(this).toggleClass("selected");
});
```

- Nombrar eventos (namespaces) para facilitar la eliminación masiva, especialmente en plugins:

```javascript
$element.on("click.myPlugin", handler);
$element.on("mouseenter.myPlugin", handler);

// Eliminar todos los handlers del namespace de una vez
$element.off(".myPlugin");
```

- Dentro de un handler, `this` (y `event.currentTarget`) se refiere al elemento al que está enlazado el handler (o la coincidencia de la delegación). `event.target` es el elemento real que originó el evento.
- Llamar a `event.preventDefault()` para cancelar la acción predeterminada del navegador; llamar a `event.stopPropagation()` para detener el burbujeo.
- Los eventos globales de Ajax (`ajaxStart`, `ajaxStop`, `ajaxSend`, `ajaxComplete`, `ajaxError`, `ajaxSuccess`) deben adjuntarse a `$(document)`:

```javascript
$(document).on("ajaxStart", function() {
    $("#spinner").show();
});
```

## Manipulación del DOM

- Usar `.append()`, `.prepend()`, `.after()`, `.before()` para inserciones.
- Construir HTML fuera de bucles — concatenar una cadena o usar un fragmento de documento, y luego insertarlo de una vez:

```javascript
// Good
var html = "";
for (var i = 0; i < items.length; i++) {
    html += "<li>" + items[i].name + "</li>";
}
$("ul#results").append(html);

// Bad — provoca reflow en cada iteración
for (var i = 0; i < items.length; i++) {
    $("ul#results").append("<li>" + items[i].name + "</li>");
}
```

- Usar `.detach()` en lugar de `.remove()` cuando se planea reinsertar un elemento — `.detach()` preserva eventos ligados y datos de jQuery.
- Usar `.clone(true)` para clonar en profundidad un elemento junto con sus eventos y datos; `.clone(false)` copia sin eventos.
- Usar `.empty()` para eliminar todos los hijos de un elemento manteniendo el elemento en sí.
- Nunca pasar datos no confiables a `.html()`, `.append()`, `.prepend()`, `.after()`, `.before()` o `$()` — usar `.text()` para inserciones de texto.

## Ajax

- Preferir `$.ajax()` para control total. Usar la interfaz Deferred (`.done()`, `.fail()`, `.always()`) en lugar de las propiedades de callback heredadas (`success`, `error`, `complete`):

```javascript
$.ajax({
    url: "/api/data",
    method: "GET",
    dataType: "json"
}).done(function(data) {
    // Manejar éxito
}).fail(function(jqXHR, textStatus, errorThrown) {
    // Manejar error
}).always(function() {
    // Se ejecuta tras éxito o error
});
```

- Métodos abreviados para casos comunes:
  - `$.get(url, data, callback, dataType)`
  - `$.post(url, data, callback, dataType)`
  - `$.getJSON(url, data, callback)` — equivalente a `$.get` con `dataType: "json"`
  - `$.getScript(url, callback)` — carga y ejecuta un script remoto
  - `$element.load(url, data, callback)` — obtiene HTML e inyecta el resultado en el elemento coincidente
- Establecer `dataType` explícitamente (`"json"`, `"html"`, `"text"`, `"xml"`, `"script"`) para mayor claridad y evitar parseos ambiguos.
- Manejar siempre errores — un `.fail()` sin manejar silenciosamente oculta errores de red y servidor.
- Usar cabeceras CORS en el servidor para solicitudes cross-domain. JSONP (`dataType: "jsonp"`) está disponible pero conlleva riesgos de seguridad — evitarlo cuando CORS sea una opción.
- Usar `$.ajaxSetup()` con moderación — anula los valores por defecto para **todas** las solicitudes Ajax futuras y dificulta razonar sobre el comportamiento.

## Deferred y promesas

- `$.Deferred()` crea un nuevo Deferred; llamar `.resolve()` o `.reject()` para resolverlo.
- `.promise()` devuelve una promesa de solo lectura a partir de un Deferred — exponer esto en lugar del Deferred para evitar que llamadores externos lo resuelvan o rechacen.
- Encadenar operaciones asíncronas con `.then(doneFn, failFn)` — preferir `.then()` sobre `.done()` + `.fail()` por permitir encadenamiento adicional:

```javascript
$.ajax("/api/user")
    .then(function(user) {
        return $.ajax("/api/orders?userId=" + user.id);
    })
    .then(function(orders) {
        renderOrders(orders);
    })
    .fail(function() {
        showError();
    });
```

- Usar `$.when()` para esperar múltiples Deferreds:

```javascript
$.when($.ajax("/api/users"), $.ajax("/api/roles"))
    .done(function(usersResult, rolesResult) {
        // usersResult[0] y rolesResult[0 contienen los datos de respuesta
    })
    .fail(function() {
        // Al menos una solicitud falló
    });
```

## Efectos y animación

- Visibilidad: `.show()`, `.hide()`, `.toggle()`
- Fundidos: `.fadeIn()`, `.fadeOut()`, `.fadeTo()`, `.fadeToggle()`
- Deslizamientos: `.slideDown()`, `.slideUp()`, `.slideToggle()`
- Animación personalizada con `.animate()` — solo propiedades CSS numéricas son animables:

```javascript
$element.animate({ opacity: 0.5, height: "200px" }, 400, function() {
    // Callback al completar la animación
});
```

- Llamar `.stop(true, true)` antes de iniciar una nueva animación sobre el mismo elemento para limpiar la cola y avanzar al estado final — evita acumulación de colas en interacciones rápidas:

```javascript
$element.stop(true, true).fadeIn(300);
```

- Desactivar animaciones globalmente con `$.fx.off = true` para pruebas de accesibilidad o entornos de bajo consumo.

## Creación de plugins

- Extender `$.fn` con un **único** nombre de método por plugin para minimizar colisiones de espacio de nombres:

```javascript
(function($) {

    $.fn.myPlugin = function(options) {
        var settings = $.extend({
            color: "#556b2f",
            speed: 400
        }, options);

        return this.each(function() {
            var $el = $(this);
            $el.css("color", settings.color);
        });
    };

}(jQuery));
```

- Siempre devolver `this` (o `return this.each(...)`) para preservar el encadenamiento de métodos jQuery.
- Aceptar un objeto `options` combinado con valores por defecto mediante `$.extend({}, defaults, options)`.
- Usar `.each()` cuando se necesite lógica por elemento (leer atributos `data`, calcular posiciones).
- Usar strings de parámetros (`$el.myPlugin("destroy")`) o un mapa de métodos separado para manejar múltiples acciones dentro de una única ranura `$.fn`.

## Rendimiento

- **Cachear objetos jQuery** — almacenar selecciones en variables; evitar reconsultar el DOM dentro de bucles.
- **Delegar eventos** — ligar un handler en un ancestro compartido en vez de handlers individuales en muchos elementos.
- **Append fuera de bucles** — construir la cadena HTML completa o un fragmento de documento y luego insertarlo de una vez.
- **Cachear `.length`** en variables de bucle:

```javascript
for (var i = 0, len = $items.length; i < len; i++) {
    // Usar $items.eq(i) o $items[i]
}
```

- **Detach antes de manipulaciones pesadas** — llamar `.detach()`, modificar el subárbol y luego reinsertar con `.appendTo()` para evitar reflows repetidos.
- **Usar selectores de ID** cuando sea posible — son una orden de magnitud más rápidos que selectores por clase o atributos.
- **Preferir clases CSS sobre `.css()`** para cambios de estilo en bloque — añadir una clase provoca un único reflow; llamar `.css()` en muchos elementos provoca uno por llamada.
- **Evitar `.each()`** cuando un método jQuery ya opera sobre la colección completa — métodos como `.addClass()`, `.css()`, `.attr()` iteran internamente.

## Seguridad

- **Nunca** pasar entrada de usuario sin sanear a `$()`, `.html()`, `.append()`, `.prepend()`, `.after()` o `.before()` — estos parsean y ejecutan HTML, abriendo la puerta a ataques XSS.
- Usar `.text()` en lugar de `.html()` para insertar contenido proporcionado por usuarios.
- Usar `$.parseHTML(htmlString, document, false)` con `false` como tercer argumento para deshabilitar la ejecución de scripts al parsear HTML de terceros.
- Validar y sanear las respuestas del servidor antes de renderizarlas en el DOM.
- Evitar `eval()` y `$.globalEval()` — ejecutan código arbitrario.
- En `$.ajax()`, establecer `contentType` y `dataType` explícitamente para evitar interpretaciones inesperadas de la respuesta.
- Evitar JSONP (`dataType: "jsonp"`) para endpoints no confiables — JSONP inyecta una etiqueta `<script>`, permitiendo al servidor remoto ejecutar JavaScript arbitrario en la página.

## API obsoletas y eliminadas

Los siguientes métodos fueron **eliminados** en jQuery 1.9 (y por tanto no existen en 2.1.3) o están **obsoletos** (siguen funcionando pero no deben usarse). Usar siempre el reemplazo.

| API | Estado en 2.1.3 | Reemplazo |
| :--- | :--- | :--- |
| `.live()` | Eliminado | `.on()` con delegación en `$(document)` |
| `.die()` | Eliminado | `.off()` con delegación |
| `jQuery.browser` | Eliminado | Detección de características (por ejemplo, Modernizr) |
| `jQuery.sub()` | Eliminado | Ninguno — eliminado del core |
| `.toggle(fn, fn)` (evento) | Eliminado | Seguimiento manual de estado con `.on("click", ...)` |
| `.bind()` | Obsoleto | `.on()` |
| `.unbind()` | Obsoleto | `.off()` |
| `.delegate()` | Obsoleto | `.on()` con argumento selector |
| `.undelegate()` | Obsoleto | `.off()` con argumento selector |
| `.andSelf()` | Obsoleto | `.addBack()` |
| `.size()` | Obsoleto | Propiedad `.length` |
| `.context` | Obsoleto | No usar |
| `.selector` | Obsoleto | No usar |
| `deferred.pipe()` | Obsoleto | `.then()` |
| `jQuery.parseJSON()` | Disponible | Preferir `JSON.parse()` nativo |
| `jQuery.isArray()` | Disponible | Preferir `Array.isArray()` nativo |
| `jQuery.isFunction()` | Disponible | Preferir `typeof fn === "function"` |
| `jQuery.isNumeric()` | Disponible | Preferir comprobaciones nativas |
| `jQuery.type()` | Disponible | Preferir `typeof` / `instanceof` nativos |
| `jQuery.trim()` | Disponible | Preferir `String.prototype.trim()` nativo |
| `$.ajaxSetup()` | Disponible | Evitar — afecta a todas las peticiones globalmente |

## Patrones comunes

### Document ready

```javascript
$(function() {
    // DOM listo — inicializar la aplicación aquí.
    initNavigation();
    loadDashboardData();
});
```

### Delegación de eventos

```javascript
(function($) {

    var $todoList = $("#todo-list");

    // Funciona para .delete-btn actuales y futuros dentro de #todo-list
    $todoList.on("click", ".delete-btn", function(event) {
        event.preventDefault();
        $(this).closest("li").fadeOut(300, function() {
            $(this).remove();
        });
    });

}(jQuery));
```

### Ajax GET con encadenamiento Deferred

```javascript
function fetchUser(userId) {
    return $.ajax({
        url: "/api/users/" + encodeURIComponent(userId),
        method: "GET",
        dataType: "json"
    });
}

fetchUser(42)
    .done(function(user) {
        $("#username").text(user.name);
    })
    .fail(function(jqXHR) {
        $("#username").text("Error al cargar el usuario");
    });
```

### Ajax POST con datos

```javascript
$.ajax({
    url: "/api/orders",
    method: "POST",
    contentType: "application/json; charset=utf-8",
    data: JSON.stringify({ productId: 101, quantity: 2 }),
    dataType: "json"
}).done(function(response) {
    alert("Pedido creado: " + response.orderId);
}).fail(function(jqXHR, textStatus) {
    alert("Error: " + textStatus);
});
```

### Esqueleto de plugin

```javascript
(function($) {

    var defaults = {
        speed: 400,
        easing: "swing",
        onComplete: $.noop
    };

    $.fn.accordion = function(options) {
        var settings = $.extend({}, defaults, options);

        return this.each(function() {
            var $accordion = $(this);
            var $headers = $accordion.find(".accordion-header");

            $headers.on("click.accordion", function() {
                var $panel = $(this).next(".accordion-panel");
                $panel.stop(true, true).slideToggle(settings.speed, settings.easing, settings.onComplete);
            });
        });
    };

}(jQuery));

// Uso
$(function() {
    $("#faq").accordion({ speed: 300 });
});
```

### Parseo seguro de HTML

```javascript
// Parsear una cadena HTML SIN ejecutar scripts embebidos
var nodes = $.parseHTML("<div>Hello</div><script>alert('xss')<\/script>", document, false);
$("#container").empty().append(nodes);
```

## Referencias

- [jQuery API Documentation](https://api.jquery.com/)
- [jQuery Learning Center](https://learn.jquery.com/)
- [jQuery 2.1.3 Release Blog Post](https://blog.jquery.com/2014/12/18/jquery-1-11-2-and-2-1-3-released-safari-fail-safe-edition/)
- [jQuery Browser Support](https://jquery.com/browser-support/)
