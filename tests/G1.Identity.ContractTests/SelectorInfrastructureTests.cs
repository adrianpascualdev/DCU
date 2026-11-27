using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using G1.Identity.Mock;
using G1.Identity.Mock.Endpoints;
using G1.Identity.Mock.Extensions;
using G1.Identity.Mock.Persistence;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Comprueba la infraestructura DEV1 del selector: contexto opaco, validación con OpenIddict,
    /// antiforgery, CSP, archivos estáticos y restricción de entorno.
    /// </summary>
    public sealed class SelectorInfrastructureTests
    {
        private const string ClientId = @"selector-client";
        private static readonly string RequestUriPrefix = Constants.Selector.RequestUriPrefix;

        [Fact]
        public async Task RequestUriIdentifierRejectsManipulation()
        {
            var valid = RequestUriPrefix + "Abc123_-";
            Assert.True(AuthorizationContextBinder.TryGetIdentifier(valid, out var identifier));
            Assert.Equal(@"Abc123_-", identifier);

            foreach (var invalid in new[]
                     {
                         string.Empty,
                         @"https://example.com/authorize?x=1",
                         RequestUriPrefix,
                         RequestUriPrefix + new string('a', Constants.Selector.MaximumIdentifierLength + 1),
                         RequestUriPrefix + "abc/../admin",
                         RequestUriPrefix + "abc?client_id=other",
                         RequestUriPrefix + "abc#fragment",
                         "URN:IETF:PARAMS:OAUTH:REQUEST_URI:abc",
                     })
            {
                Assert.False(
                    AuthorizationContextBinder.TryGetIdentifier(invalid, out _),
                    $"La referencia {invalid} no debería aceptarse");
            }
        }

        [Fact]
        public async Task BindingIsTiedToBrowserExpiresAndIsSingleUse()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var time = new MutableTimeProvider(DateTimeOffset.UtcNow);
            var binder = new AuthorizationContextBinder(time);
            var requestUri = RequestUriPrefix + @"flow-one";

            var initiator = new DefaultHttpContext();
            Assert.True(await binder.BindContextAsync(
                initiator,
                new OpenIddictRequest { RequestUri = requestUri, ClientId = ClientId },
                cancellationToken));

            var flowCookie = GetCookieHeader(initiator.Response.Headers.SetCookie.ToString());
            var browser = CreateContextWithCookies(flowCookie);
            var otherBrowser = CreateContextWithCookies($"{Constants.Selector.FlowCookieName}=tampered");

            Assert.True(await binder.MatchesAsync(browser, requestUri, ClientId, cancellationToken));
            Assert.False(await binder.MatchesAsync(otherBrowser, requestUri, ClientId, cancellationToken));
            Assert.False(await binder.MatchesAsync(browser, RequestUriPrefix + @"flow-two", ClientId, cancellationToken));
            Assert.False(await binder.MatchesAsync(browser, requestUri, @"another-client", cancellationToken));

            Assert.True(await binder.TryConsumeAsync(browser, requestUri, ClientId, cancellationToken));
            Assert.False(await binder.TryConsumeAsync(browser, requestUri, ClientId, cancellationToken));
            Assert.False(await binder.MatchesAsync(browser, requestUri, ClientId, cancellationToken));

            var expiringUri = RequestUriPrefix + @"flow-expiring";
            var expiringInitiator = new DefaultHttpContext();
            Assert.True(await binder.BindContextAsync(
                expiringInitiator,
                new OpenIddictRequest { RequestUri = expiringUri, ClientId = ClientId },
                cancellationToken));
            time.Advance(Constants.Selector.BindingLifetime + TimeSpan.FromSeconds(1));
            Assert.False(await binder.MatchesAsync(
                CreateContextWithCookies(GetCookieHeader(expiringInitiator.Response.Headers.SetCookie.ToString())),
                expiringUri,
                ClientId,
                cancellationToken));
        }

        [Fact]
        public async Task ValidatorUsesOpenIddictTokenStoreAndFailsClosed()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var time = new MutableTimeProvider(DateTimeOffset.UtcNow);
            await using var fixture = await TokenFixture.CreateAsync(time, cancellationToken);

            var requestUri = RequestUriPrefix + fixture.TokenId;
            Assert.True(await fixture.Validator.IsValidAsync(requestUri, ClientId, cancellationToken));
            Assert.False(await fixture.Validator.IsValidAsync(requestUri, @"other-client", cancellationToken));
            Assert.False(await fixture.Validator.IsValidAsync(
                RequestUriPrefix + @"unknown-token",
                ClientId,
                cancellationToken));
            Assert.False(await fixture.Validator.IsValidAsync(@"not-a-request-uri", ClientId, cancellationToken));

            Assert.True(await fixture.Tokens.TryRedeemAsync(fixture.Token, cancellationToken));
            Assert.False(await fixture.Validator.IsValidAsync(requestUri, ClientId, cancellationToken));

            var descriptor = new OpenIddictTokenDescriptor();
            await fixture.Tokens.PopulateAsync(descriptor, fixture.Token, cancellationToken);
            descriptor.Status = OpenIddictConstants.Statuses.Valid;
            descriptor.ExpirationDate = time.GetUtcNow().AddMinutes(-1);
            await fixture.Tokens.UpdateAsync(fixture.Token, descriptor, cancellationToken);
            Assert.False(await fixture.Validator.IsValidAsync(requestUri, ClientId, cancellationToken));
        }

        [Fact]
        public async Task ValidatorFailsClosedWhenOpenIddictIsNotRegistered()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<TimeProvider>(TimeProvider.System);
            services.AddScoped<IAuthorizationContextValidator, AuthorizationContextValidator>();
            await using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var validator = scope.ServiceProvider.GetRequiredService<IAuthorizationContextValidator>();
            Assert.False(await validator.IsValidAsync(RequestUriPrefix + @"anything", ClientId, cancellationToken));
        }

        [Theory]
        [InlineData(@"Development")]
        [InlineData(@"Testing")]
        public async Task SelectorRendersBoundContextWithHardenedHeaders(string environment)
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var time = new MutableTimeProvider(DateTimeOffset.UtcNow);
            await using var host = await SelectorHost.CreateAsync(environment, time, cancellationToken);
            var fixture = await TokenFixture.CreateAsync(host.App.Services, time, cancellationToken);
            var requestUri = RequestUriPrefix + fixture.TokenId;
            var flowCookie = await host.BindAsync(requestUri, cancellationToken);

            using var get = new HttpRequestMessage(
                HttpMethod.Get,
                $"{Constants.Paths.Selector}?clientId={ClientId}&requestUri={Uri.EscapeDataString(requestUri)}");
            get.Headers.Add(@"Cookie", flowCookie);
            using var rendered = await host.Client.SendAsync(get, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, rendered.StatusCode);
            Assert.Equal(
                Constants.SecurityHeaders.SelectorContentSecurityPolicy,
                Assert.Single(rendered.Headers.GetValues(Constants.Headers.ContentSecurityPolicy)));
            Assert.DoesNotContain(@"unsafe-inline", rendered.Headers.GetValues(Constants.Headers.ContentSecurityPolicy));
            Assert.Contains(
                Constants.Headers.NoStore,
                Assert.Single(rendered.Headers.GetValues(Constants.Headers.CacheControl)),
                StringComparison.Ordinal);

            var html = await rendered.Content.ReadAsStringAsync(cancellationToken);
            Assert.Contains($"{Constants.Selector.RequestUriAttribute}=\"{WebUtility.HtmlEncode(requestUri)}\"", html);
            Assert.Contains($"{Constants.Selector.ClientIdAttribute}=\"{ClientId}\"", html);
            var antiforgery = Regex.Match(
                html,
                $"{Constants.Selector.AntiforgeryAttribute}=\"([^\"]+)\"");
            Assert.True(antiforgery.Success);
            Assert.False(string.IsNullOrWhiteSpace(antiforgery.Groups[1].Value));
            Assert.Contains($@"src=""{Constants.Paths.SelectorAssets}/assets/", html);

            var cookies = MergeCookies(flowCookie, rendered.Headers.GetValues(@"Set-Cookie"));
            using var post = new HttpRequestMessage(HttpMethod.Post, Constants.Paths.Selector)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    [@"requestUri"] = requestUri,
                    [@"clientId"] = ClientId,
                    [@"personaKey"] = @"persona-activa",
                }),
            };
            post.Headers.Add(@"Cookie", cookies);
            post.Headers.Add(Constants.Headers.AntiforgeryToken, antiforgery.Groups[1].Value);
            using var selected = await host.Client.SendAsync(post, cancellationToken);

            Assert.Equal(HttpStatusCode.SeeOther, selected.StatusCode);
            var location = Assert.Single(selected.Headers.GetValues(@"Location"));
            Assert.StartsWith(Constants.Paths.Authorize, location, StringComparison.Ordinal);
            Assert.Contains($@"client_id={ClientId}", location, StringComparison.Ordinal);
            Assert.Contains($@"request_uri={Uri.EscapeDataString(requestUri)}", location, StringComparison.Ordinal);

            // El mismo contexto no puede utilizarse dos veces, aunque se reenvíe el antiforgery válido.
            using var replay = new HttpRequestMessage(HttpMethod.Post, Constants.Paths.Selector)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    [@"requestUri"] = requestUri,
                    [@"clientId"] = ClientId,
                    [@"personaKey"] = @"persona-activa",
                }),
            };
            replay.Headers.Add(@"Cookie", cookies);
            replay.Headers.Add(Constants.Headers.AntiforgeryToken, antiforgery.Groups[1].Value);
            using var replayed = await host.Client.SendAsync(replay, cancellationToken);
            Assert.Equal(HttpStatusCode.BadRequest, replayed.StatusCode);
        }

        [Fact]
        public async Task SelectorRejectsInvalidContextAndReturnUrl()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var time = new MutableTimeProvider(DateTimeOffset.UtcNow);
            await using var host = await SelectorHost.CreateAsync(@"Testing", time, cancellationToken);
            var fixture = await TokenFixture.CreateAsync(host.App.Services, time, cancellationToken);
            var requestUri = RequestUriPrefix + fixture.TokenId;

            // Sin vinculación con el navegador: la sola referencia opaca no basta.
            using var unbound = new HttpRequestMessage(
                HttpMethod.Get,
                $"{Constants.Paths.Selector}?clientId={ClientId}&requestUri={Uri.EscapeDataString(requestUri)}");
            using var unboundResponse = await host.Client.SendAsync(unbound, cancellationToken);
            Assert.Equal(HttpStatusCode.BadRequest, unboundResponse.StatusCode);

            var flowCookie = await host.BindAsync(requestUri, cancellationToken);
            using var get = new HttpRequestMessage(
                HttpMethod.Get,
                $"{Constants.Paths.Selector}?clientId={ClientId}&requestUri={Uri.EscapeDataString(requestUri)}&returnUrl=/admin");
            get.Headers.Add(@"Cookie", flowCookie);
            using var rendered = await host.Client.SendAsync(get, cancellationToken);
            Assert.Equal(HttpStatusCode.OK, rendered.StatusCode);

            var html = await rendered.Content.ReadAsStringAsync(cancellationToken);
            var antiforgery = Regex.Match(
                html,
                $"{Constants.Selector.AntiforgeryAttribute}=\"([^\"]+)\"");
            Assert.True(antiforgery.Success);

            using var post = new HttpRequestMessage(HttpMethod.Post, Constants.Paths.Selector)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    [@"requestUri"] = requestUri,
                    [@"clientId"] = ClientId,
                    [@"personaKey"] = @"persona-activa",
                    [@"returnUrl"] = @"https://evil.example/phishing",
                }),
            };
            post.Headers.Add(@"Cookie", MergeCookies(flowCookie, rendered.Headers.GetValues(@"Set-Cookie")));
            post.Headers.Add(Constants.Headers.AntiforgeryToken, antiforgery.Groups[1].Value);
            using var rejected = await host.Client.SendAsync(post, cancellationToken);
            Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);

            // El contexto rechazado sigue intacto: un POST legítimo posterior debe funcionar.
            using var retry = new HttpRequestMessage(HttpMethod.Post, Constants.Paths.Selector)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    [@"requestUri"] = requestUri,
                    [@"clientId"] = ClientId,
                    [@"personaKey"] = @"persona-activa",
                }),
            };
            retry.Headers.Add(@"Cookie", MergeCookies(flowCookie, rendered.Headers.GetValues(@"Set-Cookie")));
            retry.Headers.Add(Constants.Headers.AntiforgeryToken, antiforgery.Groups[1].Value);
            using var accepted = await host.Client.SendAsync(retry, cancellationToken);
            Assert.Equal(HttpStatusCode.SeeOther, accepted.StatusCode);
        }

        [Fact]
        public async Task SelectorFailsClosedWhenPersonaValidatorIsMissing()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var time = new MutableTimeProvider(DateTimeOffset.UtcNow);
            await using var host = await SelectorHost.CreateAsync(
                @"Testing",
                time,
                cancellationToken,
                registerPersonaValidator: false);
            var fixture = await TokenFixture.CreateAsync(host.App.Services, time, cancellationToken);
            var requestUri = RequestUriPrefix + fixture.TokenId;
            var flowCookie = await host.BindAsync(requestUri, cancellationToken);

            using var get = new HttpRequestMessage(
                HttpMethod.Get,
                $"{Constants.Paths.Selector}?clientId={ClientId}&requestUri={Uri.EscapeDataString(requestUri)}");
            get.Headers.Add(@"Cookie", flowCookie);
            using var rendered = await host.Client.SendAsync(get, cancellationToken);
            Assert.Equal(HttpStatusCode.OK, rendered.StatusCode);

            var html = await rendered.Content.ReadAsStringAsync(cancellationToken);
            var antiforgery = Regex.Match(html, $"{Constants.Selector.AntiforgeryAttribute}=\"([^\"]+)\"");
            Assert.True(antiforgery.Success);

            using var post = new HttpRequestMessage(HttpMethod.Post, Constants.Paths.Selector)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    [@"requestUri"] = requestUri,
                    [@"clientId"] = ClientId,
                    [@"personaKey"] = @"persona-activa",
                }),
            };
            post.Headers.Add(@"Cookie", MergeCookies(flowCookie, rendered.Headers.GetValues(@"Set-Cookie")));
            post.Headers.Add(Constants.Headers.AntiforgeryToken, antiforgery.Groups[1].Value);
            using var response = await host.Client.SendAsync(post, cancellationToken);

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.False(
                response.Headers.TryGetValues(@"Set-Cookie", out var cookies)
                && cookies.Any(cookie => cookie.Contains(@"__Host-g1.session", StringComparison.Ordinal)));
        }

        [Fact]
        public async Task SelectorRejectsPostWithoutAntiforgeryToken()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var time = new MutableTimeProvider(DateTimeOffset.UtcNow);
            await using var host = await SelectorHost.CreateAsync(@"Testing", time, cancellationToken);
            var fixture = await TokenFixture.CreateAsync(host.App.Services, time, cancellationToken);
            var requestUri = RequestUriPrefix + fixture.TokenId;
            var flowCookie = await host.BindAsync(requestUri, cancellationToken);

            using var post = new HttpRequestMessage(HttpMethod.Post, Constants.Paths.Selector)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    [@"requestUri"] = requestUri,
                    [@"clientId"] = ClientId,
                    [@"personaKey"] = @"persona-activa",
                }),
            };
            post.Headers.Add(@"Cookie", flowCookie);
            using var response = await host.Client.SendAsync(post, cancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SelectorAndAssetsAreNotPublishedInProduction()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var time = new MutableTimeProvider(DateTimeOffset.UtcNow);
            await using var host = await SelectorHost.CreateAsync(@"Production", time, cancellationToken);

            foreach (var path in new[]
                     {
                         Constants.Paths.Selector,
                         $"{Constants.Paths.Selector}?clientId=x&requestUri=y",
                         $"{Constants.Paths.SelectorAssets}/assets/probe.js",
                     })
            {
                using var response = await host.Client.GetAsync(path, cancellationToken);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task SelectorAssetsAreServedOnlyInNonProduction()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var time = new MutableTimeProvider(DateTimeOffset.UtcNow);
            await using var host = await SelectorHost.CreateAsync(@"Testing", time, cancellationToken);

            using var response = await host.Client.GetAsync(
                $"{Constants.Paths.SelectorAssets}/assets/probe.js",
                cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(@"console.log('probe');", await response.Content.ReadAsStringAsync(cancellationToken));
            Assert.Equal(
                Constants.SecurityHeaders.SelectorContentSecurityPolicy,
                Assert.Single(response.Headers.GetValues(Constants.Headers.ContentSecurityPolicy)));
        }

        private static DefaultHttpContext CreateContextWithCookies(string cookies)
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Cookie = cookies;
            return context;
        }

        private static string GetCookieHeader(string setCookieHeader)
        {
            var match = Regex.Match(setCookieHeader, @"([^=;,\s]+)=([^;]+)");
            Assert.True(match.Success, "La respuesta debería contener una cookie de vinculación");
            return $"{match.Groups[1].Value}={match.Groups[2].Value}";
        }

        private static string MergeCookies(string first, IEnumerable<string> setCookieHeaders)
        {
            var cookies = new List<string> { first };
            foreach (var header in setCookieHeaders)
            {
                var match = Regex.Match(header, @"^\s*([^=;,\s]+)=([^;]+)");
                if (match.Success)
                {
                    cookies.Add($"{match.Groups[1].Value}={match.Groups[2].Value}");
                }
            }

            return string.Join("; ", cookies);
        }

        private sealed class MutableTimeProvider : TimeProvider
        {
            private DateTimeOffset _now;

            internal MutableTimeProvider(DateTimeOffset now) => _now = now;

            internal void Advance(TimeSpan value) => _now += value;

            public override DateTimeOffset GetUtcNow() => _now;
        }

        private sealed class StubPersonaValidator : IPersonaSelectionValidator
        {
            public ValueTask<ClaimsPrincipal?> ValidateAsync(string personaKey, CancellationToken cancellationToken)
            {
                if (!string.Equals(personaKey, @"persona-activa", StringComparison.Ordinal))
                {
                    return ValueTask.FromResult<ClaimsPrincipal?>(null);
                }

                var identity = new ClaimsIdentity(
                    new[] { new Claim(OpenIddictConstants.Claims.Subject, @"persona-activa") },
                    @"MockPersona");
                return ValueTask.FromResult<ClaimsPrincipal?>(new ClaimsPrincipal(identity));
            }
        }

        private sealed class TokenFixture : IAsyncDisposable
        {
            private IServiceScope? _scope;
            private ServiceProvider? _ownedProvider;

            private TokenFixture(
                IOpenIddictApplicationManager applications,
                IOpenIddictTokenManager tokens,
                IAuthorizationContextValidator validator,
                string tokenId,
                object token)
            {
                Applications = applications;
                Tokens = tokens;
                Validator = validator;
                TokenId = tokenId;
                Token = token;
            }

            internal IOpenIddictApplicationManager Applications { get; }

            internal IOpenIddictTokenManager Tokens { get; }

            internal IAuthorizationContextValidator Validator { get; }

            internal string TokenId { get; }

            internal object Token { get; }

            internal static async Task<TokenFixture> CreateAsync(
                TimeProvider time,
                CancellationToken cancellationToken)
            {
                var services = new ServiceCollection();
                services.AddLogging();
                services.AddIdentityPersistence(@"SelectorValidator_" + Guid.NewGuid().ToString("N"));
                services.AddSingleton(time);
                services.AddScoped<IAuthorizationContextValidator, AuthorizationContextValidator>();
                var provider = services.BuildServiceProvider();
                var fixture = await CreateAsync(provider, time, cancellationToken);
                fixture._ownedProvider = provider;
                return fixture;
            }

            internal static async Task<TokenFixture> CreateAsync(
                IServiceProvider services,
                TimeProvider time,
                CancellationToken cancellationToken)
            {
                var scope = services.CreateScope();
                var scoped = scope.ServiceProvider;
                var applications = scoped.GetRequiredService<IOpenIddictApplicationManager>();
                var tokens = scoped.GetRequiredService<IOpenIddictTokenManager>();

                var application = await applications.CreateAsync(
                    new OpenIddictApplicationDescriptor
                    {
                        ApplicationType = OpenIddictConstants.ApplicationTypes.Web,
                        ClientId = ClientId,
                        ClientType = OpenIddictConstants.ClientTypes.Public,
                        ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                        DisplayName = @"Selector de pruebas",
                    },
                    cancellationToken);
                var applicationId = await applications.GetIdAsync(application, cancellationToken);

                var principal = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(OpenIddictConstants.Claims.Subject, @"usuario-prueba") },
                    @"OpenIddict"));
                var token = await tokens.CreateAsync(
                    new OpenIddictTokenDescriptor
                    {
                        ApplicationId = applicationId,
                        ExpirationDate = time.GetUtcNow().Add(Constants.Selector.TokenLifetime),
                        Principal = principal,
                        Status = OpenIddictConstants.Statuses.Valid,
                        Subject = @"usuario-prueba",
                        Type = OpenIddictConstants.TokenTypeIdentifiers.Private.RequestToken,
                    },
                    cancellationToken);
                var tokenId = await tokens.GetIdAsync(token, cancellationToken);

                var fixture = new TokenFixture(
                    applications,
                    tokens,
                    scoped.GetRequiredService<IAuthorizationContextValidator>(),
                    tokenId!,
                    token);
                fixture._scope = scope;
                return fixture;
            }

            public async ValueTask DisposeAsync()
            {
                _scope?.Dispose();
                if (_ownedProvider is not null)
                {
                    await _ownedProvider.DisposeAsync();
                }
            }
        }

        private sealed class SelectorHost : IAsyncDisposable
        {
            private readonly WebApplication _app;

            private SelectorHost(WebApplication app, HttpClient client, CancellationToken cancellationToken)
            {
                _app = app;
                Client = client;
                CancellationToken = cancellationToken;
            }

            internal WebApplication App => _app;

            internal HttpClient Client { get; }

            private CancellationToken CancellationToken { get; }

            internal async Task<string> BindAsync(string requestUri, CancellationToken cancellationToken)
            {
                using var scope = _app.Services.CreateScope();
                var binder = scope.ServiceProvider.GetRequiredService<IAuthorizationContextBinder>();
                var context = new DefaultHttpContext();
                Assert.True(await binder.BindContextAsync(
                    context,
                    new OpenIddictRequest { RequestUri = requestUri, ClientId = ClientId },
                    cancellationToken));
                return GetCookieHeader(context.Response.Headers.SetCookie.ToString());
            }

            internal static async Task<SelectorHost> CreateAsync(
                string environment,
                MutableTimeProvider time,
                CancellationToken cancellationToken,
                bool registerPersonaValidator = true)
            {
                var root = AppContext.BaseDirectory;
                var selectorRoot = Path.Combine(root, @"wwwroot", @"selector");
                Directory.CreateDirectory(Path.Combine(selectorRoot, @"assets"));
                await File.WriteAllTextAsync(
                    Path.Combine(selectorRoot, @"index.html"),
                    @"<!doctype html><html><head><title>Selector</title></head><body><div id=""root""></div><script type=""module"" src=""./assets/index.js""></script></body></html>",
                    cancellationToken);
                await File.WriteAllTextAsync(
                    Path.Combine(selectorRoot, @"assets", @"probe.js"),
                    @"console.log('probe');",
                    cancellationToken);

                var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                {
                    Args = [],
                    ContentRootPath = root,
                    EnvironmentName = environment,
                });
                builder.Configuration.AddJsonFile(
                    Path.Combine(root, @"appsettings.mock.json"),
                    optional: false);
                builder.WebHost.UseUrls(@"http://127.0.0.1:0");
                builder.Configure();
                builder.Services.AddSingleton<TimeProvider>(time);
                builder.Services.AddIdentityPersistence(@"SelectorHost_" + Guid.NewGuid().ToString("N"));
                builder.Services.AddAuthentication().AddCookie();
                if (registerPersonaValidator)
                {
                    builder.Services.AddScoped<IPersonaSelectionValidator, StubPersonaValidator>();
                }

                // El host de prueba usa HTTP; la configuración real exige HTTPS en producción.
                builder.Services.Configure<AntiforgeryOptions>(
                    options => options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest);

                var app = builder.Build();
                app.UseMiddlewares().UseRegisterEndpointDefinitions();
                await app.StartAsync(cancellationToken);

                var handler = new HttpClientHandler
                {
                    UseCookies = false,
                    AllowAutoRedirect = false,
                };
                var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri(app.Urls.Single()),
                };
                return new SelectorHost(app, client, cancellationToken);
            }

            public async ValueTask DisposeAsync()
            {
                Client.Dispose();
                await _app.StopAsync(CancellationToken);
                await _app.DisposeAsync();
            }
        }
    }
}
