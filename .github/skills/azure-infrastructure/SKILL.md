---
name: azure-infrastructure
description: 'Diseña y mantiene infraestructura Azure con Bicep. Úsala al crear o revisar plantillas, integrar Application Insights con Log Analytics, comprobar recursos existentes o documentar una arquitectura Azure.'
---

# Infraestructura Azure con Bicep

Aplica prácticas de infraestructura Azure sin asumir convenciones organizativas, nombres de recursos ni topologías que no estén confirmados para el proyecto.

## Cuándo usar esta skill

- Al crear o modificar plantillas Bicep.
- Al integrar Application Insights con un Log Analytics Workspace.
- Al revisar grupos de recursos, ámbitos de despliegue o permisos necesarios.
- Antes de proponer recursos nuevos, cuando haya que comprobar si ya existen en Azure.
- Al crear o actualizar diagramas de infraestructura a petición de la persona usuaria.

## Principios generales

- Usa la configuración y las convenciones del proyecto como fuente de verdad. Si no existen o son ambiguas, pregunta antes de fijar nombres, topología, SKU, retención o separación de entornos.
- Conserva los identificadores simbólicos, nombres de parámetros y nombres ARM existentes. Cámbialos solo si se solicita o hay una necesidad técnica concreta, y explica el motivo.
- Mantén secretos y credenciales fuera de las plantillas, los parámetros versionados y la documentación.
- No despliegues, modifiques ni elimines recursos remotos salvo que la persona usuaria lo solicite expresamente.

## Verificación de recursos existentes

- Si las herramientas MCP de Azure están disponibles, usa consultas de solo lectura para comprobar recursos existentes antes de proponer otros nuevos.
- Confirma la suscripción y el grupo de recursos que corresponden al entorno y a la carga de trabajo. No deduzcas la existencia de recursos a partir de plantillas exportadas o parámetros locales.
- Contrasta lo observado en Azure con el entrypoint Bicep y sus módulos. Distingue los recursos remotos, los declarados en Bicep y los archivos de referencia.
- Si no puedes consultar Azure o no hay contexto de autenticación, indica esa limitación y no presentes datos locales como verificación remota.

## Organización y despliegues Bicep

- Sigue la estructura de infraestructura ya establecida por el proyecto. Si no hay una, acuerda una ubicación antes de introducirla.
- Mantén explícito el ámbito de despliegue cuando los recursos pertenezcan a grupos de recursos distintos.
- Comprueba qué permisos requiere cada ámbito y quién ejecutará el despliegue. No presupongas acceso a otros grupos o suscripciones.
- Usa `what-if` para revisar cambios de despliegue cuando se solicite validar un plan. No ejecutes el despliegue sin autorización explícita.
- Trata las plantillas exportadas como referencia, no como sustituto de los módulos de trabajo del proyecto.

## Application Insights y Log Analytics

- Reutiliza un workspace si la configuración existente identifica claramente el destino previsto.
- Si no se conoce el workspace, el grupo de recursos destino, el SKU o la retención, pregunta antes de añadir o modificar recursos.
- Si el workspace está en otro grupo de recursos, declara explícitamente ese ámbito y confirma los permisos necesarios para ambos grupos.
- Configura Application Insights con el `resourceId` del workspace correspondiente mediante `workspaceResourceId`.
- Mantén coherentes el SKU y la retención entre los despliegues que compartan un workspace, cuando esos valores los determine el proyecto.

## Diagramas de infraestructura

- Crea o actualiza diagramas solo cuando la persona usuaria lo solicite.
- Distingue la topología declarada en Bicep del estado observado en Azure. Para representar el estado remoto, usa una consulta actual de solo lectura.
- Agrupa los recursos por sus grupos de recursos reales y dibuja solo relaciones verificadas. No afirmes conectividad extremo a extremo sin comprobarla.
- Distingue los datos observados de los pendientes o no disponibles. Indica las fuentes consultadas, las limitaciones y, para una instantánea del estado remoto, la fecha y hora de consulta con su zona horaria.
- No hagas cambios en Bicep para que coincida con una instantánea remota, salvo que se solicite también actualizar la infraestructura declarada.

## Iconos oficiales de Azure

- Reutiliza primero los SVG disponibles en `assets/azure-icons/` dentro de esta skill.
- Si hace falta un icono que no está disponible, utiliza únicamente el [paquete oficial de iconos de Azure](https://learn.microsoft.com/azure/architecture/icons/) y conserva el SVG original sin modificarlo.
- Usa como caché temporal `azure-service-icons` y reutiliza cualquier descarga disponible antes de descargar otra. No recorras el equipo buscando copias.
- Comprueba el tipo ARM en la declaración Bicep o en el módulo utilizado. No infieras el tipo por el nombre, entorno o DNS del recurso.
- Ajusta la ruta relativa al README que contiene el diagrama y verifica que cada archivo referenciado existe.

| Tipo ARM | Servicio | Archivo SVG oficial |
| --- | --- | --- |
| `Microsoft.Web/serverfarms` | App Service Plan | `00046-icon-service-App-Service-Plans.svg` |
| `Microsoft.Web/sites` (Web App, no Function App) | App Service | `10035-icon-service-App-Services.svg` |
| `Microsoft.Web/staticSites` | Static Web Apps | `01007-icon-service-Static-Apps.svg` |
| `Microsoft.Insights/components` | Application Insights | `00012-icon-service-Application-Insights.svg` |
| `Microsoft.Storage/storageAccounts` | Storage Account | `10086-icon-service-Storage-Accounts.svg` |
| `Microsoft.Network/virtualNetworks` | Virtual Network | `10061-icon-service-Virtual-Networks.svg` |
| `Microsoft.Network/privateEndpoints` | Private Endpoint | `02579-icon-service-Private-Endpoints.svg` |
| `Microsoft.Network/networkInterfaces` | Network Interface | `10080-icon-service-Network-Interfaces.svg` |
| `Microsoft.OperationalInsights/workspaces` | Log Analytics Workspace | `00009-icon-service-Log-Analytics-Workspaces.svg` |

## Gotchas

- No conviertas una práctica frecuente de Azure en una regla del proyecto. Confirma los requisitos de nombres, red, entornos y monitorización con la configuración o con la persona usuaria.
- Una plantilla exportada no demuestra que el recurso vaya a formar parte del despliegue actual.
- No presentes una referencia Bicep, un parámetro o un resultado DNS como prueba de que un recurso está desplegado o accesible.
