---
description: 'Convenciones para diseñar y organizar factorías C# estáticas o de instancia en carpetas propias.'
applyTo: '**/Factories/**/*.cs, **/*Factory.cs'
---

# Convenciones para factorías C#

Estas reglas se aplican a todas las factorías C# del repositorio. Su objetivo es mantener una construcción de objetos explícita, testeable y coherente con la arquitectura por capas.

## Organización obligatoria

- Ubica cada factoría dentro de una carpeta `Factories` propia del proyecto o del área funcional correspondiente.
- Mantén una factoría por archivo `.cs`.
- Haz coincidir el nombre del archivo con el nombre de la clase, por ejemplo `PostChatMessageCommandFactory.cs`.
- No mezcles la factoría con el tipo que construye ni con servicios, endpoints, agentes o tests.
- Mantén la factoría junto a la característica que posee la construcción, respetando los límites de las capas.

## Decisión entre factoría estática y de instancia

### Factoría estática

Usa una clase `static` y métodos `static` cuando la factoría:

- no mantiene estado entre llamadas;
- no recibe dependencias por constructor;
- solo transforma parámetros en una nueva instancia;
- no necesita configuración mutable ni servicios externos.

```csharp
namespace TemplateManager.Application.Contracts.ChatSessions.Factories
{
    /// <summary>
    /// Crea comandos para publicar mensajes de chat.
    /// </summary>
    public static class PostChatMessageCommandFactory
    {
        /// <summary>
        /// Crea un comando de publicación con los datos recibidos.
        /// </summary>
        /// <param name="sessionId">Identificador de la sesión.</param>
        /// <param name="identity">Identidad de la persona usuaria.</param>
        /// <param name="text">Texto del mensaje.</param>
        /// <param name="generate">Indica si se solicita generar una plantilla.</param>
        /// <param name="document">Documento adjunto, si existe.</param>
        /// <param name="requestedAtUtc">Fecha y hora UTC de la solicitud.</param>
        /// <returns>Comando listo para ejecutar.</returns>
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
}
```

### Factoría de instancia

Usa una clase sellada de instancia únicamente cuando la construcción necesite dependencias o estado legítimo, por ejemplo:

- `IClock`, `IOptions<TSettings>` u otro servicio inyectado;
- validadores, repositorios o proveedores externos;
- configuración que no debe recibirse como parámetro en cada llamada;
- estado interno necesario para toda la vida de la factoría.

En ese caso:

- inyecta las dependencias por constructor;
- valida las dependencias con guard clauses;
- usa métodos `Create` o `Create{TypeName}`;
- no conviertas una factoría de instancia en singleton si sus dependencias no son singleton.

## Convenciones de nombres

- Usa el sufijo `Factory` en la clase y el archivo.
- Usa `Create` cuando la factoría construya un único tipo principal.
- Usa `Create{TypeName}` cuando la factoría exponga varias operaciones de creación.
- Mantén los identificadores en inglés.
- Escribe la documentación XML en castellano.

## Contratos y validación

- Conserva el contrato público del tipo construido; la factoría no debe ocultar cambios de comportamiento.
- Valida argumentos obligatorios antes de construir el objeto.
- No introduzcas factorías para envolver una sola expresión si no aportan una responsabilidad de construcción reutilizable.
- No uses factorías como sustituto de inyección de dependencias ni como contenedor de lógica de negocio.
- Documenta con `///` todos los tipos y miembros públicos, incluyendo cada `<param>` y `<returns>` cuando corresponda.
- Ejecuta el build del proyecto afectado y las pruebas de la característica después de modificar una factoría.

## Antipatrones

| Antipatrón | Corrección |
| --- | --- |
| Clase `Factory` de instancia sin estado ni dependencias | Convertirla en `static` |
| Método `Create` de instancia en una factoría sin estado | Convertirlo en `static` |
| Factoría en la misma carpeta que un endpoint o servicio sin motivo | Moverla a `Factories` |
| Varias factorías o tipos públicos en un archivo | Separarlos, un tipo por archivo |
| Constructor que recibe servicios no utilizados | Eliminar dependencias innecesarias |
| Factoría que contiene reglas de negocio | Delegar las reglas en Application o Domain |

## Validación mínima

```powershell
dotnet build .\src\TemplateManager.Application\TemplateManager.Application.csproj
dotnet test .\tests\TemplateManager.Application.UnitTests\TemplateManager.Application.UnitTests.csproj --no-restore
```

Adapta las rutas al proyecto afectado y usa una salida de compilación aislada cuando exista riesgo de bloqueo del `testhost`.
