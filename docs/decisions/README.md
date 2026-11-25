# Decisiones de arquitectura

Este directorio contiene las decisiones de arquitectura (ADR) y el registro único de decisiones pendientes que afectan a más de un módulo. La constitución exige documentar las decisiones de arquitectura, persistencia, despliegue, autenticación y proveedores, y no resolver implícitamente las decisiones pendientes.

## Índice de ADR

| ADR | Título | Estado | Fecha |
| --- | --- | --- | --- |
| [ADR-0001](ADR-0001-servidor-oauth-propio-identity-openiddict.md) | Servidor OAuth 2.0/OIDC propio con ASP.NET Core Identity y OpenIddict | Propuesto | 2026-11-11 |
| [ADR-0002](ADR-0002-inmemory-persistencia-academica.md) | EF Core InMemory como persistencia académica | Aceptado (excepción académica, ratificada por equipo G1 el 2026-11-25) | 2026-11-11 |

## Registro de decisiones pendientes

Las decisiones abiertas se registran en [`pending-decisions.md`](pending-decisions.md). Ninguna se considera resuelta por un valor por defecto, una suposición o una decisión implícita en código.

## Estados de un ADR

- **Propuesto**: documentado y pendiente de ratificación. No autoriza a implementar el flujo afectado si contradice una regla MUST.
- **Aceptado**: ratificado por el responsable correspondiente. Se puede implementar.
- **Rechazado**: no se adopta. Se documenta la alternativa elegida.
- **Reemplazado**: sustituido por otro ADR; se conserva el histórico y se enlaza el sustituto.
- **Obsoleto**: ya no aplica, pero se conserva por trazabilidad.

## Cómo proponer un ADR

1. Crear un archivo `ADR-000N-titulo-corto.md` con contexto, decisión, alternativas, consecuencias, cumplimiento de la constitución y condiciones de ratificación.
2. Enlazarlo desde este índice y desde la especificación afectada.
3. Registrar las cuestiones abiertas en `pending-decisions.md`, no en el código.
4. Pedir revisión al responsable del proyecto y a los responsables de los módulos afectados.
5. Si el ADR contradice una regla MUST de la constitución, tramitar una enmienda antes de implementar.

## Cómo cerrar una decisión pendiente

1. Actualizar la fila correspondiente de `pending-decisions.md` con la opción elegida, el responsable y la fecha.
2. Actualizar la especificación, el ADR o el contrato afectado.
3. Añadir o actualizar las pruebas que verifican la decisión.
4. Avisar a los consumidores si cambia un contrato compartido.
5. Registrar el cambio en `CHANGELOG.md` si altera comportamiento público.
