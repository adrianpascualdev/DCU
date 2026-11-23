---
applyTo: "**/*.csproj"
description: "Directrices para configurar proyectos .csproj que generan paquetes NuGet, asegurando que la documentación XML, el README y los metadatos se empaquetan correctamente."
---

# Directrices de empaquetado NuGet

Directrices para garantizar que los proyectos que generan paquetes NuGet incluyan correctamente la documentación XML, metadatos y ficheros complementarios.

## Contexto del proyecto

- **Propósito**: asegurar que cada paquete NuGet publicado incluya documentación XML, README y metadatos completos para que IntelliSense, GitHub Copilot y otros LLMs puedan entender la API pública del paquete.
- **Ámbito**: todo proyecto `.csproj` con `<GeneratePackageOnBuild>True</GeneratePackageOnBuild>`.
- **Regla de aclaración**: aplicar también `/.github/instructions/wow-follow-up-question.instructions.md` cuando no se tenga al menos un 95% de confianza sobre los cambios a realizar.

## Reglas obligatorias

### Documentación XML

Todo proyecto que genere un paquete NuGet **DEBE** incluir ambas propiedades en el `<PropertyGroup>`:

```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<DocumentationFile>bin\$(Configuration)\$(TargetFramework)\$(AssemblyName).xml</DocumentationFile>
```

- `GenerateDocumentationFile` garantiza la generación del fichero XML de documentación para cada ensamblado.
- `DocumentationFile` fija una ruta de salida predecible y evita configuraciones implícitas difíciles de revisar.

### README del paquete

Todo paquete NuGet **DEBE** definir un README visible para el consumidor:

```xml
<PackageReadmeFile>README.md</PackageReadmeFile>
```

Y debe empaquetarlo explícitamente en un `ItemGroup`:

```xml
<None Include="README.md" Pack="true" PackagePath="\" />
```

### Skills empaquetadas para GitHub Copilot

Todo proyecto de biblioteca que genere una `.dll` y publique un paquete NuGet **DEBE** incluir también la estructura necesaria para distribuir *skills* de GitHub Copilot junto con el paquete.

#### Estructura de carpetas obligatoria

- `Skills` en la raíz del proyecto. Debe contener la estructura de las *skills* que el desarrollador quiera distribuir, incluyendo sus archivos `SKILL.md` en Markdown y los recursos auxiliares que correspondan. Para la estructura interna y la redacción de cada *skill*, aplicar también `/.github/instructions/creating-agent-skills.instructions.md`.
- `Build` en la raíz del proyecto. Debe contener un archivo `.targets` que copie de forma recursiva todo el contenido de `Skills` a `.github\skills\` en la raíz del proyecto o solución consumidora cuando la referencia al paquete sea directa.
- `BuildTransitive` en la raíz del proyecto. Debe contener un archivo `.targets` que copie de forma recursiva todo el contenido de `Skills` a `.github\skills\` en la raíz del proyecto o solución consumidora cuando la dependencia llegue de forma transitiva.

La carpeta `Skills` debe viajar dentro del paquete NuGet para que ambos archivos `.targets` puedan localizarla desde la caché local del paquete.

#### Requisitos de los archivos `.targets`

Ambos archivos `.targets` **DEBEN** ser funcionalmente equivalentes. La diferencia entre ellos debe limitarse a la carpeta en la que se empaquetan, `Build` o `BuildTransitive`, y por tanto al modo en que NuGet los aplica al proyecto consumidor.

- Cada archivo **DEBE** declarar `InitialTargets` apuntando a un `Target` propio que ejecute la copia al inicio de cualquier invocación de MSBuild en la que el paquete esté importado.
- Cada archivo **DEBE** definir en un `PropertyGroup` inicial una ruta origen hacia `$(MSBuildThisFileDirectory)..\Skills\`.
- Cada archivo **DEBE** definir una propiedad pública de opt-out para que el consumidor pueda desactivar la copia estableciendo `true`, inicializándola a `false` cuando no venga informada.
- El `Target` principal **DEBE** ejecutarse solo cuando la copia no esté deshabilitada y la carpeta origen exista.
- La resolución de la raíz de destino **DEBE** seguir este orden: `$(SolutionDir)` cuando exista y no sea `*Undefined*`, búsqueda ascendente de `.gitignore`, búsqueda ascendente de `.gitattributes`, búsqueda ascendente de `.editorconfig` y `$(MSBuildProjectDirectory)` como último recurso.
- El directorio de destino **DEBE** ser `.github\skills\` bajo la raíz detectada.
- El `Target` **DEBE** recopilar todos los archivos de `Skills` mediante un patrón recursivo equivalente a `**\*.*`.
- El `Target` **DEBE** crear la carpeta de destino si no existe mediante `MakeDir`.
- El `Target` **DEBE** copiar los archivos preservando la estructura relativa de carpetas y usando `SkipUnchangedFiles="true"`.
- El `Target` **DEBE** emitir al menos un mensaje final de confirmación tras la copia. Puede emitir además mensajes diagnósticos intermedios, como la raíz detectada.
- Los nombres del `Target`, de las propiedades privadas y de los elementos internos **DEBEN** usar un prefijo específico del paquete para evitar colisiones con otros paquetes que también inyecten lógica MSBuild.
- El nombre del archivo `.targets` **DEBE** coincidir exactamente con el `PackageId` del paquete para que NuGet lo importe automáticamente.

#### Empaquetado de los archivos `.targets`

El `.csproj` **DEBE** empaquetar también el contenido de `Skills` dentro de una carpeta `Skills\` del propio paquete, preservando su jerarquía, porque ambos archivos `.targets` lo resuelven mediante `$(MSBuildThisFileDirectory)..\Skills\`.

El `.csproj` **DEBE** incluir, como mínimo, las entradas siguientes para empaquetar ambos archivos `.targets`:

```xml
<ItemGroup>
  <!-- Para referencias directas al proyecto -->
  <None Include="Build\$(PackageId).targets" Pack="true" PackagePath="Build\" />
  <!-- Para referencias transitivas al paquete -->
  <None Include="BuildTransitive\$(PackageId).targets" Pack="true" PackagePath="BuildTransitive\" />
</ItemGroup>
```

Si el proyecto no usa `$(PackageId)` en estas rutas, debe escribir el nombre efectivo del paquete y mantener la coincidencia exacta con los nombres de ambos archivos `.targets`.

### Metadatos mínimos del paquete

Todo `.csproj` que genere un paquete NuGet **DEBE** incluir, como mínimo, estos metadatos:

- `<Authors>`
- `<Company>`
- `<Description>`
- `<PackageTags>`
- `<RepositoryUrl>`
- `<RepositoryType>`
- `<Version>`

Si el proyecto publica símbolos de depuración, también debe incluir:

```xml
<IncludeSymbols>true</IncludeSymbols>
<SymbolPackageFormat>snupkg</SymbolPackageFormat>
```

### Relación con la documentación XML del código

La configuración del `.csproj` no sustituye a la documentación del código fuente:

- Las interfaces públicas deben documentarse completamente para que las implementaciones puedan usar `<inheritdoc />`.
- Las propiedades y métodos públicos exclusivos de una implementación o DTO deben documentarse de forma explícita.
- Las excepciones específicas de una implementación deben documentarse en el miembro concreto cuando no formen parte del contrato de la interfaz.

Para estas reglas de redacción, aplicar también `/.github/instructions/coding-style-csharp-xml-documentation.instructions.md`.

## Verificación

Antes de dar por correcto el empaquetado de un proyecto NuGet, comprobar:

1. Que el `.nupkg` contiene la `.dll` y su correspondiente `.xml` por cada target framework empaquetado.
2. Que el `.nupkg` contiene el `README.md` en la ruta esperada.
3. Que el proyecto tiene los metadatos mínimos requeridos.
4. Que no hay bloques de documentación truncados ni propiedades obligatorias omitidas en el `.csproj`.

## Antipatrones

| Antipatrón | Corrección |
| --- | --- |
| Generar paquete sin `GenerateDocumentationFile` | Añadir la propiedad de forma explícita en el `PropertyGroup` principal |
| Definir `PackageReadmeFile` sin empaquetar el archivo real | Añadir la entrada `None Include=... Pack=true` correspondiente |
| Publicar un paquete sin `PackageTags` o sin `RepositoryUrl` | Completar los metadatos mínimos antes de publicar |
| Confiar en que la configuración del `.csproj` sustituye la documentación del código | Documentar interfaces, miembros públicos y excepciones según las reglas XML |
