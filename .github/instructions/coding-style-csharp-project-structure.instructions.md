---
applyTo: "**/*.csproj"
description: "Directrices sobre la organización de carpetas dentro de cada proyecto de la solución, indicando qué carpetas son obligatorias, cuáles son condicionales según las capacidades del proyecto y su propósito."
---

# Directrices de organización de carpetas por proyecto

Instrucciones para organizar las carpetas dentro de cada proyecto de una solución .NET, describiendo el propósito de cada carpeta convencional y aclarando que su presencia depende de las capacidades reales de cada proyecto.

## Contexto del proyecto

- **Tipos de proyecto habituales**: API HTTP (ASP.NET Core), librería de contratos/modelos, librería de proxies/clientes hacia servicios externos, servicio de aplicación (background/consola/mensajería).
- **Combinar con instrucciones**:
  - [Directrices de métodos de extensión](/.github/instructions/coding-style-csharp-extension-methods.instructions.md) para la carpeta `Extensions/`.
  - [Directrices de archivos de recursos](/.github/instructions/coding-style-csharp-resource-files.instructions.md) para la carpeta `Resources/`.
  - [Directrices de clases de settings](/.github/instructions/coding-style-csharp-settings-classes.instructions.md) para la carpeta `Settings/`.
  - [Directrices de implementación de servicios](/.github/instructions/coding-style-csharp-service-implementation.instructions.md) para las carpetas `Services/` y `Abstractions/`.
  - [Directrices de Program.cs](/.github/instructions/coding-style-csharp-program.instructions.md) y [Directrices de AppExtensions.cs](/.github/instructions/coding-style-csharp-app-extensions.instructions.md) para proyectos de tipo API.

## Principio fundamental

**Ninguna carpeta de este catálogo es obligatoria en todos los proyectos.** Cada carpeta representa una capacidad concreta (autenticación, mensajería, feature flags, documentación de API, etc.) y solo debe crearse en el proyecto que efectivamente implemente esa capacidad. Por ejemplo, `Scalar/` o `Swagger/` solo tienen sentido en un proyecto API que expone documentación OpenAPI; un servicio de consola o una librería de proxies no debe crear ninguna de esas carpetas solo por convención. Para documentación OpenAPI en proyectos nuevos, preferir `Scalar/` (`Scalar.AspNetCore`) sobre `Swagger/` (`Swashbuckle`).

- **No crear carpetas vacías** ni "por si acaso". Una carpeta debe aparecer únicamente cuando exista al menos un archivo real que la justifique.
- **No replicar la estructura de un proyecto en otro** si el proyecto destino no tiene esa responsabilidad. Cada proyecto adopta solo el subconjunto de carpetas que sus capacidades requieren.
- Al añadir una nueva capacidad a un proyecto, crear la carpeta convencional correspondiente de esta guía en lugar de inventar una nueva estructura ad hoc, salvo que ninguna de las descritas encaje con el propósito.

## Catálogo de carpetas convencionales

### Comunes a varios tipos de proyecto

| Carpeta | Propósito | Aplica típicamente a |
| --- | --- | --- |
| `Extensions/` | Métodos de extensión agrupados por tipo extendido y dominio funcional. | Todos los proyectos con métodos de extensión. |
| `Settings/` | Clases de opciones fuertemente tipadas (Options Pattern). | Proyectos que leen configuración propia. |
| `Services/` | Implementaciones de servicios de aplicación o dominio. | Proyectos con lógica de servicio propia. |
| `Abstractions/` | Interfaces públicas de contratos de servicio o de infraestructura. | Proyectos con servicios inyectables. |
| `Exceptions/` (o `Exception/`) | Excepciones de dominio personalizadas. | Proyectos con reglas de negocio propias que fallan de forma controlada. |
| `Model/` | Modelos y DTOs del dominio. | Librerías de contratos/modelos. |
| `Resources/` | Archivos `.resx` de localización. | Proyectos con textos dirigidos al usuario final. |
| `Clients/` | Clientes HTTP/SDK hacia sistemas externos. | Proyectos que consumen APIs o servicios externos. |
| `Contracts/` | Contratos de mensajería (eventos/comandos) usados por consumidores o publicadores. | Proyectos con mensajería asíncrona. |
| `Consumers/` | Consumidores de mensajería (por ejemplo, MassTransit). | Proyectos con mensajería asíncrona. |
| `Proxy/` | Interfaces de los proxies consumidos por otros proyectos. | Librerías de contratos/modelos. |
| `Factories/` | Fábricas para construir instancias complejas del dominio. | Proyectos con construcción compleja de objetos de dominio. |
| `Helper/` | Utilidades puntuales sin cabida en otra carpeta temática. | Cualquier proyecto, con moderación. |
| `JsonConverters/` | Convertidores JSON personalizados (`System.Text.Json`/`Newtonsoft.Json`). | Proyectos con serialización específica. |
| `Security/` | Componentes de autenticación/autorización propios del dominio. | Proyectos con lógica de seguridad propia. |
| `Base/` | Clases base compartidas por implementaciones similares (por ejemplo, proxies). | Librerías de proxies/clientes. |

### Específicas de proyectos tipo API (ASP.NET Core)

| Carpeta | Propósito | Obligatoriedad |
| --- | --- | --- |
| `ApisRest/` | Definiciones de endpoints REST (minimal APIs o controladores agrupados por recurso). | Solo si el proyecto expone endpoints HTTP. |
| `Middlewares/` | Middlewares personalizados del pipeline (`AuthMiddleware`, `LogAccessMiddleware`, etc.). | Solo si el proyecto necesita interceptar la tubería HTTP más allá de los middlewares estándar. |
| `Scalar/` (preferida) o `Swagger/` | Filtros, convenciones y configuración de la documentación OpenAPI (por ejemplo, `DefaultHeaderValues`). | **Opcional**: solo si el proyecto publica documentación OpenAPI. Preferir `Scalar/` (`Scalar.AspNetCore`) para nuevos proyectos; usar `Swagger/` (`Swashbuckle`) solo en proyectos existentes que ya lo utilicen. Un proyecto API que no publique documentación OpenAPI no debe crear ninguna de las dos. |
| `FeatureFlags/` | Filtros de *feature management* (`Microsoft.FeatureManagement`). | **Opcional**: solo si el proyecto usa feature flags. |
| `HostedServices/` | Servicios en segundo plano (`IHostedService`/`BackgroundService`). | **Opcional**: solo si el proyecto ejecuta tareas en background. |

## Reglas de aplicación

1. **Justificar antes de crear**: antes de añadir una carpeta de este catálogo, confirmar que el proyecto va a alojar al menos un archivo con esa responsabilidad.
2. **No mezclar responsabilidades**: no colocar, por ejemplo, un middleware dentro de `Extensions/` ni una excepción de dominio dentro de `Helper/`.
3. **Consistencia de nombres**: usar el nombre exacto de la tabla (singular o plural según se indique) para mantener homogeneidad entre proyectos; no introducir variantes como `Exceptions/` en un proyecto y `Errors/` en otro para el mismo propósito.
4. **Carpetas no listadas**: si una nueva responsabilidad no encaja en ninguna carpeta de este catálogo, proponer un nombre descriptivo siguiendo el mismo criterio (sustantivo en inglés, PascalCase, alineado con el tipo de artefacto que contiene) y, si es recurrente, actualizar esta guía.
5. **Proyectos de prueba**: los proyectos de prueba siguen la misma jerarquía que el proyecto de producción que verifican (una carpeta espejo por proyecto/característica), no este catálogo de capacidades.

## Ejemplo de aplicación

El siguiente ejemplo usa nombres de proyecto genéricos únicamente para ilustrar el criterio; adapta los nombres a la solución concreta:

```text
src/Api/                           # Expone endpoints REST, consume mensajería y publica documentación OpenAPI
    ApisRest/                      # Definiciones de endpoints
    Consumers/                     # Consumidores de mensajería
    Middlewares/                   # Middlewares de autenticación y logging
    Scalar/                        # Configuración OpenAPI (proyecto SÍ expone documentación; preferido sobre Swagger)
    FeatureFlags/                  # Filtros de feature management
    HostedServices/                # Tareas en background
    Abstractions/                  # Contratos de endpoints (IEndpointDefinition)
    Extensions/                    # BuilderExtensions.cs, AppExtensions.cs, etc.

src/Proxy/                         # Solo expone proxies tipados hacia una API REST externa
    Base/                          # Clase base común de los proxies
    Extensions/                    # Métodos de extensión de registro de proxies
    # Sin Scalar/ ni Swagger/, sin Middlewares/, sin HostedServices/: el proyecto no expone endpoints ni pipeline propio
```

## Autocomprobación

- ¿Existe al menos un archivo real que justifique cada carpeta creada?
- ¿Alguna carpeta fue copiada de otro proyecto sin que este proyecto tenga esa capacidad?
- ¿El nombre de la carpeta coincide con el catálogo de esta guía o, si es nueva, sigue el mismo criterio de nomenclatura?
