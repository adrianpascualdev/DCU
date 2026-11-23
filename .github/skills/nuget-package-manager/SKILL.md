---
name: nuget-package-manager
description: "Gestiona paquetes NuGet en proyectos y soluciones .NET. Usa esta skill cuando necesites agregar, quitar o actualizar versiones de paquetes NuGet. Obliga a usar la CLI `dotnet` para la gestión de paquetes y define procedimientos estrictos para editar archivos directamente solo al actualizar versiones."
---

# Gestor de NuGet

## Descripción general

Esta skill garantiza una gestión coherente y segura de paquetes NuGet en proyectos .NET. Prioriza el uso de la CLI `dotnet` para mantener la integridad del proyecto y aplica un flujo estricto de verificación y restauración al actualizar versiones.

## Requisitos previos

- SDK de .NET instalado, normalmente .NET 8.0 o posterior, o una versión compatible con la solución de destino. Revisa el `global.json` de la solución si existe.
- CLI `dotnet` disponible en tu `PATH`.
- `jq` como procesador JSON o PowerShell para verificar versiones con `dotnet package search`.

## Reglas principales

1. **NUNCA** edites directamente archivos `.csproj`, `.props` o `Directory.Packages.props` para **agregar** o **quitar** paquetes. Usa siempre los comandos `dotnet add package` y `dotnet remove package`.
2. La **EDICIÓN DIRECTA** solo está permitida para **cambiar versiones** de paquetes existentes.
3. Las **ACTUALIZACIONES DE VERSIÓN** deben seguir este flujo obligatorio:
   - Verifica que la versión objetivo exista en NuGet.
   - Determina si las versiones se gestionan por proyecto en `.csproj` o de forma centralizada en `Directory.Packages.props`.
   - Actualiza la cadena de versión en el archivo correspondiente.
   - Ejecuta `dotnet restore` inmediatamente para verificar la compatibilidad.

## Flujos de trabajo

### Agregar un paquete

Usa `dotnet add [<PROJECT>] package <PACKAGE_NAME> [--version <VERSION>]`.
Ejemplo: `dotnet add src/MyProject/MyProject.csproj package Newtonsoft.Json`

### Quitar un paquete

Usa `dotnet remove [<PROJECT>] package <PACKAGE_NAME>`.
Ejemplo: `dotnet remove src/MyProject/MyProject.csproj package Newtonsoft.Json`

### Actualizar versiones de paquetes

Cuando actualices una versión, sigue estos pasos:

1. **Verifica que la versión exista**: Comprueba si la versión existe usando `dotnet package search` con coincidencia exacta y salida JSON.

   - Con `jq`:
     `dotnet package search <PACKAGE_NAME> --exact-match --format json | jq -e '.searchResult[].packages[] | select(.version == "<VERSION>")'`

   - Con PowerShell:
     `(dotnet package search <PACKAGE_NAME> --exact-match --format json | ConvertFrom-Json).searchResult.packages | Where-Object { $_.version -eq "<VERSION>" }`

2. **Determina cómo se gestiona la versión**:

   - Busca `Directory.Packages.props` en la raíz de la solución. Si existe, las versiones deben gestionarse ahí mediante `<PackageVersion Include="Package.Name" Version="1.2.3" />`.

   - Si no existe, revisa los `.csproj` individuales para localizar `<PackageReference Include="Package.Name" Version="1.2.3" />`.

3. **Aplica los cambios**: Modifica el archivo identificado con la nueva versión.

4. **Verifica la estabilidad**: Ejecuta `dotnet restore` sobre el proyecto o la solución. Si hay errores, revierte el cambio e investiga la causa.

## Ejemplos

### Usuario: "Add Serilog to the WebApi project"

**Acción**: Ejecuta `dotnet add src/WebApi/WebApi.csproj package Serilog`.

### Usuario: "Remove Newtonsoft.Json from the WebApi project"

**Acción**: Ejecuta `dotnet remove src/WebApi/WebApi.csproj package Newtonsoft.Json`.

### Usuario: "Update CommunityToolkit.Diagnostics to 8.4.0 in the whole solution"

**Acción**:

1. Verifica que exista la versión 8.4.0 con `dotnet package search CommunityToolkit.Diagnostics --exact-match --format json` y confirma en la salida que aparece `8.4.0`.
2. Encuentra dónde se define, por ejemplo en `Directory.Packages.props`.
3. Edita el archivo para actualizar la versión.
4. Ejecuta `dotnet restore`.
