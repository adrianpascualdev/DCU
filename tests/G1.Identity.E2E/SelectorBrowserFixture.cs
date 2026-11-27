using System.Diagnostics;
using Microsoft.Playwright;

namespace G1.Identity.E2E
{
    /// <summary>
    /// Comparte Chromium y los dos modos de Vite; cada prueba crea un contexto nuevo.
    /// </summary>
    public sealed class SelectorBrowserFixture : IAsyncLifetime
    {
        private const string TestDirectory = "tests/G1.Identity.E2E";
        private const string UiDirectory = "src/G1.Identity.Mock.Ui";
        private const string ReadyPrefix = "G1_SELECTOR_READY ";
        private const string StartVite = """
            import { createServer, preview } from 'vite';
            const server = { host: '127.0.0.1', port: 0 };
            const dev = await createServer({ server });
            await dev.listen();
            const built = await preview({ preview: server });
            console.log(`G1_SELECTOR_READY ${dev.httpServer.address().port} ${built.httpServer.address().port}`);
            """;

        private Process? _vite;
        private IPlaywright? _playwright;
        private IBrowser? _browser;

        /// <summary>
        /// Habilita las pruebas del selector una vez preparados Node, el build y Chromium.
        /// </summary>
        public static bool IsEnabled => Environment.GetEnvironmentVariable("G1_RUN_SELECTOR_E2E") == "1";

        internal const string EnableInstructions = "Prepara la UI, axe-core y Chromium según el README E2E y establece G1_RUN_SELECTOR_E2E=1.";
        internal const string PersonasRoute = "**/api/v1/identity/mock/personas?*";
        internal const string PersonaKey = "coach.ana";
        internal const string PersonaName = "Ana Entrenadora";

        internal string DevelopmentUrl { get; private set; } = null!;
        internal string BuildUrl { get; private set; } = null!;
        internal string AxePath { get; private set; } = null!;

        /// <inheritdoc />
        public async ValueTask InitializeAsync()
        {
            if (!IsEnabled)
            {
                return;
            }

            var repository = FindRepository();
            var uiPath = Path.Combine(repository, UiDirectory);
            AxePath = Path.Combine(repository, TestDirectory, "node_modules", "axe-core", "axe.min.js");
            if (!File.Exists(AxePath) || !File.Exists(Path.Combine(uiPath, "dist", "index.html")))
            {
                throw new InvalidOperationException(EnableInstructions);
            }

            try
            {
                await StartViteAsync(uiPath);
                _playwright = await Playwright.CreateAsync();
                _browser = await _playwright.Chromium.LaunchAsync();
            }
            catch
            {
                await DisposeAsync();
                throw;
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_browser is not null)
                {
                    var browser = _browser;
                    _browser = null;
                    await browser.CloseAsync();
                }
            }
            finally
            {
                _playwright?.Dispose();
                _playwright = null;
                if (_vite is not null)
                {
                    using var vite = _vite;
                    _vite = null;
                    if (!vite.HasExited)
                    {
                        vite.Kill(entireProcessTree: true);
                        await vite.WaitForExitAsync();
                    }
                }
            }
        }

        internal Task<IBrowserContext> NewContextAsync(bool mobileViewport = false)
        {
            var viewport = mobileViewport
                ? new ViewportSize { Width = 320, Height = 640 }
                : new ViewportSize { Width = 1280, Height = 800 };
            return _browser!.NewContextAsync(new BrowserNewContextOptions { ViewportSize = viewport });
        }

        internal static Task FulfillPersonasAsync(IRoute route, bool empty = false)
        {
            // Ejemplo de listMockPersonas en el OpenAPI normativo.
            var persona = new
            {
                key = PersonaKey,
                userId = "usr_01HXCOACHANA",
                displayName = PersonaName,
                email = "ana.entrenadora@example.test",
                emailVerified = true,
                phoneVerified = true,
                status = "active",
                isMinor = false,
                roles = new[] { "coach" },
                permissions = Array.Empty<string>(),
                authenticationMethods = new[] { "mock_persona" },
                labels = new[] { "restricted-scope" },
            };
            return route.FulfillAsync(new RouteFulfillOptions
            {
                Json = new
                {
                    fixtureVersion = "1.0.0",
                    items = empty ? [] : new[] { persona },
                    page = 1,
                    pageSize = 100,
                    total = empty ? 0 : 1,
                },
            });
        }

        private async Task StartViteAsync(string uiPath)
        {
            var startInfo = new ProcessStartInfo("node")
            {
                WorkingDirectory = uiPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            startInfo.ArgumentList.Add("--input-type=module");
            startInfo.ArgumentList.Add("--eval");
            startInfo.ArgumentList.Add(StartVite);
            _vite = Process.Start(startInfo) ?? throw new InvalidOperationException("No se pudo iniciar Vite.");
            var errors = _vite.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));

            while (await _vite.StandardOutput.ReadLineAsync(timeout.Token) is { } line)
            {
                if (line.StartsWith(ReadyPrefix, StringComparison.Ordinal))
                {
                    var ports = line[ReadyPrefix.Length..].Split(' ');
                    DevelopmentUrl = $"http://127.0.0.1:{ports[0]}";
                    BuildUrl = $"http://127.0.0.1:{ports[1]}";
                    return;
                }
            }

            throw new InvalidOperationException($"Vite terminó antes de iniciar las vistas del selector: {await errors}");
        }

        private static string FindRepository()
        {
            var root = Environment.GetEnvironmentVariable("G1_SELECTOR_REPO_ROOT") ?? Directory.GetCurrentDirectory();
            DirectoryInfo? directory = new DirectoryInfo(root);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "G1.Identity.slnx")))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }

            throw new InvalidOperationException("No se encontró el repositorio DCU. Indica su raíz en G1_SELECTOR_REPO_ROOT.");
        }
    }

    /// <summary>
    /// Aloja las suites del selector en una misma colección de xUnit.
    /// </summary>
    [CollectionDefinition(nameof(SelectorBrowserCollection))]
    public sealed class SelectorBrowserCollection : ICollectionFixture<SelectorBrowserFixture>;
}
