using System.Collections.Concurrent;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace G1.Identity.E2E
{
    /// <summary>
    /// Comprueba el almacenamiento del selector aislado; no inspecciona cookies de sesión del host.
    /// </summary>
    [Collection(nameof(SelectorBrowserCollection))]
    public sealed class TokenStorageSpecs(SelectorBrowserFixture browser)
    {
        /// <summary>
        /// Comprueba el almacenamiento antes y después de confirmar una persona en la demo.
        /// </summary>
        [Fact(SkipType = typeof(SelectorBrowserFixture), SkipUnless = nameof(SelectorBrowserFixture.IsEnabled),
            Skip = SelectorBrowserFixture.EnableInstructions)]
        public async Task DevelopmentSelectionDoesNotStoreTokensOrCallOidc()
        {
            await using var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            var oidcRequests = RecordOidcRequests(page);
            await page.GotoAsync(browser.DevelopmentUrl);
            var selector = page.GetByRole(AriaRole.Combobox);
            await Expect(selector).ToBeVisibleAsync();
            await AssertNoClientStorageAsync(page, context);

            await selector.SelectOptionAsync(SelectorBrowserFixture.PersonaKey);
            await AssertNoClientStorageAsync(page, context);
            await page.GetByRole(AriaRole.Button, new() { Name = "Probar selección" }).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Status).Filter(new() { HasTextString = "Selección de prueba:" }))
                .ToHaveTextAsync($"Selección de prueba: {SelectorBrowserFixture.PersonaName}.");
            await AssertNoClientStorageAsync(page, context);
            Assert.Empty(oidcRequests);
        }

        /// <summary>
        /// Comprueba la selección en el build sin adaptador de autorización.
        /// </summary>
        [Fact(SkipType = typeof(SelectorBrowserFixture), SkipUnless = nameof(SelectorBrowserFixture.IsEnabled),
            Skip = SelectorBrowserFixture.EnableInstructions)]
        public async Task BuiltSelectorDoesNotContinueAuthorizationWhenCallbackIsMissing()
        {
            await using var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            var oidcRequests = RecordOidcRequests(page);
            await page.RouteAsync(SelectorBrowserFixture.PersonasRoute, route => SelectorBrowserFixture.FulfillPersonasAsync(route));
            await page.GotoAsync(browser.BuildUrl);
            var selector = page.GetByRole(AriaRole.Combobox);
            await Expect(selector).ToBeVisibleAsync();
            await AssertNoClientStorageAsync(page, context);

            await selector.SelectOptionAsync(SelectorBrowserFixture.PersonaKey);
            await Expect(selector).ToHaveValueAsync(SelectorBrowserFixture.PersonaKey);
            await Expect(selector).ToHaveAccessibleDescriptionAsync("Todavía no puedes continuar con la autorización desde este selector.");
            await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Continuar", Exact = true })).ToHaveCountAsync(0);
            await AssertNoClientStorageAsync(page, context);
            Assert.Empty(oidcRequests);
        }

        private static ConcurrentQueue<string> RecordOidcRequests(IPage page)
        {
            var requests = new ConcurrentQueue<string>();
            page.Request += (_, request) =>
            {
                var path = new Uri(request.Url).AbsolutePath;
                if (path is "/connect/authorize" or "/connect/token")
                {
                    requests.Enqueue(path);
                }
            };
            return requests;
        }

        private static async Task AssertNoClientStorageAsync(IPage page, IBrowserContext context)
        {
            Assert.Equal(0, await page.EvaluateAsync<int>("() => localStorage.length"));
            Assert.Equal(0, await page.EvaluateAsync<int>("() => sessionStorage.length"));
            Assert.Empty(await context.CookiesAsync());
        }
    }
}
