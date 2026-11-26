using System.Net;
using System.Security.Claims;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.RateLimiting;
using G1.Identity.Mock;
using G1.Identity.Mock.Abstractions;
using G1.Identity.Mock.Auth;
using G1.Identity.Mock.Endpoints;
using G1.Identity.Mock.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SessionSettings = G1.Identity.Mock.Settings.SessionOptions;

namespace G1.Identity.ContractTests
{
    /// <summary>Comprueba el contrato HTTP de sesiones con una identidad de prueba.</summary>
    public sealed class SessionEndpointTests
    {
        [Fact]
        public async Task OwnSessionsCanBeListedAndRevokedButForeignSessionsAreHidden()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(cancellationToken);
            host.Sessions.Create(@"ses_current", @"usr_1", @"cli_1", [@"mock"], @"Actual");
            host.Sessions.Create(@"ses_other", @"usr_1", @"cli_1", [@"mock"], @"Otro");
            host.Sessions.Create(@"ses_foreign", @"usr_2", @"cli_1", [@"mock"], @"Ajeno");

            using var listed = await host.Client.GetAsync(Constants.Paths.Sessions, cancellationToken);
            Assert.Equal(HttpStatusCode.OK, listed.StatusCode);
            Assert.Equal(Constants.Headers.NoStore, Assert.Single(listed.Headers.GetValues(Constants.Headers.CacheControl)));
            using var document = JsonDocument.Parse(await listed.Content.ReadAsStringAsync(cancellationToken));
            Assert.Equal(2, document.RootElement.GetProperty(@"total").GetInt32());
            Assert.DoesNotContain(@"ses_foreign", document.RootElement.ToString(), StringComparison.Ordinal);

            using var foreign = await DeleteAsync(host.Client, @"ses_foreign", cancellationToken);
            Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
            using var problem = JsonDocument.Parse(await foreign.Content.ReadAsStringAsync(cancellationToken));
            Assert.Equal(Constants.ErrorCodes.UserNotFound, problem.RootElement.GetProperty(@"code").GetString());

            using var revoked = await DeleteAsync(host.Client, @"ses_other", cancellationToken);
            Assert.Equal(HttpStatusCode.NoContent, revoked.StatusCode);
            using var repeated = await DeleteAsync(host.Client, @"ses_other", cancellationToken);
            Assert.Equal(HttpStatusCode.NoContent, repeated.StatusCode);
            Assert.Single(host.Sessions.ListActive(@"usr_1"));
        }

        [Fact]
        public async Task RevokeAllKeepsCurrentSessionAndLogoutRevokesIt()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(cancellationToken);
            host.Sessions.Create(@"ses_current", @"usr_1", @"cli_1", [@"mock"], @"Actual");
            host.Sessions.Create(@"ses_other", @"usr_1", @"cli_1", [@"mock"], @"Otro");
            Assert.True(host.Sessions.LinkRefreshToken(@"ses_current", @"rft_current"));
            Assert.True(host.Sessions.LinkRefreshToken(@"ses_other", @"rft_other"));

            using var revokeAll = new HttpRequestMessage(HttpMethod.Post, Constants.Paths.RevokeAllSessions)
            {
                Content = JsonContent.Create(new { keepCurrentSession = true }),
            };
            revokeAll.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(@"Bearer", @"fixture");
            using var response = await host.Client.SendAsync(revokeAll, cancellationToken);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.True(host.Sessions.IsRefreshTokenActive(@"ses_current", @"rft_current"));
            Assert.False(host.Sessions.IsRefreshTokenActive(@"ses_other", @"rft_other"));

            using var logout = new HttpRequestMessage(HttpMethod.Post, Constants.Paths.Logout);
            logout.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(@"Bearer", @"fixture");
            using var closed = await host.Client.SendAsync(logout, cancellationToken);
            Assert.Equal(HttpStatusCode.NoContent, closed.StatusCode);
            Assert.False(host.Sessions.IsRefreshTokenActive(@"ses_current", @"rft_current"));
            Assert.Empty(host.Sessions.ListActive(@"usr_1"));
        }

        [Fact]
        public async Task RevokeAllCanIncludeCurrentSession()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            await using var host = await CreateHostAsync(cancellationToken);
            host.Sessions.Create(@"ses_current", @"usr_1", @"cli_1", [@"mock"], @"Actual");

            using var request = new HttpRequestMessage(HttpMethod.Post, Constants.Paths.RevokeAllSessions)
            {
                Content = JsonContent.Create(new { keepCurrentSession = false }),
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(@"Bearer", @"fixture");
            using var response = await host.Client.SendAsync(request, cancellationToken);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Empty(host.Sessions.ListActive(@"usr_1"));
        }

        private static async Task<HttpResponseMessage> DeleteAsync(HttpClient client, string sessionId, CancellationToken token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $@"/api/v1/identity/sessions/{sessionId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(@"Bearer", @"fixture");
            return await client.SendAsync(request, token);
        }

        private static async Task<TestHost> CreateHostAsync(CancellationToken token)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = [],
                ContentRootPath = AppContext.BaseDirectory,
                EnvironmentName = @"Testing",
            });
            builder.WebHost.UseUrls(@"http://127.0.0.1:0");
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization(PermissionPolicies.Configure);
            PermissionPolicies.RegisterResultHandler(builder.Services);
            builder.Services.AddAntiforgery();
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddSingleton<ISessionService, SessionService>();
            builder.Services.AddSingleton<IAuditService, AuditService>();
            builder.Services.AddOptions<SessionSettings>();
            builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter(
                Constants.Policies.Summary,
                limiter =>
                {
                    limiter.PermitLimit = Constants.RateLimits.SummaryPermitLimit;
                    limiter.Window = TimeSpan.FromSeconds(Constants.RateLimits.WindowSeconds);
                    limiter.QueueLimit = 0;
                    limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                }));

            var app = builder.Build();
            app.Use(async (context, next) =>
            {
                context.User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(Constants.Claims.Subject, @"usr_1"), new Claim(Constants.Claims.SessionId, @"ses_current")],
                    @"test"));
                await next(context);
            });
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();
            app.UseRateLimiter();
            new SessionEndpointDefinition().RegisterEndpoints(app);
            await app.StartAsync(token);
            return new TestHost(app, new HttpClient { BaseAddress = new Uri(app.Urls.Single()) },
                app.Services.GetRequiredService<ISessionService>(), token);
        }

        private sealed class TestHost(WebApplication app, HttpClient client, ISessionService sessions, CancellationToken token)
            : IAsyncDisposable
        {
            internal HttpClient Client { get; } = client;
            internal ISessionService Sessions { get; } = sessions;

            public async ValueTask DisposeAsync()
            {
                Client.Dispose();
                await app.StopAsync(token);
                await app.DisposeAsync();
            }
        }
    }
}
