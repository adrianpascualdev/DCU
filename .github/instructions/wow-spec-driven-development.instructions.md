---
applyTo: "**"
description: "Impone el desarrollo guiado por especificaciones: toda implementación, pruebas, automatización y documentación deben derivar de especificaciones claras, completas y comprobables redactadas antes de producir código."
---

# Directrices de Desarrollo Guiado por Especificaciones(`Spec-Driven Development Guidelines`)

Directrices para el desarrollo guiado por especificaciones, donde toda implementación, pruebas, automatización y documentación deben derivar de especificaciones claras, completas y comprobables redactadas antes de producir código.

Todo el trabajo en este repositorio **debe comenzar con una especificación clara, completa y comprobable** antes de crear cualquier implementación, automatización, prueba o documentación. La especificación actúa como la **fuente única de verdad** para requisitos, comportamiento, interfaces, restricciones y criterios de aceptación.

## Contexto del Proyecto

El Desarrollo Guiado por Especificaciones (SDD) garantiza la alineación entre requisitos, diseño, código generado por IA, pruebas y documentación al exigir que **las especificaciones precedan a la implementación** y permanezcan como referencia autorizada.

Este proceso se aplica a:

- Todos los lenguajes de programación, frameworks, plataformas y herramientas.
- Todos los flujos de trabajo (funcionalidades, correcciones, refactors, cambios de diseño, APIs, modelado de datos, infraestructura, automatización, CI/CD, etc.).
- Tanto el código escrito manualmente como el generado por IA (por ejemplo, GitHub Copilot).

Las especificaciones guían decisiones, evitan ambigüedades y mantienen la trazabilidad a lo largo del ciclo de vida.

## Objetivo

Asegurar que todos los artefactos de implementación (código, pruebas, configuraciones, documentación, pipelines) sean:

- Derivados de una especificación explícita, aprobada y mantenida.
- Consistentes con el comportamiento documentado.
- Trazables hasta las secciones o identificadores de la especificación.
- Actualizados sólo cuando la especificación cambie.

## Reglas Principales

### Especificación Antes de la Implementación

No debe comenzar ninguna implementación—código, pruebas, automatización o documentación—sin una **especificación aprobada** que describa el comportamiento esperado, el alcance, las restricciones y los criterios de aceptación.

### La Especificación como Fuente de Verdad

La especificación debe regir:

- El comportamiento del sistema.
- Las interfaces y contratos de datos.
- Los casos límite y las restricciones.
- Los criterios de aceptación.
- Las pruebas y la automatización.

Toda implementación debe permanecer consistente con la especificación.

### Incluir Criterios de Aceptación Concretos

Cada especificación debe incluir **criterios medibles y comprobables**.  
Estos criterios impulsan la creación de pruebas, la validación y la automatización.

### Mantener las Especificaciones Agnósticas a la Tecnología

La especificación debe describir **qué** hace el sistema, no **cómo** se implementa.

Evitar referencias a frameworks, librerías o lenguajes concretos salvo que sean necesarias o expresamente requeridas (por ejemplo, por las directrices del proyecto).

### Descomponer el Trabajo desde la Especificación

Descomponer las especificaciones en tareas trazables, casos de prueba y pasos de implementación.

Referenciar el ID de la especificación y las secciones relevantes en commits, PRs, pruebas y automatizaciones.

### Implementar y Probar Conforme a la Especificación

Todo el trabajo de implementación y pruebas debe seguir la especificación y mapearse a los criterios de aceptación.

Cuando el trabajo de implementación aporte nuevas ideas, actualizar primero la especificación y luego el código.

### Iterar Especificación e Implementación Juntas

Las especificaciones son documentos vivos. Evolucionan con el sistema, pero **los cambios deben realizarse en la especificación antes de modificar la implementación**.

### Asegurar Trazabilidad y Revisabilidad

Todo código, commits, PRs y suites de pruebas automatizadas deben referenciar su especificación de origen y los criterios de aceptación.

### Usar Especificaciones para Guiar IA y Herramientas

Al generar código o pruebas con herramientas de IA (por ejemplo, GitHub Copilot), proporcionar la especificación como entrada primaria para asegurar el cumplimiento fiel.

### Rechazar Cambios Sin Especificación

No se aceptará ningún comportamiento nuevo, cambio significativo o modificación incompatible sin una actualización acompañante de la especificación.

## Estructura de la Especificación

Cada especificación debe incluir:

### Descripción General

- Propósito
- Alcance (incluido/excluido)
- Contexto y encaje en el sistema

### Requisitos

- Requisitos Funcionales (FR-###)
- Requisitos No Funcionales (NFR-###)
- Restricciones (técnicas, regulatorias, de negocio)

### Definiciones de Interfaz

- Entradas y salidas
- Contratos (precondiciones, postcondiciones, invariantes)
- Manejo de errores y modos de fallo

### Especificaciones de Comportamiento

- Escenarios
- Casos límite
- Transiciones de estado
- Dependencias

### Criterios de Aceptación

- Condiciones precisas, comprobables y medibles
- Métricas de éxito
- Métodos de validación

### Ejemplos e Ilustraciones

- Casos de uso
- Datos de ejemplo
- Diagramas de actividad/secuencia/estado

### Metadatos

- ID de la especificación
- Versión
- Autores
- Estado (Draft, Approved, Implemented, Deprecated)
- Especificaciones relacionadas

## Orientación Específica por Tecnología

### Lenguajes de Propósito General

- Definir comportamiento de módulos/clases/funciones
- Documentar tipos y restricciones de entrada/salida
- Guardar las especificaciones en markdown junto al código fuente
- Referenciar IDs de especificación en comentarios y pruebas
- Derivar casos de prueba de las definiciones de escenario

### Desarrollo de APIs

- Las especificaciones pueden incluir OpenAPI, GraphQL SDL o protobuf
- Documentar comportamiento y casos límite en la especificación
- Validar la conformidad de solicitudes/respuestas durante las pruebas

### Bases de Datos y Modelado de Datos

- Definir esquemas, restricciones y reglas de integridad
- Documentar requisitos de rendimiento esperados para consultas
- Especificar el comportamiento de migraciones y compatibilidad retroactiva

### Frontend

- Documentar flujos UX, comportamiento de componentes y reglas de accesibilidad
- Incluir estados de carga/error/condiciones límite en la UI
- Referenciar el sistema de diseño o la especificación visual cuando sea necesario

### Infraestructura, DevOps y CI/CD

- Documentar topología, restricciones de recursos y comportamientos por entorno
- Especificar seguridad, observabilidad, recuperación y reglas de respaldo
- Describir estrategias de despliegue y rollback

### Testing y QA

- Derivar casos de prueba de los criterios de aceptación
- Mapear pruebas a requisitos específicos de la especificación
- Usar escenarios de la especificación para pruebas de integración, rendimiento y seguridad

## Integración en el Flujo de Trabajo

### Antes de Comenzar el Trabajo

1. Verificar si existe una especificación
2. Crear o actualizar la especificación si no existe
3. Asegurar que los criterios de aceptación sean claros
4. Hacer revisar y aprobar la especificación

### Durante la Implementación

1. Referenciar el ID de la especificación en commits y PRs
2. Implementar exactamente lo definido
3. Escribir pruebas que se correspondan directamente con los criterios de aceptación
4. Documentar supuestos y hallazgos en la especificación

### Después de la Implementación

1. Validar la implementación frente a todos los criterios de aceptación
2. Actualizar el estado de la especificación a "Implemented"
3. Enlazar los artefactos de implementación con la especificación
4. Realizar una revisión que compruebe explícitamente la conformidad con la especificación

### Cuando los Requisitos Cambian

1. Actualizar la especificación primero
2. Revisar la especificación actualizada
3. Actualizar las especificaciones relacionadas
4. Ajustar la implementación en consecuencia

## Formatos de Especificación

- Especificaciones narrativas en Markdown
- Especificaciones formales (matemáticas/rigurosas)
- Gherkin (Given/When/Then) para comportamiento
- OpenAPI / JSON Schema / GraphQL SDL / Protobuf
- Architecture Decision Records (ADRs)
- Documentos estilo RFC para propuestas mayores

## Resultados Esperados

- Cada característica o cambio dispone de una especificación preexistente
- Las especificaciones son claras, comprobables, completas y están actualizadas
- La implementación refleja fielmente el comportamiento documentado
- Trazabilidad completa entre especificaciones, implementación, pruebas y automatización
- Las revisiones de código disponen de criterios objetivos
- Las salidas generadas por IA se alinean con la especificación
- La especificación se convierte en documentación fiable y duradera

## Cumplimiento

El cumplimiento se hace efectivo mediante:

- Revisiones de especificación antes de la implementación
- Revisiones de código que validan la conformidad con la especificación
- Pruebas automatizadas derivadas de los criterios de aceptación
- Auditorías de documentación
- Controles de trazabilidad

Patrones anti-óptimos comunes a rechazar:

- Implementación sin especificación
- Especificaciones vagas o no comprobables
- Cambios en código sin actualización de la especificación
- Especificaciones que prescriben detalles de implementación
- Criterios de aceptación faltantes o ambiguos
- Trazabilidad rota

## Aplicabilidad

**IMPORTANTE**: Estas reglas se aplican a **todos los colaboradores**, **todos los cambios** (funcionalidades, correcciones, refactors) y **todos los flujos de generación**, manuales o por IA. No se permitirán merges o releases sin una especificación correspondiente y trazabilidad.

**NOTA**: Si un cambio no puede especificarse por completo inicialmente, documentar la comprensión actual, marcar preguntas abiertas y refinar de forma iterativa, **pero no permitir que el código se aleje de la especificación**.

> **Cuando haya dudas, prefiera el enfoque más sencillo, pregunte al usuario o solicite mejores especificaciones**