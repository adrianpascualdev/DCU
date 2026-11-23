<!--
Sync Impact Report
==================
Version change: (4 constituciones separadas) → 1.0.0
Bump rationale: Primera constitución global unificada. Sustituye a:
  - constitution v0.2.0 (Gestor del Club de Rugby)
  - constitution v1.0.0 (Módulo base: identidad y autorización)
  - Parte Común de la constitución del Módulo 3 v1.1.0 (Comunicación y Equipaciones)
  - Principios y flujo comunes de la constitución del Módulo de Socios v1.1.0

Excluido deliberadamente (es específico de módulo y va en la constitución/spec de cada módulo):
  - Módulo 3: Parte Específica (alcance, matriz de permisos, tabla de API, estructuras
    Noticia/Aviso/Equipación/Incidencia).
  - Módulo de Socios: propósito y alcance (directorio, cuotas, registro manual de pagos).

Principios:
  (Sección añadida: Módulos, Responsabilidades y Dependencias — G1 Infra, G2 Gest. Deportiva,
   G3 Socios y Cuotas, G4 Comunicaciones y Equipaciones; dependencias propuestas, a confirmar.)
  I. Fidelidad al dominio y control del alcance
  II. Arquitectura modular e independiente
  III. Desarrollo guiado por contratos (API-First)
  IV. Seguridad, privacidad e identidad única (NON-NEGOTIABLE)
  V. Integridad de datos y trazabilidad
  VI. Experiencia por rol, accesibilidad e idioma
  VII. Calidad comprobable y pruebas
  VIII. Simplicidad y mantenimiento
  IX. Requisitos confirmados frente a decisiones pendientes
  X. Integración progresiva con datos simulados

Conflictos entre fuentes resueltos (revisar) :
  - Canales: se adopta la visión de v0.2.0 (web pública + web de gestión PC + PWA móvil);
    "Mobile-First" del Módulo 3 se aplica a la PWA y a las vistas de entrenador.
  - Almacenamiento: el Módulo de Socios decía "reutilizar su almacenamiento"; se reinterpreta
    como "reutilizar la infraestructura de persistencia del proyecto" sin acceso a datos de
    otro módulo (Principio II).

Decisiones pendientes registradas (ver sección "Decisiones Pendientes"):
  - Catálogo único de roles; tutor/familiar con o sin cuenta; autenticación y 2FA;
    recuperación de cuentas; consentimiento de menores; asignación de roles;
    BD/alojamiento/pasarela/WhatsApp/observabilidad; fecha de ratificación.
-->

# Gestor del Club de Rugby (València Rugby Club) Constitution

Esta constitución establece las reglas de producto, ingeniería e integración que deben
cumplir las especificaciones, planes, tareas e implementaciones de **todos los módulos** del
sistema de gestión del club. Cada módulo puede añadir una constitución o sección específica
con su alcance, pero **no puede contradecir** esta constitución.

## Core Principles

### I. Fidelidad al dominio y control del alcance

El sistema MUST representar el funcionamiento real del club y MUST NOT atribuirle
responsabilidades que pertenecen a terceros.

- El club utiliza instalaciones públicas de terceros. El sistema solo gestionará y consultará
  las franjas reservadas al club; MUST NOT gestionar la instalación, su mantenimiento, su
  disponibilidad global ni las reservas de otras entidades.
- MUST NOT existir venta de entradas a espectadores como fuente de ingresos. Las cuotas son el
  modelo económico principal. Ventas, patrocinios, gastos, descuentos y regalos se registran
  cuando correspondan a la operativa administrativa.
- Se distinguen las modalidades Rugby 15, Rugby 7 y Rugby Adaptado, y la estructura
  modalidad → categoría → equipo (una categoría puede contener varios equipos).
- Se mantienen diferenciados posible cliente, simpatizante, socio y deportista federado. Ser
  socio y estar federado NO son conceptos equivalentes.
- Las etiquetas de patrocinadores no son excluyentes: un patrocinador puede tener varios ámbitos.
- Las equipaciones reutilizables tienen trazabilidad de asignación, devolución y limpieza; MUST
  NOT suponerse que todas las prendas siguen el mismo ciclo.
- Cada funcionalidad MUST declarar a qué módulo pertenece. Los módulos no aprobados o fuera de
  alcance MUST NOT introducirse como dependencias implícitas. Toda ampliación requiere
  especificación y aprobación explícitas.

### II. Arquitectura modular e independiente

Cada módulo es dueño de sus datos, reglas de negocio, validaciones y ciclo de vida.

- Ningún módulo MUST leer o escribir directamente las tablas, colecciones o repositorios
  internos de otro módulo.
- La integración entre módulos se realiza solo mediante contratos REST versionados y
  documentados, con propietario identificado.
- Cada equipo MUST poder desarrollar y probar su módulo de forma independiente.
- `userId` es la referencia de vinculación con la identidad; los módulos no duplican el perfil
  completo ni crean identidades ni modifican roles.
- Un módulo se integra en la navegación del proyecto sin alterar rutas ni estilos de otros
  módulos, y reutiliza la sesión y los permisos del proyecto a través de los contratos
  definidos.
- Se evitan duplicaciones de reglas de negocio entre web de gestión, PWA y web pública; las
  reglas viven en el backend.

### III. Desarrollo guiado por contratos (API-First)

Toda comunicación entre frontends, backend y módulos se realiza mediante endpoints REST.

- El contrato MUST definirse antes de implementar proveedor o consumidor, e incluir método,
  ruta, parámetros, autenticación, autorización, esquema de respuesta, errores y versión.
- Los consumidores MUST NOT depender de detalles internos del proveedor.
- Un cambio incompatible requiere nueva versión de contrato, plan de migración y periodo de
  compatibilidad explícito.
- Las respuestas son deterministas y usan nombres de campo consistentes en español cuando sean
  datos funcionales; los nombres de endpoints permanecen estables.
- Convenciones comunes:
  - Colecciones: lista con metadatos de paginación cuando proceda; recurso: objeto identificado.
  - Fechas en ISO 8601 con zona horaria.
  - Identificadores opacos; no se interpretan como índices secuenciales.
  - `400` petición inválida, `401` identidad ausente o inválida, `403` rol insuficiente,
    `404` recurso inexistente o no visible, `409` conflicto de estado.
  - Los errores incluyen código estable, mensaje para la interfaz y, si es seguro, campos
    afectados. MUST NOT devolverse trazas internas.
- Los contratos se documentan con OpenAPI y Swagger UI (ver Restricciones Técnicas).

### IV. Seguridad, privacidad e identidad única (NON-NEGOTIABLE)

- **Validación en servidor.** La identidad, el rol, el ámbito del recurso y las reglas de
  negocio MUST validarse en el servidor o capa de dominio. Ocultar un botón o validar solo en
  la interfaz NO es control de acceso. El cliente nunca puede elevar privilegios mediante
  parámetros, mocks o cambios de interfaz; un `userId` o `rol` enviado por el cliente no basta
  y MUST contrastarse con la identidad autenticada.
- **Denegación por defecto y mínimo privilegio.** Cada persona tiene una identidad única, aunque
  pueda acumular varios perfiles. La autorización se expresa mediante una matriz explícita de
  permisos por perfil, recurso y ámbito. Añadir un perfil o permiso es un cambio deliberado y
  revisado. La asignación de roles registra quién la concede, a quién y cuándo.
- **Respuestas explícitas.** Las operaciones restringidas devuelven un error explícito, no una
  respuesta vacía que parezca éxito. Los datos se minimizan según rol y contexto; las alertas o
  datos restringidos se filtran antes de llegar al cliente y no son accesibles por
  identificadores alternativos.
- **Datos sensibles.** Los datos de menores y de lesiones/salud requieren protección reforzada:
  solo los consultan quienes los necesitan para sus funciones. Los datos del menor y su tutor
  tienen tratamiento diferenciado, con autorización del tutor, visibilidad restringida y sin
  exposición pública por defecto. Correo y teléfono son obligatorios en el registro; el DNI es
  opcional y solo se exige si el perfil o un requisito legal lo imponen, con acceso restringido.
  Se recoge el mínimo de datos necesario con finalidad declarada.
- **Credenciales y secretos.** Contraseñas con hash adaptativo y sal; sesiones con expiración y
  revocables; secretos fuera del repositorio. Ninguna credencial, token, dato de pago completo,
  dato personal o de salud innecesario MUST aparecer en código, registros, trazas o mensajes de
  error.
- **Reparto:** autenticación, verificación, recuperación, 2FA, roles y matriz base son
  responsabilidad de G1; el resto de módulos las consume por contrato y solo implementa la
  autorización de sus propios recursos.
- **Verificación y recuperación (G1).** El estado de verificación de correo y teléfono forma parte de
  la identidad. La recuperación de cuenta y cualquier segundo factor no revelan si una cuenta
  existe y usan mecanismos de un solo uso con caducidad.
- **Auditoría.** Los eventos de autenticación, cambios de permisos, operaciones administrativas
  sensibles y cambios relevantes de estado dejan traza con marca temporal, actor y resultado,
  sin secretos ni datos personales innecesarios.
- **Errores al usuario.** Comprensibles y sin información interna, datos de terceros ni
  detalles que faciliten accesos no autorizados.
- El cumplimiento de RGPD y normativa española de protección de datos MUST verificarse antes de
  exponer datos reales; los tratamientos de menores, salud y pagos requieren validación legal y
  operativa del club. Esta constitución no sustituye dicha validación.

### V. Integridad de datos y trazabilidad

Las reglas de dominio de esta sección las implementa **únicamente el módulo propietario**
(ver Módulos y Responsabilidades); los demás las consumen por contrato.

- Las reglas de negocio críticas no dependen solo del estado mostrado en pantalla. Las
  modificaciones se validan y mantienen coherencia entre interfaces. Las integraciones externas
  no son fuente de verdad implícita.
- Las entradas se validan y las operaciones relacionadas evitan resultados parciales
  inconsistentes.
- La asistencia distingue como mínimo confirmada, ausencia, lesión y falta de respuesta; la falta
  de respuesta NO se interpreta como confirmación.
- Actividades y entrenamientos se asocian a las franjas reservadas por el club.
- Cuotas, pagos, ventas, patrocinios, gastos, descuentos y regalos son consultables de forma
  consistente desde áreas autorizadas, y el historial identifica concepto y naturaleza de cada
  operación. Un regalo se registra como tal y no se confunde con ingreso efectivo, ni siquiera
  como operación de importe cero. Registrar un pago es dejar constancia de un cobro; no ejecuta
  un cargo bancario.
- Inventario y asignaciones de equipación reflejan el estado real de cada unidad (asignada,
  pendiente de devolución, pendiente de lavar, en lavado, limpia/lista, según el flujo definido).
- Antes de usar datos reales MUST comprobarse la recuperación de copias de seguridad. Los
  cambios con riesgo de pérdida de información requieren estrategia de recuperación.

### VI. Experiencia por rol, accesibilidad e idioma

- **Canales:** web pública (noticias, eventos abiertos, vías de registro), web de gestión para
  PC (administración) y PWA móvil para socios y jugadores, con vista móvil específica para el
  entrenador. La PWA se diseña primero para teléfono y uso táctil con una mano.
- **Segmentación por rol:** la interfaz refleja necesidades y permisos de cada perfil; no se
  impone una interfaz única que exponga funciones irrelevantes o prohibidas. La segmentación no
  puede ocultar el motivo de una acción no disponible ni exponer datos de otro rol.
- **Accesibilidad:** cumplimiento de WCAG 2.1 AA (contraste, foco visible, etiquetas de
  formulario, mensajes de error). Operaciones esenciales compatibles con teclado y tecnologías
  de asistencia; MUST NOT depender de gestos complejos (arrastrar, pellizcar, mantener); áreas
  táctiles suficientes; información importante MUST NOT transmitirse solo por color o iconos sin
  etiqueta. La accesibilidad se comprueba en los flujos principales, no al final.
- **Estados:** todo flujo muestra de forma visible carga, vacío, éxito y error con texto
  comprensible; los errores indican qué ocurrió y cómo corregirlo. Las acciones críticas piden
  confirmación perceptible.
- **Idioma:** español como idioma de referencia, con vocabulario del club (modalidades,
  categorías, equipos, convocatorias, cuotas, fichas federativas, equipaciones) y comprensible
  sin conocimientos técnicos; textos centralizados y traducibles.
- Las vistas de gestión se revisan en resolución de ordenador y las generales en móvil y
  ordenador antes de aceptarse.

### VII. Calidad comprobable y pruebas

Una función no está terminada porque funcione en el caso ideal ni mientras incumpla criterios
obligatorios.

- Cada función define quién la usa, qué necesidad resuelve, cómo se comporta y cómo se verifica.
- Cada entrega indica qué criterios se verificaron y cuáles siguen pendientes, e incluye
  instrucciones de uso y limitaciones o integraciones pendientes.
- Las reglas de negocio, permisos y persistencia tienen pruebas automatizadas proporcionales al
  riesgo. Las rutas de autenticación/autorización tienen pruebas de acceso permitido **y**
  denegado. Cada API tiene pruebas de contrato.
- Backend: xUnit (reglas de negocio e integración de APIs/persistencia). Navegador y E2E:
  Playwright (flujos críticos y combinaciones de roles relevantes).
- Los errores corregidos añaden prueba de regresión cuando sea viable.
- Antes de integrar se ejecutan pruebas, análisis estático, formato y compilación.
- Se priorizan los flujos de necesidad acreditada (la encuesta inicial de 12 respuestas respalda
  confirmación de asistencia y consulta de cuota/ficha federativa; es orientativa).

### VIII. Simplicidad y mantenimiento

- Se elige la solución más sencilla que cumpla los requisitos. No se añaden capas,
  dependencias, servicios o automatizaciones sin necesidad demostrable; la complejidad añadida
  se justifica en el plan.
- La configuración se separa de la lógica de negocio. Las instrucciones de instalación,
  ejecución y verificación se mantienen actualizadas.
- La lógica de negocio es comprobable sin depender de la interfaz y se mantiene separada de la
  presentación y de proveedores externos; las integraciones de pago y mensajería se encapsulan
  para poder sustituir proveedor.
- Las decisiones de arquitectura, persistencia, despliegue, autenticación y proveedores que
  afecten a más de un módulo se documentan (ADR).

### IX. Requisitos confirmados frente a decisiones pendientes

- Las especificaciones separan visiblemente requisitos confirmados, decisiones pendientes y
  fuera de alcance; las suposiciones se distinguen de los requisitos.
- Una decisión pendiente MUST NOT resolverse implícitamente en código, valores por defecto no
  documentados o suposiciones. Se registra con alcance, alternativas, responsable y efecto en
  seguridad, menores y UX. Mientras siga abierta, el código la trata como configurable o
  explícitamente no soportada, y las pruebas lo reflejan.
- Las decisiones pendientes viven en un registro único y se cierran antes de implementar el
  flujo afectado; su cierre actualiza la especificación.

### X. Integración progresiva con datos simulados

- Los mocks permiten desarrollo en paralelo mientras la infraestructura real no esté
  disponible. Están en español, usan los mismos campos, estados, permisos y errores que la
  integración real y son sustituibles sin cambiar consumidores.
- Los mocks NO son una vía alternativa de autorización. Incluyen casos públicos, restringidos,
  vacíos, inválidos y de error.
- La documentación identifica qué datos están simulados y qué queda pendiente de integración
  real. Una demo aislada no acredita integración: esta se considera completa cuando se conecta a
  los servicios reales y se verifican los permisos.
- Demostraciones, pruebas y seeds usan datos ficticios, sin secretos ni datos personales reales.

## Módulos, Responsabilidades y Dependencias

El proyecto se divide en cuatro módulos, uno por grupo. **Cada grupo entrega, solo para su
dominio, la parte de usuario (PWA móvil) y la parte de administración (web de gestión en PC).**

| Grupo | Módulo | Es propietario de (y nadie más implementa) |
|---|---|---|
| G1 | Infraestructura | Identidad única, usuarios, registro, autenticación, sesión, verificación, recuperación de cuenta, 2FA, roles y matriz base de permisos, auditoría de autenticación, administración de usuarios, estructura común del proyecto (navegación/panel general, ver pendiente) |
| G2 | Gestión Deportiva | Modalidades → categorías → equipos, deportistas/federados, convocatorias, asistencia, lesiones, entrenamientos y franjas reservadas, vista móvil del entrenador |
| G3 | Socios y Cuotas | Socios y su directorio, cuotas, registro de pagos, facturación |
| G4 | Comunicaciones y Equipaciones | Noticias, agenda pública, avisos y alertas, inventario y ciclo de equipaciones, incidencias de material |

### Reglas de responsabilidad

- **Responsabilidad limitada:** un grupo MUST NOT implementar funcionalidad que pertenezca a
  otro módulo (p. ej. login, gestión de usuarios, equipos o fichas federativas fuera de su
  dueño). Si necesita algo ajeno, lo solicita al propietario como contrato; no lo reimplementa
  ni lo duplica.
- Un módulo guarda de otro solo referencias opacas (`userId`, `equipoId`, etc.), nunca copias
  del dato ni de las reglas de negocio del propietario.
- Los datos ajenos se muestran en la interfaz consultando la API del propietario, aplicando
  siempre sus permisos.

### Dependencias (propuesta, a confirmar entre grupos)

| Consumidor → Proveedor | Qué necesita |
|---|---|
| G2, G3, G4 → G1 | Validar sesión, `userId`, `rol` y ámbitos; todos dependen de G1 |
| G4 → G2 | Segmentar avisos por equipo/categoría; ámbito deportivo del entrenador; deportista afectado en equipaciones e incidencias |
| G3 → G2 | Relación socio ↔ deportista federado; menores y su tutor |
| G4 → G3 | Segmentar comunicaciones por colectivos (socios) |

- Las dependencias van en un solo sentido y MUST NOT formar ciclos; cualquier dependencia
  nueva se añade aquí con propietario y contrato.
- **Nadie queda bloqueado en el desarrollo:** el proveedor publica el contrato (OpenAPI) y un
  mock fiel **antes** de que el consumidor implemente (Principio X). Lo único que puede
  esperar a un proveedor es la **integración real**, nunca el desarrollo ni las pruebas
  del consumidor.
- **G1 desbloquea al resto:** su primer entregable prioritario es el contrato de identidad
  (sesión → `userId`, `rol`, ámbitos) con mock operativo. Hasta que exista la integración
  real, el resto usa ese mock sin atajos de autorización.
- Integración por fases: (1) contrato + mock, (2) pruebas de contrato contra el proveedor,
  (3) integración real con permisos verificados.
- Un cambio en un contrato compartido requiere aviso y acuerdo de proveedor y consumidores;
  los incompatibles siguen el Principio III. El proveedor mantiene disponibles su Swagger y su
  mock actualizados.
- Los bloqueos entre grupos que no se resuelvan se registran como decisión pendiente y se
  escalan al responsable del proyecto.

## Restricciones Técnicas

1. **Frontend:** React + TypeScript, aplicación independiente del backend (proyecto y
   build/despliegue propios), que consume APIs por contrato. Node.js es el entorno de
   herramientas del frontend, no del backend.
2. **Backend:** .NET 10.0 y C# 14.0 con ASP.NET Core Minimal APIs; sin controladores MVC salvo
   enmienda aprobada. Cualquier cambio de versión requiere enmienda.
3. **Documentación de API:** OpenAPI + Swagger UI, con exposición segura según entorno.
4. **Persistencia:** Entity Framework Core (compatible con .NET 10), migraciones versionadas y
   revisables; seeding idempotente de datos iniciales/referencia, sin secretos ni datos
   personales reales, que no sustituye a las migraciones.
5. **Mensajería asíncrona:** Azure Service Bus para tareas por colas, con manejo de fallos,
   reintentos y entregas duplicadas (procesamiento idempotente cuando sea posible).
6. **Tiempo real:** SignalR (WebSockets preferente) solo cuando un requisito funcional lo
   justifique; no se añade por defecto.
7. **Pruebas:** xUnit (backend) y Playwright (navegador/E2E).
8. **Comunicaciones:** segmentables por colectivos, categorías o equipos. La aplicación es el
   canal; WhatsApp es complemento, sujeto a viabilidad técnica, configuración y consentimiento.
9. **Fuera del alcance inicial:** gestión de instalaciones de terceros, venta de entradas a
   espectadores, comunicación directa entrenador–jugador desde el sistema y supervisión global
   del Director Deportivo (fase futura), salvo aprobación posterior.
10. **Entorno de desarrollo inicial:** Windows, PowerShell y Spec Kit. El plan documenta
    requisitos de ejecución y decisiones de almacenamiento, alojamiento e integraciones.

## Decisiones Pendientes

Se resuelven por registro formal (Principio IX) antes de implementar el flujo afectado:

- **Reparto de dominios sin dueño claro:** patrocinadores, tienda/ventas/descuentos/regalos,
  gastos, fichas federativas y comunicación segmentada con WhatsApp no están asignados a ningún
  grupo con certeza (G3 y G4 podrían solaparse). También: quién es propietario del panel
  general/navegación y de la web pública (¿G1?).
- **Confirmar el mapa de dependencias** entre G1–G4.
- **Catálogo único de roles.** Las fuentes usan nombres distintos: gestor del club /
  administrador, gestor de equipos, entrenador, jugador / deportista, familiar / tutor,
  socio, patrocinador, simpatizante, personal de material, Director Deportivo (futuro).
- **Tutor/familiar:** v0.2.0 establece que el tutor no tiene cuenta propia; el módulo base y
  el Módulo 3 contemplan perfil «familia/tutor» / `familiar` con permisos. Resolver.
- Proveedor y 2FA de autenticación, recuperación de cuentas, autoridad de asignación de roles.
- Consentimiento y tratamiento de datos de menores.
- Motor de base de datos, alojamiento, pasarela de pago, proveedor de WhatsApp, copias de
  seguridad y observabilidad.

## Flujo de Desarrollo y Puertas de Calidad

1. **Especificar:** propósito, usuarios, alcance, reglas de negocio y criterios de aceptación
   verificables antes de implementar. Cambio de alcance ⇒ actualizar especificación y tareas.
2. **Aclarar** dudas que afecten a alcance, datos o flujos principales.
3. **Planificar contra la constitución** con comprobación explícita de principios; un conflicto
   se corrige, no se ignora.
4. **Descomponer** en tareas pequeñas y verificables vinculadas a requisitos, con dependencias y
   pruebas.
5. **Implementar por incrementos** y verificar los criterios afectados.
6. **Revisar** coherencia entre especificación, plan, tareas y resultado antes de entregar.

Cada pull request indica si modifica contratos, permisos, estructuras de datos o presentación
móvil/escritorio, y verifica: propiedad de datos y ausencia de dependencias directas, contrato
actualizado, permisos por rol, mocks y errores, flujo en móvil/escritorio con navegación
accesible, y documentación de cambios incompatibles.

**No se acepta una integración si:**
1. Permite acceder a datos fuera del rol o ámbito del solicitante.
2. Un mock difiere en forma de la respuesta real.
3. Un cambio incompatible no está versionado ni documentado.
4. Falta un estado de carga, vacío, error o confirmación.
5. Un endpoint no especifica IDs, campos obligatorios y errores esperados.
6. Hay pruebas relevantes fallidas, errores de compilación o incumplimientos conocidos de
   seguridad o accesibilidad.
7. Hay secretos en el código o datos personales en registros.

## Governance

Esta constitución prevalece sobre prácticas, plantillas y convenciones locales que la
contradigan, incluida cualquier constitución de módulo. Ante contradicción con una regla MUST,
prevalece hasta que se apruebe formalmente una enmienda.

- **Enmiendas:** cualquier miembro puede proponerlas con motivación e impacto en usuarios,
  datos, permisos, contratos, pruebas y trabajo existente. Requieren revisión y aprobación del
  responsable del proyecto o del equipo (y de los responsables de módulos afectados). Deben
  incluir plan de migración si afectan a trabajo entregado, actualizar la fecha de revisión,
  conservar la fecha de adopción inicial y comprobar la compatibilidad de los documentos
  dependientes.
- **Versionado (SemVer):** MAJOR para eliminar o redefinir principios o gobernanza de forma
  incompatible; MINOR para añadir principios/secciones o ampliar materialmente obligaciones;
  PATCH para aclaraciones sin cambio de obligaciones.
- **Cumplimiento:** cada revisión y entrega verifica los principios aplicables. Las excepciones
  no son tácitas: se documentan con motivo, alcance, riesgo, responsable, alternativa adoptada y
  fecha de revisión; si contradicen una regla MUST, requieren enmienda previa. Las decisiones
  pendientes no autorizan a implementar comportamiento no especificado.
- **Spec Kit:** tras una enmienda se revisan especificaciones, planes y tareas afectados.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): pendiente de aprobación del equipo | **Last Amended**: 2026-11-10
