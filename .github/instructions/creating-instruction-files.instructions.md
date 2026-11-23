---
applyTo: '**/*.instructions.md'
description: 'Directrices para crear archivos de instrucciones personalizados de alta calidad para GitHub Copilot'
---

# Directrices para archivos de instrucciones personalizados

Instrucciones para crear archivos de instrucciones personalizados efectivos y mantenibles que guíen a GitHub Copilot en la generación de código específico de dominio y en el cumplimiento de las convenciones del proyecto.

## Contexto del proyecto

- Público objetivo: Desarrolladores y GitHub Copilot que trabajan con código específico de dominio
- Formato de archivo: Markdown con frontmatter YAML
- Convención de nombres de archivo: minúsculas con guiones (ej.: `react-best-practices.instructions.md`)
- Ubicación: directorio `.github/instructions/`
- Propósito: Proveer orientación con contexto para la generación de código, revisión y documentación

## Frontmatter requerido

Cada archivo de instrucciones debe incluir un frontmatter YAML con los siguientes campos:

```yaml
---
description: 'Descripción breve del propósito y alcance de las instrucciones'
applyTo: 'patrón glob para los archivos objetivo (ej.: **/*.ts, **/*.py)'
---
```

### Directrices para el frontmatter

- **description**: Cadena entre comillas simples, 1-500 caracteres, que indique claramente el propósito
- **applyTo**: Patrón(es) glob que especifican a qué archivos se aplican estas instrucciones
  - Patrón único: `'**/*.ts'`
  - Múltiples patrones: `'**/*.ts, **/*.tsx, **/*.js'`
  - Archivos específicos: `'src/**/*.py'`
  - Todos los archivos: `'**'`

## Estructura del archivo

Un archivo de instrucciones bien estructurado debe incluir las siguientes secciones:

### 1. Título y visión general

- Título claro y descriptivo usando un encabezado `#`
- Breve introducción que explique propósito y alcance
- Opcional: sección de contexto del proyecto con tecnologías clave y versiones

### 2. Secciones principales

Organiza el contenido en secciones lógicas según el dominio:

- **Instrucciones generales**: Directrices y principios de alto nivel
- **Buenas prácticas**: Patrones y enfoques recomendados
- **Estándares de código**: Convenciones de nombres, formato, reglas de estilo
- **Arquitectura/estructura**: Organización del proyecto y patrones de diseño
- **Patrones comunes**: Implementaciones frecuentemente usadas
- **Seguridad**: Consideraciones de seguridad (si aplica)
- **Rendimiento**: Directrices de optimización (si aplica)
- **Pruebas**: Estándares y enfoques para testing (si aplica)

### 3. Ejemplos y fragmentos de código

Proporciona ejemplos concretos con etiquetas claras:

````markdown
### Buen ejemplo

```language
// Enfoque recomendado
code example here
```

### Mal ejemplo

```language
// Evitar este patrón
code example here
```
````

### 4. Validación y verificación (opcional pero recomendado)

- Comandos de compilación para verificar el código
- Herramientas de linting y formateo
- Requisitos de pruebas
- Pasos de verificación

## Directrices de contenido

### Estilo de redacción

- Usa un lenguaje claro y conciso
- Escribe en modo imperativo ("Usa", "Implementa", "Evita")
- Sé específico y accionable
- Evita términos ambiguos como "debería", "podría", "posiblemente"
- Usa viñetas y listas para mejorar la legibilidad
- Mantén las secciones enfocadas y fáciles de escanear

### Buenas prácticas

- **Sé específico**: Proporciona ejemplos concretos en lugar de conceptos abstractos
- **Explica el porqué**: Detalla la razón detrás de las recomendaciones cuando aporte valor
- **Usa tablas**: Para comparar opciones, listar reglas o mostrar patrones
- **Incluye ejemplos**: Fragmentos de código reales son más efectivos que descripciones
- **Mantente actualizado**: Referencia versiones actuales y buenas prácticas
- **Enlaza recursos**: Incluye documentación oficial y fuentes autorizadas

### Grado de especificidad (zona de Ricitos de Oro)

- Comienza con el conjunto mínimo de reglas que definan completamente los resultados esperados
- Agrega restricciones después de observar fallos, no por casos hipotéticos
- Prefiere ejemplos con alta señal en lugar de tablas de decisiones exhaustivas

| Grado | Modo de fallo | Resultado |
| --- | --- | --- |
| Sobreespecificado | Prosa frágil con if-else | Falla en casos no listados |
| Subespecificado | Asume contexto compartido | Salidas genéricas |
| Grado adecuado | Heurísticas + ejemplos | Calidad estable y generalizable |

### Patrones comunes a incluir

1. **Convenciones de nombres**: Cómo nombrar variables, funciones, clases, archivos
2. **Organización del código**: Estructura de archivos, organización de módulos, orden de imports
3. **Manejo de errores**: Patrones preferidos para el tratamiento de errores
4. **Dependencias**: Cómo gestionar y documentar dependencias
5. **Comentarios y documentación**: Cuándo y cómo documentar el código
6. **Información de versiones**: Versiones objetivo del lenguaje/framework

## Patrones a seguir

### Viñetas y listas

```markdown
## Buenas prácticas de seguridad

- Valida siempre la entrada de usuario antes de procesarla
- Usa consultas parametrizadas para prevenir inyección SQL
- Almacena secretos en variables de entorno, nunca en el código
- Implementa autenticación y autorización adecuadas
- Habilita HTTPS para todos los endpoints de producción
```

### Tablas para información estructurada

```markdown
## Problemas comunes

| Problema | Solución | Ejemplo |
| ---------------- | ------------------- | ------------------------------- |
| Números mágicos | Usa constantes con nombre | `const MAX_RETRIES = 3` |
| Anidamiento profundo | Extrae funciones | Refactorizar sentencias if anidadas |
| Valores hardcodeados | Usa configuración | Almacenar URLs de API en config |
```

### Comparación de código

````markdown
### Buen ejemplo - usando interfaces de TypeScript

```typescript
interface User {
  id: string;
  name: string;
  email: string;
}

function getUser(id: string): User {
  // Implementación
}
```

### Mal ejemplo - usando tipo any

```typescript
function getUser(id: any): any {
  // Pierde seguridad de tipos
}
```
````

### Orientación condicional

```markdown
## Selección de framework

- **Para proyectos pequeños**: Usa un enfoque Minimal API
- **Para proyectos grandes**: Usa arquitectura basada en controladores con separación clara
- **Para microservicios**: Considera patrones de diseño orientados al dominio
```

## Patrones a evitar

- **Explicaciones excesivamente verbosas**: Mantenlo conciso y fácil de escanear
- **Información desactualizada**: Referencia siempre versiones y prácticas actuales
- **Directrices ambiguas**: Sé específico sobre qué hacer o evitar
- **Falta de ejemplos**: Reglas abstractas sin fragmentos de código concretos
- **Consejos contradictorios**: Asegura coherencia en todo el archivo
- **Copiar-pegar de la documentación**: Añade valor destilando y contextualizando
- **Inflación hipotética de reglas**: No añadas reglas para fallos que no han ocurrido

## Probar tus instrucciones

Antes de finalizar los archivos de instrucciones:

1. **Probar con Copilot**: Ensaya las instrucciones con prompts reales en VS Code
2. **Verificar ejemplos**: Asegura que los fragmentos de código sean correctos y se ejecuten sin errores
3. **Comprobar patrones glob**: Confirma que los patrones `applyTo` coinciden con los archivos previstos

## Estructura de ejemplo

A continuación un ejemplo mínimo de estructura para un nuevo archivo de instrucciones:

````markdown
---
description: 'Descripción breve del propósito'
applyTo: '**/*.ext'
---

# Desarrollo para [nombre de la tecnología]

Breve introducción y contexto.

## Instrucciones generales

- Directriz de alto nivel 1
- Directriz de alto nivel 2

## Buenas prácticas

- Práctica específica 1
- Práctica específica 2

## Estándares de código

### Convenciones de nombres

- Regla 1
- Regla 2

### Organización de archivos

- Estructura 1
- Estructura 2

## Patrones comunes

### Patrón 1

Descripción y ejemplo

```language
code example
```

### Patrón 2

Descripción y ejemplo

## Validación

- Comando de build: `command to verify`
- Linting: `command to lint`
- Pruebas: `command to test`
````

## Mantenimiento

- Revisa las instrucciones cuando las dependencias o frameworks sean actualizados
- Actualiza los ejemplos para reflejar las mejores prácticas actuales
- Elimina patrones obsoletos o características deprecadas
- Añade nuevos patrones a medida que surjan en la comunidad
- Mantén los patrones glob precisos según evolucione la estructura del proyecto

## Recursos adicionales

- [Documentación de instrucciones personalizadas](https://code.visualstudio.com/docs/copilot/customization/custom_instructions)
- [Awesome Copilot Instructions](https://github.com/github/awesome-copilot/tree/main/instructions)
- [System Prompt Altitude - Effective Context Engineering for AI Agents](https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents#the-anatomy-of-effective-context)
