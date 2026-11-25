using Microsoft.Playwright;

namespace G1.Identity.E2E
{
    /// <summary>
    /// Guardia de configuración mientras los escenarios de navegador se implementan en otra fase.
    /// </summary>
    public sealed class BrowserRuntimeGuardTests
    {
        [Fact]
        public void PlaywrightRuntimeIsAvailable()
        {
            var assembly = typeof(IPage).Assembly;

            Assert.NotNull(assembly);
            Assert.Contains(@"Playwright", assembly.GetName().Name, StringComparison.Ordinal);
        }
    }
}
