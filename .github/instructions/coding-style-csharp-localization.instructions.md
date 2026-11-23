---
applyTo: "**/*.cs"
description: "Directrices de idioma y localización para código C#: castellano en textos legibles, inglés en identificadores, y uso obligatorio de archivos de recursos (.resx) para textos destinados al usuario."
---

# Directrices de idioma y localización en C\#

Directrices para asegurar la coherencia de idioma en el código C# y la correcta localización de textos destinados al usuario mediante archivos de recursos (`.resx`).

## Contexto del proyecto

- **Idioma de textos legibles**: castellano (documentación XML, logs, mensajes de excepción descriptivos, telemetría, comentarios explicativos)
- **Idioma de identificadores de código**: inglés (clases, métodos, propiedades, variables, namespaces, tipos, parámetros)
- **Localización de textos del usuario**: archivos de recursos (`.resx`) para permitir soporte multi-idioma
- **Combinar con instrucciones**:
  - [Directrices de documentación XML](coding-style-csharp-xml-documentation.instructions.md) para reglas específicas de `<summary>`, `<param>`, `<returns>`, `<exception>` e `<inheritdoc />`
  - [Directrices de clases Settings](coding-style-csharp-settings-classes.instructions.md) para el patrón de `ErrorMessageResourceType` en atributos de validación

## Reglas de idioma obligatorias

- La documentación XML (`<summary>`, `<remarks>`, `<param>`, `<returns>`, `<exception>`) debe estar en castellano.
- Los mensajes de logs (`LogInformation`, `LogWarning`, `LogError`, etc.) deben estar en castellano.
- Los mensajes de excepciones descriptivas y telemetría descriptiva deben estar en castellano.
- Los identificadores de código deben mantenerse en inglés.
- Esta regla no aplica a métodos, tipos, propiedades o contratos externos ajenos al proyecto, ni a código generado automáticamente a partir de dichos contratos externos (por ejemplo, clientes SOAP, paquetes NuGet usados externos al proyecto, proxies o referencias de servicio generadas), cuyos nombres deben preservarse tal como los define el proveedor o la herramienta de generación.
- Solo se permite inglés en claves técnicas externas cuando lo exija el framework o proveedor (por ejemplo, headers HTTP estándar, nombres de configuración o contratos externos).

## Textos destinados al usuario y archivos de recursos (.resx)

### Regla principal

Los mensajes de validación, excepciones u otros textos destinados a mostrarse al usuario deben definirse en archivos de recursos (`.resx`) para permitir localización y soporte multi-idioma.

### Qué va en `.resx`

- Mensajes de error de validación (`ErrorMessage` de Data Annotations)
- Textos de excepciones que se muestran al usuario final
- Etiquetas y descripciones de interfaz de usuario
- Mensajes de notificación al usuario

### Qué NO va en `.resx`

- Mensajes de logs (van directamente en castellano en el código)
- Consultas SQL
- Nombres de configuración y claves técnicas
- Cadenas internas de infraestructura o implementación
- Identificadores de telemetría

### Patrón con Data Annotations

Usar `ErrorMessageResourceType` y `ErrorMessageResourceName` en los atributos de validación para obtener los mensajes desde recursos. Evitar `ErrorMessage` inline cuando el texto pueda llegar al usuario final.

```csharp
[Required(
    ErrorMessageResourceType = typeof(ValidationMessages),
    ErrorMessageResourceName = nameof(ValidationMessages.EndpointRequired))]
public required Uri Endpoint { get; init; }

[Range(
    1,
    300,
    ErrorMessageResourceType = typeof(ValidationMessages),
    ErrorMessageResourceName = nameof(ValidationMessages.TimeoutSecondsRange))]
public double TimeoutSeconds { get; init; } = 30;
```

## Antipatrones

| Antipatrón | Corrección |
| --- | --- |
| `ErrorMessage = "El campo es obligatorio"` inline en atributos | Usar `ErrorMessageResourceType` y `ErrorMessageResourceName` |
| Mensajes de logs en inglés | Escribir los mensajes de logs en castellano |
| Textos de usuario hardcodeados en el código | Mover a archivos `.resx` |
| Identificadores de código en castellano | Mantener siempre en inglés |
| Mezclar idiomas en el mismo propósito | Ser consistente: castellano para textos legibles, inglés para código |
