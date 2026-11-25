# Registro de decisiones pendientes

Este registro es la fuente única de las decisiones de identidad, tanto las pendientes como las cerradas para la fase A. Una decisión pendiente no se resuelve con un valor por defecto, una suposición o una implementación silenciosa. Mientras siga abierta, el código la trata como configurable o explícitamente no soportada, y las pruebas lo reflejan.

- **Última actualización**: 2026-11-25
- **Responsable del registro**: equipo G1
- **Ámbito**: identidad, autenticación y autorización de G1, con impacto en G2, G3 y G4

## Resumen

| ID | Decisión | Recomendación | Bloquea | Estado |
| --- | --- | --- | --- | --- |
| D-001 | BFF con cookies o SPA pública con tokens | BFF con cookies `HttpOnly` | Fase B | Abierta |
| D-002 | Catálogo de roles y matriz de permisos | Catálogo provisional v1 deny-by-default | Ratificación en fase B | Resuelta para fase A (provisional) |
| D-003 | Ámbitos de rol y propiedad | G1 emite roles; cada módulo resuelve su ámbito | Fase C (verificación con G2) | Resuelta para fase A (2026-11-24) |
| D-004 | Duración de tokens, refresco y sesión | Valores de desarrollo/pruebas aprobados; producción pendiente | Fase B (valores de producción) | Resuelta para fase A (mock) |
| D-005 | Estrategia de revocación | Sin introspección en v1; JWT corto + revocación de sesión/refresh | Producción | Parcial para fase A |
| D-006 | Segundo factor | TOTP + códigos de recuperación; obligatorio para administradores | Fase B | Abierta |
| D-007 | Registro, contraseñas, verificación y menores | Correo y teléfono obligatorios; verificación; consentimiento de tutor | Fase B | Abierta |
| D-008 | Retención y acceso a auditoría | Un año para eventos de seguridad; minimización | Fase B | Abierta |
| D-009 | Persistencia definitiva | InMemory académico; SQLite o SQL Server antes de datos reales | Producción | Excepción académica ratificada (2026-11-25); persistencia definitiva abierta |
| D-010 | Proveedores de correo y SMS | Abstracción + sumidero local; proveedor antes de fase B | Fase B | Abierta |
| D-011 | Alojamiento, TLS y claves | Contenedor Linux; claves en almacén de secretos; key ring persistente | Producción | Abierta |
| D-012 | Consentimiento de primera parte | Implícito y documentado; explícito para terceros | Fase B | Abierta |
| D-013 | Eliminación y anonimización | Soft delete + anonimización tras retención | Fase B y RGPD | Abierta |
| D-014 | Deprecación de versiones | Seis meses o duración del curso | Fase B | Abierta |
| D-015 | Multi-club o multi-tenancy | Fuera de alcance inicial | Futuro | Abierta |
| D-016 | Audiencias definitivas por API | Matriz de audiencias antes de integración; validar `aud` siempre | Fase B e integración G2/G3/G4 | Abierta |
| D-017 | Fuente de datos de contacto para G4 | G1 solo expone el contacto propio; G4 lo obtiene de G3 | Fase C (contrato G3-G4) | Resuelta para fase A (2026-11-24) |

## D-001: BFF con cookies o SPA pública con tokens

- **Contexto**: el cliente es React + TypeScript y el contrato admite cookie o Bearer. La elección cambia el transporte, el almacenamiento de tokens, CSRF, CORS y el riesgo de XSS.
- **Opciones**:
  - BFF con cookies `HttpOnly`, `Secure`, `SameSite=Lax` y antiforgery. Los tokens nunca llegan al navegador.
  - SPA pública con Authorization Code + PKCE, token de acceso en memoria y refresco rotativo. Los tokens viven en el navegador.
  - Híbrido: BFF para la web de gestión y SPA pública para la web pública.
- **Recomendación**: BFF para la web de gestión y para la PWA autenticada; la SPA pública solo para contenido abierto.
- **Impacto**: el contrato debe declarar el modo activo por despliegue; el mock admite ambos para pruebas.
- **Responsable**: G1 con el responsable de frontend.
- **Bloquea**: fase B.
- **Criterio de cierre**: ADR específico o actualización de este registro, con pruebas de CSRF, CORS, logout y expiración.

## D-002: Catálogo de roles y matriz de permisos

- **Contexto**: la constitución exige una matriz explícita por perfil, recurso y ámbito, con denegación por defecto y asignación auditada. Las fuentes usan nombres distintos para roles equivalentes.
- **Opciones**:
  - Catálogo mínimo para el alcance académico: administrador del club, gestor deportivo, entrenador, jugador, socio, tutor, personal de material y comunicaciones.
  - Catálogo completo con permisos por recurso y ámbito, incluidos patrocinadores, simpatizantes y futuros perfiles.
  - Matriz provisional en el mock y matriz definitiva antes de la fase B.
- **Recomendación**: aprobar un catálogo mínimo y una matriz deny-by-default antes de la fase B; publicar códigos estables y etiquetas en castellano.
- **Decisión adoptada (2026-11-22)**: catálogo provisional v1 con diez roles: `club_admin`, `sports_manager`, `coach`, `player`, `tutor`, `member`, `sponsor`, `supporter`, `kit_staff` y `communications_staff`. Los siete perfiles exigidos quedan cubiertos, incluidos `sponsor` y `supporter`. La matriz se limita a los permisos propiedad de G1 (`identity.users.summary.read`, `identity.users.lookup` y `identity.admin`), con denegación por defecto; los permisos de G2, G3 y G4 los publicarán sus propietarios. El catálogo se publica con `catalogVersion` y `provisional: true`.
- **Estado**: resuelta para la fase A como catálogo provisional, pendiente de validación del equipo. La ratificación definitiva corresponde a la fase B.
- **Impacto**: define los códigos de rol y permiso, los fixtures y la autorización real.
- **Responsable**: G1 con los propietarios de G2, G3 y G4.
- **Bloquea**: ratificación en fase B.
- **Criterio de cierre**: catálogo y matriz publicados en `/api/v1/identity/catalog/*`, marcados como provisionales y cubiertos por pruebas de acceso permitido y denegado.

## D-003: Ámbitos de rol y propiedad de los ámbitos

- **Contexto**: G2 es dueño de equipos y categorías; G3 es dueño de socios y cuotas; G4 es dueño de comunicaciones y equipaciones. G1 no debe duplicar reglas de negocio ni almacenar datos de otro módulo.
- **Opciones**:
  - G1 emite solo roles y permisos; cada módulo resuelve su ámbito con `userId`.
  - G1 almacena ámbitos opacos proporcionados por el módulo propietario y los incluye en el token.
  - G1 consulta al módulo propietario en cada validación.
- **Recomendación**: G1 emite roles y permisos; cada módulo resuelve su ámbito con `userId` y sus propios datos. Si un rol necesita ámbito, G1 transporta una referencia opaca sin interpretarla.
- **Decisión adoptada (2026-11-24)**: opción A. `/me` devuelve `roles` y `permissions`; G1 publica códigos estables y cada módulo resuelve sus ámbitos (equipo, categoría, colectivo) con el `userId` y sus propios datos. G1 no interpreta ni valida reglas de ámbito de otro módulo. El catálogo de roles y permisos está versionado y se amplía de forma aditiva sin romper a los consumidores.
- **Estado**: resuelta para la fase A (decisión de diseño del contrato y del mock). La verificación con un entrenador de un equipo y otro de varios equipos corresponde a la fase C.
- **Impacto**: evita dependencias circulares y duplicación de reglas; condiciona el contrato de G2.
- **Responsable**: G1 y G2.
- **Bloquea**: fase C (verificación de integración con G2); no bloquea el plan de la fase A.
- **Criterio de cierre**: contrato de ámbito acordado y probado con un entrenador de un equipo y un entrenador de varios equipos.

## D-004: Duración de tokens, refresco y sesión

- **Contexto**: la duración afecta a seguridad, UX, revocación y carga de validación.
- **Opciones**:
  - Tokens de acceso de 5 a 10 minutos; identificador de 5 a 10 minutos; refresco rotativo de 8 horas; inactividad de 30 minutos; absoluta de 12 horas.
  - Tokens de acceso de 1 hora; sin refresco; sesión de 8 horas.
  - Tokens de referencia con introspección obligatoria.
- **Recomendación**: acceso 5 minutos, identificador 5 minutos, código 5 minutos, refresco rotativo 8 horas, inactividad 30 minutos, absoluta 12 horas, tolerancia de reloj 2 minutos. Ajustar tras medir.
- **Decisión adoptada (2026-11-22)**: opción A con valores configurables para desarrollo y pruebas: código de autorización 5 min, access token 5 min, ID token 5 min, refresh token rotativo 8 h, inactividad de sesión 30 min, duración absoluta 12 h y tolerancia de reloj máxima 2 min. La revocación de sesión y refresh se prueba por separado de la caducidad del access token; no se promete revocación de JWT en 30 segundos sin un mecanismo verificable.
- **Estado**: resuelta para la fase A (mock y pruebas). Los valores de producción y la ventana de revocación de access tokens quedan pendientes de ratificación.
- **Impacto**: ventana de exposición del access token, UX de sesión y número de refrescos.
- **Responsable**: G1.
- **Bloquea**: fase B (valores de producción).
- **Criterio de cierre**: configuración de producción aprobada, documentada y probada con expiración, refresco, reutilización y revocación separada.

## D-005: Estrategia de revocación

- **Contexto**: un JWT stateless sigue siendo válido hasta su expiración aunque se revoque la sesión. La constitución exige sesiones revocables.
- **Opciones**:
  - JWT de corta duración y revocación de refresco; la ventana de acceso es el TTL.
  - Introspección en cada petición; revocación inmediata, más latencia y acoplamiento.
  - Lista de revocación (denylist) consultada por los módulos; revocación casi inmediata, más estado compartido.
  - Tokens de referencia opacos; revocación inmediata, introspección obligatoria.
- **Recomendación**: JWT corto + revocación de refresco para todos los módulos; introspección para operaciones de alto riesgo o módulos que la requieran. Ventana objetivo ≤ 5 minutos.
- **Decisión adoptada (2026-11-22)**: no se incluye `/connect/introspect` en el contrato v1 ni en el mock (opción de JWT corto + revocación de refresh, sin introspección). Se usan access tokens de 5 min y revocación de sesión/refresh; un access token JWT sigue siendo aceptado hasta su `exp`. La introspección podrá añadirse de forma aditiva en una versión posterior.
- **Estado**: parcial para la fase A. La estrategia de revocación de access tokens en producción y la posible introspección o denylist siguen abiertas.
- **Impacto**: seguridad, latencia y complejidad operativa.
- **Responsable**: G1 con G2, G3 y G4.
- **Bloquea**: producción.
- **Criterio de cierre**: mecanismo de producción aprobado, con revocación de sesión/refresh y exposición del access token documentada y probada.

## D-006: Segundo factor

- **Contexto**: la constitución exige 2FA como responsabilidad de G1, pero no define métodos ni obligatoriedad.
- **Opciones**:
  - TOTP con aplicación autenticadora y códigos de recuperación.
  - OTP por correo o SMS.
  - WebAuthn o llaves de seguridad.
  - Combinación por riesgo y step-up.
- **Recomendación**: TOTP + códigos de recuperación; obligatorio para administradores y opcional para el resto; WebAuthn en una fase posterior.
- **Impacto**: login, soporte, accesibilidad y recuperación.
- **Responsable**: G1.
- **Bloquea**: fase B para flujos de administración.
- **Criterio de cierre**: métodos, obligatoriedad, step-up y recuperación aprobados y probados.

## D-007: Registro, contraseñas, verificación y menores

- **Contexto**: la constitución exige correo y teléfono obligatorios, verificación, tratamiento de menores y consentimiento del tutor.
- **Opciones**:
  - Registro con correo, teléfono, nombre visible y contraseña; verificación de correo y teléfono; consentimiento del tutor para menores.
  - Registro solo con correo; teléfono opcional; verificación diferida.
  - Registro asistido por administrador para menores y deportistas federados.
- **Recomendación**: correo y teléfono obligatorios, contraseña de al menos 12 caracteres con comprobación de contraseñas filtradas si es viable, verificación de correo y teléfono, consentimiento del tutor para menores y aceptación de términos.
- **Impacto**: RGPD, UX, datos sensibles y soporte.
- **Responsable**: G1 con el responsable del club y asesoría legal.
- **Bloquea**: fase B.
- **Criterio de cierre**: flujo aprobado, textos legales revisados y pruebas de menores y tutor.

## D-008: Retención y acceso a auditoría

- **Contexto**: la auditoría contiene datos personales y debe equilibrar seguridad, RGPD y operación.
- **Opciones**:
  - Retención de 90 días.
  - Retención de un año.
  - Retención durante la vida del curso y anonimización posterior.
- **Recomendación**: un año para eventos de seguridad, con acceso restringido, minimización y anonimización al final.
- **Impacto**: cumplimiento, almacenamiento y capacidad de investigación.
- **Responsable**: G1 con el responsable de protección de datos.
- **Bloquea**: fase B.
- **Criterio de cierre**: política de retención, acceso y borrado aprobada.

## D-009: Persistencia definitiva

- **Contexto**: InMemory no cumple migraciones, restricciones, transacciones ni copias de seguridad. La constitución exige migraciones versionadas.
- **Opciones**:
  - InMemory con excepción académica (ADR-0002).
  - SQLite en archivo, con migraciones.
  - SQL Server LocalDB o contenedor.
  - PostgreSQL en contenedor.
- **Recomendación**: InMemory para el alcance académico con excepción ratificada; SQLite o SQL Server/PostgreSQL antes de datos reales.
- **Decisión ratificada (2026-11-25)**: el equipo G1 aprobó ADR-0002 como excepción académica para desarrollo, demostración y pruebas de las fases A/B, sin datos reales ni producción. No se han inventado aprobaciones adicionales.
- **Impacto**: cumple o no la restricción técnica 4; afecta a pruebas y despliegue. La excepción no declara cumplidas las migraciones versionadas.
- **Responsable**: responsable del proyecto.
- **Bloquea**: producción (persistencia definitiva, proveedor relacional y plan de migración).
- **Estado**: excepción académica ratificada; la persistencia definitiva sigue abierta.
- **Criterio de cierre restante**: elegir el proveedor relacional, generar migraciones versionadas y registrar un ADR de persistencia relacional antes de datos reales; la prueba de CI con proveedor relacional (T055) sigue pendiente.

## D-010: Proveedores de correo y SMS

- **Contexto**: la verificación y la recuperación necesitan un canal real; el mock no debe enviar mensajes.
- **Opciones**:
  - SMTP propio.
  - SendGrid, Azure Communication Services, Twilio u otro proveedor.
  - Sumidero local en desarrollo y proveedor en producción.
- **Recomendación**: abstracción de mensajería con sumidero local en desarrollo; proveedor concreto antes de la fase B.
- **Impacto**: coste, entregabilidad, privacidad y configuración.
- **Responsable**: G1 con el responsable del club.
- **Bloquea**: fase B.
- **Criterio de cierre**: proveedor elegido, credenciales fuera del repositorio y pruebas con sumidero.

## D-011: Alojamiento, TLS y gestión de claves

- **Contexto**: las claves de firma y el key ring de Data Protection son críticos. Si se pierden, se invalidan tokens y sesiones.
- **Opciones**:
  - Kestrel local con certificado de desarrollo.
  - Contenedor Linux con terminación TLS en un proxy.
  - Azure App Service u otro servicio gestionado.
  - Almacén de claves: certificate store, Azure Key Vault, fichero protegido o secreto de despliegue.
- **Recomendación**: contenedor Linux con TLS terminado en un proxy; claves en un almacén de secretos; key ring persistente y respaldado.
- **Impacto**: seguridad, operación y continuidad.
- **Responsable**: responsable del proyecto con G1.
- **Bloquea**: producción.
- **Criterio de cierre**: arquitectura de despliegue y gestión de claves aprobada y probada.

## D-012: Consentimiento de módulos de primera parte

- **Contexto**: G2, G3 y G4 son módulos del mismo club, no terceros. El consentimiento explícito puede añadir fricción innecesaria.
- **Opciones**:
  - Consentimiento implícito o sistemático para primera parte.
  - Consentimiento explícito por módulo y ámbito.
  - Consentimiento explícito solo para terceros.
- **Recomendación**: implícito y documentado para primera parte; explícito para terceros.
- **Impacto**: UX y modelo de autorización.
- **Responsable**: G1 con los propietarios de módulo.
- **Bloquea**: fase B.
- **Criterio de cierre**: política de consentimiento aprobada y probada.

## D-013: Eliminación y anonimización de cuentas

- **Contexto**: el RGPD permite supresión, pero la auditoría y las obligaciones del club pueden exigir conservar un mínimo.
- **Opciones**:
  - Borrado físico inmediato.
  - Soft delete con tombstone.
  - Anonimización tras un periodo de retención.
- **Recomendación**: soft delete, anonimización tras la retención y conservación mínima de auditoría sin datos identificativos.
- **Impacto**: RGPD, integridad referencial y auditoría.
- **Responsable**: G1 con el responsable de protección de datos.
- **Bloquea**: fase B y RGPD.
- **Criterio de cierre**: procedimiento aprobado y probado con un usuario ficticio.

## D-014: Política de deprecación de versiones

- **Contexto**: G2, G3 y G4 necesitan tiempo para migrar cuando cambia un contrato compartido.
- **Opciones**:
  - Tres meses.
  - Seis meses.
  - La duración del curso académico.
- **Recomendación**: seis meses o la duración del curso, lo que sea mayor; anuncio en CHANGELOG y guía de migración.
- **Impacto**: compatibilidad y planificación de los módulos.
- **Responsable**: G1 con los consumidores.
- **Bloquea**: fase B.
- **Criterio de cierre**: política publicada y aplicada en el primer cambio incompatible.

## D-015: Multi-club o multi-tenancy

- **Contexto**: el alcance inicial es un club. Añadir multi-tenancy cambia el modelo de identidad, roles y ámbitos.
- **Opciones**:
  - Un solo club.
  - Varios clubes con aislamiento por tenant.
  - Federación o agrupación de clubes.
- **Recomendación**: un solo club; no diseñar multi-tenancy sin un requisito aprobado.
- **Impacto**: modelo de datos, tokens y autorización.
- **Responsable**: responsable del proyecto.
- **Bloquea**: futuro.
- **Criterio de cierre**: requisito aprobado y ADR específico.

## D-016: Audiencias definitivas por API

- **Contexto**: el contrato v1 emite tokens con `aud=identity-api` de forma provisional. G2, G3 y G4 son APIs distintas y no deben aceptar un token de G1 por el mero hecho de estar firmado. La audiencia definitiva cambia la validación, el intercambio de tokens y la configuración de cada módulo.
- **Opciones**:
  - Audiencia única `identity-api` para todas las APIs; cada módulo autoriza por scopes y permisos.
  - Una audiencia por API (`identity-api`, `g2-api`, `g3-api`, `g4-api`), con emisión o intercambio de tokens por audiencia.
  - Audiencia única más un scope obligatorio por módulo (`g2.read`, `g3.write`, etc.).
- **Recomendación**: decidir una matriz de audiencias antes de la integración. Como mínimo, cada API DEBE validar `aud` y rechazar audiencias que no le correspondan. No se asume que `identity-api` sea válida para G2, G3 o G4.
- **Impacto**: validación de tokens, flujos de intercambio, configuración de clientes y superficie de ataque.
- **Responsable**: G1 con G2, G3 y G4.
- **Bloquea**: fase B e integración de G2, G3 y G4.
- **Criterio de cierre**: matriz de audiencias aprobada, publicada en la guía de consumo y probada con un token válido y otro rechazado en cada API.

## D-017: Fuente de datos de contacto para G4

- **Contexto**: G4 (comunicaciones) necesita correo o teléfono de socios y familias para enviar avisos. El contrato de G1 solo expone el correo propio en `/me` y excluye todo dato de contacto del resumen de otros usuarios. Exponer un directorio de contacto en G1 ampliaría la superficie sensible y chocaría con la minimización.
- **Opciones**:
  - Endpoint dedicado de contacto por lote, protegido por un permiso nuevo y auditado.
  - Ampliar `/users/{userId}/summary` y `/users/lookup` con `include=contact`.
  - Solo autoservicio vía userinfo; G4 obtiene los contactos de G3.
- **Recomendación**: endpoint dedicado, minimizado y auditado, si G1 debe ser la fuente; en su defecto, autoservicio y coordinación con G3.
- **Decisión adoptada (2026-11-24)**: opción C. G1 no expone datos de contacto de terceros ni actúa como directorio. `/connect/userinfo` devuelve únicamente los datos del usuario autenticado (`sub`, `name`, `email`, `email_verified` con scope `email`; el teléfono no se publica en v1 porque no hay scope `phone`). G4 obtiene los contactos de G3, que los gestiona en la ficha de socio.
- **Estado**: resuelta para la fase A (contrato y mock). La integración depende del contrato de contacto que publique G3.
- **Impacto**: evita un directorio sensible en G1 y duplica la coordinación con G3; cualquier ampliación futura exige permiso explícito, auditoría y consentimiento de tutor para menores.
- **Responsable**: G1 con G3 y G4.
- **Bloquea**: fase C (contrato G3-G4).
- **Criterio de cierre**: contrato de contacto de G3 publicado y probado con G4; prueba de que G1 no expone contacto ajeno.

## Decisiones ya cerradas

- **D-002 — cerrada para la fase A (2026-11-22)**: catálogo provisional v1 de roles y matriz deny-by-default, pendiente de validación del equipo y de ratificación formal en fase B.
- **D-003 — cerrada para la fase A (2026-11-24)**: G1 emite roles y permisos; cada módulo resuelve su ámbito con `userId` y sus propios datos. La verificación de integración con G2 queda para la fase C.
- **D-004 — cerrada para la fase A (2026-11-22)**: valores de desarrollo y pruebas aprobados y configurables; los valores de producción siguen pendientes.
- **D-009 — excepción académica ratificada (2026-11-25)**: ADR-0002 aprobado por equipo G1 para desarrollo, demostración y pruebas; la persistencia definitiva y la migración relacional siguen abiertas.
- **D-005 — cerrada para la fase A (2026-11-22)**: sin introspección en v1; la revocación de access tokens JWT en producción sigue abierta.
- **D-017 — cerrada para la fase A (2026-11-24)**: G1 solo expone el contacto del usuario autenticado; G4 obtiene los contactos de G3. La integración depende del contrato de contacto de G3.

Estas decisiones se reflejan en la especificación y en el ADR-0001. No implican la ratificación formal de la fase B.
