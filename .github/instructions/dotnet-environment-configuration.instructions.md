---
applyTo: "**/appsettings*.json"
description: "Patron de configuracion por entornos en proyectos ASP.NET Core. Aplica al crear o modificar appsettings."
---

# Configuracion por entornos en ASP.NET Core

## Patron

- **`appsettings.json`** -> valores base para PRE-PROD/produccion.
- **`appsettings.{Entorno}.json`** -> solo las claves que difieren. No repetir toda la configuracion.
- `ASPNETCORE_ENVIRONMENT=Development` activa el archivo de desarrollo. Visual Studio y VS Code lo inyectan automaticamente.

## Reglas

1. **Nunca comentar ni duplicar** claves en `appsettings.json` para alternar entornos.
2. Los secretos (contrasenas, ClientSecret, etc.) **no deben commitearse**. Usar [Secret Manager](https://learn.microsoft.com/aspnet/core/security/app-secrets) o variables de entorno.
3. Cada nueva clave que varie por entorno -> valor de produccion en `appsettings.json`, valor local en `appsettings.Development.json`.