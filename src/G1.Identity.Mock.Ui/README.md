# Selector de personas de prueba de G1

Proyecto React 19, TypeScript estricto y Vite. Desde esta carpeta, con Node.js LTS:

```sh
npm ci
npm run typecheck
npm run build
npm run dev
```

Abre la URL local que muestra Vite. En `npm run dev` se usa solo el ejemplo de
personas de OpenAPI para probar la selección con teclado, sin backend. La acción
«Probar selección» confirma el nombre elegido; no inicia sesión. El ejemplo no
se incluye en `dist/`.

## Integración pendiente

- **DEV1 (T030):** publicar `dist/` en el host del mock, solo en
  `Development`/`Testing`. Los assets usan rutas relativas; si se sirve bajo un
  subdirectorio, servir la página con la barra final o alinear la base de Vite
  con la ruta que se acuerde.
- **DEV2 (T028):** acordar con DEV1 cómo se entrega al selector una autorización
  en curso y cómo se reanuda `/connect/authorize` manteniendo la petición
  original. `App` acepta el callback `onContinueAuthorization(personaKey)`, que
  debe aportar el adaptador de integración. Recibe solo la clave publicada por
  el mock; no recibe ni construye URLs, parámetros OAuth, códigos o tokens. Por
  ahora el build no conecta el callback y oculta la acción de continuar.
- **DEV3 (T048):** el build obtiene las personas del `GET
  /api/v1/identity/mock/personas` del mismo origen. Solicita la primera página
  con el máximo contractual (100), suficiente para las 15 personas de fase A,
  y solo muestra el nombre y usa la clave. Hasta que se integre el endpoint, el
  build puede mostrar el estado de error al servirse sin el host.

La UI no canjea códigos ni almacena tokens. El selector es una página aislada
del mock; usa componentes Bootstrap y tokens locales mientras no esté
disponible `myadmon-theme`.
