# Validación de fase A — US1 (DEV1)

- **Fecha**: 2026-11-25
- **Rama**: `dev1/infra-contract`
- **Alcance**: T019–T024, secciones 2 y 3 de [`quickstart.md`](../../specs/001-g1-identidad-contrato-mock/quickstart.md)
- **Objetivo**: comprobar que el contrato OpenAPI 3.1 es navegable desde Swagger UI y verificable en CI, sin implementar US2 ni tareas de DEV2–DEV5.

## 1. Comandos ejecutados

```bash
dotnet restore G1.Identity.slnx
dotnet build G1.Identity.slnx --configuration Release --no-restore --no-incremental
dotnet test --solution G1.Identity.slnx --configuration Release --no-build

scripts/export-openapi.sh check
scripts/export-openapi.sh runtime
scripts/check-openapi-breaking.sh
```

Para la comprobación del contrato se usaron `@redocly/cli@2.83.0` y `oasdiff` mediante la imagen Docker `tufin/oasdiff:latest`, por no estar `oasdiff` instalado en el PATH local.

## 2. Resultados

| Comprobación | Resultado |
| --- | --- |
| Restauración | Correcta |
| Compilación Release | `0 Advertencia(s)`, `0 Error(es)` |
| Pruebas automatizadas | `41/41` correctas; 9 nuevas pruebas de US1 (4 de documento y 5 de Swagger/disponibilidad) |
| Contrato normativo | OpenAPI `3.1.0`, `info.version` `1.0.0`, 18 rutas, 19 operaciones, 23 esquemas, 5 etiquetas |
| Gobernanza | Presentes `x-reviewed-at` y `x-persistence-exception` (ADR-0002, alcance académico) |
| Seguridad | `cookieAuth` (`apiKey`/`cookie`, `__Host-g1.session`) y `bearerAuth` (`http`/`bearer`) |
| Ejemplos | Las 19 operaciones tienen ejemplo de éxito y de error; las respuestas vacías conservan `x-success-example` |
| Redocly | Válido con 5 advertencias preexistentes (localhost, `4XX` ausente en descubrimiento/JWKS y `2XX` ausente en la redirección 302 de autorización) |
| oasdiff | Sin cambios incompatibles frente a `origin/001-g1-identidad-contrato-mock` |
| Copia incrustada | `scripts/export-openapi.sh check` termina sin diff entre `openapi.v1.yaml` y `ContractDocument.json` |
| Documento publicado | `scripts/export-openapi.sh runtime` confirma equivalencia exacta con la copia incrustada |
| Caché del documento | `Cache-Control: no-cache`, ETag fuerte `"A00558FB50DE541C049C8A307F966E2DCA99D8BA642A3D2697FDDA7949C29ED4"` y `304` ante `If-None-Match` |
| Entornos | `/swagger` y `/openapi/v1.json` responden en Development/Testing y devuelven `404` en Production |
| Escaneo de secretos | Sin coincidencias con el patrón de claves, tokens de GitHub ni JWT de ejemplo |

## 3. Swagger UI real

Se arrancó el host en `Testing` sobre `http://127.0.0.1:5191` y se comprobó:

1. `/swagger` redirige a `/swagger/index.html` y devuelve HTML.
2. Se descargan correctamente `swagger-ui.css`, `swagger-ui-bundle.js`, `swagger-ui-standalone-preset.js`, `index.js` e `index.css`.
3. `index.js` apunta a `/openapi/v1.json`.
4. La cabecera `Content-Security-Policy` de `/swagger*` permite scripts propios e inline, estilos inline, imágenes `data:`, prohíbe `object-src`, fija `base-uri 'self'` y `frame-ancestors 'none'`.
5. En Chromium (Playwright 1.64.1, modo headless) la página cargó sin errores de consola ni avisos de CSP, renderizó las cinco etiquetas acordadas y mostró las 19 operaciones. El título mostrado fue `G1 Identity API 1.0.0 OAS 3.1` (Swagger UI añade el sufijo `OAS 3.1`).

La ejecución en navegador se realizó desde un directorio temporal fuera del repositorio; no se añadieron dependencias ni archivos de DEV5.

## 4. Salvedades de alcance

- El paso 2 de `quickstart.md` (build de `src/G1.Identity.Mock.Ui`) pertenece a T004 de DEV5 y aún no está implementado. No afecta a US1: Swagger UI se sirve desde el host con `Swashbuckle.AspNetCore.SwaggerUI`.
- El contrato documenta las 19 operaciones acordadas, pero DEV2–DEV5 aún no han implementado sus endpoints. Por ello, pulsar **Try it out** en operaciones ajenas a la documentación devolverá `404` hasta que se integren esas tareas; no se han creado endpoints falsos ni definiciones duplicadas.
- No se han resuelto D-001, D-012, D-016 ni la elección definitiva de proveedor de persistencia; ADR-0002 sigue limitada a desarrollo, demostración y pruebas.
- Esta validación cubre solo US1. La trazabilidad completa de SC-001 a SC-012 corresponde a T065 y no se marca aquí como completada.

## 5. Conclusión

- **SC-001** (contrato utilizable por consumidores desde la documentación): verificado mediante documento runtime equivalente, 19 operaciones visibles y ejemplos de éxito/error.
- **SC-006** (documentación disponible solo en Development/Testing, validada en CI): verificado en host real, en pruebas automatizadas y en el pipeline mediante Redocly, oasdiff y la comprobación de la copia incrustada.

US1 queda verificada de forma aislada. No se inicia US2.
