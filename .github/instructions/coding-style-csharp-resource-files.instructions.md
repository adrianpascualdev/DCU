---
applyTo: "**/Resources/**/*.resx, **/Resources/**/*.Designer.cs, **/*.csproj"
description: "Directrices para organizar archivos de recursos (.resx), sus archivos generados (.Designer.cs) y su configuración en proyectos .NET."
---

# Directrices de archivos de recursos en `C#`

Directrices para organizar archivos de recursos (`.resx`), sus archivos generados (`.Designer.cs`) y su configuración en proyectos .NET.

## Contexto del proyecto

- **Objetivo**: mantener una ubicación, nomenclatura y vinculación consistentes para los recursos localizables del proyecto
- **Artefactos cubiertos**: archivos `.resx`, archivos `.Designer.cs` asociados y metadatos relacionados en `.csproj`
- **Combinar con instrucciones**:
  - [Directrices de idioma y localización](/.github/instructions/coding-style-csharp-localization.instructions.md) para decidir qué textos deben vivir en recursos
  - [Directrices de configuración de paquetes y proyectos](/.github/instructions/coding-style-csharp-nuget-packaging.instructions.md) cuando aplique a proyectos empaquetables

## Ubicación obligatoria

- Los archivos de recursos deben ubicarse en una carpeta `Resources/` dentro del proyecto que los consume
- No distribuir archivos `.resx` en carpetas arbitrarias salvo una necesidad explícita y justificada
- El archivo `.Designer.cs` asociado debe permanecer junto al `.resx` correspondiente dentro de `Resources/`

## Nomenclatura

- Usar `Name.resx` para el recurso neutro
- Usar `Name.es.resx`, `Name.en.resx` u otras variantes de cultura cuando existan traducciones
- Usar `Name.Designer.cs` para el archivo generado asociado
- Mantener el mismo nombre base en todos los archivos relacionados

## Estructura esperada

```text
Project/
├── Resources/
│   ├── Exceptions.resx
│   ├── Exceptions.es.resx
│   ├── Exceptions.en.resx
│   └── Exceptions.Designer.cs
```

## Regla para archivos `.Designer.cs`

- Los archivos `.Designer.cs` de recursos se consideran generados
- No deben editarse manualmente salvo un caso excepcional y justificado
- No deben moverse ni renombrarse de forma independiente respecto al `.resx`
- No deben contener lógica manual añadida por el desarrollador

## Configuración del proyecto `.csproj`

Cuando el proyecto declare recursos con generación de diseñador, seguir este patrón:

```xml
<ItemGroup>
  <Compile Update="Resources\Exceptions.Designer.cs">
    <DesignTime>True</DesignTime>
    <AutoGen>True</AutoGen>
    <DependentUpon>Exceptions.resx</DependentUpon>
  </Compile>
</ItemGroup>

<ItemGroup>
  <EmbeddedResource Update="Resources\Exceptions.resx">
    <Generator>ResXFileCodeGenerator</Generator>
    <LastGenOutput>Exceptions.Designer.cs</LastGenOutput>
  </EmbeddedResource>
</ItemGroup>
```

## Reglas de consistencia

- Todo `.Designer.cs` debe tener un `.resx` base asociado
- Todo `.resx` principal con generación de diseñador debe reflejarse correctamente en el `.csproj`
- Las variantes de cultura deben compartir el mismo nombre base que el recurso neutro
- La carpeta `Resources/` debe ser la referencia por defecto para nuevos recursos del proyecto

## Antipatrones

| Antipatrón | Corrección |
| --- | --- |
| Crear `Messages.resx` fuera de `Resources/` | Ubicarlo en `Resources/Messages.resx` |
| Editar manualmente `Messages.Designer.cs` | Regenerarlo desde el recurso o mantenerlo generado |
| Tener `Messages.en.resx` sin `Messages.resx` base | Crear el recurso neutro correspondiente |
| Renombrar `Messages.resx` sin actualizar el `.Designer.cs` y el `.csproj` | Mantener los tres alineados |
| Dejar un `.Designer.cs` sin `DependentUpon` | Declarar la dependencia en el `.csproj` |
