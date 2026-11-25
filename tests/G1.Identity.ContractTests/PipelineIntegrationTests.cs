using System.Net;
using System.Text.Json;
using G1.Identity.Mock;
using G1.Identity.Mock.Exceptions;
using G1.Identity.Mock.Extensions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Verifica el pipeline T014 con un host Kestrel en proceso y peticiones HTTP reales.
    /// </summary>
    public sealed class PipelineIntegrationTests
    {
        [Fact]
        public async Task CorrelationIdIsEchoedAndGeneratedWhenMissing()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                mapEndpoints: static app => app.MapGet("/ping", () => TypedResults.Ok("pong")),
                cancellationToken: cancellationToken);

            using var withHeader = new HttpRequestMessage(HttpMethod.Get, "/ping");
            withHeader.Headers.TryAddWithoutValidation(Constants.Headers.CorrelationId, "correlacion-prueba-123");
            using var echoed = await host.Client.SendAsync(withHeader, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, echoed.StatusCode);
            Assert.Equal(@"correlacion-prueba-123", Assert.Single(echoed.Headers.GetValues(Constants.Headers.CorrelationId)));

            using var withoutHeader = await host.Client.GetAsync("/ping", cancellationToken);
            var generated = Assert.Single(withoutHeader.Headers.GetValues(Constants.Headers.CorrelationId));
            Assert.False(string.IsNullOrWhiteSpace(generated));
            Assert.NotEqual(@"correlacion-prueba-123", generated);
        }

        [Fact]
        public async Task SecurityHeadersAreAppliedAndSwaggerGetsContentSecurityPolicy()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                mapEndpoints: static app =>
                {
                    app.MapGet("/ping", () => TypedResults.Ok("pong"));
                    app.MapGet("/swagger/index.html", () => TypedResults.Content("<html></html>", "text/html"));
                },
                cancellationToken: cancellationToken);

            using var response = await host.Client.GetAsync("/ping", cancellationToken);

            Assert.Equal(Constants.Headers.NoSniff, Assert.Single(response.Headers.GetValues(Constants.Headers.ContentTypeOptions)));
            Assert.Equal(Constants.Headers.Deny, Assert.Single(response.Headers.GetValues(Constants.Headers.FrameOptions)));
            Assert.Equal(Constants.Headers.NoReferrer, Assert.Single(response.Headers.GetValues(Constants.Headers.ReferrerPolicy)));
            Assert.False(response.Headers.Contains(Constants.Headers.ContentSecurityPolicy));

            using var swagger = await host.Client.GetAsync("/swagger/index.html", cancellationToken);
            Assert.Equal(HttpStatusCode.OK, swagger.StatusCode);
            var policy = Assert.Single(swagger.Headers.GetValues(Constants.Headers.ContentSecurityPolicy));
            Assert.Contains(@"default-src 'self'", policy, StringComparison.Ordinal);
            Assert.Contains(@"frame-ancestors 'none'", policy, StringComparison.Ordinal);
            Assert.Equal(Constants.Headers.NoSniff, Assert.Single(swagger.Headers.GetValues(Constants.Headers.ContentTypeOptions)));
        }

        [Fact]
        public async Task CorsAllowsOnlyConfiguredOriginWithCredentials()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var overrides = new Dictionary<string, string?>
            {
                [@"Identity:Cors:AllowedOrigins:0"] = @"https://localhost:3000",
            };

            await using var host = await CreateHostAsync(
                overrides,
                static app => app.MapGet("/ping", () => TypedResults.Ok("pong")),
                cancellationToken: cancellationToken);

            using var allowedRequest = new HttpRequestMessage(HttpMethod.Get, "/ping");
            allowedRequest.Headers.TryAddWithoutValidation(@"Origin", @"https://localhost:3000");
            using var allowed = await host.Client.SendAsync(allowedRequest, cancellationToken);

            Assert.Equal(@"https://localhost:3000", Assert.Single(allowed.Headers.GetValues(@"Access-Control-Allow-Origin")));
            Assert.Equal(@"true", Assert.Single(allowed.Headers.GetValues(@"Access-Control-Allow-Credentials")));

            using var deniedRequest = new HttpRequestMessage(HttpMethod.Get, "/ping");
            deniedRequest.Headers.TryAddWithoutValidation(@"Origin", @"https://malicioso.example");
            using var denied = await host.Client.SendAsync(deniedRequest, cancellationToken);

            Assert.False(denied.Headers.Contains(@"Access-Control-Allow-Origin"));
        }

        [Fact]
        public async Task CorsPreflightAdvertisesConfiguredOriginAndAntiforgeryHeader()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var overrides = new Dictionary<string, string?>
            {
                [@"Identity:Cors:AllowedOrigins:0"] = @"https://localhost:3000",
            };

            await using var host = await CreateHostAsync(
                overrides,
                static app => app.MapPost("/comando", () => TypedResults.Ok("ok")),
                cancellationToken: cancellationToken);

            using var preflight = new HttpRequestMessage(HttpMethod.Options, "/comando");
            preflight.Headers.TryAddWithoutValidation(@"Origin", @"https://localhost:3000");
            preflight.Headers.TryAddWithoutValidation(@"Access-Control-Request-Method", @"POST");
            preflight.Headers.TryAddWithoutValidation(@"Access-Control-Request-Headers", @"content-type, x-xsrf-token");
            using var response = await host.Client.SendAsync(preflight, cancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(@"https://localhost:3000", Assert.Single(response.Headers.GetValues(@"Access-Control-Allow-Origin")));
            Assert.Equal(@"true", Assert.Single(response.Headers.GetValues(@"Access-Control-Allow-Credentials")));
            Assert.Contains(
                @"x-xsrf-token",
                response.Headers.GetValues(@"Access-Control-Allow-Headers").Single(),
                StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RateLimitingReturnsProblemJsonWithRetryAfter()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                mapEndpoints: static app => app
                    .MapGet("/resumen", () => TypedResults.Ok("ok"))
                    .RequireRateLimiting(Constants.Policies.Summary),
                cancellationToken: cancellationToken);

            for (var attempt = 1; attempt <= Constants.RateLimits.SummaryPermitLimit; attempt++)
            {
                using var allowed = await host.Client.GetAsync("/resumen", cancellationToken);
                Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
            }

            using var limited = await host.Client.GetAsync("/resumen", cancellationToken);

            Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
            Assert.Equal(Constants.ContentTypes.ProblemJson, limited.Content.Headers.ContentType?.MediaType);

            using var body = JsonDocument.Parse(await limited.Content.ReadAsStringAsync(cancellationToken));
            Assert.Equal(ErrorCatalog.GetType(Constants.ErrorCodes.RateLimited), body.RootElement.GetProperty(@"type").GetString());
            Assert.Equal(Constants.ErrorCodes.RateLimited, body.RootElement.GetProperty(@"code").GetString());
            Assert.Equal(429, body.RootElement.GetProperty(@"status").GetInt32());
            Assert.False(string.IsNullOrWhiteSpace(body.RootElement.GetProperty(@"traceId").GetString()));

            var retryAfter = Assert.Single(limited.Headers.GetValues(Constants.Headers.RetryAfter));
            Assert.True(int.TryParse(retryAfter, out var seconds));
            Assert.True(seconds > 0);
        }

        [Fact]
        public void AntiforgeryUsesSecureHostCookieAndDedicatedHeader()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = [],
                ContentRootPath = AppContext.BaseDirectory,
                EnvironmentName = @"Production",
            });
            builder.Configure();

            using var provider = builder.Services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<AntiforgeryOptions>>().Value;

            Assert.Equal(Constants.Headers.AntiforgeryToken, options.HeaderName);
            Assert.Equal(Constants.Antiforgery.CookieName, options.Cookie.Name);
            Assert.True(options.Cookie.HttpOnly);
            Assert.Equal(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
            Assert.Equal(SameSiteMode.Lax, options.Cookie.SameSite);
        }

        [Fact]
        public async Task AntiforgeryRequiresValidHeaderAndCookieToken()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                mapEndpoints: static app =>
                {
                    app.MapGet("/token", (HttpContext context, IAntiforgery antiforgery) =>
                        TypedResults.Ok(antiforgery.GetAndStoreTokens(context).RequestToken));

                    // El middleware no cortocircuita: publica el resultado en IAntiforgeryValidationFeature.
                    app.MapPost("/protegido", (HttpContext context) =>
                        {
                            IResult result = context.Features.Get<IAntiforgeryValidationFeature>() is { IsValid: true }
                                ? TypedResults.Ok("ok")
                                : TypedResults.BadRequest();
                            return result;
                        })
                        .WithMetadata(new RequireAntiforgeryMetadata());
                },
                // El host de prueba usa HTTP; la configuración real exige HTTPS en producción.
                configureServices: static services => services.Configure<AntiforgeryOptions>(
                    options => options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest),
                cancellationToken: cancellationToken);

            using var tokenResponse = await host.Client.GetAsync("/token", cancellationToken);
            Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);

            var setCookie = Assert.Single(tokenResponse.Headers.GetValues(@"Set-Cookie"));
            Assert.Contains(@"__Host-g1.antiforgery=", setCookie, StringComparison.Ordinal);
            Assert.Contains(@"httponly", setCookie, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(@"samesite=lax", setCookie, StringComparison.OrdinalIgnoreCase);

            var cookieValue = setCookie.Split(';')[0];
            var requestToken = JsonSerializer.Deserialize<string>(
                await tokenResponse.Content.ReadAsStringAsync(cancellationToken));
            Assert.False(string.IsNullOrWhiteSpace(requestToken));

            using var rejected = await host.Client.PostAsync("/protegido", content: null, cancellationToken);
            Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);

            using var request = new HttpRequestMessage(HttpMethod.Post, "/protegido");
            request.Headers.TryAddWithoutValidation(@"Cookie", cookieValue);
            request.Headers.TryAddWithoutValidation(Constants.Headers.AntiforgeryToken, requestToken!);
            using var accepted = await host.Client.SendAsync(request, cancellationToken);

            Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        }

        [Fact]
        public async Task UnhandledExceptionReturnsProblemJsonWithoutInternalDetails()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                mapEndpoints: static app => app.MapGet("/fallo", ThrowUnexpectedException),
                cancellationToken: cancellationToken);

            using var response = await host.Client.GetAsync("/fallo", cancellationToken);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(Constants.ContentTypes.ProblemJson, response.Content.Headers.ContentType?.MediaType);
            Assert.True(response.Headers.Contains(Constants.Headers.CorrelationId));

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            Assert.DoesNotContain(@"SECRETO_INTERNO", body, StringComparison.Ordinal);
            Assert.DoesNotContain(@"InvalidOperationException", body, StringComparison.Ordinal);

            using var problem = JsonDocument.Parse(body);
            Assert.Equal(ErrorCatalog.GetType(Constants.ErrorCodes.InternalError), problem.RootElement.GetProperty(@"type").GetString());
            Assert.Equal(ErrorCatalog.GetTitle(Constants.ErrorCodes.InternalError), problem.RootElement.GetProperty(@"title").GetString());
            Assert.Equal(ErrorCatalog.GetDetail(Constants.ErrorCodes.InternalError), problem.RootElement.GetProperty(@"detail").GetString());
            Assert.Equal(Constants.ErrorCodes.InternalError, problem.RootElement.GetProperty(@"code").GetString());
            Assert.Equal(500, problem.RootElement.GetProperty(@"status").GetInt32());
            Assert.False(string.IsNullOrWhiteSpace(problem.RootElement.GetProperty(@"traceId").GetString()));
        }

        private static IResult ThrowUnexpectedException()
        {
            throw new InvalidOperationException(@"SECRETO_INTERNO_NO_EXPONER");
        }

        private static async Task<TestHost> CreateHostAsync(
            IDictionary<string, string?>? overrides = null,
            Action<WebApplication>? mapEndpoints = null,
            Action<IServiceCollection>? configureServices = null,
            CancellationToken cancellationToken = default)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = [],
                ContentRootPath = AppContext.BaseDirectory,
                EnvironmentName = @"Production",
            });

            builder.Configuration.AddJsonFile(
                Path.Combine(AppContext.BaseDirectory, @"appsettings.mock.json"),
                optional: false);

            if (overrides is not null)
            {
                builder.Configuration.AddInMemoryCollection(overrides);
            }

            builder.WebHost.UseUrls(@"http://127.0.0.1:0");
            builder.Configure();
            configureServices?.Invoke(builder.Services);

            var app = builder.Build();
            app.UseMiddlewares();
            mapEndpoints?.Invoke(app);
            await app.StartAsync(cancellationToken);

            return new TestHost(app, new HttpClient { BaseAddress = new Uri(app.Urls.Single()) }, cancellationToken);
        }

        private sealed class RequireAntiforgeryMetadata : IAntiforgeryMetadata
        {
            public bool RequiresValidation => true;
        }

        private sealed class TestHost : IAsyncDisposable
        {
            private readonly WebApplication _app;
            private readonly CancellationToken _cancellationToken;

            internal TestHost(WebApplication app, HttpClient client, CancellationToken cancellationToken)
            {
                _app = app;
                _cancellationToken = cancellationToken;
                Client = client;
            }

            internal HttpClient Client { get; }

            public async ValueTask DisposeAsync()
            {
                Client.Dispose();
                await _app.StopAsync(_cancellationToken);
                await _app.DisposeAsync();
            }
        }
    }
}
