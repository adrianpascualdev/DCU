# Pruebas E2E del selector de personas

Las pruebas de T054/T070 ejecutan el selector React real en Chromium. Un único
fixture arranca Vite (desarrollo y vista previa del build) en puertos locales
libres. Cada prueba abre un contexto de navegador nuevo. axe-core se instala
solo en este proyecto de pruebas e inyecta su análisis WCAG 2.1 AA en Playwright.

## Ejecutar desde la raíz

Con Node.js LTS, npm, .NET SDK `10.0.404` y PowerShell:

```powershell
npm ci --prefix src/G1.Identity.Mock.Ui
npm run build --prefix src/G1.Identity.Mock.Ui
npm ci --prefix tests/G1.Identity.E2E
dotnet build tests/G1.Identity.E2E
pwsh tests/G1.Identity.E2E/bin/Debug/net10.0/playwright.ps1 install chromium
$env:G1_RUN_SELECTOR_E2E = '1'
dotnet test --project tests/G1.Identity.E2E/G1.Identity.E2E.csproj --no-build --filter-class '*AccessibilitySpecs'
dotnet test --project tests/G1.Identity.E2E/G1.Identity.E2E.csproj --no-build --filter-class '*TokenStorageSpecs'
```

Las suites requieren `G1_RUN_SELECTOR_E2E=1` hasta que DEV1 prepare en CI el
build de la UI, axe-core y Chromium antes de ejecutar los tests (T056).
Con esa variable activa, una dependencia ausente hace fallar la ejecución.
Si los artefactos de .NET se generan fuera del repositorio, define también
`$env:G1_SELECTOR_REPO_ROOT = (Get-Location).Path` desde la raíz.

## Cobertura y límites

- **T054:** axe-core comprueba el selector, incluida la lista en carga, vacía,
  con error y recuperada, en escritorio y a 320 px. El teclado comprueba
  etiquetas, ayudas, orden y visibilidad de foco, selección y reintento.
  Las respuestas de listado se interceptan según el contrato. Sigue pendiente
  probar las páginas de error propias del host y hacer un recorrido manual
  con lector de pantalla, incluidos los anuncios de carga y error.
- **T070:** las pruebas comprueban que los almacenamientos y las cookies del
  contexto aislado quedan vacíos antes y después de la selección. El build
  aún no conecta `onContinueAuthorization`; se comprueba que no ofrece la
  continuación ni realiza peticiones de autorización o canje. La validación
  del callback integrado, PKCE S256 y la distinción en navegador entre una
  cookie de sesión del servidor y el almacenamiento de tokens quedan
  pendientes de T028/T030 y de la autenticación de persona del host.

## Resultado local

El 27 de noviembre de 2026 pasaron 6 de 6 casos en Chromium `156.0.8078.4`
(Playwright `1.64.1`): 4 de accesibilidad y 2 de almacenamiento, sin
omisiones. La ejecución aislada no cubre los pendientes del host.
