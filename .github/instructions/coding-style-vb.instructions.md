---
applyTo: "**/*.vb,**/*.vbproj"
description: "Convenciones de estilo de codificación para proyectos Visual Basic .NET en .NET y .NET Framework. Estas instrucciones son siempre válidas también para .NET Framework; cuando el objetivo sea net472, cargar además la skill `netfx-vb` para las restricciones específicas del framework."
---

# Directrices de estilo de codificación para Visual Basic .NET

Directrices para escribir código Visual Basic .NET limpio, mantenible y consistente siguiendo las convenciones modernas de .NET.

> Estas instrucciones son válidas tanto para proyectos .NET modernos como para proyectos .NET Framework. Si el proyecto tiene como objetivo .NET Framework 4.7.2 (`net472`), cargar y aplicar también la skill `netfx-vb`. La skill es aditiva y proporciona únicamente las restricciones específicas del framework y la orientación sobre APIs que difieren de .NET moderno.

## Contexto del proyecto

- **Frameworks de destino**: .NET y .NET Framework con Visual Basic .NET
- **Referencia de estilo**: [Microsoft Visual Basic Coding Conventions](https://learn.microsoft.com/en-us/dotnet/visual-basic/programming-guide/program-structure/coding-conventions)
- **Formato**: El archivo `.editorconfig` **SIEMPRE** tiene precedencia cuando está presente

## Principios generales

- **Claridad sobre brevedad**: Preferir código legible sobre atajos ingeniosos.
- **Consistencia**: Usar formato y nomenclatura uniformes en todo el código.
- **Documentación**: Escribir comentarios claros y concisos para todas las APIs públicas.
- **Características modernas**: Aprovechar las características modernas de VB.NET solo cuando el usuario lo solicite explícitamente.
- **Directivas Option**: Declarar siempre `Option Strict On` y `Option Explicit On` al inicio de cada archivo, salvo que la configuración a nivel de proyecto ya las aplique.

## Convenciones de nomenclatura

- Usar `PascalCase` para nombres de clases, módulos, estructuras, métodos, propiedades y eventos.
- Usar `camelCase` para variables locales y parámetros de métodos.
- Usar `ALL_CAPS` para constantes.
- Prefijar los nombres de interfaces con `I` (p. ej., `IOrderService`).
- **Nunca** usar `My` o `my` como parte del nombre de una variable — genera confusión con el objeto integrado `My`.
- Los nombres deben comenzar con un carácter alfabético. Evitar guiones bajos al inicio; no son conformes con CLS.
- Comenzar los nombres de funciones y métodos con un verbo (p. ej., `InitNameArray`, `CloseDialog`).
- Comenzar los nombres de clases, estructuras, módulos y propiedades con un sustantivo (p. ej., `EmployeeName`, `CarAccessory`).
- Prefijar los nombres de interfaces con `I` seguido de un sustantivo o adjetivo (p. ej., `IComponent`, `IPersistable`).
- Nombrar los tipos de delegados de eventos con el sufijo `EventHandler` (p. ej., `MouseEventHandler`).
- Nombrar las clases de argumentos de eventos con el sufijo `EventArgs` (p. ej., `SampleEventArgs`).
- Para eventos con concepto de antes/después, usar tiempo presente o pasado (p. ej., `ControlAdd`, `ControlAdded`).
- Usar nombres significativos y descriptivos; evitar abreviaturas salvo que sean ampliamente reconocidas y coherentes en todo el proyecto (p. ej., `HTML` es aceptable).
- Evitar nombres en ámbitos internos que oculten nombres de ámbitos externos.
- Usar las convenciones de nomenclatura establecidas en el archivo `.editorconfig` cuando esté presente; tienen precedencia sobre estas directrices.

### Plantillas de nomenclatura

Usar las siguientes plantillas estructurales al construir nombres:

| Construcción | Plantilla |
| :--- | :--- |
| Namespace | `CompanyName.ProjectName.PackageName.SubsystemName` |
| Clase / Estructura | Sustantivo + Calificador(es) |
| Interfaz | `I` + Sustantivo o Adjetivo |
| Constante / `Shared ReadOnly` | Adjetivo(s) + Sustantivo + Calificador(es) |
| Enumeración | Adjetivo(s) + Sustantivo + Calificador(es) |
| Propiedad | Adjetivo(s) + Sustantivo + Calificador(es) |
| Variable / Campo / Parámetro | Adjetivo(s) + Sustantivo + Calificador(es) |
| Método / Función / Sub | Verbo + Adjetivo(s) + Sustantivo + Calificador(es) |
| Tipo de delegado de evento | Sustantivo + `EventHandler` |
| Método manejador de evento | `ObjectName_EventName` |

## Formato

- Usar 4 espacios para la indentación (no tabulaciones).
- Usar solo una sentencia por línea. **No** usar el carácter separador de línea de Visual Basic (`:`).
- Usar solo una declaración por línea.
- Evitar el carácter explícito de continuación de línea (`_`) en favor de la continuación implícita siempre que el lenguaje lo permita.
- Cuando se requiera indentación manual de líneas de continuación, indentar un nivel de tabulación y alinear a la izquierda los elementos de la lista.
- Añadir al menos una línea en blanco entre definiciones de métodos y propiedades.
- Agrupar las directivas `Imports` al inicio del archivo, antes de la declaración `Namespace`. Colocar primero los espacios de nombres `System`, luego el resto en orden alfabético.
- **Siempre** aplicar el estilo de formato de código definido en `.editorconfig`; esas reglas tienen precedencia sobre estas directrices.

### Ejemplo: directivas option y orden de imports

```vb
Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Text

Imports MyCompany.Core
Imports MyCompany.Domain
```

### Indicar el ámbito

Calificar completamente los nombres de tipos y miembros solo para resolver ambigüedades o enfatizar que una referencia pertenece a un espacio de nombres externo o desconocido.

- **No** usar la palabra clave `Me` antes de accesos a miembros salvo que sea necesario para resolver ambigüedad por sombra.
- Usar sentencias `Imports` para evitar repetir prefijos de espacios de nombres conocidos en todo el archivo.

## Convenciones de comentarios

- Colocar los comentarios en una línea separada en lugar de al final de una línea de código.
- Comenzar el texto del comentario con mayúscula y terminar con punto.
- Insertar un espacio entre el delimitador de comentario (`'`) y el texto del comentario.

```vb
' Este es un comentario con formato correcto.
```

- No rodear los comentarios con bloques de asteriscos formateados.
- Usar comentarios de documentación XML (`'''`) para todos los miembros y tipos públicos. Incluir documentación `<example>` y `<code>` cuando sea aplicable.
- Escribir comentarios para explicar el *por qué*, no el *qué*, cuando sea necesario.
- Todos los comentarios y la documentación deben estar siempre en castellano. Los términos de dominio específicos pueden mantenerse en su idioma original si son ampliamente reconocidos, y deben escribirse entre backticks.

### Ejemplo: comentario de documentación XML

```vb
''' <summary>
''' Calcula el precio total para el pedido indicado.
''' </summary>
''' <param name="order">El pedido sobre el que se calcula el total.</param>
''' <returns>El precio total como <see cref="Decimal"/>.</returns>
''' <example>
''' <code>
''' Dim total = CalculateTotalPrice(myOrder)
''' </code>
''' </example>
Public Function CalculateTotalPrice(ByVal order As Order) As Decimal
    ' ...
End Function
```

## Estructura del código

- Usar un tipo (clase, interfaz, enumeración, estructura, módulo) por archivo cuando sea posible.
- Organizar los archivos por funcionalidad o dominio.
- Usar `Namespace` para organizar el código en grupos lógicos.
- Usar clases `Partial` solo cuando sea necesario (p. ej., para generación de código por diseñadores).
- Evitar bloques de código profundamente anidados; preferir retornos anticipados y cláusulas de guarda.
- Asignar los valores predeterminados de campos en la línea de declaración en lugar de en el constructor. Esto mantiene los valores predeterminados en un solo lugar, lo cual es especialmente importante cuando una clase tiene múltiples constructores.
- Nombrar las clases y estructuras usando la plantilla **Sustantivo + Calificador(es)**. Cuando aplique, elegir calificadores que reflejen la derivación de una clase base (p. ej., `CustomerForm Inherits Form`, `CustomerCollection Inherits CollectionBase`).

### Orden de los miembros

Se recomienda organizar los miembros dentro de un tipo en el siguiente orden:

1. Campos
2. Constructores
3. Propiedades
4. Métodos
5. Eventos

## Declaración de variables

- Usar `As <Type>` al declarar variables para habilitar la comprobación de tipos con `Option Strict On`.
- Declarar las variables cerca de su punto de uso.
- Usar `Dim` para declaraciones de variables locales con un tipo explícito o un inicializador claro.

```vb
' BIEN
Dim count As Integer = 0
Dim message As String = "Hello"
Dim employees As New List(Of String)

' MAL - dependiendo del tipado implícito sin Option Infer
Dim x = 42
```

Directrices adicionales:

- Declarar cada variable y campo en una línea separada.
- Asignar valores iniciales siempre que sea posible; no depender de la inicialización por defecto a cero o `Nothing` del runtime.
- Evitar nombres sin significado como `i`, `j`, `k` y `temp`. Describir lo que representa la variable (p. ej., usar `index` en lugar de `i`, `swapValue` en lugar de `tempValue`).
- Usar una connotación positiva para los nombres de variables booleanas (p. ej., `isOpen` en lugar de `notOpen`).

## Directrices del lenguaje

### Constantes

Nombrar las constantes usando PascalCase y la plantilla **Adjetivo(s) + Sustantivo + Calificador(es)**.

```vb
Public Const DefaultRetryCount As Integer = 3
Public Shared ReadOnly DefaultDatabaseName As String = "Membership"
```

- Preferir `Shared ReadOnly` sobre `Const` para constantes públicas. Los valores declarados con `Const` se insertan en el momento de la compilación; los valores `Shared ReadOnly` se resuelven en tiempo de ejecución, lo que es más seguro y menos propenso a problemas cuando se accede desde un ensamblado diferente.

### Enumeraciones

Nombrar las enumeraciones usando PascalCase y la plantilla **Adjetivo(s) + Sustantivo + Calificador(es)**. Los nombres de los miembros de la enumeración también usan PascalCase.

```vb
' Enumera las formas en que un cliente puede realizar una compra.
<Flags()>
Public Enum PurchaseMethod
    None = 0
    Cash = 1
    Check = 2
    CreditCard = 4
    DebitCard = 8
    Voucher = 16
End Enum
```

- Usar el atributo `<Flags()>` solo para indicar que la enumeración puede tratarse como un campo de bits (un conjunto de indicadores).

### Tipo de datos String

- Usar interpolación de cadenas para concatenar cadenas cortas.

```vb
MsgBox($"Hola {firstName}, bienvenido a {applicationName}.")
```

- Usar `StringBuilder` para concatenar cadenas dentro de bucles.

```vb
Dim longString As New System.Text.StringBuilder()
For count As Integer = 1 To 1000
    longString.Append(count)
Next
```

- Usar `String.Empty` en lugar de `""` para cadenas vacías.

### Arrays

- Usar la sintaxis de inicialización abreviada al declarar e inicializar arrays en la misma línea.

```vb
' BIEN
Dim letters As String() = {"a", "b", "c"}

' MAL
Dim letters2() As String = New String() {"a", "b", "c"}
```

- Colocar el designador de array en el tipo, no en el nombre de la variable.

```vb
' BIEN
Dim letters As String() = {"a", "b", "c"}

' MAL
Dim letters() As String = {"a", "b", "c"}
```

### Creación de objetos

- Usar la forma de instanciación abreviada con `As New`.

```vb
' BIEN
Dim employees As New List(Of String)

' MAL
Dim employees As List(Of String) = New List(Of String)
```

- Usar inicializadores de objetos en lugar de un constructor sin parámetros seguido de asignaciones de propiedades.

```vb
Dim orderLog As New EventLog With {
    .Log = "Application",
    .Source = "Application Name",
    .MachineName = "Computer Name"
}
```

### La palabra clave `With`

Usar `With` cuando se realice una serie de llamadas o asignaciones sobre el mismo objeto para mejorar la legibilidad.

```vb
With orderLog
    .Log = "Application"
    .Source = "Application Name"
    .MachineName = "Computer Name"
End With
```

### Comprobaciones de nulo y Nothing

- Usar `Is Nothing` e `IsNot Nothing` para comprobaciones de nulo/nothing. Nunca usar `= Nothing` ni `<> Nothing`.
- Preferir `IsNot Nothing` sobre `Not ... Is Nothing`.

```vb
' BIEN
If customer IsNot Nothing Then
    ' ...
End If

' MAL
If Not customer Is Nothing Then
    ' ...
End If
```

### Manejo de excepciones

- Usar `Try...Catch...Finally` para todo el manejo de excepciones.
- **Nunca** usar `On Error GoTo` — es una construcción heredada que debe evitarse en todo código nuevo.
- Usar cláusulas de guarda para la validación de argumentos al inicio de los procedimientos.
- Usar `NameOf` al pasar nombres de parámetros a las excepciones.

```vb
' GOOD
If order Is Nothing Then
    Throw New ArgumentNullException(NameOf(order))
End If

Try
    ProcessOrder(order)
Catch ex As InvalidOperationException
    ' Handle specific exception.
Finally
    CleanUp()
End Try

' BAD
On Error GoTo ErrorHandler
```

- Usar la sentencia `Using` para garantizar la disposición determinista de los recursos.

```vb
Using connection As New SqlConnection(connectionString)
    connection.Open()
    ' ...
End Using
```

Directrices adicionales:

- Usar excepciones solo para condiciones excepcionales, no para el flujo rutinario del programa. Las excepciones lanzadas conllevan una sobrecarga de rendimiento significativa.
- Pasar un mensaje descriptivo y gramaticalmente correcto al constructor de la excepción. Cada oración debe terminar con punto.
- Documentar en el comentario XML del método o propiedad qué excepciones puede lanzar, especificando las condiciones que las causan.
- Elegir el tipo de excepción más específico disponible:
  - `ArgumentException` / `ArgumentNullException` / `ArgumentOutOfRangeException` — parámetros de entrada no válidos.
  - `InvalidOperationException` — una llamada a método no válida para el estado actual del objeto.
  - `NotSupportedException` — una operación no admitida por la clase.
  - `NotImplementedException` — un método stub que aún no ha sido implementado.
- Derivar las clases de excepción personalizadas de `Exception` (aplicaciones web) o `ApplicationException` (aplicaciones Windows) para escenarios de error específicos del dominio.

```vb
Public Class DeletedByAnotherUserException
    Inherits Exception

    Public Sub New(ByVal message As String)
        MyBase.New(message)
    End Sub
End Class
```

### Tipo de datos sin signo

- Usar `Integer` en lugar de tipos sin signo (p. ej., `UInteger`, `ULong`) salvo que se requiera aritmética sin signo explícitamente.

### ByVal y ByRef

- Pasar parámetros `ByVal` por defecto.
- Usar `ByRef` solo cuando el método necesite modificar explícitamente la variable del llamador, y siempre documentar el motivo en el comentario XML del método.

```vb
' BIEN - ByVal es la intención predeterminada
Private Sub ProcessOrder(ByVal order As Order)
    ' ...
End Sub

' ACEPTABLE - ByRef con intención clara y documentación
''' <param name="result">Se rellena con el valor parseado si la operación tiene éxito.</param>
Private Function TryParse(ByVal input As String, ByRef result As Integer) As Boolean
    ' ...
End Function
```

### Propiedades

Nombrar las propiedades usando PascalCase y la plantilla **Adjetivo(s) + Sustantivo + Calificador(es)** (p. ej., `BackColor`, `TotalPrice`).

Usar los siguientes prefijos comunes para propiedades de inspección (propiedades que devuelven información de consulta sobre un objeto):

| Prefijo | Ejemplo |
| :--- | :--- |
| `Allow…` / `Allows…` | `AllowEdit` |
| `Can…` | `CanDelete` |
| `Contains…` | `ContainsKey` |
| `Has…` | `HasChildren` |
| `Is…` | `IsVisible` |
| `Use…` / `Uses…` | `UsesCache` |

Cuando un setter de propiedad actualiza un campo de respaldo que también es gestionado por el setter de otra propiedad:

- Establecer el campo de respaldo directamente en lugar de llamar al setter de la otra propiedad.
- El setter de origen es responsable de lanzar los eventos que normalmente lanzaría el otro setter.
- Si el valor a establecer no corresponde a un campo de respaldo privado, llamar al setter de propiedad o método adecuado para establecerlo.

### Métodos

Nombrar los métodos usando PascalCase y la plantilla **Verbo + Adjetivo(s) + Sustantivo + Calificador(es)** (p. ej., `FindRedCansByPrice`, `CalculateDiscountedAmount`).

- Mantener los métodos pequeños. Cuando el cuerpo de un método se aproxime a 20-30 líneas, buscar bloques que puedan extraerse en métodos auxiliares separados.
- Si el mismo bloque de código aparece más de una vez, extraerlo en un método reutilizable.
- Los métodos relacionados que comparten listas de parámetros similares deben listar sus parámetros en el mismo orden.
- Las funciones deben usar siempre la palabra clave `Return` para devolver un valor.

```vb
Private Function CalculateDiscountedAmount(ByVal amount As Decimal,
                                           ByVal purchaseMethod As PurchaseMethod) As Decimal
    ' Calcular el descuento según el método de pago.
    Dim discount As Decimal = 0D

    Select Case purchaseMethod
        Case PurchaseMethod.Cash
            discount = CalculateCashDiscount(amount)
        Case PurchaseMethod.CreditCard
            discount = CalculateCreditCardDiscount(amount)
    End Select

    ' Asegurar que el importe descontado nunca sea negativo.
    Dim discountedAmount As Decimal = amount - discount
    If discountedAmount < 0D Then
        discountedAmount = 0D
    End If

    Return discountedAmount
End Function
```

### Manejo de eventos

Nombrar los métodos manejadores de eventos con el formato `ObjectName_EventName` (p. ej., `HelpButton_Click`, `Form1_Load`).

- Preferir `Handles` sobre `AddHandler` para suscripciones a eventos conocidos estáticamente.

```vb
Private Sub ToolStripMenuItem1_Click() Handles ToolStripMenuItem1.Click
    ' ...
End Sub
```

- Usar `AddressOf` sin instanciar explícitamente el delegado.

```vb
Dim closeItem As New ToolStripMenuItem("Cerrar", Nothing, AddressOf ToolStripMenuItem1_Click)
```

- Usar la sintaxis abreviada de declaración de eventos y dejar que el compilador defina el delegado.

```vb
Public Event SampleEvent As EventHandler(Of SampleEventArgs)
```

- No comprobar si un evento es `Nothing` antes de llamar a `RaiseEvent`; el runtime lo gestiona internamente.

### Delegados relajados en manejadores de eventos

Cuando los argumentos del evento no se utilizan, omitirlos usando delegados relajados.

```vb
' BIEN - delegado relajado, argumentos omitidos
Public Sub Form1_Load() Handles Form1.Load
End Sub

' INNECESARIO - argumentos presentes pero no usados
Public Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Form1.Load
End Sub
```

### Miembros Shared

Llamar a los miembros `Shared` (estáticos) a través del nombre de la clase, no a través de una variable de instancia.

```vb
' BIEN
Dim now = DateTime.Now

' MAL
Dim instance As New DateTime()
Dim now = instance.Now
```

### Consultas LINQ

- Usar nombres significativos para las variables de consulta y de rango.
- Usar `PascalCase` para las propiedades de tipos anónimos proyectados desde consultas.
- Renombrar las propiedades ambiguas en los resultados de consultas en lugar de dejar nombres genéricos.
- Usar inferencia de tipos para las declaraciones de variables de consulta y de rango.
- Alinear las cláusulas de consulta bajo la sentencia `From`.
- Colocar las cláusulas `Where` antes que el resto de cláusulas para que las siguientes operen sobre el conjunto filtrado.
- Usar la cláusula `Join` explícitamente en lugar de la cláusula `Where` para definir operaciones de unión.

```vb
Dim customerOrders = From customer In customers
                     Join order In orders
                       On customer.CustomerID Equals order.CustomerID
                     Where order.Amount > 100
                     Select CustomerName = customer.Name,
                            OrderID = order.ID
```

### Literales XML

- Usar literales XML para crear documentos y fragmentos XML en lugar de llamar directamente a las APIs de XML.
- Importar los espacios de nombres XML a nivel de archivo o de proyecto para mejorar el rendimiento.
- Usar las propiedades de eje XML para acceder a elementos y atributos.
- Usar expresiones embebidas (`<%= %>`) para enlazar valores en lugar de llamadas a la API `Add`.

## Características modernas de VB.NET

Dado que la organización puede usar tanto .NET como .NET Framework, aprovechar las características más recientes del lenguaje solo cuando el usuario lo solicite explícitamente. Si el proyecto tiene como objetivo .NET Framework 4.7.2, la skill `netfx-vb` define el techo de lenguaje compatible con el framework y las excepciones específicas del framework a las directrices que siguen. Al usar características modernas, seguir estas directrices:

- Preferir `Async`/`Await` para operaciones públicas, enlazadas a I/O o potencialmente de larga duración.
- Incluir un parámetro `CancellationToken` cuando la cancelación pueda propagarse de manera significativa, especialmente en operaciones públicas, enlazadas a I/O o de larga duración.
- Agregar el sufijo `Async` a todos los métodos `Async` (p. ej., `GetOrderAsync`, `SaveChangesAsync`).
- Nunca bloquear en código asíncrono llamando a `.Result` o `.Wait()` — usar siempre `Await` en la tarea.
- Inyectar dependencias a través del contenedor de Inyección de Dependencias (DI) integrado.
- Usar capacidades de rastreo y registro mediante `ILogger(Of T)`.
- Preferir LINQ sobre bucles manuales al expresar transformaciones de datos.
- Usar inicializadores de colección e inicializadores de objetos donde mejoren la legibilidad.

## Archivos de configuración

Para las aplicaciones .NET modernas que usan el host genérico o la canalización de configuración de ASP.NET Core, gestionar los archivos de configuración para diferentes entornos de la siguiente manera. Si el proyecto tiene como objetivo .NET Framework 4.7.2, usar `App.config` o `Web.config` en su lugar, tal como se define en la skill `netfx-vb`:

```vb
If Debugger.IsAttached Then
    builder.Configuration.AddJsonFile("appsettings.debug.json", optional:=True, reloadOnChange:=True)
End If

builder.Configuration _
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional:=True, reloadOnChange:=True) _
    .AddJsonFile($"appsettings.{Environment.UserName}.json", optional:=True, reloadOnChange:=True) _
    .AddEnvironmentVariables()
```

## Clases selladas

Marcar las clases como `NotInheritable` por defecto. Solo permitir la herencia explícitamente declarando la clase sin modificadores restrictivos (o usando `MustInherit` para clases base abstractas).

## Referencias

- [Visual Basic Coding Conventions](https://learn.microsoft.com/en-us/dotnet/visual-basic/programming-guide/program-structure/coding-conventions)
- [Visual Basic Naming Conventions](https://learn.microsoft.com/en-us/dotnet/visual-basic/programming-guide/program-structure/naming-conventions)
- [Program Structure and Code Conventions](https://learn.microsoft.com/en-us/dotnet/visual-basic/programming-guide/program-structure/program-structure-and-code-conventions)
