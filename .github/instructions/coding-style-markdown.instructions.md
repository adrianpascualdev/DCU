---
applyTo: "**/*.md"
description: "Directrices de estilo y formato para redactar documentación Markdown consistente y amigable para LLM."
---

# Directrices de estilo para Markdown

Directrices para redactar documentación Markdown limpia, consistente y portable, optimizada tanto para la legibilidad humana como para el procesamiento por LLM.

## Contexto del proyecto

- **Estándar**: Markdown compatible con CommonMark
- **Renderizado**: Debe funcionar con procesadores Markdown estándar y herramientas de documentación
- **Diagramas**: Usar Mermaid para diagramas en lugar de imágenes cuando sea posible

## Principios generales

- Producir Markdown que se renderice correctamente en diffs de git, terminales y sitios de documentación.
- Optimizar la legibilidad en forma cruda y renderizada.
- Preferir construcciones simples y portables sobre extensiones específicas del renderizador.
- Evitar usar HTML salvo cuando sea estrictamente necesario o lo exijan las especificaciones del proyecto o los usuarios.

## Espaciado y formato

- Usar una línea en blanco para separar elementos de bloque (encabezados, párrafos, listas, tablas, bloques de código, citas).
- No usar más de una línea en blanco consecutiva.
- No insertar espacios iniciales arbitrarios salvo los requeridos para anidar listas o bloques de código con cercos.
- Evitar espacios finales al final de las líneas.
- Finalizar siempre el archivo con una sola nueva línea (una línea en blanco al final).

## Encabezados

- Usar estilo ATX (`#`, `##`, `###`, `####`) con un espacio único después del numeral y **sin `#` de cierre**.
- Usar `sentence case` en los encabezados (capitalizar la primera palabra y nombres propios).
- Usar exactamente un H1 (`#`) por documento, como primer encabezado.
- No saltar niveles de encabezado; cambiar de nivel de uno en uno (H1 → H2 → H3 → H4).
- Colocar exactamente una línea en blanco antes y después de cada encabezado, salvo cuando el encabezado sea la primera línea.
- No repetir textos de encabezados.
  **NO HACER**:

  ```md
  # Some text

  ## Some text
  ```

## Párrafos

- Definir párrafos como una o más líneas de texto separadas de otros bloques por una sola línea en blanco.
- No sangrar la primera línea de un párrafo.
- Mantener cada párrafo como su propio bloque separado por una línea en blanco.

## Énfasis, negrita y código inline

- Usar `*italic*` para énfasis y `**bold**` para negrita; evitar variantes con `_`.
- Evitar el uso excesivo de negrita o cursiva, sobre todo en encabezados. Mantener los encabezados en texto normal.
- Usar backticks para código inline: `` `Get-Help` ``.
- Cuando el código inline contiene backticks, aumentar el tamaño del delimitador. Usar tantos backticks como sea necesario, por ejemplo:
  - Si el código contiene un solo backtick: usar backticks dobles: `` ``XY`` ``.
  - Si el código contiene contenido similar a triple backticks, usar cuatro backticks: `` ```x``` ``.

## Listas

- Usar `-` para listas desordenadas (no `*` ni `+`).
- Usar numeración secuencial para listas ordenadas (`1.`, `2.`, `3.`).
- Colocar exactamente un espacio entre el marcador o número y el texto (`- item`, `1. item`).
- Insertar una sola línea en blanco antes y después de una lista cuando esté rodeada por otros elementos de bloque.
- Mantener listas simples contiguas (sin líneas en blanco entre ítems).
- Para ítems complejos que contengan varios párrafos o bloques de código:
  - Empezar el primer párrafo en la misma línea que la viñeta.
  - Sangrar los párrafos subsecuentes, bloques de código o sublistas con **dos espacios** para que sigan perteneciendo al mismo ítem.
  - Dejar una sola línea en blanco entre párrafos o bloques dentro del mismo ítem.
- Puntuación: no añadir punto final a ítems de lista salvo cuando el ítem sea una frase completa.

### Listas anidadas

- Sangrar las listas anidadas con **dos espacios** desde la columna del texto del marcador padre.
- Usar el mismo estilo de viñeta (`-`) y reglas de numeración que la lista padre.

## Bloques de código

- Preferir bloques de código con cercos (triple backticks) sobre bloques indentados.
- Especificar siempre el lenguaje cuando se conozca (`powershell`, `bash`, `json`, `yaml`, `csharp`, `vb`, `xml`, `xsd`, etc.). Usar `text` para ejemplos no ejecutables o por defecto.
- Insertar una sola línea en blanco antes y después de bloques de código con cercos.
- Colocar las líneas de cierre y apertura de los cercos en la columna 0, salvo que el bloque esté dentro de un ítem de lista.
- Para bloques de código dentro de ítems de lista:
  - Sangrar el bloque con **dos espacios** para alinearlo con el contenido del ítem.
  - Mantener la alineación entre la línea de apertura, la de cierre y el código interno.
- Evitar múltiples líneas en blanco consecutivas dentro de bloques de código.

## Enlaces y URLs

- Usar texto descriptivo en los enlaces en lugar de URLs crudas. Ejemplo: `[Markdown syntax guide](https://www.markdownguide.org/basic-syntax/)`.
- No partir definiciones de enlaces en varias líneas salvo cuando sea necesario.
- Para enlaces de estilo referencia, agrupar las definiciones cerca del final del archivo separadas por una sola línea en blanco.
- Preferir rutas relativas para enlaces/imágenes internas y URLs absolutas para recursos externos.

## Imágenes y accesibilidad

- Usar la sintaxis Markdown para imágenes; no usar `<img>` en HTML crudo.
- Proveer texto alternativo (`alt`) tanto como sea posible: `![Texto alternativo breve que describa la imagen](/assets/diagram.png)`.
- Si la imagen requiere una explicación más larga, añadir un pie de figura o un párrafo descriptivo tras la imagen.
- Preferir rutas relativas para imágenes del repositorio y para imágenes pequeñas en `assets/`.
- Ejemplo con pie de figura:

```md
![Architecture overview](/assets/arch.png)

*Figure 1 — high-level architecture overview.*
```

## Tablas

- Usar tablas con pipes (`|`) incluyendo fila de encabezado y separador (Estilo `compact`).
- Alinear columnas con dos puntos cuando la alineación importe (`:---`, `:---:`, `---:`).
- No rellenar las celdas con espacios para alinear columnas visualmente; escribir el contenido de cada celda sin espaciado de relleno.
- Colocar una sola línea en blanco antes y después de las tablas.
- Mantener el contenido de las celdas breve; trasladar explicaciones largas a un párrafo posterior.

Ejemplos:

``` md

| Cabecera Columna 1 | Cabecera Columna 2 |
| --- | --- |
| Contenido Fila 1 Columna 1 | Contenido Fila 1 Columna 2 |

```

## Sintaxis extendida

- Listas de tareas: `- [ ]` y `- [x]`.
- Notas al pie: usar `[^1]` con definiciones agrupadas al final.
- Tachado: `~~texto~~`.
- Rodear bloques de listas de tareas con una sola línea en blanco antes y después.
- Agrupar las definiciones de notas al pie separadas del contenido principal por una sola línea en blanco.

## Idioma, tono y estilo

- Usar el español como idioma (excepto para tecnicimos que estarán entre comillas ``)
- Usar oraciones claras y concisas y voz activa.
- Preferir la segunda persona ("tú" o "usted" según contexto) para documentación de tipo how-to y destinada al usuario.
- Para comandos o código:
  - Proveer una frase corta de contexto antes del bloque de código.
  - Seguir el bloque de código con una breve explicación de lo que hace.

## Directrices específicas para LLM

- Respetar la regla de una línea en blanco entre elementos de bloque.
- Minimizar cambios en el espaciado existente salvo para corregir inconsistencias.
- Conservar la sangría existente dentro de listas y bloques de código; ajustar solo cuando esté rota.
- Al realizar ediciones, preferir commits pequeños y focalizados que minimicen el reformatado no relacionado.
- Para contenido generado: cuando proceda, incluir ejemplos cortos y pruebas mínimas.

## Referencias

- [Markdown Guide: Basic Syntax](https://www.markdownguide.org/basic-syntax/)
- [Markdown Guide: Extended Syntax](https://www.markdownguide.org/extended-syntax/)
- [Mermaid Syntax](https://mermaid.js.org/intro/syntax-reference.html)
