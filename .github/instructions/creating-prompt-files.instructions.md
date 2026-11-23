---
applyTo: '**/*.prompt.md'
description: 'Directrices para crear archivos de prompt de alta calidad para GitHub Copilot'
---

# Directrices para archivos de prompt de Copilot

Directrices para crear archivos de prompt eficaces y mantenibles que guíen a GitHub Copilot hacia resultados consistentes y de alta calidad en cualquier repositorio.

## Alcance y principios

- Audiencia objetivo: personas mantenedoras y colaboradoras que redactan prompts reutilizables para Copilot Chat
- Objetivos: comportamiento predecible, expectativas claras, permisos mínimos y portabilidad entre repositorios
- Referencias principales: documentación de VS Code sobre archivos de prompt y convenciones específicas de la organización

## Requisitos del frontmatter

Cada archivo de prompt debe incluir frontmatter YAML con los siguientes campos:

### Campos obligatorios o recomendados

| Field | Required | Description |
| --- | --- | --- |
| `description` | Recomendado | Descripción breve del prompt en una sola frase y orientada a la acción |
| `name` | Opcional | Nombre que se muestra tras escribir `/` en el chat. Si no se indica, se usa el nombre del archivo |
| `agent` | Recomendado | Agente que debe usarse: `ask`, `edit`, `agent` o un agente personalizado. Por defecto se usa el agente actual |
| `model` | Opcional | Modelo de lenguaje que debe utilizarse. Por defecto se usa el modelo seleccionado en ese momento |
| `tools` | Opcional | Lista de herramientas o conjuntos de herramientas disponibles para el prompt |
| `argument-hint` | Opcional | Texto de ayuda que se muestra en la entrada del chat para orientar la interacción |

### Directrices

- Usa un criterio de comillas consistente, con preferencia por comillas simples, y mantén un campo por línea para favorecer la legibilidad y la claridad en control de versiones
- Si especificas `tools` y el agente actual es `ask` o `edit`, el agente por defecto pasa a ser `agent`
- Conserva cualquier metadato adicional (`language`, `tags`, `visibility`, etc.) que requiera tu organización

## Nombres de archivo y ubicación

- Usa nombres en `kebab-case` terminados en `.prompt.md` y guárdalos en `.github/prompts/`, salvo que el estándar del workspace indique otra ubicación
- Elige un nombre corto que comunique la acción, por ejemplo `generate-readme.prompt.md` en lugar de `prompt1.prompt.md`

## Estructura del cuerpo

- Empieza con un encabezado de nivel `#` que refleje la intención del prompt para que aparezca con claridad en la búsqueda de Quick Pick
- Organiza el contenido en secciones predecibles. La base recomendada es: `Mission` o `Primary Directive`, `Scope & Preconditions`, `Inputs`, `Workflow`, `Output Expectations` y `Quality Assurance`
- Ajusta los nombres de sección al dominio, pero conserva el flujo lógico: propósito -> contexto -> entradas -> acciones -> salidas -> validación
- Referencia prompts o archivos de instrucciones relacionados mediante enlaces relativos para facilitar su descubrimiento

## Gestión de entradas y contexto

- Usa `${input:variableName[:placeholder]}` para los valores obligatorios y explica cuándo debe proporcionarlos la persona usuaria. Ofrece valores por defecto o alternativas siempre que sea posible
- Menciona variables de contexto como `${selection}`, `${file}` o `${workspaceFolder}` solo cuando sean esenciales, e indica cómo debe interpretarlas Copilot
- Documenta cómo debe actuarse cuando falte contexto obligatorio, por ejemplo: "Solicita la ruta del archivo y detente si sigue sin definirse"

## Directrices sobre herramientas y permisos

- Limita `tools` al conjunto mínimo que permita completar la tarea. Enuméralas en el orden preferido de ejecución cuando la secuencia importe
- Si el prompt hereda herramientas de un modo de chat, indica esa relación y aclara los comportamientos críticos o efectos laterales relevantes
- Advierte sobre operaciones destructivas, como creación de archivos, ediciones o comandos de terminal, e incluye pasos de control o confirmación dentro del flujo

## Tono y estilo de las instrucciones

- Escribe instrucciones directas, en imperativo y dirigidas a Copilot, por ejemplo: "Analiza", "Genera" o "Resume"
- Mantén frases cortas y sin ambigüedad para facilitar la localización y la interpretación consistente
- Evita modismos, humor o referencias culturales específicas; prioriza un lenguaje neutro e inclusivo

## Definición de la salida

- Especifica el formato, la estructura y la ubicación de los resultados esperados, por ejemplo: "Crea `docs/adr/adr-XXXX.md` usando la plantilla siguiente"
- Incluye criterios de éxito y condiciones de fallo para que Copilot sepa cuándo debe detenerse o reintentar
- Proporciona pasos de validación, como comprobaciones manuales, comandos automáticos o listas de aceptación, que puedan ejecutar quienes revisen el resultado

## Ejemplos y recursos reutilizables

- Incluye ejemplos de buenas y malas salidas o plantillas reutilizables, como esqueletos Markdown o estructuras JSON, que el prompt deba seguir o generar
- Mantén tablas de referencia en línea, como capacidades, códigos de estado o descripciones de roles, para que el prompt sea autocontenido. Actualízalas cuando cambien los recursos de origen
- Enlaza a documentación autoritativa en lugar de duplicar orientación extensa

## Lista de comprobación de calidad

- [ ] Los campos del frontmatter están completos, son precisos y siguen el principio de mínimo privilegio
- [ ] Las entradas contemplan placeholders, comportamientos por defecto y alternativas de respaldo
- [ ] El flujo cubre preparación, ejecución y posprocesado sin lagunas
- [ ] La definición de la salida incluye detalles de formato y almacenamiento
- [ ] Los pasos de validación son accionables, con comandos, revisión de diffs o prompts de revisión
- [ ] Las políticas de seguridad, cumplimiento y privacidad referenciadas por el prompt están vigentes
- [ ] El prompt se ejecuta correctamente en VS Code con `Chat: Run Prompt` usando escenarios representativos

## Directrices de mantenimiento

- Mantén los prompts bajo control de versiones junto al código al que afectan y actualízalos cuando cambien las dependencias, las herramientas o los procesos de revisión
- Revisa los prompts periódicamente para asegurar que las listas de herramientas, los requisitos de modelo y los documentos enlazados sigan siendo válidos
- Coordínate con otros repositorios: cuando un prompt resulte ampliamente útil, extrae la orientación común a archivos de instrucciones o paquetes de prompts compartidos

## Recursos adicionales

- [Prompt Files Documentation](https://code.visualstudio.com/docs/copilot/customization/prompt-files#_prompt-file-format)
- [Awesome Copilot Prompt Files](https://github.com/github/awesome-copilot/tree/main/prompts)
- [Tool Configuration](https://code.visualstudio.com/docs/copilot/chat/chat-agent-mode#_agent-mode-tools)
