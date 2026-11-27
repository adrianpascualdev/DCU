using System.Text.Json;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace G1.Identity.E2E
{
    /// <summary>
    /// Comprueba el selector React aislado y los estados de error que renderiza la UI.
    /// </summary>
    [Collection(nameof(SelectorBrowserCollection))]
    public sealed class AccessibilitySpecs(SelectorBrowserFixture browser)
    {
        /// <summary>
        /// Comprueba el orden de foco y la selección por teclado en escritorio y en móvil.
        /// </summary>
        [Theory(SkipType = typeof(SelectorBrowserFixture), SkipUnless = nameof(SelectorBrowserFixture.IsEnabled),
            Skip = SelectorBrowserFixture.EnableInstructions)]
        [InlineData(false, "Enter")]
        [InlineData(true, "Space")]
        public async Task SelectorCanBeCompletedUsingOnlyKeyboard(bool mobileViewport, string activationKey)
        {
            await using var context = await browser.NewContextAsync(mobileViewport);
            var page = await context.NewPageAsync();
            await page.GotoAsync(browser.DevelopmentUrl);
            var selector = page.GetByRole(AriaRole.Combobox, new() { Name = "Persona de prueba (campo obligatorio)" });
            var submit = page.GetByRole(AriaRole.Button, new() { Name = "Probar selección" });

            await Expect(page.Locator("html")).ToHaveAttributeAsync("lang", "es");
            await Expect(selector).ToBeVisibleAsync();
            await Expect(selector).ToHaveAccessibleDescriptionAsync("Confirma la selección para probar el formulario.");

            await page.Keyboard.PressAsync("Tab");
            await AssertVisibleFocusAsync(selector);
            await page.Keyboard.PressAsync("ArrowDown");
            await Expect(selector).ToHaveValueAsync(SelectorBrowserFixture.PersonaKey);
            await page.Keyboard.PressAsync("Tab");
            await AssertVisibleFocusAsync(submit);
            await page.Keyboard.PressAsync("Shift+Tab");
            await AssertVisibleFocusAsync(selector);
            await page.Keyboard.PressAsync("Tab");
            await page.Keyboard.PressAsync(activationKey);

            await Expect(page.GetByRole(AriaRole.Status).Filter(new() { HasTextString = "Selección de prueba:" }))
                .ToHaveTextAsync($"Selección de prueba: {SelectorBrowserFixture.PersonaName}.");
            Assert.True(await page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth <= innerWidth"));
            await AssertWcagAsync(page, "selección confirmada");
        }

        /// <summary>
        /// Audita carga, error, lista vacía y recuperación del selector construido.
        /// </summary>
        [Theory(SkipType = typeof(SelectorBrowserFixture), SkipUnless = nameof(SelectorBrowserFixture.IsEnabled),
            Skip = SelectorBrowserFixture.EnableInstructions)]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SelectorStatesPassAxeAndRetrySupportsKeyboard(bool mobileViewport)
        {
            await using var context = await browser.NewContextAsync(mobileViewport);
            var page = await context.NewPageAsync();
            var releaseError = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var requestCount = 0;
            await page.RouteAsync(SelectorBrowserFixture.PersonasRoute, async route =>
            {
                switch (Interlocked.Increment(ref requestCount))
                {
                    case 1:
                        await releaseError.Task;
                        await route.FulfillAsync(new()
                        {
                            Status = 404,
                            ContentType = "application/problem+json",
                            Body = """
                                {"type":"https://identity.example.test/errors/user-not-found","title":"Recurso no encontrado",
                                "status":404,"code":"identity.user.not_found","traceId":"accessibility-e2e"}
                                """,
                        });
                        break;
                    case 2:
                        await SelectorBrowserFixture.FulfillPersonasAsync(route, empty: true);
                        break;
                    default:
                        await SelectorBrowserFixture.FulfillPersonasAsync(route);
                        break;
                }
            });

            try
            {
                await page.GotoAsync(browser.BuildUrl);
                var status = page.GetByRole(AriaRole.Status);
                var retry = page.GetByRole(AriaRole.Button, new() { Name = "Volver a cargar la lista" });
                var heading = page.GetByRole(AriaRole.Heading, new() { Name = "Selector de personas de prueba" });

                await Expect(status).ToHaveTextAsync("Cargando personas de prueba…");
                await AssertWcagAsync(page, "carga");
                releaseError.SetResult();

                await Expect(page.GetByRole(AriaRole.Alert))
                    .ToHaveTextAsync("No se pudo cargar la lista de personas de prueba. Vuelve a intentarlo.");
                await AssertWcagAsync(page, "error del listado");
                await page.Keyboard.PressAsync("Tab");
                await AssertVisibleFocusAsync(retry);
                await page.Keyboard.PressAsync("Enter");
                await Expect(status).ToHaveTextAsync("No hay personas de prueba disponibles.");
                await Expect(heading).ToBeFocusedAsync();
                await AssertWcagAsync(page, "lista vacía");

                await page.Keyboard.PressAsync("Tab");
                await AssertVisibleFocusAsync(retry);
                await page.Keyboard.PressAsync("Space");
                var selector = page.GetByRole(AriaRole.Combobox);
                await Expect(selector).ToBeVisibleAsync();
                await page.Keyboard.PressAsync("Tab");
                await AssertVisibleFocusAsync(selector);
                await AssertWcagAsync(page, "listado recuperado");
            }
            finally
            {
                releaseError.TrySetResult();
            }
        }

        private async Task AssertWcagAsync(IPage page, string state)
        {
            if (!await page.EvaluateAsync<bool>("() => typeof window.axe !== 'undefined'"))
            {
                await page.AddScriptTagAsync(new() { Path = browser.AxePath });
            }

            var audit = await page.EvaluateAsync<JsonElement>("""
                async () => {
                    const result = await axe.run({
                        runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'] }
                    });
                    return {
                        contrastPassed: result.passes.some(rule => rule.id === 'color-contrast'),
                        violations: result.violations.map(rule => ({
                            id: rule.id,
                            nodes: rule.nodes.map(node => ({ target: node.target, summary: node.failureSummary }))
                        }))
                    };
                }
                """);

            var violations = audit.GetProperty("violations");
            Assert.True(violations.GetArrayLength() == 0, $"axe-core en {state}: {violations}");
            Assert.True(audit.GetProperty("contrastPassed").GetBoolean(), $"axe-core no verificó el contraste en {state}.");
        }

        private static async Task AssertVisibleFocusAsync(ILocator control)
        {
            await Expect(control).ToBeFocusedAsync();
            Assert.True(await control.EvaluateAsync<bool>("""
                element => {
                    const style = getComputedStyle(element);
                    return element.matches(':focus-visible') &&
                        style.outlineStyle !== 'none' && parseFloat(style.outlineWidth) > 0;
                }
                """), "El control enfocado por teclado debe mostrar un contorno visible.");
        }
    }
}
