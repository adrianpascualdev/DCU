# ADR-0001: Servidor OAuth 2.0/OIDC propio con ASP.NET Core Identity y OpenIddict

- **Estado**: Propuesto — pendiente de ratificación
- **Fecha**: 2026-11-11
- **Decisores**: responsable del proyecto y equipo G1
- **Alcance**: identidad, autenticación, sesiones, roles y permisos de G1
- **Relacionado**: `specs/001-g1-identidad-contrato-mock/spec.md`, `ADR-0002-inmemory-persistencia-academica.md`, `docs/decisions/pending-decisions.md`

## 1. Contexto

El club necesita una identidad única para todos los módulos. G1 es el propietario de la autenticación, las sesiones, los roles y la matriz base de permisos; G2, G3 y G4 solo consumen esa identidad por contrato y no pueden implementar login, usuarios ni autorización propia.

El usuario ha descartado explícitamente proveedores de identidad externos como Google, Microsoft, GitHub o Auth0. También ha descartado implementar criptografía manual o un servidor OAuth casero. La solución debe basarse en bibliotecas mantenidas y mecanismos estándar.

La primera entrega es un contrato OpenAPI y un mock de identidad para desbloquear a G2, G3 y G4. El servidor real se implementará en una fase posterior, pero la decisión de arquitectura debe tomarse ahora porque condiciona el contrato, el modelo de datos, las sesiones y la revocación.

## 2. Decisión

Se construirá un **servidor de autorización OAuth 2.0 y OpenID Connect propio** con:

- **ASP.NET Core Identity** para usuarios, contraseñas, roles, bloqueo, verificación y segundo factor.
- **OpenIddict 7.8.0** para el servidor OAuth 2.0/OIDC, la emisión y validación de tokens, el descubrimiento, JWKS, autorización, token, userinfo, revocación y logout.
- **ASP.NET Core Minimal APIs** sobre **.NET 10.0** y **C# 14**.
- **Entity Framework Core InMemory** para la persistencia académica, con la excepción documentada en el ADR-0002.
- **OpenAPI 3.1 + Swagger UI** para documentar y probar el contrato.
- **React + TypeScript** para el cliente web, con build y despliegue independientes.
- **xUnit** para pruebas de backend y **Playwright** para pruebas de navegador.

El flujo de autenticación será **Authorization Code con PKCE S256**. Se prohíben el implicit flow, el password grant, el client credentials para identidad de usuario y cualquier emisión de tokens fuera de los endpoints estándar.

OpenID Connect se adopta sobre OAuth 2.0 porque el cliente necesita un `id_token` verificable, descubrimiento, userinfo y logout de sesión. OAuth 2.0 solo no cubre la identidad del usuario de forma interoperable.

## 3. Motivaciones

- **Control y propiedad**: el club es dueño de su identidad y no depende de un proveedor externo.
- **Alineación con .NET**: Identity y OpenIddict son bibliotecas .NET mantenidas, con integración directa con ASP.NET Core y EF Core.
- **Estándares**: Authorization Code + PKCE, OIDC, JWT, JWKS, RFC 7009 y RFC 7662 son mecanismos probados; no se inventa protocolo ni criptografía.
- **Requisito académico**: el proyecto debe demostrar un servidor OAuth propio y el uso de Identity; un proveedor externo no cumple el objetivo.
- **Modularidad**: los módulos pueden validar tokens localmente con JWKS o consultar introspección, sin compartir almacenes ni reglas de identidad.
- **Trazabilidad**: las sesiones y la revocación se pueden auditar y explicar, algo que un proveedor externo dificultaría en el contexto del proyecto.

## 4. Alternativas consideradas

| Alternativa | Motivo de descarte |
| --- | --- |
| Proveedor externo (Google, Microsoft, GitHub, Auth0, Entra) | Prohibido por el usuario; el club perdería control y no se cumpliría el objetivo de servidor propio |
| Servidor OAuth casero | Prohibido; riesgo alto de errores criptográficos y de protocolo |
| Duende IdentityServer | Resuelve el problema, pero añade condiciones de licencia y no es necesario para el alcance académico |
| Keycloak | Servicio externo en Java, con su propio modelo de usuarios; no usa ASP.NET Core Identity y añade complejidad operativa |
| ASP.NET Core Identity solo, sin OpenIddict | Identity no es un servidor OAuth/OIDC; habría que construir el protocolo a mano, lo que está prohibido |
| OpenIddict 8.0 preview | No es una versión estable; no se adopta para el alcance actual |

## 5. Versiones verificadas

La verificación se realizó el 2026-11-11 contra NuGet y con un spike aislado en `/tmp/opencode/g1-compat`. El spike no forma parte del repositorio ni del entregable.

| Componente | Versión | Verificación |
| --- | --- | --- |
| .NET SDK | 10.0.404 | `dotnet --version` |
| C# | 14 | TFM `net10.0` |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.15 | NuGet y compilación |
| `Microsoft.EntityFrameworkCore.InMemory` | 10.0.15 | NuGet y ejecución |
| `OpenIddict.AspNetCore` | 7.8.0 | NuGet y ejecución |
| `OpenIddict.EntityFrameworkCore` | 7.8.0 | NuGet y ejecución |
| `Microsoft.AspNetCore.OpenApi` | 10.0.15 | NuGet; OpenAPI 3.1 por defecto en .NET 10 |
| `Swashbuckle.AspNetCore.SwaggerUI` | 10.3.1 | NuGet; soporte `net10.0` |
| `xunit.v3` | 4.0.3 | NuGet; soporte .NET 8 o superior |
| `Microsoft.Playwright` | 1.64.1 | NuGet; versión actual a fijar en el plan |

OpenIddict 8.0 solo está disponible como preview en la fecha de la decisión. Se usa 7.8.0, la última versión estable. `OpenIddict.EntityFrameworkCore` 7.8.0 declara una dependencia de `Microsoft.EntityFrameworkCore.Relational` 10.0.14; NuGet la resolvió con Identity e InMemory 10.0.15 y el spike compiló y ejecutó correctamente.

## 6. Evidencia del spike

El spike verificó, con un `DbContext` que hereda de `IdentityDbContext` y usa `UseInMemoryDatabase` + `UseOpenIddict()`:

- Creación del modelo de EF Core InMemory con las entidades de Identity y OpenIddict.
- Creación de un rol y asignación a un usuario de Identity.
- Creación de una aplicación OpenIddict, una autorización y un token.
- Revocación por sujeto: el token pasa a estado `revoked`.
- Borrado de una aplicación OpenIddict y comprobación de que ya no se encuentra.

El hallazgo crítico es que **OpenIddict con InMemory requiere `DisableBulkOperations()`**. Sin esa opción, la revocación lanza:

```text
System.InvalidOperationException: The methods 'ExecuteUpdate' and 'ExecuteUpdateAsync'
are not supported by the current database provider.
```

Con `DisableBulkOperations()` la revocación y el borrado funcionan. Esta condición es obligatoria en la implementación y está documentada en el ADR-0002.

No se ha verificado todavía el flujo HTTP completo de Authorization Code + PKCE, la firma y validación real de JWT, el logout OIDC ni la rotación de refresco. Esas pruebas pertenecen a la fase B y son una condición previa a su aceptación.

## 7. Consecuencias

### 7.1 Positivas

- Identidad propia, sin dependencia de proveedores externos.
- Uso de bibliotecas mantenidas y estándares abiertos.
- Integración natural con ASP.NET Core, Identity y EF Core.
- Control total sobre sesiones, revocación, auditoría y matriz de permisos.
- Posibilidad de validar tokens localmente o por introspección según el riesgo del módulo.
- El contrato y el mock pueden evolucionar hacia el servidor real sin cambiar a los consumidores.

### 7.2 Negativas

- El equipo asume la seguridad del servidor de autorización: claves, cookies, CSRF, CORS, rate limiting, auditoría y actualizaciones.
- Hay que gestionar claves de firma y cifrado, y persistir el key ring de Data Protection en producción.
- OpenIddict 8.0 aún no es estable; la migración futura requiere revisión.
- InMemory no sirve para producción y obliga a una excepción constitucional y a un plan de migración.
- La implementación real es más costosa que delegar en un proveedor externo.
- La ventana de revocación de un JWT stateless debe diseñarse y probarse; no es inmediata por defecto.

### 7.3 Neutras

- El cliente React necesitará una biblioteca OIDC estándar o un BFF; la decisión D-001 sigue abierta.
- El catálogo de roles y la matriz de permisos siguen pendientes (D-002).
- La duración de tokens y la política de refresco siguen pendientes (D-004).

## 8. Condiciones de aceptación

La decisión se considera ratificada solo cuando:

1. El responsable del proyecto aprueba este ADR y el ADR-0002.
2. Se aprueba la especificación `001-g1-identidad-contrato-mock`.
3. Se resuelven las decisiones D-001 a D-005 antes de implementar la fase B.
4. Se completa una prueba HTTP real de Authorization Code + PKCE, firma JWKS, userinfo, logout y revocación.
5. Se define el modelo de amenazas y la gestión de claves de producción.
6. Se revisa la seguridad antes de usar datos reales.

Si no se ratifica el uso de InMemory, la alternativa recomendada es SQLite o SQL Server/PostgreSQL antes de empezar la fase B, manteniendo el mismo contrato y el mismo modelo de dominio.

## 9. Cumplimiento de la constitución

| Principio o restricción | Cumplimiento |
| --- | --- |
| IV. Seguridad, privacidad e identidad única | Identity, OpenIddict, PKCE, tokens asimétricos, sesiones revocables, denegación por defecto y auditoría |
| III. Desarrollo guiado por contratos | OpenAPI 3.1 y Swagger UI antes de implementar consumidores |
| VIII. Simplicidad y mantenimiento | Bibliotecas estándar, sin criptografía manual; complejidad justificada |
| IX. Requisitos confirmados frente a pendientes | Decisiones D-001 a D-015 registradas y no resueltas implícitamente |
| X. Integración progresiva con datos simulados | Mock fiel, sustituible y sin atajos de autorización |
| Restricción técnica 2: .NET 10 y Minimal APIs | TFM `net10.0`, C# 14 y Minimal APIs |
| Restricción técnica 3: OpenAPI + Swagger UI | `Microsoft.AspNetCore.OpenApi` + `Swashbuckle.AspNetCore.SwaggerUI` |
| Restricción técnica 4: migraciones | Conflicto con InMemory; se resuelve mediante el ADR-0002 y requiere excepción formal |
| Restricción técnica 7: xUnit y Playwright | xUnit v3 y Playwright |

La skill interna del repositorio prefiere Scalar sobre Swagger UI, pero la constitución y el usuario exigen Swagger UI. Prevalece la constitución. La generación del documento se hace con `Microsoft.AspNetCore.OpenApi`; Swashbuckle solo aporta la UI, no la generación.

## 10. Referencias

- [OpenIddict documentation](https://documentation.openiddict.com/)
- [OpenIddict Entity Framework Core integration](https://documentation.openiddict.com/integrations/entity-framework-core)
- [OpenIddict Proof Key for Code Exchange](https://documentation.openiddict.com/configuration/proof-key-for-code-exchange)
- [ASP.NET Core Identity documentation](https://learn.microsoft.com/aspnet/core/security/authentication/identity)
- [ASP.NET Core OpenAPI in .NET 10](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0)
- [RFC 6749: The OAuth 2.0 Authorization Framework](https://www.rfc-editor.org/rfc/rfc6749)
- [RFC 7636: Proof Key for Code Exchange](https://www.rfc-editor.org/rfc/rfc7636)
- [RFC 7009: OAuth 2.0 Token Revocation](https://www.rfc-editor.org/rfc/rfc7009)
- [RFC 7662: OAuth 2.0 Token Introspection](https://www.rfc-editor.org/rfc/rfc7662)
- [RFC 8414: OAuth 2.0 Authorization Server Metadata](https://www.rfc-editor.org/rfc/rfc8414)
- [RFC 9457: Problem Details for HTTP APIs](https://www.rfc-editor.org/rfc/rfc9457)
- [OpenID Connect Core 1.0](https://openid.net/specs/openid-connect-core-1_0.html)
