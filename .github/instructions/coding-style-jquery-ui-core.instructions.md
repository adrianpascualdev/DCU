---
description: 'Directrices generales de estilo para escribir código jQuery UI correcto, idiomático, accesible y tematizable en archivos JavaScript y HTML.'
applyTo: '**/scripts/**/*.js, **/Scripts/**/*.js, **/*.html, **/*.htm, **/*.aspx, **/*.cshtml'
---

# Directrices de desarrollo para jQuery UI

Buenas prácticas generales para escribir código jQuery UI correcto, idiomático, accesible y facilmente tematizable. Estas directrices aplican independientemente de la versión concreta de jQuery UI en uso y cubren el ciclo de vida de widgets, accesibilidad, seguridad, rendimiento y principios de tematización.

> **Guía por versión**: La skill `jquery-ui` aporta restricciones específicas para jQuery UI 1.11.3 (URLs de CDN, funcionalidades no disponibles, riesgos por versión y referencias de API detalladas para el Widget Factory, widgets incorporados, interacciones y efectos). Cargar siempre esa skill junto con este archivo cuando se trabaje en el proyecto.

## Instrucciones generales

- Incluir siempre los tres archivos requeridos en este orden — CSS del tema, jQuery core y, a continuación, jQuery UI.
- Encapsular todo el código jQuery UI dentro de `$(document).ready()` o el atajo `$(function() { ... })` para asegurar que el DOM esté listo antes de inicializar widgets.
- Envolver cada módulo o plugin independiente en una IIFE que reciba `jQuery`:

```javascript
(function($) {
    // Uso seguro de $ como jQuery aquí.
    $("#datepicker").datepicker();
}(jQuery));
```

- Seguir el patrón de ciclo de vida de widgets: **inicializar → interactuar → destruir**.
- Usar el Download Builder en `https://jqueryui.com/download/` para incluir solo los componentes que el proyecto necesita y así reducir el tamaño de archivo.
- No mezclar versiones de jQuery UI en la misma página — usar una sola versión de `jquery-ui.js` y su CSS de tema correspondiente.

## Ciclo de vida del widget

Todos los widgets de jQuery UI comparten una API consistente provista por el Widget Factory.

### Inicializar

Invocar el plugin del widget sobre uno o varios elementos con un hash de opciones opcional:

```javascript
// Opciones predeterminadas
$("#accordion").accordion();

// Opciones personalizadas
$("#dialog").dialog({
    modal: true,
    width: 500,
    title: "Confirmar acción"
});
```

### Invocar métodos

Pasar el nombre del método como argumento string a la misma función del plugin:

```javascript
// Llamar a close() en un diálogo abierto
$("#dialog").dialog("close");

// Call value() on a progressbar with a parameter
$("#progressbar").progressbar("value", 75);
```

Las llamadas a métodos que devuelven información (por ejemplo, `isOpen()`, `value()` como getter) rompen la cadena jQuery y devuelven el valor directamente. Todas las demás llamadas devuelven el objeto jQuery para encadenamiento:

```javascript
// Encadenamiento — funciona porque close() devuelve jQuery
$("#dialog").dialog("close").css("color", "red");

// Getter — devuelve un booleano, no jQuery
var isOpen = $("#dialog").dialog("isOpen");
```

### Métodos comunes en todos los widgets

| Método | Descripción |
| :--- | :--- |
| `option(name)` | Obtener el valor actual de una opción |
| `option(name, value)` | Establecer una opción después de la inicialización |
| `option(hash)` | Establecer múltiples opciones a la vez |
| `enable()` | Habilitar el widget |
| `disable()` | Deshabilitar el widget |
| `destroy()` | Eliminar la funcionalidad del widget y devolver el elemento a su estado previo a la inicialización |
| `widget()` | Devolver el objeto jQuery del wrapper generado (o el elemento original) |
| `instance()` | Devolver el objeto instancia del widget, o `undefined` si no está inicializado (añadido en 1.11) |

### Obtener y establecer opciones

```javascript
// Obtener una opción
var currentValue = $("#slider").slider("option", "value");

// Establecer una opción
$("#slider").slider("option", "value", 50);

// Establecer múltiples opciones a la vez
$("#slider").slider("option", { min: 0, max: 200, value: 100 });
```

### Acceder a la instancia del widget

Usar `instance()` (nuevo en 1.11) o `jQuery.data()` para recuperar la instancia y llamar métodos directamente:

```javascript
// Con instance() — devuelve undefined si no está inicializado
var dialogInstance = $("#dialog").dialog("instance");

// Con jQuery.data() — usar el nombre completo del widget como clave
var dialogInstance = $("#dialog").data("ui-dialog");

// Llamar a métodos directamente en la instancia
dialogInstance.close();
```

### Eventos y callbacks

Cada widget expone eventos que se disparan cuando el estado cambia. Vincularlos como opciones durante la inicialización o como eventos DOM:

```javascript
// Como callback de opción
$("#accordion").accordion({
    activate: function(event, ui) {
        console.log("New panel:", ui.newHeader.text());
    }
});

// Como evento DOM (nombre de evento = nombrewidget + nombreevento, en minúsculas)
$("#accordion").on("accordionactivate", function(event, ui) {
    console.log("New panel:", ui.newHeader.text());
});
```

Todos los widgets disparan un evento `create` al inicializarse.

### Destruir un widget

Destruir siempre los widgets antes de eliminar sus elementos del DOM para evitar fugas de memoria. El Widget Factory realiza destroy automático cuando un elemento se elimina con métodos jQuery (`.remove()`, `.html()`, `.empty()`):

```javascript
// Destrucción manual
$("#dialog").dialog("destroy");
```

## Rendimiento

- Usar el **Download Builder** para incluir solo los componentes necesarios — la librería completa de jQuery UI es mucho mayor de lo que la mayoría de proyectos requiere.
- Cachear las instancias de widgets — recuperar vía `.data("ui-widgetname")` o `instance()` y reutilizar.
- Al inicializar `.draggable()` en cientos de elementos, establecer `addClasses: false` para evitar añadir la clase `ui-draggable`.
- Desacoplar (detach) antes de manipulaciones pesadas — llamar `.detach()`, modificar el subárbol y luego reinsertar para reducir reflows.
- Preferir clases CSS sobre `.css()` para cambios de estilo en bloque.
- Evitar inicializar widgets en elementos ocultos cuando sea posible — widgets como Dialog y Datepicker calculan dimensiones en la inicialización.
- Usar `refreshPositions: true` en Draggable **solo** cuando los objetivos de drop se muevan con frecuencia — recalcula en cada mousemove.
- Para Sortable, estrechar el selector `items` para que coincida solo con los hijos relevantes en lugar de `"> *"`.

## Accesibilidad

Los widgets jQuery UI implementan roles WAI-ARIA, estados y navegación por teclado por defecto:

- No eliminar ni sobrescribir atributos ARIA añadidos por los widgets (`role`, `aria-expanded`, `aria-selected`, `aria-disabled`, `tabindex`).
- Los diálogos modales atrapan el foco del teclado — usar el punto de extensión `_allowInteraction()` para incluir elementos externos en la whitelist (por ejemplo, dropdowns de terceros):

```javascript
$.widget("ui.dialog", $.ui.dialog, {
    _allowInteraction: function(event) {
        return !!$(event.target).closest(".third-party-dropdown").length ||
            this._super(event);
    }
});
```

- Usar los pseudo-selectores `:focusable` y `:tabbable` (proporcionados por jQuery UI Core) para localizar elementos enfocables.
- Accordion, Tabs, Menu y Dialog soportan navegación completa por teclado de serie.
- Usar `$.fx.off = true` para desactivar animaciones globalmente para usuarios que prefieran reducción de movimiento.

## Seguridad

- Nunca pasar entrada de usuario sin sanear a opciones de widgets que rendericen HTML — en particular:
  - Opción `title` de Dialog — usar `.text()` en el elemento del título en su lugar.
  - Opción `content` de Tooltip cuando se use una función que devuelva HTML.
  - Renderizado personalizado de Autocomplete vía `_renderItem`.
- Usar `.text()` en lugar de `.html()` en métodos de renderizado personalizados.
- Sanear datos retornados desde fuentes remotas antes de pasarlos al callback `source` de Autocomplete.
- Evitar pasar HTML no confiable a `$()` — usar `$.parseHTML(html, document, false)` con el tercer argumento en `false`.
- Al construir widgets personalizados, validar todos los valores de opciones en `_setOption()` antes de aplicarlos al DOM.
- No usar `eval()` ni `$.globalEval()` para procesar configuraciones de widgets.

## Principios de tematización

- No codificar estilos de widgets directamente — usar siempre las clases del framework CSS de jQuery UI (`ui-widget`, `ui-widget-content`, `ui-state-*`, `ui-corner-*`).
- Usar ThemeRoller para generar temas coherentes; no editar manualmente archivos `jquery-ui.css` generados.
- Sobrescribir estilos específicos en una hoja de estilos separada que cargue **después** del CSS del tema.
- Al crear widgets personalizados, aplicar las clases del framework apropiadas para mantener la coherencia visual.
- Aplicar `ui-state-hover` y `ui-state-focus` mediante los helpers `_hoverable()` y `_focusable()` del Widget Factory en lugar de enlazar eventos manualmente.
