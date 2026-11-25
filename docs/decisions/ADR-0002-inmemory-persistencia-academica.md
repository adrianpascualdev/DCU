# ADR-0002: EF Core InMemory como persistencia académica

- **Estado**: Aceptado — excepción académica ratificada
- **Fecha**: 2026-11-11 (propuesta)
- **Ratificación**: 2026-11-25 — aprobada por equipo G1
- **Decisores**: equipo G1
- **Alcance**: persistencia de G1 durante desarrollo, demostración y pruebas
- **Relacionado**: `ADR-0001-servidor-oauth-propio-identity-openiddict.md`, `specs/001-g1-identidad-contrato-mock/spec.md`

## 1. Contexto

El usuario ha solicitado **Entity Framework Core InMemory** como persistencia académica. La constitución exige, en su restricción técnica 4, persistencia con EF Core compatible con .NET 10, **migraciones versionadas y revisables**, y seeding idempotente. El principio V exige integridad de datos, coherencia y recuperación antes de usar datos reales.

EF Core InMemory no es una base de datos relacional. No soporta migraciones, restricciones de clave externa, transacciones, concurrencia optimista real, copias de seguridad ni recuperación. Los datos se pierden al reiniciar el proceso. Por tanto, no puede cumplir por sí solo la restricción técnica 4 ni el principio V.

La decisión no puede resolverse implícitamente. Necesita una excepción documentada, con alcance, riesgo, alternativa y fecha de revisión, tal como exige la gobernanza de la constitución.

## 2. Decisión

Se usará **EF Core InMemory 10.0.15** como persistencia **exclusiva de desarrollo, demostración y pruebas** durante el alcance académico, con estas condiciones obligatorias:

1. **Nunca se usará con datos reales ni en producción.**
2. El contexto se creará con `EnsureCreated()`; no se generarán migraciones para InMemory.
3. Se llamará a `DisableBulkOperations()` en la integración de OpenIddict con EF Core, porque InMemory no soporta `ExecuteUpdate` ni `ExecuteDelete`.
4. El modelo de dominio y el `DbContext` serán agnósticos del proveedor: sin SQL específico, sin funciones de base de datos y sin depender de restricciones relacionales para reglas críticas.
5. Las reglas de integridad que la base de datos no puede garantizar se validarán en el dominio y se probarán explícitamente.
6. El seeding será idempotente, con datos ficticios, sin secretos ni datos personales reales.
7. La documentación y la UI de desarrollo advertirán de que los datos son volátiles y se pierden al reiniciar.
8. Antes de usar datos reales se migrará a un proveedor relacional (SQLite, SQL Server o PostgreSQL) con migraciones versionadas.

La excepción se limita al alcance académico y a la fase A/B. No se extiende a producción ni a datos de personas reales.

## 3. Motivaciones

- El usuario lo ha solicitado explícitamente para la persistencia académica.
- Elimina la dependencia de un servidor de base de datos en el entorno de desarrollo y en la demostración.
- Permite arrancar y probar el contrato y el mock sin infraestructura adicional.
- Facilita pruebas deterministas y reinicios limpios.
- Mantiene el foco en el contrato, la seguridad y los flujos de identidad, no en la operación de una base de datos.

## 4. Alternativas consideradas

| Alternativa | Ventajas | Inconvenientes | Valoración |
| --- | --- | --- | --- |
| InMemory con excepción formal | Zero instalación, rápido, cumple el requisito académico | Sin migraciones, sin restricciones, sin transacciones, volátil | **Elegida para el alcance académico** |
| SQLite en archivo | Relacional, soporta migraciones, zero instalación, portable | No es InMemory; requiere decidir el archivo y su ciclo de vida | Alternativa recomendada si se exige cumplir la restricción 4 |
| SQL Server LocalDB o contenedor | Cercano a producción, migraciones, restricciones | Requiere instalación o Docker; más peso en el entorno | Alternativa para la fase de integración |
| PostgreSQL en contenedor | Relacional, multiplataforma, migraciones | Requiere Docker; más operación | Alternativa para producción |
| Mantener InMemory y fingir migraciones | Ninguna | Incumple la constitución y oculta riesgos | Rechazada |

## 5. Evidencia

El spike aislado en `/tmp/opencode/g1-compat` confirmó:

- El modelo de EF Core InMemory se crea con las entidades de Identity y OpenIddict.
- Identity crea roles y usuarios, y asigna roles.
- OpenIddict crea aplicaciones, autorizaciones y tokens.
- La revocación por sujeto funciona y deja el token en estado `revoked`.
- El borrado de una aplicación funciona.

Sin `DisableBulkOperations()`, la revocación falla con:

```text
System.InvalidOperationException: The methods 'ExecuteUpdate' and 'ExecuteUpdateAsync'
are not supported by the current database provider. Please contact the publisher of the
database provider for more information.
```

Con `DisableBulkOperations()`, la revocación y el borrado funcionan. Esta opción es obligatoria y debe aparecer en la configuración del `AddCore` de OpenIddict.

## 6. Consecuencias

### 6.1 Positivas

- Entorno de desarrollo y demostración sin base de datos externa.
- Arranque rápido y pruebas deterministas.
- Reinicio limpio de fixtures y sesiones.
- Menos configuración para el alcance académico.

### 6.2 Negativas

- **Pérdida total de datos al reiniciar**; no hay recuperación.
- **Sin migraciones**: no se puede demostrar el requisito de migraciones versionadas de la constitución.
- **Sin restricciones relacionales**: claves externas, unicidad y borrados en cascada no se aplican.
- **Sin transacciones**: las operaciones compuestas pueden quedar parcialmente aplicadas.
- **Sin concurrencia real**: no hay tokens de concurrencia ni bloqueos de fila.
- **Sin copias de seguridad**: no se puede cumplir el principio V antes de datos reales.
- **Operaciones masivas no soportadas**: OpenIddict necesita `DisableBulkOperations()`.
- **Consultas no representativas**: el rendimiento y los planes de consulta no se parecen a los de una base real.
- **Pruebas engañosas**: una prueba verde en InMemory puede fallar en SQL Server o PostgreSQL.
- **Claves de Data Protection volátiles**: si no se persisten, las cookies y la protección de tokens se invalidan al reiniciar.

### 6.3 Neutras

- El contrato y el modelo de dominio no dependen del proveedor.
- La sustitución por SQLite o SQL Server requiere un `DbContext` compatible y migraciones nuevas.
- El seeding sigue siendo necesario en todos los proveedores.

## 7. Mitigaciones obligatorias

- `DisableBulkOperations()` en OpenIddict.
- `EnsureCreated()` en desarrollo y pruebas; ninguna migración de InMemory.
- Modelo y consultas agnósticos del proveedor.
- Validación de unicidad e integridad en el dominio, no solo en la base de datos.
- Pruebas de integración con un proveedor relacional en CI, como mínimo SQLite o SQL Server en contenedor, para detectar diferencias.
- Persistencia del key ring de Data Protection en desarrollo si se quieren conservar sesiones entre reinicios.
- Aviso visible en README, Swagger y logs de arranque: “datos en memoria, se pierden al reiniciar”.
- Prohibición de datos reales, secretos y datos personales en fixtures.
- Plan de migración documentado antes de la fase de producción.

## 8. Conflicto con la constitución

La restricción técnica 4 exige migraciones versionadas y revisables. InMemory no las soporta. El principio V exige integridad y recuperación antes de datos reales. InMemory no las garantiza.

Este ADR **no enmienda la constitución**. Registra una excepción académica y exige ratificación explícita. Si el responsable del proyecto no la aprueba, la alternativa es cambiar a SQLite o SQL Server/PostgreSQL antes de implementar la fase B. La excepción no autoriza a usar InMemory con datos reales ni a declarar que se cumplen las migraciones.

## 9. Condiciones de ratificación

1. El responsable del proyecto aprueba la excepción y su fecha de revisión.
2. La especificación `001-g1-identidad-contrato-mock` se aprueba con esta limitación visible.
3. Se acepta que la fase B no cumplirá la restricción de migraciones mientras use InMemory.
4. Se define el proveedor relacional de destino y el plan de migración antes de usar datos reales.
5. Se añade una prueba de CI contra un proveedor relacional antes de la integración real.
6. Se documenta la pérdida de datos y la ausencia de copias de seguridad en README y Swagger.

Si no se ratifica, se sustituye InMemory por SQLite en archivo o por SQL Server/PostgreSQL, se generan migraciones y se actualiza este ADR como reemplazado.

### Ratificación registrada (2026-11-25)

El **equipo G1** aprobó la excepción académica el **2026-11-25**, siguiendo las indicaciones del profesor. No se han inventado nombres, firmas ni aprobaciones adicionales. La ratificación autoriza únicamente el alcance descrito en la sección 2: desarrollo, demostración y pruebas de las fases A/B, sin datos reales ni producción.

Sigue pendiente, y se registra como trabajo futuro, la prueba de CI con un proveedor relacional (T055, responsabilidad de DEV3). La elección del proveedor relacional definitivo y el plan de migración antes de datos reales permanecen abiertos en D-009. Esta ratificación no modifica `.specify/memory/constitution.md` ni declara cumplida la restricción técnica 4.

## 10. Disparadores de revisión

- Antes de usar cualquier dato real.
- Antes de desplegar en un entorno compartido o de producción.
- Si una funcionalidad requiere transacciones, restricciones o concurrencia.
- Si OpenIddict introduce una operación que InMemory no soporta incluso con `DisableBulkOperations()`.
- Si el equipo decide cumplir estrictamente la restricción técnica 4.
- Si se adopta multi-tenancy o un volumen de datos que InMemory no pueda sostener.

## 11. Referencias

- [EF Core In-Memory Database Provider](https://learn.microsoft.com/ef/core/providers/in-memory/)
- [EF Core testing with the in-memory provider](https://learn.microsoft.com/ef/core/testing/testing-without-the-database)
- [OpenIddict Entity Framework Core integration](https://documentation.openiddict.com/integrations/entity-framework-core)
- [ASP.NET Core Data Protection](https://learn.microsoft.com/aspnet/core/security/data-protection/introduction)
- `specs/001-g1-identidad-contrato-mock/spec.md`
- `docs/decisions/ADR-0001-servidor-oauth-propio-identity-openiddict.md`
