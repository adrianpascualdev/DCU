---
name: microsoft-docs-and-code-consultant
description: "Consulta documentación oficial de Microsoft para entender conceptos, encontrar tutoriales, aprender cómo funcionan los servicios, localizar referencias de API, encontrar muestras de código funcionales y verificar que el código de SDK sea correcto. Usa esta skill al trabajar con SDK de Azure, bibliotecas .NET o API de Microsoft para encontrar el método adecuado, comprobar parámetros, obtener ejemplos válidos o diagnosticar errores. Reduce alucinaciones de métodos, firmas incorrectas y patrones obsoletos al consultar documentación oficial. Obtén información precisa y actual de `learn.microsoft.com` y otros sitios oficiales de Microsoft sobre arquitectura, quickstarts, configuración, límites y buenas prácticas."
compatibility: "Requires Microsoft Learn MCP Server (https://learn.microsoft.com/api/mcp)"
---

# Documentación de Microsoft

## Herramientas

| Herramienta | Úsala para |
| --- | --- |
| `microsoft_docs_search` | Encontrar documentación sobre conceptos, guías, tutoriales y configuración |
| `microsoft_docs_fetch` | Obtener el contenido completo de una página cuando los fragmentos no bastan |
| `microsoft_code_sample_search` | Encontrar muestras oficiales de código que puedas usar como referencia o inspiración |

## Cuándo usarla

- **Entender conceptos**: "How does Cosmos DB partitioning work?"
- **Aprender un servicio**: "Azure Functions overview", "Container Apps architecture"
- **Encontrar tutoriales**: "quickstart", "getting started", "step-by-step"
- **Opciones de configuración**: "App Service configuration settings"
- **Límites y cuotas**: "Azure OpenAI rate limits", "Service Bus quotas"
- **Buenas prácticas**: "Azure security best practices"
- **Antes de escribir código**, para encontrar un patrón funcional que seguir
- **Después de errores**, para comparar tu código con una muestra conocida como válida
- **Cuando no tengas claro el setup o la inicialización**, porque las muestras muestran contexto completo

## Eficacia de las consultas

Las buenas consultas son específicas:

```text
MALO - demasiado amplia
"Azure Functions"

BUENO - específica
"Azure Functions Python v2 programming model"
"Cosmos DB partition key design best practices"
"Container Apps scaling rules KEDA"
```

Incluye contexto:

- **Versión** cuando sea relevante, como `.NET 8`, `.NET 10`, `C# 14` o `EF Core 8`
- **Intención de la tarea**, como `quickstart`, `tutorial`, `overview` o `limits`
- **Plataforma** cuando la documentación sea multiplataforma, por ejemplo `Linux` o `Windows`

## Cuándo traer la página completa

Usa `microsoft_docs_fetch` tras la búsqueda cuando:

- **Tutoriales**: necesites instrucciones completas paso a paso
- **Guías de configuración**: necesites todas las opciones listadas
- **Deep dives**: la persona usuaria quiera cobertura completa
- **El fragmento de búsqueda esté cortado**: necesites el contexto completo

## Cómo encontrar muestras de código

Usa `microsoft_code_sample_search` para obtener ejemplos oficiales y funcionales:

```text
microsoft_code_sample_search(query: "upload file to blob storage", language: "csharp")
microsoft_code_sample_search(query: "authenticate with managed identity", language: "python")
microsoft_code_sample_search(query: "send message service bus", language: "javascript")
```

### Búsqueda de API

```text
Verifica que el método exista e incluye el namespace para ganar precisión
"BlobClient UploadAsync Azure.Storage.Blobs"
"GraphServiceClient Users Microsoft.Graph"

Encuentra una clase o interfaz
"DefaultAzureCredential class Azure.Identity"

Encuentra el paquete correcto
"Azure Blob Storage NuGet package"
"azure-storage-blob pip package"
```

Trae la página completa cuando el método tenga varias sobrecargas o necesites todos los detalles de sus parámetros.

## Diagnóstico de errores

- Usa `microsoft_code_sample_search` para localizar muestras funcionales y compararlas con tu implementación.
- Para errores concretos, usa `microsoft_docs_search` y `microsoft_docs_fetch`:

| Tipo de error | Consulta |
| --- | --- |
| Método no encontrado | `"[ClassName] methods [Namespace]"` |
| Tipo no encontrado | `"[TypeName] NuGet package namespace"` |
| Firma incorrecta | `"[ClassName] [MethodName] overloads"` y luego trae la página completa |
| Advertencia de obsolescencia | `"[OldType] migration v12"` |
| Fallo de autenticación | `"DefaultAzureCredential troubleshooting"` |
| 403 Forbidden | `"[ServiceName] RBAC permissions"` |

## Cuándo verificar

Verifica siempre cuando:

- El nombre del método parezca "demasiado conveniente", como `UploadFile` frente al real `Upload`
- Estés mezclando versiones de SDK, por ejemplo v11 `CloudBlobClient` frente a v12 `BlobServiceClient`
- El nombre del paquete no siga las convenciones, como `Azure.*` para .NET o `azure-*` para Python
- Estés usando una API por primera vez

## Flujo de validación

Antes de generar código con SDK de Microsoft, verifica que sea correcto:

1. **Confirma que el método o paquete existe** con `microsoft_docs_search(query: "[ClassName] [MethodName] [Namespace]")`
2. **Trae el detalle completo** cuando haya sobrecargas o parámetros complejos con `microsoft_docs_fetch(url: "...")`
3. **Encuentra una muestra funcional** con `microsoft_code_sample_search(query: "[task]", language: "[lang]")`

Para consultas simples puede bastar con el paso 1. Para usos complejos de una API, completa los tres pasos.

## Por qué usarla

- **Precisión**: documentación viva y actual, no datos de entrenamiento potencialmente desactualizados
- **Completitud**: los tutoriales incluyen todos los pasos, no fragmentos aislados
- **Autoridad**: documentación oficial de Microsoft
