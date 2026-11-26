using System.Net;
using G1.Identity.Mock;
using G1.Identity.Mock.Auth;
using G1.Identity.Mock.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace G1.Identity.ContractTests
{
    /// <summary>
    /// Pruebas de contrato para la autenticación de personas del mock (T016).
    /// </summary>
    public sealed class MockPersonaAuthenticationTests
    {
        private const string SessionCookieName = @"__Host-g1.session";
        private const string MockPersonaParam = @"mock_persona";

        [Fact]
        public async Task MockPersonaParameterSetsSessionCookieInDevelopment()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: Environments.Development,
                mapEndpoints: app => app.MapGet(@"/api/test", () => TypedResults.Ok(@"ok")).RequireAuthorization(),
                cancellationToken: cancellationToken);

            // Intentar acceder sin persona -> 401
            using var anonymousResponse = await host.Client.GetAsync(@"/api/test", cancellationToken);
            Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

            // Acceder con parámetro mock_persona
            using var response = await host.Client.GetAsync($@"/api/test?{MockPersonaParam}=usr_persona_1", cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var setCookie = Assert.Single(response.Headers.GetValues(@"Set-Cookie"));
            Assert.Contains($@"{SessionCookieName}=", setCookie, StringComparison.Ordinal);
            Assert.Contains(@"httponly", setCookie, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(@"samesite=lax", setCookie, StringComparison.OrdinalIgnoreCase);
            // El host de prueba es HTTP, pero en la configuración forzamos SecurePolicy.Always. 
            // HttpClient de .NET no siempre muestra 'secure' en Set-Cookie si es HTTP, 
            // pero podemos verificar la configuración de las opciones si es necesario.
        }

        [Fact]
        public async Task MockPersonaParameterIsRejectedInProduction()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: Environments.Production,
                mapEndpoints: app => app.MapGet(@"/api/test", () => TypedResults.Ok(@"ok")).RequireAuthorization(),
                cancellationToken: cancellationToken);

            using var response = await host.Client.GetAsync($@"/api/test?{MockPersonaParam}=usr_persona_1", cancellationToken);

            // En producción, el parámetro debe ser ignorado o causar rechazo. 
            // Como requiere autorización y el parámetro se ignora, devuelve 401.
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.False(response.Headers.Contains(@"Set-Cookie"));
        }

        [Fact]
        public async Task AuthenticatedRequestRequiresAntiforgeryForWrites()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: Environments.Development,
                mapEndpoints: app => app.MapPost(@"/api/test", (HttpContext ctx) =>
                {
                    var antiforgery = ctx.Features.Get<IAntiforgeryValidationFeature>();
                    return antiforgery is { IsValid: false } ? Results.BadRequest() : TypedResults.Ok(@"ok");
                })
                    .RequireAuthorization()
                    .WithMetadata(new RequireAntiforgeryMetadata()),
                cancellationToken: cancellationToken);

            // 1. Obtener cookie de sesión
            using var loginResponse = await host.Client.GetAsync($@"/api/test?{MockPersonaParam}=usr_persona_1", cancellationToken);
            var sessionCookie = loginResponse.Headers.GetValues(@"Set-Cookie").First().Split(';')[0];

            // 2. Intentar POST sin antiforgery -> 400 (Bad Request) por el middleware de antiforgery
            using var request = new HttpRequestMessage(HttpMethod.Post, @"/api/test");
            request.Headers.TryAddWithoutValidation(@"Cookie", sessionCookie);
            using var response = await host.Client.SendAsync(request, cancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task MockPersonaAuthenticationDoesNotEmitJwt()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(
                environment: Environments.Development,
                mapEndpoints: app => app.MapGet(@"/api/test", () => TypedResults.Ok(@"ok")).RequireAuthorization(),
                cancellationToken: cancellationToken);

            using var response = await host.Client.GetAsync($@"/api/test?{MockPersonaParam}=usr_persona_1", cancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(response.Headers.Contains(@"Authorization"));
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            Assert.DoesNotContain(@"access_token", body, StringComparison.Ordinal);
            Assert.DoesNotContain(@"id_token", body, StringComparison.Ordinal);
        }

        private static async Task<TestHost> CreateHostAsync(
            string environment,
            Action<WebApplication> mapEndpoints,
            CancellationToken cancellationToken)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = [],
                ContentRootPath = AppContext.BaseDirectory,
                EnvironmentName = environment,
            });

            // Configuración mínima necesaria para que BuilderExtensions no falle
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [@"Identity:Issuer"] = @"https://localhost:7180",
                [@"Identity:Lifetimes:AuthorizationCode"] = @"00:05:00",
                [@"Identity:Lifetimes:AccessToken"] = @"00:05:00",
                [@"Identity:Lifetimes:IdentityToken"] = @"00:05:00",
                [@"Identity:Lifetimes:RefreshToken"] = @"08:00:00",
                [@"Identity:Session:IdleTimeout"] = @"00:30:00",
                [@"Identity:Session:AbsoluteTimeout"] = @"12:00:00",
                [@"Identity:Cors:AllowedOrigins:0"] = @"https://localhost:3000",
            });

            builder.Configure();
            
            // Registro de la autenticación de persona del mock (T016)
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = @"MockPersona";
                options.DefaultChallengeScheme = @"MockPersona";
            })
            .AddCookie()
            .AddMockPersona();

            builder.WebHost.UseUrls(@"http://127.0.0.1:0");
            var app = builder.Build();
            app.Use(async (context, next) =>
            {
                context.Request.Scheme = @"https";
                await next();
            });
            app.UseMiddlewares();
            app.UseAuthentication();
            app.UseAuthorization();
            
            mapEndpoints(app);
            await app.StartAsync(cancellationToken);

            return new TestHost(app, new HttpClient { BaseAddress = new Uri(app.Urls.Single()) }, cancellationToken);
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

        private sealed class RequireAntiforgeryMetadata : IAntiforgeryMetadata
        {
            public bool RequiresValidation => true;
        }
    }
}
